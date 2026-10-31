using System;
using Mordex.Windows.DataGrid.Internal;
using System.Collections;
using System.Data;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class GridDataSourceManager : DataSourceManager
	{
		private MxDataGrid _owner;

		private VirtualDataRow _virtualDataRow;

		private DataMordexRow _dataMordexRow;

		internal bool _isUpdatingFromManager;

		private object _dataRow;

		public override bool InvokeRequired
		{
			get
			{
				if (_owner != null && _owner.IsHandleCreated && !_owner.IsDisposed)
				{
					return _owner.InvokeRequired;
				}
				return base.InvokeRequired;
			}
		}

		internal GridDataSourceManager(MxDataGrid owner)
		{
			_owner = owner;
			_virtualDataRow = new VirtualDataRow(this);
			_dataMordexRow = new DataMordexRow(this);
			base.HasRowManager = true;
		}

		protected override void OnBeforeReset()
		{
			base.OnBeforeReset();
			if (GetOwner() != null)
			{
				GetOwner().UpdateData();
				if (GetOwner().DataChanged)
				{
					GetOwner().CancelCurrentEdit();
				}
			}
		}

		internal MxDataGrid GetOwner()
		{
			return _owner;
		}

		protected override MordexTableBase CreateMordexTable()
		{
			return new GridTable();
		}

		protected override void SetDefaultValues(MordexNewDataRow dataRow)
		{
			base.SetDefaultValues(dataRow);
			if (dataRow == null || dataRow.Table == null)
			{
				return;
			}
			GridTable gridTable = (GridTable)dataRow.Table;
			foreach (GridColumn column in gridTable.Columns)
			{
				if (column.DefaultValue != null)
				{
					dataRow.SetValue(column, column.DefaultValue);
				}
			}
		}

		public override IMordexCondition CreateMordexCondition()
		{
			return new GridFilterCondition();
		}

		protected override MordexFieldBase CreateMordexField()
		{
			return new GridColumn();
		}

		internal VirtualDataRow GetVirtualDataRow()
		{
			return _virtualDataRow;
		}

		internal DataMordexRow GetDataMordexRow()
		{
			return _dataMordexRow;
		}

		internal int GetRootRecordCount()
		{
			if (base.RootRow == null)
			{
				return 0;
			}
			return base.RootRow.RecordCount;
		}

		internal MordexNewDataRow GetRootNewRow()
		{
			if (base.RootRow == null)
			{
				return null;
			}
			return base.RootRow.NewRow;
		}

		protected override bool IncludeTotalRowInRowManager()
		{
			if (_owner.View == View.TableView)
			{
				return _owner.TotalRowPosition == TotalRowPosition.BottomScrollable;
			}
			return false;
		}

		internal MordexTotalRow GetRootTotalRow()
		{
			if (base.RootRow == null)
			{
				return null;
			}
			return base.RootRow.TotalRow;
		}

		internal ArrayList ComputeArrayList(bool condition, bool otherCondition)
		{
			ArrayList arrayList = new ArrayList();
			if (base.RootRow != null)
			{
				ProcessArrayList(arrayList, base.RootRow.GetChildRows(), condition, (MordexTableBase)base.RootTable, otherCondition);
			}
			return arrayList;
		}

		internal ArrayList CollectRootChildRows(bool condition, bool otherCondition)
		{
			ArrayList arrayList = new ArrayList();
			if (base.RootRow != null)
			{
				ProcessArrayList(arrayList, base.RootRow.GetChildRows(), condition, null, otherCondition);
			}
			return arrayList;
		}

		private void ProcessArrayList(ArrayList arrayList, MordexRow[] mordexRowArray, bool condition, MordexTableBase mordexTableBase, bool otherCondition)
		{
			if (mordexRowArray == null || mordexRowArray.Length == 0)
			{
				return;
			}
			foreach (MordexRow mordexRow in mordexRowArray)
			{
				if (mordexRow.Visible && (mordexTableBase == null || mordexRow.Table == mordexTableBase))
				{
					if ((!otherCondition || RowStateHelper.GetRowTag(mordexRow).GetDataState3Boolean()) && !mordexRow.IsVirtualRow() && !(mordexRow is MordexNewDataRow) && mordexRow.Table is GridTable)
					{
						arrayList.Add(mordexRow);
					}
					if (mordexRow.Expanded || condition)
					{
						ProcessArrayList(arrayList, mordexRow.GetChildRows(), condition, mordexTableBase, otherCondition);
					}
					else if (mordexRow is MordexGroupRow mordexGroupRow && mordexGroupRow.Table is GridTable gridTable && gridTable.ComputeGroupTotals() == GroupTotals.Always)
					{
						arrayList.Add(mordexGroupRow.TotalRow);
					}
				}
			}
		}

		protected override MordexValueItemCollectionBase CreateValueList()
		{
			return new GridValueListItemCollection();
		}

		internal ArrayList CollectCheckedRows()
		{
			if (base.RootRow != null)
			{
				ArrayList arrayList = new ArrayList();
				AddCheckedRows(arrayList, base.RootRow);
				return arrayList;
			}
			return new ArrayList();
		}

		internal ArrayList CollectUncheckedRows()
		{
			if (base.RootRow != null)
			{
				ArrayList arrayList = new ArrayList();
				AddUncheckedRows(arrayList, base.RootRow);
				return arrayList;
			}
			return new ArrayList();
		}

		private void AddUncheckedRows(ArrayList arrayList, MordexRow mordexRow)
		{
			if (mordexRow is MordexDataRow && mordexRow.CheckedState == CheckedState.Unchecked)
			{
				arrayList.Add(mordexRow);
			}
			if (!mordexRow.ChildrenLoaded)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)mordexRow.Rows)
			{
				AddUncheckedRows(arrayList, item);
			}
		}

		private void AddCheckedRows(ArrayList arrayList, MordexRow mordexRow)
		{
			if (!mordexRow.Visible || !(mordexRow.Table is GridTable))
			{
				return;
			}
			if (mordexRow is MordexDataRow && mordexRow.CheckedState != CheckedState.Unchecked)
			{
				arrayList.Add(mordexRow);
			}
			if (!mordexRow.ChildrenLoaded)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)mordexRow.Rows)
			{
				AddCheckedRows(arrayList, item);
			}
		}

		protected override void OnRowManagerChange()
		{
			if (_owner != null)
			{
				_owner.NotifyAccessibilityReorder();
			}
		}

		public override void Invoke(Delegate method, params object[] methodParams)
		{
			if (_owner != null)
			{
				try
				{
					_owner.Invoke(method, methodParams);
					return;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					return;
				}
			}
			base.Invoke(method, methodParams);
		}

		protected override void OnRowChange(MordexRow row, RowChangeType change)
		{
			if (_owner != null)
			{
				HandleRowChange(row, change);
			}
		}

		private void HandleRowChange(MordexRow mordexRow, RowChangeType rowChangeType)
		{
			switch (rowChangeType)
			{
			case RowChangeType.Add:
				_owner.RefreshRowCountAndInvalidate();
				break;
			case RowChangeType.Delete:
				_owner.MarkDescendantRowsDirty(mordexRow);
				break;
			case RowChangeType.Change:
				_owner.MarkGroupRowsDirty(mordexRow);
				break;
			case RowChangeType.ChildRowReset:
				if (!_isUpdatingFromManager)
				{
					_owner.RebuildRows(false);
				}
				break;
			case RowChangeType.TotalChanged:
				_owner.RefreshRowAndTotalRow(mordexRow);
				break;
			}
			_owner.HandleRowChange(mordexRow, rowChangeType);
		}

		protected override void BeforeUpdateDataRowOverride(object dataRow)
		{
			base.BeforeUpdateDataRowOverride(dataRow);
			if (_owner.CurrentRow != null && _owner.CurrentRow.DataRow == dataRow)
			{
				_dataRow = dataRow;
			}
		}

		protected override void OnReinsertDataRowOverride(object dataRow)
		{
			base.OnReinsertDataRowOverride(dataRow);
			if (_dataRow != null && _dataRow == dataRow)
			{
				_owner.MoveTo(_owner.GetRow(dataRow));
			}
			_dataRow = null;
		}

		public override void SourceListReset(object parentDataRow, MordexTableBase table)
		{
			bool condition = _owner.Row != -3;
			base.SourceListReset(parentDataRow, table);
			_owner.RebuildRows(true);
			if (_owner.Row == -3 && condition && !base.RowManager.IsIndexSuspend && _owner.RowCount > 0)
			{
				_owner.Row = 0;
			}
		}

		protected override DataTable GetDataTableCore(IList list)
		{
			if (list is BindingSource bindingSource && bindingSource.List is DataView dataView)
			{
				return dataView.Table;
			}
			return base.GetDataTableCore(list);
		}

		internal void ResetRowFormatStyles()
		{
			ResetRowFormatStylesRecursive(base.RootRow.Rows);
		}

		private void ResetRowFormatStylesRecursive(MordexRowCollection mordexRowCollection)
		{
			foreach (MordexRow item in (IEnumerable)mordexRowCollection)
			{
				if (item is MordexDataRow mordexDataRow && mordexDataRow.DataHolder.Tag is RowTag dataState)
				{
					dataState.SetFormatStyleState(null);
				}
				if (item.ChildrenLoaded)
				{
					ResetRowFormatStylesRecursive(item.Rows);
				}
			}
		}

		protected override object CreateNewRow(MordexNewDataRow row)
		{
			return _owner.RaiseGetNewRow(row);
		}

		internal MordexRow GetRowByIndex(int value)
		{
			switch (value)
			{
			case -2:
				return GetVirtualDataRow();
			case -1:
				return GetRootNewRow();
			default:
				if (value == base.RowManager.Count)
				{
					if (!IncludeTotalRowInRowManager() && (base.RootTable.TotalRow == Mordex.Data.InheritableBoolean.True || (base.RootTable.TotalRow == Mordex.Data.InheritableBoolean.Default && base.TableTotalRows)))
					{
						return GetRootTotalRow();
					}
					throw new ArgumentOutOfRangeException();
				}
				if (value >= 0)
				{
					return base.RowManager[value];
				}
				return null;
			}
		}

		protected override void OnFetchExternalData(MordexRow row)
		{
			if (row.ExternalData == null)
			{
				row.ExternalData = _owner.GetOrCreateGridRow(row, false);
			}
		}

		protected override void OnReleaseExternalData(MordexRow row)
		{
			row.ExternalData = null;
		}

		protected override string GetNameOfOutlookRange(OutlookDateRange range)
		{
			GridBuiltInTextList builtInTexts = _owner.BuiltInTexts;
			switch (range)
			{
			case OutlookDateRange.BeyondNextMonth:
				return builtInTexts[GridBuiltInText.OutlookDateBeyondNextMonth];
			case OutlookDateRange.EarlierThisMonth:
				return builtInTexts[GridBuiltInText.OutlookDateEarlierThisMonth];
			case OutlookDateRange.LastMonth:
				return builtInTexts[GridBuiltInText.OutlookDateLastMonth];
			case OutlookDateRange.LastWeek:
				return builtInTexts[GridBuiltInText.OutlookDateLastWeek];
			case OutlookDateRange.LaterThisMonth:
				return builtInTexts[GridBuiltInText.OutlookDateLaterThisMonth];
			case OutlookDateRange.NextMonth:
				return builtInTexts[GridBuiltInText.OutlookDateNextMonth];
			case OutlookDateRange.NextWeek:
				return builtInTexts[GridBuiltInText.OutlookDateNextWeek];
			case OutlookDateRange.NullDate:
				return builtInTexts[GridBuiltInText.OutlookDateEmpty];
			case OutlookDateRange.Older:
				return builtInTexts[GridBuiltInText.OutlookDateOlder];
			case OutlookDateRange.ThreeWeeksAgo:
				return builtInTexts[GridBuiltInText.OutlookDateThreeWeeksAgo];
			case OutlookDateRange.ThreeWeeksAway:
				return builtInTexts[GridBuiltInText.OutlookDateThreeWeeksAway];
			case OutlookDateRange.Today:
				return builtInTexts[GridBuiltInText.OutlookDateToday];
			case OutlookDateRange.Tomorrow:
				return builtInTexts[GridBuiltInText.OutlookDateTomorrow];
			case OutlookDateRange.TwoWeeksAgo:
				return builtInTexts[GridBuiltInText.OutlookDateTwoWeeksAgo];
			case OutlookDateRange.TwoWeeksAway:
				return builtInTexts[GridBuiltInText.OutlookDateTwoWeeksAway];
			case OutlookDateRange.Yesterday:
				return builtInTexts[GridBuiltInText.OutlookDateYesterday];
			default:
				return "";
			}
		}

		protected override bool OnCollapsingRow(MordexRow row)
		{
			return _owner.CanCollapseRow(row);
		}

		protected override bool OnExpandingRow(MordexRow row)
		{
			return _owner.CanExpandRow(row);
		}

		protected override IList GetCustomChildList(MordexRow row, MordexTableBase childTable)
		{
			return _owner.ComputeIList(row, (GridTable)childTable);
		}

		protected override void OnRowExpanded(MordexRow row)
		{
			_owner.NotifyRowExpanded(row);
		}

		protected override void OnRowCollapsed(MordexRow row)
		{
			_owner.NotifyRowCollapsed(row);
		}

		protected override void OnSuspendRowManagerIndex()
		{
			_owner.SaveCurrentRowBeforeSuspend();
		}

		protected override void OnResumeRowManagerIndex()
		{
			_owner.RestoreCurrentRowAfterResume();
		}

		protected override void GroupComputeStart()
		{
			_owner.OnGroupComputeStarted();
		}

		protected override void GroupComputeEnd()
		{
			_owner.OnGroupComputeEnded();
		}
	}
}
