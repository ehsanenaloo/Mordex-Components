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
		private void InitializeCollectionEditor()
		{
			if (_selectedCollection == null)
			{
				_collectionType = CollectionType.None;
			}
			else if (_selectedCollection is GridColumnCollection)
			{
				_collectionType = CollectionType.Columns;
			}
			else if (_selectedCollection is GridColumnSetCollection)
			{
				_collectionType = CollectionType.ColumnSets;
			}
			else if (_selectedCollection is GridChildTableCollection)
			{
				_collectionType = CollectionType.ChildTables;
			}
			else if (_selectedCollection is GridFormatConditionCollection)
			{
				_collectionType = CollectionType.FormatConditions;
			}
			else if (_selectedCollection is GridFormatStyleCollection)
			{
				_collectionType = CollectionType.FormatStyles;
			}
			else if (_selectedCollection is GridSortKeyCollection)
			{
				_collectionType = CollectionType.SortKeys;
			}
			else if (_selectedCollection is GridGroupCollection)
			{
				_collectionType = CollectionType.Groups;
			}
			else if (_selectedCollection is GridDropDownCollection)
			{
				_collectionType = CollectionType.DropDowns;
			}
			else if (_selectedCollection is GridGroupHeaderTotalCollection)
			{
				_collectionType = CollectionType.GroupHeaderTotals;
			}
			else if (_selectedCollection is GridStoredFilterCollection)
			{
				_collectionType = CollectionType.StoredFilters;
			}
			else if (_selectedCollection is GridCustomGroupCollection)
			{
				_collectionType = CollectionType.CustomGroups;
			}
			else if (_selectedCollection is GridCustomGroupRowCollection)
			{
				_collectionType = CollectionType.CustomGroupRows;
			}
			if (_collectionType == CollectionType.Columns)
			{
				if (((GridColumnCollection)_selectedCollection).DataGrid is GridDropDown)
				{
					_tbtnValueList.Visible = false;
				}
				else
				{
					_tbtnValueList.Visible = true;
				}
				_tbtnGroupProperties.Visible = true;
			}
			else
			{
				_tbtnGroupProperties.Visible = false;
				_tbtnValueList.Visible = false;
			}
			if (ShouldGroupProperties())
			{
				_properties.PropertySort = PropertySort.CategorizedAlphabetical;
				_tbtnGroupProperties.Pushed = true;
			}
			else
			{
				_properties.PropertySort = PropertySort.Alphabetical;
				_tbtnGroupProperties.Pushed = false;
			}
			_tbtnColumnSets.Visible = _collectionType == CollectionType.ColumnSets;
			if (_selectedCollection != null)
			{
				_arrayList = new ArrayList(_selectedCollection.Count);
				foreach (object item in _selectedCollection)
				{
					_arrayList.Add(item);
				}
			}
			else
			{
				_arrayList = new ArrayList();
			}
			RefreshList();
			if (_itemListBox.Items.Count > 0)
			{
				_itemListBox.SelectedIndex = 0;
			}
			UpdateToolbarState();
		}

		bool IGridElement.CommitChanges()
		{
			switch (_collectionType)
			{
			case CollectionType.ChildTables:
				return CommitChildTables();
			case CollectionType.Columns:
				return CommitColumns();
			case CollectionType.ColumnSets:
				return CommitColumnSets();
			case CollectionType.DropDowns:
				return CommitDropDowns();
			case CollectionType.FormatConditions:
				return CommitFormatConditions();
			case CollectionType.SortKeys:
				return CommitSortKeys();
			case CollectionType.FormatStyles:
				return CommitFormatStyles();
			case CollectionType.Groups:
				return CommitGroups();
			case CollectionType.GroupHeaderTotals:
				return CommitGroupHeaderTotals();
			case CollectionType.StoredFilters:
				return CommitStoredFilters();
			case CollectionType.CustomGroups:
				return CommitCustomGroups();
			case CollectionType.CustomGroupRows:
				return CommitCustomGroupRows();
			default:
				return true;
			}
		}

		private bool CommitChildTables()
		{
			GridChildTableCollection gridChildTableCollection = (GridChildTableCollection)_selectedCollection;
			((ISupportInitialize)gridChildTableCollection.DataGrid).BeginInit();
			gridChildTableCollection.Clear();
			foreach (GridTable item in _arrayList)
			{
				gridChildTableCollection.Add(item);
			}
			((ISupportInitialize)gridChildTableCollection.DataGrid).EndInit();
			return true;
		}

		private bool ValidateFormatConditions()
		{
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridFormatCondition gridFormatCondition = (GridFormatCondition)_arrayList[i];
				if (!gridFormatCondition.IsValidForTable(((GridFormatConditionCollection)_selectedCollection)._ownerTable))
				{
					_itemListBox.SelectedIndex = i;
					MessageBox.Show("One or more conditions are not valid.\n\rColumn property must be set in the condition to be valid.", "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitFormatConditions()
		{
			if (!ValidateFormatConditions())
			{
				return false;
			}
			GridFormatConditionCollection gridFormatConditionCollection = (GridFormatConditionCollection)_selectedCollection;
			((ISupportInitialize)gridFormatConditionCollection.ComputeDataGrid()).BeginInit();
			gridFormatConditionCollection.Clear();
			GridFormatCondition[] array = new GridFormatCondition[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridFormatCondition[] otherArray = array;
			foreach (GridFormatCondition gridFormatCondition in otherArray)
			{
				gridFormatCondition.SetOwnerCollection(null);
			}
			gridFormatConditionCollection.AddRange(array);
			((ISupportInitialize)gridFormatConditionCollection.ComputeDataGrid()).EndInit();
			return true;
		}

		private bool ValidateGroups()
		{
			GridTable gridTable = ((GridGroupCollection)_selectedCollection).GetOwnerTable();
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridGroup gridGroup = (GridGroup)_arrayList[i];
				if (!gridGroup.CanGroupByTable(gridTable))
				{
					_itemListBox.SelectedIndex = i;
					MessageBox.Show("One or more Groups are not valid.\n\rColumn property must be set in the object to be valid.", "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			return true;
		}

		private bool CommitGroups()
		{
			if (!ValidateGroups())
			{
				return false;
			}
			GridGroupCollection gridGroupCollection = (GridGroupCollection)_selectedCollection;
			((ISupportInitialize)gridGroupCollection.ComputeDataGrid()).BeginInit();
			gridGroupCollection.Clear();
			GridGroup[] array = new GridGroup[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridGroup[] otherArray = array;
			foreach (GridGroup gridGroup in otherArray)
			{
				gridGroup._ownerTable = null;
			}
			gridGroupCollection.AddRange(array);
			((ISupportInitialize)gridGroupCollection.ComputeDataGrid()).EndInit();
			return true;
		}

		private bool ValidateCustomGroups()
		{
			GridTable table = ((GridCustomGroupCollection)_selectedCollection).Table;
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridCustomGroup gridCustomGroup = (GridCustomGroup)_arrayList[i];
				if (gridCustomGroup.CustomGroupType != CustomGroupType.CompositeColumns)
				{
					continue;
				}
				if (gridCustomGroup.CompositeColumns == null)
				{
					MessageBox.Show("CompositeColumns are not defined in the group", "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					_itemListBox.SelectedIndex = i;
					return false;
				}
				GridColumn[] compositeColumns = gridCustomGroup.CompositeColumns;
				foreach (GridColumn gridColumn in compositeColumns)
				{
					if (!table.CanGroupBy(gridColumn))
					{
						MessageBox.Show("Column '" + GetItemDisplayText(gridColumn.Index, gridColumn) + "' can not be used in a group a this table.", "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return false;
					}
				}
			}
			return true;
		}

		private bool ValidateCustomGroupRows()
		{
			GridTable gridTable = ((GridCustomGroupRowCollection)_selectedCollection).GetOwnerTable();
			for (int i = 0; i < _arrayList.Count; i++)
			{
				GridCustomGroupRow gridCustomGroupRow = (GridCustomGroupRow)_arrayList[i];
				if (gridCustomGroupRow.Condition != null)
				{
					try
					{
						gridCustomGroupRow.Condition.ValidateBaseCondition(gridTable);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message, "Mordex DataGrid Designer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						_itemListBox.SelectedIndex = i;
						return false;
					}
				}
			}
			return true;
		}

		private bool CommitCustomGroups()
		{
			if (!ValidateCustomGroups())
			{
				return false;
			}
			GridCustomGroupCollection gridCustomGroupCollection = (GridCustomGroupCollection)_selectedCollection;
			gridCustomGroupCollection.Clear();
			foreach (GridCustomGroup item in _arrayList)
			{
				item._ownerTable = null;
				gridCustomGroupCollection.Add(item);
			}
			GridTable table = gridCustomGroupCollection.Table;
			for (int index = table.Groups.Count - 1; index >= 0; index--)
			{
				GridGroup gridGroup = table.Groups[index];
				if (gridGroup.CustomGroup != null && !table.CustomGroups.Contains(gridGroup.CustomGroup))
				{
					table.Groups.RemoveAt(index);
				}
			}
			return true;
		}

		private bool CommitCustomGroupRows()
		{
			if (!ValidateCustomGroupRows())
			{
				return false;
			}
			GridCustomGroupRowCollection gridCustomGroupRowCollection = (GridCustomGroupRowCollection)_selectedCollection;
			gridCustomGroupRowCollection.Clear();
			foreach (GridCustomGroupRow item in _arrayList)
			{
				item._ownerTable = null;
				gridCustomGroupRowCollection.Add(item);
			}
			return true;
		}

		private bool CommitColumns()
		{
			GridColumnCollection gridColumnCollection = (GridColumnCollection)_selectedCollection;
			GridColumn[] array = new GridColumn[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridColumn[] otherArray = array;
			foreach (GridColumn gridColumn in otherArray)
			{
				if (!gridColumnCollection.Contains(gridColumn))
				{
					gridColumn.SetBaseParentCollection(null);
				}
			}
			gridColumnCollection.ReplaceColumns(array);
			return true;
		}

		private bool CommitFormatStyles()
		{
			GridFormatStyleCollection gridFormatStyleCollection = (GridFormatStyleCollection)_selectedCollection;
			((ISupportInitialize)gridFormatStyleCollection.GetOwner()).BeginInit();
			gridFormatStyleCollection.Clear();
			GridFormatStyle[] array = new GridFormatStyle[_arrayList.Count];
			_arrayList.CopyTo(array);
			GridFormatStyle[] otherArray = array;
			foreach (GridFormatStyle gridFormatStyle in otherArray)
			{
				gridFormatStyle._owner = null;
			}
			gridFormatStyleCollection.AddRange(array);
			((ISupportInitialize)gridFormatStyleCollection.GetOwner()).EndInit();
			return true;
		}

		private bool CommitColumnSets()
		{
			GridColumnSetCollection gridColumnSetCollection = (GridColumnSetCollection)_selectedCollection;
			((ISupportInitialize)gridColumnSetCollection.ComputeDataGrid()).BeginInit();
			gridColumnSetCollection.ProcessArrayList(_arrayList);
			((ISupportInitialize)gridColumnSetCollection.ComputeDataGrid()).EndInit();
			return true;
		}

		private void RemoveSelectedItem()
		{
			if (_itemListBox.SelectedItem == null)
			{
				return;
			}
			int selectedIndex = _itemListBox.SelectedIndex;
			_arrayList.Remove(((GridValueListItem)_itemListBox.SelectedItem).Value);
			RefreshList();
			if (selectedIndex < _itemListBox.Items.Count)
			{
				_itemListBox.SelectedIndex = selectedIndex;
			}
			else
			{
				_itemListBox.SelectedIndex = _itemListBox.Items.Count - 1;
				if (_itemListBox.SelectedIndex == -1)
				{
					_properties.SelectedObject = null;
					ProcessEventArgs(EventArgs.Empty);
				}
			}
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

		private void EditValueList()
		{
			GridColumn gridColumn = (GridColumn)((GridValueListItem)_itemListBox.SelectedItem).Value;
			ValueListDesigner valueListDesigner = new ValueListDesigner();
			valueListDesigner.LoadValueListItems(gridColumn.ValueList, gridColumn.Type, gridColumn.DataGrid.ComputeImageList());
			valueListDesigner.ShowDialog();
		}

		private void MoveSelectedItemUp()
		{
			MoveItemUp(_itemListBox.SelectedIndex);
		}

		private void MoveSelectedItemDown()
		{
			MoveItemDown(_itemListBox.SelectedIndex);
		}

		private void EditColumnSets()
		{
			GridTable table = ((GridColumnSetCollection)_selectedCollection)._ownerTable;
			if (table != null)
			{
				CommitColumnSets();
				ColumnSetsDesignerDialog columnSetsDesignerDialog = new ColumnSetsDesignerDialog();
				if (columnSetsDesignerDialog.ComputeDialogResult(table.DataGrid, table) == DialogResult.Cancel)
				{
					_selectedCollection = null;
					InitializeCollectionEditor();
					RaiseDataChangedEvent(EventArgs.Empty);
				}
				else
				{
					InitializeCollectionEditor();
				}
			}
		}
	}
}
