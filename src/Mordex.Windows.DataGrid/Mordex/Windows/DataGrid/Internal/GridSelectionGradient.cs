using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	// The selected row of a themed grid in Office 2003 and 2007: the orange gradient of a checked button. The colors are
	// the ones the native lists of the suite use (ThemeSelection in the common library, which the grid cannot see); a
	// test keeps the two the same.
	internal static class GridSelectionGradient
	{
		private static readonly Color[] Office2003Colors = { Color.FromArgb(255, 223, 154), Color.FromArgb(255, 195, 116), Color.FromArgb(255, 166, 76) };

		private static readonly float[] Office2003Stops = { 0f, 0.5f, 1f };

		private static readonly Color[] Office2007Colors = { Color.FromArgb(255, 220, 163), Color.FromArgb(255, 189, 114), Color.FromArgb(255, 171, 62), Color.FromArgb(255, 225, 120) };

		private static readonly float[] Office2007Stops = { 0f, 0.38f, 0.39f, 1f };

		// The gradient from top to bottom, or null when the theme selects with one color.
		internal static ColorBlend For(MxTheme theme)
		{
			switch (theme?.Family)
			{
			case "Office 2003":
				return new ColorBlend { Colors = (Color[])Office2003Colors.Clone(), Positions = (float[])Office2003Stops.Clone() };
			case "Office 2007":
				return new ColorBlend { Colors = (Color[])Office2007Colors.Clone(), Positions = (float[])Office2007Stops.Clone() };
			default:
				return null;
			}
		}

		// A vertical brush over the height of the bounds; it starts one pixel above and ends one below, so the first and
		// last rows do not wrap to the other end. The caller disposes it.
		internal static Brush CreateBrush(ColorBlend blend, Rectangle bounds)
		{
			Rectangle area = new Rectangle(bounds.X, bounds.Y - 1, System.Math.Max(1, bounds.Width), System.Math.Max(1, bounds.Height) + 2);
			LinearGradientBrush brush = new LinearGradientBrush(area, blend.Colors[0], blend.Colors[blend.Colors.Length - 1], LinearGradientMode.Vertical);
			brush.InterpolationColors = blend;
			return brush;
		}
	}
}
