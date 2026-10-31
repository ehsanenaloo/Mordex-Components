using System;

namespace Mordex.Windows.Scheduler
{
	internal interface ISchedulerWorkingHoursOwner
	{
		MxScheduler Scheduler { get; }

		DateTime MinDate { get; }

		SchedulerDayOfWeek FirstDayOfWeek { get; }

		SchedulerDayOfWeek WorkWeek { get; }

		void InvalidateHour();
	}
}
