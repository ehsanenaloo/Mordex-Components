using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public class MordexRowCollection : IList, ICollection, IEnumerable
	{
		// Holds the rows that share one key when the source list has the same item more than once.
		private sealed class KeyedRows : List<MordexRow>
		{
		}

		private bool _ignoreCase;

		private MordexRow _parentRow;

		private IMordexGroup _group;

		private ArrayList _rows;

		private Hashtable _rowsByKey;

		private RowCollectionType _collectionType;

		private bool _isSorted;

		private bool _isLoading;

		public int VisibleCount
		{
			get
			{
				int visibleCount = 0;
				for (int i = 0; i < Count; i++)
				{
					MordexRow mordexRow = this[i];
					if (!mordexRow.Visible)
					{
						continue;
					}
					if (mordexRow.IsVirtualRow())
					{
						if (mordexRow.Table != null && !mordexRow.Table.HasOwnerField())
						{
							visibleCount += mordexRow.Rows.VisibleCount;
						}
					}
					else
					{
						visibleCount++;
					}
				}
				return visibleCount;
			}
		}

		public MordexGroupBase Group
		{
			get
			{
				return (MordexGroupBase)_group;
			}
			set
			{
				if (_group == value)
				{
					return;
				}
				_group = value;
				_ignoreCase = false;
				if (_group != null && _group.Field != null && _group.CustomGroup == null)
				{
					_ignoreCase = _group.Field.CompareIgnoreCase;
				}
				if (_group == null || _group.CustomGroup == null || _group.CustomGroup.CustomGroupType != CustomGroupType.ConditionalGroupRows)
				{
					return;
				}
				bool hasRemainingRowsGroup = false;
				bool expanded = true;
				if (ParentRow != null && ParentRow.GetTableInterface() != null && ParentRow.Table.GetEffectiveGroupMode() == GroupMode.Collapsed)
				{
					expanded = false;
				}
				_collectionType = RowCollectionType.CustomGroupRows;
				Clear();
				BeginLoad();
				try
				{
					foreach (IMordexCustomGroupRow groupRow in _group.CustomGroup.GroupRows)
					{
						MordexRow mordexRow = new MordexGroupRow(groupRow, groupRow);
						mordexRow.Expanded = expanded;
						mordexRow.Visible = groupRow.ShowWhenEmpty;
						if (groupRow.ShowRemainingRows)
						{
							hasRemainingRowsGroup = true;
						}
						Add(mordexRow);
					}
					if (!hasRemainingRowsGroup)
					{
						IMordexCustomGroupRow remainingRowsGroup = MordexCustomGroupRowBase.CreateDefault();
						remainingRowsGroup.ShowRemainingRows = true;
						remainingRowsGroup.ShowWhenEmpty = false;
						MordexGroupRow mordexGroupRow = new MordexGroupRow(remainingRowsGroup, remainingRowsGroup);
						mordexGroupRow._alwaysHidden = true;
						mordexGroupRow.Visible = false;
						Add(mordexGroupRow);
					}
				}
				finally
				{
					EndLoad();
				}
			}
		}

		public MordexRow ParentRow => _parentRow;

		public int Count => _rows.Count;

		public MordexRow this[int index] => (MordexRow)_rows[index];

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => _rows.SyncRoot;

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return _rows[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal MordexRowCollection(MordexRow parentRow, RowCollectionType collectionType)
			: this()
		{
			_parentRow = parentRow;
			_collectionType = collectionType;
		}

		internal RowCollectionType GetCollectionType()
		{
			return _collectionType;
		}

		internal void IndexRow(MordexRow mordexRow)
		{
			if (_rowsByKey == null)
			{
				if (GetCollectionType() == RowCollectionType.GroupRows && Group != null && Group.GetGroupComparer() is IEqualityComparer)
				{
					_rowsByKey = new Hashtable(Group.GetGroupComparer() as IEqualityComparer);
				}
				else
				{
					_rowsByKey = new Hashtable();
				}
			}
			object key = mordexRow.GetCachedRowKey() ?? DBNull.Value;
			object existing = _rowsByKey[key];
			if (existing == null)
			{
				_rowsByKey.Add(key, mordexRow);
			}
			else if (existing is KeyedRows keyedRows)
			{
				if (!keyedRows.Contains(mordexRow))
				{
					keyedRows.Add(mordexRow);
				}
			}
			else if (existing != mordexRow)
			{
				_rowsByKey[key] = new KeyedRows { (MordexRow)existing, mordexRow };
			}
		}

		internal void UnindexRow(MordexRow mordexRow)
		{
			if (_rowsByKey != null)
			{
				object key = mordexRow.GetCachedRowKey() ?? DBNull.Value;
				object existing = _rowsByKey[key];
				if (existing == mordexRow)
				{
					_rowsByKey.Remove(key);
				}
				else if (existing is KeyedRows keyedRows)
				{
					keyedRows.Remove(mordexRow);
					if (keyedRows.Count == 1)
					{
						_rowsByKey[key] = keyedRows[0];
					}
				}
			}
		}

		private MordexDataRow FindRowForHolder(DataHolderRow dataHolderRow)
		{
			object existing = _rowsByKey?[dataHolderRow.GetCachedRowKey() ?? DBNull.Value];
			if (existing is KeyedRows keyedRows)
			{
				foreach (MordexRow item in keyedRows)
				{
					if (item is MordexDataRow keyedDataRow && keyedDataRow.DataHolder == dataHolderRow)
					{
						return keyedDataRow;
					}
				}
				return null;
			}
			if (existing is MordexDataRow mordexDataRow && mordexDataRow.DataHolder == dataHolderRow)
			{
				return mordexDataRow;
			}
			return null;
		}

		protected MordexRowCollection()
		{
			_rows = new ArrayList();
		}

		public int Add(MordexRow row)
		{
			int insertIndex = Count;
			if (_isSorted)
			{
				if (row is MordexDataRow mordexDataRow)
				{
					mordexDataRow.ResetSortValues();
					mordexDataRow.LoadSortValues(true);
				}
				insertIndex = _rows.BinarySearch(row);
				if (insertIndex < 0)
				{
					insertIndex = -insertIndex;
					insertIndex--;
				}
			}
			Insert(insertIndex, row);
			return insertIndex;
		}

		public void Clear()
		{
			OnClear();
			_rows.Clear();
			MordexRow.NoteStructureChanged();
			OnClearComplete();
		}

		public bool Contains(MordexRow row)
		{
			return _rows.Contains(row);
		}

		public int IndexOf(MordexRow row)
		{
			if (_isSorted)
			{
				int sortedIndex = _rows.BinarySearch(row);
				if (sortedIndex >= 0 && sortedIndex < _rows.Count && _rows[sortedIndex] == row)
				{
					return sortedIndex;
				}
			}
			return _rows.IndexOf(row);
		}

		public void Insert(int index, MordexRow row)
		{
			OnInsert(index, row);
			_rows.Insert(index, row);
			MordexRow.NoteStructureChanged();
			OnInsertComplete(index, row);
		}

		internal void ReplaceRowList(ArrayList arrayList)
		{
			_rows = arrayList;
			MordexRow.NoteStructureChanged();
		}

		public void Remove(MordexRow row)
		{
			int index = IndexOf(row);
			if (index >= 0)
			{
				RemoveAt(index);
			}
		}

		internal void RemoveChildRow(MordexRow row)
		{
			Remove(row);
		}

		public void RemoveAt(int index)
		{
			MordexRow row = this[index];
			OnRemove(index, row);
			_rows.RemoveAt(index);
			MordexRow.NoteStructureChanged();
			OnRemoveComplete(index, row);
		}

		protected virtual void OnRemove(int index, MordexRow row)
		{
		}

		protected virtual void OnRemoveComplete(int index, MordexRow row)
		{
			MordexRow parentRow = ParentRow;
			parentRow?.RaiseRowChange(row, RowChangeType.BeforeDelete);
			row.SetParentCollection(null);
			parentRow?.RaiseRowChange(row, RowChangeType.Delete);
		}

		protected virtual void OnInsert(int index, MordexRow row)
		{
		}

		protected virtual void OnInsertComplete(int index, MordexRow row)
		{
			row.SetParentCollection(this);
			row.SetIndexInParent(index);
			if (!_isLoading)
			{
				row.RaiseRowChange(row, RowChangeType.Add);
			}
		}

		protected virtual void OnClear()
		{
			foreach (MordexRow item in _rows)
			{
				item.SetParentCollection(null);
			}
		}

		protected virtual void OnClearComplete()
		{
		}

		internal bool IsSortingEnabled()
		{
			return _isSorted;
		}

		internal void SetSortingEnabled(bool mordexRowCollectionBoolean)
		{
			if (_isSorted != mordexRowCollectionBoolean)
			{
				_isSorted = mordexRowCollectionBoolean;
				ResortRows();
			}
		}

		internal void ResortRows()
		{
			if (!_isSorted)
			{
				return;
			}
			if (_collectionType == RowCollectionType.DataRows)
			{
				bool hasDataRows = false;
				foreach (MordexRow item in _rows)
				{
					if (item is MordexDataRow mordexDataRow)
					{
						mordexDataRow.ResetSortValues();
						mordexDataRow.LoadSortValues(true);
						hasDataRows = true;
					}
				}
				if (hasDataRows)
				{
					MordexDataRowComparer mordexDataRowComparer = new MordexDataRowComparer(_parentRow.GetTableInterface());
					if (!mordexDataRowComparer.IsSorted(_rows))
					{
						_rows.Sort(mordexDataRowComparer);
					}
				}
				else
				{
					_rows.Sort();
				}
			}
			else
			{
				_rows.Sort();
			}
		}

		internal MordexRow FindRowByKey(object value)
		{
			if (_rowsByKey != null)
			{
				if (value == null)
				{
					value = DBNull.Value;
				}
				if (_ignoreCase && value is string)
				{
					value = ((string)value).ToUpper();
				}
				object existing = _rowsByKey[value];
				if (existing is KeyedRows keyedRows)
				{
					return keyedRows[0];
				}
				return (MordexRow)existing;
			}
			return null;
		}

		internal void SyncDataHolderRow(DataHolderRow dataHolderRow)
		{
			switch (_collectionType)
			{
			case RowCollectionType.DataRows:
				SyncDataRows(dataHolderRow);
				break;
			case RowCollectionType.TableRows:
			{
				foreach (MordexRow item in (IEnumerable)this)
				{
					if (item.Table == dataHolderRow.Table)
					{
						item.SetShouldRetrieveChildRows(false);
						item.Rows.SyncDataHolderRow(dataHolderRow);
						item.UpdateVisibility();
					}
					else
					{
						if (!item.Table.IsAncestorOf(dataHolderRow.Table))
						{
							continue;
						}
						if (item.Table.IsSelfReferencingTable())
						{
							MordexDataRow mordexDataRow = (MordexDataRow)ParentRow;
							if (mordexDataRow.DataHolder != dataHolderRow.ParentRow.ParentRow)
							{
								item.SetShouldRetrieveChildRows(false);
								item.Rows.SyncDataHolderRow(dataHolderRow);
							}
						}
						else
						{
							item.SetShouldRetrieveChildRows(false);
							item.Rows.SyncDataHolderRow(dataHolderRow);
						}
					}
				}
				break;
			}
			case RowCollectionType.GroupRows:
				SyncGroupRows(dataHolderRow);
				break;
			case RowCollectionType.CustomGroupRows:
				SyncDataHolderIntoCustomGroups(dataHolderRow);
				break;
			}
		}

		internal void ApplyCacheToExistingRows(DataSourceManager dataSourceManager, List<MordexRowCache> list)
		{
			if (list == null)
			{
				return;
			}
			switch (_collectionType)
			{
			case RowCollectionType.GroupRows:
			{
				foreach (MordexRowCache item in list)
				{
					FindGroupRow(item.GetFieldValue(0))?.LoadTreeCacheLight(dataSourceManager, item);
				}
				break;
			}
			case RowCollectionType.DataRows:
			{
				if (ParentRow.GetTableInterface().DataKeyMembers == null || ParentRow.GetTableInterface().DataKeyMembers.Length <= 0)
				{
					break;
				}
				ArrayList arrayList = new ArrayList();
				foreach (MordexDataRow item2 in (IEnumerable)this)
				{
					arrayList.Add(item2);
				}
				DataComparer comparer = new DataComparer(ParentRow.DataSourceManager.CurrentCulture);
				arrayList.Sort(comparer);
				{
					foreach (MordexRowCache item3 in list)
					{
						int matchIndex = arrayList.BinarySearch(item3.DataRowKeys, comparer);
						if (matchIndex >= 0)
						{
							MordexRow matchedRow = (MordexRow)arrayList[matchIndex];
							matchedRow.LoadTreeCacheLight(dataSourceManager, item3);
						}
					}
					break;
				}
			}
			case RowCollectionType.TableRows:
			{
				foreach (MordexRowCache item4 in list)
				{
					MordexRow mordexRow = null;
					int reference = item4.GetReferenceIndex();
					if (reference >= 0)
					{
						continue;
					}
					if (reference == int.MinValue)
					{
						foreach (MordexTableRow item5 in (IEnumerable)this)
						{
							if (item5.Table == ParentRow.Table)
							{
								mordexRow = item5;
								break;
							}
						}
					}
					else
					{
						int index = -reference - 1;
						MordexTableBase mordexTableBase = (MordexTableBase)ParentRow.GetTableInterface().ChildTables[index];
						for (int i = 0; i < Count; i++)
						{
							if (this[i].Table == mordexTableBase)
							{
								mordexRow = this[i];
								break;
							}
						}
					}
					mordexRow?.LoadTreeCacheLight(dataSourceManager, item4);
				}
				break;
			}
			}
		}

		internal void CreateRowsFromCache(DataSourceManager dataSourceManager, List<MordexRowCache> list, object[] valueArray)
		{
			if (list == null)
			{
				return;
			}
			foreach (MordexRowCache item in list)
			{
				MordexRow mordexRow = null;
				switch (_collectionType)
				{
				case RowCollectionType.DataRows:
					mordexRow = new MordexDataRow((DataHolderRow)valueArray[item.GetReferenceIndex()]);
					break;
				case RowCollectionType.TableRows:
				{
					int reference = item.GetReferenceIndex();
					if (reference >= 0)
					{
						TableListHolderRow tableListHolderRow = (TableListHolderRow)valueArray[reference];
						mordexRow = new MordexTableRow(tableListHolderRow.Table, preventGroups: false);
					}
					else if (reference <= -32768)
					{
						int index = -(reference - -32768);
						MordexFieldBase mordexFieldBase = (MordexFieldBase)ParentRow.GetTableInterface().Fields[index];
						MordexTableBase table = (MordexTableBase)mordexFieldBase.GetMultipleValuesTable();
						mordexRow = new MordexTableRow(table, preventGroups: false);
					}
					else
					{
						int childTableIndex = -reference - 1;
						MordexTableBase childTable = (MordexTableBase)ParentRow.GetTableInterface().ChildTables[childTableIndex];
						mordexRow = new MordexTableRow(childTable, preventGroups: false);
					}
					break;
				}
				case RowCollectionType.GroupRows:
				case RowCollectionType.CustomGroupRows:
					mordexRow = new MordexGroupRow(null, null) { _ignoreCase = _ignoreCase };
					break;
				}
				if (mordexRow != null)
				{
					_rows.Add(mordexRow);
					MordexRow.NoteStructureChanged();
					mordexRow.SetParentCollection(this);
					mordexRow.LoadTreeCacheRows(dataSourceManager, item, valueArray);
					IndexRow(mordexRow);
				}
			}
			_isSorted = true;
		}

		internal void RefreshRowKeyAndPosition(MordexRow mordexRow)
		{
			object rowKey = mordexRow.GetCachedRowKey();
			if (rowKey != null)
			{
				if (mordexRow.HasRowKeyChanged())
				{
					UnindexRow(mordexRow);
					mordexRow.RefreshCachedRowKey();
					if (_rowsByKey != null)
					{
						IndexRow(mordexRow);
					}
				}
				else if (_rowsByKey != null)
				{
					IndexRow(mordexRow);
				}
			}
			if (_isSorted)
			{
				int oldIndex = mordexRow.GetIndexInParent();
				if (oldIndex >= 0 && oldIndex < _rows.Count)
				{
					_rows.RemoveAt(oldIndex);
				}
				MordexDataRow mordexDataRow = mordexRow as MordexDataRow;
				if (mordexDataRow != null)
				{
					mordexDataRow.ResetSortValues();
					mordexDataRow.LoadSortValues(true);
				}
				int newIndex = _rows.BinarySearch(mordexRow);
				if (newIndex < 0)
				{
					newIndex = -newIndex;
					newIndex--;
				}
				_rows.Insert(newIndex, mordexRow);
				mordexDataRow?.ResetSortValues();
				if (newIndex != oldIndex)
				{
					mordexRow.RaiseRowChange(mordexRow, RowChangeType.Move);
				}
			}
		}

		private void SyncDataRows(DataHolderRow sourceHolderRow)
		{
			if (ParentRow.Table.IsSelfReferencingTable() && sourceHolderRow.Table == ParentRow.Table)
			{
				SyncSelfReferencingDataRows(sourceHolderRow);
				return;
			}
			DataHolderRow dataHolderRow = sourceHolderRow.FindHolderForTable(ParentRow.Table);
			MordexDataRow mordexDataRow = FindRowForHolder(dataHolderRow);
			if (mordexDataRow == null)
			{
				mordexDataRow = new MordexDataRow(dataHolderRow);
				IndexRow(mordexDataRow);
				Add(mordexDataRow);
				if (mordexDataRow.Table != null && mordexDataRow.DataHolder == sourceHolderRow && (mordexDataRow.Table.UsesDynamicFiltering() || IsLoading()))
				{
					((MordexRow)mordexDataRow).UpdateVisibility();
				}
			}
			if (mordexDataRow.DataHolder != sourceHolderRow && mordexDataRow.DataHolder.Table.IsAncestorOf(sourceHolderRow.Table))
			{
				mordexDataRow.Rows.SyncDataHolderRow(sourceHolderRow);
				if (ParentRow.Table.UsesDynamicFiltering())
				{
					((MordexRow)mordexDataRow).UpdateVisibility();
				}
			}
		}

		private void SyncSelfReferencingDataRows(DataHolderRow sourceHolderRow)
		{
			int depth = 0;
			DataHolderRow dataHolderRow = null;
			if (ParentRow is MordexTableRow)
			{
				ArrayList arrayList = sourceHolderRow.GetSameTablePath();
				depth = ((MordexTableRow)ParentRow).GetSelfReferencingDepth();
				if (depth < arrayList.Count)
				{
					dataHolderRow = (DataHolderRow)arrayList[depth];
				}
			}
			else if (ParentRow is MordexGroupRow)
			{
				if (ParentRow.GetTableInterface().SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.ParentRows)
				{
					ArrayList holderPath = sourceHolderRow.GetSameTablePath();
					depth = 0;
					if (depth < holderPath.Count)
					{
						dataHolderRow = (DataHolderRow)holderPath[depth];
					}
				}
				else
				{
					dataHolderRow = sourceHolderRow;
				}
			}
			if (dataHolderRow != null)
			{
				MordexDataRow mordexDataRow = FindRowForHolder(dataHolderRow);
				if (mordexDataRow == null)
				{
					mordexDataRow = new MordexDataRow(dataHolderRow);
					Add(mordexDataRow);
					IndexRow(mordexDataRow);
					((MordexRow)mordexDataRow).UpdateVisibility();
				}
				if (mordexDataRow.DataHolder != sourceHolderRow)
				{
					mordexDataRow.Rows.SyncDataHolderRow(sourceHolderRow);
				}
			}
		}

		private void SyncDataHolderIntoMatchingCustomGroup(DataHolderRow dataHolderRow)
		{
			bool matchedCondition = false;
			MordexGroupRow mordexGroupRow = null;
			foreach (MordexGroupRow item in (IEnumerable)this)
			{
				IMordexCustomGroupRow mordexCustomGroupRow = (IMordexCustomGroupRow)item.GroupValue;
				if (mordexCustomGroupRow.ShowRemainingRows)
				{
					mordexGroupRow = item;
				}
				else if (mordexCustomGroupRow.Condition != null && ((MordexConditionBase)mordexCustomGroupRow.Condition).EvaluateRow(dataHolderRow))
				{
					matchedCondition = true;
					item.Rows.SetSortingEnabled(true);
					item.Rows.SyncDataHolderRow(dataHolderRow);
					item.UpdateVisibility();
				}
			}
			if (!matchedCondition && mordexGroupRow != null)
			{
				mordexGroupRow.Rows.SyncDataHolderRow(dataHolderRow);
				mordexGroupRow.UpdateVisibility();
			}
		}

		private void SyncDataHolderIntoCustomGroups(DataHolderRow dataHolderRow)
		{
			IList list = dataHolderRow.GetRowsForGroup(ParentRow.Table, ((MordexGroupBase)_group).Index == 0, true);
			foreach (DataHolderRow item in list)
			{
				SyncDataHolderIntoMatchingCustomGroup(item);
			}
		}

		private void SyncGroupRows(DataHolderRow dataHolderRow)
		{
			MordexGroupBase mordexGroupBase = (MordexGroupBase)_group;
			MordexTableBase mordexTableBase = mordexGroupBase.GetGroupTable();
			IList list = dataHolderRow.GetRowsForGroup(mordexTableBase, mordexGroupBase.Index == 0, true);
			foreach (DataHolderRow item in list)
			{
				object originalValue = mordexGroupBase.GetGroupValue(item);
				object groupKey = mordexGroupBase.GetGroupValueFromBaseFields(item);
				if (groupKey == null)
				{
					groupKey = DBNull.Value;
				}
				MordexGroupRow mordexGroupRow = FindGroupRow(groupKey);
				if (mordexGroupRow == null)
				{
					mordexGroupRow = new MordexGroupRow(originalValue, groupKey);
					mordexGroupRow._ignoreCase = _ignoreCase;
					if (ParentRow.Table.GetEffectiveGroupMode() == GroupMode.Expanded)
					{
						mordexGroupRow.Expanded = true;
					}
					if (_group.GroupInterval == GroupInterval.Text)
					{
						mordexGroupRow.GroupText = string.Concat(mordexGroupRow.GroupValue);
					}
					else if (_group.GroupInterval == GroupInterval.OutlookDates)
					{
						mordexGroupRow.GroupText = mordexGroupBase.FormatGroupText(groupKey);
					}
					else
					{
						mordexGroupRow.GroupText = mordexGroupBase.FormatGroupText(originalValue);
					}
					Add(mordexGroupRow);
					IndexRow(mordexGroupRow);
					mordexGroupRow.Rows.SetSortingEnabled(true);
				}
				mordexGroupRow.Rows.SyncDataHolderRow(item);
				mordexGroupRow.UpdateVisibility();
			}
		}

		private MordexGroupRow FindGroupRow(object value)
		{
			if (_group.Field != null || _group.Fields != null)
			{
				IComparer comparer = ((MordexGroupBase)_group).GetGroupComparer();
				if (comparer == null || comparer is IEqualityComparer)
				{
					return (MordexGroupRow)FindRowByKey(value);
				}
				foreach (MordexGroupRow item in (IEnumerable)this)
				{
					if (comparer.Compare(item.GroupValue, value) == 0)
					{
						return item;
					}
				}
				return null;
			}
			return null;
		}

		public bool IsLoading()
		{
			if (_isLoading)
			{
				return _isLoading;
			}
			if (ParentRow.ParentCollection != null)
			{
				return ParentRow.ParentCollection.IsLoading();
			}
			return false;
		}

		internal void BeginLoad()
		{
			_isLoading = true;
		}

		internal void EndLoad()
		{
			_isLoading = false;
			SetSortingEnabled(true);
			foreach (MordexRow item in (IEnumerable)this)
			{
				if (item.HasRowCollection())
				{
					item.Rows.EndLoad();
				}
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			_rows.CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _rows.GetEnumerator();
		}

		int IList.Add(object value)
		{
			return Add((MordexRow)value);
		}

		bool IList.Contains(object value)
		{
			return Contains((MordexRow)value);
		}

		int IList.IndexOf(object value)
		{
			return IndexOf((MordexRow)value);
		}

		void IList.Insert(int index, object value)
		{
			Insert(index, (MordexRow)value);
		}

		void IList.Remove(object value)
		{
			Remove((MordexRow)value);
		}
	}
}
