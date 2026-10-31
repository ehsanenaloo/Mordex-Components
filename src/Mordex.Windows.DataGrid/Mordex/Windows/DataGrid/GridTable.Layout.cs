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
		internal bool AreColumnSetHeadersVisible()
		{
			if (_columnSetHeaders == InheritableBoolean.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.ShouldShowColumnSetHeaders();
				}
			}
			else if (_columnSetHeaders == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool AreColumnHeadersVisible()
		{
			if (_columnHeaders == InheritableBoolean.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.ShouldShowColumnHeaders();
				}
			}
			else if (_columnHeaders == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool AreRowHeadersVisible()
		{
			if (_rowHeaders == InheritableBoolean.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.ShouldShowRowHeaders();
				}
			}
			else if (_rowHeaders == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		public void ResetRowHeaderWidth()
		{
			RowHeaderWidth = -1;
		}

		internal int CalculateColumnsWidth()
		{
			int columnsWidth = 0;
			if (_cellLayoutMode == CellLayoutMode.UseColumns)
			{
				foreach (GridColumn column in Columns)
				{
					if (column.Visible)
					{
						columnsWidth += column.Width;
					}
				}
			}
			else
			{
				foreach (GridColumnSet item in _columnSets)
				{
					if (item.Visible)
					{
						columnsWidth += item.Width;
					}
				}
			}
			return columnsWidth;
		}

		internal int CalculateFullWidth()
		{
			int value = 0;
			if (AreRowHeadersVisible())
			{
				value += RowHeaderWidth;
			}
			value += CalculateIndentWidth(true);
			return value + CalculateColumnsWidth();
		}

		internal int CalculateHierarchyWidth(bool condition)
		{
			int hierarchyWidth = 0;
			if (AreRowHeadersVisible())
			{
				hierarchyWidth += RowHeaderWidth;
			}
			hierarchyWidth += CalculateColumnsWidth();
			if (condition)
			{
				if (IsRootTable())
				{
					return hierarchyWidth + CalculateIndentWidth(true);
				}
				return hierarchyWidth;
			}
			hierarchyWidth += CalculateIndentWidth(true);
			if (IsHierarchical())
			{
				foreach (GridTable childTable in ChildTables)
				{
					int otherHierarchyWidth = childTable.CalculateHierarchyWidth(false);
					if (otherHierarchyWidth > hierarchyWidth)
					{
						hierarchyWidth = otherHierarchyWidth;
					}
				}
			}
			return hierarchyWidth;
		}

		internal int GetIndentWidth()
		{
			if (DataGrid == null)
			{
				return 18;
			}
			int indentWidth = DataGrid.Indent;
			if (indentWidth % 2 != 0)
			{
				indentWidth++;
			}
			return indentWidth;
		}

		private int GetGroupIndentWidth()
		{
			if (DataGrid == null)
			{
				return 16;
			}
			return DataGrid.GroupIndent;
		}

		internal int CalculateIndentWidth(bool condition)
		{
			int indentWidth = 0;
			if (ParentTable != null)
			{
				indentWidth = ParentTable.CalculateIndentWidth(condition);
			}
			if ((IsHierarchical() && condition && !GetBaseIsSelfReferencingTable()) || ParentTable != null)
			{
				indentWidth += GetIndentWidth();
			}
			indentWidth += Groups.Count * GetGroupIndentWidth();
			if (Groups.Count > 0 && ParentTable != null && IsHierarchical() && !GetBaseIsSelfReferencingTable() && condition)
			{
				indentWidth += GetIndentWidth();
			}
			return indentWidth;
		}

		internal void ApplyIndentLayout(MxRowLayoutState mordexRowLayoutState, MordexRow mordexRow)
		{
			BuildIndentLayout(mordexRowLayoutState, mordexRow, true, true);
		}

		internal int BuildIndentLayout(MxRowLayoutState owner, MordexRow otherMordexRow, bool condition, bool otherCondition)
		{
			if (otherMordexRow == null)
			{
				return -1;
			}
			ArrayList arrayList = RowStateHelper.ComputeArrayList(otherMordexRow);
			MordexRow mordexRow = RowStateHelper.GetNextManagedRow(otherMordexRow);
			RowType rowType = RowStateHelper.ComputeRowType(otherMordexRow);
			for (int i = 0; i < arrayList.Count; i++)
			{
				MordexRow arrayListItem = (MordexRow)arrayList[i];
				RowType otherRowType = RowStateHelper.ComputeRowType(arrayListItem);
				GridTable gridTable = (GridTable)arrayListItem.Table;
				if (otherRowType == RowType.Record && gridTable.GetBaseIsSelfReferencingTable() && arrayListItem.Table == otherMordexRow.Table && (i == 0 || ((MordexRow)arrayList[i - 1]).Table == otherMordexRow.Table))
				{
					continue;
				}
				if (otherRowType == RowType.GroupHeader)
				{
					Rectangle rectangle;
					GridGroupLayoutState gridGroupLayoutState;
					if (arrayListItem.Group.Index == 0 && !gridTable.IsRootTable())
					{
						rectangle = new Rectangle(owner._nextItemX, owner._top, GetIndentWidth(), owner._rowHeight);
						gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.Connector, owner, null, rectangle);
						if (gridTable.GetEffectiveGridLines() == GridLines.Both || gridTable.GetEffectiveGridLines() == GridLines.Vertical)
						{
							gridGroupLayoutState._borderFlags = BorderFlags.Right;
						}
						if (owner._rowType != LayoutRowType.ColumnHeader && arrayListItem == otherMordexRow)
						{
							if (owner._rowType != LayoutRowType.GroupTotalRow)
							{
								gridGroupLayoutState._borderFlags = gridGroupLayoutState._borderFlags | BorderFlags.ConnectorTop | BorderFlags.ConnectorRight;
								if ((!RowStateHelper.IsLastVisibleSibling(arrayListItem) && RowStateHelper.GetNextManagedRow(arrayListItem) != null) || owner._rowType == LayoutRowType.EmptyRow)
								{
									gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorBottom;
								}
								else if (RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
								{
									gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorBottom;
								}
							}
							else if ((!RowStateHelper.IsLastVisibleSibling(arrayListItem) && RowStateHelper.GetNextManagedRow(arrayListItem) != null) || owner._rowType == LayoutRowType.EmptyRow)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
							}
							else if (RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
							}
						}
						else if ((!RowStateHelper.IsLastVisibleSibling(arrayListItem) && RowStateHelper.GetNextManagedRow(arrayListItem) != null && RowStateHelper.ComputeRowType(RowStateHelper.GetNextManagedRow(arrayListItem)) != RowType.GroupFooter) || owner._rowType == LayoutRowType.EmptyRow)
						{
							gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
						}
						else if (owner._rowType == LayoutRowType.ColumnHeader && arrayListItem == otherMordexRow)
						{
							gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
						}
						else if (RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
						{
							gridGroupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
						}
						owner._nextItemX += rectangle.Width;
						owner._arrayList.Add(gridGroupLayoutState);
					}
					rectangle = new Rectangle(owner._nextItemX, owner._top, GetGroupIndentWidth(), owner._rowHeight);
					if (otherMordexRow == arrayListItem && owner._rowType == LayoutRowType.GroupHeaderRow)
					{
						if (!AreGroupsExpandable())
						{
							gridGroupLayoutState = null;
							continue;
						}
						rectangle.Width--;
						gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupExpandBox, owner, null, rectangle);
						gridGroupLayoutState._gridGroup = (GridGroup)arrayListItem.Group;
						gridGroupLayoutState.SetMordexRow(arrayListItem);
						if (!arrayListItem.Expanded)
						{
							if (RowStateHelper.GetNextManagedRow(arrayListItem) == null)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							else if (otherRowType != RowType.GroupFooter)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
						}
					}
					else
					{
						if ((owner._rowType == LayoutRowType.ColumnHeader && (arrayListItem == otherMordexRow || (!((GridTable)otherMordexRow.Table).IsRootTable() && arrayListItem.Table == otherMordexRow.Table))))
						{
							continue;
						}
						gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupIndent, owner, null, rectangle);
						gridGroupLayoutState._gridGroup = (GridGroup)arrayListItem.Group;
						gridGroupLayoutState.SetMordexRow(arrayListItem);
						gridGroupLayoutState._borderFlags = BorderFlags.Right;
						switch (owner._rowType)
						{
						case LayoutRowType.GroupTotalRow:
							if (otherMordexRow == arrayListItem)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							break;
						default:
							if (mordexRow == null)
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							else if (!mordexRow.IsDescendentOf(arrayListItem) && (mordexRow.Table == otherMordexRow.Table || DataGrid.TableSpacing <= 0))
							{
								gridGroupLayoutState._borderFlags |= BorderFlags.Bottom;
							}
							break;
						case LayoutRowType.ColumnHeader:
							break;
						}
					}
					owner._nextItemX += rectangle.Width;
					owner._arrayList.Add(gridGroupLayoutState);
				}
				else
				{
					if ((!gridTable.IsHierarchical() || otherRowType == RowType.TotalRow) && gridTable.ParentTable == null)
					{
						continue;
					}
					Rectangle otherRectangle = new Rectangle(owner._nextItemX, owner._top, GetIndentWidth(), owner._rowHeight);
					GridGroupLayoutState groupLayoutState;
					if (otherMordexRow == arrayListItem && !gridTable.GetBaseIsSelfReferencingTable() && RowStateHelper.IsContainerRow(otherMordexRow) && owner._rowType != LayoutRowType.ColumnHeader && owner._rowType != LayoutRowType.GroupTotalRow && (rowType != RowType.NewRecord || (otherMordexRow.CanHaveChildren() && DataGrid.Hierarchical && ((GridTable)otherMordexRow.Table).ComputeNewRowPosition() == NewRowPosition.BottomRow)) && owner._rowType != LayoutRowType.TotalRow && RowStateHelper.RowHasChildren(otherMordexRow))
					{
						if (!otherCondition)
						{
							continue;
						}
						groupLayoutState = new GridGroupLayoutState(LayoutItemType.ExpandBox, owner, null, otherRectangle);
						if (mordexRow != null)
						{
							RowType nextRowType = RowStateHelper.ComputeRowType(mordexRow);
							if (mordexRow.Table == otherMordexRow.Table && (nextRowType == RowType.GroupHeader || nextRowType == RowType.GroupFooter))
							{
								groupLayoutState._borderFlags = BorderFlags.Bottom;
							}
						}
					}
					else
					{
						if (!otherCondition)
						{
							continue;
						}
						groupLayoutState = new GridGroupLayoutState(LayoutItemType.Connector, owner, null, otherRectangle);
					}
					if (arrayListItem == otherMordexRow && rowType != RowType.TotalRow)
					{
						if (owner._rowType != LayoutRowType.ColumnHeader && owner._rowType != LayoutRowType.GroupTotalRow)
						{
							groupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorRight;
							if (!RowStateHelper.IsLastVisibleSibling(arrayListItem) || owner._rowType == LayoutRowType.EmptyRow)
							{
								groupLayoutState._borderFlags |= BorderFlags.ConnectorBottom;
							}
							else if (RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
							{
								groupLayoutState._borderFlags |= BorderFlags.ConnectorBottom;
							}
						}
						else
						{
							groupLayoutState._borderFlags |= BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
						}
					}
					else if (!RowStateHelper.IsLastVisibleSibling(arrayListItem))
					{
						groupLayoutState._borderFlags = BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
					}
					else if (RowStateHelper.GetNextVisibleSibling(arrayListItem) != null)
					{
						groupLayoutState._borderFlags = BorderFlags.ConnectorTop | BorderFlags.ConnectorBottom;
					}
					owner._nextItemX += otherRectangle.Width;
					if (arrayListItem == otherMordexRow && (gridTable.GetEffectiveGridLines() == GridLines.Both || gridTable.GetEffectiveGridLines() == GridLines.Vertical) && !AreRowHeadersVisible())
					{
						groupLayoutState._borderFlags |= BorderFlags.Right;
					}
					owner._arrayList.Add(groupLayoutState);
				}
			}
			if (owner._rowType != LayoutRowType.GroupTotalRow && owner._rowType != LayoutRowType.GroupHeaderRow && AreRowHeadersVisible() && condition)
			{
				GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, owner, null, new Rectangle(owner._nextItemX, owner._top, RowHeaderWidth, owner._rowHeight));
				otherGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
				owner._nextItemX += otherGroupLayoutState._rect.Width;
				owner._arrayList.Add(otherGroupLayoutState);
			}
			int indentLayout = 0;
			int index = -1;
			if (otherMordexRow != null)
			{
				GridTable table = (GridTable)otherMordexRow.Table;
				if (owner._rowType == LayoutRowType.ColumnHeader)
				{
					if (rowType == RowType.GroupHeader)
					{
						index = otherMordexRow.Group.Index;
						for (int value = Groups.Count - 1; value >= index; value--)
						{
							indentLayout += GetGroupIndentWidth();
						}
					}
					else if (!IsHierarchical() || rowType == RowType.NewRecord || rowType == RowType.TotalRow)
					{
						index = table.Groups.Count;
						foreach (GridGroup group in Groups)
						{
							indentLayout += GetGroupIndentWidth();
						}
					}
				}
				else if (rowType == RowType.TotalRow || rowType == RowType.Record || rowType == RowType.NewRecord)
				{
					foreach (GridGroup otherGroup in Groups)
					{
						indentLayout += GetGroupIndentWidth();
					}
				}
				else if (otherMordexRow.Group != null)
				{
					index = otherMordexRow.Group.Index;
					for (int i = index; i >= 0; i--)
					{
						indentLayout += GetGroupIndentWidth();
					}
				}
				if (table.Groups.Count > 0 && table.IsHierarchical())
				{
					if ((rowType == RowType.GroupHeader || rowType == RowType.GroupFooter || rowType == RowType.NewRecord || rowType == RowType.TotalRow) && !table.GetBaseIsSelfReferencingTable())
					{
						indentLayout += GetIndentWidth();
					}
				}
				else if (table.IsRootTable() && table.IsHierarchical() && rowType == RowType.TotalRow && !table.GetBaseIsSelfReferencingTable())
				{
					indentLayout += GetIndentWidth();
				}
			}
			arrayList.Clear();
			arrayList = null;
			for (int j = owner._arrayList.Count - 1; j >= 0; j--)
			{
				GridGroupLayoutState otherArrayListItem = (GridGroupLayoutState)owner._arrayList[j];
				if (otherArrayListItem._type == LayoutItemType.ExpandBox)
				{
					break;
				}
				if (otherArrayListItem._type == LayoutItemType.Connector)
				{
					otherArrayListItem._rightMargin = 0;
					break;
				}
			}
			return indentLayout;
		}

		internal MxRowLayoutState BuildColumnHeaderLayout(Graphics graphics, MordexRow mordexRow, MordexRow otherMordexRow, GridFormatStyle gridFormatStyle, int value, bool condition)
		{
			MxRowLayoutState mordexRowLayoutState = null;
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				mordexRowLayoutState = BuildColumnSetHeaderLayout(graphics, mordexRow, otherMordexRow, gridFormatStyle, value, condition);
			}
			else
			{
				bool otherCondition = AreColumnHeadersVisible() || condition;
				_ = DataGrid.Font;
				int nextItemX = 0;
				int indentWidth = 0;
				mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, this);
				int columnHeaderHeight = CalculateColumnHeaderHeight(graphics, gridFormatStyle, false);
				int tableHeaderHeight = CalculateTableHeaderHeight(graphics);
				if (otherCondition)
				{
					mordexRowLayoutState._rowHeight = columnHeaderHeight;
				}
				if ((otherMordexRow == null || mordexRow == null) && IsRootTable())
				{
					mordexRowLayoutState._top = tableHeaderHeight;
					indentWidth = CalculateIndentWidth(true);
					if (AreRowHeadersVisible() && otherCondition)
					{
						indentWidth += RowHeaderWidth;
						GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
						if (IsTableHeaderVisible())
						{
							gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
						}
						mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						gridGroupLayoutState._isHorizontallyScrolled = value == 0;
						mordexRowLayoutState._nextItemX += indentWidth;
						indentWidth = 0;
					}
				}
				else
				{
					mordexRowLayoutState._rowHeight += tableHeaderHeight;
					indentWidth = BuildIndentLayout(mordexRowLayoutState, mordexRow, false, true);
					foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
					{
						item._isHorizontallyScrolled = value == 0;
					}
					nextItemX = mordexRowLayoutState._nextItemX;
					mordexRowLayoutState._top += tableHeaderHeight;
					mordexRowLayoutState._rowHeight = columnHeaderHeight;
					if (AreRowHeadersVisible() && otherCondition)
					{
						indentWidth += RowHeaderWidth;
						GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
						groupLayoutState._isHorizontallyScrolled = value == 0;
						if (IsTableHeaderVisible())
						{
							groupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
						}
						mordexRowLayoutState._arrayList.Add(groupLayoutState);
						mordexRowLayoutState._nextItemX += indentWidth;
						indentWidth = 0;
					}
				}
				int right = 0;
				GridGroupLayoutState[] array = new GridGroupLayoutState[Columns.Count];
				for (int i = 0; i < Columns.Count; i++)
				{
					GridColumn gridColumn = Columns[i];
					int field_ = gridColumn._position;
					array[field_] = new GridGroupLayoutState(LayoutItemType.ColumnHeader, mordexRowLayoutState, gridColumn);
				}
				int otherValue = -1;
				int nextValue = -1;
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j]._column.Visible)
					{
						if (otherValue == -1)
						{
							otherValue = j;
						}
						nextValue = j;
					}
				}
				bool nextCondition = !IsTableHeaderVisible();
				if (otherCondition)
				{
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k]._column.Visible)
						{
							int width = array[k]._column.Width + indentWidth;
							Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight);
							GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnHeader, mordexRowLayoutState, array[k]._column, rectangle);
							if (k != otherValue || AreRowHeadersVisible())
							{
								otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorLeft;
							}
							if (!nextCondition)
							{
								otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
							}
							if (k == nextValue)
							{
								otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Right;
							}
							else
							{
								otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorRight;
							}
							otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Bottom;
							if (array[k]._column.ActAsSelector && otherGroupLayoutState._column.UseHeaderSelector)
							{
								otherGroupLayoutState.SetMordexRow(mordexRow);
							}
							if (otherGroupLayoutState._column._position < value)
							{
								otherGroupLayoutState._isHorizontallyScrolled = false;
							}
							mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
							otherGroupLayoutState._leftMargin = 1;
							otherGroupLayoutState._rightMargin = 1;
							otherGroupLayoutState._topMargin = 1;
							if (indentWidth != 0)
							{
								otherGroupLayoutState._leftMargin += indentWidth;
								indentWidth = 0;
							}
							if (right < rectangle.Right)
							{
								right = rectangle.Right;
							}
							mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
						}
					}
				}
				if (IsTableHeaderVisible())
				{
					int hierarchyWidth = CalculateHierarchyWidth(true);
					if (IsRootTable())
					{
						hierarchyWidth -= nextItemX;
					}
					Rectangle otherRectangle = new Rectangle(nextItemX, 0, hierarchyWidth, tableHeaderHeight);
					GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TableHeader, mordexRowLayoutState, null, otherRectangle);
					nextGroupLayoutState._isHorizontallyScrolled = value == 0;
					nextGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom;
					mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
				}
				mordexRowLayoutState._rightEdge = 0;
				mordexRowLayoutState._nextItemX = nextItemX;
				if (otherCondition)
				{
					mordexRowLayoutState._rowHeight = mordexRowLayoutState._top + mordexRowLayoutState._rowHeight;
				}
				else
				{
					mordexRowLayoutState._rowHeight = tableHeaderHeight;
				}
				mordexRowLayoutState.SetLocation(new Point(0, 0));
			}
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildForcedColumnSetHeaderLayout(Graphics graphics, GridFormatStyle gridFormatStyle)
		{
			InheritableBoolean inheritableBoolean = _columnSetHeaders;
			_columnSetHeaders = InheritableBoolean.True;
			MxRowLayoutState result = BuildColumnSetHeaderLayout(graphics, null, null, gridFormatStyle, 0, false);
			_columnSetHeaders = inheritableBoolean;
			return result;
		}

		internal MxRowLayoutState BuildColumnSetHeaderLayout(Graphics graphics, MordexRow mordexRow, MordexRow otherMordexRow, GridFormatStyle formatStyle, int value, bool condition)
		{
			int nextItemX = 0;
			int indentWidth = 0;
			int otherValue = 0;
			int tableHeaderHeight = CalculateTableHeaderHeight(graphics);
			int[] array = new int[_columnSetRowCount];
			Font font = DataGrid.Font;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, this);
			if (AreColumnHeadersVisible() || condition)
			{
				for (int i = 0; i < _columnSetRowCount; i++)
				{
					int nextValue = formatStyle.ResolveFont(font).Height * _headerLines;
					if (nextValue < DataGrid.GetImageSize().Height + 2)
					{
						nextValue = DataGrid.GetImageSize().Height + 2;
					}
					foreach (GridColumnSet item in _columnSets)
					{
						for (int j = 0; j < item.ColumnCount; j++)
						{
							GridColumn gridColumn = item[i, j];
							if (gridColumn == null)
							{
								continue;
							}
							if (gridColumn.HeaderStyle != null)
							{
								GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridColumn.HeaderStyle);
								int previousValue = gridFormatStyle.ResolveFont(font).Height * _headerLines;
								if (previousValue > nextValue)
								{
									nextValue = previousValue;
								}
								if (gridFormatStyle != formatStyle)
								{
									gridFormatStyle.DisposeResources();
									gridFormatStyle = null;
								}
							}
							j += gridColumn._colSpan - 1;
						}
					}
					array[i] = nextValue + 4;
					mordexRowLayoutState._rowHeight += array[i];
				}
			}
			if (AreColumnSetHeadersVisible())
			{
				otherValue = formatStyle.ResolveFont(font).Height * _columnSetHeaderLines;
				if (otherValue < DataGrid.GetImageSize().Height + 2)
				{
					otherValue = DataGrid.GetImageSize().Height + 2;
				}
				foreach (GridColumnSet columnSet in _columnSets)
				{
					if (columnSet.HeaderStyle != null)
					{
						GridFormatStyle applyOverrides = formatStyle.ApplyOverrides(columnSet.HeaderStyle);
						int firstValue = applyOverrides.ResolveFont(font).Height * _columnSetHeaderLines;
						if (firstValue > otherValue)
						{
							otherValue = firstValue;
						}
						if (applyOverrides != formatStyle)
						{
							applyOverrides.DisposeResources();
							applyOverrides = null;
						}
					}
				}
				otherValue += 4;
				mordexRowLayoutState._rowHeight += otherValue;
			}
			if ((otherMordexRow == null || mordexRow == null) && IsRootTable())
			{
				mordexRowLayoutState._top = tableHeaderHeight;
				indentWidth = CalculateIndentWidth(true);
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					if (IsTableHeaderVisible())
					{
						gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
					}
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					gridGroupLayoutState._isHorizontallyScrolled = value == 0;
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			else
			{
				mordexRowLayoutState._rowHeight += tableHeaderHeight;
				indentWidth = BuildIndentLayout(mordexRowLayoutState, mordexRow, false, true);
				foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
				{
					arrayListItem._isHorizontallyScrolled = value == 0;
				}
				nextItemX = mordexRowLayoutState._nextItemX;
				mordexRowLayoutState._top += tableHeaderHeight;
				mordexRowLayoutState._rowHeight -= tableHeaderHeight;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					if (IsTableHeaderVisible())
					{
						groupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
					}
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
					groupLayoutState._isHorizontallyScrolled = value == 0;
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			int right = 0;
			int mordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
			int otherMordexRowLayoutStateInteger = mordexRowLayoutStateInteger;
			int leftMargin = indentWidth;
			GridColumnSet[] columnSets = new GridColumnSet[_columnSets.Count];
			int secondValue = -1;
			int thirdValue = -1;
			for (int k = 0; k < _columnSets.Count; k++)
			{
				GridColumnSet otherColumnSet = _columnSets[k];
				int position = otherColumnSet._position;
				columnSets[position] = otherColumnSet;
			}
			for (int n = 0; n < columnSets.Length; n++)
			{
				if (columnSets[n].Visible)
				{
					if (secondValue == -1)
					{
						secondValue = n;
					}
					thirdValue = n;
				}
			}
			bool otherCondition = !IsTableHeaderVisible();
			if (AreColumnSetHeadersVisible())
			{
				mordexRowLayoutState._rowHeight = otherValue;
				otherMordexRowLayoutStateInteger = mordexRowLayoutStateInteger + leftMargin;
				mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
				for (int m = 0; m < _columnSets.Count; m++)
				{
					GridColumnSet nextColumnSet = columnSets[m];
					if (!nextColumnSet.Visible)
					{
						continue;
					}
					int otherWidth = nextColumnSet.Width + leftMargin;
					Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, mordexRowLayoutState._rowHeight);
					GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnSetHeader, mordexRowLayoutState, null, rectangle);
					if (m == secondValue)
					{
						if (AreRowHeadersVisible())
						{
							otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorLeft;
						}
					}
					else
					{
						otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorLeft;
					}
					if (otherCondition)
					{
						otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
					}
					if (m == thirdValue)
					{
						otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Right;
					}
					else
					{
						otherGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorRight;
					}
					mordexRowLayoutState._nextItemX += otherWidth;
					if (leftMargin != 0)
					{
						otherGroupLayoutState._leftMargin = leftMargin;
						leftMargin = 0;
					}
					otherGroupLayoutState._columnSet = nextColumnSet;
					otherGroupLayoutState._isHorizontallyScrolled = nextColumnSet.Position >= value;
					mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
				}
				mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				otherCondition = false;
			}
			if (AreColumnHeadersVisible() || condition)
			{
				for (int n = 0; n < _columnSetRowCount; n++)
				{
					mordexRowLayoutState._rowHeight = array[n];
					leftMargin = indentWidth;
					otherMordexRowLayoutStateInteger = mordexRowLayoutStateInteger + leftMargin;
					mordexRowLayoutState._nextItemX = mordexRowLayoutStateInteger;
					for (int i = 0; i < _columnSets.Count; i++)
					{
						GridColumnSet previousColumnSet = columnSets[i];
						if (!previousColumnSet.Visible)
						{
							continue;
						}
						int columnCount = previousColumnSet.ColumnCount;
						for (int column = 0; column < columnCount; column++)
						{
							GridColumn otherColumn = previousColumnSet[n, column];
							if (otherColumn != null && otherColumn.Visible && otherColumn._columnSetRow == n && otherColumn._columnSetColumn == column)
							{
								int width = otherColumn.Width + leftMargin;
								int height = 0;
								if (otherColumn._rowSpan > 1)
								{
									for (int columnSetRow = otherColumn._columnSetRow; columnSetRow < otherColumn._columnSetRow + otherColumn._rowSpan; columnSetRow++)
									{
										height += array[columnSetRow];
									}
								}
								else
								{
									height = mordexRowLayoutState._rowHeight;
								}
								Rectangle otherRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, height);
								GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnHeader, mordexRowLayoutState, otherColumn, otherRectangle);
								if (i != secondValue || column != 0)
								{
									nextGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.ConnectorLeft;
								}
								if (otherCondition && n == 0 && (AreColumnSetHeadersVisible() || IsTableHeaderVisible()))
								{
									nextGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Top;
								}
								else
								{
									nextGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
								}
								if (i == thirdValue && column + otherColumn._colSpan == columnCount)
								{
									nextGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Right;
								}
								else
								{
									nextGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorRight;
								}
								nextGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Bottom;
								if (otherColumn.ActAsSelector && otherColumn.UseHeaderSelector)
								{
									nextGroupLayoutState.SetMordexRow(mordexRow);
								}
								nextGroupLayoutState._columnSet = previousColumnSet;
								mordexRowLayoutState._nextItemX += nextGroupLayoutState._rect.Width;
								if (leftMargin != 0)
								{
									nextGroupLayoutState._leftMargin = leftMargin;
									leftMargin = 0;
								}
								if (right < otherRectangle.Right)
								{
									right = otherRectangle.Right;
								}
								mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
								nextGroupLayoutState._isHorizontallyScrolled = previousColumnSet.Position >= value;
							}
							else if (otherColumn == null || !otherColumn.Visible)
							{
								int nextWidth = previousColumnSet.GetColumnWidth(column) + leftMargin;
								int otherHeight = mordexRowLayoutState._rowHeight;
								Rectangle nextRectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, nextWidth, otherHeight);
								GridGroupLayoutState previousGroupLayoutState = new GridGroupLayoutState(LayoutItemType.EmptyHeader, mordexRowLayoutState, null, nextRectangle);
								previousGroupLayoutState._leftMargin = column;
								previousGroupLayoutState._topMargin = n;
								previousGroupLayoutState._columnSet = previousColumnSet;
								mordexRowLayoutState._nextItemX += previousGroupLayoutState._rect.Width;
								if (leftMargin != 0)
								{
									leftMargin = 0;
								}
								if (right < nextRectangle.Right)
								{
									right = nextRectangle.Right;
								}
								mordexRowLayoutState._arrayList.Add(previousGroupLayoutState);
								previousGroupLayoutState._isHorizontallyScrolled = previousColumnSet.Position >= value;
							}
							else
							{
								if (otherColumn._columnSetRow != n)
								{
									mordexRowLayoutState._nextItemX += previousColumnSet.GetColumnWidth(column) + leftMargin;
								}
								leftMargin = 0;
							}
						}
						mordexRowLayoutState._nextItemX = otherMordexRowLayoutStateInteger + previousColumnSet.Width;
						otherMordexRowLayoutStateInteger = mordexRowLayoutState._nextItemX;
					}
					mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
				}
			}
			if (IsTableHeaderVisible())
			{
				Rectangle previousRectangle = new Rectangle(nextItemX, 0, CalculateHierarchyWidth(true), tableHeaderHeight);
				GridGroupLayoutState firstGroupLayoutState = new GridGroupLayoutState(LayoutItemType.TableHeader, mordexRowLayoutState, null, previousRectangle);
				mordexRowLayoutState._arrayList.Add(firstGroupLayoutState);
				firstGroupLayoutState._isHorizontallyScrolled = value == 0;
			}
			mordexRowLayoutState._rightEdge = 0;
			mordexRowLayoutState._nextItemX = nextItemX;
			mordexRowLayoutState._rowHeight = mordexRowLayoutState._top;
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}

		internal MxRowLayoutState BuildNewRowLayout(Graphics graphics, GridRow gridRow, MordexRow mordexRow, GridFormatStyle formatStyle, int value)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return BuildColumnSetNewRowLayout(graphics, gridRow, mordexRow, formatStyle, value);
			}
			int indentWidth = 0;
			int nextItemX = 0;
			Font font = DataGrid.Font;
			int height = 0;
			height = DataGrid.GetImageSize().Height;
			MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.NewRow, this);
			if (_rowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridRow.RowStyle);
				Font otherFont = gridFormatStyle.ResolveFont(font);
				int otherValue = GridHelper.RoundToInt(otherFont.GetHeight(graphics)) + 2;
				mordexRowLayoutState._rowHeight = 0;
				foreach (GridColumn column in Columns)
				{
					if (!column.Visible)
					{
						continue;
					}
					GridCell gridCell = gridRow.Cells[column.Index];
					int nextValue = otherValue;
					if (gridCell.CellDisplayType == ColumnType.BoundImage)
					{
						if (gridCell.ImageSize.IsEmpty)
						{
							if (column.BoundImageSize != Size.Empty)
							{
								nextValue = column.BoundImageSize.Height + column.TopMargin + column.BottomMargin;
							}
							else if (gridCell.Value is Image image)
							{
								nextValue = image.Size.Height + column.TopMargin + column.BottomMargin;
							}
						}
						else
						{
							nextValue = gridCell.ImageSize.Height + column.TopMargin + column.BottomMargin;
						}
					}
					else if (gridCell.CellDisplayType == ColumnType.Link || gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
					{
						GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(column.CellStyle);
						GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridCell.FormatStyle);
						Font nextFont = otherApplyOverrides.ResolveFont(font);
						if (nextFont != otherFont || column.WordWrap)
						{
							TextLayoutRenderer textLayoutRenderer = column.ComputeTextLayoutRenderer(otherApplyOverrides, !DataGrid.UseCompatibleTextRendering);
							nextValue = GridHelper.CalculateTextHeight(graphics, nextFont, gridCell.Text, textLayoutRenderer, column._width - column.LeftMargin - column.RightMargin - 2, column.MinLines, column.MaxLines);
							textLayoutRenderer.ReleaseStringFormat();
						}
						if (gridCell.CellDisplayType == ColumnType.ImageAndText && nextValue < height)
						{
							nextValue = height;
						}
						nextValue += column.TopMargin + column.BottomMargin;
						if (otherApplyOverrides != applyOverrides)
						{
							otherApplyOverrides.DisposeResources();
						}
						if (applyOverrides != gridFormatStyle)
						{
							applyOverrides.DisposeResources();
						}
					}
					else if (gridCell.CellDisplayType == ColumnType.ImageAndText)
					{
						nextValue = height + column.TopMargin + column.BottomMargin;
					}
					else if (gridCell.CellDisplayType == ColumnType.CheckBox)
					{
						nextValue = 14 + column.TopMargin + column.BottomMargin;
					}
					if (mordexRowLayoutState._rowHeight < nextValue)
					{
						mordexRowLayoutState._rowHeight = nextValue;
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
			if (mordexRow == null && IsRootTable())
			{
				indentWidth = CalculateIndentWidth(true);
				if (value != 0)
				{
					foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
					{
						item._isHorizontallyScrolled = false;
					}
				}
				nextItemX = mordexRowLayoutState._nextItemX;
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
					gridGroupLayoutState._isHorizontallyScrolled = value == 0;
					mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
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
				if (value != 0)
				{
					foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
					{
						arrayListItem._isHorizontallyScrolled = false;
					}
				}
				nextItemX = mordexRowLayoutState._nextItemX;
				if (ComputeNewRowPosition() == NewRowPosition.TopRow)
				{
					mordexRowLayoutState._rowHeight -= 4;
				}
				if (AreRowHeadersVisible())
				{
					indentWidth += RowHeaderWidth;
					GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.RowHeader, mordexRowLayoutState, null, new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, indentWidth, mordexRowLayoutState._rowHeight));
					groupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.ConnectorTop;
					groupLayoutState._isHorizontallyScrolled = value == 0;
					mordexRowLayoutState._arrayList.Add(groupLayoutState);
					mordexRowLayoutState._nextItemX += indentWidth;
					indentWidth = 0;
				}
			}
			GridGroupLayoutState[] array = new GridGroupLayoutState[Columns.Count];
			for (int i = 0; i < Columns.Count; i++)
			{
				GridColumn otherColumn = Columns[i];
				int field_ = otherColumn._position;
				array[field_] = new GridGroupLayoutState(LayoutItemType.Cell, mordexRowLayoutState, otherColumn);
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j]._column.Visible)
				{
					int width = array[j]._column.Width + indentWidth;
					GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(bounds: new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, width, mordexRowLayoutState._rowHeight), type: LayoutItemType.Cell, owner: mordexRowLayoutState, column: array[j]._column);
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Horizontal)
					{
						otherGroupLayoutState._borderFlags = BorderFlags.Bottom;
					}
					if (GetEffectiveGridLines() == GridLines.Both || GetEffectiveGridLines() == GridLines.Vertical)
					{
						otherGroupLayoutState._borderFlags |= BorderFlags.Right;
					}
					if (otherGroupLayoutState._column._position < value)
					{
						otherGroupLayoutState._isHorizontallyScrolled = false;
					}
					otherGroupLayoutState.CopyColumnMargins(array[j]._column);
					mordexRowLayoutState._nextItemX += otherGroupLayoutState._rect.Width;
					if (indentWidth != 0)
					{
						otherGroupLayoutState._leftMargin += indentWidth;
						indentWidth = 0;
					}
					mordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
				}
			}
			mordexRowLayoutState._top += mordexRowLayoutState._rowHeight;
			if (ComputeNewRowPosition() == NewRowPosition.TopRow)
			{
				int otherWidth = mordexRowLayoutState._nextItemX - nextItemX;
				mordexRowLayoutState._rowHeight = 4;
				mordexRowLayoutState._nextItemX = nextItemX;
				Rectangle rectangle = new Rectangle(mordexRowLayoutState._nextItemX, mordexRowLayoutState._top, otherWidth, mordexRowLayoutState._rowHeight);
				GridGroupLayoutState nextGroupLayoutState = new GridGroupLayoutState(LayoutItemType.NewRowSeparator, mordexRowLayoutState, null, rectangle);
				mordexRowLayoutState._nextItemX += nextGroupLayoutState._rect.Width;
				mordexRowLayoutState._arrayList.Add(nextGroupLayoutState);
			}
			mordexRowLayoutState._rowHeight = (mordexRowLayoutState._top += mordexRowLayoutState._rowHeight);
			mordexRowLayoutState.SetLocation(new Point(0, 0));
			return mordexRowLayoutState;
		}
	}
}
