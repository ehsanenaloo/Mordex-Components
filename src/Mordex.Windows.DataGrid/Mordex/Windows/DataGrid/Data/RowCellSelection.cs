using System.Collections.Generic;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class RowCellSelection
	{
		private CellSelectionStatus _cellSelectionStatus;

		private MordexRow _row;

		private Dictionary<GridColumn, bool> _dictionary;

		internal RowCellSelection(MordexRow row)
		{
			_row = row;
			_cellSelectionStatus = CellSelectionStatus.NoCellIsSelected;
		}

		internal CellSelectionStatus GetCellSelectionStatus()
		{
			return _cellSelectionStatus;
		}

		internal bool IsColumnSelected(GridColumn gridColumn)
		{
			if (gridColumn.Table != _row.Table)
			{
				return false;
			}
			if (_cellSelectionStatus == CellSelectionStatus.NoCellIsSelected)
			{
				return false;
			}
			if (!GridColumn.IsEffectivelyVisible(gridColumn))
			{
				return false;
			}
			if (_cellSelectionStatus == CellSelectionStatus.AllCellsAreSelected)
			{
				return true;
			}
			return HasColumnEntry(gridColumn);
		}

		internal void SetColumnSelected(GridColumn key, bool condition)
		{
			if (condition)
			{
				if (!IsColumnSelected(key))
				{
					if (_dictionary == null)
					{
						_dictionary = new Dictionary<GridColumn, bool>();
					}
					_dictionary[key] = true;
					_cellSelectionStatus = CellSelectionStatus.SomeCellsAreSelected;
				}
			}
			else
			{
				if (!IsColumnSelected(key))
				{
					return;
				}
				if (_cellSelectionStatus == CellSelectionStatus.AllCellsAreSelected)
				{
					_dictionary = new Dictionary<GridColumn, bool>();
					GridTable gridTable = GetGridTable();
					foreach (GridColumn column in gridTable.Columns)
					{
						if (column != key && GridColumn.IsEffectivelyVisible(column))
						{
							_dictionary.Add(column, value: true);
						}
					}
					_cellSelectionStatus = CellSelectionStatus.SomeCellsAreSelected;
				}
				else if (_dictionary != null)
				{
					_dictionary.Remove(key);
					if (_dictionary.Count == 0)
					{
						_dictionary = null;
						_cellSelectionStatus = CellSelectionStatus.NoCellIsSelected;
					}
				}
			}
		}

		internal void ResetToAllCellsSelected()
		{
			_cellSelectionStatus = CellSelectionStatus.AllCellsAreSelected;
			if (_dictionary != null)
			{
				_dictionary.Clear();
			}
			_dictionary = null;
		}

		internal void DeselectAllColumns()
		{
			_cellSelectionStatus = CellSelectionStatus.NoCellIsSelected;
			if (_dictionary != null)
			{
				_dictionary.Clear();
			}
			_dictionary = null;
		}

		internal void SelectOnlyColumn(GridColumn key)
		{
			if (key == null || !IsColumnSelected(key))
			{
				DeselectAllColumns();
				return;
			}
			if (_cellSelectionStatus == CellSelectionStatus.AllCellsAreSelected || _dictionary == null)
			{
				_cellSelectionStatus = CellSelectionStatus.SomeCellsAreSelected;
				if (_dictionary == null)
				{
					_dictionary = new Dictionary<GridColumn, bool>();
				}
				_dictionary.Add(key, value: true);
				return;
			}
			GridColumn[] array = new GridColumn[_dictionary.Keys.Count];
			_dictionary.Keys.CopyTo(array, 0);
			GridColumn[] otherArray = array;
			foreach (GridColumn gridColumn in otherArray)
			{
				if (gridColumn != key)
				{
					_dictionary.Remove(gridColumn);
				}
			}
			_cellSelectionStatus = CellSelectionStatus.SomeCellsAreSelected;
		}

		private GridTable GetGridTable()
		{
			return _row.Table as GridTable;
		}

		private bool HasColumnEntry(GridColumn key)
		{
			if (_dictionary == null)
			{
				return false;
			}
			return _dictionary.ContainsKey(key);
		}

		internal List<GridColumn> GetSelectedColumnList()
		{
			if (_dictionary != null && _dictionary.Count > 0)
			{
				List<GridColumn> list = new List<GridColumn>();
				{
					foreach (GridColumn key in _dictionary.Keys)
					{
						list.Add(key);
					}
					return list;
				}
			}
			return null;
		}
	}
}
