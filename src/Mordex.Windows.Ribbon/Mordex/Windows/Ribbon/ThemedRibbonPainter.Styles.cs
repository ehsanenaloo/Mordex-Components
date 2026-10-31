using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	// The shapes that the ribbon style of a theme asks for: rounded or raised tabs, boxed groups, the capsule of the quick access toolbar.
	internal static partial class ThemedRibbonPainter
	{
		private static GraphicsPath TopRoundedPath(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			if (radius <= 0)
			{
				path.AddRectangle(r);
				return path;
			}
			int d = radius * 2;
			path.AddArc(r.X, r.Y, d, d, 180, 90);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
			path.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
			path.CloseFigure();
			return path;
		}

		private static GraphicsPath RoundedPath(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = System.Math.Max(1, radius * 2);
			path.AddArc(r.X, r.Y, d, d, 180, 90);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
			path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
			path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		private static void ShapedTab(Graphics graphics, ResolvedTheme theme, RibbonThemeStyle style, Rectangle bounds, bool selected, bool hot, Color accentOverride)
		{
			if (!selected && !hot)
			{
				return;
			}
			if (selected && accentOverride.IsEmpty && theme.Theme.HasOfficePalette && theme.Theme.Family == "Office 2007")
			{
				Office2007SelectedTab(graphics, theme, style, bounds);
				return;
			}
			int radius = theme.Scaled(style.TabShape == RibbonTabShape.Flat ? 0 : System.Math.Max(2, style.Radius));
			Rectangle face = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height + (selected ? 1 : 0));
			SmoothingMode previous = graphics.SmoothingMode;
			graphics.SmoothingMode = radius > 0 ? SmoothingMode.AntiAlias : SmoothingMode.None;
			Color fill = selected ? SelectedTabFill(theme) : theme.Color(ThemeRole.TabHot);
			Color top = style.ContentGradient || style.TabShape == RibbonTabShape.Rounded ? Lighten(fill, 0.45f) : fill;
			using (GraphicsPath path = TopRoundedPath(face, radius))
			{
				if (theme.Shape.IsFlat && style.TabShape != RibbonTabShape.Rounded)
				{
					graphics.FillPath(GdiCache.Brush(fill), path);
				}
				else
				{
					using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(face.X, face.Y - 1, face.Width + 1, face.Height + 2), top, fill, LinearGradientMode.Vertical))
					{
						graphics.FillPath(brush, path);
					}
				}
				graphics.DrawPath(theme.Pen(selected ? ThemeRole.Border : ThemeRole.BorderHot), path);
			}
			graphics.SmoothingMode = SmoothingMode.None;
			if (selected)
			{
				// The tab is open to the content area: hide the outline on its lower edge.
				graphics.DrawLine(GdiCache.Pen(fill), face.X + 1, face.Bottom, face.Right - 1, face.Bottom);
				if (!accentOverride.IsEmpty)
				{
					graphics.FillRectangle(GdiCache.Brush(accentOverride), face.X + 1, face.Y + 1, face.Width - 1, theme.Scaled(2));
				}
				else if (style.TabShape == RibbonTabShape.Beveled && theme.Theme.Style.Bevel == ThemeBevel.None)
				{
					graphics.DrawLine(theme.Pen(ThemeRole.Accent), face.X + radius, face.Y + 1, face.Right - radius, face.Y + 1);
				}
			}
			graphics.SmoothingMode = previous;
		}

		// The selected tab of Office 2007, measured on Word 2007 Blue: an outline in the tab border color, a light inner line
		// and a fill that runs from near white to the selected tab color of the scheme.
		private static void Office2007SelectedTab(Graphics graphics, ResolvedTheme theme, RibbonThemeStyle style, Rectangle bounds)
		{
			int radius = theme.Scaled(System.Math.Max(2, style.Radius));
			Rectangle face = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height + 1);
			Color bottom = theme.OfficeColorOr(SlotSelectedTabGradient, SlotSelectedTab);
			Color top = Lighten(bottom, 0.8f);
			SmoothingMode previous = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = TopRoundedPath(face, radius))
			{
				using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(face.X, face.Y - 1, face.Width + 1, face.Height + 2), top, bottom, LinearGradientMode.Vertical))
				{
					graphics.FillPath(brush, path);
				}
				using (GraphicsPath inner = TopRoundedPath(new Rectangle(face.X + 1, face.Y + 1, face.Width - 2, face.Height), System.Math.Max(1, radius - 1)))
				{
					graphics.DrawPath(GdiCache.Pen(MxTheme.Blend(Color.White, theme.OfficeColorOr(SlotTabBorderLight, SlotSelectedTab), 0.3f)), inner);
				}
				graphics.DrawPath(GdiCache.Pen(theme.OfficeColorOr(SlotTabBorder, 17)), path);
			}
			graphics.SmoothingMode = SmoothingMode.None;
			// The tab is open to the content area: hide the outline on its lower edge.
			graphics.DrawLine(GdiCache.Pen(bottom), face.X + 1, face.Bottom, face.Right - 1, face.Bottom);
			graphics.SmoothingMode = previous;
		}

		private const int SlotTabBorder = 13;
		private const int SlotSelectedTab = 50;
		private const int SlotSelectedTabGradient = 51;
		private const int SlotTabBorderLight = 192;

		/// <summary>The gradient of the content area under the tabs, for the styles that have one.</summary>
		private static void ContentFill(Graphics graphics, ResolvedTheme theme, Rectangle area)
		{
			RibbonThemeStyle style = RibbonThemeStyle.For(theme.Theme);
			Color panel = theme.Color(ThemeRole.ToolbarBack);
			if (style.ContentGradient && theme.Theme.HasOfficePalette && area.Height > 2 && theme.Theme.Family == "Office 2010")
			{
				Office2010ContentFill(graphics, theme, area);
				return;
			}
			if (style.ContentGradient && theme.Theme.HasOfficePalette && area.Height > 2)
			{
				GdiCache.FillGradient(graphics, area, Lighten(theme.OfficeColor(20), 0.3f), theme.OfficeColorOr(24, 22), LinearGradientMode.Vertical);
				return;
			}
			if (style.ContentGradient && theme.Theme.Style.FaceGradient != ThemeGradient.None && area.Height > 2)
			{
				GdiCache.FillGradient(graphics, area, theme.Theme.Style.LightEnd(panel), theme.Theme.Style.DarkEnd(panel), LinearGradientMode.Vertical);
				return;
			}
			graphics.FillRectangle(theme.Brush(ThemeRole.ToolbarBack), area);
		}

		// The content area of Office 2010 runs from the light to the dark ribbon color. The share of the dark color at each
		// height was measured on real Office 2010 windows: Blue reaches its dark color at two thirds, Silver darkens late.
		private static void Office2010ContentFill(Graphics graphics, ResolvedTheme theme, Rectangle area)
		{
			Color light = theme.OfficeColor(20);
			Color dark = theme.OfficeColorOr(24, 22);
			float[] positions = { 0f, 0.2f, 0.4f, 0.6f, 0.7f, 0.8f, 1f };
			float[] share = theme.Theme.Name.EndsWith("Blue") ? new[] { 0f, 0.43f, 0.7f, 0.91f, 1f, 1f, 1f }
				: theme.Theme.Name.EndsWith("Silver") ? new[] { 0f, 0.1f, 0.19f, 0.38f, 0.5f, 0.67f, 1f }
				: positions;
			Color[] colors = new Color[positions.Length];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = MxTheme.Blend(light, dark, share[i]);
			}
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(area.X, area.Y - 1, area.Width, area.Height + 1), light, dark, LinearGradientMode.Vertical))
			{
				brush.InterpolationColors = new ColorBlend(colors.Length) { Colors = colors, Positions = positions };
				graphics.FillRectangle(brush, area);
			}
		}

		/// <summary>The box of a group for the styles that frame their groups.</summary>
		internal static bool GroupBox(Graphics graphics, ResolvedTheme theme, Rectangle area, Rectangle captionBounds)
		{
			RibbonThemeStyle style = RibbonThemeStyle.For(theme.Theme);
			if (style.GroupFrame == RibbonGroupFrameKind.Separator)
			{
				return false;
			}
			Rectangle box = Rectangle.Inflate(area, -2, 0);
			box.Height -= 1;
			if (box.Width <= 4 || box.Height <= 4)
			{
				return true;
			}
			if (style.GroupFrame == RibbonGroupFrameKind.Etched)
			{
				graphics.DrawRectangle(GdiCache.Pen(theme.Theme.Style.BevelLight.IsEmpty ? Color.White : theme.Theme.Style.BevelLight), box.X + 1, box.Y + 1, box.Width - 1, box.Height - 1);
				graphics.DrawRectangle(theme.Pen(ThemeRole.Border), box.X, box.Y, box.Width - 1, box.Height - 1);
				return true;
			}
			int radius = theme.Scaled(style.Radius + 1);
			Color panel = theme.Color(ThemeRole.ToolbarBack);
			SmoothingMode previous = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = RoundedPath(new Rectangle(box.X, box.Y, box.Width - 1, box.Height - 1), radius))
			{
				bool office = theme.Theme.HasOfficePalette;
				Color boxTop = office ? Lighten(theme.OfficeColor(20), 0.45f) : Lighten(panel, 0.35f);
				Color boxBottom = office ? theme.OfficeColor(22) : Lighten(theme.Color(ThemeRole.SurfaceRaised), 0.15f);
				using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(box.X, box.Y - 1, box.Width, box.Height + 2), boxTop, boxBottom, LinearGradientMode.Vertical))
				{
					graphics.FillPath(brush, path);
				}
				if (style.GroupCaptionBand && captionBounds.Height > 0)
				{
					GraphicsState state = graphics.Save();
					graphics.SetClip(path, CombineMode.Intersect);
					Rectangle band = new Rectangle(box.X, captionBounds.Y, box.Width, box.Bottom - captionBounds.Y);
					if (office)
					{
						GdiCache.FillGradient(graphics, band, theme.OfficeColorOr(24, 22), theme.OfficeColorOr(25, 22), LinearGradientMode.Vertical);
						graphics.DrawLine(GdiCache.Pen(Color.FromArgb(150, 255, 255, 255)), band.X, band.Y + 1, band.Right - 1, band.Y + 1);
						graphics.DrawLine(GdiCache.Pen(Color.FromArgb(120, theme.OfficeColorOr(32, 17))), band.X, band.Y, band.Right - 1, band.Y);
					}
					else
					{
						graphics.FillRectangle(GdiCache.Brush(Color.FromArgb(110, theme.Color(ThemeRole.SurfaceAlt))), band);
						graphics.DrawLine(GdiCache.Pen(Color.FromArgb(120, theme.Color(ThemeRole.Border))), band.X, band.Y, band.Right - 1, band.Y);
					}
					graphics.Restore(state);
				}
				graphics.DrawPath(theme.Pen(ThemeRole.Border), path);
			}
			graphics.SmoothingMode = previous;
			return true;
		}
	}
}
