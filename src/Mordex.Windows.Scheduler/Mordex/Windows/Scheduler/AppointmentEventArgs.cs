using System;

namespace Mordex.Windows.Scheduler
{
	public class AppointmentEventArgs : EventArgs
	{
		private SchedulerAppointment _app;

		public SchedulerAppointment Appointment => _app;

		public AppointmentEventArgs(SchedulerAppointment app)
		{
			_app = app;
		}
	}
}
