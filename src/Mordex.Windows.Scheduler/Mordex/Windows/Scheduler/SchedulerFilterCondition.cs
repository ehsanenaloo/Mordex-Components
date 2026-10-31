using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(SchedulerFilterConditionConverter))]
	public class SchedulerFilterCondition : ICloneable, IMordexXmlLayoutsSupport, IFilterCondition
	{
		internal MxScheduler _scheduler = default;

		internal static string _text = "Filter condition properties can not be changed once it is applied to a Schedule control.";

		internal SchedulerFilterConditionCollection _owner;

		internal ISchedulerFilterOwner _filterOwner;

		private ConditionOperator _conditionOperator;

		private SchedulerField _field;

		private object _firstValue;

		private object _secondValue;

		private SchedulerField _firstValueField;

		private SchedulerField _secondValueField;

		private bool _negation;

		private string _key = "";

		private LogicalOperator _logicalOperator;

		private SchedulerFilterConditionCollection _conditions;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public MxScheduler Scheduler
		{
			get
			{
				MxScheduler scheduler = ((_owner != null) ? _owner.GetSchedulerFilterCondition().Scheduler : ((_filterOwner != null) ? _filterOwner.Scheduler : null));
				if (scheduler == null && _field != null)
				{
					return _field.Scheduler;
				}
				return scheduler;
			}
		}

		[DefaultValue(ConditionOperator.Equal)]
		[Description("Indicates the operator used for comparison.")]
		public ConditionOperator ConditionOperator
		{
			get
			{
				return _conditionOperator;
			}
			set
			{
				if (_filterOwner != null)
				{
					throw new InvalidOperationException(_text);
				}
				if (value != _conditionOperator)
				{
					_conditionOperator = value;
				}
			}
		}

		[Description("The SchedulerField to compare the value(s).")]
		[DefaultValue(null)]
		[Editor(typeof(FieldEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(FieldConverter))]
		public SchedulerField Field
		{
			get
			{
				return _field;
			}
			set
			{
				if (_filterOwner != null)
				{
					throw new InvalidOperationException(_text);
				}
				if (value != _field)
				{
					_field = value;
				}
			}
		}

		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		[Description("The object value to compare")]
		public object Value1
		{
			get
			{
				return _firstValue;
			}
			set
			{
				if (_filterOwner != null)
				{
					throw new InvalidOperationException(_text);
				}
				if (value != _firstValue)
				{
					_firstValue = value;
				}
			}
		}

		[DefaultValue(null)]
		[Description("The object value to compare when the condition operator requires two values.")]
		[TypeConverter(typeof(StringConverter))]
		public object Value2
		{
			get
			{
				return _secondValue;
			}
			set
			{
				if (_filterOwner != null)
				{
					throw new InvalidOperationException(_text);
				}
				if (value != _secondValue)
				{
					_secondValue = value;
				}
			}
		}

		[Description("The SchedulerField object that has the value to compare.")]
		[DefaultValue(null)]
		public SchedulerField ValueField1
		{
			get
			{
				return _firstValueField;
			}
			set
			{
				_firstValueField = value;
			}
		}

		[DefaultValue(null)]
		[Description("The SchedulerField object that has the value to compare when the condition operator requires two values.")]
		public SchedulerField ValueField2
		{
			get
			{
				return _secondValueField;
			}
			set
			{
				_secondValueField = value;
			}
		}

		[DefaultValue(false)]
		public bool Negation
		{
			get
			{
				return _negation;
			}
			set
			{
				if (_filterOwner != null)
				{
					throw new InvalidOperationException(_text);
				}
				if (_negation != value)
				{
					_negation = value;
				}
			}
		}

		[Description("The key used to identify this object in a colletion.")]
		[DefaultValue("")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		public bool IsComposite => _conditions.Count > 0;

		[Description("The logical operator used when the filter condition is composed by two or more conditions.")]
		[DefaultValue(LogicalOperator.None)]
		public LogicalOperator LogicalOperator
		{
			get
			{
				return _logicalOperator;
			}
			set
			{
				if (_filterOwner != null)
				{
					throw new InvalidOperationException(_text);
				}
				if (!Enum.IsDefined(typeof(LogicalOperator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _logicalOperator)
				{
					if (_owner != null && value == LogicalOperator.None)
					{
						throw new ArgumentException("LogicalOperator can not be none in a child condition.");
					}
					_logicalOperator = value;
				}
			}
		}

		SchedulerFilterCondition IFilterCondition.FilterCondition => this;

		public SchedulerFilterConditionCollection Conditions => _conditions;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_negation = (bool)LayoutReader.GetPropertyValue(info, "Negation", typeof(bool), false);
			_conditionOperator = (ConditionOperator)LayoutReader.GetPropertyValue(info, "ConditionOperator", typeof(ConditionOperator), ConditionOperator.Equal);
			_logicalOperator = (LogicalOperator)LayoutReader.GetPropertyValue(info, "LogicalOperator", typeof(LogicalOperator), LogicalOperator.None);
			int fieldIndex = -1;
			if (_filterOwner != null && _filterOwner.Scheduler != null)
			{
				fieldIndex = (int)LayoutReader.GetPropertyValue(info, "FieldIndex", typeof(int), -1);
				if (fieldIndex != -1)
				{
					_field = _filterOwner.Scheduler.Fields[fieldIndex];
				}
				fieldIndex = (int)LayoutReader.GetPropertyValue(info, "ValueFieldIndex1", typeof(int), -1);
				if (fieldIndex != -1)
				{
					_firstValueField = _filterOwner.Scheduler.Fields[fieldIndex];
				}
				fieldIndex = (int)LayoutReader.GetPropertyValue(info, "ValueFieldIndex2", typeof(int), -1);
				if (fieldIndex != -1)
				{
					_secondValueField = _filterOwner.Scheduler.Fields[fieldIndex];
				}
			}
			string text = (string)LayoutReader.GetPropertyValue(info, "Value1", typeof(string), null);
			string valueTypeName = "";
			if (text != null)
			{
				valueTypeName = (string)LayoutReader.GetPropertyValue(info, "TypeNameValue1", typeof(string), null);
				if (valueTypeName == null)
				{
					_firstValue = text;
				}
				else
				{
					try
					{
						_firstValue = PropertyValue.ConvertValue(text, LayoutTypeResolver.Resolve(valueTypeName));
					}
					catch
					{
						_firstValue = text;
					}
				}
			}
			text = (string)LayoutReader.GetPropertyValue(info, "Value2", typeof(string), null);
			if (text != null)
			{
				valueTypeName = (string)LayoutReader.GetPropertyValue(info, "TypeNameValue2", typeof(string), null);
				if (valueTypeName == null)
				{
					_secondValue = text;
				}
				else
				{
					try
					{
						_secondValue = PropertyValue.ConvertValue(text, LayoutTypeResolver.Resolve(valueTypeName));
					}
					catch
					{
						_secondValue = text;
					}
				}
			}
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			info.DeserializeObject("Conditions", _conditions);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_negation)
			{
				info.AddValue("Negation", _negation);
			}
			if (_field != null)
			{
				info.AddValue("FieldIndex", _field.Index);
			}
			if (_conditionOperator != ConditionOperator.Equal)
			{
				info.AddValue("ConditionOperator", _conditionOperator);
			}
			if (_logicalOperator != LogicalOperator.None)
			{
				info.AddValue("LogicalOperator", _logicalOperator);
			}
			if (_firstValue != null)
			{
				if (!(_firstValue is string))
				{
					info.AddValue("TypeNameValue1", _firstValue.GetType().FullName);
				}
				info.AddValue("Value1", _firstValue);
			}
			if (_secondValue != null)
			{
				if (!(_secondValue is string))
				{
					info.AddValue("TypeNameValue2", _secondValue.GetType().FullName);
				}
				info.AddValue("Value2", _secondValue);
			}
			if (_firstValueField != null)
			{
				info.AddValue("ValueFieldIndex1", _firstValueField._index);
			}
			if (_secondValueField != null)
			{
				info.AddValue("ValueFieldIndex2", _secondValueField._index);
			}
			if (_conditions.Count > 0)
			{
				info.AddValue("Conditions", _conditions);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
		}

		public SchedulerFilterCondition()
			: this(null, ConditionOperator.Equal, null, null)
		{
		}

		public SchedulerFilterCondition(SchedulerField field, ConditionOperator conditionOperator, object value1)
			: this(field, conditionOperator, value1, null)
		{
		}

		public SchedulerFilterCondition(SchedulerField field, ConditionOperator conditionOperator, object value1, object value2)
		{
			_field = field;
			_conditionOperator = conditionOperator;
			if (value1 is SchedulerField)
			{
				_firstValueField = (SchedulerField)value1;
			}
			else
			{
				_firstValue = value1;
			}
			if (value2 is SchedulerField)
			{
				_secondValueField = (SchedulerField)value2;
			}
			else
			{
				_secondValue = value2;
			}
			_conditions = new SchedulerFilterConditionCollection(this);
		}

		internal bool UsesField(SchedulerField schedulerField)
		{
			if (_field == schedulerField)
			{
				return true;
			}
			foreach (SchedulerFilterCondition item in (IEnumerable)Conditions)
			{
				if (item.UsesField(schedulerField))
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsAppointmentAccepted(SchedulerAppointment schedulerAppointment)
		{
			return MatchesAppointment(schedulerAppointment);
		}

		public override string ToString()
		{
			return BuildText();
		}

		private string BuildText()
		{
			if (IsComposite)
			{
				string text = "(";
				if (!IsEmptyCondition())
				{
					text += BuildSingleConditionText();
				}
				foreach (SchedulerFilterCondition item in (IEnumerable)_conditions)
				{
					if (_conditions.IndexOf(item) != 0 || !IsEmptyCondition())
					{
						if (item._logicalOperator == LogicalOperator.And)
						{
							text += " And ";
						}
						else if (item._logicalOperator == LogicalOperator.Xor)
						{
							text += " Xor ";
						}
						else if (item._logicalOperator == LogicalOperator.Or)
						{
							text += " Or ";
						}
					}
					text += item.BuildText();
				}
				return text + ")";
			}
			return BuildSingleConditionText();
		}

		private string BuildSingleConditionText()
		{
			if (IsEmptyCondition())
			{
				if (_negation)
				{
					return "(False)";
				}
				return "(Empty)";
			}
			string text = ((_field == null) ? "(null)" : ((!(_field.Key == "")) ? _field.Key : _field._index.ToString()));
			string conditionText = "([" + text + "] " + GetOperatorName(_conditionOperator) + " ";
			conditionText = ((_firstValueField != null) ? string.Concat(str2: (!(_firstValueField.Key == "")) ? _firstValueField.Key : _firstValueField._index.ToString(), str0: conditionText, str1: "[", str3: "]") : (conditionText + PropertyValue.ConvertObjectToString(_firstValue)));
			if (_conditionOperator == ConditionOperator.Between || _conditionOperator == ConditionOperator.NotBetween)
			{
				conditionText = ((_secondValueField == null) ? (conditionText + " And " + PropertyValue.ConvertObjectToString(_secondValue)) : string.Concat(str2: (!(_secondValueField.Key == "")) ? _secondValueField.Key : _secondValueField._index.ToString(), str0: conditionText, str1: " And [", str3: "]"));
			}
			conditionText += ")";
			if (_negation)
			{
				conditionText = "(Not " + conditionText + ")";
			}
			return conditionText;
		}

		internal string BuildTextUpTo(SchedulerFilterCondition schedulerFilterCondition, ref bool isStopReached)
		{
			if (isStopReached || this == schedulerFilterCondition)
			{
				isStopReached = true;
				return "";
			}
			isStopReached = false;
			if (!IsComposite)
			{
				return BuildSingleConditionText();
			}
			string text = "(";
			if (!IsEmptyCondition())
			{
				text += BuildSingleConditionText();
			}
			foreach (SchedulerFilterCondition item in (IEnumerable)_conditions)
			{
				if (_conditions.IndexOf(item) != 0 || !IsEmptyCondition())
				{
					if (item._logicalOperator == LogicalOperator.And)
					{
						text += " And ";
					}
					else if (item._logicalOperator == LogicalOperator.Xor)
					{
						text += " Xor ";
					}
					else if (item._logicalOperator == LogicalOperator.Or)
					{
						text += " Or ";
					}
				}
				text += item.BuildTextUpTo(schedulerFilterCondition, ref isStopReached);
				if (isStopReached)
				{
					break;
				}
			}
			if (!isStopReached)
			{
				text += ")";
			}
			return text;
		}

		internal bool IsEmptyCondition()
		{
			if (_field != null)
			{
				return false;
			}
			if (_firstValue != null)
			{
				return false;
			}
			if (_firstValueField != null)
			{
				return false;
			}
			if (_conditionOperator != ConditionOperator.Equal)
			{
				return false;
			}
			if (_conditionOperator == ConditionOperator.Between || _conditionOperator == ConditionOperator.NotBetween)
			{
				if (_secondValue != null)
				{
					return false;
				}
				if (_secondValueField != null)
				{
					return false;
				}
			}
			return true;
		}

		private string GetOperatorName(ConditionOperator conditionOperator)
		{
			switch (_conditionOperator)
			{
			case ConditionOperator.BeginsWith:
				return "BeginsWith";
			case ConditionOperator.Between:
				return "Between";
			case ConditionOperator.Contains:
				return "Contains";
			case ConditionOperator.EndsWith:
				return "EndsWith";
			case ConditionOperator.Equal:
				return "=";
			case ConditionOperator.GreaterThan:
				return ">";
			case ConditionOperator.GreaterThanOrEqualTo:
				return ">=";
			case ConditionOperator.LessThan:
				return "<";
			case ConditionOperator.LessThanOrEqualTo:
				return "<=";
			case ConditionOperator.NotBetween:
				return "NotBetween";
			case ConditionOperator.NotContains:
				return "Doesn´t contain";
			case ConditionOperator.NotEqual:
				return "<>";
			case ConditionOperator.IsNull:
				return "Is Null";
			case ConditionOperator.NotIsNull:
				return "Isn't Null";
			case ConditionOperator.IsEmpty:
				return "Is empty";
			case ConditionOperator.NotIsEmpty:
				return "Isn't empty";
			case ConditionOperator.In:
				return "Is In";
			case ConditionOperator.NotIn:
				return "Isn't In";
			default:
				return "";
			}
		}

		private bool MatchesOwnCondition(SchedulerAppointment schedulerAppointment)
		{
			object dataRow = schedulerAppointment.DataRow;
			if (Scheduler == null || _field == null)
			{
				return true;
			}
			object fieldValue;
			if (schedulerAppointment.HasFieldValues())
			{
				fieldValue = schedulerAppointment.GetValue(_field);
			}
			else
			{
				if (dataRow == null)
				{
					return false;
				}
				fieldValue = Scheduler.GetFieldValue(schedulerAppointment, _field);
			}
			object firstCompareValue = _firstValue;
			object secondCompareValue = _secondValue;
			if (_firstValueField != null)
			{
				firstCompareValue = _scheduler.GetFieldValue(schedulerAppointment, _firstValueField);
			}
			if (_secondValueField != null)
			{
				secondCompareValue = _scheduler.GetFieldValue(schedulerAppointment, _secondValueField);
			}
			switch (_conditionOperator)
			{
			case ConditionOperator.IsNull:
				if (fieldValue != null)
				{
					return Convert.IsDBNull(fieldValue);
				}
				return true;
			case ConditionOperator.NotIsNull:
				if (fieldValue != null)
				{
					return !Convert.IsDBNull(fieldValue);
				}
				return false;
			case ConditionOperator.IsEmpty:
				if (fieldValue == null || Convert.IsDBNull(fieldValue))
				{
					return true;
				}
				if (fieldValue is string && (string)fieldValue == "")
				{
					return true;
				}
				return false;
			case ConditionOperator.NotIsEmpty:
				if (fieldValue == null || Convert.IsDBNull(fieldValue))
				{
					return false;
				}
				if (fieldValue is string && (string)fieldValue == "")
				{
					return false;
				}
				return true;
			case ConditionOperator.In:
				if (SchedulerRenderer.IsEqualToAny(fieldValue, firstCompareValue, null, _field.CompareIgnoreCase))
				{
					return true;
				}
				return false;
			case ConditionOperator.NotIn:
				if (SchedulerRenderer.IsEqualToAny(fieldValue, firstCompareValue, null, _field.CompareIgnoreCase))
				{
					return false;
				}
				return true;
			case ConditionOperator.Equal:
				return SchedulerRenderer.CompareValues(fieldValue, firstCompareValue, null, _field.CompareIgnoreCase) == 0;
			case ConditionOperator.NotEqual:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) != 0;
			case ConditionOperator.Between:
				if (SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) >= 0)
				{
					return SchedulerRenderer.CompareIgnoringCase(fieldValue, secondCompareValue, null) <= 0;
				}
				return false;
			case ConditionOperator.Contains:
				return SchedulerRenderer.ContainsIgnoreCase(fieldValue, firstCompareValue);
			case ConditionOperator.EndsWith:
				return SchedulerRenderer.EndsWithIgnoreCase(fieldValue, firstCompareValue);
			case ConditionOperator.BeginsWith:
				return SchedulerRenderer.StartsWithIgnoreCase(fieldValue, firstCompareValue);
			case ConditionOperator.GreaterThan:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) > 0;
			case ConditionOperator.GreaterThanOrEqualTo:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) >= 0;
			case ConditionOperator.LessThan:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) < 0;
			case ConditionOperator.LessThanOrEqualTo:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) <= 0;
			case ConditionOperator.NotBetween:
				if (SchedulerRenderer.CompareIgnoringCase(fieldValue, firstCompareValue, null) >= 0)
				{
					return SchedulerRenderer.CompareIgnoringCase(fieldValue, secondCompareValue, null) > 0;
				}
				return true;
			case ConditionOperator.NotContains:
				return !SchedulerRenderer.ContainsIgnoreCase(fieldValue, firstCompareValue);
			default:
				return false;
			}
		}

		private bool IsFieldMissing()
		{
			if (Scheduler == null)
			{
				return true;
			}
			if (_field == null)
			{
				return true;
			}
			return false;
		}

		internal bool MatchesAppointment(SchedulerAppointment schedulerAppointment)
		{
			bool isMatch = MatchesOwnCondition(schedulerAppointment);
			if (_conditions.Count == 0)
			{
				if (_negation)
				{
					return !isMatch;
				}
				return isMatch;
			}
			bool isOwnConditionSkipped = IsFieldMissing();
			foreach (SchedulerFilterCondition item in (IEnumerable)_conditions)
			{
				switch (item.LogicalOperator)
				{
				case LogicalOperator.Or:
					isMatch = ((!isOwnConditionSkipped) ? (isMatch || item.IsAppointmentAccepted(schedulerAppointment)) : item.IsAppointmentAccepted(schedulerAppointment));
					break;
				case LogicalOperator.Xor:
					isMatch = ((!isOwnConditionSkipped) ? (isMatch ^ item.IsAppointmentAccepted(schedulerAppointment)) : item.IsAppointmentAccepted(schedulerAppointment));
					break;
				default:
					isMatch = ((!isOwnConditionSkipped) ? (isMatch && item.IsAppointmentAccepted(schedulerAppointment)) : item.IsAppointmentAccepted(schedulerAppointment));
					break;
				}
				isOwnConditionSkipped = false;
			}
			if (_negation)
			{
				return !isMatch;
			}
			return isMatch;
		}

		internal void UpdateSchedulerFilterCondition()
		{
			if (_field != null && _field.FindPropertyDescriptor() != null)
			{
				if (_firstValue != null)
				{
					_firstValue = PropertyValue.ConvertValue((string)_firstValue, _field.FindPropertyDescriptor().PropertyType);
				}
				if (_secondValue != null)
				{
					_secondValue = PropertyValue.ConvertValue((string)_secondValue, _field.FindPropertyDescriptor().PropertyType);
				}
			}
			if (_conditions == null)
			{
				return;
			}
			foreach (SchedulerFilterCondition item in (IEnumerable)_conditions)
			{
				item.UpdateSchedulerFilterCondition();
			}
		}

		public object Clone()
		{
			SchedulerFilterCondition schedulerFilterCondition = (SchedulerFilterCondition)MemberwiseClone();
			schedulerFilterCondition._filterOwner = null;
			schedulerFilterCondition._owner = null;
			if (_conditions != null)
			{
				schedulerFilterCondition._conditions = (SchedulerFilterConditionCollection)((ICloneable)_conditions).Clone();
				schedulerFilterCondition._conditions.SetSchedulerFilterCondition(schedulerFilterCondition);
			}
			return schedulerFilterCondition;
		}

		public SchedulerFilterCondition AddCondition()
		{
			SchedulerFilterCondition schedulerFilterCondition = new SchedulerFilterCondition();
			AddCondition(schedulerFilterCondition);
			return schedulerFilterCondition;
		}

		public void AddCondition(SchedulerFilterCondition condition)
		{
			AddCondition(LogicalOperator.None, condition);
		}

		public void AddCondition(LogicalOperator logicalOperator, SchedulerFilterCondition condition)
		{
			_conditions.AddWithOperator(logicalOperator, condition);
		}

		public void RemoveCondition(SchedulerFilterCondition condition)
		{
			_conditions.Remove(condition);
		}

		public void RemoveConditionAt(int index)
		{
			RemoveCondition(_conditions[index]);
		}

		public void Clear()
		{
			_field = null;
			_conditionOperator = ConditionOperator.Equal;
			_firstValue = null;
			_secondValue = null;
			_firstValueField = null;
			_secondValueField = null;
			_negation = false;
			_conditions.ClearConditions();
		}

		internal bool IsValidForScheduler(MxScheduler scheduler)
		{
			if (scheduler == null)
			{
				return true;
			}
			if (!IsEmptyCondition())
			{
				if (_field != null && scheduler != _field.Scheduler)
				{
					return false;
				}
				if (_firstValueField != null && _firstValueField.Scheduler != scheduler)
				{
					return false;
				}
				if ((_conditionOperator == ConditionOperator.BeginsWith || _conditionOperator == ConditionOperator.NotBetween) && _secondValueField != null && _secondValueField.Scheduler != scheduler)
				{
					return false;
				}
			}
			if (_conditions != null)
			{
				foreach (SchedulerFilterCondition item in (IEnumerable)_conditions)
				{
					if (!item.IsValidForScheduler(scheduler))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
