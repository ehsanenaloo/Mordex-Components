using System.Collections;
using System.Reflection;

namespace Mordex.Windows.DataGrid.Internal
{
	[DefaultMember("Item")]
	internal class TableLayoutCollection : IEnumerable
	{
		private ArrayList _arrayList;

		private int _currentIndex;

		internal TableLayoutCollection()
		{
			_arrayList = new ArrayList();
		}

		internal int GetCount()
		{
			return _arrayList.Count;
		}

		internal GridTableLayoutState GetTableLayout(int value)
		{
			return (GridTableLayoutState)_arrayList[value];
		}

		internal void AddTableLayout(GridTableLayoutState gridTableLayoutState)
		{
			_arrayList.Add(gridTableLayoutState);
		}

		internal void ClearAndDispose()
		{
			foreach (GridTableLayoutState item in _arrayList)
			{
				item.DisposeDataGrid();
			}
			_arrayList.Clear();
		}

		public IEnumerator GetEnumerator()
		{
			return _arrayList.GetEnumerator();
		}

		internal int GetCurrentIndex()
		{
			return _currentIndex;
		}

		internal void SetCurrentIndex(int value)
		{
			_currentIndex = value;
		}

		internal ArrayList GetArrayList()
		{
			return _arrayList;
		}
	}
}
