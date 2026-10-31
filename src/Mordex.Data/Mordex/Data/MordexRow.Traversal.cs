using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Mordex.Data
{
	partial class MordexRow
	{
		internal MordexRow GetNextChildAfter(MordexRow row)
		{
			if (row.ParentRow != this)
			{
				throw new InvalidOperationException();
			}
			int position = Rows.IndexOf(row);
			if (position >= 0 && position < Rows.Count)
			{
				position++;
				if (position < Rows.Count)
				{
					return Rows[position];
				}
				MordexRow[] postListRows = GetPostListRows();
				if (postListRows != null && postListRows.Length > 0)
				{
					return postListRows[0];
				}
				return null;
			}
			MordexRow[] preListRows = GetPreListRows();
			position = -1;
			if (preListRows != null)
			{
				for (int i = 0; i < preListRows.Length; i++)
				{
					if (preListRows[i] == row)
					{
						position = i;
						break;
					}
				}
				if (position != -1)
				{
					if (position == preListRows.Length - 1)
					{
						if (Rows.Count == 0)
						{
							preListRows = GetPostListRows();
							if (preListRows == null || preListRows.Length == 0)
							{
								return null;
							}
							return preListRows[0];
						}
						return Rows[0];
					}
					return preListRows[position + 1];
				}
			}
			preListRows = GetPostListRows();
			if (preListRows != null)
			{
				position = -1;
				for (int j = 0; j < preListRows.Length; j++)
				{
					if (preListRows[j] == row)
					{
						position = j;
						break;
					}
				}
				if (position != -1 && position + 1 < preListRows.Length)
				{
					return preListRows[position + 1];
				}
			}
			return null;
		}

		internal MordexRow GetPreviousChildBefore(MordexRow row)
		{
			if (row.ParentRow != this)
			{
				throw new InvalidOperationException();
			}
			int position = Rows.IndexOf(row);
			if (position >= 0 && position < Rows.Count)
			{
				position--;
				if (position >= 0)
				{
					return Rows[position];
				}
				MordexRow[] preListRows = GetPreListRows();
				if (preListRows != null && preListRows.Length > 0)
				{
					return preListRows[preListRows.Length - 1];
				}
				return null;
			}
			MordexRow[] postListRows = GetPostListRows();
			position = -1;
			if (postListRows != null)
			{
				for (int i = 0; i < postListRows.Length; i++)
				{
					if (postListRows[i] == row)
					{
						position = i;
						break;
					}
				}
				switch (position)
				{
				case 0:
					if (Rows.Count == 0)
					{
						postListRows = GetPreListRows();
						if (postListRows == null || postListRows.Length == 0)
						{
							return null;
						}
						return postListRows[postListRows.Length - 1];
					}
					return Rows[Rows.Count - 1];
				default:
					return postListRows[position - 1];
				case -1:
					break;
				}
			}
			postListRows = GetPreListRows();
			if (postListRows != null)
			{
				position = -1;
				for (int j = 0; j < postListRows.Length; j++)
				{
					if (postListRows[j] == row)
					{
						position = j;
						break;
					}
				}
				if (position != -1 && position > 0)
				{
					return postListRows[position - 1];
				}
			}
			return null;
		}

		internal MordexRow[] GetPostListRowsInternal()
		{
			return GetPostListRows();
		}

		internal void SetRowManagerPosition(int position)
		{
			_rowManagerPosition = position;
		}

		public MordexRow[] GetLoadedDataRows(MordexTableBase table)
		{
			ArrayList arrayList = new ArrayList();
			bool isSelfReferencing = false;
			if (((IMordexTable)table).HierarchicalMode == HierarchicalMode.SelfReferencing)
			{
				isSelfReferencing = true;
			}
			CollectDataRows(table, arrayList, false, false, isSelfReferencing);
			MordexRow[] dataRows = new MordexRow[arrayList.Count];
			arrayList.CopyTo(dataRows);
			return dataRows;
		}

		public MordexRow[] GetDataRows(MordexTableBase table)
		{
			return GetDataRows(table, includeFilteredRows: false);
		}

		internal MordexRow[] CollectDataRowsToArray(MordexTableBase mordexTableBase, bool boolean, bool includeHiddenRows, bool includeChildDataRows)
		{
			ArrayList arrayList = new ArrayList();
			CollectDataRows(mordexTableBase, arrayList, boolean, includeHiddenRows, includeChildDataRows);
			MordexRow[] result = new MordexRow[arrayList.Count];
			arrayList.CopyTo(result);
			return result;
		}

		public MordexRow[] GetDataRows(MordexTableBase table, bool includeFilteredRows)
		{
			ArrayList arrayList = new ArrayList();
			CollectDataRows(table, arrayList, true, includeFilteredRows, true);
			MordexRow[] result = new MordexRow[arrayList.Count];
			arrayList.CopyTo(result);
			return result;
		}

		private MordexRow[] GetFilteredDataRows(MordexTableBase mordexTableBase, MordexConditionBase mordexConditionBase, bool boolean)
		{
			MordexRow[] rows = CollectDataRowsToArray(mordexTableBase, true, false, boolean);
			if (mordexConditionBase == null)
			{
				return rows;
			}
			ArrayList arrayList = new ArrayList();
			MordexRow[] snapshot = rows;
			foreach (MordexRow mordexRow in snapshot)
			{
				if (mordexConditionBase.EvaluateRow(mordexRow))
				{
					arrayList.Add(mordexRow);
				}
			}
			rows = new MordexRow[arrayList.Count];
			for (int j = 0; j < arrayList.Count; j++)
			{
				rows[j] = (MordexRow)arrayList[j];
			}
			return rows;
		}

		public MordexRow[] GetDataRows(MordexTableBase table, MordexConditionBase condition)
		{
			return GetFilteredDataRows(table, condition, true);
		}

		internal void UpdateVisibility()
		{
			OnEvaluateVisibility();
		}

		protected virtual void OnEvaluateVisibility()
		{
		}

		private void CollectDataRows(MordexTableBase mordexTableBase, ArrayList arrayList, bool boolean, bool includeHiddenRows, bool includeChildDataRows)
		{
			MordexRowCollection mordexRowCollection = null;
			mordexRowCollection = ((!boolean) ? _rows : Rows);
			if (mordexRowCollection == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)mordexRowCollection)
			{
				if (!includeHiddenRows && !item.Visible)
				{
					continue;
				}
				if (item is MordexDataRow && item.Table == mordexTableBase)
				{
					arrayList.Add(item);
					if (item.Table.IsAncestorOf(mordexTableBase))
					{
						item.CollectDataRows(mordexTableBase, arrayList, boolean, includeHiddenRows, includeChildDataRows);
					}
					else if (item.GetTableInterface().HierarchicalMode != HierarchicalMode.UseChildTables && includeChildDataRows)
					{
						item.CollectDataRows(mordexTableBase, arrayList, boolean, includeHiddenRows, includeChildDataRows);
					}
				}
				else
				{
					item.CollectDataRows(mordexTableBase, arrayList, boolean, includeHiddenRows, includeChildDataRows);
				}
			}
		}

		public virtual void ExpandGroups(MordexGroupBase group)
		{
			if (Rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)Rows)
			{
				item.ExpandGroups(group);
			}
		}

		public virtual object GetValue(MordexFieldBase field)
		{
			if (field != null)
			{
				return GetValue(((IMordexField)field).DataMember);
			}
			return null;
		}

		public virtual object GetValue(string dataMember)
		{
			if (DataRow != null)
			{
				return DataValueHelper.GetPropertyValue(DataRow, null, dataMember);
			}
			return null;
		}

		public virtual void SetValue(MordexFieldBase field, object newValue)
		{
		}

		public virtual void SetValue(MordexFieldBase field, string dataMember, object newValue)
		{
		}

		public virtual void SetValue(string dataMember, object newValue)
		{
			SetValue(null, dataMember, newValue);
		}

		public virtual void BeginEdit()
		{
			if (DataRow is IEditableObject editableObject)
			{
				editableObject.BeginEdit();
			}
		}

		public virtual void EndEdit()
		{
			if (DataRow is IEditableObject editableObject)
			{
				editableObject.EndEdit();
			}
		}

		public virtual void CancelEdit()
		{
			if (DataRow is IEditableObject editableObject)
			{
				editableObject.CancelEdit();
			}
		}

		public virtual bool CanDelete()
		{
			return false;
		}

		public virtual void Delete()
		{
		}

		internal void RaiseRowChange(MordexRow row, RowChangeType change)
		{
			OnRowChange(row, change);
		}

		protected virtual void OnRowChange(MordexRow row, RowChangeType change)
		{
			if (change == RowChangeType.Change)
			{
				_aggregates?.NoteChanged(row);
			}
			if (ParentRow != null && !ParentRow.IsLoading())
			{
				ParentRow.OnRowChange(row, change);
			}
		}

		public bool IsLoading()
		{
			if (HasRowCollection())
			{
				return Rows.IsLoading();
			}
			return false;
		}

		public MordexRow[] GetChildRows()
		{
			ArrayList arrayList = new ArrayList();
			MordexRow[] preListRows = GetPreListRows();
			if (preListRows != null)
			{
				MordexRow[] leadingRows = preListRows;
				foreach (MordexRow value in leadingRows)
				{
					arrayList.Add(value);
				}
			}
			if (Rows != null)
			{
				foreach (MordexRow item in (IEnumerable)Rows)
				{
					arrayList.Add(item);
				}
			}
			preListRows = GetPostListRows();
			if (preListRows != null)
			{
				MordexRow[] trailingRows = preListRows;
				foreach (MordexRow value3 in trailingRows)
				{
					arrayList.Add(value3);
				}
			}
			MordexRow[] childRows = new MordexRow[arrayList.Count];
			arrayList.CopyTo(childRows);
			return childRows;
		}

		public virtual void FillList(List<MordexRow> list, bool childRowsOnly)
		{
			if (!Visible)
			{
				return;
			}
			if (!IsVirtualRow() && !childRowsOnly)
			{
				list.Add(this);
			}
			if (!Expanded)
			{
				return;
			}
			MordexRow[] preListRows = GetPreListRows();
			if (preListRows != null)
			{
				MordexRow[] leadingRows = preListRows;
				foreach (MordexRow mordexRow in leadingRows)
				{
					mordexRow.FillList(list, childRowsOnly: false);
				}
			}
			MordexRowCollection rows = Rows;
			foreach (MordexRow item in (IEnumerable)rows)
			{
				item.FillList(list, childRowsOnly: false);
			}
			preListRows = GetPostListRows();
			if (preListRows != null)
			{
				MordexRow[] trailingRows = preListRows;
				foreach (MordexRow mordexRow3 in trailingRows)
				{
					mordexRow3.FillList(list, childRowsOnly: false);
				}
			}
		}

		public object GetTotal(MordexGroupTotalBase groupTotal)
		{
			return GetTotal((MordexFieldBase)((IMordexGroupTotal)groupTotal).Field, ((IMordexGroupTotal)groupTotal).AggregateFunction, (MordexConditionBase)((IMordexGroupTotal)groupTotal).Condition);
		}

		public object GetTotal(MordexFieldBase field, AggregateFunction aggregateFunction)
		{
			return GetTotal(field, aggregateFunction, null);
		}

		public virtual object GetTotal(MordexFieldBase field, AggregateFunction aggregateFunction, MordexConditionBase condition)
		{
			if (field == null)
			{
				return null;
			}
			bool includeAllRows = ((IMordexField)field).Table.SelfReferencingSettings.AggregationMode == SelfReferencingAggregationMode.AllRows;
			bool cacheable = condition == null && CanCacheAggregate(field, aggregateFunction, includeAllRows);
			long stamp = StructureStamp;
			if (cacheable && TryGetCachedAggregate(field, aggregateFunction, includeAllRows, stamp, out object cached))
			{
				return cached;
			}
			MordexRow[] dataRows = GetFilteredDataRows((MordexTableBase)((IMordexField)field).Table, condition, includeAllRows);
			return cacheable ? ComputeAndRemember(field, aggregateFunction, includeAllRows, stamp, dataRows) : ComputeAggregate(field, aggregateFunction, dataRows);
		}

		// The totals of several fields, collecting the rows of each table once.
		public object[] GetTotals(MordexFieldBase[] fields, AggregateFunction[] aggregateFunctions)
		{
			object[] totals = new object[fields.Length];
			MordexRow[] dataRows = null;
			MordexTableBase rowsTable = null;
			bool rowsIncludeAll = false;
			long stamp = StructureStamp;
			for (int i = 0; i < fields.Length; i++)
			{
				MordexFieldBase field = fields[i];
				if (field == null)
				{
					continue;
				}
				MordexTableBase table = (MordexTableBase)((IMordexField)field).Table;
				bool includeAllRows = ((IMordexField)field).Table.SelfReferencingSettings.AggregationMode == SelfReferencingAggregationMode.AllRows;
				bool cacheable = CanCacheAggregate(field, aggregateFunctions[i], includeAllRows);
				if (cacheable && TryGetCachedAggregate(field, aggregateFunctions[i], includeAllRows, stamp, out object cached))
				{
					totals[i] = cached;
					continue;
				}
				if (dataRows == null || table != rowsTable || includeAllRows != rowsIncludeAll)
				{
					dataRows = GetFilteredDataRows(table, null, includeAllRows);
					rowsTable = table;
					rowsIncludeAll = includeAllRows;
				}
				totals[i] = cacheable ? ComputeAndRemember(field, aggregateFunctions[i], includeAllRows, stamp, dataRows) : ComputeAggregate(field, aggregateFunctions[i], dataRows);
			}
			return totals;
		}

		internal static object ComputeAggregate(MordexFieldBase mordexFieldBase, AggregateFunction aggregateFunction, MordexRow[] mordexRowArray)
		{
			if (aggregateFunction == AggregateFunction.None)
			{
				return null;
			}
			if (mordexFieldBase == null && aggregateFunction != AggregateFunction.Count)
			{
				return null;
			}
			object value = null;
			decimal sum = 0m;
			int count = 0;
			switch (aggregateFunction)
			{
			case AggregateFunction.Count:
				return mordexRowArray.Length;
			case AggregateFunction.Max:
			{
				object maxValue = null;
				for (int l = 0; l < mordexRowArray.Length; l++)
				{
					MordexDataRow dataRow = (MordexDataRow)mordexRowArray[l];
					value = mordexFieldBase.GetRowValue(dataRow);
					if (!DataValueHelper.IsNullOrDBNull(value))
					{
						if (maxValue == null)
						{
							maxValue = value;
						}
						else if (mordexFieldBase.CompareValues(maxValue, value) < 0)
						{
							maxValue = value;
						}
					}
				}
				return maxValue;
			}
			case AggregateFunction.Min:
			{
				object minValue = null;
				for (int k = 0; k < mordexRowArray.Length; k++)
				{
					MordexDataRow mordexDataRow = (MordexDataRow)mordexRowArray[k];
					value = mordexFieldBase.GetRowValue(mordexDataRow);
					if (!DataValueHelper.IsNullOrDBNull(value))
					{
						if (minValue == null)
						{
							minValue = value;
						}
						else if (mordexFieldBase.CompareValues(minValue, value) > 0)
						{
							minValue = value;
						}
					}
				}
				return minValue;
			}
			case AggregateFunction.Average:
			{
				for (int n = 0; n < mordexRowArray.Length; n++)
				{
					MordexDataRow dataRow = (MordexDataRow)mordexRowArray[n];
					value = mordexFieldBase.GetRowValue(dataRow);
					if (DataValueHelper.CanConvertToDecimal(value))
					{
						sum += Convert.ToDecimal(value);
						count++;
					}
				}
				if (count > 0)
				{
					return (double)sum / (double)count;
				}
				return null;
			}
			case AggregateFunction.Sum:
				foreach (MordexRow mordexRow2 in mordexRowArray)
				{
					value = mordexFieldBase.GetRowValue(mordexRow2);
					if (DataValueHelper.CanConvertToDecimal(value))
					{
						sum += Convert.ToDecimal(value);
					}
				}
				return sum;
			case AggregateFunction.ValueCount:
			{
				for (int m = 0; m < mordexRowArray.Length; m++)
				{
					MordexDataRow dataRow = (MordexDataRow)mordexRowArray[m];
					value = mordexFieldBase.GetRowValue(dataRow);
					if (!DataValueHelper.IsNullOrDBNull(value))
					{
						count++;
					}
				}
				return count;
			}
			case AggregateFunction.StdDeviation:
			{
				// Welford's running mean keeps the spread exact when the values share a large offset.
				double mean = 0.0;
				double sumOfSquaredDeviations = 0.0;
				foreach (MordexRow mordexRow in mordexRowArray)
				{
					value = mordexFieldBase.GetRowValue(mordexRow);
					if (DataValueHelper.CanConvertToDecimal(value))
					{
						double number = Convert.ToDouble(value);
						count++;
						double deviation = number - mean;
						mean += deviation / count;
						sumOfSquaredDeviations += deviation * (number - mean);
					}
				}
				if (count > 1)
				{
					return Math.Sqrt(sumOfSquaredDeviations / (count - 1));
				}
				return null;
			}
			default:
				return null;
			}
		}

		public virtual void RefreshFilter(MordexTableBase table)
		{
			if (Table == table && Table.IsSelfReferencingTable())
			{
				LoadChildRows();
				if (_rows != null)
				{
					foreach (MordexRow item in (IEnumerable)Rows)
					{
						item.RefreshFilter(table);
					}
				}
			}
			else if (Table == table)
			{
				if (_rows != null)
				{
					foreach (MordexRow item2 in (IEnumerable)Rows)
					{
						item2.RefreshFilter(table);
					}
				}
			}
			else if (Table != table && Table.IsAncestorOf(table) && (((IMordexTable)table).FilterParentRows || _rows != null))
			{
				LoadChildRows();
				if (_rows != null)
				{
					foreach (MordexRow item3 in (IEnumerable)Rows)
					{
						item3.RefreshFilter(table);
					}
				}
			}
			UpdateVisibility();
		}

		public virtual void CollapseDataRows(MordexTableBase table)
		{
			if (_rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.CollapseDataRows(table);
			}
		}

		public virtual void ExpandDataRows(MordexTableBase table)
		{
			if (Table.HasOwnerField())
			{
				return;
			}
			_suppressExpandedNotification = true;
			try
			{
				Expanded = true;
			}
			finally
			{
				_suppressExpandedNotification = false;
			}
			if ((table == Table && !Table.IsSelfReferencingTable() && !(this is MordexTableRow)) || Rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.ExpandDataRows(table);
			}
		}

		internal MordexTableRow GetOwningTableRow()
		{
			MordexRow mordexRow = this;
			while (mordexRow != null && !(mordexRow is MordexTableRow))
			{
				mordexRow = mordexRow.ParentRow;
			}
			return (MordexTableRow)mordexRow;
		}

		internal void CollapseGroupRows(MordexGroupBase group)
		{
			CollapseGroupRowsCore(group);
		}

		protected virtual void CollapseGroupRowsCore(MordexGroupBase group)
		{
			if (_rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.CollapseGroupRowsCore(group);
			}
		}

		internal void ExpandGroupRows(MordexGroupBase group)
		{
			ExpandGroupRowsCore(group);
		}

		protected virtual void ExpandGroupRowsCore(MordexGroupBase group)
		{
			if (_rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.ExpandGroupRowsCore(group);
			}
		}

		public bool HasChildren()
		{
			if (CanHaveChildren())
			{
				if (ChildrenLoaded)
				{
					if (Rows.GetCollectionType() == RowCollectionType.TableRows)
					{
						foreach (MordexRow item in (IEnumerable)Rows)
						{
							if (!item.Table.HasOwnerField())
							{
								if (item.HasChildren())
								{
									return true;
								}
								MordexRow[] preListRows = item.GetPreListRows();
								if (preListRows != null && preListRows.Length > 0)
								{
									return true;
								}
								preListRows = item.GetPostListRows();
								if (preListRows != null && preListRows.Length > 0)
								{
									return true;
								}
							}
						}
						return false;
					}
					foreach (MordexRow item2 in (IEnumerable)Rows)
					{
						if (item2.Visible)
						{
							return true;
						}
					}
					return false;
				}
				return true;
			}
			return false;
		}

		private CheckedState GetStateFromChildRows()
		{
			if (Rows != null)
			{
				if (Rows.VisibleCount == 0)
				{
					if (IsCheckedCore)
					{
						return CheckedState.Checked;
					}
					return CheckedState.Unchecked;
				}
				CheckedState checkedState = CheckedState.Mixed;
				bool hasFirstState = false;
				for (int i = 0; i < Rows.Count; i++)
				{
					if (!_rows[i].Visible)
					{
						continue;
					}
					CheckedState childState = _rows[i].CheckedState;
					if (hasFirstState)
					{
						if (childState != checkedState)
						{
							return CheckedState.Mixed;
						}
					}
					else
					{
						hasFirstState = true;
						checkedState = childState;
					}
				}
				return checkedState;
			}
			if (IsCheckedCore)
			{
				return CheckedState.Checked;
			}
			return CheckedState.Unchecked;
		}

		protected virtual bool CheckedDependsOnChild()
		{
			DataSourceManager dataSourceManager = DataSourceManager;
			if (dataSourceManager != null && dataSourceManager.CheckedBehavior == CheckedBehavior.CheckChildRows)
			{
				return true;
			}
			return false;
		}

		internal void LoadTreeCacheLight(DataSourceManager manager, MordexRowCache cacheRow)
		{
			LoadTreeCacheLightCore(manager, cacheRow);
		}

		protected virtual void LoadTreeCacheLightCore(DataSourceManager manager, MordexRowCache cacheRow)
		{
			_expanded = cacheRow.GetExpandedFlag();
			_visible = cacheRow.GetVisibleFlag();
			NoteStructureChanged();
			if (cacheRow._childRows != null)
			{
				Rows?.ApplyCacheToExistingRows(manager, cacheRow._childRows);
			}
			IsChecked = cacheRow.GetCheckedFlag();
		}

		internal void LoadTreeCacheRows(DataSourceManager manager, MordexRowCache cacheRow, object[] dataRowReferences)
		{
			LoadTreeCacheRowsCore(manager, cacheRow, dataRowReferences);
		}

		protected virtual void LoadTreeCacheRowsCore(DataSourceManager manager, MordexRowCache cacheRow, object[] dataRowReferences)
		{
			if (IsTreeRow)
			{
				_expanded = cacheRow.GetExpandedFlag();
				_visible = cacheRow.GetVisibleFlag();
				NoteStructureChanged();
			}
			if (LoadIsChecked)
			{
				_isCheckedCore = cacheRow.GetCheckedFlag();
			}
			SetShouldRetrieveChildRows(false);
			if (cacheRow._childRows != null)
			{
				MordexRowCollection rows = Rows;
				if (rows.GetCollectionType() == RowCollectionType.CustomGroupRows)
				{
					rows.Clear();
				}
				rows?.CreateRowsFromCache(manager, cacheRow._childRows, dataRowReferences);
			}
			SetShouldRetrieveChildRows(true);
		}

		internal int GetCacheIndex()
		{
			return _cacheIndex;
		}

		internal void BuildTreeCache(DataSourceManager manager, List<MordexRowCache> list)
		{
			CacheRowCore(manager, list);
			CacheChildRowsCore(manager, list);
		}

		protected virtual void CacheRowCore(DataSourceManager manager, List<MordexRowCache> list)
		{
			MordexRowCache mordexRowCache = manager.CreateRowCacheFor(this);
			mordexRowCache.SetCacheIndex(list.Count);
			_cacheIndex = mordexRowCache.GetCacheIndex();
			list.Add(mordexRowCache);
		}

		protected virtual void CacheChildRowsCore(DataSourceManager manager, List<MordexRowCache> list)
		{
			if (_rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)Rows)
			{
				item.BuildTreeCache(manager, list);
			}
		}

		private void LoadDataRowKeys()
		{
			if (Table != null && GetTableInterface().DataKeyMembers != null && DataRow != null)
			{
				string[] dataKeyMembers = GetTableInterface().DataKeyMembers;
				if (dataKeyMembers.Length > 0)
				{
					if (DataRow is MordexRowCache)
					{
						_dataRowKeys = ((MordexRowCache)DataRow).DataRowKeys;
						return;
					}
					_dataRowKeys = new object[dataKeyMembers.Length];
					for (int i = 0; i < dataKeyMembers.Length; i++)
					{
						MordexFieldBase mordexFieldBase = Table.FindAnyFieldByDataMember(dataKeyMembers[i]);
						if (mordexFieldBase == null)
						{
							_dataRowKeys[i] = GetValue(dataKeyMembers[i]);
						}
						else
						{
							_dataRowKeys[i] = GetValue(mordexFieldBase);
						}
					}
				}
				else
				{
					_dataRowKeys = null;
				}
			}
			else
			{
				_dataRowKeys = null;
			}
		}
	}
}
