using System;
using System.Collections.Generic;

namespace Mordex.Windows.Scheduler
{
	/// <summary>
	/// Finds the appointments of one owner that overlap a range. The ranges asked for must move forward:
	/// each start and each end must be at or after the one before. Single appointments are read once in order
	/// of start, so a whole free-time search costs about one pass over them. Each recurring series is expanded
	/// over windows that move forward with the ranges, so a long search expands each part of a series about once.
	/// A window starts no earlier than the range asked for, so time that the search jumps over is never expanded.
	/// </summary>
	internal sealed class SchedulerBusyTimeSearch
	{
		private readonly MxScheduler _scheduler;

		private readonly object _owner;

		private readonly List<SchedulerAppointment> _singles = new List<SchedulerAppointment>();

		// The recurring series in order of the end of the time they are expanded over.
		private readonly PriorityQueue<RecurringSeries, DateTime> _recurring = new PriorityQueue<RecurringSeries, DateTime>();

		// Expanded occurrences that are not read yet, in order of start.
		private readonly PriorityQueue<SchedulerAppointment, DateTime> _occurrences = new PriorityQueue<SchedulerAppointment, DateTime>();

		private int _nextSingle;

		// The latest end of the single appointments that start before the current range end.
		private DateTime _latestSingleEnd = DateTime.MinValue;

		// The latest end of the occurrences of the owner that start before the current range end.
		private DateTime _latestOccurrenceEnd = DateTime.MinValue;

		private static readonly TimeSpan FirstWindow = TimeSpan.FromDays(1.0);

		private static readonly TimeSpan LargestWindow = TimeSpan.FromDays(64.0);

		/// <summary>The number of times a recurring series was expanded. Tests read it to measure the work of a search.</summary>
		internal int Expansions { get; private set; }

		/// <summary>The most occurrences that one expansion gave. Tests read it to measure the work of one expansion.</summary>
		internal int LargestExpansion { get; private set; }

		internal SchedulerBusyTimeSearch(SchedulerAppointmentCollection appointments, object owner)
		{
			_scheduler = appointments.Scheduler;
			_owner = owner;
			foreach (SchedulerAppointment item in appointments._sortedAppointments)
			{
				if (item.IsRecurring)
				{
					_recurring.Enqueue(new RecurringSeries(item.RecurrencePattern), DateTime.MinValue);
				}
				else if (SchedulerAppointmentCollection.MatchesOwner(item, owner))
				{
					_singles.Add(item);
				}
			}
			_singles.Sort((first, second) => first.StartTime.CompareTo(second.StartTime));
		}

		/// <summary>Returns the latest end of the appointments that overlap the range, or null when none does.</summary>
		internal DateTime? FindBusyUntil(DateTime start, DateTime end)
		{
			bool isEmptyRange = start == end;
			while (_nextSingle < _singles.Count)
			{
				DateTime itemStart = _singles[_nextSingle].StartTime;
				if (itemStart > end || (itemStart == end && !isEmptyRange))
				{
					break;
				}
				DateTime itemEnd = _singles[_nextSingle].EndTime;
				if (itemEnd > _latestSingleEnd)
				{
					_latestSingleEnd = itemEnd;
				}
				_nextSingle++;
			}
			// Every single read so far starts before the range end, so it overlaps exactly when it ends after the start.
			DateTime? busyUntil = _latestSingleEnd > start ? _latestSingleEnd : null;
			if (_scheduler != null)
			{
				ReadOccurrences(start, end);
				if (_latestOccurrenceEnd > start && (busyUntil == null || _latestOccurrenceEnd > busyUntil.Value))
				{
					busyUntil = _latestOccurrenceEnd;
				}
			}
			return busyUntil;
		}

		// An occurrence overlaps a range exactly when it starts before the range end and ends after the range start,
		// so the occurrences are read like the single appointments. Every occurrence that overlaps a later range
		// overlaps one of the windows, because the windows cover the time from each range start to its end. A window
		// starts at the range start when the earlier windows end before it: an expansion gives every occurrence that
		// overlaps the window, also one that starts earlier, so the time between is not needed.
		private void ReadOccurrences(DateTime start, DateTime end)
		{
			while (_recurring.TryPeek(out RecurringSeries series, out DateTime expandedUntil) && expandedUntil < end)
			{
				_recurring.Dequeue();
				DateTime windowStart = expandedUntil > start ? expandedUntil : start;
				DateTime windowEnd = RecurrenceCalculator.AddSaturating(windowStart, series.Window);
				if (windowEnd < end)
				{
					windowEnd = end;
				}
				Expansions++;
				SchedulerAppointment[] occurrences = series.Pattern.GetOccurrences(new DateRange(windowStart, windowEnd));
				if (occurrences.Length > LargestExpansion)
				{
					LargestExpansion = occurrences.Length;
				}
				foreach (SchedulerAppointment occurrence in occurrences)
				{
					_occurrences.Enqueue(occurrence, occurrence.StartTime);
				}
				series.Window = series.Window < LargestWindow ? series.Window + series.Window : LargestWindow;
				_recurring.Enqueue(series, windowEnd);
			}
			while (_occurrences.TryPeek(out SchedulerAppointment occurrence, out DateTime occurrenceStart) && occurrenceStart < end)
			{
				_occurrences.Dequeue();
				if (occurrence.EndTime > _latestOccurrenceEnd && SchedulerAppointmentCollection.MatchesOwner(occurrence, _owner) && _scheduler.MatchesFilter(occurrence))
				{
					_latestOccurrenceEnd = occurrence.EndTime;
				}
			}
		}

		private sealed class RecurringSeries
		{
			internal readonly RecurrencePattern Pattern;

			internal TimeSpan Window = FirstWindow;

			internal RecurringSeries(RecurrencePattern pattern)
			{
				Pattern = pattern;
			}
		}
	}
}
