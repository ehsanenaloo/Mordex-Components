#pragma warning disable CS0618 // Bridges the obsolete Office2007 API to the current one.
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI
{
	internal partial class RenderingHelper
	{
		public static Brush CreateAlphaBrush(Color baseColor, int alpha, AlphaMode alphaMode)
		{
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			Color color = baseColor;
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				color = Color.Empty;
				break;
			case AlphaMode.UseAlpha:
				if (alpha == -1)
				{
					alpha = MaxChannelValue;
				}
				color = Color.FromArgb(alpha, baseColor);
				break;
			}
			if (!color.IsEmpty)
			{
				return new SolidBrush(color);
			}
			return null;
		}

		public static Brush CreateGradientBrush(Color startColor, Color endColor, Rectangle rect, BackgroundGradientMode backgroundGradientMode, int alpha, AlphaMode alphaMode)
		{
			if (rect.Width > 0 && rect.Height > 0)
			{
				if (alphaMode == AlphaMode.Empty)
				{
					alphaMode = AlphaMode.Opaque;
				}
				switch (alphaMode)
				{
				case AlphaMode.Transparent:
					return null;
				case AlphaMode.UseAlpha:
					if (alpha == -1)
					{
						alpha = MaxChannelValue;
					}
					startColor = Color.FromArgb(alpha, startColor);
					endColor = Color.FromArgb(alpha, endColor);
					break;
				}
				switch (backgroundGradientMode)
				{
				case BackgroundGradientMode.Horizontal:
					return new LinearGradientBrush(rect, startColor, endColor, 0f);
				case BackgroundGradientMode.Vertical:
					return new LinearGradientBrush(rect, startColor, endColor, 90f);
				case BackgroundGradientMode.Diagonal:
					return new LinearGradientBrush(rect, startColor, endColor, 45f);
				case BackgroundGradientMode.DiagonalBackwards:
					return new LinearGradientBrush(rect, startColor, endColor, 225f);
				default:
					return new SolidBrush(startColor);
				}
			}
			return null;
		}

		public static Color GetSchemeColor(ColorName colorName)
		{
			switch (colorName)
			{
			case ColorName.Control:
				return SystemColors.Control;
			case ColorName.ControlText:
				return SystemColors.ControlText;
			case ColorName.ControlDark:
				return SystemColors.ControlDark;
			case ColorName.Highlight:
				return SystemColors.Highlight;
			case ColorName.HighlightText:
				return SystemColors.HighlightText;
			case ColorName.Info:
				return SystemColors.Info;
			case ColorName.InfoText:
				return SystemColors.InfoText;
			case ColorName.GrayText:
				return SystemColors.GrayText;
			case ColorName.DisabledColor:
				return SystemColors.Control;
			case ColorName.Menu:
				return SystemColors.Menu;
			case ColorName.Window:
				return SystemColors.Window;
			case ColorName.ActiveCaption:
				return SystemColors.ActiveCaption;
			case ColorName.WindowText:
				return SystemColors.WindowText;
			default:
				return Color.Empty;
			}
		}

		public static void DrawFormatStyleState(Graphics graphics, FormatStyleLayoutState uiFormatStyleLayoutState, Image image, FormatStyle uiFormatStyle, bool isHot, bool isPressed)
		{
			if (isPressed || isHot)
			{
				Brush brush = uiFormatStyle.GetBackgroundBrush(uiFormatStyleLayoutState._bounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, uiFormatStyleLayoutState._bounds);
				}
				Pen pen = GdiCache.Pen(uiFormatStyle.GetStateBorderColor(isHot, isPressed));
				DrawRectangleOutline(graphics, pen, uiFormatStyleLayoutState._bounds);
				pen = null;
			}
			if (image != null)
			{
				DrawRecoloredImage(graphics, image, uiFormatStyleLayoutState._bounds, uiFormatStyle.ForeColor);
			}
		}

		public static void DrawHorizontalArrow(Graphics graphics, int size, Rectangle rectangle, int direction, Pen pen, bool isFilled)
		{
			int halfHeight = size / 2 + 1;
			if (halfHeight % 2 == 0)
			{
				halfHeight--;
			}
			int arrowDepth = halfHeight / 2 + 1;
			halfHeight /= 2;
			int baseX;
			int x;
			if (direction > 0)
			{
				baseX = rectangle.Left + (rectangle.Width - arrowDepth) / 2 - 1;
				x = rectangle.Left + (rectangle.Width - arrowDepth) / 2 + arrowDepth - 2;
			}
			else
			{
				x = rectangle.Left + (rectangle.Width - arrowDepth) / 2 + 1;
				baseX = rectangle.Left + (rectangle.Width - arrowDepth) / 2 + arrowDepth;
			}
			int centerY = rectangle.Top + (rectangle.Height - halfHeight) / 2;
			graphics.DrawLine(pen, x, centerY, baseX, centerY + halfHeight);
			graphics.DrawLine(pen, x, centerY, baseX, centerY - halfHeight);
			graphics.DrawLine(pen, baseX, centerY + halfHeight - 1, baseX, centerY - halfHeight + 1);
			if (isFilled)
			{
				centerY -= halfHeight;
				halfHeight = halfHeight * 2 + 1;
				while (halfHeight >= 0)
				{
					graphics.DrawLine(pen, baseX, centerY, baseX, centerY + halfHeight - 1);
					baseX += direction;
					halfHeight -= 2;
					centerY++;
				}
			}
		}

		public static void DrawSideTriangle(Graphics graphics, int size, Rectangle rectangle, int direction, Pen pen)
		{
			int lineLength = size / 2;
			if (lineLength % 2 == 0)
			{
				lineLength--;
			}
			int triangleWidth = lineLength / 2 + 1;
			int columnX = ((direction <= 0) ? (rectangle.Left + (rectangle.Width - triangleWidth) / 2 + triangleWidth - 1) : (rectangle.Left + (rectangle.Width - triangleWidth) / 2));
			int columnY = rectangle.Top + (rectangle.Height - lineLength) / 2;
			while (lineLength >= 0)
			{
				if (lineLength == 1)
				{
					graphics.DrawLine(pen, columnX - direction, columnY, columnX, columnY);
				}
				else
				{
					graphics.DrawLine(pen, columnX, columnY, columnX, columnY + lineLength - 1);
				}
				columnX += direction;
				lineLength -= 2;
				columnY++;
			}
		}

		public static void DrawUpDownTriangle(Graphics graphics, int size, Rectangle rectangle, int direction, Pen pen)
		{
			int lineLength = size / 2;
			if (lineLength % 2 == 0)
			{
				lineLength--;
			}
			int triangleHeight = lineLength / 2 + 1;
			int rowY = ((direction <= 0) ? (rectangle.Top + (rectangle.Height - triangleHeight) / 2 + triangleHeight - 1) : (rectangle.Top + (rectangle.Height - triangleHeight) / 2));
			int rowX = rectangle.Left + (rectangle.Width - lineLength) / 2;
			while (lineLength >= 0)
			{
				if (lineLength == 1)
				{
					graphics.DrawLine(pen, rowX, rowY - direction, rowX, rowY);
				}
				else
				{
					graphics.DrawLine(pen, rowX, rowY, rowX + lineLength - 1, rowY);
				}
				rowX++;
				lineLength -= 2;
				rowY += direction;
			}
		}

		public static Color BlendColors(Color color, Color otherColor, float weight)
		{
			if (color == Color.Empty)
			{
				return otherColor;
			}
			if (otherColor == Color.Empty)
			{
				return color;
			}
			return Color.FromArgb(BlendChannel(color.R, otherColor.R, weight), BlendChannel(color.G, otherColor.G, weight), BlendChannel(color.B, otherColor.B, weight));
		}

		private static int BlendChannel(int first, int second, float firstWeight)
		{
			float mixed = first * firstWeight + second * (1f - firstWeight);
			return Math.Min((int)Math.Round(mixed, 0), MaxChannelValue);
		}
	}
}
