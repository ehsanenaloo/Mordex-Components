using System;

namespace Mordex.Windows.UI.Docking
{
	[Flags]
	internal enum ButtonState
	{
		Normal = 0,
		Hot = 1,
		Selected = 2,
		Pressed = 4,
		Disabled = 8
	}
}
