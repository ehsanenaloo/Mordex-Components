using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>Which caption button is meant, for hover and press.</summary>
	internal enum FrameButton
	{
		None,
		Minimize,
		Maximize,
		Close
	}

	/// <summary>Paints the caption and the frame of a window in the look of a theme. It paints into a graphics surface; it knows nothing about messages.</summary>
	internal static class ThemedFramePainter
	{
		internal sealed class State
		{
			internal MxTheme Theme;
			internal FormFrameStyle Style;
			internal FormFrameMetrics Metrics;
			internal bool Active;
			internal FrameButton Hot;
			internal FrameButton Pressed;
			internal string Title;
			internal Bitmap Icon;
			internal Font Font;
			internal bool Mirrored;
			internal bool Maximized;
			internal bool MinimizeEnabled;
			internal bool MaximizeEnabled;
			internal int Dpi = 96;
			internal IFrameCaptionExtension Extension;
		}

		/// <summary>Paints the whole frame: the caption (double buffered) and the three borders.</summary>
		internal static void Paint(Graphics target, Size window, State s)
		{
			FormFrameMetrics m = s.Metrics;
			FormFrameColors colors = FormFrameColors.Create(s.Theme, s.Style.Kind, s.Active);
			int captionHeight = m.Client.Y - m.Visible.Y;
			if (captionHeight > 0 && m.Visible.Width > 0)
			{
				using (Bitmap buffer = new Bitmap(m.Visible.Width, captionHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
				{
					buffer.SetResolution(s.Dpi, s.Dpi);
					using (Graphics g = Graphics.FromImage(buffer))
					{
						g.TranslateTransform(-m.Visible.X, -m.Visible.Y);
						PaintCaption(g, s, colors);
					}
					target.DrawImageUnscaled(buffer, m.Visible.X, m.Visible.Y);
				}
			}
			if (!m.Maximized)
			{
				PaintBorders(target, s, colors);
			}
		}

		/// <summary>Paints the caption only, for a change of hover or press.</summary>
		internal static void PaintCaptionOnly(Graphics target, State s)
		{
			FormFrameMetrics m = s.Metrics;
			FormFrameColors colors = FormFrameColors.Create(s.Theme, s.Style.Kind, s.Active);
			int captionHeight = m.Client.Y - m.Visible.Y;
			if (captionHeight <= 0 || m.Visible.Width <= 0)
			{
				return;
			}
			using (Bitmap buffer = new Bitmap(m.Visible.Width, captionHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
			{
				buffer.SetResolution(s.Dpi, s.Dpi);
				using (Graphics g = Graphics.FromImage(buffer))
				{
					g.TranslateTransform(-m.Visible.X, -m.Visible.Y);
					PaintCaption(g, s, colors);
				}
				target.DrawImageUnscaled(buffer, m.Visible.X, m.Visible.Y);
			}
		}

		private static void PaintCaption(Graphics g, State s, FormFrameColors c)
		{
			FormFrameMetrics m = s.Metrics;
			Rectangle caption = m.Caption;
			int radius = m.Maximized ? 0 : (int)Math.Round(s.Style.TopRadius * s.Dpi / 96f);
			g.SmoothingMode = SmoothingMode.None;
			switch (s.Style.Kind)
			{
			case FormFrameKind.Luna:
				PaintLunaCaption(g, caption, radius, c);
				break;
			case FormFrameKind.Classic:
				PaintClassicCaption(g, s, caption, c);
				break;
			case FormFrameKind.Glass:
				PaintGlassCaption(g, caption, radius, c);
				break;
			case FormFrameKind.Browser:
				PaintBrowserCaption(g, caption, c);
				break;
			default:
				PaintGradientCaption(g, caption, c, m.Maximized);
				break;
			}
			if (s.Icon != null && !m.Icon.IsEmpty)
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(s.Icon, m.Icon);
			}
			PaintTitle(g, s, c);
			if (s.Extension != null)
			{
				GraphicsState saved = g.Save();
				g.SetClip(caption, System.Drawing.Drawing2D.CombineMode.Intersect);
				s.Extension.Paint(g, caption, s.Active);
				g.Restore(saved);
			}
			PaintButton(g, s, c, FrameButton.Minimize, m.MinimizeButton);
			PaintButton(g, s, c, FrameButton.Maximize, m.MaximizeButton);
			PaintButton(g, s, c, FrameButton.Close, m.CloseButton);
		}

		private static GraphicsPath TopRounded(Rectangle r, int radius)
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

		private static void FillVertical(Graphics g, Rectangle r, GraphicsPath clip, Color[] colors, float[] positions)
		{
			if (r.Width <= 0 || r.Height <= 0)
			{
				return;
			}
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(r.X, r.Y - 1, r.Width, r.Height + 2), colors[0], colors[colors.Length - 1], LinearGradientMode.Vertical))
			{
				brush.InterpolationColors = new ColorBlend(colors.Length) { Colors = colors, Positions = positions };
				if (clip != null)
				{
					g.FillPath(brush, clip);
				}
				else
				{
					g.FillRectangle(brush, r);
				}
			}
		}

		private static void PaintLunaCaption(Graphics g, Rectangle caption, int radius, FormFrameColors c)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = TopRounded(new Rectangle(caption.X, caption.Y, caption.Width - 1, caption.Height + 1), radius))
			{
				Color b = c.CaptionMiddle;
				// The bands of the Luna caption of Windows XP, measured on the real caption: a pale top line, a dark body, a
				// brighter lower third and a dark last line. The pale and bright bands also turn a few degrees toward cyan.
				float[] lightness = { 14f, 14f, 3.5f, 1f, 0f, 0f, 3.7f, 5f, 6.3f, 5.8f, 4.7f, -6f, -6f };
				float[] hue = { -4.5f, -4.5f, -2.4f, -1f, 0f, 0f, 0f, -1f, -3f, -2.4f, -1f, 3f, 3f };
				Color[] colors = new Color[lightness.Length];
				for (int i = 0; i < colors.Length; i++)
				{
					colors[i] = ShiftHsl(b, hue[i] / 360f, lightness[i] / 100f);
				}
				FillVertical(g, caption, path, colors, new[] { 0f, 0.07f, 0.14f, 0.21f, 0.3f, 0.45f, 0.57f, 0.64f, 0.71f, 0.79f, 0.84f, 0.88f, 1f });
				g.DrawPath(GdiCache.Pen(c.FrameOuter), path);
			}
			g.SmoothingMode = SmoothingMode.None;
		}

		// The color with its HSL hue and lightness moved (both 0 to 1), saturation kept.
		private static Color ShiftHsl(Color color, float hueDelta, float delta)
		{
			float r = color.R / 255f;
			float gr = color.G / 255f;
			float bl = color.B / 255f;
			float max = Math.Max(r, Math.Max(gr, bl));
			float min = Math.Min(r, Math.Min(gr, bl));
			float l = (max + min) / 2f;
			float h = 0f;
			float s = 0f;
			if (max > min)
			{
				float d = max - min;
				s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
				h = max == r ? (gr - bl) / d + (gr < bl ? 6f : 0f) : max == gr ? (bl - r) / d + 2f : (r - gr) / d + 4f;
				h /= 6f;
			}
			h = (h + hueDelta + 1f) % 1f;
			l = Math.Max(0f, Math.Min(1f, l + delta));
			if (s == 0f)
			{
				int gray = (int)Math.Round(l * 255f);
				return Color.FromArgb(color.A, gray, gray, gray);
			}
			float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
			float p = 2f * l - q;
			return Color.FromArgb(color.A, Channel(p, q, h + 1f / 3f), Channel(p, q, h), Channel(p, q, h - 1f / 3f));
		}

		private static int Channel(float p, float q, float t)
		{
			if (t < 0f)
			{
				t += 1f;
			}
			if (t > 1f)
			{
				t -= 1f;
			}
			float v = t < 1f / 6f ? p + (q - p) * 6f * t : t < 0.5f ? q : t < 2f / 3f ? p + (q - p) * (2f / 3f - t) * 6f : p;
			return Math.Max(0, Math.Min(255, (int)Math.Round(v * 255f)));
		}

		private static void PaintClassicCaption(Graphics g, State s, Rectangle caption, FormFrameColors c)
		{
			FormFrameMetrics m = s.Metrics;
			int b = Math.Max(1, m.Border);
			g.FillRectangle(GdiCache.Brush(c.Frame), caption);
			if (!m.Maximized)
			{
				Raised(g, new Rectangle(caption.X, caption.Y, caption.Width, caption.Height + 1), c, true);
			}
			Rectangle bar = new Rectangle(caption.X + b, caption.Y + b, caption.Width - b * 2, caption.Height - b - 1);
			if (m.Maximized)
			{
				bar = new Rectangle(caption.X, caption.Y, caption.Width, caption.Height - 1);
			}
			if (bar.Width > 0 && bar.Height > 0)
			{
				using (LinearGradientBrush brush = new LinearGradientBrush(bar, c.CaptionTop, c.CaptionBottom, LinearGradientMode.Horizontal))
				{
					g.FillRectangle(brush, bar);
				}
			}
		}

		private static void PaintGlassCaption(Graphics g, Rectangle caption, int radius, FormFrameColors c)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = TopRounded(new Rectangle(caption.X, caption.Y, caption.Width - 1, caption.Height + 1), radius))
			{
				FillVertical(g, caption, path, new[] { c.CaptionTop, c.CaptionMiddle, c.CaptionMiddle, c.CaptionBottom }, new[] { 0f, 0.48f, 0.5f, 1f });
				g.DrawPath(GdiCache.Pen(c.FrameOuter), path);
			}
			g.SmoothingMode = SmoothingMode.None;
			g.DrawLine(GdiCache.Pen(Color.FromArgb(150, 255, 255, 255)), caption.X + radius, caption.Y + 1, caption.Right - radius - 2, caption.Y + 1);
		}

		private static void PaintBrowserCaption(Graphics g, Rectangle caption, FormFrameColors c)
		{
			if (caption.Width <= 0 || caption.Height <= 0)
			{
				return;
			}
			if (c.CaptionTop == c.CaptionBottom)
			{
				g.FillRectangle(GdiCache.Brush(c.CaptionTop), caption);
				return;
			}
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(caption.X - 1, caption.Y, caption.Width + 2, caption.Height), c.CaptionTop, c.CaptionBottom, LinearGradientMode.Horizontal))
			{
				g.FillRectangle(brush, caption);
			}
		}

		private static void PaintGradientCaption(Graphics g, Rectangle caption, FormFrameColors c, bool maximized)
		{
			FillVertical(g, caption, null, new[] { c.CaptionTop, c.CaptionBottom }, new[] { 0f, 1f });
			if (!maximized)
			{
				g.DrawLine(GdiCache.Pen(c.FrameOuter), caption.X, caption.Y, caption.Right - 1, caption.Y);
				g.DrawLine(GdiCache.Pen(c.FrameOuter), caption.X, caption.Y, caption.X, caption.Bottom);
				g.DrawLine(GdiCache.Pen(c.FrameOuter), caption.Right - 1, caption.Y, caption.Right - 1, caption.Bottom);
			}
		}

		private static void Raised(Graphics g, Rectangle r, FormFrameColors c, bool outer)
		{
			Pen light = GdiCache.Pen(c.FrameInner);
			Pen dark = GdiCache.Pen(c.FrameOuter);
			Pen face = GdiCache.Pen(SystemColors.ControlDark);
			g.DrawLine(light, r.X, r.Y, r.Right - 2, r.Y);
			g.DrawLine(light, r.X, r.Y, r.X, r.Bottom - 2);
			g.DrawLine(dark, r.Right - 1, r.Y, r.Right - 1, r.Bottom - 1);
			g.DrawLine(dark, r.X, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
			if (outer)
			{
				g.DrawLine(face, r.Right - 2, r.Y + 1, r.Right - 2, r.Bottom - 2);
				g.DrawLine(face, r.X + 1, r.Bottom - 2, r.Right - 2, r.Bottom - 2);
				g.DrawLine(GdiCache.Pen(c.Frame), r.X + 1, r.Y + 1, r.Right - 3, r.Y + 1);
				g.DrawLine(GdiCache.Pen(c.Frame), r.X + 1, r.Y + 1, r.X + 1, r.Bottom - 3);
			}
		}

		private static void PaintTitle(Graphics g, State s, FormFrameColors c)
		{
			Rectangle area = s.Metrics.Title;
			if (area.Width <= 4 || string.IsNullOrEmpty(s.Title) || s.Font == null || (s.Extension != null && s.Extension.HidesTitle))
			{
				return;
			}
			bool center = s.Style.Kind == FormFrameKind.Glass || s.Style.Kind == FormFrameKind.Gradient;
			using (StringFormat format = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter, LineAlignment = StringAlignment.Center, Alignment = center ? StringAlignment.Center : StringAlignment.Near })
			{
				if (s.Mirrored)
				{
					format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				}
				TextRenderingHint hint = g.TextRenderingHint;
				g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				RectangleF box = area;
				if (!c.TextShadow.IsEmpty)
				{
					g.DrawString(s.Title, s.Font, GdiCache.Brush(c.TextShadow), new RectangleF(box.X + 1, box.Y + 1, box.Width, box.Height), format);
				}
				g.DrawString(s.Title, s.Font, GdiCache.Brush(c.Text), box, format);
				g.TextRenderingHint = hint;
			}
		}

		private static void PaintButton(Graphics g, State s, FormFrameColors c, FrameButton which, Rectangle r)
		{
			if (r.IsEmpty)
			{
				return;
			}
			bool hot = s.Hot == which;
			bool pressed = s.Pressed == which && hot;
			bool enabled = which != FrameButton.Minimize || s.MinimizeEnabled;
			if (which == FrameButton.Maximize)
			{
				enabled = s.MaximizeEnabled;
			}
			Color glyph = c.Glyph;
			switch (s.Style.Kind)
			{
			case FormFrameKind.Luna:
				PaintLunaButton(g, s, which, r, hot, pressed);
				glyph = Color.White;
				break;
			case FormFrameKind.Classic:
				PaintClassicButton(g, r, pressed);
				if (pressed)
				{
					r.Offset(1, 1);
				}
				break;
			case FormFrameKind.Glass:
				PaintGlassButton(g, s, c, which, r, hot, pressed);
				if (which == FrameButton.Close)
				{
					glyph = Color.White;
				}
				break;
			default:
			{
				if (s.Theme.Family == "Office 2010")
				{
					PaintGlassButton(g, s, c, which, r, hot, pressed);
					if (which == FrameButton.Close)
					{
						glyph = Color.White;
					}
					break;
				}
				Color fill = Color.Empty;
				if (which == FrameButton.Close && hot)
				{
					fill = pressed ? Color.FromArgb(160, 38, 36) : Color.FromArgb(214, 64, 56);
					glyph = Color.White;
				}
				else if (hot)
				{
					fill = Color.FromArgb(pressed ? 90 : 55, s.Theme.Kind == ThemeKind.Dark || MxTheme.Luminance(c.CaptionMiddle) < 0.45 ? Color.White : Color.Black);
				}
				if (!fill.IsEmpty)
				{
					g.FillRectangle(GdiCache.Brush(fill), r);
				}
				break;
			}
			}
			if (!enabled)
			{
				glyph = MxTheme.Blend(glyph, c.CaptionMiddle, 0.6f);
			}
			DrawGlyph(g, s, which, r, glyph);
		}

		// The glass caption buttons of Office 2007: a lit box with a dark edge, red for close.
		private static void PaintGlassButton(Graphics g, State s, FormFrameColors c, FrameButton which, Rectangle r, bool hot, bool pressed)
		{
			Color top, bottom, edge;
			if (which == FrameButton.Close)
			{
				top = hot ? Color.FromArgb(244, 160, 140) : Color.FromArgb(226, 150, 132);
				bottom = hot ? Color.FromArgb(214, 70, 44) : Color.FromArgb(190, 76, 56);
				edge = Color.FromArgb(120, 56, 40);
			}
			else
			{
				top = Color.FromArgb(hot ? 245 : 200, 255, 255, 255);
				bottom = MxTheme.Blend(c.CaptionBottom, Color.White, hot ? 0.45f : 0.2f);
				edge = MxTheme.Darken(c.FrameOuter, 0.15f);
			}
			if (pressed)
			{
				top = MxTheme.Darken(top, 0.15f);
				bottom = MxTheme.Darken(bottom, 0.15f);
			}
			Rectangle body = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(body.X, body.Y, body.Width, body.Height + 1), top, bottom, LinearGradientMode.Vertical))
			{
				brush.InterpolationColors = new ColorBlend(4) { Colors = new[] { top, MxTheme.Blend(top, bottom, 0.45f), MxTheme.Darken(bottom, 0.06f), bottom }, Positions = new[] { 0f, 0.48f, 0.5f, 1f } };
				g.FillRectangle(brush, body);
			}
			g.DrawRectangle(GdiCache.Pen(Color.FromArgb(150, edge)), body);
			g.DrawLine(GdiCache.Pen(Color.FromArgb(180, 255, 255, 255)), body.X + 1, body.Y + 1, body.Right - 1, body.Y + 1);
		}

		private static void PaintLunaButton(Graphics g, State s, FrameButton which, Rectangle r, bool hot, bool pressed)
		{
			Color top, bottom;
			if (which == FrameButton.Close)
			{
				top = Color.FromArgb(226, 133, 108);
				bottom = Color.FromArgb(200, 70, 40);
			}
			else
			{
				Color caption = s.Theme.GetColor(s.Active ? ThemeRole.Caption : ThemeRole.CaptionInactive);
				top = MxTheme.Lighten(caption, 0.35f);
				bottom = MxTheme.Darken(caption, 0.05f);
			}
			if (hot)
			{
				top = MxTheme.Lighten(top, 0.25f);
				bottom = MxTheme.Lighten(bottom, 0.2f);
			}
			if (pressed)
			{
				top = MxTheme.Darken(top, 0.15f);
				bottom = MxTheme.Darken(bottom, 0.15f);
			}
			int radius = (int)Math.Round(3 * s.Dpi / 96f);
			Rectangle body = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath path = RoundedRect(body, radius))
			using (LinearGradientBrush brush = new LinearGradientBrush(body, top, bottom, LinearGradientMode.ForwardDiagonal))
			{
				g.FillPath(brush, path);
				g.DrawPath(GdiCache.Pen(Color.White, Math.Max(1f, s.Dpi / 96f)), path);
			}
			g.SmoothingMode = SmoothingMode.None;
		}

		private static void PaintClassicButton(Graphics g, Rectangle r, bool pressed)
		{
			g.FillRectangle(GdiCache.Brush(SystemColors.Control), r);
			Pen light = GdiCache.Pen(SystemColors.ControlLightLight);
			Pen dark = GdiCache.Pen(SystemColors.ControlDarkDark);
			Pen mid = GdiCache.Pen(SystemColors.ControlDark);
			if (pressed)
			{
				g.DrawRectangle(mid, r.X, r.Y, r.Width - 1, r.Height - 1);
				g.DrawRectangle(dark, r.X - 1, r.Y - 1, r.Width, r.Height);
				return;
			}
			g.DrawLine(light, r.X, r.Y, r.Right - 2, r.Y);
			g.DrawLine(light, r.X, r.Y, r.X, r.Bottom - 2);
			g.DrawLine(dark, r.Right - 1, r.Y, r.Right - 1, r.Bottom - 1);
			g.DrawLine(dark, r.X, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
			g.DrawLine(mid, r.Right - 2, r.Y + 1, r.Right - 2, r.Bottom - 2);
			g.DrawLine(mid, r.X + 1, r.Bottom - 2, r.Right - 2, r.Bottom - 2);
		}

		private static GraphicsPath RoundedRect(Rectangle r, int radius)
		{
			GraphicsPath path = new GraphicsPath();
			int d = Math.Max(1, radius * 2);
			path.AddArc(r.X, r.Y, d, d, 180, 90);
			path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
			path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
			path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}

		private static void DrawGlyph(Graphics g, State s, FrameButton which, Rectangle r, Color color)
		{
			float scale = s.Dpi / 96f;
			FormFrameKind kind = s.Style.Kind;
			// Windows 10 and 11 and the browsers draw thin 10 pixel glyphs; the older frames draw bold pixel glyphs.
			bool thin = kind != FormFrameKind.Classic && kind != FormFrameKind.Luna && kind != FormFrameKind.Glass && s.Theme.Family != "Office 2010";
			bool crisp = kind == FormFrameKind.Classic || kind == FormFrameKind.Luna;
			int size = (int)Math.Round((kind == FormFrameKind.Classic ? 8 : thin ? 10 : 9) * scale);
			int thick = Math.Max(1, (int)Math.Round((kind == FormFrameKind.Classic ? 2 : thin ? 1 : 1.5f) * scale));
			int x = r.X + (r.Width - size) / 2;
			int y = r.Y + (r.Height - size) / 2;
			Pen pen = GdiCache.Pen(color, thick);
			SmoothingMode mode = g.SmoothingMode;
			switch (which)
			{
			case FrameButton.Minimize:
				g.FillRectangle(GdiCache.Brush(color), x, thin ? y + size / 2 : y + size - thick - (int)Math.Round(1 * scale), size, thick);
				break;
			case FrameButton.Maximize when thin:
				if (s.Maximized)
				{
					int o = Math.Max(2, (int)Math.Round(2 * scale));
					g.DrawRectangle(pen, x, y + o, size - o - 1, size - o - 1);
					g.DrawLine(pen, x + o, y, x + size - 1, y);
					g.DrawLine(pen, x + size - 1, y, x + size - 1, y + size - o - 1);
				}
				else
				{
					g.DrawRectangle(pen, x, y, size - 1, size - 1);
				}
				break;
			case FrameButton.Close when crisp:
				// Square pixel steps, as the bitmap glyphs of these frames had; smoothing makes them look blurred.
				for (int i = 0; i < size; i++)
				{
					g.FillRectangle(GdiCache.Brush(color), x + i, y + i, Math.Min(thick, size - i), 1);
					g.FillRectangle(GdiCache.Brush(color), Math.Max(x, x + size - i - thick), y + i, Math.Min(thick, size - i), 1);
				}
				break;
			case FrameButton.Maximize:
				if (s.Maximized)
				{
					int o = Math.Max(2, size / 4);
					// The back window shows only where the front one does not cover it.
					g.DrawLine(pen, x + size - 1, y, x + size - 1, y + size - o - 1);
					g.DrawLine(pen, x + o, y, x + o, y + o);
					g.DrawLine(pen, x + size - o, y + size - o - 1, x + size - 1, y + size - o - 1);
					g.FillRectangle(GdiCache.Brush(color), x + o, y, size - o, thick);
					g.DrawRectangle(pen, x, y + o, size - o - 1, size - o - 1);
					g.FillRectangle(GdiCache.Brush(color), x, y + o, size - o, thick);
				}
				else
				{
					g.DrawRectangle(pen, x, y, size - 1, size - 1);
					g.FillRectangle(GdiCache.Brush(color), x, y, size, thick);
				}
				break;
			default:
				g.SmoothingMode = SmoothingMode.AntiAlias;
				if (thin)
				{
					// Pixel centers, so a one pixel line stays one pixel wide.
					g.DrawLine(pen, x + 0.5f, y + 0.5f, x + size - 0.5f, y + size - 0.5f);
					g.DrawLine(pen, x + size - 0.5f, y + 0.5f, x + 0.5f, y + size - 0.5f);
				}
				else
				{
					g.DrawLine(pen, x, y, x + size - 1, y + size - 1);
					g.DrawLine(pen, x + size - 1, y, x, y + size - 1);
				}
				g.SmoothingMode = mode;
				break;
			}
		}

		private static void PaintBorders(Graphics g, State s, FormFrameColors c)
		{
			FormFrameMetrics m = s.Metrics;
			Rectangle v = m.Visible;
			int b = m.Border;
			if (b <= 0)
			{
				return;
			}
			Rectangle left = new Rectangle(v.X, m.Client.Y, b, m.Client.Height);
			Rectangle right = new Rectangle(m.Client.Right, m.Client.Y, b, m.Client.Height);
			Rectangle bottom = new Rectangle(v.X, m.Client.Bottom, v.Width, b);
			Brush frame = GdiCache.Brush(c.Frame);
			g.FillRectangle(frame, left);
			g.FillRectangle(frame, right);
			g.FillRectangle(frame, bottom);
			Pen outer = GdiCache.Pen(c.FrameOuter);
			switch (s.Style.Kind)
			{
			case FormFrameKind.Classic:
			{
				Rectangle all = new Rectangle(v.X, m.Client.Y - 1, v.Width, v.Bottom - m.Client.Y + 1);
				// The top part of the 3D edge is part of the caption; the side and bottom parts continue here.
				g.DrawLine(GdiCache.Pen(c.FrameInner), all.X, all.Y, all.X, all.Bottom - 2);
				g.DrawLine(GdiCache.Pen(c.Frame), all.X + 1, all.Y, all.X + 1, all.Bottom - 3);
				g.DrawLine(outer, all.Right - 1, all.Y, all.Right - 1, all.Bottom - 1);
				g.DrawLine(outer, all.X, all.Bottom - 1, all.Right - 1, all.Bottom - 1);
				g.DrawLine(GdiCache.Pen(SystemColors.ControlDark), all.Right - 2, all.Y, all.Right - 2, all.Bottom - 2);
				g.DrawLine(GdiCache.Pen(SystemColors.ControlDark), all.X + 1, all.Bottom - 2, all.Right - 2, all.Bottom - 2);
				break;
			}
			default:
				g.DrawLine(outer, v.X, m.Client.Y - 1, v.X, v.Bottom - 1);
				g.DrawLine(outer, v.Right - 1, m.Client.Y - 1, v.Right - 1, v.Bottom - 1);
				g.DrawLine(outer, v.X, v.Bottom - 1, v.Right - 1, v.Bottom - 1);
				if (b >= 3)
				{
					Pen inner = GdiCache.Pen(c.FrameInner);
					g.DrawLine(inner, v.X + b - 1, m.Client.Y, v.X + b - 1, m.Client.Bottom);
					g.DrawLine(inner, v.Right - b, m.Client.Y, v.Right - b, m.Client.Bottom);
					g.DrawLine(inner, v.X + b - 1, v.Bottom - b, v.Right - b, v.Bottom - b);
				}
				break;
			}
		}
	}
}
