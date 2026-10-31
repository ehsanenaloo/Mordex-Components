using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public class TimelineFilterCondition : MordexConditionBase, ICloneable, IMordexXmlLayoutsSupport, IFilterCondition
	{

		private string _key = "";

		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

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

		[Editor(typeof(FieldEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[TypeConverter(typeof(FieldConverter))]
		[Description("The TimelineField to compare the value(s).")]
		public TimelineField Field
		{
			get
			{
				return (TimelineField)base.BaseField;
			}
			set
			{
				base.BaseField = value;
			}
		}

		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return base.BaseEnabled;
			}
			set
			{
				base.BaseEnabled = value;
			}
		}

		[DefaultValue(null)]
		[Description("The TimelineField object that has the value to compare.")]
		public TimelineField ValueField1
		{
			get
			{
				return (TimelineField)base.BaseValueField1;
			}
			set
			{
				base.BaseValueField1 = value;
			}
		}

		[DefaultValue(false)]
		[Description("Determines whether to apply a negation logical operator to the result of the filter condition.")]
		public bool Negation
		{
			get
			{
				return base.BaseNegation;
			}
			set
			{
				base.BaseNegation = value;
			}
		}

		[DefaultValue(null)]
		[Description("The TimelineField object that has the value to compare when the condition operator requires two values.")]
		public TimelineField ValueField2
		{
			get
			{
				return (TimelineField)base.BaseValueField2;
			}
			set
			{
				base.BaseValueField2 = value;
			}
		}

		[Description("Indicates the operator used for comparison.")]
		[DefaultValue(ConditionOperator.Equal)]
		public ConditionOperator ConditionOperator
		{
			get
			{
				return DataState.ToTimelineConditionOperator(base.BaseConditionOperator);
			}
			set
			{
				base.BaseConditionOperator = DataState.ToDataConditionOperator(value);
			}
		}

		[Description("The logical operator used when the filter condition is composed by two or more conditions.")]
		[DefaultValue(LogicalOperator.None)]
		public LogicalOperator LogicalOperator
		{
			get
			{
				return DataState.ToTimelineLogicalOperator(base.BaseLogicalOperator);
			}
			set
			{
				base.BaseLogicalOperator = DataState.ToDataLogicalOperator(value);
			}
		}

		[DefaultValue(null)]
		[Description("The object value to compare")]
		[TypeConverter(typeof(StringConverter))]
		public object Value1
		{
			get
			{
				return base.BaseValue1;
			}
			set
			{
				base.BaseValue1 = value;
			}
		}

		[Description("The object value to compare when the condition operator requires two values.")]
		[DefaultValue(null)]
		[TypeConverter(typeof(StringConverter))]
		public object Value2
		{
			get
			{
				return base.BaseValue2;
			}
			set
			{
				base.BaseValue2 = value;
			}
		}

		public TimelineFilterConditionCollection Conditions => (TimelineFilterConditionCollection)base.BaseConditions;

		TimelineFilterCondition IFilterCondition.FilterCondition => this;

		public bool IsComposite => base.BaseIsComposite;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			base.DisableValidation = true;
			Negation = (bool)LayoutReader.GetPropertyValue(info, "Negation", typeof(bool), false);
			ConditionOperator = (ConditionOperator)LayoutReader.GetPropertyValue(info, "ConditionOperator", typeof(ConditionOperator), ConditionOperator.Equal);
			LogicalOperator = (LogicalOperator)LayoutReader.GetPropertyValue(info, "LogicalOperator", typeof(LogicalOperator), LogicalOperator.None);
			Enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			int fieldIndex = -1;
			if (GetOwnerTimeline() != null)
			{
				fieldIndex = (int)LayoutReader.GetPropertyValue(info, "FieldIndex", typeof(int), -1);
				if (fieldIndex != -1)
				{
					Field = GetOwnerTimeline().Fields[fieldIndex];
				}
				fieldIndex = (int)LayoutReader.GetPropertyValue(info, "ValueFieldIndex1", typeof(int), -1);
				if (fieldIndex != -1)
				{
					ValueField1 = GetOwnerTimeline().Fields[fieldIndex];
				}
				fieldIndex = (int)LayoutReader.GetPropertyValue(info, "ValueFieldIndex2", typeof(int), -1);
				if (fieldIndex != -1)
				{
					ValueField1 = GetOwnerTimeline().Fields[fieldIndex];
				}
			}
			string text = (string)LayoutReader.GetPropertyValue(info, "Value1", typeof(string), null);
			string valueTypeName = "";
			if (text != null)
			{
				valueTypeName = (string)LayoutReader.GetPropertyValue(info, "TypeNameValue1", typeof(string), null);
				if (valueTypeName == null)
				{
					Value1 = text;
				}
				else
				{
					try
					{
						Value1 = PropertyValue.ConvertValue(text, LayoutTypeResolver.Resolve(valueTypeName));
					}
					catch
					{
						Value1 = text;
					}
				}
			}
			text = (string)LayoutReader.GetPropertyValue(info, "Value2", typeof(string), null);
			if (text != null)
			{
				valueTypeName = (string)LayoutReader.GetPropertyValue(info, "TypeNameValue2", typeof(string), null);
				if (valueTypeName == null)
				{
					Value2 = text;
				}
				else
				{
					try
					{
						Value2 = PropertyValue.ConvertValue(text, LayoutTypeResolver.Resolve(valueTypeName));
					}
					catch
					{
						Value2 = text;
					}
				}
			}
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			info.DeserializeObject("Conditions", Conditions);
			base.DisableValidation = false;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Negation)
			{
				info.AddValue("Negation", Negation);
			}
			if (Field != null)
			{
				info.AddValue("FieldIndex", Field.Index);
			}
			if (ConditionOperator != ConditionOperator.Equal)
			{
				info.AddValue("ConditionOperator", ConditionOperator);
			}
			if (LogicalOperator != LogicalOperator.None)
			{
				info.AddValue("LogicalOperator", LogicalOperator);
			}
			if (Value1 != null)
			{
				if (!(Value1 is string))
				{
					info.AddValue("TypeNameValue1", Value1.GetType().FullName);
				}
				info.AddValue("Value1", Value1);
			}
			if (Value2 != null)
			{
				if (!(Value2 is string))
				{
					info.AddValue("TypeNameValue2", Value2.GetType().FullName);
				}
				info.AddValue("Value2", Value2);
			}
			if (!Enabled)
			{
				info.AddValue("Enabled", Enabled);
			}
			if (ValueField1 != null)
			{
				info.AddValue("ValueFieldIndex1", ValueField1.Index);
			}
			if (ValueField2 != null)
			{
				info.AddValue("ValueFieldIndex2", ValueField2.Index);
			}
			if (Conditions.Count > 0)
			{
				info.AddValue("Conditions", Conditions);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
		}

		internal bool ReferencesField(TimelineField timelineField)
		{
			if (Field == timelineField)
			{
				return true;
			}
			foreach (TimelineFilterCondition condition in Conditions)
			{
				if (condition.ReferencesField(timelineField))
				{
					return true;
				}
			}
			return false;
		}

		public TimelineFilterCondition()
			: this(null, ConditionOperator.Equal, null, null)
		{
		}

		public TimelineFilterCondition(TimelineField field, ConditionOperator conditionOperator, object value1)
			: this(field, conditionOperator, value1, null)
		{
		}

		public TimelineFilterCondition(TimelineField field, ConditionOperator conditionOperator, object value1, object value2)
			: base(field, DataState.ToDataConditionOperator(conditionOperator), value1, value2)
		{
		}

		protected override MordexConditionCollectionBase CreateConditions()
		{
			return new TimelineFilterConditionCollection(this);
		}

		protected override MordexConditionBase CreateCondition()
		{
			return new TimelineFilterCondition();
		}

		internal MxTimeline GetOwnerTimeline()
		{
			if (base.BaseTable is TimelineMordexTableBase timelineMordexTableBase)
			{
				return timelineMordexTableBase.GetTimeline();
			}
			return null;
		}

		internal bool IsValid(MxTimeline timeline)
		{
			try
			{
				BaseValidate((MordexTableBase)timeline.GetTimelineDataSourceManager().RootTable);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public object Clone()
		{
			return BaseClone();
		}

		private string GetConditionText()
		{
			if (IsComposite)
			{
				string text = "(";
				if (!IsEmpty())
				{
					text += GetSingleConditionText();
				}
				foreach (TimelineFilterCondition condition in Conditions)
				{
					if (Conditions.IndexOf(condition) != 0 || !IsEmpty())
					{
						if (condition.LogicalOperator == LogicalOperator.And)
						{
							text += " And ";
						}
						else if (condition.LogicalOperator == LogicalOperator.Xor)
						{
							text += " Xor ";
						}
						else if (condition.LogicalOperator == LogicalOperator.Or)
						{
							text += " Or ";
						}
					}
					text += condition.GetConditionText();
				}
				return text + ")";
			}
			return GetSingleConditionText();
		}

		public bool EvaluateItem(TimelineItem item)
		{
			MordexRow row = item._row;
			if (row != null)
			{
				return BaseEvaluateRow(row);
			}
			return false;
		}

		private string GetSingleConditionText()
		{
			if (IsEmpty())
			{
				if (Negation)
				{
					return "(False)";
				}
				return "(Empty)";
			}
			string text = ((Field == null) ? "(null)" : ((!(Field.Key == "")) ? Field.Key : Field.Index.ToString()));
			string conditionText = "([" + text + "] " + GetOperatorText(ConditionOperator) + " ";
			conditionText = ((ValueField1 != null) ? string.Concat(str2: (!(ValueField1.Key == "")) ? ValueField1.Key : ValueField1.Index.ToString(), str0: conditionText, str1: "[", str3: "]") : (conditionText + PropertyValue.ConvertObjectToString(Value1)));
			if (ConditionOperator == ConditionOperator.Between || ConditionOperator == ConditionOperator.NotBetween)
			{
				conditionText = ((ValueField2 == null) ? (conditionText + " And " + PropertyValue.ConvertObjectToString(Value2)) : string.Concat(str2: (!(ValueField2.Key == "")) ? ValueField2.Key : ValueField2.Index.ToString(), str0: conditionText, str1: " And [", str3: "]"));
			}
			conditionText += ")";
			if (Negation)
			{
				conditionText = "(Not " + conditionText + ")";
			}
			return conditionText;
		}

		internal string GetTextBeforeCondition(TimelineFilterCondition timelineFilterCondition, ref bool hasReachedCondition)
		{
			if (hasReachedCondition || this == timelineFilterCondition)
			{
				hasReachedCondition = true;
				return "";
			}
			hasReachedCondition = false;
			if (!IsComposite)
			{
				return GetSingleConditionText();
			}
			string text = "(";
			if (!IsEmpty())
			{
				text += GetSingleConditionText();
			}
			foreach (TimelineFilterCondition condition in Conditions)
			{
				if (Conditions.IndexOf(condition) != 0 || !IsEmpty())
				{
					if (condition.LogicalOperator == LogicalOperator.And)
					{
						text += " And ";
					}
					else if (condition.LogicalOperator == LogicalOperator.Xor)
					{
						text += " Xor ";
					}
					else if (condition.LogicalOperator == LogicalOperator.Or)
					{
						text += " Or ";
					}
				}
				text += condition.GetTextBeforeCondition(timelineFilterCondition, ref hasReachedCondition);
				if (hasReachedCondition)
				{
					break;
				}
			}
			if (!hasReachedCondition)
			{
				text += ")";
			}
			return text;
		}

		private string GetOperatorText(ConditionOperator conditionOperator)
		{
			switch (ConditionOperator)
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
			default:
				return "";
			}
		}

		public override string ToString()
		{
			return GetConditionText();
		}

		public TimelineFilterCondition AddCondition()
		{
			TimelineFilterCondition timelineFilterCondition = new TimelineFilterCondition();
			AddCondition(timelineFilterCondition);
			return timelineFilterCondition;
		}

		public void AddCondition(TimelineFilterCondition condition)
		{
			AddCondition(LogicalOperator.None, condition);
		}

		public void AddCondition(LogicalOperator logicalOperator, TimelineFilterCondition condition)
		{
			BaseAddCondition(DataState.ToDataLogicalOperator(logicalOperator), condition);
		}

		public void RemoveCondition(TimelineFilterCondition condition)
		{
			BaseRemoveCondition(condition);
		}

		public void RemoveConditionAt(int index)
		{
			BaseRemoveConditionAt(index);
		}

		public void Clear()
		{
			BaseClear();
		}

		internal bool IsEmpty()
		{
			if (Field != null)
			{
				return false;
			}
			if (Value1 != null)
			{
				return false;
			}
			if (ValueField1 != null)
			{
				return false;
			}
			if (ConditionOperator != ConditionOperator.Equal)
			{
				return false;
			}
			if (ConditionOperator == ConditionOperator.Between || ConditionOperator == ConditionOperator.NotBetween)
			{
				if (Value2 != null)
				{
					return false;
				}
				if (ValueField2 != null)
				{
					return false;
				}
			}
			return true;
		}

		internal void UpdateTimelineFilterCondition()
		{
			if (Field != null && Field.Type != null && Field.CompareTarget != FieldCompareTarget.Text)
			{
				if (Value1 != null)
				{
					Value1 = PropertyValue.ConvertValue((string)Value1, Field.Type);
				}
				if (Value2 != null)
				{
					Value2 = PropertyValue.ConvertValue((string)Value2, Field.Type);
				}
			}
			if (Conditions == null)
			{
				return;
			}
			foreach (TimelineFilterCondition condition in Conditions)
			{
				condition.UpdateTimelineFilterCondition();
			}
		}

		internal TimelineFilterCondition GetOwnerCondition()
		{
			if (base.BaseParentCollection != null)
			{
				return ((TimelineFilterConditionCollection)base.BaseParentCollection).GetOwner();
			}
			return null;
		}

		internal IConditionOwner GetBaseOwner()
		{
			return base.BaseOwner;
		}

		internal void SetConditionOwner(IConditionOwner iConditionOwner)
		{
			base.BaseOwner = iConditionOwner;
		}
	}
}
