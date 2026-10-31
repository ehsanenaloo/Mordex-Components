using System;

namespace Mordex.Windows.DataGrid
{
	[Flags]
	public enum GridOwnerDrawnArea
	{
		None = 0,
		GroupByBoxBackground = 1,
		GroupByBoxInfoText = 2,
		GroupByBoxTableInfo = 4,
		RowHeaders = 8,
		TableHeaders = 0x10,
		NewAndTotalRowSeparators = 0x20,
		PreviewRows = 0x40,
		Background = 0x80,
		GroupRow = 0x100,
		CardCaption = 0x200,
		ColumnSetHeaders = 0x400
	}
}
