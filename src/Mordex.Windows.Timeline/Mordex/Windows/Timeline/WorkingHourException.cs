using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	[TypeConverter(typeof(ExceptionConverter))]
	public class WorkingHourException : ITimelineWorkingHoursOwner, IMordexXmlLayoutsSupport
	{
		private DateRange _dateRange;

		private TimelineHourRange _hourRange;

		private string _key = "";

		private object _tag;

		internal WorkingHourExceptionCollection _owner;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Specifies the date at which this exception occurs.")]
		public DateRange DateRange
		{
			get
			{
				return _dateRange;
			}
			set
			{
				_dateRange = value;
				UpdateWorkingHourException();
			}
		}

		[Description("Represents the object used to format the background of the rows in the Timeline control.")]
		[TypeConverter(typeof(HourRangeConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TimelineHourRange HourRange => _hourRange;

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

		[Browsable(false)]
		public object Tag => _tag;

		[Browsable(false)]
		public MxTimeline Timeline
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

		MxTimeline ITimelineWorkingHoursOwner.Timeline => Timeline;

		DateTime ITimelineWorkingHoursOwner.MinDate
		{
			get
			{
				if (Timeline != null)
				{
					return Timeline.MinDate;
				}
				return DateTime.MinValue.AddMonths(1);
			}
		}

		DayOfWeek ITimelineWorkingHoursOwner.FirstDayOfWeek
		{
			get
			{
				if (Timeline != null)
				{
					return Timeline.FirstDayOfWeek;
				}
				return DayOfWeek.Sunday;
			}
		}

		DayOfWeek ITimelineWorkingHoursOwner.WorkWeek
		{
			get
			{
				if (Timeline != null)
				{
					return Timeline.WorkWeek;
				}
				return DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_dateRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
			_dateRange.Start = (DateTime)LayoutReader.GetPropertyValue(info, "DateStart", typeof(DateTime), DateTime.MinValue);
			_dateRange.End = (DateTime)LayoutReader.GetPropertyValue(info, "DateEnd", typeof(DateTime), DateTime.MinValue);
			PropertyValue propertyValue = info["HourRange"];
			_hourRange = new TimelineHourRange();
			_hourRange._owner = this;
			propertyValue?.DeserializeObject(_hourRange);
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
			if (_dateRange.Start != DateTime.MinValue)
			{
				info.AddValue("DateStart", _dateRange.Start);
			}
			if (_dateRange.End != DateTime.MinValue)
			{
				info.AddValue("DateEnd", _dateRange.End);
			}
			info.AddValue("HourRange", _hourRange);
			if (_tag == null)
			{
				return;
			}
			if (_tag.GetType() != typeof(string))
			{
				info.AddValue("TagTypeName", _tag.GetType().FullName);
			}
			info.AddValue("Tag", _tag);
		}

		public WorkingHourException(DateRange dateRange)
		{
			_dateRange = dateRange;
			DayOfWeek dayOfWeek = DayOfWeek.Sunday | DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday | DayOfWeek.Saturday;
			_hourRange = new TimelineHourRange(dayOfWeek, TimeSpan.Zero, new TimeSpan(24, 0, 0));
			_hourRange._owner = this;
		}

		public WorkingHourException()
			: this(new DateRange(DateTime.Today, DateTime.Today.AddDays(1.0)))
		{
		}

		private void UpdateWorkingHourException()
		{
			if (_owner != null)
			{
				_owner.InvalidateOwnerLayout();
			}
		}

		void ITimelineWorkingHoursOwner.InvalidateWorkingTime()
		{
			if (Timeline != null)
			{
				Timeline.Invalidate();
			}
		}
	}
}
