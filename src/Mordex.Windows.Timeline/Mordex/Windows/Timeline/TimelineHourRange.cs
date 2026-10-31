using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	[TypeConverter(typeof(HourRangeConverter))]
	public class TimelineHourRange : ITimelineOwner, IMordexXmlLayoutsSupport
	{
		private string _key = "";

		private DayOfWeek _dayOfWeek = DayOfWeek.Monday;

		private TimeSpan _startTime = new TimeSpan(8, 0, 0);

		private TimeSpan _endTime = new TimeSpan(17, 0, 0);

		private TimelineFormatStyle _formatStyle;

		private bool _isWorkingTime = true;

		private WorkingHourRecurrencePattern _recurrencePattern;

		private object _tag;

		internal int _index = -1;

		internal ITimelineWorkingHoursOwner _owner;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue("")]
		[Description("The key used to identify an object in a colletion.")]
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

		[TypeConverter(typeof(FlagsEnumConverter))]
		[DefaultValue(DayOfWeek.Monday)]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		public DayOfWeek DayOfWeek
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
					InvalidateOwnerWorkingTime();
				}
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
					InvalidateOwnerWorkingTime();
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
					InvalidateOwnerWorkingTime();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The TimelineFormatStyle object used to draw the background of the Timeline in the specified hour range.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
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
		}

		[Localizable(true)]
		[DefaultValue(true)]
		[Description("Specifies whether this hour range should be consider as working time.")]
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
					InvalidateOwnerWorkingTime();
				}
			}
		}

		[DefaultValue(null)]
		[TypeConverter(typeof(RecurrencePatternConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor(typeof(RecurrencePatternEditor), typeof(UITypeEditor))]
		[Description("Represents the recurrence attributes of the HourRange")]
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
						if (_owner != null)
						{
							_recurrencePattern.PatternStartDate = _owner.MinDate.Date;
						}
						else
						{
							_recurrencePattern.PatternStartDate = DateTime.MinValue.AddMonths(1);
						}
					}
					if (_recurrencePattern.DayOfWeek == (DayOfWeek)0)
					{
						if (_dayOfWeek != 0)
						{
							_recurrencePattern.DayOfWeek = _dayOfWeek;
						}
						else
						{
							_recurrencePattern.DayOfWeek = DayOfWeek.Sunday | DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday | DayOfWeek.Saturday;
						}
					}
					if (_recurrencePattern.StartTime == TimeSpan.Zero && RecurrencePattern.EndTime == TimeSpan.Zero)
					{
						_recurrencePattern.BeginEdit();
						_recurrencePattern.StartTime = StartTime;
						_recurrencePattern.Duration = EndTime.Subtract(StartTime);
						_recurrencePattern.EndEdit();
					}
					_recurrencePattern._owner = this;
				}
				InvalidateOwnerWorkingTime();
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

		MxTimeline ITimelineOwner.Timeline
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Timeline;
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_formatStyle = (TimelineFormatStyle)LayoutReader.GetPropertyValue(info, "FormatStyle", typeof(TimelineFormatStyle), null);
			if (_formatStyle != null)
			{
				_formatStyle._owner = this;
			}
			_dayOfWeek = (DayOfWeek)LayoutReader.GetPropertyValue(info, "DayOfWeek", typeof(DayOfWeek), DayOfWeek.Monday);
			_startTime = (TimeSpan)LayoutReader.GetPropertyValue(info, "StartTime", typeof(TimeSpan), new TimeSpan(8, 0, 0));
			_endTime = (TimeSpan)LayoutReader.GetPropertyValue(info, "EndTime", typeof(TimeSpan), new TimeSpan(17, 0, 0));
			_isWorkingTime = (bool)LayoutReader.GetPropertyValue(info, "IsWorkingTime", typeof(bool), true);
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
			if (_dayOfWeek != DayOfWeek.Monday)
			{
				info.AddValue("DayOfWeek", _dayOfWeek);
			}
			if (_formatStyle != null && !_formatStyle.HasOnlyDefaultValues())
			{
				info.AddValue("FormatStyle", _formatStyle);
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
				info.AddValue("Tag", _tag);
			}
			if (_recurrencePattern != null)
			{
				info.AddValue("RecurrencePattern", RecurrencePattern);
			}
		}

		public TimelineHourRange()
		{
		}

		public TimelineHourRange(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
		{
			_dayOfWeek = dayOfWeek;
			_startTime = startTime;
			_endTime = endTime;
		}

		internal TimelineHourRange CreateCopyWithRange(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
		{
			TimelineHourRange timelineHourRange = CreateCopy();
			timelineHourRange._dayOfWeek = dayOfWeek;
			timelineHourRange._startTime = startTime;
			timelineHourRange._endTime = endTime;
			return timelineHourRange;
		}

		internal TimelineHourRange CreateCopy()
		{
			TimelineHourRange timelineHourRange = new TimelineHourRange();
			timelineHourRange._dayOfWeek = _dayOfWeek;
			timelineHourRange._endTime = _endTime;
			if (_formatStyle != null)
			{
				timelineHourRange._formatStyle = _formatStyle.Clone();
				timelineHourRange._formatStyle._owner = this;
			}
			timelineHourRange._key = _key;
			timelineHourRange._startTime = _startTime;
			timelineHourRange._isWorkingTime = _isWorkingTime;
			timelineHourRange._tag = _tag;
			return timelineHourRange;
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

		internal void InvalidateOwnerWorkingTime()
		{
			if (_owner != null)
			{
				_owner.InvalidateWorkingTime();
			}
		}
	}
}
