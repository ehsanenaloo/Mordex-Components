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
using System.Runtime.CompilerServices;
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
		private void UpdateSelectionVisibility()
		{
			if (base.ContainsFocus)
			{
				_isSelectionVisible = true;
			}
			else
			{
				_isSelectionVisible = _hideSelection != HideSelection.HideSelection;
			}
		}

		private void UpdateCellButtonVisibility(GridGroupLayoutState gridGroupLayoutState)
		{
			if (gridGroupLayoutState._children == null)
			{
				return;
			}
			GridGroupLayoutState[] gridGroupLayoutStateArray = gridGroupLayoutState._children;
			foreach (GridGroupLayoutState gridGroupLayoutStateArrayItem in gridGroupLayoutStateArray)
			{
				if (gridGroupLayoutState._type == LayoutItemType.RowSelector)
				{
					continue;
				}
				if (gridGroupLayoutState._type == LayoutItemType.FilterRowCell)
				{
					if (gridGroupLayoutStateArrayItem._type == LayoutItemType.FilterRowDropDown)
					{
						if (gridGroupLayoutState._gridGroupLayoutStateOwner != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null)
						{
							bool isCellButtonVisible = IsCellButtonVisible(gridGroupLayoutStateArrayItem, CellButtonDisplayMode.CurrentCell);
							if (!isCellButtonVisible)
							{
								GridCell gridCell = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Cells[gridGroupLayoutState._column];
								if (gridCell.Value != null)
								{
									isCellButtonVisible = true;
								}
								else if (gridCell.Column.FilterRowComparison == ConditionOperator.IsEmpty || gridCell.Column.FilterRowComparison == ConditionOperator.NotIsEmpty)
								{
									isCellButtonVisible = true;
								}
							}
							gridGroupLayoutStateArrayItem._isVisible = isCellButtonVisible;
						}
						else
						{
							gridGroupLayoutStateArrayItem._isVisible = false;
						}
					}
					else
					{
						gridGroupLayoutStateArrayItem._isVisible = IsCellButtonVisible(gridGroupLayoutStateArrayItem, CellButtonDisplayMode.CurrentCell);
					}
				}
				else if (gridGroupLayoutStateArrayItem._type == LayoutItemType.CellColumnButton)
				{
					gridGroupLayoutStateArrayItem._isVisible = IsCellButtonVisible(gridGroupLayoutStateArrayItem, gridGroupLayoutStateArrayItem._column.ButtonDisplayMode);
				}
				else if (gridGroupLayoutStateArrayItem.IsButtonItem())
				{
					gridGroupLayoutStateArrayItem._isVisible = IsCellButtonVisible(gridGroupLayoutStateArrayItem, gridGroupLayoutStateArrayItem._column.EditButtonDisplayMode);
				}
			}
		}

		private Rectangle ExpandRectForGridLines(GridGroupLayoutState gridGroupLayoutState, Rectangle rectangle)
		{
			if (gridGroupLayoutState.HasTopBorder())
			{
				rectangle.Y--;
				rectangle.Height++;
			}
			if (gridGroupLayoutState.HasLeftBorder())
			{
				rectangle.X--;
				rectangle.Width++;
			}
			return rectangle;
		}

		private GridFormatStyle ResolveGroupRowFormatStyle(GridGroupLayoutState gridGroupLayoutState, bool condition)
		{
			if (_gridControlStyle.GetLayoutState().GetGroupRowsStyle()._isThemed)
			{
				return GetGroupRowStyle(gridGroupLayoutState._gridGroup.Table);
			}
			GridRow gridRow = GetGridRowForLogicalRow(gridGroupLayoutState.GetMordexRow());
			GridFormatStyle groupRowStyle = GetGroupRowStyle(gridGroupLayoutState._gridGroup.Table);
			GridFormatStyle gridFormatStyle = groupRowStyle.ApplyOverrides(gridGroupLayoutState._gridGroup.GetGroupRowStyle());
			GridFormatStyle applyOverrides;
			if (gridRow != null)
			{
				GridFormatStyle otherApplyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
				applyOverrides = otherApplyOverrides.ApplyOverrides(gridRow.GetCachedFormatStyle());
			}
			else
			{
				GridFormatStyle otherGridFormatStyle = gridFormatStyle;
				applyOverrides = gridFormatStyle;
			}
			if (condition && gridGroupLayoutState._type == LayoutItemType.GroupExpandBox && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Selected)
			{
				return applyOverrides.ApplyOverrides(GetSelectionFormatStyle());
			}
			return applyOverrides;
		}

		internal void RestoreCurrentRowAfterResume()
		{
			_rowCache.ReleaseAllRows();
			if (_isNoRowSelected)
			{
				Row = -3;
				return;
			}
			_row = null;
			if (_mordexRow != null)
			{
				if (_mordexRow.DataSourceManager == _gridDataSourceManager && _mordexRow.IsRowVisible())
				{
					GridRow gridRow = GetGridRowForLogicalRow(_mordexRow);
					gridRow?.GetLogicalRow().EnsureVisible();
					if (gridRow.GetLogicalRow().IsRowVisible())
					{
						MoveTo(gridRow);
					}
					else if (RowCount > 0)
					{
						Row = 0;
					}
					return;
				}
			}
			else if (_currentRowData != null)
			{
				GridRow row = GetRow(_currentRowData);
				bool selected = false;
				if (row != null && row.GetLogicalRow().IsRowVisible())
				{
					selected = row.Selected;
					List<MordexRow> list = new List<MordexRow>();
					foreach (GridSelectedItem item in (IEnumerable)SelectedItems)
					{
						if (item._row != row.GetLogicalRow() && item._row.RowManagerPosition >= 0 && item._row.DataSourceManager == GetRowSourceManager())
						{
							list.Add(item._row);
						}
					}
					if (!selected)
					{
						selected = SelectedItems.Count > 0 && list.Count == 0;
					}
					_isEditCancelling = true;
					try
					{
						_isSelectionUpdating = true;
						_row = null;
						_gridSelectedItemCollection.Clear();
						_isSelectionUpdating = false;
						_isSelectionNotifyEnabled = false;
						_isCurrentCellChanging = !_isCurrentCellVisible;
						CanActivateCellAt(row.Position, _currentColumn, false);
						_isCurrentCellChanging = false;
						if (SelectionMode != SelectionMode.SingleSelection)
						{
							if (selected)
							{
								SyncCellSelectionWithCurrent();
							}
							_isSelectionUpdating = true;
							foreach (MordexRow listItem in list)
							{
								if (listItem.Visible && listItem.RowManagerPosition >= 0)
								{
									SelectedItems.Add(listItem.RowManagerPosition);
								}
							}
							_isSelectionUpdating = false;
						}
						else
						{
							SyncCellSelectionWithCurrent();
						}
					}
					finally
					{
						_isCurrentCellChanging = false;
						_isSelectionUpdating = false;
						_isEditCancelling = false;
						_isSelectionNotifyEnabled = true;
					}
					return;
				}
				if (RowCount > 0)
				{
					Row = 0;
					return;
				}
			}
			if (_lastRecordPosition == 0 && _currentRowData != null)
			{
				GridRow visibleRowForDataRow = FindVisibleRowForDataRow(_currentRowData);
				if (visibleRowForDataRow != null)
				{
					MoveTo(visibleRowForDataRow);
					EnsureVisible();
					return;
				}
			}
			if (_lastRecordPosition == -1)
			{
				if (RootTable != null && RootTable.IsAddNewAllowed())
				{
					int rowManagerPosition = -1;
					if (RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow)
					{
						rowManagerPosition = -1;
					}
					else
					{
						if (_gridDataSourceManager.GetRootNewRow() != null)
						{
							rowManagerPosition = _gridDataSourceManager.GetRootNewRow().RowManagerPosition;
						}
						if (rowManagerPosition == -1)
						{
							return;
						}
					}
					bool autoEdit = _autoEdit;
					_autoEdit = false;
					if (IsRowFocusableInCurrentColumn(rowManagerPosition))
					{
						SyncCellSelectionWithCurrent();
					}
					_autoEdit = autoEdit;
				}
			}
			else
			{
				SetCurrentRowAndSelect(_lastRecordPosition);
			}
			RefreshRowCountAndInvalidate();
			EnsureVisible();
		}

		private GridFormatStyle ResolveRowFormatStyle(GridRow gridRow)
		{
			GridFormatStyle gridFormatStyle = null;
			switch (gridRow.RowType)
			{
			case RowType.FilterRow:
				gridFormatStyle = GetFilterRowStyle(gridRow.Table);
				break;
			case RowType.GroupFooter:
				gridFormatStyle = GetGroupTotalRowStyle(gridRow.Table);
				if (gridRow.Group != null && gridRow.Group.Column != null)
				{
					gridFormatStyle = gridFormatStyle.ApplyOverrides(gridRow.Group.Column.GroupRowStyle);
				}
				break;
			case RowType.Record:
				gridFormatStyle = GetAlternatingRowStyle(gridRow.Table, gridRow.GetPosition());
				break;
			case RowType.NewRecord:
				if (gridRow.Table.ComputeNewRowPosition() == NewRowPosition.TopRow)
				{
					gridFormatStyle = GetNewRowStyle(gridRow.Table);
					break;
				}
				gridFormatStyle = GetAlternatingRowStyle(gridRow.Table, gridRow.GetPosition());
				gridFormatStyle = gridFormatStyle.ApplyOverrides(_gridGridFormatStyle);
				if (gridRow.Table != null && gridRow.Table.NewRowFormatStyle != null)
				{
					gridFormatStyle = gridFormatStyle.ApplyOverrides(gridRow.Table.NewRowFormatStyle);
				}
				break;
			case RowType.GroupHeader:
				gridFormatStyle = GetGroupRowStyle(gridRow.Table);
				if (gridRow.Group != null && gridRow.Group.Column != null)
				{
					gridFormatStyle = gridFormatStyle.ApplyOverrides(gridRow.Group.Column.GroupRowStyle);
				}
				break;
			case RowType.TotalRow:
				gridFormatStyle = GetTotalRowStyle(gridRow.Table);
				break;
			}
			gridFormatStyle = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
			return gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
		}

		public GridFormatStyle GetRowFormatStyle(GridRow row)
		{
			GridFormatStyle gridFormatStyle = ResolveRowFormatStyle(row);
			CopyFontToFormatStyle(gridFormatStyle, gridFormatStyle.ResolveFont(Font));
			gridFormatStyle.SetReadOnly(true);
			return gridFormatStyle;
		}

		public GridFormatStyle GetCellFormatStyle(GridCell cell)
		{
			GridFormatStyle gridFormatStyle = ResolveRowFormatStyle(cell.Row);
			if (cell.Row.RowType != RowType.GroupFooter)
			{
				gridFormatStyle = gridFormatStyle.ApplyOverrides(cell.Column.CellStyle);
			}
			if (cell.Row.RowType == RowType.Record)
			{
				gridFormatStyle = gridFormatStyle.ApplyOverrides(cell.GetEffectiveFormatStyle());
			}
			CopyFontToFormatStyle(gridFormatStyle, gridFormatStyle.ResolveFont(Font));
			gridFormatStyle.SetReadOnly(true);
			return gridFormatStyle;
		}

		internal void HandleRowChange(MordexRow row, RowChangeType change)
		{
			OnDataSourceManagerRowChange(row, change);
			if (_tableAccessibleObject != null)
			{
				switch (change)
				{
				case RowChangeType.Delete:
				case RowChangeType.Add:
				case RowChangeType.Move:
				case RowChangeType.ChildRowReset:
				case RowChangeType.VisibleChanged:
				case RowChangeType.ExpandedChanged:
					_tableAccessibleObject.NotifyClients(AccessibleEvents.Reorder);
					break;
				}
			}
			if (!_isBuildingBoundData && (change == RowChangeType.ChildRowReset || change == RowChangeType.Add) && _row == null)
			{
				MoveToCurrentPosition(false);
			}
		}

		protected virtual void OnDataSourceManagerRowChange(MordexRow row, RowChangeType change)
		{
		}

		private void ResetDataSourceRowState()
		{
			_gridDataSourceManager.ResetRowFormatStyles();
		}

		public void SelectAll()
		{
			switch (SelectionMode)
			{
			case SelectionMode.SingleSelection:
				throw new InvalidOperationException("SelectAll is unavailable for SelectionMode.SingleSelection");
			case SelectionMode.MultipleSelection:
				using (BeginSelectionChangeBatch())
				{
					if (RowCount > 0 && IsRowFocusableInCurrentColumn(0))
					{
						for (int k = 0; k < RowCount; k++)
						{
							_gridSelectedItemCollection.Add(k);
						}
					}
				}
				break;
			case SelectionMode.MultipleSelectionSameTable:
				using (BeginSelectionChangeBatch())
				{
					if (RowCount <= 0)
					{
						break;
					}
					MordexTableBase rootTable = RootTable;
					if (!IsRowFocusableInCurrentColumn(0))
					{
						break;
					}
					_gridSelectedItemCollection.Clear();
					_isSelectingAll = true;
					for (int j = 0; j < RowCount; j++)
					{
						if (GetRowManager()[j].Table == rootTable)
						{
							_gridSelectedItemCollection.Add(j);
						}
					}
					_isSelectingAll = false;
				}
				break;
			case SelectionMode.MultipleCellSelection:
				using (BeginSelectionChangeBatch())
				{
					if (RowCount <= 0)
					{
						break;
					}
					MordexRowManager mordexRowManager = GetRowManager();
					if (IsRowFocusableInCurrentColumn(0))
					{
						for (int i = 0; i < RowCount; i++)
						{
							_gridSelectedItemCollection.Add(i);
							RowStateHelper.SelectAllRowCells(mordexRowManager[i]);
						}
					}
				}
				break;
			}
			Invalidate();
		}

		public void ClearCellSelection()
		{
			_gridSelectedItemCollection.Clear();
		}
	}
}
