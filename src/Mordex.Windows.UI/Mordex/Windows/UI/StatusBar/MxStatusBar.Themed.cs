using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	public partial class MxStatusBar
	{
		private ThemeHook _themeHook;

		// The BackColor of Control: it knows whether the color is set on the bar or comes from the parent.
		private static readonly System.ComponentModel.PropertyDescriptor ControlBackColor = System.ComponentModel.TypeDescriptor.GetProperties(typeof(Control))["BackColor"];

		private bool IsThemed => true;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				if (!IsThemed)
				{
					return;
				}
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(IsThemed);
		}

		// Paints the bar, the panels and the size grip from the default theme. The layout has been made by the caller.
		private void PaintThemed(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			Rectangle client = base.ClientRectangle;
			// A back color the bar only inherits (a themed parent has the theme surface) is not a color of its own.
			if (BackColor == SystemColors.Control || !ControlBackColor.ShouldSerializeValue(this))
			{
				theme.FillBand(graphics, client, ThemeRole.StatusBarBack);
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(BackColor), client);
			}
			graphics.DrawLine(theme.Pen(ThemeRole.Border), client.Left, client.Top, client.Right - 1, client.Top);
			GraphicsState state = graphics.Save();
			if (_sizeGripState != null)
			{
				graphics.SetClip(new Rectangle(client.X, client.Y, Math.Max(0, _sizeGripState._bounds.Left + 1), client.Height), CombineMode.Intersect);
			}
			foreach (MxStatusBarPanel panel in _panels)
			{
				if (panel.Visible)
				{
					PaintThemedPanel(graphics, theme, panel, textLayoutRenderer, enabled);
				}
			}
			graphics.Restore(state);
			if (_sizeGripState != null)
			{
				Pen grip = theme.Pen(ThemeRole.BorderStrong);
				Rectangle bounds = _sizeGripState._bounds;
				int right = bounds.Right - 2;
				int bottom = bounds.Bottom - 2;
				int step = Math.Max(3, theme.Scaled(4));
				for (int i = 1; i <= 3; i++)
				{
					graphics.DrawLine(grip, right - i * step, bottom, right, bottom - i * step);
				}
			}
		}

		private void PaintThemedPanel(Graphics graphics, ResolvedTheme theme, MxStatusBarPanel panel, TextLayoutRenderer textLayoutRenderer, bool enabled)
		{
			Rectangle bounds = panel._bounds;
			Color custom = panel.FormatStyle.BackColor;
			if (!custom.IsEmpty)
			{
				graphics.FillRectangle(GdiCache.Brush(custom), bounds);
			}
			else if (enabled && panel.GetIsHot())
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceHot), bounds);
			}
			if (panel.PanelType == StatusBarPanelType.Item || panel.PanelType == StatusBarPanelType.ToggleKeyState)
			{
				string text = panel.GetDisplayText();
				bool hasImage = DrawPanelImage(graphics, panel, theme.Color(ThemeRole.StatusBarBack));
				if (text.Length > 0)
				{
					textLayoutRenderer.SetStringAlignment(panel.Alignment == HorizontalAlignment.Left ? StringAlignment.Near : panel.Alignment == HorizontalAlignment.Right ? StringAlignment.Far : hasImage ? StringAlignment.Near : StringAlignment.Center);
					Color fore = panel.FormatStyle.ForeColor;
					Brush brush = !enabled ? theme.Brush(ThemeRole.TextDisabled) : fore.IsEmpty ? theme.Brush(ThemeRole.StatusBarText) : GdiCache.Brush(fore);
					textLayoutRenderer.DrawTextWithBrush(graphics, text, panel._contentBounds, panel.FormatStyle.ResolveFont(Font), brush);
				}
			}
			else if (panel.PanelType == StatusBarPanelType.ProgressBar)
			{
				double fraction = ProgressFraction.Of(panel.ProgressBarValue, panel.ProgressBarMinValue, panel.ProgressBarMaxValue);
				Rectangle track = Rectangle.Inflate(bounds, -theme.Scaled(3), -theme.Scaled(4));
				if (track.Width > 0 && track.Height > 0)
				{
					SmoothingMode smoothing = graphics.SmoothingMode;
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					int radius = Math.Min(theme.Shape.CornerRadius, track.Height / 2);
					theme.FillRounded(graphics, track, ThemeRole.SurfaceAlt, radius);
					if (fraction > 0)
					{
						Rectangle fill = track;
						fill.Width = Math.Max(1, (int)(track.Width * fraction));
						theme.FillRounded(graphics, fill, ThemeRole.Accent, radius);
					}
					graphics.SmoothingMode = smoothing;
				}
			}
			if (panel.DrawBorder && theme.Style.Bevel == ThemeBevel.Raised)
			{
				theme.DrawThinSunken(graphics, Rectangle.Inflate(bounds, -1, -2));
			}
			else if (panel.DrawBorder)
			{
				graphics.DrawLine(theme.Pen(ThemeRole.Border), bounds.Right - 1, bounds.Top + theme.Scaled(3), bounds.Right - 1, bounds.Bottom - theme.Scaled(3));
			}
		}
	}
}
