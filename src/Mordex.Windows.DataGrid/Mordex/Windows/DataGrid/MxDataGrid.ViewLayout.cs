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
		private GridTableLayoutState BuildCardLayoutWithColumnSets(Graphics graphics, bool condition, GridRow gridRow, GridFormatStyle otherGridFormatStyle, GridFormatStyle formatStyle, GridFormatStyle otherFormatStyle, int otherValue)
		{
			int height = 0;
			height = GetImageSize().Height;
			GridTableLayoutState gridTableLayoutState = new GridTableLayoutState(LayoutRowType.DataRow, RootTable);
			gridTableLayoutState._gridRow = gridRow;
			int otherHeight = 0;
			if (_cardHeaders)
			{
				if (gridRow.CardCaptionText.Length > 0 && gridRow.Table.Columns._owner != null)
				{
					TextLayoutRenderer textLayoutRenderer = GridHelper.ComputeTextLayoutRenderer(otherGridFormatStyle, !base.UseCompatibleTextRendering);
					otherHeight = GridHelper.CalculateTextHeight(graphics, otherGridFormatStyle.ResolveFont(Font), gridRow.CardCaptionText, textLayoutRenderer, otherValue - 4, RootTable.Columns._owner.MinLines, RootTable.Columns._owner.MaxLines) + 8;
					textLayoutRenderer.ReleaseStringFormat();
				}
				else
				{
					otherHeight = (int)otherGridFormatStyle.ResolveFont(Font).GetHeight(graphics) + 8;
				}
				gridTableLayoutState._headerHeight = otherHeight;
				GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CardCaption, gridTableLayoutState, null, new Rectangle(0, 0, otherValue + 1, otherHeight));
				gridGroupLayoutState._leftMargin = 2;
				if (RootTable.Columns._gridColumnCollectionOwner != null)
				{
					gridGroupLayoutState._ownImage = gridRow.Cells[RootTable.Columns._gridColumnCollectionOwner.Index].ResolveImage(ComputeImageList());
				}
				if (_showErrors && HasRowError(gridRow))
				{
					gridGroupLayoutState._ownImage = GetErrorImage();
				}
				gridTableLayoutState._arrayList.Add(gridGroupLayoutState);
				if (_expandableCards)
				{
					GridGroupLayoutState value = new GridGroupLayoutState(LayoutItemType.CardExpandBox, gridTableLayoutState, null, new Rectangle(otherValue - otherHeight - 2, 0, otherHeight, otherHeight));
					gridGroupLayoutState._rightMargin = otherHeight + 4;
					gridTableLayoutState._arrayList.Add(value);
				}
				bool visible = false;
				GridColumn column = null;
				foreach (GridColumn otherColumn in RootTable.Columns)
				{
					if (otherColumn.ActAsSelector)
					{
						visible = otherColumn.Visible;
						column = otherColumn;
						break;
					}
				}
				if (visible)
				{
					gridGroupLayoutState._leftMargin += gridGroupLayoutState._rect.Height + 2;
					Rectangle bounds = gridGroupLayoutState._rect;
					bounds.Width = bounds.Height;
					bounds.X = 2;
					GridGroupLayoutState value = new GridGroupLayoutState(LayoutItemType.CardSelector, gridTableLayoutState, column, bounds);
					gridTableLayoutState._arrayList.Add(value);
					gridTableLayoutState._gridGroupLayoutState = value;
				}
			}
			int nextHeight = otherHeight + _cardInnerSpacing;
			int cardInnerSpacing = _cardInnerSpacing;
			if (condition || RowStateHelper.GetRowTag(gridRow.GetLogicalRow()).GetIsExpanded() || !_expandableCards)
			{
				otherValue++;
				GridFormatStyle gridFormatStyle = otherFormatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
				Font font = applyOverrides.ResolveFont(Font);
				GridHelper.RoundToInt(font.GetHeight(graphics));
				GridColumnSet[] array = new GridColumnSet[RootTable.ColumnSets.Count];
				foreach (GridColumnSet columnSet in RootTable.ColumnSets)
				{
					array[columnSet._position] = columnSet;
				}
				foreach (GridColumnSet arrayItem in array)
				{
					if (!arrayItem.Visible)
					{
						continue;
					}
					_ = arrayItem.Width;
					int[] values = new int[RootTable.ColumnSetRowCount];
					int[] otherValues = new int[arrayItem.ColumnCount];
					for (int j = 0; j < RootTable.ColumnSetRowCount; j++)
					{
						for (int k = 0; k < arrayItem.ColumnCount; k++)
						{
							GridColumn gridColumn = arrayItem[j, k];
							if (gridColumn != null && gridColumn.Visible && gridColumn.CardViewCaptionVisible && gridColumn._columnSetColumn == k && gridColumn._columnSetRow == j)
							{
								int columnHeaderHeight = CalculateColumnHeaderHeight(graphics, formatStyle, gridColumn);
								if (columnHeaderHeight > otherValues[k])
								{
									otherValues[k] = columnHeaderHeight;
								}
							}
						}
					}
					for (int i = 0; i < RootTable.ColumnSetRowCount; i++)
					{
						values[i] = (int)applyOverrides.ResolveFont(Font).GetHeight(graphics);
						for (int m = 0; m < arrayItem.ColumnCount; m++)
						{
							GridColumn nextColumn = arrayItem[i, m];
							if (nextColumn == null || !nextColumn.Visible || nextColumn.GetLastColumnSetRow() != i || nextColumn._columnSetColumn != m)
							{
								continue;
							}
							int layoutWidth = otherValue / arrayItem.ColumnCount * nextColumn.ColSpan;
							if (m + nextColumn.ColSpan == arrayItem.ColumnCount)
							{
								layoutWidth += otherValue % arrayItem.ColumnCount;
							}
							nextColumn._layoutWidth = layoutWidth;
							int nextValue = 0;
							if (nextColumn.RowSpan == 1)
							{
								nextValue = values[i];
							}
							else
							{
								for (int n = i; n >= i - nextColumn._rowSpan + 1; n--)
								{
									nextValue += values[n];
								}
							}
							GridCell gridCell = gridRow.Cells[nextColumn.Index];
							if (gridCell.CellDisplayType == ColumnType.BoundImage)
							{
								if (gridCell.ImageSize.IsEmpty)
								{
									if (nextColumn.BoundImageSize != Size.Empty)
									{
										nextValue = nextColumn.BoundImageSize.Height + nextColumn.TopMargin + nextColumn.BottomMargin;
									}
									else if (gridCell.Value is Image image)
									{
										nextValue = image.Size.Height + nextColumn.TopMargin + nextColumn.BottomMargin;
									}
								}
								else
								{
									nextValue = gridCell.ImageSize.Height + nextColumn.TopMargin + nextColumn.BottomMargin;
								}
							}
							else if (gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
							{
								GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(nextColumn.CellStyle);
								GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridCell.GetEffectiveFormatStyle());
								GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridCell.FormatStyle);
								Font otherFont = previousApplyOverrides.ResolveFont(Font);
								TextLayoutRenderer otherTextLayoutRenderer = nextColumn.ComputeTextLayoutRenderer(previousApplyOverrides, !base.UseCompatibleTextRendering);
								nextValue = GridHelper.CalculateTextHeight(graphics, otherFont, gridCell.Text, otherTextLayoutRenderer, layoutWidth - otherValues[m] - nextColumn.LeftMargin - nextColumn.RightMargin - 2, nextColumn.MinLines, nextColumn.MaxLines);
								otherTextLayoutRenderer.ReleaseStringFormat();
								if (gridCell.CellDisplayType == ColumnType.ImageAndText && nextValue < height)
								{
									nextValue = height;
								}
								nextValue += nextColumn.TopMargin + nextColumn.BottomMargin;
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
								int previousHeight = height;
								Image otherImage = gridCell.Image;
								if (otherImage != null)
								{
									previousHeight = otherImage.Height;
								}
								nextValue = previousHeight + nextColumn.TopMargin + nextColumn.BottomMargin;
							}
							else if (gridCell.CellDisplayType == ColumnType.CheckBox)
							{
								nextValue = 14 + nextColumn.TopMargin + nextColumn.BottomMargin;
							}
							nextColumn._layoutHeight = nextValue + 2;
							if (nextColumn._rowSpan > 1)
							{
								int firstValue = 0;
								for (int n = nextColumn._columnSetRow; n < nextColumn._columnSetRow + nextColumn._rowSpan - 1; n++)
								{
									firstValue += values[n];
								}
								nextValue -= firstValue;
							}
							if (nextValue > values[i])
							{
								values[i] = nextValue;
							}
						}
						values[i] += 2;
					}
					for (int p = 0; p < RootTable.ColumnSetRowCount; p++)
					{
						cardInnerSpacing = _cardInnerSpacing;
						for (int q = 0; q < arrayItem.ColumnCount; q++)
						{
							GridColumn previousColumn = arrayItem[p, q];
							if (previousColumn != null && previousColumn.Visible && previousColumn._columnSetColumn == q && previousColumn._columnSetRow == p)
							{
								if (previousColumn._rowSpan > 1)
								{
									int layoutHeight = 0;
									for (int fourthValue = p; fourthValue < p + previousColumn._rowSpan; fourthValue++)
									{
										layoutHeight += values[fourthValue];
									}
									layoutHeight += _cardInnerSpacing * (previousColumn._rowSpan - 1);
									if (layoutHeight > previousColumn._layoutHeight)
									{
										previousColumn._layoutHeight = layoutHeight;
									}
								}
								GridGroupLayoutState value;
								if (previousColumn.CardViewCaptionVisible && cardInnerSpacing < otherValue)
								{
									value = new GridGroupLayoutState(LayoutItemType.CardHeader, gridTableLayoutState, previousColumn);
									switch (_cardViewGridlines)
									{
									case CardViewGridlines.Both:
										value._borderFlags = BorderFlags.Right | BorderFlags.Bottom;
										if (_cardInnerSpacing > 0)
										{
											value._borderFlags |= BorderFlags.Left | BorderFlags.Top;
										}
										break;
									case CardViewGridlines.Vertical:
										value._borderFlags = BorderFlags.Right;
										if (_cardInnerSpacing > 0)
										{
											value._borderFlags |= BorderFlags.Left;
										}
										break;
									case CardViewGridlines.Horizontal:
										value._borderFlags = BorderFlags.Bottom;
										if (_cardInnerSpacing > 0)
										{
											value._borderFlags |= BorderFlags.Top;
										}
										break;
									}
									int width = otherValues[q];
									if (cardInnerSpacing + width > otherValue)
									{
										width = otherValue - cardInnerSpacing;
									}
									if (width > 0)
									{
										value._rect = new Rectangle(cardInnerSpacing, nextHeight, width, previousColumn._layoutHeight);
										if (previousColumn.RowSpan == 1)
										{
											value._rect.Height = values[p];
										}
										gridTableLayoutState._arrayList.Add(value);
										cardInnerSpacing += value._rect.Width + _cardInnerSpacing;
									}
								}
								if (cardInnerSpacing >= otherValue)
								{
									continue;
								}
								value = new GridGroupLayoutState(LayoutItemType.CardCell, gridTableLayoutState, previousColumn);
								switch (_cardViewGridlines)
								{
								case CardViewGridlines.FieldsOnly:
									value._borderFlags = BorderFlags.All;
									break;
								case CardViewGridlines.Both:
									value._borderFlags = BorderFlags.Right | BorderFlags.Bottom;
									if (_cardInnerSpacing > 0)
									{
										value._borderFlags |= BorderFlags.Left | BorderFlags.Top;
									}
									break;
								case CardViewGridlines.Vertical:
									value._borderFlags = BorderFlags.Right;
									if (_cardInnerSpacing > 0)
									{
										value._borderFlags |= BorderFlags.Left;
									}
									break;
								case CardViewGridlines.Horizontal:
									value._borderFlags = BorderFlags.Bottom;
									if (_cardInnerSpacing > 0)
									{
										value._borderFlags |= BorderFlags.Top;
									}
									break;
								}
								int gridColumnInteger = previousColumn._layoutWidth;
								if (previousColumn.CardViewCaptionVisible)
								{
									gridColumnInteger -= otherValues[q] + _cardInnerSpacing * 2;
									if (previousColumn._columnSetColumn + previousColumn._colSpan == arrayItem.ColumnCount)
									{
										gridColumnInteger -= _cardInnerSpacing;
									}
								}
								else
								{
									gridColumnInteger -= _cardInnerSpacing * 2;
								}
								if (gridColumnInteger + cardInnerSpacing >= otherValue)
								{
									gridColumnInteger = otherValue - cardInnerSpacing - 1;
								}
								if (gridColumnInteger > 0)
								{
									value._rect = new Rectangle(cardInnerSpacing, nextHeight, gridColumnInteger, previousColumn._layoutHeight);
									if (previousColumn.RowSpan == 1)
									{
										value._rect.Height = values[p];
									}
									value.CopyColumnMargins(previousColumn);
									gridTableLayoutState._arrayList.Add(value);
									cardInnerSpacing += value._rect.Width + _cardInnerSpacing;
								}
							}
							else if (previousColumn == null || !previousColumn.Visible || previousColumn._columnSetRow != p)
							{
								cardInnerSpacing += otherValue / arrayItem.ColumnCount;
							}
						}
						nextHeight += values[p] + _cardInnerSpacing;
					}
				}
			}
			if (_cardViewGridlines == CardViewGridlines.Both || _cardViewGridlines == CardViewGridlines.Horizontal || CardViewGridlines == CardViewGridlines.FieldsOnly)
			{
				nextHeight--;
			}
			gridTableLayoutState._contentSize = new Size(otherValue - 1, nextHeight);
			return gridTableLayoutState;
		}

		private void LayoutTableView(Graphics graphics)
		{
			LayoutTableViewRows(graphics, false);
		}

		private void LayoutTableViewRows(Graphics graphics, bool condition)
		{
			_bodyTopY = 0;
			GridTable gridTable = RootTable;
			_isLayoutValid = true;
			long ticks = DateTime.Now.Ticks;
			int top = base.ClientRectangle.Top;
			Rectangle clientRectangle = base.ClientRectangle;
			_bodyBottomY = clientRectangle.Bottom;
			if (_columnAutoResize)
			{
				foreach (GridTable item in _gridTableCollection)
				{
					if (item._isAutoResizePending)
					{
						item.FitToWidth(clientRectangle.Width);
					}
				}
				OnFitColumns(EventArgs.Empty);
			}
			if (_bodyRowLayouts == null)
			{
				_bodyRowLayouts = new RowLayoutList();
			}
			if (_headerRowLayouts == null)
			{
				_headerRowLayouts = new RowLayoutList();
			}
			if (_footerRowLayouts == null)
			{
				_footerRowLayouts = new RowLayoutList();
			}
			_bodyRowLayouts.ClearRows(_previousBodyRowLayouts == null);
			_headerRowLayouts.ClearAll();
			_footerRowLayouts.ClearAll();
			if (RootTable != null)
			{
				_contentWidth = RootTable.CalculateHierarchyWidth(false);
				if (_contentWidth > clientRectangle.Width)
				{
					int value = 0;
					if (GetEffectiveFrozenColumns() > 0)
					{
						value += _groupIndent * RootTable.Groups.Count;
						if (RootTable.CellLayoutMode == CellLayoutMode.UseColumns)
						{
							for (int i = 0; i < GetEffectiveFrozenColumns(); i++)
							{
								GridColumn columnInPosition = RootTable.Columns.GetColumnInPosition(i);
								if (columnInPosition.Visible)
								{
									value += columnInPosition.Width;
								}
							}
						}
						else
						{
							for (int j = 0; j < GetEffectiveFrozenColumns(); j++)
							{
								if (j < RootTable.ColumnSets.Count)
								{
									GridColumnSet columnSetInPosition = RootTable.ColumnSets.GetColumnSetInPosition(j);
									if (columnSetInPosition.Visible)
									{
										value += columnSetInPosition.Width;
									}
								}
							}
						}
					}
					int otherValue = clientRectangle.Width - value;
					ConfigureHorizontalScrollBar(0, _contentWidth - value - 1, otherValue, _horizontalScrollOffset);
					if (!IsHorizontalScrollRequired())
					{
						_isLayoutValid = false;
					}
					base.SetHorizontalScrollRequired(true);
					base.HorizontalScroll.SmallChange = 8;
					if (!_isLayoutValid)
					{
						return;
					}
				}
				else
				{
					SetHorizontalScrollOffset(0);
					if (IsHorizontalScrollRequired())
					{
						_isLayoutValid = false;
					}
					base.SetHorizontalScrollRequired(false);
					if (!_isLayoutValid)
					{
						return;
					}
				}
				top += LayoutGroupByBox(_headerRowLayouts, graphics);
				if (RootTable.HasAnyHeaderRow())
				{
					GridFormatStyle gridFormatStyle = GetHeaderStyle(RootTable);
					MxRowLayoutState mordexRowLayoutState = RootTable.BuildColumnHeaderLayout(graphics, null, null, gridFormatStyle, GetEffectiveFrozenColumns(), false);
					gridFormatStyle.DisposeResources();
					mordexRowLayoutState.SetLocation(new Point(base.ClientRectangle.Left, top));
					top = (_bodyTopY = top + mordexRowLayoutState._rowHeight);
					if (_groupByBoxVisible)
					{
						mordexRowLayoutState._isFirstRow = true;
					}
					_headerRowLayouts.AddRow(mordexRowLayoutState);
				}
				if (_filterMode != FilterMode.None)
				{
					GridFormatStyle gridFormatStyle = GetFilterRowStyle(RootTable);
					MxRowLayoutState filterRowLayout = RootTable.BuildFilterRowLayout(graphics, gridFormatStyle, GetEffectiveFrozenColumns());
					filterRowLayout._gridRow = GetFilterGridRow();
					gridFormatStyle.DisposeResources();
					filterRowLayout.SetLocation(new Point(base.ClientRectangle.Left, top));
					top = (_bodyTopY = top + filterRowLayout._rowHeight);
					_headerRowLayouts.AddRow(filterRowLayout);
				}
				int firstRow = _firstRow;
				if (condition)
				{
					_firstRow = 0;
				}
				MordexRowManager mordexRowManager = GetRowManager();
				_rowCache.MarkOtherRowsStale(_row);
				if (RootTable.IsAddNewAllowed() && RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow && _gridDataSourceManager.GetRootNewRow() != null)
				{
					GridRow gridRow = GetGridRowForLogicalRow(_gridDataSourceManager.GetRootNewRow());
					gridRow._isLayoutDirty = false;
					PrepareRowForDisplay(gridRow, false);
					GridFormatStyle gridFormatStyle = GetNewRowStyle(RootTable);
					MxRowLayoutState newRowLayout = RootTable.BuildNewRowLayout(graphics, gridRow, null, gridFormatStyle, GetEffectiveFrozenColumns());
					gridFormatStyle.DisposeResources();
					newRowLayout._gridRow = gridRow;
					newRowLayout.SetLocation(new Point(base.ClientRectangle.Left, top));
					top += newRowLayout._rowHeight;
					_headerRowLayouts.AddRow(newRowLayout);
					_bodyTopY = top;
				}
				if (_totalRowPosition == TotalRowPosition.BottomFixed && _gridDataSourceManager.GetRootTotalRow() != null)
				{
					GridRow gridRow = GetGridRowForLogicalRow(_gridDataSourceManager.GetRootTotalRow());
					if (gridRow.RowType == RowType.TotalRow && gridRow.Table == RootTable)
					{
						gridRow._isLayoutDirty = false;
						PrepareRowForDisplay(gridRow, false);
						GridFormatStyle gridFormatStyle = GetTotalRowStyle(RootTable);
						MxRowLayoutState totalRowLayout = RootTable.BuildTotalRowLayout(graphics, gridRow, null, gridFormatStyle, GetEffectiveFrozenColumns());
						gridFormatStyle.DisposeResources();
						totalRowLayout._gridRow = gridRow;
						_bodyBottomY -= totalRowLayout._rowHeight;
						totalRowLayout.SetLocation(new Point(base.ClientRectangle.Left, _bodyBottomY));
						_footerRowLayouts.AddRow(totalRowLayout);
					}
				}
				int otherTop = top;
				MordexRow mordexRow = null;
				if (firstRow >= 0 && firstRow < mordexRowManager.Count)
				{
					mordexRow = mordexRowManager[firstRow];
				}
				MordexRow mordexRowManagerItem = null;
				bool otherCondition = true;
				int positionOfChild = -1;
				GridTable table = null;
				bool nextCondition = false;
				while (mordexRow != null && (top < _bodyBottomY || condition))
				{
					table = (GridTable)mordexRow.Table;
					MxRowLayoutState spaceRowLayout;
					if (gridTable != table)
					{
						if (_tableSpacing > 0)
						{
							if (mordexRowManagerItem == null && firstRow > 0)
							{
								mordexRowManagerItem = mordexRowManager[firstRow - 1];
							}
							if (mordexRowManagerItem != null)
							{
								nextCondition = true;
								spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.Space, mordexRow, true));
								if (spaceRowLayout == null)
								{
									spaceRowLayout = RootTable.BuildSpaceRowLayout(graphics, mordexRow, mordexRowManagerItem, _tableSpacing, false);
									spaceRowLayout._mordexRow = mordexRow;
								}
								spaceRowLayout.SetLocation(new Point(base.ClientRectangle.Left, top));
								_bodyRowLayouts.AddRow(spaceRowLayout);
								top += spaceRowLayout._rowHeight;
							}
							if (otherCondition)
							{
								mordexRowManagerItem = null;
							}
						}
						if ((table.AreHeadersRepeated() || RowStateHelper.IsFirstSibling(mordexRow) || mordexRowManagerItem == null) && table.HasAnyHeaderRow())
						{
							spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.ColumnHeader, mordexRow, true));
							if (spaceRowLayout == null)
							{
								GridFormatStyle gridFormatStyle = GetHeaderStyle(table);
								spaceRowLayout = table.BuildColumnHeaderLayout(graphics, mordexRow, mordexRowManagerItem, gridFormatStyle, GetEffectiveFrozenColumns(), false);
								gridFormatStyle.DisposeResources();
								spaceRowLayout._mordexRow = mordexRow;
								spaceRowLayout._showTreeLines = !RowStateHelper.IsFirstSibling(mordexRow) && mordexRowManagerItem != null;
							}
							spaceRowLayout.SetLocation(new Point(base.ClientRectangle.Left, top));
							if (nextCondition)
							{
								spaceRowLayout._isFirstRow = true;
								nextCondition = false;
							}
							_bodyRowLayouts.AddRow(spaceRowLayout);
							top += spaceRowLayout._rowHeight;
						}
						gridTable = table;
					}
					otherCondition = false;
					GridRow gridRow = GetGridRowForLogicalRow(mordexRow);
					PrepareRowForDisplay(gridRow, false);
					gridRow._isLayoutDirty = false;
					switch (gridRow.RowType)
					{
					case RowType.GroupHeader:
						spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.GroupHeaderRow, mordexRow, true));
						if (spaceRowLayout == null)
						{
							GridFormatStyle gridFormatStyle = GetGroupRowStyle(table);
							spaceRowLayout = table.BuildGroupHeaderRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, GetEffectiveFrozenColumns());
							gridFormatStyle.DisposeResources();
						}
						break;
					case RowType.NewRecord:
						spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.NewRow, mordexRow, true));
						if (spaceRowLayout == null)
						{
							GridFormatStyle gridFormatStyle;
							if (table.ComputeNewRowPosition() == NewRowPosition.TopRow)
							{
								gridFormatStyle = GetNewRowStyle(table);
								spaceRowLayout = table.BuildNewRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, GetEffectiveFrozenColumns());
							}
							else
							{
								positionOfChild = mordexRow.ParentRow.PositionOfChild(mordexRow);
								gridFormatStyle = GetAlternatingRowStyle(table, positionOfChild);
								spaceRowLayout = table.BuildDataRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, base.ClientRectangle.Width, GetEffectiveFrozenColumns());
								spaceRowLayout._rowType = LayoutRowType.NewRow;
							}
							gridFormatStyle.DisposeResources();
						}
						break;
					case RowType.GroupFooter:
						spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.GroupTotalRow, mordexRow, true));
						if (spaceRowLayout == null)
						{
							GridFormatStyle gridFormatStyle = GetGroupTotalRowStyle(table);
							spaceRowLayout = table.BuildGroupTotalRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, GetEffectiveFrozenColumns());
							gridFormatStyle.DisposeResources();
						}
						break;
					case RowType.TotalRow:
						spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.TotalRow, mordexRow, true));
						if (spaceRowLayout == null)
						{
							GridFormatStyle gridFormatStyle = GetTotalRowStyle(table);
							spaceRowLayout = ((!table.IsRootTable()) ? table.BuildTotalRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, GetEffectiveFrozenColumns()) : table.BuildTotalRowLayout(graphics, gridRow, null, gridFormatStyle, GetEffectiveFrozenColumns()));
							gridFormatStyle.DisposeResources();
						}
						break;
					default:
						spaceRowLayout = ((_previousBodyRowLayouts == null) ? null : _previousBodyRowLayouts.FindRow(LayoutRowType.DataRow, mordexRow, true));
						if (spaceRowLayout == null)
						{
							positionOfChild = mordexRow.ParentRow.PositionOfChild(mordexRow);
							GridFormatStyle gridFormatStyle = GetAlternatingRowStyle(table, positionOfChild);
							spaceRowLayout = table.BuildDataRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, base.ClientRectangle.Width, GetEffectiveFrozenColumns());
							gridFormatStyle.DisposeResources();
						}
						break;
					}
					if (gridRow.Table == RootTable)
					{
						positionOfChild = gridRow.GetPosition();
					}
					spaceRowLayout._gridRow = gridRow;
					spaceRowLayout._rowPosition = firstRow;
					spaceRowLayout.SetLocation(new Point(base.ClientRectangle.Left, top));
					if (nextCondition)
					{
						spaceRowLayout._isFirstRow = true;
						nextCondition = false;
					}
					spaceRowLayout._mordexRow = mordexRow;
					_bodyRowLayouts.AddRow(spaceRowLayout);
					top += spaceRowLayout._rowHeight;
					mordexRowManagerItem = mordexRow;
					firstRow++;
					mordexRow = ((firstRow >= mordexRowManager.Count) ? null : mordexRowManager[firstRow]);
				}
				if (_emptyRows && top < _bodyBottomY)
				{
					GridRow gridRow = new GridRow(_gridDataSourceManager.GetDataMordexRow(), this);
					MxRowLayoutState dataRowLayout;
					for (; top < _bodyBottomY; top += dataRowLayout._rowHeight)
					{
						positionOfChild++;
						GridFormatStyle gridFormatStyle = GetAlternatingRowStyle(RootTable, positionOfChild);
						dataRowLayout = RootTable.BuildDataRowLayout(graphics, gridRow, mordexRowManagerItem, gridFormatStyle, base.ClientRectangle.Width, GetEffectiveFrozenColumns());
						gridFormatStyle.DisposeResources();
						dataRowLayout._rowType = LayoutRowType.EmptyRow;
						mordexRowManagerItem = _gridDataSourceManager.GetDataMordexRow();
						dataRowLayout._gridRow = gridRow;
						dataRowLayout._rowPosition = positionOfChild;
						dataRowLayout.SetLocation(new Point(base.ClientRectangle.Left, top));
						_bodyRowLayouts.AddRow(dataRowLayout);
					}
				}
				_bodyRowLayouts.LayoutRows(otherTop, _bodyBottomY - otherTop);
				if (RowCount > _bodyRowLayouts.GetVisibleRowCount())
				{
					ConfigureVerticalScrollBar(0, RowCount - 1, _bodyRowLayouts.GetVisibleRowCount(), _firstRow);
					base.SetVerticalScrollRequired(true);
				}
				else
				{
					FirstRow = 0;
					base.SetVerticalScrollRequired(false);
				}
				_rowCache.ReleaseStaleRows();
				ticks = (DateTime.Now.Ticks - ticks) / 10000;
			}
			else
			{
				base.SetVerticalScrollRequired(false);
				base.SetHorizontalScrollRequired(false);
				SetHorizontalScrollOffset(0);
				_contentWidth = clientRectangle.Width;
				LayoutGroupByBox(_headerRowLayouts, graphics);
			}
		}

		private int LayoutGroupByBox(RowLayoutList state, Graphics graphics)
		{
			int result = 0;
			int top = base.ClientRectangle.Top;
			if (!GroupByBoxVisible || RootTable == null)
			{
				_groupByBoxHeight = 0;
				return result;
			}
			return _groupByBoxHeight = BuildGroupByBoxLayout(state, graphics, RootTable, _hierarchical, top);
		}

		private int BuildGroupByBoxLayout(RowLayoutList state, Graphics graphics, GridTable table, bool condition, int value)
		{
			ArrayList arrayList = new ArrayList();
			int otherHeight = 0;
			int otherValue = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.GroupByBox, table);
			if (!table.AnyGroupsDefined())
			{
				if (!table.AnyChildGroupsDefined())
				{
					string text = BuiltInTexts[GridBuiltInText.GroupByBoxInfo];
					GridFormatStyle gridFormatStyle = GetGroupByBoxInfoStyle(table);
					otherHeight = ((!condition) ? ((int)gridFormatStyle.ResolveFont(Font).GetHeight(graphics) + 9) : ((int)gridFormatStyle.ResolveFont(Font).GetHeight(graphics) + 7));
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxInfo, mordexRowLayoutState, null);
					gridGroupLayoutState._isHorizontallyScrolled = false;
					TextLayoutRenderer textLayoutRenderer = gridGroupLayoutState.ComputeTextLayoutRenderer(gridFormatStyle, !base.UseCompatibleTextRendering);
					int width = (int)textLayoutRenderer.MeasureText(graphics, text, gridFormatStyle.ResolveFont(Font), 0).Width + 8;
					otherValue = 5;
					gridGroupLayoutState._rect = new Rectangle(4 + table.HierarchyLevel * _indent, otherValue, width, otherHeight);
					gridGroupLayoutState._leftMargin = 4;
					if (text.Length > 0)
					{
						gridGroupLayoutState._gridGroupLayoutStateOwner = mordexRowLayoutState;
						mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					}
					otherHeight += 11;
					gridFormatStyle.DisposeResources();
				}
				else
				{
					GridFormatStyle groupByBoxInfoStyle = GetGroupByBoxInfoStyle(table);
					otherHeight = (int)groupByBoxInfoStyle.ResolveFont(Font).GetHeight(graphics) + 4;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxTableHeader, mordexRowLayoutState, null);
					groupLayoutState._isHorizontallyScrolled = false;
					TextLayoutRenderer otherTextLayoutRenderer = groupLayoutState.ComputeTextLayoutRenderer(groupByBoxInfoStyle, !base.UseCompatibleTextRendering);
					int otherWidth = (int)otherTextLayoutRenderer.MeasureText(graphics, table.Caption, groupByBoxInfoStyle.ResolveFont(Font), 0).Width + 4;
					otherTextLayoutRenderer.ReleaseStringFormat();
					if (table.HasTableImage())
					{
						otherWidth += GetImageSize().Width + 4;
						if (otherHeight < GetImageSize().Width + 4)
						{
							otherHeight = GetImageSize().Width + 4;
						}
					}
					otherValue = (table.IsRootTable() ? 5 : 0);
					groupLayoutState._rect = new Rectangle(4 + table.HierarchyLevel * _indent, otherValue, otherWidth, otherHeight);
					groupLayoutState._leftMargin = 2;
					groupLayoutState._gridGroupLayoutStateOwner = mordexRowLayoutState;
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
					otherHeight += otherValue + 5;
					groupByBoxInfoStyle.DisposeResources();
				}
			}
			else
			{
				int nextHeight = 0;
				int nextValue = 4 + table.HierarchyLevel * _indent;
				int previousValue = (table.IsRootTable() ? 5 : 0);
				if (condition)
				{
					GridFormatStyle otherGroupByBoxInfoStyle = GetGroupByBoxInfoStyle(table);
					nextHeight = (int)otherGroupByBoxInfoStyle.ResolveFont(Font).GetHeight(graphics) + 4;
					GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxTableHeader, mordexRowLayoutState, null);
					otherGroupLayoutState._isHorizontallyScrolled = false;
					TextLayoutRenderer nextTextLayoutRenderer = otherGroupLayoutState.ComputeTextLayoutRenderer(otherGroupByBoxInfoStyle, !base.UseCompatibleTextRendering);
					int nextWidth = (int)nextTextLayoutRenderer.MeasureText(graphics, table.Caption, otherGroupByBoxInfoStyle.ResolveFont(Font), 0).Width + 4;
					if (table.HasTableImage())
					{
						nextWidth += GetImageSize().Width + 4;
						if (nextHeight < GetImageSize().Width + 4)
						{
							nextHeight = GetImageSize().Width + 4;
						}
					}
					otherGroupLayoutState._rect = new Rectangle(nextValue, previousValue, nextWidth, nextHeight);
					otherGroupLayoutState._leftMargin = 2;
					otherGroupLayoutState._gridGroupLayoutStateOwner = mordexRowLayoutState;
					mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
					nextValue += otherGroupLayoutState._rect.Width + 8;
					otherGroupByBoxInfoStyle.DisposeResources();
					GridFormatStyle headerStyle = GetHeaderStyle(table);
					foreach (GridGroup group in table.Groups)
					{
						otherGroupLayoutState = BuildGroupByBoxHeaderItem(graphics, group, headerStyle);
						arrayList.Add(group);
						otherGroupLayoutState._gridGroupLayoutStateOwner = mordexRowLayoutState;
						otherGroupLayoutState._rect.Offset(nextValue, previousValue);
						mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						nextValue += otherGroupLayoutState._rect.Width + 8;
						if (otherGroupLayoutState._rect.Height > nextHeight)
						{
							nextHeight = otherGroupLayoutState._rect.Height;
						}
					}
					headerStyle.DisposeResources();
					otherHeight = previousValue + nextHeight;
					otherHeight += 5;
					GridGroupLayoutState nextGroupLayoutState = null;
					Point point = new Point(0, 0);
					int count = mordexRowLayoutState._arrayList.Count;
					for (int i = 0; i < count; i++)
					{
						GridGroupLayoutState arrayListItem = (GridGroupLayoutState)mordexRowLayoutState._arrayList[i];
						if (nextGroupLayoutState != null)
						{
							int height = nextGroupLayoutState._rect.Height;
							if (height > arrayListItem._rect.Height)
							{
								height = arrayListItem._rect.Height;
							}
							otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxConnector, mordexRowLayoutState, null);
							otherGroupLayoutState._isHorizontallyScrolled = false;
							otherGroupLayoutState._rect = new Rectangle(point.X, point.Y, 7, height);
							otherGroupLayoutState._borderFlags = BorderFlags.ConnectorLeft | BorderFlags.ConnectorRight;
							otherGroupLayoutState._gridGroupLayoutStateOwner = mordexRowLayoutState;
							mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						}
						point = new Point(arrayListItem._rect.Right, arrayListItem._rect.Top);
						nextGroupLayoutState = arrayListItem;
					}
				}
				else
				{
					GridFormatStyle otherHeaderStyle = GetHeaderStyle(table);
					foreach (GridGroup otherGroup in table.Groups)
					{
						GridGroupLayoutState groupByBoxHeaderItem = BuildGroupByBoxHeaderItem(graphics, otherGroup, otherHeaderStyle);
						arrayList.Add(otherGroup);
						groupByBoxHeaderItem._gridGroupLayoutStateOwner = mordexRowLayoutState;
						groupByBoxHeaderItem._rect.Offset(nextValue, previousValue);
						mordexRowLayoutState._arrayList.Add(groupByBoxHeaderItem);
						nextValue += groupByBoxHeaderItem._rect.Width + 8;
						previousValue += groupByBoxHeaderItem._rect.Height / 2;
						nextHeight = groupByBoxHeaderItem._rect.Bottom;
					}
					otherHeaderStyle.DisposeResources();
					otherHeight = nextHeight;
					otherHeight += 5;
					GridGroupLayoutState previousGroupLayoutState = null;
					Point otherPoint = new Point(0, 0);
					int firstValue = mordexRowLayoutState._arrayList.Count;
					for (int j = 0; j < firstValue; j++)
					{
						GridGroupLayoutState otherArrayListItem = (GridGroupLayoutState)mordexRowLayoutState._arrayList[j];
						if (previousGroupLayoutState != null)
						{
							int previousHeight = otherArrayListItem._rect.Bottom - previousGroupLayoutState._rect.Bottom;
							GridGroupLayoutState firstGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxConnector, mordexRowLayoutState, null);
							firstGroupLayoutState._isHorizontallyScrolled = false;
							firstGroupLayoutState._rect = new Rectangle(otherPoint.X, otherPoint.Y, 22, previousHeight);
							firstGroupLayoutState._borderFlags = BorderFlags.ConnectorTop | BorderFlags.ConnectorRight;
							mordexRowLayoutState._arrayList.Add(firstGroupLayoutState);
						}
						otherPoint = new Point(otherArrayListItem._rect.Right - 15, otherArrayListItem._rect.Bottom);
						previousGroupLayoutState = otherArrayListItem;
					}
				}
			}
			if (!table.IsRootTable())
			{
				GridGroupLayoutState secondGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxConnector, mordexRowLayoutState, null);
				secondGroupLayoutState._isHorizontallyScrolled = false;
				secondGroupLayoutState._borderFlags = BorderFlags.ConnectorTop | BorderFlags.ConnectorRight;
				int topMargin = 0;
				int secondValue = otherHeight;
				int thirdValue = state.GetCount() - 1;
				while (thirdValue >= 0 && state.GetRowAt(thirdValue).LayoutTable != mordexRowLayoutState.LayoutTable.ParentTable)
				{
					topMargin -= state.GetRowAt(thirdValue)._rowHeight;
					secondValue += state.GetRowAt(thirdValue)._rowHeight;
					thirdValue--;
				}
				secondGroupLayoutState._rect = new Rectangle(4 + (table.HierarchyLevel - 1) * _indent, -5, _indent - 1, otherHeight);
				secondGroupLayoutState._topMargin = topMargin;
				mordexRowLayoutState._arrayList.Add(secondGroupLayoutState);
			}
			mordexRowLayoutState._rowHeight = otherHeight;
			mordexRowLayoutState.SetLocation(new Point(base.ClientRectangle.Left, value));
			value += otherHeight;
			state.AddRow(mordexRowLayoutState);
			if (_hierarchical)
			{
				foreach (GridTable childTable in table.ChildTables)
				{
					if (childTable.AnyGroupsDefined() || childTable.AnyChildGroupsDefined())
					{
						int groupByBoxLayout = BuildGroupByBoxLayout(state, graphics, childTable, true, value);
						value += groupByBoxLayout;
						otherHeight += groupByBoxLayout;
					}
				}
			}
			if (_groupAccessibleObject != null)
			{
				if (arrayList.Count == 0)
				{
					_groupAccessibleObject.SetGroups(null);
				}
				else
				{
					_groupAccessibleObject.SetGroups((GridGroup[])arrayList.ToArray(typeof(GridGroup)));
				}
			}
			return otherHeight;
		}

		private Rectangle GetRowAreaRectangle()
		{
			if (_view == View.TableView)
			{
				return Rectangle.FromLTRB(base.ClientRectangle.Left, _bodyTopY, base.ClientRectangle.Right, _bodyBottomY);
			}
			return base.ClientRectangle;
		}

		public int LastVisibleRow(bool includePartialRows)
		{
			int result = FirstRow;
			if (_view == View.TableView)
			{
				if (_bodyRowLayouts != null)
				{
					for (int value = _bodyRowLayouts.GetCount() - 1; value >= 0; value--)
					{
						MxRowLayoutState mordexRowLayoutState = _bodyRowLayouts.GetRowAt(value);
						Rectangle rectangle = AdjustRectangleToViewport(mordexRowLayoutState.GetContentBounds(), true);
						if (includePartialRows)
						{
							if (rectangle.Top < base.ClientRectangle.Bottom && mordexRowLayoutState._gridRow != null)
							{
								result = mordexRowLayoutState._gridRow.Position;
								break;
							}
						}
						else if (rectangle.Bottom <= base.ClientRectangle.Bottom && mordexRowLayoutState._gridRow != null)
						{
							result = mordexRowLayoutState._gridRow.Position;
							break;
						}
					}
				}
			}
			else if (_view == View.CardView && _cardRowLayouts != null)
			{
				for (int i = _cardRowLayouts.GetCount() - 1; i >= 0; i--)
				{
					GridTableLayoutState gridTableLayoutState = _cardRowLayouts.GetTableLayout(i);
					Rectangle adjustRectangleToViewport = AdjustRectangleToViewport(gridTableLayoutState.GetBounds(), true);
					if (includePartialRows)
					{
						if (adjustRectangleToViewport.Left < base.ClientRectangle.Right)
						{
							result = gridTableLayoutState._gridRow.Position;
							break;
						}
					}
					else if (adjustRectangleToViewport.Right <= base.ClientRectangle.Right)
					{
						result = gridTableLayoutState._gridRow.Position;
						break;
					}
				}
			}
			return result;
		}

		private void SetPressedLayoutItem(GridGroupLayoutState gridGridGroupLayoutState)
		{
			if (_gridGridGroupLayoutState == gridGridGroupLayoutState)
			{
				return;
			}
			if (_gridGridGroupLayoutState != null)
			{
				_gridGridGroupLayoutState._isPressed = false;
				InvalidateLayoutItem(_gridGridGroupLayoutState);
			}
			_gridGridGroupLayoutState = gridGridGroupLayoutState;
			if (_gridGridGroupLayoutState == null)
			{
				return;
			}
			_gridGridGroupLayoutState._isPressed = true;
			InvalidateLayoutItem(_gridGridGroupLayoutState);
			switch (_gridGridGroupLayoutState._type)
			{
			case LayoutItemType.CellDropDownButton:
				EditMode = EditMode.EditOn;
				if (_gridColumnContainerControl != null)
				{
					Update();
					_gridColumnContainerControl.NotifyDropDownButtonMouseDown();
				}
				break;
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
				if (_gridColumnContainerControl != null)
				{
					if (_gridGridGroupLayoutState._type == LayoutItemType.CellDownButton)
					{
						_gridColumnContainerControl.NotifyUpDownButtonClick(-1);
					}
					else
					{
						_gridColumnContainerControl.NotifyUpDownButtonClick(1);
					}
				}
				break;
			}
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			if (_view == View.TableView)
			{
				switch (scrollCode)
				{
				case 2:
					SetHorizontalScrollOffset(GetHorizontalScrollBar().GetScrollValue());
					break;
				case 3:
					SetHorizontalScrollOffset(GetHorizontalScrollBar().GetScrollValue());
					Update();
					break;
				case 1:
					if (_continuousScroll)
					{
						FirstRow = GetVScroll().GetScrollValue();
						Update();
					}
					else
					{
						GetVScroll().Update();
					}
					break;
				case 0:
					FirstRow = GetVScroll().GetScrollValue();
					break;
				}
				return;
			}
			switch (scrollCode)
			{
			case 2:
				FirstRow = GetHorizontalScrollBar().GetScrollValue();
				break;
			case 3:
				if (_continuousScroll)
				{
					FirstRow = GetHorizontalScrollBar().GetScrollValue();
				}
				break;
			case 0:
			case 1:
				SetVerticalScrollOffset(GetVScroll().GetScrollValue());
				break;
			}
		}

		protected internal override void OnScrollBarScrollEvent(ScrollEventArgs e)
		{
			base.OnScrollBarScrollEvent(e);
			if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll && View == View.TableView && _tableViewHorizontalScrollIncrement == -1)
			{
				if (e.Type == ScrollEventType.SmallIncrement)
				{
					e.NewValue = FindNextColumnScrollOffset();
				}
				else if (e.Type == ScrollEventType.SmallDecrement)
				{
					e.NewValue = FindPreviousColumnScrollOffset();
				}
			}
		}

		private bool TryScrollRowsByPixels(int value, int otherValue)
		{
			if (!CanScrollByBlit() || GetEffectiveFrozenColumns() != 0 || GetScrollableBaseBoolean())
			{
				return false;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			if (_headerRowLayouts != null)
			{
				foreach (MxRowLayoutState item in _headerRowLayouts)
				{
					if (item._rowType == LayoutRowType.GroupByBox)
					{
						int nextValue = item.GetLocation().Y + item._rowHeight;
						if (nextValue > clientRectangle.Y)
						{
							clientRectangle.Height = clientRectangle.Bottom - nextValue;
							clientRectangle.Y = nextValue;
						}
					}
				}
			}
			int previousValue = value - otherValue;
			ScrollClientAreaBlit(previousValue, 0, clientRectangle, true);
			InvalidateDeferred();
			return true;
		}
	}
}
