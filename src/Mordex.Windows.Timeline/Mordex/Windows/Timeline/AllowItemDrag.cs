using System;

namespace Mordex.Windows.Timeline
{
	[Flags]
	public enum AllowItemDrag
	{
		None = 0,
		Move = 1,
		Resize = 2,
		GroupChange = 4,
		EstimatedDurationChange = 8,
		All = 0xF
	}
}
