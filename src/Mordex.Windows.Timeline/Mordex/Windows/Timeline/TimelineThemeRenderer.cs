using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline
{
	internal class TimelineThemeRenderer
	{
		internal Color _buttonBorderColor;

		internal Color _controlBorderColor;

		internal Color _headerGradientStartColor;

		internal Color _tierGradientStartColor;

		internal Color _tierGradientEndColor;

		internal Color _calendarHighlightColor;

		internal Color _currentDateFillColor;

		internal Color _lineColor;

		internal Color _textColor;

		private Color _normalFillColor;

		private Color _hotFillColor;

		private Color _pressedFillColor;

		private Color _hotBorderColor;

		public TimelineThemeRenderer()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault();
			_textColor = theme.Color(ThemeRole.Text);
			_currentDateFillColor = theme.Color(ThemeRole.AccentSubtle);
			_controlBorderColor = theme.Color(ThemeRole.Border);
			_buttonBorderColor = _controlBorderColor;
			_normalFillColor = theme.Color(ThemeRole.SurfaceRaised);
			_calendarHighlightColor = theme.Color(ThemeRole.Header);
			_tierGradientStartColor = theme.Color(ThemeRole.Header);
			_tierGradientEndColor = _tierGradientStartColor;
			_headerGradientStartColor = _tierGradientStartColor;
			_lineColor = theme.Color(ThemeRole.Border);
			_hotBorderColor = theme.Color(ThemeRole.BorderHot);
			_hotFillColor = theme.Color(ThemeRole.SurfaceHot);
			_pressedFillColor = theme.Color(ThemeRole.SurfacePressed);
		}

		public Brush GetTierGradientBrush()
		{
			return GdiCache.Brush(_tierGradientStartColor);
		}

		public Pen GetBorderPen()
		{
			return GdiCache.Pen(_lineColor);
		}

		public Pen CreateBorderPen()
		{
			return new Pen(_lineColor);
		}

		public void DrawButtonBackground(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			if (!isEnabled)
			{
				return;
			}
			Color fill = isPressed ? _pressedFillColor : isHot ? _hotFillColor : _normalFillColor;
			graphics.FillRectangle(GdiCache.Brush(fill), bounds);
			TimelineUtilities.DrawFlatBorder(graphics, GdiCache.Pen(isHot || isPressed ? _hotBorderColor : _buttonBorderColor), bounds);
		}
	}
}
