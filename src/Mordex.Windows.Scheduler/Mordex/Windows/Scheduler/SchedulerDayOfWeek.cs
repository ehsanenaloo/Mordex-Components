using System;

namespace Mordex.Windows.Scheduler
{
	[Flags]
	public enum SchedulerDayOfWeek
	{
		Sunday = 1,
		Monday = 2,
		Tuesday = 4,
		Wednesday = 8,
		Thursday = 0x10,
		Friday = 0x20,
		Saturday = 0x40
	}
}
