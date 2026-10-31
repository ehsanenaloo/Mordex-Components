using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// A tooltip that draws itself with the colors and the border of the default theme at the DPI of its control. It
	/// replaces the native tooltip window in the controls of the suite. The box is square, because the native tooltip
	/// window has no corner radius to set; a theme with a flat shape gets a thin border, a classic theme a gradient.
	/// </summary>
	[ToolboxItem(false)]
	public class ThemedToolTip : ToolTip
	{
		private const int PaddingX = 6;

		private const int PaddingY = 4;

		private static readonly System.Collections.Generic.Dictionary<int, Font> Fonts = new System.Collections.Generic.Dictionary<int, Font>();

		public ThemedToolTip()
		{
			Initialize();
		}

		public ThemedToolTip(IContainer container) : base(container)
		{
			Initialize();
		}

		private void Initialize()
		{
			OwnerDraw = true;
			Popup += OnPopup;
			Draw += OnDraw;
		}

		/// <summary>The font of themed tooltips at a DPI. The fonts are shared and live as long as the process.</summary>
		public static Font GetFont(int dpi)
		{
			lock (Fonts)
			{
				if (!Fonts.TryGetValue(dpi, out Font font))
				{
					Font basis = SystemFonts.StatusFont;
					font = new Font(basis.FontFamily, basis.SizeInPoints * dpi / 72f, basis.Style, GraphicsUnit.Pixel);
					Fonts[dpi] = font;
				}
				return font;
			}
		}

		private static int DpiOf(Control control)
		{
			return control == null || control.IsDisposed ? 96 : control.DeviceDpi;
		}

		/// <summary>The size of a tooltip box for a text at a DPI, as the popup event measures it.</summary>
		public static Size Measure(string text, int dpi)
		{
			Size size = TextRenderer.MeasureText(text ?? string.Empty, GetFont(dpi), new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
			float scale = dpi / 96f;
			return new Size(size.Width + (int)Math.Round(2 * PaddingX * scale) + 2, size.Height + (int)Math.Round(2 * PaddingY * scale) + 2);
		}

		private void OnPopup(object sender, PopupEventArgs e)
		{
			e.ToolTipSize = Measure(GetToolTip(e.AssociatedControl), DpiOf(e.AssociatedControl));
		}

		private void OnDraw(object sender, DrawToolTipEventArgs e)
		{
			int dpi = DpiOf(e.AssociatedControl);
			Paint(e.Graphics, e.Bounds, e.ToolTipText, MxThemeManager.ResolveDefault(dpi), dpi);
		}

		/// <summary>Draws a tooltip box with a theme. The tooltip window and the tests use it, so both give the same picture.</summary>
		public static void Paint(Graphics graphics, Rectangle bounds, string text, ResolvedTheme theme, int dpi)
		{
			PaintBox(graphics, bounds, theme);
			int padX = (int)Math.Round(PaddingX * dpi / 96f) + 1;
			int padY = (int)Math.Round(PaddingY * dpi / 96f) + 1;
			Rectangle textBounds = new Rectangle(bounds.X + padX, bounds.Y + padY, Math.Max(0, bounds.Width - 2 * padX), Math.Max(0, bounds.Height - 2 * padY));
			TextRenderer.DrawText(graphics, text ?? string.Empty, GetFont(dpi), textBounds, theme.Color(ThemeRole.TooltipText), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
		}

		/// <summary>
		/// Draws the back and the border of a tooltip box with a theme and returns the area inside the border. Tip windows
		/// of the suite that draw their own text use it, so they look like the themed tooltip.
		/// </summary>
		public static Rectangle PaintBox(Graphics graphics, Rectangle bounds, ResolvedTheme theme)
		{
			ThemeShape shape = theme.Shape;
			graphics.FillRectangle(theme.Brush(ThemeRole.TooltipBack), bounds);
			if (!shape.IsFlat)
			{
				theme.FillGradient(graphics, new Rectangle(bounds.X, bounds.Y, bounds.Width, Math.Max(1, bounds.Height / 2)), ThemeRole.SurfaceRaised, ThemeRole.TooltipBack, LinearGradientMode.Vertical);
			}
			graphics.DrawRectangle(theme.Pen(shape.IsFlat ? ThemeRole.Border : ThemeRole.BorderStrong), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			return Rectangle.Inflate(bounds, -1, -1);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Popup -= OnPopup;
				Draw -= OnDraw;
			}
			base.Dispose(disposing);
		}
	}
}
