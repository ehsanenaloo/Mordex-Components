using System;

namespace Mordex.Windows.Scheduler
{
	public class AddAppointmentEventArgs : EventArgs
	{
		private AddAction _addAction;

		private SchedulerAppointment _app;

		private bool _cancel;

		public AddAction AddAction => _addAction;

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

		internal AddAppointmentEventArgs(SchedulerAppointment app, AddAction addAction)
		{
			_app = app;
			_addAction = addAction;
		}
	}
}
