using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable
	{
		internal MxRowLayoutState BuildGroupHeaderRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value)
		{
			bool condition = false;
			foreach (GridCell item in (IEnumerable)gridRow.Cells)
			{
				if (item.Text.Length != 0)
				{
					condition = true;
					break;
				}
			}
			int nextItemX = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.GroupHeaderRow, this);
			if (_rowHeight == -1)
			{
				int height = 0;
				height = DataGrid.GetImageSize().Height;
				if (gridRow.GroupImage != null)
				{
					height = gridRow.GroupImage.Size.Height;
				}
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.Group.GetGroupRowStyle());
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridRow.GetCachedFormatStyle());
				mordexRowLayoutState._rowHeight = (int)otherApplyOverrides.ResolveFont(DataGrid.Font).GetHeight(graphics) + 6;
				if (mordexRowLayoutState._rowHeight < height + 4)
				{
					mordexRowLayoutState._rowHeight = height + 4;
				}
				if (mordexRowLayoutState._rowHeight < 14)
				{
					mordexRowLayoutState._rowHeight = 14;
				}
				if (otherApplyOverrides != applyOverrides)
				{
					otherApplyOverrides.DisposeResources();
				}
				if (applyOverrides != gridFormatStyle)
				{
					applyOverrides.DisposeResources();
				}
				if (gridFormatStyle != formatStyle)
				{
					gridFormatStyle.DisposeResources();
				}
			}
			else
			{
				mordexRowLayoutState._rowHeight = _rowHeight;
			}
			ApplyIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow());
			foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
			{
				arrayListItem._isHorizontallyScrolled = value == 0;
			}
			int width = CalculateIndentWidth(true) + CalculateColumnsWidth();
			if (AreRowHeadersVisible())
			{
				width += RowHeaderWidth;
			}
			width -= mordexRowLayoutState._nextItemX;
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: LayoutItemType.GroupHeaderRow, owner: mordexRowLayoutState, column: gridRow.Group.Column);
			gridGroupLayoutState._isHorizontallyScrolled = value == 0;
			if (UsesGroupRowSelector())
			{
				GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowSelector, mordexRowLayoutState, null);
				groupLayoutState._rect = new Rectangle(mordexRowLayoutState._nextItemX + 2, mordexRowLayoutState._top, 16, mordexRowLayoutState._rowHeight);
				gridGroupLayoutState._children = new GridGroupLayoutState[1] { groupLayoutState };
				gridGroupLayoutState._leftMargin += 18;
				groupLayoutState._isHorizontallyScrolled = value == 0;
			}
			if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
			{
				gridGroupLayoutState._borderFlags = BorderFlags.Right;
			}
			if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
			{
				gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
			}
			nextItemX = ((mordexRowLayoutState._arrayList.Count != 0) ? (((GridGroupLayoutState)mordexRowLayoutState._arrayList[mordexRowLayoutState._arrayList.Count - 1])._rect.Left - 1) : 0);
			mordexRowLayoutState._nextItemX += CalculateIndentWidth(true);
			mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			if (condition)
			{
				BuildGroupHeaderCellLayouts(graphics, gridRow, mordexRow, formatStyle, value, mordexRowLayoutState, gridGroupLayoutState);
			}
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
			mordexRowLayoutState._top = -1;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal NewRowPosition ComputeNewRowPosition()
		{
			if (NewRowPosition == NewRowPosition.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.ComputeNewRowPosition();
				}
				return NewRowPosition.TopRow;
			}
			return NewRowPosition;
		}

		internal int CalculateColumnHeaderHeight(Graphics graphics, GridFormatStyle formatStyle, bool condition)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumns)
			{
				if (!condition && !AreColumnHeadersVisible() && !IsTableHeaderVisible())
				{
					return 0;
				}
				int value = GridHelper.RoundToInt(formatStyle.ResolveFont(DataGrid.Font).GetHeight(graphics)) * _headerLines + 2;
				if (value < DataGrid.GetImageSize().Height + 2)
				{
					value = DataGrid.GetImageSize().Height + 2;
				}
				if (AreColumnHeadersVisible())
				{
					foreach (GridColumn column in Columns)
					{
						if (column.HeaderStyle != null)
						{
							GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(column.HeaderStyle);
							int otherValue = GridHelper.RoundToInt(gridFormatStyle.ResolveFont(DataGrid.Font).GetHeight(graphics)) * _headerLines + 5;
							if (otherValue > value)
							{
								value = otherValue;
							}
							if (gridFormatStyle != formatStyle)
							{
								gridFormatStyle.DisposeResources();
							}
						}
					}
				}
				return value + 3;
			}
			if (!AreColumnHeadersVisible() && !AreColumnSetHeadersVisible() && !IsTableHeaderVisible())
			{
				return 0;
			}
			MxRowLayoutState mordexRowLayoutState = BuildColumnSetHeaderLayout(graphics, null, null, formatStyle, 0, false);
			return mordexRowLayoutState._rowHeight;
		}

		internal int CalculateColumnSetHeaderHeight(Graphics graphics, GridFormatStyle formatStyle)
		{
			int value = GridHelper.RoundToInt(formatStyle.ResolveFont(DataGrid.Font).GetHeight(graphics)) * _headerLines + 2;
			if (value < DataGrid.GetImageSize().Height + 2)
			{
				value = DataGrid.GetImageSize().Height + 2;
			}
			foreach (GridColumnSet columnSet in ColumnSets)
			{
				if (columnSet.HeaderStyle != null)
				{
					GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(columnSet.HeaderStyle);
					int otherValue = GridHelper.RoundToInt(gridFormatStyle.ResolveFont(DataGrid.Font).GetHeight(graphics)) * _headerLines + 5;
					if (otherValue > value)
					{
						value = otherValue;
					}
					gridFormatStyle.DisposeResources();
				}
			}
			return value + 2;
		}

		internal int CalculateTableHeaderHeight(Graphics graphics)
		{
			if (!IsTableHeaderVisible())
			{
				return 0;
			}
			GridFormatStyle gridFormatStyle = DataGrid.GetTableHeaderStyle(this);
			int value = GridHelper.RoundToInt(gridFormatStyle.ResolveFont(DataGrid.Font).GetHeight(graphics)) + 5;
			if (value < DataGrid.GetImageSize().Height + 2)
			{
				value = DataGrid.GetImageSize().Height + 2;
			}
			return value + 2;
		}

		internal void FitToWidth(int value)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumns)
			{
				FitColumnsToWidth(value);
			}
			else
			{
				FitColumnSetsToWidth(value);
			}
		}

		internal void AdjustColumnWidths(GridColumn otherColumn, int width)
		{
			if (DataGrid == null || _cellLayoutMode != CellLayoutMode.UseColumns)
			{
				return;
			}
			int value = 0;
			int otherValue = 0;
			int nextValue = 0;
			GridColumn[] array = new GridColumn[Columns.Count];
			GridColumn gridColumn = null;
			foreach (GridColumn column in Columns)
			{
				array[column._position] = column;
				if (column.Visible)
				{
					if (gridColumn == null || gridColumn._position < column._position)
					{
						gridColumn = column;
					}
					nextValue += column._width;
				}
			}
			if (nextValue == 0)
			{
				return;
			}
			int previousValue = otherColumn.Position + 1;
			if (otherColumn == gridColumn)
			{
				return;
			}
			for (int i = previousValue; i < array.Length; i++)
			{
				GridColumn arrayItem = array[i];
				if (arrayItem.Visible && arrayItem != otherColumn)
				{
					otherValue += array[i]._width;
					value = ((!arrayItem.AllowSize) ? (value + arrayItem._width) : (value + 8));
				}
			}
			if (width > otherValue + otherColumn._width - value)
			{
				width = otherValue + otherColumn._width - value;
			}
			int firstValue = otherColumn._width - width;
			int secondValue = 0;
			for (int j = previousValue; j < array.Length; j++)
			{
				GridColumn otherArrayItem = array[j];
				if (otherArrayItem.Visible && otherArrayItem.AllowSize && otherArrayItem != otherColumn)
				{
					int otherWidth = (int)((float)firstValue * ((float)otherArrayItem._width / (float)otherValue));
					if (otherArrayItem._width + otherWidth >= 8)
					{
						otherArrayItem._width += otherWidth;
						secondValue += otherWidth;
					}
					else
					{
						secondValue += 8 - otherArrayItem._width;
						otherArrayItem._width = 8;
					}
				}
			}
			while (secondValue != firstValue)
			{
				int thirdValue = 0;
				for (int k = previousValue; k < array.Length; k++)
				{
					GridColumn nextArrayItem = array[k];
					if (secondValue != firstValue && nextArrayItem.Visible && nextArrayItem.AllowSize && nextArrayItem != otherColumn)
					{
						int nextWidth = 1;
						if (firstValue < 0)
						{
							nextWidth = -1;
						}
						if (nextArrayItem._width + nextWidth >= 8)
						{
							thirdValue++;
							nextArrayItem._width += nextWidth;
							secondValue += nextWidth;
						}
					}
				}
				if (thirdValue == 0)
				{
					break;
				}
			}
			otherColumn._width = width;
			RefreshDataGrid();
		}

		private void FitColumnsToWidth(int value)
		{
			_isAutoResizePending = false;
			int indentWidth = CalculateIndentWidth(true);
			if (AreRowHeadersVisible())
			{
				indentWidth += RowHeaderWidth;
			}
			int otherValue = 0;
			int nextValue = 0;
			int previousValue = 0;
			int firstValue = 0;
			foreach (GridColumn column in Columns)
			{
				if (column.Visible)
				{
					firstValue++;
					previousValue += column._width;
					if (column.AllowSize)
					{
						otherValue += 8;
						nextValue += column._width;
					}
					else
					{
						otherValue += column._width;
					}
				}
			}
			if (previousValue == 0 || previousValue + indentWidth == value)
			{
				return;
			}
			if (firstValue == 1 && GetBaseIsSelfReferencingTable() && GetExpandColumn().Visible)
			{
				GridColumn expandColumn = GetExpandColumn();
				if (SelfReferencingSettings.AutoSizeColumnOnExpand)
				{
					DataGrid.AutoSizeColumnToContent(expandColumn, false);
					_isAutoResizePending = false;
					return;
				}
			}
			otherValue += indentWidth;
			if (otherValue >= value)
			{
				foreach (GridColumn otherColumn in Columns)
				{
					if (otherColumn.Visible && otherColumn.AllowSize)
					{
						otherColumn._width = 8;
					}
				}
			}
			else
			{
				previousValue += indentWidth;
				int secondValue = value - previousValue;
				int thirdValue = 0;
				foreach (GridColumn nextColumn in Columns)
				{
					if (nextColumn.Visible && nextColumn.AllowSize)
					{
						int width = (int)((float)secondValue * ((float)nextColumn._width / (float)nextValue));
						if (nextColumn._width + width >= 8)
						{
							nextColumn._width += width;
							thirdValue += width;
						}
						else
						{
							thirdValue += 8 - nextColumn._width;
							nextColumn._width = 8;
						}
					}
				}
				GridColumn[] array = new GridColumn[Columns.Count];
				foreach (GridColumn previousColumn in Columns)
				{
					if (previousColumn._position == -1)
					{
						array[previousColumn.Index] = previousColumn;
					}
					else
					{
						array[previousColumn._position] = previousColumn;
					}
				}
				while (thirdValue != secondValue)
				{
					int fourthValue = 0;
					for (int i = Columns.Count - 1; i >= 0; i--)
					{
						GridColumn arrayItem = array[i];
						if (thirdValue != secondValue && arrayItem.Visible && arrayItem.AllowSize)
						{
							int otherWidth = 1;
							if (secondValue < 0)
							{
								otherWidth = -1;
							}
							if (arrayItem._width + otherWidth >= 8)
							{
								fourthValue++;
								arrayItem._width += otherWidth;
								thirdValue += otherWidth;
							}
						}
					}
					if (fourthValue == 0)
					{
						break;
					}
				}
			}
			RefreshDataGrid();
		}

		private void FitColumnSetsToWidth(int value)
		{
			_isAutoResizePending = false;
			int indentWidth = CalculateIndentWidth(true);
			if (AreRowHeadersVisible())
			{
				indentWidth += RowHeaderWidth;
			}
			int otherValue = 0;
			int nextValue = 0;
			int previousValue = 0;
			foreach (GridColumnSet item in _columnSets)
			{
				if (item.Visible)
				{
					if (item.AllowSize)
					{
						previousValue += item.Width;
						otherValue += item.ColumnCount * 8;
						nextValue += item.Width;
					}
					else
					{
						previousValue += item.Width;
						otherValue += item.Width;
					}
				}
			}
			if (previousValue == 0 || previousValue == value)
			{
				return;
			}
			otherValue += indentWidth;
			if (otherValue >= value)
			{
				foreach (GridColumnSet columnSet in _columnSets)
				{
					if (columnSet.Visible && columnSet.AllowSize)
					{
						columnSet.Width = columnSet.ColumnCount * 8;
					}
				}
			}
			else
			{
				previousValue += indentWidth;
				int firstValue = value - previousValue;
				int secondValue = 0;
				foreach (GridColumnSet otherColumnSet in _columnSets)
				{
					if (otherColumnSet.Visible && otherColumnSet.AllowSize)
					{
						int width = (int)((float)firstValue * ((float)otherColumnSet.Width / (float)nextValue));
						if (otherColumnSet.Width + width >= otherColumnSet.ColumnCount * 8)
						{
							otherColumnSet.Width += width;
							secondValue += width;
						}
						else
						{
							secondValue += otherColumnSet.ColumnCount * 8 - otherColumnSet.Width;
							otherColumnSet.Width = 8;
						}
					}
				}
				GridColumnSet[] array = new GridColumnSet[_columnSets.Count];
				foreach (GridColumnSet nextColumnSet in _columnSets)
				{
					array[nextColumnSet._position] = nextColumnSet;
				}
				while (secondValue != firstValue)
				{
					int thirdValue = 0;
					for (int i = _columnSets.Count - 1; i >= 0; i--)
					{
						GridColumnSet arrayItem = array[i];
						if (secondValue != firstValue && arrayItem.Visible && arrayItem.AllowSize)
						{
							int otherWidth = 1;
							if (firstValue < 0)
							{
								otherWidth = -1;
							}
							if (arrayItem.Width + otherWidth >= arrayItem.ColumnCount * 8)
							{
								thirdValue++;
								arrayItem.Width += otherWidth;
								secondValue += otherWidth;
							}
						}
					}
					if (thirdValue == 0)
					{
						break;
					}
				}
			}
			RefreshDataGrid();
		}

		internal void InitializePrintLayout(MxDataGridPrintDocument gridPrintDocument, Rectangle rectangle)
		{
			if (_printLayout != null)
			{
				return;
			}
			_printLayout = new ColumnLayoutMap(this);
			int indentWidth = CalculateIndentWidth(gridPrintDocument.PrintHierarchical);
			if (AreRowHeadersVisible())
			{
				indentWidth += RowHeaderWidth;
			}
			if (gridPrintDocument.FitColumns == FitColumnsMode.NoFit)
			{
				_printLayout.BuildWrappedLines(indentWidth, rectangle.Width, gridPrintDocument.ExpandFarColumn);
				return;
			}
			_printLayout.BuildUnwrappedLines(indentWidth, gridPrintDocument.ExpandFarColumn);
			if (gridPrintDocument.FitColumns == FitColumnsMode.SizingColumns)
			{
				_printLayout._gridState18Array[0].FitWidthsTo(rectangle.Width);
			}
		}

		internal MxRowLayoutState BuildPrintDataRowLayout(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridRow gridRow, MordexRow mordexRow, GridFormatStyle otherGridFormatStyle)
		{
			int nextItemX = 0;
			int value = ((_cellLayoutMode == CellLayoutMode.UseColumns) ? 1 : _columnSetRowCount);
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int[] array = new int[value];
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.DataRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = otherGridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
				Font otherFont = applyOverrides.ResolveFont(font);
				int arrayItem = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				for (int i = 0; i < value; i++)
				{
					array[i] = arrayItem;
					for (int j = 0; j < _printLayout._lineCount; j++)
					{
						ColumnLayoutMap gridState18ArrayItem = _printLayout._gridState18Array[j];
						for (int k = 0; k < gridState18ArrayItem.GetColumnCount(); k++)
						{
							ColumnCellPlacement cell = gridState18ArrayItem.GetCell(i, k);
							if (cell == null || cell._row + cell._rowSpan - 1 != i || cell._col != k)
							{
								continue;
							}
							GridColumn column = cell._column;
							int sumWidths = gridState18ArrayItem.SumWidths(cell._col, cell._col + cell._colSpan - 1);
							GridCell gridCell = gridRow.Cells[column.Index];
							int otherArrayItem = arrayItem;
							if (gridCell.CellDisplayType == ColumnType.BoundImage)
							{
								if (gridCell.ImageSize.IsEmpty)
								{
									if (column.BoundImageSize != Size.Empty)
									{
										otherArrayItem = column.BoundImageSize.Height + column.TopMargin + column.BottomMargin;
									}
									else if (gridCell.Value is Image image)
									{
										otherArrayItem = image.Size.Height + column.TopMargin + column.BottomMargin;
									}
								}
								else
								{
									otherArrayItem = gridCell.ImageSize.Height + column.TopMargin + column.BottomMargin;
								}
							}
							else if (gridCell.CellDisplayType == ColumnType.Link || gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
							{
								GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(column.CellStyle);
								GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridRow.Cells[column.Index].GetEffectiveFormatStyle());
								GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridRow.Cells[column.Index].FormatStyle);
								Font nextFont = previousApplyOverrides.ResolveFont(font);
								if (nextFont != otherFont || column.WordWrap)
								{
									TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(previousApplyOverrides, false);
									otherArrayItem = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, sumWidths - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
									textLayoutRenderer.ReleaseStringFormat();
								}
								if (gridCell.CellDisplayType == ColumnType.ImageAndText && otherArrayItem < height)
								{
									otherArrayItem = height;
								}
								otherArrayItem += column.TopMargin + column.BottomMargin;
								if (previousApplyOverrides != nextApplyOverrides)
								{
									previousApplyOverrides.DisposeResources();
								}
								if (nextApplyOverrides != otherApplyOverrides)
								{
									nextApplyOverrides.DisposeResources();
								}
								if (otherApplyOverrides != applyOverrides)
								{
									otherApplyOverrides.DisposeResources();
								}
							}
							else if (gridCell.CellDisplayType == ColumnType.Image)
							{
								otherArrayItem = height + column.TopMargin + column.BottomMargin;
							}
							else if (gridCell.CellDisplayType == ColumnType.CheckBox)
							{
								otherArrayItem = 14 + column.TopMargin + column.BottomMargin;
							}
							if (column._rowSpan > 1 && value > 1)
							{
								int otherValue = 0;
								for (int columnSetRow = column._columnSetRow; columnSetRow < column._columnSetRow + column._rowSpan - 1; columnSetRow++)
								{
									otherValue += array[columnSetRow];
								}
								otherArrayItem -= otherValue;
							}
							if (otherArrayItem > array[i])
							{
								array[i] = otherArrayItem;
							}
						}
					}
					if (array[i] < height + 2)
					{
						array[i] = height + 2;
					}
					array[i] += 2;
					mordexRowLayoutState._rowHeight += array[i];
				}
				if (applyOverrides != gridFormatStyle)
				{
					applyOverrides.DisposeResources();
				}
				if (gridFormatStyle != otherGridFormatStyle)
				{
					gridFormatStyle.DisposeResources();
				}
			}
			else
			{
				for (int m = 0; m < value; m++)
				{
					array[m] = _rowHeight;
				}
				mordexRowLayoutState._rowHeight = value * _rowHeight;
			}
			BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), true, gridPrintDocument.PrintHierarchical);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				item._rowHeight = 0;
			}
			int count = mordexRowLayoutState._arrayList.Count;
			int otherNextItemX = mordexRowLayoutState._nextItemX;
			int top = mordexRowLayoutState._top;
			int nextValue = 0;
			for (int n = 0; n < _printLayout._lineCount; n++)
			{
				mordexRowLayoutState._top = top;
				ColumnLayoutMap gridState18Argument = _printLayout._gridState18Array[n];
				for (int p = 0; p < value; p++)
				{
					mordexRowLayoutState._rowHeight = array[p];
					mordexRowLayoutState._nextItemX = otherNextItemX;
					for (int q = 0; q < gridState18Argument.GetColumnCount(); q++)
					{
						ColumnCellPlacement gridColumnState3Argument = gridState18Argument.GetCell(p, q);
						if (gridColumnState3Argument != null && gridColumnState3Argument._row == p && gridColumnState3Argument._col == q)
						{
							int width = gridState18Argument.SumWidths(gridColumnState3Argument._col, gridColumnState3Argument._col + gridColumnState3Argument._colSpan - 1);
							int nextArrayItem = 0;
							if (gridColumnState3Argument._rowSpan > 1)
							{
								for (int row = gridColumnState3Argument._row; row < gridColumnState3Argument._row + gridColumnState3Argument._rowSpan; row++)
								{
									nextArrayItem += array[row];
								}
							}
							else
							{
								nextArrayItem = array[p];
							}
							GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, nextArrayItem), type: LayoutItemType.Cell, owner: mordexRowLayoutState, column: gridColumnState3Argument._column);
							gridGroupLayoutState._rowHeight = n;
							gridGroupLayoutState.CopyColumnMargins(gridColumnState3Argument._column);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
								if (mordexRowLayoutState._nextItemX == 0)
								{
									gridGroupLayoutState._borderFlags |= BorderFlags.Left;
								}
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else if (gridColumnState3Argument == null)
						{
							int otherSumWidths = gridState18Argument.SumWidths(q, q);
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherSumWidths, otherHeight);
							GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, null, rectangle);
							groupLayoutState._rowHeight = n;
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								groupLayoutState._borderFlags = BorderFlags.Right;
								if (mordexRowLayoutState._nextItemX == 0)
								{
									groupLayoutState._borderFlags = BorderFlags.Left;
								}
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								groupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += groupLayoutState._rect.Width;
							mordexRowLayoutState._arrayList.Add(groupLayoutState);
						}
						else if (gridColumnState3Argument._row != p)
						{
							mordexRowLayoutState._nextItemX += gridState18Argument.SumWidths(q, q);
						}
					}
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				}
				mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
				if (gridRow.PreviewRowVisible)
				{
					string previewRowText = gridRow.PreviewRowText;
					if (previewRowText.Length > 0)
					{
						GridFormatStyle firstApplyOverrides = otherGridFormatStyle.ApplyOverrides(gridRow.RowStyle);
						GridFormatStyle secondApplyOverrides = firstApplyOverrides.ApplyOverrides(gridRow.GetCachedFormatStyle());
						GridFormatStyle thirdApplyOverrides = secondApplyOverrides.ApplyOverrides(DataGrid.PreviewRowFormatStyle);
						if (_previewRowFormatStyle != null)
						{
							GridFormatStyle fourthApplyOverrides = thirdApplyOverrides.ApplyOverrides(_previewRowFormatStyle);
							thirdApplyOverrides.DisposeResources();
							thirdApplyOverrides = fourthApplyOverrides;
						}
						int secondValue = 0;
						int otherWidth = mordexRowLayoutState._rightEdge - otherNextItemX;
						secondValue = _previewRowLeftMargin + 2;
						if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
						{
							secondValue++;
						}
						otherWidth -= secondValue;
						SizeF sizeF = new SizeF(0f, 0f);
						GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null);
						otherGroupLayoutState._rowHeight = n;
						TextLayoutRenderer otherTextLayoutRenderer = otherGroupLayoutState.ComputeTextLayoutRenderer(otherGridFormatStyle, false);
						otherTextLayoutRenderer.SetWordWrap(true);
						sizeF = otherTextLayoutRenderer.MeasureText(graphics, previewRowText, thirdApplyOverrides.ResolveFont(font), otherWidth);
						otherWidth += secondValue + 1;
						if (n == 0)
						{
							string text = "X";
							for (int i = 1; i < _previewRowLines; i++)
							{
								text += "\n\rX";
							}
							mordexRowLayoutState._rowHeight = (int)sizeF.Height;
							int nextHeight = (int)otherTextLayoutRenderer.MeasureText(graphics, text, thirdApplyOverrides.ResolveFont(font), otherWidth).Height;
							if (mordexRowLayoutState._rowHeight > nextHeight)
							{
								mordexRowLayoutState._rowHeight = nextHeight;
							}
							mordexRowLayoutState._rowHeight += 6;
							nextValue = mordexRowLayoutState._rowHeight;
						}
						else
						{
							mordexRowLayoutState._rowHeight = nextValue;
						}
						Rectangle otherRectangle = new Rectangle(otherNextItemX, mordexRowLayoutState._top, otherWidth, mordexRowLayoutState._rowHeight);
						otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null, otherRectangle);
						otherGroupLayoutState._rowHeight = n;
						if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
						{
							otherGroupLayoutState._borderFlags = BorderFlags.Right;
							if (count == 0 || n != 0)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Left;
							}
						}
						if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
						{
							otherGroupLayoutState._borderFlags |= BorderFlags.Bottom;
						}
						mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
						otherGroupLayoutState._leftMargin = _previewRowLeftMargin;
						otherGroupLayoutState._topMargin = 2;
						otherGroupLayoutState._rightMargin = 2;
						otherGroupLayoutState._bottomMargin = 2;
						mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						if (n == 0)
						{
							for (int j = 0; j < count; j++)
							{
								((GridGroupLayoutState)mordexRowLayoutState._arrayList[j])._rect.Height += mordexRowLayoutState._rowHeight;
							}
						}
						foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
						{
							if (arrayListItem._type == LayoutItemType.Cell && arrayListItem._rect.Bottom == mordexRowLayoutState._top && _cellLayoutMode == CellLayoutMode.UseColumns)
							{
								arrayListItem._drawBottomGridline = false;
							}
						}
						if (thirdApplyOverrides != secondApplyOverrides)
						{
							thirdApplyOverrides.DisposeResources();
						}
						if (secondApplyOverrides != firstApplyOverrides)
						{
							secondApplyOverrides.DisposeResources();
						}
						if (firstApplyOverrides != otherGridFormatStyle)
						{
							firstApplyOverrides.DisposeResources();
						}
						mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
					}
				}
				otherNextItemX = 0;
			}
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildPrintGroupHeaderCellsLayout(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridRow gridRow, MordexRow mordexRow, GridFormatStyle otherGridFormatStyle)
		{
			int nextItemX = 0;
			int value = ((_cellLayoutMode == CellLayoutMode.UseColumns) ? 1 : _columnSetRowCount);
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int[] array = new int[value];
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.DataRow, this);
			if (_rowHeight == -1)
			{
				for (int i = 0; i < value; i++)
				{
					array[i] = -1;
					for (int j = 0; j < _printLayout._lineCount; j++)
					{
						ColumnLayoutMap gridState18ArrayItem = _printLayout._gridState18Array[j];
						for (int k = 0; k < gridState18ArrayItem.GetColumnCount(); k++)
						{
							ColumnCellPlacement cell = gridState18ArrayItem.GetCell(i, k);
							if (cell == null || cell._row + cell._rowSpan - 1 != i || cell._col != k)
							{
								continue;
							}
							GridColumn column = cell._column;
							GridCell gridCell = gridRow.Cells[column.Index];
							int arrayItem = -1;
							if (gridCell.CellDisplayType == ColumnType.BoundImage)
							{
								if (gridCell.ImageSize.IsEmpty)
								{
									if (column.BoundImageSize != Size.Empty)
									{
										arrayItem = column.BoundImageSize.Height + column.TopMargin + column.BottomMargin;
									}
									else if (gridCell.Value is Image image)
									{
										arrayItem = image.Size.Height + column.TopMargin + column.BottomMargin;
									}
								}
								else
								{
									arrayItem = gridCell.ImageSize.Height + column.TopMargin + column.BottomMargin;
								}
							}
							else if (gridCell.CellDisplayType == ColumnType.Link || gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
							{
								GridFormatStyle cellFormatStyle = DataGrid.GetCellFormatStyle(gridCell);
								Font otherFont = cellFormatStyle.ResolveFont(font);
								TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(cellFormatStyle, false);
								arrayItem = GridHelper.CalculateTextHeight(graphics, otherFont, gridCell.Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
								textLayoutRenderer.ReleaseStringFormat();
								if (gridCell.CellDisplayType == ColumnType.ImageAndText && arrayItem < height)
								{
									arrayItem = height;
								}
								arrayItem += column.TopMargin + column.BottomMargin;
							}
							else if (gridCell.CellDisplayType == ColumnType.Image)
							{
								arrayItem = height + column.TopMargin + column.BottomMargin;
							}
							else if (gridCell.CellDisplayType == ColumnType.CheckBox)
							{
								arrayItem = 14 + column.TopMargin + column.BottomMargin;
							}
							if (column._rowSpan > 1 && value > 1)
							{
								int otherValue = 0;
								for (int columnSetRow = column._columnSetRow; columnSetRow < column._columnSetRow + column._rowSpan - 1; columnSetRow++)
								{
									otherValue += array[columnSetRow];
								}
								arrayItem -= otherValue;
							}
							if (arrayItem > array[i])
							{
								array[i] = arrayItem;
							}
						}
					}
					if (array[i] < height + 2)
					{
						array[i] = height + 2;
					}
					array[i] += 2;
					mordexRowLayoutState._rowHeight += array[i];
				}
			}
			else
			{
				for (int m = 0; m < value; m++)
				{
					array[m] = _rowHeight;
				}
				mordexRowLayoutState._rowHeight = value * _rowHeight;
			}
			BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), true, gridPrintDocument.PrintHierarchical);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				item._rowHeight = 0;
			}
			int count = mordexRowLayoutState._arrayList.Count;
			int otherNextItemX = mordexRowLayoutState._nextItemX;
			int top = mordexRowLayoutState._top;
			int nextValue = 0;
			for (int n = 0; n < _printLayout._lineCount; n++)
			{
				mordexRowLayoutState._top = top;
				ColumnLayoutMap gridState18Argument = _printLayout._gridState18Array[n];
				for (int p = 0; p < value; p++)
				{
					mordexRowLayoutState._rowHeight = array[p];
					mordexRowLayoutState._nextItemX = otherNextItemX;
					for (int q = 0; q < gridState18Argument.GetColumnCount(); q++)
					{
						ColumnCellPlacement gridColumnState3Argument = gridState18Argument.GetCell(p, q);
						if (gridColumnState3Argument != null && gridColumnState3Argument._row == p && gridColumnState3Argument._col == q)
						{
							int width = gridState18Argument.SumWidths(gridColumnState3Argument._col, gridColumnState3Argument._col + gridColumnState3Argument._colSpan - 1);
							int otherArrayItem = 0;
							if (gridColumnState3Argument._rowSpan > 1)
							{
								for (int row = gridColumnState3Argument._row; row < gridColumnState3Argument._row + gridColumnState3Argument._rowSpan; row++)
								{
									otherArrayItem += array[row];
								}
							}
							else
							{
								otherArrayItem = array[p];
							}
							GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, otherArrayItem), type: LayoutItemType.Cell, owner: mordexRowLayoutState, column: gridColumnState3Argument._column);
							gridGroupLayoutState._rowHeight = n;
							gridGroupLayoutState.CopyColumnMargins(gridColumnState3Argument._column);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
								if (mordexRowLayoutState._nextItemX == 0)
								{
									gridGroupLayoutState._borderFlags |= BorderFlags.Left;
								}
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else if (gridColumnState3Argument == null)
						{
							int sumWidths = gridState18Argument.SumWidths(q, q);
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, sumWidths, otherHeight);
							GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, null, rectangle);
							groupLayoutState._rowHeight = n;
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								groupLayoutState._borderFlags = BorderFlags.Right;
								if (mordexRowLayoutState._nextItemX == 0)
								{
									groupLayoutState._borderFlags = BorderFlags.Left;
								}
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								groupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += groupLayoutState._rect.Width;
							mordexRowLayoutState._arrayList.Add(groupLayoutState);
						}
						else if (gridColumnState3Argument._row != p)
						{
							mordexRowLayoutState._nextItemX += gridState18Argument.SumWidths(q, q);
						}
					}
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				}
				mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
				if (gridRow.RowType == RowType.Record && gridRow.PreviewRowVisible)
				{
					string previewRowText = gridRow.PreviewRowText;
					if (previewRowText.Length > 0)
					{
						GridFormatStyle gridFormatStyle = otherGridFormatStyle.ApplyOverrides(gridRow.RowStyle);
						GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
						GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(DataGrid.PreviewRowFormatStyle);
						if (_previewRowFormatStyle != null)
						{
							GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(_previewRowFormatStyle);
							otherApplyOverrides.DisposeResources();
							otherApplyOverrides = nextApplyOverrides;
						}
						int secondValue = 0;
						int otherWidth = mordexRowLayoutState._rightEdge - otherNextItemX;
						secondValue = _previewRowLeftMargin + 2;
						if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
						{
							secondValue++;
						}
						otherWidth -= secondValue;
						SizeF sizeF = new SizeF(0f, 0f);
						GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null);
						otherGroupLayoutState._rowHeight = n;
						TextLayoutRenderer otherTextLayoutRenderer = otherGroupLayoutState.ComputeTextLayoutRenderer(otherGridFormatStyle, false);
						otherTextLayoutRenderer.SetWordWrap(true);
						sizeF = otherTextLayoutRenderer.MeasureText(graphics, previewRowText, otherApplyOverrides.ResolveFont(font), otherWidth);
						otherWidth += secondValue + 1;
						if (n == 0)
						{
							string text = "X";
							for (int i = 1; i < _previewRowLines; i++)
							{
								text += "\n\rX";
							}
							mordexRowLayoutState._rowHeight = (int)sizeF.Height;
							int nextHeight = (int)otherTextLayoutRenderer.MeasureText(graphics, text, otherApplyOverrides.ResolveFont(font), otherWidth).Height;
							if (mordexRowLayoutState._rowHeight > nextHeight)
							{
								mordexRowLayoutState._rowHeight = nextHeight;
							}
							mordexRowLayoutState._rowHeight += 6;
							nextValue = mordexRowLayoutState._rowHeight;
						}
						else
						{
							mordexRowLayoutState._rowHeight = nextValue;
						}
						Rectangle otherRectangle = new Rectangle(otherNextItemX, mordexRowLayoutState._top, otherWidth, mordexRowLayoutState._rowHeight);
						otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null, otherRectangle);
						otherGroupLayoutState._rowHeight = n;
						if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
						{
							otherGroupLayoutState._borderFlags = BorderFlags.Right;
							if (count == 0 || n != 0)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Left;
							}
						}
						if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
						{
							otherGroupLayoutState._borderFlags |= BorderFlags.Bottom;
						}
						mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
						otherGroupLayoutState._leftMargin = _previewRowLeftMargin;
						otherGroupLayoutState._topMargin = 2;
						otherGroupLayoutState._rightMargin = 2;
						otherGroupLayoutState._bottomMargin = 2;
						mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						if (n == 0)
						{
							for (int j = 0; j < count; j++)
							{
								((GridGroupLayoutState)mordexRowLayoutState._arrayList[j])._rect.Height += mordexRowLayoutState._rowHeight;
							}
						}
						foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
						{
							if (arrayListItem._type == LayoutItemType.Cell && arrayListItem._rect.Bottom == mordexRowLayoutState._top && _cellLayoutMode == CellLayoutMode.UseColumns)
							{
								arrayListItem._drawBottomGridline = false;
							}
						}
						if (otherApplyOverrides != applyOverrides)
						{
							otherApplyOverrides.DisposeResources();
						}
						if (applyOverrides != gridFormatStyle)
						{
							applyOverrides.DisposeResources();
						}
						if (gridFormatStyle != otherGridFormatStyle)
						{
							gridFormatStyle.DisposeResources();
						}
						mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
					}
				}
				otherNextItemX = 0;
			}
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildPrintGroupHeaderRowLayout(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle)
		{
			bool condition = false;
			foreach (GridCell item in (IEnumerable)gridRow.Cells)
			{
				if (item.Text.Length != 0)
				{
					condition = true;
				}
			}
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				condition = false;
			}
			int nextItemX = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.GroupHeaderRow, this);
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.Group.GetGroupRowStyle());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridRow.GetCachedFormatStyle());
			mordexRowLayoutState._rowHeight = (int)otherApplyOverrides.ResolveFont(DataGrid.Font).GetHeight(graphics) + 4;
			if (mordexRowLayoutState._rowHeight < height + 2)
			{
				mordexRowLayoutState._rowHeight = height + 2;
			}
			if (mordexRowLayoutState._rowHeight < 12)
			{
				mordexRowLayoutState._rowHeight = 12;
			}
			if (otherApplyOverrides != applyOverrides)
			{
				otherApplyOverrides.DisposeResources();
			}
			if (applyOverrides != gridFormatStyle)
			{
				applyOverrides.DisposeResources();
			}
			if (gridFormatStyle != formatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
			BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), false, gridPrintDocument.PrintHierarchical);
			foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
			{
				arrayListItem._rowHeight = 0;
			}
			int totalWidth = _printLayout._gridState18Array[0].GetTotalWidth();
			int width = totalWidth - mordexRowLayoutState._nextItemX;
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: LayoutItemType.GroupHeaderRow, owner: mordexRowLayoutState, column: gridRow.Group.Column);
			gridGroupLayoutState._rowHeight = 0;
			if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
			{
				gridGroupLayoutState._borderFlags = BorderFlags.Right;
			}
			if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
			{
				gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
			}
			nextItemX = ((mordexRowLayoutState._arrayList.Count > 0) ? (((GridGroupLayoutState)mordexRowLayoutState._arrayList[mordexRowLayoutState._arrayList.Count - 1])._rect.Left - 1) : 0);
			mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
			mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			for (int i = 1; i < _printLayout._lineCount; i++)
			{
				mordexRowLayoutState._nextItemX = 0;
				width = _printLayout._gridState18Array[i].GetTotalWidth();
				gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: LayoutItemType.GroupHeaderRow, owner: mordexRowLayoutState, column: gridRow.Group.Column);
				gridGroupLayoutState._rowHeight = i;
				if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
				{
					gridGroupLayoutState._borderFlags = BorderFlags.Left | BorderFlags.Right;
				}
				if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
				{
					gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
				}
				mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			}
			if (condition)
			{
				int value = 0;
				MxRowLayoutState printGroupHeaderCellsLayout = BuildPrintGroupHeaderCellsLayout(graphics, gridPrintDocument, gridRow, mordexRow, formatStyle);
				int indentWidth = CalculateIndentWidth(true);
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
				}
				int maxValue = int.MaxValue;
				foreach (GridGroupLayoutState otherArrayListItem in printGroupHeaderCellsLayout._arrayList)
				{
					if (otherArrayListItem._rowHeight == 0 && otherArrayListItem._type == LayoutItemType.Cell && otherArrayListItem._rect.Left < maxValue)
					{
						maxValue = otherArrayListItem._rect.Left;
					}
				}
				indentWidth -= maxValue;
				foreach (GridGroupLayoutState nextArrayListItem in printGroupHeaderCellsLayout._arrayList)
				{
					if (nextArrayListItem._type != LayoutItemType.Cell)
					{
						continue;
					}
					if (value != nextArrayListItem._rowHeight)
					{
						nextArrayListItem._type = LayoutItemType.GroupHeaderCell;
						if (nextArrayListItem._rowHeight == 0)
						{
							nextArrayListItem._rect.X += indentWidth;
						}
						nextArrayListItem._gridGroupLayoutStateOwner = mordexRowLayoutState;
						mordexRowLayoutState._arrayList.Add(nextArrayListItem);
					}
					else
					{
						value++;
					}
				}
				if (mordexRowLayoutState._rowHeight != printGroupHeaderCellsLayout._rowHeight)
				{
					foreach (GridGroupLayoutState previousArrayListItem in mordexRowLayoutState._arrayList)
					{
						previousArrayListItem._rect.Height = printGroupHeaderCellsLayout._rowHeight;
					}
					mordexRowLayoutState._rowHeight = printGroupHeaderCellsLayout._rowHeight;
				}
			}
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
			mordexRowLayoutState._top = -1;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal bool IsTableHeaderVisible()
		{
			if (_tableHeader == InheritableBoolean.Default)
			{
				if (ParentTable != null)
				{
					return ParentTable.IsTableHeaderVisible();
				}
				if (DataGrid != null)
				{
					return DataGrid.ShouldShowTableHeaders();
				}
			}
			else if (_tableHeader == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}
	}
}
