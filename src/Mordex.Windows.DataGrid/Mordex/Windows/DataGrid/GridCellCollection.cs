using System;
using System.Collections;

namespace Mordex.Windows.DataGrid
{
	public class GridCellCollection : ICollection, IEnumerable
	{
		private GridCell[] _cells;

		private GridRow _row;

		public GridRow Row => _row;

		public GridCell this[int index]
		{
			get
			{
				if (_cells[index] == null)
				{
					_cells[index] = new GridCell(_row, _row.Table.Columns[index]);
					_cells[index].RefreshErrors();
				}
				return _cells[index];
			}
		}

		public GridCell this[string columnKey]
		{
			get
			{
				if (_row != null)
				{
					GridColumn gridColumn = _row.Table.Columns[columnKey];
					return this[gridColumn.Index];
				}
				throw new InvalidOperationException();
			}
		}

		public GridCell this[GridColumn column]
		{
			get
			{
				if (_row.Table == column.Table)
				{
					return this[column.Index];
				}
				return null;
			}
		}

		public int Count => _cells.Length;

		bool ICollection.IsSynchronized => _cells.IsSynchronized;

		object ICollection.SyncRoot => _cells.SyncRoot;

		internal GridCellCollection(GridRow row)
		{
			_row = row;
			if (row.Table != null)
			{
				_cells = new GridCell[row.Table.Columns.Count];
			}
			else
			{
				_cells = new GridCell[0];
			}
		}

		internal void UpdateRow()
		{
			_row = null;
			GridCell[] array = _cells;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Detach();
			}
		}

		internal void RefreshCellFormatStyles()
		{
			if (_cells != null)
			{
				for (int i = 0; i < _cells.Length; i++)
				{
					_cells[i]?.DisposeEffectiveFormatStyle();
				}
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			_cells.CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (_row != null && _row.Table != null)
			{
				for (int i = 0; i < _cells.Length; i++)
				{
					if (_cells[i] == null && i < _row.Table.Columns.Count)
					{
						_cells[i] = new GridCell(_row, _row.Table.Columns[i]);
					}
				}
			}
			return _cells.GetEnumerator();
		}
	}
}
