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
		protected override void OnBindingContextChanged(EventArgs e)
		{
			if (!IsDropDownOpenState())
			{
				base.OnBindingContextChanged(e);
			}
		}

		internal GridValueListItemCollection BuildValueList(bool condition)
		{
			BuildLookupCache();
			if (_hashtable == null)
			{
				return null;
			}
			GridValueListItemCollection gridValueListItemCollection = new GridValueListItemCollection();
			foreach (object key in _hashtable.Keys)
			{
				string text = string.Concat(_hashtable[key]);
				if (condition)
				{
					gridValueListItemCollection.Add(text, text);
				}
				else if (key is CaseInsensitiveKey state)
				{
					gridValueListItemCollection.Add(state.GetKeyValue(), text);
				}
				else
				{
					gridValueListItemCollection.Add(key, text);
				}
			}
			if (condition)
			{
				gridValueListItemCollection.Sort();
			}
			return gridValueListItemCollection;
		}

		internal string GetDisplayTextForValue(object value, string text, bool otherCondition)
		{
			if (value != null)
			{
				if (otherCondition)
				{
					BuildLookupCache();
				}
				if (_hashtable != null)
				{
					CaseInsensitiveKey key = new CaseInsensitiveKey(value);
					if (_hashtable.Contains(key))
					{
						return (string)_hashtable[key];
					}
					if (GridHelper.IsNullOrDBNull(value))
					{
						return text;
					}
					return GridHelper.ToSafeString(value);
				}
			}
			if (base.CurrencyManager == null)
			{
				if (GridHelper.IsNullOrDBNull(value))
				{
					return text;
				}
				return GridHelper.ToSafeString(value);
			}
			object listItem = null;
			MordexRow mordexRow = null;
			GridColumn gridColumn = FindColumnByKey(_valueMember);
			if (gridColumn != null)
			{
				GridFilterCondition condition = new GridFilterCondition(gridColumn, ConditionOperator.Equal, value);
				mordexRow = GetRowSourceManager().FindRow(condition, null, 1);
			}
			else
			{
				PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
				if (propertyDescriptor != null)
				{
					if (value != null && value != DBNull.Value && !propertyDescriptor.PropertyType.IsAssignableFrom(value.GetType()))
					{
						return GridHelper.ToSafeString(value);
					}
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
			}
			if (mordexRow == null && listItem != null)
			{
				mordexRow = GetRowSourceManager().FindDataRow(listItem);
			}
			if (mordexRow != null)
			{
				GridColumn columnByKey = FindColumnByKey(_displayMember);
				if (columnByKey != null)
				{
					GridRow gridRow = GetGridRowForLogicalRow(mordexRow);
					return gridRow.Cells[columnByKey].Text;
				}
				PropertyDescriptor otherPropertyDescriptor = ComputePropertyDescriptor(_displayMember);
				if (otherPropertyDescriptor != null)
				{
					return GridHelper.ToSafeString(otherPropertyDescriptor.GetValue(mordexRow.DataRow));
				}
			}
			if (value == null)
			{
				return "";
			}
			return GridHelper.ToSafeString(value);
		}

		internal object ParseValueText(string text)
		{
			if (base.CurrencyManager == null || string.IsNullOrEmpty(text))
			{
				return text;
			}
			GridColumn gridColumn = FindColumnByKey(_valueMember);
			if (gridColumn != null)
			{
				return GridHelper.ParseValue(text, gridColumn.Type, CultureInfo.InvariantCulture);
			}
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
			if (propertyDescriptor != null)
			{
				for (int i = 0; i < base.CurrencyManager.Count; i++)
				{
					object value = propertyDescriptor.GetValue(base.CurrencyManager.List[i]);
					if (value != null && !(value is DBNull) && value.GetType() != typeof(string))
					{
						object parseValue = GridHelper.ParseValue(text, value.GetType(), CultureInfo.InvariantCulture);
						if (GridHelper.CompareValuesWithFallback(parseValue, value, null, true) == 0)
						{
							return parseValue;
						}
					}
				}
			}
			if (text == "")
			{
				return null;
			}
			return text;
		}

		internal Image GetImageForValue(object value)
		{
			if (GridHelper.IsNullOrDBNull(value) || base.CurrencyManager == null || _imageColumn == null)
			{
				return null;
			}
			object dataRow = FindItem(value);
			GridRow gridRow = null;
			if (dataRow != null)
			{
				gridRow = GetRow(dataRow);
			}
			if (gridRow != null)
			{
				Image image = null;
				if (_imageColumn.ColumnType == ColumnType.BoundImage)
				{
					image = gridRow.Cells[_imageColumn].Value as Image;
					if (image != null)
					{
						image = (_imageColumn.BoundImageSize.IsEmpty ? new Bitmap(image) : new Bitmap(image, _imageColumn.BoundImageSize));
					}
					if (!_imageColumn.BoundImageMaskColor.IsEmpty && image is Bitmap)
					{
						((Bitmap)image).MakeTransparent(_imageColumn.BoundImageMaskColor);
					}
				}
				if (image == null)
				{
					image = gridRow.Cells[_imageColumn].ResolveImage(ComputeImageList());
				}
				return image;
			}
			return null;
		}

		internal bool IsDisplayTextInList(string value)
		{
			if (base.CurrencyManager == null)
			{
				return false;
			}
			GridColumn gridColumn = FindColumnByKey(_displayMember);
			if (gridColumn != null)
			{
				GridFilterCondition condition = new GridFilterCondition(gridColumn, ConditionOperator.Equal, value);
				MordexRow mordexRow = GetRowSourceManager().FindRow(condition, null, 1);
				if (mordexRow != null)
				{
					if (!mordexRow.IsRowVisible())
					{
						MordexRow[] array = GetRowSourceManager().FindAll(condition);
						if (array != null)
						{
							MordexRow[] otherArray = array;
							foreach (MordexRow otherMordexRow in otherArray)
							{
								if (otherMordexRow.IsRowVisible())
								{
									return true;
								}
							}
						}
						return false;
					}
					return true;
				}
				return false;
			}
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_displayMember);
			if (propertyDescriptor != null)
			{
				for (int j = 0; j < GetRowManager().Count; j++)
				{
					MordexRow rowManagerItem = GetRowManager()[j];
					if (rowManagerItem is MordexDataRow && GridHelper.CompareValuesWithFallback(value, GridHelper.ToSafeString(propertyDescriptor.GetValue(rowManagerItem.DataRow)), null, true) == 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override void OnRootTableChanged(EventArgs e)
		{
			if (base.RootTable == null)
			{
				base.RootTable = new GridTable();
			}
			else
			{
				base.OnRootTableChanged(e);
			}
		}

		private IMordexValueItem ComputeIMordexValueItem(object dataRow)
		{
			if (dataRow == null)
			{
				return null;
			}
			IMordexValueItem mordexValueItem = new GridValueItem(dataRow);
			mordexValueItem.Value = GetValueFromDataRow(dataRow);
			mordexValueItem.Image = GetImageForValue(mordexValueItem.Value);
			mordexValueItem.Text = GetDisplayTextForValue(mordexValueItem.Value, "", false);
			return mordexValueItem;
		}

		IMordexValueItem ILookUpTable.GetItemFromText(string text)
		{
			object value = FindValueByDisplayText(text);
			return ComputeIMordexValueItem(FindItem(value));
		}

		IMordexValueItem ILookUpTable.GetItemFromValue(object value)
		{
			return ComputeIMordexValueItem(FindItem(value));
		}

		internal void SetHashtable()
		{
			_hashtable = null;
		}
	}
}
