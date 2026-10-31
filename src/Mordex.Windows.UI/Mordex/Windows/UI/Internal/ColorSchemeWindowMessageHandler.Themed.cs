using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	internal partial class ColorSchemeWindowMessageHandler
	{
		// The strength of the classic shadow; the corner fade of a theme scales from it.
		private const int ClassicShadowAlpha = 113;

		// A themed look has a shadow only when its theme asks for one.
		internal static bool IsShadowAllowed()
		{
			return MxThemeManager.DefaultTheme.Shape.HasShadow;
		}

		// The color and the strength (alpha) of the shadow come from the Shadow role of the default theme.
		private static Color ThemedShadowColor()
		{
			return MxThemeManager.DefaultTheme.GetColor(ThemeRole.Shadow);
		}

		private static int ThemedCornerUnit(Color shadow)
		{
			return Math.Max(1, CornerSizeLarge * shadow.A / ClassicShadowAlpha);
		}

		private void DrawThemedRightShadow(Graphics graphics, Rectangle rectangle)
		{
			rectangle.Height -= ShadowSize;
			rectangle.Width -= ShadowSize;
			Color shadow = ThemedShadowColor();
			if (_drawRightShadowTopCorner)
			{
				int unit = ThemedCornerUnit(shadow);
				RenderingHelper.DrawHorizontalFadeLines(graphics, new Point(rectangle.Right, rectangle.Y + ShadowSize), unit, unit, Color.FromArgb(shadow.R, shadow.G, shadow.B));
			}
			int start = _drawRightShadowTopCorner ? ShadowStartOffset : 0;
			Rectangle strip = new Rectangle(rectangle.Right - 1, rectangle.Y + start, ShadowGradientWidth, rectangle.Height - start);
			GdiCache.FillGradient(graphics, strip, shadow, Color.FromArgb(0, shadow), LinearGradientMode.Horizontal);
		}

		private void DrawThemedBottomShadow(Graphics graphics, Rectangle rectangle)
		{
			rectangle.Height -= ShadowSize;
			rectangle.Width -= ShadowSize;
			Color shadow = ThemedShadowColor();
			Color rgb = Color.FromArgb(shadow.R, shadow.G, shadow.B);
			int unit = ThemedCornerUnit(shadow);
			if (_drawBothBottomCorners || !_bottomCornerOnRight)
			{
				RenderingHelper.DrawVerticalFadeLines(graphics, new Point(rectangle.X + ShadowSize, rectangle.Bottom), unit, unit, rgb);
			}
			if (_drawBothBottomCorners || _bottomCornerOnRight)
			{
				RenderingHelper.DrawHorizontalFadeLines(graphics, new Point(rectangle.Right, rectangle.Bottom), unit * CornerLengthLarge / CornerSizeLarge, -unit, rgb);
			}
			Rectangle strip = !_drawBothBottomCorners && _bottomCornerOnRight
				? new Rectangle(rectangle.X, rectangle.Bottom - 1, rectangle.Width, ShadowGradientWidth)
				: new Rectangle(rectangle.X + ShadowStartOffset, rectangle.Bottom - 1, rectangle.Width - ShadowStartOffset, ShadowGradientWidth);
			GdiCache.FillGradient(graphics, strip, shadow, Color.FromArgb(0, shadow), LinearGradientMode.Vertical);
		}
	}
}
