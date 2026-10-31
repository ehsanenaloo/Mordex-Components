using System;

namespace Mordex.Windows.Scheduler
{
	public class AppointmentDragEventArgs : EventArgs
	{
		private SchedulerAppointment _app;

		private AppointmentDragType _dragType;

		private bool _cancel;

		public SchedulerAppointment Appointment => _app;

		public AppointmentDragType DragType => _dragType;

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

		internal AppointmentDragEventArgs(SchedulerAppointment app, AppointmentDragType dragType)
		{
			_app = app;
			_dragType = dragType;
		}
	}
}
