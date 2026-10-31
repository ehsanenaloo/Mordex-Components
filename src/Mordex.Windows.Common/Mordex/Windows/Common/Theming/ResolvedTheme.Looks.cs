using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	// Looks of whole controls that need more than a color: buttons, progress bars, grips, pointer thumbs. They follow the
	// ThemeStyle and read cached objects; a size or a color that is new makes one brush, kept until the theme goes.
	public sealed partial class ResolvedTheme
	{
		private readonly System.Collections.Generic.Dictionary<(int, int, int), LinearGradientBrush> _colorFaces = new System.Collections.Generic.Dictionary<(int, int, int), LinearGradientBrush>();

		private readonly System.Collections.Generic.Dictionary<(int, int), LinearGradientBrush> _blockBrushes = new System.Collections.Generic.Dictionary<(int, int), LinearGradientBrush>();

		private readonly System.Collections.Generic.Dictionary<(int, int, int), LinearGradientBrush> _captionBrushes = new System.Collections.Generic.Dictionary<(int, int, int), LinearGradientBrush>();

		/// <summary>Draws a one pixel outline of a rounded rectangle in any color, using the shared pens.</summary>
		public void DrawRounded(Graphics graphics, Rectangle bounds, Color color, int radius)
		{
			if (bounds.Width < 3 || bounds.Height < 3)
			{
				return;
			}
			Pen pen = GdiCache.Pen(color);
			if (radius <= 0)
			{
				graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
				return;
			}
			lock (_cacheLock)
			{
				GraphicsPath path = GetRoundedPath(bounds.Width - 1, bounds.Height - 1, Scaled(radius));
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.DrawPath(pen, path);
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		/// <summary>
		/// Draws a button face in the look of the style (Luna, glass, classic). Returns false when the style has no button look,
		/// and the caller draws the face and the edge from the roles.
		/// </summary>
		public bool DrawButtonLook(Graphics graphics, Rectangle bounds, ThemeRole fill, ThemeRole border, int radius, bool enabled, bool hot, bool pressed, bool isDefault)
		{
			ThemeStyle style = Theme.Style;
			if (bounds.Width >= 5 && bounds.Height >= 5 && DrawGlassButton(graphics, bounds, radius, enabled, hot, pressed, isDefault))
			{
				return true;
			}
			if (style.ButtonLook == ThemeButtonLook.Default || bounds.Width < 5 || bounds.Height < 5)
			{
				return false;
			}
			int inner = Math.Max(0, radius - 1);
			switch (style.ButtonLook)
			{
			case ThemeButtonLook.Classic:
			{
				graphics.FillRectangle(Brush(fill), bounds);
				Rectangle edge = bounds;
				if (enabled && isDefault)
				{
					graphics.DrawRectangle(Pen(ThemeRole.BorderFocus), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
					edge.Inflate(-1, -1);
				}
				DrawBevel(graphics, edge, pressed);
				break;
			}
			case ThemeButtonLook.Luna:
				if (pressed)
				{
					FillRoundedPlain(graphics, bounds, ThemeRole.SurfacePressed, radius);
				}
				else
				{
					FillFace(graphics, bounds, enabled ? fill : ThemeRole.SurfaceDisabled, radius, LinearGradientMode.Vertical, false);
				}
				DrawRounded(graphics, bounds, enabled ? ThemeRole.BorderStrong : ThemeRole.BorderDisabled, radius);
				if (enabled && !pressed && (hot || isDefault))
				{
					Color ring = hot ? style.ButtonGlow : style.DefaultGlow;
					if (!ring.IsEmpty)
					{
						Rectangle one = Rectangle.Inflate(bounds, -1, -1);
						DrawRounded(graphics, one, ring, inner);
						DrawRounded(graphics, Rectangle.Inflate(one, -1, -1), MxTheme.Lighten(ring, 0.55f), Math.Max(0, inner - 1));
					}
				}
				break;
			default:
				FillFace(graphics, bounds, fill, radius, LinearGradientMode.Vertical, true);
				DrawRounded(graphics, bounds, border, radius);
				if (enabled)
				{
					DrawRounded(graphics, Rectangle.Inflate(bounds, -1, -1), System.Drawing.Color.FromArgb(pressed ? 70 : hot ? 170 : 120, 255, 255, 255), inner);
				}
				break;
			}
			return true;
		}

		/// <summary>Fills a rounded rectangle with a brush that the caller owns, from the cached shape.</summary>
		/// <remarks>The shape is drawn with its top-left corner at the origin, so a brush with a position, such as a gradient, is
		/// laid out from the top-left corner of <paramref name="bounds"/>.</remarks>
		public void FillRoundedWith(Graphics graphics, Rectangle bounds, Brush brush, int radius)
		{
			if (bounds.Width <= 1 || bounds.Height <= 1)
			{
				return;
			}
			lock (_cacheLock)
			{
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.FillPath(brush, GetRoundedPath(bounds.Width, bounds.Height, Scaled(radius)));
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
				graphics.SmoothingMode = smoothing;
			}
		}

		/// <summary>Draws the outline of a rounded rectangle with a pen that the caller owns, from the cached shape.</summary>
		public void DrawRoundedWith(Graphics graphics, Rectangle bounds, Pen pen, int radius)
		{
			if (bounds.Width <= 2 || bounds.Height <= 2)
			{
				return;
			}
			lock (_cacheLock)
			{
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.DrawPath(pen, GetRoundedPath(bounds.Width - 1, bounds.Height - 1, Scaled(radius)));
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
				graphics.SmoothingMode = smoothing;
			}
		}

		/// <summary>Draws the sunken edge of a round box (the classic radio button): dark on the upper left, light on the lower right.</summary>
		public void DrawSunkenEllipse(Graphics graphics, Rectangle bounds)
		{
			Rectangle outer = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
			Rectangle inner = Rectangle.Inflate(outer, -1, -1);
			graphics.DrawArc(Pen(ThemeRole.Border), outer, 135, 180);
			graphics.DrawArc(LightEdgePen, outer, -45, 180);
			graphics.DrawArc(Pen(ThemeRole.BorderStrong), inner, 135, 180);
			graphics.DrawArc(Pen(ThemeRole.Surface), inner, -45, 180);
		}

		/// <summary>Draws the raised 3D edge of a tab above its page: light on the top and left, dark on the right, open at the bottom.</summary>
		public void DrawRaisedTab(Graphics graphics, Rectangle bounds)
		{
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			graphics.DrawLine(LightEdgePen, bounds.X, bounds.Y + 2, bounds.X, bottom);
			graphics.DrawLine(LightEdgePen, bounds.X + 2, bounds.Y, right - 2, bounds.Y);
			graphics.DrawLine(LightEdgePen, bounds.X + 1, bounds.Y + 1, bounds.X + 1, bounds.Y + 1);
			graphics.DrawLine(Pen(ThemeRole.BorderStrong), right, bounds.Y + 2, right, bottom);
			graphics.DrawLine(Pen(ThemeRole.BorderStrong), right - 1, bounds.Y + 1, right - 1, bounds.Y + 1);
			graphics.DrawLine(Pen(ThemeRole.Border), right - 1, bounds.Y + 2, right - 1, bottom);
		}

		/// <summary>Draws the colored line on top of a selected or hot tab, when the style has one.</summary>
		public void DrawTabAccent(Graphics graphics, Rectangle bounds, int alignment)
		{
			Color accent = Theme.Style.TabAccent;
			if (accent.IsEmpty)
			{
				return;
			}
			if (Theme.Style.Modern.TabLine == ThemeTabLine.Inner)
			{
				alignment ^= 1;
			}
			int lineWidth = Theme.Style.Modern.TabLineWidth > 0 ? Theme.Style.Modern.TabLineWidth : 2;
			Pen pen = GdiCache.Pen(accent, Math.Max(1f, Scaled(lineWidth)));
			int t = Math.Max(1, Scaled(1));
			switch (alignment)
			{
			case 0:
				graphics.DrawLine(pen, bounds.X + 1, bounds.Y + t, bounds.Right - 2, bounds.Y + t);
				break;
			case 1:
				graphics.DrawLine(pen, bounds.X + 1, bounds.Bottom - 1 - t, bounds.Right - 2, bounds.Bottom - 1 - t);
				break;
			case 2:
				graphics.DrawLine(pen, bounds.X + t, bounds.Y + 1, bounds.X + t, bounds.Bottom - 2);
				break;
			default:
				graphics.DrawLine(pen, bounds.Right - 1 - t, bounds.Y + 1, bounds.Right - 1 - t, bounds.Bottom - 2);
				break;
			}
		}

		/// <summary>The color of the progress bar fill: the color the style names, or the accent.</summary>
		public Color ProgressColor => Theme.Style.ProgressFill.IsEmpty ? Theme.ColorsNoCopy[(int)ThemeRole.Accent] : Theme.Style.ProgressFill;

		/// <summary>
		/// Draws the filled part of a progress bar as blocks or as a glossy bar. <paramref name="area"/> is the whole area that
		/// the bar can fill; <paramref name="fraction"/> is how much of it is filled. Returns false for the default look.
		/// </summary>
		public bool DrawProgressFill(Graphics graphics, Rectangle area, float fraction, bool rightToLeft, bool enabled, int radius)
		{
			ThemeStyle style = Theme.Style;
			if (style.ProgressLook == ThemeProgressLook.Default || area.Width <= 0 || area.Height <= 0)
			{
				return false;
			}
			Color color = enabled ? ProgressColor : Theme.ColorsNoCopy[(int)ThemeRole.BorderDisabled];
			if (style.ProgressLook == ThemeProgressLook.Glossy)
			{
				int width = (int)(area.Width * fraction);
				if (width > 0)
				{
					Rectangle fill = new Rectangle(rightToLeft ? area.Right - width : area.X, area.Y, width, area.Height);
					FillColorFace(graphics, fill, color, 0.35f, 0.18f, radius, true);
				}
				return true;
			}
			int gap = Math.Max(1, Scaled(2));
			int pitch = Math.Max(gap + 1, Scaled(style.ProgressBlock > 0 ? style.ProgressBlock : 8));
			int count = (int)(area.Width * fraction / pitch);
			if (fraction > 0f && count == 0 && area.Width >= pitch)
			{
				count = 1;
			}
			if (count == 0)
			{
				return true;
			}
			LinearGradientBrush gradient = style.FaceGradient == ThemeGradient.None ? null : BlockBrush(color, area.Height);
			SolidBrush solid = gradient == null ? GdiCache.Brush(color) : null;
			int blockWidth = pitch - gap;
			lock (_cacheLock)
			{
				for (int i = 0; i < count; i++)
				{
					int x = rightToLeft ? area.Right - (i + 1) * pitch + gap : area.X + i * pitch;
					if (gradient == null)
					{
						graphics.FillRectangle(solid, x, area.Y, blockWidth, area.Height);
					}
					else
					{
						graphics.TranslateTransform(x, area.Y);
						graphics.FillRectangle(gradient, 0, 0, blockWidth, area.Height);
						graphics.TranslateTransform(-x, -area.Y);
					}
				}
			}
			return true;
		}

		// Light at the top and the bottom, full color in the middle: the green block of the Windows XP progress bar.
		private LinearGradientBrush BlockBrush(Color color, int height)
		{
			lock (_cacheLock)
			{
				(int, int) key = (color.ToArgb(), height);
				if (!_blockBrushes.TryGetValue(key, out LinearGradientBrush brush))
				{
					ThrowIfDisposed();
					if (_blockBrushes.Count >= LargestShapeCache)
					{
						DisposeAll(_blockBrushes.Values);
						_blockBrushes.Clear();
					}
					brush = new LinearGradientBrush(new Rectangle(0, 0, 2, Math.Max(2, height)), color, color, LinearGradientMode.Vertical);
					brush.InterpolationColors = new ColorBlend(3)
					{
						Positions = new[] { 0f, 0.5f, 1f },
						Colors = new[] { MxTheme.Lighten(color, 0.7f), color, MxTheme.Lighten(color, 0.45f) }
					};
					_blockBrushes[key] = brush;
				}
				return brush;
			}
		}

		/// <summary>Fills a rectangle with a gradient made from one color, light at the top, with an optional glossy band.</summary>
		public void FillColorFace(Graphics graphics, Rectangle bounds, System.Drawing.Color color, float light, float dark, int radius, bool gloss)
		{
			if (bounds.Width <= 1 || bounds.Height <= 1)
			{
				return;
			}
			lock (_cacheLock)
			{
				(int, int, int) key = (color.ToArgb() ^ (int)(light * 1000) * 31, bounds.Width, bounds.Height);
				if (!_colorFaces.TryGetValue(key, out LinearGradientBrush brush))
				{
					ThrowIfDisposed();
					if (_colorFaces.Count >= LargestShapeCache)
					{
						DisposeAll(_colorFaces.Values);
						_colorFaces.Clear();
					}
					brush = new LinearGradientBrush(new Rectangle(0, 0, bounds.Width, bounds.Height), MxTheme.Lighten(color, light), MxTheme.Darken(color, dark), LinearGradientMode.Vertical);
					_colorFaces[key] = brush;
				}
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.TranslateTransform(bounds.X, bounds.Y);
				if (radius <= 0)
				{
					graphics.FillRectangle(brush, 0, 0, bounds.Width, bounds.Height);
				}
				else
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.FillPath(brush, GetRoundedPath(bounds.Width, bounds.Height, Scaled(radius)));
					graphics.SmoothingMode = smoothing;
				}
				if (gloss && bounds.Height > 3)
				{
					FillGlossBand(graphics, bounds.Width, bounds.Height, radius);
				}
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		// Called with the graphics moved to the face and the cache lock held.
		private void FillGlossBand(Graphics graphics, int width, int height, int radius)
		{
			ThemeStyle style = Theme.Style;
			int inset = Math.Max(1, Scaled(radius) / 2);
			int glossHeight = height / 2;
			int top = style.GlossAlpha > 0 ? style.GlossAlpha : 120;
			int bottom = style.GlossBand ? top : 30;
			(int, int, int) glossKey = (glossHeight, top, bottom);
			if (!_glossBrushes.TryGetValue(glossKey, out LinearGradientBrush gloss))
			{
				if (_glossBrushes.Count >= LargestShapeCache)
				{
					DisposeAll(_glossBrushes.Values);
					_glossBrushes.Clear();
				}
				gloss = new LinearGradientBrush(new Rectangle(0, 0, 2, glossHeight), System.Drawing.Color.FromArgb(top, 255, 255, 255), System.Drawing.Color.FromArgb(bottom, 255, 255, 255), LinearGradientMode.Vertical);
				_glossBrushes[glossKey] = gloss;
			}
			graphics.FillRectangle(gloss, inset, 1, width - inset * 2, glossHeight);
		}

		/// <summary>
		/// Fills a caption bar with a gradient that runs from the left color to the right color of the style (the Windows
		/// Classic title bar). Returns false when the style has no caption gradient.
		/// </summary>
		public bool FillCaption(Graphics graphics, Rectangle bounds, ThemeRole role)
		{
			Color end = Theme.Style.CaptionEnd;
			if (end.IsEmpty || bounds.Width <= 1 || bounds.Height <= 1)
			{
				return false;
			}
			Color start = Theme.ColorsNoCopy[(int)role];
			if (role != ThemeRole.Caption)
			{
				end = MxTheme.Blend(end, Theme.ColorsNoCopy[(int)ThemeRole.Surface], 0.5f);
			}
			lock (_cacheLock)
			{
				(int, int, int) key = (start.ToArgb() ^ end.ToArgb() * 17, bounds.Width, bounds.Height);
				if (!_captionBrushes.TryGetValue(key, out LinearGradientBrush brush))
				{
					ThrowIfDisposed();
					if (_captionBrushes.Count >= LargestShapeCache)
					{
						DisposeAll(_captionBrushes.Values);
						_captionBrushes.Clear();
					}
					brush = new LinearGradientBrush(new Rectangle(0, 0, bounds.Width, bounds.Height), start, end, LinearGradientMode.Horizontal);
					_captionBrushes[key] = brush;
				}
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.FillRectangle(brush, 0, 0, bounds.Width, bounds.Height);
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
			return true;
		}

		/// <summary>
		/// Draws the thumb of a track bar with a pointed end on the side of its ticks. <paramref name="direction"/>: 0 up, 1
		/// down, 2 left, 3 right is where the point looks. The fill and the edge come from the roles.
		/// </summary>
		public void DrawPointerThumb(Graphics graphics, Rectangle bounds, int direction, ThemeRole fill, ThemeRole edge)
		{
			int tip = Math.Max(2, Math.Min(bounds.Width, bounds.Height) / 2);
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			Span<Point> shape = stackalloc Point[5];
			switch (direction)
			{
			case 0:
				shape[0] = new Point(bounds.X, bounds.Y + tip);
				shape[1] = new Point(bounds.X + bounds.Width / 2, bounds.Y);
				shape[2] = new Point(right, bounds.Y + tip);
				shape[3] = new Point(right, bottom);
				shape[4] = new Point(bounds.X, bottom);
				break;
			case 1:
				shape[0] = new Point(bounds.X, bounds.Y);
				shape[1] = new Point(right, bounds.Y);
				shape[2] = new Point(right, bottom - tip);
				shape[3] = new Point(bounds.X + bounds.Width / 2, bottom);
				shape[4] = new Point(bounds.X, bottom - tip);
				break;
			case 2:
				shape[0] = new Point(bounds.X + tip, bounds.Y);
				shape[1] = new Point(right, bounds.Y);
				shape[2] = new Point(right, bottom);
				shape[3] = new Point(bounds.X + tip, bottom);
				shape[4] = new Point(bounds.X, bounds.Y + bounds.Height / 2);
				break;
			default:
				shape[0] = new Point(bounds.X, bounds.Y);
				shape[1] = new Point(right - tip, bounds.Y);
				shape[2] = new Point(right, bounds.Y + bounds.Height / 2);
				shape[3] = new Point(right - tip, bottom);
				shape[4] = new Point(bounds.X, bottom);
				break;
			}
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.None;
			graphics.FillPolygon(Brush(fill), shape);
			graphics.DrawPolygon(Pen(edge), shape);
			if (Theme.Style.Bevel == ThemeBevel.Raised && direction == 1)
			{
				graphics.DrawLine(LightEdgePen, bounds.X + 1, bounds.Y + 1, right - 2, bounds.Y + 1);
				graphics.DrawLine(LightEdgePen, bounds.X + 1, bounds.Y + 1, bounds.X + 1, bottom - tip - 1);
			}
			graphics.SmoothingMode = smoothing;
		}

		/// <summary>Draws the dotted grip of a toolbar or a menu bar in <paramref name="area"/>, a column of two dots wide.</summary>
		public void DrawGrip(Graphics graphics, Rectangle area, bool vertical)
		{
			int step = Math.Max(3, Scaled(4));
			Brush dark = Brush(ThemeRole.BorderStrong);
			Brush light = Brush(ThemeRole.SurfaceRaised);
			int dot = Math.Max(2, Scaled(2));
			if (!vertical)
			{
				int x = area.X + (area.Width - dot) / 2;
				for (int y = area.Y + step / 2; y + dot <= area.Bottom; y += step)
				{
					graphics.FillRectangle(light, x + 1, y + 1, dot, dot);
					graphics.FillRectangle(dark, x, y, dot, dot);
				}
			}
			else
			{
				int y = area.Y + (area.Height - dot) / 2;
				for (int x = area.X + step / 2; x + dot <= area.Right; x += step)
				{
					graphics.FillRectangle(light, x + 1, y + 1, dot, dot);
					graphics.FillRectangle(dark, x, y, dot, dot);
				}
			}
		}

		private void DisposeLookObjects()
		{
			DisposeAll(_colorFaces.Values);
			_colorFaces.Clear();
			DisposeAll(_blockBrushes.Values);
			_blockBrushes.Clear();
			DisposeAll(_captionBrushes.Values);
			_captionBrushes.Clear();
		}
	}
}
