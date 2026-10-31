using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable
	{
		internal Pen GetOrCreateGridLinePen()
		{
			if (_gridLinePen == null)
			{
				GridLineStyle gridLineStyle = _gridLineStyle;
				if (gridLineStyle == GridLineStyle.Default && DataGrid != null)
				{
					gridLineStyle = DataGrid.GridLineStyle;
				}
				if (gridLineStyle == GridLineStyle.Solid)
				{
					_gridLinePen = GdiCache.Pen(GetEffectiveGridLineColor());
					_isGridLinePenShared = true;
				}
				else if (DataGrid != null)
				{
					GridFormatStyle gridFormatStyle = DataGrid.GetRowStyle(this);
					_gridLinePen = GridHelper.CreateHatchPen(GetEffectiveGridLineColor(), gridFormatStyle.GetEffectiveBackColor());
				}
				else
				{
					_gridLinePen = GridHelper.CreateHatchPen(GetEffectiveGridLineColor(), SystemColors.Window);
				}
			}
			return _gridLinePen;
		}

		internal void DisposeGridLinePen()
		{
			if (_gridLinePen != null)
			{
				if (!_isGridLinePenShared)
				{
					_gridLinePen.Dispose();
				}
				_isGridLinePenShared = false;
				_gridLinePen = null;
			}
			if (ChildTables == null)
			{
				return;
			}
			foreach (GridTable childTable in ChildTables)
			{
				childTable.DisposeGridLinePen();
			}
		}

		internal void InvalidateGridLayout()
		{
			DataGrid?.InvalidateAfterTableChange(this);
		}

		internal MxRowLayoutState RenderContent(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, MordexRow mordexRow, MordexRow otherMordexRow, GridFormatStyle formatStyle)
		{
			int nextItemX = 0;
			int otherNextItemX = 0;
			int indentWidth = 0;
			int value = 0;
			int otherValue = ((_cellLayoutMode == CellLayoutMode.UseColumns) ? 1 : (AreColumnHeadersVisible() ? _columnSetRowCount : 0));
			Font font = DataGrid.Font;
			int arrayItem = (int)formatStyle.ResolveFont(font).GetHeight(graphics) * _headerLines + 4;
			int top = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, this);
			if (IsTableHeaderVisible())
			{
				GridFormatStyle gridFormatStyle = DataGrid.GetTableHeaderStyle(this);
				gridFormatStyle.UpdateFloatValue(formatStyle.GetFloatValue());
				top = GridHelper.RoundToInt(gridFormatStyle.ResolveFont(DataGrid.Font).GetHeight(graphics)) + 2;
				if (top < DataGrid.GetImageSize().Height + 2)
				{
					top = DataGrid.GetImageSize().Height + 2;
				}
				top += 2;
				mordexRowLayoutState._rowHeight += top;
			}
			int[] array = new int[otherValue];
			for (int i = 0; i < otherValue; i++)
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
						int nextValue;
						if (cell._column.HeaderStyle != null)
						{
							GridFormatStyle applyOverrides = formatStyle.ApplyOverrides(cell._column.HeaderStyle);
							nextValue = (int)applyOverrides.ResolveFont(font).GetHeight(graphics) * _headerLines + 4;
							if (applyOverrides != formatStyle)
							{
								applyOverrides.DisposeResources();
								applyOverrides = null;
							}
						}
						else
						{
							nextValue = arrayItem;
						}
						if (cell._rowSpan > 1)
						{
							int previousValue = 0;
							for (int otherRow = cell._row; otherRow < cell._row + cell._rowSpan - 1; otherRow++)
							{
								previousValue += array[otherRow];
							}
							nextValue -= previousValue;
						}
						if (nextValue > array[i])
						{
							array[i] = nextValue + 4;
						}
						k += cell._colSpan - 1;
					}
				}
				mordexRowLayoutState._rowHeight += array[i];
			}
			if (AreColumnSetHeadersVisible() && _cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				value = (int)formatStyle.ResolveFont(font).GetHeight(graphics) * _columnSetHeaderLines;
				foreach (GridColumnSet item in _columnSets)
				{
					if (item.Visible && item.HeaderStyle != null)
					{
						GridFormatStyle otherApplyOverrides = formatStyle.ApplyOverrides(item.HeaderStyle);
						int firstValue = (int)otherApplyOverrides.ResolveFont(font).GetHeight(graphics) * _columnSetHeaderLines;
						if (firstValue > value)
						{
							value = firstValue;
						}
						if (otherApplyOverrides != formatStyle)
						{
							otherApplyOverrides.DisposeResources();
							otherApplyOverrides = null;
						}
					}
				}
				value += 4;
				mordexRowLayoutState._rowHeight += value;
			}
			if (otherMordexRow == null || mordexRow == null)
			{
				mordexRowLayoutState._top = top;
				indentWidth = CalculateIndentWidth(gridPrintDocument.PrintHierarchical);
				otherNextItemX = mordexRowLayoutState._nextItemX;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight - top));
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else if (mordexRow is MordexGroupRow)
			{
				indentWidth = BuildIndentLayout(mordexRowLayoutState, mordexRow, false, gridPrintDocument.PrintHierarchical);
				nextItemX = mordexRowLayoutState._nextItemX;
				otherNextItemX = mordexRowLayoutState._nextItemX;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top + top, indentWidth, mordexRowLayoutState._rowHeight - top));
					groupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else
			{
				indentWidth = BuildIndentLayout(mordexRowLayoutState, mordexRow, false, gridPrintDocument.PrintHierarchical);
				nextItemX = mordexRowLayoutState._nextItemX;
				otherNextItemX = mordexRowLayoutState._nextItemX;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top + top, indentWidth, mordexRowLayoutState._rowHeight - top));
					otherGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			mordexRowLayoutState._top = top;
			foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
			{
				arrayListItem._rowHeight = 0;
			}
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			int leftMargin = indentWidth;
			int otherTop = mordexRowLayoutState._top;
			for (int m = 0; m < _printLayout._lineCount; m++)
			{
				mordexRowLayoutState._top = otherTop;
				ColumnLayoutMap gridState18Argument = _printLayout._gridState18Array[m];
				if (AreColumnSetHeadersVisible() && _cellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					mordexRowLayoutState._rowHeight = value;
					mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
					GridColumnSet columnSet = null;
					for (int n = 0; n < gridState18Argument.GetColumnCount(); n++)
					{
						GridColumnSet otherColumnSet = gridState18Argument.GetColumnSet(n);
						if (otherColumnSet != columnSet)
						{
							columnSet = otherColumnSet;
							int otherWidth = gridState18Argument.GetColumnSetWidth(otherColumnSet) + leftMargin;
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, mordexRowLayoutState._rowHeight);
							GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnSetHeader, mordexRowLayoutState, null, rectangle);
							nextGroupLayoutState._rowHeight = m;
							mordexRowLayoutState._nextItemX += otherWidth;
							if (leftMargin != 0)
							{
								nextGroupLayoutState._leftMargin = leftMargin;
								leftMargin = 0;
							}
							nextGroupLayoutState._columnSet = otherColumnSet;
							mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
						}
					}
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				}
				for (int i = 0; i < otherValue; i++)
				{
					mordexRowLayoutState._rowHeight = array[i];
					leftMargin = ((m == 0) ? indentWidth : 0);
					mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
					for (int j = 0; j < gridState18Argument.GetColumnCount(); j++)
					{
						ColumnCellPlacement gridColumnState3Argument = gridState18Argument.GetCell(i, j);
						if (gridColumnState3Argument != null && gridColumnState3Argument._row == i && gridColumnState3Argument._col == j)
						{
							int width = gridState18Argument.SumWidths(gridColumnState3Argument._col, gridColumnState3Argument._col + gridColumnState3Argument._colSpan - 1) + leftMargin;
							int height = 0;
							if (gridColumnState3Argument._rowSpan > 1)
							{
								for (int row = gridColumnState3Argument._row; row < gridColumnState3Argument._row + gridColumnState3Argument._rowSpan; row++)
								{
									height += array[row];
								}
							}
							else
							{
								height = mordexRowLayoutState._rowHeight;
							}
							GridGroupLayoutState previousGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, height), type: LayoutItemType.ColumnHeader, owner: mordexRowLayoutState, column: gridColumnState3Argument._column);
							previousGroupLayoutState._rowHeight = m;
							mordexRowLayoutState._nextItemX += previousGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								previousGroupLayoutState._leftMargin = leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._arrayList.Add(previousGroupLayoutState);
						}
						else if (gridColumnState3Argument == null)
						{
							int nextWidth = gridState18Argument.SumWidths(j, j) + leftMargin;
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, nextWidth, otherHeight);
							GridGroupLayoutState firstGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyHeader, mordexRowLayoutState, null, otherRectangle);
							firstGroupLayoutState._rowHeight = m;
							mordexRowLayoutState._nextItemX += firstGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								firstGroupLayoutState._leftMargin = leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._arrayList.Add(firstGroupLayoutState);
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
				if (IsTableHeaderVisible())
				{
					int fourthValue = 0;
					if (m == 0)
					{
						fourthValue = otherNextItemX;
					}
					Rectangle nextRectangle = new Rectangle(fourthValue, 0, mordexRowLayoutState._nextItemX - fourthValue, top);
					GridGroupLayoutState secondGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TableHeader, mordexRowLayoutState, null, nextRectangle);
					secondGroupLayoutState._rowHeight = m;
					mordexRowLayoutState._arrayList.Add(secondGroupLayoutState);
				}
				mordexRowLayoutStateInteger = 0;
			}
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		void IFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			RefreshDataGrid();
		}
	}
}
