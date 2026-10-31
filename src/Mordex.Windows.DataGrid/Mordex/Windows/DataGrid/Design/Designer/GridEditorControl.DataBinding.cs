using Mordex.Windows.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;
using Microsoft.Win32;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal partial class GridEditorControl
	{
		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		private void RefreshList()
		{
			_itemListBox.DisplayMember = "Text";
			_itemListBox.Items.Clear();
			for (int i = 0; i < _arrayList.Count; i++)
			{
				_itemListBox.Items.Add(new GridValueListItem(_arrayList[i], GetItemDisplayText(i, _arrayList[i])));
			}
		}

		private string GetItemDisplayText(int value, object component)
		{
			if (component is GridCustomGroupRow gridCustomGroupRow)
			{
				if (gridCustomGroupRow.GroupCaption.Length == 0)
				{
					return "[" + value + "] - {" + gridCustomGroupRow.GetType().Name + "}";
				}
				return "[" + value + "] - '" + gridCustomGroupRow.GroupCaption + "'";
			}
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component).Find("Key", ignoreCase: true);
			string text = "";
			if (propertyDescriptor != null)
			{
				text = (string)propertyDescriptor.GetValue(component);
			}
			if (text.Length > 0)
			{
				return "[" + value + "] - '" + text + "'";
			}
			return "[" + value + "] - {" + component.GetType().Name + "}";
		}

		private void AddItem()
		{
			switch (_collectionType)
			{
			case CollectionType.GroupHeaderTotals:
				AddGroupHeaderTotalItem();
				break;
			case CollectionType.Columns:
				AddColumnItem();
				break;
			case CollectionType.ColumnSets:
				AddColumnSetItem();
				break;
			case CollectionType.ChildTables:
				AddTableItem();
				break;
			case CollectionType.DropDowns:
				AddDropDownItem();
				break;
			case CollectionType.FormatConditions:
				AddFormatConditionItem();
				break;
			case CollectionType.FormatStyles:
				AddFormatStyleItem();
				break;
			case CollectionType.Groups:
				AddGroupItem();
				break;
			case CollectionType.SortKeys:
				AddSortKeyItem();
				break;
			case CollectionType.StoredFilters:
				AddStoredFilterItem();
				break;
			case CollectionType.CustomGroups:
				AddCustomGroupItem();
				break;
			case CollectionType.CustomGroupRows:
				AddCustomGroupRowItem();
				break;
			}
		}

		private void ClearItems()
		{
			_arrayList.Clear();
			RefreshList();
			_properties.SelectedObject = null;
			ProcessEventArgs(EventArgs.Empty);
			if (_collectionType == CollectionType.ChildTables)
			{
				CommitChildTables();
				RaiseDataChangedEvent(EventArgs.Empty);
			}
			else if (_collectionType == CollectionType.DropDowns)
			{
				CommitDropDowns();
				RaiseDataChangedEvent(EventArgs.Empty);
			}
		}

		private void MoveItemUp(int otherValue)
		{
			if (otherValue > 0)
			{
				object value = _arrayList[otherValue];
				int selectedIndex = otherValue - 1;
				_arrayList[otherValue] = _arrayList[selectedIndex];
				_arrayList[selectedIndex] = value;
				RefreshList();
				_itemListBox.SelectedIndex = selectedIndex;
			}
		}

		private void MoveItemDown(int otherValue)
		{
			if (otherValue < _arrayList.Count - 1)
			{
				object value = _arrayList[otherValue];
				int selectedIndex = otherValue + 1;
				_arrayList[otherValue] = _arrayList[selectedIndex];
				_arrayList[selectedIndex] = value;
				RefreshList();
				_itemListBox.SelectedIndex = selectedIndex;
			}
		}

		private void AddColumnItem()
		{
			AddColumnDialog addColumnDialog = new AddColumnDialog();
			GridTable gridTable = ((GridColumnCollection)_selectedCollection).GetOwnerTable();
			GridColumn gridColumn = addColumnDialog.ShowAndCreateColumn(gridTable, GenerateUniqueKey(typeof(GridColumn), "Column", 1), FindForm());
			if (gridColumn != null)
			{
				AddAndSelectItem(gridColumn);
			}
		}

		private void AddColumnSetItem()
		{
			GridColumnSet gridColumnSet = new GridColumnSet();
			gridColumnSet.SetOwnerCollection((GridColumnSetCollection)_selectedCollection);
			gridColumnSet.Key = GenerateUniqueKey(typeof(GridColumnSet), "ColumnSet", 1);
			AddAndSelectItem(gridColumnSet);
		}

		private void AddFormatConditionItem()
		{
			GridFormatCondition gridFormatCondition = new GridFormatCondition();
			gridFormatCondition.SetOwnerCollection((GridFormatConditionCollection)_selectedCollection);
			gridFormatCondition.Key = GenerateUniqueKey(typeof(GridFormatCondition), "FormatCondition", 1);
			AddAndSelectItem(gridFormatCondition);
		}

		private void AddFormatStyleItem()
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle();
			gridFormatStyle._owner = (GridFormatStyleCollection)_selectedCollection;
			gridFormatStyle.Key = GenerateUniqueKey(typeof(GridFormatStyle), "FormatStyle", 1);
			AddAndSelectItem(gridFormatStyle);
		}

		private void AddGroupItem()
		{
			GridGroup gridGroup = new GridGroup();
			gridGroup._ownerTable = ((GridGroupCollection)_selectedCollection).GetOwnerTable();
			AddAndSelectItem(gridGroup);
		}

		private void AddCustomGroupItem()
		{
			GridCustomGroup gridCustomGroup = new GridCustomGroup();
			gridCustomGroup.Key = GenerateUniqueKey(typeof(GridCustomGroup), "CustomGroup", 1);
			gridCustomGroup._ownerTable = ((GridCustomGroupCollection)_selectedCollection).Table;
			AddAndSelectItem(gridCustomGroup);
		}

		private void AddTableItem()
		{
			GridTable gridTable = new GridTable();
			gridTable.Key = GenerateUniqueKey(typeof(GridTable), "Table", 1);
			AddAndSelectItem(gridTable);
			CommitChildTables();
			RaiseDataChangedEvent(EventArgs.Empty);
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			if (eventArgs.ChangedItem.Label == "Key" || (eventArgs.ChangedItem.Label == "GroupCaption" && _selectedCollection is GridCustomGroupRowCollection))
			{
				int selectedIndex = _itemListBox.SelectedIndex;
				RefreshList();
				_itemListBox.SelectedIndex = selectedIndex;
			}
			else if (eventArgs.ChangedItem.Label == "HasValueList")
			{
				ProcessEventArgs(EventArgs.Empty);
			}
			OnDataChanged(EventArgs.Empty);
		}
	}
}
