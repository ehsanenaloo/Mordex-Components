using System.Collections;
using System.Reflection;

namespace Mordex.Windows.DataGrid.Internal
{
	[DefaultMember("Item")]
	internal class ColumnLayoutMap
	{
		private int _indentWidth;

		private int _gridState18Integer;

		private int[] _columnWidths = new int[1] { 100 };

		private GridColumnSet[] _columnSets;

		private ColumnCellPlacement[,] _gridColumnState3Array;

		internal int _lineCount = 1;

		internal ColumnLayoutMap[] _gridState18Array;

		internal ColumnLayoutMap()
		{
		}

		internal ColumnLayoutMap(GridTable table)
		{
			if (table.CellLayoutMode == CellLayoutMode.UseColumns)
			{
				BuildFromPrintColumns(table);
			}
			else
			{
				BuildFromColumnSets(table);
			}
		}

		private void BuildFromColumnSets(GridTable gridTable)
		{
			GridColumnSet[] array = new GridColumnSet[gridTable.ColumnSets.Count];
			int value = 0;
			foreach (GridColumnSet columnSet in gridTable.ColumnSets)
			{
				array[columnSet._position] = columnSet;
				if (columnSet.Visible)
				{
					value += columnSet.ColumnCount;
				}
			}
			AllocateCells(gridTable.ColumnSetRowCount, value);
			int otherValue = 0;
			foreach (GridColumnSet arrayItem in array)
			{
				if (!arrayItem.Visible)
				{
					continue;
				}
				for (int j = 0; j < arrayItem.ColumnCount; j++)
				{
					_columnSets[otherValue + j] = arrayItem;
					_columnWidths[otherValue + j] = arrayItem.GetColumnWidth(j);
					for (int k = 0; k < gridTable.ColumnSetRowCount; k++)
					{
						GridColumn gridColumn = arrayItem[k, j];
						if (gridColumn != null && ((gridColumn.PrintMode == ColumnPrintMode.PrintWhenVisible && gridColumn.Visible) || gridColumn.PrintMode == ColumnPrintMode.PrintAlways) && GetCell(k, otherValue + j) == null)
						{
							ColumnCellPlacement columnCellPlacement = new ColumnCellPlacement(gridColumn, k, otherValue + j, gridColumn._rowSpan, gridColumn._colSpan);
							PlaceCell(columnCellPlacement);
						}
					}
				}
				otherValue += arrayItem.ColumnCount;
			}
		}

		private void BuildFromPrintColumns(GridTable gridTable)
		{
			GridColumn[] array = new GridColumn[gridTable.Columns.Count];
			int value = 0;
			foreach (GridColumn column in gridTable.Columns)
			{
				array[column._position] = column;
				if ((column.PrintMode == ColumnPrintMode.PrintWhenVisible && column.Visible) || column.PrintMode == ColumnPrintMode.PrintAlways)
				{
					value++;
				}
			}
			AllocateCells(1, value);
			int otherValue = 0;
			foreach (GridColumn arrayItem in array)
			{
				if ((arrayItem.PrintMode == ColumnPrintMode.PrintWhenVisible && arrayItem.Visible) || arrayItem.PrintMode == ColumnPrintMode.PrintAlways)
				{
					ColumnCellPlacement columnCellPlacement = new ColumnCellPlacement(arrayItem, 0, otherValue);
					_columnWidths[otherValue] = arrayItem._width;
					PlaceCell(columnCellPlacement);
					otherValue++;
				}
			}
		}

		internal int GetColumnCount()
		{
			if (_gridColumnState3Array != null)
			{
				return _gridColumnState3Array.GetLength(1);
			}
			return 0;
		}

		internal ColumnCellPlacement GetCell(int value, int otherValue)
		{
			return _gridColumnState3Array[value, otherValue];
		}

		internal GridColumnSet GetColumnSet(int value)
		{
			return _columnSets[value];
		}

		private void AllocateCells(int value, int otherValue)
		{
			_gridColumnState3Array = new ColumnCellPlacement[value, otherValue];
			_columnWidths = new int[otherValue];
			_columnSets = new GridColumnSet[otherValue];
		}

		private void PlaceCell(ColumnCellPlacement gridColumnState3ArrayItem)
		{
			for (int i = gridColumnState3ArrayItem._row; i < gridColumnState3ArrayItem._row + gridColumnState3ArrayItem._rowSpan; i++)
			{
				for (int j = gridColumnState3ArrayItem._col; j < gridColumnState3ArrayItem._col + gridColumnState3ArrayItem._colSpan; j++)
				{
					_gridColumnState3Array[i, j] = gridColumnState3ArrayItem;
				}
			}
			_gridState18Integer++;
		}

		internal int GetTotalWidth()
		{
			return _indentWidth + SumWidths(0, _gridColumnState3Array.GetLength(1) - 1);
		}

		internal int SumWidths(int value, int otherValue)
		{
			int sumWidths = 0;
			for (int i = value; i <= otherValue; i++)
			{
				sumWidths += _columnWidths[i];
			}
			return sumWidths;
		}

		internal void FitWidthsTo(int value)
		{
			value -= _indentWidth;
			GetColumnCount();
			if (_gridColumnState3Array == null)
			{
				return;
			}
			if (value < GetColumnCount() * 8)
			{
				value = GetColumnCount() * 8;
			}
			int[] array = new int[GetColumnCount()];
			for (int i = 0; i < GetColumnCount(); i++)
			{
				array[i] = _columnWidths[i];
			}
			int sumWidths = SumWidths(0, GetColumnCount() - 1);
			int otherValue = 0;
			int nextValue = value - sumWidths;
			for (int j = 0; j < GetColumnCount(); j++)
			{
				int arrayItem = (int)((float)nextValue * (float)array[j] / (float)sumWidths);
				if (array[j] + arrayItem < 8)
				{
					arrayItem = 8 - array[j];
				}
				array[j] += arrayItem;
				otherValue += arrayItem;
			}
			array[GetColumnCount() - 1] += nextValue - otherValue;
			_columnWidths = array;
		}

		internal void BuildUnwrappedLines(int value, bool condition)
		{
			BuildWrappedLines(value, SumWidths(0, GetColumnCount() - 1) + value, condition);
		}

		internal void BuildWrappedLines(int value, int otherValue, bool condition)
		{
			ArrayList arrayList = new ArrayList();
			int nextValue = value;
			int previousValue = -1;
			for (int i = 0; i < _gridColumnState3Array.GetLength(1); i++)
			{
				if (previousValue != -1 && nextValue + _columnWidths[i] > otherValue)
				{
					arrayList.Add(previousValue);
					i = previousValue;
					previousValue = -1;
					nextValue = 0;
				}
				else
				{
					nextValue += _columnWidths[i];
					if (CanBreakAfterColumn(i))
					{
						previousValue = i;
					}
				}
			}
			arrayList.Add(previousValue);
			_gridState18Array = new ColumnLayoutMap[arrayList.Count];
			int firstValue = 0;
			_lineCount = 0;
			for (int j = 0; j < arrayList.Count; j++)
			{
				ColumnLayoutMap columnLayoutMap = new ColumnLayoutMap();
				_lineCount++;
				int arrayListItem = (int)arrayList[j];
				columnLayoutMap.AllocateCells(_gridColumnState3Array.GetLength(0), arrayListItem - firstValue + 1);
				int secondValue = 0;
				for (int k = firstValue; k <= arrayListItem; k++)
				{
					for (int n = 0; n < _gridColumnState3Array.GetLength(0); n++)
					{
						ColumnCellPlacement gridColumnState3ArrayItem = _gridColumnState3Array[n, k];
						if (gridColumnState3ArrayItem != null && columnLayoutMap._gridColumnState3Array[n, secondValue] == null)
						{
							ColumnCellPlacement gridColumnState3Argument = new ColumnCellPlacement(gridColumnState3ArrayItem._column, n, secondValue, gridColumnState3ArrayItem._rowSpan, gridColumnState3ArrayItem._colSpan);
							columnLayoutMap.PlaceCell(gridColumnState3Argument);
						}
					}
					columnLayoutMap._columnWidths[secondValue] = _columnWidths[k];
					columnLayoutMap._columnSets[secondValue] = _columnSets[k];
					secondValue++;
				}
				_gridState18Array[j] = columnLayoutMap;
				if (j == 0)
				{
					columnLayoutMap._indentWidth = value;
				}
				if (condition)
				{
					int totalWidth = columnLayoutMap.GetTotalWidth();
					if (totalWidth < otherValue && columnLayoutMap._columnWidths.Length > 0)
					{
						columnLayoutMap._columnWidths[columnLayoutMap._columnWidths.Length - 1] += otherValue - totalWidth;
					}
				}
				firstValue = arrayListItem + 1;
			}
		}

		private bool CanBreakAfterColumn(int value)
		{
			if (value == _gridColumnState3Array.GetLength(1) - 1)
			{
				return true;
			}
			for (int i = 0; i < _gridColumnState3Array.GetLength(0); i++)
			{
				if (_gridColumnState3Array[i, value] == _gridColumnState3Array[i, value + 1])
				{
					return false;
				}
			}
			return true;
		}

		internal int GetColumnSetWidth(GridColumnSet gridColumnSet)
		{
			int lengthCore = 0;
			for (int i = 0; i < _gridColumnState3Array.GetLength(1); i++)
			{
				if (_columnSets[i] == gridColumnSet)
				{
					lengthCore += _columnWidths[i];
				}
			}
			return lengthCore;
		}
	}
}
