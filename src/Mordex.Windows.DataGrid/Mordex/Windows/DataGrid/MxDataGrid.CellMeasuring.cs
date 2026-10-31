using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class MxDataGrid
	{
		private bool IsRowInVisibleTableRange(int value)
		{
			if (_bodyRowLayouts == null)
			{
				return true;
			}
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (value >= _firstRow && value < _firstRow + _bodyRowLayouts.GetVisibleRowCount())
			{
				return true;
			}
			return false;
		}

		private bool IsRowInVisibleCardRange(int value)
		{
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (_cardRowLayouts != null && value >= _firstRow && value < _firstRow + _cardRowLayouts.GetCurrentIndex())
			{
				return true;
			}
			return false;
		}

		private bool IsValidRowPosition(int value)
		{
			switch (value)
			{
			case -3:
				return false;
			case -2:
				return _filterMode != FilterMode.None;
			case -1:
				if (RootTable != null && RootTable.IsAddNewAllowed() && RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow)
				{
					if (_view == View.TableView)
					{
						return true;
					}
					return false;
				}
				return false;
			default:
			{
				int totalRowPosition = GetTotalRowPosition();
				if (totalRowPosition != -1 && value == totalRowPosition)
				{
					return true;
				}
				if (value < -1 || value >= GetRowManager().Count)
				{
					return false;
				}
				return true;
			}
			}
		}

		private bool CanFocusLayoutItem(GridGroupLayoutState gridGroupLayoutState, bool condition)
		{
			if (gridGroupLayoutState != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow() != null)
			{
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.FilterRow)
				{
					return CanActivateCellAt(-2, gridGroupLayoutState._column, condition);
				}
				if (gridGroupLayoutState._type == LayoutItemType.SelfReferencingExpandBox)
				{
					return CanFocusCell(gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Position, null);
				}
				if (gridGroupLayoutState._type != LayoutItemType.GroupHeaderRow)
				{
					return CanActivateCellAt(gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Position, gridGroupLayoutState._column, condition);
				}
				return CanFocusCell(gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Position, null);
			}
			return true;
		}

		public bool IsFieldChooserVisible()
		{
			if (_gridFieldChooser != null && _gridFieldChooser.Visible)
			{
				return true;
			}
			return false;
		}

		private void MarkTablesForAutoResize()
		{
			if (_view != View.TableView || !_columnAutoResize)
			{
				return;
			}
			foreach (GridTable item in _gridTableCollection)
			{
				item._isAutoResizePending = true;
			}
			RefreshRowCountAndInvalidate();
		}

		public bool IsCellVisible(int rowPosition, int colPosition)
		{
			if (RootTable == null)
			{
				return false;
			}
			GridColumn columnInPosition = RootTable.Columns.GetColumnInPosition(colPosition);
			return IsCellVisible(rowPosition, columnInPosition);
		}

		public bool IsCellVisible(int rowPosition, GridColumn column)
		{
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			GridRow row = GetRow(rowPosition);
			GridGroupLayoutState gridGroupLayoutState = null;
			if (row != null)
			{
				gridGroupLayoutState = ((row.RowType != RowType.FilterRow) ? FindCellLayoutItem(row, column) : FindFilterCellLayoutItem(row, column));
			}
			if (gridGroupLayoutState != null && gridGroupLayoutState._type != LayoutItemType.EmptyItem)
			{
				Rectangle rectangle = gridGroupLayoutState.GetAbsoluteItemRect();
				rectangle = AdjustRectangleToViewport(rectangle, gridGroupLayoutState._isHorizontallyScrolled);
				Rectangle clientRectangle = base.ClientRectangle;
				if (rectangle.Left > clientRectangle.Right || rectangle.Right < clientRectangle.Left)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public Rectangle GetCellBounds(int rowPosition, int colPosition)
		{
			GridRow row = GetRow(rowPosition);
			GridColumn columnInPosition = row.Table.Columns.GetColumnInPosition(colPosition);
			return GetCellBounds(rowPosition, columnInPosition);
		}

		public Rectangle GetCellBounds(int rowPosition, GridColumn column)
		{
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			GridRow row = GetRow(rowPosition);
			if (column == null)
			{
				MxRowLayoutState mordexRowLayoutState = null;
				if (_bodyRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _bodyRowLayouts)
					{
						if (item._gridRow == row)
						{
							mordexRowLayoutState = item;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null)
				{
					return Rectangle.Empty;
				}
				Rectangle rectangle = mordexRowLayoutState.GetContentBounds();
				return AdjustRectangleToViewport(rectangle, true);
			}
			GridGroupLayoutState gridGroupLayoutState = FindCellLayoutItem(row, column);
			if (gridGroupLayoutState != null && gridGroupLayoutState._type != LayoutItemType.EmptyItem)
			{
				Rectangle itemRectCore = gridGroupLayoutState.GetAbsoluteItemRect();
				itemRectCore = AdjustRectangleToViewport(itemRectCore, gridGroupLayoutState._isHorizontallyScrolled);
				Rectangle clientRectangle = base.ClientRectangle;
				if (itemRectCore.Left > clientRectangle.Right || itemRectCore.Right < clientRectangle.Left)
				{
					return Rectangle.Empty;
				}
				return itemRectCore;
			}
			return Rectangle.Empty;
		}

		internal NewRowPosition ComputeNewRowPosition()
		{
			if (NewRowPosition == NewRowPosition.Default)
			{
				return NewRowPosition.TopRow;
			}
			return NewRowPosition;
		}

		private int MeasureValueListCellWidth(Graphics graphics, object value, GridFormatStyle gridFormatStyle, TextLayoutRenderer textLayoutRenderer, Image image, GridColumn gridColumn)
		{
			ILookUpTable lookUpTable = null;
			if (gridColumn.ValueList != null && gridColumn.ValueList.ReplaceValues)
			{
				lookUpTable = gridColumn.ValueList;
			}
			else if (gridColumn.DropDown != null && gridColumn.DropDown.ReplaceValues)
			{
				lookUpTable = gridColumn.DropDown;
			}
			int measureListItems = ListCellRenderer.MeasureListItems(graphics, value, lookUpTable, ComputeImageList(), gridFormatStyle.ResolveFont(Font), textLayoutRenderer);
			measureListItems = ((gridColumn == null) ? (measureListItems + 2) : (measureListItems + (gridColumn.LeftMargin + gridColumn.RightMargin)));
			if (measureListItems < 8)
			{
				measureListItems = 8;
			}
			return measureListItems + 3;
		}

		// Widths by text for each (font, renderer settings, wrap) of one automatic column sizing. The group of the last lookup is
		// kept, because a column asks for the same group row after row.
		private sealed class MeasureMemo
		{
			private readonly Dictionary<(Font, int, bool), Dictionary<string, int>> _groups = new Dictionary<(Font, int, bool), Dictionary<string, int>>();

			private Font _lastFont;

			private int _lastRenderer;

			private bool _lastWrap;

			private Dictionary<string, int> _last;

			internal Dictionary<string, int> Group(Font font, int renderer, bool wordWrap)
			{
				if (_last != null && ReferenceEquals(font, _lastFont) && renderer == _lastRenderer && wordWrap == _lastWrap)
				{
					return _last;
				}
				if (!_groups.TryGetValue((font, renderer, wordWrap), out Dictionary<string, int> group))
				{
					group = new Dictionary<string, int>(StringComparer.Ordinal);
					_groups[(font, renderer, wordWrap)] = group;
				}
				_lastFont = font;
				_lastRenderer = renderer;
				_lastWrap = wordWrap;
				_last = group;
				return group;
			}
		}

		// Text widths already measured during one automatic column sizing; null at other times.
		private MeasureMemo _measureMemo;

		private GridPassStyleCache _autoSizeStyles;

		private int MeasureTextCellWidth(Graphics graphics, string text, GridFormatStyle gridFormatStyle, TextLayoutRenderer textLayoutRenderer, Image image, GridColumn gridColumn)
		{
			if (!GridHelper.IsTextLengthValid(text))
			{
				text = GridHelper.TruncateText(text);
			}
			int value = 0;
			if (image != null && image.Width > 8 && (gridFormatStyle.ImageHorizontalAlignment == ImageHorizontalAlignment.AfterText || gridFormatStyle.ImageHorizontalAlignment == ImageHorizontalAlignment.BeforeText || gridFormatStyle.ImageHorizontalAlignment == ImageHorizontalAlignment.Empty))
			{
				value += image.Width + 2;
			}
			if (text != null && text.Length > 0)
			{
				bool wordWrap = false;
				if (gridColumn != null)
				{
					wordWrap = gridColumn.WordWrap;
				}
				Font font = gridFormatStyle.ResolveFont(Font);
				int width;
				if (_measureMemo == null)
				{
					width = (int)textLayoutRenderer.RenderContent(graphics, text, font, 0, wordWrap).Width;
				}
				else
				{
					Dictionary<string, int> widths = _measureMemo.Group(font, textLayoutRenderer.GetMeasureSignature(), wordWrap);
					if (!widths.TryGetValue(text, out width))
					{
						width = (int)textLayoutRenderer.RenderContent(graphics, text, font, 0, wordWrap).Width;
						widths[text] = width;
					}
				}
				value += width;
			}
			value = ((gridColumn == null) ? (value + 2) : (value + (gridColumn.LeftMargin + gridColumn.RightMargin)));
			if (value < 8)
			{
				value = 8;
			}
			return value + 3;
		}

		internal void AutoSizeColumnToContent(GridColumn column, bool condition)
		{
			if (column == null)
			{
				return;
			}
			AutoSizeColumnsToContent(new List<GridColumn> { column }, condition);
		}

		// Measures the cells once for all given columns of one table, then sizes the columns in the given order.
		internal void AutoSizeColumnsToContent(List<GridColumn> columns, bool condition)
		{
			if (base.IsDisposed || base.Disposing || GetBindingContext() == null || View != View.TableView || columns.Count == 0)
			{
				return;
			}
			Dictionary<GridColumn, int> proposedWidths = new Dictionary<GridColumn, int>();
			Dictionary<GridTable, List<GridColumn>> allCellColumns = new Dictionary<GridTable, List<GridColumn>>();
			Dictionary<GridTable, List<GridColumn>> displayedColumns = new Dictionary<GridTable, List<GridColumn>>();
			MeasureMemo previousMemo = _measureMemo;
			_measureMemo = new MeasureMemo();
			GridPassStyleCache previousStyles = _autoSizeStyles;
			GridFontCache.BeginDeferral();
			_autoSizeStyles = new GridPassStyleCache();
			try
			{
				using (Graphics graphics = CreateGraphics())
				{
					foreach (GridColumn column in columns)
					{
						ColumnAutoSizeMode columnAutoSizeMode = column.ComputeColumnAutoSizeMode(this);
						proposedWidths[column] = 0;
						switch (columnAutoSizeMode)
						{
						case ColumnAutoSizeMode.AllCellsAndHeader:
						case ColumnAutoSizeMode.AllCells:
							AddToTableGroup(allCellColumns, column);
							break;
						case ColumnAutoSizeMode.DiaplayedCells:
						case ColumnAutoSizeMode.DisplayedCellsAndHeader:
							AddToTableGroup(displayedColumns, column);
							break;
						}
						if (columnAutoSizeMode == ColumnAutoSizeMode.AllCellsAndHeader || columnAutoSizeMode == ColumnAutoSizeMode.ColumnHeader || columnAutoSizeMode == ColumnAutoSizeMode.DisplayedCellsAndHeader)
						{
							proposedWidths[column] = MeasureHeaderWidth(graphics, column);
						}
					}
					foreach (KeyValuePair<GridTable, List<GridColumn>> group in allCellColumns)
					{
						ArrayList rows = new ArrayList();
						if (_gridDataSourceManager != null && _gridDataSourceManager.RootRow != null)
						{
							ProcessArrayList(rows, group.Key, _gridDataSourceManager.RootRow, false);
						}
						MeasureRowsForColumns(graphics, rows, group.Value, proposedWidths);
					}
					foreach (KeyValuePair<GridTable, List<GridColumn>> group in displayedColumns)
					{
						while (!_isLayoutValid)
						{
							CalculateLayoutStates(graphics);
						}
						ArrayList rows = new ArrayList();
						foreach (MxRowLayoutState item in _bodyRowLayouts)
						{
							if (item.LayoutTable == group.Key && item.GridRow != null && item.GridRow.GetLogicalRow() != null)
							{
								rows.Add(item.GridRow.GetLogicalRow());
							}
						}
						MeasureRowsForColumns(graphics, rows, group.Value, proposedWidths);
					}
				}
			}
			finally
			{
				_measureMemo = previousMemo;
				_autoSizeStyles.Release();
				_autoSizeStyles = previousStyles;
				GridFontCache.EndDeferral();
			}
			foreach (GridColumn column in columns)
			{
				if (!ApplyAutoSizeWidth(column, proposedWidths[column], condition))
				{
					return;
				}
			}
		}

		private static void AddToTableGroup(Dictionary<GridTable, List<GridColumn>> groups, GridColumn column)
		{
			if (!groups.TryGetValue(column.Table, out List<GridColumn> list))
			{
				list = new List<GridColumn>();
				groups[column.Table] = list;
			}
			list.Add(column);
		}

		private int MeasureHeaderWidth(Graphics graphics, GridColumn column)
		{
			GridFormatStyle gridFormatStyle = GetHeaderStyle(column.Table);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(column.HeaderStyle);
			Image image = null;
			if (column.HeaderImage != null)
			{
				image = column.HeaderImage;
			}
			else
			{
				ImageList imageList = ComputeImageList();
				if (imageList != null && column.HeaderImageKey.Length > 0)
				{
					if (imageList.Images.ContainsKey(column.HeaderImageKey))
					{
						image = imageList.Images[column.HeaderImageKey];
					}
				}
				else if (imageList != null && column.HeaderImageIndex >= 0 && column.HeaderImageIndex < imageList.Images.Count)
				{
					image = imageList.Images[column.HeaderImageIndex];
				}
			}
			TextLayoutRenderer textLayoutRenderer = column.CreateHeaderLayoutRenderer(applyOverrides, !base.UseCompatibleTextRendering);
			textLayoutRenderer.SetWordWrap(column.Table.HeaderLines > 1);
			int proposedWidth = MeasureTextCellWidth(graphics, column.Caption, applyOverrides, textLayoutRenderer, image, null) + 3;
			textLayoutRenderer.ReleaseStringFormat();
			if (column.SortOrder != SortOrder.Unsorted)
			{
				proposedWidth += 12;
			}
			applyOverrides.DisposeResources();
			gridFormatStyle.DisposeResources();
			return proposedWidth;
		}

		private void MeasureRowsForColumns(Graphics graphics, ArrayList rows, List<GridColumn> columns, Dictionary<GridColumn, int> proposedWidths)
		{
			GetRowManager();
			foreach (MordexRow row in rows)
			{
				GridColumn first = columns[0];
				if (row.Table != first.Table || !(row is MordexDataRow || row is MordexTotalRow || row is MordexGroupRow))
				{
					continue;
				}
				GridRow gridRow = GetGridRowForLogicalRow(row);
				GridFormatStyle alternatingRowStyle = null;
				GridFormatStyle rowChain = null;
				switch (gridRow.RowType)
				{
				case RowType.Record:
					alternatingRowStyle = _autoSizeStyles != null ? _autoSizeStyles.GetRowStyle(this, gridRow.Table, gridRow.Position) : GetAlternatingRowStyle(gridRow.Table, gridRow.Position);
					rowChain = alternatingRowStyle.ApplyOverrides(gridRow.RowStyle);
					GridFormatStyle recordStyle = rowChain.ApplyOverrides(gridRow.GetCachedFormatStyle());
					foreach (GridColumn column in columns)
					{
						RaiseWidth(proposedWidths, column, MeasureCellContent(graphics, gridRow, recordStyle, column));
					}
					if (recordStyle != rowChain)
					{
						recordStyle.DisposeResources();
					}
					if (rowChain != alternatingRowStyle)
					{
						rowChain.DisposeResources();
					}
					break;
				case RowType.GroupFooter:
					alternatingRowStyle = GetGroupTotalRowStyle(gridRow.Table);
					foreach (GridColumn column in columns)
					{
						RaiseWidth(proposedWidths, column, MeasureRecordRowCellWidth(graphics, gridRow, alternatingRowStyle, column));
					}
					break;
				case RowType.TotalRow:
					alternatingRowStyle = GetTotalRowStyle(gridRow.Table);
					foreach (GridColumn column in columns)
					{
						RaiseWidth(proposedWidths, column, MeasureSimpleRowCellWidth(graphics, gridRow, alternatingRowStyle, column));
					}
					break;
				case RowType.GroupHeader:
					alternatingRowStyle = GetGroupRowStyle(gridRow.Table);
					foreach (GridColumn column in columns)
					{
						RaiseWidth(proposedWidths, column, MeasureGroupHeaderCellWidth(graphics, gridRow, alternatingRowStyle, column));
					}
					break;
				}
				alternatingRowStyle?.DisposeResources();
			}
		}

		private static void RaiseWidth(Dictionary<GridColumn, int> proposedWidths, GridColumn column, int width)
		{
			if (width > proposedWidths[column])
			{
				proposedWidths[column] = width;
			}
		}

		// Returns false when the sizing was cancelled.
		private bool ApplyAutoSizeWidth(GridColumn column, int proposedWidth, bool condition)
		{
			if (column.GetButtonStyle() != ButtonStyle.NoButton && column.GetButtonStyle() != ButtonStyle.ButtonCell)
			{
				proposedWidth += SystemInformation.VerticalScrollBarWidth;
			}
			proposedWidth += 2;
			proposedWidth += 2;
			if (column.Table == RootTable && column.Table.GetExpandColumn() == column && column.Visible && column.GetOwnerColumnCollection().CountVisibleColumns() == 1)
			{
				int width = base.ClientRectangle.Width;
				if (column.Table.AreRowHeadersVisible())
				{
					width -= column.Table.RowHeaderWidth;
				}
				if (proposedWidth < width)
				{
					proposedWidth = width;
				}
			}
			if (condition)
			{
				SizingColumnEventArgs e = new SizingColumnEventArgs(column, proposedWidth, cancel: false);
				OnSizingColumn(e);
				if (e.Cancel)
				{
					return false;
				}
				proposedWidth = e.ProposedWidth;
			}
			if (_columnAutoResize)
			{
				bool allowSize = column.AllowSize;
				column.AllowSize = false;
				column.Width = proposedWidth;
				CalculateLayoutIfReady();
				column.AllowSize = allowSize;
				column.Table._isAutoResizePending = true;
				RefreshRowCountAndInvalidate();
			}
			else
			{
				column.Width = proposedWidth;
			}
			if (condition)
			{
				OnColumnAutoSize(new ColumnActionEventArgs(column));
			}
			return true;
		}

		private int MeasureCellContent(Graphics graphics, GridRow gridRow, GridFormatStyle applyOverrides, GridColumn gridColumn)
		{
			GridCell gridCell = gridRow.Cells[gridColumn.Index];
			if (gridCell == null)
			{
				return 0;
			}
			if (gridCell.CellDisplayType == ColumnType.CheckBox)
			{
				return 18;
			}
			if (gridCell.CellDisplayType == ColumnType.BoundImage)
			{
				if (gridColumn.BoundImageSize.IsEmpty)
				{
					try
					{
						Image image = (Image)gridCell.Value;
						if (image != null)
						{
							return image.Width + gridColumn.LeftMargin + gridColumn.RightMargin + 2;
						}
						return 0;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						return 0;
					}
				}
				return gridColumn.BoundImageSize.Width;
			}
			GridFormatStyle otherApplyOverrides = null;
			GridFormatStyle nextApplyOverrides = null;
			GridFormatStyle previousApplyOverrides = null;
			GridFormatStyle otherFormatStyle = null;
			otherFormatStyle = ((gridCell.CellDisplayType != ColumnType.Link) ? applyOverrides : applyOverrides.ApplyOverrides(_linkFormatStyle));
			otherApplyOverrides = (_autoSizeStyles != null && otherFormatStyle._isPassShared) ? _autoSizeStyles.GetCellStyle(otherFormatStyle, gridColumn) : otherFormatStyle.ApplyOverrides(gridColumn.CellStyle);
			nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridRow.Cells[gridColumn.Index].GetEffectiveFormatStyle());
			previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
			Image containsKeyCore = null;
			if (gridCell.CellDisplayType == ColumnType.Image || gridCell.CellDisplayType == ColumnType.ImageAndText)
			{
				containsKeyCore = gridCell.ResolveImage(ComputeImageList());
			}
			string text = "";
			if (gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText || gridCell.CellDisplayType == ColumnType.Link || gridColumn.MultipleValues)
			{
				text = gridRow.Cells[gridColumn.Index].Text;
			}
			TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(previousApplyOverrides, !base.UseCompatibleTextRendering);
			int measureValueListCellWidth = -1;
			if (gridColumn.MultipleValues)
			{
				measureValueListCellWidth = MeasureValueListCellWidth(graphics, gridRow.Cells[gridColumn.Index].Value, previousApplyOverrides, textLayoutRenderer, containsKeyCore, gridColumn);
			}
			if (measureValueListCellWidth == -1)
			{
				measureValueListCellWidth = MeasureTextCellWidth(graphics, text, previousApplyOverrides, textLayoutRenderer, containsKeyCore, gridColumn);
			}
			textLayoutRenderer.ReleaseStringFormat();
			if (gridColumn.Table.GetExpandColumn() == gridColumn)
			{
				int value = RowStateHelper.GetSelfReferencingDepth(gridRow.GetLogicalRow()) + 1;
				value *= gridColumn.Table.GetIndentWidth();
				measureValueListCellWidth += value - 4;
			}
			if (gridColumn.ShowRowSelector)
			{
				measureValueListCellWidth += 16;
			}
			if (previousApplyOverrides != nextApplyOverrides)
			{
				previousApplyOverrides.DisposeResources();
			}
			if (nextApplyOverrides != otherApplyOverrides)
			{
				nextApplyOverrides.DisposeResources();
			}
			if (otherApplyOverrides != otherFormatStyle)
			{
				otherApplyOverrides.DisposeResources();
			}
			if (otherFormatStyle != applyOverrides)
			{
				otherFormatStyle.DisposeResources();
			}
			return measureValueListCellWidth;
		}

		private int MeasureRecordRowCellWidth(Graphics graphics, GridRow gridRow, GridFormatStyle formatStyle, GridColumn gridColumn)
		{
			GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.Group.GetGroupRowStyle());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
			Image image = gridRow.Cells[gridColumn.Index].ResolveImage(ComputeImageList());
			string text = "";
			text = gridRow.Cells[gridColumn.Index].Text;
			TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(otherApplyOverrides, !base.UseCompatibleTextRendering);
			int result = MeasureTextCellWidth(graphics, text, otherApplyOverrides, textLayoutRenderer, image, gridColumn);
			textLayoutRenderer.ReleaseStringFormat();
			otherApplyOverrides.DisposeResources();
			applyOverrides.DisposeResources();
			gridFormatStyle.DisposeResources();
			return result;
		}

		private int MeasureGroupHeaderCellWidth(Graphics graphics, GridRow gridRow, GridFormatStyle formatStyle, GridColumn gridColumn)
		{
			GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.Group.GetGroupRowStyle());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
			GridGroupHeaderTotal gridGroupHeaderTotal = gridRow.Cells[gridColumn]._groupHeaderTotal as GridGroupHeaderTotal;
			GridFormatStyle otherApplyOverrides = null;
			GridFormatStyle nextApplyOverrides;
			if (gridGroupHeaderTotal != null)
			{
				otherApplyOverrides = applyOverrides.ApplyOverrides(gridGroupHeaderTotal.FormatStyle);
				nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
			}
			else
			{
				nextApplyOverrides = applyOverrides.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
			}
			Image image = gridRow.Cells[gridColumn.Index].ResolveImage(ComputeImageList());
			string text = "";
			text = gridRow.Cells[gridColumn.Index].Text;
			TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(nextApplyOverrides, !base.UseCompatibleTextRendering);
			int result = MeasureTextCellWidth(graphics, text, nextApplyOverrides, textLayoutRenderer, image, gridColumn);
			textLayoutRenderer.ReleaseStringFormat();
			nextApplyOverrides.DisposeResources();
			applyOverrides.DisposeResources();
			gridFormatStyle.DisposeResources();
			otherApplyOverrides?.DisposeResources();
			return result;
		}

		private int MeasureSimpleRowCellWidth(Graphics graphics, GridRow gridRow, GridFormatStyle formatStyle, GridColumn gridColumn)
		{
			GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
			Image image = gridRow.Cells[gridColumn.Index].ResolveImage(ComputeImageList());
			string text = "";
			text = gridRow.Cells[gridColumn.Index].Text;
			TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(applyOverrides, !base.UseCompatibleTextRendering);
			int result = MeasureTextCellWidth(graphics, text, applyOverrides, textLayoutRenderer, image, gridColumn);
			textLayoutRenderer.ReleaseStringFormat();
			applyOverrides.DisposeResources();
			gridFormatStyle.DisposeResources();
			return result;
		}

		public void AutoSizeColumns()
		{
			AutoSizeColumns(RootTable);
		}

		public void AutoSizeColumns(GridTable table)
		{
			if (table == null)
			{
				return;
			}
			List<GridColumn> sizable = new List<GridColumn>();
			foreach (GridColumn column in table.Columns)
			{
				if (column.AllowSize)
				{
					if (column.DataGrid != this)
					{
						column.AutoSize();
						continue;
					}
					sizable.Add(column);
				}
			}
			AutoSizeColumnsToContent(sizable, false);
		}

		internal bool RaiseColumnVisibleChanging(GridColumn column)
		{
			ColumnActionCancelEventArgs e = new ColumnActionCancelEventArgs(column, cancel: false);
			OnColumnVisibleChanging(e);
			return !e.Cancel;
		}

		internal void RaiseColumnVisibleChanged(GridColumn column)
		{
			ColumnActionEventArgs e = new ColumnActionEventArgs(column);
			OnColumnVisibleChanged(e);
		}

		protected virtual void OnColumnAutoSize(ColumnActionEventArgs e)
		{
			if (base.Events[_columnAutoSizeEventKey] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnColumnVisibleChanging(ColumnActionCancelEventArgs e)
		{
			if (base.Events[_columnVisibleChangingEventKey] is ColumnActionCancelEventHandler columnActionCancelEventHandler)
			{
				columnActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnColumnVisibleChanged(ColumnActionEventArgs e)
		{
			if (base.Events[_columnVisibleChangedEventKey] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnScroll(EventArgs e)
		{
			if (base.Events[_eventKeyScroll] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[_eventKeyLayoutLoad] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[_eventKeyCurrentLayoutChanged] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[_eventKeyCurrentLayoutChanging] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		public GridLayout GetLayout()
		{
			GridLayout gridLayout = new GridLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			if (this is GridDropDown)
			{
				((IMordexXmlLayoutsSupport)this).Serialize(mordexLayoutWriter);
			}
			else
			{
				WriteLayoutByMode(mordexLayoutWriter, gridLayout.GetPersistenceMode());
			}
			LayoutWriter.StoreInLayout("GridLayoutData", mordexLayoutWriter, gridLayout);
			gridLayout.DataMember = DataMember;
			gridLayout.DataSource = DataSource;
			return gridLayout;
		}

		public Rectangle GetCardBounds(int rowPosition)
		{
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (_view != View.TableView)
			{
				foreach (GridTableLayoutState item in _cardRowLayouts)
				{
					if (item._gridRow.Position == rowPosition)
					{
						return AdjustRectangleToViewport(item.GetBounds(), true);
					}
				}
			}
			return Rectangle.Empty;
		}

		private static bool IsScrollableLayoutItem(GridGroupLayoutState gridGroupLayoutState)
		{
			if (!gridGroupLayoutState._isHorizontallyScrolled && gridGroupLayoutState._type != LayoutItemType.PreviewRow && gridGroupLayoutState._type != LayoutItemType.TableHeader)
			{
				return true;
			}
			return false;
		}

		private static bool IsLayoutItemSelected(GridGroupLayoutState gridGroupLayoutState)
		{
			switch (gridGroupLayoutState._type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.GroupHeaderRow:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.GroupExpandBox:
			case LayoutItemType.CardCaption:
			case LayoutItemType.CardExpandBox:
				if (gridGroupLayoutState._gridGroupLayoutStateOwner != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null)
				{
					return gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Selected;
				}
				return false;
			case LayoutItemType.EmptyCell:
				if (gridGroupLayoutState._gridGroupLayoutStateOwner != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.GroupFooter)
				{
					return gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Selected;
				}
				return false;
			default:
				return false;
			}
		}

		private bool IsCellButtonVisible(GridGroupLayoutState gridGroupLayoutState, CellButtonDisplayMode cellButtonDisplayMode)
		{
			switch (cellButtonDisplayMode)
			{
			case CellButtonDisplayMode.CurrentRow:
				if (_row != null)
				{
					return gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow() == _row.GetLogicalRow();
				}
				return false;
			case CellButtonDisplayMode.Always:
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord)
				{
					return gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow == _row;
				}
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.FilterRow)
				{
					return gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow == _row;
				}
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow() is DataMordexRow)
				{
					return false;
				}
				return true;
			case CellButtonDisplayMode.EditingCell:
				if (_gridColumnContainerControl != null && _editDelayTimer == null)
				{
					if (_row != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow() == _row.GetLogicalRow())
					{
						return gridGroupLayoutState._column == _currentColumn;
					}
					return false;
				}
				if (_currentColumn != null)
				{
					GridCell gridCell = gridGroupLayoutState.GetCell();
					if (gridCell != null && gridCell.EditType != EditType.NoEdit)
					{
						if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow == _row && gridGroupLayoutState._column == _currentColumn)
						{
							return EditMode == EditMode.EditOn;
						}
						return false;
					}
					return false;
				}
				return false;
			default:
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow == _row)
				{
					return gridGroupLayoutState._column == _currentColumn;
				}
				return false;
			}
		}

		private void CalculateLayoutUntilComplete()
		{
			if (!_isLayoutValid && base.IsHandleCreated)
			{
				Graphics graphics = CreateGraphics();
				while (!_isLayoutValid)
				{
					CalculateLayoutStates(graphics);
				}
				graphics.Dispose();
			}
		}

		internal Rectangle GetGroupByBoxInfoBounds(bool condition)
		{
			if (View != View.TableView)
			{
				return Rectangle.Empty;
			}
			CalculateLayoutUntilComplete();
			if (_headerRowLayouts != null)
			{
				foreach (MxRowLayoutState item in _headerRowLayouts)
				{
					if (item._rowType != LayoutRowType.GroupByBox)
					{
						continue;
					}
					if (condition)
					{
						foreach (GridGroupLayoutState arrayListItem in item._arrayList)
						{
							if (arrayListItem._type == LayoutItemType.GroupByBoxInfo)
							{
								Rectangle result = AdjustRectangleToViewport(arrayListItem.GetAbsoluteItemRect(), arrayListItem._isHorizontallyScrolled);
								result.Location = PointToScreen(result.Location);
								return result;
							}
						}
						continue;
					}
					Rectangle groupByBoxInfoBounds = GetGroupByBoxRectangle();
					groupByBoxInfoBounds.Location = PointToScreen(groupByBoxInfoBounds.Location);
					return groupByBoxInfoBounds;
				}
			}
			return Rectangle.Empty;
		}

		internal Rectangle GetGroupByBoxHeaderBounds(GridGroup gridGroup)
		{
			Rectangle result = Rectangle.Empty;
			if (_headerRowLayouts != null)
			{
				MxRowLayoutState mordexRowLayoutState = null;
				foreach (MxRowLayoutState item in _headerRowLayouts)
				{
					if (item._rowType == LayoutRowType.GroupByBox)
					{
						mordexRowLayoutState = item;
						break;
					}
				}
				if (mordexRowLayoutState != null)
				{
					foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
					{
						if (arrayListItem._type == LayoutItemType.GroupByBoxHeader && arrayListItem._gridGroup == gridGroup)
						{
							result = AdjustRectangleToViewport(arrayListItem.GetAbsoluteItemRect(), arrayListItem._isHorizontallyScrolled);
							result.Intersect(base.ClientRectangle);
							if (result.Width > 0 && result.Height > 0)
							{
								result.Location = PointToScreen(result.Location);
							}
							else
							{
								result = Rectangle.Empty;
							}
							break;
						}
					}
				}
			}
			return result;
		}
	}
}
