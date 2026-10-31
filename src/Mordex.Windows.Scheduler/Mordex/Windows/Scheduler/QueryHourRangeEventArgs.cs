using System;

namespace Mordex.Windows.Scheduler
{
	public class QueryHourRangeEventArgs : EventArgs
	{
		private DateTime _date;

		private SchedulerAppointmentOwner _owner;

		private SchedulerHourRangeCollection _hourRanges;

		public DateTime Date => _date;

		public SchedulerAppointmentOwner Owner => _owner;

		public SchedulerHourRangeCollection HourRanges => _hourRanges;

		internal QueryHourRangeEventArgs(SchedulerHourRangeCollection hourRanges, DateTime date, SchedulerAppointmentOwner owner)
		{
			_hourRanges = hourRanges;
			_date = date;
			_owner = owner;
		}
	}
}
