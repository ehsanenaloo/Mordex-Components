using System;

namespace Mordex.Windows.Ribbon
{
	[Flags]
	internal enum BorderFlags
	{
		None = 0,
		TopLeft = 1,
		TopRight = 2,
		BottomLeft = 4,
		BottomRight = 8,
		All = 0xF
	}
}
