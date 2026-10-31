namespace Mordex.Windows.Scheduler
{
	public class AppointmentCopiedCancelEventArgs : AppointmentCopiedEventArgs
	{
		private bool _cancel;

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

		internal AppointmentCopiedCancelEventArgs(SchedulerAppointment sourceApp, SchedulerAppointment copyApp)
			: base(sourceApp, copyApp)
		{
		}
	}
}
