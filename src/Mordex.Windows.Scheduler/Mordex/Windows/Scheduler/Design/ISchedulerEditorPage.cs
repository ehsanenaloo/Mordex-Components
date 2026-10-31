using System;

namespace Mordex.Windows.Scheduler.Design
{
	internal interface ISchedulerEditorPage
	{
		MxScheduler Scheduler { get; set; }

		event EventHandler SelectTab;

		bool CommitChanges();

		void Unload();
	}
}
