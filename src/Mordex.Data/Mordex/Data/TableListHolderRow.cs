using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;

namespace Mordex.Data
{
	public class TableListHolderRow : MordexRow
	{
		private MordexFieldBaseState _hierarchyState;

		private BindableList _bindableList;

		private IMordexTable _table;

		private Hashtable _descriptorCache;

		private PropertyDescriptorCollection _itemProperties;

		private int _loadedRowCount;

		private bool _isAddingNew;

		private bool _ignoreListChanges;

		private bool _isCacheBacked;

		private bool _needsResync;

		private int _newRowIndex = -1;

		private bool _wasReset;

		private bool _isReloading;
#pragma warning disable CS0414 // Layout state kept for the persisted format.

		private int _reservedIndex = -1;
#pragma warning restore CS0414

		public override MordexTableBase Table => (MordexTableBase)_table;

		internal TableListHolderRow(IMordexTable table)
		{
			SetShouldRetrieveChildRows(true);
			_table = table;
		}

		internal void SetRowValue(CultureInfo cultureInfo, MordexFieldBase mordexFieldBase, object value, string text, object newValue)
		{
			PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(mordexFieldBase, text, value);
			DataValueHelper.SetPropertyValue(cultureInfo, value, propertyDescriptor, text, newValue);
		}

		internal object GetRowValue(MordexFieldBase mordexFieldBase, object value, string text)
		{
			PropertyDescriptor propertyDescriptor = GetPropertyDescriptor(mordexFieldBase, text, value);
			return DataValueHelper.GetPropertyValue(value, propertyDescriptor, text);
		}

		internal object GetMemberValue(object value, string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			PropertyDescriptor propertyDescriptor = null;
			if (_itemProperties == null)
			{
				LoadProperties(value);
			}
			if (_itemProperties != null)
			{
				propertyDescriptor = _itemProperties.Find(name, ignoreCase: true);
			}
			return DataValueHelper.GetPropertyValue(value, propertyDescriptor, name);
		}

		private PropertyDescriptor GetPropertyDescriptor(MordexFieldBase key, string name, object value)
		{
			if (_itemProperties == null)
			{
				LoadProperties(value);
			}
			if (_itemProperties != null)
			{
				if (_descriptorCache == null)
				{
					_descriptorCache = new Hashtable(GetTableInterface().Fields.Count);
				}
				if (key != null && _descriptorCache.ContainsKey(key))
				{
					return _descriptorCache[key] as PropertyDescriptor;
				}
				PropertyDescriptor propertyDescriptor = _itemProperties.Find(name, ignoreCase: true);
				if (propertyDescriptor != null)
				{
					if (key != null)
					{
						_descriptorCache.Add(key, propertyDescriptor);
					}
					return propertyDescriptor;
				}
			}
			return null;
		}

		private void LoadProperties(object component)
		{
			BoundListInspector descriptorSource = new BoundListInspector(_bindableList.List, DataSourceManager);
			if (descriptorSource != null)
			{
				_itemProperties = descriptorSource.GetItemProperties(null);
			}
			else if (component != null)
			{
				_itemProperties = TypeDescriptor.GetProperties(component);
			}
		}

		internal BindableList GetBindableList()
		{
			return _bindableList;
		}

		internal void SetBindableList(BindableList bindableList)
		{
			if (_bindableList != bindableList)
			{
				ApplyBindableList(bindableList);
			}
		}

		private bool HasChangeNotification()
		{
			if (_bindableList != null && _bindableList.SupportsChangeNotification)
			{
				return _bindableList.SupportsChangeNotification;
			}
			return false;
		}

		protected virtual DataHolderRow AddDataHolderRow(int index)
		{
			if (_bindableList == null)
			{
				return null;
			}
			if (_bindableList.Count > index && _bindableList.List[index] is DataRowView)
			{
				DataRowView dataRowView = (DataRowView)_bindableList.List[index];
				if (dataRowView.IsNew)
				{
					return null;
				}
			}
			lock (this)
			{
				DataHolderRow dataHolderRow = new DataHolderRow(index);
				base.Rows.Insert(index, dataHolderRow);
				dataHolderRow.CaptureRowKey();
				DataSourceManager?.RaiseDataRowRead(dataHolderRow);
				return dataHolderRow;
			}
		}

		private void ApplyBindableList(BindableList bindableList)
		{
			if (_table.HierarchicalMode == HierarchicalMode.SelfReferencing && (ParentRow == null || ParentRow.Table != Table))
			{
				if (bindableList == null)
				{
					if (_hierarchyState != null)
					{
						_hierarchyState.Detach();
						_hierarchyState = null;
					}
				}
				else
				{
					_hierarchyState?.Detach();
					_hierarchyState = new MordexFieldBaseState(DataSourceManager, Table, bindableList);
					bindableList = new BindableList(_hierarchyState.GetRootNodes());
				}
			}
			UnsubscribeFromListChanges();
			_bindableList = bindableList;
			if (_bindableList != null)
			{
				for (int i = 0; i < _bindableList.Count; i++)
				{
					AddDataHolderRow(i);
				}
				_loadedRowCount = base.Rows.Count;
			}
			SubscribeToListChanges();
		}

		internal void RefreshParentLinkedRows()
		{
			if (ParentRow is DataHolderRow)
			{
				DataHolderRow dataHolderRow = (DataHolderRow)ParentRow;
				dataHolderRow.RefreshLinkedRows();
			}
		}

		internal void DisposeAllRows()
		{
			for (int i = base.Rows.Count - 1; i >= 0; i--)
			{
				DataHolderRow dataHolderRow = (DataHolderRow)base.Rows[i];
				base.Rows.RemoveAt(i);
				dataHolderRow.DeleteLinkedRows();
				dataHolderRow.Dispose();
			}
		}

		private void RemoveRowAt(int index)
		{
			if (index < base.Rows.Count)
			{
				DataHolderRow dataHolderRow = (DataHolderRow)base.Rows[index];
				dataHolderRow.DisposeChildTableRows();
				base.Rows.RemoveAt(index);
				for (int i = index; i < base.Rows.Count; i++)
				{
					((DataHolderRow)base.Rows[i]).SetDataRowIndex(i);
				}
				dataHolderRow.DeleteLinkedRows();
				dataHolderRow.Dispose();
				RefreshParentLinkedRows();
			}
		}

		private void HandleRowAdded(int index)
		{
			if (_ignoreListChanges)
			{
				_newRowIndex = index;
				return;
			}
			if (index > base.Rows.Count && _bindableList.BindingList is DataView)
			{
				ResyncRows();
				return;
			}
			if (_bindableList.Count == base.Rows.Count)
			{
				if (base.Rows.Count != _loadedRowCount)
				{
					ResyncRows();
				}
				return;
			}
			DataHolderRow dataHolderRow = AddDataHolderRow(index);
			if (dataHolderRow != null)
			{
				for (int i = index + 1; i < base.Rows.Count; i++)
				{
					((DataHolderRow)base.Rows[i]).SetDataRowIndex(i);
				}
				if (DataSourceManager != null)
				{
					DataSourceManager.RefreshRowsForHolder(dataHolderRow);
				}
				RefreshParentLinkedRows();
			}
		}

		internal void RefreshRowAt(int integer)
		{
			SyncRowWithSource(integer);
		}

		private void SyncRowWithSource(int integer)
		{
			if (integer >= 0 && integer < base.Rows.Count)
			{
				DataHolderRow dataHolderRow = (DataHolderRow)base.Rows[integer];
				if (integer < _bindableList.Count)
				{
					RefreshRowKey(dataHolderRow, _bindableList[integer]);
					dataHolderRow.RefreshLinkedRows();
				}
			}
		}

		private void RefreshRowKey(DataHolderRow dataHolderRow, object value)
		{
			if (dataHolderRow.GetCachedRowKey() != DataHolderRow.UnwrapRow(value))
			{
				dataHolderRow.CaptureRowKey();
				((MordexRow)dataHolderRow).RefreshCachedRowKey();
			}
		}

		protected virtual void ResetAllRows()
		{
			base.Rows.BeginLoad();
			try
			{
				base.Rows.Clear();
				if (_bindableList != null)
				{
					for (int i = 0; i < _bindableList.Count; i++)
					{
						AddDataHolderRow(i);
					}
				}
			}
			finally
			{
				base.Rows.EndLoad();
			}
			DataSourceManager?.Refresh(refreshAll: true);
		}

		private void NotifyBeforeReset()
		{
			if (DataSourceManager != null)
			{
				DataSourceManager.RaiseBeforeReset();
			}
		}

		private void ResyncRows()
		{
			NotifyBeforeReset();
			if (_descriptorCache != null)
			{
				_descriptorCache = null;
			}
			_itemProperties = null;
			_wasReset = true;
			if (_isAddingNew)
			{
				return;
			}
			if ((GetBindableList() == null || GetBindableList().Count == 0 || base.Rows.Count == 0) && ParentRow == null)
			{
				ResetAllRows();
				if (DataSourceManager != null)
				{
					if (ParentRow != null)
					{
						DataSourceManager.SourceListReset(ParentRow.DataRow, Table);
					}
					else
					{
						DataSourceManager.SourceListReset(null, Table);
					}
				}
				return;
			}
			// A list can hold the same item more than once, or null, so each key keeps its rows in list order.
			Dictionary<object, Queue<DataHolderRow>> rowsByKey = new Dictionary<object, Queue<DataHolderRow>>(base.Rows.Count);
			foreach (DataHolderRow item in (IEnumerable)base.Rows)
			{
				item._resyncIndex = -2;
				object rowKey = item.GetCachedRowKey() ?? DBNull.Value;
				if (!rowsByKey.TryGetValue(rowKey, out Queue<DataHolderRow> sameKeyRows))
				{
					sameKeyRows = new Queue<DataHolderRow>();
					rowsByKey.Add(rowKey, sameKeyRows);
				}
				sameKeyRows.Enqueue(item);
			}
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < _bindableList.Count; i++)
			{
				object listItem = _bindableList[i];
				DataHolderRow matchedRow = null;
				if (listItem is HierarchyNode node)
				{
					listItem = node.GetSourceItem();
				}
				object itemKey = DataHolderRow.UnwrapRow(listItem) ?? DBNull.Value;
				if (rowsByKey.TryGetValue(itemKey, out Queue<DataHolderRow> candidates) && candidates.Count > 0)
				{
					matchedRow = candidates.Dequeue();
				}
				if (matchedRow != null)
				{
					matchedRow._resyncIndex = i;
				}
				else
				{
					matchedRow = new DataHolderRow(i);
					matchedRow._resyncIndex = -1;
					base.Rows.Add(matchedRow);
				}
				arrayList.Add(matchedRow);
			}
			DataSourceManager dataSourceManager = DataSourceManager;
			if (dataSourceManager.RowManager != null)
			{
				dataSourceManager.RowManager.SuspendIndex();
			}
			try
			{
				MordexRowCollection rows = base.Rows;
				for (int i = rows.Count - 1; i >= 0; i--)
				{
					DataHolderRow staleRow = (DataHolderRow)rows[i];
					if (staleRow._resyncIndex == -2)
					{
						rows.RemoveAt(i);
						staleRow.DeleteLinkedRows();
						dataSourceManager?.RaiseDataRowDelete(staleRow);
						staleRow.Dispose();
					}
				}
				for (int i = rows.Count - 1; i >= 0; i--)
				{
					DataHolderRow keptRow = (DataHolderRow)rows[i];
					if (keptRow._resyncIndex >= 0)
					{
						keptRow.SetDataRowIndex(keptRow._resyncIndex);
						if (_bindableList[keptRow._resyncIndex] is HierarchyNode)
						{
							keptRow.ReloadChildTableRows();
						}
					}
				}
				base.Rows.ReplaceRowList(arrayList);
				for (int j = 0; j < base.Rows.Count; j++)
				{
					DataHolderRow currentRow = (DataHolderRow)base.Rows[j];
					if (currentRow._resyncIndex == -1)
					{
						currentRow.CaptureRowKey();
						if (dataSourceManager != null)
						{
							dataSourceManager.RaiseDataRowRead(currentRow);
							dataSourceManager.RefreshRowsForHolder(currentRow);
						}
					}
					else
					{
						SyncRowWithSource(j);
					}
				}
			}
			finally
			{
				if (dataSourceManager.RowManager != null)
				{
					dataSourceManager.RowManager.ResumeIndex();
				}
			}
			if (_isReloading)
			{
				return;
			}
			RefreshParentLinkedRows();
			if (DataSourceManager != null)
			{
				if (ParentRow != null)
				{
					DataSourceManager.SourceListReset(ParentRow.DataRow, Table);
				}
				else
				{
					DataSourceManager.SourceListReset(null, Table);
				}
			}
		}

		internal bool IsCacheBacked()
		{
			return _isCacheBacked;
		}

		internal void AddNewToSource(MordexNewDataRow mordexNewDataRow)
		{
			if (_hierarchyState != null)
			{
				mordexNewDataRow._dataItem = _hierarchyState.AddNewItem();
				mordexNewDataRow._mustAddToList = false;
			}
			else if (_bindableList.BindingList != null)
			{
				bool ignoredListChanges = _ignoreListChanges;
				bool wasAddingNew = _isAddingNew;
				_ignoreListChanges = true;
				_isAddingNew = true;
				_wasReset = false;
				try
				{
					mordexNewDataRow._dataItem = _bindableList.AddNew();
				}
				finally
				{
					_isAddingNew = wasAddingNew;
					_ignoreListChanges = ignoredListChanges;
				}
				mordexNewDataRow._mustAddToList = false;
				if (_wasReset)
				{
					_needsResync = true;
					_wasReset = false;
				}
			}
		}

		internal int CommitNewRow(MordexNewDataRow mordexNewDataRow, int insertIndex)
		{
			_ignoreListChanges = true;
			_newRowIndex = -1;
			try
			{
				if (!(mordexNewDataRow._dataItem is DataRowView) && !(mordexNewDataRow._dataItem is MordexRowCache))
				{
					mordexNewDataRow.CommitMultiValueFieldValues();
					mordexNewDataRow.DiscardMultiValueFieldValues();
				}
				if (_bindableList.List is RelatedDataView)
				{
					_ = mordexNewDataRow._dataItem;
					((RelatedDataView)_bindableList.List).EndEditRow(mordexNewDataRow._dataItem);
				}
				else
				{
					if (mordexNewDataRow._dataItem is IEditableObject editableObject)
					{
						_wasReset = false;
						editableObject.EndEdit();
						if (_wasReset)
						{
							return _newRowIndex;
						}
					}
					_wasReset = false;
					if (_bindableList == null)
					{
						return _newRowIndex;
					}
					_bindableList.EndAddNew(mordexNewDataRow.DataRow);
					if (_wasReset)
					{
						return _newRowIndex;
					}
				}
			}
			finally
			{
				_ignoreListChanges = false;
			}
			if (mordexNewDataRow._mustAddToList)
			{
				if (insertIndex < 0)
				{
					_newRowIndex = _bindableList.List.Add(mordexNewDataRow._dataItem);
				}
				else
				{
					if (insertIndex > _bindableList.Count)
					{
						insertIndex = _bindableList.Count;
					}
					_bindableList.List.Insert(insertIndex, mordexNewDataRow._dataItem);
					_newRowIndex = insertIndex;
				}
			}
			else if (_newRowIndex < 0)
			{
				_newRowIndex = _bindableList.List.IndexOf(mordexNewDataRow._dataItem);
				if (_newRowIndex >= _bindableList.Count)
				{
					_newRowIndex = -1;
				}
			}
			if (_needsResync)
			{
				ResyncRows();
			}
			else if (_newRowIndex >= 0)
			{
				bool resetChildLists = false;
				HandleRowAdded(_newRowIndex);
				DataHolderRow dataHolderRow = (DataHolderRow)base.Rows[_newRowIndex];
				foreach (IMordexField field in mordexNewDataRow.GetTableInterface().Fields)
				{
					MordexFieldBase mordexFieldBase = field as MordexFieldBase;
					if (!field.MultipleValues || field.BoundMode != ColumnBoundMode.Bound)
					{
						continue;
					}
					object fieldValue = mordexNewDataRow.GetUnboundValue(mordexFieldBase);
					if (dataHolderRow.DataRow is MordexRowCache)
					{
						MordexRowCollection rows = dataHolderRow.Rows;
						foreach (TableListHolderRow item in (IEnumerable)rows)
						{
							if (item.GetTableInterface() == mordexFieldBase.GetMultipleValuesTable())
							{
								item.ApplyBindableList(new BindableList(new List<MordexRowCache>()));
								item._isCacheBacked = true;
								((MordexRowCache)dataHolderRow.DataRow)._fieldValues[mordexFieldBase.Index] = mordexNewDataRow.GetUnboundValue(mordexFieldBase);
								resetChildLists = true;
							}
						}
					}
					if (fieldValue != null)
					{
						mordexFieldBase.CommitListValue(dataHolderRow, fieldValue);
					}
				}
				if (resetChildLists)
				{
					dataHolderRow?.ResyncChildLists();
				}
				mordexNewDataRow.DiscardMultiValueFieldValues();
				dataHolderRow.ApplyUnboundValues(mordexNewDataRow._unboundValues);
				dataHolderRow.CopyDataRowKeys(mordexNewDataRow);
			}
			return _newRowIndex;
		}

		internal void ReloadFromSource()
		{
			bool wasReloading = _isReloading;
			_isReloading = true;
			try
			{
				UnsubscribeFromListChanges();
				try
				{
					DataSourceManager dataSourceManager = DataSourceManager;
					if (dataSourceManager != null)
					{
						_bindableList = dataSourceManager.GetChildBindableList(ParentRow, Table);
					}
				}
				finally
				{
					SubscribeToListChanges();
				}
				ResyncRows();
			}
			finally
			{
				_isReloading = wasReloading;
			}
		}

		internal void ForceResync()
		{
			ResyncRows();
		}

		internal void CancelAddNew(MordexNewDataRow mordexNewDataRow)
		{
			if (mordexNewDataRow.DataRow != null && _bindableList != null)
			{
				if (_hierarchyState != null)
				{
					_hierarchyState.CancelNewItem(mordexNewDataRow.DataRow);
				}
				else
				{
					_bindableList.CancelAddNew(_bindableList.List.IndexOf(mordexNewDataRow.DataRow));
				}
				_newRowIndex = -1;
			}
		}

		// Removes a new item whose edit never started. A list without ICancelAddNew, such as a DataView,
		// removes its pending item when the item's edit is cancelled.
		internal void DiscardNewItem(object dataItem)
		{
			if (dataItem == null || _bindableList == null)
			{
				return;
			}
			try
			{
				if (_hierarchyState != null)
				{
					// _bindableList shows the root nodes; the item was allocated by the list under them.
					_hierarchyState.DiscardNewItem(dataItem);
				}
				else if (_bindableList.List is ICancelAddNew)
				{
					_bindableList.CancelAddNew(_bindableList.List.IndexOf(dataItem));
				}
				else if (dataItem is IEditableObject editableObject)
				{
					editableObject.CancelEdit();
				}
			}
			finally
			{
				_newRowIndex = -1;
			}
		}

		private void MoveRow(int index, int oldIndex)
		{
			if (index == oldIndex)
			{
				return;
			}
			DataHolderRow row = (DataHolderRow)base.Rows[oldIndex];
			base.Rows.RemoveAt(oldIndex);
			base.Rows.Insert(index, row);
			if (index < oldIndex)
			{
				for (int i = index; i <= oldIndex; i++)
				{
					((DataHolderRow)base.Rows[i]).SetDataRowIndex(i);
				}
			}
			else if (index > oldIndex)
			{
				for (int j = oldIndex; j <= index; j++)
				{
					((DataHolderRow)base.Rows[j]).SetDataRowIndex(j);
				}
			}
			ResyncRows();
		}

		private void OnBindableListChanged(object sender, ListChangedEventArgs eventArgs)
		{
			if (DataSourceManager == null)
			{
				return;
			}
			if (DataSourceManager.InvokeRequired)
			{
				DataCallback method = HandleListChanged;
				DataSourceManager.Invoke(method, eventArgs);
				return;
			}
			HandleListChanged(eventArgs);
		}

		private void HandleListChanged(ListChangedEventArgs eventArgs)
		{
			if (_bindableList == null)
			{
				return;
			}
			lock (this)
			{
				switch (eventArgs.ListChangedType)
				{
				case ListChangedType.ItemAdded:
					HandleRowAdded(eventArgs.NewIndex);
					break;
				case ListChangedType.ItemDeleted:
					if (eventArgs.NewIndex != _newRowIndex)
					{
						RemoveRowAt(eventArgs.NewIndex);
					}
					break;
				case ListChangedType.ItemChanged:
					SyncRowWithSource(eventArgs.NewIndex);
					break;
				case ListChangedType.ItemMoved:
					if (!_ignoreListChanges)
					{
						MoveRow(eventArgs.NewIndex, eventArgs.OldIndex);
					}
					break;
				case ListChangedType.Reset:
					ResyncRows();
					break;
				}
			}
		}

		private void EnsureBindableList()
		{
			DataSourceManager dataSourceManager = DataSourceManager;
			if (_bindableList == null && dataSourceManager != null)
			{
				SetBindableList(dataSourceManager.GetChildBindableList(ParentRow, Table));
			}
		}

		public override bool CanHaveChildren()
		{
			return true;
		}

		protected override MordexRowCollection CreateChildRows()
		{
			return new MordexRowCollection(this, RowCollectionType.DataRows);
		}

		protected override void RetrieveChildRows()
		{
			EnsureBindableList();
		}

		internal object GetListItem(int integer)
		{
			if (_bindableList != null && integer >= 0 && integer < _bindableList.Count)
			{
				try
				{
					return _bindableList[integer];
				}
				catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is IndexOutOfRangeException)
				{
					// The list changed while the value was read.
					return null;
				}
			}
			return null;
		}

		protected override void OnDisposing()
		{
			base.OnDisposing();
			SetBindableList(null);
		}

		protected override int CompareTo(MordexRow row)
		{
			return 0;
		}

		internal MordexDataRow[] FindLinkedRows(object value)
		{
			foreach (DataHolderRow item in (IEnumerable)base.Rows)
			{
				if (item.DataRow == value)
				{
					return item.GetLinkedRows();
				}
				if (!item.ChildrenLoaded)
				{
					continue;
				}
				foreach (TableListHolderRow item2 in (IEnumerable)item.Rows)
				{
					MordexDataRow[] rows = item2.FindLinkedRows(value);
					if (rows != null)
					{
						return rows;
					}
				}
			}
			return null;
		}

		internal bool TryDeleteRow(DataHolderRow dataHolderRow, int index)
		{
			if (_bindableList.AllowsRemove())
			{
				UnsubscribeFromListChanges();
				try
				{
					_bindableList.List.RemoveAt(index);
					RemoveRowAt(index);
				}
				finally
				{
					SubscribeToListChanges();
				}
				DataSourceManager?.RaiseDataRowDelete(dataHolderRow);
				return true;
			}
			return false;
		}

		private void SubscribeToListChanges()
		{
			if (HasChangeNotification())
			{
				_bindableList.ListChanged += OnBindableListChanged;
			}
		}

		private void UnsubscribeFromListChanges()
		{
			if (HasChangeNotification())
			{
				_bindableList.ListChanged -= OnBindableListChanged;
			}
		}

		internal void LoadFromRowCache(MordexRowCache cacheRow, object[] valueArray)
		{
			if (cacheRow._childElements != null)
			{
				_hierarchyState = new MordexFieldBaseState(DataSourceManager, Table, null);
				_hierarchyState.LoadFromRowCache(cacheRow._childElements, valueArray);
				_bindableList = new BindableList(_hierarchyState.GetRootNodes());
			}
			else if (ParentRow is DataHolderRow dataHolderRow && dataHolderRow.GetHierarchyNode() != null)
			{
				_bindableList = new BindableList(dataHolderRow.GetHierarchyNode().GetChildren());
			}
			else
			{
				if (cacheRow._childRows == null)
				{
					_bindableList = new BindableList(new List<MordexRowCache>());
				}
				else
				{
					_bindableList = new BindableList(cacheRow._childRows);
				}
				_isCacheBacked = true;
			}
			valueArray[cacheRow.GetCacheIndex()] = this;
			if (cacheRow._childRows == null)
			{
				return;
			}
			for (int i = 0; i < cacheRow._childRows.Count; i++)
			{
				MordexRowCache mordexRowCache = cacheRow._childRows[i];
				DataHolderRow childRow = AddDataHolderRow(i);
				valueArray[mordexRowCache.GetCacheIndex()] = childRow;
				if (mordexRowCache._childRows != null)
				{
					childRow.LoadChildRows();
					for (int j = 0; j < mordexRowCache._childRows.Count; j++)
					{
						MordexRowCache childCacheRow = mordexRowCache._childRows[j];
						TableListHolderRow tableListHolderRow = childRow.Rows[j] as TableListHolderRow;
						tableListHolderRow.SetShouldRetrieveChildRows(false);
						tableListHolderRow.LoadFromRowCache(childCacheRow, valueArray);
					}
				}
				childRow.IsCheckedCore = mordexRowCache.GetCheckedFlag();
			}
		}

		protected override void CacheChildRowsCore(DataSourceManager manager, List<MordexRowCache> list)
		{
			if (_hierarchyState != null)
			{
				_hierarchyState.AppendToRowCache(manager, list, GetCacheIndex());
			}
			bool hasMultipleValueField = false;
			foreach (IMordexField field in _table.Fields)
			{
				if (field.MultipleValues)
				{
					hasMultipleValueField = true;
					break;
				}
			}
			if (hasMultipleValueField)
			{
				foreach (MordexRow item in (IEnumerable)base.Rows)
				{
					foreach (TableListHolderRow item2 in (IEnumerable)item.Rows)
					{
						if (item2.GetTableInterface().IsAuxiliaryTable)
						{
							item2.LoadChildRows();
						}
					}
				}
			}
			base.CacheChildRowsCore(manager, list);
		}

		public override bool IsVirtualRow()
		{
			return true;
		}
	}
}
