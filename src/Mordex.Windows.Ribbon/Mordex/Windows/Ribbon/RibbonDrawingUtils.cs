#pragma warning disable CS0618 // Bridges the obsolete Office2007 API to the current one.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonDrawingUtils
	{
		public static Size GetFrameBorderSize()
		{
			try
			{
				int frameWidth = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXFRAME);
				int paddedBorderWidth = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXPADDEDBORDER);
				return new Size(frameWidth + paddedBorderWidth, frameWidth + paddedBorderWidth);
			}
			catch (Exception ex) when (ex is EntryPointNotFoundException || ex is DllNotFoundException)
			{
				return SystemInformation.FrameBorderSize;
			}
		}

		internal static bool IsClearTypeFont(Font font)
		{
			if (font == null)
			{
				return false;
			}
			string name;
			if ((name = font.FontFamily.Name) != null)
			{
				if (RibbonStringSwitchCache.s_fontFamilyCases == null)
				{
					RibbonStringSwitchCache.s_fontFamilyCases = new Dictionary<string, int>(7)
					{
						{
							"Segoe UI",
							0
						},
						{
							"Calibri",
							1
						},
						{
							"Cambria",
							2
						},
						{
							"Candara",
							3
						},
						{
							"Consolas",
							4
						},
						{
							"Constantia",
							5
						},
						{
							"Corbel",
							6
						}
					};
				}
				if (RibbonStringSwitchCache.s_fontFamilyCases.TryGetValue(name, out var value))
				{
					switch (value)
					{
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
						return true;
					}
				}
			}
			return false;
		}

		internal static int GetLightness(Color color)
		{
			int maxComponent = Math.Max(Math.Max(color.R, color.G), color.B);
			int minComponent = Math.Min(Math.Min(color.R, color.G), color.B);
			int componentSum = maxComponent + minComponent;
			return (componentSum * 240 + 255) / 510;
		}

		internal static void DrawBackground(Graphics graphics, string text, Rectangle rectangle, Font font, Color color, Color disabledColor, bool isEnabled, RibbonForegroundStyle ribbonForegroundStyle)
		{
			color = ribbonForegroundStyle.ResolveForeColor(color, disabledColor, isEnabled);
			using GraphicsPath graphicsPath = new GraphicsPath();
			SolidBrush solidBrush = GdiCache.Brush(color);
			NativeMethods.LogFont a = new NativeMethods.LogFont();
			font.ToLogFont(a, graphics);
			float emSize = (float)Math.Abs(a._height) + 0f;
			int y = rectangle.Y + (rectangle.Height - font.Height) / 2 - 1;
			graphicsPath.AddString(text, font.FontFamily, (int)font.Style, emSize, new Point(rectangle.X, y), StringFormat.GenericDefault);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.FillPath(solidBrush, graphicsPath);
			graphics.SmoothingMode = smoothingMode;
		}

		internal static Color LightenColor(Color baseColor, float floatValue)
		{
			Color color = baseColor;
			Color lightColor = ControlPaint.Light(baseColor);
			int redDifference = color.R - lightColor.R;
			int greenDifference = color.G - lightColor.G;
			int blueDifference = color.B - lightColor.B;
			return Color.FromArgb(ShiftComponent(color.R, redDifference, floatValue), ShiftComponent(color.G, greenDifference, floatValue), ShiftComponent(color.B, blueDifference, floatValue));
		}

		internal static Color DarkenColor(Color baseColor, float floatValue)
		{
			Color color = baseColor;
			Color darkColor = ControlPaint.Dark(baseColor);
			int redDifference = color.R - darkColor.R;
			int greenDifference = color.G - darkColor.G;
			int blueDifference = color.B - darkColor.B;
			return Color.FromArgb(ShiftComponent(color.R, redDifference, floatValue), ShiftComponent(color.G, greenDifference, floatValue), ShiftComponent(color.B, blueDifference, floatValue));
		}

		private static int ShiftComponent(byte component, int difference, float floatValue)
		{
			return Math.Clamp(component - (int)(difference * floatValue), 0, 255);
		}

		public static Bitmap MakeMagentaTransparent(Bitmap bitmap)
		{
			try
			{
				bitmap?.MakeTransparent(Color.Magenta);
				return bitmap;
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is ExternalException)
			{
				// Indexed or unsupported pixel formats cannot be made transparent.
				return null;
			}
		}

		public static void DrawImage(Graphics graphics, Image image, Rectangle rectangle, Color color, Color secondOldColor, Color firstNewColor, Color secondNewColor)
		{
			ImageAttributes imageAttributes = new ImageAttributes();
			ColorMap colorMap = new ColorMap();
			colorMap.NewColor = firstNewColor;
			colorMap.OldColor = color;
			ColorMap secondColorMap = new ColorMap();
			secondColorMap.NewColor = secondNewColor;
			secondColorMap.OldColor = secondOldColor;
			ColorMap[] remapTable = new ColorMap[2] { colorMap, secondColorMap };
			imageAttributes.SetRemapTable(remapTable);
			Rectangle destRect = new Rectangle(rectangle.Left + (rectangle.Width - image.Width) / 2, rectangle.Top + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			imageAttributes.Dispose();
			imageAttributes = null;
		}

		public static void DrawBlackRemappedImage(Graphics graphics, Image image, Rectangle rectangle, Color color)
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

		internal static Form FindParentForm(Control control)
		{
			return control.FindForm();
		}

		internal static Control GetParentControl(Control control)
		{
			return control.Parent;
		}

		internal static Graphics CreateGraphicsFromHdc(IntPtr hdc)
		{
			return Graphics.FromHdc(hdc);
		}

		internal static void SetCapture(Control control, bool capture)
		{
			control.Capture = capture;
		}

		internal static bool TryFocusControl(Control control)
		{
			return control.Focus();
		}

		public static string GetShortcutText(Shortcut shortcut)
		{
			char c = (char)(shortcut & (Shortcut)65535);
			if (c >= '0' && c <= '9')
			{
				string text = "";
				int modifierBits = (int)((long)shortcut & 0xFFFF0000L);
				if ((modifierBits & 0x20000) != 0)
				{
					text += "Ctrl+";
				}
				if ((modifierBits & 0x10000) != 0)
				{
					text += "Shift+";
				}
				if ((modifierBits & 0x40000) != 0)
				{
					text += "Alt+";
				}
				return text + c;
			}
			return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys)shortcut);
		}

		public static int MeasureTwoLineTextWidth(Graphics graphics, TextLayoutRenderer textLayoutRenderer, string text, RibbonForegroundStyle ribbonForegroundStyle, Font font, int minWidth, int extraWidth)
		{
			string[] words = text.Split(null);
			int[] wordWidths = new int[words.Length];
			for (int i = 0; i < words.Length; i++)
			{
				wordWidths[i] = (int)textLayoutRenderer.MeasureStyledText(graphics, words[i], ribbonForegroundStyle, font, 0, 0).Width;
			}
			int firstLineWidth = 0;
			int secondLineWidth = 0;
			for (int j = 0; j < words.Length; j++)
			{
				firstLineWidth += wordWidths[j];
			}
			int wordsOnSecondLine = 0;
			int spaceWidth = 0;
			while (firstLineWidth > secondLineWidth)
			{
				int wordWidth = wordWidths[wordWidths.Length - 1 - wordsOnSecondLine];
				if (wordsOnSecondLine != 0 && secondLineWidth + wordWidth >= firstLineWidth)
				{
					break;
				}
				secondLineWidth += wordWidth;
				firstLineWidth -= wordWidth;
				wordsOnSecondLine++;
				if (wordsOnSecondLine == words.Length - 1)
				{
					break;
				}
			}
			if (firstLineWidth > secondLineWidth + extraWidth)
			{
				if (firstLineWidth < minWidth)
				{
					return minWidth;
				}
				return firstLineWidth + spaceWidth * (words.Length - wordsOnSecondLine - 1);
			}
			if (secondLineWidth < minWidth)
			{
				return minWidth;
			}
			return secondLineWidth + spaceWidth * (words.Length - wordsOnSecondLine) + extraWidth;
		}

		public static Rectangle DrawRectangleOutline(Graphics graphics, Pen pen, Rectangle rectangle)
		{
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			rectangle.Inflate(-1, -1);
			return rectangle;
		}

		public static bool HasImageSource(Icon icon, Image image, ImageList imageList, int index, string key)
		{
			if (icon != null || image != null)
			{
				return true;
			}
			if (imageList != null)
			{
				if (index >= 0 && index < imageList.Images.Count)
				{
					return true;
				}
				if (key != null && imageList.Images.ContainsKey(key))
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasButtonImage(ButtonCommand buttonCommand, CommandSizeStyle commandSizeStyle, ImageList imageList, ImageList largeImageList)
		{
			if (buttonCommand.GetEffectiveCommandStyle() == CommandStyle.Text)
			{
				return false;
			}
			if (commandSizeStyle == CommandSizeStyle.Large)
			{
				if (buttonCommand.LargeIcon != null || buttonCommand.LargeImage != null)
				{
					return true;
				}
				if (largeImageList != null)
				{
					if (buttonCommand.LargeImageKey != null && largeImageList.Images.ContainsKey(buttonCommand.LargeImageKey))
					{
						return true;
					}
					if (buttonCommand.LargeImageIndex >= 0 && buttonCommand.LargeImageIndex < largeImageList.Images.Count)
					{
						return true;
					}
				}
			}
			if (buttonCommand.Icon != null || buttonCommand.Image != null)
			{
				return true;
			}
			if (imageList != null)
			{
				if (buttonCommand.ImageKey != null && imageList.Images.ContainsKey(buttonCommand.ImageKey))
				{
					return true;
				}
				if (buttonCommand.ImageIndex >= 0 && buttonCommand.ImageIndex < imageList.Images.Count)
				{
					return true;
				}
			}
			return false;
		}

		public static void DrawIcon(Graphics graphics, Icon original, Rectangle rect, bool isEnabled, Color background, bool useBitmap)
		{
			if (original == null)
			{
				return;
			}
			Icon scaled = null;
			try
			{
				Icon icon = original;
				if (original.Size != rect.Size)
				{
					scaled = new Icon(original, rect.Size);
					if (scaled.Size == rect.Size)
					{
						icon = scaled;
					}
				}
				if (isEnabled)
				{
					if (useBitmap)
					{
						using Image image = icon.ToBitmap();
						graphics.DrawImage(image, rect);
					}
					else
					{
						graphics.DrawIcon(icon, rect);
					}
				}
				else
				{
					Bitmap bitmap = CreateIconBitmap(icon);
					if (bitmap.Size != rect.Size)
					{
						Bitmap resized = new Bitmap(bitmap, rect.Size);
						bitmap.Dispose();
						bitmap = resized;
					}
					using (bitmap)
					{
						ControlPaint.DrawImageDisabled(graphics, bitmap, rect.X, rect.Y, background);
					}
				}
			}
			finally
			{
				scaled?.Dispose();
			}
		}

		public static void DrawTintedImage(Graphics targetGraphics, Image sourceImage, Rectangle rect, RibbonColorScheme colorScheme)
		{
			Size size = sourceImage.Size;
			float[][] matrixRows = new float[5][];
			Color color = Color.FromArgb(100, 110, 127);
			color = BlendColors(colorScheme._contentGradientBottomStart, Color.Black, 0.5f);
			matrixRows[0] = new float[5] { 0.2125f, 0.2125f, 0.2125f, 0f, 0f };
			matrixRows[1] = new float[5] { 0.2577f, 0.2577f, 0.2577f, 0f, 0f };
			matrixRows[2] = new float[5] { 0.0361f, 0.0361f, 0.0361f, 0f, 0f };
			matrixRows[3] = new float[5] { 0f, 0f, 0f, 1f, 0f };
			float tintRed = (float)(int)color.R / 255f;
			float tintGreen = (float)(int)color.G / 255f;
			float tintBlue = (float)(int)color.B / 255f;
			matrixRows[4] = new float[5] { tintRed, tintGreen, tintBlue, 0f, 1f };
			ColorMatrix colorMatrix = new ColorMatrix(matrixRows);
			using ImageAttributes imageAttributes = new ImageAttributes();
			imageAttributes.ClearColorKey();
			imageAttributes.SetColorMatrix(colorMatrix);
			using (Bitmap image = new Bitmap(sourceImage.Width, sourceImage.Height))
			{
				using (Graphics graphics = Graphics.FromImage(image))
				{
					graphics.DrawImage(sourceImage, new Rectangle(0, 0, size.Width, size.Height), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, imageAttributes);
				}
				targetGraphics.DrawImage(image, rect);
			}
		}

		public static void DrawImageFromSources(Graphics graphics, Icon icon, Image image, int index, string key, ImageList imageList, Rectangle rect, bool isEnabled, bool useBitmap, RibbonColorScheme colorScheme)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			if (icon != null)
			{
				DrawIcon(graphics, icon, rect, isEnabled, SystemColors.Window, useBitmap);
			}
			else if (image != null)
			{
				DrawEnabledOrDisabledImage(graphics, image, rect, isEnabled, colorScheme);
			}
			else if (imageList != null)
			{
				if (key != null && imageList.Images.ContainsKey(key))
				{
					DrawImageListImage(graphics, imageList, imageList.Images.IndexOfKey(key), rect, isEnabled, true, colorScheme);
				}
				else if (index >= 0 && index < imageList.Images.Count)
				{
					DrawImageListImage(graphics, imageList, index, rect, isEnabled, true, colorScheme);
				}
			}
		}

		public static void DrawButtonImage(Graphics graphics, ButtonCommand buttonCommand, ImageList imageList, ImageList smallImageList, Rectangle rect, CommandSizeStyle commandSizeStyle, bool useBitmap, RibbonColorScheme colorScheme)
		{
			if (rect.Width <= 0 || rect.Height <= 0)
			{
				return;
			}
			bool enabled = buttonCommand.IsEnabled;
			if (commandSizeStyle == CommandSizeStyle.Large)
			{
				if (buttonCommand.LargeIcon != null)
				{
					DrawIcon(graphics, buttonCommand.LargeIcon, rect, enabled, SystemColors.Window, useBitmap);
					return;
				}
				if (buttonCommand.LargeImage != null)
				{
					DrawEnabledOrDisabledImage(graphics, buttonCommand.LargeImage, rect, enabled, colorScheme);
					return;
				}
				if (imageList != null)
				{
					if (buttonCommand.LargeImageKey != null && imageList.Images.ContainsKey(buttonCommand.LargeImageKey))
					{
						DrawImageListImage(graphics, imageList, imageList.Images.IndexOfKey(buttonCommand.LargeImageKey), rect, enabled, false, colorScheme);
						return;
					}
					if (buttonCommand.LargeImageIndex >= 0 && buttonCommand.LargeImageIndex < imageList.Images.Count)
					{
						DrawImageListImage(graphics, imageList, buttonCommand.LargeImageIndex, rect, enabled, false, colorScheme);
						return;
					}
				}
			}
			if (buttonCommand.Icon != null)
			{
				DrawIcon(graphics, buttonCommand.Icon, rect, enabled, SystemColors.Window, useBitmap);
			}
			else if (buttonCommand.Image != null)
			{
				DrawEnabledOrDisabledImage(graphics, buttonCommand.Image, rect, enabled, colorScheme);
			}
			else if (smallImageList != null)
			{
				if (buttonCommand.ImageKey != null && smallImageList.Images.ContainsKey(buttonCommand.ImageKey))
				{
					DrawImageListImage(graphics, smallImageList, smallImageList.Images.IndexOfKey(buttonCommand.ImageKey), rect, enabled, false, colorScheme);
				}
				else if (buttonCommand.ImageIndex >= 0 && buttonCommand.ImageIndex < smallImageList.Images.Count)
				{
					DrawImageListImage(graphics, smallImageList, buttonCommand.ImageIndex, rect, enabled, false, colorScheme);
				}
			}
		}

		private static void DrawEnabledOrDisabledImage(Graphics graphics, Image image, Rectangle rect, bool enabled, RibbonColorScheme colorScheme)
		{
			if (enabled)
			{
				graphics.DrawImage(image, rect);
			}
			else
			{
				DrawTintedImage(graphics, image, rect, colorScheme);
			}
		}

		// ImageList.Images[index] returns a new copy on every call, so the copy is disposed here.
		private static void DrawImageListImage(Graphics graphics, ImageList imageList, int index, Rectangle rect, bool enabled, bool checkVerticalClip, RibbonColorScheme colorScheme)
		{
			if (enabled)
			{
				RectangleF clipBounds = graphics.ClipBounds;
				bool insideClip = clipBounds.X <= (float)rect.X && clipBounds.Right >= (float)rect.Right;
				if (checkVerticalClip)
				{
					insideClip = insideClip && clipBounds.Y <= (float)rect.Y && clipBounds.Bottom >= (float)rect.Bottom;
				}
				if (insideClip && imageList.ImageSize.Width == rect.Width && imageList.ImageSize.Height == rect.Height)
				{
					imageList.Draw(graphics, rect.X, rect.Y, rect.Width, rect.Height, index);
					return;
				}
			}
			using Image image = imageList.Images[index];
			DrawEnabledOrDisabledImage(graphics, image, rect, enabled, colorScheme);
		}

		public static bool IsHighColorDisplay()
		{
			IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
			if (screenDc == IntPtr.Zero)
			{
				return false;
			}
			try
			{
				return NativeMethods.ToInt32Unchecked(NativeMethods.GetDeviceCaps(screenDc, NativeMethods.BITSPIXEL)) >= 16;
			}
			finally
			{
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
			}
		}

		// Returns a bitmap that keeps the per-pixel alpha of the icon when it has any.
		private static Bitmap CreateIconBitmap(Icon icon)
		{
			if (!NativeMethods.GetIconInfo(icon.Handle, out NativeMethods.IconInfo iconInfo))
			{
				return icon.ToBitmap();
			}
			try
			{
				if (iconInfo._colorBitmap == IntPtr.Zero)
				{
					return icon.ToBitmap();
				}
				using (Bitmap colorBitmap = Image.FromHbitmap(iconInfo._colorBitmap))
				{
					if (Image.GetPixelFormatSize(colorBitmap.PixelFormat) < 32)
					{
						return icon.ToBitmap();
					}
					Rectangle bounds = new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height);
					BitmapData data = colorBitmap.LockBits(bounds, ImageLockMode.ReadOnly, colorBitmap.PixelFormat);
					try
					{
						if (!HasPartialAlpha(data))
						{
							return icon.ToBitmap();
						}
						using (Bitmap wrapper = new Bitmap(data.Width, data.Height, data.Stride, PixelFormat.Format32bppArgb, data.Scan0))
						{
							return new Bitmap(wrapper);
						}
					}
					finally
					{
						colorBitmap.UnlockBits(data);
					}
				}
			}
			finally
			{
				if (iconInfo._colorBitmap != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(iconInfo._colorBitmap);
				}
				if (iconInfo._maskBitmap != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(iconInfo._maskBitmap);
				}
			}
		}

		private static unsafe bool HasPartialAlpha(BitmapData data)
		{
			byte* bitmapStart = (byte*)data.Scan0;
			for (int y = 0; y < data.Height; y++)
			{
				byte* alpha = bitmapStart + (long)y * data.Stride + 3;
				for (int x = 0; x < data.Width; x++, alpha += 4)
				{
					if (*alpha > 0 && *alpha < byte.MaxValue)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Draws text with a glow on a glass caption, through a 32 bit DIB so the alpha channel is kept.
		public static void DrawText(Graphics graphics, string text, Font font, Rectangle layoutRectangle, int glowSize, bool isGlassEnabled)
		{
			NativeMethods.NativeRect destination = default(NativeMethods.NativeRect);
			NativeMethods.NativeRect textBounds = default(NativeMethods.NativeRect);
			destination._left = layoutRectangle.Left - glowSize;
			destination._right = layoutRectangle.Right + 2 * glowSize;
			destination._top = layoutRectangle.Top;
			destination._bottom = layoutRectangle.Bottom;
			textBounds._right = destination._right - destination._left;
			textBounds._bottom = destination._bottom - destination._top;
			NativeMethods.BitmapInfo bitmapInfo = new NativeMethods.BitmapInfo
			{
				_header =
				{
					_height = -(destination._bottom - destination._top),
					_width = destination._right - destination._left,
					_planes = 1,
					_size = Marshal.SizeOf(typeof(NativeMethods.BitmapInfoHeader)),
					_bitCount = 32,
					_compression = 0
				}
			};
			IntPtr hdc = graphics.GetHdc();
			try
			{
				IntPtr memoryDc = NativeMethods.CreateCompatibleDC(hdc);
				if (memoryDc == IntPtr.Zero)
				{
					return;
				}
				try
				{
					DrawThemedText(memoryDc, hdc, text, font, glowSize, ref bitmapInfo, ref destination, ref textBounds);
				}
				finally
				{
					NativeMethods.DeleteDC(memoryDc);
				}
			}
			finally
			{
				graphics.ReleaseHdc(hdc);
			}
		}

		private static void DrawThemedText(IntPtr memoryDc, IntPtr targetDc, string text, Font font, int glowSize, ref NativeMethods.BitmapInfo bitmapInfo, ref NativeMethods.NativeRect destination, ref NativeMethods.NativeRect textBounds)
		{
			if (NativeMethods.SaveDC(memoryDc) == 0)
			{
				return;
			}
			IntPtr dib = NativeMethods.CreateDIBSection(memoryDc, ref bitmapInfo, 0u, 0, IntPtr.Zero, 0u);
			if (dib == IntPtr.Zero)
			{
				return;
			}
			IntPtr previousBitmap = NativeMethods.SelectObject(memoryDc, dib);
			IntPtr fontHandle = font.ToHfont();
			IntPtr previousFont = NativeMethods.SelectObject(memoryDc, fontHandle);
			IntPtr textBuffer = IntPtr.Zero;
			try
			{
				VisualStyleRenderer captionStyle = new VisualStyleRenderer(VisualStyleElement.Window.Caption.Active);
				NativeMethods.ThemeTextOptions options = new NativeMethods.ThemeTextOptions
				{
					_size = (uint)Marshal.SizeOf(typeof(NativeMethods.ThemeTextOptions)),
					_flags = NativeMethods.DTT_COMPOSITED | NativeMethods.DTT_GLOWSIZE | NativeMethods.DTT_TEXTCOLOR,
					_glowSize = glowSize
				};
				textBuffer = Marshal.StringToBSTR(text);
				NativeMethods.DrawThemeTextEx(captionStyle.Handle, memoryDc, 0, 0, textBuffer, -1, NativeMethods.CaptionTextFormat, ref textBounds, ref options);
				NativeMethods.BitBlt(targetDc, destination._left, destination._top, destination._right - destination._left, destination._bottom - destination._top, memoryDc, 0, 0, NativeMethods.SRCCOPY);
			}
			catch (InvalidOperationException)
			{
				// Visual styles are off, so the caption text is not drawn.
			}
			finally
			{
				if (textBuffer != IntPtr.Zero)
				{
					Marshal.FreeBSTR(textBuffer);
				}
				NativeMethods.SelectObject(memoryDc, previousBitmap);
				NativeMethods.SelectObject(memoryDc, previousFont);
				NativeMethods.DeleteObject(dib);
				NativeMethods.DeleteObject(fontHandle);
			}
		}

		public static void DrawVerticalArrow(Graphics graphics, int arrowSize, Rectangle rectangle, int direction, Pen pen)
		{
			int lineLength = arrowSize / 2;
			if (lineLength % 2 == 0)
			{
				lineLength--;
			}
			int arrowDepth = lineLength / 2 + 1;
			int lineY = ((direction <= 0) ? (rectangle.Top + (rectangle.Height - arrowDepth) / 2 + arrowDepth - 1) : (rectangle.Top + (rectangle.Height - arrowDepth) / 2));
			int lineX = rectangle.Left + (rectangle.Width - lineLength) / 2;
			while (lineLength >= 0)
			{
				if (lineLength == 1)
				{
					graphics.DrawLine(pen, lineX, lineY, (float)lineX + 0.5f, lineY);
				}
				else
				{
					graphics.DrawLine(pen, lineX, lineY, lineX + lineLength - 1, lineY);
				}
				lineX++;
				lineLength -= 2;
				lineY += direction;
			}
		}

		public static void DrawHorizontalArrow(Graphics graphics, int arrowSize, Rectangle rectangle, int direction, Pen pen)
		{
			int lineLength = arrowSize / 2;
			if (lineLength % 2 == 0)
			{
				lineLength--;
			}
			int arrowDepth = lineLength / 2 + 1;
			int lineX = ((direction <= 0) ? (rectangle.Left + (rectangle.Width - arrowDepth) / 2 + arrowDepth - 1) : (rectangle.Left + (rectangle.Width - arrowDepth) / 2));
			int lineY = rectangle.Top + (rectangle.Height - lineLength) / 2;
			while (lineLength >= 0)
			{
				if (lineLength == 1)
				{
					graphics.DrawLine(pen, lineX - direction, lineY, lineX, lineY);
				}
				else
				{
					graphics.DrawLine(pen, lineX, lineY, lineX, lineY + lineLength - 1);
				}
				lineX += direction;
				lineLength -= 2;
				lineY++;
			}
		}

		public static RectangleF MeasureLastWord(Graphics graphics, string text, Rectangle layoutRect, Font font)
		{
			int lastSpaceIndex = text.LastIndexOf(" ");
			CharacterRange[] measurableCharacterRanges = new CharacterRange[1]
			{
				new CharacterRange(lastSpaceIndex + 1, text.Length - lastSpaceIndex - 1)
			};
			using StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.SetMeasurableCharacterRanges(measurableCharacterRanges);
			Region[] regions = graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat);
			try
			{
				return regions[0].GetBounds(graphics);
			}
			finally
			{
				foreach (Region region in regions)
				{
					region.Dispose();
				}
			}
		}

		public static Rectangle GetLastWordBounds(Graphics graphics, TextLayoutRenderer textLayoutRenderer, string text, Rectangle layoutRect, Font font, int width, bool isCentered)
		{
			if (text.Length == 0)
			{
				return Rectangle.Empty;
			}
			if (char.IsSeparator(text[text.Length - 1]) || char.IsPunctuation(text[text.Length - 1]))
			{
				text = text.Substring(0, text.Length - 1);
				return GetLastWordBounds(graphics, textLayoutRenderer, text, layoutRect, font, width, isCentered);
			}
			int textHeight = layoutRect.Height / 2;
			textHeight = font.Height + 1;
			if (isCentered)
			{
				return new Rectangle(layoutRect.Left + (layoutRect.Width - width) / 2, layoutRect.Top + textHeight - 1, width, textHeight);
			}
			int lastSeparatorIndex = text.LastIndexOf(" ");
			int lastHyphenIndex = text.LastIndexOf("-");
			if (lastHyphenIndex > lastSeparatorIndex && lastHyphenIndex < text.Length - 1)
			{
				lastSeparatorIndex = lastHyphenIndex;
			}
			CharacterRange[] measurableCharacterRanges = new CharacterRange[1]
			{
				new CharacterRange(lastSeparatorIndex + 1, text.Length - lastSeparatorIndex - 1)
			};
			using StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.SetMeasurableCharacterRanges(measurableCharacterRanges);
			Region[] regions = graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat);
			int rightOffset = (int)(font.SizeInPoints / 2f);
			if (textLayoutRenderer._useTextRenderer)
			{
				rightOffset = (int)font.SizeInPoints - (int)font.SizeInPoints / 4;
			}
			RectangleF bounds;
			try
			{
				bounds = regions[0].GetBounds(graphics);
			}
			finally
			{
				foreach (Region region in regions)
				{
					region.Dispose();
				}
			}
			if (bounds.Y <= (float)(layoutRect.Y + 5))
			{
				return new Rectangle(layoutRect.Left + (layoutRect.Width - width) / 2, layoutRect.Top + textHeight - 1, width, textHeight);
			}
			return new Rectangle((int)bounds.Right - rightOffset, layoutRect.Top + textHeight - 1, width, textHeight);
		}

		public static Color BlendColors(Color color, Color otherColor, float floatValue)
		{
			if (color == Color.Empty)
			{
				return otherColor;
			}
			if (otherColor == Color.Empty)
			{
				return color;
			}
			float redBlend = (float)(int)color.R * floatValue + (float)(int)otherColor.R * (1f - floatValue);
			int red = (int)Math.Round(redBlend, 0);
			float greenBlend = (float)(int)color.G * floatValue + (float)(int)otherColor.G * (1f - floatValue);
			int green = (int)Math.Round(greenBlend, 0);
			float blueBlend = (float)(int)color.B * floatValue + (float)(int)otherColor.B * (1f - floatValue);
			int blue = (int)Math.Round(blueBlend, 0);
			if (red > 255)
			{
				red = 255;
			}
			if (green > 255)
			{
				green = 255;
			}
			if (blue > 255)
			{
				blue = 255;
			}
			return Color.FromArgb(red, green, blue);
		}

		public static string GetIndexKeyLabel(object value)
		{
			string text = "";
			if (value is RibbonTab)
			{
				RibbonTab ribbonTab = (RibbonTab)value;
				text = "[" + ribbonTab.Index + "]";
				if (ribbonTab.Key.Length > 0)
				{
					text = text + " - '" + ribbonTab.Key + "'";
				}
			}
			else if (value is RibbonContextualTab)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)value;
				text = "[" + ribbonContextualTab.GetIndexInOwner() + "]";
				if (ribbonContextualTab.Key.Length > 0)
				{
					text = text + " - '" + ribbonContextualTab.Key + "'";
				}
			}
			else if (value is CommandBase)
			{
				CommandBase commandBase = (CommandBase)value;
				text = "[" + commandBase.GetIndexInOwner() + "]";
				if (commandBase.Key.Length > 0)
				{
					text = text + " - '" + commandBase.Key + "'";
				}
			}
			return text;
		}

		public static int LayoutImageAndText(Graphics graphics, Size size, ref Rectangle rectangle, int maxTextWidth, HorizontalAlignment horizontalAlignment, string text, TextLayoutRenderer textLayoutRenderer, Font font, RibbonForegroundStyle ribbonForegroundStyle)
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
				switch (horizontalAlignment)
				{
				case HorizontalAlignment.Left:
					horizontalAlignment = HorizontalAlignment.Right;
					break;
				case HorizontalAlignment.Right:
					horizontalAlignment = HorizontalAlignment.Left;
					break;
				}
			}
			switch (horizontalAlignment)
			{
			case HorizontalAlignment.Left:
			{
				int imageLeft = rectangle.Left + 2;
				textWidth = (int)textLayoutRenderer.MeasureStyledText(graphics, text, ribbonForegroundStyle, font, maxTextWidth, 0).Width + 1;
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
				return imageLeft;
			}
			case HorizontalAlignment.Right:
			{
				int result = rectangle.Right - size.Width - 2;
				textWidth = (int)textLayoutRenderer.MeasureStyledText(graphics, text, ribbonForegroundStyle, font, maxTextWidth, 0).Width + 1;
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
				return result;
			}
			default:
				return rectangle.Left + (rectangle.Width - size.Width) / 2;
			}
		}
	}
}
