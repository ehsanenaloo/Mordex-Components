using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler.Design
{
	internal class SchedulerAboutDialog : MxAboutDialog
	{
		internal SchedulerAboutDialog()
			: base(typeof(SchedulerAboutDialog).Assembly)
		{
		}

		internal void ShowAboutDialog(MxScheduler scheduler)
		{
			ShowDialog();
		}
	}
}
