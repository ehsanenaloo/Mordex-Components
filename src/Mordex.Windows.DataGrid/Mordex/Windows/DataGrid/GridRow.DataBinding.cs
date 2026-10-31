using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridRow
	{
		internal object GetColumnValue(GridColumn gridColumn)
		{
			if (gridColumn == null)
			{
				return null;
			}
			if (gridColumn.ActAsSelector)
			{
				return IsChecked;
			}
			if (RowType == RowType.NewRecord)
			{
				if (DataRow != null)
				{
					GridCell gridCell = Cells[gridColumn];
					if (gridCell != null && !gridCell.DataChanged && gridColumn.DefaultValue != null)
					{
						return gridColumn.DefaultValue;
					}
					return gridColumn.GetValueForCell(gridCell);
				}
				return gridColumn.DefaultValue;
			}
			if (_logicalRow.DataRow == null || gridColumn == null)
			{
				return null;
			}
			GridCell cell = Cells[gridColumn.Index];
			return gridColumn.GetValueForCell(cell);
		}

		internal void SetDataChanged(bool hasChangedCells)
		{
			if (hasChangedCells == _hasChangedCells)
			{
				return;
			}
			_hasChangedCells = hasChangedCells;
			if (_hasChangedCells)
			{
				return;
			}
			foreach (GridCell item in (IEnumerable)_gridCellCollection)
			{
				item.DataChanged = false;
			}
		}

		internal void SetDataChangedFlag(bool hasChangedCells)
		{
			_hasChangedCells = hasChangedCells;
		}

		internal void RecalculateDataChanged()
		{
			bool hasChangedCells = false;
			foreach (GridCell item in (IEnumerable)_gridCellCollection)
			{
				if (item.DataChanged)
				{
					hasChangedCells = true;
					break;
				}
			}
			_hasChangedCells = hasChangedCells;
		}

		private void AppendColumnsClipText(StringBuilder stringBuilder, char character, char otherCharacter)
		{
			GridColumn[] array = new GridColumn[Table.Columns.Count];
			foreach (GridColumn column in Table.Columns)
			{
				array[column.Position] = column;
			}
			bool condition = true;
			foreach (GridColumn arrayItem in array)
			{
				if (arrayItem.Visible)
				{
					if (!condition)
					{
						stringBuilder.Append(character);
					}
					else
					{
						condition = false;
					}
					if (arrayItem.Type == typeof(string))
					{
						stringBuilder.Append('"');
						stringBuilder.Append(EscapeQuotes(Cells[arrayItem.Index].Text));
						stringBuilder.Append('"');
					}
					else
					{
						stringBuilder.Append(Cells[arrayItem.Index].Text);
					}
				}
			}
		}

		private void AppendColumnSetsClipText(StringBuilder stringBuilder, char character, char value)
		{
			GridColumnSet[] array = new GridColumnSet[Table.ColumnSets.Count];
			foreach (GridColumnSet columnSet in Table.ColumnSets)
			{
				array[columnSet._position] = columnSet;
			}
			bool condition = true;
			for (int i = 0; i < Table.ColumnSetRowCount; i++)
			{
				if (!condition)
				{
					stringBuilder.Append(value);
				}
				else
				{
					condition = false;
				}
				bool otherCondition = true;
				for (int j = 0; j < array.Length; j++)
				{
					if (!array[j].Visible)
					{
						continue;
					}
					for (int k = 0; k < array[j].ColumnCount; k++)
					{
						if (!otherCondition)
						{
							stringBuilder.Append(character);
						}
						else
						{
							otherCondition = false;
						}
						GridColumn gridColumn = array[j][i, k];
						if (gridColumn != null && gridColumn.Visible && gridColumn.ColumnSetRow == i && gridColumn.ColumnSetColumn == k)
						{
							if (gridColumn.Type == typeof(string))
							{
								stringBuilder.Append('"');
								stringBuilder.Append(EscapeQuotes(Cells[gridColumn.Index].Text));
								stringBuilder.Append('"');
							}
							else
							{
								stringBuilder.Append(Cells[gridColumn.Index].Text);
							}
						}
					}
				}
			}
		}

		internal ColumnFormatStyleState GetColumnFormatState(GridColumn gridColumn, bool condition)
		{
			return _gridFormatStyleState.GetColumnStyleState(gridColumn, condition);
		}
	}
}
