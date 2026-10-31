using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal interface IGridEnvironment
	{
		Font DefaultFont { get; }

		bool RightToLeft { get; }

		MxDataGridPrintDocument PrintDocument { get; }

		GridTextBatch PaintTextBatch { get; }

		ImageList GetImageList();

		bool IsPrinting();
	}
}
