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
		private int CalculateFirstRowForCardView(Graphics graphics, int value)
		{
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			GridTableLayoutState gridTableLayoutState = null;
			if (_cardRowLayouts != null)
			{
				foreach (GridTableLayoutState item in _cardRowLayouts)
				{
					if (item.GetRowPosition() == value)
					{
						gridTableLayoutState = item;
						break;
					}
				}
			}
			if (gridTableLayoutState != null)
			{
				if (gridTableLayoutState._cardColumnIndex == 0)
				{
					return _firstRow;
				}
				foreach (GridTableLayoutState cardRowLayout in _cardRowLayouts)
				{
					if (cardRowLayout._cardColumnIndex == 1 && cardRowLayout._location.Y == _cardSpacing)
					{
						return cardRowLayout.GetRowPosition();
					}
				}
			}
			if (_cardRowLayouts == null)
			{
				_cardRowLayouts = new TableLayoutCollection();
			}
			_cardRowLayouts.ClearAndDispose();
			GridColumnCollection columns = RootTable.Columns;
			_ = columns._owner;
			Rectangle clientRectangle = base.ClientRectangle;
			_rowCache.MarkOtherRowsStale(_row);
			int otherValue = 0;
			int cardColumnIndex = 0;
			int nextValue = 0;
			GridFormatStyle gridFormatStyle = GetCardCaptionStyle();
			MordexRowManager mordexRowManager = GetRowManager();
			Point point = new Point(_cardSpacing, _cardSpacing);
			int previousValue = 0;
			int firstRowForCardView = value;
			MordexRow mordexRow = null;
			if (firstRowForCardView >= 0 && firstRowForCardView < mordexRowManager.Count)
			{
				mordexRow = mordexRowManager[firstRowForCardView];
			}
			while (mordexRow != null)
			{
				GridRow gridRow = GetGridRowForLogicalRow(mordexRow);
				PrepareRowForDisplay(gridRow, false);
				gridRow._isLayoutDirty = false;
				GridFormatStyle cardColumnHeaderStyle = GetCardColumnHeaderStyle(gridRow.GetPosition());
				GridFormatStyle alternatingRowStyle = GetAlternatingRowStyle(RootTable, gridRow.GetPosition());
				GridTableLayoutState cardLayout = BuildCardLayout(graphics, false, gridRow, gridFormatStyle, cardColumnHeaderStyle, alternatingRowStyle);
				alternatingRowStyle.DisposeResources();
				cardColumnHeaderStyle.DisposeResources();
				if (point.Y == _cardSpacing)
				{
					cardLayout._location = point;
				}
				else
				{
					if (point.Y - previousValue + cardLayout.GetHeight() > clientRectangle.Bottom)
					{
						point.X += _cardWidth + _cardSpacing + previousValue;
						if (point.X + cardLayout.GetWidth() > clientRectangle.Right)
						{
							return firstRowForCardView + 1;
						}
						cardColumnIndex++;
						point.Y = _cardSpacing;
					}
					cardLayout._location = point;
				}
				if (point.X == CardSpacing || point.X + cardLayout.GetWidth() < clientRectangle.Right)
				{
					otherValue++;
				}
				cardLayout._cardColumnIndex = cardColumnIndex;
				if (cardColumnIndex == 0)
				{
					nextValue++;
				}
				_cardRowLayouts.AddTableLayout(cardLayout);
				point.Y += cardLayout.GetHeight() + _cardSpacing + previousValue;
				firstRowForCardView--;
				mordexRow = ((firstRowForCardView < 0 || firstRowForCardView >= mordexRowManager.Count) ? null : mordexRowManager[firstRowForCardView]);
			}
			return firstRowForCardView;
		}

		private int CalculateFirstRowForTableView(Graphics graphics, int firstRowForTableView)
		{
			_ = base.ClientRectangle;
			int value = _bodyBottomY - _bodyTopY;
			if (value <= 0 || GetRowManager() == null)
			{
				return firstRowForTableView;
			}
			MordexRowManager mordexRowManager = GetRowManager();
			int[] array = new int[mordexRowManager.Count];
			for (int i = 0; i < mordexRowManager.Count; i++)
			{
				array[i] = -1;
			}
			int otherResult = firstRowForTableView;
			int result = firstRowForTableView;
			while (true)
			{
				GridTable gridTable = RootTable;
				int otherValue = 0;
				int bodyTopY = _bodyTopY;
				MordexRow mordexRow = null;
				int nextValue = otherResult;
				if (nextValue >= 0 && nextValue < mordexRowManager.Count)
				{
					mordexRow = mordexRowManager[nextValue];
				}
				MordexRow otherMordexRow = null;
				while (mordexRow != null && bodyTopY < base.ClientRectangle.Bottom && nextValue <= firstRowForTableView)
				{
					GridTable table = (GridTable)mordexRow.Table;
					int columnHeaderHeight = 0;
					if (gridTable != table)
					{
						if (_tableSpacing > 0)
						{
							otherValue += _tableSpacing;
							bodyTopY += _tableSpacing;
						}
						if (table.AreHeadersRepeated() || RowStateHelper.IsFirstSibling(mordexRow) || otherMordexRow == null)
						{
							GridFormatStyle gridFormatStyle = GetHeaderStyle(table);
							columnHeaderHeight = table.CalculateColumnHeaderHeight(graphics, gridFormatStyle, false);
							if (table.IsTableHeaderVisible())
							{
								columnHeaderHeight += table.CalculateTableHeaderHeight(graphics);
							}
							gridFormatStyle.DisposeResources();
							otherValue += columnHeaderHeight;
							bodyTopY += columnHeaderHeight;
						}
						gridTable = table;
					}
					if (array[nextValue] == -1)
					{
						GridRow gridRow = GetGridRowForLogicalRow(mordexRow);
						PrepareRowForDisplay(gridRow, false);
						gridRow._isLayoutDirty = false;
						MxRowLayoutState mordexRowLayoutState;
						switch (gridRow.RowType)
						{
						case RowType.GroupHeader:
						{
							GridFormatStyle gridFormatStyle = GetGroupRowStyle(table);
							mordexRowLayoutState = table.BuildGroupHeaderRowLayout(graphics, gridRow, otherMordexRow, gridFormatStyle, GetEffectiveFrozenColumns());
							gridFormatStyle.DisposeResources();
							break;
						}
						case RowType.NewRecord:
						{
							GridFormatStyle gridFormatStyle;
							if (table.ComputeNewRowPosition() == NewRowPosition.TopRow)
							{
								gridFormatStyle = GetNewRowStyle(table);
								mordexRowLayoutState = table.BuildNewRowLayout(graphics, gridRow, otherMordexRow, gridFormatStyle, GetEffectiveFrozenColumns());
							}
							else
							{
								int positionOfChild = mordexRow.ParentRow.PositionOfChild(mordexRow);
								gridFormatStyle = GetAlternatingRowStyle(table, positionOfChild);
								mordexRowLayoutState = table.BuildDataRowLayout(graphics, gridRow, otherMordexRow, gridFormatStyle, base.ClientRectangle.Width, GetEffectiveFrozenColumns());
							}
							gridFormatStyle.DisposeResources();
							break;
						}
						case RowType.GroupFooter:
						{
							GridFormatStyle gridFormatStyle = GetGroupTotalRowStyle(table);
							mordexRowLayoutState = table.BuildGroupTotalRowLayout(graphics, gridRow, otherMordexRow, gridFormatStyle, GetEffectiveFrozenColumns());
							gridFormatStyle.DisposeResources();
							break;
						}
						case RowType.TotalRow:
						{
							GridFormatStyle gridFormatStyle = GetTotalRowStyle(table);
							mordexRowLayoutState = ((!table.IsRootTable()) ? table.BuildTotalRowLayout(graphics, gridRow, otherMordexRow, gridFormatStyle, GetEffectiveFrozenColumns()) : table.BuildTotalRowLayout(graphics, gridRow, null, gridFormatStyle, GetEffectiveFrozenColumns()));
							gridFormatStyle.DisposeResources();
							break;
						}
						default:
						{
							int otherPositionOfChild = mordexRow.ParentRow.PositionOfChild(mordexRow);
							GridFormatStyle gridFormatStyle = GetAlternatingRowStyle(table, otherPositionOfChild);
							mordexRowLayoutState = table.BuildDataRowLayout(graphics, gridRow, otherMordexRow, gridFormatStyle, base.ClientRectangle.Width, GetEffectiveFrozenColumns());
							gridFormatStyle.DisposeResources();
							break;
						}
						}
						mordexRowLayoutState._gridRow = gridRow;
						columnHeaderHeight = (array[nextValue] = mordexRowLayoutState._rowHeight);
						mordexRowLayoutState.ReleaseItems();
					}
					else
					{
						columnHeaderHeight = array[nextValue];
					}
					otherValue += columnHeaderHeight;
					if (otherValue > value)
					{
						break;
					}
					otherMordexRow = mordexRow;
					nextValue++;
					mordexRow = ((nextValue >= mordexRowManager.Count) ? null : mordexRowManager[nextValue]);
				}
				if (otherValue >= value + 1)
				{
					break;
				}
				result = otherResult;
				if (otherResult <= 0)
				{
					break;
				}
				otherResult--;
			}
			return result;
		}

		public void ExpandGroups()
		{
			_gridDataSourceManager.ExpandGroups();
		}

		internal void ExpandGroupInSource(GridGroup group)
		{
			_gridDataSourceManager.ExpandGroup(group);
		}

		internal void CollapseGroupInSource(GridGroup group)
		{
			_gridDataSourceManager.CollapseGroup(group);
		}

		public void CollapseGroups()
		{
			_gridDataSourceManager.CollapseGroups();
		}

		public void ExpandRecords()
		{
			ExpandRecords(null);
		}

		public void ExpandRecords(GridTable table)
		{
			if (table != null && table.DataGrid != this)
			{
				throw new InvalidOperationException("The table doesn't belong to the control.");
			}
			_gridDataSourceManager.ExpandDataRows(table);
			if (table == null)
			{
				table = RootTable;
			}
			if (table != null && table.GetBaseIsSelfReferencingTable() && table.SelfReferencingSettings.AutoSizeColumnOnExpand)
			{
				table.GetExpandColumn()?.AutoSize();
			}
		}

		public void CollapseRecords()
		{
			_gridDataSourceManager.CollapseDataRows(null);
		}

		public void CollapseRecords(GridTable table)
		{
			if (table != null && table.DataGrid != this)
			{
				throw new InvalidOperationException("The table doesn't belong to the control.");
			}
			_gridDataSourceManager.CollapseDataRows(table);
		}

		private void AddRowToSelection(int position)
		{
			if (IsValidRowPosition(position))
			{
				_gridSelectedItemCollection.Add(position);
			}
		}

		private void SyncCellSelectionWithCurrent()
		{
			SyncRowSelectionWithCurrent();
			if (SelectionMode == SelectionMode.MultipleCellSelection && _row != null)
			{
				CellSelectionStatus cellSelectionStatus = RowStateHelper.ComputeCellSelectionStatus(_row.GetLogicalRow());
				RowStateHelper.SelectOnlyColumnCell(_row.GetLogicalRow(), CurrentColumn);
				if (cellSelectionStatus != RowStateHelper.ComputeCellSelectionStatus(_row.GetLogicalRow()))
				{
					RaiseSelectionChangedOrDefer();
				}
				if (CurrentColumn != null)
				{
					SetCellSelected(_row.Cells[CurrentColumn], true);
					SetLastSelectedColumn(CurrentColumn);
				}
			}
		}

		private void SyncRowSelectionWithCurrent()
		{
			if ((Row != -3 || _gridSelectedItemCollection.Count != 0) && (_gridSelectedItemCollection.Count != 1 || _gridSelectedItemCollection[0].Position != Row || _row == null || _row.GetLogicalRow() != _gridSelectedItemCollection[0]._row))
			{
				_isSelectionUpdating = true;
				_gridSelectedItemCollection.Clear();
				AddRowToSelection(Row);
				if (Row == -2)
				{
					_selectionAnchorRow = 0;
				}
				else
				{
					_selectionAnchorRow = Row;
				}
				if (_isSelectionNotifyEnabled)
				{
					RaiseSelectionChangedOrDefer();
				}
				_isSelectionUpdating = false;
			}
		}

		private GridFormatStyle BuildCellFormatStyle(GridGroupLayoutState gridGroupLayoutState)
		{
			GridRow gridRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow;
			GridCell cell = gridRow.Cells[gridGroupLayoutState._column.Index];
			// The caller owns the style that this method returns. Each merge that makes a new style releases the style
			// it replaces; a merge that changes nothing returns the same style.
			GridFormatStyle style = (gridGroupLayoutState.GetOwnerTable().ComputeNewRowPosition() != NewRowPosition.TopRow || gridRow.RowType != RowType.NewRecord) ? GetAlternatingRowStyle(gridGroupLayoutState.GetOwnerTable(), gridRow.GetPosition()) : GetNewRowStyle(gridGroupLayoutState.GetOwnerTable());
			try
			{
				style = ApplyOwned(style, gridRow.RowStyle);
				style = ApplyOwned(style, gridRow.GetCachedFormatStyle());
				style = ApplyOwned(style, gridGroupLayoutState._column.CellStyle);
				style = ApplyOwned(style, cell.GetEffectiveFormatStyle());
				style = ApplyOwned(style, cell.FormatStyle);
				if (HasRowError(gridRow))
				{
					style = ApplyOwned(style, _rowWithErrorsFormatStyle);
				}
				if (base.ContainsFocus || _isEnteringForEdit)
				{
					style = ApplyOwned(style, FocusCellDisplayMode != FocusCellDisplayMode.UseFocusCellFormatStyle ? _selectedFormatStyle : _focusCellFormatStyle);
				}
			}
			catch
			{
				style.DisposeResources();
				throw;
			}
			return style;
		}

		private static GridFormatStyle ApplyOwned(GridFormatStyle owned, GridFormatStyle overrides)
		{
			GridFormatStyle merged = owned.ApplyOverrides(overrides);
			if (merged != owned)
			{
				owned.DisposeResources();
			}
			return merged;
		}

		void IGridDropListOwner.OnComputeRows()
		{
		}

		internal void RemoveRecordRow(GridRow gridRow)
		{
			bool condition = false;
			if (gridRow == null || gridRow.RowType != RowType.Record)
			{
				return;
			}
			int row = Row;
			_gridSelectedItemCollection.Sort();
			if (gridRow.Position >= 0 && gridRow.Position < row)
			{
				row = gridRow.Position;
			}
			EndCellEdit(false);
			CancelCurrentEdit();
			_isSelectionUpdating = true;
			_isSelectionNotifyEnabled = false;
			try
			{
				if (_hasCurrencyManagerHandlers)
				{
					_currencyManager.PositionChanged -= OnCurrencyManagerPositionChanged;
				}
				gridRow.SetDataChanged(false);
				gridRow.GetLogicalRow().Delete();
				condition = true;
			}
			catch (Exception ex)
			{
				ProcessException(ex, true);
			}
			finally
			{
				if (row < RecordCount)
				{
					_lastRecordPosition = row;
				}
				else
				{
					_lastRecordPosition = row - 1;
				}
				if (_hasCurrencyManagerHandlers)
				{
					_currencyManager.PositionChanged += OnCurrencyManagerPositionChanged;
				}
				_isSelectionUpdating = false;
				if (row < RowCount)
				{
					SetCurrentRowAndSelect(row);
				}
				else
				{
					SetCurrentRowAndSelect(row - 1);
				}
			}
			_isSelectionNotifyEnabled = true;
			if (condition)
			{
				EnsureVisible();
				RaiseSelectionChangedOrDefer();
			}
			Refresh();
		}

		private bool IsRowFocusableInCurrentColumn(int value)
		{
			return CanFocusCell(value, _currentColumn);
		}

		private bool IsColumnFocusableInCurrentRow(GridColumn gridColumn)
		{
			return CanFocusCell(_row.Position, gridColumn);
		}

		private bool CanFocusCell(int value, GridColumn gridColumn)
		{
			return CanActivateCellAt(value, gridColumn, false);
		}

		private static bool IsCellSelectable(GridRow gridRow, GridColumn gridColumn)
		{
			if (gridRow == null || gridColumn == null || !gridColumn.Selectable || !gridColumn.Visible)
			{
				return false;
			}
			if (gridColumn.Table != null && gridColumn.Table.CellLayoutMode == CellLayoutMode.UseColumnSets && (gridColumn._owner == null || !gridColumn.ColumnSet.Visible))
			{
				return false;
			}
			switch (gridRow.RowType)
			{
			case RowType.FilterRow:
				return (gridColumn.SelectableCells & SelectableCells.FilterRowCells) != 0;
			case RowType.NewRecord:
				return (gridColumn.SelectableCells & SelectableCells.NewRowCells) != 0;
			case RowType.GroupFooter:
				return (gridColumn.SelectableCells & SelectableCells.GroupTotalRowCells) != 0;
			case RowType.GroupHeader:
				return true;
			case RowType.Record:
				return (gridColumn.SelectableCells & SelectableCells.RowCells) != 0;
			case RowType.TotalRow:
				return (gridColumn.SelectableCells & SelectableCells.TotalRowCells) != 0;
			default:
				return false;
			}
		}

		private bool CanActivateCellAt(int value, GridColumn otherGridColumn, bool condition)
		{
			GridRow gridRow = GetGridRowByPosition(value);
			GridColumn gridColumn;
			if (otherGridColumn != null && gridRow != null)
			{
				if (gridRow != null && otherGridColumn.Table == gridRow.Table)
				{
					gridColumn = otherGridColumn;
					if (!IsCellSelectable(gridRow, gridColumn))
					{
						if (otherGridColumn.Table.CellLayoutMode == CellLayoutMode.UseColumns)
						{
							return HasFocusableColumnFrom(value, gridColumn._position);
						}
						return HasFocusableColumnFrom(value, gridColumn._gridColumnPosition);
					}
				}
				else
				{
					gridColumn = GetFirstEditableColumn(gridRow);
				}
				if (gridColumn != null && _view != View.TableView && !RowStateHelper.GetRowTag(gridRow.GetLogicalRow()).GetIsExpanded())
				{
					gridColumn = null;
				}
			}
			else
			{
				gridColumn = null;
			}
			if (gridRow == _row && gridColumn == _currentColumn)
			{
				return true;
			}
			if (_row != null && _row == _gridRow && gridRow != _row && _filterRowUpdateMode == FilterRowUpdateMode.WhenReturnKeyIsPressed && _row.HasChangedCells())
			{
				return false;
			}
			CurrentCellChangingEventArgs e = new CurrentCellChangingEventArgs(gridRow, gridColumn, cancel: false);
			if (!_isEditCancelling)
			{
				OnCurrentCellChanging(e);
			}
			if (e.Cancel)
			{
				return false;
			}
			if (!AcceptCellEdit())
			{
				return false;
			}
			if (_gridValueListItemCollectionScrollableBase != null)
			{
				((IGridDropListOwner)this).OnDropListHide(cancel: true);
			}
			gridRow = GetGridRowByPosition(value);
			if (_dragSelectStartRow != -1)
			{
				gridColumn = null;
			}
			if (gridRow != _row)
			{
				if (!UpdateData())
				{
					return false;
				}
				if (!IsValidRowPosition(value) && value != -3)
				{
					value = RowCount - 1;
				}
				if (value != -3)
				{
					if (gridRow == null || gridRow.GetLogicalRow() == null || gridRow.Position != value || gridRow.GetLogicalRow().DataSourceManager == null || !gridRow.IsVisible)
					{
						gridRow = GetGridRowByPosition(value);
					}
					else if (gridRow.GetLogicalRow() is MordexTotalRow)
					{
						gridRow = GetGridRowByPosition(value);
					}
					else if (gridRow.GetLogicalRow() is MordexNewDataRow)
					{
						gridRow = GetGridRowByPosition(value);
					}
					InvalidateGridRowLayout(_row);
					SetCurrentGridRow(gridRow, true);
					InvalidateGridRowLayout(_row);
				}
				else
				{
					InvalidateGridRowLayout(_row);
					SetCurrentGridRow(null, true);
				}
			}
			else if (_currentColumn != gridColumn)
			{
				InvalidateGridRowLayout(_row);
			}
			AcceptCellEdit();
			if (_row != null && gridColumn != null && _row.Table == gridColumn.Table)
			{
				_currentColumn = gridColumn;
			}
			else
			{
				_currentColumn = null;
			}
			if (gridRow != null)
			{
				if (gridRow.Position == -1)
				{
					gridRow = _row;
				}
				if (_currencyManager != null && _isDataChangeTrackingEnabled)
				{
					MordexDataRow mordexDataRow = RowStateHelper.FindRootDataRow(gridRow.GetLogicalRow());
					int dataRowIndex = -1;
					if (mordexDataRow != null)
					{
						dataRowIndex = mordexDataRow.DataRowIndex;
					}
					else if (_currencyManager.Count > 0)
					{
						dataRowIndex = 0;
					}
					if (dataRowIndex != -1 && _currencyManager.Position != dataRowIndex)
					{
						_isDataChanging = true;
						if (gridRow.GetLogicalRow().ParentCollection != null)
						{
							try
							{
								_currencyManager.Position = dataRowIndex;
							}
							catch (Exception ex)
							{
								ProcessException(ex, true);
								_isDataChanging = false;
								OnCurrencyManagerPositionChanged(_currencyManager, EventArgs.Empty);
							}
						}
						_isDataChanging = false;
					}
				}
			}
			if (_row != null && _row.RowType == RowType.FilterRow && _filterRowUpdateMode == FilterRowUpdateMode.Default)
			{
				UpdateData();
			}
			EnsureVisible();
			if (_autoEdit && base.ContainsFocus)
			{
				if (_currentColumn != otherGridColumn)
				{
					condition = false;
				}
				BeginCellEdit(condition, '\0', false);
			}
			if (!_isEditCancelling)
			{
				OnCurrentCellChanged(EventArgs.Empty);
			}
			return true;
		}

		private bool ConfirmRecordUpdate(GridRow gridRow)
		{
			CancelEventArgs e = new CancelEventArgs(cancel: false);
			if (gridRow.RowType == RowType.NewRecord)
			{
				OnAddingRecord(e);
			}
			else
			{
				OnUpdatingRecord(e);
			}
			return !e.Cancel;
		}

		internal object RaiseGetNewRow(MordexNewDataRow mordexNewDataRow)
		{
			GetNewRowEventArgs e = new GetNewRowEventArgs();
			OnGetNewRow(e);
			return e.NewRow;
		}

		public void CollapseCards()
		{
			if (_view != View.TableView)
			{
				for (int i = 0; i < GetRowManager().Count; i++)
				{
					MordexRow mordexRow = GetRowManager()[i];
					RowStateHelper.GetRowTag(mordexRow).SetIsExpanded(false);
				}
			}
			RefreshRowCountAndInvalidate();
		}

		public void ExpandCards()
		{
			if (_gridDataSourceManager != null)
			{
				for (int i = 0; i < GetRowManager().Count; i++)
				{
					MordexRow mordexRow = GetRowManager()[i];
					RowStateHelper.GetRowTag(mordexRow).SetIsExpanded(true);
				}
			}
			RefreshRowCountAndInvalidate();
		}

		internal void SyncSelectionWithCurrentRow(bool condition)
		{
			if (_row == null || _row.GetLogicalRow() == null || GetRowManager() == null || GetRowManager().IsIndexSuspend)
			{
				return;
			}
			if (_row.GetLogicalRow().DataSourceManager == _gridDataSourceManager)
			{
				if (!condition)
				{
					return;
				}
				GridSelectedItem gridSelectedItem = null;
				for (int i = 0; i < _gridSelectedItemCollection.Count; i++)
				{
					GridSelectedItem gridSelectedItemCollectionItem = _gridSelectedItemCollection[i];
					if (gridSelectedItemCollectionItem.Position == Row)
					{
						gridSelectedItem = gridSelectedItemCollectionItem;
						break;
					}
				}
				int row = Row;
				_row = null;
				if (!IsValidRowPosition(row))
				{
					SetCurrentRowAndSelect(-3);
					return;
				}
				_row = GetRow(row);
				if (gridSelectedItem != null)
				{
					gridSelectedItem._isValid = false;
				}
				_gridSelectedItemCollection.RemoveInvalidItems();
				if (_row != null && _row.Selected)
				{
					RowStateHelper.GetRowTag(_row.GetLogicalRow()).SetDataState3Boolean(true);
					_gridSelectedItemCollection.AddItem(new GridSelectedItem(this, _row.GetLogicalRow()));
					_gridSelectedItemCollection.Sort();
				}
				RefreshRowCountAndInvalidate();
				return;
			}
			_row = null;
			if (_currencyManager != null && _currencyManager != null && _currencyManager.Position >= 0)
			{
				MordexRow mordexRow = _gridDataSourceManager.FindDataRow(_currencyManager.Current);
				if (mordexRow != null)
				{
					if (mordexRow.RowManagerPosition != -1)
					{
						SetCurrentRowAndSelect(mordexRow.RowManagerPosition);
						return;
					}
					while (mordexRow != null && mordexRow.RowManagerPosition == -1)
					{
						mordexRow = mordexRow.ParentRow;
					}
					if (mordexRow != null && mordexRow.RowManagerPosition != -1)
					{
						SetCurrentRowAndSelect(mordexRow.RowManagerPosition);
						return;
					}
				}
			}
			SetCurrentRowAndSelect(0);
		}

		internal void RebuildRows(bool condition)
		{
			if (_row != null && (_row.GetLogicalRow() == null || _row.GetLogicalRow().DataSourceManager != _gridDataSourceManager))
			{
				condition = true;
			}
			_rowCache.ReleaseAllRows();
			RefreshRowCountAndInvalidate();
			if (_row != null && _row.RowType != RowType.FilterRow)
			{
				SyncSelectionWithCurrentRow(condition);
			}
			Hashtable hashtable = null;
			if (_gridRow != null && FilterMode != FilterMode.None)
			{
				if (RootTable != null && RootTable.Columns.Count > 0)
				{
					hashtable = new Hashtable();
					for (int i = 0; i < _gridRow.Cells.Count; i++)
					{
						hashtable.Add(_gridRow.Cells[i].Column, _gridRow.Cells[i].Value);
					}
				}
				_gridRow = null;
				_gridRow = GetFilterGridRow();
				for (int j = 0; j < RootTable.Columns.Count; j++)
				{
					GridCell gridCell = _gridRow.Cells[j];
					if (hashtable.ContainsKey(gridCell.Column))
					{
						gridCell.AssignValue(hashtable[gridCell.Column], false);
					}
				}
			}
			RefreshRowCountAndInvalidate();
		}

		internal MordexRowManager GetRowManager()
		{
			if (_gridDataSourceManager == null)
			{
				return null;
			}
			return _gridDataSourceManager.RowManager;
		}

		internal GridDataSourceManager GetRowSourceManager()
		{
			return _gridDataSourceManager;
		}

		internal bool ShouldShowRowHeaders()
		{
			InheritableBoolean inheritableBoolean = _rowHeaders;
			if (inheritableBoolean == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool ShouldAllowExpandableGroups()
		{
			if (_expandableGroups == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		private bool RaiseRowCheckStateChanging(GridRow row, CheckStateChangeType changeType, RowCheckState oldCheckState, RowCheckState checkState)
		{
			RowCheckStateChangingEventArgs e = new RowCheckStateChangingEventArgs(row, changeType, oldCheckState, checkState);
			OnRowCheckStateChanging(e);
			return !e.Cancel;
		}

		protected virtual void OnFetchCellToolTipText(FetchCellToolTipTextEventArgs e)
		{
			if (base.Events[_fetchCellToolTipTextEventKey] is FetchCellToolTipTextEventHandler fetchCellToolTipTextEventHandler)
			{
				fetchCellToolTipTextEventHandler(this, e);
			}
		}

		protected virtual void OnRowCheckStateChanging(RowCheckStateChangingEventArgs e)
		{
			if (base.Events[_rowCheckStateChangingEventKey] is RowCheckStateChangingEventHandler rowCheckStateChangingEventHandler)
			{
				rowCheckStateChangingEventHandler(this, e);
			}
		}

		protected virtual void OnRowCheckStateChanged(RowCheckStateChangeEventArgs e)
		{
			if (base.Events[_rowCheckStateChangedEventKey] is RowCheckStateChangeEventHandler rowCheckStateChangeEventHandler)
			{
				rowCheckStateChangeEventHandler(this, e);
			}
		}

		protected virtual void OnDeletingRecord(RowActionCancelEventArgs e)
		{
			if (base.Events[_deletingRecordEventKey] is RowActionCancelEventHandler rowActionCancelEventHandler)
			{
				rowActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnGetChildList(GetChildListEventArgs e)
		{
			if (base.Events[_getChildListEventKey] is GetChildListEventHandler getChildListEventHandler)
			{
				getChildListEventHandler(this, e);
			}
		}

		protected virtual void OnCellValueChanged(ColumnActionEventArgs e)
		{
			if (base.Events[_cellValueChangedEventKey] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnGetNewRow(GetNewRowEventArgs e)
		{
			if (base.Events[_eventKeyGetNewRow] is GetNewRowEventHandler getNewRowEventHandler)
			{
				getNewRowEventHandler(this, e);
			}
		}

		protected virtual void OnRowCountChanged(EventArgs e)
		{
			if (base.Events[_eventKeyRowCountChanged] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnFormattingRow(RowLoadEventArgs e)
		{
			if (!(base.Events[_eventKeyFormattingRow] is RowLoadEventHandler rowLoadEventHandler))
			{
				return;
			}
			try
			{
				e.Row._isLayoutSuspended = true;
				rowLoadEventHandler(this, e);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			finally
			{
				e.Row._isLayoutSuspended = false;
			}
		}

		protected virtual void OnLoadingRow(RowLoadEventArgs e)
		{
			if (base.Events[_eventKeyLoadingRow] is RowLoadEventHandler rowLoadEventHandler)
			{
				try
				{
					e.Row._isLoading = true;
					rowLoadEventHandler(this, e);
					e.Row._isLoading = false;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
		}

		protected virtual void OnCellUpdated(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyCellUpdated] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnUpdatingCell(UpdatingCellEventArgs e)
		{
			if (base.Events[_eventKeyUpdatingCell] is UpdatingCellEventHandler updatingCellEventHandler)
			{
				updatingCellEventHandler(this, e);
			}
		}

		protected virtual void OnDeletingRecords(CancelEventArgs e)
		{
			if (base.Events[_eventKeyDeletingRecords] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnRecordUpdated(EventArgs e)
		{
			if (base.Events[_eventKeyRecordUpdated] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnRecordAdded(EventArgs e)
		{
			if (base.Events[_eventKeyRecordAdded] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnUpdatingRecord(CancelEventArgs e)
		{
			if (base.Events[_eventKeyUpdatingRecord] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnAddingRecord(CancelEventArgs e)
		{
			if (base.Events[_eventKeyAddingRecord] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnSizingCards(SizingCardsEventArgs e)
		{
			if (base.Events[_eventKeySizingCards] is SizingCardsEventHandler sizingCardsEventHandler)
			{
				sizingCardsEventHandler(this, e);
			}
		}

		protected virtual void OnCurrentCellChanged(EventArgs e)
		{
			if (_tableAccessibleObject != null)
			{
				_tableAccessibleObject.NotifyClients(AccessibleEvents.Selection);
			}
			if (base.Events[_eventKeyCurrentCellChanged] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentCellChanging(CurrentCellChangingEventArgs e)
		{
			if (base.Events[_eventKeyCurrentCellChanging] is CurrentCellChangingEventHandler currentCellChangingEventHandler)
			{
				currentCellChangingEventHandler(this, e);
			}
		}

		protected virtual void OnSelectionChanged(EventArgs e)
		{
			if (base.Events[_eventKeySelectionChanged] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnExpandingRow(RowActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyExpandingRow] is RowActionCancelEventHandler rowActionCancelEventHandler)
			{
				rowActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnCollapsingRow(RowActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyCollapsingRow] is RowActionCancelEventHandler rowActionCancelEventHandler)
			{
				rowActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnRowExpanded(RowActionEventArgs e)
		{
			if (base.Events[_eventKeyRowExpanded] is RowActionEventHandler rowActionEventHandler)
			{
				rowActionEventHandler(this, e);
			}
		}

		protected virtual void OnRowCollapsed(RowActionEventArgs e)
		{
			if (base.Events[_eventKeyRowCollapsed] is RowActionEventHandler rowActionEventHandler)
			{
				rowActionEventHandler(this, e);
			}
		}

		public void CheckAllRecords()
		{
			if (AcceptCellEdit() && _gridDataSourceManager.RootRow != null)
			{
				CheckedBehavior checkedBehavior = _gridDataSourceManager.CheckedBehavior;
				_gridDataSourceManager.CheckedBehavior = CheckedBehavior.CheckChildRows;
				_gridDataSourceManager.RootRow.IsChecked = true;
				_gridDataSourceManager.CheckedBehavior = checkedBehavior;
				RefreshRowCountAndInvalidate();
			}
		}

		public void UnCheckAllRecords()
		{
			if (AcceptCellEdit() && _gridDataSourceManager.RootRow != null)
			{
				CheckedBehavior checkedBehavior = _gridDataSourceManager.CheckedBehavior;
				_gridDataSourceManager.CheckedBehavior = CheckedBehavior.CheckChildRows;
				_gridDataSourceManager.RootRow.IsChecked = false;
				_gridDataSourceManager.CheckedBehavior = checkedBehavior;
				RefreshRowCountAndInvalidate();
			}
		}

		public GridRow[] GetCheckedRows()
		{
			ArrayList arrayList = _gridDataSourceManager.CollectCheckedRows();
			GridRow[] array = new GridRow[arrayList.Count];
			for (int i = 0; i < arrayList.Count; i++)
			{
				array[i] = GetGridRowForLogicalRow((MordexRow)arrayList[i]);
			}
			return array;
		}

		public GridRow[] GetUncheckedRows()
		{
			ArrayList arrayList = _gridDataSourceManager.CollectUncheckedRows();
			GridRow[] array = new GridRow[arrayList.Count];
			for (int i = 0; i < arrayList.Count; i++)
			{
				array[i] = GetGridRowForLogicalRow((MordexRow)arrayList[i]);
			}
			return array;
		}

		internal bool CanExpandRow(MordexRow mordexRow)
		{
			if (mordexRow is MordexRootRow)
			{
				return true;
			}
			GridRow row = GetGridRowForLogicalRow(mordexRow);
			RowActionCancelEventArgs e = new RowActionCancelEventArgs(row);
			OnExpandingRow(e);
			return !e.Cancel;
		}

		internal bool CanCollapseRow(MordexRow mordexRow)
		{
			if (mordexRow is MordexRootRow)
			{
				return true;
			}
			GridRow row = GetGridRowForLogicalRow(mordexRow);
			RowActionCancelEventArgs e = new RowActionCancelEventArgs(row);
			OnCollapsingRow(e);
			return !e.Cancel;
		}

		internal void NotifyRowExpanded(MordexRow mordexRow)
		{
			if (!(mordexRow is MordexRootRow))
			{
				GridRow row = GetGridRowForLogicalRow(mordexRow);
				RowActionEventArgs e = new RowActionEventArgs(row);
				OnRowExpanded(e);
				RefreshRowCountAndInvalidate();
			}
		}

		internal void NotifyRowCollapsed(MordexRow mordexRow)
		{
			if (!(mordexRow is MordexRootRow))
			{
				GridRow row = GetGridRowForLogicalRow(mordexRow);
				RowActionEventArgs e = new RowActionEventArgs(row);
				OnRowCollapsed(e);
			}
		}

		public void SelectCurrentCellText()
		{
			if (_previousColumnContainerControl != null)
			{
				if (_previousColumnContainerControl.ComputeTextBox() != null)
				{
					_previousColumnContainerControl.ComputeTextBox().SelectAll();
				}
				return;
			}
			if (EditMode != EditMode.EditOn)
			{
				BeginCellEdit(false, '\0', false);
			}
			if (_gridColumnContainerControl != null && EditTextBox != null)
			{
				EditTextBox.SelectAll();
			}
		}
	}
}
