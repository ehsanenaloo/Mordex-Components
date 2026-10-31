using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridColorTable
	{
		internal Color GetGridColor(GridColorKey gridColors)
		{
			return _colors[(int)gridColors];
		}

		internal bool IsThemedLook => _themedLook;

		// The gradient of a selected row in the themed look, or null when the selection has one color.
		internal System.Drawing.Drawing2D.ColorBlend SelectionBlend => _selectionBlend;

		internal string GetThemeColorName()
		{
			return _themeName;
		}
	}
}
