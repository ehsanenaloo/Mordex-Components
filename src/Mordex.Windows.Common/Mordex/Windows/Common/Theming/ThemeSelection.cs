using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// The selected row of grids and lists in the older looks. Office 2003 and Office 2007 fill it with the orange
	/// gradient of a checked button; the other older looks fill it with the selection color of the theme.
	/// </summary>
	internal static class ThemeSelection
	{
		// The checked button of Office 2003 (the same in the blue, olive and silver schemes): begin, middle and end.
		private static readonly Color[] Office2003Colors = { Color.FromArgb(255, 223, 154), Color.FromArgb(255, 195, 116), Color.FromArgb(255, 166, 76) };

		private static readonly float[] Office2003Stops = { 0f, 0.5f, 1f };

		// The checked button of Office 2007, measured on the selected Mail button of Outlook 2007: a light upper part with a
		// sharp split at two fifths into a deeper orange that turns yellow toward the bottom. The same in every scheme.
		private static readonly Color[] Office2007Colors = { Color.FromArgb(255, 220, 163), Color.FromArgb(255, 189, 114), Color.FromArgb(255, 171, 62), Color.FromArgb(255, 225, 120) };

		private static readonly float[] Office2007Stops = { 0f, 0.38f, 0.39f, 1f };

		/// <summary>True for the looks of older products, whose lists are drawn by the suite and not in the Windows 11 look.</summary>
		internal static bool IsOlderLook(MxTheme theme)
		{
			if (theme == null)
			{
				return false;
			}
			ThemeStyle style = theme.Style;
			return style.FaceGradient != ThemeGradient.None || style.ButtonLook != ThemeButtonLook.Default || style.Detail.ChoiceLook != ThemeChoiceLook.Default;
		}

		/// <summary>The gradient of the selected row, from top to bottom, or null when the theme selects with one color. A new copy each time.</summary>
		internal static ColorBlend Gradient(MxTheme theme)
		{
			if (theme == null)
			{
				return null;
			}
			switch (theme.Family)
			{
			case "Office 2003":
				return new ColorBlend { Colors = (Color[])Office2003Colors.Clone(), Positions = (float[])Office2003Stops.Clone() };
			case "Office 2007":
				return new ColorBlend { Colors = (Color[])Office2007Colors.Clone(), Positions = (float[])Office2007Stops.Clone() };
			default:
				return null;
			}
		}

		/// <summary>A vertical brush that paints the gradient over the height of the bounds. The caller disposes it.</summary>
		internal static Brush CreateGradientBrush(ColorBlend blend, Rectangle bounds)
		{
			// The brush starts one pixel above and ends one below, so the first and last rows do not wrap to the other end.
			Rectangle area = new Rectangle(bounds.X, bounds.Y - 1, System.Math.Max(1, bounds.Width), System.Math.Max(1, bounds.Height) + 2);
			LinearGradientBrush brush = new LinearGradientBrush(area, blend.Colors[0], blend.Colors[blend.Colors.Length - 1], LinearGradientMode.Vertical);
			brush.InterpolationColors = blend;
			return brush;
		}

		/// <summary>Fills a selected row. An inactive row (the list has no focus) takes the inactive selection color.</summary>
		internal static void Fill(Graphics graphics, Rectangle bounds, MxTheme theme, bool active)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			ColorBlend blend = active ? Gradient(theme) : null;
			if (blend == null)
			{
				graphics.FillRectangle(GdiCache.Brush(theme.GetColor(active ? ThemeRole.Selection : ThemeRole.SelectionInactive)), bounds);
				return;
			}
			using (Brush brush = CreateGradientBrush(blend, bounds))
			{
				graphics.FillRectangle(brush, bounds);
			}
		}

		/// <summary>The text color on a selected row.</summary>
		internal static Color TextColor(MxTheme theme, bool active)
		{
			return theme.GetColor(active ? ThemeRole.SelectionText : ThemeRole.SelectionInactiveText);
		}
	}
}
