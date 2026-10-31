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
		public static void DrawTiledImage(Graphics graphics, Image image, Rectangle rect)
		{
			if (image != null)
			{
				using (Brush brush = new TextureBrush(image, WrapMode.Tile))
				{
					graphics.FillRectangle(brush, rect);
				}
			}
		}

		private static Bitmap ConvertIconToBitmap(Icon icon)
		{
			if (!NativeMethods.GetIconInfo(icon.Handle, out NativeMethods.NativeIconInfo iconInfo))
			{
				return icon.ToBitmap();
			}
			IntPtr maskBitmap = iconInfo._maskBitmap;
			IntPtr colorBitmapHandle = iconInfo._colorBitmap;
			try
			{
				// Monochrome icons have no color bitmap.
				if (colorBitmapHandle == IntPtr.Zero)
				{
					return icon.ToBitmap();
				}
				using (Bitmap colorBitmap = Image.FromHbitmap(colorBitmapHandle))
				{
					if (Image.GetPixelFormatSize(colorBitmap.PixelFormat) < 32)
					{
						return icon.ToBitmap();
					}
					Rectangle rect = new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height);
					BitmapData bitmapData = colorBitmap.LockBits(rect, ImageLockMode.ReadOnly, colorBitmap.PixelFormat);
					try
					{
						// FromHbitmap drops the alpha channel from the format, so the raw pixels are read as ARGB
						// and used only when the icon really has partial transparency.
						if (!HasPartialAlpha(bitmapData))
						{
							return icon.ToBitmap();
						}
						using (Bitmap pixels = new Bitmap(bitmapData.Width, bitmapData.Height, bitmapData.Stride, PixelFormat.Format32bppArgb, bitmapData.Scan0))
						{
							return new Bitmap(pixels);
						}
					}
					finally
					{
						colorBitmap.UnlockBits(bitmapData);
					}
				}
			}
			finally
			{
				if (colorBitmapHandle != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(colorBitmapHandle);
				}
				if (maskBitmap != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(maskBitmap);
				}
			}
		}

		private static bool HasPartialAlpha(BitmapData bitmapData)
		{
			for (int i = 0; i < bitmapData.Height; i++)
			{
				for (int j = 0; j < bitmapData.Width; j++)
				{
					int alpha = (Marshal.ReadInt32(bitmapData.Scan0, bitmapData.Stride * i + 4 * j) >> 24) & 0xFF;
					if (alpha > 0 && alpha < 255)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void DrawIcon(Graphics graphics, Icon original, Rectangle targetRect, bool isEnabled, Color background)
		{
			Icon drawnIcon = null;
			Icon resizedIcon = null;
			if (original.Size == targetRect.Size)
			{
				drawnIcon = original;
			}
			else
			{
				resizedIcon = new Icon(original, targetRect.Size);
				drawnIcon = ((!(resizedIcon.Size != targetRect.Size)) ? resizedIcon : original);
			}
			if (isEnabled)
			{
				graphics.DrawIcon(drawnIcon, targetRect);
			}
			else
			{
				Bitmap bitmap = ConvertIconToBitmap(drawnIcon);
				if (bitmap.Size != targetRect.Size)
				{
					Bitmap resized = new Bitmap(bitmap, targetRect.Size);
					bitmap.Dispose();
					bitmap = resized;
				}
				ControlPaint.DrawImageDisabled(graphics, bitmap, targetRect.X, targetRect.Y, background);
				bitmap.Dispose();
				bitmap = null;
			}
			if (resizedIcon != null && resizedIcon != original)
			{
				resizedIcon.Dispose();
				resizedIcon = null;
			}
		}

		public static void DrawFormatStyleBackground(Graphics graphics, FormatStyle uiFormatStyle, Rectangle rectangle, Rectangle backgroundBounds)
		{
			Rectangle rect = Rectangle.Intersect(rectangle, backgroundBounds);
			if (rect.Width == 0 || rect.Height == 0)
			{
				return;
			}
			if (uiFormatStyle.BackgroundImage != null)
			{
				switch (uiFormatStyle.BackgroundImageDrawMode)
				{
				case BackgroundImageDrawMode.Center:
					DrawCenteredImage(graphics, uiFormatStyle.BackgroundImage, backgroundBounds);
					break;
				case BackgroundImageDrawMode.Empty:
				case BackgroundImageDrawMode.Stretch:
					DrawStretchedImage(graphics, uiFormatStyle.BackgroundImage, backgroundBounds);
					break;
				case BackgroundImageDrawMode.Tile:
					DrawTiledBackgroundImage(graphics, uiFormatStyle.BackgroundImage, backgroundBounds);
					break;
				}
			}
			Brush brush = uiFormatStyle.GetBackgroundBrush(backgroundBounds);
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
		}

		public static void DrawCenteredImage(Graphics graphics, Image image, Rectangle rect)
		{
			Region clip = null;
			Region region = null;
			if (image.Width > rect.Width || image.Height > rect.Height)
			{
				clip = graphics.Clip;
				region = new Region(rect);
				graphics.Clip = region;
			}
			int x = rect.X + (rect.Width - image.Width) / 2;
			int y = rect.Y + (rect.Height - image.Height) / 2;
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
			if (region != null)
			{
				graphics.Clip = clip;
				clip.Dispose();
				region.Dispose();
			}
		}

		public static void DrawStretchedImage(Graphics graphics, Image image, Rectangle destRect)
		{
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		public static void DrawTiledBackgroundImage(Graphics graphics, Image image, Rectangle rect)
		{
			DrawTiledImage(graphics, image, rect);
		}

		public static Bitmap MakeMagentaTransparent(Bitmap bitmap)
		{
			try
			{
				bitmap?.MakeTransparent(Color.Magenta);
				return bitmap;
			}
			catch (Exception ex) when (ex is ArgumentException || ex is ExternalException || ex is InvalidOperationException)
			{
				// Some pixel formats cannot be made transparent; callers then skip the image.
				return null;
			}
		}

		public static void DrawRecoloredImage(Graphics graphics, Image image, Rectangle rectangle, Color color)
		{
			ImageAttributes imageAttributes = new ImageAttributes();
			ColorMap colorMap = new ColorMap();
			colorMap.NewColor = color;
			colorMap.OldColor = Color.Black;
			ColorMap[] remapTable = new ColorMap[1] { colorMap };
			imageAttributes.SetRemapTable(remapTable);
			Rectangle destRect = new Rectangle(rectangle.Left + (rectangle.Width - image.Width) / 2, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			imageAttributes.Dispose();
			imageAttributes = null;
		}
	}
}
