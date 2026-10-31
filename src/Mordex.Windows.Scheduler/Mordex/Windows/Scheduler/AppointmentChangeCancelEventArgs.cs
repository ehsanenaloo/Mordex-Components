using System;

namespace Mordex.Windows.Scheduler
{
	public class AppointmentChangeCancelEventArgs : EventArgs
	{
		private object _proposedValue;

		private SchedulerAppointment _app;

		private AppointmentChange _propertyChanged;

		private bool _cancel;

		public object ProposedValue
		{
			get
			{
				return _proposedValue;
			}
			set
			{
				_proposedValue = value;
			}
		}

		public SchedulerAppointment Appointment => _app;

		public AppointmentChange PropertyChanged => _propertyChanged;

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

		internal AppointmentChangeCancelEventArgs(SchedulerAppointment app, AppointmentChange propertyChanged, object newValue)
		{
			_app = app;
			_propertyChanged = propertyChanged;
			_proposedValue = newValue;
		}
	}
}
