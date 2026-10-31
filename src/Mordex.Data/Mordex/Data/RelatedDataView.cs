using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mordex.Data
{
	[DefaultMember("Item")]
	internal class RelatedDataView : IBindingList, IList, ICollection, IEnumerable, ITypedList
	{
		private DataRowView _parentRow;

		private ArrayList _rows;

		private RelationChildListProvider _childListProvider;

		private object _parentDataRow;

		internal object[] _parentKeyValues;

		private bool _disposed;

		private ListChangedEventHandler _listChanged;

		private bool _bulkLoading;

		int ICollection.Count
		{
			get
			{
				if (_disposed)
				{
					throw new Exception("RelatedDataView is disposed");
				}
				return _rows.Count;
			}
		}

		object ICollection.SyncRoot => _rows.SyncRoot;

		bool ICollection.IsSynchronized => _rows.IsSynchronized;

		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => true;

		object IList.this[int index]
		{
			get
			{
				return GetRowView(index);
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		bool IBindingList.AllowEdit
		{
			get
			{
				if (_childListProvider == null)
				{
					return false;
				}
				return _childListProvider.GetDataView().AllowEdit;
			}
		}

		bool IBindingList.AllowNew
		{
			get
			{
				if (_childListProvider == null)
				{
					return false;
				}
				return _childListProvider.GetDataView().AllowNew;
			}
		}

		bool IBindingList.AllowRemove
		{
			get
			{
				if (_childListProvider == null)
				{
					return false;
				}
				return _childListProvider.GetDataView().AllowDelete;
			}
		}

		bool IBindingList.IsSorted => false;

		ListSortDirection IBindingList.SortDirection
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get
			{
				throw new NotImplementedException();
			}
		}

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

		internal RelatedDataView(RelationChildListProvider manager, DataRowView parentRow)
		{
			_parentRow = parentRow;
			_childListProvider = manager;
			_rows = new ArrayList();
			_parentDataRow = parentRow.Row;
		}

		internal bool HasSameKeys(object[] valueArray)
		{
			if (_parentKeyValues == null)
			{
				return false;
			}
			if (valueArray == null)
			{
				return false;
			}
			if (_parentKeyValues.Length != valueArray.Length)
			{
				return false;
			}
			int count = _parentKeyValues.Length;
			for (int i = 0; i < count; i++)
			{
				if (!_parentKeyValues[i].Equals(valueArray[i]))
				{
					return false;
				}
			}
			return true;
		}

		internal DataRowView GetParentRow()
		{
			return _parentRow;
		}

		internal object GetParentDataRow()
		{
			return _parentDataRow;
		}

		internal void DisposeView()
		{
			if (_disposed)
			{
				return;
			}
			foreach (RelatedDataViewRow item in _rows)
			{
				item.SetView(null);
			}
			_childListProvider.RemoveCollection(this);
			_parentRow = null;
			_rows.Clear();
			_rows = null;
			_childListProvider = null;
			_disposed = true;
		}

		internal DataRowView GetRowView(int integer)
		{
			if (integer >= 0 && integer < _rows.Count)
			{
				return ((RelatedDataViewRow)_rows[integer]).GetRowView();
			}
			return null;
		}

		internal void AddRow(RelatedDataViewRow row)
		{
			row.SetView(this);
			if (!_bulkLoading)
			{
				int index = _rows.BinarySearch(row);
				if (index < 0)
				{
					index = -index;
					index--;
					_rows.Insert(index, row);
					RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index, -1));
				}
			}
			else
			{
				_rows.Add(row);
			}
		}

		internal void RemoveRow(RelatedDataViewRow row)
		{
			if (row.GetView() == this)
			{
				row.SetView(null);
				int index = _rows.IndexOf(row);
				_rows.RemoveAt(index);
				RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index, -1));
			}
		}

		internal void EndEditRow(object value)
		{
			_childListProvider.EndEditIgnoringMoves((DataRowView)value);
		}

		// The list shows DataRowView items everywhere: the indexer, enumeration, CopyTo, Contains and IndexOf.
		// RelatedDataViewRow stays inside this class.
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetRowViews().GetEnumerator();
		}

		void ICollection.CopyTo(Array destination, int index)
		{
			GetRowViews().CopyTo(destination, index);
		}

		private DataRowView[] GetRowViews()
		{
			DataRowView[] views = new DataRowView[_rows.Count];
			for (int i = 0; i < views.Length; i++)
			{
				views[i] = ((RelatedDataViewRow)_rows[i]).GetRowView();
			}
			return views;
		}

		private int IndexOfRowView(object value)
		{
			if (value is DataRowView)
			{
				for (int i = 0; i < _rows.Count; i++)
				{
					if (Equals(((RelatedDataViewRow)_rows[i]).GetRowView(), value))
					{
						return i;
					}
				}
			}
			return -1;
		}

		int IList.Add(object value)
		{
			throw new NotImplementedException();
		}

		void IList.Clear()
		{
			throw new NotImplementedException();
		}

		bool IList.Contains(object value)
		{
			return IndexOfRowView(value) >= 0;
		}

		int IList.IndexOf(object value)
		{
			return IndexOfRowView(value);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		void IList.Remove(object value)
		{
			throw new NotImplementedException();
		}

		void IList.RemoveAt(int index)
		{
			((RelatedDataViewRow)_rows[index]).DeleteRow();
		}

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
		}

		object IBindingList.AddNew()
		{
			if (_childListProvider != null)
			{
				DataRowView dataRowView = _childListProvider.GetDataView().AddNew();
				try
				{
					for (int i = 0; i < _childListProvider.GetRelation().ParentColumns.Length; i++)
					{
						dataRowView[_childListProvider.GetRelation().ChildColumns[i].ColumnName] = _parentRow[_childListProvider.GetRelation().ParentColumns[i].ColumnName];
					}
				}
				catch
				{
					// Left pending, the row would be committed by the next AddNew of the shared view.
					dataRowView.CancelEdit();
					throw;
				}
				return dataRowView;
			}
			throw new InvalidOperationException();
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
		}

		void IBindingList.RemoveSort()
		{
			throw new NotImplementedException();
		}

		private void RaiseListChanged(ListChangedEventArgs eventArgs)
		{
			if (_listChanged != null)
			{
				_listChanged(this, eventArgs);
			}
		}

		internal void NotifyRowChanged(RelatedDataViewRow row)
		{
			int newIndex = _rows.IndexOf(row);
			RaiseListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, newIndex, -1));
		}

		internal void BeginBulkLoad(bool boolean)
		{
			_bulkLoading = true;
			if (boolean)
			{
				foreach (RelatedDataViewRow item in _rows)
				{
					item.SetView(null);
				}
				_rows.Clear();
			}
		}

		internal void EndBulkLoad()
		{
			_rows.Sort();
			_bulkLoading = false;
			RaiseListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] accessors)
		{
			return ((ITypedList)_childListProvider.GetDataView()).GetItemProperties(accessors);
		}

		string ITypedList.GetListName(PropertyDescriptor[] accessors)
		{
			return "";
		}
	}
}
