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
		public static int MeasureWrappedTextWidth(Graphics graphics, TextLayoutRenderer textLayoutRenderer, string text, FormatStyle uiFormatStyle, Font font, int minWidth, int tolerance)
		{
			string[] words = text.Split(null);
			int[] wordWidths = new int[words.Length];
			for (int i = 0; i < words.Length; i++)
			{
				wordWidths[i] = (int)textLayoutRenderer.MeasureText(graphics, words[i], font, 0).Width;
			}
			int headWidth = 0;
			int tailWidth = 0;
			for (int j = 0; j < words.Length; j++)
			{
				headWidth += wordWidths[j];
			}
			int tailWordCount = 0;
			int wordGap = 0;
			while (headWidth > tailWidth)
			{
				int nextTailWordWidth = wordWidths[wordWidths.Length - 1 - tailWordCount];
				if (tailWordCount != 0 && tailWidth + nextTailWordWidth >= headWidth)
				{
					break;
				}
				tailWidth += nextTailWordWidth;
				headWidth -= nextTailWordWidth;
				tailWordCount++;
				if (tailWordCount == words.Length - 1)
				{
					break;
				}
			}
			if (headWidth > tailWidth + tolerance)
			{
				if (headWidth < minWidth)
				{
					return minWidth;
				}
				return headWidth + wordGap * (words.Length - tailWordCount - 1);
			}
			if (tailWidth < minWidth)
			{
				return minWidth;
			}
			return tailWidth + wordGap * (words.Length - tailWordCount) + tolerance;
		}

		public static int LayoutImageHorizontally(Graphics graphics, Size size, ref Rectangle rectangle, ImageHorizontalAlignment imageHorizontalAlignment, string text, TextLayoutRenderer textLayoutRenderer, Font font)
		{
			return LayoutImageHorizontallyWithMaxWidth(graphics, size, ref rectangle, 0, imageHorizontalAlignment, text, textLayoutRenderer, font);
		}

		public static int LayoutImageHorizontallyWithMaxWidth(Graphics graphics, Size size, ref Rectangle rectangle, int maxTextWidth, ImageHorizontalAlignment imageHorizontalAlignment, string text, TextLayoutRenderer textLayoutRenderer, Font font)
		{
			int textWidth = 0;
			StringAlignment stringAlignment = textLayoutRenderer.GetStringAlignment();
			if (textLayoutRenderer.GetRightToLeft() == RightToLeft.Yes)
			{
				switch (stringAlignment)
				{
				case StringAlignment.Far:
					stringAlignment = StringAlignment.Near;
					break;
				case StringAlignment.Near:
					stringAlignment = StringAlignment.Far;
					break;
				}
				switch (imageHorizontalAlignment)
				{
				case ImageHorizontalAlignment.AfterText:
					imageHorizontalAlignment = ImageHorizontalAlignment.BeforeText;
					break;
				case ImageHorizontalAlignment.BeforeText:
					imageHorizontalAlignment = ImageHorizontalAlignment.AfterText;
					break;
				case ImageHorizontalAlignment.Far:
					imageHorizontalAlignment = ImageHorizontalAlignment.Near;
					break;
				case ImageHorizontalAlignment.Near:
					imageHorizontalAlignment = ImageHorizontalAlignment.Far;
					break;
				}
			}
			switch (imageHorizontalAlignment)
			{
			case ImageHorizontalAlignment.Near:
			{
				int nearImageX = rectangle.Left + 2;
				textWidth = (int)textLayoutRenderer.MeasureText(graphics, text, font, maxTextWidth).Width + 1;
				switch (stringAlignment)
				{
				case StringAlignment.Center:
					rectangle.X += size.Width + 4 + (rectangle.Width - size.Width - textWidth - 4) / 2;
					rectangle.Width = textWidth;
					break;
				case StringAlignment.Far:
					rectangle.X = rectangle.Right - textWidth;
					rectangle.Width = textWidth;
					break;
				default:
					rectangle.X += size.Width + 4;
					rectangle.Width = textWidth;
					break;
				}
				return nearImageX;
			}
			case ImageHorizontalAlignment.Center:
				return rectangle.Left + (rectangle.Width - size.Width) / 2;
			case ImageHorizontalAlignment.Far:
			{
				int farImageX = rectangle.Right - size.Width - 2;
				textWidth = (int)textLayoutRenderer.MeasureText(graphics, text, font, maxTextWidth).Width + 1;
				switch (stringAlignment)
				{
				case StringAlignment.Center:
					rectangle.X += (rectangle.Width - textWidth - size.Width - 4) / 2;
					rectangle.Width = textWidth;
					break;
				case StringAlignment.Far:
					rectangle.X = rectangle.Right - size.Width - textWidth - 4;
					rectangle.Width = textWidth;
					break;
				default:
					rectangle.Width = textWidth;
					break;
				}
				return farImageX;
			}
			case ImageHorizontalAlignment.AfterText:
				textWidth = (int)textLayoutRenderer.MeasureText(graphics, text, font, maxTextWidth).Width + 1;
				switch (stringAlignment)
				{
				case StringAlignment.Center:
					rectangle.X += (rectangle.Width - textWidth - size.Width) / 2;
					rectangle.Width = textWidth;
					return rectangle.Right;
				case StringAlignment.Far:
					rectangle.X = rectangle.Right - textWidth - size.Width;
					rectangle.Width = textWidth;
					return rectangle.Right;
				default:
					rectangle.Width = textWidth;
					return rectangle.Right;
				}
			default:
				textWidth = (int)textLayoutRenderer.MeasureText(graphics, text, font, maxTextWidth).Width + 1;
				switch (stringAlignment)
				{
				case StringAlignment.Center:
					rectangle.X += (rectangle.Width - size.Width - textWidth) / 2 + size.Width + 2;
					rectangle.Width = textWidth;
					return rectangle.X - size.Width - 2;
				case StringAlignment.Far:
					rectangle.X = rectangle.Right - textWidth;
					rectangle.Width = textWidth;
					return rectangle.X - size.Width - 2;
				default:
				{
					int x = rectangle.X;
					rectangle.X += size.Width + 2;
					rectangle.Width = textWidth;
					return x;
				}
				}
			}
		}

		public static int LayoutImageVertically(Graphics graphics, Size size, ref Rectangle rectangle, ImageVerticalAlignment imageVerticalAlignment, string text, Font font, TextLayoutRenderer textLayoutRenderer)
		{
			int textHeight = 0;
			int top = rectangle.Top;
			switch (imageVerticalAlignment)
			{
			case ImageVerticalAlignment.BottomOfText:
			{
				textHeight = (int)textLayoutRenderer.MeasureText(graphics, text, font, rectangle.Width).Height;
				int topOffset = (rectangle.Height - textHeight - size.Height - 2) / 2;
				if (topOffset > 0)
				{
					rectangle.Y += topOffset;
				}
				rectangle.Height = textHeight;
				return rectangle.Y + textHeight + 2;
			}
			case ImageVerticalAlignment.Near:
				return rectangle.Top;
			case ImageVerticalAlignment.Far:
				return rectangle.Bottom - size.Height;
			case ImageVerticalAlignment.TopOfText:
				textHeight = (int)textLayoutRenderer.MeasureText(graphics, text, font, rectangle.Width).Height;
				top += (rectangle.Height - textHeight - size.Height) / 2;
				if (top < rectangle.Top)
				{
					top = rectangle.Top;
				}
				rectangle.Y = top + size.Height + 1;
				rectangle.Height = textHeight;
				return top;
			default:
				return rectangle.Top + (rectangle.Height - size.Height) / 2;
			}
		}

		public static void DrawImageWithText(Graphics graphics, Brush brush, string text, Image image, int index, ImageList imageList, Rectangle rect, ImageHorizontalAlignment imageHorizontalAlignment, ImageVerticalAlignment imageVerticalAlignment, TextLayoutRenderer textLayoutRenderer, Font font, bool isEnabled, Color background)
		{
			if (rect.IsEmpty)
			{
				return;
			}
			if (image != null)
			{
				Region region = new Region(rect);
				Region clip = graphics.Clip;
				graphics.Clip = region;
				Point location = rect.Location;
				location.X = LayoutImageHorizontally(graphics, image.Size, ref rect, imageHorizontalAlignment, text, textLayoutRenderer, font);
				location.Y = LayoutImageVertically(graphics, image.Size, ref rect, imageVerticalAlignment, text, font, textLayoutRenderer);
				if (textLayoutRenderer.GetRightToLeft() != RightToLeft.Yes)
				{
					rect.Width += 10;
				}
				if (isEnabled)
				{
					if (index >= 0 && imageList != null && index < imageList.Images.Count)
					{
						imageList.Draw(graphics, location.X, location.Y, image.Width, image.Height, index);
					}
					else
					{
						graphics.DrawImage(image, location.X, location.Y, image.Width, image.Height);
					}
				}
				else
				{
					Bitmap bitmap = new Bitmap(image, image.Width, image.Height);
					ControlPaint.DrawImageDisabled(graphics, bitmap, location.X, location.Y, background);
					bitmap.Dispose();
					bitmap = null;
				}
				if (text.Length > 0 && brush != null)
				{
					textLayoutRenderer.DrawTextWithBrush(graphics, text, rect, font, brush);
				}
				graphics.Clip = clip;
				clip.Dispose();
				region.Dispose();
				region = null;
			}
			else if (brush != null && text.Length > 0)
			{
				textLayoutRenderer.DrawTextWithBrush(graphics, text, rect, font, brush);
			}
		}
	}
}
