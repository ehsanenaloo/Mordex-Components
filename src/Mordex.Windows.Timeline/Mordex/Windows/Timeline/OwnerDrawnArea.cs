using System;

namespace Mordex.Windows.Timeline
{
	[Flags]
	public enum OwnerDrawnArea
	{
		None = 0,
		ControlBackground = 1,
		GroupRow = 2,
		RowHeader = 4,
		Items = 8,
		TopTierInterval = 0x10,
		MiddleTierInterval = 0x20,
		BottomTierInterval = 0x40,
		All = 0x7F
	}
}
