using System;
using System.Collections.Generic;
using Mordex.Windows.Recurrence;

namespace Mordex.Windows.Timeline
{
	/// <summary>Connects a <see cref="RecurrencePatternBase"/> to the shared date rules.</summary>
	internal class RecurrenceCalculator
	{

		protected readonly RecurrencePatternBase _pattern;

		protected readonly RecurrenceRules _rules;

		internal RecurrenceCalculator(RecurrencePatternBase pattern, DayOfWeek firstDayOfWeek, DayOfWeek workWeek)
		{
			_pattern = pattern;
			_rules = new RecurrenceRules
			{
				Kind = (RecurrenceRuleKind)(int)pattern.RecurrenceType,
				DailyMode = (DailyRuleMode)(int)pattern.DailyRecurrenceMode,
				Interval = pattern.Interval,
				WeekdayMask = (int)pattern.DayOfWeek & RecurrenceRules.AllDaysMask,
				WorkWeekMask = (int)workWeek & RecurrenceRules.AllDaysMask,
				DayOfMonth = pattern.DayOfMonth,
				MonthOfYear = pattern.MonthOfYear,
				StartDate = pattern.PatternStartDate,
				FirstDayOfWeek = TimelineUtilities.ToSystemDayOfWeek(firstDayOfWeek),
				NthWeekdayOfMonth = (year, month) => TimelineUtilities.GetNthWeekdayInMonth(new DateTime(year, month, 1), pattern.DayOfWeek, pattern.OccurrenceInMonth).Day
			};
		}

		internal static DateTime AddSaturating(DateTime value, TimeSpan offset)
		{
			if (offset > TimeSpan.Zero)
			{
				return DateTime.MaxValue - value < offset ? DateTime.MaxValue : value + offset;
			}
			return value - DateTime.MinValue < -offset ? DateTime.MinValue : value + offset;
		}

		/// <summary>Last moment of the pattern, or <see cref="DateTime.MaxValue"/> when it never ends.</summary>
		internal DateTime GetRecurrenceEnd()
		{
			TimeSpan endOffset = _pattern.EndTime.TotalDays < 1.0 ? _pattern.EndTime : TimeSpan.Zero;
			switch (_pattern.RecurrenceEndMode)
			{
			case RecurrenceEndMode.NoEndDate:
				return DateTime.MaxValue;
			case RecurrenceEndMode.EndDate:
				return AddSaturating(_pattern.PatternEndDate.Date, endOffset);
			default:
			{
				DateTime? lastOccurrence = _rules.NthOccurrence(_pattern.Occurrences);
				return lastOccurrence.HasValue ? AddSaturating(lastOccurrence.Value, endOffset) : DateTime.MaxValue;
			}
			}
		}

		/// <summary>Last date that a range [start, end) touches, as a whole day.</summary>
		internal static DateTime LastDayOfRange(DateTime start, DateTime end)
		{
			return end.TimeOfDay == TimeSpan.Zero && end > start ? end.Date.AddDays(-1) : end.Date;
		}

		/// <summary>Occurrence dates from <paramref name="firstDay"/> to <paramref name="lastDay"/>, both included.</summary>
		internal IEnumerable<DateTime> EnumerateOccurrences(DateTime firstDay, DateTime lastDay)
		{
			DateTime? day = _rules.NextOccurrence(firstDay, lastDay);
			while (day.HasValue)
			{
				yield return day.Value;
				day = day.Value < lastDay ? _rules.NextOccurrence(day.Value.AddDays(1), lastDay) : null;
			}
		}
	}
}
