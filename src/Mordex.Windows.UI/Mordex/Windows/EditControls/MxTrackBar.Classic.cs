using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxTrackBar
	{
		// The Windows Classic look, as the track bar of Windows 95 to 2000 draws it without visual styles: a sunken channel
		// (shadow and dark shadow on the top and left, light and highlight on the bottom and right), a thumb with the raised
		// edge of DrawEdge (highlight and light on the edges that face up or left, dark shadow and shadow on the others), a
		// point toward the ticks, black tick marks and a dotted focus rectangle around the control.
		private static bool IsClassicLook(ResolvedTheme theme)
		{
			return theme.Theme.Style.ButtonLook == ThemeButtonLook.Classic;
		}

		private void PaintClassic(Graphics graphics, ResolvedTheme theme)
		{
			bool enabled = base.Enabled;
			bool horizontal = _orientation == Orientation.Horizontal;
			ThemedBackdrop.Paint(graphics, base.ClientRectangle, this);
			GraphicsState state = graphics.Save();
			if (RightToLeft == RightToLeft.Yes)
			{
				graphics.TranslateTransform(base.Width, 0);
				graphics.ScaleTransform(-1f, 1f);
			}
			graphics.SmoothingMode = SmoothingMode.None;
			DrawClassicChannel(graphics, theme, _trackBounds);
			// A disabled Windows 2000 track bar draws its ticks in the shadow color.
			Pen tickPen = theme.Pen(enabled ? ThemeRole.Text : ThemeRole.Border);
			if (horizontal)
			{
				float start = _trackBounds.X + TrackEndInset;
				float length = _trackBounds.Width - 2 * TrackEndInset;
				if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.TopLeft)
				{
					DrawTickRow(graphics, tickPen, base.ClientRectangle.Y + TickNearOffset, start, length);
				}
				if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.BottomRight)
				{
					DrawTickRow(graphics, tickPen, _trackBounds.Bottom + TickFarOffset, start, length);
				}
			}
			else
			{
				float start = _trackBounds.Y + TrackEndInset;
				float length = _trackBounds.Height - 2 * TrackEndInset;
				if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.TopLeft)
				{
					DrawTickRow(graphics, tickPen, base.ClientRectangle.X + TickNearOffset, start, length);
				}
				if (_tickStyle == TickStyle.Both || _tickStyle == TickStyle.BottomRight)
				{
					DrawTickRow(graphics, tickPen, _trackBounds.Right + TickFarOffset, start, length);
				}
			}
			// Windows points the thumb away from the top or left ticks; only ticks on both sides give a plain box.
			int direction = _tickStyle == TickStyle.Both ? -1 : _tickStyle == TickStyle.TopLeft ? (horizontal ? 0 : 2) : (horizontal ? 1 : 3);
			// Windows 2000 fills a disabled thumb with the same check as a pressed one.
			DrawClassicThumb(graphics, theme, _thumbBounds, direction, !enabled || IsThumbPressed());
			if (base.ContainsFocus && ShowFocusRectangle)
			{
				DrawClassicFocus(graphics, theme, base.ClientRectangle);
			}
			graphics.Restore(state);
		}

		// The channel is the EDGE_SUNKEN of DrawEdge: shadow outside and dark shadow inside on the top and left, light inside
		// and highlight outside on the bottom and right.
		internal static void DrawClassicChannel(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			if (bounds.Width < 4 || bounds.Height < 4)
			{
				return;
			}
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			Pen shadow = theme.Pen(ThemeRole.Border);
			Pen dark = theme.Pen(ThemeRole.BorderStrong);
			Pen light = theme.Pen(ThemeRole.Surface);
			Pen highlight = theme.LightEdgePen;
			graphics.DrawLine(shadow, bounds.X, bounds.Y, right - 1, bounds.Y);
			graphics.DrawLine(shadow, bounds.X, bounds.Y, bounds.X, bottom - 1);
			graphics.DrawLine(highlight, bounds.X, bottom, right, bottom);
			graphics.DrawLine(highlight, right, bounds.Y, right, bottom);
			graphics.DrawLine(dark, bounds.X + 1, bounds.Y + 1, right - 2, bounds.Y + 1);
			graphics.DrawLine(dark, bounds.X + 1, bounds.Y + 1, bounds.X + 1, bottom - 2);
			graphics.DrawLine(light, bounds.X + 1, bottom - 1, right - 1, bottom - 1);
			graphics.DrawLine(light, right - 1, bounds.Y + 1, right - 1, bottom - 1);
		}

		// Draws the thumb. Direction: 0 points up, 1 down, 2 left, 3 right, -1 a box with no point. A pressed thumb is filled
		// with the checkered highlight of Windows Classic.
		internal static void DrawClassicThumb(Graphics graphics, ResolvedTheme theme, Rectangle bounds, int direction, bool pressed)
		{
			bool vertical = direction == 2 || direction == 3;
			int across = vertical ? bounds.Height : bounds.Width;
			int along = vertical ? bounds.Width : bounds.Height;
			if (across < 4 || along < 4)
			{
				return;
			}
			int tip = direction < 0 ? 0 : Math.Min((across - 1) / 2, along - 3);
			Point[] outer = ThumbOutline(across, along, tip, 0);
			Point[] inner = ThumbOutline(across, along, tip, 1);
			for (int i = 0; i < outer.Length; i++)
			{
				outer[i] = MapThumbPoint(bounds, direction, outer[i], along);
				inner[i] = MapThumbPoint(bounds, direction, inner[i], along);
			}
			Color face = theme.Color(ThemeRole.Surface);
			if (pressed)
			{
				using (HatchBrush dither = new HatchBrush(HatchStyle.Percent50, theme.LightEdgeColor, face))
				{
					graphics.RenderingOrigin = bounds.Location;
					graphics.FillPolygon(dither, outer);
				}
			}
			else
			{
				graphics.FillPolygon(theme.Brush(ThemeRole.Surface), outer);
			}
			PointF center = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
			// Light lines first, so the dark lines own the corners where the two meet, as DrawEdge does.
			DrawThumbEdges(graphics, outer, center, theme.LightEdgePen, null);
			DrawThumbEdges(graphics, outer, center, null, theme.Pen(ThemeRole.BorderStrong));
			DrawThumbEdges(graphics, inner, center, theme.Pen(ThemeRole.Surface), null);
			DrawThumbEdges(graphics, inner, center, null, theme.Pen(ThemeRole.Border));
		}

		// The outline of a thumb that points down, in its own coordinates: across from 0, along from 0 to the point.
		private static Point[] ThumbOutline(int across, int along, int tip, int inset)
		{
			int right = across - 1 - inset;
			int left = inset;
			int top = inset;
			if (tip == 0)
			{
				int bottom = along - 1 - inset;
				return new[] { new Point(left, top), new Point(right, top), new Point(right, bottom), new Point(left, bottom) };
			}
			int shoulder = along - 1 - tip;
			return new[] { new Point(left, top), new Point(right, top), new Point(right, shoulder), new Point(tip, along - 1 - inset), new Point(left, shoulder) };
		}

		private static Point MapThumbPoint(Rectangle bounds, int direction, Point p, int along)
		{
			switch (direction)
			{
			case 0:
				return new Point(bounds.X + p.X, bounds.Y + along - 1 - p.Y);
			case 2:
				return new Point(bounds.X + along - 1 - p.Y, bounds.Y + p.X);
			case 3:
				return new Point(bounds.X + p.Y, bounds.Y + p.X);
			default:
				return new Point(bounds.X + p.X, bounds.Y + p.Y);
			}
		}

		// The light comes from the top left: an edge that faces left or up, even partly, gets the light pen; an edge that faces
		// only right, down or both gets the dark pen. So the slope of a thumb that points down is light on the left and dark
		// on the right.
		private static void DrawThumbEdges(Graphics graphics, Point[] outline, PointF center, Pen light, Pen dark)
		{
			for (int i = 0; i < outline.Length; i++)
			{
				Point a = outline[i];
				Point b = outline[(i + 1) % outline.Length];
				int nx = b.Y - a.Y;
				int ny = a.X - b.X;
				float midX = (a.X + b.X) / 2f - center.X;
				float midY = (a.Y + b.Y) / 2f - center.Y;
				if (nx * midX + ny * midY < 0)
				{
					nx = -nx;
					ny = -ny;
				}
				bool facesLight = nx < 0 || ny < 0;
				Pen pen = facesLight ? light : dark;
				if (pen != null)
				{
					graphics.DrawLine(pen, a, b);
				}
			}
		}

		// DrawFocusRect: every other pixel of the edge of the control.
		private static void DrawClassicFocus(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			Brush brush = theme.Brush(ThemeRole.BorderFocus);
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			for (int x = bounds.X; x <= right; x++)
			{
				DotIfEven(graphics, brush, bounds, x, bounds.Y);
				DotIfEven(graphics, brush, bounds, x, bottom);
			}
			for (int y = bounds.Y + 1; y < bottom; y++)
			{
				DotIfEven(graphics, brush, bounds, bounds.X, y);
				DotIfEven(graphics, brush, bounds, right, y);
			}
		}

		private static void DotIfEven(Graphics graphics, Brush brush, Rectangle bounds, int x, int y)
		{
			if (((x - bounds.X) + (y - bounds.Y)) % 2 == 0)
			{
				graphics.FillRectangle(brush, x, y, 1, 1);
			}
		}
	}
}
