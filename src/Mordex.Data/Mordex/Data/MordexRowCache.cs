using System.Collections.Generic;
using System.Reflection;

namespace Mordex.Data
{
	[DefaultMember("Item")]
	public class MordexRowCache
	{
		internal const int TypeDataHolder = 0;

		internal const int TypeTableListHolder = 1;

		internal const int TypeTableRow = 2;

		internal const int TypeDataRow = 3;

		internal const int TypeGroupRow = 4;

		internal const int TypeSelfReferencingElement = 5;

		private const int SlotRowType = 0;

		private const int SlotCacheIndex = 1;

		private const int SlotParentIndex = 2;

		private const int SlotChecked = 3;

		private const int SlotExpanded = 4;

		private const int SlotVisible = 5;

		private const int SlotReferenceIndex = 6;

		private const int SlotElementValue = 3;

		private const int SlotElementSecondValue = 4;

		private const int SlotParentElementIndex = 5;

		private const int SlotElementDataRowIndex = 6;

		internal object[] _fieldValues;

		internal object[] _rowSettings;

		private bool _saveFieldValues;

		private object[] _dataRowKeys;

		internal List<MordexRowCache> _childRows;

		internal List<MordexRowCache> _childElements;

		protected bool SaveFieldValues
		{
			get
			{
				return _saveFieldValues;
			}
			set
			{
				_saveFieldValues = value;
			}
		}

		public int RowType
		{
			get
			{
				if (_rowSettings != null)
				{
					return (int)_rowSettings[SlotRowType];
				}
				return -1;
			}
		}

		public object[] FieldValues
		{
			get
			{
				return _fieldValues;
			}
			set
			{
				_fieldValues = value;
			}
		}

		public object[] RowSettings
		{
			get
			{
				return _rowSettings;
			}
			set
			{
				_rowSettings = value;
			}
		}

		public object[] DataRowKeys
		{
			get
			{
				return _dataRowKeys;
			}
			set
			{
				_dataRowKeys = value;
			}
		}

		public MordexRowCache()
		{
			_saveFieldValues = true;
		}

		internal void SaveSelfReferencingElement(IMordexTable iMordexTable, HierarchyNode element, int integer)
		{
			EnsureRowSettingsForType(TypeSelfReferencingElement);
			SetElementValue(element.GetParentMemberValue());
			SetElementSecondValue(element.GetChildMemberValue());
			if (element.GetParent() == null)
			{
				SetParentElementIndex(-2);
			}
			else if (!(element.GetParent() is HierarchyNode parentElement))
			{
				SetParentElementIndex(-1);
			}
			else
			{
				SetParentElementIndex(parentElement._rowCacheIndex);
			}
			SetElementDataRowIndex(integer);
		}

		internal object GetElementValue()
		{
			if (_rowSettings != null)
			{
				return _rowSettings[SlotElementValue];
			}
			return null;
		}

		internal void SetElementValue(object value)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotElementValue] = value;
			}
		}

		internal object GetElementSecondValue()
		{
			if (_rowSettings != null)
			{
				return _rowSettings[SlotElementSecondValue];
			}
			return null;
		}

		internal void SetElementSecondValue(object value)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotElementSecondValue] = value;
			}
		}

		internal int GetParentElementIndex()
		{
			if (_rowSettings != null)
			{
				return (int)_rowSettings[SlotParentElementIndex];
			}
			return -1;
		}

		internal void SetParentElementIndex(int integer)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotParentElementIndex] = integer;
			}
		}

		internal int GetElementDataRowIndex()
		{
			if (_rowSettings != null)
			{
				return (int)_rowSettings[SlotElementDataRowIndex];
			}
			return -1;
		}

		internal void SetElementDataRowIndex(int integer)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotElementDataRowIndex] = integer;
			}
		}

		internal void SaveRow(MordexRow row)
		{
			SaveRowSettings(row);
		}

		internal void MoveValuesToElementRow(List<MordexRowCache> list)
		{
			if (RowType != 0)
			{
				return;
			}
			int referenceIndex = GetReferenceIndex();
			if (referenceIndex >= 0 && referenceIndex < list.Count)
			{
				MordexRowCache mordexRowCache = list[referenceIndex];
				if (mordexRowCache.RowType == TypeSelfReferencingElement)
				{
					mordexRowCache._fieldValues = _fieldValues;
					_fieldValues = null;
					mordexRowCache._dataRowKeys = _dataRowKeys;
					_dataRowKeys = null;
				}
			}
		}

		internal void SetSaveFieldValues(bool saveFieldValues)
		{
			_saveFieldValues = saveFieldValues;
		}

		protected virtual void SaveRowSettings(MordexRow row)
		{
			bool savesRowFlags = true;
			if (row is DataHolderRow)
			{
				EnsureRowSettingsForType(TypeDataHolder);
				DataHolderRow dataHolderRow = row as DataHolderRow;
				if (_saveFieldValues)
				{
					_fieldValues = new object[row.GetTableInterface().Fields.Count];
					for (int i = 0; i < row.GetTableInterface().Fields.Count; i++)
					{
						if (!dataHolderRow.GetTableInterface().Fields[i].MultipleValues)
						{
							_fieldValues[i] = dataHolderRow.GetValue((MordexFieldBase)dataHolderRow.GetTableInterface().Fields[i]);
						}
					}
				}
				HierarchyNode holderState = dataHolderRow.GetHierarchyNode();
				if (holderState != null)
				{
					SetReferenceIndex(holderState._rowCacheIndex);
				}
				else
				{
					SetReferenceIndex(-1);
				}
				SetCheckedFlag(row.IsCheckedCore);
				_dataRowKeys = dataHolderRow.DataRowKeys;
				savesRowFlags = false;
			}
			else if (row is TableListHolderRow)
			{
				EnsureRowSettingsForType(TypeTableListHolder);
				savesRowFlags = false;
			}
			else if (row is MordexTableRow)
			{
				EnsureRowSettingsForType(TypeTableRow);
				MordexTableRow mordexTableRow = (MordexTableRow)row;
				if (_saveFieldValues && mordexTableRow.GetTableListHolderRow() != null)
				{
					SetReferenceIndex(mordexTableRow.GetTableListHolderRow().GetCacheIndex());
				}
				else if (row.Table.HasOwnerField())
				{
					SetReferenceIndex(-32768 - row.Table.GetOwnerField().Index);
				}
				else if (row.GetTableInterface().HierarchicalMode != HierarchicalMode.UseChildTables)
				{
					SetReferenceIndex(int.MinValue);
				}
				else
				{
					SetReferenceIndex(-row.Table.Index - 1);
				}
			}
			else if (row is MordexDataRow)
			{
				EnsureRowSettingsForType(TypeDataRow);
				MordexDataRow mordexDataRow = row as MordexDataRow;
				SetReferenceIndex(mordexDataRow.DataHolder.GetCacheIndex());
				_dataRowKeys = mordexDataRow.DataRowKeys;
			}
			else if (row is MordexGroupRow)
			{
				EnsureRowSettingsForType(TypeGroupRow);
				MordexGroupRow mordexGroupRow = row as MordexGroupRow;
				if (mordexGroupRow.GroupValue is MordexCustomGroupRowBase mordexCustomGroupRowBase)
				{
					_fieldValues = new object[1];
					_fieldValues[0] = mordexCustomGroupRowBase.Index;
				}
				else if (_saveFieldValues)
				{
					_fieldValues = new object[3];
					_fieldValues[0] = mordexGroupRow.GroupValue;
					_fieldValues[2] = mordexGroupRow.GroupText;
				}
				else
				{
					_fieldValues = new object[1];
					_fieldValues[0] = mordexGroupRow.GroupValue;
				}
			}
			if (row.ParentRow != null)
			{
				SetParentIndex(row.ParentRow.GetCacheIndex());
			}
			else
			{
				SetParentIndex(-1);
			}
			if (savesRowFlags)
			{
				SetCheckedFlag(row.IsCheckedCore);
				SetExpandedFlag(row.Expanded);
				SetVisibleFlag(row.Visible);
			}
		}

		internal object GetFieldValue(int integer)
		{
			if (_fieldValues != null)
			{
				return _fieldValues[integer];
			}
			return null;
		}

		internal void SetFieldValue(int integer, object value)
		{
			if (_fieldValues != null)
			{
				_fieldValues[integer] = value;
			}
		}

		internal void EnsureRowSettingsForType(int integer)
		{
			if (integer != RowType)
			{
				int settingsLength = 0;
				switch (integer)
				{
				case TypeDataHolder:
				case TypeTableRow:
				case TypeDataRow:
				case TypeSelfReferencingElement:
					settingsLength = SlotReferenceIndex;
					break;
				case TypeGroupRow:
					settingsLength = SlotVisible;
					break;
				case TypeTableListHolder:
					settingsLength = SlotParentIndex;
					break;
				}
				_rowSettings = new object[settingsLength + 1];
				_rowSettings[SlotRowType] = integer;
			}
		}

		internal int GetReferenceIndex()
		{
			if (_rowSettings != null)
			{
				return (int)_rowSettings[SlotReferenceIndex];
			}
			return -1;
		}

		internal void SetReferenceIndex(int integer)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotReferenceIndex] = integer;
			}
		}

		internal bool GetVisibleFlag()
		{
			if (_rowSettings != null)
			{
				return (bool)_rowSettings[SlotVisible];
			}
			return true;
		}

		internal void SetVisibleFlag(bool boolean)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotVisible] = boolean;
			}
		}

		internal bool GetCheckedFlag()
		{
			if (_rowSettings != null)
			{
				return (bool)_rowSettings[SlotChecked];
			}
			return true;
		}

		internal void SetCheckedFlag(bool boolean)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotChecked] = boolean;
			}
		}

		internal bool GetExpandedFlag()
		{
			if (_rowSettings != null)
			{
				return (bool)_rowSettings[SlotExpanded];
			}
			return true;
		}

		internal void SetExpandedFlag(bool boolean)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotExpanded] = boolean;
			}
		}

		internal int GetParentIndex()
		{
			if (_rowSettings != null)
			{
				return (int)_rowSettings[SlotParentIndex];
			}
			return -1;
		}

		internal void SetParentIndex(int integer)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotParentIndex] = integer;
			}
		}

		internal int GetCacheIndex()
		{
			if (_rowSettings != null)
			{
				return (int)_rowSettings[SlotCacheIndex];
			}
			return -1;
		}

		internal void SetCacheIndex(int integer)
		{
			if (_rowSettings != null)
			{
				_rowSettings[SlotCacheIndex] = integer;
			}
		}
	}
}
