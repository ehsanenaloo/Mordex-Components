using System;
using System.Collections.Generic;

namespace Mordex.Windows.Scheduler
{
	/// <summary>
	/// Finds the first start where a span of one length is working time for one owner. It gives the same start as
	/// trying every slot in turn, but it jumps over slots whose result cannot change, it stops at once when no working
	/// run is long enough, and it stops when the calendar repeats without a fit. When the calendar has no period that
	/// is short enough, it stops at the day from which no run of the week day rules and the patterns can be long enough, or after 400 years.
	/// </summary>
	/// <remarks>
	/// After the first candidate every candidate is on the slot grid of its day. A start on the grid checks slot pieces
	/// that each lie inside one day, so a fitting span lies inside the working time that the base rules and the
	/// working hour ranges can give. Hour ranges that are not working time can only take working time away. Each piece
	/// is checked with the hour ranges of its own day, so a span that crosses midnight sees the dated rules of each day.
	/// </remarks>
	internal sealed class SchedulerWorkingTimeSearch
	{
		// 400 Gregorian years: the calendar, with leap years and week days, repeats after this many days.
		private const long GregorianCycleDays = 146097;

		private const int GregorianCycleMonths = 4800;

		private const int GregorianCycleYears = 400;

		// Longer periods are not used: the set of seen days would grow too large.
		private const long LargestPeriodDays = 2 * GregorianCycleDays;

		private static readonly TimeSpan OneDay = TimeSpan.FromDays(1.0);

		private readonly SchedulerTimeSlots _timeSlots;

		private readonly MxScheduler _scheduler;

		private readonly SchedulerAppointmentOwner _owner;

		private readonly TimeSpan _length;

		private readonly TimeSpan _slotLength;

		// The length of the first slot piece of a span.
		private readonly TimeSpan _firstPiece;

		private readonly SchedulerDayHourRanges _days;

		private TimeSpan? _longestRun;

		private long? _longestRunAfterRules;

		private RepeatingCalendar _repeating;

		internal SchedulerWorkingTimeSearch(SchedulerTimeSlots timeSlots, MxScheduler scheduler, TimeSpan length, SchedulerAppointmentOwner owner)
		{
			_timeSlots = timeSlots;
			_scheduler = scheduler;
			_owner = owner;
			_length = length;
			_slotLength = new TimeSpan(0, scheduler.MinuteInterval, 0);
			_firstPiece = length < _slotLength ? length : _slotLength;
			_days = new SchedulerDayHourRanges(scheduler, owner);
		}

		/// <summary>
		/// Returns the first start at or after <paramref name="from"/>, on or before the MaxDate day, where the whole
		/// span is working time and ends at or before <see cref="DateTime.MaxValue"/>. Returns null when there is none.
		/// </summary>
		internal DateTime? Find(DateTime from)
		{
			if (!IsAllowedStart(from))
			{
				return null;
			}
			DateTime? blocked = from.TimeOfDay.Ticks % _slotLength.Ticks == 0 ? FirstNonWorkingSlot(from) : _timeSlots.FirstNonWorkingSlot(_days, from, _length);
			if (blocked == null)
			{
				return from;
			}
			if (blocked.Value.Date > _scheduler.MaxDate || LongestRun < _length)
			{
				return null;
			}
			DateTime? candidate = NextGridPoint(blocked.Value);
			RepeatingCalendar repeating = Repeating;
			HashSet<long> seen = null;
			DateTime lastSeenDate = DateTime.MinValue;
			while (candidate != null && IsAllowedStart(candidate.Value))
			{
				DateTime start = candidate.Value;
				if (start.Date >= repeating.From && !repeating.Repeats)
				{
					// The calendar has no short period, so the walk could go on to MaxDate. It stops after a whole Gregorian
					// cycle of days, which is longer than the period of every pattern that has a short one.
					if ((start.Date - Later(repeating.From, from.Date)).TotalDays > GregorianCycleDays)
					{
						return null;
					}
					// From here on every span lies in days that the week day rules and the patterns without an end
					// decide, so it needs a run of full slots that those days can make.
					if (_length.Ticks / _slotLength.Ticks * _slotLength.Ticks > LongestRunAfterRules)
					{
						return null;
					}
				}
				else if (start.Date >= repeating.From && start.Date > lastSeenDate)
				{
					// From here on the calendar repeats with the period, and everything after the first candidate of a
					// day follows from that candidate alone. So when the first candidate of a day sits at the same place
					// in the period as an earlier one, the search would go round the same period for ever.
					seen ??= new HashSet<long>();
					if (!seen.Add(start.Ticks % repeating.PeriodTicks))
					{
						return null;
					}
					lastSeenDate = start.Date;
				}
				blocked = FirstNonWorkingSlot(start);
				if (blocked == null)
				{
					return start;
				}
				if (blocked.Value == start)
				{
					candidate = NextChange(_days.For(start), start, _firstPiece);
				}
				else if (blocked.Value.Date > _scheduler.MaxDate)
				{
					return null;
				}
				else
				{
					candidate = NextGridPoint(blocked.Value);
				}
			}
			return null;
		}

		private bool IsAllowedStart(DateTime start)
		{
			return start.Date <= _scheduler.MaxDate && _length <= DateTime.MaxValue - start;
		}

		// The first grid point after the slot that holds the time, or null past the end of the calendar.
		private DateTime? NextGridPoint(DateTime time)
		{
			long slotTicks = _slotLength.Ticks;
			long next = time.TimeOfDay.Ticks / slotTicks * slotTicks + slotTicks;
			return AtTimeOfDay(time.Date, next);
		}

		private static DateTime? AtTimeOfDay(DateTime date, long ticks)
		{
			if (DateTime.MaxValue.Ticks - date.Ticks < ticks)
			{
				return null;
			}
			return date.AddTicks(ticks);
		}

		/// <summary>
		/// Does what <see cref="SchedulerTimeSlots.FirstNonWorkingSlot"/> does for a start on the slot grid, but checks
		/// one full slot for each stretch where the result of a full slot cannot change. A stretch ends at midnight at
		/// the latest, so each day is checked with its own hour ranges.
		/// </summary>
		private DateTime? FirstNonWorkingSlot(DateTime start)
		{
			long slotTicks = _slotLength.Ticks;
			DateTime slot = start;
			TimeSpan remaining = _length;
			while (remaining > TimeSpan.Zero)
			{
				SchedulerHourRangeCollection ranges = _days.For(slot);
				TimeSpan piece = remaining < _slotLength ? remaining : _slotLength;
				if (!_timeSlots.IsWorkingTimeSlot(ranges, slot, piece))
				{
					return slot;
				}
				if (piece < _slotLength)
				{
					break;
				}
				// The full slots from here up to the next change are working time too.
				long pieces = remaining.Ticks / slotTicks;
				DateTime? change = NextChange(ranges, slot, _slotLength);
				if (change != null)
				{
					pieces = Math.Min(pieces, (change.Value - slot).Ticks / slotTicks);
				}
				slot = slot.AddTicks(pieces * slotTicks);
				remaining -= new TimeSpan(pieces * slotTicks);
			}
			return null;
		}

		/// <summary>
		/// Returns the next grid point after <paramref name="start"/> where a piece of the given length can give
		/// another result, or the next midnight. The piece at a grid point is working time when it lies inside a range;
		/// that can change only where a range starts, or just after the last point where the piece still ends inside it.
		/// </summary>
		private DateTime? NextChange(SchedulerHourRangeCollection ranges, DateTime start, TimeSpan piece)
		{
			long now = start.TimeOfDay.Ticks;
			long next = OneDay.Ticks;
			if (ranges != null)
			{
				foreach (SchedulerHourRange range in ranges)
				{
					next = EarliestChange(next, now, piece, range.StartTime, range.EndTime);
				}
			}
			next = EarliestChange(next, now, piece, _scheduler.WorkStartTime, _scheduler.WorkEndTime);
			return AtTimeOfDay(start.Date, next);
		}

		private long EarliestChange(long next, long now, TimeSpan piece, TimeSpan rangeStart, TimeSpan rangeEnd)
		{
			long slotTicks = _slotLength.Ticks;
			if (rangeStart.Ticks > now && rangeStart.Ticks < next)
			{
				long on = (rangeStart.Ticks + slotTicks - 1) / slotTicks * slotTicks;
				next = Math.Min(next, on);
			}
			if (rangeEnd >= piece && rangeEnd - piece < OneDay)
			{
				long off = (rangeEnd - piece).Ticks / slotTicks * slotTicks + slotTicks;
				if (off > now && off < next)
				{
					next = off;
				}
			}
			// A closing range that a piece only partly covers closes it: that starts at the first piece that ends after
			// the range starts, and stops at the first piece that starts at or after the range end.
			if (rangeStart.Ticks >= piece.Ticks)
			{
				long reach = (rangeStart.Ticks - piece.Ticks) / slotTicks * slotTicks + slotTicks;
				if (reach > now && reach < next)
				{
					next = reach;
				}
			}
			long clear = (rangeEnd.Ticks + slotTicks - 1) / slotTicks * slotTicks;
			if (clear > now && clear < next)
			{
				next = clear;
			}
			return next;
		}

		/// <summary>
		/// The day from which the hour ranges of each day depend only on its week day and on the patterns without an end,
		/// and the period after which they repeat. Exceptions and patterns that end only delay that day. A pattern without
		/// an end repeats with its own period; the period of the calendar is the least common multiple of a week and those
		/// periods.
		/// </summary>
		private RepeatingCalendar Repeating
		{
			get
			{
				if (_repeating == null)
				{
					RepeatingCalendar repeating = new RepeatingCalendar();
					AddRules(repeating, _scheduler.WorkingHourSchema);
					if (_owner != null)
					{
						AddRules(repeating, _owner.WorkingHourSchema);
					}
					_repeating = repeating;
				}
				return _repeating;
			}
		}

		private static void AddRules(RepeatingCalendar repeating, WorkingHoursSchema schema)
		{
			foreach (WorkingHourException exception in schema.Exceptions)
			{
				// An exception applies to a day only while its date range ends after the start of that day.
				DateTime end = exception.DateRange.End;
				repeating.StartAfter(end.TimeOfDay == TimeSpan.Zero ? end : NextDay(end));
			}
			foreach (SchedulerHourRange range in schema.WorkingHoursRange)
			{
				WorkingHourRecurrencePattern pattern = range.RecurrencePattern;
				if (pattern == null)
				{
					continue;
				}
				DateTime end = pattern.GetRecurrenceEnd();
				if (end < DateTime.MaxValue)
				{
					repeating.StartAfter(NextDay(end));
				}
				else
				{
					repeating.StartAfter(pattern.PatternStartDate.Date);
					repeating.AddPeriod(PeriodDays(pattern));
				}
			}
		}

		/// <summary>
		/// The number of days after which the occurrence days of a pattern repeat, from its start date on, or 0 when
		/// it is not known. Months and years repeat with the 400-year Gregorian cycle, which is a whole number of weeks.
		/// </summary>
		private static long PeriodDays(WorkingHourRecurrencePattern pattern)
		{
			long interval = Math.Max(1, pattern.Interval);
			switch (pattern.RecurrenceType)
			{
			case RecurrenceType.Daily:
				return pattern.DailyRecurrenceMode == DailyRecurrenceMode.UseInterval ? interval : 7;
			case RecurrenceType.Weekly:
				return 7 * interval;
			case RecurrenceType.Monthly:
			case RecurrenceType.MontNth:
				return GregorianCycleDays * (interval / GreatestCommonDivisor(interval, GregorianCycleMonths));
			case RecurrenceType.Yearly:
			case RecurrenceType.YearNth:
				return GregorianCycleDays * (interval / GreatestCommonDivisor(interval, GregorianCycleYears));
			default:
				return 0;
			}
		}

		private static long GreatestCommonDivisor(long first, long second)
		{
			while (second != 0)
			{
				long rest = first % second;
				first = second;
				second = rest;
			}
			return first;
		}

		private sealed class RepeatingCalendar
		{
			internal DateTime From = DateTime.MinValue;

			// False when a pattern has no known period or the period is too long to use.
			internal bool Repeats = true;

			private long _periodDays = 7;

			internal long PeriodTicks => _periodDays * TimeSpan.TicksPerDay;

			internal void StartAfter(DateTime day)
			{
				From = Later(From, day);
			}

			internal void AddPeriod(long periodDays)
			{
				if (periodDays <= 0 || periodDays > LargestPeriodDays)
				{
					Repeats = false;
					return;
				}
				long combined = _periodDays / GreatestCommonDivisor(_periodDays, periodDays) * periodDays;
				if (combined > LargestPeriodDays)
				{
					Repeats = false;
					return;
				}
				_periodDays = combined;
			}
		}

		private static DateTime NextDay(DateTime time)
		{
			return time.Date < DateTime.MaxValue.Date ? time.Date.AddDays(1.0) : DateTime.MaxValue;
		}

		private static DateTime Later(DateTime first, DateTime second)
		{
			return first > second ? first : second;
		}

		/// <summary>
		/// The longest stretch that any mix of the base rules and the working hour ranges can make working time, or
		/// <see cref="TimeSpan.MaxValue"/> when it has no end. A span on the slot grid that is longer cannot fit.
		/// </summary>
		private TimeSpan LongestRun
		{
			get
			{
				if (_longestRun == null)
				{
					_longestRun = FindLongestRun();
				}
				return _longestRun.Value;
			}
		}

		private TimeSpan FindLongestRun()
		{
			List<long[]>[] days = new List<long[]>[7];
			for (int day = 0; day < 7; day++)
			{
				days[day] = new List<long[]>();
			}
			AddWorkingTime(days, _scheduler.WorkWeek, _scheduler.WorkStartTime, _scheduler.WorkEndTime);
			AddWorkingRanges(days, _scheduler.WorkingHourSchema);
			if (_owner != null)
			{
				AddWorkingRanges(days, _owner.WorkingHourSchema);
			}
			bool everyDayIsFull = true;
			for (int day = 0; day < 7; day++)
			{
				days[day].Sort((first, second) => first[0].CompareTo(second[0]));
				everyDayIsFull &= CoversWholeDay(days[day]);
			}
			if (everyDayIsFull)
			{
				return TimeSpan.MaxValue;
			}
			// A run that is not endless is shorter than eight days, so three weeks hold each one in full.
			long longest = 0;
			long runStart = 0;
			long runEnd = -1;
			for (int index = 0; index < 21; index++)
			{
				long dayStart = index * TimeSpan.TicksPerDay;
				foreach (long[] interval in days[index % 7])
				{
					long start = dayStart + interval[0];
					long end = dayStart + interval[1];
					if (start > runEnd)
					{
						runStart = start;
					}
					runEnd = Math.Max(runEnd, end);
					longest = Math.Max(longest, runEnd - runStart);
				}
			}
			return new TimeSpan(longest);
		}

		/// <summary>
		/// The longest run of full working slots on the grid, in ticks, that days from <see cref="RepeatingCalendar.From"/>
		/// on can make, or <see cref="long.MaxValue"/> when it has no end or is not worked out. From that day on the hour
		/// ranges of a day are its week day ranges and the occurrences of the patterns without an end on that day. Each week day
		/// is worked out with every opening pattern present and every closing pattern absent, so the run is at least as long
		/// as any real one.
		/// </summary>
		private long LongestRunAfterRules
		{
			get
			{
				if (_longestRunAfterRules == null)
				{
					_longestRunAfterRules = FindLongestRunAfterRules();
				}
				return _longestRunAfterRules.Value;
			}
		}

		private long FindLongestRunAfterRules()
		{
			// The ranges in the order the hour ranges of a day list them: those of the owner come first.
			List<SchedulerHourRange> ranges = new List<SchedulerHourRange>();
			if (_owner != null)
			{
				AddRulesAfterEnd(ranges, _owner.WorkingHourSchema);
			}
			AddRulesAfterEnd(ranges, _scheduler.WorkingHourSchema);
			int slots = (int)(TimeSpan.TicksPerDay / _slotLength.Ticks);
			int[] prefix = new int[7];
			int[] suffix = new int[7];
			bool[] full = new bool[7];
			int longest = 0;
			for (int day = 0; day < 7; day++)
			{
				// 2 January 2000 is a Sunday.
				DateTime date = new DateTime(2000, 1, 2).AddDays(day);
				SchedulerDayOfWeek dayOfWeek = SchedulerRenderer.ToSchedulerDayOfWeek(date.DayOfWeek);
				// Patterns that open time are all present and patterns that close time are all absent: that gives the most
				// working time that any mix of them can give, so one mix is enough.
				{
					SchedulerHourRangeCollection dayRanges = new SchedulerHourRangeCollection(null);
					foreach (SchedulerHourRange range in ranges)
					{
						WorkingHourRecurrencePattern recurrence = range.RecurrencePattern;
						if (recurrence != null)
						{
							if (range.IsWorkingTime)
							{
								dayRanges.Add(range.CloneWithDayAndTimes(dayOfWeek, recurrence.StartTime, recurrence.EndTime));
							}
						}
						else if ((range.DayOfWeek & dayOfWeek) != 0)
						{
							dayRanges.Add(range);
						}
					}
					int run = 0;
					int first = -1;
					for (int slot = 0; slot < slots; slot++)
					{
						if (_timeSlots.IsWorkingTimeSlot(dayRanges, date.AddTicks(slot * _slotLength.Ticks), _slotLength))
						{
							run++;
							longest = Math.Max(longest, run);
						}
						else
						{
							if (first < 0)
							{
								first = slot;
							}
							run = 0;
						}
					}
					if (first < 0)
					{
						full[day] = true;
					}
					else
					{
						prefix[day] = Math.Max(prefix[day], first);
						suffix[day] = Math.Max(suffix[day], run);
					}
				}
			}
			int partDay = Array.IndexOf(full, false);
			if (partDay < 0)
			{
				return long.MaxValue;
			}
			// Go once round the week from a day that is never full: a run that ends at a midnight starts after it.
			long endingRun = suffix[partDay];
			long longestRun = longest;
			for (int step = 1; step <= 7; step++)
			{
				int day = (partDay + step) % 7;
				longestRun = Math.Max(longestRun, endingRun + prefix[day]);
				endingRun = full[day] ? endingRun + slots : suffix[day];
				longestRun = Math.Max(longestRun, endingRun);
			}
			return longestRun * _slotLength.Ticks;
		}

		// Adds the ranges that still apply after the rules with an end are over: patterns that end are left out.
		private static void AddRulesAfterEnd(List<SchedulerHourRange> ranges, WorkingHoursSchema schema)
		{
			foreach (SchedulerHourRange range in schema.WorkingHoursRange)
			{
				if (range.RecurrencePattern == null || range.RecurrencePattern.GetRecurrenceEnd() == DateTime.MaxValue)
				{
					ranges.Add(range);
				}
			}
		}

		private static bool CoversWholeDay(List<long[]> intervals)
		{
			long covered = 0;
			foreach (long[] interval in intervals)
			{
				if (interval[0] > covered)
				{
					return false;
				}
				covered = Math.Max(covered, interval[1]);
			}
			return covered >= TimeSpan.TicksPerDay;
		}

		private static void AddWorkingRanges(List<long[]>[] days, WorkingHoursSchema schema)
		{
			foreach (WorkingHourException exception in schema.Exceptions)
			{
				SchedulerHourRange range = exception.HourRange;
				if (range != null && range.IsWorkingTime)
				{
					AddWorkingTime(days, range.DayOfWeek, range.StartTime, range.EndTime);
				}
			}
			foreach (SchedulerHourRange range in schema.WorkingHoursRange)
			{
				if (!range.IsWorkingTime)
				{
					continue;
				}
				WorkingHourRecurrencePattern pattern = range.RecurrencePattern;
				if (pattern != null)
				{
					// An occurrence can fall on any day and takes its times from the pattern.
					AddWorkingTime(days, (SchedulerDayOfWeek)0x7F, pattern.StartTime, pattern.EndTime);
				}
				else
				{
					AddWorkingTime(days, range.DayOfWeek, range.StartTime, range.EndTime);
				}
			}
		}

		private static void AddWorkingTime(List<long[]>[] days, SchedulerDayOfWeek dayOfWeek, TimeSpan start, TimeSpan end)
		{
			long startTicks = Math.Max(start.Ticks, 0);
			long endTicks = Math.Min(end.Ticks, TimeSpan.TicksPerDay);
			if (endTicks <= startTicks)
			{
				return;
			}
			for (int day = 0; day < 7; day++)
			{
				if ((SchedulerRenderer.ToSchedulerDayOfWeek((DayOfWeek)day) & dayOfWeek) != 0)
				{
					days[day].Add(new[] { startTicks, endTicks });
				}
			}
		}
	}
}
