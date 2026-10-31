using System;

namespace Mordex.Windows.DataGrid
{
	[Flags]
	internal enum BorderFlags
	{
		None = 0,
		Left = 1,
		Top = 2,
		Right = 4,
		Bottom = 8,
		All = 0xF,
		ConnectorLeft = 0x10,
		ConnectorTop = 0x20,
		ConnectorRight = 0x40,
		ConnectorBottom = 0x80
	}
}
