using System;

namespace Mordex.Windows.Scheduler
{
	/// <summary>
	/// Gives the hour ranges of one owner for one day at a time and keeps the last day it loaded. A range in a
	/// collection applies to every day of its week days, so the dated exceptions and pattern occurrences in a
	/// collection are right only for the day it was loaded for. A check that crosses midnight asks again.
	/// </summary>
	internal sealed class SchedulerDayHourRanges
	{
		private readonly MxScheduler _scheduler;

		private readonly SchedulerAppointmentOwner _owner;

		private DateTime _date;

		private SchedulerHourRangeCollection _ranges;

		internal SchedulerDayHourRanges(MxScheduler scheduler, SchedulerAppointmentOwner owner)
		{
			_scheduler = scheduler;
			_owner = owner;
		}

		/// <summary>The hour ranges for the day that holds the time.</summary>
		internal SchedulerHourRangeCollection For(DateTime time)
		{
			DateTime date = time.Date;
			if (_ranges == null || _date != date)
			{
				DateTime end = date < DateTime.MaxValue.Date ? date.AddDays(1.0) : DateTime.MaxValue;
				_ranges = _scheduler.GetHourRangesForOwner(new DateRange(date, end), _owner);
				_date = date;
			}
			return _ranges;
		}
	}
}
