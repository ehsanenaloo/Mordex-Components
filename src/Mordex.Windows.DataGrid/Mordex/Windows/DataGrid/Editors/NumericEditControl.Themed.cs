using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal partial class NumericEditControl
	{
		private readonly Point[] _glyphPoints = new Point[3];

		private bool IsThemedLook => _cStyle != null && _cStyle.GetLayoutState().GetEditControlsStyle().IsThemedLook;

		// The record navigator in the themed look: a flat bar, buttons that show a face only when hot or pressed, and glyphs
		// drawn as shapes in the text color of the theme.
		private void PaintThemedContent(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			graphics.FillRectangle(theme.Brush(ThemeRole.Surface), base.ClientRectangle);
			Color textColor = theme.Color(ThemeRole.Text);
			Font font = _editorFont;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(GetUseCompatibleTextRendering());
			if (_captionText.Length > 0)
			{
				textLayoutRenderer.DrawTextWithColor(graphics, _captionText, _captionBounds, font, textColor);
			}
			if (_ofLabel.Length > 0)
			{
				textLayoutRenderer.DrawTextWithColor(graphics, _ofLabel, _ofTextBounds, font, textColor);
			}
			textLayoutRenderer.DrawTextWithColor(graphics, GetTotalCount().ToString("N0", Application.CurrentCulture), _totalCountBounds, font, textColor);
			for (int i = 0; i < 5; i++)
			{
				PaintThemedButton(graphics, theme, i);
			}
			if (_numericEdit != null)
			{
				Rectangle bounds = _numericEdit.Bounds;
				bounds.Inflate(1, 1);
				theme.DrawRounded(graphics, bounds, ThemeRole.Border, 0);
			}
			textLayoutRenderer.ReleaseStringFormat();
		}

		private void PaintThemedButton(Graphics graphics, ResolvedTheme theme, int index)
		{
			if (!_buttonStates[index]._isVisible)
			{
				return;
			}
			bool enabled = _buttonStates[index]._isEnabled;
			Rectangle bounds = _buttonStates[index]._bounds;
			if (enabled && (_buttonStates[index]._isPressed || _buttonStates[index]._isHot))
			{
				Rectangle face = Rectangle.Inflate(bounds, -1, -theme.Scaled(2));
				theme.FillRounded(graphics, face, _buttonStates[index]._isPressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, theme.Shape.CornerRadius);
			}
			Brush brush = theme.Brush(enabled ? ThemeRole.Text : ThemeRole.TextDisabled);
			int half = theme.Scaled(4);
			int bar = System.Math.Max(1, theme.Scaled(2));
			int cx = bounds.X + bounds.Width / 2;
			int cy = bounds.Y + bounds.Height / 2;
			if (index == 4)
			{
				graphics.FillRectangle(brush, cx - half, cy - bar / 2, half * 2 + 1, bar);
				graphics.FillRectangle(brush, cx - bar / 2, cy - half, bar, half * 2 + 1);
				return;
			}
			bool toLeft = index < 2;
			if (RightToLeft != RightToLeft.No)
			{
				toLeft = !toLeft;
			}
			bool withBar = index == 0 || index == 3;
			int shift = withBar ? (bar + 1) / 2 + 1 : 0;
			// The triangle moves away from the bar, which lies at the side it points to.
			int tipX = toLeft ? cx - half + shift : cx + half - shift;
			int baseX = toLeft ? cx + half - 1 + shift : cx - half + 1 - shift;
			_glyphPoints[0] = new Point(tipX, cy);
			_glyphPoints[1] = new Point(baseX, cy - half);
			_glyphPoints[2] = new Point(baseX, cy + half);
			graphics.FillPolygon(brush, _glyphPoints);
			if (withBar)
			{
				int barX = toLeft ? tipX - bar - 1 : tipX + 2;
				graphics.FillRectangle(brush, barX, cy - half, bar, half * 2 + 1);
			}
		}
	}
}
