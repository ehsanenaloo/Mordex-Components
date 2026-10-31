using System;

namespace Mordex.Windows.UI.Docking
{
	[Flags]
	internal enum BorderEdges
	{
		None = 0,
		Left = 1,
		Top = 2,
		Right = 4,
		Bottom = 8,
		All = 0xF
	}
}
