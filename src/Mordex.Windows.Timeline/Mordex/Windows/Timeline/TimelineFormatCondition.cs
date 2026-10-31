using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public class TimelineFormatCondition : MordexConditionBase, ITimelineOwner, ICloneable, IMordexXmlLayoutsSupport, IConditionOwner
	{
		private bool _allowMerge = true;

		private TimelineFilterCondition _filterCondition;

		private TimelineFormatConditionCollection _owningCollection;

		private int _imageIndex = -1;

		private string _imageKey = "";

		internal int _index = -1;

		private bool _enabled = true;

		private TimelineFormatStyle _formatStyle;

		private TimelineFormatStyle _itemsBarFormatStyle;

		private TimelineFormatStyle _itemsEstimatedBarFormatStyle;

		private string _key = "";

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Determines whether the format style associated to this TimelineFormatCondition should be merged with the format style of another TimelineFormatCondition")]
		[DefaultValue(true)]
		public bool AllowMerge
		{
			get
			{
				return _allowMerge;
			}
			set
			{
				if (_allowMerge != value)
				{
					_allowMerge = value;
					InvalidateControl();
				}
			}
		}

		[Description("The TimelineFilterCondition that rows need to meet in order to apply the FormatStyle associated to this instance in them.")]
		[DefaultValue(null)]
		[TypeConverter(typeof(FilterConditionConverter))]
		[Editor(typeof(FilterConditionEditor), typeof(UITypeEditor))]
		public TimelineFilterCondition FilterCondition
		{
			get
			{
				return _filterCondition;
			}
			set
			{
				if (_filterCondition != value)
				{
					if (_filterCondition != null)
					{
						_filterCondition.SetConditionOwner(null);
					}
					_filterCondition = value;
					if (_filterCondition != null)
					{
						_filterCondition.SetConditionOwner(this);
					}
				}
			}
		}

		[DefaultValue(-1)]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image that will be displayed in the items that match the condition")]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					InvalidateControl();
				}
			}
		}

		[TypeConverter(typeof(ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[Description("Represents the image list index value of the image that will be displayed in the items that match the condition")]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					InvalidateControl();
				}
			}
		}

		[DefaultValue(-1)]
		[Browsable(false)]
		public override int Index => _index;

		[DefaultValue(true)]
		[Description("Determines whether to apply the TimelineFormatCondition in the items of the Timeline.")]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					InvalidateControl();
				}
			}
		}

		[DefaultValue(null)]
		[Editor(typeof(FieldEditor), typeof(UITypeEditor))]
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
				InvalidateControl();
			}
		}

		[Description("Represents the operator used for comparison.")]
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
				InvalidateControl();
			}
		}

		[Description("Represents the object value to compare")]
		[TypeConverter(typeof(StringConverter))]
		[DefaultValue(null)]
		public object Value1
		{
			get
			{
				return base.BaseValue1;
			}
			set
			{
				base.BaseValue1 = value;
				InvalidateControl();
			}
		}

		[DefaultValue(null)]
		[Description("The object value to compare when the condition operator requires two values.")]
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
				InvalidateControl();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DefaultValue(null)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle object that will be applied to the items that match the condition.")]
		public TimelineFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
				}
				return _formatStyle;
			}
			set
			{
				if (_formatStyle != value)
				{
					_formatStyle = value;
					InvalidateControl();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DefaultValue(null)]
		[Description("The TimelineFormatStyle object that will be applied to the duration bar of the items that match the condition.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle ItemsBarFormatStyle
		{
			get
			{
				if (_itemsBarFormatStyle == null)
				{
					_itemsBarFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
				}
				return _itemsBarFormatStyle;
			}
			set
			{
				if (_itemsBarFormatStyle != value)
				{
					_itemsBarFormatStyle = value;
					InvalidateControl();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The TimelineFormatStyle object that will be applied to the estimated duration bar of the items that match the condition.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle ItemsEstimatedBarFormatStyle
		{
			get
			{
				if (_itemsEstimatedBarFormatStyle == null)
				{
					_itemsEstimatedBarFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
				}
				return _itemsEstimatedBarFormatStyle;
			}
			set
			{
				if (_itemsEstimatedBarFormatStyle != value)
				{
					_itemsEstimatedBarFormatStyle = value;
					InvalidateControl();
				}
			}
		}

		[Description("Represents the key used to identify an object in a colletion.")]
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

		[Browsable(false)]
		public MxTimeline Timeline
		{
			get
			{
				if (_owningCollection != null)
				{
					return _owningCollection._timeline;
				}
				return null;
			}
		}

		IMordexTable IConditionOwner.Table
		{
			get
			{
				if (Timeline != null)
				{
					return Timeline.GetTimelineDataSourceManager().RootTable;
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (Timeline != null)
			{
				int fieldIndex = (int)LayoutReader.GetPropertyValue(info, "FieldIndex", typeof(int), -1);
				if (fieldIndex >= 0 && fieldIndex < Timeline.Fields.Count)
				{
					Field = Timeline.Fields[fieldIndex];
				}
			}
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			ConditionOperator = (ConditionOperator)LayoutReader.GetPropertyValue(info, "ConditionOperator", typeof(ConditionOperator), ConditionOperator.Equal);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			Value1 = LayoutReader.GetPropertyValue(info, "Value1", typeof(string), null);
			Value2 = LayoutReader.GetPropertyValue(info, "Value2", typeof(string), null);
			_allowMerge = (bool)LayoutReader.GetPropertyValue(info, "AllowMerge", typeof(bool), true);
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				_filterCondition = new TimelineFilterCondition();
				_filterCondition.SetConditionOwner(this);
				propertyValue.DeserializeObject(_filterCondition);
			}
			else
			{
				_filterCondition = null;
			}
			info.DeserializeObject("FormatStyle", FormatStyle);
			info.DeserializeObject("ItemsBarFormatStyle", ItemsBarFormatStyle);
			info.DeserializeObject("ItemsEstimatedBarFormatStyle", ItemsEstimatedBarFormatStyle);
			UpdateTimelineFormatCondition();
		}

		internal void UpdateTimelineFormatCondition()
		{
			if (Field != null && Field.Type != typeof(string) && Field.CompareTarget == FieldCompareTarget.Value)
			{
				if (Value1 != null && Value1 is string)
				{
					Value1 = PropertyValue.ConvertValue((string)Value1, Field.Type);
				}
				if (Value2 != null && Value2 is string)
				{
					Value2 = PropertyValue.ConvertValue((string)Value2, Field.Type);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Field != null)
			{
				info.AddValue("FieldIndex", Field.Index);
			}
			if (ConditionOperator != ConditionOperator.Equal)
			{
				info.AddValue("ConditionOperator", ConditionOperator);
			}
			if (ShouldSerializeFormatStyle())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (ShouldSerializeItemsBarFormatStyle())
			{
				info.AddValue("ItemsBarFormatStyle", _itemsBarFormatStyle);
			}
			if (ShouldSerializeItemsEstimatedBarFormatStyle())
			{
				info.AddValue("ItemsEstimatedBarFormatStyle", _itemsEstimatedBarFormatStyle);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_key != string.Empty)
			{
				info.AddValue("Key", _key);
			}
			if (!_enabled)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (Value1 != null)
			{
				info.AddValue("Value1", Value1);
			}
			if (Value2 != null)
			{
				info.AddValue("Value2", Value2);
			}
			if (_filterCondition != null)
			{
				info.AddValue("FilterCondition", _filterCondition);
			}
			if (!_allowMerge)
			{
				info.AddValue("AllowMerge", _allowMerge);
			}
		}

		public TimelineFormatCondition()
			: this(null, ConditionOperator.Equal, null, null)
		{
		}

		public TimelineFormatCondition(TimelineField field, ConditionOperator conditionOperator, object value)
			: this(field, conditionOperator, value, null)
		{
		}

		public TimelineFormatCondition(TimelineField field, ConditionOperator conditionOperator, object value1, object value2)
			: base(field, DataState.ToDataConditionOperator(conditionOperator), value1, value2)
		{
			_index = -1;
		}

		internal TimelineFormatConditionCollection GetTimelineFormatConditionCollection()
		{
			return _owningCollection;
		}

		internal void SetFormatConditionCollection(TimelineFormatConditionCollection timelineFormatConditionCollection)
		{
			if (timelineFormatConditionCollection != _owningCollection)
			{
				_owningCollection = timelineFormatConditionCollection;
				if (_owningCollection == null)
				{
					_index = -1;
				}
			}
		}

		public object Clone()
		{
			TimelineFormatCondition timelineFormatCondition = new TimelineFormatCondition(Field, ConditionOperator, Value1, Value2);
			timelineFormatCondition._enabled = _enabled;
			timelineFormatCondition._formatStyle = FormatStyle.Clone();
			timelineFormatCondition._formatStyle._owner = timelineFormatCondition;
			timelineFormatCondition._index = -1;
			timelineFormatCondition._itemsBarFormatStyle = ItemsBarFormatStyle.Clone();
			timelineFormatCondition._itemsBarFormatStyle._owner = timelineFormatCondition;
			timelineFormatCondition._itemsEstimatedBarFormatStyle = ItemsEstimatedBarFormatStyle.Clone();
			timelineFormatCondition._itemsEstimatedBarFormatStyle._owner = timelineFormatCondition;
			timelineFormatCondition._key = _key;
			timelineFormatCondition._imageIndex = _imageIndex;
			timelineFormatCondition._imageKey = _imageKey;
			return timelineFormatCondition;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			if (_formatStyle != null)
			{
				_formatStyle = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle != null)
			{
				return !_formatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsBarFormatStyle()
		{
			if (_itemsBarFormatStyle != null)
			{
				return !_itemsBarFormatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsBarFormatStyle()
		{
			if (_itemsBarFormatStyle != null)
			{
				_itemsBarFormatStyle = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsEstimatedBarFormatStyle()
		{
			if (_itemsEstimatedBarFormatStyle != null)
			{
				return !_itemsEstimatedBarFormatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsEstimatedBarFormatStyle()
		{
			if (_itemsEstimatedBarFormatStyle != null)
			{
				_itemsEstimatedBarFormatStyle = null;
			}
		}

		public bool EvaluateItem(TimelineItem item)
		{
			MordexRow row = item._row;
			if (row != null)
			{
				if (_filterCondition != null)
				{
					return _filterCondition.EvaluateItem(item);
				}
				return BaseEvaluateRow(row);
			}
			return false;
		}

		internal bool IsValid(MxTimeline timeline)
		{
			if (timeline == null)
			{
				return true;
			}
			if (Field == null && _filterCondition == null)
			{
				return false;
			}
			return true;
		}

		internal void InvalidateControl()
		{
			if (_owningCollection != null)
			{
				_owningCollection._timeline.Invalidate();
			}
		}
	}
}
