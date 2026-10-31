using System.Collections;
using System.ComponentModel;

namespace Mordex.Data
{
	public class MordexTableRow : MordexRow
	{
		private bool _recordPositionsValid;

		private MordexNewDataRow _newRow;

		private MordexTotalRow _totalRow;

		private MordexTableBase _table;

		internal bool _preventGroups;

		private TableListHolderRow _tableListHolderRow;

		private MordexNewDataRow _bottomExtraNewRow;

		internal bool _isRefreshingFilter;

		private int _recordCount;

		protected override bool DefaultExpandedValue => true;

		public override MordexTableBase Table => _table;

		protected virtual TableListHolderRow BaseList
		{
			get
			{
				if (ParentRow is MordexDataRow)
				{
					MordexDataRow mordexDataRow = (MordexDataRow)ParentRow;
					return mordexDataRow.DataHolder.FindTableList(_table);
				}
				return null;
			}
		}

		public MordexNewDataRow NewRow => _newRow;

		public MordexTotalRow TotalRow
		{
			get
			{
				if (Table.ShowsTotalRow())
				{
					if (_totalRow == null)
					{
						_totalRow = new MordexTotalRow(this);
					}
					return _totalRow;
				}
				return null;
			}
		}

		public override MordexNewRowPosition NewRowPosition
		{
			get
			{
				if (Table.AllowsAddNew())
				{
					NewRowPosition newRowPosition = Table.GetEffectiveNewRowPosition();
					if (newRowPosition == Mordex.Data.NewRowPosition.BottomRow)
					{
						return MordexNewRowPosition.Bottom;
					}
					return MordexNewRowPosition.Top;
				}
				return MordexNewRowPosition.None;
			}
		}

		// The number of data rows of the table that are loaded and visible. It is kept by the row changes that arrive one at a
		// time (a row added, removed, shown or hidden) and counted again after any other change to the rows, so it is right
		// at once and costs nothing per added row. Record positions are not touched: they are assigned when a row asks.
		public int RecordCount
		{
			get
			{
				if (Table.IsSelfReferencingTable())
				{
					AssignRecordPositions(false);
					return _recordCount;
				}
				if (!_liveCountValid)
				{
					_liveCount = GetLoadedDataRows(Table).Length;
					_liveCountValid = true;
				}
				return _liveCount;
			}
		}

		private int _liveCount;

		private bool _liveCountValid;

		private static MordexTableRow OwningTableRow(MordexRow row)
		{
			MordexRow node = row.ParentRow;
			while (node != null && !(node is MordexTableRow))
			{
				node = node.ParentRow;
			}
			return (MordexTableRow)node;
		}

		private bool CountsAsRecord(MordexRow row)
		{
			for (MordexRow node = row; node != this; node = node.ParentRow)
			{
				if (node == null || !node.Visible)
				{
					return false;
				}
			}
			return true;
		}

		private void UpdateLiveCount(MordexRow row, RowChangeType change)
		{
			if (!_liveCountValid)
			{
				return;
			}
			if (change == RowChangeType.Change || change == RowChangeType.Move || change == RowChangeType.ExpandedChanged || change == RowChangeType.TotalChanged)
			{
				return;
			}
			// The new row template is not in the rows, and rows of a child table never count for this table.
			if (row is MordexNewDataRow || (row is MordexDataRow && OwningTableRow(row) != this) || (row is MordexTableRow && row != this))
			{
				return;
			}
			if (!(row is MordexDataRow))
			{
				// A group or the table itself: its rows were not seen one by one.
				_liveCountValid = false;
				return;
			}
			switch (change)
			{
			case RowChangeType.Add:
				_liveCount += CountsAsRecord(row) ? 1 : 0;
				break;
			case RowChangeType.BeforeDelete:
				// Still in its place, so the walk up works; the Delete that follows has nothing left to do.
				_liveCount -= CountsAsRecord(row) ? 1 : 0;
				break;
			case RowChangeType.VisibleChanged:
				if (row.ParentRow != null && CountsAsRecord(row.ParentRow))
				{
					_liveCount += row.Visible ? 1 : -1;
				}
				break;
			case RowChangeType.Delete:
				break;
			default:
				_liveCountValid = false;
				break;
			}
		}
		protected override bool IsTreeRow => true;

		protected override bool LoadIsChecked => true;

		internal MordexTableRow(MordexTableBase table, bool preventGroups)
		{
			_preventGroups = preventGroups;
			_newRow = new MordexNewDataRow(this);
			_table = table;
			if (_table.HasOwnerField())
			{
				base.Expanded = false;
			}
			base.SetShouldRetrieveChildRows(true);
		}

		public override bool CanHaveChildren()
		{
			return true;
		}

		protected override MordexRowCollection CreateChildRows()
		{
			MordexRowCollection mordexRowCollection;
			if (Table.IsSelfReferencingTable() && ParentRow != null && ParentRow.Table == Table)
			{
				mordexRowCollection = new MordexRowCollection(this, RowCollectionType.DataRows);
			}
			else if (!_preventGroups && GetTableInterface().Groups.Count > 0)
			{
				mordexRowCollection = ((GetTableInterface().Groups[0].CustomGroup != null && GetTableInterface().Groups[0].CustomGroup.CustomGroupType != CustomGroupType.CompositeColumns) ? new MordexRowCollection(this, RowCollectionType.CustomGroupRows) : new MordexRowCollection(this, RowCollectionType.GroupRows));
				mordexRowCollection.Group = (MordexGroupBase)GetTableInterface().Groups[0];
			}
			else
			{
				mordexRowCollection = new MordexRowCollection(this, RowCollectionType.DataRows);
			}
			return mordexRowCollection;
		}

		internal TableListHolderRow GetTableListHolderRow()
		{
			return _tableListHolderRow;
		}

		protected override void RetrieveChildRows()
		{
			_tableListHolderRow = BaseList;
			MordexRowCollection rows = base.Rows;
			if (_tableListHolderRow == null || rows == null)
			{
				return;
			}
			rows.BeginLoad();
			try
			{
				foreach (DataHolderRow item in (IEnumerable)_tableListHolderRow.Rows)
				{
					rows.SyncDataHolderRow(item);
				}
			}
			finally
			{
				rows.EndLoad();
			}
			_recordPositionsValid = false;
			_liveCountValid = false;
			base.UpdateVisibility();
		}

		protected override int CompareTo(MordexRow row)
		{
			if (row is MordexTableRow mordexTableRow)
			{
				return _table.Index.CompareTo(mordexTableRow._table.Index);
			}
			return -1;
		}

		public override bool IsVirtualRow()
		{
			return true;
		}

		protected override void RefreshGroupsCore(MordexTableBase table)
		{
			if (table == _table)
			{
				foreach (MordexGroupBase group in GetTableInterface().Groups)
				{
					group.CaptureComparisonSnapshot();
				}
				RefreshChildRows();
				{
					foreach (MordexGroupBase group2 in GetTableInterface().Groups)
					{
						group2.ReleaseComparisonSnapshot();
					}
					return;
				}
			}
			base.RefreshGroupsCore(table);
		}

		internal void BeginAddNewRow(MordexNewDataRow mordexNewDataRow)
		{
			if (DataSourceManager != null)
			{
				DataSourceManager.AssignNewRowValue(mordexNewDataRow, _tableListHolderRow);
			}
		}

		internal void CommitNewDataRow(MordexNewDataRow mordexNewDataRow)
		{
			CommitNewDataRow(mordexNewDataRow, -1);
		}

		internal int CommitNewDataRow(MordexNewDataRow mordexNewDataRow, int integer)
		{
			int result = -1;
			if (_tableListHolderRow == null)
			{
				if (ParentRow is MordexDataRow mordexDataRow)
				{
					DataHolderRow dataHolder = mordexDataRow.DataHolder;
					if (dataHolder != null)
					{
						TableListHolderRow tableListHolderRow = dataHolder.FindTableList(Table);
						result = tableListHolderRow.CommitNewRow(mordexNewDataRow, integer);
					}
				}
			}
			else
			{
				result = _tableListHolderRow.CommitNewRow(mordexNewDataRow, integer);
			}
			_newRow = new MordexNewDataRow(this);
			_bottomExtraNewRow = null;
			RaiseRowChange(this, RowChangeType.ChildRowReset);
			return result;
		}

		internal MordexRow CommitAndFindNewRow(int integer)
		{
			MordexNewDataRow newRow = NewRow;
			newRow.BeginEdit();
			object value = newRow._dataItem;
			newRow.CommitNewRow(integer);
			MordexRow[] dataRows = DataSourceManager.RootRow.CollectDataRowsToArray(Table, false, true, true);
			if (dataRows != null)
			{
				MordexRow[] rows = dataRows;
				foreach (MordexRow mordexRow in rows)
				{
					if (mordexRow.DataRow == value)
					{
						return mordexRow;
					}
				}
			}
			return null;
		}

		internal void CancelNewDataRow(MordexNewDataRow mordexNewDataRow, bool boolean)
		{
			object dataRow = mordexNewDataRow.DataRow;
			if (boolean && dataRow is IEditableObject)
			{
				((IEditableObject)dataRow).CancelEdit();
			}
			_tableListHolderRow.CancelAddNew(mordexNewDataRow);
			_newRow = new MordexNewDataRow(this);
			_bottomExtraNewRow = null;
			RaiseRowChange(this, RowChangeType.ChildRowReset);
		}

		internal void DiscardNewRowItem(MordexNewDataRow mordexNewDataRow)
		{
			object dataItem = mordexNewDataRow._dataItem;
			bool isInSource = !mordexNewDataRow._mustAddToList;
			mordexNewDataRow._dataItem = null;
			mordexNewDataRow._mustAddToList = false;
			if (isInSource && _tableListHolderRow != null)
			{
				try
				{
					_tableListHolderRow.DiscardNewItem(dataItem);
				}
				catch
				{
					// The item is still in the source, so the row keeps it and the next try can cancel it.
					mordexNewDataRow._dataItem = dataItem;
					throw;
				}
			}
		}

		internal void AddBottomExtraNewRow()
		{
			if (NewRowPosition == MordexNewRowPosition.Bottom)
			{
				_bottomExtraNewRow = new MordexNewDataRow(this);
				RaiseRowChange(_bottomExtraNewRow, RowChangeType.Add);
			}
		}

		protected override object GetRowKey()
		{
			return _table;
		}

		public override void ExpandDataRows(MordexTableBase table)
		{
			base.ExpandDataRows(table);
			if (Table != null && Table.IsSelfReferencingTable())
			{
				_recordPositionsValid = false;
			}
		}

		protected override void OnRowChange(MordexRow row, RowChangeType change)
		{
			base.OnRowChange(row, change);
			UpdateLiveCount(row, change);
			if (row.ParentRow == this)
			{
				switch (change)
				{
				case RowChangeType.ExpandedChanged:
					if (Table != null && Table.IsSelfReferencingTable())
					{
						_recordPositionsValid = false;
					}
					break;
				case RowChangeType.Delete:
				case RowChangeType.Add:
				case RowChangeType.ChildRowReset:
				case RowChangeType.VisibleChanged:
					base.UpdateVisibility();
					break;
				}
			}
			if (row is MordexDataRow)
			{
				switch (change)
				{
				case RowChangeType.Delete:
				case RowChangeType.Add:
				case RowChangeType.Move:
				case RowChangeType.ChildRowReset:
				case RowChangeType.VisibleChanged:
					_recordPositionsValid = false;
					break;
				}
			}
			if (change != RowChangeType.ExpandedChanged && !IsLoading() && row != this && row != _totalRow)
			{
				NotifyTotalRowChanged();
			}
		}

		protected override void OnEvaluateVisibility()
		{
			if (GetTableInterface().ParentTable == null)
			{
				return;
			}
			if (!GetTableInterface().IsFiltered)
			{
				base.Visible = true;
			}
			else
			{
				if (!Table.ShouldFilterParentRows() || !ChildrenLoaded)
				{
					return;
				}
				foreach (MordexRow item in (IEnumerable)base.Rows)
				{
					if (item.Visible)
					{
						base.Visible = true;
						return;
					}
				}
				if (NewRowPosition == MordexNewRowPosition.None)
				{
					base.Visible = false;
				}
				else
				{
					base.Visible = true;
				}
			}
		}

		public override void RefreshFilter(MordexTableBase table)
		{
			bool wasRefreshingFilter = _isRefreshingFilter;
			_isRefreshingFilter = true;
			try
			{
				base.RefreshFilter(table);
				if (table == Table)
				{
					AssignRecordPositions(true);
				}
			}
			finally
			{
				_isRefreshingFilter = wasRefreshingFilter;
			}
		}

		private void NotifyTotalRowChanged()
		{
			if (_totalRow != null)
			{
				_totalRow.RaiseRowChange(_totalRow, RowChangeType.Change);
			}
		}

		internal override void RefreshSort(MordexTableBase table)
		{
			if (table == _table)
			{
				_recordPositionsValid = false;
			}
			base.RefreshSort(table);
		}

		internal void AssignRecordPositions(bool boolean)
		{
			if (Table != null && Table.IsSelfReferencingTable() && ParentRow != null && ParentRow.Table == Table)
			{
				MordexRow parentRow = ParentRow;
				while (parentRow != null && !(parentRow is MordexTableRow))
				{
					parentRow = parentRow.ParentRow;
				}
				if (parentRow != null)
				{
					MordexTableRow mordexTableRow = (MordexTableRow)parentRow;
					if (!mordexTableRow._isRefreshingFilter)
					{
						mordexTableRow.AssignRecordPositions(boolean);
						return;
					}
				}
			}
			if (!_recordPositionsValid || boolean)
			{
				MordexRow[] loadedDataRows = GetLoadedDataRows(Table);
				for (int i = 0; i < loadedDataRows.Length; i++)
				{
					((MordexDataRow)loadedDataRows[i]).SetRecordPosition(i);
				}
				_recordCount = loadedDataRows.Length;
				_recordPositionsValid = true;
			}
		}

		protected override MordexRow[] GetPreListRows()
		{
			if (Table.IsSelfReferencingTable() && ParentRow != null)
			{
				return null;
			}
			OnEvaluateVisibility();
			if (NewRowPosition == MordexNewRowPosition.Top && NewRow != null)
			{
				return new MordexRow[1] { NewRow };
			}
			return null;
		}

		protected override MordexRow[] GetPostListRows()
		{
			if (Table.IsSelfReferencingTable() && ParentRow != null && ParentRow.Table == Table)
			{
				return null;
			}
			bool hasNewRow = false;
			bool hasExtraNewRow = false;
			bool hasTotalRow = false;
			int rowCount = 0;
			if (NewRowPosition == MordexNewRowPosition.Bottom && NewRow != null && GetTableInterface().Groups.Count == 0)
			{
				hasNewRow = true;
				rowCount++;
				if (_bottomExtraNewRow != null)
				{
					rowCount++;
					hasExtraNewRow = true;
				}
			}
			if (TotalRow != null && (Table != DataSourceManager.RootTable || DataSourceManager.ShouldIncludeTotalRowInRowManager()) && (Table == DataSourceManager.RootTable || base.Rows.Count != 0))
			{
				rowCount++;
				hasTotalRow = true;
			}
			if (rowCount == 0)
			{
				return null;
			}
			MordexRow[] postRows = new MordexRow[rowCount];
			int position = 0;
			if (hasNewRow)
			{
				postRows[position] = NewRow;
				position++;
			}
			if (hasExtraNewRow)
			{
				postRows[position] = _bottomExtraNewRow;
				position++;
			}
			if (hasTotalRow)
			{
				postRows[position] = TotalRow;
				position++;
			}
			return postRows;
		}

		protected override bool CheckedDependsOnChild()
		{
			return true;
		}

		internal int GetSelfReferencingDepth()
		{
			MordexRow mordexRow = this;
			int depth = -1;
			while (mordexRow != null)
			{
				if (mordexRow is MordexTableRow && mordexRow.Table == Table)
				{
					depth++;
				}
				mordexRow = mordexRow.ParentRow;
			}
			return depth;
		}

		protected override void LoadTreeCacheRowsCore(DataSourceManager manager, MordexRowCache cacheRow, object[] dataRowReferences)
		{
			int holderIndex = cacheRow.GetReferenceIndex();
			if (holderIndex >= 0)
			{
				_tableListHolderRow = (TableListHolderRow)dataRowReferences[holderIndex];
			}
			base.LoadTreeCacheRowsCore(manager, cacheRow, dataRowReferences);
			_recordPositionsValid = false;
		}
	}
}
