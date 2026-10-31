using System;
using System.Collections;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class RowStateHelper
	{
		internal static RowTag GetRowTag(MordexRow mordexRow)
		{
			if (!(mordexRow.Tag is RowTag))
			{
				mordexRow.Tag = new RowTag();
			}
			return (RowTag)mordexRow.Tag;
		}

		internal static CellSelectionStatus ComputeCellSelectionStatus(MordexRow mordexRow)
		{
			if (mordexRow == null)
			{
				return CellSelectionStatus.NoCellIsSelected;
			}
			RowTag rowTag = GetRowTag(mordexRow);
			if (rowTag.GetCellSelectionState() != null)
			{
				return rowTag.GetCellSelectionState().GetCellSelectionStatus();
			}
			return CellSelectionStatus.NoCellIsSelected;
		}

		internal static bool IsCellSelected(GridCell gridCell)
		{
			GridRow row = gridCell.Row;
			if (row == null)
			{
				return false;
			}
			MxDataGrid dataGrid = row.DataGrid;
			if (dataGrid == null)
			{
				return false;
			}
			if (dataGrid.SelectionMode == SelectionMode.MultipleCellSelection)
			{
				RowTag rowTag = GetRowTag(gridCell.Row.GetLogicalRow());
				if (rowTag.GetCellSelectionState() != null)
				{
					return rowTag.GetCellSelectionState().IsColumnSelected(gridCell.Column);
				}
				return false;
			}
			if (dataGrid.CellSelectionMode == CellSelectionMode.SingleCell)
			{
				if (row.Selected && dataGrid.SelectionMode == SelectionMode.SingleSelection)
				{
					if (dataGrid.CurrentColumn == null)
					{
						return true;
					}
					return dataGrid.CurrentColumn == gridCell.Column;
				}
				return false;
			}
			return row.Selected;
		}

		internal static void SetCellSelected(GridCell gridCell, bool condition)
		{
			GridRow row = gridCell.Row;
			if (row != null)
			{
				RowTag rowTag = GetRowTag(gridCell.Row.GetLogicalRow());
				if (rowTag.GetCellSelectionState() == null)
				{
					rowTag.SetCellSelectionState(new RowCellSelection(row.GetLogicalRow()));
				}
				rowTag.GetCellSelectionState().SetColumnSelected(gridCell.Column, condition);
			}
		}

		internal static void SelectAllRowCells(MordexRow row)
		{
			RowTag rowTag = GetRowTag(row);
			if (rowTag.GetCellSelectionState() == null)
			{
				rowTag.SetCellSelectionState(new RowCellSelection(row));
			}
			rowTag.GetCellSelectionState().ResetToAllCellsSelected();
		}

		internal static void DeselectAllRowCells(MordexRow row)
		{
			RowTag rowTag = GetRowTag(row);
			if (rowTag.GetCellSelectionState() == null)
			{
				rowTag.SetCellSelectionState(new RowCellSelection(row));
			}
			rowTag.GetCellSelectionState().DeselectAllColumns();
		}

		internal static void SelectOnlyColumnCell(MordexRow row, GridColumn gridColumn)
		{
			RowTag rowTag = GetRowTag(row);
			if (rowTag.GetCellSelectionState() == null)
			{
				rowTag.SetCellSelectionState(new RowCellSelection(row));
			}
			rowTag.GetCellSelectionState().SelectOnlyColumn(gridColumn);
		}

		internal static GridGroup GetRowGroup(MordexRow mordexRow)
		{
			return (GridGroup)mordexRow.Group;
		}

		internal static bool IsContainerRow(MordexRow mordexRow)
		{
			if (mordexRow is MordexGroupRow)
			{
				return true;
			}
			if (mordexRow is MordexTableRow)
			{
				return true;
			}
			if (mordexRow is MordexDataRow)
			{
				GridTable gridTable = (GridTable)mordexRow.Table;
				if (gridTable != null)
				{
					return gridTable.IsHierarchical();
				}
			}
			return false;
		}

		internal static ArrayList ComputeArrayList(MordexRow otherMordexRow)
		{
			ArrayList arrayList = new ArrayList();
			for (MordexRow mordexRow = ((otherMordexRow.ParentRow is MordexGroupRow && !IsContainerRow(otherMordexRow)) ? otherMordexRow.ParentRow : otherMordexRow); mordexRow != null; mordexRow = mordexRow.ParentRow)
			{
				if (!mordexRow.IsVirtualRow())
				{
					arrayList.Add(mordexRow);
				}
			}
			arrayList.Reverse();
			return arrayList;
		}

		internal static int GetSelfReferencingDepth(MordexRow mordexRow)
		{
			int selfReferencingDepth = 0;
			GridTable gridTable = (GridTable)mordexRow.Table;
			if (gridTable != null && gridTable.GetBaseIsSelfReferencingTable())
			{
				for (MordexRow parentRow = mordexRow.ParentRow; parentRow != null; parentRow = parentRow.ParentRow)
				{
					if (parentRow is MordexDataRow && parentRow.Table == mordexRow.Table)
					{
						selfReferencingDepth++;
					}
				}
			}
			return selfReferencingDepth;
		}

		internal static bool HasSelfReferencingChildren(MordexRow mordexRow)
		{
			if (mordexRow is MordexDataRow)
			{
				MordexDataRow mordexDataRow = (MordexDataRow)mordexRow;
				GridTable gridTable = (GridTable)mordexDataRow.Table;
				if (gridTable.GetBaseIsSelfReferencingTable())
				{
					return mordexRow.Rows.VisibleCount > 0;
				}
			}
			return false;
		}

		internal static bool IsFirstSibling(MordexRow mordexRow)
		{
			if (mordexRow.ParentCollection != null)
			{
				return mordexRow.ParentCollection[0] == mordexRow;
			}
			return false;
		}

		internal static bool IsLastVisibleSibling(MordexRow otherMordexRow)
		{
			if (otherMordexRow.ParentRow != null)
			{
				if (otherMordexRow is MordexTotalRow)
				{
					return true;
				}
				MordexRow mordexRow = otherMordexRow.ParentRow.LastVisibleChild;
				if (mordexRow is MordexTotalRow)
				{
					mordexRow = mordexRow.PreviousSibling;
				}
				if (mordexRow == otherMordexRow)
				{
					return true;
				}
				while (mordexRow != null && !mordexRow.Visible)
				{
					mordexRow = mordexRow.PreviousSibling;
				}
				return mordexRow == otherMordexRow;
			}
			return false;
		}

		internal static MordexRow GetNextManagedRow(MordexRow mordexRow)
		{
			GridDataSourceManager gridDataSourceManager = (GridDataSourceManager)mordexRow.DataSourceManager;
			if (gridDataSourceManager != null)
			{
				int rowManagerPosition = mordexRow.RowManagerPosition;
				if (rowManagerPosition >= 0 && rowManagerPosition < gridDataSourceManager.RowManager.Count - 1)
				{
					return gridDataSourceManager.RowManager[rowManagerPosition + 1];
				}
			}
			return null;
		}

		internal static MordexRow GetNextVisibleSibling(MordexRow row)
		{
			if (row.ParentCollection != null)
			{
				int value = row.ParentCollection.IndexOf(row);
				if (value < row.ParentCollection.Count - 1)
				{
					for (int i = value + 1; i < row.ParentCollection.Count; i++)
					{
						MordexRow mordexRow = row.ParentCollection[i];
						if (mordexRow.Visible)
						{
							return mordexRow;
						}
					}
				}
			}
			return null;
		}

		internal static MordexDataRow FindRootDataRow(MordexRow parentRow)
		{
			if (parentRow.Table == null)
			{
				return null;
			}
			GridDataSourceManager gridDataSourceManager = (GridDataSourceManager)parentRow.DataSourceManager;
			if (parentRow is MordexTotalRow && parentRow.ParentRow is MordexGroupRow)
			{
				parentRow = parentRow.ParentRow;
			}
			for (MordexRow mordexRow = parentRow; mordexRow != null; mordexRow = mordexRow.ParentRow)
			{
				if (mordexRow is MordexDataRow && mordexRow.Table == gridDataSourceManager.RootTable)
				{
					return (MordexDataRow)mordexRow;
				}
			}
			if (parentRow is MordexGroupRow && ((GridTable)parentRow.Table).IsRootTable())
			{
				MordexRow mordexRow = parentRow;
				while (mordexRow != null)
				{
					mordexRow = ((mordexRow.Rows.Count <= 0) ? null : mordexRow.Rows[0]);
					if (mordexRow is MordexDataRow && mordexRow.Table == gridDataSourceManager.RootTable)
					{
						return (MordexDataRow)mordexRow;
					}
				}
			}
			return null;
		}

		internal static MordexTableRow FindParentTableRow(MordexRow mordexRow)
		{
			while (mordexRow != null)
			{
				if (mordexRow is MordexTableRow)
				{
					return (MordexTableRow)mordexRow;
				}
				mordexRow = mordexRow.ParentRow;
			}
			return null;
		}

		internal static RowType ComputeRowType(MordexRow mordexRow)
		{
			if (mordexRow is MordexGroupRow)
			{
				return RowType.GroupHeader;
			}
			if (mordexRow is MordexDataRow || mordexRow is DataHolderRow)
			{
				return RowType.Record;
			}
			if (mordexRow is MordexNewDataRow)
			{
				return RowType.NewRecord;
			}
			if (mordexRow is MordexTotalRow)
			{
				if (mordexRow.ParentRow is MordexGroupRow)
				{
					return RowType.GroupFooter;
				}
				return RowType.TotalRow;
			}
			if (mordexRow is VirtualDataRow)
			{
				return RowType.FilterRow;
			}
			if (mordexRow is DataMordexRow)
			{
				return RowType.Record;
			}
			throw new InvalidOperationException();
		}

		internal static bool CanExpandRow(MxDataGrid dataGrid, MordexRow mordexRow)
		{
			if (mordexRow is MordexNewDataRow)
			{
				return false;
			}
			if (mordexRow is MordexDataRow && !dataGrid.Hierarchical)
			{
				return false;
			}
			return RowHasChildren(mordexRow);
		}

		internal static bool RowHasChildren(MordexRow mordexRow)
		{
			if (mordexRow.HasChildren())
			{
				return true;
			}
			return false;
		}
	}
}
