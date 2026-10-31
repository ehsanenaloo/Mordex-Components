using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal partial class StyledScrollBar : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			Invalidate();
		}

		private void UpdateThemeListening()
		{
			bool wanted = IsHandleCreated;
			if (wanted && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
			}
			else if (!wanted && _listensToThemes)
			{
				StopThemeListening();
			}
		}

		private void StopThemeListening()
		{
			if (_listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			StopThemeListening();
			base.OnHandleDestroyed(e);
		}

		// Track, thumb and arrow buttons from the roles of the default theme.
		private void RenderThemedScrollBar(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool vertical = _type == ScrollBarType.Vertical;
			if (ThemeScrollBarPainter.Applies(theme))
			{
				ThemeScrollBarPainter.Paint(graphics, theme, base.ClientRectangle, vertical, base.Enabled, Part(_scrollBarStyleControl2DataArray[0]), Part(_scrollBarStyleControl2DataArray[4]), Part(_scrollBarStyleControl2DataArray[2]));
				return;
			}
			graphics.FillRectangle(theme.Brush(ThemeRole.ScrollTrack), base.ClientRectangle);
			ScrollBarStyleControl2Data thumb = _scrollBarStyleControl2DataArray[2];
			if (thumb._isVisible && base.Enabled && !thumb._bounds.IsEmpty)
			{
				ThemeRole role = thumb._isPressed ? ThemeRole.ScrollThumbPressed : thumb._isHovered ? ThemeRole.ScrollThumbHot : ThemeRole.ScrollThumb;
				Rectangle bounds = thumb._bounds;
				int gap = theme.Scaled(2);
				bounds.Inflate(vertical ? -gap : 0, vertical ? 0 : -gap);
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					theme.FillRounded(graphics, bounds, role, Math.Min(theme.Shape.CornerRadius, 4));
				}
			}
			RenderThemedArrow(graphics, theme, _scrollBarStyleControl2DataArray[0], vertical ? 0 : 2);
			RenderThemedArrow(graphics, theme, _scrollBarStyleControl2DataArray[4], vertical ? 1 : 3);
		}

		private static ScrollBarPart Part(ScrollBarStyleControl2Data area)
		{
			return new ScrollBarPart { Bounds = area._bounds, Present = area._isVisible, Hot = area._isHovered, Pressed = area._isPressed };
		}

		// direction: 0 up, 1 down, 2 left, 3 right.
		private void RenderThemedArrow(Graphics graphics, ResolvedTheme theme, ScrollBarStyleControl2Data area, int direction)
		{
			if (!area._isVisible || area._bounds.IsEmpty)
			{
				return;
			}
			bool enabled = base.Enabled;
			if (enabled && (area._isPressed || area._isHovered))
			{
				graphics.FillRectangle(theme.Brush(area._isPressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot), area._bounds);
			}
			int half = Math.Max(2, theme.Scaled(3));
			int centerX = area._bounds.Left + area._bounds.Width / 2;
			int centerY = area._bounds.Top + area._bounds.Height / 2;
			Point[] triangle;
			switch (direction)
			{
			case 0:
				triangle = new[] { new Point(centerX - half, centerY + half / 2), new Point(centerX + half, centerY + half / 2), new Point(centerX, centerY - half / 2 - 1) };
				break;
			case 1:
				triangle = new[] { new Point(centerX - half, centerY - half / 2), new Point(centerX + half, centerY - half / 2), new Point(centerX, centerY + half / 2 + 1) };
				break;
			case 2:
				triangle = new[] { new Point(centerX + half / 2, centerY - half), new Point(centerX + half / 2, centerY + half), new Point(centerX - half / 2 - 1, centerY) };
				break;
			default:
				triangle = new[] { new Point(centerX - half / 2, centerY - half), new Point(centerX - half / 2, centerY + half), new Point(centerX + half / 2 + 1, centerY) };
				break;
			}
			graphics.FillPolygon(theme.Brush(enabled ? ThemeRole.Text : ThemeRole.TextDisabled), triangle);
		}
	}
}
