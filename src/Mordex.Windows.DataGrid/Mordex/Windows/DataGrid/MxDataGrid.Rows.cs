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
		private void AdjustRowAfterCountChange()
		{
			if (RowCount == _rowCount || GetRowManager() == null || GetRowManager().IsIndexSuspend)
			{
				return;
			}
			_rowCount = RowCount;
			if (_rowCount > 0 && _firstRow < 0)
			{
				_firstRow = 0;
			}
			if (_rowCount == 0 && Row != -2)
			{
				if (RootTable != null && RootTable.IsAddNewAllowed())
				{
					MoveToNewRecord();
				}
				else if (FilterMode != FilterMode.None)
				{
					Row = -2;
				}
				else
				{
					Row = -3;
				}
			}
			// The record count is kept by the data layer, so this costs the same for the first row and the last.
			if (_numericEditControl != null)
			{
				_numericEditControl.SetTotalCount(_gridDataSourceManager.GetRootRecordCount());
			}
			OnRowCountChanged(EventArgs.Empty);
			RefreshRowCountAndInvalidate();
		}

		internal void PrepareRowForDisplay(GridRow row, bool condition)
		{
			if (row == null || row.GetLogicalRow() == null || row.GetLogicalRow().ParentRow == null || (row._isFormatting && !condition))
			{
				return;
			}
			if (_showErrors && (row.RowType == RowType.Record || row.RowType == RowType.NewRecord))
			{
				if (row.DataRow != null)
				{
					row._hasTextOverride = false;
					HasRowError(row);
				}
				foreach (GridCell item in (IEnumerable)row.Cells)
				{
					if (item != null)
					{
						UpdateCellError(item);
					}
				}
			}
			if (row.RowType == RowType.Record)
			{
				if (row.DataRow != null)
				{
					foreach (GridFormatCondition formatCondition in row.Table.FormatConditions)
					{
						if (formatCondition.Enabled && formatCondition.MatchesRow(row.GetLogicalRow()))
						{
							if (formatCondition.TargetColumn == null)
							{
								row.MergeFormatStyle(formatCondition.FormatStyle, formatCondition.AllowMerge);
							}
							else
							{
								row.Cells[formatCondition.TargetColumn.Index].MergeFormatStyle(formatCondition.FormatStyle, formatCondition.AllowMerge);
							}
							row.MergeRowFormatStyle(formatCondition.PreviewRowFormatStyle, formatCondition.AllowMerge);
						}
					}
				}
			}
			else if (row.RowType == RowType.GroupHeader)
			{
				if (row.Table.UseGroupCondition && row.Table.ShowGroupConditionCount)
				{
					MordexRow[] dataRows = row.GetLogicalRow().GetDataRows(row.Table, row.Table.GroupCondition);
					if ((row._groupConditionCount = dataRows.Length) > 0)
					{
						row.MergeFormatStyle(row.Table.GroupCondition.FormatStyle, false);
					}
				}
				else if (row.Table.UseGroupCondition)
				{
					MordexRow[] otherDataRows = row.GetLogicalRow().GetDataRows(row.Table, row.Table.GroupCondition);
					int value = otherDataRows.Length;
					if (value > 0)
					{
						row.MergeFormatStyle(row.Table.GroupCondition.FormatStyle, false);
					}
				}
				else
				{
					row._groupConditionCount = 0;
				}
			}
			OnFormattingRow(new RowLoadEventArgs(row));
			row._isFormatting = true;
		}

		internal void MarkDescendantRowsDirty(MordexRow row)
		{
			if (row == null)
			{
				return;
			}
			GridRow gridRow = null;
			gridRow = _rowCache.FindGridRow(row);
			bool condition = false;
			foreach (GridRow item in _rowCache)
			{
				if (item.GetLogicalRow() != null && item.GetLogicalRow().IsDescendentOf(row))
				{
					condition = true;
					item._isLayoutDirty = true;
				}
			}
			if (gridRow != null)
			{
				condition = true;
				gridRow._isLayoutDirty = true;
			}
			if (condition)
			{
				_rowCache.ReleaseStaleRows();
				RefreshRowCountAndInvalidate();
			}
			bool otherCondition = false;
			if (row != null)
			{
				RowStateHelper.GetRowTag(row).GetDataState3Boolean();
			}
			if (_row != null && _row.GetLogicalRow() != null && (_row.GetLogicalRow() == row || (_row.RowType != RowType.FilterRow && _row.GetLogicalRow().DataSourceManager == null)))
			{
				EndCellEdit(false);
				otherCondition = true;
				SetCurrentGridRow(null, false);
				_currentColumn = null;
			}
			try
			{
				if (otherCondition)
				{
					if (!_isDataChanging)
					{
						if (row.RowManagerPosition < RowCount)
						{
							if (row is MordexDataRow && ((MordexDataRow)row).IsDataRowDeleted)
							{
								SetCurrentRowKeepSelection(row.RowManagerPosition);
							}
							else
							{
								_isSelectionUpdating = true;
								_isSelectionNotifyEnabled = false;
								SetCurrentRowAndSelect(row.RowManagerPosition);
								_isSelectionNotifyEnabled = true;
								_isSelectionUpdating = false;
							}
						}
						else
						{
							SetCurrentRowAndSelect(row.RowManagerPosition - 1);
						}
					}
				}
				else if (_row != null && !_gridDataSourceManager.RowManager.IsIndexSuspend)
				{
					SetCurrentRowKeepSelection(_row.Position);
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			RefreshRowCountAndInvalidate();
		}

		internal void MarkGroupRowsDirty(MordexRow mordexRow)
		{
			if (mordexRow == null)
			{
				return;
			}
			GridRow gridRow = null;
			gridRow = _rowCache.FindGridRow(mordexRow);
			bool condition = false;
			foreach (GridRow item in _rowCache)
			{
				if (item.RowType == RowType.GroupHeader || item.RowType == RowType.TotalRow)
				{
					condition = true;
					item._isLayoutDirty = true;
				}
				if (mordexRow != null && mordexRow.IsDescendentOf(item.GetLogicalRow()))
				{
					condition = true;
					item._isLayoutDirty = !item._isLoading;
				}
			}
			if (condition || gridRow != null)
			{
				_rowCache.ReleaseStaleRows();
				RefreshRowCountAndInvalidate();
			}
			if (_row != null && _row.GetLogicalRow() == mordexRow)
			{
				MarkRowDirty(mordexRow);
			}
			RefreshRowCountAndInvalidate();
		}

		internal void MarkRowDirty(MordexRow mordexRow)
		{
			if (_row != null && _row.GetLogicalRow() == mordexRow && _row.HasChangedCells())
			{
				_row.Format();
				return;
			}
			GridRow gridRow = _rowCache.FindGridRow(mordexRow);
			bool condition = false;
			bool otherCondition = false;
			if (_row != null && _row.GetLogicalRow() == mordexRow)
			{
				gridRow = _row;
			}
			if (gridRow != null)
			{
				gridRow._isLayoutDirty = true;
				condition = true;
				if (gridRow == _row)
				{
					otherCondition = true;
				}
			}
			if (condition)
			{
				_rowCache.ReleaseStaleRows();
				RefreshRowCountAndInvalidate();
			}
			if (!otherCondition)
			{
				return;
			}
			if (!_row._isEditing)
			{
				_hasStaleRowCells = true;
				return;
			}
			foreach (GridCell item in (IEnumerable)_row.Cells)
			{
				item._isTextCached = false;
			}
			PrepareRowForDisplay(_row, true);
		}

		internal GridFormatStyle GetRowHeaderStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = GetHeaderStyle(gridTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(_rowHeaderFormatStyle);
			if (gridTable != null && !gridTable.RowHeaderFormatStyle.IsDefaultStyle())
			{
				GridFormatStyle otherApplyOverrides = gridFormatStyle.ApplyOverrides(gridTable.RowHeaderFormatStyle);
				if (otherApplyOverrides != applyOverrides)
				{
					applyOverrides.DisposeResources();
				}
				applyOverrides = otherApplyOverrides;
			}
			if (applyOverrides != gridFormatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
			return applyOverrides;
		}

		internal GridFormatStyle GetRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = null;
			gridFormatStyle = _rowFormatStyle.CloneWithDefaults(_defaultAlphaLevel, GetBackColorAlphaMode(), GetForeColorAlphaMode(), _blendColor);
			if (gridTable != null && gridTable.RowFormatStyle != null)
			{
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridTable.RowFormatStyle);
				if (applyOverrides != gridFormatStyle)
				{
					gridFormatStyle.DisposeResources();
					gridFormatStyle = applyOverrides;
				}
			}
			return gridFormatStyle;
		}

		internal GridFormatStyle GetAlternatingRowStyle(GridTable gridTable, int value)
		{
			if (!_alternatingColors)
			{
				return GetRowStyle(gridTable);
			}
			GridFormatStyle gridFormatStyle = null;
			if (value % 2 == 0)
			{
				gridFormatStyle = GetRowStyle(gridTable);
			}
			else
			{
				GridFormatStyle rowStyle = GetRowStyle(gridTable);
				gridFormatStyle = rowStyle.ApplyOverrides(_alternatingRowFormatStyle);
				if (gridTable != null && gridTable.AlternatingRowFormatStyle != null)
				{
					GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridTable.AlternatingRowFormatStyle);
					if (applyOverrides != gridFormatStyle)
					{
						gridFormatStyle.DisposeResources();
						gridFormatStyle = applyOverrides;
					}
				}
				if (gridFormatStyle != rowStyle)
				{
					rowStyle.DisposeResources();
				}
			}
			return gridFormatStyle;
		}

		internal GridFormatStyle GetGroupRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = null;
			gridFormatStyle = _groupRowFormatStyle.CloneWithDefaults(_defaultAlphaLevel, GetBackColorAlphaMode(), GetForeColorAlphaMode(), _blendColor);
			if (gridTable != null && gridTable.GroupRowFormatStyle != null)
			{
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridTable.GroupRowFormatStyle);
				if (applyOverrides != gridFormatStyle)
				{
					gridFormatStyle.DisposeResources();
					gridFormatStyle = applyOverrides;
				}
			}
			return gridFormatStyle;
		}

		private GridFormatStyle GetNewRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = GetRowStyle(gridTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(_gridGridFormatStyle);
			if (gridTable != null && gridTable.NewRowFormatStyle != null)
			{
				GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridTable.NewRowFormatStyle);
				if (otherApplyOverrides != applyOverrides)
				{
					applyOverrides.DisposeResources();
				}
				applyOverrides = otherApplyOverrides;
			}
			gridFormatStyle.DisposeResources();
			return applyOverrides;
		}

		internal GridFormatStyle GetCardCaptionStyle()
		{
			return _cardCaptionFormatStyle.CloneWithDefaults(_defaultAlphaLevel, GetBackColorAlphaMode(), GetForeColorAlphaMode(), _blendColor);
		}

		internal GridFormatStyle GetCardColumnHeaderStyle(int value)
		{
			GridFormatStyle gridFormatStyle = GetAlternatingRowStyle(RootTable, value);
			GridFormatStyle result = gridFormatStyle.ApplyOverrides(_cardColumnHeaderFormatStyle);
			gridFormatStyle.DisposeResources();
			return result;
		}

		private static bool HasRowError(GridRow gridRow)
		{
			try
			{
				if (!gridRow._hasTextOverride)
				{
					if (gridRow.DataRow is IDataErrorInfo)
					{
						IDataErrorInfo dataErrorInfo = (IDataErrorInfo)gridRow.DataRow;
						gridRow.SetTextOverride(dataErrorInfo.Error ?? "");
					}
					else if (gridRow.DataRow is DataRow)
					{
						gridRow.SetTextOverride(((DataRow)gridRow.DataRow).RowError ?? "");
					}
				}
				return gridRow.GetTextOrEmpty().Length > 0;
			}
			catch (Exception ex)
			{
				gridRow.SetTextOverride(ex.Message);
				return true;
			}
		}

		internal void UpdateCellError(GridCell gridCell)
		{
			if (_showErrors)
			{
				GridRow row = gridCell.Row;
				if (row.DataRow is IDataErrorInfo)
				{
					IDataErrorInfo dataErrorInfo = (IDataErrorInfo)row.DataRow;
					ProcessIDataErrorInfo(dataErrorInfo, gridCell);
				}
				else if (row.DataRow is DataRow)
				{
					ProcessDataRow((DataRow)row.DataRow, gridCell);
				}
			}
		}

		private void ProcessDataRow(DataRow dataRow, GridCell gridCell)
		{
			if (gridCell == null || gridCell.Column.BoundMode != ColumnBoundMode.Bound || dataRow.Table == null || dataRow.Table.Columns[gridCell.Column.DataMember] == null)
			{
				return;
			}
			string columnError = dataRow.GetColumnError(gridCell.Column.DataMember);
			if (columnError.Length > 0)
			{
				gridCell.SetErrorText(columnError);
				gridCell.Image = GetErrorImage();
				gridCell.SetHasErrorIcon(gridCell.Image != null);
				return;
			}
			gridCell.SetErrorText("");
			if (gridCell.HasErrorIcon())
			{
				gridCell.Image = null;
				gridCell.SetHasErrorIcon(false);
			}
		}

		private static bool AreSameLogicalRow(GridRow gridRow, GridRow otherGridRow)
		{
			if (gridRow == null)
			{
				if (otherGridRow == null)
				{
					return true;
				}
				return false;
			}
			if (otherGridRow == null)
			{
				return false;
			}
			if (gridRow.GetLogicalRow() == otherGridRow.GetLogicalRow())
			{
				return true;
			}
			return false;
		}

		private void ToggleRowCheckState(MordexRow mordexRow)
		{
			if (mordexRow == null)
			{
				if (_gridDataSourceManager.RootRow != null)
				{
					RowCheckState rowCheckState = DataState.ComputeRowCheckState(_gridDataSourceManager.RootRow.CheckedState);
					RowCheckState checkState = RowCheckState.Checked;
					if (rowCheckState == RowCheckState.Checked)
					{
						checkState = RowCheckState.Unchecked;
					}
					if (RaiseRowCheckStateChanging(null, CheckStateChangeType.ColumnHeaderChange, rowCheckState, checkState))
					{
						_gridDataSourceManager.RootRow.IsChecked = !_gridDataSourceManager.RootRow.IsChecked;
						RefreshRowCountAndInvalidate();
						OnRowCheckStateChanged(new RowCheckStateChangeEventArgs(null, CheckStateChangeType.ColumnHeaderChange, rowCheckState, checkState));
					}
				}
			}
			else if (AcceptCellEdit())
			{
				MordexTableRow mordexTableRow = RowStateHelper.FindParentTableRow(mordexRow);
				RowCheckState otherRowCheckState = DataState.ComputeRowCheckState(mordexTableRow.CheckedState);
				RowCheckState otherCheckState = RowCheckState.Checked;
				if (otherRowCheckState == RowCheckState.Checked)
				{
					otherCheckState = RowCheckState.Unchecked;
				}
				if (RaiseRowCheckStateChanging(GetGridRowForLogicalRow(mordexTableRow.ParentRow), CheckStateChangeType.ColumnHeaderChange, otherRowCheckState, otherCheckState))
				{
					mordexTableRow.IsChecked = !mordexTableRow.IsChecked;
					RefreshRowCountAndInvalidate();
					OnRowCheckStateChanged(new RowCheckStateChangeEventArgs(GetGridRowForLogicalRow(mordexTableRow.ParentRow), CheckStateChangeType.ColumnHeaderChange, otherRowCheckState, otherCheckState));
				}
			}
		}

		private void SetLastSelectedColumn(GridColumn gridColumn)
		{
			if (_gridColumn != gridColumn)
			{
				_gridColumn = gridColumn;
			}
		}

		private void DeselectCellsOutsideRange(Rectangle rectangle, Rectangle bounds)
		{
			for (int i = rectangle.Y; i < rectangle.Bottom; i++)
			{
				GridRow row = GetRow(i);
				if (row.RowType == RowType.GroupHeader || row.Table != CurrentTable)
				{
					continue;
				}
				for (int j = rectangle.X; j < rectangle.Right; j++)
				{
					if (!bounds.Contains(j, i))
					{
						GridColumn columnInPosition = row.Table.Columns.GetColumnInPosition(j);
						if (columnInPosition != null)
						{
							SetCellSelected(row.Cells[columnInPosition], false);
						}
					}
				}
			}
		}

		private void SelectCellsInRange(Rectangle rectangle)
		{
			for (int i = rectangle.Y; i < rectangle.Bottom; i++)
			{
				GridRow row = GetRow(i);
				if (row.RowType == RowType.GroupHeader || row.Table != CurrentTable)
				{
					continue;
				}
				for (int j = rectangle.X; j < rectangle.Right; j++)
				{
					GridColumn columnInPosition = row.Table.Columns.GetColumnInPosition(j);
					if (columnInPosition != null && IsCellSelectable(row, columnInPosition))
					{
						SetCellSelected(row.Cells[columnInPosition], true);
					}
				}
			}
		}

		internal void SetCellSelected(GridCell gridCell, bool condition)
		{
			if (SelectionMode == SelectionMode.MultipleCellSelection)
			{
				if (RowStateHelper.IsCellSelected(gridCell) != condition)
				{
					RowStateHelper.SetCellSelected(gridCell, condition);
					if (RowStateHelper.ComputeCellSelectionStatus(gridCell.Row.GetLogicalRow()) == CellSelectionStatus.NoCellIsSelected)
					{
						RemoveSelectedItemAtPosition(gridCell.Row.Position);
					}
					else
					{
						AddRowToSelection(gridCell.Row.Position);
					}
					InvalidateGridRowLayout(gridCell.Row);
					RaiseSelectionChangedOrDefer();
				}
				return;
			}
			throw new InvalidOperationException();
		}

		internal string GetCellToolTipText()
		{
			if (_toolTipLayoutItem != null && _toolTipLayoutItem._gridGroupLayoutStateOwner != null && _toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow != null && _toolTipLayoutItem._column != null && (_toolTipLayoutItem._type == LayoutItemType.CardCell || _toolTipLayoutItem._type == LayoutItemType.Cell || _toolTipLayoutItem._type == LayoutItemType.TotalRowCell || _toolTipLayoutItem._type == LayoutItemType.GroupTotalCell))
			{
				FetchCellToolTipTextEventArgs e = new FetchCellToolTipTextEventArgs(_toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow, _toolTipLayoutItem._column);
				OnFetchCellToolTipText(e);
				return e.ToolTipText;
			}
			return "";
		}

		internal void SetRowExpandedState(MordexRow mordexRow, bool condition)
		{
			if (_view == View.TableView)
			{
				if (mordexRow.Expanded != condition)
				{
					ToggleRowExpanded(mordexRow);
				}
			}
			else if (RowStateHelper.GetRowTag(mordexRow).GetIsExpanded() != condition)
			{
				ToggleCardExpanded(mordexRow);
			}
		}

		internal void ToggleRowExpanded(MordexRow mordexRow)
		{
			GridTable gridTable = (GridTable)mordexRow.Table;
			if (gridTable.AreGroupsExpandable() || mordexRow is MordexDataRow)
			{
				mordexRow.Expanded = !mordexRow.Expanded;
				RefreshRowCountAndInvalidate();
			}
		}

		internal void ToggleCardExpanded(MordexRow mordexRow)
		{
			if (!_expandableCards || !AcceptCellEdit())
			{
				return;
			}
			RowTag rowTag = RowStateHelper.GetRowTag(mordexRow);
			if (rowTag.GetIsExpanded())
			{
				if (!CanCollapseRow(mordexRow))
				{
					return;
				}
			}
			else if (!CanExpandRow(mordexRow))
			{
				return;
			}
			rowTag.SetIsExpanded(!rowTag.GetIsExpanded());
			RefreshRowCountAndInvalidate();
			GridRow row = GetGridRowForLogicalRow(mordexRow);
			if (rowTag.GetIsExpanded())
			{
				OnRowExpanded(new RowActionEventArgs(row));
			}
			else
			{
				OnRowCollapsed(new RowActionEventArgs(row));
			}
		}

		private bool IsSingleCellSelection()
		{
			if (SelectionMode != SelectionMode.MultipleCellSelection)
			{
				return _cellSelectionMode == CellSelectionMode.SingleCell;
			}
			return true;
		}

		internal void ResetRowCacheOnTableChange(GridTable gridTable)
		{
			RebuildRows(false);
			RefreshRowCountAndInvalidate();
		}

		public GridRow GetRow()
		{
			if (_cachedRow != null && _cachedRow.Table != null)
			{
				return _cachedRow;
			}
			try
			{
				if (Row != -3)
				{
					return GetRow(Row);
				}
				return null;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return null;
			}
		}

		public GridRow GetRow(int rowPosition)
		{
			MordexRow mordexRow = _gridDataSourceManager.GetRowByIndex(rowPosition);
			return GetGridRowForLogicalRow(mordexRow);
		}

		public GridRow GetRow(object dataRow)
		{
			MordexRow mordexRow = _gridDataSourceManager.FindDataRow(dataRow);
			if (mordexRow == null)
			{
				return null;
			}
			return GetGridRowForLogicalRow(mordexRow);
		}

		public GridRow[] GetRows()
		{
			if (_gridDataSourceManager != null && _gridDataSourceManager.RootRow != null)
			{
				MordexRowCollection rows = _gridDataSourceManager.RootRow.Rows;
				List<GridRow> list = new List<GridRow>();
				for (int i = 0; i < rows.Count; i++)
				{
					if (rows[i].IsRowVisible())
					{
						list.Add(GetGridRowForLogicalRow(rows[i]));
					}
				}
				return list.ToArray();
			}
			return new GridRow[0];
		}

		public GridRow[] GetDataRows()
		{
			if (_gridDataSourceManager != null && _gridDataSourceManager.RootRow != null)
			{
				MordexRow[] dataRows = _gridDataSourceManager.RootRow.GetDataRows(RootTable);
				GridRow[] array = new GridRow[dataRows.Length];
				for (int i = 0; i < dataRows.Length; i++)
				{
					array[i] = GetGridRowForLogicalRow(dataRows[i]);
				}
				return array;
			}
			return new GridRow[0];
		}

		internal void MarkRowForRefresh(GridRow otherGridRow)
		{
			GridRow gridRow = _rowCache.FindGridRow(otherGridRow.GetLogicalRow());
			if (gridRow == otherGridRow)
			{
				otherGridRow._isLayoutDirty = true;
				_rowCache.ReleaseStaleRows();
				RefreshRowCountAndInvalidate();
			}
		}

		internal GridRow GetOrCreateGridRow(MordexRow key, bool condition)
		{
			if (key == null)
			{
				return null;
			}
			if (_dictionary != null && _dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			value = new GridRow(key, this);
			if (!(key is MordexNewDataRow))
			{
				OnLoadingRow(new RowLoadEventArgs(value));
			}
			if (condition)
			{
				PrepareRowForDisplay(value, false);
			}
			if (_dictionary != null)
			{
				_dictionary.Add(key, value);
			}
			return value;
		}

		internal void RaiseFormattingRow(GridRow row)
		{
			OnFormattingRow(new RowLoadEventArgs(row));
			row._isFormatting = true;
		}

		internal GridRow GetGridRowForLogicalRow(MordexRow logicalRow)
		{
			if (_row != null && logicalRow == _row.GetLogicalRow())
			{
				if (_hasStaleRowCells)
				{
					int position = _row.Position;
					SetCurrentGridRow(null, true);
					SetCurrentGridRow(GetRow(position), true);
					_hasStaleRowCells = false;
				}
				return _row;
			}
			if (logicalRow == null || logicalRow.Table == null)
			{
				return null;
			}
			GridRow gridRow = _rowCache.FindGridRow(logicalRow);
			if (gridRow == null)
			{
				if (logicalRow is VirtualDataRow)
				{
					gridRow = GetFilterGridRow();
				}
				else if (_row != null && logicalRow == _row.GetLogicalRow())
				{
					gridRow = _row;
				}
				else
				{
					gridRow = new GridRow(logicalRow, this);
					_rowCache.AddRow(gridRow);
					if (!(logicalRow is MordexNewDataRow))
					{
						OnLoadingRow(new RowLoadEventArgs(gridRow));
					}
					PrepareRowForDisplay(gridRow, false);
				}
			}
			return gridRow;
		}

		internal void EnableRowCache()
		{
			_isClearing = true;
			_dictionary = new Dictionary<MordexRow, GridRow>();
		}

		private bool IsNewRowAtTop()
		{
			if (RootTable.IsAddNewAllowed() && RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow)
			{
				return true;
			}
			return false;
		}

		private void SelectRowRange(int value, int otherValue, bool condition)
		{
			_isSelectionUpdating = true;
			_gridSelectedItemCollection.Clear();
			if (value > otherValue)
			{
				int nextValue = value;
				value = otherValue;
				otherValue = nextValue;
			}
			if (condition)
			{
				MordexRowManager mordexRowManager = GetRowManager();
				for (int i = value; i <= otherValue; i++)
				{
					if ((i == -2 && _row.Table == RootTable) || mordexRowManager[i].Table == _row.Table)
					{
						AddRowToSelection(i);
						if (SelectionMode == SelectionMode.MultipleCellSelection)
						{
							RowStateHelper.SelectAllRowCells(mordexRowManager[i]);
						}
					}
				}
			}
			else
			{
				for (int j = value; j <= otherValue; j++)
				{
					AddRowToSelection(j);
				}
			}
			_isSelectionUpdating = false;
			RaiseSelectionChangedOrDefer();
		}

		private void SelectRowRangeWithAllCells(int value, int otherValue)
		{
			_isSelectionUpdating = true;
			_gridSelectedItemCollection.Clear();
			if (value > otherValue)
			{
				int nextValue = value;
				value = otherValue;
				otherValue = nextValue;
			}
			MordexRowManager mordexRowManager = GetRowManager();
			for (int i = value; i <= otherValue; i++)
			{
				if (mordexRowManager[i].Table == _row.Table)
				{
					AddRowToSelection(i);
					RowStateHelper.SelectAllRowCells(mordexRowManager[i]);
				}
			}
			_isSelectionUpdating = false;
			RaiseSelectionChangedOrDefer();
		}

		private void MoveCurrentToNextCell()
		{
			if (_row == null)
			{
				HasFocusableColumnFrom(0, 0);
				SyncCellSelectionWithCurrent();
				return;
			}
			if (_row.RowType == RowType.GroupHeader)
			{
				SetHorizontalScrollOffset(0);
				HasFocusableColumnFrom(Row + 1, 0);
				SyncCellSelectionWithCurrent();
				return;
			}
			GridColumn gridColumn = GetLastEditableColumn(_row);
			if (_currentColumn != gridColumn)
			{
				HasFocusableColumnFrom(Row, Col + 1);
				SyncCellSelectionWithCurrent();
			}
			else
			{
				if (_view == View.SingleCard)
				{
					return;
				}
				if (Row != RowCount - 1)
				{
					if (HasFocusableColumnFrom(Row + 1, 0))
					{
						SyncCellSelectionWithCurrent();
						if (_view == View.TableView)
						{
							SetHorizontalScrollOffset(0);
						}
						else
						{
							SetVerticalScrollOffset(0);
						}
					}
				}
				else if (_row != null && _row.RowType == RowType.NewRecord && DataChanged && UpdateData())
				{
					SyncCellSelectionWithCurrent();
					Col = 0;
				}
			}
		}

		private void MoveCurrentToRowLastColumn(int rowPosition)
		{
			if (IsValidRowPosition(rowPosition))
			{
				GridRow row = GetRow(rowPosition);
				if (row.RowType == RowType.GroupHeader)
				{
					IsRowFocusableInCurrentColumn(rowPosition);
					SyncCellSelectionWithCurrent();
				}
				else
				{
					GridColumn gridColumn = GetLastEditableColumn(row);
					CanFocusCell(rowPosition, gridColumn);
					SyncCellSelectionWithCurrent();
				}
			}
		}

		private void MoveCurrentToNextRow(bool condition)
		{
			if (_view == View.SingleCard)
			{
				return;
			}
			if (_row == null)
			{
				IsRowFocusableInCurrentColumn(0);
				if (condition)
				{
					SyncCellSelectionWithCurrent();
				}
			}
			else
			{
				if (Row >= RowCount - 1)
				{
					return;
				}
				bool otherCondition = false;
				if (condition && _currentColumn == null && SelectionMode == SelectionMode.MultipleCellSelection && _row != null && RowStateHelper.ComputeCellSelectionStatus(_row.GetLogicalRow()) == CellSelectionStatus.AllCellsAreSelected)
				{
					otherCondition = true;
				}
				_isCurrentCellChanging = true;
				if (Row == -2)
				{
					if (IsValidRowPosition(-1))
					{
						IsRowFocusableInCurrentColumn(-1);
					}
					else
					{
						IsRowFocusableInCurrentColumn(0);
					}
				}
				else
				{
					IsRowFocusableInCurrentColumn(Row + 1);
				}
				if (condition)
				{
					SyncCellSelectionWithCurrent();
					if (otherCondition)
					{
						RowStateHelper.SelectAllRowCells(_row.GetLogicalRow());
					}
				}
				_isCurrentCellChanging = false;
				EnsureVisible();
			}
		}

		private void MoveCurrentToPreviousRow(bool condition)
		{
			if (_view == View.SingleCard)
			{
				return;
			}
			if (_row == null)
			{
				if (IsValidRowPosition(-1))
				{
					Row = -1;
				}
				else if (IsValidRowPosition(0))
				{
					Row = 0;
				}
			}
			else if (Row == 0)
			{
				if (_view != View.TableView)
				{
					return;
				}
				if (IsNewRowAtTop())
				{
					IsRowFocusableInCurrentColumn(-1);
					if (condition)
					{
						SyncCellSelectionWithCurrent();
					}
				}
				else if (_filterMode != FilterMode.None)
				{
					IsRowFocusableInCurrentColumn(-2);
					if (condition)
					{
						SyncCellSelectionWithCurrent();
					}
				}
			}
			else if (Row > 0)
			{
				bool otherCondition = false;
				if (condition && _currentColumn == null && SelectionMode == SelectionMode.MultipleCellSelection && _row != null && RowStateHelper.ComputeCellSelectionStatus(_row.GetLogicalRow()) == CellSelectionStatus.AllCellsAreSelected)
				{
					otherCondition = true;
				}
				_isCurrentCellChanging = true;
				IsRowFocusableInCurrentColumn(Row - 1);
				if (condition)
				{
					SyncCellSelectionWithCurrent();
					if (otherCondition)
					{
						RowStateHelper.SelectAllRowCells(_row.GetLogicalRow());
					}
				}
				_isCurrentCellChanging = false;
				EnsureVisible();
			}
			else if (Row == -1 && _filterMode != FilterMode.None)
			{
				IsRowFocusableInCurrentColumn(-2);
				if (condition)
				{
					SyncCellSelectionWithCurrent();
				}
			}
		}

		private void MoveCurrentToPreviousCell()
		{
			if (_row == null)
			{
				return;
			}
			if (_row.RowType == RowType.GroupHeader)
			{
				MoveCurrentToRowLastColumn(Row - 1);
				return;
			}
			GridColumn gridColumn = GetFirstEditableColumn(_row);
			GridColumn otherColumnInPosition = null;
			if (_currentColumn != null && _currentColumn != gridColumn)
			{
				for (int position = Col - 1; position >= 0; position--)
				{
					GridColumn columnInPosition = CurrentTable.Columns.GetColumnInPosition(position);
					if (IsCellSelectable(_row, columnInPosition))
					{
						otherColumnInPosition = columnInPosition;
						break;
					}
				}
				IsColumnFocusableInCurrentRow(otherColumnInPosition);
				SyncCellSelectionWithCurrent();
			}
			else if (_view != View.SingleCard)
			{
				MoveCurrentToRowLastColumn(Row - 1);
			}
		}

		private void SetCurrentRowKeepSelection(int value)
		{
			if (!IsValidRowPosition(value) && value != -3)
			{
				value = -3;
			}
			IsRowFocusableInCurrentColumn(value);
			if (_row == null || !_row.Selected)
			{
				SyncCellSelectionWithCurrent();
			}
		}

		private void SetCurrentRowAndSelect(int value)
		{
			if (!IsValidRowPosition(value) && value != -3)
			{
				value = -3;
			}
			IsRowFocusableInCurrentColumn(value);
			SyncCellSelectionWithCurrent();
		}

		private void SetCurrentGridRow(GridRow row, bool condition)
		{
			if (_row != row)
			{
				if (_row != null && _row.HasChangedCells() && !_isRowEditSuppressed)
				{
					_row.CancelEdit();
				}
				if (row != null && row.RowType == RowType.NewRecord)
				{
					row.CommitCells();
				}
				if (row != null && row.RowType == RowType.Record)
				{
					_currentRowData = row.DataRow;
				}
				else if (condition)
				{
					_currentRowData = null;
				}
				_row = row;
				SyncRecordNavigatorPosition();
			}
		}

		public void SelectCellRange(int rowFrom, int colFrom, int rowTo, int colTo)
		{
			if (SelectionMode != SelectionMode.MultipleCellSelection)
			{
				throw new InvalidOperationException("Cell ranges can only be selected when SelectionMode is MultipleCellSelection");
			}
			Rectangle rectangle = RectangleFromCorners(new Point(colFrom, rowFrom), new Point(colTo, rowTo));
			SelectCellsInRange(rectangle);
		}
	}
}
