using System;

namespace Mordex.Windows.Scheduler
{
	public class DroppingAppointmentEventArgs : EventArgs
	{
		private DateTime _newStart = DateTime.MinValue;

		private DateTime _newEnd = DateTime.MinValue;

		private object _newOwner;

		private SchedulerAppointment _app;

		private bool _cancel;

		public DateTime ProposedStartTime => _newStart;

		public DateTime ProposedEndTime => _newEnd;

		public object ProposedOwner => _newOwner;

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

		internal DroppingAppointmentEventArgs(SchedulerAppointment app, DateTime newStart, DateTime newEnd, object newOwner)
		{
			_app = app;
			_newEnd = newEnd;
			_newStart = newStart;
			_newOwner = newOwner;
		}
	}
}
