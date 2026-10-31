using System;

namespace Mordex.Windows.Scheduler
{
	public class RemovingRecurrentAppointmentEventArgs : EventArgs
	{
		private SchedulerAppointment _app;

		private bool _cancel;

		private bool _deleteTheSeries;

		public SchedulerAppointment RecurrentAppointment => _app;

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

		public bool DeleteTheSeries
		{
			get
			{
				return _deleteTheSeries;
			}
			set
			{
				_deleteTheSeries = value;
			}
		}

		internal RemovingRecurrentAppointmentEventArgs(SchedulerAppointment app)
		{
			_app = app;
		}
	}
}
