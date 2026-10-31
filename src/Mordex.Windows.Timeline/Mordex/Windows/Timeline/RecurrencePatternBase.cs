using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class RecurrencePatternBase : IMordexXmlLayoutsSupport
	{
		internal bool _isEditing;

		private const int AllWeekDaysMask = 0x7F;

		private const int MaxDayOfMonth = 31;

		private const int MonthsInYear = 12;

		private RecurrenceType _recurrenceType = RecurrenceType.Daily;

		private OccurrenceInMonth _occurrenceInMonth = OccurrenceInMonth.First;

		private DayOfWeek _dayOfWeek;

		private DailyRecurrenceMode _dailyRecurrenceMode;

		private int _dayOfMonth = 1;

		private TimeSpan _duration = TimeSpan.Zero;

		private TimeSpan _startTime = TimeSpan.Zero;

		private TimeSpan _endTime = TimeSpan.Zero;

		private int _monthOfYear = 1;

		private int _occurrences = 10;

		private int _interval = 1;

		private RecurrenceEndMode _recurrenceEndMode = RecurrenceEndMode.Occurrences;

		private DateTime _patternStartDate = DateTime.MinValue;

		private DateTime _patternEndDate = DateTime.MaxValue;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public RecurrenceType RecurrenceType
		{
			get
			{
				return _recurrenceType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(RecurrenceType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_recurrenceType != value)
				{
					_recurrenceType = value;
					InvalidateOwner();
				}
			}
		}

		public OccurrenceInMonth OccurrenceInMonth
		{
			get
			{
				return _occurrenceInMonth;
			}
			set
			{
				if (_occurrenceInMonth != value)
				{
					if (!Enum.IsDefined(typeof(OccurrenceInMonth), value))
					{
						throw new InvalidEnumArgumentException();
					}
					_occurrenceInMonth = value;
					InvalidateOwner();
				}
			}
		}

		public DayOfWeek DayOfWeek
		{
			get
			{
				return _dayOfWeek;
			}
			set
			{
				if (value <= (DayOfWeek)0)
				{
					throw new InvalidEnumArgumentException();
				}
				if (_dayOfWeek != value)
				{
					_dayOfWeek = value;
					InvalidateOwner();
				}
			}
		}

		public DailyRecurrenceMode DailyRecurrenceMode
		{
			get
			{
				return _dailyRecurrenceMode;
			}
			set
			{
				if (_dailyRecurrenceMode != value)
				{
					_dailyRecurrenceMode = value;
					if (RecurrenceType == RecurrenceType.Daily)
					{
						InvalidateOwner();
					}
				}
			}
		}

		public int DayOfMonth
		{
			get
			{
				return _dayOfMonth;
			}
			set
			{
				if (_dayOfMonth != value)
				{
					if (value < 1 || value > MaxDayOfMonth)
					{
						throw new InvalidOperationException("The value must be between 1 and 31");
					}
					_dayOfMonth = value;
					InvalidateOwner();
				}
			}
		}

		public TimeSpan Duration
		{
			get
			{
				if (_duration == TimeSpan.Zero)
				{
					return EndTime.Subtract(StartTime);
				}
				return _duration;
			}
			set
			{
				if (_duration != value)
				{
					_duration = value;
					_endTime = _startTime.Add(_duration);
					InvalidateOwner();
				}
			}
		}

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
					if (!_isEditing && _endTime < value)
					{
						throw new ArgumentException("The EndTime must be bigger that the StartTime");
					}
					_startTime = value;
					if (_endTime > _startTime)
					{
						_duration = _endTime.Subtract(_startTime);
					}
					InvalidateOwner();
				}
			}
		}

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
					if (!_isEditing && value < _startTime)
					{
						throw new ArgumentException("The EndTime must be bigger that the StartTime");
					}
					_endTime = value;
					if (_endTime > _startTime)
					{
						_duration = _endTime.Subtract(_startTime);
					}
					InvalidateOwner();
				}
			}
		}

		public int MonthOfYear
		{
			get
			{
				return _monthOfYear;
			}
			set
			{
				if (_monthOfYear != value)
				{
					if (value <= 0 || value > MonthsInYear)
					{
						throw new InvalidOperationException("The value must be between 1 and 12");
					}
					_monthOfYear = value;
					InvalidateOwner();
				}
			}
		}

		public int Occurrences
		{
			get
			{
				return _occurrences;
			}
			set
			{
				if (_occurrences != value)
				{
					if (value <= 0)
					{
						throw new InvalidOperationException();
					}
					_occurrences = value;
					InvalidateOwner();
				}
			}
		}

		public int Interval
		{
			get
			{
				return _interval;
			}
			set
			{
				if (_interval != value)
				{
					if (value <= 0)
					{
						throw new ArgumentException("Interval must be bigger or equal to one");
					}
					_interval = value;
					InvalidateOwner();
				}
			}
		}

		public RecurrenceEndMode RecurrenceEndMode
		{
			get
			{
				return _recurrenceEndMode;
			}
			set
			{
				if (_recurrenceEndMode != value)
				{
					_recurrenceEndMode = value;
					InvalidateOwner();
				}
			}
		}

		public DateTime PatternStartDate
		{
			get
			{
				return _patternStartDate;
			}
			set
			{
				value = value.Date;
				if (_patternStartDate != value)
				{
					if (_patternEndDate < value)
					{
						throw new ArgumentException("The PatternEndDate must be bigger that the PatternStartDate");
					}
					_patternStartDate = value;
					InvalidateOwner();
				}
			}
		}

		public DateTime PatternEndDate
		{
			get
			{
				return _patternEndDate;
			}
			set
			{
				value = value.Date;
				if (_patternEndDate != value)
				{
					if (value < _patternStartDate)
					{
						throw new ArgumentException("The PatternEndDate must be bigger that the PatternStartDate");
					}
					_patternEndDate = value;
					InvalidateOwner();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			DeserializeRecurrencePatternBaseState(info);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			SerializeRecurrencePatternBaseState(info);
		}

		internal void SerializeRecurrencePatternBaseState(LayoutWriter mordexLayoutWriter)
		{
			if (_dailyRecurrenceMode != DailyRecurrenceMode.UseInterval)
			{
				mordexLayoutWriter.AddValue("DailyRecurrenceMode", _dailyRecurrenceMode);
			}
			if (_dayOfMonth != 1)
			{
				mordexLayoutWriter.AddValue("DayOfMonth", _dayOfMonth);
			}
			if (_dayOfWeek != DayOfWeek.Monday)
			{
				mordexLayoutWriter.AddValue("DayOfWeek", _dayOfWeek);
			}
			if (_duration != TimeSpan.Zero)
			{
				mordexLayoutWriter.AddValue("Duration", _duration);
			}
			if (_endTime != TimeSpan.Zero)
			{
				mordexLayoutWriter.AddValue("EndTime", _endTime);
			}
			if (_interval != 1)
			{
				mordexLayoutWriter.AddValue("Interval", _interval);
			}
			if (_monthOfYear != 1)
			{
				mordexLayoutWriter.AddValue("MonthOfYear", _monthOfYear);
			}
			if (_occurrenceInMonth != OccurrenceInMonth.First)
			{
				mordexLayoutWriter.AddValue("OccurrenceInMonth", _occurrenceInMonth);
			}
			if (_occurrences != 10)
			{
				mordexLayoutWriter.AddValue("Occurrences", _occurrences);
			}
			if (_patternEndDate != DateTime.MaxValue)
			{
				mordexLayoutWriter.AddValue("PatternEndDate", _patternEndDate);
			}
			if (_patternStartDate != DateTime.MinValue)
			{
				mordexLayoutWriter.AddValue("PatternStartDate", _patternStartDate);
			}
			if (_recurrenceEndMode != RecurrenceEndMode.Occurrences)
			{
				mordexLayoutWriter.AddValue("RecurrenceEndMode", _recurrenceEndMode);
			}
			if (_recurrenceType != RecurrenceType.Daily)
			{
				mordexLayoutWriter.AddValue("RecurrenceType", _recurrenceType);
			}
			if (_startTime != TimeSpan.Zero)
			{
				mordexLayoutWriter.AddValue("StartTime", _startTime);
			}
		}

		internal void DeserializeRecurrencePatternBaseState(LayoutReader reader)
		{
			_dailyRecurrenceMode = (DailyRecurrenceMode)LayoutReader.GetPropertyValue(reader, "DailyRecurrenceMode", typeof(DailyRecurrenceMode), DailyRecurrenceMode.UseInterval);
			_dayOfMonth = (int)LayoutReader.GetPropertyValue(reader, "DayOfMonth", typeof(int), 1);
			_dayOfWeek = (DayOfWeek)LayoutReader.GetPropertyValue(reader, "DayOfWeek", typeof(DayOfWeek), DayOfWeek.Monday);
			_duration = (TimeSpan)LayoutReader.GetPropertyValue(reader, "Duration", typeof(TimeSpan), TimeSpan.Zero);
			_recurrenceEndMode = (RecurrenceEndMode)LayoutReader.GetPropertyValue(reader, "RecurrenceEndMode", typeof(RecurrenceEndMode), RecurrenceEndMode.Occurrences);
			_endTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "EndTime", typeof(TimeSpan), TimeSpan.Zero);
			_interval = (int)LayoutReader.GetPropertyValue(reader, "Interval", typeof(int), 1);
			_monthOfYear = (int)LayoutReader.GetPropertyValue(reader, "MonthOfYear", typeof(int), 1);
			_occurrenceInMonth = (OccurrenceInMonth)LayoutReader.GetPropertyValue(reader, "OccurrenceInMonth", typeof(OccurrenceInMonth), OccurrenceInMonth.First);
			_occurrences = (int)LayoutReader.GetPropertyValue(reader, "Occurrences", typeof(int), 10);
			_patternEndDate = (DateTime)LayoutReader.GetPropertyValue(reader, "PatternEndDate", typeof(DateTime), DateTime.MaxValue);
			_patternStartDate = (DateTime)LayoutReader.GetPropertyValue(reader, "PatternStartDate", typeof(DateTime), DateTime.MinValue);
			_recurrenceType = (RecurrenceType)LayoutReader.GetPropertyValue(reader, "RecurrenceType", typeof(RecurrenceType), RecurrenceType.Daily);
			_startTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "StartTime", typeof(TimeSpan), TimeSpan.Zero);
			NormalizeDeserializedValues();
		}

		/// <summary>Brings values read from a layout into the ranges that the property setters allow.</summary>
		private void NormalizeDeserializedValues()
		{
			if (!Enum.IsDefined(typeof(RecurrenceType), _recurrenceType))
			{
				_recurrenceType = RecurrenceType.Daily;
			}
			if (!Enum.IsDefined(typeof(OccurrenceInMonth), _occurrenceInMonth))
			{
				_occurrenceInMonth = OccurrenceInMonth.First;
			}
			if (!Enum.IsDefined(typeof(DailyRecurrenceMode), _dailyRecurrenceMode))
			{
				_dailyRecurrenceMode = DailyRecurrenceMode.UseInterval;
			}
			if (!Enum.IsDefined(typeof(RecurrenceEndMode), _recurrenceEndMode))
			{
				_recurrenceEndMode = RecurrenceEndMode.Occurrences;
			}
			_dayOfWeek = (DayOfWeek)((int)_dayOfWeek & AllWeekDaysMask);
			if (_dayOfWeek == 0)
			{
				_dayOfWeek = DayOfWeek.Monday;
			}
			_interval = Math.Max(1, _interval);
			_occurrences = Math.Max(1, _occurrences);
			_dayOfMonth = Math.Min(Math.Max(1, _dayOfMonth), MaxDayOfMonth);
			_monthOfYear = Math.Min(Math.Max(1, _monthOfYear), MonthsInYear);
			_patternStartDate = _patternStartDate.Date;
			_patternEndDate = _patternEndDate.Date;
			if (_patternEndDate < _patternStartDate)
			{
				_patternEndDate = _patternStartDate;
			}
		}

		public void BeginEdit()
		{
			_isEditing = true;
		}

		public void EndEdit()
		{
			_isEditing = false;
			if (_endTime < _startTime)
			{
				_endTime = _startTime.Add(_duration);
			}
			_duration = _endTime.Subtract(_startTime);
			InvalidateOwner();
		}

		public void SetDefaultValuesForDate(DateTime date)
		{
			_dayOfMonth = date.Day;
			_dayOfWeek = TimelineUtilities.ToDayOfWeekFlag(date.DayOfWeek);
			_monthOfYear = date.Month;
			_occurrenceInMonth = TimelineUtilities.GetOccurrenceInMonth(date, _dayOfWeek);
		}

		internal virtual void InvalidateOwner()
		{
		}
	}
}
