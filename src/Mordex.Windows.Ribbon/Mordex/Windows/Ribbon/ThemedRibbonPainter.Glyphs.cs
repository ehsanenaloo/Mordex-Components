using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal enum ThemedCaptionGlyph
	{
		Minimize,
		Maximize,
		Restore,
		Close,
		Help
	}

	internal static partial class ThemedRibbonPainter
	{
		/// <summary>The pen of glyphs on the ribbon content, in the text color of the toolbar back. A width of 0 is one device pixel.</summary>
		private static Pen GlyphPen(ResolvedTheme theme, bool enabled, int width)
		{
			Color text = theme.Color(ThemeRole.ToolbarText);
			ThemeRole role = enabled ? ThemeRole.Text : ThemeRole.TextDisabled;
			if (text.ToArgb() == theme.Color(ThemeRole.Text).ToArgb())
			{
				return width <= 0 ? theme.Pen(role) : theme.Pen(role, width);
			}
			Color color = enabled ? text : MxTheme.Blend(text, theme.Color(ThemeRole.ToolbarBack), 0.55f);
			return GdiCache.Pen(color, width <= 0 ? 1f : theme.Scaled(width));
		}

		/// <summary>The fill of the selected tab. A light ribbon card in a dark window gives the card color, so the tab opens into it.</summary>
		internal static Color SelectedTabFill(ResolvedTheme theme)
		{
			return theme.Color(ThemeRole.ToolbarText).ToArgb() != theme.Color(ThemeRole.Text).ToArgb() ? theme.Color(ThemeRole.ToolbarBack) : theme.Color(ThemeRole.TabActive);
		}

		/// <summary>A drop-down triangle built from horizontal lines (vertical) or vertical lines (horizontal), like the classic arrows.</summary>
		internal static void Arrow(Graphics graphics, ResolvedTheme theme, Rectangle rectangle, int arrowSize, int direction, bool vertical, bool enabled)
		{
			Pen pen = GlyphPen(theme, enabled, 0);
			int length = arrowSize / 2;
			if (length % 2 == 0)
			{
				length--;
			}
			int depth = length / 2 + 1;
			if (vertical)
			{
				int y = direction <= 0 ? rectangle.Top + (rectangle.Height - depth) / 2 + depth - 1 : rectangle.Top + (rectangle.Height - depth) / 2;
				int x = rectangle.Left + (rectangle.Width - length) / 2;
				while (length >= 0)
				{
					if (length == 1)
					{
						graphics.DrawLine(pen, x, y, x + 0.5f, y);
					}
					else
					{
						graphics.DrawLine(pen, x, y, x + length - 1, y);
					}
					x++;
					length -= 2;
					y += direction;
				}
			}
			else
			{
				int x = direction <= 0 ? rectangle.Left + (rectangle.Width - depth) / 2 + depth - 1 : rectangle.Left + (rectangle.Width - depth) / 2;
				int y = rectangle.Top + (rectangle.Height - length) / 2;
				while (length >= 0)
				{
					if (length == 1)
					{
						graphics.DrawLine(pen, x - direction, y, x, y);
					}
					else
					{
						graphics.DrawLine(pen, x, y, x, y + length - 1);
					}
					x += direction;
					length -= 2;
					y++;
				}
			}
		}

		/// <summary>A chevron that points up or down, centered in the bounds.</summary>
		internal static void Chevron(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool up, bool enabled)
		{
			int half = theme.Scaled(4);
			int depth = half;
			int cx = bounds.X + bounds.Width / 2;
			int cy = bounds.Y + bounds.Height / 2;
			int near = up ? cy + depth / 2 : cy - depth / 2;
			int far = up ? cy - depth / 2 : cy + depth / 2;
			Pen pen = theme.Pen(enabled ? ThemeRole.Text : ThemeRole.TextDisabled, 1);
			graphics.DrawLine(pen, cx - half, near, cx, far);
			graphics.DrawLine(pen, cx, far, cx + half, near);
		}

		/// <summary>A double chevron that points right: the overflow mark of the quick access toolbar.</summary>
		internal static void DoubleChevron(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			int half = theme.Scaled(3);
			int depth = theme.Scaled(3);
			int gap = theme.Scaled(4);
			int cx = bounds.X + bounds.Width / 2 - gap / 2;
			int cy = bounds.Y + bounds.Height / 2;
			Pen pen = theme.Pen(ThemeRole.Text, 1);
			for (int i = 0; i < 2; i++)
			{
				int x = cx + i * gap;
				graphics.DrawLine(pen, x, cy - half, x + depth, cy);
				graphics.DrawLine(pen, x + depth, cy, x, cy + half);
			}
		}

		/// <summary>The mark of the customize button of the quick access toolbar: a line over a drop-down triangle.</summary>
		internal static void CustomizeGlyph(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool enabled)
		{
			int w = theme.Scaled(7);
			if (w % 2 == 0)
			{
				w++;
			}
			int x = bounds.X + (bounds.Width - w) / 2;
			int y = bounds.Y + bounds.Height / 2 - 3;
			graphics.DrawLine(theme.Pen(enabled ? ThemeRole.Text : ThemeRole.TextDisabled), x, y, x + w - 1, y);
			Arrow(graphics, theme, new Rectangle(bounds.X, y + 3, bounds.Width, 4), w * 2, 1, true, enabled);
		}

		/// <summary>A check mark in the bounds.</summary>
		internal static void Check(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemeRole role)
		{
			int size = System.Math.Min(bounds.Width, bounds.Height);
			int w = System.Math.Max(5, size * 5 / 8);
			int x = bounds.X + (bounds.Width - w) / 2;
			int y = bounds.Y + (bounds.Height - w * 2 / 3) / 2;
			int knee = w * 2 / 5;
			Pen pen = theme.Pen(role, 2);
			SmoothingMode previous = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawLine(pen, x, y + w / 3, x + knee, y + w * 2 / 3);
			graphics.DrawLine(pen, x + knee, y + w * 2 / 3, x + w, y);
			graphics.SmoothingMode = previous;
		}

		/// <summary>The dialog launcher mark of a group: a corner bracket with an arrow to the lower right.</summary>
		internal static void DialogLauncher(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			int s = theme.Scaled(8);
			int x = bounds.X + (bounds.Width - s) / 2 + 1;
			int y = bounds.Y + (bounds.Height - s) / 2;
			Pen pen = theme.Pen(ThemeRole.TextMuted);
			graphics.DrawLine(pen, x, y, x + s - 3, y);
			graphics.DrawLine(pen, x, y, x, y + s - 3);
			int head = theme.Scaled(3);
			graphics.DrawLine(pen, x + 2, y + 2, x + s - 1, y + s - 1);
			graphics.DrawLine(pen, x + s - 1, y + s - 1, x + s - 1 - head, y + s - 1);
			graphics.DrawLine(pen, x + s - 1, y + s - 1, x + s - 1, y + s - 1 - head);
		}

		/// <summary>The glyph of a caption button: minimize, maximize, restore, close or help.</summary>
		internal static void CaptionGlyph(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ThemedCaptionGlyph glyph, ThemeRole role)
		{
			int s = System.Math.Min(theme.Scaled(10), System.Math.Min(bounds.Width, bounds.Height) - 4);
			if (s < 4)
			{
				return;
			}
			int x = bounds.X + (bounds.Width - s) / 2;
			int y = bounds.Y + (bounds.Height - s) / 2;
			Pen pen = theme.Pen(role);
			switch (glyph)
			{
			case ThemedCaptionGlyph.Minimize:
				graphics.DrawLine(pen, x, y + s - 2, x + s - 1, y + s - 2);
				break;
			case ThemedCaptionGlyph.Maximize:
				graphics.DrawRectangle(pen, x, y, s - 1, s - 1);
				break;
			case ThemedCaptionGlyph.Restore:
			{
				int inner = s - 3;
				graphics.DrawRectangle(pen, x, y + 3, inner - 1, inner - 1);
				graphics.DrawLine(pen, x + 3, y + 2, x + 3, y);
				graphics.DrawLine(pen, x + 3, y, x + s - 1, y);
				graphics.DrawLine(pen, x + s - 1, y, x + s - 1, y + s - 4);
				graphics.DrawLine(pen, x + s - 1, y + s - 4, x + inner, y + s - 4);
				break;
			}
			case ThemedCaptionGlyph.Close:
			{
				graphics.DrawLine(pen, x, y, x + s - 1, y + s - 1);
				graphics.DrawLine(pen, x + s - 1, y, x, y + s - 1);
				break;
			}
			default:
			{
				SmoothingMode previous = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				int r = s / 2;
				graphics.DrawArc(pen, x + s / 4, y, r + 1, r, 200, 270);
				graphics.DrawLine(pen, x + s / 2, y + r, x + s / 2, y + s - 4);
				graphics.DrawLine(pen, x + s / 2, y + s - 1, x + s / 2, y + s - 1);
				graphics.SmoothingMode = previous;
				break;
			}
			}
		}
	}
}
