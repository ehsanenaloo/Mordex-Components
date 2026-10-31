using System;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid
{
	public class GridSelectedItem
	{
		private MxDataGrid _dataGrid;

		internal bool _isValid;

		internal MordexRow _row;

		public RowType RowType
		{
			get
			{
				EnsureValid();
				return RowStateHelper.ComputeRowType(_row);
			}
		}

		public GridSelectedCellsCollection SelectedCells
		{
			get
			{
				if (GetDataGrid().SelectionMode == SelectionMode.MultipleCellSelection)
				{
					GridSelectedCellsCollection gridSelectedCellsCollection = new GridSelectedCellsCollection();
					gridSelectedCellsCollection.AddSelectedCells(this);
					return gridSelectedCellsCollection;
				}
				return null;
			}
		}

		public int Position
		{
			get
			{
				EnsureValid();
				if (_row is VirtualDataRow)
				{
					return -2;
				}
				if (_row.DataSourceManager != null && _row.IsRowVisible())
				{
					return _row.RowManagerPosition;
				}
				return -4;
			}
		}

		public GridTable Table
		{
			get
			{
				EnsureValid();
				return (GridTable)_row.Table;
			}
		}

		internal GridSelectedItem(MxDataGrid dataGrid, MordexRow row)
		{
			_row = row;
			_dataGrid = dataGrid;
			_isValid = true;
		}

		internal MxDataGrid GetDataGrid()
		{
			return _dataGrid;
		}

		private void EnsureValid()
		{
			if (!_isValid)
			{
				throw new InvalidOperationException();
			}
		}

		public GridRow GetRow()
		{
			EnsureValid();
			if (_dataGrid != null && _row != null)
			{
				return _dataGrid.GetGridRowForLogicalRow(_row);
			}
			return null;
		}
	}
}
