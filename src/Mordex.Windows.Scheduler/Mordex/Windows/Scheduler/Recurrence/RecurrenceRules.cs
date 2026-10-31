using System;

namespace Mordex.Windows.Recurrence
{
	internal enum RecurrenceRuleKind
	{
		Daily = 1,
		Weekly,
		Monthly,
		MonthNth,
		Yearly,
		YearNth
	}

	internal enum DailyRuleMode
	{
		UseInterval,
		WeekDays,
		WeekendDays
	}

	/// <summary>
	/// Pure date arithmetic for a recurrence pattern. It has no knowledge of appointments or
	/// exceptions. All dates are handled as whole days, times of day are ignored.
	/// Day masks use one bit per <see cref="DayOfWeek"/> value (Sunday = bit 0).
	/// </summary>
	internal sealed class RecurrenceRules
	{
		internal const int DaysPerWeek = 7;

		internal const int MonthsPerYear = 12;

		internal const int AllDaysMask = 0x7F;

		private const int MaxSearchPasses = 3;

		private int _interval = 1;

		internal RecurrenceRuleKind Kind { get; set; } = RecurrenceRuleKind.Daily;

		internal DailyRuleMode DailyMode { get; set; }

		internal int Interval
		{
			get { return _interval; }
			set { _interval = Math.Max(1, value); }
		}

		internal int WeekdayMask { get; set; }

		internal int WorkWeekMask { get; set; } = (int)(1 << (int)DayOfWeek.Monday | 1 << (int)DayOfWeek.Tuesday | 1 << (int)DayOfWeek.Wednesday | 1 << (int)DayOfWeek.Thursday | 1 << (int)DayOfWeek.Friday);

		internal int DayOfMonth { get; set; } = 1;

		internal int MonthOfYear { get; set; } = 1;

		internal DateTime StartDate { get; set; } = DateTime.MinValue;

		internal DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Sunday;

		/// <summary>Returns the day of month of the Nth weekday for a year and month (MonthNth, YearNth).</summary>
		internal Func<int, int, int> NthWeekdayOfMonth { get; set; }

		internal static int MaskOf(DayOfWeek dayOfWeek)
		{
			return 1 << (int)dayOfWeek;
		}

		internal static int CountBits(int mask)
		{
			int count = 0;
			for (mask &= AllDaysMask; mask != 0; mask &= mask - 1)
			{
				count++;
			}
			return count;
		}

		internal bool IsOccurrence(DateTime date)
		{
			date = date.Date;
			if (date < StartDate.Date)
			{
				return false;
			}
			switch (Kind)
			{
			case RecurrenceRuleKind.Daily:
				return IsDailyOccurrence(date);
			case RecurrenceRuleKind.Weekly:
				return (WeekdayMask & MaskOf(date.DayOfWeek)) != 0 && WeeksBetween(StartDate.Date, date) % Interval == 0;
			case RecurrenceRuleKind.Monthly:
			case RecurrenceRuleKind.MonthNth:
				return MonthsBetween(StartDate, date) % Interval == 0 && date.Day == DayInMonth(date.Year, date.Month);
			case RecurrenceRuleKind.Yearly:
			case RecurrenceRuleKind.YearNth:
				return date.Month == MonthOfYear && (date.Year - StartDate.Year) % Interval == 0 && date.Day == DayInMonth(date.Year, date.Month);
			default:
				return false;
			}
		}

		/// <summary>First occurrence date in [onOrAfter, lastDate], or null.</summary>
		internal DateTime? NextOccurrence(DateTime onOrAfter, DateTime lastDate)
		{
			DateTime from = onOrAfter.Date;
			if (from < StartDate.Date)
			{
				from = StartDate.Date;
			}
			lastDate = lastDate.Date;
			if (from > lastDate)
			{
				return null;
			}
			DateTime? found;
			try
			{
				found = FindNext(from);
			}
			catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is OverflowException)
			{
				return null;
			}
			if (found.HasValue && found.Value <= lastDate)
			{
				return found;
			}
			return null;
		}

		/// <summary>Last occurrence date in [StartDate, onOrBefore], or null.</summary>
		internal DateTime? PreviousOccurrence(DateTime onOrBefore)
		{
			DateTime to = onOrBefore.Date;
			if (to < StartDate.Date)
			{
				return null;
			}
			try
			{
				return FindPrevious(to);
			}
			catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is OverflowException)
			{
				return null;
			}
		}

		/// <summary>Date of the Nth (1-based) occurrence, or null when it is beyond the supported date range.</summary>
		internal DateTime? NthOccurrence(int n)
		{
			if (n < 1)
			{
				return null;
			}
			try
			{
				return FindNth(n);
			}
			catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is OverflowException)
			{
				return null;
			}
		}

		private bool IsDailyOccurrence(DateTime date)
		{
			switch (DailyMode)
			{
			case DailyRuleMode.WeekDays:
				return (WorkWeekMask & MaskOf(date.DayOfWeek)) != 0;
			case DailyRuleMode.WeekendDays:
				return (WorkWeekMask & MaskOf(date.DayOfWeek)) == 0;
			default:
				return (date - StartDate.Date).Days % Interval == 0;
			}
		}

		private int DailyMask
		{
			get
			{
				return DailyMode == DailyRuleMode.WeekDays ? WorkWeekMask & AllDaysMask : ~WorkWeekMask & AllDaysMask;
			}
		}

		// Weekly arithmetic works on day numbers (days since 1 January 0001, a Monday). A week can start before
		// DateTime.MinValue or end after DateTime.MaxValue; day numbers keep that week usable, and only days
		// inside the DateTime range are turned back into dates.
		private static readonly long LastDayNumber = DateTime.MaxValue.Ticks / TimeSpan.TicksPerDay;

		private static long DayNumber(DateTime date)
		{
			return date.Ticks / TimeSpan.TicksPerDay;
		}

		private static DateTime DateOf(long dayNumber)
		{
			return new DateTime(dayNumber * TimeSpan.TicksPerDay);
		}

		private static DayOfWeek DayOfWeekOf(long dayNumber)
		{
			return (DayOfWeek)(int)(((dayNumber + 1) % DaysPerWeek + DaysPerWeek) % DaysPerWeek);
		}

		private long WeekStartDay(long dayNumber)
		{
			return dayNumber - (DaysPerWeek + (int)DayOfWeekOf(dayNumber) - (int)FirstDayOfWeek) % DaysPerWeek;
		}

		private int WeeksBetween(DateTime start, DateTime date)
		{
			return (int)((WeekStartDay(DayNumber(date)) - WeekStartDay(DayNumber(start))) / DaysPerWeek);
		}

		private static int MonthIndex(int year, int month)
		{
			return year * MonthsPerYear + month - 1;
		}

		private static int MonthsBetween(DateTime start, DateTime date)
		{
			return MonthIndex(date.Year, date.Month) - MonthIndex(start.Year, start.Month);
		}

		private bool UsesNthWeekday
		{
			get { return Kind == RecurrenceRuleKind.MonthNth || Kind == RecurrenceRuleKind.YearNth; }
		}

		private int DayInMonth(int year, int month)
		{
			int days = DateTime.DaysInMonth(year, month);
			if (UsesNthWeekday)
			{
				return NthWeekdayOfMonth != null ? Math.Min(NthWeekdayOfMonth(year, month), days) : 1;
			}
			return Math.Min(DayOfMonth, days);
		}

		private DateTime OccurrenceInMonth(int monthIndex)
		{
			int year = Math.DivRem(monthIndex, MonthsPerYear, out int zeroBasedMonth);
			return new DateTime(year, zeroBasedMonth + 1, DayInMonth(year, zeroBasedMonth + 1));
		}

		private DateTime OccurrenceInYear(int year)
		{
			return new DateTime(year, MonthOfYear, DayInMonth(year, MonthOfYear));
		}

		private static int FloorDiv(int value, int divisor)
		{
			int quotient = value / divisor;
			return value % divisor != 0 && value < 0 ? quotient - 1 : quotient;
		}

		private static int CeilDiv(int value, int divisor)
		{
			return -FloorDiv(-value, divisor);
		}

		private DateTime? FindNext(DateTime from)
		{
			DateTime start = StartDate.Date;
			switch (Kind)
			{
			case RecurrenceRuleKind.Daily:
				if (DailyMode == DailyRuleMode.UseInterval)
				{
					int steps = CeilDiv((from - start).Days, Interval);
					return start.AddDays((long)steps * Interval);
				}
				return ScanForward(from, DailyMask, DaysPerWeek);
			case RecurrenceRuleKind.Weekly:
				return NextWeekly(from, start);
			case RecurrenceRuleKind.Monthly:
			case RecurrenceRuleKind.MonthNth:
			{
				int startIndex = MonthIndex(start.Year, start.Month);
				int index = startIndex + CeilDiv(MonthIndex(from.Year, from.Month) - startIndex, Interval) * Interval;
				for (int pass = 0; pass < MaxSearchPasses; pass++, index += Interval)
				{
					DateTime candidate = OccurrenceInMonth(index);
					if (candidate >= from)
					{
						return candidate;
					}
				}
				return null;
			}
			case RecurrenceRuleKind.Yearly:
			case RecurrenceRuleKind.YearNth:
			{
				int year = start.Year + CeilDiv(from.Year - start.Year, Interval) * Interval;
				for (int pass = 0; pass < MaxSearchPasses; pass++, year += Interval)
				{
					DateTime candidate = OccurrenceInYear(year);
					if (candidate >= from)
					{
						return candidate;
					}
				}
				return null;
			}
			}
			return null;
		}

		private DateTime? NextWeekly(DateTime from, DateTime start)
		{
			int mask = WeekdayMask & AllDaysMask;
			if (mask == 0)
			{
				return null;
			}
			long day = DayNumber(from);
			long startWeek = WeekStartDay(DayNumber(start));
			for (int pass = 0; pass < MaxSearchPasses; pass++)
			{
				if (day > LastDayNumber)
				{
					return null;
				}
				long weekStart = WeekStartDay(day);
				long offset = (weekStart - startWeek) / DaysPerWeek % Interval;
				if (offset != 0)
				{
					weekStart += (Interval - offset) * DaysPerWeek;
					day = weekStart;
				}
				long weekEnd = Math.Min(weekStart + DaysPerWeek - 1, LastDayNumber);
				for (; day <= weekEnd; day++)
				{
					if ((mask & MaskOf(DayOfWeekOf(day))) != 0)
					{
						return DateOf(day);
					}
				}
				day = weekStart + (long)Interval * DaysPerWeek;
			}
			return null;
		}

		private static DateTime? ScanForward(DateTime from, int mask, int days)
		{
			if (mask == 0)
			{
				return null;
			}
			for (int i = 0; i < days; i++)
			{
				DateTime day = from.AddDays(i);
				if ((mask & MaskOf(day.DayOfWeek)) != 0)
				{
					return day;
				}
			}
			return null;
		}

		private static DateTime? ScanBackward(DateTime from, DateTime notBefore, int mask)
		{
			if (mask == 0)
			{
				return null;
			}
			for (DateTime day = from; day >= notBefore && (from - day).Days < DaysPerWeek; day = day.AddDays(-1))
			{
				if ((mask & MaskOf(day.DayOfWeek)) != 0)
				{
					return day;
				}
			}
			return null;
		}

		private DateTime? FindPrevious(DateTime to)
		{
			DateTime start = StartDate.Date;
			switch (Kind)
			{
			case RecurrenceRuleKind.Daily:
				if (DailyMode == DailyRuleMode.UseInterval)
				{
					int steps = FloorDiv((to - start).Days, Interval);
					return start.AddDays((long)steps * Interval);
				}
				return ScanBackward(to, start, DailyMask);
			case RecurrenceRuleKind.Weekly:
				return PreviousWeekly(to, start);
			case RecurrenceRuleKind.Monthly:
			case RecurrenceRuleKind.MonthNth:
			{
				int startIndex = MonthIndex(start.Year, start.Month);
				int index = startIndex + FloorDiv(MonthIndex(to.Year, to.Month) - startIndex, Interval) * Interval;
				for (int pass = 0; pass < MaxSearchPasses && index >= startIndex; pass++, index -= Interval)
				{
					DateTime candidate = OccurrenceInMonth(index);
					if (candidate <= to && candidate >= start)
					{
						return candidate;
					}
				}
				return null;
			}
			case RecurrenceRuleKind.Yearly:
			case RecurrenceRuleKind.YearNth:
			{
				int year = start.Year + FloorDiv(to.Year - start.Year, Interval) * Interval;
				for (int pass = 0; pass < MaxSearchPasses && year >= start.Year; pass++, year -= Interval)
				{
					DateTime candidate = OccurrenceInYear(year);
					if (candidate <= to && candidate >= start)
					{
						return candidate;
					}
				}
				return null;
			}
			}
			return null;
		}

		private DateTime? PreviousWeekly(DateTime to, DateTime start)
		{
			int mask = WeekdayMask & AllDaysMask;
			if (mask == 0)
			{
				return null;
			}
			long day = DayNumber(to);
			long startDay = DayNumber(start);
			long startWeek = WeekStartDay(startDay);
			for (int pass = 0; pass < MaxSearchPasses; pass++)
			{
				if (day < startDay)
				{
					return null;
				}
				long weekStart = WeekStartDay(day);
				long offset = (weekStart - startWeek) / DaysPerWeek % Interval;
				if (offset != 0)
				{
					weekStart -= offset * DaysPerWeek;
					day = weekStart + DaysPerWeek - 1;
				}
				long low = Math.Max(weekStart, startDay);
				for (; day >= low; day--)
				{
					if ((mask & MaskOf(DayOfWeekOf(day))) != 0)
					{
						return DateOf(day);
					}
				}
				day = weekStart - (long)(Interval - 1) * DaysPerWeek - 1;
			}
			return null;
		}

		private DateTime? FindNth(int n)
		{
			DateTime start = StartDate.Date;
			switch (Kind)
			{
			case RecurrenceRuleKind.Daily:
				if (DailyMode == DailyRuleMode.UseInterval)
				{
					return start.AddDays((long)(n - 1) * Interval);
				}
				return NthDailyOnMask(n, start);
			case RecurrenceRuleKind.Weekly:
				return NthWeekly(n, start);
			case RecurrenceRuleKind.Monthly:
			case RecurrenceRuleKind.MonthNth:
			{
				int startIndex = MonthIndex(start.Year, start.Month);
				int skip = OccurrenceInMonth(startIndex) >= start ? n - 1 : n;
				return OccurrenceInMonth(checked(startIndex + skip * Interval));
			}
			case RecurrenceRuleKind.Yearly:
			case RecurrenceRuleKind.YearNth:
			{
				int skip = OccurrenceInYear(start.Year) >= start ? n - 1 : n;
				return OccurrenceInYear(checked(start.Year + skip * Interval));
			}
			}
			return null;
		}

		private DateTime? NthDailyOnMask(int n, DateTime start)
		{
			int perWeek = CountBits(DailyMask);
			if (perWeek == 0)
			{
				return null;
			}
			DateTime cursor = start.AddDays((long)((n - 1) / perWeek) * DaysPerWeek);
			int remaining = (n - 1) % perWeek;
			for (int i = 0; i < DaysPerWeek; i++)
			{
				DateTime day = cursor.AddDays(i);
				if ((DailyMask & MaskOf(day.DayOfWeek)) != 0 && remaining-- == 0)
				{
					return day;
				}
			}
			return null;
		}

		private DateTime? NthWeekly(int n, DateTime start)
		{
			int mask = WeekdayMask & AllDaysMask;
			int perWeek = CountBits(mask);
			if (perWeek == 0)
			{
				return null;
			}
			long startDay = DayNumber(start);
			long firstWeekStart = WeekStartDay(startDay);
			int inFirstWeek = 0;
			for (long day = startDay; day < firstWeekStart + DaysPerWeek && day <= LastDayNumber; day++)
			{
				if ((mask & MaskOf(DayOfWeekOf(day))) != 0)
				{
					inFirstWeek++;
				}
			}
			if (n <= inFirstWeek)
			{
				return NthMatchInWeek(startDay, mask, n - 1, firstWeekStart + DaysPerWeek);
			}
			int rest = n - inFirstWeek - 1;
			long blocks = rest / perWeek + 1;
			long weekStart = checked(firstWeekStart + blocks * Interval * DaysPerWeek);
			return NthMatchInWeek(weekStart, mask, rest % perWeek, weekStart + DaysPerWeek);
		}

		private static DateTime? NthMatchInWeek(long from, int mask, int zeroBasedIndex, long limit)
		{
			for (long day = from; day < limit && day <= LastDayNumber; day++)
			{
				if ((mask & MaskOf(DayOfWeekOf(day))) != 0 && zeroBasedIndex-- == 0)
				{
					return DateOf(day);
				}
			}
			return null;
		}
	}
}
