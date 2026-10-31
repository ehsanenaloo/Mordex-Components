using System;

namespace Mordex.Windows.Scheduler
{
	public class AppointmentChangeEventArgs : EventArgs
	{
		private SchedulerAppointment _app;

		private AppointmentChange _propertyChanged;

		public SchedulerAppointment Appointment => _app;

		public AppointmentChange PropertyChanged => _propertyChanged;

		internal AppointmentChangeEventArgs(SchedulerAppointment app, AppointmentChange propertyChanged)
		{
			_app = app;
			_propertyChanged = propertyChanged;
		}
	}
}
