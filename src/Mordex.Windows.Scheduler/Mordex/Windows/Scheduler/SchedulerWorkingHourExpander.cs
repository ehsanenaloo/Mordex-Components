using System;
using System.Collections.Generic;

namespace Mordex.Windows.Scheduler
{
	/// <summary>Creates the working hour ranges of a working hour pattern for a date range.</summary>
	internal class SchedulerWorkingHourExpander : RecurrenceCalculator
	{
		private readonly WorkingHourRecurrencePattern _workingHourPattern;

		internal SchedulerWorkingHourExpander(WorkingHourRecurrencePattern pattern, SchedulerDayOfWeek firstDayOfWeek, SchedulerDayOfWeek workWeek)
			: base(pattern, firstDayOfWeek, workWeek)
		{
			_workingHourPattern = pattern;
		}

		internal SchedulerHourRange[] GetHourRanges(DateRange range)
		{
			DateTime recurrenceEnd = GetRecurrenceEnd();
			if (recurrenceEnd < range.Start || range.End <= _pattern.PatternStartDate)
			{
				return new SchedulerHourRange[0];
			}
			DateTime lastDay = LastDayOfRange(range.Start, range.End);
			if (lastDay > recurrenceEnd.Date)
			{
				lastDay = recurrenceEnd.Date;
			}
			var ranges = new List<SchedulerHourRange>();
			foreach (DateTime day in EnumerateOccurrences(range.Start.Date, lastDay))
			{
				ranges.Add(_workingHourPattern.HourRange.CloneWithDayAndTimes(SchedulerRenderer.ToSchedulerDayOfWeek(day.DayOfWeek), _workingHourPattern.StartTime, _workingHourPattern.EndTime));
			}
			return ranges.ToArray();
		}
	}
}
