using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	internal interface IGridStyleSource
	{
		Font ControlStyleFont { get; }

		bool IsRightToLeft { get; }

		MxColorSchemeAppearanceState GetDrawingStyle(GridStyleArea styleArea);

		ImageList GetImageList();

		void OnControlStyleChanged();
	}
}
