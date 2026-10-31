using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(SchedulerFormatConditionConverter))]
	public class SchedulerFormatCondition : ISchedulerFormatStyleOwner, ICloneable, ISchedulerFilterOwner, IMordexXmlLayoutsSupport
	{
		internal int _collectionIndex;

		private bool _allowMerge = true;

		private bool _enabled = true;

		private string _firstImageKey = "";

		private string _secondImageKey = "";

		private string _fourthImageKey = "";

		private string _thirdImageKey = "";

		private int _firstImageIndex = -1;

		private int _secondImageIndex = -1;

		private int _thirdImageIndex = -1;

		private int _fourthImageIndex = -1;

		private SchedulerField _field;

		private ConditionOperator _conditionOperator;

		private object _firstValue;

		private object _secondValue;

		private Color _borderColor = Color.Empty;

		private SchedulerFormatStyle _formatStyle;

		private string _key = "";

		private SchedulerFilterCondition _filterCondition;

		internal SchedulerFormatConditionCollection _owner;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		int ISchedulerFormatStyleOwner.DefaultAlpha => -1;

		AlphaMode ISchedulerFormatStyleOwner.BackColorAlphaMode => AlphaMode.Empty;

		float ISchedulerFormatStyleOwner.DefaultBlend => -1f;

		Color ISchedulerFormatStyleOwner.BlendColor => Color.Empty;

		AlphaMode ISchedulerFormatStyleOwner.ForeColorAlphaMode => AlphaMode.Empty;

		MxScheduler ISchedulerFormatStyleOwner.Scheduler => Scheduler;

		MxScheduler ISchedulerFilterOwner.Scheduler => Scheduler;

		[Browsable(false)]
		public int Index => _collectionIndex;

		[DefaultValue(true)]
		[Description("Determines whether the format style associated to this SchedulerFormatCondition should be merged with the format style of another SchedulerFormatCondition")]
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
					InvalidateScheduler();
				}
			}
		}

		[Description("Determines whether to apply the SchedulerFormatCondition in the appointments of the Schedule.")]
		[DefaultValue(true)]
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
					InvalidateScheduler();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(ImageKeyConverter))]
		[Description("The key accessor for the image in the ImageList will be the first image in the appointments that match the condition.")]
		[DefaultValue("")]
		public string ImageKey1
		{
			get
			{
				return _firstImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_firstImageKey != value)
				{
					_firstImageKey = value;
					_firstImageIndex = -1;
					InvalidateScheduler();
				}
			}
		}

		[TypeConverter(typeof(ImageKeyConverter))]
		[Description("The key accessor for the image in the ImageList that will be the second image in the appointments that match the condition.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string ImageKey2
		{
			get
			{
				return _secondImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_secondImageKey != value)
				{
					_secondImageKey = value;
					_secondImageIndex = -1;
					InvalidateScheduler();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList that will be the forth image in the appointments that match the condition.")]
		public string ImageKey4
		{
			get
			{
				return _fourthImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_fourthImageKey != value)
				{
					_fourthImageKey = value;
					_fourthImageIndex = -1;
					InvalidateScheduler();
				}
			}
		}

		[Description("The key accessor for the image in the ImageList that will be the third image in the appointments that match the condition.")]
		[TypeConverter(typeof(ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string ImageKey3
		{
			get
			{
				return _thirdImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_thirdImageKey != value)
				{
					_thirdImageKey = value;
					_thirdImageIndex = -1;
					InvalidateScheduler();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Description("Represents the image list index value of the first image that will be applied to the appointments that match the condition.")]
		[DefaultValue(-1)]
		public int ImageIndex1
		{
			get
			{
				return _firstImageIndex;
			}
			set
			{
				if (_firstImageIndex != value)
				{
					_firstImageIndex = value;
					_firstImageKey = "";
					InvalidateScheduler();
				}
			}
		}

		[TypeConverter(typeof(ImageIndexConverter))]
		[Description("Represents the image list index value of the second image that will be applied to the appointments that match the condition.")]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		public int ImageIndex2
		{
			get
			{
				return _secondImageIndex;
			}
			set
			{
				if (_secondImageIndex != value)
				{
					_secondImageIndex = value;
					_secondImageKey = "";
					InvalidateScheduler();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(ImageIndexConverter))]
		[DefaultValue(-1)]
		[Description("Represents the image list index value of the third image that will be applied to the appointments that match the condition.")]
		public int ImageIndex3
		{
			get
			{
				return _thirdImageIndex;
			}
			set
			{
				if (_thirdImageIndex != value)
				{
					_thirdImageIndex = value;
					_thirdImageKey = "";
					InvalidateScheduler();
				}
			}
		}

		[DefaultValue(-1)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Description("Represents the image list index value of the forth image that will be applied to the appointments that match the condition.")]
		public int ImageIndex4
		{
			get
			{
				return _fourthImageIndex;
			}
			set
			{
				if (_fourthImageIndex != value)
				{
					_fourthImageIndex = value;
					_fourthImageKey = "";
					InvalidateScheduler();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the SchedulerField to compare the value(s).")]
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
				if (_field != value)
				{
					_field = value;
					InvalidateScheduler();
				}
			}
		}

		[Description("Represents the operator used for comparison.")]
		[DefaultValue(ConditionOperator.Equal)]
		public ConditionOperator ConditionOperator
		{
			get
			{
				return _conditionOperator;
			}
			set
			{
				if (value != _conditionOperator)
				{
					_conditionOperator = value;
					InvalidateScheduler();
				}
			}
		}

		[DefaultValue(null)]
		[TypeConverter(typeof(StringConverter))]
		[Description("Represents the object value to compare")]
		public object Value1
		{
			get
			{
				return _firstValue;
			}
			set
			{
				if (_firstValue != value)
				{
					_firstValue = value;
					InvalidateScheduler();
				}
			}
		}

		[TypeConverter(typeof(StringConverter))]
		[Description("The object value to compare when the condition operator requires two values.")]
		[DefaultValue(null)]
		public object Value2
		{
			get
			{
				return _secondValue;
			}
			set
			{
				if (_secondValue != value)
				{
					_secondValue = value;
					InvalidateScheduler();
				}
			}
		}

		[Description("Represents the color of the border for to the appointments that match the condition.")]
		public Color BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				if (_borderColor != value)
				{
					_borderColor = value;
					InvalidateScheduler();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[DefaultValue(null)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object that will be applied to the appointments that match the condition.")]
		public SchedulerFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
				}
				return _formatStyle;
			}
			set
			{
				if (_formatStyle != value)
				{
					_formatStyle = value;
					InvalidateScheduler();
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents the key used to identify an object in a colletion.")]
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

		[Description("The SchedulerFilterCondition that appointments need to meet in order to apply the FormatStyle associated to this instance in them.")]
		[DefaultValue(null)]
		[TypeConverter(typeof(FilterConditionConverter))]
		[Editor(typeof(FilterConditionEditor), typeof(UITypeEditor))]
		public SchedulerFilterCondition FilterCondition
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
						_filterCondition._filterOwner = null;
					}
					_filterCondition = value;
					if (_filterCondition != null)
					{
						_filterCondition._filterOwner = this;
					}
				}
			}
		}

		[Browsable(false)]
		public MxScheduler Scheduler
		{
			get
			{
				if (_owner != null)
				{
					return _owner._scheduler;
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (Scheduler != null)
			{
				int fieldIndex = (int)LayoutReader.GetPropertyValue(info, "FieldIndex", typeof(int), -1);
				if (fieldIndex >= 0 && fieldIndex < Scheduler.Fields.Count)
				{
					_field = Scheduler.Fields[fieldIndex];
				}
			}
			_conditionOperator = (ConditionOperator)LayoutReader.GetPropertyValue(info, "ConditionOperator", typeof(ConditionOperator), ConditionOperator.Equal);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			_firstValue = LayoutReader.GetPropertyValue(info, "Value1", typeof(string), null);
			_secondValue = LayoutReader.GetPropertyValue(info, "Value2", typeof(string), null);
			_borderColor = (Color)LayoutReader.GetPropertyValue(info, "BorderColor", typeof(Color), Color.Empty);
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				_filterCondition = new SchedulerFilterCondition();
				_filterCondition._filterOwner = this;
				propertyValue.DeserializeObject(_filterCondition);
			}
			else
			{
				_filterCondition = null;
			}
			_firstImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex1", typeof(int), -1);
			_secondImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex2", typeof(int), -1);
			_thirdImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex3", typeof(int), -1);
			_fourthImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex4", typeof(int), -1);
			_firstImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey1", typeof(string), "");
			_secondImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey2", typeof(string), "");
			_thirdImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey3", typeof(string), "");
			_fourthImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey4", typeof(string), "");
			info.DeserializeObject("FormatStyle", FormatStyle);
			UpdateSchedulerFormatCondition();
		}

		internal void UpdateSchedulerFormatCondition()
		{
			if (_field != null && _field.Type != typeof(string))
			{
				if (_firstValue != null)
				{
					_firstValue = PropertyValue.ConvertValue((string)_firstValue, _field.Type);
				}
				if (_secondValue != null)
				{
					_secondValue = PropertyValue.ConvertValue((string)_secondValue, _field.Type);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_field != null)
			{
				info.AddValue("FieldIndex", _field._index);
			}
			if (_conditionOperator != ConditionOperator.Equal)
			{
				info.AddValue("ConditionOperator", _conditionOperator);
			}
			if (ShouldSerializeFormatStyle())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (!_borderColor.IsEmpty)
			{
				info.AddValue("BorderColor", _borderColor);
			}
			if (_key.Length > 0)
			{
				info.AddValue("Key", _key);
			}
			if (!_enabled)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (_firstValue != null)
			{
				info.AddValue("Value1", _firstValue);
			}
			if (_secondValue != null)
			{
				info.AddValue("Value2", _secondValue);
			}
			if (_filterCondition != null)
			{
				info.AddValue("FilterCondition", _filterCondition);
			}
			if (_firstImageIndex != -1)
			{
				info.AddValue("ImageIndex1", _firstImageIndex);
			}
			if (_secondImageIndex != -1)
			{
				info.AddValue("ImageIndex2", _secondImageIndex);
			}
			if (_thirdImageIndex != -1)
			{
				info.AddValue("ImageIndex3", _thirdImageIndex);
			}
			if (_fourthImageIndex != -1)
			{
				info.AddValue("ImageIndex4", _fourthImageIndex);
			}
			if (_firstImageKey.Length > 0)
			{
				info.AddValue("ImageKey1", _firstImageKey);
			}
			if (_secondImageKey.Length > 0)
			{
				info.AddValue("ImageKey2", _secondImageKey);
			}
			if (_thirdImageKey.Length > 0)
			{
				info.AddValue("ImageKey3", _thirdImageKey);
			}
			if (_fourthImageKey.Length > 0)
			{
				info.AddValue("ImageKey4", _fourthImageKey);
			}
		}

		void ISchedulerFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			if (Scheduler != null)
			{
				((ISchedulerFormatStyleOwner)Scheduler).InvalidateFormatStyleLayout();
			}
		}

		public SchedulerFormatCondition()
			: this(null, ConditionOperator.Equal, null, null)
		{
		}

		public SchedulerFormatCondition(SchedulerField field, ConditionOperator conditionOperator, object value)
			: this(field, conditionOperator, value, null)
		{
		}

		public SchedulerFormatCondition(SchedulerField field, ConditionOperator conditionOperator, object value1, object value2)
		{
			_field = field;
			_conditionOperator = conditionOperator;
			_firstValue = value1;
			_secondValue = value2;
			_collectionIndex = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			FormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle == null)
			{
				return false;
			}
			if (_formatStyle._owner == this && _formatStyle.HasOnlyDefaultValues())
			{
				return false;
			}
			return true;
		}

		public object Clone()
		{
			SchedulerFormatCondition schedulerFormatCondition = new SchedulerFormatCondition(_field, _conditionOperator, _firstValue, _secondValue);
			schedulerFormatCondition._enabled = _enabled;
			schedulerFormatCondition._formatStyle = FormatStyle.CloneAsCustomStyle();
			schedulerFormatCondition._formatStyle._owner = schedulerFormatCondition;
			schedulerFormatCondition._firstImageIndex = _firstImageIndex;
			schedulerFormatCondition._secondImageIndex = _secondImageIndex;
			schedulerFormatCondition._thirdImageIndex = _thirdImageIndex;
			schedulerFormatCondition._fourthImageIndex = _fourthImageIndex;
			schedulerFormatCondition._firstImageKey = _firstImageKey;
			schedulerFormatCondition._secondImageKey = _secondImageKey;
			schedulerFormatCondition._thirdImageKey = _thirdImageKey;
			schedulerFormatCondition._fourthImageKey = _fourthImageKey;
			schedulerFormatCondition._collectionIndex = -1;
			schedulerFormatCondition._key = _key;
			return schedulerFormatCondition;
		}

		internal bool IsValid(MxScheduler scheduler)
		{
			if (scheduler == null)
			{
				return true;
			}
			if (_field == null && _filterCondition == null)
			{
				return false;
			}
			return true;
		}

		internal bool MatchesAppointment(SchedulerAppointment schedulerAppointment)
		{
			object dataRow = schedulerAppointment.DataRow;
			if (Scheduler == null || (_field == null && _filterCondition == null))
			{
				return true;
			}
			if (_filterCondition != null)
			{
				return _filterCondition.IsAppointmentAccepted(schedulerAppointment);
			}
			if (_field == null)
			{
				return false;
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
			if (!SchedulerRenderer.IsNullOrDBNull(_firstValue) && !SchedulerRenderer.IsNullOrDBNull(fieldValue))
			{
				_firstValue = Convert.ChangeType(_firstValue, fieldValue.GetType());
			}
			if (!SchedulerRenderer.IsNullOrDBNull(_secondValue) && !SchedulerRenderer.IsNullOrDBNull(fieldValue))
			{
				_secondValue = Convert.ChangeType(_secondValue, fieldValue.GetType());
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
				if (SchedulerRenderer.IsEqualToAny(fieldValue, _firstValue, null, _field.CompareIgnoreCase))
				{
					return true;
				}
				return false;
			case ConditionOperator.NotIn:
				if (SchedulerRenderer.IsEqualToAny(fieldValue, _firstValue, null, _field.CompareIgnoreCase))
				{
					return false;
				}
				return true;
			case ConditionOperator.Equal:
				return SchedulerRenderer.CompareValues(fieldValue, _firstValue, null, _field.CompareIgnoreCase) == 0;
			case ConditionOperator.NotEqual:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) != 0;
			case ConditionOperator.Between:
				if (SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) >= 0)
				{
					return SchedulerRenderer.CompareIgnoringCase(fieldValue, _secondValue, null) <= 0;
				}
				return false;
			case ConditionOperator.Contains:
				return SchedulerRenderer.ContainsIgnoreCase(fieldValue, _firstValue);
			case ConditionOperator.EndsWith:
				return SchedulerRenderer.EndsWithIgnoreCase(fieldValue, _firstValue);
			case ConditionOperator.BeginsWith:
				return SchedulerRenderer.StartsWithIgnoreCase(fieldValue, _firstValue);
			case ConditionOperator.GreaterThan:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) > 0;
			case ConditionOperator.GreaterThanOrEqualTo:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) >= 0;
			case ConditionOperator.LessThan:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) < 0;
			case ConditionOperator.LessThanOrEqualTo:
				return SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) <= 0;
			case ConditionOperator.NotBetween:
				if (SchedulerRenderer.CompareIgnoringCase(fieldValue, _firstValue, null) >= 0)
				{
					return SchedulerRenderer.CompareIgnoringCase(fieldValue, _secondValue, null) > 0;
				}
				return true;
			case ConditionOperator.NotContains:
				return !SchedulerRenderer.ContainsIgnoreCase(fieldValue, _firstValue);
			default:
				return false;
			}
		}

		internal void InvalidateScheduler()
		{
			if (_owner != null)
			{
				_owner._scheduler.InvalidateAppointmentStyles();
				_owner._scheduler.Invalidate();
			}
		}
	}
}
