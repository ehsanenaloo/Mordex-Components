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

		public static Rectangle DrawTabBorder(Graphics graphics, TabStripAlignment tabStripAlignment, Color baseColor, Color color, Rectangle rectangle, Rectangle stripBounds, bool isSelected, bool booleanArgument, Brush brush)
		{
			Pen outerHighlightPen = null;
			Pen innerHighlightPen = null;
			Pen outerShadowPen = null;
			Pen innerShadowPen = null;
			bool ownsPens = false;
			if (baseColor.IsEmpty)
			{
				baseColor = color;
			}
			if (baseColor != SystemColors.Control)
			{
				ownsPens = true;
				outerHighlightPen = new Pen(ControlPaint.LightLight(baseColor));
				innerShadowPen = new Pen(ControlPaint.Dark(baseColor, 0f));
				innerHighlightPen = new Pen(ControlPaint.Light(baseColor));
				outerShadowPen = new Pen(ControlPaint.Dark(baseColor));
			}
			else
			{
				outerHighlightPen = SystemPens.ControlLightLight;
				innerHighlightPen = SystemPens.ControlLight;
				outerShadowPen = SystemPens.ControlDarkDark;
				innerShadowPen = SystemPens.ControlDark;
			}
			switch (tabStripAlignment)
			{
			case TabStripAlignment.Top:
			{
				int sideBottom = rectangle.Bottom - 1;
				if (isSelected)
				{
					sideBottom--;
					if (rectangle.X == stripBounds.Left)
					{
						sideBottom++;
					}
				}
				graphics.DrawLine(outerHighlightPen, rectangle.X + 2, rectangle.Y, rectangle.Right - 3, rectangle.Y);
				graphics.DrawLine(outerHighlightPen, rectangle.X, rectangle.Y + 2, rectangle.X, sideBottom);
				graphics.DrawLine(outerHighlightPen, rectangle.X, rectangle.Y + 2, rectangle.X + 2, rectangle.Y);
				graphics.DrawLine(innerHighlightPen, rectangle.X + 2, rectangle.Y + 1, rectangle.Right - 3, rectangle.Y + 1);
				graphics.DrawLine(innerHighlightPen, rectangle.X + 1, rectangle.Y + 2, rectangle.X + 1, sideBottom);
				graphics.DrawLine(innerHighlightPen, rectangle.X + 1, rectangle.Y + 2, rectangle.X + 2, rectangle.Y + 1);
				sideBottom = rectangle.Bottom - 1;
				if (isSelected)
				{
					sideBottom--;
					if (rectangle.Right == stripBounds.Right)
					{
						sideBottom++;
					}
				}
				graphics.DrawLine(innerShadowPen, rectangle.Right - 2, rectangle.Y + 2, rectangle.Right - 2, sideBottom);
				graphics.DrawLine(outerShadowPen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, sideBottom);
				graphics.DrawLine(outerShadowPen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 2, rectangle.Y + 1);
				rectangle.Inflate(-2, -2);
				rectangle.Height += 2;
				break;
			}
			case TabStripAlignment.Bottom:
			{
				int sideTop = rectangle.Y;
				if (isSelected)
				{
					sideTop++;
					if (rectangle.X == stripBounds.Left)
					{
						sideTop--;
					}
				}
				graphics.DrawLine(outerHighlightPen, rectangle.X, sideTop, rectangle.X, rectangle.Bottom - 3);
				graphics.DrawLine(innerHighlightPen, rectangle.X + 1, sideTop, rectangle.X + 1, rectangle.Bottom - 3);
				graphics.DrawLine(outerHighlightPen, rectangle.X + 1, rectangle.Bottom - 2, rectangle.X + 2, rectangle.Bottom - 2);
				graphics.DrawLine(innerShadowPen, rectangle.X + 2, rectangle.Bottom - 2, rectangle.Right - 3, rectangle.Bottom - 2);
				graphics.DrawLine(outerShadowPen, rectangle.X + 2, rectangle.Bottom - 1, rectangle.Right - 3, rectangle.Bottom - 1);
				sideTop = rectangle.Y;
				if (isSelected)
				{
					sideTop++;
					if (rectangle.Right == stripBounds.Right)
					{
						sideTop--;
					}
				}
				graphics.DrawLine(innerShadowPen, rectangle.Right - 2, sideTop, rectangle.Right - 2, rectangle.Bottom - 3);
				graphics.DrawLine(outerShadowPen, rectangle.Right - 1, sideTop, rectangle.Right - 1, rectangle.Bottom - 3);
				graphics.DrawLine(outerShadowPen, rectangle.Right - 2, rectangle.Bottom - 2, rectangle.Right - 1, rectangle.Bottom - 3);
				rectangle.Inflate(-2, -2);
				rectangle.Height += 2;
				rectangle.Y -= 2;
				break;
			}
			case TabStripAlignment.Left:
			{
				int edgeRight = rectangle.Right - 1;
				if (isSelected)
				{
					edgeRight--;
					if (rectangle.Y == stripBounds.Top)
					{
						edgeRight++;
					}
				}
				graphics.DrawLine(outerHighlightPen, rectangle.X + 2, rectangle.Y, edgeRight, rectangle.Y);
				graphics.DrawLine(innerHighlightPen, rectangle.X + 2, rectangle.Y + 1, edgeRight, rectangle.Y + 1);
				graphics.DrawLine(outerHighlightPen, rectangle.X + 2, rectangle.Y, rectangle.X + 1, rectangle.Y + 1);
				graphics.DrawLine(outerHighlightPen, rectangle.X, rectangle.Y + 2, rectangle.X, rectangle.Bottom - 3);
				graphics.DrawLine(innerHighlightPen, rectangle.X + 1, rectangle.Y + 2, rectangle.X + 1, rectangle.Bottom - 3);
				edgeRight = rectangle.Right - 1;
				if (isSelected)
				{
					edgeRight--;
					if (rectangle.Bottom == stripBounds.Bottom)
					{
						edgeRight++;
					}
				}
				graphics.DrawLine(outerShadowPen, rectangle.X + 2, rectangle.Bottom - 1, edgeRight, rectangle.Bottom - 1);
				graphics.DrawLine(innerShadowPen, rectangle.X + 2, rectangle.Bottom - 2, edgeRight, rectangle.Bottom - 2);
				graphics.DrawLine(outerShadowPen, rectangle.X + 2, rectangle.Bottom - 1, rectangle.X + 1, rectangle.Bottom - 2);
				rectangle.Inflate(-2, -2);
				rectangle.Width += 2;
				break;
			}
			case TabStripAlignment.Right:
			{
				int edgeLeft = rectangle.X;
				if (isSelected)
				{
					edgeLeft++;
					if (rectangle.X == stripBounds.Left)
					{
						edgeLeft--;
					}
				}
				graphics.DrawLine(outerHighlightPen, edgeLeft, rectangle.Y, rectangle.Right - 2, rectangle.Y);
				graphics.DrawLine(innerHighlightPen, edgeLeft, rectangle.Y + 1, rectangle.Right - 2, rectangle.Y + 1);
				graphics.DrawLine(outerHighlightPen, rectangle.Right - 2, rectangle.Y, rectangle.Right - 1, rectangle.Y + 1);
				edgeLeft = rectangle.X;
				if (isSelected)
				{
					edgeLeft++;
					if (rectangle.Bottom == stripBounds.Bottom)
					{
						edgeLeft--;
					}
				}
				graphics.DrawLine(outerShadowPen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom - 3);
				graphics.DrawLine(innerShadowPen, rectangle.Right - 2, rectangle.Y + 2, rectangle.Right - 2, rectangle.Bottom - 3);
				graphics.DrawLine(outerShadowPen, rectangle.Right - 1, rectangle.Bottom - 3, rectangle.Right - 2, rectangle.Bottom - 2);
				graphics.DrawLine(outerShadowPen, edgeLeft, rectangle.Bottom - 1, rectangle.Right - 3, rectangle.Bottom - 1);
				graphics.DrawLine(innerShadowPen, edgeLeft, rectangle.Bottom - 2, rectangle.Right - 3, rectangle.Bottom - 2);
				rectangle.Inflate(-2, -2);
				rectangle.Width += 2;
				rectangle.X -= 2;
				break;
			}
			}
			if (ownsPens)
			{
				outerHighlightPen.Dispose();
				outerHighlightPen = null;
				innerHighlightPen.Dispose();
				innerHighlightPen = null;
				outerShadowPen.Dispose();
				outerShadowPen = null;
				innerShadowPen.Dispose();
				innerShadowPen = null;
			}
			return rectangle;
		}

		public static void DrawStateImage(Graphics graphics, Icon icon, Image image, int index, ImageList imageList, Rectangle rect, bool isEnabled, bool isHot, bool isPressed, Color background, bool isThemed, ImageAttributes imageAttr)
		{
			if (isEnabled && (isHot || isPressed))
			{
				if (isPressed)
				{
					rect.Offset(1, 1);
				}
			}
			if (icon != null)
			{
				DrawIcon(graphics, icon, rect, isEnabled, background);
			}
			else
			{
				if (image == null)
				{
					return;
				}
				if (isEnabled)
				{
					if (imageAttr == null)
					{
						if (index >= 0 && imageList != null && index < imageList.Images.Count && imageList.ImageSize == rect.Size)
						{
							imageList.Draw(graphics, rect.X, rect.Y, rect.Width, rect.Height, index);
						}
						else
						{
							graphics.DrawImage(image, rect);
						}
					}
					else
					{
						graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
					}
				}
				else if (rect.Width > 0 && rect.Height > 0)
				{
					Bitmap bitmap = new Bitmap(image, rect.Width, rect.Height);
					ControlPaint.DrawImageDisabled(graphics, bitmap, rect.X, rect.Y, background);
					bitmap.Dispose();
					bitmap = null;
				}
			}
		}

		public static void DrawThemedTabBackground(Graphics targetGraphics, ref Bitmap image, Rectangle bounds, Rectangle srcRect, TabStripAlignment tabStripAlignment, VisualStyleElement visualStyleElement)
		{
			if (tabStripAlignment == TabStripAlignment.Top)
			{
				VisualStylePainter.DrawThemeElement(targetGraphics, bounds, visualStyleElement);
			}
			else
			{
				if (bounds.Width <= 0 || bounds.Height <= 0)
				{
					return;
				}
				if (image == null)
				{
					Graphics graphics = null;
					switch (tabStripAlignment)
					{
					case TabStripAlignment.Bottom:
						image = new Bitmap(bounds.Width, bounds.Height);
						graphics = Graphics.FromImage(image);
						VisualStylePainter.DrawThemeElement(graphics, bounds, visualStyleElement);
						image.RotateFlip(RotateFlipType.Rotate180FlipX);
						break;
					case TabStripAlignment.Right:
					{
						Rectangle rightRotatedBounds = new Rectangle(0, 0, bounds.Height, bounds.Width);
						image = new Bitmap(bounds.Height, bounds.Width);
						graphics = Graphics.FromImage(image);
						VisualStylePainter.DrawThemeElement(graphics, rightRotatedBounds, visualStyleElement);
						image.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;
					}
					default:
					{
						Rectangle leftRotatedBounds = new Rectangle(0, 0, bounds.Height, bounds.Width);
						image = new Bitmap(bounds.Height, bounds.Width);
						graphics = Graphics.FromImage(image);
						VisualStylePainter.DrawThemeElement(graphics, leftRotatedBounds, visualStyleElement);
						image.RotateFlip(RotateFlipType.Rotate90FlipX);
						break;
					}
					}
					graphics.Dispose();
					graphics = null;
				}
				targetGraphics.DrawImage(srcRect: new Rectangle(0, 0, srcRect.Width, srcRect.Height), image: image, destRect: srcRect, srcUnit: GraphicsUnit.Pixel);
			}
		}
	}
}
