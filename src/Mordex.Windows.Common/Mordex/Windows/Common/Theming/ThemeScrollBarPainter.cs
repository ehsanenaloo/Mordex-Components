using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	/// <summary>The state and place of one part of a scroll bar.</summary>
	public struct ScrollBarPart
	{
		public Rectangle Bounds;

		public bool Present;

		public bool Hot;

		public bool Pressed;
	}

	/// <summary>
	/// Paints a scroll bar in the style of a theme that sets <see cref="ThemeScrollBarStyle.Custom"/>: a track, a thumb that
	/// can be rectangular, rounded or a pill, with a gradient, a 3D edge and a grip, and arrow buttons with their own face.
	/// A control calls this first and paints its own way when the style is not custom, so the default look stays as it is.
	/// </summary>
	public static class ThemeScrollBarPainter
	{
		public static bool Applies(ResolvedTheme theme)
		{
			return theme.Style.ScrollBar.Custom;
		}

		public static void Paint(Graphics graphics, ResolvedTheme theme, Rectangle client, bool vertical, bool enabled, in ScrollBarPart low, in ScrollBarPart high, in ScrollBarPart thumb)
		{
			ThemeScrollBarStyle style = theme.Style.ScrollBar;
			if (style.Edge == ThemeBorder.Raised && style.ButtonFaces)
			{
				graphics.FillRectangle(theme.TrackHatch, client);
			}
			else
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.ScrollTrack), client);
			}
			PaintButton(graphics, theme, style, low, vertical ? 0 : 2, enabled);
			PaintButton(graphics, theme, style, high, vertical ? 1 : 3, enabled);
			if (thumb.Present && enabled && !thumb.Bounds.IsEmpty)
			{
				PaintThumb(graphics, theme, style, thumb, vertical);
			}
		}

		private static bool IsLuna(ResolvedTheme theme)
		{
			return theme.Style.ButtonLook == ThemeButtonLook.Luna && theme.Style.ScrollBar.Thumb == ThemeThumbShape.Rounded;
		}

		// A Luna scroll bar part: a rounded face that is light at the top and a tint of the scheme accent below, with a darker tint as edge.
		private static void PaintLunaFace(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool vertical, bool hot, bool pressed, int radius)
		{
			Color accent = theme.Color(ThemeRole.Accent);
			Color top = MxTheme.Lighten(accent, pressed ? 0.45f : hot ? 0.9f : 0.82f);
			Color bottom = MxTheme.Lighten(accent, pressed ? 0.7f : hot ? 0.65f : 0.5f);
			GdiCache.FillGradient(graphics, Rectangle.Inflate(bounds, -1, -1), top, bottom, vertical ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical);
			theme.DrawRounded(graphics, bounds, MxTheme.Lighten(accent, 0.1f), radius);
		}

		private static void PaintButton(Graphics graphics, ResolvedTheme theme, ThemeScrollBarStyle style, in ScrollBarPart part, int direction, bool enabled)
		{
			if (!part.Present || part.Bounds.IsEmpty)
			{
				return;
			}
			Rectangle bounds = part.Bounds;
			bool pressed = enabled && part.Pressed;
			bool hot = enabled && part.Hot;
			if (style.ButtonFaces)
			{
				ThemeRole face = pressed ? ThemeRole.SurfacePressed : hot ? ThemeRole.SurfaceHot : ThemeRole.SurfaceRaised;
				int radius = style.Thumb == ThemeThumbShape.Rectangle ? 0 : Math.Min(theme.Radius(ThemeElement.Button), 3);
				bool vertical = direction < 2;
				if (IsLuna(theme))
				{
					PaintLunaFace(graphics, theme, bounds, vertical, hot, pressed, 3);
				}
				else
				{
					theme.FillFace(graphics, bounds, face, radius, vertical ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical);
				}
				if (IsLuna(theme))
				{
				}
				else if (style.Edge == ThemeBorder.Raised && bounds.Width > 3 && bounds.Height > 3)
				{
					theme.DrawBorder(graphics, bounds, pressed ? ThemeBorder.Sunken : ThemeBorder.Raised, ThemeRole.Border, 0);
				}
				else
				{
					theme.DrawRounded(graphics, bounds, hot || pressed ? ThemeRole.BorderHot : ThemeRole.Border, radius);
				}
				if (pressed)
				{
					bounds.Offset(1, 1);
				}
			}
			else if (hot || pressed)
			{
				graphics.FillRectangle(theme.Brush(pressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot), bounds);
			}
			ThemeRole ink = !enabled ? ThemeRole.TextDisabled : style.ButtonFaces ? ThemeRole.Text : hot || pressed ? ThemeRole.Text : ThemeRole.TextMuted;
			theme.DrawArrow(graphics, bounds, direction, ink, ThemeArrowGlyph.Triangle);
		}

		private static void PaintThumb(Graphics graphics, ResolvedTheme theme, ThemeScrollBarStyle style, in ScrollBarPart thumb, bool vertical)
		{
			Rectangle bounds = thumb.Bounds;
			ThemeRole role = thumb.Pressed ? ThemeRole.ScrollThumbPressed : thumb.Hot ? ThemeRole.ScrollThumbHot : ThemeRole.ScrollThumb;
			int radius = style.Thumb == ThemeThumbShape.Rectangle ? 0 : style.Thumb == ThemeThumbShape.Pill ? 99 : Math.Min(Math.Max(theme.Radius(ThemeElement.Button), 2), 3);
			if (style.Thumb != ThemeThumbShape.Rectangle)
			{
				bounds.Inflate(vertical ? -theme.Scaled(1) : 0, vertical ? 0 : -theme.Scaled(1));
			}
			if (bounds.Width <= 1 || bounds.Height <= 1)
			{
				return;
			}
			SmoothingMode smoothing = graphics.SmoothingMode;
			if (radius > 0)
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
			}
			bool luna = IsLuna(theme);
			if (luna)
			{
				PaintLunaFace(graphics, theme, bounds, vertical, thumb.Hot, thumb.Pressed, 3);
			}
			else if (style.ThumbGradient)
			{
				theme.FillFace(graphics, bounds, role, radius, vertical ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical);
			}
			else
			{
				theme.FillRounded(graphics, bounds, role, radius);
			}
			if (luna)
			{
			}
			else if (style.Edge == ThemeBorder.Raised && radius == 0)
			{
				theme.DrawBorder(graphics, bounds, ThemeBorder.Raised, ThemeRole.Border, 0);
			}
			else
			{
				theme.DrawRounded(graphics, bounds, thumb.Hot || thumb.Pressed ? ThemeRole.BorderHot : ThemeRole.Border, radius);
			}
			graphics.SmoothingMode = smoothing;
			if (style.Grip != ThemeGrip.None)
			{
				PaintGrip(graphics, theme, style.Grip, bounds, vertical);
			}
		}

		// Three marks across the middle of the thumb, at right angles to the direction of the bar.
		private static void PaintGrip(Graphics graphics, ResolvedTheme theme, ThemeGrip grip, Rectangle bounds, bool vertical)
		{
			int length = vertical ? bounds.Width : bounds.Height;
			int along = vertical ? bounds.Height : bounds.Width;
			int step = Math.Max(3, theme.Scaled(3));
			if (along < step * 4 + 2)
			{
				return;
			}
			int mark = Math.Min(length - theme.Scaled(6), theme.Scaled(8));
			if (mark < 3)
			{
				return;
			}
			int cx = bounds.X + bounds.Width / 2;
			int cy = bounds.Y + bounds.Height / 2;
			Pen dark = theme.Pen(ThemeRole.Border);
			Pen light = theme.LightEdgePen;
			for (int i = -1; i <= 1; i++)
			{
				if (grip == ThemeGrip.Lines)
				{
					if (vertical)
					{
						int y = cy + i * step;
						graphics.DrawLine(dark, cx - mark / 2, y, cx + mark / 2 - 1, y);
						graphics.DrawLine(light, cx - mark / 2, y + 1, cx + mark / 2 - 1, y + 1);
					}
					else
					{
						int x = cx + i * step;
						graphics.DrawLine(dark, x, cy - mark / 2, x, cy + mark / 2 - 1);
						graphics.DrawLine(light, x + 1, cy - mark / 2, x + 1, cy + mark / 2 - 1);
					}
				}
				else
				{
					int x = vertical ? cx - 1 : cx + i * step - 1;
					int y = vertical ? cy + i * step - 1 : cy - 1;
					graphics.FillRectangle(theme.LightEdgeBrush, x + 1, y + 1, 2, 2);
					graphics.FillRectangle(theme.Brush(ThemeRole.Border), x, y, 2, 2);
				}
			}
		}
	}
}
