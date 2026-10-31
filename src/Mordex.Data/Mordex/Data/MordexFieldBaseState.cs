using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Mordex.Data
{
	internal class MordexFieldBaseState : IHierarchyParent
	{
		private ArrayList _sortedNodes;

		private ArrayList _nodesByRowIndex;

		private HierarchyNodeCollection _rootNodes;

		private MordexTableBase _table;

		private BindableList _bindableList;

		private PropertyDescriptor _parentPropertyDescriptor;

		private PropertyDescriptor _childPropertyDescriptor;

		private string _parentMember;

		private string _childMember;

		private MordexFieldBase _parentField;

		private MordexFieldBase _childField;

		private DataSourceManager _dataSourceManager;

		MordexFieldBaseState IHierarchyParent.SelfReferencingManager => this;

		HierarchyNodeCollection IHierarchyParent.ChildRows => _rootNodes;

		internal MordexFieldBaseState(DataSourceManager dataSourceManager, MordexTableBase table, BindableList sourceList)
		{
			_dataSourceManager = dataSourceManager;
			_childMember = string.Empty;
			_parentMember = string.Empty;
			_rootNodes = new HierarchyNodeCollection(this, null);
			_table = table;
			UpdateBindableList(sourceList);
		}

		internal DataSourceManager GetDataSourceManager()
		{
			return _dataSourceManager;
		}

		// Parent keys, child keys and the root value are all normalized to the declared type of the parent key, so the
		// sorted node list and the parent lookups share one order.
		internal int CompareKeys(object value, object other)
		{
			Type domain = _parentField != null ? _parentField.GetValueDomain() : _parentPropertyDescriptor?.PropertyType;
			return DataValueHelper.CompareInDomain(_dataSourceManager.CurrentCulture, value, other, null, true, domain);
		}

		internal void LoadFromRowCache(List<MordexRowCache> list, object[] valueArray)
		{
			if (list != null)
			{
				_bindableList = new BindableList(list);
				_sortedNodes = new ArrayList(_bindableList.Count);
				_nodesByRowIndex = new ArrayList(_bindableList.Count);
				_rootNodes.ClearNodes();
				HierarchyNode[] nodesBySortOrder = new HierarchyNode[_bindableList.Count];
				for (int i = 0; i < _bindableList.Count; i++)
				{
					MordexRowCache mordexRowCache = list[i];
					HierarchyNode node = new HierarchyNode(this, i);
					valueArray[mordexRowCache.GetCacheIndex()] = node;
					node.SetChildMemberValue(mordexRowCache.GetElementSecondValue());
					node.SetParentMemberValue(mordexRowCache.GetElementValue());
					_nodesByRowIndex.Add(node);
					int sortedPosition = mordexRowCache.GetElementDataRowIndex();
					if (sortedPosition != -1)
					{
						nodesBySortOrder[sortedPosition] = node;
					}
				}
				for (int j = 0; j < nodesBySortOrder.Length; j++)
				{
					_sortedNodes.Add(nodesBySortOrder[j]);
					MordexRowCache cacheRow = list[j];
					int parentIndex = cacheRow.GetParentElementIndex();
					switch (parentIndex)
					{
					case -1:
						((HierarchyNode)_nodesByRowIndex[j]).SetParent(this, false);
						break;
					default:
						((HierarchyNode)_nodesByRowIndex[j]).SetParent((HierarchyNode)valueArray[parentIndex], false);
						break;
					case -2:
						break;
					}
				}
			}
			else
			{
				_bindableList = null;
				_sortedNodes = null;
				_nodesByRowIndex = null;
				_rootNodes.ClearNodes();
			}
		}

		internal void AddItem(object value)
		{
			int index = _bindableList.List.Add(value);
			if (!_bindableList.SupportsChangeNotification)
			{
				HandleItemAdded(index);
			}
		}

		internal BindableList GetBindableList()
		{
			return _bindableList;
		}

		internal void UpdateBindableList(BindableList bindableList)
		{
			if (_bindableList != bindableList)
			{
				UnsubscribeFromListChanges();
				_bindableList = bindableList;
				RebuildNodes();
				SubscribeToListChanges();
			}
		}

		internal PropertyDescriptorCollection GetItemProperties()
		{
			BoundListInspector descriptorSource = new BoundListInspector(_bindableList.List, _dataSourceManager);
			return descriptorSource.GetItemProperties(null);
		}

		private void ResolveMemberBindings()
		{
			if (_bindableList == null)
			{
				return;
			}
			IMordexSelfReferencingSettings selfReferencingSettings = ((IMordexTable)_table).SelfReferencingSettings;
			_parentMember = selfReferencingSettings.ParentMember;
			_childMember = selfReferencingSettings.ChildMember;
			_parentField = _table.FindBoundFieldByDataMember(_parentMember);
			if (_parentField == null && GetDataSourceManager().BoundMode == BoundMode.Unbound)
			{
				_parentField = ((IMordexTable)_table).Fields[_parentMember] as MordexFieldBase;
			}
			_childField = _table.FindBoundFieldByDataMember(_childMember);
			if (_childField == null && GetDataSourceManager().BoundMode == BoundMode.Unbound)
			{
				_childField = ((IMordexTable)_table).Fields[_childMember] as MordexFieldBase;
			}
			BoundListInspector descriptorSource = new BoundListInspector(_bindableList.List, _dataSourceManager);
			PropertyDescriptorCollection propertyDescriptorCollection = descriptorSource.GetItemProperties(null);
			if (propertyDescriptorCollection != null)
			{
				if (_parentMember.Length > 0)
				{
					if (_parentField == null)
					{
						_parentPropertyDescriptor = propertyDescriptorCollection.Find(_parentMember, ignoreCase: true);
					}
				}
				else
				{
					_parentPropertyDescriptor = null;
				}
				if (_childMember.Length > 0)
				{
					if (_childField == null)
					{
						_childPropertyDescriptor = propertyDescriptorCollection.Find(_childMember, ignoreCase: true);
					}
				}
				else
				{
					_childPropertyDescriptor = null;
				}
			}
			else
			{
				_parentPropertyDescriptor = null;
				_childPropertyDescriptor = null;
			}
		}

		private void UpdateNodeKeys(HierarchyNode node)
		{
			if (_parentMember.Length <= 0 || _childMember.Length <= 0)
			{
				return;
			}
			object sourceItem = node.GetSourceItem();
			object newValue = null;
			if (_childField != null)
			{
				node.SetChildMemberValue(_childField.GetItemValue(sourceItem));
			}
			else
			{
				node.SetChildMemberValue(DataValueHelper.GetPropertyValue(sourceItem, _childPropertyDescriptor, _childMember));
			}
			newValue = ((_parentField == null) ? DataValueHelper.GetPropertyValue(sourceItem, _parentPropertyDescriptor, _parentMember) : _parentField.GetItemValue(sourceItem));
			if (CompareKeys(newValue, node.GetParentMemberValue()) == 0)
			{
				return;
			}
			int index;
			if (_sortedNodes.Contains(node))
			{
				index = _sortedNodes.BinarySearch(node);
				if (index >= 0 && index < _sortedNodes.Count)
				{
					_sortedNodes.RemoveAt(index);
				}
			}
			node.SetParentMemberValue(newValue);
			index = _sortedNodes.BinarySearch(node);
			if (index < 0)
			{
				index = -index;
				index--;
				_sortedNodes.Insert(index, node);
			}
			foreach (HierarchyNode item in _nodesByRowIndex)
			{
				if (CompareKeys(item.GetChildMemberValue(), node.GetParentMemberValue()) == 0)
				{
					if (item == node)
					{
						item.SetParent(this, true);
					}
					else
					{
						item.SetParent(node, true);
					}
				}
			}
		}

		private bool IsRootNode(HierarchyNode node)
		{
			IMordexSelfReferencingSettings selfReferencingSettings = ((IMordexTable)_table).SelfReferencingSettings;
			switch (selfReferencingSettings.ParentRootMode)
			{
			case ParentRootMode.NullValue:
				if (DataValueHelper.IsNullOrDBNull(node.GetChildMemberValue()))
				{
					return true;
				}
				return false;
			case ParentRootMode.SelfReferencingNode:
				if (CompareKeys(node.GetParentMemberValue(), node.GetChildMemberValue()) == 0)
				{
					return true;
				}
				return false;
			case ParentRootMode.UseParentRootValue:
				if (CompareKeys(node.GetChildMemberValue(), selfReferencingSettings.ParentRootValue) == 0)
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		private void AssignParent(HierarchyNode node, bool boolean)
		{
			IHierarchyParent parent = null;
			if (IsRootNode(node))
			{
				parent = this;
			}
			else
			{
				int parentIndex = _sortedNodes.BinarySearch(node.GetChildMemberValue());
				if (parentIndex >= 0)
				{
					HierarchyNode parentNode = (HierarchyNode)_sortedNodes[parentIndex];
					if (parentNode != node)
					{
						parent = parentNode;
					}
				}
				if (parent == null && ((IMordexTable)_table).SelfReferencingSettings.TreatOrphanRowsAsRoot)
				{
					parent = this;
				}
			}
			node.SetParent(parent, boolean);
		}

		private void RebuildNodes()
		{
			ResolveMemberBindings();
			if (_bindableList != null)
			{
				_sortedNodes = new ArrayList();
				_nodesByRowIndex = new ArrayList();
				_rootNodes.ClearNodes();
				for (int i = 0; i < _bindableList.Count; i++)
				{
					object sourceItem = _bindableList[i];
					HierarchyNode node = new HierarchyNode(this, i);
					if (_childField != null)
					{
						node.SetChildMemberValue(_childField.GetItemValue(sourceItem));
					}
					else
					{
						node.SetChildMemberValue(DataValueHelper.GetPropertyValue(sourceItem, _childPropertyDescriptor, _childMember));
					}
					if (_parentField != null)
					{
						node.SetParentMemberValue(_parentField.GetItemValue(sourceItem));
					}
					else
					{
						node.SetParentMemberValue(DataValueHelper.GetPropertyValue(sourceItem, _parentPropertyDescriptor, _parentMember));
					}
					_sortedNodes.Add(node);
					_nodesByRowIndex.Add(node);
				}
				_sortedNodes.Sort();
				foreach (HierarchyNode item in _nodesByRowIndex)
				{
					AssignParent(item, false);
				}
				_rootNodes.NotifyReset();
			}
			else
			{
				_sortedNodes = null;
				_nodesByRowIndex = null;
				_rootNodes.ClearNodes();
			}
		}

		private void UnsubscribeFromListChanges()
		{
			if (_bindableList != null && _bindableList.SupportsChangeNotification)
			{
				_bindableList.ListChanged -= OnBindableListChanged;
			}
		}

		private void SubscribeToListChanges()
		{
			if (_bindableList != null && _bindableList.SupportsChangeNotification)
			{
				_bindableList.ListChanged += OnBindableListChanged;
			}
		}

		internal void OnNodeChanged(HierarchyNode node, bool boolean)
		{
			if (!_bindableList.SupportsChangeNotification || boolean)
			{
				HandleItemChanged(node.GetDataRowIndex());
			}
		}

		internal void OnNodeEditEnded(HierarchyNode node)
		{
			if (!_bindableList.SupportsChangeNotification)
			{
				HandleItemChanged(node.GetDataRowIndex());
			}
		}

		internal object AddNewItem()
		{
			return _bindableList.AddNew();
		}

		// Cancels an item that AddNewItem allocated, through the list that allocated it.
		internal void CancelNewItem(object dataItem)
		{
			_bindableList.CancelAddNew(_bindableList.List.IndexOf(dataItem));
		}

		internal void DiscardNewItem(object dataItem)
		{
			if (_bindableList.List is ICancelAddNew)
			{
				_bindableList.CancelAddNew(_bindableList.List.IndexOf(dataItem));
			}
			else if (dataItem is IEditableObject editableObject)
			{
				editableObject.CancelEdit();
			}
		}

		private void OnBindableListChanged(object sender, ListChangedEventArgs eventArgs)
		{
			if ((sender == _bindableList || (_bindableList != null && _bindableList.List == sender)) && _dataSourceManager != null)
			{
				if (_dataSourceManager.InvokeRequired)
				{
					DataCallback method = HandleListChanged;
					_dataSourceManager.Invoke(method, eventArgs);
				}
				else
				{
					HandleListChanged(eventArgs);
				}
			}
		}

		private void HandleListChanged(ListChangedEventArgs eventArgs)
		{
			switch (eventArgs.ListChangedType)
			{
			case ListChangedType.Reset:
				ResetNodes();
				break;
			case ListChangedType.ItemDeleted:
				HandleItemDeleted(eventArgs.NewIndex);
				break;
			case ListChangedType.ItemChanged:
				HandleItemChanged(eventArgs.NewIndex);
				break;
			case ListChangedType.ItemAdded:
				HandleItemAdded(eventArgs.NewIndex);
				break;
			case ListChangedType.ItemMoved:
				HandleItemMoved(eventArgs.NewIndex, eventArgs.OldIndex);
				break;
			}
		}

		private void HandleItemMoved(int integer, int oldIndex)
		{
			if (oldIndex >= 0 && oldIndex < _bindableList.Count)
			{
				HandleItemDeleted(oldIndex);
			}
			if (integer >= 0 && integer < _bindableList.Count)
			{
				HandleItemAdded(integer);
			}
		}

		private void ResetNodes()
		{
			RebuildNodes();
		}

		private void HandleItemAdded(int dataRowIndex)
		{
			if (!(_bindableList[dataRowIndex] is DataRowView dataRowView) || !dataRowView.IsNew)
			{
				HierarchyNode node = new HierarchyNode(this, dataRowIndex);
				_nodesByRowIndex.Insert(dataRowIndex, node);
				for (int i = dataRowIndex + 1; i < _nodesByRowIndex.Count; i++)
				{
					((HierarchyNode)_nodesByRowIndex[i]).SetDataRowIndex(i);
				}
				UpdateNodeKeys(node);
				AssignParent(node, true);
			}
		}

		private void HandleItemChanged(int integer)
		{
			HierarchyNode node = (HierarchyNode)_nodesByRowIndex[integer];
			if (!node._isEditing)
			{
				UpdateNodeKeys(node);
				IHierarchyParent oldParent = node.GetParent();
				AssignParent(node, true);
				if (node.GetParent() != null && node.GetParent() == oldParent)
				{
					node.GetParent().ChildRows.NotifyNodeChanged(node);
				}
			}
		}

		private void HandleItemDeleted(int index)
		{
			HierarchyNode node = (HierarchyNode)_nodesByRowIndex[index];
			_sortedNodes.Remove(node);
			_nodesByRowIndex.RemoveAt(index);
			if (node.GetParent() != null)
			{
				node.GetParent().RemoveNode(node);
			}
			for (int i = index; i < _nodesByRowIndex.Count; i++)
			{
				((HierarchyNode)_nodesByRowIndex[i]).SetDataRowIndex(i);
			}
		}

		internal IList GetRootNodes()
		{
			return _rootNodes;
		}

		internal void Detach()
		{
			UpdateBindableList(null);
		}

		internal void RemoveItemAt(int index)
		{
			_bindableList.List.RemoveAt(index);
			if (!_bindableList.SupportsChangeNotification)
			{
				HandleItemDeleted(index);
			}
		}

		void IHierarchyParent.AddNode(HierarchyNode node, bool notify)
		{
			_rootNodes.InsertNode(node, notify);
		}

		void IHierarchyParent.RemoveNode(HierarchyNode node)
		{
			_rootNodes.RemoveNode(node);
		}

		internal void AppendToRowCache(DataSourceManager dataSourceManager, List<MordexRowCache> list, int integer)
		{
			_ = list.Count;
			foreach (HierarchyNode item in _nodesByRowIndex)
			{
				AppendNodeCacheRow(dataSourceManager, list, item, integer);
			}
			foreach (HierarchyNode item2 in _nodesByRowIndex)
			{
				FillNodeCacheRow(list, item2);
			}
		}

		private void AppendNodeCacheRow(DataSourceManager dataSourceManager, List<MordexRowCache> list, HierarchyNode node, int integer)
		{
			MordexRowCache mordexRowCache = dataSourceManager.CreateRowCacheFor(null);
			mordexRowCache.EnsureRowSettingsForType(MordexRowCache.TypeSelfReferencingElement);
			mordexRowCache.SetParentIndex(integer);
			mordexRowCache.SetCacheIndex(list.Count);
			node._rowCacheIndex = mordexRowCache.GetCacheIndex();
			list.Add(mordexRowCache);
		}

		private void FillNodeCacheRow(List<MordexRowCache> list, HierarchyNode node)
		{
			MordexRowCache mordexRowCache = list[node._rowCacheIndex];
			int sortedPosition = _sortedNodes.BinarySearch(node);
			if (sortedPosition < 0)
			{
				sortedPosition = -1;
			}
			mordexRowCache.SaveSelfReferencingElement(_table, node, sortedPosition);
		}
	}
}
