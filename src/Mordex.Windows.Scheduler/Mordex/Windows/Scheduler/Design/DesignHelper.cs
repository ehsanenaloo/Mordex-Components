using System;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.Design
{
	public class DesignHelper
	{


		public static void ShowAboutBox(MxScheduler scheduler)
		{
			SchedulerAboutDialog schedulerAboutDialog = new SchedulerAboutDialog();
			schedulerAboutDialog.ShowAboutDialog(scheduler);
			schedulerAboutDialog.Dispose();
		}

		public static DialogResult ShowSchedulerDesigner(MxScheduler scheduler)
		{
			SchedulerEditorDialog schedulerEditorDialog = new SchedulerEditorDialog();
			schedulerEditorDialog.UpdateSchedule(scheduler);
			DialogResult dialogResult = schedulerEditorDialog.DialogResult;
			schedulerEditorDialog.Dispose();
			return dialogResult;
		}

	}
}
