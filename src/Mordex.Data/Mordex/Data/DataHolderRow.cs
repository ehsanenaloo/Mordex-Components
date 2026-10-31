using System.Collections;
using System.ComponentModel;
using System.Data;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public class DataHolderRow : MordexRow
	{
		private UnboundFieldValues _unboundValues;

		internal int _resyncIndex;

		private ArrayList _linkedRows;

		private int _dataRowIndex;

		private object _rowKey;

		private bool _isEditing;

		private bool _isCheckedCore;

		public int DataRowIndex
		{
			get
			{
				object sourceItem = GetSourceItem();
				if (sourceItem is HierarchyNode node)
				{
					return node.GetDataRowIndex();
				}
				return _dataRowIndex;
			}
		}

		protected override bool LoadIsChecked => true;

		public override object DataRow
		{
			get
			{
				object sourceItem = GetSourceItem();
				if (sourceItem is HierarchyNode node)
				{
					return node.GetSourceItem();
				}
				return sourceItem;
			}
		}

		public override int RecordPosition => _dataRowIndex;

		public override bool IsCheckedCore
		{
			get
			{
				return _isCheckedCore;
			}
			set
			{
				_isCheckedCore = value;
			}
		}

		public bool IsCache => DataRow is MordexRowCache;

		internal DataHolderRow(int rowIndex)
		{
			_dataRowIndex = rowIndex;
			SetShouldRetrieveChildRows(true);
		}

		internal void AddToLinkedRowCollections(DataHolderRow dataHolderRow)
		{
			if (_linkedRows == null)
			{
				return;
			}
			foreach (MordexDataRow item in _linkedRows)
			{
				item.Rows.SyncDataHolderRow(dataHolderRow);
			}
		}

		internal void SetDataRowIndex(int dataRowIndex)
		{
			_dataRowIndex = dataRowIndex;
		}

		internal void ReloadChildTableRows()
		{
			if (!ChildrenLoaded)
			{
				return;
			}
			foreach (TableListHolderRow item in (IEnumerable)base.Rows)
			{
				item.ReloadFromSource();
			}
		}

		public override void SetValue(MordexFieldBase field, string dataMember, object newValue)
		{
			if (GetParentTableList() != null)
			{
				GetParentTableList().SetRowValue(DataValueHelper.GetCulture(DataSourceManager), field, DataRow, dataMember, newValue);
			}
			else
			{
				DataValueHelper.SetPropertyValue(DataValueHelper.GetCulture(DataSourceManager), DataRow, null, dataMember, newValue);
			}
		}

		internal IList GetChildTableList(IMordexTable iMordexTable)
		{
			if (iMordexTable != null)
			{
				if (GetParentTableList() != null)
				{
					object childValue = GetParentTableList().GetMemberValue(DataRow, iMordexTable.DataMember);
					if (childValue is DataTable)
					{
						return ((DataTable)childValue).DefaultView;
					}
					if (childValue is IListSource)
					{
						IListSource listSource = childValue as IListSource;
						if (!listSource.ContainsListCollection)
						{
							return listSource.GetList();
						}
						return listSource as IList;
					}
					return childValue as IList;
				}
				return ((MordexTableBase)iMordexTable).GetChildListOfRow(DataRow);
			}
			return null;
		}

		protected override object GetValueOfFieldCore(MordexFieldBase field, string dataMember)
		{
			if (field != null)
			{
				if (((IMordexField)field).MultipleValues && ((IMordexField)field).BoundMode != ColumnBoundMode.UnboundKeepValues)
				{
					foreach (TableListHolderRow item in (IEnumerable)base.Rows)
					{
						if (item.Table.GetOwnerField() == field)
						{
							object fieldList = item.GetBindableList();
							if (fieldList is BindableList)
							{
								return ((BindableList)fieldList).List;
							}
							return fieldList;
						}
					}
				}
				if (IsCache)
				{
					return ((MordexRowCache)DataRow).GetFieldValue(field.Index);
				}
				if (field != null && ((IMordexField)field).BoundMode == ColumnBoundMode.UnboundKeepValues)
				{
					return GetUnboundValue(field);
				}
				if (GetParentTableList() != null)
				{
					return GetParentTableList().GetRowValue(field, DataRow, dataMember);
				}
				return field.GetItemValue(DataRow);
			}
			return null;
		}

		private UnboundFieldValues GetUnboundValues()
		{
			if (GetSourceItem() is MordexUnboundDataRow mordexUnboundDataRow)
			{
				return mordexUnboundDataRow._fieldValues;
			}
			return _unboundValues;
		}

		private void SetUnboundValues(UnboundFieldValues values)
		{
			if (GetSourceItem() is MordexUnboundDataRow mordexUnboundDataRow)
			{
				mordexUnboundDataRow._fieldValues = values;
			}
			else
			{
				_unboundValues = values;
			}
		}

		internal object GetUnboundValue(MordexFieldBase mordexFieldBase)
		{
			if (GetUnboundValues() == null)
			{
				return null;
			}
			return GetUnboundValues().GetValue(mordexFieldBase);
		}

		internal void CopyDataRowKeys(MordexRow mordexRow)
		{
			base.DataRowKeys = mordexRow.DataRowKeys;
		}

		internal void ApplyUnboundValues(UnboundFieldValues values)
		{
			if (values == null || values.IsEmpty())
			{
				SetUnboundValues(null);
			}
			else
			{
				SetUnboundValues(values);
			}
			RefreshLinkedRows();
		}

		public override object GetValue(MordexFieldBase field)
		{
			return GetValueOfFieldCore(field, ((IMordexField)field).DataMember);
		}

		public override object GetValue(string dataMember)
		{
			if (GetParentTableList() != null)
			{
				return GetParentTableList().GetRowValue(null, DataRow, dataMember);
			}
			return DataValueHelper.GetPropertyValue(DataRow, null, dataMember);
		}

		public override void SetValue(MordexFieldBase field, object newValue)
		{
			if (IsCache)
			{
				MordexRowCache mordexRowCache = (MordexRowCache)DataRow;
				if (mordexRowCache._fieldValues != null && field.Index < mordexRowCache._fieldValues.Length)
				{
					mordexRowCache.SetFieldValue(field.Index, newValue);
				}
			}
			else if (field != null)
			{
				if (((IMordexField)field).BoundMode == ColumnBoundMode.Bound)
				{
					SetValue(field, ((IMordexField)field).DataMember, newValue);
				}
				else if (((IMordexField)field).BoundMode == ColumnBoundMode.UnboundKeepValues)
				{
					SetUnboundValue(field, newValue);
				}
			}
		}

		internal void SetUnboundValue(MordexFieldBase mordexFieldBase, object value)
		{
			if (GetUnboundValues() == null)
			{
				SetUnboundValues(new UnboundFieldValues());
			}
			GetUnboundValues().SetValue(mordexFieldBase, value);
		}

		protected override object GetRowKey()
		{
			return _rowKey;
		}

		internal void CaptureRowKey()
		{
			_rowKey = UnwrapRow(DataRow);
		}

		private TableListHolderRow GetParentTableList()
		{
			return (TableListHolderRow)ParentRow;
		}

		internal IList GetSelfReferencingChildren()
		{
			object sourceItem = GetParentTableList().GetListItem(_dataRowIndex);
			if (GetTableInterface().HierarchicalMode == HierarchicalMode.SelfReferencing)
			{
				if (sourceItem is HierarchyNode node)
				{
					return node.GetChildren();
				}
			}
			else if (GetTableInterface().HierarchicalMode == HierarchicalMode.SelfReferencingChildList && sourceItem != null)
			{
				string childListMember = GetTableInterface().SelfReferencingSettings.ChildListMember;
				if (childListMember.Length > 0)
				{
					PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(sourceItem);
					PropertyDescriptor propertyDescriptor = properties.Find(childListMember, ignoreCase: true);
					if (propertyDescriptor != null && propertyDescriptor.GetValue(sourceItem) is IList result)
					{
						return result;
					}
				}
			}
			return null;
		}

		internal HierarchyNode GetHierarchyNode()
		{
			object sourceItem = GetSourceItem();
			return sourceItem as HierarchyNode;
		}

		public override bool CanHaveChildren()
		{
			if (Table != null && GetTableInterface().HasChildTables)
			{
				return true;
			}
			return false;
		}

		protected override MordexRowCollection CreateChildRows()
		{
			return new MordexRowCollection(this, RowCollectionType.TableRows);
		}

		protected override void RetrieveChildRows()
		{
			if (Table.IsSelfReferencingTable())
			{
				base.Rows.Add(new TableListHolderRow(Table));
			}
			foreach (MordexFieldBase field in GetTableInterface().Fields)
			{
				if (field.GetMultipleValuesTable() != null)
				{
					TableListHolderRow tableListHolderRow = new TableListHolderRow(field.GetMultipleValuesTable());
					base.Rows.Add(tableListHolderRow);
					tableListHolderRow.LoadChildRows();
				}
			}
			if (Table.IsSelfReferencingTable())
			{
				return;
			}
			foreach (IMordexTable childTable in GetTableInterface().ChildTables)
			{
				base.Rows.Add(new TableListHolderRow(childTable));
			}
		}

		internal void RefreshLinkedRows()
		{
			if (DataSourceManager == null)
			{
				return;
			}
			TableListHolderRow tableListHolderRow = GetParentTableList();
			if (_isEditing)
			{
				return;
			}
			DataSourceManager.RaiseBeforeUpdateDataRow(DataRow);
			bool needsReinsert = false;
			if (_linkedRows != null)
			{
				for (int i = _linkedRows.Count - 1; i >= 0; i--)
				{
					MordexDataRow mordexDataRow = (MordexDataRow)_linkedRows[i];
					mordexDataRow.ApplyDataRowChange();
					if (mordexDataRow.ParentRow == null)
					{
						needsReinsert = true;
					}
				}
			}
			if (needsReinsert && DataSourceManager != null)
			{
				DataSourceManager.RefreshRowsForHolder(this);
				DataSourceManager.RaiseReinsertDataRow(DataRow);
			}
			tableListHolderRow?.RefreshParentLinkedRows();
		}

		internal void DisposeChildTableRows()
		{
			if (!ChildrenLoaded)
			{
				return;
			}
			foreach (TableListHolderRow item in (IEnumerable)base.Rows)
			{
				item.DisposeAllRows();
			}
		}

		internal void DeleteLinkedRows()
		{
			if (_linkedRows != null)
			{
				for (int i = _linkedRows.Count - 1; i >= 0; i--)
				{
					_ = _linkedRows[i];
					((MordexDataRow)_linkedRows[i]).DeleteRowForRemovedDataRow();
				}
			}
		}

		internal static object UnwrapRow(object value)
		{
			if (value is DataRowView dataRowView)
			{
				return dataRowView.Row;
			}
			return value;
		}

		internal void AddLinkedRow(MordexDataRow mordexDataRow)
		{
			if (_linkedRows == null)
			{
				_linkedRows = new ArrayList(1);
			}
			_linkedRows.Add(mordexDataRow);
		}

		internal void RemoveLinkedRow(MordexDataRow row)
		{
			if (_linkedRows != null)
			{
				_linkedRows.Remove(row);
			}
		}

		internal TableListHolderRow FindTableList(MordexTableBase mordexTableBase)
		{
			if (base.Rows != null)
			{
				foreach (TableListHolderRow item in (IEnumerable)base.Rows)
				{
					if (item.Table == mordexTableBase)
					{
						return item;
					}
				}
			}
			return null;
		}

		protected override int CompareTo(MordexRow row)
		{
			if (row is DataHolderRow dataHolderRow)
			{
				return _dataRowIndex.CompareTo(dataHolderRow._dataRowIndex);
			}
			return -1;
		}

		internal DataHolderRow FindHolderForTable(MordexTableBase mordexTableBase)
		{
			if (mordexTableBase == Table)
			{
				return this;
			}
			if (mordexTableBase.IsAncestorOf(Table))
			{
				MordexRow mordexRow = this;
				while (mordexRow != null && (mordexRow.Table != mordexTableBase || !(mordexRow is DataHolderRow)))
				{
					mordexRow = mordexRow.ParentRow;
				}
				return (DataHolderRow)mordexRow;
			}
			return null;
		}

		internal ArrayList GetSameTablePath()
		{
			ArrayList arrayList = new ArrayList();
			for (MordexRow mordexRow = this; mordexRow != null; mordexRow = mordexRow.ParentRow)
			{
				if (mordexRow is DataHolderRow && mordexRow.Table == Table)
				{
					arrayList.Add(mordexRow);
				}
			}
			arrayList.Reverse();
			return arrayList;
		}

		internal DataHolderRow GetTopmostSameTableHolder()
		{
			MordexRow mordexRow = this;
			DataHolderRow result = this;
			while (mordexRow != null)
			{
				if (mordexRow.Table == Table && mordexRow is DataHolderRow dataHolderRow)
				{
					result = dataHolderRow;
				}
				mordexRow = mordexRow.ParentRow;
			}
			return result;
		}

		private void CollectSubtree(ArrayList arrayList)
		{
			arrayList.Add(this);
			if (base.Rows == null)
			{
				return;
			}
			foreach (TableListHolderRow item in (IEnumerable)base.Rows)
			{
				if (item.Table != Table)
				{
					continue;
				}
				foreach (DataHolderRow item2 in (IEnumerable)item.Rows)
				{
					item2.CollectSubtree(arrayList);
				}
			}
		}

		internal IList GetRowsForGroup(MordexTableBase mordexTableBase, bool boolean, bool expandAllRows)
		{
			if (mordexTableBase == Table)
			{
				if (mordexTableBase.IsSelfReferencingTable())
				{
					if (!expandAllRows && GetTableInterface().SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.AllRows)
					{
						boolean = false;
					}
					if (!boolean || GetTableInterface().SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.ParentRows)
					{
						return new MordexRow[1] { this };
					}
					ArrayList arrayList = new ArrayList();
					CollectSubtree(arrayList);
					return arrayList;
				}
				return new DataHolderRow[1] { this };
			}
			if (mordexTableBase.IsAncestorOf(Table))
			{
				return new DataHolderRow[1] { this };
			}
			if (mordexTableBase.HasOwnerField() && ((MordexTableBase)((IMordexTable)mordexTableBase).ParentTable).IsAncestorOf(Table))
			{
				return ((DataHolderRow)GetParentTableList().ParentRow).GetRowsForGroup(mordexTableBase, boolean, expandAllRows);
			}
			ArrayList collected = new ArrayList();
			if (Table.IsSelfReferencingTable() && GetTableInterface().SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.AllRows && boolean)
			{
				ArrayList subtreeRows = new ArrayList();
				CollectSubtree(subtreeRows);
				{
					foreach (DataHolderRow item in subtreeRows)
					{
						ArrayList rowsUnderItem = new ArrayList();
						item.CollectRowsOfTable(mordexTableBase, rowsUnderItem);
						if (rowsUnderItem.Count == 0 && Table.IsAncestorOf(mordexTableBase))
						{
							rowsUnderItem.Add(item);
						}
						collected.AddRange(rowsUnderItem);
					}
					return collected;
				}
			}
			CollectRowsOfTable(mordexTableBase, collected);
			if (collected.Count == 0 && Table.IsAncestorOf(mordexTableBase))
			{
				collected.Add(this);
			}
			return collected;
		}

		internal IList GetRowsForTable(MordexTableBase mordexTableBase)
		{
			if (mordexTableBase == Table)
			{
				return new DataHolderRow[1] { this };
			}
			if (mordexTableBase.IsAncestorOf(Table))
			{
				return new DataHolderRow[1] { FindHolderForTable(mordexTableBase) };
			}
			ArrayList arrayList = new ArrayList();
			CollectRowsOfTable(mordexTableBase, arrayList);
			return arrayList;
		}

		private void CollectRowsOfTable(IMordexTable iMordexTable, IList iList)
		{
			if (iMordexTable == Table)
			{
				iList.Add(this);
			}
			else
			{
				if (base.Rows == null)
				{
					return;
				}
				foreach (TableListHolderRow item in (IEnumerable)base.Rows)
				{
					foreach (DataHolderRow item2 in (IEnumerable)item.Rows)
					{
						item2.CollectRowsOfTable(iMordexTable, iList);
					}
				}
			}
		}

		private object GetSourceItem()
		{
			if (ParentRow != null)
			{
				return GetParentTableList().GetListItem(_dataRowIndex);
			}
			return null;
		}

		public override void BeginEdit()
		{
			bool wasEditing = _isEditing;
			_isEditing = true;
			try
			{
				if (GetSourceItem() is IEditableObject editableObject)
				{
					editableObject.BeginEdit();
				}
			}
			catch
			{
				_isEditing = wasEditing;
				throw;
			}
		}

		public override void EndEdit()
		{
			_isEditing = false;
			if (GetSourceItem() is IEditableObject editableObject)
			{
				editableObject.EndEdit();
			}
			if (GetParentTableList() != null)
			{
				ResyncChildLists();
				GetParentTableList().RefreshRowAt(_dataRowIndex);
			}
		}

		internal void ResyncChildLists()
		{
			if (GetParentTableList() == null || !ChildrenLoaded)
			{
				return;
			}
			foreach (TableListHolderRow item in (IEnumerable)base.Rows)
			{
				if (item.Table.GetOwnerField() != null && item.GetBindableList() != null && !item.GetBindableList().SupportsChangeNotification)
				{
					item.ForceResync();
				}
			}
		}

		public override void CancelEdit()
		{
			_isEditing = false;
			if (GetSourceItem() is IEditableObject editableObject)
			{
				editableObject.CancelEdit();
			}
		}

		internal MordexDataRow[] GetLinkedRows()
		{
			if (_linkedRows != null && _linkedRows.Count > 0)
			{
				MordexDataRow[] rows = new MordexDataRow[_linkedRows.Count];
				for (int i = 0; i < _linkedRows.Count; i++)
				{
					rows[i] = (MordexDataRow)_linkedRows[i];
				}
				return rows;
			}
			return null;
		}

		public override bool CanDelete()
		{
			return GetParentTableList().GetBindableList().AllowsRemove();
		}

		public override void Delete()
		{
			GetParentTableList().TryDeleteRow(this, _dataRowIndex);
		}
	}
}
