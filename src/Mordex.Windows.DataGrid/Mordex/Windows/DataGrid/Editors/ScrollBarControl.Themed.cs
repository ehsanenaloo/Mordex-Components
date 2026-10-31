using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Editors
{
	internal partial class ScrollBarControl
	{
		// Areas: 0 is the first arrow, 4 the last arrow, 2 the thumb. The two tracks (1 and 3) show the track color.
		private void DrawThemedScrollBar(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool vertical = _type == ScrollBarType.Vertical;
			bool reversed = !vertical && RightToLeft == RightToLeft.Yes;
			if (!reversed && ThemeScrollBarPainter.Applies(theme))
			{
				ThemeScrollBarPainter.Paint(graphics, theme, base.ClientRectangle, vertical, base.Enabled, Part(_controlStyleControl2DataArray[0]), Part(_controlStyleControl2DataArray[4]), Part(_controlStyleControl2DataArray[2]));
				return;
			}
			graphics.FillRectangle(theme.Brush(ThemeRole.ScrollTrack), base.ClientRectangle);
			DrawThemedArrow(graphics, theme, 0, vertical ? ArrowDirection.Up : (reversed ? ArrowDirection.Right : ArrowDirection.Left));
			DrawThemedArrow(graphics, theme, 4, vertical ? ArrowDirection.Down : (reversed ? ArrowDirection.Left : ArrowDirection.Right));
			ControlStyleControl2Data thumb = _controlStyleControl2DataArray[2];
			if (thumb._isVisible && base.Enabled)
			{
				Rectangle bounds = MirrorForRightToLeft(thumb._bounds);
				int across = theme.Scaled(3);
				int along = theme.Scaled(1);
				bounds.Inflate(vertical ? -across : -along, vertical ? -along : -across);
				ThemeRole role = thumb._isPressed ? ThemeRole.ScrollThumbPressed : thumb._controlStyleControl2DataBoolean ? ThemeRole.ScrollThumbHot : ThemeRole.ScrollThumb;
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					theme.FillRounded(graphics, bounds, role, theme.Shape.CornerRadius);
				}
			}
		}

		private static ScrollBarPart Part(ControlStyleControl2Data area)
		{
			return new ScrollBarPart { Bounds = area._bounds, Present = area._isVisible, Hot = area._controlStyleControl2DataBoolean, Pressed = area._isPressed };
		}

		private void DrawThemedArrow(Graphics graphics, ResolvedTheme theme, int area, ArrowDirection direction)
		{
			ControlStyleControl2Data data = _controlStyleControl2DataArray[area];
			if (!data._isVisible)
			{
				return;
			}
			Rectangle bounds = MirrorForRightToLeft(data._bounds);
			bool enabled = base.Enabled;
			if (enabled && data._isPressed)
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.SurfacePressed), bounds);
			}
			else if (enabled && data._controlStyleControl2DataBoolean)
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.SurfaceHot), bounds);
			}
			int half = theme.Scaled(3);
			int cx = bounds.X + bounds.Width / 2;
			int cy = bounds.Y + bounds.Height / 2;
			Point[] points;
			switch (direction)
			{
			case ArrowDirection.Up:
				points = new[] { new Point(cx - half, cy + half / 2), new Point(cx + half, cy + half / 2), new Point(cx, cy - half / 2 - 1) };
				break;
			case ArrowDirection.Down:
				points = new[] { new Point(cx - half, cy - half / 2), new Point(cx + half, cy - half / 2), new Point(cx, cy + half / 2 + 1) };
				break;
			case ArrowDirection.Left:
				points = new[] { new Point(cx + half / 2, cy - half), new Point(cx + half / 2, cy + half), new Point(cx - half / 2 - 1, cy) };
				break;
			default:
				points = new[] { new Point(cx - half / 2, cy - half), new Point(cx - half / 2, cy + half), new Point(cx + half / 2 + 1, cy) };
				break;
			}
			ThemeRole role = !enabled ? ThemeRole.TextDisabled : (data._isPressed || data._controlStyleControl2DataBoolean) ? ThemeRole.Text : ThemeRole.TextMuted;
			graphics.FillPolygon(theme.Brush(role), points);
		}
	}
}
