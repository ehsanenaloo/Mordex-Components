using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonControl
	{
		private ThemeSubscription _themeSubscription;

		private ThemeSubscription ThemeSub => _themeSubscription ?? (_themeSubscription = new ThemeSubscription(OnDefaultThemeChanged));

		private void OnDefaultThemeChanged()
		{
			if (!IsDisposed)
			{
				Invalidate();
			}
		}

		// Listens to the theme manager only while the bar is themed and has a handle.
		private void UpdateThemeListening()
		{
			bool wanted = IsHandleCreated;
			if (wanted || _themeSubscription != null)
			{
				ThemeSub.Set(wanted);
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeSubscription?.Set(false);
			base.OnHandleDestroyed(e);
		}

		// The system arrow size is for the system DPI; scale it to the DPI of this window.
		private int ArrowHeight()
		{
			return ScaleToDevice(SystemInformation.VerticalScrollBarArrowHeight, SystemInformation.VerticalScrollBarWidth, SystemInformation.GetVerticalScrollBarWidthForDpi(DeviceDpi));
		}

		private int ArrowWidth()
		{
			return ScaleToDevice(SystemInformation.HorizontalScrollBarArrowWidth, SystemInformation.HorizontalScrollBarHeight, SystemInformation.GetHorizontalScrollBarHeightForDpi(DeviceDpi));
		}

		private static int ScaleToDevice(int value, int systemReference, int deviceReference)
		{
			return systemReference <= 0 ? value : (int)Math.Round(value * (double)deviceReference / systemReference);
		}

		// Track, thumb and arrows from the roles of the default theme.
		private void PaintDefaultTheme(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			bool vertical = _orientation == ScrollBarType.Vertical;
			graphics.FillRectangle(theme.Brush(ThemeRole.ScrollTrack), base.ClientRectangle);
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			RibbonControlData thumb = _areas[2];
			if (thumb._isVisible && enabled && !thumb._bounds.IsEmpty)
			{
				int inset = theme.Scaled(3);
				Rectangle bounds = vertical ? Rectangle.Inflate(thumb._bounds, -inset, 0) : Rectangle.Inflate(thumb._bounds, 0, -inset);
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					ThemeRole role = thumb._isPressed ? ThemeRole.ScrollThumbPressed : thumb._isHot ? ThemeRole.ScrollThumbHot : ThemeRole.ScrollThumb;
					theme.FillRounded(graphics, bounds, role, Math.Min(theme.Shape.CornerRadius + 1, Math.Min(bounds.Width, bounds.Height) / 2));
				}
			}
			PaintDefaultThemeArrow(graphics, theme, _areas[0], vertical, -1);
			PaintDefaultThemeArrow(graphics, theme, _areas[4], vertical, 1);
			graphics.SmoothingMode = smoothing;
		}

		private void PaintDefaultThemeArrow(Graphics graphics, ResolvedTheme theme, RibbonControlData area, bool vertical, int direction)
		{
			if (!area._isVisible || area._bounds.IsEmpty)
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
