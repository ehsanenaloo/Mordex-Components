using System;

namespace Mordex.Windows.DataGrid.Design.Designer
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
		RetrieveStructure = 0x20,
		All = 0x3F
	}
}
