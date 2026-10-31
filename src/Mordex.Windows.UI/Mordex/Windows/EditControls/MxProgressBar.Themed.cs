using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxProgressBar
	{
		private ThemeHook _themeHook;

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

		private static double Contrast(Color a, Color b)
		{
			return ColorContrast.Ratio(a, b);
		}

		// Paints the track, the bar and the percentage from the default theme.
		private void PaintThemed(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			Rectangle track = base.ClientRectangle;
			bool rtl = RightToLeft == RightToLeft.Yes;
			int radius = Math.Min(theme.Radius(ThemeElement.Button), Math.Min(track.Width, track.Height) / 2);
			ThemeRole trackRole = enabled ? ThemeRole.SurfaceAlt : ThemeRole.SurfaceDisabled;
			ThemeRole barRole = enabled ? ThemeRole.Accent : ThemeRole.BorderDisabled;
			ThemedBackdrop.Paint(graphics, track, this);
			System.Drawing.Drawing2D.SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			theme.FillRounded(graphics, track, trackRole, radius);
			bool bordered = _borderStyle != Mordex.Windows.UI.BorderStyle.None;
			Rectangle bar = bordered ? Rectangle.Inflate(track, -1, -1) : track;
			double progress = ProgressFraction.Of(Value, _minimum, _maximum);
			int fill = Math.Max(0, Math.Min(bar.Width, (int)(bar.Width * progress)));
			Rectangle fillBounds = new Rectangle(rtl ? bar.Right - fill : bar.X, bar.Y, fill, bar.Height);
			if (theme.Style.ProgressLook != ThemeProgressLook.Default)
			{
				int pad = bordered ? theme.Scaled(theme.Style.ProgressLook == ThemeProgressLook.Blocks ? 2 : 1) : 0;
				Rectangle area = Rectangle.Inflate(track, -pad, -pad);
				float fraction = (float)progress;
				theme.DrawProgressFill(graphics, area, fraction, rtl, enabled, Math.Max(0, radius - 1));
				fillBounds = new Rectangle(rtl ? area.Right - (int)(area.Width * fraction) : area.X, area.Y, (int)(area.Width * fraction), area.Height);
			}
			else if (fill > 0)
			{
				theme.FillRounded(graphics, fillBounds, barRole, Math.Max(0, radius - 1));
			}
			if (bordered)
			{
				theme.DrawBorder(graphics, track, theme.Style.InputBorder, enabled ? ThemeRole.Border : ThemeRole.BorderDisabled, radius);
			}
			graphics.SmoothingMode = smoothing;
			if (_showPercentage)
			{
				int percent = (int)(100.0 * progress);
				TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
				flags |= _percentageAlignment == StringAlignment.Center ? TextFormatFlags.HorizontalCenter : (_percentageAlignment == StringAlignment.Far) != rtl ? TextFormatFlags.Right : TextFormatFlags.Left;
				if (rtl)
				{
					flags |= TextFormatFlags.RightToLeft;
				}
				Rectangle area = Rectangle.Inflate(track, -theme.Scaled(4), 0);
				string text = percent + "%";
				Color onTrack = theme.Color(enabled ? ThemeRole.Text : ThemeRole.TextDisabled);
				Color trackColor = theme.Color(trackRole);
				Color fillColor = theme.Style.ProgressLook != ThemeProgressLook.Default ? theme.ProgressColor : theme.Color(barRole);
				Color onBar = theme.Color(enabled ? ThemeRole.TextOnAccent : ThemeRole.TextDisabled);
				if (Contrast(onBar, fillColor) < Contrast(onTrack, fillColor))
				{
					onBar = onTrack;
				}
				if (theme.Style.ProgressLook == ThemeProgressLook.Blocks)
				{
					// Blocks leave gaps of track color, so one color must read on both.
					double barReach = Math.Min(Contrast(onBar, fillColor), Contrast(onBar, trackColor));
					double trackReach = Math.Min(Contrast(onTrack, fillColor), Contrast(onTrack, trackColor));
					if (Math.Max(barReach, trackReach) >= 3)
					{
						TextRenderer.DrawText(graphics, text, Font, area, barReach >= trackReach ? onBar : onTrack, flags);
						return;
					}
				}
				// The part of the text over the bar uses the bar text color and the rest uses the track text color. TextRenderer
				// takes the clip of the Graphics only with PreserveGraphicsClipping.
				flags |= TextFormatFlags.PreserveGraphicsClipping;
				System.Drawing.Region clip = graphics.Clip;
				try
				{
					graphics.SetClip(fillBounds, System.Drawing.Drawing2D.CombineMode.Intersect);
					TextRenderer.DrawText(graphics, text, Font, area, onBar, flags);
					graphics.Clip = clip;
					graphics.ExcludeClip(fillBounds);
					TextRenderer.DrawText(graphics, text, Font, area, onTrack, flags);
				}
				finally
				{
					graphics.Clip = clip;
					clip.Dispose();
				}
			}
		}
	}
}
