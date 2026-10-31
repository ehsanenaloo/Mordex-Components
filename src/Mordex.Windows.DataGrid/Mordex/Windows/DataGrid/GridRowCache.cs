using System;
using System.Collections;
using System.Reflection;
using Mordex.Data;

namespace Mordex.Windows.DataGrid
{
	[DefaultMember("Item")]
	internal class GridRowCache : ICollection, IEnumerable
	{
		private Hashtable _hashtable;

		internal readonly MxDataGrid _owner;

		public int Count => _hashtable.Count;

		public bool IsSynchronized => _hashtable.IsSynchronized;

		public object SyncRoot => _hashtable.SyncRoot;

		internal GridRowCache(MxDataGrid owner)
		{
			_owner = owner;
			_hashtable = new Hashtable();
		}

		internal void MarkOtherRowsStale(GridRow gridRow)
		{
			foreach (GridRow value in _hashtable.Values)
			{
				if (value != gridRow)
				{
					value._isLayoutDirty = true;
				}
			}
		}

		internal void AddRow(GridRow gridRow)
		{
			if (_hashtable.Contains(gridRow.GetLogicalRow()))
			{
				_hashtable.Remove(gridRow.GetLogicalRow());
			}
			_hashtable.Add(gridRow.GetLogicalRow(), gridRow);
		}

		internal GridRow FindGridRow(MordexRow key)
		{
			if (_hashtable.ContainsKey(key))
			{
				return (GridRow)_hashtable[key];
			}
			return null;
		}

		internal void DisposeAllRows()
		{
			GridRow[] array = new GridRow[_hashtable.Values.Count];
			_hashtable.Values.CopyTo(array, 0);
			GridRow[] otherArray = array;
			foreach (GridRow gridRow in otherArray)
			{
				gridRow.ResetRow();
			}
			_hashtable.Clear();
		}

		internal void ReleaseAllRows()
		{
			foreach (GridRow value in _hashtable.Values)
			{
				value.DisposeFormatStyles();
			}
			_hashtable.Clear();
		}

		internal void ReleaseStaleRows()
		{
			ArrayList arrayList = new ArrayList();
			foreach (object key in _hashtable.Keys)
			{
				GridRow gridRow = (GridRow)_hashtable[key];
				if (gridRow._isLayoutDirty)
				{
					gridRow.DisposeFormatStyles();
					arrayList.Add(key);
				}
			}
			foreach (object item in arrayList)
			{
				_hashtable.Remove(item);
			}
		}

		public void CopyTo(Array array, int index)
		{
			_hashtable.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return _hashtable.Values.GetEnumerator();
		}
	}
}
