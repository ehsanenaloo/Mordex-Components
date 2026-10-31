using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	// Painting that follows the ThemeStyle of the theme: radii of each kind of face, 3D and etched edges, arrow and check
	// marks, and bands. Nothing here makes a GDI+ object during a paint, except the first time a size or a shape is used.
	public sealed partial class ResolvedTheme
	{
		private HatchBrush _trackHatch;

		private Pen _lightEdgePen;

		private SolidBrush _lightEdgeBrush;

		public ThemeStyle Style => Theme.Style;

		// The light side of a 3D edge: the color the style names, or the raised surface color.
		internal Color LightEdgeColor => Theme.Style.BevelLight.IsEmpty ? Theme.ColorsNoCopy[(int)ThemeRole.SurfaceRaised] : Theme.Style.BevelLight;

		internal Pen LightEdgePen
		{
			get
			{
				lock (_cacheLock)
				{
				if (_lightEdgePen == null)
				{
					ThrowIfDisposed();
					_lightEdgePen = new Pen(LightEdgeColor, 1f);
				}
				return _lightEdgePen;
				}
			}
		}

		internal SolidBrush LightEdgeBrush
		{
			get
			{
				lock (_cacheLock)
				{
				if (_lightEdgeBrush == null)
				{
					ThrowIfDisposed();
					_lightEdgeBrush = new SolidBrush(LightEdgeColor);
				}
				return _lightEdgeBrush;
				}
			}
		}

		/// <summary>The corner radius of a kind of face, in 96 DPI pixels.</summary>
		public int Radius(ThemeElement element)
		{
			return Theme.Style.RadiusOf(element, Theme.Shape.CornerRadius);
		}

		/// <summary>
		/// The corner radius of a kind of face. A style that sets none gives the radius of the shape plus
		/// <paramref name="extraFromShape"/>, which is what controls with a rounder face than the shape added before.
		/// </summary>
		public int Radius(ThemeElement element, int extraFromShape)
		{
			return Theme.Style.RadiusOf(element, Theme.Shape.CornerRadius + extraFromShape);
		}

		/// <summary>
		/// Fills a straight band (a header, a toolbar, a status bar, a caption) with the face gradient of the theme. A theme
		/// without a gradient gets the plain color.
		/// </summary>
		public void FillBand(Graphics graphics, Rectangle bounds, ThemeRole role, LinearGradientMode mode = LinearGradientMode.Vertical)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			FillFace(graphics, bounds, role, 0, mode, false);
		}

		/// <summary>Draws the edge of a part in the way the style says for its kind. Flat edges use <paramref name="flatRole"/>.</summary>
		public void DrawElementBorder(Graphics graphics, Rectangle bounds, ThemeElement element, ThemeRole flatRole)
		{
			ThemeStyle style = Theme.Style;
			ThemeBorder kind;
			switch (element)
			{
			case ThemeElement.Input:
				kind = style.InputBorder;
				break;
			case ThemeElement.Panel:
				kind = style.PanelBorder;
				break;
			case ThemeElement.Header:
				kind = style.HeaderBorder;
				break;
			default:
				kind = ThemeBorder.Flat;
				break;
			}
			DrawBorder(graphics, bounds, kind, flatRole, Radius(element));
		}

		/// <summary>Draws an edge of the given kind. The edge lies inside the bounds.</summary>
		public void DrawBorder(Graphics graphics, Rectangle bounds, ThemeBorder kind, ThemeRole flatRole, int radius)
		{
			if (kind == ThemeBorder.Flat || bounds.Width < 4 || bounds.Height < 4)
			{
				DrawRounded(graphics, bounds, flatRole, radius);
				return;
			}
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			switch (kind)
			{
			case ThemeBorder.Raised:
				DrawBevel(graphics, bounds, false);
				break;
			case ThemeBorder.Sunken:
				graphics.DrawLine(Pen(ThemeRole.Border), bounds.X, bounds.Y, right - 1, bounds.Y);
				graphics.DrawLine(Pen(ThemeRole.Border), bounds.X, bounds.Y, bounds.X, bottom - 1);
				graphics.DrawLine(LightEdgePen, bounds.X, bottom, right, bottom);
				graphics.DrawLine(LightEdgePen, right, bounds.Y, right, bottom);
				graphics.DrawLine(Pen(ThemeRole.BorderStrong), bounds.X + 1, bounds.Y + 1, right - 2, bounds.Y + 1);
				graphics.DrawLine(Pen(ThemeRole.BorderStrong), bounds.X + 1, bounds.Y + 1, bounds.X + 1, bottom - 2);
				graphics.DrawLine(Pen(ThemeRole.Surface), bounds.X + 1, bottom - 1, right - 1, bottom - 1);
				graphics.DrawLine(Pen(ThemeRole.Surface), right - 1, bounds.Y + 1, right - 1, bottom - 1);
				break;
			default:
				graphics.DrawRectangle(Pen(ThemeRole.Border), bounds.X, bounds.Y, bounds.Width - 2, bounds.Height - 2);
				graphics.DrawRectangle(LightEdgePen, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
				break;
			}
		}

		/// <summary>
		/// Draws an arrow in the middle of an area. <paramref name="direction"/>: 0 up, 1 down, 2 left, 3 right. The
		/// <paramref name="fallback"/> shape is used when the style leaves the arrow to the control.
		/// </summary>
		public void DrawArrow(Graphics graphics, Rectangle area, int direction, ThemeRole role, ThemeArrowGlyph fallback)
		{
			ThemeArrowGlyph glyph = Theme.Style.Arrow == ThemeArrowGlyph.Default ? fallback : Theme.Style.Arrow;
			int cx = area.X + area.Width / 2;
			int cy = area.Y + area.Height / 2;
			int half = Math.Max(2, Scaled(3));
			if (glyph == ThemeArrowGlyph.Triangle)
			{
				Span<Point> triangle = stackalloc Point[3];
				switch (direction)
				{
				case 0:
					triangle[0] = new Point(cx - half, cy + half / 2);
					triangle[1] = new Point(cx + half, cy + half / 2);
					triangle[2] = new Point(cx, cy - half / 2 - 1);
					break;
				case 1:
					triangle[0] = new Point(cx - half, cy - half / 2);
					triangle[1] = new Point(cx + half, cy - half / 2);
					triangle[2] = new Point(cx, cy + half / 2 + 1);
					break;
				case 2:
					triangle[0] = new Point(cx + half / 2, cy - half);
					triangle[1] = new Point(cx + half / 2, cy + half);
					triangle[2] = new Point(cx - half / 2 - 1, cy);
					break;
				default:
					triangle[0] = new Point(cx - half / 2, cy - half);
					triangle[1] = new Point(cx - half / 2, cy + half);
					triangle[2] = new Point(cx + half / 2 + 1, cy);
					break;
				}
				graphics.FillPolygon(Brush(role), triangle);
				return;
			}
			Pen pen = Pen(role);
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			float s = half;
			switch (direction)
			{
			case 0:
				graphics.DrawLine(pen, cx - s, cy + s / 2f, cx, cy - s / 2f);
				graphics.DrawLine(pen, cx, cy - s / 2f, cx + s, cy + s / 2f);
				break;
			case 1:
				graphics.DrawLine(pen, cx - s, cy - s / 2f, cx, cy + s / 2f);
				graphics.DrawLine(pen, cx, cy + s / 2f, cx + s, cy - s / 2f);
				break;
			case 2:
				graphics.DrawLine(pen, cx + s / 2f, cy - s, cx - s / 2f, cy);
				graphics.DrawLine(pen, cx - s / 2f, cy, cx + s / 2f, cy + s);
				break;
			default:
				graphics.DrawLine(pen, cx - s / 2f, cy - s, cx + s / 2f, cy);
				graphics.DrawLine(pen, cx + s / 2f, cy, cx - s / 2f, cy + s);
				break;
			}
			graphics.SmoothingMode = smoothing;
		}

		/// <summary>Draws the mark of a checked box inside <paramref name="box"/> in the shape the style asks for.</summary>
		public void DrawCheck(Graphics graphics, Rectangle box, ThemeRole role, ThemeCheckGlyph glyph)
		{
			DrawCheck(graphics, box, Pen(role, glyph == ThemeCheckGlyph.BoldTick ? 2 : 1), glyph);
		}

		/// <summary>Draws the mark in a color that is not a role. The pen comes from <see cref="GdiCache"/>.</summary>
		public void DrawCheck(Graphics graphics, Rectangle box, Color color, ThemeCheckGlyph glyph)
		{
			DrawCheck(graphics, box, GdiCache.Pen(color, Math.Max(1f, Scale) * (glyph == ThemeCheckGlyph.BoldTick ? 2f : 1f)), glyph);
		}

		private static void DrawCheck(Graphics graphics, Rectangle box, Pen pen, ThemeCheckGlyph glyph)
		{
			if (box.Width < 4 || box.Height < 4)
			{
				return;
			}
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			float x = box.X;
			float y = box.Y;
			float w = box.Width;
			float h = box.Height;
			if (glyph == ThemeCheckGlyph.Cross)
			{
				graphics.DrawLine(pen, x + w * 0.25f, y + h * 0.25f, x + w * 0.75f, y + h * 0.75f);
				graphics.DrawLine(pen, x + w * 0.75f, y + h * 0.25f, x + w * 0.25f, y + h * 0.75f);
			}
			else
			{
				graphics.DrawLine(pen, x + w * 0.22f, y + h * 0.52f, x + w * 0.42f, y + h * 0.74f);
				graphics.DrawLine(pen, x + w * 0.42f, y + h * 0.74f, x + w * 0.78f, y + h * 0.28f);
			}
			graphics.SmoothingMode = smoothing;
		}

		// The dithered track of a classic scroll bar: every other pixel is the face color and the light color.
		internal HatchBrush TrackHatch
		{
			get
			{
				lock (_cacheLock)
				{
				if (_trackHatch == null)
				{
					ThrowIfDisposed();
					_trackHatch = new HatchBrush(HatchStyle.Percent50, Theme.ColorsNoCopy[(int)ThemeRole.Surface], LightEdgeColor);
				}
				return _trackHatch;
				}
			}
		}

		private void DisposeStyleObjects()
		{
			DisposeLookObjects();
			_trackHatch?.Dispose();
			_trackHatch = null;
			_lightEdgePen?.Dispose();
			_lightEdgePen = null;
			_lightEdgeBrush?.Dispose();
			_lightEdgeBrush = null;
		}
	}
}
