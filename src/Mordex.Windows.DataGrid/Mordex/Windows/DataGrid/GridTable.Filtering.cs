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
		protected override MordexSortFieldCollectionBase CreateSortFieldCollection()
		{
			return new GridSortKeyCollection(this);
		}

		protected override MordexGroupTotalCollectionBase CreateGroupTotalCollection()
		{
			return new GridGroupHeaderTotalCollection(this);
		}

		protected override MordexStoredFilterCollectionBase CreateStoredFiltersCollection()
		{
			return new GridStoredFilterCollection(this);
		}

		internal MxRowLayoutState BuildFilterRowLayout(Graphics graphics, GridFormatStyle formatStyle, int value)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return BuildColumnSetFilterRowLayout(graphics, formatStyle, value);
			}
			int indentWidth = 0;
			int nextItemX = 0;
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			if (height < 16)
			{
				height = 17;
			}
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.FilterRow, this);
			if (_rowHeight == -1)
			{
				Font otherFont = formatStyle.ResolveFont(font);
				int otherValue = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				mordexRowLayoutState._rowHeight = 0;
				foreach (GridColumn column in Columns)
				{
					if (!column.Visible)
					{
						continue;
					}
					int roundCore = otherValue;
					if (column.ColumnType == ColumnType.Text || column.ColumnType == ColumnType.ImageAndText)
					{
						GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(column.CellStyle);
						Font nextFont = gridFormatStyle.ResolveFont(font);
						if (nextFont != otherFont)
						{
							roundCore = GridHelper.RoundToInt(nextFont.GetHeight(graphics));
						}
						if (column.ColumnType == ColumnType.ImageAndText && roundCore < height)
						{
							roundCore = height;
						}
						roundCore += column.TopMargin + column.BottomMargin;
						if (gridFormatStyle != formatStyle)
						{
							gridFormatStyle.DisposeResources();
						}
					}
					else if (column.ColumnType == ColumnType.CheckBox)
					{
						roundCore = 14 + column.TopMargin + column.BottomMargin;
					}
					if (mordexRowLayoutState._rowHeight < roundCore)
					{
						mordexRowLayoutState._rowHeight = roundCore;
					}
				}
				if (mordexRowLayoutState._rowHeight < height + 2)
				{
					mordexRowLayoutState._rowHeight = height + 2;
				}
				mordexRowLayoutState._rowHeight += 2;
			}
			else
			{
				mordexRowLayoutState._rowHeight = _rowHeight;
			}
			indentWidth = CalculateIndentWidth(true);
			if (value != 0)
			{
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					item._isHorizontallyScrolled = false;
				}
			}
			nextItemX = mordexRowLayoutState._nextItemX;
			GridGroupLayoutState gridGroupLayoutState;
			if (AreRowHeadersVisible())
			{
				indentWidth += RowHeaderWidth;
				gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
				gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom;
				if (AreColumnHeadersVisible() || IsTableHeaderVisible())
				{
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
				}
				gridGroupLayoutState._isHorizontallyScrolled = value == 0;
				mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				mordexRowLayoutState._nextItemX += indentWidth;
				indentWidth = 0;
			}
			GridGroupLayoutState[] array = new GridGroupLayoutState[Columns.Count];
			for (int i = 0; i < Columns.Count; i++)
			{
				GridColumn otherColumn = Columns[i];
				int field_ = otherColumn._position;
				array[field_] = new GridGroupLayoutState(LayoutItemType.FilterRowCell, mordexRowLayoutState, otherColumn);
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j]._column.Visible)
				{
					int width = array[j]._column.Width + indentWidth;
					gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: LayoutItemType.FilterRowCell, owner: mordexRowLayoutState, column: array[j]._column);
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						gridGroupLayoutState._borderFlags = BorderFlags.Right;
					}
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
					}
					if (gridGroupLayoutState._column._position < value)
					{
						gridGroupLayoutState._isHorizontallyScrolled = false;
					}
					gridGroupLayoutState.CopyColumnMargins(array[j]._column);
					mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
					if (indentWidth != 0)
					{
						gridGroupLayoutState._leftMargin += indentWidth;
						indentWidth = 0;
					}
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				}
			}
			mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			int otherWidth = mordexRowLayoutState._nextItemX - nextItemX;
			mordexRowLayoutState._rowHeight = 4;
			mordexRowLayoutState._nextItemX = nextItemX;
			Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, mordexRowLayoutState._rowHeight);
			gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.NewRowSeparator, mordexRowLayoutState, null, rectangle);
			mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
			mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			mordexRowLayoutState._rowHeight = (mordexRowLayoutState._top += mordexRowLayoutState._rowHeight);
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildColumnSetFilterRowLayout(Graphics graphics, GridFormatStyle formatStyle, int value)
		{
			int indentWidth = 0;
			int nextItemX = 0;
			int[] array = new int[_columnSetRowCount];
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			LayoutItemType type = LayoutItemType.FilterRowCell;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.FilterRow, this);
			if (_rowHeight == -1)
			{
				Font otherFont = formatStyle.ResolveFont(font);
				int arrayItem = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				for (int i = 0; i < _columnSetRowCount; i++)
				{
					array[i] = arrayItem;
					foreach (GridColumnSet item in _columnSets)
					{
						if (!item.Visible)
						{
							continue;
						}
						for (int j = 0; j < item.ColumnCount; j++)
						{
							GridColumn gridColumn = item[i, j];
							if (gridColumn == null || !gridColumn.Visible || gridColumn.GetLastColumnSetRow() != i || gridColumn._columnSetColumn != j)
							{
								continue;
							}
							int roundCore = arrayItem;
							if (gridColumn.ColumnType == ColumnType.Text || gridColumn.ColumnType == ColumnType.ImageAndText)
							{
								GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridColumn.CellStyle);
								Font nextFont = gridFormatStyle.ResolveFont(font);
								if (nextFont != otherFont)
								{
									roundCore = GridHelper.RoundToInt(nextFont.GetHeight(graphics));
								}
								if (gridColumn.ColumnType == ColumnType.ImageAndText && roundCore < height)
								{
									roundCore = height;
								}
								roundCore += gridColumn.TopMargin + gridColumn.BottomMargin;
								if (gridFormatStyle != formatStyle)
								{
									gridFormatStyle.DisposeResources();
								}
							}
							else if (gridColumn.ColumnType == ColumnType.CheckBox)
							{
								roundCore = 14 + gridColumn.TopMargin + gridColumn.BottomMargin;
							}
							if (gridColumn._rowSpan > 1)
							{
								int otherValue = 0;
								for (int k = gridColumn._columnSetRow; k < gridColumn._columnSetRow + gridColumn._rowSpan - 1; k++)
								{
									otherValue += array[k];
								}
								roundCore -= otherValue;
							}
							if (roundCore > array[i])
							{
								array[i] = roundCore;
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
				for (int n = 0; n < _columnSetRowCount; n++)
				{
					array[n] = _rowHeight;
				}
				mordexRowLayoutState._rowHeight = _columnSetRowCount * _rowHeight;
			}
			indentWidth = CalculateIndentWidth(true);
			if (value != 0)
			{
				foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
				{
					arrayListItem._isHorizontallyScrolled = false;
				}
			}
			GridGroupLayoutState gridGroupLayoutState;
			if (AreRowHeadersVisible())
			{
				indentWidth += RowHeaderWidth;
				gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
				gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom;
				if (AreColumnHeadersVisible() || IsTableHeaderVisible() || AreColumnSetHeadersVisible())
				{
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
				}
				gridGroupLayoutState._isHorizontallyScrolled = value == 0;
				mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				mordexRowLayoutState._nextItemX += indentWidth;
				indentWidth = 0;
			}
			_ = mordexRowLayoutState._arrayList.Count;
			nextItemX = mordexRowLayoutState._nextItemX;
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			GridColumnSet[] columnSets = new GridColumnSet[_columnSets.Count];
			for (int m = 0; m < _columnSets.Count; m++)
			{
				GridColumnSet columnSet = _columnSets[m];
				int position = columnSet._position;
				columnSets[position] = columnSet;
			}
			for (int n = 0; n < _columnSetRowCount; n++)
			{
				int nextValue = indentWidth;
				mordexRowLayoutState._rowHeight = array[n];
				mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
				for (int p = 0; p < _columnSets.Count; p++)
				{
					GridColumnSet otherColumnSet = columnSets[p];
					if (!otherColumnSet.Visible)
					{
						continue;
					}
					for (int column = 0; column < otherColumnSet.ColumnCount; column++)
					{
						GridColumn otherColumn = otherColumnSet[n, column];
						if (otherColumn != null && otherColumn.Visible && otherColumn._columnSetRow == n && otherColumn._columnSetColumn == column)
						{
							int width = otherColumn.Width + nextValue;
							int otherArrayItem = 0;
							if (otherColumn._rowSpan > 1)
							{
								for (int columnSetRow = otherColumn._columnSetRow; columnSetRow < otherColumn._columnSetRow + otherColumn._rowSpan; columnSetRow++)
								{
									otherArrayItem += array[columnSetRow];
								}
							}
							else
							{
								otherArrayItem = array[n];
							}
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, otherArrayItem);
							gridGroupLayoutState = new GridGroupLayoutState(type, mordexRowLayoutState, otherColumn, rectangle);
							gridGroupLayoutState.CopyColumnMargins(otherColumn);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							if (nextValue != 0)
							{
								gridGroupLayoutState._leftMargin += indentWidth;
								nextValue = 0;
							}
							gridGroupLayoutState._isHorizontallyScrolled = otherColumnSet._position >= value;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else if (otherColumn == null || !otherColumn.Visible)
						{
							int otherWidth = otherColumnSet.GetColumnWidth(column) + nextValue;
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, otherHeight);
							gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, otherColumn, otherRectangle);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							if (nextValue != 0)
							{
								gridGroupLayoutState._leftMargin += indentWidth;
								nextValue = 0;
							}
							gridGroupLayoutState._isHorizontallyScrolled = otherColumnSet._position >= value;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else
						{
							if (otherColumn._columnSetRow != n)
							{
								mordexRowLayoutState._nextItemX += otherColumnSet.GetColumnWidth(column) + nextValue;
							}
							nextValue = 0;
						}
					}
				}
				mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			}
			mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX;
			mordexRowLayoutState._rowHeight = 4;
			Rectangle nextRectangle = new Rectangle(0, mordexRowLayoutState._top, mordexRowLayoutState._rightEdge, mordexRowLayoutState._rowHeight);
			gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.NewRowSeparator, mordexRowLayoutState, null, nextRectangle);
			mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._top = -1;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildColumnSetTotalRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value)
		{
			int nextItemX = 0;
			int[] array = new int[_columnSetRowCount];
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int indentLayout = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.TotalRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
				Font otherFont = gridFormatStyle.ResolveFont(font);
				int arrayItem = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				mordexRowLayoutState._rowHeight = 0;
				for (int i = 0; i < _columnSetRowCount; i++)
				{
					array[i] = arrayItem;
					foreach (GridColumnSet item in _columnSets)
					{
						if (!item.Visible)
						{
							continue;
						}
						for (int j = 0; j < item.ColumnCount; j++)
						{
							GridColumn gridColumn = item[i, j];
							if (gridColumn == null || !gridColumn.Visible || gridColumn.GetLastColumnSetRow() != i || gridColumn._columnSetColumn != j)
							{
								continue;
							}
							int otherArrayItem = arrayItem;
							GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
							Font nextFont = applyOverrides.ResolveFont(font);
							if (nextFont != otherFont || gridColumn.WordWrap)
							{
								TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(applyOverrides, !DataGrid.UseCompatibleTextRendering);
								otherArrayItem = GridHelper.CalculateTextHeight(graphics, nextFont, gridRow.Cells[gridColumn.Index].Text, textLayoutRenderer, gridColumn._width - gridColumn.LeftMargin - gridColumn.RightMargin - 2, gridColumn.MinLines, gridColumn.MaxLines);
								textLayoutRenderer.ReleaseStringFormat();
							}
							if (gridRow.Cells[gridColumn.Index].HasImageReference() && otherArrayItem < height)
							{
								otherArrayItem = (height = gridColumn.TopMargin + gridColumn.BottomMargin);
							}
							otherArrayItem += gridColumn.TopMargin + gridColumn.BottomMargin;
							if (applyOverrides != gridFormatStyle)
							{
								applyOverrides.DisposeResources();
							}
							if (gridColumn._rowSpan > 1)
							{
								int otherValue = 0;
								for (int k = gridColumn._columnSetRow; k < gridColumn._columnSetRow + gridColumn._rowSpan - 1; k++)
								{
									otherValue += array[k];
								}
								otherArrayItem -= otherValue;
							}
							if (otherArrayItem > array[i])
							{
								array[i] = otherArrayItem;
							}
						}
					}
					mordexRowLayoutState._rowHeight += array[i];
					if (gridFormatStyle != formatStyle)
					{
						gridFormatStyle.DisposeResources();
					}
				}
			}
			else
			{
				for (int n = 0; n < _columnSetRowCount; n++)
				{
					array[n] = _rowHeight;
				}
				mordexRowLayoutState._rowHeight = _columnSetRowCount * _rowHeight;
			}
			mordexRowLayoutState._rowHeight += 4;
			indentLayout = BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), false, true);
			nextItemX = mordexRowLayoutState._nextItemX;
			mordexRowLayoutState._rowHeight -= 4;
			mordexRowLayoutState._top += 4;
			GridGroupLayoutState gridGroupLayoutState;
			if (AreRowHeadersVisible())
			{
				indentLayout += RowHeaderWidth;
				gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentLayout, mordexRowLayoutState._rowHeight));
				gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
				mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				mordexRowLayoutState._nextItemX += indentLayout;
				indentLayout = 0;
			}
			if (value != 0)
			{
				foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
				{
					arrayListItem._isHorizontallyScrolled = false;
				}
			}
			_ = mordexRowLayoutState._nextItemX;
			_ = mordexRowLayoutState._nextItemX;
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			GridColumnSet[] columnSets = new GridColumnSet[_columnSets.Count];
			for (int m = 0; m < _columnSets.Count; m++)
			{
				GridColumnSet columnSet = _columnSets[m];
				int position = columnSet._position;
				columnSets[position] = columnSet;
			}
			for (int n = 0; n < _columnSetRowCount; n++)
			{
				int leftMargin = indentLayout;
				mordexRowLayoutState._rowHeight = array[n];
				mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
				for (int p = 0; p < _columnSets.Count; p++)
				{
					GridColumnSet otherColumnSet = columnSets[p];
					if (!otherColumnSet.Visible)
					{
						continue;
					}
					for (int column = 0; column < otherColumnSet.ColumnCount; column++)
					{
						GridColumn otherColumn = otherColumnSet[n, column];
						if (otherColumn != null && otherColumn.Visible && otherColumn._columnSetRow == n && otherColumn._columnSetColumn == column)
						{
							int width = otherColumn.Width + leftMargin;
							int nextArrayItem = 0;
							if (otherColumn._rowSpan > 1)
							{
								for (int columnSetRow = otherColumn._columnSetRow; columnSetRow < otherColumn._columnSetRow + otherColumn._rowSpan; columnSetRow++)
								{
									nextArrayItem += array[columnSetRow];
								}
							}
							else
							{
								nextArrayItem = array[n];
							}
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, nextArrayItem);
							gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TotalRowCell, mordexRowLayoutState, otherColumn, rectangle);
							gridGroupLayoutState.CopyColumnMargins(otherColumn);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								gridGroupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							gridGroupLayoutState._isHorizontallyScrolled = otherColumnSet._position >= value;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else if (otherColumn == null || !otherColumn.Visible)
						{
							int otherWidth = otherColumnSet.GetColumnWidth(column) + leftMargin;
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, otherHeight);
							gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, otherColumn, otherRectangle);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								gridGroupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							gridGroupLayoutState._isHorizontallyScrolled = otherColumnSet._position >= value;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else
						{
							if (otherColumn.ColumnSetRow != n)
							{
								mordexRowLayoutState._nextItemX += otherColumnSet.GetColumnWidth(column) + leftMargin;
							}
							leftMargin = 0;
						}
					}
				}
				mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			}
			int nextWidth = mordexRowLayoutState._nextItemX - nextItemX;
			mordexRowLayoutState._nextItemX = nextItemX;
			Rectangle nextRectangle = new Rectangle(mordexRowLayoutState._nextItemX, 0, nextWidth, 4);
			gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TotalRowSeparator, mordexRowLayoutState, null, nextRectangle);
			mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
			mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildTotalRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return BuildColumnSetTotalRowLayout(graphics, gridRow, mordexRow, formatStyle, value);
			}
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			height += 2;
			int indentLayout = 0;
			int nextItemX = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.TotalRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
				Font otherFont = gridFormatStyle.ResolveFont(font);
				int otherValue = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				mordexRowLayoutState._rowHeight = height;
				foreach (GridColumn column in Columns)
				{
					if (column.Visible)
					{
						int nextValue = otherValue;
						GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.Cells[column.Index].FormatStyle);
						Font nextFont = applyOverrides.ResolveFont(font);
						if (nextFont != otherFont)
						{
							TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(applyOverrides, !DataGrid.UseCompatibleTextRendering);
							nextValue = GridHelper.CalculateTextHeight(graphics, nextFont, gridRow.Cells[column.Index].Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
							textLayoutRenderer.ReleaseStringFormat();
						}
						if (gridRow.Cells[column.Index].HasImageReference() && nextValue < height)
						{
							nextValue = height;
						}
						nextValue += column.TopMargin + column.BottomMargin;
						if (applyOverrides != gridFormatStyle)
						{
							applyOverrides.DisposeResources();
						}
						if (mordexRowLayoutState._rowHeight < nextValue)
						{
							mordexRowLayoutState._rowHeight = nextValue;
						}
					}
				}
				if (mordexRowLayoutState._rowHeight < height + 2)
				{
					mordexRowLayoutState._rowHeight = height + 2;
				}
				mordexRowLayoutState._rowHeight += 2;
				if (gridFormatStyle != formatStyle)
				{
					gridFormatStyle.DisposeResources();
				}
			}
			else
			{
				mordexRowLayoutState._rowHeight = _rowHeight;
			}
			mordexRowLayoutState._rowHeight += 4;
			indentLayout = BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), false, true);
			if (value != 0)
			{
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					item._isHorizontallyScrolled = false;
				}
			}
			nextItemX = mordexRowLayoutState._nextItemX;
			mordexRowLayoutState._rowHeight -= 4;
			mordexRowLayoutState._top += 4;
			GridGroupLayoutState gridGroupLayoutState;
			if (AreRowHeadersVisible())
			{
				indentLayout += RowHeaderWidth;
				gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentLayout, mordexRowLayoutState._rowHeight));
				gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
				gridGroupLayoutState._isHorizontallyScrolled = value == 0;
				mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				mordexRowLayoutState._nextItemX += indentLayout;
				indentLayout = 0;
			}
			GridGroupLayoutState[] array = new GridGroupLayoutState[Columns.Count];
			for (int i = 0; i < Columns.Count; i++)
			{
				GridColumn otherColumn = Columns[i];
				int field_ = otherColumn._position;
				array[field_] = new GridGroupLayoutState(LayoutItemType.TotalRowCell, mordexRowLayoutState, otherColumn);
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j]._column.Visible)
				{
					int width = array[j]._column.Width + indentLayout;
					gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: LayoutItemType.TotalRowCell, owner: mordexRowLayoutState, column: array[j]._column);
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						gridGroupLayoutState._borderFlags = BorderFlags.Right;
					}
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
					}
					if (gridGroupLayoutState._column._position < value)
					{
						gridGroupLayoutState._isHorizontallyScrolled = false;
					}
					gridGroupLayoutState.CopyColumnMargins(array[j]._column);
					mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
					if (indentLayout != 0)
					{
						gridGroupLayoutState._leftMargin += indentLayout;
						indentLayout = 0;
					}
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				}
			}
			mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			int otherWidth = mordexRowLayoutState._nextItemX - nextItemX;
			mordexRowLayoutState._top -= mordexRowLayoutState._rowHeight + 4;
			mordexRowLayoutState._nextItemX = nextItemX;
			Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, 4);
			gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TotalRowSeparator, mordexRowLayoutState, null, rectangle);
			mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
			mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
			mordexRowLayoutState._rowHeight += 4;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildColumnSetGroupTotalLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value)
		{
			int nextItemX = 0;
			int[] array = new int[_columnSetRowCount];
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int otherValue = 0;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.GroupTotalRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.Group.GetGroupRowStyle());
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				Font otherFont = applyOverrides.ResolveFont(font);
				int arrayItem = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				mordexRowLayoutState._rowHeight = 0;
				for (int i = 0; i < _columnSetRowCount; i++)
				{
					array[i] = arrayItem;
					foreach (GridColumnSet item in _columnSets)
					{
						if (!item.Visible)
						{
							continue;
						}
						for (int j = 0; j < item.ColumnCount; j++)
						{
							GridColumn gridColumn = item[i, j];
							if (gridColumn == null || !gridColumn.Visible || gridColumn.GetLastColumnSetRow() != i || gridColumn._columnSetColumn != j)
							{
								continue;
							}
							int otherArrayItem = arrayItem;
							GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridRow.Cells[gridColumn.Index].FormatStyle);
							Font nextFont = otherApplyOverrides.ResolveFont(font);
							if (nextFont != otherFont)
							{
								otherArrayItem = (int)nextFont.GetHeight(graphics) + 2;
							}
							if (gridRow.Cells[gridColumn.Index].HasImageReference() && otherArrayItem < height)
							{
								otherArrayItem = (height = gridColumn.TopMargin + gridColumn.BottomMargin);
							}
							otherArrayItem += gridColumn.TopMargin + gridColumn.BottomMargin;
							if (otherApplyOverrides != applyOverrides)
							{
								otherApplyOverrides.DisposeResources();
							}
							if (gridColumn._rowSpan > 1)
							{
								int nextValue = 0;
								for (int k = gridColumn._columnSetRow; k < gridColumn._columnSetRow + gridColumn._rowSpan - 1; k++)
								{
									nextValue += array[k];
								}
								otherArrayItem -= nextValue;
							}
							if (otherArrayItem > array[i])
							{
								array[i] = otherArrayItem;
							}
						}
					}
					mordexRowLayoutState._rowHeight += array[i];
					if (applyOverrides != gridFormatStyle)
					{
						applyOverrides.DisposeResources();
					}
					if (gridFormatStyle != formatStyle)
					{
						gridFormatStyle.DisposeResources();
					}
				}
			}
			else
			{
				for (int n = 0; n < _columnSetRowCount; n++)
				{
					array[n] = _rowHeight;
				}
				mordexRowLayoutState._rowHeight = _columnSetRowCount * _rowHeight;
			}
			BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow().ParentRow, false, true);
			if (value != 0)
			{
				foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
				{
					arrayListItem._isHorizontallyScrolled = false;
				}
			}
			int index = gridRow.GetLogicalRow().ParentRow.Group.Index;
			int previousValue = Groups.Count - (index + 1);
			otherValue = previousValue * GetGroupIndentWidth();
			if (AreRowHeadersVisible())
			{
				otherValue += RowHeaderWidth;
			}
			if (IsHierarchical() && HierarchicalMode == HierarchicalMode.UseChildTables)
			{
				otherValue += GetIndentWidth();
			}
			nextItemX = mordexRowLayoutState._nextItemX;
			_ = mordexRowLayoutState._arrayList.Count;
			nextItemX = mordexRowLayoutState._nextItemX;
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			GridColumnSet[] columnSets = new GridColumnSet[_columnSets.Count];
			GridColumnSet columnSet = null;
			for (int m = 0; m < _columnSets.Count; m++)
			{
				GridColumnSet otherColumnSet = _columnSets[m];
				int position = otherColumnSet._position;
				columnSets[position] = otherColumnSet;
				if (columnSet == null || columnSet._position < position)
				{
					columnSet = otherColumnSet;
				}
			}
			for (int n = 0; n < _columnSetRowCount; n++)
			{
				int leftMargin = otherValue;
				mordexRowLayoutState._rowHeight = array[n];
				mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
				for (int p = 0; p < _columnSets.Count; p++)
				{
					GridColumnSet nextColumnSet = columnSets[p];
					if (!nextColumnSet.Visible)
					{
						continue;
					}
					for (int column = 0; column < nextColumnSet.ColumnCount; column++)
					{
						GridColumn otherColumn = nextColumnSet[n, column];
						if (otherColumn != null && otherColumn.Visible && otherColumn._columnSetRow == n && otherColumn._columnSetColumn == column)
						{
							int width = otherColumn.Width + leftMargin;
							int nextArrayItem = 0;
							if (otherColumn._rowSpan > 1)
							{
								for (int columnSetRow = otherColumn._columnSetRow; columnSetRow < otherColumn._columnSetRow + otherColumn._rowSpan; columnSetRow++)
								{
									nextArrayItem += array[columnSetRow];
								}
							}
							else
							{
								nextArrayItem = array[n];
							}
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, nextArrayItem);
							GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupTotalCell, mordexRowLayoutState, otherColumn, rectangle);
							gridGroupLayoutState.CopyColumnMargins(otherColumn);
							if ((nextColumnSet == columnSet && otherColumn._columnSetColumn + otherColumn._colSpan == nextColumnSet.ColumnCount && GetEffectiveGridLines() == GridLines.Both) || GetEffectiveGridLines() == GridLines.Vertical)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (otherColumn.ColumnSetRow + otherColumn._rowSpan == _columnSetRowCount && (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal))
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								gridGroupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							gridGroupLayoutState._isHorizontallyScrolled = nextColumnSet.Position >= value;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						else if (otherColumn == null || !otherColumn.Visible)
						{
							int otherWidth = nextColumnSet.GetColumnWidth(column) + leftMargin;
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, otherHeight);
							GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, otherColumn, otherRectangle);
							mordexRowLayoutState._nextItemX += groupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								groupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							groupLayoutState._isHorizontallyScrolled = nextColumnSet.Position >= value;
							mordexRowLayoutState._arrayList.Add(groupLayoutState);
						}
						else
						{
							if (otherColumn.ColumnSetRow != n)
							{
								mordexRowLayoutState._nextItemX += nextColumnSet.GetColumnWidth(column) + leftMargin;
							}
							leftMargin = 0;
						}
					}
				}
				mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			}
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildGroupTotalRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return BuildColumnSetGroupTotalLayout(graphics, gridRow, mordexRow, formatStyle, value);
			}
			int nextItemX = 0;
			int leftMargin = 0;
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			LayoutItemType type = LayoutItemType.GroupTotalCell;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.GroupTotalRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
				Font otherFont = gridFormatStyle.ResolveFont(font);
				int otherValue = (int)otherFont.GetHeight(graphics) + 2;
				mordexRowLayoutState._rowHeight = 0;
				foreach (GridColumn column in Columns)
				{
					if (column.Visible)
					{
						int nextValue = otherValue;
						GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.Cells[column.Index].FormatStyle);
						Font nextFont = applyOverrides.ResolveFont(font);
						if (nextFont != otherFont)
						{
							nextValue = (int)nextFont.GetHeight(graphics) + 2;
						}
						if (gridRow.Cells[column.Index].HasImageReference() && nextValue < height)
						{
							nextValue = (height = column.TopMargin + column.BottomMargin);
						}
						nextValue += column.TopMargin + column.BottomMargin;
						if (applyOverrides != gridFormatStyle)
						{
							applyOverrides.DisposeResources();
						}
						if (mordexRowLayoutState._rowHeight < nextValue)
						{
							mordexRowLayoutState._rowHeight = nextValue;
						}
					}
				}
				if (mordexRowLayoutState._rowHeight < height + 2)
				{
					mordexRowLayoutState._rowHeight = height + 2;
				}
				mordexRowLayoutState._rowHeight += 2;
				if (gridFormatStyle != formatStyle)
				{
					gridFormatStyle.DisposeResources();
				}
			}
			else
			{
				mordexRowLayoutState._rowHeight = _rowHeight;
			}
			BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow().ParentRow, false, true);
			if (value != 0)
			{
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					item._isHorizontallyScrolled = false;
				}
			}
			int index = gridRow.GetLogicalRow().ParentRow.Group.Index;
			int previousValue = Groups.Count - (index + 1);
			leftMargin = previousValue * GetGroupIndentWidth();
			if (AreRowHeadersVisible())
			{
				leftMargin += RowHeaderWidth;
			}
			if (IsHierarchical() && HierarchicalMode == HierarchicalMode.UseChildTables)
			{
				leftMargin += GetIndentWidth();
			}
			nextItemX = mordexRowLayoutState._nextItemX;
			_ = mordexRowLayoutState._arrayList.Count;
			nextItemX = mordexRowLayoutState._nextItemX;
			GridGroupLayoutState[] array = new GridGroupLayoutState[Columns.Count];
			for (int i = 0; i < Columns.Count; i++)
			{
				GridColumn otherColumn = Columns[i];
				int field_ = otherColumn._position;
				array[field_] = new GridGroupLayoutState(LayoutItemType.ColumnHeader, mordexRowLayoutState, otherColumn);
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j]._column.Visible)
				{
					int width = array[j]._column.Width + leftMargin;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: type, owner: mordexRowLayoutState, column: array[j]._column);
					if (gridGroupLayoutState._column._position < value)
					{
						gridGroupLayoutState._isHorizontallyScrolled = false;
					}
					if ((j == array.Length - 1 && GetEffectiveGridLines() == GridLines.Both) || GetEffectiveGridLines() == GridLines.Vertical)
					{
						gridGroupLayoutState._borderFlags = BorderFlags.Right;
					}
					gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
					gridGroupLayoutState.CopyColumnMargins(array[j]._column);
					if (leftMargin != 0)
					{
						gridGroupLayoutState._leftMargin += leftMargin;
						leftMargin = 0;
					}
					mordexRowLayoutState._nextItemX += gridGroupLayoutState._rect.Width;
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
				}
			}
			mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._top = -1;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal bool IsTotalRowVisible()
		{
			if (TotalRow == InheritableBoolean.Default)
			{
				if (DataGrid != null && DataGrid.TotalRow == InheritableBoolean.True)
				{
					return true;
				}
			}
			else if (TotalRow == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal GridGroup FindGroupForColumn(GridColumn gridColumn)
		{
			foreach (GridGroup group in Groups)
			{
				if (group.Column == gridColumn)
				{
					return group;
				}
			}
			return null;
		}

		public void ApplyFilter(GridFilterCondition condition)
		{
			FilterCondition = condition;
		}

		public void RemoveFilter()
		{
			FilterCondition = null;
		}

		internal void ClearFilters()
		{
			FilterCondition = null;
			foreach (GridTable childTable in ChildTables)
			{
				childTable.ClearFilters();
			}
			foreach (GridColumn column in Columns)
			{
				column.ResetFilterRowComparison();
			}
		}
	}
}
