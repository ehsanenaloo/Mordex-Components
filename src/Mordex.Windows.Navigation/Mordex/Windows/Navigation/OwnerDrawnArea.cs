using System;

namespace Mordex.Windows.Navigation
{
	[Flags]
	public enum OwnerDrawnArea
	{
		None = 0,
		Background = 1,
		GroupHeader = 2,
		GroupBackground = 4,
		Items = 8,
		All = 0xFF
	}
}
