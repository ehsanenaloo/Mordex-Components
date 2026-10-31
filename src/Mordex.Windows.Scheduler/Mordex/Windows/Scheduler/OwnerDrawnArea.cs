using System;

namespace Mordex.Windows.Scheduler
{
	[Flags]
	public enum OwnerDrawnArea
	{
		None = 0,
		AllDayArea = 1,
		Appointments = 2,
		ControlBackground = 4,
		DateHeader = 8,
		DayViewRow = 0x10,
		MonthViewDay = 0x20,
		OwnerHeader = 0x40,
		OwnerListItem = 0x80,
		ScrollDayButton = 0x100,
		TimeNavigator = 0x200,
		WeekViewDay = 0x400,
		WeekHeader = 0x800,
		All = 0xFFF
	}
}
