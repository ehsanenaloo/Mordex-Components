using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public class TimelineGroupHeaderTotal : MordexGroupTotalBase, IMordexXmlLayoutsSupport
	{
		private string _totalSuffix = ")";

		private string _totalPrefix = "(";

		private FormatMode _totalFormatMode = FormatMode.UseIFormattable;

		private string _totalFormatString = "";

		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxTimeline Timeline
		{
			get
			{
				if (GetParentCollection() != null)
				{
					return GetParentCollection().Timeline;
				}
				return null;
			}
		}

		[DefaultValue(null)]
		[Description("The TimelineField object to summarize.")]
		[Editor(typeof(FieldEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(FieldConverter))]
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

		[DefaultValue(AggregateFunction.None)]
		[Description("Indicates the aggregate function whose result will be shown in the total row.")]
		public AggregateFunction AggregateFunction
		{
			get
			{
				return DataState.ToTimelineAggregateFunction(base.BaseAggregateFunction);
			}
			set
			{
				base.BaseAggregateFunction = DataState.ToDataAggregateFunction(value);
			}
		}

		[DefaultValue(")")]
		[Description("Indicates the string displayed after the total value.")]
		public string TotalSuffix
		{
			get
			{
				return _totalSuffix;
			}
			set
			{
				if (_totalSuffix != value)
				{
					_totalSuffix = value;
				}
			}
		}

		[Description("Indicates the string displayed before the total value")]
		[DefaultValue("(")]
		public string TotalPrefix
		{
			get
			{
				return _totalPrefix;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentException("TotalPrefix can not be null.");
				}
				if (_totalPrefix != value)
				{
					_totalPrefix = value;
				}
			}
		}

		[Description("Determines how values will be formatted.")]
		[DefaultValue(FormatMode.UseIFormattable)]
		public FormatMode TotalFormatMode
		{
			get
			{
				return _totalFormatMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FormatMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _totalFormatMode)
				{
					_totalFormatMode = value;
				}
			}
		}

		[DefaultValue("")]
		[Description("Indicates the format to use when rendering total values.")]
		public string TotalFormatString
		{
			get
			{
				return _totalFormatString;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentException("TotalFormatString can not be null.");
				}
				if (value != _totalFormatString)
				{
					_totalFormatString = value;
				}
			}
		}

		[Editor(typeof(FilterConditionEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the TimelineFilterCondition that rows need to meet in order to include them in the summary operation.")]
		[TypeConverter(typeof(FilterConditionConverter))]
		public TimelineFilterCondition FilterCondition
		{
			get
			{
				return (TimelineFilterCondition)base.BaseCondition;
			}
			set
			{
				base.BaseCondition = value;
			}
		}

		[DefaultValue("")]
		[Description("The key used to identify an object in a colletion.")]
		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				base.BaseKey = value;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (Timeline != null)
			{
				int fieldIndex = (int)LayoutReader.GetPropertyValue(info, "ColIndex", typeof(int), -1);
				if (fieldIndex >= 0 && fieldIndex < Timeline.Fields.Count)
				{
					Field = Timeline.Fields[fieldIndex];
				}
			}
			AggregateFunction = (AggregateFunction)LayoutReader.GetPropertyValue(info, "AggregateFunction", typeof(AggregateFunction), AggregateFunction.None);
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				TimelineFilterCondition timelineFilterCondition = new TimelineFilterCondition();
				timelineFilterCondition.SetConditionOwner(this);
				propertyValue.DeserializeObject(timelineFilterCondition);
				FilterCondition = timelineFilterCondition;
			}
			_totalFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "TotalFormatMode", typeof(FormatMode), FormatMode.UseIFormattable);
			_totalFormatString = (string)LayoutReader.GetPropertyValue(info, "TotalFormatString", typeof(string), "");
			_totalPrefix = (string)LayoutReader.GetPropertyValue(info, "TotalPrefix", typeof(string), "(");
			_totalSuffix = (string)LayoutReader.GetPropertyValue(info, "TotalSuffix", typeof(string), ")");
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Field != null && Field.Index != -1)
			{
				info.AddValue("ColIndex", Field.Index);
			}
			if (AggregateFunction != AggregateFunction.None)
			{
				info.AddValue("AggregateFunction", AggregateFunction);
			}
			if (FilterCondition != null)
			{
				info.AddValue("FilterCondition", FilterCondition);
			}
			if (_totalFormatMode != FormatMode.UseIFormattable)
			{
				info.AddValue("TotalFormatMode", _totalFormatMode);
			}
			if (_totalFormatString != "")
			{
				info.AddValue("TotalFormatString", _totalFormatString);
			}
			if (_totalPrefix != "(")
			{
				info.AddValue("TotalPrefix", _totalPrefix);
			}
			if (_totalSuffix != ")")
			{
				info.AddValue("TotalSuffix", _totalSuffix);
			}
			if (Key.Length > 0)
			{
				info.AddValue("Key", Key);
			}
		}

		internal TimelineGroupHeaderTotalCollection GetParentCollection()
		{
			return (TimelineGroupHeaderTotalCollection)base.BaseParentCollection;
		}

		internal void SetParentCollection(TimelineGroupHeaderTotalCollection parent)
		{
			BaseSetParentCollection(parent);
		}

		internal bool IsValid(MxTimeline timeline)
		{
			if (AggregateFunction == AggregateFunction.None)
			{
				return false;
			}
			if (Field == null)
			{
				if (AggregateFunction != AggregateFunction.Count)
				{
					return false;
				}
				if (FilterCondition != null && !FilterCondition.IsValid(timeline))
				{
					return false;
				}
			}
			else if (FilterCondition != null && !FilterCondition.IsValid(Field.Timeline))
			{
				return false;
			}
			return true;
		}
	}
}
