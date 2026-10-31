using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal partial class VisualStylePainter
	{

		private void DrawTabThemeBackground(Graphics targetGraphics, Rectangle bounds, int partId, int stateId, bool isSelected)
		{
			if (_tabThemeHandle == IntPtr.Zero)
			{
				_tabThemeHandle = NativeMethods.OpenThemeData(IntPtr.Zero, "Tab");
			}
			else if (string.Compare(_className, "Tab", ignoreCase: true) != 0)
			{
				NativeMethods.CloseThemeData(_tabThemeHandle);
				_tabThemeHandle = IntPtr.Zero;
				_tabThemeHandle = NativeMethods.OpenThemeData(IntPtr.Zero, "Tab");
			}
			if (isSelected)
			{
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
					using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
					{
						IntPtr bitmapHdc = bitmapGraphics.GetHdc();
						try
						{
							Rectangle bitmapBounds = new Rectangle(0, 0, bounds.Width, bounds.Height);
							NativeMethods.NativeRectClass bitmapThemeRect = NativeMethods.NativeRectClass.FromRectangle(bitmapBounds);
							NativeMethods.DrawThemeBackground(_tabThemeHandle, bitmapHdc, partId, stateId, bitmapThemeRect, bitmapThemeRect);
						}
						finally
						{
							RenderingHelper.ReleaseGraphicsHdc(bitmapGraphics, bitmapHdc);
						}
						// The edge colour is read a few pixels below the top; a very small tab uses its last row.
						Pen pen = GdiCache.Pen(bitmap.GetPixel(bounds.Width - 1, GetTabEdgeSampleRow(bounds.Height)));
						bitmapGraphics.DrawLine(pen, bounds.Width - 1, bounds.Height - 1, bounds.Width - 1, bounds.Height);
						targetGraphics.DrawImageUnscaled(bitmap, bounds);
					}
				}
			}
			else
			{
				IntPtr targetHdc = targetGraphics.GetHdc();
				try
				{
					NativeMethods.NativeRectClass targetThemeRect = NativeMethods.NativeRectClass.FromRectangle(bounds);
					NativeMethods.DrawThemeBackground(_tabThemeHandle, targetHdc, partId, stateId, targetThemeRect, targetThemeRect);
				}
				finally
				{
					RenderingHelper.ReleaseGraphicsHdc(targetGraphics, targetHdc);
				}
			}
		}

		internal static int GetTabEdgeSampleRow(int height)
		{
			return Math.Min(5, height - 1);
		}

		public void DrawAlignedTabThemeBackground(Graphics targetGraphics, Rectangle bounds, TabStripAlignment tabStripAlignment, int partId, int stateId, bool isSelected)
		{
			if (tabStripAlignment == TabStripAlignment.Top)
			{
				DrawTabThemeBackground(targetGraphics, bounds, partId, stateId, isSelected);
			}
			else if (bounds.Height > 0 && bounds.Width > 0)
			{
				bool vertical = tabStripAlignment != TabStripAlignment.Bottom;
				Size size = vertical ? new Size(bounds.Height, bounds.Width) : bounds.Size;
				using (Bitmap bitmap = new Bitmap(size.Width, size.Height))
				{
					using (Graphics bitmapGraphics = Graphics.FromImage(bitmap))
					{
						DrawTabThemeBackground(bitmapGraphics, new Rectangle(Point.Empty, size), partId, stateId, isSelected);
					}
					switch (tabStripAlignment)
					{
					case TabStripAlignment.Bottom:
						bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
						break;
					case TabStripAlignment.Right:
						bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;
					default:
						bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
						break;
					}
					targetGraphics.DrawImageUnscaled(bitmap, bounds);
				}
			}
		}
	}
}
