using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace Mordex.Data
{
	partial class MordexTableBase
	{
		protected virtual MordexSelfReferencingSettingsBase CreateSelfReferencingSettings()
		{
			return MordexSelfReferencingSettingsBase.CreateForTable(this);
		}

		protected virtual MordexGroupCollectionBase CreateGroupsCollection()
		{
			return MordexGroupCollectionBase.CreateForOwner(this);
		}

		protected virtual MordexFieldCollectionBase CreateFieldCollection()
		{
			return MordexFieldCollectionBase.CreateForOwner(this);
		}

		internal GroupMode GetEffectiveGroupMode()
		{
			if (_groupMode == GroupMode.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().GroupMode;
				}
				return GroupMode.Expanded;
			}
			return _groupMode;
		}

		protected void BaseResetCollections()
		{
			if (_fieldCollection != null)
			{
				((MordexFieldCollectionBase)_fieldCollection).DetachOwner();
			}
			_fieldCollection = CreateFieldCollection();
			if (_sortFieldCollection != null)
			{
				((MordexSortFieldCollectionBase)_sortFieldCollection).DetachOwner();
			}
			_sortFieldCollection = CreateSortFieldCollection();
			if (_groupCollection != null)
			{
				((MordexGroupCollectionBase)_groupCollection).DetachOwner();
			}
			_groupCollection = CreateGroupsCollection();
			if (_childTableCollection != null)
			{
				((MordexChildTableCollectionBase)_childTableCollection).DetachOwner();
			}
			_childTableCollection = CreateChildTableCollection();
			if (_storedFilterCollection != null)
			{
				((MordexStoredFilterCollectionBase)_storedFilterCollection).DetachOwner();
			}
			if (_groupTotalCollection != null)
			{
				((MordexGroupTotalCollectionBase)_groupTotalCollection).DetachOwner();
			}
			_groupTotalCollection = CreateGroupTotalCollection();
			_storedFilterCollection = CreateStoredFiltersCollection();
		}

		internal IList GetChildListOfRow(object parentRow)
		{
			return GetChildList(parentRow);
		}

		protected virtual IList GetChildList(object parentRow)
		{
			if (parentRow is MordexRowCache && _ownerField != null)
			{
				int index = ((MordexFieldBase)_ownerField).Index;
				MordexRowCache mordexRowCache = parentRow as MordexRowCache;
				if (mordexRowCache.FieldValues != null && index < mordexRowCache.FieldValues.Length)
				{
					return mordexRowCache.GetFieldValue(index) as IList;
				}
			}
			if (parentRow != null)
			{
				object childValue = DataValueHelper.GetPropertyValue(parentRow, null, _dataMember);
				if (childValue is IListSource)
				{
					IListSource listSource = childValue as IListSource;
					if (!listSource.ContainsListCollection)
					{
						return ((IListSource)childValue).GetList();
					}
					return childValue as IList;
				}
				return childValue as IList;
			}
			return null;
		}

		internal NewRowPosition GetEffectiveNewRowPosition()
		{
			if (_newRowPosition == NewRowPosition.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().NewRowPosition;
				}
				return NewRowPosition.TopRow;
			}
			return _newRowPosition;
		}

		protected virtual void OnFilterConditionChanged()
		{
			RaiseFilterConditionChanged(EventArgs.Empty);
		}

		internal bool ShouldFilterParentRows()
		{
			if ((_isApplyingFilter || _filterCondition != null) && _filterParentRows)
			{
				return true;
			}
			if (_childTableCollection != null)
			{
				foreach (MordexTableBase item in _childTableCollection)
				{
					if (item.ShouldFilterParentRows())
					{
						return true;
					}
				}
			}
			return false;
		}

		internal MordexFieldBase FindBoundFieldByDataMember(string strA)
		{
			if (!string.IsNullOrEmpty(strA))
			{
				foreach (IMordexField item in _fieldCollection)
				{
					if (item.BoundMode == ColumnBoundMode.Bound && string.Compare(strA, item.DataMember, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						return item as MordexFieldBase;
					}
				}
			}
			return null;
		}

		internal MordexFieldBase FindFieldByDataMember(string strA, bool boolean)
		{
			if (!string.IsNullOrEmpty(strA))
			{
				foreach (IMordexField item in _fieldCollection)
				{
					if ((item.BoundMode == ColumnBoundMode.Bound || boolean) && string.Compare(strA, item.DataMember, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						return item as MordexFieldBase;
					}
				}
			}
			return null;
		}

		internal void RemoveFieldReferences(MordexFieldBase field)
		{
			IMordexGroupCollection mordexGroupCollection = _groupCollection;
			for (int groupIndex = mordexGroupCollection.Count - 1; groupIndex >= 0; groupIndex--)
			{
				IMordexGroup mordexGroup = mordexGroupCollection[groupIndex];
				if (field == null || mordexGroup.Field == field)
				{
					mordexGroupCollection.RemoveAt(groupIndex);
				}
			}
			for (int sortIndex = _sortFieldCollection.Count - 1; sortIndex >= 0; sortIndex--)
			{
				IMordexSortField mordexSortField = _sortFieldCollection[sortIndex];
				if (field == null || mordexSortField.Field == field)
				{
					_sortFieldCollection.RemoveAt(sortIndex);
				}
			}
			for (int totalIndex = _groupTotalCollection.Count - 1; totalIndex >= 0; totalIndex--)
			{
				IMordexGroupTotal mordexGroupTotal = _groupTotalCollection[totalIndex];
				if (field == null || mordexGroupTotal.Field == field)
				{
					_groupTotalCollection.RemoveAt(totalIndex);
				}
			}
			OnFieldRemoved(field);
			RaiseColumnsChanged(EventArgs.Empty);
		}

		internal void NotifyFieldAdded(MordexFieldBase field)
		{
			OnFieldAdded(field);
			RaiseColumnsChanged(EventArgs.Empty);
		}

		protected virtual void OnFieldRemoved(MordexFieldBase field)
		{
		}

		protected virtual void OnFieldAdded(MordexFieldBase field)
		{
		}

		protected virtual CollectionChangingAction OnSortSettingsChanging(CollectionChangeType change, MordexSortFieldBase sortField)
		{
			return CollectionChangingAction.Continue;
		}

		protected void BaseSuspendSort()
		{
			_isSortSuspended = true;
		}

		protected void BaseResumeSort()
		{
			_isSortSuspended = false;
			BaseRefreshSort();
		}

		protected virtual void OnSortSettingsChanged(CollectionChangeType change, MordexSortFieldBase sortField)
		{
			if (!_isSortSuspended)
			{
				BaseRefreshSort();
			}
		}

		protected void BaseRefreshSort()
		{
			if (GetDataSourceManager() != null)
			{
				GetDataSourceManager().RefreshTableSort(this);
			}
		}

		protected virtual CollectionChangingAction OnGroupSettingsChanging(CollectionChangeType change, MordexGroupBase group)
		{
			return CollectionChangingAction.Continue;
		}

		protected virtual void OnGroupSettingsChanged(CollectionChangeType change, MordexGroupBase group)
		{
			if (GetDataSourceManager() != null)
			{
				GetDataSourceManager().RefreshTableGroups(this);
			}
		}

		protected virtual void OnChildTablesChanged(CollectionChangeType change, MordexTableBase table)
		{
			if (change == CollectionChangeType.Insert)
			{
				table.SetDataSourceManager(GetDataSourceManager());
			}
		}

		void ICollectionOwner.OnCollectionChanged(object collection, CollectionChangeType change, object item)
		{
			if (!_suppressCollectionEvents)
			{
				if (collection == _sortFieldCollection)
				{
					OnSortSettingsChanged(change, (MordexSortFieldBase)item);
				}
				else if (collection == _groupCollection)
				{
					OnGroupSettingsChanged(change, (MordexGroupBase)item);
				}
				else if (collection == _childTableCollection)
				{
					OnChildTablesChanged(change, (MordexTableBase)item);
				}
			}
		}

		CollectionChangingAction ICollectionOwner.OnCollectionChanging(MordexCollectionBase collection, CollectionChangeType change, object item)
		{
			if (collection == _sortFieldCollection)
			{
				return OnSortSettingsChanging(change, (MordexSortFieldBase)item);
			}
			if (collection == _groupCollection)
			{
				return OnGroupSettingsChanging(change, (MordexGroupBase)item);
			}
			return CollectionChangingAction.Continue;
		}

		internal bool CheckCanGroupByField(MordexFieldBase field)
		{
			return CanGroupByField(field);
		}

		protected bool CanGroupByField(MordexFieldBase field)
		{
			if (((IMordexField)field).Table == this)
			{
				if (((IMordexField)field).MultipleValues)
				{
					if (IsSelfReferencingTable() && ((IMordexSelfReferencingSettings)_selfReferencingSettings).HierarchicalGroupMode == HierarchicalGroupMode.ParentRows)
					{
						return false;
					}
					foreach (IMordexGroup baseGroup in BaseGroups)
					{
						if (baseGroup.Field != null && baseGroup.Field.MultipleValues)
						{
							return false;
						}
					}
				}
				return true;
			}
			if (AllowsChildTableGroups())
			{
				if (!IsAncestorOf((MordexTableBase)((IMordexField)field).Table))
				{
					return false;
				}
				for (MordexTableBase mordexTableBase = (MordexTableBase)((IMordexField)field).Table; mordexTableBase != BaseParentTable; mordexTableBase = mordexTableBase.BaseParentTable)
				{
					if (mordexTableBase.IsSelfReferencingTable())
					{
						return false;
					}
				}
				foreach (IMordexGroup item in _groupCollection)
				{
					IMordexField[] groupFields = item.Field != null ? new IMordexField[1] { item.Field } : item.Fields;
					if (groupFields == null)
					{
						continue;
					}
					foreach (IMordexField groupField in groupFields)
					{
						MordexTableBase groupTable = (MordexTableBase)groupField.Table;
						MordexTableBase candidateTable = (MordexTableBase)((IMordexField)field).Table;
						if (groupTable != candidateTable && !groupTable.IsAncestorOf(candidateTable) && !candidateTable.IsAncestorOf(groupTable))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		internal bool CanUseGroup(IMordexGroup iMordexGroup)
		{
			if (iMordexGroup.Fields != null && iMordexGroup.Fields.Length > 1)
			{
				for (int i = 0; i < iMordexGroup.Fields.Length; i++)
				{
					if (!CanGroupByField((MordexFieldBase)iMordexGroup.Fields[i]))
					{
						return false;
					}
				}
				return true;
			}
			if (iMordexGroup.Field == null)
			{
				if (iMordexGroup.CustomGroup == null)
				{
					return false;
				}
				return true;
			}
			return CanGroupByField((MordexFieldBase)iMordexGroup.Field);
		}

		internal bool IsAncestorOf(MordexTableBase mordexTableBase)
		{
			if (mordexTableBase == null)
			{
				return false;
			}
			for (mordexTableBase = mordexTableBase.BaseParentTable; mordexTableBase != null; mordexTableBase = mordexTableBase.BaseParentTable)
			{
				if (mordexTableBase == this)
				{
					return true;
				}
			}
			return false;
		}

		internal DataSourceManager GetDataSourceManager()
		{
			if (_ownerField != null)
			{
				return _ownerField.Table.DataSourceManager;
			}
			return _dataSourceManager;
		}

		internal void SetDataSourceManager(DataSourceManager dataSourceManager)
		{
			if (_dataSourceManager == dataSourceManager)
			{
				return;
			}
			_dataSourceManager = dataSourceManager;
			foreach (IMordexTable item in _childTableCollection)
			{
				item.DataSourceManager = dataSourceManager;
			}
		}

		internal PropertyDescriptor FindPropertyDescriptor(string text)
		{
			if (GetDataSourceManager() != null)
			{
				return GetDataSourceManager().FindPropertyDescriptor(text, GetPropertyPath());
			}
			return null;
		}

		internal PropertyDescriptor[] GetPropertyPath()
		{
			if (_dataSourceManager != null && _dataSourceManager.RootTable == this)
			{
				return null;
			}
			if (BaseParentTable != null && _dataMember != null)
			{
				PropertyDescriptor[] parentPath = BaseParentTable.GetPropertyPath();
				if (parentPath != null)
				{
					PropertyDescriptor[] path = new PropertyDescriptor[parentPath.Length + 1];
					parentPath.CopyTo(path, 0);
					path[path.Length - 1] = PropertyDescriptor;
					return path;
				}
				if (PropertyDescriptor != null)
				{
					return new PropertyDescriptor[1] { PropertyDescriptor };
				}
				return null;
			}
			return null;
		}

		private bool HasFilterInTree()
		{
			if (_filterCondition != null)
			{
				return true;
			}
			foreach (MordexTableBase item in _childTableCollection)
			{
				if (item.HasFilterInTree() && item._filterParentRows)
				{
					return true;
				}
			}
			return false;
		}

		private bool IsMultipleValuesTable()
		{
			return _ownerField != null;
		}

		private bool HasChildLevels()
		{
			if (_hierarchicalMode != HierarchicalMode.UseChildTables)
			{
				return true;
			}
			if (_childTableCollection.Count > 0)
			{
				return true;
			}
			foreach (IMordexField item in _fieldCollection)
			{
				if (item.MultipleValues)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsGrouped()
		{
			if (_groupCollection.Count > 0)
			{
				return true;
			}
			for (IMordexTable mordexTable = BaseParentTable; mordexTable != null; mordexTable = mordexTable.ParentTable)
			{
				foreach (IMordexGroup group in mordexTable.Groups)
				{
					IMordexField field = group.Field;
					if (field != null && field.Table == this)
					{
						return true;
					}
				}
			}
			return false;
		}

		internal GroupTotals GetEffectiveGroupTotals()
		{
			if (_groupTotalRows == GroupTotals.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().GroupTotalRows;
				}
				return GroupTotals.Never;
			}
			return _groupTotalRows;
		}

		internal void RefreshFieldPropertyDescriptors()
		{
			PropertyDescriptorCollection propertyDescriptorCollection = null;
			if (GetDataSourceManager() != null)
			{
				propertyDescriptorCollection = GetDataSourceManager().GetPropertyDescriptors(GetPropertyPath());
			}
			if (propertyDescriptorCollection != null)
			{
				foreach (IMordexField item in _fieldCollection)
				{
					MordexFieldBase mordexFieldBase = item as MordexFieldBase;
					if (item.BoundMode == ColumnBoundMode.Bound && item.DataMember.Length > 0)
					{
						mordexFieldBase.SetPropertyDescriptor(propertyDescriptorCollection.Find(item.DataMember, ignoreCase: true));
					}
					else
					{
						mordexFieldBase.SetPropertyDescriptor(null);
					}
				}
			}
			else
			{
				foreach (MordexFieldBase item2 in _fieldCollection)
				{
					item2.SetPropertyDescriptor(null);
				}
			}
			if (_childTableCollection == null)
			{
				return;
			}
			foreach (MordexTableBase item3 in _childTableCollection)
			{
				item3.RefreshFieldPropertyDescriptors();
			}
		}

		internal bool IsSelfReferencingTable()
		{
			return BaseIsSelfReferencingTable;
		}

		internal bool ActsAsSelfReferencingTable()
		{
			return BaseActAsSelfReferencingTable;
		}

		internal void OnSelfReferencingSettingChanged(string text)
		{
			if (text == "HierarchicalGroupMode")
			{
				if (_groupCollection.Count > 0 && GetDataSourceManager() != null)
				{
					GetDataSourceManager().RefreshTableGroups(this);
				}
			}
			else if (GetDataSourceManager() != null)
			{
				GetDataSourceManager().Refresh(refreshAll: true);
			}
		}

		void IMordexTable.Initialize()
		{
			Initialize("");
		}

		void IMordexTable.Initialize(string dataMember)
		{
			Initialize(dataMember);
		}

		IMordexCondition IMordexTable.CreateCondition()
		{
			return CreateCondition();
		}

		internal int CompareRows(MordexDataRow drx, MordexDataRow dry)
		{
			return CompareDataRows(drx, dry);
		}

		protected virtual int CompareDataRows(MordexDataRow drx, MordexDataRow dry)
		{
			if (drx == null)
			{
				if (dry == null)
				{
					return 0;
				}
				return -1;
			}
			if (dry == null)
			{
				return 1;
			}
			return drx.DataHolder.DataRowIndex.CompareTo(dry.DataHolder.DataRowIndex);
		}

		internal MordexFieldBase FindAnyFieldByDataMember(string strB)
		{
			if (strB == null || strB.Length == 0)
			{
				return null;
			}
			foreach (IMordexField item in _fieldCollection)
			{
				if (string.Compare(item.DataMember, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return (MordexFieldBase)item;
				}
			}
			return null;
		}

		private void AddColumnsChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _columnsChanged;
			EventHandler snapshot;
			do
			{
				snapshot = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(snapshot, handler);
				eventHandler = Interlocked.CompareExchange(ref _columnsChanged, value, snapshot);
			}
			while ((object)eventHandler != snapshot);
		}

		private void RemoveColumnsChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _columnsChanged;
			EventHandler snapshot;
			do
			{
				snapshot = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(snapshot, handler);
				eventHandler = Interlocked.CompareExchange(ref _columnsChanged, value, snapshot);
			}
			while ((object)eventHandler != snapshot);
		}

		private void RaiseColumnsChanged(EventArgs eventArgs)
		{
			if (_columnsChanged != null)
			{
				_columnsChanged(this, eventArgs);
			}
		}

		private void AddFilterConditionChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _filterConditionChanged;
			EventHandler snapshot;
			do
			{
				snapshot = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(snapshot, handler);
				eventHandler = Interlocked.CompareExchange(ref _filterConditionChanged, value, snapshot);
			}
			while ((object)eventHandler != snapshot);
		}

		private void RemoveFilterConditionChangedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _filterConditionChanged;
			EventHandler snapshot;
			do
			{
				snapshot = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(snapshot, handler);
				eventHandler = Interlocked.CompareExchange(ref _filterConditionChanged, value, snapshot);
			}
			while ((object)eventHandler != snapshot);
		}

		private void RaiseFilterConditionChanged(EventArgs eventArgs)
		{
			if (_filterConditionChanged != null)
			{
				_filterConditionChanged(this, eventArgs);
			}
		}
	}
}
