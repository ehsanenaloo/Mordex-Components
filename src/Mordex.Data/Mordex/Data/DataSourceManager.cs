using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Globalization;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public partial class DataSourceManager
	{
		private bool _saveFieldValuesInCacheRow;

		private CheckedBehavior _checkedBehavior;

		private GroupMode _groupMode;

		private DataSourceManagerTableListHolderRow _rootListHolderRow;

		private MordexRootRow _mordexRootRow;

		private MordexRootRow _nonGroupedRootRow;

		private IMordexTable _rootTable;

		private BindableList _bindableRootList;

		private BoundListInspector _listInspector;

		private Hashtable _relationProvidersByName;

		private bool _dynamicFiltering;

		private MordexRowManager _mordexRowManager;

		private MordexRowManager _nonGroupedRowManager;

		private NewRowPosition _newRowPosition;

		private GroupTotals _groupTotalRows;

		private bool _allowAddNew;

		private bool _allowChildTableGroups;

		private bool _tableTotalRows;

		private bool _hasRowManager;

		private bool _hasNonGroupedView;

		private BoundMode _boundMode;

		private bool _useSampleRows;

		private List<MordexUnboundDataRow> _unboundRows;

		private bool _hierarchical;

		public bool UseSampleRows
		{
			get
			{
				return _useSampleRows;
			}
			set
			{
				if (_useSampleRows != value)
				{
					_useSampleRows = value;
					Refresh(refreshAll: true);
				}
			}
		}

		public BoundMode BoundMode
		{
			get
			{
				return _boundMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BoundMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_boundMode != value)
				{
					_boundMode = value;
					if (_boundMode == BoundMode.Bound)
					{
						RootList = null;
						return;
					}
					_unboundRows = new List<MordexUnboundDataRow>();
					RootList = _unboundRows;
				}
			}
		}

		public CheckedBehavior CheckedBehavior
		{
			get
			{
				return _checkedBehavior;
			}
			set
			{
				_checkedBehavior = value;
			}
		}

		public bool HasNonGroupedView
		{
			get
			{
				return _hasNonGroupedView;
			}
			set
			{
				if (_hasNonGroupedView == value)
				{
					return;
				}
				if (_nonGroupedRootRow != null)
				{
					_nonGroupedRootRow.Dispose();
					_nonGroupedRootRow = null;
				}
				_hasNonGroupedView = value;
				if (_hasNonGroupedView)
				{
					if (_rootTable != null)
					{
						_nonGroupedRootRow = new MordexRootRow(this, preventGroups: true);
					}
					_nonGroupedRowManager = new MordexRowManager(_nonGroupedRootRow, this, 0);
				}
			}
		}

		public virtual bool InvokeRequired => false;

		public bool SaveFieldValuesInCacheRow
		{
			get
			{
				return _saveFieldValuesInCacheRow;
			}
			set
			{
				_saveFieldValuesInCacheRow = value;
			}
		}

		public bool HasRowManager
		{
			get
			{
				return _hasRowManager;
			}
			set
			{
				if (_hasRowManager != value)
				{
					_hasRowManager = value;
					if (_hasRowManager)
					{
						_mordexRowManager = new MordexRowManager(_mordexRootRow, this, 0);
					}
					else
					{
						_mordexRowManager = null;
					}
				}
			}
		}

		public bool AllowChildTableGroups
		{
			get
			{
				return _allowChildTableGroups;
			}
			set
			{
				_allowChildTableGroups = value;
			}
		}

		public MordexRowManager RowManager => _mordexRowManager;

		public MordexRowManager NonGroupedRowManager => _nonGroupedRowManager;

		public bool DynamicFiltering
		{
			get
			{
				return _dynamicFiltering;
			}
			set
			{
				_dynamicFiltering = value;
			}
		}

		public GroupMode GroupMode
		{
			get
			{
				return _groupMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == GroupMode.Default)
				{
					value = GroupMode.Expanded;
				}
				_groupMode = value;
			}
		}

		public IMordexTable RootTable
		{
			get
			{
				return _rootTable;
			}
			set
			{
				if (_rootTable != value)
				{
					if (_rootTable != null)
					{
						_rootTable.DataSourceManager = null;
					}
					_rootTable = value;
					if (_rootTable != null)
					{
						_rootTable.DataSourceManager = this;
					}
					RefreshAll();
				}
			}
		}

		public BindableList BindableRootList
		{
			get
			{
				return _bindableRootList;
			}
			set
			{
				if (_bindableRootList != value)
				{
					_bindableRootList = value;
					if (RootList != null)
					{
						_listInspector = new BoundListInspector(RootList, this);
					}
					else
					{
						_listInspector = null;
					}
					Refresh(refreshAll: true);
				}
			}
		}

		public IList RootList
		{
			get
			{
				if (_bindableRootList != null)
				{
					return _bindableRootList.List;
				}
				return null;
			}
			set
			{
				if (RootList != value)
				{
					_bindableRootList = new BindableList(value);
					if (RootList != null)
					{
						_listInspector = new BoundListInspector(RootList, this);
					}
					else
					{
						_listInspector = null;
					}
					Refresh(refreshAll: true);
				}
			}
		}

		public MordexRootRow RootRow => _mordexRootRow;

		public MordexRow NonGroupedRootRow => _nonGroupedRootRow;

		public virtual CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

		public bool Hierarchical
		{
			get
			{
				return _hierarchical;
			}
			set
			{
				_hierarchical = value;
			}
		}

		public GroupTotals GroupTotalRows
		{
			get
			{
				return _groupTotalRows;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupTotals), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == GroupTotals.Default)
				{
					value = GroupTotals.Never;
				}
				if (value != _groupTotalRows)
				{
					_groupTotalRows = value;
					if (_mordexRowManager != null)
					{
						_mordexRowManager.Refresh();
						OnRowManagerChange();
					}
				}
			}
		}

		public NewRowPosition NewRowPosition
		{
			get
			{
				return _newRowPosition;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NewRowPosition), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == NewRowPosition.Default)
				{
					value = NewRowPosition.TopRow;
				}
				if (_newRowPosition != value)
				{
					_newRowPosition = value;
					RefreshRowManagers();
				}
			}
		}

		public bool AllowAddNew
		{
			get
			{
				return _allowAddNew;
			}
			set
			{
				if (_allowAddNew != value)
				{
					_allowAddNew = value;
					RefreshRowManagers();
				}
			}
		}

		public bool TableTotalRows
		{
			get
			{
				return _tableTotalRows;
			}
			set
			{
				if (_tableTotalRows != value)
				{
					_tableTotalRows = value;
					RefreshRowManagers();
				}
			}
		}

		public DataSourceManager()
		{
			Initialize();
			_boundMode = BoundMode.Bound;
			_newRowPosition = NewRowPosition.TopRow;
			_groupTotalRows = GroupTotals.Never;
			_groupMode = GroupMode.Expanded;
		}

		internal void RaiseBeforeReset()
		{
			OnBeforeReset();
		}

		protected virtual void OnBeforeReset()
		{
		}

		public virtual void Invoke(Delegate method, params object[] args)
		{
			method.DynamicInvoke(args);
		}

		internal void RefreshRowsForHolder(DataHolderRow holderRow)
		{
			if (_mordexRootRow != null && _mordexRootRow.ChildrenLoaded)
			{
				if (holderRow.Table.GetOwnerField() != null && holderRow.GetTableInterface().ParentTable != null && ((MordexTableBase)holderRow.GetTableInterface().ParentTable).IsSelfReferencingTable())
				{
					DataHolderRow dataHolderRow = holderRow.ParentRow.ParentRow as DataHolderRow;
					dataHolderRow.AddToLinkedRowCollections(holderRow);
				}
				else
				{
					_mordexRootRow.Rows.SyncDataHolderRow(holderRow);
				}
			}
			if (_nonGroupedRootRow != null)
			{
				_nonGroupedRootRow.Rows.SyncDataHolderRow(holderRow);
			}
		}

		internal void RaiseBeforeUpdateDataRow(object dataRow)
		{
			BeforeUpdateDataRowOverride(dataRow);
		}

		protected virtual void BeforeUpdateDataRowOverride(object dataRow)
		{
		}

		internal void RaiseReinsertDataRow(object dataRow)
		{
			OnReinsertDataRowOverride(dataRow);
		}

		protected virtual void OnReinsertDataRowOverride(object dataRow)
		{
		}

		protected virtual void Initialize()
		{
			HasRowManager = true;
			_saveFieldValuesInCacheRow = true;
		}

		internal void CollectDistinctValues(IMordexField iMordexField, IMordexValueItemCollection iMordexValueItemCollection)
		{
			if (_mordexRootRow != null)
			{
				_mordexRootRow.CollectDistinctValues(iMordexField, iMordexValueItemCollection);
			}
		}

		private void RefreshAll()
		{
			Refresh(refreshAll: true);
		}

		public void SuspendDataBinding()
		{
			if (_mordexRootRow != null)
			{
				_mordexRootRow.Dispose();
				_mordexRootRow = null;
			}
		}

		public void ResumeDataBinding()
		{
			RebuildRootRows();
		}

		public MordexDataRow AddItem()
		{
			return AddItem(-1);
		}

		public MordexDataRow AddItem(int index)
		{
			if (_boundMode != BoundMode.Unbound)
			{
				throw new InvalidOperationException();
			}
			if (_mordexRootRow != null)
			{
				MordexNewDataRow newRow = _mordexRootRow.NewRow;
				newRow.BeginEdit();
				object value = newRow._dataItem;
				SetDefaultValues(newRow);
				newRow.CommitNewRow(index);
				return (MordexDataRow)FindDataRow(value);
			}
			return null;
		}

		protected virtual void SetDefaultValues(MordexNewDataRow dataRow)
		{
		}

		public MordexDataRow AddItem(MordexDataRow parentRow, MordexTableBase table)
		{
			return AddItem(-1, parentRow, table);
		}

		public MordexDataRow AddItem(int index, MordexDataRow parentRow, MordexTableBase table)
		{
			if (parentRow == null)
			{
				if (table == null || table == _rootTable)
				{
					return AddItem(index);
				}
				throw new InvalidOperationException();
			}
			if (table == null)
			{
				if (parentRow.GetTableInterface().HierarchicalMode == HierarchicalMode.SelfReferencingChildList)
				{
					table = parentRow.Table;
				}
				else
				{
					if (parentRow.GetTableInterface().ChildTables.Count == 0)
					{
						throw new InvalidOperationException();
					}
					table = parentRow.GetTableInterface().ChildTables[0] as MordexTableBase;
				}
			}
			MordexTableRow mordexTableRow = null;
			foreach (MordexTableRow item in (IEnumerable)parentRow.Rows)
			{
				if (item.Table == table)
				{
					mordexTableRow = item;
					break;
				}
			}
			if (mordexTableRow.Rows != null)
			{
				return (MordexDataRow)mordexTableRow.CommitAndFindNewRow(index);
			}
			return null;
		}

		public void ClearItems()
		{
			if (_boundMode == BoundMode.Unbound)
			{
				RootList.Clear();
				Refresh(refreshAll: true);
				return;
			}
			throw new InvalidOperationException();
		}

		internal PropertyDescriptor FindPropertyDescriptor(string name, PropertyDescriptor[] propertyDescriptorArray)
		{
			if (_listInspector != null)
			{
				PropertyDescriptorCollection propertyDescriptorCollection = _listInspector.GetItemProperties(propertyDescriptorArray);
				return propertyDescriptorCollection.Find(name, ignoreCase: true);
			}
			return null;
		}

		internal PropertyDescriptorCollection GetPropertyDescriptors(PropertyDescriptor[] propertyDescriptorArray)
		{
			if (_listInspector != null)
			{
				return _listInspector.GetItemProperties(propertyDescriptorArray);
			}
			return null;
		}

		public void Refresh(bool refreshAll)
		{
			if (refreshAll)
			{
				if (_rootTable != null)
				{
					((MordexTableBase)_rootTable).RefreshFieldPropertyDescriptors();
				}
				if (_rootListHolderRow != null)
				{
					_rootListHolderRow.Dispose();
					_rootListHolderRow = null;
				}
				if (_rootTable != null)
				{
					if (_useSampleRows)
					{
						ArrayList arrayList = new ArrayList();
						arrayList.Add(new SampleRow());
						arrayList.Add(new SampleRow());
						arrayList.Add(new SampleRow());
						_rootListHolderRow = new DataSourceManagerTableListHolderRow(this, _rootTable);
						_rootListHolderRow.SetBindableList(new BindableList(arrayList));
					}
					else if (_bindableRootList != null)
					{
						_rootListHolderRow = new DataSourceManagerTableListHolderRow(this, _rootTable);
						_rootListHolderRow.SetBindableList(_bindableRootList);
					}
				}
				if (_relationProvidersByName != null)
				{
					foreach (RelationChildListProvider value in _relationProvidersByName.Values)
					{
						value.DisposeResources();
					}
					_relationProvidersByName.Clear();
					_relationProvidersByName = null;
				}
			}
			RebuildRootRows();
		}

		private void RebuildRootRows()
		{
			if (_mordexRootRow != null)
			{
				_mordexRootRow.Dispose();
				_mordexRootRow = null;
			}
			if (_nonGroupedRootRow != null)
			{
				_nonGroupedRootRow.Dispose();
				_nonGroupedRootRow = null;
			}
			if (_rootListHolderRow != null && _rootTable != null)
			{
				_mordexRootRow = new MordexRootRow(this, preventGroups: false);
				_mordexRootRow.LoadChildRows();
				if (_hasNonGroupedView)
				{
					_nonGroupedRootRow = new MordexRootRow(this, preventGroups: true);
					_nonGroupedRowManager = new MordexRowManager(_nonGroupedRootRow, this, 0);
					OnNonGroupRowManagerChange();
				}
			}
			if (_hasRowManager)
			{
				int suspendCount = 0;
				if (_mordexRowManager != null)
				{
					suspendCount = _mordexRowManager._indexSuspendCount;
					_mordexRowManager = new MordexRowManager(_mordexRootRow, this, suspendCount);
				}
			}
			if (_mordexRootRow != null)
			{
				OnRowChange(_mordexRootRow, RowChangeType.ChildRowReset);
			}
		}

		internal DataSourceManagerTableListHolderRow GetRootListHolderRow()
		{
			return _rootListHolderRow;
		}

		internal void AssignNewRowValue(MordexNewDataRow row, TableListHolderRow tableListHolderRow)
		{
			object newValue = null;
			newValue = ((_boundMode != BoundMode.Unbound || tableListHolderRow.IsCacheBacked()) ? CreateNewRow(row) : new MordexUnboundDataRow(null));
			if (newValue == null)
			{
				tableListHolderRow.AddNewToSource(row);
				return;
			}
			row._dataItem = newValue;
			row._mustAddToList = true;
		}

		protected virtual object CreateNewRow(MordexNewDataRow row)
		{
			return null;
		}

		internal string GetOutlookRangeName(OutlookDateRange range)
		{
			return GetNameOfOutlookRange(range);
		}

		protected virtual string GetNameOfOutlookRange(OutlookDateRange range)
		{
			return "";
		}

		public void CollapseDataRows(MordexTableBase table)
		{
			if (_mordexRootRow != null)
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.CollapseDataRows(table);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						_mordexRowManager.ResumeIndex();
					}
				}
			}
		}

		public void ExpandDataRows(MordexTableBase table)
		{
			if (_mordexRootRow != null)
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.ExpandDataRows(table);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						_mordexRowManager.ResumeIndex();
					}
				}
			}
		}

		public void LoadAllDataRows()
		{
			if (_rootListHolderRow != null)
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.SuspendIndex();
				}
				try
				{
					LoadRowsRecursively(_rootListHolderRow);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						_mordexRowManager.ResumeIndex();
					}
				}
			}
		}

		private void LoadRowsRecursively(MordexRow mordexRow)
		{
			if (mordexRow.Rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)mordexRow.Rows)
			{
				LoadRowsRecursively(item);
			}
		}

		protected virtual void GroupComputeStart()
		{
		}

		protected virtual void GroupComputeEnd()
		{
		}

		internal void RefreshTableGroups(MordexTableBase mordexTableBase)
		{
			if (_mordexRootRow != null)
			{
				_ = DateTime.Now.Ticks;
				GroupComputeStart();
				if (_mordexRowManager != null)
				{
					_mordexRowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.RefreshGroupRows(mordexTableBase);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						_mordexRowManager.ResumeIndex();
					}
				}
				GroupComputeEnd();
			}
		}

		internal void RefreshTableSort(MordexTableBase table)
		{
			long ticks = DateTime.Now.Ticks;
			if (_mordexRowManager != null)
			{
				_mordexRowManager.SuspendIndex();
			}
			try
			{
				if (_mordexRootRow != null)
				{
					_mordexRootRow.RefreshSort(table);
				}
			}
			finally
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.ResumeIndex();
				}
			}
			ticks = DateTime.Now.Ticks - ticks;
		}

		internal void RefreshRowManagers()
		{
			if (_mordexRowManager != null)
			{
				_mordexRowManager.Refresh();
				OnRowManagerChange();
			}
			if (_nonGroupedRowManager != null)
			{
				_nonGroupedRowManager.Refresh();
				OnNonGroupRowManagerChange();
			}
		}

		public void ExpandGroups()
		{
			if (_mordexRootRow != null)
			{
				if (RowManager != null)
				{
					RowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.ExpandGroupRows(null);
				}
				finally
				{
					if (RowManager != null)
					{
						RowManager.ResumeIndex();
					}
				}
			}
		}

		public void CollapseGroups()
		{
			if (_mordexRootRow != null)
			{
				if (RowManager != null)
				{
					RowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.CollapseGroupRows(null);
				}
				finally
				{
					if (RowManager != null)
					{
						RowManager.ResumeIndex();
					}
				}
			}
		}

		public void ExpandGroup(MordexGroupBase group)
		{
			if (_mordexRootRow != null)
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.ExpandGroupRows(group);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						_mordexRowManager.ResumeIndex();
					}
				}
			}
		}

		public void CollapseGroup(MordexGroupBase group)
		{
			if (_mordexRootRow != null)
			{
				if (_mordexRowManager != null)
				{
					RowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.CollapseGroupRows(group);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						RowManager.ResumeIndex();
					}
				}
			}
		}

		public void Dispose()
		{
			if (_relationProvidersByName != null)
			{
				foreach (RelationChildListProvider state in _relationProvidersByName.Values)
				{
					state.DisposeResources();
				}
				_relationProvidersByName.Clear();
				_relationProvidersByName = null;
			}
			if (_rootListHolderRow != null)
			{
				_rootListHolderRow.Dispose();
				_rootListHolderRow = null;
			}
			if (_mordexRootRow != null)
			{
				_mordexRootRow.Dispose();
				_mordexRootRow = null;
			}
			if (_nonGroupedRootRow != null)
			{
				_nonGroupedRootRow.Dispose();
				_nonGroupedRootRow = null;
			}
		}
	}
}
