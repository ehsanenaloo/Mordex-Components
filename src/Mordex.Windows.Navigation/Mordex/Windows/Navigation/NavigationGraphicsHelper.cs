#pragma warning disable CS0618 // Bridges the obsolete Office2007 API to the current one.
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation
{
	internal class NavigationGraphicsHelper
	{
		private NavigationGraphicsHelper()
		{
		}

		public static bool IsFeatureAvailable()
		{
			try
			{
				return true;
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return false;
			}
		}

		internal static Graphics GraphicsFromHdc(IntPtr hdc)
		{
			return Graphics.FromHdc(hdc);
		}


		internal static void SetAllowDrop(Control control, bool allowDrop)
		{
			control.AllowDrop = allowDrop;
		}

		internal static void FocusControl(Control control)
		{
			control.Focus();
		}

		public static Bitmap MakeMagentaTransparent(Bitmap sourceBitmap)
		{
			try
			{
				sourceBitmap?.MakeTransparent(Color.Magenta);
				return sourceBitmap;
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return null;
			}
		}

		private static Bitmap IconToBitmap(Icon icon)
		{
			NativeMethods.NativeIconInfo iconInfo;
			if (!NativeMethods.GetIconInfo(icon.Handle, out iconInfo))
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
					Rectangle lockRect = new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height);
					BitmapData bitmapData = colorBitmap.LockBits(lockRect, ImageLockMode.ReadOnly, colorBitmap.PixelFormat);
					Bitmap alphaCopy = null;
					try
					{
						if (!HasPartialAlpha(bitmapData))
						{
							return icon.ToBitmap();
						}
						using (Bitmap wrapper = new Bitmap(bitmapData.Width, bitmapData.Height, bitmapData.Stride, PixelFormat.Format32bppArgb, bitmapData.Scan0))
						{
							alphaCopy = new Bitmap(wrapper);
						}
						return alphaCopy;
					}
					finally
					{
						colorBitmap.UnlockBits(bitmapData);
					}
				}
			}
			finally
			{
				NativeMethods.DeleteObject(iconInfo._colorBitmap);
				NativeMethods.DeleteObject(iconInfo._maskBitmap);
			}
		}

		private static bool HasPartialAlpha(BitmapData bitmapData)
		{
			for (int row = 0; row < bitmapData.Height; row++)
			{
				for (int column = 0; column < bitmapData.Width; column++)
				{
					int alpha = (Marshal.ReadInt32(bitmapData.Scan0, bitmapData.Stride * row + 4 * column) >> 24) & 0xFF;
					if (alpha > 0 && alpha < byte.MaxValue)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void DrawIcon(Graphics graphics, Icon original, Rectangle targetRect, bool isEnabled, Color background)
		{
			Icon icon = null;
			Icon scaledIcon = null;
			if (original.Size == targetRect.Size)
			{
				icon = original;
			}
			else
			{
				scaledIcon = new Icon(original, targetRect.Size);
				icon = ((!(scaledIcon.Size != targetRect.Size)) ? scaledIcon : original);
			}
			if (isEnabled)
			{
				graphics.DrawIcon(icon, targetRect);
			}
			else
			{
				using (Bitmap iconBitmap = IconToBitmap(icon))
				{
					if (iconBitmap.Size != targetRect.Size)
					{
						using (Bitmap scaledBitmap = new Bitmap(iconBitmap, targetRect.Size))
						{
							ControlPaint.DrawImageDisabled(graphics, scaledBitmap, targetRect.X, targetRect.Y, background);
						}
					}
					else
					{
						ControlPaint.DrawImageDisabled(graphics, iconBitmap, targetRect.X, targetRect.Y, background);
					}
				}
			}
			if (scaledIcon != null && scaledIcon != original)
			{
				scaledIcon.Dispose();
				scaledIcon = null;
			}
		}

		public static void DrawIconOrImage(Graphics graphics, Icon icon, Image image, int index, ImageList imageList, bool isEnabled, Rectangle rect, Color background)
		{
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
					Bitmap disabledBitmap = new Bitmap(image, rect.Width, rect.Height);
					ControlPaint.DrawImageDisabled(graphics, disabledBitmap, rect.X, rect.Y, background);
					disabledBitmap.Dispose();
					disabledBitmap = null;
				}
			}
		}

		public static void DrawImageWithShadow(Graphics graphics, Icon icon, Image image, Rectangle destRect)
		{
			ImageAttributes imageAttributes = new ImageAttributes();
			float[][] matrixRows = new float[5][];
			float[] redRow = new float[5];
			matrixRows[0] = redRow;
			float[] greenRow = new float[5];
			matrixRows[1] = greenRow;
			float[] blueRow = new float[5];
			matrixRows[2] = blueRow;
			matrixRows[3] = new float[5] { 0f, 0f, 0f, 0.25f, 0f };
			matrixRows[4] = new float[5] { 0f, 0f, 0f, 0f, 1f };
			float[][] newColorMatrix = matrixRows;
			ColorMatrix shadowColorMatrix = new ColorMatrix(newColorMatrix);
			imageAttributes.SetColorMatrix(shadowColorMatrix, ColorMatrixFlag.Default);
			destRect.Offset(1, 1);
			if (icon != null)
			{
				using (Bitmap iconSource = icon.ToBitmap())
				using (Bitmap iconBitmap = new Bitmap(iconSource, destRect.Size))
				{
					graphics.DrawImage(iconBitmap, destRect, 1, 1, destRect.Width, destRect.Height, GraphicsUnit.Pixel, imageAttributes);
				}
				destRect.Offset(-2, -2);
				graphics.DrawIcon(icon, destRect);
			}
			else if (image != null)
			{
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
				destRect.Offset(-2, -2);
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
			}
			imageAttributes.Dispose();
			imageAttributes = null;
		}

		internal static string GenerateUniqueKey(Type componentType, string keyPrefix, int startIndex, ArrayList arrayList)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find("Key", ignoreCase: true);
			if (propertyDescriptor == null)
			{
				return "";
			}
			HashSet<string> existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (object item in arrayList)
			{
				existingKeys.Add((string)propertyDescriptor.GetValue(item));
			}
			int index = startIndex;
			while (existingKeys.Contains(keyPrefix + index))
			{
				index++;
			}
			return keyPrefix + index;
		}

		public static Brush CreateSolidBrush(Color baseColor, int alpha, AlphaMode alphaMode)
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
					alpha = 255;
				}
				return new SolidBrush(Color.FromArgb(alpha, baseColor));
			default:
				return new SolidBrush(baseColor);
			}
		}

		public static int CompareValues(object leftValue, object rightValue, IComparer iComparer)
		{
			int comparison = 0;
			try
			{
				if (iComparer == null)
				{
					if (leftValue == DBNull.Value)
					{
						leftValue = null;
					}
					if (rightValue == DBNull.Value)
					{
						rightValue = null;
					}
					if (leftValue is string && rightValue is string)
					{
						comparison = CultureInfo.CurrentCulture.CompareInfo.Compare((string)leftValue, (string)rightValue);
					}
					else if (leftValue != null)
					{
						if (rightValue != null)
						{
							if (leftValue is IComparable)
							{
								comparison = ((IComparable)leftValue).CompareTo(rightValue);
							}
						}
						else
						{
							comparison = 1;
						}
					}
					else if (leftValue == null)
					{
						if (rightValue != null)
						{
							comparison = -1;
						}
					}
					else if (rightValue != null)
					{
						comparison = 1;
					}
				}
				else
				{
					comparison = iComparer.Compare(leftValue, rightValue);
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				comparison = 1;
			}
			return comparison;
		}

		public static Rectangle DrawBorderEdges(Graphics graphics, Pen pen, Rectangle bounds, BorderFlags borderFlags)
		{
			Rectangle innerBounds = bounds;
			if ((borderFlags & BorderFlags.Left) != BorderFlags.None)
			{
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
				innerBounds.X++;
				innerBounds.Width--;
			}
			if ((borderFlags & BorderFlags.Top) != BorderFlags.None)
			{
				graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
				innerBounds.Y++;
				innerBounds.Height--;
			}
			if ((borderFlags & BorderFlags.Bottom) != BorderFlags.None)
			{
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
				innerBounds.Height--;
			}
			if ((borderFlags & BorderFlags.Right) != BorderFlags.None)
			{
				graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
				innerBounds.Width--;
			}
			return innerBounds;
		}

		public static Rectangle DrawBorderRectangle(Graphics graphics, Pen pen, Rectangle bounds)
		{
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			bounds.Inflate(-1, -1);
			return bounds;
		}

		public static Color BlendColors(Color baseColor, Color mixColor, float floatValue)
		{
			if (baseColor == Color.Empty)
			{
				return mixColor;
			}
			if (mixColor == Color.Empty)
			{
				return baseColor;
			}
			float redMix = (float)(int)baseColor.R * floatValue + (float)(int)mixColor.R * (1f - floatValue);
			int red = (int)Math.Round(redMix, 0);
			float greenMix = (float)(int)baseColor.G * floatValue + (float)(int)mixColor.G * (1f - floatValue);
			int green = (int)Math.Round(greenMix, 0);
			float blueMix = (float)(int)baseColor.B * floatValue + (float)(int)mixColor.B * (1f - floatValue);
			int blue = (int)Math.Round(blueMix, 0);
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
	}
}
