using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	partial class MordexFieldBase
	{
		protected object BaseGetValue(MordexRow row)
		{
			if (row != null)
			{
				if (_isListItemValue)
				{
					return row.DataRow;
				}
				switch (BaseBoundMode)
				{
				case ColumnBoundMode.UnboundKeepValues:
					if (BaseTable.HasOwnerField() && BaseTable.GetOwnerField().BaseBoundMode == ColumnBoundMode.UnboundKeepValues)
					{
						return row.DataRow;
					}
					return row.ReadFieldValue(this, _dataMember);
				case ColumnBoundMode.Bound:
					if (!_multipleValues)
					{
						return row.ReadFieldValue(this, _dataMember);
					}
					return BaseGetMultipleValuesList(row);
				case ColumnBoundMode.UnboundFetch:
					return GetUnboundValue(row);
				}
			}
			return null;
		}

		protected void BaseSetImageValue(MordexRow row, object value)
		{
			if (BasePropertyDescriptor != null && value is Image image && BasePropertyDescriptor.PropertyType == typeof(byte[]))
			{
				MemoryStream memoryStream = new MemoryStream();
				image.Save(memoryStream, ImageFormat.Bmp);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				byte[] buffer = new byte[memoryStream.Length];
				memoryStream.Read(buffer, 0, (int)memoryStream.Length);
				memoryStream.Close();
				BaseSetValue(row, memoryStream.ToArray());
			}
			else
			{
				BaseSetValue(row, value);
			}
		}

		internal void CommitListValue(MordexRow mordexRow, object value)
		{
			bool wasCommitting = _isCommittingListValue;
			_isCommittingListValue = true;
			try
			{
				SyncListValue(mordexRow, value);
			}
			finally
			{
				_isCommittingListValue = wasCommitting;
			}
		}

		private void SyncListValue(MordexRow mordexRow, object newValues)
		{
			object currentValue = mordexRow.ReadFieldValue(this, _dataMember);
			IList list = null;
			list = ((newValues != null) ? (newValues as IList) : new ArrayList());
			if (list == null)
			{
				throw new InvalidOperationException("Value in column is not a list");
			}
			IList existingItems = ((!(currentValue is BindableList)) ? (currentValue as IList) : ((BindableList)currentValue).List);
			if (existingItems == null || list == null)
			{
				return;
			}
			if (mordexRow is MordexNewDataRow && !_isCommittingListValue)
			{
				((MordexNewDataRow)mordexRow).SetUnboundValue(this, newValues);
				return;
			}
			bool[] isMatched = new bool[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				isMatched[i] = false;
			}
			MordexFieldBase mordexFieldBase = (MordexFieldBase)GetMultipleValuesTable().Fields[0];
			for (int index = existingItems.Count - 1; index >= 0; index--)
			{
				object item = existingItems[index];
				object value = mordexFieldBase.GetItemValue(item);
				int newListIndex = list.IndexOf(value);
				if (newListIndex == -1)
				{
					existingItems.RemoveAt(index);
				}
				else
				{
					isMatched[newListIndex] = true;
				}
			}
			for (int j = 0; j < isMatched.Length; j++)
			{
				if (isMatched[j])
				{
					continue;
				}
				object newItem = CreateMultipleValueItem(list[j]);
				if (newItem == null && existingItems.GetType() == typeof(List<MordexRowCache>))
				{
					newItem = new MordexRowCache();
					((MordexRowCache)newItem)._fieldValues = new object[1] { list[j] };
				}
				if (newItem != null)
				{
					existingItems.Add(newItem);
				}
				else if (existingItems is IBindingList bindingList)
				{
					newItem = bindingList.AddNew();
					DataValueHelper.SetPropertyValue(DataValueHelper.GetCulture(GetDataSourceManager()), newItem, mordexFieldBase.BasePropertyDescriptor, mordexFieldBase.BaseDataMember, list[j]);
					if (bindingList is RelatedDataView)
					{
						((RelatedDataView)bindingList).EndEditRow(newItem);
					}
					else if (newItem is IEditableObject editableObject)
					{
						editableObject.EndEdit();
					}
				}
				else
				{
					existingItems.Add(list[j]);
				}
			}
		}

		protected virtual object CreateMultipleValueItem(object value)
		{
			return null;
		}

		protected void BaseSetValue(MordexRow row, object value)
		{
			if (_multipleValues && BaseBoundMode == ColumnBoundMode.Bound)
			{
				SyncListValue(row, value);
				return;
			}
			if (BaseBoundMode == ColumnBoundMode.UnboundKeepValues && value != null && value != DBNull.Value && _dataTypeCode != TypeCode.Object)
			{
				value = Convert.ChangeType(value, _dataTypeCode);
			}
			row.SetValue(this, value);
		}

		internal object GetItemValue(object value)
		{
			if (_isListItemValue)
			{
				return value;
			}
			if (value is MordexRowCache mordexRowCache)
			{
				return mordexRowCache.GetFieldValue(Index);
			}
			return DataValueHelper.GetPropertyValue(value, null, _dataMember);
		}

		protected virtual object BaseGetValueToCompare(MordexRow row)
		{
			object dataRow = row.DataRow;
			if (dataRow != null)
			{
				if (_compareTarget == CompareTarget.Value)
				{
					object boundValue = BaseGetValue(row);
					if (boundValue == null)
					{
						return DBNull.Value;
					}
					return boundValue;
				}
				if (_multipleValues && BaseGetValue(row) is Array valueArray)
				{
					string[] textArray = new string[valueArray.Length];
					for (int i = 0; i < textArray.Length; i++)
					{
						textArray[i] = FormatValue(valueArray.GetValue(i));
					}
					return textArray;
				}
				return BaseGetText(row);
			}
			return DBNull.Value;
		}

		string IValueConverter.FormatValue(object value)
		{
			return FormatValue(value);
		}

		protected virtual string BaseGetText(MordexRow row)
		{
			if (BaseTable.HasOwnerField())
			{
				return BaseTable.GetOwnerField().FormatValue(BaseGetValue(row));
			}
			return FormatValue(BaseGetValue(row));
		}

		internal string FormatWithFieldSettings(object value)
		{
			return FormatValue(value, _formatMode, _formatString, _nullText);
		}

		internal string FormatWithGroupSettings(object value, FormatMode formatMode, string formatString, string nullText)
		{
			return FormatValue(value, formatMode, formatString, nullText);
		}

		protected string FormatValue(object value)
		{
			if (_multipleValues)
			{
				if (value is Array items)
				{
					string text = "";
					for (int i = 0; i < items.Length; i++)
					{
						text += FormatValue(items.GetValue(i), _formatMode, _formatString, _nullText);
						if (i < items.Length - 1)
						{
							text += ", ";
						}
					}
					return text;
				}
				return FormatValue(value, _formatMode, _formatString, _nullText);
			}
			return FormatValue(value, _formatMode, _formatString, _nullText);
		}

		protected virtual string FormatValue(object value, FormatMode formatMode, string formatString, string nullText)
		{
			if (_hasValueList && _valueList != null)
			{
				IMordexValueItemCollection mordexValueItemCollection = _valueList;
				if (mordexValueItemCollection.ReplaceValues)
				{
					IMordexValueItem mordexValueItem = mordexValueItemCollection[value];
					if (mordexValueItem != null)
					{
						return mordexValueItem.Text;
					}
				}
			}
			if (value == null || value == DBNull.Value)
			{
				return nullText;
			}
			switch (formatMode)
			{
			case FormatMode.UseInputMask:
				return GetMaskState().ApplyMask(value);
			case FormatMode.UseStringFormat:
				if (formatString.Length > 0)
				{
					return string.Format(DataValueHelper.GetCulture(GetDataSourceManager()), formatString, value);
				}
				break;
			default:
				if (formatString.Length > 0 && value is IFormattable formattable)
				{
					try
					{
						return formattable.ToString(formatString, GetDataSourceManager().CurrentCulture);
					}
					catch (FormatException)
					{
						// An invalid format string falls back to the plain text of the value.
					}
				}
				break;
			}
			return value.ToString();
		}

		object IValueConverter.ParseText(string text)
		{
			return ParseText(text);
		}

		protected virtual object ParseText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			return text;
		}

		private void EnsureMultipleValuesTable()
		{
			if (_multipleValues && _multipleValuesTable == null)
			{
				_multipleValuesTable = new MordexTableBase(this);
			}
		}

		internal IMordexTable GetMultipleValuesTable()
		{
			EnsureMultipleValuesTable();
			return _multipleValuesTable;
		}

		internal void InitializeFromDescriptor(PropertyDescriptor propertyDescriptor)
		{
			Initialize(propertyDescriptor);
		}

		internal void InitializeFromDataTable(DataTable dataTable)
		{
			if (dataTable != null && BaseBoundMode == ColumnBoundMode.Bound && dataTable.Columns.Contains(_dataMember))
			{
				InitializeDataColumnProperties(dataTable.Columns[_dataMember]);
			}
		}

		protected virtual void InitializeDataColumnProperties(DataColumn column)
		{
			if (column != null && !string.IsNullOrEmpty(column.Caption))
			{
				_displayName = Regex.Replace(column.Caption, "(\\p{Ll})(\\p{Lu})|_+", "$1 $2");
			}
		}

		internal object GetRowValue(MordexRow row)
		{
			return BaseGetValue(row);
		}

		internal string GetRowText(MordexRow row)
		{
			return BaseGetText(row);
		}

		internal object GetRowValueToCompare(MordexRow row)
		{
			return BaseGetValueToCompare(row);
		}

		private int CompareLists(object value, object otherValue)
		{
			IList list = value as IList;
			IList otherList = otherValue as IList;
			if (list != null)
			{
				if (otherList != null)
				{
					CultureInfo cultureInfo = DataValueHelper.GetCulture(GetDataSourceManager());
					for (int i = 0; i < list.Count; i++)
					{
						if (i < otherList.Count)
						{
							int result = DataValueHelper.CompareValues(cultureInfo, list[i], otherList[i], _sortComparer, _compareIgnoreCase, false);
							if (result != 0)
							{
								return result;
							}
							continue;
						}
						return 1;
					}
					if (otherList.Count == list.Count)
					{
						return 0;
					}
					return -1;
				}
				return 1;
			}
			if (otherList != null)
			{
				return -1;
			}
			return 0;
		}

		protected virtual Image BaseGetDisplayImage()
		{
			return null;
		}

		protected virtual MordexValueItemCollectionBase BaseGetFilterValueList()
		{
			if (_hasValueList)
			{
				if (_compareTarget == CompareTarget.Text)
				{
					IMordexValueItemCollection mordexValueItemCollection = CreateValueList();
					foreach (IMordexValueItem item in (IEnumerable)_valueList)
					{
						mordexValueItemCollection.Add(item.Text, item.Text);
					}
					mordexValueItemCollection.Sort();
					return mordexValueItemCollection as MordexValueItemCollectionBase;
				}
				return _valueList;
			}
			if (BaseTable != null)
			{
				DataSourceManager dataSourceManager = ((IMordexTable)BaseTable).DataSourceManager;
				if (dataSourceManager != null)
				{
					MordexValueItemCollectionBase mordexValueItemCollectionBase = CreateValueList();
					dataSourceManager.CollectDistinctValues(this, mordexValueItemCollectionBase);
					mordexValueItemCollectionBase.SortByValue();
					return mordexValueItemCollectionBase;
				}
			}
			return null;
		}

		// The declared type of the field's values, or null when it is not known. Converted comparisons normalize
		// both sides to it.
		internal Type GetValueDomain()
		{
			if (_multipleValues)
			{
				IMordexTable table = GetMultipleValuesTable();
				if (table != null && table.Fields.Count > 0 && table.Fields[0] is MordexFieldBase itemField)
				{
					return itemField.GetValueDomain();
				}
			}
			PropertyDescriptor propertyDescriptor = BasePropertyDescriptor;
			return propertyDescriptor != null ? propertyDescriptor.PropertyType : DataValueHelper.GetTypeOfCode(_dataTypeCode);
		}

		// The type of the values that GetRowValueToCompare returns.
		internal Type GetCompareDomain()
		{
			return _compareTarget == CompareTarget.Text ? typeof(string) : GetValueDomain();
		}

		internal int CompareValues(object value, object otherValue)
		{
			if (_multipleValues)
			{
				return CompareLists(value, otherValue);
			}
			return DataValueHelper.CompareValues(DataValueHelper.GetCulture(GetDataSourceManager()), value, otherValue, _sortComparer, _compareIgnoreCase, false);
		}

		Image IMordexField.GetDisplayImage()
		{
			return BaseGetDisplayImage();
		}

		IMordexValueItemCollection IMordexField.GetFilterValueList()
		{
			return BaseGetFilterValueList();
		}

		internal void SetPropertyDescriptor(PropertyDescriptor basePropertyDescriptor)
		{
			_propertyDescriptor = basePropertyDescriptor;
		}
	}
}
