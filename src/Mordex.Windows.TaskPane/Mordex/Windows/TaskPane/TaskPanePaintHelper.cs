using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.TaskPane
{
	internal class TaskPanePaintHelper
	{

		internal static void FocusControl(Control control)
		{
			control.Focus();
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

		public static Bitmap MakeMagentaTransparent(Bitmap bitmap)
		{
			try
			{
				bitmap?.MakeTransparent(Color.Magenta);
				return bitmap;
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return null;
			}
		}

		public static int CompareValues(object left, object right, IComparer iComparer)
		{
			int comparison = 0;
			try
			{
				if (iComparer == null)
				{
					if (left == DBNull.Value)
					{
						left = null;
					}
					if (right == DBNull.Value)
					{
						right = null;
					}
					if (left is string && right is string)
					{
						comparison = CultureInfo.CurrentCulture.CompareInfo.Compare((string)left, (string)right);
					}
					else if (left != null)
					{
						if (right != null)
						{
							if (left is IComparable)
							{
								comparison = ((IComparable)left).CompareTo(right);
							}
						}
						else
						{
							comparison = 1;
						}
					}
					else if (left == null)
					{
						if (right != null)
						{
							comparison = -1;
						}
					}
					else if (right != null)
					{
						comparison = 1;
					}
				}
				else
				{
					comparison = iComparer.Compare(left, right);
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				comparison = 1;
			}
			return comparison;
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

		public static Rectangle DrawOutline(Graphics graphics, Pen pen, Rectangle bounds)
		{
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			bounds.Inflate(-1, -1);
			return bounds;
		}

		public static Rectangle DrawBorder(Graphics graphics, Color baseColor, Color flatBorderColor, Rectangle bounds, BorderStyle borderStyle)
		{
			if (borderStyle == BorderStyle.None)
			{
				return bounds;
			}
			Color darkColor = ControlPaint.Dark(baseColor, 0f);
			Color darkDarkColor = ControlPaint.DarkDark(baseColor);
			Color lightColor = ControlPaint.Light(baseColor, 0f);
			Color lightLightColor = ControlPaint.LightLight(baseColor);
			Pen pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Flat:
				bounds = DrawOutline(graphics, GdiCache.Pen(flatBorderColor), bounds);
				return bounds;
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = GdiCache.Pen(lightLightColor);
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = GdiCache.Pen(darkColor);
				break;
			}
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
			switch (borderStyle)
			{
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = GdiCache.Pen(darkColor);
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = GdiCache.Pen(lightLightColor);
				break;
			}
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			if (borderStyle == BorderStyle.Raised || borderStyle == BorderStyle.Sunken)
			{
				bounds.Inflate(-1, -1);
				pen = GdiCache.Pen(borderStyle != BorderStyle.Raised ? darkDarkColor : lightColor);
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
				graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
				pen = GdiCache.Pen(borderStyle != BorderStyle.Raised ? lightColor : darkColor);
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
				graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			}
			if (borderStyle != BorderStyle.None)
			{
				bounds.Inflate(-1, -1);
			}
			return bounds;
		}

		public static Rectangle DrawBorderFromStyle(Graphics graphics, ControlStyle controlStyle, Rectangle bounds, BorderStyle borderStyle)
		{
			if (controlStyle == null && borderStyle == BorderStyle.None)
			{
				return bounds;
			}
			Pen pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Flat:
				bounds = DrawOutline(graphics, controlStyle.GetFramePen(), bounds);
				return bounds;
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = controlStyle.GetLightLightPen();
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = controlStyle.GetDarkPen();
				break;
			}
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
			switch (borderStyle)
			{
			case BorderStyle.Raised:
				pen = controlStyle.GetDarkDarkPen();
				break;
			case BorderStyle.RaisedLight3D:
				pen = controlStyle.GetDarkPen();
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = controlStyle.GetLightLightPen();
				break;
			}
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			if (borderStyle == BorderStyle.Raised || borderStyle == BorderStyle.Sunken)
			{
				bounds.Inflate(-1, -1);
				pen = ((borderStyle != BorderStyle.Raised) ? controlStyle.GetDarkDarkPen() : controlStyle.GetLightPen());
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
				graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
				pen = ((borderStyle != BorderStyle.Raised) ? controlStyle.GetLightPen() : controlStyle.GetDarkPen());
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
				graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			}
			if (borderStyle != BorderStyle.None)
			{
				bounds.Inflate(-1, -1);
			}
			return bounds;
		}

		public static Brush CreateGradientBrush(Color startColor, Color endColor, Rectangle bounds, BackgroundGradientMode backgroundGradientMode)
		{
			switch (backgroundGradientMode)
			{
			case BackgroundGradientMode.Solid:
				return new SolidBrush(startColor);
			case BackgroundGradientMode.Diagonal:
				return new LinearGradientBrush(bounds, startColor, endColor, 45f);
			case BackgroundGradientMode.DiagonalBackwards:
				return new LinearGradientBrush(bounds, startColor, endColor, 225f);
			case BackgroundGradientMode.Horizontal:
				return new LinearGradientBrush(bounds, startColor, endColor, 0f);
			case BackgroundGradientMode.Vertical:
				return new LinearGradientBrush(bounds, startColor, endColor, 90f);
			default:
				return new SolidBrush(startColor);
			}
		}

		public static Color BlendColors(Color firstColor, Color secondColor, float floatValue)
		{
			if (firstColor == Color.Empty)
			{
				return secondColor;
			}
			if (secondColor == Color.Empty)
			{
				return firstColor;
			}
			float redValue = (float)(int)firstColor.R * floatValue + (float)(int)secondColor.R * (1f - floatValue);
			int red = (int)Math.Round(redValue, 0);
			float greenValue = (float)(int)firstColor.G * floatValue + (float)(int)secondColor.G * (1f - floatValue);
			int green = (int)Math.Round(greenValue, 0);
			float blueValue = (float)(int)firstColor.B * floatValue + (float)(int)secondColor.B * (1f - floatValue);
			int blue = (int)Math.Round(blueValue, 0);
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
