using System;
using System.Collections.Generic;

namespace Mordex.Windows.Timeline
{
	/// <summary>Creates the working hour ranges of a working hour pattern for a date range.</summary>
	internal class TimelineWorkingHourExpander : RecurrenceCalculator
	{
		private readonly WorkingHourRecurrencePattern _workingHourPattern;

		internal TimelineWorkingHourExpander(WorkingHourRecurrencePattern pattern, DayOfWeek firstDayOfWeek, DayOfWeek workWeek)
			: base(pattern, firstDayOfWeek, workWeek)
		{
			_workingHourPattern = pattern;
		}

		internal TimelineHourRange[] GetHourRanges(DateRange range)
		{
			DateTime recurrenceEnd = GetRecurrenceEnd();
			if (recurrenceEnd < range.Start || range.End <= _pattern.PatternStartDate)
			{
				return new TimelineHourRange[0];
			}
			DateTime lastDay = LastDayOfRange(range.Start, range.End);
			if (lastDay > recurrenceEnd.Date)
			{
				lastDay = recurrenceEnd.Date;
			}
			var ranges = new List<TimelineHourRange>();
			foreach (DateTime day in EnumerateOccurrences(range.Start.Date, lastDay))
			{
				ranges.Add(_workingHourPattern.HourRange.CreateCopyWithRange(TimelineUtilities.ToDayOfWeekFlag(day.DayOfWeek), _workingHourPattern.StartTime, _workingHourPattern.EndTime));
			}
			return ranges.ToArray();
		}
	}
}
