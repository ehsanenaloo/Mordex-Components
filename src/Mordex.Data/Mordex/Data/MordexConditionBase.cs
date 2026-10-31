using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexConditionBase : MordexCollectionElement, ICloneable, IMordexCondition, ICollectionOwner, IFilterCondition
	{
		private bool _enabled;

		private IConditionOwner _owner;

		private object _firstValue;

		private object _secondValue;

		private ConditionOperator _conditionOperator;

		private MordexFieldBase _field;

		private MordexFieldBase _firstValueField;

		private MordexFieldBase _secondValueField;

		private bool _negation;

		private LogicalOperator _logicalOperator;

		private MordexConditionCollectionBase _childConditions;

		private bool _disableValidation;

		protected MordexConditionCollectionBase BaseConditions => _childConditions;

		protected MordexTableBase BaseTable
		{
			get
			{
				if (_owner != null)
				{
					return (MordexTableBase)_owner.Table;
				}
				if (GetOwningCollection() != null)
				{
					MordexConditionBase mordexConditionBase = (MordexConditionBase)GetOwningCollection().GetOwner();
					return mordexConditionBase.BaseTable;
				}
				return null;
			}
		}

		protected IConditionOwner BaseOwner
		{
			get
			{
				return _owner;
			}
			set
			{
				_owner = value;
			}
		}

		protected bool DisableValidation
		{
			get
			{
				return _disableValidation;
			}
			set
			{
				_disableValidation = value;
			}
		}

		protected MordexFieldBase BaseField
		{
			get
			{
				return _field;
			}
			set
			{
				EnsureSettingsCanChange();
				_field = value;
			}
		}

		protected bool BaseEnabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				EnsureSettingsCanChange();
				_enabled = value;
			}
		}

		protected object BaseValue1
		{
			get
			{
				return _firstValue;
			}
			set
			{
				EnsureSettingsCanChange();
				_firstValue = value;
			}
		}

		protected object BaseValue2
		{
			get
			{
				return _secondValue;
			}
			set
			{
				EnsureSettingsCanChange();
				_secondValue = value;
			}
		}

		protected ConditionOperator BaseConditionOperator
		{
			get
			{
				return _conditionOperator;
			}
			set
			{
				EnsureSettingsCanChange();
				if (!Enum.IsDefined(typeof(ConditionOperator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_conditionOperator = value;
			}
		}

		protected bool BaseNegation
		{
			get
			{
				return _negation;
			}
			set
			{
				EnsureSettingsCanChange();
				_negation = value;
			}
		}

		protected LogicalOperator BaseLogicalOperator
		{
			get
			{
				return _logicalOperator;
			}
			set
			{
				EnsureSettingsCanChange();
				if (!Enum.IsDefined(typeof(LogicalOperator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_logicalOperator = value;
			}
		}

		protected MordexFieldBase BaseValueField1
		{
			get
			{
				return _firstValueField;
			}
			set
			{
				EnsureSettingsCanChange();
				_firstValueField = value;
			}
		}

		protected MordexFieldBase BaseValueField2
		{
			get
			{
				return _secondValueField;
			}
			set
			{
				EnsureSettingsCanChange();
				_secondValueField = value;
			}
		}

		protected bool BaseIsComposite => _childConditions.Count > 0;

		IMordexCondition IFilterCondition.FilterCondition => this;

		bool IMordexCondition.Enabled
		{
			get
			{
				return BaseEnabled;
			}
			set
			{
				BaseEnabled = value;
			}
		}

		ConditionOperator IMordexCondition.ConditionOperator
		{
			get
			{
				return BaseConditionOperator;
			}
			set
			{
				BaseConditionOperator = value;
			}
		}

		IMordexField IMordexCondition.Field
		{
			get
			{
				return BaseField;
			}
			set
			{
				BaseField = (MordexFieldBase)value;
			}
		}

		bool IMordexCondition.IsComposite => BaseIsComposite;

		LogicalOperator IMordexCondition.LogicalOperator
		{
			get
			{
				return BaseLogicalOperator;
			}
			set
			{
				BaseLogicalOperator = value;
			}
		}

		bool IMordexCondition.Negation
		{
			get
			{
				return BaseNegation;
			}
			set
			{
				BaseNegation = value;
			}
		}

		IMordexConditionCollection IMordexCondition.Conditions => BaseConditions;

		object IMordexCondition.Value1
		{
			get
			{
				return BaseValue1;
			}
			set
			{
				BaseValue1 = value;
			}
		}

		object IMordexCondition.Value2
		{
			get
			{
				return BaseValue2;
			}
			set
			{
				BaseValue2 = value;
			}
		}

		IMordexField IMordexCondition.ValueField1
		{
			get
			{
				return BaseValueField1;
			}
			set
			{
				BaseValueField1 = (MordexFieldBase)value;
			}
		}

		IMordexField IMordexCondition.ValueField2
		{
			get
			{
				return BaseValueField2;
			}
			set
			{
				BaseValueField2 = (MordexFieldBase)value;
			}
		}

		public MordexConditionBase()
			: this(null, ConditionOperator.Equal, null)
		{
		}

		public MordexConditionBase(MordexFieldBase field, ConditionOperator conditionOperator, object firstValue)
			: this(field, conditionOperator, firstValue, null)
		{
		}

		public MordexConditionBase(MordexFieldBase field, ConditionOperator conditionOperator, object firstValue, object secondValue)
		{
			InitializeDefaults();
			_field = field;
			_conditionOperator = conditionOperator;
			MordexFieldBase mordexFieldBase = firstValue as MordexFieldBase;
			MordexFieldBase secondValueField = secondValue as MordexFieldBase;
			if (mordexFieldBase != null)
			{
				_firstValueField = mordexFieldBase;
			}
			else
			{
				_firstValue = firstValue;
			}
			if (secondValueField != null)
			{
				_secondValueField = secondValueField;
			}
			else
			{
				_secondValue = secondValue;
			}
		}

		private IMordexField GetFieldAsInterface()
		{
			return _field;
		}

		private void InitializeDefaults()
		{
			_enabled = true;
			_childConditions = CreateConditions();
			_logicalOperator = LogicalOperator.None;
			_negation = false;
		}

		protected virtual MordexConditionCollectionBase CreateConditions()
		{
			return MordexConditionCollectionBase.CreateForOwner(this);
		}

		protected virtual MordexConditionBase CreateCondition()
		{
			return new MordexConditionBase();
		}

		internal IConditionOwner GetConditionOwner()
		{
			return _owner;
		}

		internal void SetConditionOwner(IConditionOwner baseOwner)
		{
			_owner = baseOwner;
		}

		internal void ValidateTable(IMordexTable iMordexTable)
		{
			IMordexTable mordexTable = null;
			IMordexField mordexField = GetFieldAsInterface();
			if (mordexField != null)
			{
				mordexTable = mordexField.Table;
			}
			if (mordexField != null && mordexTable != iMordexTable && !((MordexTableBase)iMordexTable).IsAncestorOf((MordexTableBase)mordexTable))
			{
				throw new InvalidOperationException("Field in condition must belong to the table the condition is applied or to one of its child tables.");
			}
			if (_firstValueField != null && ((IMordexField)_firstValueField).Table != mordexTable)
			{
				throw new InvalidOperationException("ValueField1 in condition does not belong to the table.");
			}
			if (_secondValueField != null && ((IMordexField)_secondValueField).Table != mordexTable)
			{
				throw new InvalidOperationException("ValueField2 in condition does not belong to the table.");
			}
			foreach (MordexConditionBase item in _childConditions)
			{
				item.ValidateTable(iMordexTable);
			}
		}

		protected void BaseValidate(MordexTableBase table)
		{
			ValidateTable(table);
		}

		private bool CanChangeSettings()
		{
			if (_disableValidation)
			{
				return true;
			}
			if (_owner != null)
			{
				return false;
			}
			return true;
		}

		private void EnsureSettingsCanChange()
		{
			if (!CanChangeSettings())
			{
				throw new InvalidOperationException("FilterCondition settings can not be changed when the filter is applied to a table.");
			}
		}

		internal bool EvaluateRow(MordexRow mordexRow)
		{
			bool result = (BaseIsComposite ? EvaluateWithChildren(GetCulture(), mordexRow) : EvaluateSingleCondition(GetCulture(), mordexRow));
			if (_negation)
			{
				return !result;
			}
			return result;
		}

		private bool HasNoField()
		{
			return _field == null;
		}

		private bool EvaluateWithChildren(CultureInfo cultureInfo, MordexRow mordexRow)
		{
			bool result = EvaluateSingleCondition(cultureInfo, mordexRow);
			bool takeNextAsResult = HasNoField();
			foreach (MordexConditionBase item in _childConditions)
			{
				if (takeNextAsResult)
				{
					result = item.EvaluateRow(mordexRow);
				}
				else
				{
					switch (item._logicalOperator)
					{
					case LogicalOperator.Or:
						result = result || item.EvaluateRow(mordexRow);
						break;
					case LogicalOperator.Xor:
						result ^= item.EvaluateRow(mordexRow);
						break;
					default:
						result = result && item.EvaluateRow(mordexRow);
						break;
					}
				}
				takeNextAsResult = false;
			}
			return result;
		}

		protected bool BaseEvaluateRow(MordexRow row)
		{
			return EvaluateRow(row);
		}

		private static bool IsBetween(CultureInfo cultureInfo, object value, object firstBound, object secondBound, bool boolean, IComparer iComparer, Type domain)
		{
			if (CompareValues(cultureInfo, firstBound, secondBound, iComparer, boolean, domain) <= 0)
			{
				if (CompareValues(cultureInfo, value, firstBound, iComparer, boolean, domain) >= 0)
				{
					return CompareValues(cultureInfo, value, secondBound, iComparer, boolean, domain) <= 0;
				}
				return false;
			}
			if (CompareValues(cultureInfo, value, secondBound, iComparer, boolean, domain) >= 0)
			{
				return CompareValues(cultureInfo, value, firstBound, iComparer, boolean, domain) <= 0;
			}
			return false;
		}

		private bool EvaluateSingleCondition(CultureInfo cultureInfo, MordexRow mordexRow)
		{
			if (_field == null || !_enabled || ((IMordexField)_field).Table == null)
			{
				return true;
			}
			MordexTableBase mordexTableBase = (MordexTableBase)GetFieldAsInterface().Table;
			if (mordexTableBase != mordexRow.Table)
			{
				DataHolderRow dataHolderRow = mordexRow as DataHolderRow;
				if (dataHolderRow == null && mordexRow is MordexDataRow mordexDataRow)
				{
					dataHolderRow = mordexDataRow.DataHolder;
				}
				if (dataHolderRow != null)
				{
					IList list = dataHolderRow.GetRowsForTable(mordexTableBase);
					foreach (MordexRow item in list)
					{
						if (EvaluateSingleCondition(cultureInfo, item))
						{
							return true;
						}
					}
					return false;
				}
			}
			object fieldValue = ((MordexFieldBase)GetFieldAsInterface()).GetRowValueToCompare(mordexRow);
			object firstValue = _firstValue;
			object secondValue = _secondValue;
			if (_firstValueField != null)
			{
				firstValue = _firstValueField.GetRowValueToCompare(mordexRow);
			}
			if (_secondValueField != null)
			{
				secondValue = _secondValueField.GetRowValueToCompare(mordexRow);
			}
			if (firstValue == null)
			{
				firstValue = DBNull.Value;
			}
			if (secondValue == null)
			{
				secondValue = DBNull.Value;
			}
			bool compareIgnoreCase = ((IMordexField)_field).CompareIgnoreCase;
			IComparer sortComparer = ((IMordexField)_field).SortComparer;
			Type domain = ((MordexFieldBase)GetFieldAsInterface()).GetCompareDomain();
			if (((IMordexField)_field).MultipleValues)
			{
				if (!(fieldValue is IList fieldValues))
				{
					switch (_conditionOperator)
					{
					case ConditionOperator.IsNull:
					case ConditionOperator.IsEmpty:
						return true;
					default:
						return false;
					}
				}
				return EvaluateValueList(_conditionOperator, cultureInfo, fieldValues, firstValue, secondValue, compareIgnoreCase, sortComparer, domain);
			}
			else
			{
				switch (_conditionOperator)
				{
				case ConditionOperator.BeginsWith:
					return StartsWithText(cultureInfo, fieldValue, firstValue, compareIgnoreCase);
				case ConditionOperator.Between:
					return IsBetween(cultureInfo, fieldValue, firstValue, secondValue, compareIgnoreCase, sortComparer, domain);
				case ConditionOperator.Contains:
					return ContainsText(cultureInfo, fieldValue, firstValue, compareIgnoreCase);
				case ConditionOperator.EndsWith:
					return EndsWithText(cultureInfo, fieldValue, firstValue, compareIgnoreCase);
				case ConditionOperator.Equal:
					return CompareValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain) == 0;
				case ConditionOperator.GreaterThan:
					return CompareValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain) > 0;
				case ConditionOperator.GreaterThanOrEqualTo:
					return CompareValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain) >= 0;
				case ConditionOperator.IsEmpty:
					return IsNullOrEmptyValue(fieldValue);
				case ConditionOperator.IsNull:
					return fieldValue == DBNull.Value;
				case ConditionOperator.LessThan:
					return CompareValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain) < 0;
				case ConditionOperator.LessThanOrEqualTo:
					return CompareValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain) <= 0;
				case ConditionOperator.NotBetween:
					return !IsBetween(cultureInfo, fieldValue, firstValue, secondValue, compareIgnoreCase, sortComparer, domain);
				case ConditionOperator.NotContains:
					return !ContainsText(cultureInfo, fieldValue, firstValue, compareIgnoreCase);
				case ConditionOperator.NotEqual:
					return CompareValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain) != 0;
				case ConditionOperator.NotIsEmpty:
					return !IsNullOrEmptyValue(fieldValue);
				case ConditionOperator.NotIsNull:
					return fieldValue != DBNull.Value;
				case ConditionOperator.In:
					return IsInValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain);
				case ConditionOperator.NotIn:
					return !IsInValues(cultureInfo, fieldValue, firstValue, sortComparer, compareIgnoreCase, domain);
				}
			}
			return true;
		}

		// Evaluates an operator against a list of values. Positive operators match when any item matches;
		// negative operators match when no item matches.
		internal static bool EvaluateValueList(ConditionOperator conditionOperator, CultureInfo cultureInfo, IList fieldValues, object firstValue, object secondValue, bool compareIgnoreCase, IComparer sortComparer, Type domain)
		{
			switch (conditionOperator)
			{
			case ConditionOperator.BeginsWith:
				foreach (object item2 in fieldValues)
				{
					if (StartsWithText(cultureInfo, item2, firstValue, compareIgnoreCase))
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.Between:
				foreach (object item3 in fieldValues)
				{
					if (IsBetween(cultureInfo, item3, firstValue, secondValue, compareIgnoreCase, sortComparer, domain))
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.Contains:
				foreach (object item4 in fieldValues)
				{
					if (ContainsText(cultureInfo, item4, firstValue, compareIgnoreCase))
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.EndsWith:
				foreach (object item5 in fieldValues)
				{
					if (EndsWithText(cultureInfo, item5, firstValue, compareIgnoreCase))
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.Equal:
				foreach (object item6 in fieldValues)
				{
					if (CompareValues(cultureInfo, item6, firstValue, sortComparer, compareIgnoreCase, domain) == 0)
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.GreaterThan:
				foreach (object item7 in fieldValues)
				{
					if (CompareValues(cultureInfo, item7, firstValue, sortComparer, compareIgnoreCase, domain) > 0)
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.GreaterThanOrEqualTo:
				foreach (object item8 in fieldValues)
				{
					if (CompareValues(cultureInfo, item8, firstValue, sortComparer, compareIgnoreCase, domain) >= 0)
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.IsEmpty:
				foreach (object item9 in fieldValues)
				{
					if (!IsNullOrEmptyValue(item9))
					{
						return false;
					}
				}
				return true;
			case ConditionOperator.IsNull:
				return false;
			case ConditionOperator.LessThan:
				foreach (object item10 in fieldValues)
				{
					if (CompareValues(cultureInfo, item10, firstValue, sortComparer, compareIgnoreCase, domain) < 0)
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.LessThanOrEqualTo:
				foreach (object item11 in fieldValues)
				{
					if (CompareValues(cultureInfo, item11, firstValue, sortComparer, compareIgnoreCase, domain) <= 0)
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.NotBetween:
				foreach (object item12 in fieldValues)
				{
					if (IsBetween(cultureInfo, item12, firstValue, secondValue, compareIgnoreCase, sortComparer, domain))
					{
						return false;
					}
				}
				return true;
			case ConditionOperator.NotContains:
				foreach (object item13 in fieldValues)
				{
					if (ContainsText(cultureInfo, item13, firstValue, compareIgnoreCase))
					{
						return false;
					}
				}
				return true;
			case ConditionOperator.NotEqual:
				foreach (object item14 in fieldValues)
				{
					if (CompareValues(cultureInfo, item14, firstValue, sortComparer, compareIgnoreCase, domain) == 0)
					{
						return false;
					}
				}
				return true;
			case ConditionOperator.NotIsEmpty:
				foreach (object item15 in fieldValues)
				{
					if (!IsNullOrEmptyValue(item15))
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.NotIsNull:
				return true;
			case ConditionOperator.In:
				foreach (object item16 in fieldValues)
				{
					if (IsInValues(cultureInfo, item16, firstValue, sortComparer, compareIgnoreCase, domain))
					{
						return true;
					}
				}
				return false;
			case ConditionOperator.NotIn:
				foreach (object item17 in fieldValues)
				{
					if (IsInValues(cultureInfo, item17, firstValue, sortComparer, compareIgnoreCase, domain))
					{
						return false;
					}
				}
				return true;
			}
			return true;
		}

		private static bool IsInValues(CultureInfo cultureInfo, object value, object candidates, IComparer iComparer, bool boolean, Type domain)
		{
			if (candidates is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					if (CompareValues(cultureInfo, value, item, iComparer, boolean, domain) == 0)
					{
						return true;
					}
				}
				return false;
			}
			return CompareValues(cultureInfo, value, candidates, iComparer, boolean, domain) == 0;
		}

		private static bool IsNullOrEmptyValue(object value)
		{
			if (value == DBNull.Value || value == null)
			{
				return true;
			}
			if (value is string text)
			{
				return text.Length == 0;
			}
			return false;
		}

		private CultureInfo GetCulture()
		{
			DataSourceManager dataSourceManager = null;
			IMordexTable table = BaseTable;
			if (table != null)
			{
				dataSourceManager = table.DataSourceManager;
			}
			return DataValueHelper.GetCulture(dataSourceManager);
		}

		private static int CompareValues(CultureInfo cultureInfo, object value, object otherValue, IComparer iComparer, bool ignoreCase, Type domain)
		{
			return DataValueHelper.CompareInDomain(cultureInfo, value, otherValue, iComparer, ignoreCase, domain);
		}

		private static CompareOptions GetCompareOptions(bool ignoreCase)
		{
			return ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
		}

		private static bool StartsWithText(CultureInfo cultureInfo, object text, object prefix, bool ignoreCase)
		{
			return cultureInfo.CompareInfo.IsPrefix(string.Concat(text), string.Concat(prefix), GetCompareOptions(ignoreCase));
		}

		private static bool ContainsText(CultureInfo cultureInfo, object text, object searchText, bool ignoreCase)
		{
			return cultureInfo.CompareInfo.IndexOf(string.Concat(text), string.Concat(searchText), GetCompareOptions(ignoreCase)) > -1;
		}

		private static bool EndsWithText(CultureInfo cultureInfo, object text, object suffix, bool ignoreCase)
		{
			return cultureInfo.CompareInfo.IsSuffix(string.Concat(text), string.Concat(suffix), GetCompareOptions(ignoreCase));
		}

		protected MordexConditionBase BaseAddCondition()
		{
			MordexConditionBase mordexConditionBase = CreateCondition();
			BaseAddCondition(mordexConditionBase);
			return mordexConditionBase;
		}

		protected void BaseAddCondition(MordexConditionBase condition)
		{
			_childConditions.AddWithDefaultOperator(condition);
		}

		protected void BaseAddCondition(LogicalOperator logicalOperator, MordexConditionBase condition)
		{
			_childConditions.AddWithOperator(logicalOperator, condition);
		}

		protected void BaseRemoveCondition(MordexConditionBase condition)
		{
			_childConditions.RemoveCondition(condition);
		}

		protected void BaseRemoveConditionAt(int index)
		{
			((IList)_childConditions).RemoveAt(index);
		}

		protected void BaseClear()
		{
			EnsureSettingsCanChange();
			_field = null;
			_firstValue = null;
			_secondValue = null;
			_firstValueField = null;
			_secondValueField = null;
			_conditionOperator = ConditionOperator.Equal;
			_negation = false;
			_logicalOperator = LogicalOperator.None;
			_childConditions.Clear();
		}

		protected MordexConditionBase BaseClone()
		{
			MordexConditionBase mordexConditionBase = CreateCondition();
			mordexConditionBase._field = _field;
			mordexConditionBase._conditionOperator = _conditionOperator;
			mordexConditionBase._logicalOperator = _logicalOperator;
			mordexConditionBase._negation = _negation;
			mordexConditionBase._firstValue = _firstValue;
			mordexConditionBase._secondValue = _secondValue;
			mordexConditionBase._firstValueField = _firstValueField;
			mordexConditionBase._secondValueField = _secondValueField;
			mordexConditionBase._enabled = _enabled;
			mordexConditionBase.BaseConditions.AddClonesFrom(_childConditions);
			return mordexConditionBase;
		}

		object ICloneable.Clone()
		{
			return BaseClone();
		}

		internal MordexTableBase FindTable()
		{
			if (_field != null)
			{
				return (MordexTableBase)((IMordexField)_field).Table;
			}
			foreach (MordexConditionBase item in _childConditions)
			{
				IMordexTable mordexTable = item.FindTable();
				if (mordexTable != null)
				{
					return (MordexTableBase)mordexTable;
				}
			}
			return null;
		}

		IMordexCondition IMordexCondition.AddCondition()
		{
			return BaseAddCondition();
		}

		void IMordexCondition.AddCondition(IMordexCondition condition)
		{
			BaseAddCondition((MordexConditionBase)condition);
		}

		void IMordexCondition.AddCondition(LogicalOperator logicalOperator, IMordexCondition condition)
		{
			BaseAddCondition(logicalOperator, (MordexConditionBase)condition);
		}

		void IMordexCondition.Clear()
		{
			BaseClear();
		}

		void IMordexCondition.RemoveCondition(IMordexCondition condition)
		{
			BaseRemoveCondition((MordexConditionBase)condition);
		}

		void IMordexCondition.RemoveConditionAt(int index)
		{
			BaseRemoveConditionAt(index);
		}

		CollectionChangingAction ICollectionOwner.OnCollectionChanging(MordexCollectionBase collection, CollectionChangeType change, object item)
		{
			if (!CanChangeSettings())
			{
				return CollectionChangingAction.Cancel;
			}
			return CollectionChangingAction.Continue;
		}

		void ICollectionOwner.OnCollectionChanged(object collection, CollectionChangeType change, object item)
		{
		}
	}
}
