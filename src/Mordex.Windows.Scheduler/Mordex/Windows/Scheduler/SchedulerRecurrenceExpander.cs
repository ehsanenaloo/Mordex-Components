using System;
using System.Collections.Generic;

namespace Mordex.Windows.Scheduler
{
	/// <summary>Creates the appointments of a recurring appointment for a date range and finds neighbours of a date.</summary>
	internal class SchedulerRecurrenceExpander : RecurrenceCalculator
	{
		private readonly RecurrencePattern _recurrencePattern;

		internal SchedulerRecurrenceExpander(RecurrencePattern pattern)
			: base(pattern, pattern.Scheduler != null ? pattern.Scheduler.FirstDayOfWeek : SchedulerDayOfWeek.Sunday, pattern.Scheduler != null ? pattern.Scheduler.WorkWeek : DefaultWorkWeek)
		{
			_recurrencePattern = pattern;
		}

		private SchedulerAppointment CreateOccurrence(DateTime date)
		{
			DateTime start = date.Add(_recurrencePattern.StartTime);
			return _recurrencePattern.MasterAppointment.CreateOccurrence(start, start.Add(_recurrencePattern.Duration));
		}

		private static bool IsReminderActive(SchedulerAppointment appointment)
		{
			return appointment.Reminder != null && appointment.Reminder.Enabled && appointment.Reminder.GetActive();
		}

		/// <summary>The appointment with the earliest active reminder after the last dismissed one, or null.</summary>
		internal SchedulerAppointment FindNextReminderAppointment()
		{
			DateTime dismissTime = _recurrencePattern.GetEffectiveReminderDismissTime();
			DateTime rangeStart = _recurrencePattern.PatternStartDate;
			if (dismissTime > rangeStart)
			{
				rangeStart = dismissTime;
			}
			DateTime rangeEnd = rangeStart;
			int interval = _recurrencePattern.Interval;
			try
			{
				switch (_recurrencePattern.RecurrenceType)
				{
				case RecurrenceType.Daily:
				case RecurrenceType.Weekly:
					rangeEnd = rangeStart.AddDays(7L * interval + 1);
					break;
				case RecurrenceType.Monthly:
					rangeEnd = rangeStart.AddMonths(interval + 1);
					break;
				default:
					rangeEnd = rangeStart.AddYears(interval + 1);
					break;
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				rangeEnd = DateTime.MaxValue.Date;
			}
			SchedulerAppointment result = null;
			DateTime earliestReminder = DateTime.MaxValue;
			SchedulerAppointment[] occurrences = ExpandRange(new DateRange(rangeStart.Date, AddSaturating(rangeEnd.Date, TimeSpan.FromDays(1))), true);
			if (occurrences != null)
			{
				foreach (SchedulerAppointment occurrence in occurrences)
				{
					RecurrenceException exception = occurrence.RecurrenceState == RecurrenceState.Exception ? occurrence.GetRecurrenceException() : null;
					if ((exception == null || !exception.Deleted) && IsReminderActive(occurrence))
					{
						DateTime reminderTime = occurrence.Reminder.GetReminderDateTime();
						if (reminderTime < earliestReminder && reminderTime > dismissTime)
						{
							earliestReminder = reminderTime;
							result = occurrence;
							break;
						}
					}
				}
			}
			foreach (RecurrenceException exception in _recurrencePattern.Exceptions)
			{
				if (!exception.Deleted && IsReminderActive(exception.Appointment))
				{
					DateTime reminderTime = exception.Appointment.Reminder.GetReminderDateTime();
					if (reminderTime < earliestReminder)
					{
						earliestReminder = reminderTime;
						result = exception.Appointment;
					}
				}
			}
			return result;
		}

		/// <summary>First occurrence or exception appointment that starts on or after the date, or null.</summary>
		internal SchedulerAppointment FindNextOccurrence(DateTime date)
		{
			DateTime recurrenceEnd = GetRecurrenceEnd();
			if (date < _pattern.PatternStartDate)
			{
				date = _pattern.PatternStartDate;
			}
			if (date > recurrenceEnd)
			{
				return null;
			}
			SchedulerAppointment found = null;
			DateTime foundDate = DateTime.MaxValue;
			DateTime? day = _rules.NextOccurrence(date, recurrenceEnd);
			while (day.HasValue)
			{
				if (_recurrencePattern.Exceptions[day.Value] == null)
				{
					found = CreateOccurrence(day.Value);
					found.SetMasterAppointment(_recurrencePattern.MasterAppointment);
					foundDate = day.Value;
					break;
				}
				day = day.Value < DateTime.MaxValue.Date ? _rules.NextOccurrence(day.Value.AddDays(1), recurrenceEnd) : null;
			}
			foreach (RecurrenceException exception in _recurrencePattern.Exceptions)
			{
				if (exception.Deleted)
				{
					continue;
				}
				DateTime exceptionDate = exception.Appointment.StartTime.Date;
				if (exceptionDate < foundDate && exceptionDate >= date)
				{
					found = exception.Appointment;
					foundDate = exceptionDate;
					if (foundDate == date)
					{
						break;
					}
				}
			}
			return found;
		}

		/// <summary>Last occurrence or exception appointment that starts on or before the date, or null.</summary>
		internal SchedulerAppointment FindPreviousOccurrence(DateTime date)
		{
			DateTime recurrenceEnd = GetRecurrenceEnd();
			DateTime limit = date.Date < recurrenceEnd.Date ? date.Date : recurrenceEnd.Date;
			SchedulerAppointment found = null;
			DateTime foundDate = DateTime.MinValue;
			DateTime? day = _rules.PreviousOccurrence(limit);
			while (day.HasValue)
			{
				if (_recurrencePattern.Exceptions[day.Value] == null)
				{
					found = CreateOccurrence(day.Value);
					found.SetMasterAppointment(_recurrencePattern.MasterAppointment);
					foundDate = day.Value;
					break;
				}
				day = day.Value > DateTime.MinValue.Date ? _rules.PreviousOccurrence(day.Value.AddDays(-1)) : null;
			}
			foreach (RecurrenceException exception in _recurrencePattern.Exceptions)
			{
				if (exception.Deleted)
				{
					continue;
				}
				DateTime exceptionDate = exception.Appointment.StartTime.Date;
				if (exceptionDate > foundDate && exceptionDate <= date.Date)
				{
					found = exception.Appointment;
					foundDate = exceptionDate;
				}
			}
			return found;
		}

		internal SchedulerAppointment[] GetOccurrences(DateRange range)
		{
			return ExpandRange(range, true) ?? new SchedulerAppointment[0];
		}

		/// <summary>Occurrences and (optionally) exception appointments that touch the range, or null when there are none.</summary>
		internal SchedulerAppointment[] ExpandRange(DateRange range, bool includeExceptions)
		{
			DateRange requested = range;
			DateTime recurrenceEnd = GetRecurrenceEnd();
			RecurrenceExceptionCollection exceptions = includeExceptions && _recurrencePattern.Exceptions.Count > 0 ? _recurrencePattern.Exceptions.GetExceptionsInRange(range) : null;
			List<SchedulerAppointment> appointments;
			// recurrenceEnd is the day of the last start; a multi-day occurrence can still run into the range.
			DateTime lastOccurrenceEnd = recurrenceEnd == DateTime.MaxValue ? DateTime.MaxValue : AddSaturating(AddSaturating(recurrenceEnd.Date, _pattern.StartTime), _pattern.Duration);
			if (lastOccurrenceEnd < range.Start || range.End <= _pattern.PatternStartDate)
			{
				if (exceptions == null || exceptions.Count == 0)
				{
					return null;
				}
				appointments = new List<SchedulerAppointment>();
			}
			else
			{
				if (range.Start.Add(_pattern.StartTime).Date != range.Start.Add(_pattern.EndTime).Date)
				{
					range = WidenForMultiDayOccurrences(range, recurrenceEnd);
				}
				appointments = ExpandDays(range, recurrenceEnd);
			}
			if (exceptions != null)
			{
				foreach (RecurrenceException exception in exceptions)
				{
					if (!exception.Deleted)
					{
						appointments.Add(exception.Appointment);
					}
				}
			}
			// The search range can be wider than the request; keep only appointments that overlap the request.
			appointments.RemoveAll(appointment => !Overlaps(appointment, requested));
			if (appointments.Count == 0)
			{
				return null;
			}
			foreach (SchedulerAppointment appointment in appointments)
			{
				appointment.SetMasterAppointment(_recurrencePattern.MasterAppointment);
			}
			return appointments.ToArray();
		}

		/// <summary>True when the appointment shares time with [range.Start, range.End). An appointment with no
		/// duration overlaps when it starts inside the range.</summary>
		internal static bool Overlaps(SchedulerAppointment appointment, DateRange range)
		{
			if (appointment.EndTime <= appointment.StartTime)
			{
				return appointment.StartTime >= range.Start && appointment.StartTime < range.End;
			}
			return appointment.StartTime < range.End && appointment.EndTime > range.Start;
		}

		private DateRange WidenForMultiDayOccurrences(DateRange range, DateTime recurrenceEnd)
		{
			range.Start = AddSaturating(range.Start, -_pattern.Duration).Date;
			if (range.Start < _pattern.PatternStartDate)
			{
				range.Start = _pattern.PatternStartDate;
			}
			range.End = AddSaturating(AddSaturating(range.End.Date, _pattern.Duration).Date, TimeSpan.FromDays(1));
			if (range.End > recurrenceEnd)
			{
				range.End = recurrenceEnd;
				if (range.Start.TimeOfDay == TimeSpan.Zero)
				{
					range.End = AddSaturating(range.End, TimeSpan.FromDays(1));
				}
			}
			return range;
		}

		private List<SchedulerAppointment> ExpandDays(DateRange range, DateTime recurrenceEnd)
		{
			var appointments = new List<SchedulerAppointment>();
			DateTime lastDay = LastDayOfRange(range.Start, range.End);
			if (lastDay > recurrenceEnd.Date)
			{
				lastDay = recurrenceEnd.Date;
			}
			foreach (DateTime day in EnumerateOccurrences(range.Start.Date, lastDay))
			{
				if (_recurrencePattern.Exceptions[day] == null)
				{
					appointments.Add(CreateOccurrence(day));
				}
			}
			return appointments;
		}
	}
}
