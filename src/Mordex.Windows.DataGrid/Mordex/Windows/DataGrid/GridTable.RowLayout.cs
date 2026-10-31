using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable
	{
		internal MxRowLayoutState BuildColumnSetNewRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle otherGridFormatStyle, int value)
		{
			int indentWidth = 0;
			int nextItemX = 0;
			int[] array = new int[_columnSetRowCount];
			Font font = DataGrid.Font;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.NewRow, this);
			if (_rowHeight == -1)
			{
				Font otherFont = otherGridFormatStyle.ResolveFont(font);
				int arrayItem = (int)otherFont.GetHeight(graphics) + 2;
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
							GridCell gridCell = gridRow.Cells[gridColumn.Index];
							int otherArrayItem = arrayItem;
							if (gridCell.CellDisplayType == ColumnType.BoundImage)
							{
								if (gridColumn.BoundImageSize != Size.Empty)
								{
									otherArrayItem = gridColumn.BoundImageSize.Height + gridColumn.TopMargin + gridColumn.BottomMargin;
								}
								else if (gridCell.Value is Image image)
								{
									otherArrayItem = image.Size.Height + gridColumn.TopMargin + gridColumn.BottomMargin;
								}
							}
							else if (gridCell.CellDisplayType == ColumnType.Link || gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
							{
								GridFormatStyle gridFormatStyle = otherGridFormatStyle.ApplyOverrides(gridColumn.CellStyle);
								Font nextFont = gridFormatStyle.ResolveFont(font);
								if (nextFont != otherFont || gridColumn.WordWrap)
								{
									TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(otherGridFormatStyle, !DataGrid.UseCompatibleTextRendering);
									otherArrayItem = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, gridColumn._width - gridColumn.LeftMargin - gridColumn.RightMargin - 2, gridColumn.MinLines, gridColumn.MaxLines);
									textLayoutRenderer.ReleaseStringFormat();
								}
								otherArrayItem += gridColumn.TopMargin + gridColumn.BottomMargin;
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
					array[i] += 2;
					if (GetEffectiveGridLines() == GridLines.Horizontal || GetEffectiveGridLines() == GridLines.Both)
					{
						array[i]++;
					}
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
			if (mordexRow == null && IsRootTable())
			{
				indentWidth = CalculateIndentWidth(true);
				nextItemX = mordexRowLayoutState._nextItemX;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					gridGroupLayoutState._isHorizontallyScrolled = value == 0;
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else
			{
				if (ComputeNewRowPosition() == NewRowPosition.TopRow)
				{
					mordexRowLayoutState._rowHeight += 4;
				}
				indentWidth = BuildIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow(), false, true);
				nextItemX = mordexRowLayoutState._nextItemX;
				if (ComputeNewRowPosition() == NewRowPosition.TopRow)
				{
					mordexRowLayoutState._rowHeight -= 4;
				}
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			_ = mordexRowLayoutState._arrayList.Count;
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
							GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.Cell, mordexRowLayoutState, otherColumn, rectangle);
							otherGroupLayoutState.CopyColumnMargins(otherColumn);
							mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
							if (nextValue != 0)
							{
								otherGroupLayoutState._leftMargin += indentWidth;
								nextValue = 0;
							}
							mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
							if (otherColumnSet.Position < value)
							{
								otherGroupLayoutState._isHorizontallyScrolled = false;
							}
						}
						else if (otherColumn == null || !otherColumn.Visible)
						{
							int otherWidth = otherColumnSet.GetColumnWidth(column) + nextValue;
							int height = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, height);
							GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, otherColumn, otherRectangle);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								nextGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								nextGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += nextGroupLayoutState._rect.Width;
							if (nextValue != 0)
							{
								nextGroupLayoutState._leftMargin += indentWidth;
								nextValue = 0;
							}
							mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
							if (otherColumnSet.Position < value)
							{
								nextGroupLayoutState._isHorizontallyScrolled = false;
							}
						}
						else
						{
							if (otherColumn.ColumnSetRow != n)
							{
								mordexRowLayoutState._nextItemX += otherColumnSet.GetColumnWidth(column) + nextValue;
							}
							nextValue = 0;
						}
					}
				}
				mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			}
			if (ComputeNewRowPosition() == NewRowPosition.TopRow)
			{
				int nextWidth = mordexRowLayoutState._nextItemX - nextItemX;
				mordexRowLayoutState._rowHeight = 4;
				mordexRowLayoutState._nextItemX = nextItemX;
				Rectangle nextRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, nextWidth, mordexRowLayoutState._rowHeight);
				GridGroupLayoutState previousGroupLayoutState = new GridGroupLayoutState(LayoutItemType.NewRowSeparator, mordexRowLayoutState, null, nextRectangle);
				mordexRowLayoutState._nextItemX += previousGroupLayoutState._rect.Width;
				mordexRowLayoutState._arrayList.Add(previousGroupLayoutState);
			}
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildDataRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle otherGridFormatStyle, int value, int otherValue)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return BuildColumnSetDataRowLayout(graphics, gridRow, mordexRow, otherGridFormatStyle, value, otherValue);
			}
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			int nextItemX = 0;
			int nextValue = 0;
			LayoutItemType type = LayoutItemType.Cell;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.DataRow, this);
			if (gridRow.GetLogicalRow() != null && gridRow.GetLogicalRow() is DataMordexRow)
			{
				mordexRowLayoutState._rowType = LayoutRowType.EmptyRow;
			}
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = otherGridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
				Font otherFont = applyOverrides.ResolveFont(font);
				int previousValue = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 3;
				mordexRowLayoutState._rowHeight = 0;
				foreach (GridColumn column in Columns)
				{
					if (!column.Visible)
					{
						continue;
					}
					int otherHeight = previousValue;
					GridCell gridCell = gridRow.Cells[column.Index];
					if (gridCell.CellDisplayType == ColumnType.BoundImage)
					{
						if (gridCell.ImageSize.IsEmpty)
						{
							if (column.BoundImageSize != Size.Empty)
							{
								otherHeight = column.BoundImageSize.Height + column.TopMargin + column.BottomMargin;
							}
							else if (gridCell.Value is Image image)
							{
								otherHeight = image.Size.Height + column.TopMargin + column.BottomMargin;
							}
						}
						else
						{
							otherHeight = gridCell.ImageSize.Height + column.TopMargin + column.BottomMargin;
						}
					}
					else if (gridCell.CellDisplayType == ColumnType.Link || gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
					{
						GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(column.CellStyle);
						GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridCell.GetEffectiveFormatStyle());
						GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridCell.FormatStyle);
						Font nextFont = previousApplyOverrides.ResolveFont(font);
						if (nextFont != otherFont || column.WordWrap)
						{
							TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(previousApplyOverrides, !DataGrid.UseCompatibleTextRendering);
							otherHeight = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
							if (column.WordWrap && column.MaxLines > 1)
							{
								otherHeight = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines) - 2;
								if (otherHeight != previousValue)
								{
									otherHeight += 2;
								}
								textLayoutRenderer.SetWordWrap(false);
							}
							else if (nextFont == otherFont && column.WordWrap && column.MaxLines == 1)
							{
								otherHeight = previousValue;
							}
							textLayoutRenderer.ReleaseStringFormat();
						}
						if (gridCell.CellDisplayType == ColumnType.ImageAndText && otherHeight < height)
						{
							otherHeight = height;
						}
						Image otherImage = gridCell.Image;
						if (otherImage != null && otherHeight < otherImage.Height)
						{
							otherHeight = otherImage.Height;
						}
						otherHeight += column.TopMargin + column.BottomMargin;
						if (previousApplyOverrides.ImageVerticalAlignment == ImageVerticalAlignment.TopOfText || previousApplyOverrides.ImageVerticalAlignment == ImageVerticalAlignment.BottomOfText)
						{
							Image containsKeyCore = gridCell.ResolveImage(DataGrid.ComputeImageList());
							if (containsKeyCore != null)
							{
								otherHeight += containsKeyCore.Height + 4;
							}
						}
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
						otherHeight = ((height + column.TopMargin + column.BottomMargin <= previousValue) ? previousValue : (height + column.TopMargin + column.BottomMargin));
					}
					else if (gridCell.CellDisplayType == ColumnType.CheckBox)
					{
						otherHeight = 14 + column.TopMargin + column.BottomMargin;
					}
					else if (gridCell.CellDisplayType == ColumnType.Link)
					{
						otherHeight += column.TopMargin + column.TopMargin + 2;
					}
					if (mordexRowLayoutState._rowHeight < otherHeight)
					{
						mordexRowLayoutState._rowHeight = otherHeight;
					}
				}
				if (mordexRowLayoutState._rowHeight < height + 3)
				{
					mordexRowLayoutState._rowHeight = height + 3;
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
				mordexRowLayoutState._rowHeight = _rowHeight;
			}
			int indentWidth = 0;
			if (mordexRowLayoutState._rowType == LayoutRowType.EmptyRow)
			{
				indentWidth = CalculateIndentWidth(true);
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					gridGroupLayoutState._isHorizontallyScrolled = otherValue == 0;
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else
			{
				ApplyIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow());
			}
			if (otherValue != 0)
			{
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					item._isHorizontallyScrolled = false;
				}
			}
			nextValue = mordexRowLayoutState._arrayList.Count;
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
					int width = array[j]._column.Width + indentWidth;
					indentWidth = 0;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: type, owner: mordexRowLayoutState, column: array[j]._column);
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
					{
						groupLayoutState._borderFlags = BorderFlags.Right;
					}
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						groupLayoutState._borderFlags |= BorderFlags.Bottom;
					}
					if (groupLayoutState._column._position < otherValue)
					{
						groupLayoutState._isHorizontallyScrolled = false;
					}
					groupLayoutState.CopyColumnMargins(array[j]._column);
					mordexRowLayoutState._nextItemX += groupLayoutState._rect.Width;
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
				}
			}
			mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
			if (gridRow.PreviewRowVisible)
			{
				string text = gridRow.PreviewRowText;
				if (!GridHelper.IsTextLengthValid(text))
				{
					text = GridHelper.TruncateText(text);
				}
				if (text.Length > 0)
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
					int firstValue = 0;
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
					int columnsWidth = CalculateColumnsWidth();
					if (columnsWidth > value)
					{
						columnsWidth = value;
					}
					firstValue = _previewRowLeftMargin + 2;
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
					{
						firstValue++;
					}
					columnsWidth -= firstValue;
					SizeF sizeF = new SizeF(0f, 0f);
					GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null);
					TextLayoutRenderer otherTextLayoutRenderer = otherGroupLayoutState.ComputeTextLayoutRenderer(otherGridFormatStyle, !DataGrid.UseCompatibleTextRendering);
					otherTextLayoutRenderer.SetWordWrap(true);
					sizeF = otherTextLayoutRenderer.MeasureText(graphics, text, thirdApplyOverrides.ResolveFont(font), columnsWidth);
					columnsWidth += firstValue;
					string otherText = "X";
					for (int k = 1; k < _previewRowLines; k++)
					{
						otherText += "\n\rX";
					}
					mordexRowLayoutState._rowHeight = (int)sizeF.Height;
					int nextHeight = (int)otherTextLayoutRenderer.MeasureText(graphics, otherText, thirdApplyOverrides.ResolveFont(font), columnsWidth).Height;
					if (mordexRowLayoutState._rowHeight > nextHeight)
					{
						mordexRowLayoutState._rowHeight = nextHeight;
					}
					mordexRowLayoutState._rowHeight += 6;
					Rectangle rectangle = new Rectangle(nextItemX, mordexRowLayoutState._top, columnsWidth, mordexRowLayoutState._rowHeight);
					if (GetEffectiveGridLines() == GridLines.Horizontal)
					{
						foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
						{
							if (arrayListItem._type == LayoutItemType.Cell)
							{
								arrayListItem._drawBottomGridline = false;
							}
						}
					}
					otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null, rectangle);
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
					{
						otherGroupLayoutState._borderFlags = BorderFlags.Right;
					}
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						otherGroupLayoutState._borderFlags |= BorderFlags.Bottom;
					}
					otherGroupLayoutState._isHorizontallyScrolled = otherValue == 0;
					mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
					otherGroupLayoutState._leftMargin = _previewRowLeftMargin;
					otherGroupLayoutState._topMargin = 2;
					otherGroupLayoutState._rightMargin = 2;
					otherGroupLayoutState._bottomMargin = 2;
					mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
					for (int m = 0; m < nextValue; m++)
					{
						((GridGroupLayoutState)mordexRowLayoutState._arrayList[m])._rect.Height += mordexRowLayoutState._rowHeight;
					}
					foreach (GridGroupLayoutState otherArrayListItem in mordexRowLayoutState._arrayList)
					{
						if (otherArrayListItem._type == LayoutItemType.Cell && otherArrayListItem._rect.Bottom == mordexRowLayoutState._top && _cellLayoutMode == CellLayoutMode.UseColumns && (otherArrayListItem._borderFlags & BorderFlags.Bottom) != BorderFlags.None)
						{
							otherArrayListItem._borderFlags ^= BorderFlags.Bottom;
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
				}
			}
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._nextItemX = nextItemX - 1;
			mordexRowLayoutState._top = -1;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildSpaceRowLayout(Graphics graphics, MordexRow otherMordexRow, MordexRow nextMordexRow, int value, bool condition)
		{
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.Space, this);
			mordexRowLayoutState._owner = this;
			mordexRowLayoutState._rowHeight = value;
			if (((GridTable)otherMordexRow.Table).HierarchyLevel > ((GridTable)nextMordexRow.Table).HierarchyLevel)
			{
				ArrayList arrayList = RowStateHelper.ComputeArrayList(otherMordexRow);
				for (int i = 0; i < arrayList.Count; i++)
				{
					MordexRow mordexRow = (MordexRow)arrayList[i];
					GridTable gridTable = (GridTable)mordexRow.Table;
					if (mordexRow is MordexGroupRow)
					{
						Rectangle rectangle;
						if (mordexRow.Group.Index == 0 && !gridTable.IsRootTable())
						{
							rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, GetIndentWidth(), mordexRowLayoutState._rowHeight);
							GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.Connector, mordexRowLayoutState, null, rectangle);
							if ((gridTable.GetEffectiveGridLines() == GridLines.Both || gridTable.GetEffectiveGridLines() == GridLines.Vertical) && mordexRow.Table != otherMordexRow.Table)
							{
								gridGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (!RowStateHelper.IsLastVisibleSibling(mordexRow) || mordexRow == otherMordexRow || RowStateHelper.GetNextVisibleSibling(mordexRow) != null)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
							}
							mordexRowLayoutState._nextItemX += rectangle.Width;
							mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
						if (mordexRow.Table == otherMordexRow.Table)
						{
							continue;
						}
						rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, GetGroupIndentWidth(), mordexRowLayoutState._rowHeight);
						GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupIndent, mordexRowLayoutState, null, rectangle);
						groupLayoutState._gridGroup = (GridGroup)mordexRow.Group;
						groupLayoutState.SetMordexRow(mordexRow);
						groupLayoutState._borderFlags = BorderFlags.Right;
						if (!mordexRow.Expanded && !condition)
						{
							groupLayoutState._borderFlags |= BorderFlags.Bottom;
						}
						mordexRowLayoutState._nextItemX += rectangle.Width;
						mordexRowLayoutState._arrayList.Add(groupLayoutState);
					}
					else if (gridTable.IsHierarchical() || gridTable.ParentTable != null)
					{
						Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, GetIndentWidth(), mordexRowLayoutState._rowHeight);
						GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.Connector, mordexRowLayoutState, null, otherRectangle);
						if (!RowStateHelper.IsLastVisibleSibling(mordexRow) || mordexRow == otherMordexRow || RowStateHelper.GetNextVisibleSibling(mordexRow) != null)
						{
							otherGroupLayoutState._borderFlags = BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
						}
						mordexRowLayoutState._nextItemX += otherRectangle.Width;
						mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
					}
				}
			}
			else
			{
				ArrayList arrayList = RowStateHelper.ComputeArrayList(nextMordexRow);
				for (int j = 0; j < arrayList.Count; j++)
				{
					MordexRow arrayListItem = (MordexRow)arrayList[j];
					GridTable table = (GridTable)arrayListItem.Table;
					if (arrayListItem is MordexGroupRow)
					{
						Rectangle nextRectangle;
						if (arrayListItem.Group.Index == 0 && !table.IsRootTable())
						{
							nextRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, GetIndentWidth(), mordexRowLayoutState._rowHeight);
							GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.Connector, mordexRowLayoutState, null, nextRectangle);
							if (table.HierarchyLevel <= ((GridTable)otherMordexRow.Table).HierarchyLevel)
							{
								nextGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (!RowStateHelper.IsLastVisibleSibling(arrayListItem) || arrayListItem == otherMordexRow || RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
							{
								nextGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
							}
							mordexRowLayoutState._nextItemX += nextRectangle.Width;
							mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
						}
						if (table.HierarchyLevel > ((GridTable)otherMordexRow.Table).HierarchyLevel)
						{
							continue;
						}
						nextRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, GetGroupIndentWidth(), mordexRowLayoutState._rowHeight);
						GridGroupLayoutState previousGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupIndent, mordexRowLayoutState, null, nextRectangle);
						previousGroupLayoutState._gridGroup = (GridGroup)arrayListItem.Group;
						previousGroupLayoutState.SetMordexRow(arrayListItem);
						previousGroupLayoutState._borderFlags = BorderFlags.Right;
						if (!arrayListItem.Expanded && !condition)
						{
							previousGroupLayoutState._borderFlags |= BorderFlags.Bottom;
						}
						mordexRowLayoutState._nextItemX += nextRectangle.Width;
						mordexRowLayoutState._arrayList.Add(previousGroupLayoutState);
					}
					else if (table.IsHierarchical() || table.ParentTable != null)
					{
						Rectangle previousRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, GetIndentWidth(), mordexRowLayoutState._rowHeight);
						GridGroupLayoutState firstGroupLayoutState = new GridGroupLayoutState(LayoutItemType.Connector, mordexRowLayoutState, null, previousRectangle);
						if (!RowStateHelper.IsLastVisibleSibling(arrayListItem) || arrayListItem == otherMordexRow || RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
						{
							firstGroupLayoutState._borderFlags = BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
						}
						mordexRowLayoutState._nextItemX += previousRectangle.Width;
						mordexRowLayoutState._arrayList.Add(firstGroupLayoutState);
					}
				}
			}
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildColumnSetDataRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle otherGridFormatStyle, int value, int otherValue)
		{
			int nextItemX = 0;
			int nextValue = 0;
			int[] array = new int[_columnSetRowCount];
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			LayoutItemType type = LayoutItemType.Cell;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.DataRow, this);
			if (gridRow != null && gridRow.GetLogicalRow() != null && gridRow.GetLogicalRow() is DataMordexRow)
			{
				mordexRowLayoutState._rowType = LayoutRowType.EmptyRow;
			}
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = otherGridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
				Font otherFont = applyOverrides.ResolveFont(font);
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
							GridCell gridCell = gridRow.Cells[gridColumn.Index];
							int otherArrayItem = arrayItem;
							if (gridCell.CellDisplayType == ColumnType.BoundImage)
							{
								if (gridCell.ImageSize.IsEmpty)
								{
									if (gridColumn.BoundImageSize != Size.Empty)
									{
										otherArrayItem = gridColumn.BoundImageSize.Height + gridColumn.TopMargin + gridColumn.BottomMargin;
									}
									else if (gridCell.Value is Image image)
									{
										otherArrayItem = image.Size.Height + gridColumn.TopMargin + gridColumn.BottomMargin;
									}
								}
								else
								{
									otherArrayItem = gridCell.ImageSize.Height + gridColumn.TopMargin + gridColumn.BottomMargin;
								}
							}
							else if (gridCell.CellDisplayType == ColumnType.Link || gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
							{
								GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridColumn.CellStyle);
								GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridCell.GetEffectiveFormatStyle());
								GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridCell.FormatStyle);
								Font nextFont = previousApplyOverrides.ResolveFont(font);
								if (nextFont != otherFont || gridColumn.WordWrap)
								{
									TextLayoutRenderer textLayoutRenderer = gridColumn.ComputeTextLayoutRenderer(previousApplyOverrides, !DataGrid.UseCompatibleTextRendering);
									otherArrayItem = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, gridColumn._width - gridColumn.LeftMargin - gridColumn.RightMargin - 2, gridColumn.MinLines, gridColumn.MaxLines);
									textLayoutRenderer.ReleaseStringFormat();
								}
								if (gridCell.CellDisplayType == ColumnType.ImageAndText && otherArrayItem < height)
								{
									otherArrayItem = height;
								}
								otherArrayItem += gridColumn.TopMargin + gridColumn.BottomMargin;
								if (previousApplyOverrides.ImageVerticalAlignment == ImageVerticalAlignment.TopOfText || previousApplyOverrides.ImageVerticalAlignment == ImageVerticalAlignment.BottomOfText)
								{
									Image containsKeyCore = gridCell.ResolveImage(DataGrid.ComputeImageList());
									if (containsKeyCore != null)
									{
										otherArrayItem += containsKeyCore.Height + 4;
									}
								}
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
							else if (gridCell.CellDisplayType == ColumnType.ImageAndText)
							{
								otherArrayItem = height + gridColumn.TopMargin + gridColumn.BottomMargin;
							}
							else if (gridCell.CellDisplayType == ColumnType.CheckBox)
							{
								otherArrayItem = 14 + gridColumn.TopMargin + gridColumn.BottomMargin;
							}
							if (gridColumn._rowSpan > 1)
							{
								int previousValue = 0;
								for (int k = gridColumn._columnSetRow; k < gridColumn._columnSetRow + gridColumn._rowSpan - 1; k++)
								{
									previousValue += array[k];
								}
								otherArrayItem -= previousValue;
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
					if (GetEffectiveGridLines() == GridLines.Horizontal || GetEffectiveGridLines() == GridLines.Both)
					{
						array[i]++;
					}
					mordexRowLayoutState._rowHeight += array[i];
					if (applyOverrides != gridFormatStyle)
					{
						applyOverrides.DisposeResources();
					}
					if (gridFormatStyle != otherGridFormatStyle)
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
			int indentWidth = 0;
			if (mordexRowLayoutState._rowType == LayoutRowType.EmptyRow)
			{
				indentWidth = CalculateIndentWidth(true);
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Left | BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
					gridGroupLayoutState._isHorizontallyScrolled = true;
					gridGroupLayoutState._isHorizontallyScrolled = otherValue == 0;
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else
			{
				ApplyIndentLayout(mordexRowLayoutState, gridRow.GetLogicalRow());
				if (otherValue != 0)
				{
					foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
					{
						arrayListItem._isHorizontallyScrolled = false;
					}
				}
			}
			nextValue = mordexRowLayoutState._arrayList.Count;
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
				int leftMargin = indentWidth;
				mordexRowLayoutState._rowHeight = array[n];
				mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
				for (int i = 0; i < _columnSets.Count; i++)
				{
					GridColumnSet otherColumnSet = columnSets[i];
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
							GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(type, mordexRowLayoutState, otherColumn, rectangle);
							groupLayoutState.CopyColumnMargins(otherColumn);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
							{
								groupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
							{
								groupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += groupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								groupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._arrayList.Add(groupLayoutState);
							groupLayoutState._isHorizontallyScrolled = otherColumnSet._position >= otherValue;
						}
						else if (otherColumn == null || !otherColumn.Visible)
						{
							int otherWidth = otherColumnSet.GetColumnWidth(column) + leftMargin;
							int otherHeight = mordexRowLayoutState._rowHeight;
							Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, otherHeight);
							GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyCell, mordexRowLayoutState, otherColumn, otherRectangle);
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical || GetEffectiveGridLines() == GridLines.RowOutline)
							{
								otherGroupLayoutState._borderFlags = BorderFlags.Right;
							}
							if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal || GetEffectiveGridLines() == GridLines.RowOutline)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
							if (leftMargin != 0)
							{
								otherGroupLayoutState._leftMargin += leftMargin;
								leftMargin = 0;
							}
							mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
							otherGroupLayoutState._isHorizontallyScrolled = otherColumnSet._position >= otherValue;
						}
						else if (otherColumn._columnSetRow != n)
						{
							mordexRowLayoutState._nextItemX += otherColumnSet.GetColumnWidth(column) + leftMargin;
							leftMargin = 0;
						}
					}
				}
				mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			}
			mordexRowLayoutState._top -= mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._rightEdge = mordexRowLayoutState._nextItemX - 1;
			if (gridRow.PreviewRowVisible)
			{
				string text = gridRow.PreviewRowText;
				if (!GridHelper.IsTextLengthValid(text))
				{
					text = GridHelper.TruncateText(text);
				}
				if (text.Length > 0)
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
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
					int columnsWidth = CalculateColumnsWidth();
					if (columnsWidth > value)
					{
						columnsWidth = value;
					}
					secondValue = _previewRowLeftMargin + 2;
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
					{
						secondValue++;
					}
					columnsWidth -= secondValue;
					SizeF sizeF = new SizeF(0f, 0f);
					GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null);
					TextLayoutRenderer otherTextLayoutRenderer = nextGroupLayoutState.ComputeTextLayoutRenderer(otherGridFormatStyle, !DataGrid.UseCompatibleTextRendering);
					otherTextLayoutRenderer.SetWordWrap(true);
					sizeF = otherTextLayoutRenderer.MeasureText(graphics, text, thirdApplyOverrides.ResolveFont(font), columnsWidth);
					columnsWidth += secondValue;
					int nextHeight = (int)thirdApplyOverrides.ResolveFont(font).GetHeight(graphics);
					mordexRowLayoutState._rowHeight = (int)sizeF.Height;
					if (mordexRowLayoutState._rowHeight > nextHeight * _previewRowLines)
					{
						mordexRowLayoutState._rowHeight = nextHeight * _previewRowLines;
					}
					mordexRowLayoutState._rowHeight += 6;
					Rectangle nextRectangle = new Rectangle(nextItemX, mordexRowLayoutState._top, columnsWidth, mordexRowLayoutState._rowHeight);
					nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.PreviewRow, mordexRowLayoutState, null, nextRectangle);
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
					{
						nextGroupLayoutState._borderFlags = BorderFlags.Right;
					}
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						nextGroupLayoutState._borderFlags |= BorderFlags.Bottom;
					}
					mordexRowLayoutState._nextItemX += nextGroupLayoutState._rect.Width;
					nextGroupLayoutState._leftMargin = _previewRowLeftMargin;
					nextGroupLayoutState._topMargin = 2;
					nextGroupLayoutState._rightMargin = 2;
					nextGroupLayoutState._bottomMargin = 2;
					nextGroupLayoutState._isHorizontallyScrolled = otherValue == 0;
					mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
					for (int i = 0; i < nextValue; i++)
					{
						((GridGroupLayoutState)mordexRowLayoutState._arrayList[i])._rect.Height += mordexRowLayoutState._rowHeight;
					}
					foreach (GridGroupLayoutState otherArrayListItem in mordexRowLayoutState._arrayList)
					{
						if (otherArrayListItem._type == LayoutItemType.Cell && otherArrayListItem._rect.Bottom == mordexRowLayoutState._top && _cellLayoutMode == CellLayoutMode.UseColumns && (otherArrayListItem._borderFlags & BorderFlags.Bottom) != BorderFlags.None)
						{
							otherArrayListItem._borderFlags ^= BorderFlags.Bottom;
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
				}
			}
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._top = -1;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal void BuildGroupHeaderCellLayouts(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value, MxRowLayoutState owner, GridGroupLayoutState gridGroupLayoutState)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return;
			}
			Font font = DataGrid.Font;
			_ = DataGrid.GetImageSize().Height;
			LayoutItemType type = LayoutItemType.GroupHeaderCell;
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
				Font otherFont = applyOverrides.ResolveFont(font);
				int otherValue = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				foreach (GridColumn column in Columns)
				{
					if (!column.Visible)
					{
						continue;
					}
					int nextValue = otherValue;
					GridFormatStyle otherApplyOverrides = applyOverrides;
					if (gridRow.Cells[column.Index]._groupHeaderTotal is GridGroupHeaderTotal gridGroupHeaderTotal)
					{
						otherApplyOverrides = applyOverrides.ApplyOverrides(gridGroupHeaderTotal.FormatStyle);
					}
					GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridRow.Cells[column.Index].FormatStyle);
					Font nextFont = nextApplyOverrides.ResolveFont(font);
					if (nextFont != otherFont)
					{
						TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(nextApplyOverrides, !DataGrid.UseCompatibleTextRendering);
						if (column.WordWrap)
						{
							nextValue = GridHelper.CalculateTextHeight(graphics, nextFont, gridRow.Cells[column.Index].Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
							textLayoutRenderer.SetWordWrap(false);
							nextValue = GridHelper.CalculateTextHeight(graphics, nextFont, gridRow.Cells[column.Index].Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
						}
						else
						{
							nextValue = GridHelper.CalculateTextHeight(graphics, nextFont, gridRow.Cells[column.Index].Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, 1, 1);
						}
						nextValue++;
					}
					nextValue += column.TopMargin + column.BottomMargin;
					if (nextApplyOverrides != otherApplyOverrides)
					{
						nextApplyOverrides.DisposeResources();
					}
					if (otherApplyOverrides != applyOverrides)
					{
						otherApplyOverrides.DisposeResources();
					}
					if (owner._rowHeight < nextValue)
					{
						owner._rowHeight = nextValue;
					}
					if (owner._rowHeight < nextValue)
					{
						owner._rowHeight = nextValue;
					}
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
			if (gridGroupLayoutState._rect.Height < owner._rowHeight)
			{
				foreach (GridGroupLayoutState item in owner._arrayList)
				{
					item._rect.Height = owner._rowHeight;
				}
				if (gridGroupLayoutState._children != null)
				{
					GridGroupLayoutState[] gridGroupLayoutStateArray = gridGroupLayoutState._children;
					foreach (GridGroupLayoutState gridGroupLayoutStateArrayItem in gridGroupLayoutStateArray)
					{
						gridGroupLayoutStateArrayItem._rect.Height = owner._rowHeight;
					}
				}
			}
			_ = owner._arrayList.Count;
			_ = owner._nextItemX;
			GridGroupLayoutState[] array = new GridGroupLayoutState[Columns.Count];
			for (int j = 0; j < Columns.Count; j++)
			{
				GridColumn otherColumn = Columns[j];
				int field_ = otherColumn._position;
				array[field_] = new GridGroupLayoutState(LayoutItemType.ColumnHeader, owner, otherColumn);
			}
			owner._nextItemX = CalculateIndentWidth(true);
			if (AreRowHeadersVisible())
			{
				owner._nextItemX += RowHeaderWidth;
			}
			bool condition = true;
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k]._column.Visible)
				{
					int width = array[k]._column.Width;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(owner._nextItemX, owner._top, width, owner._rowHeight), type: type, owner: owner, column: array[k]._column);
					if (groupLayoutState._column._position < value)
					{
						groupLayoutState._isHorizontallyScrolled = false;
					}
					groupLayoutState.CopyColumnMargins(array[k]._column);
					owner._nextItemX += groupLayoutState._rect.Width;
					owner._arrayList.Add(groupLayoutState);
					if (condition)
					{
						condition = false;
						groupLayoutState._isVisible = false;
					}
				}
			}
			owner._rightEdge = owner._nextItemX - 1;
			owner._rowHeight = owner._top + owner._rowHeight;
			owner.SetLocation(new Point(0, 0));
		}
	}
}
