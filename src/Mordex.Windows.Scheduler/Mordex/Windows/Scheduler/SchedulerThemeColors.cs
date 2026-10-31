using System.Drawing;
using System.Drawing.Imaging;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerThemeColors
	{
		internal Color _headerColor;

		internal Color _lineColor;

		internal Color _borderColor;

		internal Color _normalStartColor;

		internal Color _normalEndColor;

		internal Color _allDayAreaColor;

		internal Color _glyphColor;

		internal Color _todayBorderColor;

		private Color _selectedColor;

		private Color _hotColor;

		private Color _hotSelectedColor;

		public SchedulerThemeColors()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault();
			_headerColor = theme.Color(ThemeRole.Header);
			_borderColor = theme.Color(ThemeRole.Border);
			_lineColor = theme.Color(ThemeRole.GridLine);
			_glyphColor = theme.Color(ThemeRole.HeaderText);
			_todayBorderColor = theme.Color(ThemeRole.Accent);
			bool styled = theme.Style.FaceGradient != ThemeGradient.None;
			_normalStartColor = styled ? theme.Style.LightEnd(_headerColor) : _headerColor;
			_normalEndColor = styled ? theme.Style.DarkEnd(_headerColor) : _normalStartColor;
			_allDayAreaColor = theme.Color(ThemeRole.SurfaceAlt);
			_selectedColor = theme.Color(ThemeRole.Selection);
			_hotColor = theme.Color(ThemeRole.SurfaceHot);
			_hotSelectedColor = theme.Color(ThemeRole.AccentHot);
		}

		public static Color GetDefaultBorderColor()
		{
			return MxThemeManager.ResolveDefault().Color(ThemeRole.Border);
		}

		public void DisposeResources()
		{
		}

		public Pen GetBorderPen()
		{
			return GdiCache.Pen(_borderColor);
		}

		public void DrawImage(Graphics graphics, Image image, Rectangle rectangle, Color color)
		{
			using (ImageAttributes imageAttributes = new ImageAttributes())
			{
				ColorMap colorMap = new ColorMap();
				colorMap.NewColor = color;
				colorMap.OldColor = Color.Black;
				imageAttributes.SetRemapTable(new ColorMap[1] { colorMap });
				Rectangle destRect = new Rectangle(rectangle.Left + (rectangle.Width - image.Width) / 2, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			}
		}

		// A flat header cell: fill by state, and the edge lines that the border flags ask for.
		// A normal fill given by the caller (a color the developer set) replaces the theme color of the normal state.
		internal Rectangle DrawBackground(Graphics graphics, Rectangle bounds, bool isHot, bool isSelected, BorderFlags borderFlags, bool isToday, Brush normalFill = null)
		{
			if (normalFill != null && !isHot && !isSelected)
			{
				graphics.FillRectangle(normalFill, bounds);
			}
			else
			{
				Color fill = isHot && isSelected ? _hotSelectedColor : isHot ? _hotColor : isSelected ? _selectedColor : _normalStartColor;
				graphics.FillRectangle(GdiCache.Brush(fill), bounds);
			}
			if (isToday)
			{
				graphics.FillRectangle(GdiCache.Brush(MxThemeManager.ResolveDefault().Color(ThemeRole.AccentSubtle)), bounds);
			}
			Pen edgePen = GdiCache.Pen(isToday ? _todayBorderColor : _lineColor);
			if ((borderFlags & BorderFlags.Left) != BorderFlags.None)
			{
				graphics.DrawLine(edgePen, bounds.Left, bounds.Top + 4, bounds.Left, bounds.Bottom - 4);
			}
			if ((borderFlags & BorderFlags.Top) != BorderFlags.None)
			{
				graphics.DrawLine(edgePen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
			}
			if ((borderFlags & BorderFlags.Bottom) != BorderFlags.None)
			{
				graphics.DrawLine(edgePen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			}
			if ((borderFlags & BorderFlags.Right) != BorderFlags.None)
			{
				graphics.DrawLine(edgePen, bounds.Right - 1, bounds.Top + 3, bounds.Right - 1, bounds.Bottom - 5);
			}
			return bounds;
		}
	}
}
