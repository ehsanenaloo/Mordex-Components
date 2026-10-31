using System;

namespace Mordex.Windows.UI.Design
{
	[Flags]
	internal enum ToolbarOptions
	{
		None = 0,
		Add = 1,
		Remove = 2,
		Clear = 4,
		MoveUp = 8,
		MoveDown = 0x10,
		All = 0x1F
	}
}
