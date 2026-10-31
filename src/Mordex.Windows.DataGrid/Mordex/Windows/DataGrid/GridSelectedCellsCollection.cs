using System;
using System.Collections;
using System.Collections.Generic;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid
{
	public class GridSelectedCellsCollection : IList, ICollection, IEnumerable
	{
		private class ColumnPositionComparer : IComparer<GridColumn>
		{
			int IComparer<GridColumn>.Compare(GridColumn gridColumn, GridColumn gridColumnArgument)
			{
				return gridColumn.Position.CompareTo(gridColumnArgument.Position);
			}
		}

		private List<GridCell> _cells;

		public int Count => _cells.Count;

		public GridCell this[int index] => _cells[index];

		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => true;

		object IList.this[int index]
		{
			get
			{
				return _cells[index];
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		bool ICollection.IsSynchronized => ((ICollection)_cells).IsSynchronized;

		object ICollection.SyncRoot => ((ICollection)_cells).SyncRoot;

		internal GridSelectedCellsCollection()
		{
			_cells = new List<GridCell>();
		}

		internal void AddSelectedCells(GridSelectedItem gridSelectedItem)
		{
			CellSelectionStatus cellSelectionStatus = RowStateHelper.ComputeCellSelectionStatus(gridSelectedItem._row);
			if (cellSelectionStatus == CellSelectionStatus.NoCellIsSelected)
			{
				return;
			}
			GridRow row = gridSelectedItem.GetRow();
			if (cellSelectionStatus == CellSelectionStatus.AllCellsAreSelected)
			{
				if (row.RowType == RowType.GroupHeader)
				{
					return;
				}
				GridTable table = row.Table;
				for (int i = 0; i < table.Columns.Count; i++)
				{
					GridColumn columnInPosition = table.Columns.GetColumnInPosition(i);
					if (columnInPosition.Visible)
					{
						_cells.Add(row.Cells[columnInPosition.Index]);
					}
				}
				return;
			}
			RowTag rowTag = RowStateHelper.GetRowTag(row.GetLogicalRow());
			if (rowTag.GetCellSelectionState() == null)
			{
				return;
			}
			List<GridColumn> list = rowTag.GetCellSelectionState().GetSelectedColumnList();
			if (list == null)
			{
				return;
			}
			list.Sort(new ColumnPositionComparer());
			foreach (GridColumn item in list)
			{
				if (item.Visible)
				{
					_cells.Add(row.Cells[item.Index]);
				}
			}
		}

		int IList.Add(object value)
		{
			throw new InvalidOperationException();
		}

		void IList.Clear()
		{
			throw new InvalidOperationException();
		}

		bool IList.Contains(object value)
		{
			return _cells.Contains((GridCell)value);
		}

		int IList.IndexOf(object value)
		{
			return _cells.IndexOf((GridCell)value);
		}

		void IList.Insert(int index, object value)
		{
			throw new InvalidOperationException();
		}

		void IList.Remove(object value)
		{
			throw new InvalidOperationException();
		}

		void IList.RemoveAt(int index)
		{
			throw new InvalidOperationException();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_cells).CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _cells.GetEnumerator();
		}
	}
}
