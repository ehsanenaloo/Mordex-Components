using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
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
		public object FindItem(object value)
		{
			if (base.CurrencyManager == null || RootTable == null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(_valueMember) && RootTable.Columns.Contains(_valueMember))
			{
				GridColumn column = RootTable.Columns[_valueMember];
				GridFilterCondition condition = new GridFilterCondition(column, ConditionOperator.Equal, value);
				MordexRow mordexRow = GetRowSourceManager().FindRow(condition, null, 1);
				if (mordexRow != null)
				{
					return mordexRow.DataRow;
				}
			}
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
			object listItem = null;
			if (propertyDescriptor != null)
			{
				if (base.CurrencyManager.Position != -1 && GridHelper.CompareValuesWithFallback(value, propertyDescriptor.GetValue(base.CurrencyManager.Current), null, true) == 0)
				{
					listItem = base.CurrencyManager.Current;
				}
				if (listItem == null)
				{
					for (int i = 0; i < base.CurrencyManager.Count; i++)
					{
						if (GridHelper.CompareValuesWithFallback(value, propertyDescriptor.GetValue(base.CurrencyManager.List[i]), null, true) == 0)
						{
							listItem = base.CurrencyManager.List[i];
							break;
						}
					}
				}
			}
			return listItem;
		}

		internal object FindValueByDisplayText(string valueByDisplayText)
		{
			if (valueByDisplayText == "" || base.CurrencyManager == null)
			{
				return valueByDisplayText;
			}
			PropertyDescriptor propertyDescriptor = null;
			object dataRow = null;
			GridColumn gridColumn = FindColumnByKey(_displayMember);
			if (gridColumn != null)
			{
				ConditionOperator conditionOperator = ConditionOperator.Equal;
				GridFilterCondition condition = new GridFilterCondition(gridColumn, conditionOperator, valueByDisplayText);
				MordexRow mordexRow = GetRowSourceManager().FindRow(condition, null, 1, includeFilteredRows: true);
				if (mordexRow != null)
				{
					GridColumn columnByKey = FindColumnByKey(_valueMember);
					if (columnByKey != null)
					{
						return columnByKey.GetRowValue(mordexRow);
					}
					dataRow = mordexRow.DataRow;
				}
			}
			else
			{
				propertyDescriptor = ComputePropertyDescriptor(_displayMember);
				if (propertyDescriptor != null)
				{
					for (int i = 0; i < base.CurrencyManager.Count; i++)
					{
						if (GridHelper.CompareValuesWithFallback(valueByDisplayText, GridHelper.ToSafeString(propertyDescriptor.GetValue(base.CurrencyManager.List[i])), null, true) == 0)
						{
							dataRow = base.CurrencyManager.List[i];
							break;
						}
					}
				}
			}
			if (dataRow != null)
			{
				PropertyDescriptor otherPropertyDescriptor = ComputePropertyDescriptor(_valueMember);
				if (otherPropertyDescriptor != null)
				{
					return otherPropertyDescriptor.GetValue(dataRow);
				}
				return valueByDisplayText;
			}
			return valueByDisplayText;
		}

		internal object FindNextByFirstChar(object value, string strB)
		{
			if (strB == "" || base.CurrencyManager == null)
			{
				return null;
			}
			GridColumn gridColumn = FindColumnByKey(_displayMember);
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_displayMember);
			MordexRow mordexRow = null;
			if (propertyDescriptor != null || gridColumn != null)
			{
				SelectRowByValue(value);
				int row = base.Row;
				if (row < 0)
				{
					row = -1;
				}
				int otherRow = row;
				do
				{
					otherRow++;
					if (otherRow > base.RowCount - 1)
					{
						if (row == -1)
						{
							break;
						}
						otherRow = 0;
					}
					MordexRow rowManagerItem = GetRowManager()[otherRow];
					if (rowManagerItem is MordexDataRow)
					{
						object otherValue = null;
						if (gridColumn != null)
						{
							GridRow gridRow = GetGridRowForLogicalRow(rowManagerItem);
							otherValue = gridRow.Cells[gridColumn].Text;
						}
						else
						{
							otherValue = propertyDescriptor.GetValue(rowManagerItem.DataRow);
						}
						string strA = ((otherValue != null) ? GridHelper.ToSafeString(otherValue) : "");
						if (string.Compare(strA, 0, strB, 0, strB.Length, ignoreCase: true) == 0)
						{
							mordexRow = rowManagerItem;
							base.Row = otherRow;
							break;
						}
					}
				}
				while (otherRow != row);
			}
			if (mordexRow != null)
			{
				GridColumn columnByKey = FindColumnByKey(_valueMember);
				if (columnByKey != null)
				{
					return columnByKey.GetRowValue(mordexRow);
				}
				return ComputePropertyDescriptor(_valueMember)?.GetValue(mordexRow.DataRow);
			}
			return null;
		}

		public void ApplyFilter(GridFilterCondition condition)
		{
			base.RootTable.ApplyFilter(condition);
		}
	}
}
