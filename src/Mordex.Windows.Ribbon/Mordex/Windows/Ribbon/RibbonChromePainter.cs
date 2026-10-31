using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonChromePainter
	{
		internal static void DrawVerticalArrow(Graphics graphics, Rectangle rectangle, int direction, bool isEnabled, Color disabledColor, RibbonColorScheme colorScheme = null)
		{
			if (colorScheme != null)
			{
				ThemedRibbonPainter.Arrow(graphics, colorScheme.Theme, rectangle, rectangle.Width, direction, true, isEnabled);
				return;
			}
			int baseLength = rectangle.Width / 2;
			if (baseLength % 2 == 0)
			{
				baseLength--;
			}
			int depth = baseLength / 2 + 1;
			int lineY = ((direction <= 0) ? (rectangle.Top + (rectangle.Height - depth) / 2 + depth - 1) : (rectangle.Top + (rectangle.Height - depth) / 2));
			int lineX = rectangle.Left + (rectangle.Width - baseLength) / 2;
			Pen pen = null;
			if (isEnabled)
			{
				Rectangle rect = new Rectangle(lineX - baseLength / 2, lineY - depth / 2, baseLength * 2, depth * 2);
				Color color = Color.FromArgb(80, Color.Black);
				Color gradientEndColor = Color.FromArgb(245, Color.Black);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, gradientEndColor, LinearGradientMode.ForwardDiagonal);
				pen = new Pen(linearGradientBrush);
				linearGradientBrush.Dispose();
			}
			else
			{
				pen = new Pen(disabledColor);
			}
			while (baseLength >= 0)
			{
				if (baseLength == 1)
				{
					graphics.DrawLine(pen, lineX, lineY, (float)lineX + 0.5f, lineY);
				}
				else
				{
					graphics.DrawLine(pen, lineX, lineY, lineX + baseLength - 1, lineY);
				}
				lineX++;
				baseLength -= 2;
				lineY += direction;
			}
			pen.Dispose();
		}

		internal static void DrawHorizontalArrow(Graphics graphics, Rectangle rectangle, int direction, bool isEnabled, Color disabledColor, RibbonColorScheme colorScheme = null)
		{
			if (colorScheme != null)
			{
				ThemedRibbonPainter.Arrow(graphics, colorScheme.Theme, rectangle, rectangle.Width, direction, false, isEnabled);
				return;
			}
			int baseLength = rectangle.Width / 2;
			if (baseLength % 2 == 0)
			{
				baseLength--;
			}
			int depth = baseLength / 2 + 1;
			int lineX = ((direction <= 0) ? (rectangle.Left + (rectangle.Width - depth) / 2 + depth - 1) : (rectangle.Left + (rectangle.Width - depth) / 2));
			int lineY = rectangle.Top + (rectangle.Height - baseLength) / 2;
			Pen pen = null;
			if (isEnabled)
			{
				Rectangle rect = new Rectangle(lineX - depth / 2, lineY - baseLength / 2, depth * 2, baseLength * 2);
				Color color = Color.FromArgb(80, Color.Black);
				Color gradientEndColor = Color.FromArgb(245, Color.Black);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, gradientEndColor, LinearGradientMode.ForwardDiagonal);
				pen = new Pen(linearGradientBrush);
				linearGradientBrush.Dispose();
			}
			else
			{
				pen = new Pen(disabledColor);
			}
			while (baseLength >= 0)
			{
				if (baseLength == 1)
				{
					graphics.DrawLine(pen, lineX - direction, lineY, lineX, lineY);
				}
				else
				{
					graphics.DrawLine(pen, lineX, lineY, lineX, lineY + baseLength - 1);
				}
				lineX += direction;
				baseLength -= 2;
				lineY++;
			}
			pen.Dispose();
		}

		internal static void DrawOrbNormal(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Orb(graphics, colorScheme.Theme, bounds, ThemeRole.Accent, 255);
			return;
		}

		internal static void DrawOrbHot(Graphics graphics, Rectangle bounds, int alpha, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Orb(graphics, colorScheme.Theme, bounds, ThemeRole.AccentHot, alpha);
			return;
		}

		internal static void DrawOrbPressed(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Orb(graphics, colorScheme.Theme, bounds, ThemeRole.AccentPressed, 255);
			return;
		}

		internal static void DrawBackground(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme, bool isSmall)
		{
			DrawPressedButtonBackgroundFlat(graphics, bounds, colorScheme);
			return;
		}

		internal static void RenderContent(Graphics graphics, Rectangle rectangle, Pen pen, BorderFlags borderFlags)
		{
			int topLeftInset = (((borderFlags & BorderFlags.TopLeft) != BorderFlags.None) ? 1 : 0);
			int topRightInset = (((borderFlags & BorderFlags.TopRight) != BorderFlags.None) ? 1 : 0);
			int bottomLeftInset = (((borderFlags & BorderFlags.BottomLeft) != BorderFlags.None) ? 1 : 0);
			int bottomRightInset = (((borderFlags & BorderFlags.BottomRight) != BorderFlags.None) ? 1 : 0);
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(rectangle.X + topLeftInset, rectangle.Top, rectangle.Right - 1 - topRightInset, rectangle.Top);
			graphicsPath.AddLine(rectangle.Right - 1 - topRightInset, rectangle.Top, rectangle.Right - 1 - topRightInset, rectangle.Top + topRightInset);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Top + topRightInset, rectangle.Right - 1, rectangle.Bottom - 1 - bottomRightInset);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Bottom - 1 - bottomRightInset, rectangle.Right - 1 - bottomRightInset, rectangle.Bottom - 1 - bottomRightInset);
			graphicsPath.AddLine(rectangle.Right - 1 - bottomRightInset, rectangle.Bottom - 1, rectangle.X + bottomLeftInset, rectangle.Bottom - 1);
			graphicsPath.AddLine(rectangle.X + bottomLeftInset, rectangle.Bottom - 1, rectangle.X + bottomLeftInset, rectangle.Bottom - 1 - bottomLeftInset);
			graphicsPath.AddLine(rectangle.X, rectangle.Bottom - 1 - bottomLeftInset, rectangle.X, rectangle.Y + topLeftInset);
			graphicsPath.AddLine(rectangle.X, rectangle.Top + topLeftInset, rectangle.X + topLeftInset, rectangle.Top + topLeftInset);
			graphics.DrawPath(pen, graphicsPath);
			graphicsPath.Dispose();
		}
	}
}
