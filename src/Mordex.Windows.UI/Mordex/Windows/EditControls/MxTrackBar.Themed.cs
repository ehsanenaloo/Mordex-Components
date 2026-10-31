using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxTrackBar
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

		// Paints the track, the filled part, the thumb and the ticks from the default theme. The layout has been made by the caller.
		private void PaintThemed(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			if (IsClassicLook(theme))
			{
				PaintClassic(graphics, theme);
				return;
			}
			bool enabled = base.Enabled;
			bool horizontal = _orientation == Orientation.Horizontal;
			ThemedBackdrop.Paint(graphics, base.ClientRectangle, this);
			GraphicsState state = graphics.Save();
			if (RightToLeft == RightToLeft.Yes)
			{
				graphics.TranslateTransform(base.Width, 0);
				graphics.ScaleTransform(-1f, 1f);
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int radius = TrackThickness / 2;
			theme.FillRounded(graphics, _trackBounds, enabled ? ThemeRole.SurfaceAlt : ThemeRole.SurfaceDisabled, radius);
			Rectangle filled = _trackBounds;
			if (horizontal)
			{
				filled.Width = Math.Max(0, Math.Min(_trackBounds.Width, _thumbBounds.X + _thumbBounds.Width / 2 - _trackBounds.X));
			}
			else
			{
				int top = _thumbBounds.Y + _thumbBounds.Height / 2;
				filled.Y = Math.Max(_trackBounds.Y, Math.Min(_trackBounds.Bottom, top));
				filled.Height = _trackBounds.Bottom - filled.Y;
			}
			bool faceThumb = theme.Style.FaceThumb;
			if (filled.Width > 0 && filled.Height > 0 && !faceThumb)
			{
				theme.FillRounded(graphics, filled, enabled ? ThemeRole.Accent : ThemeRole.BorderDisabled, radius);
			}
			theme.DrawBorder(graphics, _trackBounds, theme.Style.InputBorder, enabled ? ThemeRole.Border : ThemeRole.BorderDisabled, radius);
			Pen tickPen = theme.Pen(enabled ? ThemeRole.BorderStrong : ThemeRole.BorderDisabled);
			graphics.SmoothingMode = SmoothingMode.Default;
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
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			ThemeRole thumb = !enabled ? ThemeRole.SurfaceDisabled : IsThumbPressed() ? ThemeRole.AccentPressed : IsThumbHot() ? ThemeRole.AccentHot : ThemeRole.Accent;
			int thumbRadius = Math.Min(theme.Shape.CornerRadius + 2, Math.Min(_thumbBounds.Width, _thumbBounds.Height) / 2);
			if (faceThumb)
			{
				thumb = !enabled ? ThemeRole.SurfaceDisabled : IsThumbPressed() ? ThemeRole.SurfacePressed : IsThumbHot() ? ThemeRole.SurfaceHot : ThemeRole.SurfaceRaised;
				thumbRadius = Math.Min(theme.Radius(ThemeElement.Button), Math.Min(_thumbBounds.Width, _thumbBounds.Height) / 2);
				if (theme.Style.PointerThumb && (_tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.TopLeft))
				{
					bool far = _tickStyle == TickStyle.BottomRight;
					int direction = horizontal ? (far ? 1 : 0) : (far ? 3 : 2);
					theme.DrawPointerThumb(graphics, _thumbBounds, direction, thumb, enabled ? ThemeRole.BorderStrong : ThemeRole.BorderDisabled);
				}
				else
				{
					theme.FillRounded(graphics, _thumbBounds, thumb, thumbRadius);
					theme.DrawRounded(graphics, _thumbBounds, enabled ? ThemeRole.Border : ThemeRole.BorderDisabled, thumbRadius);
				}
			}
			else
			{
				theme.FillRounded(graphics, _thumbBounds, thumb, thumbRadius);
				theme.DrawRounded(graphics, _thumbBounds, enabled ? thumb : ThemeRole.BorderDisabled, thumbRadius);
			}
			if (base.ContainsFocus && ShowFocusRectangle)
			{
				theme.DrawRounded(graphics, Rectangle.Inflate(_thumbBounds, 2, 2), ThemeRole.BorderFocus, thumbRadius + 2);
			}
			graphics.Restore(state);
		}
	}
}
