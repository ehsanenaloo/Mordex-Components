using System;

namespace Mordex.Windows.TaskPane
{
	[Flags]
	public enum OwnerDrawnArea
	{
		None = 0,
		Background = 1,
		GroupHeader = 2,
		Item = 4,
		All = 7
	}
}
