using System;

namespace Mordex.Windows.Scheduler
{
	public class ValidatingAppointmentTextEventArgs : EventArgs
	{
		private string _newValue = "";

		private SchedulerAppointment _app;

		private bool _cancel;

		public string ProposedValue => _newValue;

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

		internal ValidatingAppointmentTextEventArgs(SchedulerAppointment app, string newValue)
		{
			_app = app;
			_newValue = newValue;
		}
	}
}
