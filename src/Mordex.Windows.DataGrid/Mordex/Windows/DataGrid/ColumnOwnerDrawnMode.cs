using System;

namespace Mordex.Windows.DataGrid
{
	[Flags]
	public enum ColumnOwnerDrawnMode
	{
		None = 0,
		Cells = 1,
		NewRowCells = 2,
		CardCell = 4,
		GroupTotalCell = 8,
		TotalRowCell = 0x10,
		CardHeader = 0x20,
		ColumnHeader = 0x40,
		FilterRowCell = 0x80,
		All = 0xFF
	}
}
