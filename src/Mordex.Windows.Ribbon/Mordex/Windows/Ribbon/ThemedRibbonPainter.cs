using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	/// <summary>
	/// Paints the surfaces of a themed ribbon from a resolved theme. It creates no brush, pen or path: the theme and the shared
	/// GDI cache hold them.
	/// </summary>
	internal static partial class ThemedRibbonPainter
	{
		// A rounded face needs smooth edges; a square one stays crisp.
		private static SmoothingMode BeginShape(Graphics graphics, ResolvedTheme theme)
		{
			SmoothingMode previous = graphics.SmoothingMode;
			if (theme.Shape.CornerRadius > 0)
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
			}
			return previous;
		}

		/// <summary>A button face: a fill and an outline, with the corners of the theme.</summary>
		internal static void Face(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemeRole fill, ThemeRole border)
		{
			if (bounds.Width <= 1 || bounds.Height <= 1)
			{
				return;
			}
			int radius = ButtonRadius(theme);
			SmoothingMode previous = BeginShape(graphics, radius);
			if (theme.Style.Modern.Glass != ThemeGlassKind.None && (fill == ThemeRole.SurfaceHot || fill == ThemeRole.SurfacePressed || fill == ThemeRole.AccentSubtle) && theme.DrawButtonLook(graphics, bounds, fill, border, radius, true, fill != ThemeRole.SurfacePressed, fill == ThemeRole.SurfacePressed, false))
			{
				graphics.SmoothingMode = previous;
				return;
			}
			theme.FillRounded(graphics, bounds, fill, radius);
			theme.DrawRounded(graphics, bounds, border, radius);
			graphics.SmoothingMode = previous;
		}

		// The radius of a ribbon face: the ribbon style first, then the button radius of the theme style, then the shape.
		internal static int ButtonRadius(ResolvedTheme theme)
		{
			RibbonThemeStyle ribbon = RibbonThemeStyle.For(theme.Theme);
			return ribbon.Radius > 0 ? ribbon.Radius : theme.Theme.Style.RadiusOf(ThemeElement.Button, theme.Shape.CornerRadius);
		}

		private static SmoothingMode BeginShape(Graphics graphics, int radius)
		{
			SmoothingMode previous = graphics.SmoothingMode;
			if (radius > 0)
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
			}
			return previous;
		}

		/// <summary>A row of a menu or a list under the mouse: a fill only.</summary>
		internal static void Row(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemeRole fill)
		{
			if (bounds.Width <= 1 || bounds.Height <= 1)
			{
				return;
			}
			SmoothingMode previous = BeginShape(graphics, theme);
			theme.FillRounded(graphics, bounds, fill, theme.Shape.CornerRadius);
			graphics.SmoothingMode = previous;
		}

		/// <summary>One half of a split button is under the mouse or open. The other half stays lighter.</summary>
		internal static void SplitFace(Graphics graphics, ResolvedTheme theme, Rectangle bounds, Rectangle buttonBounds, bool arrowActive, bool large, bool open)
		{
			ThemeRole border = open ? ThemeRole.BorderPressed : ThemeRole.BorderHot;
			Face(graphics, theme, bounds, ThemeRole.SurfaceHot, border);
			Rectangle part = buttonBounds;
			if (arrowActive)
			{
				part = large
					? new Rectangle(bounds.X, buttonBounds.Bottom, bounds.Width, bounds.Bottom - buttonBounds.Bottom)
					: new Rectangle(buttonBounds.Right, bounds.Y, bounds.Right - buttonBounds.Right, bounds.Height);
			}
			part.Intersect(bounds);
			part.Inflate(-1, -1);
			if (part.Width > 0 && part.Height > 0)
			{
				graphics.FillRectangle(open ? theme.Brush(ThemeRole.SurfacePressed) : GdiCache.Brush(Color.FromArgb(120, theme.Color(ThemeRole.SurfacePressed))), part);
			}
			Pen divider = theme.Pen(border);
			if (large)
			{
				graphics.DrawLine(divider, bounds.X + 1, buttonBounds.Bottom - 1, bounds.Right - 2, buttonBounds.Bottom - 1);
			}
			else
			{
				graphics.DrawLine(divider, buttonBounds.Right - 1, bounds.Y + 1, buttonBounds.Right - 1, bounds.Bottom - 2);
			}
		}

		/// <summary>The file tab or the backstage tab: the accent color, with the lighter and darker shades for hot and open.</summary>
		internal static void FileTab(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool open, bool hot)
		{
			RibbonThemeStyle style = RibbonThemeStyle.For(theme.Theme);
			bounds.Height++;
			switch (style.AppButton)
			{
			case RibbonAppButtonKind.FileTab:
			{
				style.GetAppButtonColors(theme.Theme, out Color light, out Color dark, out Color border);
				if (hot && !open)
				{
					light = Lighten(light, 0.2f);
					dark = Lighten(dark, 0.2f);
				}
				else if (open)
				{
					light = Lighten(dark, 0.1f);
				}
				GdiCache.FillGradient(graphics, bounds, light, dark, LinearGradientMode.Vertical);
				graphics.DrawRectangle(GdiCache.Pen(border), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
				break;
			}
			case RibbonAppButtonKind.ThemedButton:
			{
				ThemeRole role = open ? ThemeRole.SurfacePressed : hot ? ThemeRole.SurfaceHot : ThemeRole.SurfaceRaised;
				SmoothingMode previous = BeginShape(graphics, ButtonRadius(theme));
				theme.FillRounded(graphics, bounds, role, ButtonRadius(theme));
				theme.DrawRounded(graphics, bounds, open ? ThemeRole.BorderPressed : ThemeRole.BorderStrong, ButtonRadius(theme));
				graphics.SmoothingMode = previous;
				break;
			}
			default:
			{
				ThemeRole fill = open ? ThemeRole.AccentPressed : hot ? ThemeRole.AccentHot : ThemeRole.Accent;
				graphics.FillRectangle(theme.Brush(fill), bounds);
				break;
			}
			}
		}

		private static Color Lighten(Color color, float amount)
		{
			return Color.FromArgb(color.A, (int)(color.R + (255 - color.R) * amount), (int)(color.G + (255 - color.G) * amount), (int)(color.B + (255 - color.B) * amount));
		}

		/// <summary>The glass orb of Office 2007: a round button in the file button colors of the theme, with a light lens over its upper half.</summary>
		internal static void AppOrb(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool open, bool hot)
		{
			RibbonThemeStyle.For(theme.Theme).GetAppButtonColors(theme.Theme, out Color light, out Color dark, out Color border);
			if (hot && !open)
			{
				light = Lighten(light, 0.25f);
				dark = Lighten(dark, 0.15f);
			}
			else if (open)
			{
				dark = Color.FromArgb(dark.A, dark.R * 4 / 5, dark.G * 4 / 5, dark.B * 4 / 5);
			}
			SmoothingMode previous = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Rectangle disc = Rectangle.Inflate(bounds, -1, -1);
			// A soft shadow under the disc, then the rim, then the white glass body inside it.
			graphics.FillEllipse(GdiCache.Brush(Color.FromArgb(50, 20, 40, 90)), new Rectangle(disc.X, disc.Y + 1, disc.Width, disc.Height));
			using (LinearGradientBrush rim = new LinearGradientBrush(new Rectangle(disc.X, disc.Y - 1, disc.Width, disc.Height + 2), Lighten(border, 0.6f), border, LinearGradientMode.Vertical))
			{
				graphics.FillEllipse(rim, disc);
			}
			int ring = System.Math.Max(2, disc.Width / 14);
			Rectangle body = Rectangle.Inflate(disc, -ring, -ring);
			using (LinearGradientBrush glass = new LinearGradientBrush(new Rectangle(body.X, body.Y - 1, body.Width, body.Height + 2), light, dark, LinearGradientMode.Vertical))
			{
				graphics.FillEllipse(glass, body);
			}
			Rectangle lens = new Rectangle(body.X + body.Width / 10, body.Y + 1, body.Width * 4 / 5, body.Height * 11 / 20);
			using (LinearGradientBrush shine = new LinearGradientBrush(new Rectangle(lens.X, lens.Y - 1, lens.Width, lens.Height + 2), Color.FromArgb(210, 255, 255, 255), Color.FromArgb(25, 255, 255, 255), LinearGradientMode.Vertical))
			{
				graphics.FillEllipse(shine, lens);
			}
			Rectangle glow = new Rectangle(body.X + body.Width / 5, body.Y + body.Height * 3 / 5, body.Width * 3 / 5, body.Height * 3 / 10);
			using (LinearGradientBrush reflection = new LinearGradientBrush(new Rectangle(glow.X, glow.Y - 1, glow.Width, glow.Height + 2), Color.FromArgb(0, 255, 255, 255), Color.FromArgb(120, 255, 255, 255), LinearGradientMode.Vertical))
			{
				graphics.FillEllipse(reflection, glow);
			}
			if (hot && !open)
			{
				graphics.DrawEllipse(GdiCache.Pen(Color.FromArgb(170, 120, 190, 255)), Rectangle.Inflate(body, -1, -1));
			}
			graphics.DrawEllipse(GdiCache.Pen(border), disc);
			graphics.DrawEllipse(GdiCache.Pen(Color.FromArgb(140, 255, 255, 255)), body);
			graphics.SmoothingMode = previous;
		}

		/// <summary>A tab of the strip. The selected tab joins the content area and has an accent line under its text.</summary>
		internal static void Tab(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool selected, bool hot, Color accentOverride)
		{
			if (bounds.Width <= 2 || bounds.Height <= 2)
			{
				return;
			}
			RibbonThemeStyle style = RibbonThemeStyle.For(theme.Theme);
			if (style.TabShape != RibbonTabShape.Underline)
			{
				ShapedTab(graphics, theme, style, bounds, selected, hot, accentOverride);
				return;
			}
			if (selected)
			{
				// One pixel lower than the tab, so it covers the line of the content area under it.
				Rectangle face = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height + 1);
				graphics.FillRectangle(GdiCache.Brush(SelectedTabFill(theme)), face);
				Pen border = theme.Pen(ThemeRole.Border);
				graphics.DrawLine(border, face.X, face.Y, face.X, face.Bottom - 1);
				graphics.DrawLine(border, face.Right - 1, face.Y, face.Right - 1, face.Bottom - 1);
				graphics.DrawLine(border, face.X, face.Y, face.Right - 1, face.Y);
				int inset = theme.Scaled(6);
				int thickness = theme.Scaled(2);
				Rectangle line = new Rectangle(face.X + inset, face.Bottom - 1 - thickness, face.Width - inset * 2, thickness);
				if (line.Width > 0)
				{
					graphics.FillRectangle(accentOverride.IsEmpty ? theme.Brush(ThemeRole.Accent) : GdiCache.Brush(accentOverride), line);
				}
			}
			else if (hot)
			{
				Rectangle face = Rectangle.Inflate(bounds, -1, 0);
				face.Height -= 1;
				SmoothingMode previous = BeginShape(graphics, theme);
				theme.FillRounded(graphics, face, ThemeRole.TabHot, theme.Shape.CornerRadius);
				graphics.SmoothingMode = previous;
			}
		}

		/// <summary>The frame of the content area under the tabs: a flat fill, a line on top that breaks under the selected tab, and a line below.</summary>
		internal static void ContentFrame(Graphics graphics, ResolvedTheme theme, Rectangle bounds, int bottomPadding, Rectangle selectedTab, Color lineOverride)
		{
			Rectangle area = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - bottomPadding);
			if (area.Width <= 0 || area.Height <= 0)
			{
				return;
			}
			ContentFill(graphics, theme, area);
			// Office 2010 draws the frame in its own ribbon border colors, lighter than the border of the controls.
			bool office2010 = theme.Theme.HasOfficePalette && theme.Theme.Family == "Office 2010";
			Pen bottom = office2010 ? GdiCache.Pen(theme.OfficeColorOr(19, 17)) : theme.Pen(ThemeRole.Border);
			Pen line = !lineOverride.IsEmpty ? GdiCache.Pen(lineOverride) : office2010 ? GdiCache.Pen(theme.OfficeColorOr(16, 17)) : theme.Pen(ThemeRole.Border);
			if (selectedTab.Width > 0)
			{
				graphics.DrawLine(line, area.X, area.Y, selectedTab.X, area.Y);
				graphics.DrawLine(line, selectedTab.Right, area.Y, area.Right - 1, area.Y);
			}
			else
			{
				graphics.DrawLine(line, area.X, area.Y, area.Right - 1, area.Y);
			}
			graphics.DrawLine(bottom, area.X, area.Bottom - 1, area.Right - 1, area.Bottom - 1);
			if (bottomPadding > 0)
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.Surface), area.X, area.Bottom, area.Width, bottomPadding);
			}
		}
	}
}
