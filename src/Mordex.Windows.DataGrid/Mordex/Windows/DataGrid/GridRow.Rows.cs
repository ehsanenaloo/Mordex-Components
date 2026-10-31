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
		internal void ResetRow()
		{
			if (!_hasBeenReset)
			{
				RefreshRowInGrid();
				DisposeFormatStyles();
				_hasBeenReset = true;
				_gridFormatStyle = null;
				_groupCaption = "";
				SetLogicalRow(null);
				_gridCellCollection.UpdateRow();
			}
		}

		private void RefreshRowInGrid()
		{
			if (_grid != null)
			{
				_grid.MarkRowForRefresh(this);
			}
		}

		public int GetRecordCount()
		{
			if (_logicalRow is MordexGroupRow || _logicalRow is MordexTableRow)
			{
				MordexRow[] dataRows = _logicalRow.GetDataRows(Table);
				return dataRows.Length;
			}
			if (_logicalRow is MordexDataRow)
			{
				int recordCount = 0;
				{
					foreach (GridTable childTable in Table.ChildTables)
					{
						recordCount += GetRecordCount(childTable);
					}
					return recordCount;
				}
			}
			if (_logicalRow is MordexTotalRow)
			{
				MordexRow[] otherDataRows = _logicalRow.ParentRow.GetDataRows(Table);
				return otherDataRows.Length;
			}
			return 0;
		}

		public int GetRecordCount(GridTable table)
		{
			if (_logicalRow is MordexTotalRow)
			{
				_ = _logicalRow.ParentRow;
			}
			MordexRow[] dataRows = _logicalRow.GetDataRows(table);
			return dataRows.Length;
		}

		public GridRow[] GetChildRecords()
		{
			ArrayList arrayList = new ArrayList();
			if (_logicalRow is MordexGroupRow || _logicalRow is MordexTableRow)
			{
				MordexRow[] dataRows = _logicalRow.GetDataRows(Table);
				MordexRow[] array = dataRows;
				foreach (MordexRow value in array)
				{
					arrayList.Add(value);
				}
			}
			else if (_logicalRow is MordexTotalRow)
			{
				MordexRow[] otherDataRows = _logicalRow.ParentRow.GetDataRows(Table);
				MordexRow[] rows = otherDataRows;
				foreach (MordexRow otherValue in rows)
				{
					arrayList.Add(otherValue);
				}
			}
			else if (_logicalRow is MordexDataRow)
			{
				if (Table.GetBaseIsSelfReferencingTable())
				{
					MordexRow[] nextDataRows = _logicalRow.GetDataRows(Table);
					MordexRow[] otherRows = nextDataRows;
					foreach (MordexRow nextValue in otherRows)
					{
						arrayList.Add(nextValue);
					}
				}
				else
				{
					foreach (GridTable childTable in Table.ChildTables)
					{
						MordexRow[] previousDataRows = _logicalRow.GetDataRows(childTable);
						MordexRow[] nextRows = previousDataRows;
						foreach (MordexRow previousValue in nextRows)
						{
							arrayList.Add(previousValue);
						}
					}
				}
			}
			int count = arrayList.Count;
			GridRow[] childRecords = new GridRow[count];
			for (int m = 0; m < count; m++)
			{
				childRecords[m] = DataGrid.GetGridRowForLogicalRow((MordexRow)arrayList[m]);
			}
			return childRecords;
		}

		public GridRow[] GetChildRecords(GridTable table)
		{
			MordexRow parentRow = _logicalRow;
			if (_logicalRow is MordexTotalRow)
			{
				parentRow = _logicalRow.ParentRow;
			}
			MordexRow[] dataRows = parentRow.GetDataRows(table);
			GridRow[] array = new GridRow[dataRows.Length];
			for (int i = 0; i < dataRows.Length; i++)
			{
				array[i] = DataGrid.GetGridRowForLogicalRow(dataRows[i]);
			}
			return array;
		}

		public GridRow[] GetChildRows()
		{
			MordexRow parentRow = _logicalRow;
			if (_logicalRow is MordexTotalRow)
			{
				parentRow = _logicalRow.ParentRow;
			}
			MordexRow[] childRows = parentRow.GetChildRows();
			if (childRows != null && childRows.Length > 0)
			{
				ArrayList arrayList = new ArrayList();
				MordexRow[] array = childRows;
				foreach (MordexRow mordexRow in array)
				{
					if (mordexRow.IsVirtualRow())
					{
						if (((IMordexTable)mordexRow.Table).IsAuxiliaryTable)
						{
							continue;
						}
						MordexRow[] otherChildRows = mordexRow.GetChildRows();
						foreach (MordexRow value in otherChildRows)
						{
							if (value.Visible)
							{
								arrayList.Add(value);
							}
						}
					}
					else if (mordexRow.Visible)
					{
						arrayList.Add(mordexRow);
					}
				}
				GridRow[] nextChildRows = new GridRow[arrayList.Count];
				for (int k = 0; k < arrayList.Count; k++)
				{
					MordexRow arrayListItem = arrayList[k] as MordexRow;
					nextChildRows[k] = DataGrid.GetGridRowForLogicalRow(arrayListItem);
				}
				return nextChildRows;
			}
			return new GridRow[0];
		}

		public GridRow[] GetChildRows(GridTable table)
		{
			MordexRow parentRow = _logicalRow;
			if (_logicalRow is MordexTotalRow)
			{
				parentRow = _logicalRow.ParentRow;
			}
			MordexRowCollection rows = parentRow.Rows;
			if (rows == null)
			{
				return new GridRow[0];
			}
			MordexRow mordexRow = _logicalRow;
			if (_logicalRow is MordexDataRow)
			{
				foreach (MordexRow item in (IEnumerable)rows)
				{
					if (item.IsVirtualRow() && item.Table == table)
					{
						mordexRow = item;
						break;
					}
				}
			}
			else if (Table != table)
			{
				return new GridRow[0];
			}
			List<GridRow> list = new List<GridRow>();
			MordexRow[] childRows = mordexRow.GetChildRows();
			MordexRow[] array = childRows;
			foreach (MordexRow arrayItem in array)
			{
				if (arrayItem.Visible)
				{
					list.Add(DataGrid.GetGridRowForLogicalRow(arrayItem));
				}
			}
			return list.ToArray();
		}

		internal MordexRow GetLogicalRow()
		{
			return _logicalRow;
		}

		internal void SetLogicalRow(MordexRow logicalRow)
		{
			if (logicalRow != _logicalRow)
			{
				_logicalRow = logicalRow;
				if (_logicalRow == null)
				{
					_dataRow = null;
				}
				else
				{
					_dataRow = _logicalRow.DataRow;
				}
			}
		}

		internal bool HasChangedCells()
		{
			return _hasChangedCells;
		}

		internal GridFormatStyle GetCachedRowStyle()
		{
			return _gridRowGridFormatStyle;
		}

		internal void MergeRowFormatStyle(GridFormatStyle formatStyle, bool condition)
		{
			if (_gridRowGridFormatStyle == null || !condition)
			{
				_gridRowGridFormatStyle = new GridFormatStyle(formatStyle);
				return;
			}
			GridFormatStyle gridFormatStyle = _gridRowGridFormatStyle;
			_gridRowGridFormatStyle = _gridRowGridFormatStyle.ApplyOverrides(formatStyle);
			gridFormatStyle.DisposeResources();
		}

		private void AppendRecordClipText(StringBuilder stringBuilder, char character, char otherCharacter)
		{
			if (Table.CellLayoutMode == CellLayoutMode.UseColumns)
			{
				AppendColumnsClipText(stringBuilder, character, otherCharacter);
			}
			else
			{
				AppendColumnSetsClipText(stringBuilder, character, otherCharacter);
			}
		}

		private void ResetCells()
		{
			if (_gridCellCollection == null)
			{
				return;
			}
			foreach (GridCell item in (IEnumerable)_gridCellCollection)
			{
				item?.ResetValueState();
			}
		}
	}
}
