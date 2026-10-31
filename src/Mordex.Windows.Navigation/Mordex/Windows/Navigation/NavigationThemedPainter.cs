using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation
{
	// Paints the areas of a themed navigation bar. The colors come from the format styles, which hold the theme defaults and any
	// color the developer set; the theme supplies borders, the accent bar and the corner radius.
	internal static class NavigationThemedPainter
	{
		internal static void FillHeader(Graphics graphics, ResolvedTheme theme, Rectangle bounds, NavigationBarFormatStyle style, BorderFlags borderFlags)
		{
			ThemeBorder edge = theme.Style.HeaderBorder;
			Rectangle inner = edge == ThemeBorder.Flat ? NavigationGraphicsHelper.DrawBorderEdges(graphics, theme.Pen(ThemeRole.Border), bounds, borderFlags) : bounds;
			if (theme.Style.FaceGradient != ThemeGradient.None && TryHeaderRole(theme, style.BackColor, out ThemeRole role))
			{
				if (theme.Style.Modern.FaceKind == ThemeFaceKind.Glass && inner.Width > 8 && inner.Height > 8)
				{
					// The group headers of Outlook 2007 are rounded glass.
					System.Drawing.Drawing2D.SmoothingMode smoothing = graphics.SmoothingMode;
					graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					theme.FillFace(graphics, inner, role, 4, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
					theme.DrawRounded(graphics, inner, ThemeRole.Border, 4);
					graphics.SmoothingMode = smoothing;
				}
				else
				{
					theme.FillBand(graphics, inner, role);
				}
			}
			else
			{
				Brush brush = style.GetCachedBackBrush(inner);
				if (brush != null)
				{
					graphics.FillRectangle(brush, inner);
				}
			}
			if (edge != ThemeBorder.Flat)
			{
				theme.DrawBorder(graphics, bounds, edge, ThemeRole.Border, 0);
			}
		}

		private static readonly ThemeRole[] HeaderRoles = { ThemeRole.Header, ThemeRole.HeaderHot, ThemeRole.HeaderPressed };

		private static bool TryHeaderRole(ResolvedTheme theme, Color color, out ThemeRole role)
		{
			int argb = color.ToArgb();
			foreach (ThemeRole candidate in HeaderRoles)
			{
				if (theme.Color(candidate).ToArgb() == argb)
				{
					role = candidate;
					return true;
				}
			}
			role = ThemeRole.Header;
			return false;
		}

		internal static void FillItem(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool isHot, bool isPressed, NavigationBarFormatStyle style, bool rightToLeft)
		{
			Brush brush = style.GetCachedBackBrush(bounds);
			if (brush == null)
			{
				return;
			}
			ThemeRole role = isPressed ? ThemeRole.AccentSubtle : ThemeRole.SurfaceHot;
			int radius = theme.Radius(ThemeElement.Button);
			if (radius > 0 && style.BackColor == theme.Color(role))
			{
				theme.FillRounded(graphics, bounds, role, radius);
			}
			else
			{
				graphics.FillRectangle(brush, bounds);
			}
			if (isPressed)
			{
				int width = theme.Scaled(3);
				int inset = theme.Scaled(4);
				int height = System.Math.Max(2, bounds.Height - inset * 2);
				int x = rightToLeft ? bounds.Right - width - 1 : bounds.Left + 1;
				graphics.FillRectangle(theme.Brush(ThemeRole.Accent), x, bounds.Top + (bounds.Height - height) / 2, width, height);
			}
		}
	}
}
