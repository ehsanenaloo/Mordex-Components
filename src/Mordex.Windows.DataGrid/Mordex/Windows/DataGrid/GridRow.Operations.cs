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
		private string BuildGroupTotalsText()
		{
			string text = "";
			if (Table.GroupHeaderTotals.Count > 0)
			{
				foreach (GridGroupHeaderTotal groupHeaderTotal in Table.GroupHeaderTotals)
				{
					if (groupHeaderTotal.ColumnAlign != null)
					{
						continue;
					}
					GridColumn gridColumn = groupHeaderTotal.Column;
					if (gridColumn == null && Table.Columns.Count > 0)
					{
						gridColumn = Table.Columns[0];
					}
					if (gridColumn != null)
					{
						object subTotal = GetSubTotal(gridColumn, groupHeaderTotal.AggregateFunction, groupHeaderTotal.FilterCondition);
						string otherText = (GridHelper.IsNullOrDBNull(subTotal) ? "" : ((groupHeaderTotal.TotalFormatMode == FormatMode.UseIFormattable) ? ((!(subTotal is IFormattable) || groupHeaderTotal.TotalFormatString.Length <= 0) ? GridHelper.ToSafeString(subTotal) : ((IFormattable)subTotal).ToString(groupHeaderTotal.TotalFormatString, GridHelper.GetCurrentCulture())) : ((groupHeaderTotal.TotalFormatString.Length <= 0) ? GridHelper.ToSafeString(subTotal) : string.Format(groupHeaderTotal.TotalFormatString, subTotal))));
						if (otherText.Length > 0)
						{
							otherText = groupHeaderTotal.TotalPrefix + otherText;
							otherText += groupHeaderTotal.TotalSuffix;
							text = text + " " + otherText;
						}
					}
				}
			}
			return text;
		}

		public object GetSubTotal(GridColumn column, AggregateFunction aggregateFunction)
		{
			return GetSubTotal(column, aggregateFunction, null);
		}

		public object GetSubTotal(GridColumn column, AggregateFunction aggregateFunction, GridFilterCondition filterCondition)
		{
			if (column == null && Table.Columns.Count > 0)
			{
				column = Table.Columns[0];
			}
			MordexRow parentRow = _logicalRow;
			if (parentRow is MordexTotalRow)
			{
				parentRow = parentRow.ParentRow;
			}
			return parentRow.GetTotal(column, DataState.ToDataAggregateFunction(aggregateFunction), filterCondition);
		}

		internal void RecalculateTotals()
		{
			if (DataGrid != null)
			{
				DataGrid.EnableRowCache();
			}
			List<GridColumn> aggregated = new List<GridColumn>();
			foreach (GridColumn column in Table.Columns)
			{
				if (column.AggregateFunction != AggregateFunction.None)
				{
					aggregated.Add(column);
				}
			}
			if (aggregated.Count > 0)
			{
				MordexFieldBase[] fields = new MordexFieldBase[aggregated.Count];
				Mordex.Data.AggregateFunction[] functions = new Mordex.Data.AggregateFunction[aggregated.Count];
				for (int i = 0; i < fields.Length; i++)
				{
					fields[i] = aggregated[i];
					functions[i] = DataState.ToDataAggregateFunction(aggregated[i].AggregateFunction);
				}
				object[] totals = _logicalRow.ParentRow.GetTotals(fields, functions);
				for (int i = 0; i < fields.Length; i++)
				{
					_gridCellCollection[aggregated[i].Index].AssignValue(totals[i], false);
				}
			}
			if (DataGrid != null)
			{
				DataGrid.ResetTotalsCache();
			}
		}

		internal GridFormatStyle GetCachedFormatStyle()
		{
			return _gridFormatStyle;
		}

		internal void DisposeFormatStyles()
		{
			if (_gridRowGridFormatStyle != null)
			{
				_gridRowGridFormatStyle.DisposeResources();
				_gridRowGridFormatStyle = null;
			}
			if (_gridFormatStyle != null)
			{
				_gridFormatStyle.DisposeResources();
				_gridFormatStyle = null;
			}
			if (_gridCellCollection != null)
			{
				_gridCellCollection.RefreshCellFormatStyles();
			}
		}

		internal void MergeFormatStyle(GridFormatStyle formatStyle, bool condition)
		{
			if (_gridFormatStyle == null || !condition)
			{
				_gridFormatStyle = new GridFormatStyle(formatStyle);
				return;
			}
			GridFormatStyle gridFormatStyle = _gridFormatStyle;
			_gridFormatStyle = _gridFormatStyle.ApplyOverrides(formatStyle);
			if (gridFormatStyle != _gridFormatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
		}

		internal void RequestLayout(bool condition)
		{
			if (!_isLayoutSuspended && _grid != null)
			{
				if (condition)
				{
					_grid.RefreshRowCountAndInvalidate();
				}
				else
				{
					_grid.InvalidateGridRowLayout(this);
				}
			}
		}

		public void Delete()
		{
			if (_rowType != RowType.Record)
			{
				throw new InvalidOperationException("Delete can be called only in RowType.Record rows.");
			}
			if (_grid != null)
			{
				_grid.RemoveRecordRow(this);
			}
		}

		public string GetClipString()
		{
			return GetClipString(includeHeaders: true);
		}

		public string GetClipString(bool includeHeaders)
		{
			return GetClipString(includeHeaders, '\t', '\r');
		}

		public string GetClipString(bool includeHeaders, char columnDelimeter, char rowDelimeter)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (includeHeaders && Table.AppendClipHeaders(stringBuilder, columnDelimeter, rowDelimeter))
			{
				stringBuilder.Append(rowDelimeter);
			}
			AppendClipText(stringBuilder, columnDelimeter, rowDelimeter);
			return stringBuilder.ToString();
		}

		internal void AppendClipText(StringBuilder stringBuilder, char character, char otherCharacter)
		{
			switch (_rowType)
			{
			case RowType.Record:
			case RowType.GroupFooter:
			case RowType.TotalRow:
				AppendRecordClipText(stringBuilder, character, otherCharacter);
				break;
			case RowType.GroupHeader:
				AppendGroupHeaderClipText(stringBuilder, character);
				break;
			case RowType.NewRecord:
				break;
			}
		}

		private string EscapeQuotes(string text)
		{
			if (text.Length > 0)
			{
				if (text.IndexOf("\"") != -1)
				{
					return text.Replace("\"", "\"\"");
				}
				return text;
			}
			return text;
		}

		private void AppendGroupHeaderClipText(StringBuilder stringBuilder, char character)
		{
			stringBuilder.Append('"');
			stringBuilder.Append(EscapeQuotes(Group.GroupPrefix));
			stringBuilder.Append(" ");
			stringBuilder.Append(EscapeQuotes(GroupCaption));
			stringBuilder.Append('"');
			if (Table.GroupHeaderTotals.Count <= 0)
			{
				return;
			}
			string[] array = new string[Table.Columns.Count];
			GridColumn[] columns = new GridColumn[Table.Columns.Count];
			foreach (GridColumn column in Table.Columns)
			{
				columns[column.Position] = column;
			}
			foreach (GridGroupHeaderTotal groupHeaderTotal in Table.GroupHeaderTotals)
			{
				if (groupHeaderTotal.ColumnAlign != null && groupHeaderTotal.ColumnAlign.Visible)
				{
					array[groupHeaderTotal.ColumnAlign.Position] = Cells[groupHeaderTotal.ColumnAlign].Text;
				}
			}
			for (int i = 1; i < array.Length; i++)
			{
				if (columns[i].Visible)
				{
					stringBuilder.Append(character);
					if (!string.IsNullOrEmpty(array[i]))
					{
						stringBuilder.Append('"');
						stringBuilder.Append(array[i]);
						stringBuilder.Append('"');
					}
				}
			}
		}

		public void BeginEdit()
		{
			if (!_isEditing)
			{
				if (_rowType != RowType.Record && _rowType != RowType.NewRecord)
				{
					throw new InvalidOperationException("Only rows that represent a record can be edited.");
				}
				_logicalRow.BeginEdit();
				_isEditing = true;
			}
		}

		internal void ResetNewRecord()
		{
			if (_rowType != RowType.NewRecord)
			{
				return;
			}
			ResetCells();
			foreach (GridCell item in (IEnumerable)_gridCellCollection)
			{
				if (item.Column.DefaultValue != null)
				{
					item.Value = item.Column.DefaultValue;
				}
			}
			SetDataChanged(false);
		}

		public void EndEdit()
		{
			if (!_isEditing)
			{
				return;
			}
			bool hasChangedCells = _hasChangedCells;
			_hasChangedCells = false;
			try
			{
				_isEditing = false;
				object dataRow = _logicalRow.DataRow;
				_logicalRow.EndEdit();
				if (_rowType == RowType.Record)
				{
					if (_logicalRow.DataRow == null && _grid != null)
					{
						_logicalRow = _grid.GetRowSourceManager().FindDataRow(dataRow);
					}
					ResetCells();
					if (_grid != null)
					{
						_grid.PrepareRowForDisplay(this, true);
					}
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				_isEditing = true;
				_hasChangedCells = hasChangedCells;
				CommitCells();
				throw;
			}
		}

		internal int GetPosition()
		{
			if (_logicalRow != null)
			{
				return _logicalRow.RowManagerPosition;
			}
			return -1;
		}

		public void CancelEdit()
		{
			_logicalRow.CancelEdit();
			_isEditing = false;
		}

		internal string GetTextOrEmpty()
		{
			if (_textOverride == null)
			{
				return "";
			}
			return _textOverride;
		}

		internal void SetTextOverride(string text)
		{
			_textOverride = text;
			_hasTextOverride = true;
		}

		public void Format()
		{
			if (_grid != null)
			{
				if (_rowType == RowType.FilterRow)
				{
					return;
				}
				foreach (GridCell item in (IEnumerable)_gridCellCollection)
				{
					if (!item.DataChanged)
					{
						item.ClearValueAndDisplayCache();
					}
				}
				_grid.PrepareRowForDisplay(this, true);
				_grid.RefreshRowCountAndInvalidate();
				return;
			}
			throw new InvalidOperationException("Row can not be formatted if it doesn't belong to a DataGrid control.");
		}

		internal void CommitCells()
		{
			if (_gridCellCollection == null)
			{
				return;
			}
			foreach (GridCell item in (IEnumerable)_gridCellCollection)
			{
				item?.InvalidateDisplayCache();
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is GridRow gridRow))
			{
				return false;
			}
			if (gridRow._logicalRow == null || _logicalRow == null)
			{
				return gridRow == this;
			}
			return gridRow._logicalRow.Equals(_logicalRow);
		}

		public override int GetHashCode()
		{
			if (_logicalRow == null)
			{
				return 0;
			}
			return _logicalRow.GetHashCode();
		}
	}
}
