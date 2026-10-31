using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(ExceptionConverter))]
	public class WorkingHourException : ISchedulerWorkingHoursOwner, IMordexXmlLayoutsSupport
	{
		private DateRange _dateRange;

		private SchedulerHourRange _hourRange;

		private string _key = "";

		private object _tag;

		internal WorkingHourExceptionCollection _owner;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.FirstDayOfWeek
		{
			get
			{
				if (_hourRange != null && _hourRange.GetOwner() != null)
				{
					return _hourRange.GetOwner().FirstDayOfWeek;
				}
				return SchedulerDayOfWeek.Sunday;
			}
		}

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.WorkWeek
		{
			get
			{
				if (_hourRange != null && _hourRange.GetOwner() != null)
				{
					return _hourRange.GetOwner().WorkWeek;
				}
				return SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			}
		}

		MxScheduler ISchedulerWorkingHoursOwner.Scheduler => Scheduler;

		DateTime ISchedulerWorkingHoursOwner.MinDate
		{
			get
			{
				if (Scheduler != null)
				{
					return Scheduler.MinDate;
				}
				return DateTime.MinValue.AddMonths(1);
			}
		}

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

		[Description("Represents the object used to format the background of the rows in the Schedule control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(HourRangeConverter))]
		public SchedulerHourRange HourRange => _hourRange;

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
		public MxScheduler Scheduler
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Scheduler;
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_dateRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
			_dateRange.Start = (DateTime)LayoutReader.GetPropertyValue(info, "DateStart", typeof(DateTime), DateTime.MinValue);
			_dateRange.End = (DateTime)LayoutReader.GetPropertyValue(info, "DateEnd", typeof(DateTime), DateTime.MinValue);
			PropertyValue propertyValue = info["HourRange"];
			_hourRange = new SchedulerHourRange();
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
			if (_tag.GetType().IsEnum)
			{
				_ = (int)_tag;
				info.AddValue("Tag", _tag);
				return;
			}
			if (_tag.GetType() != typeof(string))
			{
				info.AddValue("TagTypeName", _tag.GetType().FullName);
			}
			info.AddValue("Tag", _tag);
		}

		void ISchedulerWorkingHoursOwner.InvalidateHour()
		{
			if (Scheduler != null)
			{
				Scheduler._isHourRangeCacheStale = true;
				Scheduler.InvalidateAppointmentLayout();
			}
		}

		public WorkingHourException(DateRange dateRange)
		{
			_dateRange = dateRange;
			SchedulerDayOfWeek dayOfWeek = SchedulerDayOfWeek.Sunday | SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday | SchedulerDayOfWeek.Saturday;
			_hourRange = new SchedulerHourRange(dayOfWeek, TimeSpan.Zero, new TimeSpan(24, 0, 0));
			_hourRange.SetOwner(this);
		}

		public WorkingHourException(DateTime date)
			: this(new DateRange(date.Date, date.Date.AddDays(1.0)))
		{
		}

		public WorkingHourException()
			: this(new DateRange(DateTime.Today, DateTime.Today.AddDays(1.0)))
		{
		}

		public WorkingHourException Clone()
		{
			WorkingHourException ex = new WorkingHourException();
			ex.DateRange = _dateRange;
			ex._hourRange = _hourRange.Clone();
			ex._key = _key;
			ex._tag = _tag;
			return ex;
		}

		private void UpdateWorkingHourException()
		{
			if (_owner != null)
			{
				_owner.UpdateWorkingHourExceptionCollection();
			}
		}
	}
}
