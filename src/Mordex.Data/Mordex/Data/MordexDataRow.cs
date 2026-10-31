using System;
using System.Collections;

namespace Mordex.Data
{
	public class MordexDataRow : MordexRow
	{
		internal object[] _sortValues;

		private DataHolderRow _dataHolder;

		private bool _isDataRowDeleted;

		private bool _sortValuesPinned;

		private int _recordPosition = -1;

		public override bool ChildrenLoaded
		{
			get
			{
				if (_dataHolder == null)
				{
					return false;
				}
				return _dataHolder.ChildrenLoaded;
			}
		}

		public override object DataRow
		{
			get
			{
				if (_dataHolder != null)
				{
					return _dataHolder.DataRow;
				}
				return null;
			}
		}

		public DataHolderRow DataHolder => _dataHolder;

		public bool IsDataRowDeleted => _isDataRowDeleted;

		public override MordexTableBase Table
		{
			get
			{
				if (_dataHolder != null)
				{
					return _dataHolder.Table;
				}
				return base.Table;
			}
		}

		public int DataRowIndex
		{
			get
			{
				if (_dataHolder == null)
				{
					return -1;
				}
				return _dataHolder.DataRowIndex;
			}
		}

		protected override bool IsTreeRow => true;

		public override int RecordPosition
		{
			get
			{
				MordexRow parentRow = ParentRow;
				MordexTableRow mordexTableRow = parentRow as MordexTableRow;
				while (parentRow != null && mordexTableRow == null)
				{
					parentRow = parentRow.ParentRow;
					mordexTableRow = parentRow as MordexTableRow;
				}
				if (mordexTableRow != null)
				{
					mordexTableRow.AssignRecordPositions(_recordPosition == -1);
					return _recordPosition;
				}
				return -1;
			}
		}

		public override bool IsCheckedCore
		{
			get
			{
				if (_dataHolder != null)
				{
					return _dataHolder.IsCheckedCore;
				}
				return false;
			}
			set
			{
				if (value != IsCheckedCore && _dataHolder != null)
				{
					_dataHolder.IsCheckedCore = value;
				}
			}
		}

		internal MordexDataRow(DataHolderRow dataHolder)
		{
			base.SetShouldRetrieveChildRows(true);
			_dataHolder = dataHolder;
			_dataHolder.AddLinkedRow(this);
		}

		protected override object GetValueOfFieldCore(MordexFieldBase field, string dataMember)
		{
			if (_dataHolder != null)
			{
				return _dataHolder.ReadFieldValue(field, dataMember);
			}
			return field?.GetItemValue(DataRow);
		}

		protected override void GetDistinctValuesCore(IMordexField field, IMordexValueItemCollection valueList)
		{
			if (field.Table == GetTableInterface())
			{
				MordexFieldBase mordexFieldBase = (MordexFieldBase)field;
				object value = null;
				if (field.CompareTarget == CompareTarget.Value)
				{
					value = mordexFieldBase.GetRowValue(this);
					if (!DataValueHelper.IsNullOrDBNull(value) && !valueList.Contains(value))
					{
						valueList.Add(value, mordexFieldBase.GetRowText(this));
					}
				}
				else
				{
					value = mordexFieldBase.GetRowText(this);
					if (!DataValueHelper.IsNullOrDBNull(value) && !valueList.Contains(value))
					{
						valueList.Add(value, (string)value);
					}
				}
			}
			base.GetDistinctValuesCore(field, valueList);
		}

		protected override bool IsRowKeyChangedCore()
		{
			return GetCachedRowKey() != GetRowKey();
		}

		protected override void OnDisposed()
		{
			base.OnDisposed();
			if (_dataHolder != null)
			{
				_dataHolder.RemoveLinkedRow(this);
				_dataHolder = null;
			}
		}

		public override bool CanHaveChildren()
		{
			if (GetTableInterface() != null)
			{
				return GetTableInterface().HasChildTables;
			}
			return false;
		}

		protected override MordexRowCollection CreateChildRows()
		{
			return new MordexRowCollection(this, RowCollectionType.TableRows);
		}

		protected override void RetrieveChildRows()
		{
			if (_dataHolder == null)
			{
				return;
			}
			base.Rows.BeginLoad();
			try
			{
				foreach (MordexRow item in (IEnumerable)_dataHolder.Rows)
				{
					if (item.Table != Table || Table.ActsAsSelfReferencingTable())
					{
						MordexTableRow mordexTableRow = new MordexTableRow(item.Table, preventGroups: false);
						mordexTableRow.IsCheckedCore = IsCheckedCore;
						base.Rows.Add(mordexTableRow);
						base.Rows.IndexRow(mordexTableRow);
					}
				}
			}
			finally
			{
				base.Rows.EndLoad();
			}
		}

		internal void DeleteRowForRemovedDataRow()
		{
			_isDataRowDeleted = true;
			DeleteRowFromCollection();
			_isDataRowDeleted = false;
		}

		internal void ApplyDataRowChange()
		{
			for (MordexRow parentRow = ParentRow; parentRow != null; parentRow = parentRow.ParentRow)
			{
				if (!parentRow.CanContainRow(this))
				{
					DeleteRowForRemovedDataRow();
					return;
				}
			}
			RaiseRowChange(this, RowChangeType.Change);
			if (base.ParentCollection != null)
			{
				base.ParentCollection.RefreshRowKeyAndPosition(this);
			}
			if (Table != null && Table.UsesDynamicFiltering())
			{
				base.UpdateVisibility();
			}
		}

		internal void ResetSortValues()
		{
			_sortValues = null;
			_sortValuesPinned = false;
		}

		internal void LoadSortValues(bool pinValues)
		{
			if (_sortValues != null && _sortValuesPinned)
			{
				return;
			}
			IMordexTable mordexTable = GetTableInterface();
			if (mordexTable != null && mordexTable.SortFields.Count > 0)
			{
				bool needsExternalData = false;
				foreach (IMordexSortField sortField in mordexTable.SortFields)
				{
					if (sortField.Field.CompareTarget == CompareTarget.Text || sortField.Field.BoundMode == ColumnBoundMode.UnboundFetch)
					{
						needsExternalData = true;
						break;
					}
				}
				if (needsExternalData && DataSourceManager != null)
				{
					DataSourceManager.RaiseFetchExternalData(this);
				}
				_sortValues = new object[mordexTable.SortFields.Count];
				for (int i = 0; i < mordexTable.SortFields.Count; i++)
				{
					IMordexSortField sortField = mordexTable.SortFields[i];
					MordexFieldBase mordexFieldBase = (MordexFieldBase)sortField.Field;
					_sortValues[i] = mordexFieldBase.GetRowValueToCompare(this);
				}
				if (needsExternalData && DataSourceManager != null)
				{
					DataSourceManager.RaiseReleaseExternalData(this);
				}
			}
			_sortValuesPinned = pinValues;
		}

		protected override int CompareTo(MordexRow row)
		{
			if (!(row is MordexDataRow mordexDataRow))
			{
				return 1;
			}
			LoadSortValues(false);
			mordexDataRow.LoadSortValues(false);
			if (_sortValues == null && mordexDataRow._sortValues != null)
			{
				return 1;
			}
			if (_sortValues != null && mordexDataRow._sortValues == null)
			{
				return 1;
			}
			IMordexTable mordexTable = GetTableInterface();
			int result = 0;
			if (mordexTable != null)
			{
				for (int i = 0; i < mordexTable.SortFields.Count; i++)
				{
					MordexSortFieldBase mordexSortFieldBase = (MordexSortFieldBase)mordexTable.SortFields[i];
					result = mordexSortFieldBase.CompareValues(_sortValues[i], mordexDataRow._sortValues[i]);
					if (result != 0)
					{
						return result;
					}
				}
				for (int j = 0; j < mordexTable.Groups.Count; j++)
				{
					IMordexGroup mordexGroup = mordexTable.Groups[j];
					if (mordexGroup.Field != null && mordexGroup.Field.DataTypeCode == TypeCode.DateTime)
					{
						MordexFieldBase mordexFieldBase = (MordexFieldBase)mordexGroup.Field;
						object thisValue = mordexFieldBase.GetRowValue(this);
						object otherValue = mordexFieldBase.GetRowValue(row);
						result = DataValueHelper.CompareValues(null, thisValue, otherValue, null, true, false);
						if (mordexGroup.SortOrder == SortOrder.Descending)
						{
							result = -result;
						}
						if (result == 0)
						{
							break;
						}
						return result;
					}
				}
			}
			if (_dataHolder.Table != null && _dataHolder.Table.IsSelfReferencingTable())
			{
				ArrayList arrayList = _dataHolder.GetSameTablePath();
				ArrayList otherPath = mordexDataRow.DataHolder.GetSameTablePath();
				int count = arrayList.Count;
				if (count > otherPath.Count)
				{
					count = otherPath.Count;
				}
				for (int k = 0; k < count; k++)
				{
					result = ((IComparable)arrayList[k]).CompareTo(otherPath[k]);
					if (result != 0)
					{
						break;
					}
				}
				if (result == 0)
				{
					result = arrayList.Count.CompareTo(otherPath.Count);
				}
			}
			else
			{
				result = Table.CompareRows(this, row as MordexDataRow);
			}
			return result;
		}

		protected override int CompareRowValue(object o)
		{
			int value = (int)o;
			return _dataHolder.DataRowIndex.CompareTo(value);
		}

		protected override object GetRowKey()
		{
			object result = null;
			if (_dataHolder != null)
			{
				result = _dataHolder.GetCachedRowKey();
			}
			return result;
		}

		public override object GetValue(string dataMember)
		{
			return DataHolder.GetValue(dataMember);
		}

		public override object GetValue(MordexFieldBase field)
		{
			return DataHolder.GetValue(field);
		}

		public override void SetValue(MordexFieldBase field, string dataMember, object newValue)
		{
			DataHolder.SetValue(field, dataMember, newValue);
		}

		public override void SetValue(MordexFieldBase field, object newValue)
		{
			DataHolder.SetValue(field, newValue);
		}

		public override void BeginEdit()
		{
			if (_dataHolder != null)
			{
				_dataHolder.BeginEdit();
			}
		}

		public override void EndEdit()
		{
			if (_dataHolder != null)
			{
				_dataHolder.EndEdit();
			}
		}

		public override void CancelEdit()
		{
			if (_dataHolder != null)
			{
				_dataHolder.CancelEdit();
			}
		}

		protected override void OnEvaluateVisibility()
		{
			bool isVisible = true;
			if (GetTableInterface() == null)
			{
				return;
			}
			if (GetTableInterface().FilterCondition != null)
			{
				MordexConditionBase mordexConditionBase = (MordexConditionBase)GetTableInterface().FilterCondition.FilterCondition;
				isVisible = mordexConditionBase.EvaluateRow(this);
			}
			if (isVisible)
			{
				if (CanHaveChildren() && !Table.IsSelfReferencingTable())
				{
					bool hideForChildTable = false;
					foreach (IMordexTable childTable in GetTableInterface().ChildTables)
					{
						if (!((MordexTableBase)childTable).ShouldFilterParentRows())
						{
							continue;
						}
						MordexRow mordexRow = base.Rows.FindRowByKey(childTable);
						if (mordexRow != null)
						{
							if (!mordexRow.ChildrenLoaded && !base.Visible)
							{
								hideForChildTable = false;
								continue;
							}
							mordexRow.LoadChildRows();
							mordexRow.UpdateVisibility();
							if (!mordexRow.Visible)
							{
								hideForChildTable = true;
							}
						}
						else
						{
							hideForChildTable = true;
						}
					}
					if (hideForChildTable)
					{
						base.Visible = false;
						return;
					}
				}
			}
			else if (GetTableInterface().FilterCondition != null && Table.IsSelfReferencingTable() && base.Rows.VisibleCount > 0)
			{
				base.Visible = true;
				return;
			}
			base.Visible = isVisible;
		}

		protected override void OnRowChange(MordexRow row, RowChangeType change)
		{
			base.OnRowChange(row, change);
			if (change == RowChangeType.VisibleChanged && row.ParentRow == this)
			{
				base.UpdateVisibility();
			}
		}

		public override void CollapseDataRows(MordexTableBase table)
		{
			if (table == null || table == Table)
			{
				base.Expanded = false;
				base.CollapseDataRows(null);
			}
			else
			{
				base.CollapseDataRows(table);
			}
		}

		public override void ExpandDataRows(MordexTableBase table)
		{
			if (table == Table)
			{
				base.Expanded = true;
			}
			else
			{
				base.ExpandDataRows(table);
			}
		}

		internal void SetRecordPosition(int recordPosition)
		{
			_recordPosition = recordPosition;
		}

		public override bool CanDelete()
		{
			if (_dataHolder != null)
			{
				return _dataHolder.CanDelete();
			}
			return false;
		}

		public override void Delete()
		{
			if (_dataHolder != null)
			{
				_dataHolder.Delete();
			}
		}
	}
}
