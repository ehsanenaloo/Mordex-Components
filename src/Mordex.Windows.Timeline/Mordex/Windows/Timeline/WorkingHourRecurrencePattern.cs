using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class WorkingHourRecurrencePattern : RecurrencePatternBase
	{
		internal TimelineHourRange _owner;

		public TimelineHourRange HourRange => _owner;

		internal override void InvalidateOwner()
		{
			if (!_isEditing && HourRange != null)
			{
				HourRange.InvalidateOwnerWorkingTime();
			}
		}

		public TimelineHourRange[] GetOccurrences(DateRange range)
		{
			DayOfWeek firstDayOfWeek = DayOfWeek.Sunday;
			DayOfWeek workWeek = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;
			if (_owner != null && _owner._owner != null)
			{
				firstDayOfWeek = _owner._owner.FirstDayOfWeek;
				workWeek = _owner._owner.WorkWeek;
			}
			if (range.Start.TimeOfDay != TimeSpan.Zero)
			{
				range.Start = range.Start.Date;
			}
			if (range.End.TimeOfDay != TimeSpan.Zero && range.End.Date < DateTime.MaxValue.Date)
			{
				range.End = range.End.Date.AddDays(1.0);
			}
			return new TimelineWorkingHourExpander(this, firstDayOfWeek, workWeek).GetHourRanges(range);
		}

		public TimelineHourRange[] GetOccurrence(DateTime date)
		{
			DateTime end = date.Date < DateTime.MaxValue.Date ? date.Date.AddDays(1.0) : DateTime.MaxValue;
			DateRange range = new DateRange(date.Date, end);
			return GetOccurrences(range);
		}
	}
}
