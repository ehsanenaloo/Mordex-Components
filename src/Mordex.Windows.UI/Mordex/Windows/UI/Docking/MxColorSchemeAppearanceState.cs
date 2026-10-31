using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal partial class MxColorSchemeAppearanceState
	{
		private const int ColorCount = 56;

		private Color[] _panelColors;


		private bool _isDarkScheme;

		internal MxColorSchemeAppearanceState()
		{
			_panelColors = new Color[ColorCount];
			ApplyStyle();
		}

		internal Color GetColor(PanelColors panelColors)
		{
			return _panelColors[(int)panelColors];
		}

		private ref Color ColorRef(PanelColors panelColors)
		{
			return ref _panelColors[(int)panelColors];
		}

		internal void ApplyStyle()
		{
			RebuildColors();
		}

		internal void RebuildColors()
		{
			_isDarkScheme = false;
			ApplyThemedColors();
		}

		internal bool IsDarkScheme()
		{
			return _isDarkScheme;
		}

	}
}
