using System;

namespace Mordex.Windows.Scheduler
{
	public class AppointmentCopiedEventArgs : EventArgs
	{
		private SchedulerAppointment _sourceApp;

		private SchedulerAppointment _copyApp;

		public SchedulerAppointment SourceAppointment => _sourceApp;

		public SchedulerAppointment CopyAppointment => _copyApp;

		internal AppointmentCopiedEventArgs(SchedulerAppointment sourceApp, SchedulerAppointment copyApp)
		{
			_sourceApp = sourceApp;
			_copyApp = copyApp;
		}
	}
}
