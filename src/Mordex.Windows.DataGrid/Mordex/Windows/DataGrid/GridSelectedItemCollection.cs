using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Mordex.Windows.DataGrid
{
	public class GridSelectedItemCollection : ICollection, IEnumerable
	{
		private ArrayList _arrayList;

		private MxDataGrid _dataGrid;

		public GridSelectedItem this[int index] => (GridSelectedItem)_arrayList[index];

		public int Count => _arrayList.Count;

		bool ICollection.IsSynchronized => _arrayList.IsSynchronized;

		object ICollection.SyncRoot => _arrayList.SyncRoot;

		internal GridSelectedItemCollection(MxDataGrid dataGrid)
		{
			_dataGrid = dataGrid;
			_arrayList = new ArrayList();
		}

		public GridSelectedItem Add(int position)
		{
			return _dataGrid.CreateSelectedItem(position);
		}

		internal void AddItem(GridSelectedItem gridSelectedItem)
		{
			_arrayList.Add(gridSelectedItem);
		}

		public void Remove(GridSelectedItem item)
		{
			_arrayList.Remove(item);
			_dataGrid.DeselectItem(item);
		}

		public void Clear()
		{
			_dataGrid.DeselectAllItems();
		}

		internal void ClearItems()
		{
			_arrayList.Clear();
		}

		internal void RemoveInvalidItems()
		{
			for (int index = _arrayList.Count - 1; index >= 0; index--)
			{
				if (!((GridSelectedItem)_arrayList[index])._isValid)
				{
					_arrayList.RemoveAt(index);
				}
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			_arrayList.CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _arrayList.GetEnumerator();
		}

		public void Sort()
		{
			_arrayList.Sort(new SelectedItemPositionComparer());
		}
	}
}
