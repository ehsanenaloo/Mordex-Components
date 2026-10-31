using System;

namespace Mordex.Windows.Scheduler
{
	public class AppointmentCancelEventArgs : EventArgs
	{
		internal SchedulerAppointment _app;

		private bool _cancel;

		public SchedulerAppointment Appointment => _app;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		internal AppointmentCancelEventArgs(SchedulerAppointment app)
		{
			_app = app;
		}
	}
}
