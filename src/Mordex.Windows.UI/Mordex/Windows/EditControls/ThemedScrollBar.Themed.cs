using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	internal partial class ThemedScrollBar
	{
		private ThemeHook _themeHook;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(true);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		// Paints the track, the thumb and the arrows from the default theme.
		private void PaintDefaultTheme(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			bool vertical = _scrollBarType == ScrollBarType.Vertical;
			if (ThemeScrollBarPainter.Applies(theme))
			{
				ThemeScrollBarPainter.Paint(graphics, theme, base.ClientRectangle, vertical, enabled, Part(_areas[AreaLowArrow]), Part(_areas[AreaHighArrow]), Part(_areas[AreaThumb]));
				return;
			}
			graphics.FillRectangle(theme.Brush(ThemeRole.ScrollTrack), base.ClientRectangle);
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			ScrollAreaState thumb = _areas[AreaThumb];
			if (thumb._isPresent && enabled && !thumb._bounds.IsEmpty)
			{
				int inset = theme.Scaled(3);
				Rectangle bounds = vertical ? Rectangle.Inflate(thumb._bounds, -inset, 0) : Rectangle.Inflate(thumb._bounds, 0, -inset);
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					ThemeRole role = thumb._isPressed ? ThemeRole.ScrollThumbPressed : thumb._isHot ? ThemeRole.ScrollThumbHot : ThemeRole.ScrollThumb;
					theme.FillRounded(graphics, bounds, role, Math.Min(theme.Shape.CornerRadius + 1, Math.Min(bounds.Width, bounds.Height) / 2));
				}
			}
			PaintDefaultThemeArrow(graphics, theme, _areas[AreaLowArrow], vertical, -1);
			PaintDefaultThemeArrow(graphics, theme, _areas[AreaHighArrow], vertical, 1);
			graphics.SmoothingMode = smoothing;
		}

		private static ScrollBarPart Part(ScrollAreaState area)
		{
			return new ScrollBarPart { Bounds = area._bounds, Present = area._isPresent, Hot = area._isHot, Pressed = area._isPressed };
		}

		private void PaintDefaultThemeArrow(Graphics graphics, ResolvedTheme theme, ScrollAreaState area, bool vertical, int direction)
		{
			if (!area._isPresent || area._bounds.IsEmpty)
			{
				return;
			}
			bool enabled = base.Enabled;
			if (enabled && (area._isHot || area._isPressed))
			{
				graphics.FillRectangle(theme.Brush(area._isPressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot), area._bounds);
			}
			Pen pen = theme.Pen(!enabled ? ThemeRole.TextDisabled : area._isHot || area._isPressed ? ThemeRole.Text : ThemeRole.TextMuted);
			float cx = area._bounds.X + area._bounds.Width / 2f;
			float cy = area._bounds.Y + area._bounds.Height / 2f;
			float s = Math.Max(2f, theme.Scaled(3));
			if (vertical)
			{
				float tip = cy + direction * s / 2f;
				float back = cy - direction * s / 2f;
				graphics.DrawLine(pen, cx - s, back, cx, tip);
				graphics.DrawLine(pen, cx, tip, cx + s, back);
			}
			else
			{
				float tip = cx + direction * s / 2f;
				float back = cx - direction * s / 2f;
				graphics.DrawLine(pen, back, cy - s, tip, cy);
				graphics.DrawLine(pen, tip, cy, back, cy + s);
			}
		}
	}
}
