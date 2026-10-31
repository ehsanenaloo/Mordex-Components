using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mordex.Data
{
	internal class HierarchyNodeCollection : IBindingList, IList, ICollection, IEnumerable, ITypedList
	{
		private ArrayList _nodes;

		private MordexFieldBaseState _hierarchyState;

		private HierarchyNode _parent;

		private NodeRowIndexComparer _nodeComparer;

		private ListChangedEventHandler _listChanged;

		public int Count => _nodes.Count;

		object ICollection.SyncRoot => _nodes.SyncRoot;

		bool ICollection.IsSynchronized => _nodes.IsSynchronized;

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return _nodes[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		bool IBindingList.AllowEdit => true;

		bool IBindingList.AllowNew
		{
			get
			{
				if (_parent == null)
				{
					return true;
				}
				return false;
			}
		}

		bool IBindingList.AllowRemove => true;

		bool IBindingList.IsSorted => false;

		ListSortDirection IBindingList.SortDirection => ListSortDirection.Ascending;

		PropertyDescriptor IBindingList.SortProperty => null;

		bool IBindingList.SupportsChangeNotification => true;

		bool IBindingList.SupportsSearching => false;

		bool IBindingList.SupportsSorting => false;

		event ListChangedEventHandler IBindingList.ListChanged
		{
			add
			{
				AddListChangedHandler(value);
			}
			remove
			{
				RemoveListChangedHandler(value);
			}
		}

		internal HierarchyNodeCollection(MordexFieldBaseState manager, HierarchyNode parent)
		{
			_nodes = new ArrayList();
			_hierarchyState = manager;
			_parent = parent;
		}

		internal void RemoveNode(HierarchyNode node)
		{
			int index = _nodes.IndexOf(node);
			if (index >= 0 && index < _nodes.Count)
			{
				_nodes.RemoveAt(index);
				RaiseItemDeleted(index);
			}
		}

		internal void ClearNodes()
		{
			_nodes.Clear();
		}

		internal void NotifyReset()
		{
			RaiseListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1, -1));
		}

		internal void NotifyNodeChanged(HierarchyNode node)
		{
			int newIndex = _nodes.IndexOf(node);
			RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, newIndex, -1));
		}

		private NodeRowIndexComparer GetNodeComparer()
		{
			if (_nodeComparer == null)
			{
				_nodeComparer = new NodeRowIndexComparer();
			}
			return _nodeComparer;
		}

		internal void InsertNode(HierarchyNode node, bool boolean)
		{
			int index = _nodes.BinarySearch(node, GetNodeComparer());
			if (index < 0)
			{
				index = -index - 1;
			}
			_nodes.Insert(index, node);
			if (boolean)
			{
				RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index, -1));
			}
		}

		private void RaiseListChanged(ListChangedEventArgs eventArgs)
		{
			if (_listChanged != null)
			{
				_listChanged(this, eventArgs);
			}
		}

		private void RaiseItemDeleted(int newIndex)
		{
			if (_listChanged != null)
			{
				_listChanged(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, newIndex, -1));
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _nodes.GetEnumerator();
		}

		void ICollection.CopyTo(Array destination, int index)
		{
			_nodes.CopyTo(destination, index);
		}

		int IList.Add(object value)
		{
			_hierarchyState.AddItem(value);
			return _nodes.IndexOf(value);
		}

		void IList.Clear()
		{
			throw new InvalidOperationException();
		}

		bool IList.Contains(object value)
		{
			foreach (HierarchyNode item in _nodes)
			{
				if (item.GetSourceItem() == value)
				{
					return true;
				}
			}
			return false;
		}

		int IList.IndexOf(object value)
		{
			for (int i = 0; i < _nodes.Count; i++)
			{
				HierarchyNode node = (HierarchyNode)_nodes[i];
				if (node.GetSourceItem() == value)
				{
					return i;
				}
			}
			return -1;
		}

		void IList.Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		void IList.Remove(object value)
		{
			int index = ((IList)this).IndexOf(value);
			if (index >= 0 && index < _nodes.Count)
			{
				HierarchyNode node = (HierarchyNode)_nodes[index];
				_hierarchyState.RemoveItemAt(node.GetDataRowIndex());
				return;
			}
			throw new ArgumentOutOfRangeException();
		}

		void IList.RemoveAt(int index)
		{
			if (index >= 0 && index < _nodes.Count)
			{
				HierarchyNode node = (HierarchyNode)_nodes[index];
				_hierarchyState.RemoveItemAt(node.GetDataRowIndex());
				return;
			}
			throw new ArgumentOutOfRangeException();
		}

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
			throw new NotImplementedException();
		}

		object IBindingList.AddNew()
		{
			_ = _parent;
			return null;
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotImplementedException();
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotImplementedException();
		}

		private void AddListChangedHandler(ListChangedEventHandler handler)
		{
			ListChangedEventHandler listChangedEventHandler = _listChanged;
			ListChangedEventHandler previous;
			do
			{
				previous = listChangedEventHandler;
				ListChangedEventHandler value = (ListChangedEventHandler)Delegate.Combine(previous, handler);
				listChangedEventHandler = Interlocked.CompareExchange(ref _listChanged, value, previous);
			}
			while ((object)listChangedEventHandler != previous);
		}

		private void RemoveListChangedHandler(ListChangedEventHandler handler)
		{
			ListChangedEventHandler listChangedEventHandler = _listChanged;
			ListChangedEventHandler previous;
			do
			{
				previous = listChangedEventHandler;
				ListChangedEventHandler value = (ListChangedEventHandler)Delegate.Remove(previous, handler);
				listChangedEventHandler = Interlocked.CompareExchange(ref _listChanged, value, previous);
			}
			while ((object)listChangedEventHandler != previous);
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
			throw new NotImplementedException();
		}

		void IBindingList.RemoveSort()
		{
			throw new NotImplementedException();
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] accessors)
		{
			return _hierarchyState.GetItemProperties();
		}

		string ITypedList.GetListName(PropertyDescriptor[] accessors)
		{
			return "";
		}
	}
}
