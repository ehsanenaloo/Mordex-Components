using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridDropDown
	{
		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (GridHelper.GetParentSafe(this) is DropDownListForm)
			{
				Visible = true;
			}
			else
			{
				Visible = false;
			}
		}

		protected override void OnRowExpanded(RowActionEventArgs e)
		{
			ResizeToFitRows(_dropDownWidth);
			if (base.ParentForm is DropDownListForm)
			{
				((DropDownListForm)base.ParentForm).ResizeAndPosition();
			}
			base.OnRowExpanded(e);
		}

		protected override void OnRowCollapsed(RowActionEventArgs e)
		{
			ResizeToFitRows(_dropDownWidth);
			if (base.ParentForm is DropDownListForm)
			{
				((DropDownListForm)base.ParentForm).ResizeAndPosition();
			}
			base.OnRowCollapsed(e);
		}

		internal object SelectByPrefix(string otherValue)
		{
			if (string.IsNullOrEmpty(otherValue) || base.CurrencyManager == null)
			{
				return null;
			}
			GridColumn gridColumn = FindColumnByKey(_displayMember);
			gridColumn = null;
			object dataRow = null;
			if (gridColumn != null)
			{
				GridFilterCondition condition = new GridFilterCondition(gridColumn, ConditionOperator.BeginsWith, otherValue);
				MordexRow mordexRow = GetRowSourceManager().FindRow(condition, null, 1, includeFilteredRows: false);
				while (mordexRow != null && !mordexRow.IsRowVisible())
				{
					mordexRow = GetRowSourceManager().FindRow(condition, mordexRow, 1);
				}
				if (mordexRow != null)
				{
					dataRow = mordexRow.DataRow;
					if (mordexRow.IsRowVisible())
					{
						base.Row = mordexRow.RowManagerPosition;
					}
					GridColumn columnByKey = FindColumnByKey(_valueMember);
					if (columnByKey != null)
					{
						return columnByKey.GetRowValue(mordexRow);
					}
				}
			}
			else
			{
				PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_displayMember);
				if (propertyDescriptor != null)
				{
					for (int i = 0; i < GetRowManager().Count; i++)
					{
						MordexRow rowManagerItem = GetRowManager()[i];
						if (!(rowManagerItem is MordexDataRow))
						{
							continue;
						}
						object value = propertyDescriptor.GetValue(rowManagerItem.DataRow);
						string strA = ((value != null) ? GridHelper.ToSafeString(value) : "");
						if (string.Compare(strA, 0, otherValue, 0, otherValue.Length, ignoreCase: true) == 0)
						{
							dataRow = rowManagerItem.DataRow;
							MordexRow otherMordexRow = rowManagerItem;
							if (otherMordexRow != null)
							{
								base.Row = otherMordexRow.RowManagerPosition;
							}
							else
							{
								base.Row = -3;
							}
							break;
						}
					}
				}
			}
			if (dataRow != null && base.Row != -3)
			{
				return ComputePropertyDescriptor(_valueMember)?.GetValue(dataRow);
			}
			base.Row = -3;
			return null;
		}

		internal object GetSelectedValue()
		{
			if (base.Row < 0 || base.CurrencyManager == null)
			{
				return null;
			}
			GridColumn gridColumn = FindColumnByKey(_valueMember);
			if (gridColumn != null)
			{
				return base.CurrentRow.Cells[gridColumn].Value;
			}
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
			if (propertyDescriptor != null)
			{
				GridRow row = GetRow();
				return propertyDescriptor.GetValue(row.DataRow);
			}
			return null;
		}

		internal void SelectRowByValue(object value)
		{
			if (base.CurrencyManager != null)
			{
				try
				{
					if (base.Row != -3 && GridHelper.CompareValuesWithFallback(GetSelectedValue(), value, null, true) == 0)
					{
						return;
					}
					GridColumn gridColumn = FindColumnByKey(_valueMember);
					if (gridColumn != null)
					{
						GridFilterCondition condition = new GridFilterCondition(gridColumn, ConditionOperator.Equal, value);
						MordexRow mordexRow = GetRowSourceManager().FindRow(condition, null, 1);
						if (mordexRow != null && mordexRow.Visible)
						{
							MoveTo(GetGridRowForLogicalRow(mordexRow));
						}
						else
						{
							base.Row = -3;
						}
						return;
					}
					PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
					if (propertyDescriptor != null)
					{
						if (value == null || value == DBNull.Value)
						{
							base.Row = -3;
							return;
						}
						for (int i = 0; i < base.RowCount; i++)
						{
							GridRow row = GetRow(i);
							if (row.RowType == RowType.Record && GridHelper.CompareValuesWithFallback(value, propertyDescriptor.GetValue(row.DataRow), null, true) == 0)
							{
								base.Row = i;
								return;
							}
						}
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
			base.FirstRow = 0;
			base.Row = -3;
		}

		protected override void OnSelectionChanged(EventArgs e)
		{
			base.OnSelectionChanged(e);
			if (_isSelectionSyncEnabled && !(base.Parent is DropDownListForm))
			{
				GridRow row = GetRow();
				if (row != null && row.RowType != RowType.Record)
				{
					EnsureVisible();
					base.Row = -3;
				}
			}
		}

		protected override void OnRowCountChanged(EventArgs e)
		{
			if (_dropListOwner != null)
			{
				_dropListOwner.OnComputeRows();
			}
		}

		internal object GetValueFromDataRow(object dataRow)
		{
			GridColumn gridColumn = FindColumnByKey(_valueMember);
			if (gridColumn != null)
			{
				MordexRow mordexRow = GetRowSourceManager().FindDataRow(dataRow);
				if (mordexRow != null)
				{
					return gridColumn.GetRowValue(mordexRow);
				}
			}
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
			if (propertyDescriptor != null && dataRow != null)
			{
				return propertyDescriptor.GetValue(dataRow);
			}
			return null;
		}

		protected override void OnDataSourceManagerRowChange(MordexRow row, RowChangeType change)
		{
			if (change == RowChangeType.ChildRowReset)
			{
				RebuildRows(true);
			}
			switch (change)
			{
			case RowChangeType.Change:
			case RowChangeType.Delete:
			case RowChangeType.Add:
			case RowChangeType.ChildRowReset:
				SetHashtable();
				if (_dropListOwner != null)
				{
					_dropListOwner.OnComputeRows();
				}
				break;
			case RowChangeType.Move:
				break;
			}
		}
	}
}
