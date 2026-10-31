using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	// Glass faces and buttons, glass check boxes and the focus frame of inputs. Every brush is made once for a state and a
	// size and kept by the theme.
	public sealed partial class ResolvedTheme
	{
		private readonly Dictionary<(int, int, int), LinearGradientBrush> _glassBrushes = new Dictionary<(int, int, int), LinearGradientBrush>();

		private readonly Dictionary<(int, int, int), LinearGradientBrush> _boxBrushes = new Dictionary<(int, int, int), LinearGradientBrush>();

		/// <summary>The background of a pop-up menu.</summary>
		public System.Drawing.Color MenuBackColor => Theme.Style.Modern.MenuBack.IsEmpty ? Theme.ColorsNoCopy[(int)ThemeRole.SurfaceRaised] : Theme.Style.Modern.MenuBack;

		private const int GlassNormal = 0;

		private const int GlassHot = 1;

		private const int GlassPressed = 2;

		private const int GlassDefault = 3;

		// The brush of a plain face gradient, or of a glass face: a light upper half and a darker lower half.
		private LinearGradientBrush NewFaceBrush(Color face, int width, int height, LinearGradientMode mode)
		{
			ThemeStyle style = Theme.Style;
			Rectangle area = new Rectangle(0, 0, width, height);
			if (style.Modern.FaceKind != ThemeFaceKind.Glass || mode != LinearGradientMode.Vertical)
			{
				return new LinearGradientBrush(area, style.LightEnd(face), style.DarkEnd(face), mode);
			}
			LinearGradientBrush brush = new LinearGradientBrush(area, face, face, mode);
			float split = height > 8 ? 0.5f : 0.45f;
			brush.InterpolationColors = new ColorBlend(4)
			{
				Positions = new[] { 0f, split - 0.01f, split, 1f },
				Colors = new[]
				{
					MxTheme.Lighten(face, style.GradientAmount),
					MxTheme.Lighten(face, style.GradientAmount * 0.3f),
					MxTheme.Darken(face, style.GradientShade + 0.04f),
					MxTheme.Lighten(face, style.GradientAmount * 0.2f)
				}
			};
			return brush;
		}

		// Some slots of a palette are 0 where the look uses the color of another slot.
		private System.Drawing.Color Slot(int slot, int fallback)
		{
			System.Drawing.Color color = Theme.OfficePaletteNoCopy[slot];
			return color.A < 200 ? Theme.OfficePaletteNoCopy[fallback] : color;
		}

		/// <summary>A slot of the Office palette, or <paramref name="fallback"/> when the slot is unused (0).</summary>
		public System.Drawing.Color OfficeColorOr(int slot, int fallback)
		{
			return Slot(slot, fallback);
		}

		private Color Slot(int slot)
		{
			return Theme.OfficePaletteNoCopy[slot];
		}

		// A palette that has one color for a state (Office 2010) gets a lighter upper half.
		private static void Soften(ref System.Drawing.Color top0, ref System.Drawing.Color top1, ref System.Drawing.Color bottom0, ref System.Drawing.Color bottom1)
		{
			if (top0.ToArgb() == bottom0.ToArgb())
			{
				top0 = MxTheme.Lighten(top0, 0.6f);
				top1 = MxTheme.Lighten(top1, 0.3f);
				bottom1 = MxTheme.Darken(bottom1, 0.04f);
			}
		}

		private void GlassColors(int state, out Color top0, out Color top1, out Color bottom0, out Color bottom1, out Color border)
		{
			bool office2007 = Theme.Style.Modern.Glass == ThemeGlassKind.Office2007;
			bool palette = Theme.OfficePaletteNoCopy != null;
			if (state == GlassHot)
			{
				if (palette)
				{
					top0 = Slot(OfficeSlot.HotLightColor, OfficeSlot.HotDarkColor);
					top1 = Slot(OfficeSlot.HotLightGradientColor, OfficeSlot.HotLightColor);
					bottom0 = Slot(OfficeSlot.HotDarkColor, OfficeSlot.HotLightColor);
					bottom1 = Slot(OfficeSlot.HotDarkGradientColor, OfficeSlot.HotLightColor);
					border = Slot(OfficeSlot.HotBorderColor1, OfficeSlot.HotBorderColor2);
					Soften(ref top0, ref top1, ref bottom0, ref bottom1);
				}
				else
				{
					top0 = System.Drawing.Color.FromArgb(255, 252, 232);
					top1 = System.Drawing.Color.FromArgb(255, 241, 175);
					bottom0 = System.Drawing.Color.FromArgb(255, 226, 125);
					bottom1 = System.Drawing.Color.FromArgb(255, 238, 165);
					border = Theme.ColorsNoCopy[(int)ThemeRole.Accent];
				}
				return;
			}
			if (state == GlassPressed)
			{
				if (palette)
				{
					top0 = Slot(OfficeSlot.PressedLightColor, OfficeSlot.PressedDarkColor);
					top1 = Slot(OfficeSlot.PressedLightGradientColor, OfficeSlot.PressedLightColor);
					bottom0 = Slot(OfficeSlot.PressedDarkColor, OfficeSlot.PressedLightColor);
					bottom1 = Slot(OfficeSlot.PressedDarkGradientColor, OfficeSlot.PressedLightColor);
					border = Slot(OfficeSlot.PressedBorderColor1, OfficeSlot.PressedBorderColor2);
					bool single = top0.ToArgb() == bottom0.ToArgb();
					Soften(ref top0, ref top1, ref bottom0, ref bottom1);
					if (single)
					{
						top1 = MxTheme.Darken(top1, 0.08f);
						bottom0 = MxTheme.Darken(bottom0, 0.14f);
						bottom1 = MxTheme.Darken(bottom1, 0.1f);
					}
				}
				else
				{
					top0 = System.Drawing.Color.FromArgb(255, 232, 165);
					top1 = System.Drawing.Color.FromArgb(255, 215, 105);
					bottom0 = System.Drawing.Color.FromArgb(255, 200, 75);
					bottom1 = System.Drawing.Color.FromArgb(255, 222, 125);
					border = Theme.ColorsNoCopy[(int)ThemeRole.AccentPressed];
				}
				return;
			}
			Color light = palette ? Slot(OfficeSlot.RibbonLight) : Theme.ColorsNoCopy[(int)ThemeRole.SurfaceRaised];
			Color dark = palette ? Slot(OfficeSlot.RibbonDark) : Theme.ColorsNoCopy[(int)ThemeRole.SurfaceAlt];
			border = Theme.ColorsNoCopy[(int)ThemeRole.Border];
			if (office2007)
			{
				top0 = MxTheme.Lighten(light, 0.8f);
				top1 = MxTheme.Lighten(light, 0.2f);
				bottom0 = MxTheme.Blend(dark, Theme.ColorsNoCopy[(int)ThemeRole.Border], 0.22f);
				bottom1 = MxTheme.Lighten(dark, 0.3f);
			}
			else
			{
				top0 = MxTheme.Lighten(light, 0.85f);
				top1 = MxTheme.Lighten(light, 0.5f);
				bottom0 = MxTheme.Lighten(light, 0.1f);
				bottom1 = MxTheme.Lighten(dark, 0.4f);
			}
			if (state == GlassDefault)
			{
				border = palette ? Slot(OfficeSlot.HotBorderColor1) : Theme.ColorsNoCopy[(int)ThemeRole.Accent];
			}
		}

		// Draws a push button as glass. Returns false when the style has no glass look.
		private bool DrawGlassButton(Graphics graphics, Rectangle bounds, int radius, bool enabled, bool hot, bool pressed, bool isDefault)
		{
			ThemeStyle style = Theme.Style;
			if (style.Modern.Glass == ThemeGlassKind.None)
			{
				return false;
			}
			if (!enabled)
			{
				FillRoundedPlain(graphics, bounds, ThemeRole.SurfaceDisabled, radius);
				DrawRounded(graphics, bounds, ThemeRole.BorderDisabled, radius);
				return true;
			}
			int state = pressed ? GlassPressed : hot ? GlassHot : isDefault ? GlassDefault : GlassNormal;
			GlassColors(state, out Color top0, out Color top1, out Color bottom0, out Color bottom1, out Color border);
			lock (_cacheLock)
			{
				(int, int, int) key = (state, bounds.Width, bounds.Height);
				if (!_glassBrushes.TryGetValue(key, out LinearGradientBrush brush))
				{
					ThrowIfDisposed();
					if (_glassBrushes.Count >= LargestShapeCache)
					{
						DisposeAll(_glassBrushes.Values);
						_glassBrushes.Clear();
					}
					brush = new LinearGradientBrush(new Rectangle(0, 0, bounds.Width, bounds.Height), top0, bottom1, LinearGradientMode.Vertical);
					brush.InterpolationColors = new ColorBlend(4)
					{
						Positions = new[] { 0f, 0.49f, 0.5f, 1f },
						Colors = new[] { top0, top1, bottom0, bottom1 }
					};
					_glassBrushes[key] = brush;
				}
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.TranslateTransform(bounds.X, bounds.Y);
				if (radius <= 0)
				{
					graphics.FillRectangle(brush, 0, 0, bounds.Width, bounds.Height);
				}
				else
				{
					graphics.FillPath(brush, GetRoundedPath(bounds.Width, bounds.Height, Scaled(radius)));
				}
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
				graphics.SmoothingMode = smoothing;
			}
			DrawRounded(graphics, bounds, border, radius);
			if (style.Modern.Glass == ThemeGlassKind.Office2007 && bounds.Width > 6 && bounds.Height > 6)
			{
				int alpha = pressed ? 60 : hot ? 200 : 150;
				DrawRounded(graphics, Rectangle.Inflate(bounds, -1, -1), System.Drawing.Color.FromArgb(alpha, 255, 255, 255), Math.Max(0, radius - 1));
			}
			return true;
		}

		private LinearGradientBrush BoxBrush(Color from, Color to, int height)
		{
			(int, int, int) key = (from.ToArgb(), to.ToArgb(), height);
			if (!_boxBrushes.TryGetValue(key, out LinearGradientBrush brush))
			{
				ThrowIfDisposed();
				if (_boxBrushes.Count >= LargestShapeCache)
				{
					DisposeAll(_boxBrushes.Values);
					_boxBrushes.Clear();
				}
				brush = new LinearGradientBrush(new Rectangle(0, 0, 2, Math.Max(2, height)), from, to, LinearGradientMode.Vertical);
				_boxBrushes[key] = brush;
			}
			return brush;
		}

		/// <summary>
		/// Draws the box of a check box or the circle of a radio button as glass: a white face with a tint at the bottom, a blue
		/// edge, a gold glow when hot and an orange glow when pressed. Returns false when the style has no glass boxes.
		/// </summary>
		public bool DrawGlassBox(Graphics graphics, Rectangle box, bool radio, bool enabled, bool hot, bool pressed)
		{
			if (Theme.Style.Modern.BoxLook != ThemeBoxLook.Glass || box.Width < 6 || box.Height < 6)
			{
				return false;
			}
			Color white = Theme.ColorsNoCopy[(int)ThemeRole.InputBack];
			Color tint;
			Color border;
			if (!enabled)
			{
				tint = MxTheme.Blend(white, Theme.ColorsNoCopy[(int)ThemeRole.Surface], 0.5f);
				border = Theme.ColorsNoCopy[(int)ThemeRole.BorderDisabled];
			}
			else if (pressed)
			{
				tint = System.Drawing.Color.FromArgb(255, 208, 103);
				border = Theme.ColorsNoCopy[(int)ThemeRole.BorderStrong];
			}
			else if (hot)
			{
				tint = System.Drawing.Color.FromArgb(252, 227, 170);
				border = Theme.ColorsNoCopy[(int)ThemeRole.BorderStrong];
			}
			else
			{
				tint = MxTheme.Blend(Theme.ColorsNoCopy[(int)ThemeRole.Surface], Theme.ColorsNoCopy[(int)ThemeRole.Border], 0.4f);
				border = MxTheme.Blend(Theme.ColorsNoCopy[(int)ThemeRole.Border], white, 0.2f);
			}
			Rectangle outer = new Rectangle(box.X, box.Y, box.Width - 1, box.Height - 1);
			Rectangle inner = Rectangle.Inflate(outer, -1, -1);
			lock (_cacheLock)
			{
				LinearGradientBrush brush = BoxBrush(white, tint, inner.Height);
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.TranslateTransform(inner.X, inner.Y);
				if (radio)
				{
					graphics.FillEllipse(brush, 0, 0, inner.Width, inner.Height);
				}
				else
				{
					graphics.FillPath(brush, GetRoundedPath(inner.Width, inner.Height, Scaled(2)));
				}
				graphics.TranslateTransform(-inner.X, -inner.Y);
				Pen pen = GdiCache.Pen(border);
				if (radio)
				{
					graphics.DrawEllipse(pen, outer);
				}
				else
				{
					graphics.TranslateTransform(outer.X, outer.Y);
					graphics.DrawPath(pen, GetRoundedPath(outer.Width, outer.Height, Scaled(2)));
					graphics.TranslateTransform(-outer.X, -outer.Y);
				}
				if (enabled && (hot || pressed))
				{
					Pen glow = GdiCache.Pen(pressed ? System.Drawing.Color.FromArgb(242, 137, 38) : System.Drawing.Color.FromArgb(250, 211, 114));
					Rectangle ring = Rectangle.Inflate(outer, -1, -1);
					if (radio)
					{
						graphics.DrawEllipse(glow, ring);
					}
					else
					{
						graphics.DrawRectangle(glow, ring);
					}
				}
				graphics.SmoothingMode = smoothing;
			}
			return true;
		}

		/// <summary>The color of the mark in a glass box.</summary>
		public Color BoxMarkColor(bool enabled)
		{
			if (!enabled)
			{
				return MxTheme.Blend(Theme.ColorsNoCopy[(int)ThemeRole.Border], Theme.ColorsNoCopy[(int)ThemeRole.InputBack], 0.5f);
			}
			Color mark = Theme.Style.Modern.BoxMark;
			return mark.IsEmpty ? Theme.ColorsNoCopy[(int)ThemeRole.Text] : mark;
		}

		/// <summary>
		/// Draws the frame of an input: its border, and the focus line or ring that the style asks for when it has focus. The
		/// frame lies inside <paramref name="bounds"/>.
		/// </summary>
		public void DrawInputFrame(Graphics graphics, Rectangle bounds, ThemeRole role, int radius, bool focused)
		{
			ThemeStyle style = Theme.Style;
			if (!focused || (style.Modern.InputFocus == ThemeFocusKind.Border && style.Modern.InputFocusWidth <= 1) || bounds.Width < 8 || bounds.Height < 8)
			{
				DrawRounded(graphics, bounds, role, radius);
				return;
			}
			int width = Math.Max(1, Scaled(style.Modern.InputFocusWidth > 0 ? style.Modern.InputFocusWidth : 1));
			Color accent = Theme.ColorsNoCopy[(int)ThemeRole.BorderFocus];
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			switch (style.Modern.InputFocus)
			{
			case ThemeFocusKind.Underline:
			{
				DrawRounded(graphics, bounds, ThemeRole.Border, radius);
				int inset = Math.Max(1, Scaled(radius) / 2);
				Pen line = GdiCache.Pen(accent, width);
				float y = bounds.Bottom - 1 - width / 2f;
				graphics.DrawLine(line, bounds.X + inset, y, bounds.Right - 1 - inset, y);
				break;
			}
			case ThemeFocusKind.Ring:
				DrawRounded(graphics, bounds, accent, radius);
				DrawRounded(graphics, Rectangle.Inflate(bounds, -1, -1), System.Drawing.Color.FromArgb(110, accent), Math.Max(0, radius - 1));
				break;
			default:
				for (int i = 0; i < width; i++)
				{
					DrawRounded(graphics, Rectangle.Inflate(bounds, -i, -i), accent, Math.Max(0, radius - i));
				}
				break;
			}
			graphics.SmoothingMode = smoothing;
		}

		private readonly Dictionary<(int, int), GraphicsPath> _trapezoids = new Dictionary<(int, int), GraphicsPath>();

		/// <summary>
		/// Fills a tab as a trapezoid with curved feet and round upper corners (Chrome) and draws its outline, open at the bottom.
		/// Returns false when the style has no trapezoid tabs.
		/// </summary>
		public bool DrawTrapezoidTab(Graphics graphics, Rectangle bounds, ThemeRole fill, ThemeRole border, bool outline)
		{
			if (!Theme.Style.Modern.TrapezoidTabs || bounds.Width < 24 || bounds.Height < 12)
			{
				return false;
			}
			lock (_cacheLock)
			{
				(int, int) key = (bounds.Width, bounds.Height);
				if (!_trapezoids.TryGetValue(key, out GraphicsPath path))
				{
					ThrowIfDisposed();
					if (_trapezoids.Count >= LargestShapeCache)
					{
						DisposeAll(_trapezoids.Values);
						_trapezoids.Clear();
					}
					float w = bounds.Width - 1;
					float h = bounds.Height;
					float s = Math.Min(h * 0.6f, w / 4f);
					float r = Math.Min(Scaled(5), h / 3f);
					path = new GraphicsPath();
					path.AddBezier(0, h, s * 0.55f, h, s * 0.45f, r * 2, s, r);
					path.AddArc(s, 0, r * 2, r * 2, 180, 90);
					path.AddArc(w - s - r * 2, 0, r * 2, r * 2, 270, 90);
					path.AddBezier(w - s, r, w - s * 0.45f, r * 2, w - s * 0.55f, h, w, h);
					_trapezoids[key] = path;
				}
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.FillPath(Brush(fill), path);
				if (outline)
				{
					graphics.DrawPath(Pen(border), path);
				}
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
				graphics.SmoothingMode = smoothing;
			}
			return true;
		}

		private void DisposeOfficeObjects()
		{
			DisposeAll(_glassBrushes.Values);
			_glassBrushes.Clear();
			DisposeAll(_trapezoids.Values);
			_trapezoids.Clear();
			DisposeAll(_boxBrushes.Values);
			_boxBrushes.Clear();
		}
	}
}
