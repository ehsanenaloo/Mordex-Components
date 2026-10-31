using System;

namespace Mordex.Windows.Scheduler
{
	/// <summary>Answers working-time questions and finds free time for a scheduler.</summary>
	internal sealed class SchedulerTimeSlots
	{
		private readonly MxScheduler _scheduler;

		internal SchedulerTimeSlots(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		private TimeSpan SlotLength => new TimeSpan(0, _scheduler.MinuteInterval, 0);

		/// <summary>The number of slots checked so far. Tests read it to measure the work of a search.</summary>
		internal long SlotChecks { get; set; }

		/// <summary>
		/// The most occurrences that one expansion of a recurring series gave in the free-time searches so far. Tests
		/// read it to measure the work of a search.
		/// </summary>
		internal int LargestExpansion { get; set; }

		/// <summary>
		/// True when the slot is working time. An hour range of the owner that covers the slot decides first, and a closing range that covers part of it closes it;
		/// otherwise the scheduler work days and work hours decide.
		/// </summary>
		internal bool IsWorkingTimeSlot(SchedulerHourRangeCollection hourRanges, DateTime start, TimeSpan length)
		{
			SlotChecks++;
			DateRange slot = new DateRange(start, start.Add(length));
			if (hourRanges != null)
			{
				foreach (SchedulerHourRange hourRange in hourRanges)
				{
					// An empty range covers no time, so it cannot decide a slot, not even a point at its own time.
					if (hourRange.StartTime >= hourRange.EndTime)
					{
						continue;
					}
					if (SchedulerRenderer.IsWithinWorkingHours(slot, hourRange.DayOfWeek, hourRange))
					{
						return hourRange.IsWorkingTime;
					}
					// A closing range closes a slot that it only partly covers too.
					if (!hourRange.IsWorkingTime && OverlapsRange(start, length, hourRange))
					{
						return false;
					}
				}
			}
			// Times of day are compared so that a slot on the last day of the calendar cannot overflow.
			TimeSpan timeOfDay = start.TimeOfDay;
			return timeOfDay >= _scheduler.WorkStartTime
				&& timeOfDay + length <= _scheduler.WorkEndTime
				&& (SchedulerRenderer.ToSchedulerDayOfWeek(start.DayOfWeek) & _scheduler.WorkWeek) != 0;
		}

		private static bool OverlapsRange(DateTime start, TimeSpan length, SchedulerHourRange hourRange)
		{
			if ((hourRange.DayOfWeek & SchedulerRenderer.ToSchedulerDayOfWeek(start.DayOfWeek)) == 0)
			{
				return false;
			}
			TimeSpan timeOfDay = start.TimeOfDay;
			return timeOfDay < hourRange.EndTime && timeOfDay + length > hourRange.StartTime;
		}

		internal bool IsWorkingTime(DateTime startTime, DateTime endTime, SchedulerAppointmentOwner owner)
		{
			return FirstNonWorkingSlot(new SchedulerDayHourRanges(_scheduler, owner), startTime, endTime - startTime) == null;
		}

		// Returns the start of the first piece in the span that is not working time, or null when every piece is.
		// A piece is one slot at most and ends at midnight at the latest, so each piece is checked with the rules of
		// the day it lies in. The last piece covers only the part of the span that is left. The span must end at or
		// before DateTime.MaxValue.
		internal DateTime? FirstNonWorkingSlot(SchedulerDayHourRanges days, DateTime start, TimeSpan length)
		{
			TimeSpan slotLength = SlotLength;
			DateTime slot = start;
			TimeSpan remaining = length;
			while (remaining > TimeSpan.Zero)
			{
				TimeSpan piece = remaining < slotLength ? remaining : slotLength;
				TimeSpan toMidnight = TimeSpan.FromDays(1.0) - slot.TimeOfDay;
				if (toMidnight < piece)
				{
					piece = toMidnight;
				}
				if (!IsWorkingTimeSlot(days.For(slot), slot, piece))
				{
					return slot;
				}
				slot = slot.Add(piece);
				remaining -= piece;
			}
			return null;
		}

		/// <summary>
		/// Returns the first start at or after <paramref name="from"/>, on or before the MaxDate day, where the whole
		/// span is working time and ends at or before <see cref="DateTime.MaxValue"/>. Returns null when there is none, or
		/// when the calendar has no short period and no start lies within 400 years after the last dated rule.
		/// </summary>
		internal DateTime? FindWorkingTimeStart(DateTime from, TimeSpan length, SchedulerAppointmentOwner owner)
		{
			return new SchedulerWorkingTimeSearch(this, _scheduler, length, owner).Find(from);
		}

		/// <summary>
		/// Returns the first start at or after <paramref name="proposedDate"/>, on or before the MaxDate day, where no
		/// appointment of the owner overlaps the span. Returns <see cref="DateTime.MaxValue"/> when there is no such start.
		/// </summary>
		internal DateTime FindFreeTimeSlot(DateTime proposedDate, TimeSpan duration, bool onlyWorkingTime, SchedulerAppointmentOwner owner)
		{
			SchedulerBusyTimeSearch busyTime = new SchedulerBusyTimeSearch(_scheduler.Appointments, owner?.Value);
			SchedulerWorkingTimeSearch workingTime = onlyWorkingTime ? new SchedulerWorkingTimeSearch(this, _scheduler, duration, owner) : null;
			try
			{
				DateTime candidate = proposedDate;
				while (candidate.Date <= _scheduler.MaxDate)
				{
					if (workingTime != null)
					{
						DateTime? workingStart = workingTime.Find(candidate);
						if (workingStart == null)
						{
							break;
						}
						candidate = workingStart.Value;
					}
					if (duration > DateTime.MaxValue - candidate)
					{
						break;
					}
					DateTime? busyUntil = busyTime.FindBusyUntil(candidate, candidate.Add(duration));
					if (busyUntil == null)
					{
						return candidate;
					}
					// An overlapping appointment ends after the candidate, so this always moves forward.
					candidate = busyUntil.Value;
				}
				return DateTime.MaxValue;
			}
			finally
			{
				LargestExpansion = Math.Max(LargestExpansion, busyTime.LargestExpansion);
			}
		}
	}
}
