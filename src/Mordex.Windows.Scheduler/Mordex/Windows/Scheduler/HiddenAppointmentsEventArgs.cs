using System;

namespace Mordex.Windows.Scheduler
{
	public class HiddenAppointmentsEventArgs : EventArgs
	{
		private DateTime _date;

		private SchedulerView _view;

		public DateTime Date => _date;

		public SchedulerView SchedulerView => _view;

		internal HiddenAppointmentsEventArgs(DateTime date, SchedulerView view)
		{
			_date = date;
			_view = view;
		}
	}
}
