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
		public GridLayout FindLayout(string key)
		{
			return FindLayoutByKey(_layouts, key);
		}

		private GridFormatStyle GetFilterRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = GetRowStyle(gridTable);
			GridFormatStyle applyOverrides = null;
			applyOverrides = gridFormatStyle.ApplyOverrides(_filterRowFormatStyle);
			gridFormatStyle.DisposeResources();
			return applyOverrides;
		}

		internal GridFormatStyle GetTotalRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = GetRowStyle(gridTable);
			GridFormatStyle applyOverrides = null;
			applyOverrides = gridFormatStyle.ApplyOverrides(_totalRowFormatStyle);
			if (gridTable != null && gridTable.TotalRowFormatStyle != null)
			{
				GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridTable.TotalRowFormatStyle);
				if (otherApplyOverrides != applyOverrides)
				{
					applyOverrides.DisposeResources();
					applyOverrides = otherApplyOverrides;
				}
			}
			gridFormatStyle.DisposeResources();
			return applyOverrides;
		}

		internal GridFormatStyle GetGroupTotalRowStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = GetGroupRowStyle(gridTable);
			GridFormatStyle otherGridFormatStyle = gridFormatStyle;
			otherGridFormatStyle = gridFormatStyle.ApplyOverrides(_groupTotalRowFormatStyle);
			if (gridTable != null && gridTable.GroupTotalRowFormatStyle != null)
			{
				GridFormatStyle applyOverrides = otherGridFormatStyle.ApplyOverrides(gridTable.GroupTotalRowFormatStyle);
				otherGridFormatStyle.DisposeResources();
				otherGridFormatStyle = applyOverrides;
			}
			gridFormatStyle.DisposeResources();
			return otherGridFormatStyle;
		}

		internal void RefreshAggregateRows()
		{
			_gridDataSourceManager.RowManager.Refresh();
			foreach (GridRow item in _rowCache)
			{
				if (item.RowType == RowType.GroupFooter || item.RowType == RowType.TotalRow)
				{
					item._isLayoutDirty = true;
				}
			}
			_rowCache.ReleaseStaleRows();
			if (_row != null && (_row.RowType == RowType.GroupFooter || _row.RowType == RowType.TotalRow))
			{
				int row = Row;
				SetCurrentGridRow(null, true);
				SetCurrentGridRow(GetRow(row), true);
			}
			RefreshRowCountAndInvalidate();
		}

		public GridRow GetTotalRow()
		{
			MordexTableRow rootRow = _gridDataSourceManager.RootRow;
			if (rootRow != null)
			{
				MordexRow totalRow = rootRow.TotalRow;
				if (totalRow != null)
				{
					return GetGridRowForLogicalRow(totalRow);
				}
			}
			return null;
		}

		internal GridRow GetFilterGridRow()
		{
			if (_gridRow == null && FilterMode != FilterMode.None && _gridDataSourceManager.GetVirtualDataRow() != null && _gridDataSourceManager.GetVirtualDataRow().Table != null)
			{
				_gridRow = new GridRow(_gridDataSourceManager.GetVirtualDataRow(), this);
			}
			return _gridRow;
		}

		private MordexRow FindNextRowOfTable(int value, GridTable gridTable)
		{
			if (value < 0)
			{
				value = (IsNewRowAtTop() ? (-1) : 0);
			}
			if (value > RowCount - 1)
			{
				value = RowCount - 1;
			}
			MordexRowManager mordexRowManager = GetRowManager();
			for (int i = value + 1; i < RowCount; i++)
			{
				if (mordexRowManager[i].Table == gridTable)
				{
					return mordexRowManager[i];
				}
			}
			return null;
		}

		private MordexRow FindPreviousRowOfTable(int value, GridTable gridTable)
		{
			if (value < 0)
			{
				value = (IsNewRowAtTop() ? (-1) : 0);
			}
			else if (value == 0)
			{
				return null;
			}
			MordexRowManager mordexRowManager = GetRowManager();
			for (int i = value - 1; i >= 0; i--)
			{
				if (mordexRowManager[i].Table == gridTable)
				{
					return mordexRowManager[i];
				}
			}
			return null;
		}

		private int GetTotalRowPosition()
		{
			if (RootTable != null)
			{
				if (RootTable.TotalRow == InheritableBoolean.True)
				{
					return RowCount;
				}
				if (RootTable.TotalRow == InheritableBoolean.Default && TotalRow == InheritableBoolean.True)
				{
					return RowCount;
				}
			}
			return -3;
		}

		private GridGroupLayoutState FindCellLayoutItem(GridRow gridRow, GridColumn gridColumn)
		{
			if (gridRow == null || gridColumn == null)
			{
				return null;
			}
			if (_view == View.TableView)
			{
				LayoutItemType layoutItemType = LayoutItemType.Cell;
				if (_headerRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _headerRowLayouts)
					{
						if (item.GridRow == null || item.GridRow.GetLogicalRow() != gridRow.GetLogicalRow())
						{
							continue;
						}
						foreach (GridGroupLayoutState arrayListItem in item._arrayList)
						{
							if (arrayListItem._column == gridColumn && arrayListItem._type == layoutItemType)
							{
								return arrayListItem;
							}
						}
					}
				}
				if (_bodyRowLayouts != null)
				{
					foreach (MxRowLayoutState bodyRowLayout in _bodyRowLayouts)
					{
						if (bodyRowLayout.GridRow == null || bodyRowLayout.GridRow.GetLogicalRow() != gridRow.GetLogicalRow())
						{
							continue;
						}
						foreach (GridGroupLayoutState otherArrayListItem in bodyRowLayout._arrayList)
						{
							if (otherArrayListItem._column == gridColumn && otherArrayListItem._type == LayoutItemType.Cell)
							{
								return otherArrayListItem;
							}
						}
					}
				}
			}
			else if (_cardRowLayouts != null)
			{
				foreach (GridTableLayoutState cardRowLayout in _cardRowLayouts)
				{
					if (cardRowLayout._gridRow != gridRow)
					{
						continue;
					}
					foreach (GridGroupLayoutState nextArrayListItem in cardRowLayout._arrayList)
					{
						if (nextArrayListItem._column == gridColumn && nextArrayListItem._type == LayoutItemType.CardCell)
						{
							return nextArrayListItem;
						}
					}
				}
			}
			return null;
		}

		private GridGroupLayoutState FindFilterCellLayoutItem(GridRow gridRow, GridColumn gridColumn)
		{
			if (gridRow == null || gridColumn == null)
			{
				return null;
			}
			if (_view == View.TableView && _headerRowLayouts != null)
			{
				foreach (MxRowLayoutState item in _headerRowLayouts)
				{
					if (item._gridRow != gridRow)
					{
						continue;
					}
					foreach (GridGroupLayoutState arrayListItem in item._arrayList)
					{
						if (arrayListItem._column == gridColumn && arrayListItem._type == LayoutItemType.FilterRowCell)
						{
							return arrayListItem;
						}
					}
				}
			}
			return null;
		}

		private void ReapplyFilterRowKeepingCaret()
		{
			if (_filterRowUpdateMode == FilterRowUpdateMode.WhenReturnKeyIsPressed)
			{
				return;
			}
			_gridGridColumn = CurrentColumn;
			int value = -1;
			int selectionLength = -1;
			if (EditTextBox != null)
			{
				value = EditTextBox.SelectionStart;
				selectionLength = EditTextBox.SelectionLength;
			}
			ApplyFilterRow(false);
			CurrentColumn = _gridGridColumn;
			_gridGridColumn = null;
			if (Row == -2)
			{
				RepositionCellEditor();
				if (value >= 0 && EditTextBox != null)
				{
					EditTextBox.SelectionStart = value;
					EditTextBox.SelectionLength = selectionLength;
				}
			}
		}

		private void ClearFilterRowColumn(GridColumn column, GridRow gridRow)
		{
			AcceptCellEdit();
			Row = -2;
			if (column.FilterRowComparison == ConditionOperator.NotIsEmpty || column.FilterRowComparison == ConditionOperator.IsEmpty)
			{
				gridRow.Cells[column].DataChanged = true;
			}
			column.ResetFilterRowComparison();
			_row.Cells[column].AssignValue(null, true);
			ApplyFilterRow(true);
			try
			{
				Row = -2;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			if (gridRow != null)
			{
				OnClearFilterButtonClick(new ColumnActionEventArgs(column));
			}
		}

		private GridValueListItemCollection BuildFilterOperatorList(GridColumn gridColumn)
		{
			GridValueListItemCollection gridValueListItemCollection = new GridValueListItemCollection();
			gridValueListItemCollection.Add(DBNull.Value, _builtInTexts[GridBuiltInText.FilterRowConditionClear], GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterClearImage(), Color.Magenta));
			gridValueListItemCollection.Add(ConditionOperator.Equal, _builtInTexts[GridBuiltInText.FilterRowConditionEqual]);
			gridValueListItemCollection.Add(ConditionOperator.NotEqual, _builtInTexts[GridBuiltInText.FilterRowConditionNotEqual]);
			if (gridColumn.ColumnType != ColumnType.CheckBox && gridColumn.DataTypeCode != TypeCode.Boolean)
			{
				if (gridColumn.CompareTarget == ColumnCompareTarget.Text || gridColumn.DataTypeCode == TypeCode.String)
				{
					gridValueListItemCollection.Add(ConditionOperator.BeginsWith, _builtInTexts[GridBuiltInText.FilterRowConditionBeginsWith]);
					gridValueListItemCollection.Add(ConditionOperator.EndsWith, _builtInTexts[GridBuiltInText.FilterRowConditionEndsWith]);
					gridValueListItemCollection.Add(ConditionOperator.Contains, _builtInTexts[GridBuiltInText.FilterRowConditionContains]);
					if (gridColumn.FilterRowComparison == ConditionOperator.NotContains)
					{
						gridValueListItemCollection.Add(ConditionOperator.NotContains, _builtInTexts[GridBuiltInText.FilterRowConditionNotContains]);
					}
				}
				else
				{
					gridValueListItemCollection.Add(ConditionOperator.GreaterThan, _builtInTexts[GridBuiltInText.FilterRowConditionGreaterThan]);
					gridValueListItemCollection.Add(ConditionOperator.GreaterThanOrEqualTo, _builtInTexts[GridBuiltInText.FilterRowConditionGreaterThanOrEqual]);
					gridValueListItemCollection.Add(ConditionOperator.LessThan, _builtInTexts[GridBuiltInText.FilterRowConditionLessThan]);
					gridValueListItemCollection.Add(ConditionOperator.LessThanOrEqualTo, _builtInTexts[GridBuiltInText.FilterRowConditionLessThanOrEqual]);
				}
			}
			gridValueListItemCollection.Add(ConditionOperator.IsEmpty, _builtInTexts[GridBuiltInText.FilterRowConditionIsEmpty]);
			gridValueListItemCollection.Add(ConditionOperator.NotIsEmpty, _builtInTexts[GridBuiltInText.FilterRowConditionNotIsEmpty]);
			foreach (GridValueListItem item in (IEnumerable)gridValueListItemCollection)
			{
				if (item.Value != DBNull.Value)
				{
					item.Image = GridGroupLayoutState.GetFilterOperatorImage((ConditionOperator)item.Value);
				}
			}
			return gridValueListItemCollection;
		}

		private void StopFilterRowTimer()
		{
			if (_filterRowDelayTimer != null)
			{
				_filterRowDelayTimer.Stop();
				_filterRowDelayTimer.Tick -= OnFilterRowTimerTick;
				_filterRowDelayTimer.Dispose();
				_filterRowDelayTimer = null;
			}
		}

		private void StartFilterRowTimer()
		{
			if (_filterRowDelayTimer == null)
			{
				_filterRowDelayTimer = new System.Windows.Forms.Timer();
				_filterRowDelayTimer.Interval = FilterRowUpdateDelay;
				_filterRowDelayTimer.Tick += OnFilterRowTimerTick;
				_filterRowDelayTimer.Start();
			}
		}

		private void OnFilterRowTimerTick(object sender, EventArgs eventArgs)
		{
			ReapplyFilterRowKeepingCaret();
		}

		internal void RefreshRowAndTotalRow(MordexRow otherMordexRow)
		{
			MordexRow mordexRow = null;
			if (otherMordexRow is MordexTableRow)
			{
				mordexRow = ((MordexTableRow)otherMordexRow).TotalRow;
			}
			else if (otherMordexRow is MordexGroupRow)
			{
				MarkRowDirty(otherMordexRow);
				mordexRow = ((MordexGroupRow)otherMordexRow).TotalRow;
			}
			if (mordexRow != null)
			{
				MarkRowDirty(mordexRow);
			}
		}

		public void RemoveFilters()
		{
			_gridRow = null;
			if (RootTable != null)
			{
				RootTable.ClearFilters();
			}
		}

		private bool ApplyFilterRow(bool otherCondition)
		{
			StopFilterRowTimer();
			if (!_isPreviewTextCached && _row != null && _row.RowType == RowType.FilterRow && _row.HasChangedCells())
			{
				if (FilterRowUpdateMode == FilterRowUpdateMode.WhenReturnKeyIsPressed && !otherCondition)
				{
					return false;
				}
				_isPreviewTextCached = true;
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnApplyingFilter(e);
				_isPreviewTextCached = false;
				if (e.Cancel)
				{
					return false;
				}
			}
			if (_filterMode == FilterMode.Automatic && _row != null && _row.RowType == RowType.FilterRow && _row.HasChangedCells())
			{
				GridFilterCondition gridFilterCondition = new GridFilterCondition();
				foreach (GridCell item in (IEnumerable)_gridRow.Cells)
				{
					object value = GetValue(item.Column);
					string text = ((item.Column != CurrentColumn) ? item.Text : ((EditTextBox != null) ? EditTextBox.Text : ((item.Column.CompareTarget != ColumnCompareTarget.Value) ? item.Text : item.Column.GetDisplayText(value, true))));
					if ((GridHelper.IsNullOrDBNull(value) || string.IsNullOrEmpty(text)) && item.Column.FilterRowComparison != ConditionOperator.NotIsEmpty && item.Column.FilterRowComparison != ConditionOperator.IsEmpty)
					{
						continue;
					}
					bool nextCondition = false;
					if (value is Array)
					{
						nextCondition = true;
					}
					else if (item.Column.CompareTarget == ColumnCompareTarget.Text)
					{
						value = text;
					}
					string otherText = value as string;
					if (nextCondition)
					{
						if (item.Column.CompareTarget == ColumnCompareTarget.Text)
						{
							string[] array = item.Text.Split(',');
							for (int i = 0; i < array.Length; i++)
							{
								array[i] = array[i].Trim();
							}
							value = array;
						}
						GridFilterCondition condition = new GridFilterCondition(item.Column, ConditionOperator.In, value);
						gridFilterCondition.AddCondition(LogicalOperator.And, condition);
					}
					else if (otherText != null)
					{
						if (otherText == "*")
						{
							otherText = "**";
						}
						if (otherText.StartsWith("*") && otherText.EndsWith("*"))
						{
							otherText = otherText.Substring(1, otherText.Length - 2);
							GridFilterCondition condition = new GridFilterCondition(item.Column, ConditionOperator.Contains, otherText);
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
						else if (otherText.EndsWith("*"))
						{
							GridFilterCondition condition = new GridFilterCondition(item.Column, ConditionOperator.BeginsWith, otherText.Substring(0, otherText.Length - 1));
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
						else if (otherText.StartsWith("*"))
						{
							otherText = otherText.Substring(1);
							GridFilterCondition condition = new GridFilterCondition(item.Column, ConditionOperator.EndsWith, otherText);
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
						else
						{
							GridFilterCondition condition = new GridFilterCondition(item.Column, item.Column.FilterRowComparison, otherText);
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
					}
					else if (value is DateTime dateTime && dateTime.TimeOfDay.TotalMilliseconds == 0.0)
					{
						if (item.Column.FilterRowComparison == ConditionOperator.Equal)
						{
							GridFilterCondition condition = new GridFilterCondition(item.Column, ConditionOperator.Between, value, dateTime.AddTicks(TimeSpan.TicksPerDay - 1));
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
						else if (item.Column.FilterRowComparison == ConditionOperator.NotEqual)
						{
							GridFilterCondition condition = new GridFilterCondition(item.Column, ConditionOperator.NotBetween, value, dateTime.AddTicks(TimeSpan.TicksPerDay - 1));
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
						else
						{
							GridFilterCondition condition = new GridFilterCondition(item.Column, item.Column.FilterRowComparison, value);
							gridFilterCondition.AddCondition(LogicalOperator.And, condition);
						}
					}
					else
					{
						GridFilterCondition condition = new GridFilterCondition(item.Column, item.Column.FilterRowComparison, value);
						gridFilterCondition.AddCondition(LogicalOperator.And, condition);
					}
				}
				if (_gridRow != null)
				{
					foreach (GridCell cell in (IEnumerable)_gridRow.Cells)
					{
						if (cell.DataChanged)
						{
							cell.SetDataChanged(false);
						}
					}
					_gridRow.SetDataChangedFlag(false);
				}
				GridColumn currentColumn = _currentColumn;
				_isRowPreviewDirty = true;
				if (gridFilterCondition.Conditions.Count > 0)
				{
					_showRowPreview = false;
					RootTable.ApplyFilter(gridFilterCondition);
					if (_row != null && _row != _gridRow)
					{
						_row.CancelEdit();
					}
					SetCurrentGridRow(_gridRow, true);
					FirstRow = 0;
					Row = -2;
					CalculateLayoutIfReady();
				}
				else
				{
					_showRowPreview = true;
					RootTable.RemoveFilter();
					FirstRow = 0;
				}
				_isRowPreviewDirty = false;
				CurrentColumn = currentColumn;
				OnFilterApplied(EventArgs.Empty);
			}
			else if (FilterMode == FilterMode.Manual && _gridRow != null && _gridRow.HasChangedCells() && _gridRow != null)
			{
				foreach (GridCell otherCell in (IEnumerable)_gridRow.Cells)
				{
					if (otherCell.DataChanged)
					{
						otherCell.SetDataChanged(false);
					}
				}
				_gridRow.SetDataChangedFlag(false);
			}
			return true;
		}

		internal string GetFilterRowInfoText()
		{
			return _builtInTexts[GridBuiltInText.FilterRowInfoText];
		}

		internal void SetFilterRowInfoText(string text)
		{
			if (GetFilterRowInfoText() != text)
			{
				_builtInTexts[GridBuiltInText.FilterRowInfoText] = text;
			}
		}

		public bool Find(GridColumn column, ConditionOperator conditionOperator, object value, int start, int direction)
		{
			return Find(column, conditionOperator, value, null, start, direction);
		}

		public bool Find(GridColumn column, ConditionOperator conditionOperator, object value1, object value2, int start, int direction)
		{
			if (column == null || column.DataGrid != this)
			{
				throw new ArgumentException("parameter can not be null", "column");
			}
			GridFilterCondition condition = new GridFilterCondition(column, conditionOperator, value1, value2);
			return Find(condition, start, direction);
		}

		public bool Find(GridFilterCondition condition, int start, int direction)
		{
			if (direction != -1 && direction != 1)
			{
				throw new ArgumentOutOfRangeException("direction", direction, "direction must be either 1 or -1");
			}
			if ((start > RowCount || start < 0) && start != -1)
			{
				throw new ArgumentException("'start' parameter must be between 0 and RowCount or must be equal to -1 to find the first record", "start");
			}
			MordexRow rowManagerItem = null;
			if (start != -1)
			{
				rowManagerItem = GetRowManager()[start];
			}
			MordexRow mordexRow = _gridDataSourceManager.FindRow(condition, rowManagerItem, direction, includeFilteredRows: false);
			if (mordexRow != null)
			{
				mordexRow.EnsureVisible();
				MoveTo(GetGridRowForLogicalRow(mordexRow));
				return true;
			}
			return false;
		}

		public int FindAll(GridColumn column, ConditionOperator conditionOperator, object value)
		{
			return FindAll(column, conditionOperator, value, null);
		}

		public int FindAll(GridColumn column, ConditionOperator conditionOperator, object value1, object value2)
		{
			if (column == null || column.DataGrid != this)
			{
				throw new ArgumentException("parameter can not be null", "column");
			}
			GridFilterCondition condition = new GridFilterCondition(column, conditionOperator, value1, value2);
			return FindAll(condition);
		}

		public int FindAll(GridFilterCondition condition)
		{
			MordexRow[] array = _gridDataSourceManager.FindAll(condition);
			if (array.Length == 0)
			{
				return 0;
			}
			List<MordexRow> list = new List<MordexRow>();
			MordexRow[] otherArray = array;
			foreach (MordexRow mordexRow in otherArray)
			{
				mordexRow.EnsureVisible();
				if (mordexRow.RowManagerPosition >= 0)
				{
					list.Add(mordexRow);
				}
			}
			if (list.Count > 0)
			{
				Row = list[0].RowManagerPosition;
				if (SelectionMode != SelectionMode.SingleSelection)
				{
					for (int j = 1; j < list.Count; j++)
					{
						SelectedItems.Add(list[j].RowManagerPosition);
					}
				}
				return SelectedItems.Count;
			}
			Row = -3;
			return 0;
		}

		protected virtual void OnApplyingFilter(CancelEventArgs e)
		{
			if (base.Events[_eventKeyApplyingFilter] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnFilterApplied(EventArgs e)
		{
			if (base.Events[_eventKeyFilterApplied] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		private GridColumn GetIncrementalSearchColumn()
		{
			if (RootTable != null && _incrementalSearchMode != IncrementalSearchMode.None)
			{
				if (_searchColumn != null)
				{
					return _searchColumn;
				}
				if (RootTable.SortKeys.Count > 0)
				{
					return RootTable.SortKeys[0].Column;
				}
			}
			return null;
		}

		private bool TryIncrementalSearch(char incrementalSearchText)
		{
			GridColumn gridColumn = GetIncrementalSearchColumn();
			if (gridColumn == null || RowCount == 0)
			{
				return false;
			}
			if (_incrementalSearchMode == IncrementalSearchMode.FirstCharacter)
			{
				_incrementalSearchText = string.Concat(incrementalSearchText);
				int row = Row;
				if (row < 0)
				{
					row = -1;
				}
				if (!Find(gridColumn, ConditionOperator.BeginsWith, _incrementalSearchText, row, 1))
				{
					Find(gridColumn, ConditionOperator.BeginsWith, _incrementalSearchText, -1, 1);
				}
			}
			else
			{
				if (_incrementalSearchTimer != null)
				{
					_incrementalSearchTimer.Stop();
				}
				if (_incrementalSearchTimer == null)
				{
					_incrementalSearchTimer = new System.Windows.Forms.Timer();
					_incrementalSearchTimer.Interval = Timing.IncrementalSearchTimeout;
					_incrementalSearchTimer.Tick += OnIncrementalSearchTimerTick;
				}
				_incrementalSearchText += incrementalSearchText;
				if ((RootTable.Groups.Count == 0 && RootTable.SortKeys.Count > 0 && RootTable.SortKeys[0].Column == gridColumn) ? true : false)
				{
					if (!Find(gridColumn, ConditionOperator.BeginsWith, _incrementalSearchText, -1, 1))
					{
						_incrementalSearchText = "";
					}
				}
				else
				{
					int otherRow = Row;
					if (otherRow < 0)
					{
						otherRow = -1;
					}
					if (!Find(gridColumn, ConditionOperator.BeginsWith, _incrementalSearchText, otherRow, 1) && !Find(gridColumn, ConditionOperator.BeginsWith, _incrementalSearchText, -1, 1))
					{
						_incrementalSearchText = "";
					}
				}
				if (_incrementalSearchTimer != null)
				{
					if (_incrementalSearchText.Length == 0)
					{
						_incrementalSearchTimer.Dispose();
						_incrementalSearchTimer = null;
					}
					else
					{
						_incrementalSearchTimer.Start();
					}
				}
			}
			return true;
		}

		private void OnIncrementalSearchTimerTick(object sender, EventArgs eventArgs)
		{
			_incrementalSearchText = "";
			_incrementalSearchTimer.Stop();
			_incrementalSearchTimer.Dispose();
			_incrementalSearchTimer = null;
		}

		public object GetTotal(GridColumn column, AggregateFunction aggregateFunction)
		{
			return GetTotal(column, aggregateFunction, null);
		}

		public object GetTotal(GridColumn column, AggregateFunction aggregateFunction, GridFilterCondition filterCondition)
		{
			if (_gridDataSourceManager.RootRow != null)
			{
				return _gridDataSourceManager.RootRow.GetTotal(column, DataState.ToDataAggregateFunction(aggregateFunction), filterCondition);
			}
			return null;
		}

		private GridRow FindVisibleRowForDataRow(object dataRow)
		{
			MordexDataRow[] rowsOfDataRow = _gridDataSourceManager.GetRowsOfDataRow(dataRow);
			if (rowsOfDataRow != null)
			{
				MordexDataRow mordexDataRow = null;
				MordexDataRow[] array = rowsOfDataRow;
				foreach (MordexDataRow arrayItem in array)
				{
					if (arrayItem.IsRowVisible())
					{
						if (mordexDataRow == null)
						{
							mordexDataRow = arrayItem;
						}
						else if (mordexDataRow.RowManagerPosition > arrayItem.RowManagerPosition)
						{
							mordexDataRow = arrayItem;
						}
					}
				}
				if (mordexDataRow != null)
				{
					return GetGridRowForLogicalRow(mordexDataRow);
				}
			}
			return null;
		}

		internal void ResetFilterRowCache(GridTable gridTable)
		{
			if (!_isRowPreviewDirty)
			{
				if (gridTable == RootTable && _filterMode == FilterMode.Automatic)
				{
					_gridRow = null;
				}
				RefreshRowCountAndInvalidate();
			}
		}
	}
}
