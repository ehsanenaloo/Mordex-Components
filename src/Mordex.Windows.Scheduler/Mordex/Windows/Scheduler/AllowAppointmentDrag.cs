using System;

namespace Mordex.Windows.Scheduler
{
	[Flags]
	public enum AllowAppointmentDrag
	{
		None = 0,
		Move = 1,
		Resize = 2,
		OwnerChange = 4,
		Copy = 8,
		All = 0xF
	}
}
