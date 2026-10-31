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
		internal MxRowLayoutState BuildPrintTotalRowLayout(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle)
		{
			int nextItemX = 0;
			int value = ((_cellLayoutMode == CellLayoutMode.UseColumns) ? 1 : _columnSetRowCount);
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int[] array = new int[value];
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.TotalRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
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
								GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridCell.FormatStyle);
								Font nextFont = previousApplyOverrides.ResolveFont(font);
								if (nextFont != otherFont || column.WordWrap)
								{
									TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(previousApplyOverrides, false);
									otherArrayItem = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
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
							if (column._rowSpan > 1)
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
				if (gridFormatStyle != formatStyle)
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
			int otherNextItemX = 0;
			int indentWidth = 0;
			if (mordexRow == null)
			{
				mordexRowLayoutState._rowHeight += 4;
				indentWidth = CalculateIndentWidth(gridPrintDocument.PrintHierarchical);
				otherNextItemX = mordexRowLayoutState._nextItemX;
				mordexRowLayoutState._rowHeight -= 4;
				mordexRowLayoutState._top += 4;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else
			{
				mordexRowLayoutState._rowHeight += 4;
				indentWidth = BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), false, true);
				otherNextItemX = mordexRowLayoutState._nextItemX;
				mordexRowLayoutState._rowHeight -= 4;
				mordexRowLayoutState._top += 4;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					groupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				item._rowHeight = 0;
			}
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			int leftMargin = indentWidth;
			int top = mordexRowLayoutState._top;
			for (int n = 0; n < _printLayout._lineCount; n++)
			{
				mordexRowLayoutState._top = top;
				ColumnLayoutMap gridState18Argument = _printLayout._gridState18Array[n];
				Rectangle rectangle = new Rectangle(otherNextItemX, mordexRowLayoutState._top - 4, gridState18Argument.GetTotalWidth() - otherNextItemX, 4);
				otherNextItemX = 0;
				GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TotalRowSeparator, mordexRowLayoutState, null, rectangle);
				otherGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
				otherGroupLayoutState._rowHeight = n;
				mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
				for (int i = 0; i < value; i++)
				{
					mordexRowLayoutState._rowHeight = array[i];
					leftMargin = ((n == 0) ? indentWidth : 0);
					mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
					for (int j = 0; j < gridState18Argument.GetColumnCount(); j++)
					{
						ColumnCellPlacement gridColumnState3Argument = gridState18Argument.GetCell(i, j);
						if (gridColumnState3Argument != null && gridColumnState3Argument._row == i && gridColumnState3Argument._col == j)
						{
							int width = gridState18Argument.SumWidths(gridColumnState3Argument._col, gridColumnState3Argument._col + gridColumnState3Argument._colSpan - 1) + leftMargin;
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
								nextArrayItem = array[i];
							}
							otherGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, nextArrayItem), type: LayoutItemType.TotalRowCell, owner: mordexRowLayoutState, column: gridColumnState3Argument._column);
							otherGroupLayoutState._rowHeight = n;
							otherGroupLayoutState.CopyColumnMargins(gridColumnState3Argument._column);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								otherGroupLayoutState._borderFlags = BorderFlags.Right;
								if (mordexRowLayoutState._nextItemX == 0)
								{
									otherGroupLayoutState._borderFlags |= BorderFlags.Left;
								}
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								otherGroupLayoutState._leftMargin = leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						}
						else if (gridColumnState3Argument == null)
						{
							int sumWidths = gridState18Argument.SumWidths(j, j);
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, sumWidths, otherHeight);
							otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, null, otherRectangle);
							otherGroupLayoutState._rowHeight = n;
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								otherGroupLayoutState._borderFlags = BorderFlags.Right;
								if (mordexRowLayoutState._nextItemX == 0)
								{
									otherGroupLayoutState._borderFlags |= BorderFlags.Left;
								}
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								otherGroupLayoutState._leftMargin = leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						}
						else
						{
							if (gridColumnState3Argument._row != i)
							{
								mordexRowLayoutState._nextItemX += gridState18Argument.SumWidths(j, j) + leftMargin;
							}
							leftMargin = 0;
						}
					}
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				}
				mordexRowLayoutStateInteger = 0;
			}
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildPrintGroupTotalRowLayout(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle)
		{
			int nextItemX = 0;
			int value = ((_cellLayoutMode == CellLayoutMode.UseColumns) ? 1 : _columnSetRowCount);
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int[] array = new int[value];
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.GroupTotalRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.Group.GetGroupRowStyle());
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
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
							int otherArrayItem = arrayItem;
							GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridRow.Cells[column.Index].FormatStyle);
							Font nextFont = otherApplyOverrides.ResolveFont(font);
							if (nextFont != otherFont || column.WordWrap)
							{
								TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(otherApplyOverrides, false);
								otherArrayItem = GridHelper.CalculateTextHeight(graphics, nextFont, gridRow.Cells[column.Index].Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
								textLayoutRenderer.ReleaseStringFormat();
							}
							otherArrayItem += column.TopMargin + column.BottomMargin;
							if (otherApplyOverrides != applyOverrides)
							{
								otherApplyOverrides.DisposeResources();
							}
							if (column._rowSpan > 1)
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
				if (gridFormatStyle != formatStyle)
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
			BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), false, gridPrintDocument.PrintHierarchical);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				item._rowHeight = 0;
			}
			int index = gridRow.GetLogicalRow().ParentRow.Group.Index;
			int nextValue = Groups.Count - (index + 1);
			int previousValue = nextValue * GetGroupIndentWidth();
			if (AreRowHeadersVisible())
			{
				previousValue += RowHeaderWidth;
			}
			if (IsHierarchical() && gridPrintDocument.PrintHierarchical)
			{
				previousValue += GetIndentWidth();
			}
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			int top = mordexRowLayoutState._top;
			int leftMargin = previousValue;
			for (int n = 0; n < _printLayout._lineCount; n++)
			{
				mordexRowLayoutState._top = top;
				ColumnLayoutMap gridState18Argument = _printLayout._gridState18Array[n];
				for (int p = 0; p < value; p++)
				{
					mordexRowLayoutState._rowHeight = array[p];
					leftMargin = ((n == 0) ? previousValue : 0);
					mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
					for (int q = 0; q < gridState18Argument.GetColumnCount(); q++)
					{
						ColumnCellPlacement gridColumnState3Argument = gridState18Argument.GetCell(p, q);
						if (gridColumnState3Argument != null && gridColumnState3Argument._row == p && gridColumnState3Argument._col == q)
						{
							int width = gridState18Argument.SumWidths(gridColumnState3Argument._col, gridColumnState3Argument._col + gridColumnState3Argument._colSpan - 1) + leftMargin;
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
							GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, nextArrayItem), type: LayoutItemType.GroupTotalCell, owner: mordexRowLayoutState, column: gridColumnState3Argument._column);
							gridGroupLayoutState._rowHeight = n;
							gridGroupLayoutState.CopyColumnMargins(gridColumnState3Argument._column);
							if ((q == gridState18Argument.GetColumnCount() - 1 && GetEffectiveGridLines() == GridLines.Both) || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (mordexRowLayoutState._nextItemX == 0 && (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical))
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Left;
							}
							if (gridColumnState3Argument._row + gridColumnState3Argument._rowSpan == _columnSetRowCount && (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal))
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							if (leftMargin != 0)
							{
								gridGroupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
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
							if ((q == gridState18Argument.GetColumnCount() - 1 && GetEffectiveGridLines() == GridLines.Both) || GetEffectiveGridLines() == GridLines.Vertical)
							{
								groupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (mordexRowLayoutState._nextItemX == 0 && (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical))
							{
								groupLayoutState._borderFlags |= BorderFlags.Left;
							}
							if (p == _columnSetRowCount - 1 && (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal))
							{
								groupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							if (leftMargin != 0)
							{
								groupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._nextItemX += groupLayoutState._rect.Width;
							mordexRowLayoutState._arrayList.Add(groupLayoutState);
						}
						else
						{
							if (gridColumnState3Argument._row != p)
							{
								mordexRowLayoutState._nextItemX += gridState18Argument.SumWidths(q, q);
							}
							leftMargin = 0;
						}
					}
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				}
				mordexRowLayoutStateInteger = 0;
			}
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal GroupTotals ComputeGroupTotals()
		{
			if (GroupTotals == GroupTotals.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.GroupTotals;
				}
				return GroupTotals.Never;
			}
			return GroupTotals;
		}

		internal GridTable FindGroupedTable()
		{
			if (Groups.Count > 0)
			{
				return this;
			}
			if (ParentTable != null)
			{
				return ParentTable.FindGroupedTable();
			}
			return null;
		}

		public bool CanGroupBy(GridColumn column)
		{
			return CanGroupByField(column);
		}

		internal void SuspendBaseSort()
		{
			BaseSuspendSort();
		}

		internal void ResumeBaseSort()
		{
			BaseResumeSort();
		}

		protected override void OnFilterConditionChanged()
		{
			base.OnFilterConditionChanged();
			if (DataGrid != null)
			{
				DataGrid.ResetFilterRowCache(this);
			}
		}
	}
}
