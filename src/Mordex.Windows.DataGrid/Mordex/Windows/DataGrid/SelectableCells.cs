using System;

namespace Mordex.Windows.DataGrid
{
	[Flags]
	public enum SelectableCells
	{
		None = 0,
		RowCells = 1,
		NewRowCells = 2,
		FilterRowCells = 4,
		TotalRowCells = 8,
		GroupTotalRowCells = 0x10,
		All = 0x1F
	}
}
