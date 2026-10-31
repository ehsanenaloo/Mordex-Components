using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal static partial class ThemedRibbonPainter
	{
		/// <summary>The line after a group or a button group.</summary>
		internal static void GroupSeparator(Graphics graphics, ResolvedTheme theme, Rectangle groupBounds)
		{
			int x = groupBounds.Right - 2;
			graphics.DrawLine(theme.Pen(ThemeRole.Border), x, groupBounds.Y + 2, x, groupBounds.Bottom - 3);
		}

		/// <summary>A separator command: one line, centered.</summary>
		internal static void VerticalSeparator(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			int x = bounds.X + bounds.Width / 2;
			graphics.DrawLine(theme.Pen(ThemeRole.Border), x, bounds.Y + 5, x, bounds.Bottom - 6);
		}

		/// <summary>A collapsed group: nothing while idle, a face while hot, and the pressed face while its popup is open.</summary>
		internal static void CollapsedGroup(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool hot, bool open)
		{
			if (open)
			{
				Face(graphics, theme, bounds, ThemeRole.SurfacePressed, ThemeRole.BorderPressed);
			}
			else if (hot)
			{
				Face(graphics, theme, bounds, ThemeRole.SurfaceHot, ThemeRole.BorderHot);
			}
		}

		/// <summary>The frame behind the icon of a collapsed group.</summary>
		internal static void CollapsedIcon(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			Face(graphics, theme, bounds, ThemeRole.SurfaceRaised, ThemeRole.Border);
		}

		private const int SlotQuickAccessBorder = 43;
		private const int SlotQuickAccessBorderGradient = 44;
		private const int SlotQuickAccessFill = 45;
		private const int SlotQuickAccessFillGradient = 46;

		/// <summary>The quick access toolbar strip: a flat fill and a line above and below.</summary>
		internal static void QuickAccessBar(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			if (RibbonThemeStyle.For(theme.Theme).QuickAccessCapsule)
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.Surface), bounds);
				Rectangle capsule = Rectangle.Inflate(bounds, -1, -2);
				SmoothingMode previous = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (GraphicsPath path = RoundedPath(new Rectangle(capsule.X, capsule.Y, capsule.Width - 1, capsule.Height - 1), capsule.Height / 2))
				{
					Rectangle area = new Rectangle(capsule.X, capsule.Y - 1, capsule.Width, capsule.Height + 2);
					if (theme.Theme.HasOfficePalette && theme.Theme.Family == "Office 2007")
					{
						// The capsule of Office 2007 takes the quick access colors of the scheme for its fill and its rim.
						using (LinearGradientBrush brush = new LinearGradientBrush(area, theme.OfficeColor(SlotQuickAccessFill), theme.OfficeColorOr(SlotQuickAccessFillGradient, SlotQuickAccessFill), LinearGradientMode.Vertical))
						{
							graphics.FillPath(brush, path);
						}
						using (LinearGradientBrush rim = new LinearGradientBrush(area, theme.OfficeColor(SlotQuickAccessBorder), theme.OfficeColorOr(SlotQuickAccessBorderGradient, SlotQuickAccessBorder), LinearGradientMode.Vertical))
						using (Pen pen = new Pen(rim))
						{
							graphics.DrawPath(pen, path);
						}
						graphics.SmoothingMode = previous;
						return;
					}
					using (LinearGradientBrush brush = new LinearGradientBrush(area, Lighten(theme.Color(ThemeRole.SurfaceRaised), 0.4f), theme.Color(ThemeRole.SurfaceAlt), LinearGradientMode.Vertical))
					{
						graphics.FillPath(brush, path);
					}
					graphics.DrawPath(theme.Pen(ThemeRole.Border), path);
				}
				graphics.SmoothingMode = previous;
				return;
			}
			graphics.FillRectangle(theme.Brush(ThemeRole.Surface), bounds);
			Pen border = theme.Pen(ThemeRole.Border);
			graphics.DrawLine(border, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
			graphics.DrawLine(border, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
		}

		/// <summary>The body of a popup window: a raised surface with the border of the theme.</summary>
		internal static void Popup(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemeRole fill)
		{
			graphics.FillRectangle(theme.Brush(fill), bounds);
			graphics.DrawRectangle(theme.Pen(ThemeRole.Border), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
		}

		/// <summary>The band of a contextual tab group above the tab strip.</summary>
		internal static void ContextualBand(Graphics graphics, ResolvedTheme theme, Rectangle bounds, Color accent, bool topLine)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			graphics.FillRectangle(theme.Brush(ThemeRole.AccentSubtle), bounds);
			if (topLine)
			{
				Brush line = accent.IsEmpty ? theme.Brush(ThemeRole.Accent) : GdiCache.Brush(accent);
				graphics.FillRectangle(line, bounds.X, bounds.Y, bounds.Width, theme.Scaled(2));
			}
		}

		/// <summary>The line between two compressed tabs.</summary>
		internal static void TabSeparator(Graphics graphics, ResolvedTheme theme, Rectangle tabBounds)
		{
			int x = tabBounds.Right + 1;
			graphics.DrawLine(theme.Pen(ThemeRole.Border), x, tabBounds.Top + 4, x, tabBounds.Bottom - 5);
		}

		/// <summary>A small scroll or expand button of a tab strip, a content area or a gallery.</summary>
		internal static void ScrollButton(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool hot, bool pushed, bool enabled, bool overlapBelow)
		{
			if (overlapBelow)
			{
				bounds.Height++;
			}
			ThemeRole fill = ThemeRole.SurfaceRaised;
			ThemeRole border = enabled ? ThemeRole.Border : ThemeRole.BorderDisabled;
			if (enabled && pushed)
			{
				fill = ThemeRole.SurfacePressed;
				border = ThemeRole.BorderPressed;
			}
			else if (enabled && hot)
			{
				fill = ThemeRole.SurfaceHot;
				border = ThemeRole.BorderHot;
			}
			Face(graphics, theme, bounds, fill, border);
		}

		/// <summary>The frame of a gallery in the ribbon: a raised surface that turns hot under the mouse.</summary>
		internal static void GalleryFrame(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool hot)
		{
			Face(graphics, theme, bounds, hot ? ThemeRole.SurfaceHot : ThemeRole.SurfaceRaised, ThemeRole.Border);
		}

		/// <summary>The mark behind a checked menu item or a checked gallery entry.</summary>
		internal static void CheckedBackground(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			Face(graphics, theme, bounds, ThemeRole.AccentSubtle, ThemeRole.Accent);
		}

		/// <summary>The bar at the bottom or the corner of a resizable drop-down.</summary>
		internal static void ResizeBar(Graphics graphics, ResolvedTheme theme, Rectangle bar, bool heightOnly, Rectangle grip)
		{
			graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceAlt), bar);
			graphics.DrawLine(theme.Pen(ThemeRole.Border), bar.X, bar.Y, bar.Right - 1, bar.Y);
			Brush dot = theme.Brush(ThemeRole.TextMuted);
			int size = theme.Scaled(2);
			int step = theme.Scaled(4);
			if (heightOnly)
			{
				int x = bar.X + (bar.Width - step * 4) / 2;
				int y = bar.Y + (bar.Height - size) / 2;
				for (int i = 0; i < 4; i++)
				{
					graphics.FillRectangle(dot, x + i * step, y, size, size);
				}
				return;
			}
			int cornerX = grip.Right - step - size;
			int cornerY = grip.Bottom - step - size;
			graphics.FillRectangle(dot, cornerX, cornerY, size, size);
			graphics.FillRectangle(dot, cornerX - step, cornerY, size, size);
			graphics.FillRectangle(dot, cornerX, cornerY - step, size, size);
		}

		/// <summary>The side area of the backstage menu, with a line on its right edge.</summary>
		internal static void BackstageSide(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceAlt), bounds);
			graphics.DrawLine(theme.Pen(ThemeRole.Border), bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
		}

		/// <summary>A side tab or a button of the backstage menu. The selected one has an accent bar on its left edge.</summary>
		internal static void BackstageRow(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool hot, bool selected, bool pushed)
		{
			if (selected)
			{
				graphics.FillRectangle(theme.Brush(hot ? ThemeRole.SurfaceHot : ThemeRole.AccentSubtle), bounds);
				graphics.FillRectangle(theme.Brush(ThemeRole.Accent), bounds.X, bounds.Y, theme.Scaled(3), bounds.Height);
			}
			else if (pushed && hot)
			{
				Face(graphics, theme, bounds, ThemeRole.SurfacePressed, ThemeRole.BorderPressed);
			}
			else if (hot)
			{
				Face(graphics, theme, bounds, ThemeRole.SurfaceHot, ThemeRole.BorderHot);
			}
		}

		/// <summary>The line under the tab strip at the top of the backstage view.</summary>
		internal static void BackstageTopLine(Graphics graphics, ResolvedTheme theme, int width)
		{
			graphics.DrawLine(theme.Pen(ThemeRole.Border), 0, 0, width - 1, 0);
		}

		/// <summary>The application button: a disc in the accent color. A fade from the normal to the hot color uses the alpha.</summary>
		internal static void Orb(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemeRole fill, int alpha)
		{
			bounds.Inflate(-1, -1);
			SmoothingMode previous = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Brush brush = alpha >= 255 ? theme.Brush(fill) : GdiCache.Brush(Color.FromArgb(alpha, theme.Color(fill)));
			graphics.FillEllipse(brush, bounds);
			if (theme.Shape.IsFlat && !theme.Theme.Style.Gloss)
			{
				graphics.DrawEllipse(theme.Pen(ThemeRole.TextOnAccent, 1), Rectangle.Inflate(bounds, -2, -2));
			}
			else
			{
				GlassOrb(graphics, theme, bounds, fill, alpha);
			}
			graphics.SmoothingMode = previous;
		}

		// A glass disc for a theme with gradients or gloss: a glow in the lower half, a light lens in the upper half and a dark rim.
		private static void GlassOrb(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemeRole fill, int alpha)
		{
			Color baseColor = theme.Color(fill);
			Color light = theme.Color(ThemeRole.TextOnAccent);
			if (bounds.Width < 24 && bounds.Width > 2 && bounds.Height > 2)
			{
				// The lens and the glow are too small to read here: one smooth fade from a light top to a lit lower edge.
				using (LinearGradientBrush brush = new LinearGradientBrush(bounds, baseColor, baseColor, LinearGradientMode.Vertical))
				{
					brush.InterpolationColors = new ColorBlend(3)
					{
						Positions = new[] { 0f, 0.55f, 1f },
						Colors = new[]
						{
							Color.FromArgb(alpha, Mix(baseColor, Color.White, 0.7f)),
							Color.FromArgb(alpha, baseColor),
							Color.FromArgb(alpha, Mix(baseColor, Color.White, 0.4f))
						}
					};
					graphics.FillEllipse(brush, bounds);
				}
				graphics.DrawEllipse(GdiCache.Pen(Color.FromArgb(alpha, Mix(baseColor, Color.Black, 0.4f))), bounds);
				return;
			}
			Color glow = Color.FromArgb(alpha * 170 / 255, Mix(baseColor, light, 0.55f));
			graphics.FillEllipse(GdiCache.Brush(glow), bounds.X + bounds.Width / 6, bounds.Y + bounds.Height / 2, bounds.Width * 2 / 3, System.Math.Max(1, bounds.Height / 2 - 1));
			Color lens = Color.FromArgb(alpha * 120 / 255, Mix(baseColor, light, 0.8f));
			graphics.FillEllipse(GdiCache.Brush(lens), bounds.X + 2, bounds.Y + 1, System.Math.Max(1, bounds.Width - 4), System.Math.Max(1, bounds.Height / 2 - 1));
			graphics.DrawEllipse(GdiCache.Pen(Color.FromArgb(alpha, Mix(baseColor, Color.Black, 0.4f))), bounds);
		}

		private static Color Mix(Color from, Color to, float amount)
		{
			return Color.FromArgb(from.A, (int)(from.R + (to.R - from.R) * amount), (int)(from.G + (to.G - from.G) * amount), (int)(from.B + (to.B - from.B) * amount));
		}

		/// <summary>A key tip badge.</summary>
		internal static void KeyTip(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			SmoothingMode previous = BeginShape(graphics, theme);
			theme.FillRounded(graphics, bounds, ThemeRole.TooltipBack, theme.Shape.CornerRadius);
			theme.DrawRounded(graphics, bounds, ThemeRole.Border, theme.Shape.CornerRadius);
			graphics.SmoothingMode = previous;
		}

		/// <summary>A caption button of the title bar: flat, hot or pressed, and red for the close button.</summary>
		internal static void CaptionButton(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool hot, bool pressed, bool close)
		{
			if (!hot && !pressed)
			{
				return;
			}
			if (close)
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.Error), bounds);
				if (pressed)
				{
					graphics.FillRectangle(GdiCache.Brush(Color.FromArgb(90, theme.Color(ThemeRole.Shadow))), bounds);
				}
				return;
			}
			graphics.FillRectangle(theme.Brush(pressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot), bounds);
		}
	}
}
