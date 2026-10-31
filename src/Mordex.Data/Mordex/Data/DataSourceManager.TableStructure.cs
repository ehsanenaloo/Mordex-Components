using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	partial class DataSourceManager
	{
		public void CreateTableStructure()
		{
			CreateTableStructure(hierarchical: false);
		}

		public void CreateTableStructure(bool hierarchical)
		{
			if (_bindableRootList != null)
			{
				MordexTableBase mordexTableBase = BuildTable(null, _listInspector, hierarchical, "", null);
				if (mordexTableBase != null)
				{
					((IMordexTable)mordexTableBase).Key = _listInspector.GetListName(null);
					RootTable = mordexTableBase;
				}
			}
		}

		internal PropertyDescriptor[] GetTablePropertyDescriptors(IMordexTable iMordexTable, bool boolean, bool includeListProperties)
		{
			if (iMordexTable.DataSourceManager != this)
			{
				throw new InvalidOperationException("Can not retrieve properties from tables that does not belong the manager.");
			}
			if (RootList != null)
			{
				BoundListInspector listInspector = new BoundListInspector(RootList, this);
				ArrayList arrayList = new ArrayList();
				for (IMordexTable mordexTable = iMordexTable; mordexTable != null; mordexTable = mordexTable.ParentTable)
				{
					arrayList.Insert(0, mordexTable);
				}
				PropertyDescriptorCollection propertyDescriptorCollection = null;
				if (arrayList.Count <= 1)
				{
					propertyDescriptorCollection = listInspector.GetItemProperties(null);
				}
				else
				{
					PropertyDescriptorCollection parentProperties = null;
					PropertyDescriptor[] accessorPath = new PropertyDescriptor[arrayList.Count - 1];
					PropertyDescriptor[] accessorPathSoFar = null;
					for (int i = 1; i < arrayList.Count; i++)
					{
						parentProperties = listInspector.GetItemProperties(accessorPathSoFar);
						foreach (PropertyDescriptor item in parentProperties)
						{
							if (IsListProperty(item) && string.Compare(item.Name, ((IMordexTable)arrayList[i]).DataMember, ignoreCase: true) == 0)
							{
								accessorPath[i - 1] = item;
								accessorPathSoFar = new PropertyDescriptor[i];
								for (int j = 0; j < i; j++)
								{
									accessorPathSoFar[j] = accessorPath[j];
								}
							}
						}
					}
					propertyDescriptorCollection = listInspector.GetItemProperties(accessorPath);
				}
				if (propertyDescriptorCollection != null)
				{
					ArrayList selectedProperties = new ArrayList();
					foreach (PropertyDescriptor item2 in propertyDescriptorCollection)
					{
						if (IsListProperty(item2))
						{
							if (includeListProperties)
							{
								selectedProperties.Add(item2);
							}
						}
						else if (boolean)
						{
							selectedProperties.Add(item2);
						}
					}
					PropertyDescriptor[] result = new PropertyDescriptor[selectedProperties.Count];
					for (int k = 0; k < selectedProperties.Count; k++)
					{
						result[k] = (PropertyDescriptor)selectedProperties[k];
					}
					return result;
				}
			}
			return null;
		}

		internal void CreateFieldsFromProperties(BoundListInspector listInspector, IMordexTable iMordexTable, IList iList)
		{
			List<MordexFieldBase> list = new List<MordexFieldBase>();
			foreach (PropertyDescriptor item in iList)
			{
				if (!IsListProperty(item))
				{
					MordexFieldBase mordexFieldBase = CreateMordexField();
					mordexFieldBase.InitializeFromDescriptor(item);
					if (listInspector != null)
					{
						mordexFieldBase.InitializeFromDataTable(listInspector.GetDataTableForTable(iMordexTable));
					}
					list.Add(mordexFieldBase);
					iMordexTable.Fields.Add(mordexFieldBase);
				}
			}
		}

		internal DataTable GetDataTableForList(IList list)
		{
			return GetDataTableCore(list);
		}

		protected virtual DataTable GetDataTableCore(IList list)
		{
			return null;
		}

		internal void CreateChildTables(IMordexTable iMordexTable)
		{
			if (iMordexTable.DataSourceManager != this)
			{
				throw new InvalidOperationException("Can not retrieve child tables from tables that does not belong the manager.");
			}
			if (RootList == null)
			{
				return;
			}
			BoundListInspector listInspector = new BoundListInspector(RootList, this);
			ArrayList arrayList = new ArrayList();
			for (IMordexTable mordexTable = iMordexTable; mordexTable != null; mordexTable = mordexTable.ParentTable)
			{
				arrayList.Insert(0, mordexTable);
			}
			PropertyDescriptor[] accessorPath = new PropertyDescriptor[arrayList.Count];
			PropertyDescriptorCollection propertyDescriptorCollection = null;
			if (arrayList.Count <= 1)
			{
				propertyDescriptorCollection = listInspector.GetItemProperties(null);
			}
			else
			{
				PropertyDescriptorCollection parentProperties = null;
				PropertyDescriptor[] parentPath = null;
				for (int i = 1; i < arrayList.Count; i++)
				{
					parentProperties = listInspector.GetItemProperties(parentPath);
					foreach (PropertyDescriptor item in parentProperties)
					{
						if (IsListProperty(item) && !item.PropertyType.IsArray && string.Compare(item.Name, ((IMordexTable)arrayList[i]).DataMember, ignoreCase: true) == 0)
						{
							accessorPath[i - 1] = item;
							parentPath = new PropertyDescriptor[i];
							for (int j = 0; j < i; j++)
							{
								parentPath[j] = accessorPath[j];
							}
						}
					}
				}
				parentPath = new PropertyDescriptor[accessorPath.Length - 1];
				for (int k = 0; k < parentPath.Length; k++)
				{
					parentPath[k] = accessorPath[k];
				}
				propertyDescriptorCollection = listInspector.GetItemProperties(parentPath);
			}
			if (propertyDescriptorCollection == null)
			{
				return;
			}
			foreach (PropertyDescriptor item2 in propertyDescriptorCollection)
			{
				if (IsListProperty(item2) && !item2.PropertyType.IsArray)
				{
					accessorPath[accessorPath.Length - 1] = item2;
					BuildTable(iMordexTable, listInspector, true, item2.Name, accessorPath);
				}
			}
		}

		private MordexTableBase BuildTable(IMordexTable iMordexTable, BoundListInspector listInspector, bool boolean, string dataMember, PropertyDescriptor[] propertyDescriptorArray)
		{
			try
			{
				IMordexTable mordexTable = CreateMordexTable();
				iMordexTable?.ChildTables.Add(mordexTable);
				mordexTable.Initialize(dataMember);
				mordexTable.TableCaption = listInspector.GetListName(propertyDescriptorArray);
				PropertyDescriptorCollection propertyDescriptorCollection = listInspector.GetItemProperties(propertyDescriptorArray);
				if (propertyDescriptorCollection != null)
				{
					CreateFieldsFromProperties(listInspector, mordexTable, propertyDescriptorCollection);
					if (boolean)
					{
						foreach (PropertyDescriptor item in propertyDescriptorCollection)
						{
							if (!IsListProperty(item) || item.PropertyType.IsArray || item.Name == mordexTable.Key)
							{
								continue;
							}
							PropertyDescriptor[] childPath;
							if (propertyDescriptorArray == null)
							{
								childPath = new PropertyDescriptor[1] { item };
							}
							else
							{
								bool isRecursive = false;
								foreach (PropertyDescriptor propertyDescriptor2 in propertyDescriptorArray)
								{
									if (propertyDescriptor2.Equals(item) || propertyDescriptor2.Name == item.Name)
									{
										isRecursive = true;
										break;
									}
								}
								if (isRecursive)
								{
									continue;
								}
								childPath = new PropertyDescriptor[propertyDescriptorArray.Length + 1];
								propertyDescriptorArray.CopyTo(childPath, 0);
								childPath[propertyDescriptorArray.Length] = item;
							}
							BuildTable(mordexTable, listInspector, boolean, item.Name, childPath);
						}
					}
					return (MordexTableBase)mordexTable;
				}
				return null;
			}
			catch (Exception ex) when (ex is TargetInvocationException || ex is InvalidOperationException || ex is NotSupportedException || ex is ArgumentException)
			{
				// A data member that cannot be inspected is not shown as a child table.
				return null;
			}
		}

		private static bool IsListProperty(PropertyDescriptor propertyDescriptor)
		{
			if (typeof(IList).IsAssignableFrom(propertyDescriptor.PropertyType) || typeof(IListSource).IsAssignableFrom(propertyDescriptor.PropertyType))
			{
				if (propertyDescriptor.PropertyType != typeof(byte[]))
				{
					return true;
				}
				return false;
			}
			return false;
		}

		private RelationChildListProvider GetOrCreateRelationProvider(DataRowView dataRowView, string strB)
		{
			if (_relationProvidersByName == null)
			{
				_relationProvidersByName = new Hashtable();
			}
			RelationChildListProvider relationLists = (RelationChildListProvider)_relationProvidersByName[strB];
			if (relationLists == null)
			{
				DataTable table = dataRowView.Row.Table;
				foreach (DataRelation childRelation in table.ChildRelations)
				{
					if (string.Compare(childRelation.RelationName, strB, ignoreCase: true) == 0)
					{
						relationLists = new RelationChildListProvider(this, childRelation);
						relationLists._owner = this;
						_relationProvidersByName.Add(strB, relationLists);
					}
				}
			}
			return relationLists;
		}

		internal BindableList GetChildBindableList(MordexRow row, MordexTableBase childTable)
		{
			if (_useSampleRows)
			{
				ArrayList arrayList = new ArrayList();
				if (row == null)
				{
					arrayList.Add(new SampleRow());
					arrayList.Add(new SampleRow());
					arrayList.Add(new SampleRow());
				}
				else if (!childTable.IsSelfReferencingTable())
				{
					arrayList.Add(new SampleRow());
				}
				return new BindableList(arrayList);
			}
			if (row == null)
			{
				return _bindableRootList;
			}
			if (_boundMode == BoundMode.Unbound)
			{
				if (!childTable.HasOwnerField())
				{
					return new BindableList(new List<MordexUnboundDataRow>());
				}
				return new BindableList(childTable.GetOwnerField().GetRowValue(row) as IList);
			}
			IList list = null;
			if (!childTable.HasOwnerField() && ((IMordexTable)childTable).HierarchicalMode != HierarchicalMode.SelfReferencing)
			{
				list = GetCustomChildList(row, childTable);
				if (list != null)
				{
					return new BindableList(list);
				}
			}
			object dataRow = row.DataRow;
			string text = "";
			if (row.Table == childTable)
			{
				if (row is DataHolderRow dataHolderRow)
				{
					return new BindableList(dataHolderRow.GetSelfReferencingChildren());
				}
				return null;
			}
			text = ((IMordexTable)childTable).DataMember;
			if (dataRow is DataRowView dataRowView)
			{
				RelationChildListProvider relationLists = GetOrCreateRelationProvider(dataRowView, text);
				if (relationLists != null)
				{
					list = relationLists.GetOrCreateCollection(dataRowView);
				}
			}
			if (list == null)
			{
				list = ((childTable.HasOwnerField() && childTable.GetOwnerField() != null) ? DataValueHelper.GetList(childTable.GetOwnerField().GetItemValue(row.DataRow)) : ((!(row is DataHolderRow)) ? childTable.GetChildListOfRow(dataRow) : ((DataHolderRow)row).GetChildTableList(childTable)));
			}
			if (list != null)
			{
				return new BindableList(list);
			}
			return null;
		}

		protected virtual IList GetCustomChildList(MordexRow row, MordexTableBase childTable)
		{
			return null;
		}

		internal void RefreshTableFilter(MordexTableBase table)
		{
			if (_mordexRootRow != null)
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.SuspendIndex();
				}
				try
				{
					_mordexRootRow.RefreshFilter(table);
				}
				finally
				{
					if (_mordexRowManager != null)
					{
						_mordexRowManager.ResumeIndex();
					}
				}
			}
			if (_nonGroupedRootRow != null)
			{
				_nonGroupedRootRow.RefreshFilter(table);
			}
		}

		protected virtual MordexTableBase CreateMordexTable()
		{
			return new MordexTableBase();
		}

		protected virtual MordexFieldBase CreateMordexField()
		{
			return new MordexFieldBase();
		}

		protected virtual void OnRowChanged(MordexRow row)
		{
		}

		internal void HandleRowManagerChanged(MordexRowManager mordexRowManager)
		{
			if (mordexRowManager == _mordexRowManager)
			{
				OnRowManagerChange();
			}
			else if (mordexRowManager == _nonGroupedRowManager)
			{
				OnNonGroupRowManagerChange();
			}
		}

		protected virtual void OnRowManagerChange()
		{
		}

		protected virtual void OnNonGroupRowManagerChange()
		{
		}

		internal bool CanChangeExpandedState(MordexRow row)
		{
			if (!row.IsVirtualRow())
			{
				if (row.Expanded)
				{
					return OnCollapsingRow(row);
				}
				return OnExpandingRow(row);
			}
			return true;
		}

		protected virtual bool OnCollapsingRow(MordexRow row)
		{
			return true;
		}

		protected virtual bool OnExpandingRow(MordexRow row)
		{
			return true;
		}

		protected virtual void OnRowExpanded(MordexRow row)
		{
		}

		protected virtual void OnRowCollapsed(MordexRow row)
		{
		}

		internal void RaiseRowExpandedChanged(MordexRow row)
		{
			if (!row.IsVirtualRow())
			{
				if (row.Expanded)
				{
					OnRowExpanded(row);
				}
				else
				{
					OnRowCollapsed(row);
				}
			}
		}

		internal void HandleRowChange(MordexRow row, RowChangeType change, MordexRootRow mordexRootRow)
		{
			if (mordexRootRow == RootRow)
			{
				if (_mordexRowManager != null)
				{
					_mordexRowManager.HandleRowChange(row, change);
				}
				OnRowChange(row, change);
			}
			else if (mordexRootRow == NonGroupedRootRow)
			{
				if (_nonGroupedRowManager != null)
				{
					_nonGroupedRowManager.HandleRowChange(row, change);
				}
				OnRowChange(row, change);
			}
		}

		public virtual void SourceListReset(object parentDataRow, MordexTableBase table)
		{
			_mordexRootRow.RefreshSort(table);
		}

		internal bool ShouldIncludeTotalRowInRowManager()
		{
			return IncludeTotalRowInRowManager();
		}

		protected virtual bool IncludeTotalRowInRowManager()
		{
			return true;
		}

		protected virtual void OnRowChange(MordexRow row, RowChangeType change)
		{
		}

		internal void RaiseFetchExternalData(MordexRow row)
		{
			OnFetchExternalData(row);
		}

		protected virtual void OnFetchExternalData(MordexRow row)
		{
		}

		internal void RaiseReleaseExternalData(MordexRow row)
		{
			OnReleaseExternalData(row);
		}

		protected virtual void OnReleaseExternalData(MordexRow row)
		{
		}

		public MordexRow FindDataRow(object dataRow)
		{
			return FindDataRowInTable(dataRow, (MordexTableBase)_rootTable);
		}

		private MordexRow FindDataRowInTable(object value, MordexTableBase mordexTableBase)
		{
			if (mordexTableBase == null || _mordexRootRow == null || value == null)
			{
				return null;
			}
			MordexRow mordexRow = null;
			mordexRow = _mordexRootRow.FindRowForDataItem(value, mordexTableBase);
			if (mordexRow == null)
			{
				foreach (MordexTableBase childTable in ((IMordexTable)mordexTableBase).ChildTables)
				{
					MordexRow childResult = FindDataRowInTable(value, childTable);
					if (childResult != null)
					{
						return childResult;
					}
				}
			}
			return mordexRow;
		}

		public MordexRow[] FindAll(MordexConditionBase condition)
		{
			ArrayList arrayList = new ArrayList();
			MordexRow mordexRow = null;
			while (true)
			{
				mordexRow = FindNextMatch(condition, mordexRow);
				if (mordexRow == null)
				{
					break;
				}
				arrayList.Add(mordexRow);
			}
			MordexRow[] result = new MordexRow[arrayList.Count];
			arrayList.CopyTo(result);
			return result;
		}

		public MordexRow FindRow(MordexConditionBase condition, MordexRow start, int direction)
		{
			return FindRow(condition, start, direction, includeFilteredRows: true);
		}

		public MordexRow FindRow(MordexConditionBase condition, MordexRow start, int direction, bool includeFilteredRows)
		{
			if (_mordexRootRow == null)
			{
				return null;
			}
			MordexTableBase mordexTableBase = condition.FindTable();
			bool isNestedTable = false;
			if (mordexTableBase != null && mordexTableBase.IsSelfReferencingTable())
			{
				isNestedTable = true;
			}
			if (((IMordexTable)mordexTableBase).ParentTable != null)
			{
				isNestedTable = true;
			}
			MordexRow[] rows = _mordexRootRow.CollectDataRowsToArray(mordexTableBase, isNestedTable, includeFilteredRows, true);
			bool startPassed = start == null;
			if (direction == 1)
			{
				foreach (MordexRow mordexRow in rows)
				{
					if (startPassed)
					{
						if (condition.EvaluateRow(mordexRow))
						{
							return mordexRow;
						}
					}
					else
					{
						startPassed = mordexRow == start;
					}
				}
				return null;
			}
			for (int index = rows.Length - 1; index >= 0; index--)
			{
				MordexRow row = rows[index];
				if (startPassed)
				{
					if (condition.EvaluateRow(row))
					{
						return row;
					}
				}
				else
				{
					startPassed = row == start;
				}
			}
			return null;
		}

		private MordexRow FindNextMatch(MordexConditionBase mordexConditionBase, MordexRow afterRow)
		{
			MordexTableBase mordexTableBase = mordexConditionBase.FindTable();
			MordexRow mordexRow = ((afterRow == null) ? _mordexRootRow.FirstChild : ((!(afterRow is MordexDataRow)) ? afterRow.NextRow : ((!afterRow.Table.IsAncestorOf(mordexTableBase)) ? afterRow.NextRowLoaded : afterRow.NextRow)));
			while (mordexRow != null)
			{
				if (mordexRow is MordexDataRow)
				{
					if (mordexRow.Table == null)
					{
						return null;
					}
					if (mordexRow.Table == mordexTableBase && mordexConditionBase.EvaluateRow(mordexRow))
					{
						return mordexRow;
					}
					mordexRow = ((!mordexRow.Table.IsAncestorOf(mordexTableBase)) ? mordexRow.NextRowLoaded : mordexRow.NextRow);
				}
				else
				{
					mordexRow = mordexRow.NextRow;
				}
			}
			return null;
		}

		public MordexDataRow[] GetRowsOfDataRow(object dataRow)
		{
			return _rootListHolderRow.FindLinkedRows(dataRow);
		}

		internal void RaiseSuspendRowManagerIndex()
		{
			OnSuspendRowManagerIndex();
		}

		internal void RaiseResumeRowManagerIndex()
		{
			OnResumeRowManagerIndex();
		}

		protected virtual void OnSuspendRowManagerIndex()
		{
		}

		protected virtual void OnResumeRowManagerIndex()
		{
		}

		public virtual IMordexCondition CreateMordexCondition()
		{
			return new MordexConditionBase();
		}

		internal void RaiseDataRowRead(DataHolderRow row)
		{
			OnDataRowRead(row);
		}

		internal void RaiseDataRowDelete(DataHolderRow row)
		{
			OnDataRowDelete(row);
		}

		protected virtual void OnDataRowDelete(DataHolderRow row)
		{
		}

		protected virtual void OnDataRowRead(DataHolderRow row)
		{
		}

		public List<MordexRowCache> GetDataSourceCache()
		{
			List<MordexRowCache> list = new List<MordexRowCache>();
			_rootListHolderRow.BuildTreeCache(this, list);
			foreach (MordexRowCache item in list)
			{
				item.MoveValuesToElementRow(list);
			}
			return list;
		}

		public List<MordexRowCache> GetRowTreeCache()
		{
			List<MordexRowCache> list = new List<MordexRowCache>();
			_mordexRootRow.BuildTreeCache(this, list);
			return list;
		}

		internal MordexRowCache CreateRowCacheFor(MordexRow mordexRow)
		{
			MordexRowCache mordexRowCache = CreateRowCache();
			mordexRowCache.SetSaveFieldValues(_saveFieldValuesInCacheRow);
			if (mordexRow != null)
			{
				mordexRowCache.SaveRow(mordexRow);
			}
			return mordexRowCache;
		}

		protected virtual MordexRowCache CreateRowCache()
		{
			return new MordexRowCache();
		}

		public void LoadCacheLight(IList treeRows)
		{
			if (_mordexRowManager != null)
			{
				_mordexRowManager.ClearOrAssignRowPositions(true);
				_mordexRowManager = null;
			}
			MordexRowCache mordexRowCache = (MordexRowCache)treeRows[0];
			AttachCacheChildren(treeRows, mordexRowCache, 1, 0);
			_mordexRootRow.LoadTreeCacheLight(this, mordexRowCache);
			_mordexRowManager = new MordexRowManager(_mordexRootRow, this, 0);
		}

		public void LoadRowCache(IList dataRows, IList treeRows)
		{
			if (dataRows.Count > 0)
			{
				MordexRowCache mordexRowCache = (MordexRowCache)dataRows[0];
				AttachCacheChildren(dataRows, mordexRowCache, 1, 0);
				MordexRowCache treeRoot = (MordexRowCache)treeRows[0];
				AttachCacheChildren(treeRows, treeRoot, 1, 0);
				_rootListHolderRow = new DataSourceManagerTableListHolderRow(this, _rootTable);
				object[] loadedRows = new object[dataRows.Count];
				_rootListHolderRow.LoadFromRowCache(mordexRowCache, loadedRows);
				_mordexRootRow = new MordexRootRow(this, preventGroups: false);
				_mordexRootRow.LoadTreeCacheRows(this, treeRoot, loadedRows);
				_mordexRowManager = new MordexRowManager(_mordexRootRow, this, 0);
			}
		}

		private int AttachCacheChildren(IList iList, MordexRowCache parentCache, int integer, int level)
		{
			int index = integer;
			while (index < iList.Count)
			{
				MordexRowCache mordexRowCache = iList[index] as MordexRowCache;
				if (mordexRowCache.GetParentIndex() != level)
				{
					break;
				}
				if (mordexRowCache.RowType == MordexRowCache.TypeSelfReferencingElement)
				{
					if (parentCache._childElements == null)
					{
						parentCache._childElements = new List<MordexRowCache>();
					}
					parentCache._childElements.Add(mordexRowCache);
					index++;
					continue;
				}
				if (parentCache._childRows == null)
				{
					parentCache._childRows = new List<MordexRowCache>();
				}
				parentCache._childRows.Add(mordexRowCache);
				mordexRowCache._childRows = null;
				index = AttachCacheChildren(iList, mordexRowCache, index + 1, mordexRowCache.GetCacheIndex());
			}
			return index;
		}

		protected virtual MordexValueItemCollectionBase CreateValueList()
		{
			throw new NotImplementedException();
		}

		public IMordexValueItemCollection GetNonDuplicatedList(MordexTableBase table, MordexFieldBase field)
		{
			return GetNonDuplicatedList(table, field, excludeNonVisible: false);
		}

		public IMordexValueItemCollection GetNonDuplicatedList(MordexTableBase table, MordexFieldBase field, bool excludeNonVisible)
		{
			IMordexValueItemCollection mordexValueItemCollection = CreateValueList();
			if (RootRow != null && mordexValueItemCollection != null)
			{
				MordexRow[] dataRows = RootRow.GetDataRows(table, !excludeNonVisible);
				DataCollection dataCollection = new DataCollection(CurrentCulture);
				if (((IMordexField)field).DataTypeCode == TypeCode.DateTime)
				{
					MordexRow[] dateRows = dataRows;
					foreach (MordexRow mordexRow in dateRows)
					{
						object fieldValue = field.GetRowValue(mordexRow);
						if (fieldValue is DateTime dateTime)
						{
							fieldValue = dateTime.Date;
						}
						dataCollection.AddDistinct(fieldValue);
					}
				}
				else
				{
					MordexRow[] rows = dataRows;
					foreach (MordexRow mordexRow2 in rows)
					{
						dataCollection.AddDistinct(field.GetRowValue(mordexRow2));
					}
				}
				foreach (object item in dataCollection)
				{
					if (item != null)
					{
						mordexValueItemCollection.Add(item, field.FormatWithFieldSettings(item));
					}
				}
				if (((IMordexField)field).CompareTarget == CompareTarget.Text)
				{
					mordexValueItemCollection.Sort();
				}
			}
			return mordexValueItemCollection;
		}
	}
}
