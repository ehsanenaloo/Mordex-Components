using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridColumn
	{
		protected override void SetDefaultFieldValues()
		{
			base.SetDefaultFieldValues();
			_maskPrompt = '_';
			_passwordChar = '\0';
			_columnSetColumn = -1;
			_columnSetRow = -1;
			_colSpan = -1;
			_rowSpan = -1;
			_width = 100;
			_filterEditType = FilterEditType.SameAsEditType;
		}

		protected override void InitializeDataColumnProperties(DataColumn column)
		{
			base.InitializeDataColumnProperties(column);
			if (column != null)
			{
				if (column.DefaultValue != DBNull.Value)
				{
					_defaultValue = column.DefaultValue;
				}
				if (column.AllowDBNull)
				{
					_emptyTextValue = DBNull.Value;
				}
				else if (column.DataType == typeof(string))
				{
					_emptyTextValue = "";
				}
				else
				{
					_emptyTextValue = null;
				}
				if (column.MaxLength <= 0)
				{
					_maxLength = 0;
				}
				else
				{
					_maxLength = column.MaxLength;
				}
				if (column.ReadOnly)
				{
					_editType = EditType.NoEdit;
				}
			}
		}

		protected override object CreateMultipleValueItem(object value)
		{
			if (DataGrid != null)
			{
				return DataGrid.RaiseCreateMultipleValueItem(this, value);
			}
			return null;
		}

		internal GridColumnCollection GetOwnerColumnCollection()
		{
			return (GridColumnCollection)base.BaseParentCollection;
		}

		private void RefreshHostGrid()
		{
			DataGrid?.Refresh();
		}

		internal void NotifyIfColumnIsGrouped()
		{
			if (GetOwnerColumnCollection() == null)
			{
				return;
			}
			foreach (GridGroup group in Table.Groups)
			{
				if (group.Column == this)
				{
					NotifyCollectionLayoutChanged();
					break;
				}
			}
		}

		internal object GetBaseMultipleValuesList(MordexRow row)
		{
			return BaseGetMultipleValuesList(row);
		}

		protected override object GetUnboundValue(MordexRow row)
		{
			object unboundValue = null;
			if (row.ExternalData is GridRow)
			{
				unboundValue = ((GridRow)row.ExternalData).Cells[Index].Value;
			}
			else if (DataGrid != null)
			{
				GridRow gridRow = DataGrid.GetOrCreateGridRow(row, false);
				if (gridRow != null)
				{
					if (ActAsSelector)
					{
						return gridRow.IsChecked;
					}
					unboundValue = gridRow.Cells[Index].Value;
				}
			}
			if (unboundValue == null)
			{
				return DBNull.Value;
			}
			return unboundValue;
		}

		protected override string FormatValue(object value, Mordex.Data.FormatMode formatMode, string formatString, string nullText)
		{
			if (_dropDown != null && _dropDown.ReplaceValues && (!HasValueList || !ValueList.ReplaceValues))
			{
				return _dropDown.GetDisplayTextForValue(value, NullText, true);
			}
			string text = base.FormatValue(value, formatMode, formatString, nullText);
			switch (_characterCasing)
			{
			case CharacterCasing.Lower:
				return text.ToLower(GridHelper.GetCurrentCulture());
			case CharacterCasing.Upper:
				return text.ToUpper(GridHelper.GetCurrentCulture());
			default:
				return text;
			}
		}

		internal object FormatCellValue(object value)
		{
			return FormatValue(value);
		}

		internal void ClearColumnSetOwner()
		{
			_owner = null;
			_columnSetRow = -1;
			_columnSetColumn = -1;
			_colSpan = -1;
			_rowSpan = -1;
		}

		internal GridValueListItemCollection GetDistinctValueList()
		{
			if (DataGrid != null)
			{
				return DataGrid.GetDistinctValuesForColumn(this);
			}
			return null;
		}

		internal object GetEmptyValue(object value)
		{
			if (_emptyTextValue != null && _emptyTextValue != DBNull.Value)
			{
				return _emptyTextValue;
			}
			if (Type != typeof(object))
			{
				if (value == null && DataGrid != null && DataGrid.GetRowSourceManager().RootList is DataView)
				{
					try
					{
						DataColumn dataColumn = ((DataView)DataGrid.GetRowSourceManager().RootList).Table.Columns[DataMember];
						if (dataColumn.DataType != typeof(string) || dataColumn.AllowDBNull)
						{
							return DBNull.Value;
						}
						return string.Empty;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						return DBNull.Value;
					}
				}
				if (value is DataRowView)
				{
					try
					{
						DataColumn column = ((DataRowView)value).Row.Table.Columns[DataMember];
						if (column.DataType != typeof(string) || column.AllowDBNull)
						{
							return DBNull.Value;
						}
						return string.Empty;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						return DBNull.Value;
					}
				}
				if (Type == typeof(string))
				{
					return "";
				}
				return null;
			}
			return null;
		}

		internal InvalidValueAction ComputeInvalidValueAction()
		{
			if (_invalidValueAction == InvalidValueAction.Default && DataGrid != null)
			{
				return DataGrid.InvalidValueAction;
			}
			return _invalidValueAction;
		}

		protected override MordexValueItemCollectionBase CreateValueList()
		{
			return new GridValueListItemCollection(this);
		}
	}
}
