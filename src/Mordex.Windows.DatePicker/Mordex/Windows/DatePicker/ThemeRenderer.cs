using System.Drawing;
using System.Drawing.Imaging;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	// Colors of the default theme, read once. The owner drops the instance when the theme changes.
	internal class ThemeRenderer
	{
		internal Color _activeBorderColor;

		internal Color _borderColor;

		internal Color _selectedDayColor;

		internal Color _surfaceColor;

		internal Color _pressedColor;

		internal Color _hotColor;

		internal Color _titleBackColor;

		internal bool _titleGradient;

		internal Color _titleLightColor;

		internal Color _titleDarkColor;

		internal Color _arrowColor;

		internal Color _gridLineColor;

		internal Color _todayColor;

		public ThemeRenderer()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault();
			_borderColor = theme.Color(ThemeRole.Border);
			_activeBorderColor = theme.Color(ThemeRole.BorderHot);
			_surfaceColor = theme.Color(ThemeRole.SurfaceRaised);
			_hotColor = theme.Color(ThemeRole.SurfaceHot);
			_pressedColor = theme.Color(ThemeRole.SurfacePressed);
			_selectedDayColor = theme.Color(ThemeRole.Selection);
			_titleBackColor = theme.Color(ThemeRole.Header);
			_titleGradient = theme.Theme.Style.GradientAmount > 0f;
			_titleLightColor = theme.Theme.Style.LightEnd(_titleBackColor);
			_titleDarkColor = theme.Theme.Style.DarkEnd(_titleBackColor);
			_arrowColor = theme.Color(ThemeRole.Text);
			_gridLineColor = theme.Color(ThemeRole.GridLine);
			_todayColor = theme.Color(ThemeRole.Accent);
		}

		public void DrawImage(Graphics graphics, Image image, Rectangle rectangle, Color color, int direction, int verticalOffset)
		{
			ColorMap colorMap = new ColorMap();
			colorMap.NewColor = color;
			colorMap.OldColor = Color.Black;
			int horizontalOffset = direction > 0 ? 3 : -1;
			Rectangle destRect = new Rectangle(rectangle.Left + (rectangle.Width - image.Width) / 2 + horizontalOffset, rectangle.Top + (rectangle.Height - image.Height) / 2 + verticalOffset, image.Width, image.Height);
			using (ImageAttributes imageAttributes = new ImageAttributes())
			{
				imageAttributes.SetRemapTable(new ColorMap[1] { colorMap });
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			}
		}

		public void DrawButtonBackground(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled, bool drawBorder, bool inflateBorder)
		{
			if (!isEnabled)
			{
				return;
			}
			if (isHot || isPressed)
			{
				graphics.FillRectangle(GdiCache.Brush(isPressed ? _pressedColor : _hotColor), bounds);
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(_surfaceColor), bounds);
			}
			if (!drawBorder)
			{
				return;
			}
			if (inflateBorder)
			{
				bounds.Inflate(1, 1);
			}
			DatePickerHelper.DrawRectangleOutline(graphics, GdiCache.Pen((isHot || isPressed) ? _activeBorderColor : _borderColor), bounds);
		}
	}
}
