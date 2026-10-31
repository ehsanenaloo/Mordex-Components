using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	// The glyphs and faces of the older Windows looks: Luna, Classic and the flat Office looks. They read cached objects
	// and the colors of the style.
	public sealed partial class ResolvedTheme
	{
		// Check marks as rows of pixels; X is a pixel of the mark.
		private static readonly string[] ClassicTick = { "......X", ".....XX", "X...XXX", "XX.XXX.", "XXXXX..", ".XXX...", "..X...." };

		private static readonly string[] LunaTick = { ".......XX", "......XXX", "X....XXX.", "XX..XXX..", "XXXXXX...", ".XXXX....", "..XX....." };

		private static readonly System.Drawing.Color LunaFrameDisabled = System.Drawing.Color.FromArgb(201, 199, 186);

		private static readonly System.Drawing.Color LunaFill = System.Drawing.Color.FromArgb(237, 237, 232);

		private static readonly System.Drawing.Color LunaFillPressed = System.Drawing.Color.FromArgb(214, 209, 194);

		/// <summary>The font of the theme, or null when the theme leaves the font to the application. The font is shared and never disposed.</summary>
		public System.Drawing.Font Font => Theme.Style.Detail.Font == null ? null : ThemeFonts.Get(Theme.Style.Detail.Font.Value);

		/// <summary>The text color of a selected menu row in the older looks, or empty.</summary>
		public System.Drawing.Color MenuSelectionText => Theme.Style.Detail.MenuSelectionText;

		/// <summary>Draws the selected row of a pop-up menu in the older look. Returns false for the default look.</summary>
		public bool DrawMenuSelection(Graphics graphics, Rectangle row)
		{
			ThemeStyle style = Theme.Style;
			switch (style.Detail.MenuLook)
			{
			case ThemeMenuLook.Xp:
			case ThemeMenuLook.Classic:
				graphics.FillRectangle(GdiCache.Brush(style.Detail.MenuSelection), row);
				return true;
			case ThemeMenuLook.OfficeXp:
				graphics.FillRectangle(Brush(ThemeRole.SurfaceHot), row);
				graphics.DrawRectangle(Pen(ThemeRole.Accent), row.X, row.Y, row.Width - 1, row.Height - 1);
				return true;
			case ThemeMenuLook.Office2003:
				FillColorFace(graphics, row, Theme.ColorsNoCopy[(int)ThemeRole.AccentHot], 0.55f, 0.04f, 2, false);
				DrawRounded(graphics, row, ThemeRole.BorderStrong, 2);
				return true;
			default:
				return false;
			}
		}

		/// <summary>Draws the band behind the icons of a pop-up menu. Returns false when the generic band is to be used.</summary>
		public bool DrawMenuGutter(Graphics graphics, Rectangle gutter)
		{
			switch (Theme.Style.Detail.MenuLook)
			{
			case ThemeMenuLook.Xp:
				graphics.FillRectangle(GdiCache.Brush(System.Drawing.Color.FromArgb(244, 243, 236)), gutter);
				return true;
			case ThemeMenuLook.OfficeXp:
				graphics.FillRectangle(Brush(ThemeRole.Surface), gutter);
				return true;
			default:
				return false;
			}
		}

		/// <summary>Draws the border of a pop-up menu. Returns false when the caller draws the one pixel border.</summary>
		public bool DrawMenuFrame(Graphics graphics, Rectangle bounds)
		{
			switch (Theme.Style.Detail.MenuLook)
			{
			case ThemeMenuLook.Classic:
				DrawBevel(graphics, bounds, false);
				return true;
			case ThemeMenuLook.Xp:
				graphics.DrawRectangle(GdiCache.Pen(System.Drawing.Color.FromArgb(172, 168, 153)), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
				return true;
			case ThemeMenuLook.OfficeXp:
			case ThemeMenuLook.Office2003:
				graphics.DrawRectangle(Pen(ThemeRole.BorderStrong), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Draws a tab of a horizontal strip (docking tabs, auto-hide tabs) in the older look: a raised box in Classic, a
		/// rounded gradient tab in the other looks. <paramref name="stripAtBottom"/> is true when the page lies above the strip.
		/// Returns false for the default look.
		/// </summary>
		public bool DrawStripTab(Graphics graphics, Rectangle bounds, bool selected, bool hot, bool stripAtBottom)
		{
			return DrawStripTab(graphics, bounds, selected, hot, stripAtBottom, false);
		}

		/// <summary>The same for a vertical strip. <paramref name="stripAtBottom"/> is true when the page lies to the left of the strip.</summary>
		public bool DrawStripTab(Graphics graphics, Rectangle bounds, bool selected, bool hot, bool stripAtBottom, bool vertical)
		{
			if (vertical)
			{
				return DrawVerticalStripTab(graphics, bounds, selected, hot, stripAtBottom);
			}
			ThemeChoiceLook look = Theme.Style.Detail.ChoiceLook;
			if (look == ThemeChoiceLook.Default || bounds.Width < 6 || bounds.Height < 6)
			{
				return false;
			}
			GraphicsState state = graphics.Save();
			graphics.SetClip(bounds, CombineMode.Intersect);
			if (look == ThemeChoiceLook.Classic)
			{
				graphics.FillRectangle(Brush(selected ? ThemeRole.SurfaceRaised : ThemeRole.Surface), bounds);
				if (selected)
				{
					DrawBevel(graphics, bounds, false);
				}
			}
			else
			{
				int radius = 3;
				Rectangle shape = bounds;
				if (stripAtBottom)
				{
					shape.Y -= Scaled(radius);
				}
				shape.Height += Scaled(radius);
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				if (Theme.Style.ButtonLook == ThemeButtonLook.Luna && !stripAtBottom)
				{
					DrawLunaTab(graphics, shape, bounds, selected, hot, radius);
					graphics.Restore(state);
					return true;
				}
				ThemeRole role = selected ? ThemeRole.TabActive : hot ? ThemeRole.TabHot : ThemeRole.TabInactive;
				FillColorFace(graphics, shape, Theme.ColorsNoCopy[(int)role], selected ? 0.2f : 0.3f, selected ? 0.02f : 0.08f, radius, false);
				DrawRounded(graphics, shape, ThemeRole.Border, radius);
				if (selected)
				{
					graphics.SmoothingMode = SmoothingMode.None;
					int y = stripAtBottom ? bounds.Y : bounds.Bottom - 1;
					graphics.DrawLine(GdiCache.Pen(MxTheme.Lighten(Theme.ColorsNoCopy[(int)ThemeRole.TabActive], 0.2f)), bounds.X + 1, y, bounds.Right - 2, y);
				}
			}
			graphics.Restore(state);
			return true;
		}

		// A tab of Windows XP: a white-to-beige face with a gray-blue edge and an orange line on top when selected or hot.
		private void DrawLunaTab(Graphics graphics, Rectangle shape, Rectangle bounds, bool selected, bool hot, int radius)
		{
			System.Drawing.Color edge = System.Drawing.Color.FromArgb(145, 155, 156);
			FillColorFace(graphics, shape, selected ? System.Drawing.Color.FromArgb(250, 250, 252) : System.Drawing.Color.FromArgb(238, 237, 230), 0.85f, 0.04f, radius, false);
			DrawRounded(graphics, shape, edge, radius);
			int r = Scaled(radius);
			if (selected || hot)
			{
				graphics.SmoothingMode = SmoothingMode.None;
				int left = shape.X + r;
				int right = shape.Right - r - 1;
				graphics.DrawLine(GdiCache.Pen(System.Drawing.Color.FromArgb(255, 199, 60)), left, shape.Y + 1, right, shape.Y + 1);
				graphics.DrawLine(GdiCache.Pen(System.Drawing.Color.FromArgb(255, 214, 132)), left, shape.Y + 2, right, shape.Y + 2);
			}
			if (selected)
			{
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.DrawLine(GdiCache.Pen(System.Drawing.Color.FromArgb(252, 252, 254)), bounds.X + 1, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom - 1);
			}
		}

		private bool DrawVerticalStripTab(Graphics graphics, Rectangle bounds, bool selected, bool hot, bool pageOnLeft)
		{
			ThemeChoiceLook look = Theme.Style.Detail.ChoiceLook;
			if (look == ThemeChoiceLook.Default || bounds.Width < 6 || bounds.Height < 6)
			{
				return false;
			}
			if (look == ThemeChoiceLook.Classic)
			{
				graphics.FillRectangle(Brush(selected ? ThemeRole.SurfaceRaised : ThemeRole.Surface), bounds);
				if (selected)
				{
					DrawBevel(graphics, bounds, false);
				}
				return true;
			}
			ThemeRole role = selected ? ThemeRole.TabActive : hot ? ThemeRole.TabHot : ThemeRole.TabInactive;
			System.Drawing.Color face = Theme.ColorsNoCopy[(int)role];
			GdiCache.FillGradient(graphics, bounds, pageOnLeft ? MxTheme.Lighten(face, selected ? 0.2f : 0.3f) : MxTheme.Darken(face, 0.06f), pageOnLeft ? MxTheme.Darken(face, 0.06f) : MxTheme.Lighten(face, selected ? 0.2f : 0.3f), LinearGradientMode.Horizontal);
			Pen border = Pen(ThemeRole.Border);
			graphics.DrawLine(border, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
			graphics.DrawLine(border, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			int edge = pageOnLeft ? bounds.Right - 1 : bounds.X;
			graphics.DrawLine(border, edge, bounds.Y, edge, bounds.Bottom - 1);
			return true;
		}

		/// <summary>Draws a one pixel sunken edge: dark on the top and left, light on the bottom and right (a classic status bar panel).</summary>
		public void DrawThinSunken(Graphics graphics, Rectangle bounds)
		{
			if (bounds.Width < 3 || bounds.Height < 3)
			{
				return;
			}
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			Pen dark = Pen(ThemeRole.Border);
			graphics.DrawLine(dark, bounds.X, bounds.Y, right - 1, bounds.Y);
			graphics.DrawLine(dark, bounds.X, bounds.Y, bounds.X, bottom - 1);
			graphics.DrawLine(LightEdgePen, bounds.X, bottom, right, bottom);
			graphics.DrawLine(LightEdgePen, right, bounds.Y, right, bottom);
		}

		/// <summary>Draws a one pixel raised edge: light on the top and left, dark on the bottom and right (a classic toolbar).</summary>
		public void DrawThinRaised(Graphics graphics, Rectangle bounds)
		{
			if (bounds.Width < 3 || bounds.Height < 3)
			{
				return;
			}
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			graphics.DrawLine(LightEdgePen, bounds.X, bounds.Y, right - 1, bounds.Y);
			graphics.DrawLine(LightEdgePen, bounds.X, bounds.Y, bounds.X, bottom - 1);
			Pen dark = Pen(ThemeRole.Border);
			graphics.DrawLine(dark, bounds.X, bottom, right, bottom);
			graphics.DrawLine(dark, right, bounds.Y, right, bottom);
		}

		/// <summary>The color of the caption of a group box: the text color, or the color the style names.</summary>
		public System.Drawing.Color GroupTitleColor => Theme.ColorsNoCopy[(int)(Theme.Style.Detail.GroupTitleRole ?? ThemeRole.Text)];

		/// <summary>
		/// Draws the box of a check box or the circle of a radio button in the older look of the style. Returns false for the
		/// default look; the caller then draws the modern glyph.
		/// </summary>
		public bool DrawChoiceGlyph(Graphics graphics, Rectangle glyph, bool radio, bool isChecked, bool indeterminate, bool enabled, bool hot, bool pressed)
		{
			ThemeStyle style = Theme.Style;
			if (style.Detail.ChoiceLook == ThemeChoiceLook.Default || glyph.Width < 7 || glyph.Height < 7)
			{
				return false;
			}
			System.Drawing.Color text = Theme.ColorsNoCopy[(int)ThemeRole.Text];
			System.Drawing.Color markColor = !enabled || indeterminate ? Theme.ColorsNoCopy[(int)ThemeRole.TextDisabled] : style.Detail.ChoiceMark.IsEmpty ? text : style.Detail.ChoiceMark;
			SmoothingMode smoothing = graphics.SmoothingMode;
			switch (style.Detail.ChoiceLook)
			{
			case ThemeChoiceLook.Classic:
				if (radio)
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.FillEllipse(Brush(enabled ? ThemeRole.InputBack : ThemeRole.Surface), glyph);
					DrawSunkenEllipse(graphics, glyph);
				}
				else
				{
					graphics.SmoothingMode = SmoothingMode.None;
					graphics.FillRectangle(Brush(enabled && !pressed ? ThemeRole.InputBack : ThemeRole.Surface), glyph);
					DrawBorder(graphics, glyph, ThemeBorder.Sunken, ThemeRole.Border, 0);
				}
				break;
			case ThemeChoiceLook.Luna:
				DrawLunaBox(graphics, glyph, radio, enabled, hot, pressed, style);
				break;
			default:
				DrawFlatBox(graphics, glyph, radio, enabled, hot, pressed, style);
				break;
			}
			if (isChecked || indeterminate)
			{
				if (radio)
				{
					if (!indeterminate)
					{
						graphics.SmoothingMode = SmoothingMode.AntiAlias;
						int dotSize = Math.Max(3, (glyph.Width * 5 + 6) / 13);
						Rectangle dot = new Rectangle(glyph.X + (glyph.Width - dotSize) / 2, glyph.Y + (glyph.Height - dotSize) / 2, dotSize, dotSize);
						graphics.FillEllipse(GdiCache.Brush(markColor), dot);
						if (style.Detail.ChoiceLook == ThemeChoiceLook.Luna && enabled && dot.Width >= 5)
						{
							graphics.FillEllipse(GdiCache.Brush(MxTheme.Lighten(markColor, 0.5f)), new Rectangle(dot.X + 1, dot.Y + 1, dot.Width - 3, dot.Height - 3));
							graphics.FillEllipse(GdiCache.Brush(markColor), new Rectangle(dot.X + 2, dot.Y + 2, Math.Max(1, dot.Width - 5), Math.Max(1, dot.Height - 5)));
						}
					}
				}
				else
				{
					graphics.SmoothingMode = SmoothingMode.None;
					DrawPixelMark(graphics, glyph, style.Detail.ChoiceLook == ThemeChoiceLook.Classic ? ClassicTick : LunaTick, markColor);
				}
			}
			graphics.SmoothingMode = smoothing;
			return true;
		}

		/// <summary>
		/// Draws the drop-down button of a combo box, or an up or down button of a spin box, in the older look of the style.
		/// <paramref name="direction"/> is 1 for down and 0 for up. Returns false for the default look.
		/// </summary>
		public bool DrawDropButton(Graphics graphics, Rectangle button, bool enabled, bool hot, bool pressed, int direction = 1)
		{
			ThemeDropButtonLook look = Theme.Style.Detail.DropButton;
			if (look == ThemeDropButtonLook.Default || button.Width < 6 || button.Height < 6)
			{
				return false;
			}
			Rectangle face = Rectangle.Inflate(button, -2, -2);
			SmoothingMode smoothing = graphics.SmoothingMode;
			System.Drawing.Color arrow = enabled ? Theme.ColorsNoCopy[(int)ThemeRole.Text] : Theme.ColorsNoCopy[(int)ThemeRole.TextDisabled];
			switch (look)
			{
			case ThemeDropButtonLook.Luna:
			{
				System.Drawing.Color accent = Theme.ColorsNoCopy[(int)ThemeRole.Accent];
				arrow = enabled ? MxTheme.Darken(accent, 0.45f) : Theme.ColorsNoCopy[(int)ThemeRole.TextDisabled];
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				System.Drawing.Color baseColor = !enabled ? Theme.ColorsNoCopy[(int)ThemeRole.Surface] : pressed ? MxTheme.Blend(System.Drawing.Color.White, accent, 0.45f) : hot ? MxTheme.Blend(System.Drawing.Color.White, accent, 0.16f) : MxTheme.Blend(System.Drawing.Color.White, accent, 0.3f);
				FillColorFace(graphics, face, baseColor, pressed ? 0f : hot ? 0.45f : 0.3f, 0.08f, 2, false);
				DrawRounded(graphics, face, enabled ? (hot || pressed ? System.Drawing.Color.FromArgb(251, 183, 74) : Theme.ColorsNoCopy[(int)ThemeRole.Border]) : LunaFrameDisabled, 2);
				break;
			}
			case ThemeDropButtonLook.Classic:
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.FillRectangle(Brush(ThemeRole.Surface), face);
				DrawBevel(graphics, face, pressed);
				if (pressed)
				{
					face.Offset(1, 1);
				}
				break;
			case ThemeDropButtonLook.Flat:
				graphics.SmoothingMode = SmoothingMode.None;
				if (enabled && (hot || pressed))
				{
					graphics.FillRectangle(Brush(pressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot), face);
					graphics.DrawRectangle(Pen(ThemeRole.Accent), face.X, face.Y, face.Width - 1, face.Height - 1);
				}
				else
				{
					graphics.FillRectangle(Brush(ThemeRole.Surface), face);
				}
				break;
			default:
				graphics.SmoothingMode = SmoothingMode.None;
				if (enabled && (hot || pressed))
				{
					GdiCache.FillGradient(graphics, face, Theme.ColorsNoCopy[(int)(pressed ? ThemeRole.AccentPressed : ThemeRole.AccentHot)], Theme.ColorsNoCopy[(int)(pressed ? ThemeRole.AccentPressed : ThemeRole.Accent)], LinearGradientMode.Vertical);
					graphics.DrawRectangle(Pen(ThemeRole.BorderStrong), face.X, face.Y, face.Width - 1, face.Height - 1);
				}
				else
				{
					GdiCache.FillGradient(graphics, face, Theme.ColorsNoCopy[(int)ThemeRole.SurfaceRaised], Theme.ColorsNoCopy[(int)ThemeRole.Surface], LinearGradientMode.Vertical);
				}
				break;
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (look == ThemeDropButtonLook.Luna)
			{
				int cx = face.X + face.Width / 2;
				int cy = face.Y + face.Height / 2;
				int s = Math.Max(2, Scaled(3));
				Pen pen = GdiCache.Pen(arrow, Math.Max(1.6f, Scale * 1.6f));
				float dy = direction == 1 ? 1f : -1f;
				graphics.DrawLine(pen, cx - s, cy - dy * s / 2f, cx, cy + dy * s / 2f);
				graphics.DrawLine(pen, cx, cy + dy * s / 2f, cx + s, cy - dy * s / 2f);
			}
			else
			{
				graphics.SmoothingMode = SmoothingMode.None;
				DrawArrowColor(graphics, face, direction, arrow);
			}
			graphics.SmoothingMode = smoothing;
			return true;
		}

		// A filled triangle in a color that is not a role.
		private void DrawArrowColor(Graphics graphics, Rectangle area, int direction, System.Drawing.Color color)
		{
			int cx = area.X + area.Width / 2;
			int cy = area.Y + area.Height / 2;
			int half = Math.Max(2, Scaled(3));
			Span<Point> triangle = stackalloc Point[3];
			if (direction == 1)
			{
				triangle[0] = new Point(cx - half, cy - half / 2);
				triangle[1] = new Point(cx + half + 1, cy - half / 2);
				triangle[2] = new Point(cx, cy + half / 2 + 1);
			}
			else
			{
				triangle[0] = new Point(cx - half, cy + half / 2);
				triangle[1] = new Point(cx + half + 1, cy + half / 2);
				triangle[2] = new Point(cx, cy - half / 2 - 1);
			}
			graphics.FillPolygon(GdiCache.Brush(color), triangle);
		}

		private void DrawLunaBox(Graphics graphics, Rectangle glyph, bool radio, bool enabled, bool hot, bool pressed, ThemeStyle style)
		{
			System.Drawing.Color frame = !enabled ? LunaFrameDisabled : style.Detail.ChoiceFrame.IsEmpty ? Theme.ColorsNoCopy[(int)ThemeRole.BorderStrong] : style.Detail.ChoiceFrame;
			if (radio)
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle outer = new Rectangle(glyph.X, glyph.Y, glyph.Width - 1, glyph.Height - 1);
				graphics.FillEllipse(GdiCache.Brush(enabled ? (pressed ? LunaFillPressed : System.Drawing.Color.White) : Theme.ColorsNoCopy[(int)ThemeRole.Surface]), outer);
				if (enabled && !pressed)
				{
					GraphicsState state = graphics.Save();
					graphics.SetClip(new Rectangle(outer.X, outer.Y + outer.Height / 2, outer.Width + 1, outer.Height), CombineMode.Intersect);
					graphics.FillEllipse(GdiCache.Brush(System.Drawing.Color.FromArgb(90, 214, 211, 200)), Rectangle.Inflate(outer, -1, -1));
					graphics.Restore(state);
				}
				graphics.DrawEllipse(GdiCache.Pen(frame), outer);
				if (enabled && hot && !pressed && !style.Detail.ChoiceHot.IsEmpty)
				{
					Rectangle ring = Rectangle.Inflate(outer, -1, -1);
					graphics.DrawEllipse(GdiCache.Pen(MxTheme.Lighten(style.Detail.ChoiceHot, 0.55f)), ring);
					graphics.DrawEllipse(GdiCache.Pen(style.Detail.ChoiceHot), Rectangle.Inflate(ring, -1, -1));
				}
				return;
			}
			graphics.SmoothingMode = SmoothingMode.None;
			if (enabled)
			{
				if (pressed)
				{
					graphics.FillRectangle(GdiCache.Brush(LunaFillPressed), glyph);
				}
				else
				{
					GdiCache.FillGradient(graphics, glyph, System.Drawing.Color.White, LunaFill, LinearGradientMode.ForwardDiagonal);
				}
			}
			else
			{
				graphics.FillRectangle(Brush(ThemeRole.Surface), glyph);
			}
			graphics.DrawRectangle(GdiCache.Pen(frame), glyph.X, glyph.Y, glyph.Width - 1, glyph.Height - 1);
			if (enabled && hot && !pressed && !style.Detail.ChoiceHot.IsEmpty)
			{
				Rectangle ring = Rectangle.Inflate(glyph, -1, -1);
				graphics.DrawRectangle(GdiCache.Pen(MxTheme.Lighten(style.Detail.ChoiceHot, 0.55f)), ring.X, ring.Y, ring.Width - 1, ring.Height - 1);
				ring.Inflate(-1, -1);
				graphics.DrawRectangle(GdiCache.Pen(style.Detail.ChoiceHot), ring.X, ring.Y, ring.Width - 1, ring.Height - 1);
			}
		}

		private void DrawFlatBox(Graphics graphics, Rectangle glyph, bool radio, bool enabled, bool hot, bool pressed, ThemeStyle style)
		{
			System.Drawing.Color frame = !enabled ? Theme.ColorsNoCopy[(int)ThemeRole.BorderDisabled] : style.Detail.ChoiceFrame.IsEmpty ? Theme.ColorsNoCopy[(int)ThemeRole.BorderStrong] : style.Detail.ChoiceFrame;
			System.Drawing.Color back = !enabled ? Theme.ColorsNoCopy[(int)ThemeRole.Surface] : pressed ? Theme.ColorsNoCopy[(int)ThemeRole.SurfacePressed] : hot ? Theme.ColorsNoCopy[(int)ThemeRole.SurfaceHot] : Theme.ColorsNoCopy[(int)ThemeRole.InputBack];
			if (radio)
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle outer = new Rectangle(glyph.X, glyph.Y, glyph.Width - 1, glyph.Height - 1);
				graphics.FillEllipse(GdiCache.Brush(back), outer);
				graphics.DrawEllipse(GdiCache.Pen(frame), outer);
				return;
			}
			graphics.SmoothingMode = SmoothingMode.None;
			graphics.FillRectangle(GdiCache.Brush(back), glyph);
			graphics.DrawRectangle(GdiCache.Pen(frame), glyph.X, glyph.Y, glyph.Width - 1, glyph.Height - 1);
		}

		// Draws a mark made of pixels in the middle of the box. A pixel is one device pixel at 96 DPI and grows with the scale.
		private void DrawPixelMark(Graphics graphics, Rectangle box, string[] rows, System.Drawing.Color color)
		{
			int unit = Math.Max(1, (int)Math.Round(Scale));
			int left = box.X + (box.Width - rows[0].Length * unit) / 2;
			int top = box.Y + (box.Height - rows.Length * unit) / 2;
			SolidBrush brush = GdiCache.Brush(color);
			for (int y = 0; y < rows.Length; y++)
			{
				string row = rows[y];
				int x = 0;
				while (x < row.Length)
				{
					if (row[x] != 'X')
					{
						x++;
						continue;
					}
					int start = x;
					while (x < row.Length && row[x] == 'X')
					{
						x++;
					}
					graphics.FillRectangle(brush, left + start * unit, top + y * unit, (x - start) * unit, unit);
				}
			}
		}
	}
}
