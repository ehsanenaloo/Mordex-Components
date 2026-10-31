using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(HourRangeConverter))]
	public class SchedulerHourRange : ISchedulerFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private bool _isWorkingTime = true;

		private object _tag;

		private string _key = "";

		private SchedulerDayOfWeek _dayOfWeek = SchedulerDayOfWeek.Monday;

		private WorkingHourRecurrencePattern _recurrencePattern;

		private TimeSpan _startTime = new TimeSpan(8, 0, 0);

		private TimeSpan _endTime = new TimeSpan(17, 0, 0);

		private SchedulerFormatStyle _formatStyle;

		private SchedulerFormatStyle _alternatingFormatStyle;

		internal int _index = -1;

		private ISchedulerWorkingHoursOwner _owner;

		int ISchedulerFormatStyleOwner.DefaultAlpha => -1;

		AlphaMode ISchedulerFormatStyleOwner.BackColorAlphaMode => AlphaMode.Empty;

		float ISchedulerFormatStyleOwner.DefaultBlend => -1f;

		Color ISchedulerFormatStyleOwner.BlendColor => Color.Empty;

		AlphaMode ISchedulerFormatStyleOwner.ForeColorAlphaMode => AlphaMode.Empty;

		MxScheduler ISchedulerFormatStyleOwner.Scheduler
		{
			get
			{
				if (GetOwner() != null)
				{
					return GetOwner().Scheduler;
				}
				return null;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Indicates whether the time range represented by this class is considered as working time")]
		[DefaultValue(true)]
		public bool IsWorkingTime
		{
			get
			{
				return _isWorkingTime;
			}
			set
			{
				if (_isWorkingTime != value)
				{
					_isWorkingTime = value;
					UpdateSchedulerHourRange();
				}
			}
		}

		[Browsable(false)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				if (_tag != value)
				{
					_tag = value;
				}
			}
		}

		[Description("The key used to identify an object in a colletion.")]
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

		[DefaultValue(SchedulerDayOfWeek.Monday)]
		[TypeConverter(typeof(FlagsEnumConverter))]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		public SchedulerDayOfWeek DayOfWeek
		{
			get
			{
				return _dayOfWeek;
			}
			set
			{
				if (_dayOfWeek != value)
				{
					_dayOfWeek = value;
					UpdateSchedulerHourRange();
				}
			}
		}

		[Description("The a RecurrencePattern object that represents the recurrence attributes of the HourRange")]
		[Editor(typeof(RecurrencePatternEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(RecurrencePatternConverter))]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WorkingHourRecurrencePattern RecurrencePattern
		{
			get
			{
				return _recurrencePattern;
			}
			set
			{
				if (_recurrencePattern == value)
				{
					return;
				}
				_recurrencePattern = value;
				if (_recurrencePattern != null)
				{
					if (_recurrencePattern.PatternStartDate == DateTime.MinValue)
					{
						if (GetOwner() != null)
						{
							_recurrencePattern.PatternStartDate = GetOwner().MinDate.Date;
						}
						else
						{
							_recurrencePattern.PatternStartDate = DateTime.MinValue.AddMonths(1);
						}
					}
					if (_recurrencePattern.DayOfWeek == (SchedulerDayOfWeek)0)
					{
						if (_dayOfWeek != 0)
						{
							_recurrencePattern.DayOfWeek = _dayOfWeek;
						}
						else
						{
							_recurrencePattern.DayOfWeek = SchedulerDayOfWeek.Sunday | SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday | SchedulerDayOfWeek.Saturday;
						}
					}
					if (_recurrencePattern.StartTime == TimeSpan.Zero && RecurrencePattern.EndTime == TimeSpan.Zero)
					{
						_recurrencePattern.BeginEdit();
						_recurrencePattern.StartTime = StartTime;
						_recurrencePattern.Duration = EndTime.Subtract(StartTime);
						_recurrencePattern.EndEdit();
					}
					_recurrencePattern._hourRange = this;
				}
				UpdateSchedulerHourRange();
			}
		}

		[Description("Specifies the minimum time for the rows that will be formatted.")]
		public TimeSpan StartTime
		{
			get
			{
				return _startTime;
			}
			set
			{
				if (_startTime != value)
				{
					_startTime = value;
					UpdateSchedulerHourRange();
				}
			}
		}

		[Description("Specifies the maximum time for the rows that will be formatted.")]
		public TimeSpan EndTime
		{
			get
			{
				return _endTime;
			}
			set
			{
				if (_endTime != value)
				{
					_endTime = value;
					UpdateSchedulerHourRange();
				}
			}
		}

		[Description("The SchedulerFormatStyle object used to draw the background of the rows, in the hour range, when the Schedule is in day or work week views.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the background of alternating rows, in the hour range, when the Schedule is in day or work week views.")]
		public SchedulerFormatStyle AlternatingFormatStyle
		{
			get
			{
				if (_alternatingFormatStyle == null)
				{
					_alternatingFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
				}
				return _alternatingFormatStyle;
			}
		}

		void ISchedulerFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			UpdateSchedulerHourRange();
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_formatStyle = (SchedulerFormatStyle)LayoutReader.GetPropertyValue(info, "FormatStyle", typeof(SchedulerFormatStyle), null);
			if (_formatStyle != null)
			{
				_formatStyle._owner = this;
			}
			PropertyValue propertyValue = info["RecurrencePattern"];
			if (propertyValue != null)
			{
				WorkingHourRecurrencePattern workingHourRecurrencePattern = new WorkingHourRecurrencePattern();
				propertyValue.DeserializeObject(workingHourRecurrencePattern);
				RecurrencePattern = workingHourRecurrencePattern;
			}
			else
			{
				RecurrencePattern = null;
			}
			_alternatingFormatStyle = (SchedulerFormatStyle)LayoutReader.GetPropertyValue(info, "AlternatingFormatStyle", typeof(SchedulerFormatStyle), null);
			if (_alternatingFormatStyle != null)
			{
				_alternatingFormatStyle._owner = this;
			}
			_dayOfWeek = (SchedulerDayOfWeek)LayoutReader.GetPropertyValue(info, "DayOfWeek", typeof(SchedulerDayOfWeek), SchedulerDayOfWeek.Monday);
			_startTime = (TimeSpan)LayoutReader.GetPropertyValue(info, "StartTime", typeof(TimeSpan), new TimeSpan(8, 0, 0));
			_endTime = (TimeSpan)LayoutReader.GetPropertyValue(info, "EndTime", typeof(TimeSpan), new TimeSpan(17, 0, 0));
			_isWorkingTime = (bool)LayoutReader.GetPropertyValue(info, "IsWorkingTime", typeof(bool), true);
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (tagTypeName != null)
				{
					try
					{
						Type type = LayoutTypeResolver.Resolve(tagTypeName);
						_tag = PropertyValue.ConvertValue(text, type);
						return;
					}
					catch
					{
						_tag = text;
						return;
					}
				}
				_tag = text;
			}
			else
			{
				_tag = null;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_key != string.Empty)
			{
				info.AddValue("Key", _key);
			}
			if (_dayOfWeek != SchedulerDayOfWeek.Monday)
			{
				info.AddValue("DayOfWeek", _dayOfWeek);
			}
			if (_formatStyle != null && !_formatStyle.HasOnlyDefaultValues())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_alternatingFormatStyle != null && !_alternatingFormatStyle.HasOnlyDefaultValues())
			{
				info.AddValue("AlternatingFormatStyle", _alternatingFormatStyle);
			}
			if (ShouldSerializeStartTime())
			{
				info.AddValue("StartTime", _startTime);
			}
			if (ShouldSerializeEndTime())
			{
				info.AddValue("EndTime", _endTime);
			}
			if (!_isWorkingTime)
			{
				info.AddValue("IsWorkingTime", _isWorkingTime);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				if (_tag.GetType().IsEnum)
				{
					_ = (int)_tag;
					info.AddValue("Tag", _tag);
				}
				else
				{
					if (_tag.GetType() != typeof(string))
					{
						info.AddValue("TagTypeName", _tag.GetType().FullName);
					}
					info.AddValue("Tag", _tag);
				}
			}
			if (_recurrencePattern != null)
			{
				info.AddValue("RecurrencePattern", RecurrencePattern);
			}
		}

		public SchedulerHourRange()
		{
		}

		public SchedulerHourRange(SchedulerDayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
		{
			_dayOfWeek = dayOfWeek;
			_startTime = startTime;
			_endTime = endTime;
		}

		public SchedulerHourRange(SchedulerDayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, bool workingTime)
		{
			_dayOfWeek = dayOfWeek;
			_startTime = startTime;
			_endTime = endTime;
			_isWorkingTime = workingTime;
		}

		internal SchedulerHourRange CloneWithDayAndTimes(SchedulerDayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
		{
			SchedulerHourRange schedulerHourRange = Clone();
			schedulerHourRange._dayOfWeek = dayOfWeek;
			schedulerHourRange._startTime = startTime;
			schedulerHourRange._endTime = endTime;
			return schedulerHourRange;
		}

		public SchedulerHourRange Clone()
		{
			SchedulerHourRange schedulerHourRange = new SchedulerHourRange();
			if (_alternatingFormatStyle != null)
			{
				schedulerHourRange._alternatingFormatStyle = _alternatingFormatStyle.CloneAsCustomStyle();
				schedulerHourRange._alternatingFormatStyle._owner = this;
			}
			schedulerHourRange._dayOfWeek = _dayOfWeek;
			schedulerHourRange._endTime = _endTime;
			if (_formatStyle != null)
			{
				schedulerHourRange._formatStyle = _formatStyle.CloneAsCustomStyle();
				schedulerHourRange._formatStyle._owner = this;
			}
			schedulerHourRange._key = _key;
			schedulerHourRange._startTime = _startTime;
			schedulerHourRange._isWorkingTime = _isWorkingTime;
			schedulerHourRange._tag = _tag;
			return schedulerHourRange;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeStartTime()
		{
			return !_startTime.Equals(new TimeSpan(8, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetStartTime()
		{
			if (ShouldSerializeStartTime())
			{
				StartTime = new TimeSpan(8, 0, 0);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeEndTime()
		{
			return !_endTime.Equals(new TimeSpan(17, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetEndTime()
		{
			if (ShouldSerializeEndTime())
			{
				EndTime = new TimeSpan(17, 0, 0);
			}
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlternatingFormatStyle()
		{
			AlternatingFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlternatingFormatStyle()
		{
			if (_alternatingFormatStyle == null)
			{
				return false;
			}
			if (_alternatingFormatStyle._owner == this && _alternatingFormatStyle.HasOnlyDefaultValues())
			{
				return false;
			}
			return true;
		}

		internal ISchedulerWorkingHoursOwner GetOwner()
		{
			return _owner;
		}

		internal void SetOwner(ISchedulerWorkingHoursOwner owner)
		{
			if (_owner != owner)
			{
				_owner = owner;
			}
		}

		internal void UpdateSchedulerHourRange()
		{
			if (GetOwner() != null)
			{
				GetOwner().InvalidateHour();
			}
		}
	}
}
