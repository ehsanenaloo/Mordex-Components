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

		public static void DrawVerticalFadeLines(Graphics graphics, Point location, int baseAlpha, int alphaStep, Color baseColor)
		{
			Size size = new Size(FadeLength, FadeLength);
			for (int i = 0; i < FadeLength; i++)
			{
				Color lineColor = Color.FromArgb(baseAlpha + alphaStep * i, baseColor);
				Color transparentColor = Color.FromArgb(0, 0, 0, 0);
				Rectangle rect = new Rectangle(location, size);
				if (rect.Height > 0 && rect.Width > 0)
				{
					rect.Width++;
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, lineColor, transparentColor, 90f);
					rect.Width--;
					Pen pen = new Pen(linearGradientBrush);
					Point pt = location;
					pt.Y += FadeLength - 1;
					graphics.DrawLine(pen, location, pt);
					pen.Dispose();
					pen = null;
					linearGradientBrush.Dispose();
					linearGradientBrush = null;
				}
				location.Offset(1, 0);
			}
		}

		public static void DrawHorizontalFadeLines(Graphics graphics, Point location, int baseAlpha, int alphaStep, Color baseColor)
		{
			Size size = new Size(FadeLength, FadeLength);
			for (int i = 0; i < FadeLength; i++)
			{
				Color lineColor = Color.FromArgb(baseAlpha + alphaStep * i, baseColor);
				Color transparentColor = Color.FromArgb(0, 0, 0, 0);
				Rectangle rect = new Rectangle(location, size);
				if (rect.Width > 0 && rect.Height > 0)
				{
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, lineColor, transparentColor, 0f);
					Pen pen = new Pen(linearGradientBrush);
					Point pt = location;
					pt.X += FadeLength - 1;
					graphics.DrawLine(pen, location, pt);
					pen.Dispose();
					pen = null;
					linearGradientBrush.Dispose();
					linearGradientBrush = null;
				}
				location.Offset(0, 1);
			}
		}

		public static void InvertDragFrames(Rectangle[] frames)
		{
			if (frames.Length == 0)
			{
				return;
			}
			IntPtr region = CreateFrameRegion(frames[0], DragFrameThickness);
			try
			{
				for (int i = 1; i < frames.Length; i++)
				{
					NativeMethods.NativeRect emptyRect = NativeMethods.ToNativeRect(Rectangle.Empty);
					IntPtr nextRegion = CreateFrameRegion(frames[i], DragFrameThickness);
					IntPtr combined = NativeMethods.CreateRectRgnIndirect(ref emptyRect);
					NativeMethods.CombineRgn(combined, region, nextRegion, RegionXor);
					NativeMethods.DeleteObject(region);
					NativeMethods.DeleteObject(nextRegion);
					region = combined;
				}
				InvertWithDragHatch(region);
			}
			finally
			{
				NativeMethods.DeleteObject(region);
			}
		}

		// Draws the 50% hatch used for drag frames on the screen inside the region. It uses XOR, so a second call erases it.
		internal static void InvertWithDragHatch(IntPtr region)
		{
			IntPtr screenDC = NativeMethods.GetDC(IntPtr.Zero);
			if (screenDC == IntPtr.Zero)
			{
				return;
			}
			try
			{
				NativeMethods.SelectClipRgn(screenDC, region);
				NativeMethods.NativeRect box = default(NativeMethods.NativeRect);
				NativeMethods.GetClipBox(screenDC, ref box);
				lock (_dragHatchLock)
				{
					IntPtr oldBrush = NativeMethods.SelectObject(screenDC, GetDragHatchBrush());
					NativeMethods.PatBlt(screenDC, box._left, box._top, box._right - box._left, box._bottom - box._top, PatternInvertRop);
					NativeMethods.SelectObject(screenDC, oldBrush);
				}
				NativeMethods.SelectClipRgn(screenDC, IntPtr.Zero);
			}
			finally
			{
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);
			}
		}

		// Returns a shared brush that this class owns, so callers must not delete it. Call it inside _dragHatchLock.
		internal static IntPtr GetDragHatchBrush()
		{
			Color color = SystemColors.ControlDark;
			if (_dragHatchBrush != IntPtr.Zero && _dragHatchColor.ToArgb() == color.ToArgb())
			{
				return _dragHatchBrush;
			}
			IntPtr brush = IntPtr.Zero;
			using (Bitmap bitmap = new Bitmap(DragHatchSize, DragHatchSize, PixelFormat.Format32bppArgb))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				using (Brush hatch = new HatchBrush(HatchStyle.Percent50, Color.White, color))
				{
					graphics.FillRectangle(hatch, 0, 0, DragHatchSize, DragHatchSize);
				}
				IntPtr hbitmap = bitmap.GetHbitmap();
				try
				{
					NativeMethods.NativeLogBrush brushInfo = new NativeMethods.NativeLogBrush
					{
						_style = PatternBrushStyle,
						_hatch = hbitmap
					};
					brush = NativeMethods.CreateBrushIndirect(ref brushInfo);
				}
				finally
				{
					NativeMethods.DeleteObject(hbitmap);
				}
			}
			if (brush != IntPtr.Zero)
			{
				if (_dragHatchBrush != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(_dragHatchBrush);
				}
				_dragHatchBrush = brush;
				_dragHatchColor = color;
			}
			return _dragHatchBrush;
		}

		internal static IntPtr CreateFrameRegion(Rectangle bounds, int thickness)
		{
			NativeMethods.NativeRect boundsData = NativeMethods.ToNativeRect(bounds);
			IntPtr outerRegion = NativeMethods.CreateRectRgnIndirect(ref boundsData);
			if (bounds.Width <= thickness || thickness <= 0 || bounds.Height <= thickness)
			{
				return outerRegion;
			}
			bounds.Inflate(-thickness, -thickness);
			boundsData = NativeMethods.ToNativeRect(bounds);
			IntPtr innerRegion = NativeMethods.CreateRectRgnIndirect(ref boundsData);
			NativeMethods.NativeRect emptyData = default(NativeMethods.NativeRect);
			IntPtr frameRegion = NativeMethods.CreateRectRgnIndirect(ref emptyData);
			NativeMethods.CombineRgn(frameRegion, outerRegion, innerRegion, RegionXor);
			NativeMethods.DeleteObject(outerRegion);
			NativeMethods.DeleteObject(innerRegion);
			return frameRegion;
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

		public static Rectangle DrawColoredBorder(Graphics graphics, Color baseColor, Rectangle rectangle, BorderStyle borderStyle)
		{
			if (borderStyle == BorderStyle.None)
			{
				return rectangle;
			}
			Color shadowColor;
			Color darkShadowColor;
			Color lightColor;
			Color highlightColor;
			if (baseColor.Equals(SystemColors.Control) || baseColor.Equals(SystemColors.Window) || baseColor.IsEmpty)
			{
				shadowColor = SystemColors.ControlDark;
				darkShadowColor = SystemColors.ControlDarkDark;
				lightColor = SystemColors.ControlLight;
				highlightColor = SystemColors.ControlLightLight;
			}
			else
			{
				shadowColor = ControlPaint.Dark(baseColor, 0f);
				darkShadowColor = ControlPaint.Dark(baseColor);
				lightColor = ControlPaint.Light(baseColor);
				highlightColor = ControlPaint.LightLight(baseColor);
			}
			Pen pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Flat:
				pen = GdiCache.Pen(shadowColor);
				rectangle = DrawRectangleOutline(graphics, pen, rectangle);
				pen = null;
				return rectangle;
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = GdiCache.Pen(highlightColor);
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = GdiCache.Pen(shadowColor);
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Raised:
				pen = GdiCache.Pen(darkShadowColor);
				break;
			case BorderStyle.RaisedLight3D:
				pen = GdiCache.Pen(shadowColor);
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = GdiCache.Pen(highlightColor);
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			pen = null;
			if (borderStyle == BorderStyle.Raised || borderStyle == BorderStyle.Sunken)
			{
				rectangle.Inflate(-1, -1);
				pen = ((borderStyle != BorderStyle.Raised) ? GdiCache.Pen(darkShadowColor) : GdiCache.Pen(lightColor));
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
				pen = null;
				pen = ((borderStyle != BorderStyle.Raised) ? GdiCache.Pen(lightColor) : GdiCache.Pen(shadowColor));
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
				pen = null;
			}
			if (borderStyle != BorderStyle.None)
			{
				rectangle.Inflate(-1, -1);
			}
			return rectangle;
		}
	}
}
