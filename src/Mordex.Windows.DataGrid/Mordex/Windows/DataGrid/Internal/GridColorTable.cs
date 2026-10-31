using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridColorTable
	{
		private bool _themedLook;

		private Color[] _colors;

		private System.Drawing.Drawing2D.ColorBlend _selectionBlend;


		private string _themeName;

		private bool _useThemes;

		internal GridColorTable(bool themedLook, bool useThemes)
		{
			_themedLook = themedLook;
			_useThemes = useThemes && VisualStyleDrawer.IsThemingAvailable();
			_colors = new Color[45];
			LoadColorsForStyle();
		}
	}
}
