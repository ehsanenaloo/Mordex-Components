using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mordex.Data
{
	public class MordexNewDataRow : MordexRow
	{
		internal UnboundFieldValues _unboundValues;

		private MordexTableRow _parentTableRow;

		private bool _isEditing;

		internal object _dataItem;

		internal bool _mustAddToList;

		private bool _isCheckedCore;

		public override MordexTableBase Table => _parentTableRow.Table;

		public override MordexRow ParentRow => _parentTableRow;

		public override object DataRow => _dataItem;

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

		internal MordexNewDataRow(MordexTableRow parentTableRow)
		{
			_parentTableRow = parentTableRow;
		}

		protected override int CompareTo(MordexRow row)
		{
			if (row == this)
			{
				return 0;
			}
			NewRowPosition newRowPosition = Mordex.Data.NewRowPosition.TopRow;
			if (Table.GetEffectiveNewRowPosition() == newRowPosition)
			{
				return -1;
			}
			return 1;
		}

		public override object GetValue(MordexFieldBase field)
		{
			if (field != null && ((IMordexField)field).BoundMode == ColumnBoundMode.UnboundKeepValues)
			{
				return GetUnboundValue(field);
			}
			return GetValueOfFieldCore(field, ((IMordexField)field).DataMember);
		}

		protected override object GetValueOfFieldCore(MordexFieldBase field, string dataMember)
		{
			if (_dataItem != null && field != null)
			{
				if (_dataItem is MordexRowCache mordexRowCache)
				{
					if (mordexRowCache.GetFieldValue(field.Index) == null && ((IMordexField)field).MultipleValues)
					{
						object valueList = new List<MordexRowCache>();
						mordexRowCache.SetFieldValue(field.Index, valueList);
					}
					return mordexRowCache.GetFieldValue(field.Index);
				}
				if (_parentTableRow != null && _parentTableRow.GetTableListHolderRow() != null)
				{
					return _parentTableRow.GetTableListHolderRow().GetRowValue(field, _dataItem, dataMember);
				}
				return field.GetItemValue(_dataItem);
			}
			return null;
		}

		public override void SetValue(MordexFieldBase field, object newValue)
		{
			if (field == null)
			{
				return;
			}
			if (_dataItem is MordexRowCache mordexRowCache)
			{
				if (mordexRowCache._fieldValues != null && field.Index < mordexRowCache._fieldValues.Length)
				{
					mordexRowCache.SetFieldValue(field.Index, newValue);
				}
			}
			else if (((IMordexField)field).BoundMode == ColumnBoundMode.UnboundKeepValues)
			{
				SetUnboundValue(field, newValue);
			}
			else
			{
				SetValue(field, ((IMordexField)field).DataMember, newValue);
			}
		}

		internal object GetUnboundValue(MordexFieldBase mordexFieldBase)
		{
			if (_unboundValues == null)
			{
				return null;
			}
			return _unboundValues.GetValue(mordexFieldBase);
		}

		internal void SetUnboundValue(MordexFieldBase mordexFieldBase, object value)
		{
			if (_unboundValues == null)
			{
				_unboundValues = new UnboundFieldValues();
			}
			_unboundValues.SetValue(mordexFieldBase, value);
		}

		private TableListHolderRow GetParentListHolderRow()
		{
			if (_parentTableRow != null)
			{
				return _parentTableRow.GetTableListHolderRow();
			}
			return null;
		}

		public override void SetValue(MordexFieldBase field, string dataMember, object newValue)
		{
			if (GetParentListHolderRow() != null)
			{
				GetParentListHolderRow().SetRowValue(DataValueHelper.GetCulture(DataSourceManager), field, DataRow, dataMember, newValue);
			}
			else
			{
				DataValueHelper.SetPropertyValue(DataValueHelper.GetCulture(DataSourceManager), DataRow, null, dataMember, newValue);
			}
		}

		public override void BeginEdit()
		{
			if (!_isEditing)
			{
				if (_dataItem != null && !_mustAddToList)
				{
					// A failed rollback left an item in the source. It goes back first, so a retry never adds a second one.
					_parentTableRow.DiscardNewRowItem(this);
				}
				object previousItem = _dataItem;
				_isEditing = true;
				try
				{
					_parentTableRow.BeginAddNewRow(this);
					if (_dataItem is IEditableObject)
					{
						((IEditableObject)_dataItem).BeginEdit();
					}
				}
				catch (Exception startFailure)
				{
					// The item allocated for this attempt goes back to the source, so a retry allocates only one item.
					_isEditing = false;
					if (_dataItem != previousItem)
					{
						try
						{
							_parentTableRow.DiscardNewRowItem(this);
						}
						catch (Exception rollbackFailure)
						{
							// The start failure is the one that is thrown; the item stays pending until the next try.
							startFailure.Data["RollbackFailure"] = rollbackFailure;
						}
					}
					throw;
				}
			}
		}

		internal int CommitNewRow(int integer)
		{
			if (!_isEditing)
			{
				return -1;
			}
			int result = _parentTableRow.CommitNewDataRow(this, integer);
			_isEditing = false;
			return result;
		}

		internal void DiscardMultiValueFieldValues()
		{
			if (_unboundValues == null)
			{
				return;
			}
			foreach (IMordexField field in GetTableInterface().Fields)
			{
				MordexFieldBase mordexFieldBase = field as MordexFieldBase;
				if (field.MultipleValues && field.BoundMode == ColumnBoundMode.Bound)
				{
					_unboundValues.RemoveValue(mordexFieldBase);
				}
			}
			if (_unboundValues.IsEmpty())
			{
				_unboundValues = null;
			}
		}

		internal void CommitMultiValueFieldValues()
		{
			if (_unboundValues == null)
			{
				return;
			}
			foreach (IMordexField field in GetTableInterface().Fields)
			{
				MordexFieldBase mordexFieldBase = field as MordexFieldBase;
				if (field.MultipleValues && field.BoundMode == ColumnBoundMode.Bound)
				{
					mordexFieldBase.CommitListValue(this, GetUnboundValue(mordexFieldBase));
				}
			}
		}

		public override void EndEdit()
		{
			if (_isEditing)
			{
				_parentTableRow.CommitNewDataRow(this);
				_isEditing = false;
			}
		}

		public override void CancelEdit()
		{
			_parentTableRow.CancelNewDataRow(this, _isEditing);
			_dataItem = null;
			_isEditing = false;
		}

		public void AddExtraNewRow()
		{
			_parentTableRow.AddBottomExtraNewRow();
		}
	}
}
