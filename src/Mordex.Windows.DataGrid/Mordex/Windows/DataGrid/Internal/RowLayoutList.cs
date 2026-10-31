using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	[DefaultMember("Item")]
	internal class RowLayoutList : IEnumerable
	{
		internal class RowPositionComparer : IComparer
		{
			int IComparer.Compare(object value, object valueArgument)
			{
				MxRowLayoutState mordexRowLayoutState = value as MxRowLayoutState;
				MxRowLayoutState rowLayoutState = valueArgument as MxRowLayoutState;
				return mordexRowLayoutState.GetLocation().Y.CompareTo(rowLayoutState.GetLocation().Y);
			}
		}

		private ArrayList _arrayList;

		private int _totalRowsHeight;

		private int _gridState17Integer;

		internal RowLayoutList()
		{
			_arrayList = new ArrayList();
		}

		internal int GetCount()
		{
			return _arrayList.Count;
		}

		internal MxRowLayoutState GetRowAt(int value)
		{
			return (MxRowLayoutState)_arrayList[value];
		}

		internal void AddRow(MxRowLayoutState mordexRowLayoutState)
		{
			_arrayList.Add(mordexRowLayoutState);
		}

		internal void ClearAll()
		{
			ClearRows(true);
		}

		internal void ClearRows(bool condition)
		{
			if (condition)
			{
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)enumerator.Current;
							foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
							{
								item.SetMordexRow(null);
							}
							mordexRowLayoutState._arrayList.Clear();
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			_arrayList.Clear();
		}

		public IEnumerator GetEnumerator()
		{
			return _arrayList.GetEnumerator();
		}

		internal void LayoutRows(int value, int otherValue)
		{
			int nextValue = value;
			int previousValue = 0;
			int gridState17Integer = 0;
			_totalRowsHeight = 0;
			foreach (MxRowLayoutState item in _arrayList)
			{
				Point point = item.GetLocation();
				point.Y = nextValue;
				item.SetLocation(point);
				nextValue += item._rowHeight;
				_totalRowsHeight += item._rowHeight;
				if (item._rowType != LayoutRowType.ColumnHeader && item._rowType != LayoutRowType.Space)
				{
					previousValue++;
				}
				if (item._rowType != LayoutRowType.EmptyRow && item._rowType != LayoutRowType.Space && item._rowType != LayoutRowType.ColumnHeader && item.GetLocation().Y + item._rowHeight <= value + otherValue)
				{
					gridState17Integer++;
				}
			}
			_gridState17Integer = gridState17Integer;
		}

		internal int GetVisibleRowCount()
		{
			return _gridState17Integer;
		}

		internal MxRowLayoutState FindRow(LayoutRowType layoutRowType, MordexRow mordexRow, bool condition)
		{
			if (mordexRow == null)
			{
				return null;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)enumerator.Current;
					if (mordexRowLayoutState._rowType == layoutRowType && mordexRowLayoutState._mordexRow == mordexRow)
					{
						if (condition)
						{
							_arrayList.Remove(mordexRowLayoutState);
						}
						return mordexRowLayoutState;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}

		internal int IndexOfColumnHeader(GridTable gridTable)
		{
			for (int i = 0; i < _arrayList.Count; i++)
			{
				MxRowLayoutState mordexRowLayoutState = GetRowAt(i);
				if (mordexRowLayoutState._rowType == LayoutRowType.ColumnHeader && mordexRowLayoutState._owner == gridTable)
				{
					return i;
				}
			}
			return -1;
		}

		internal RowLayoutList CopyList()
		{
			RowLayoutList rowLayoutList = new RowLayoutList();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)enumerator.Current;
					rowLayoutList.AddRow(mordexRowLayoutState);
				}
				return rowLayoutList;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		internal ArrayList GetArrayList()
		{
			return _arrayList;
		}
	}
}
