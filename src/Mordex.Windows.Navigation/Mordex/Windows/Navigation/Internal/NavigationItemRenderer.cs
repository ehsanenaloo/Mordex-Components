using System;
using System.Drawing;
using System.Drawing.Text;
using System.Security;

namespace Mordex.Windows.Navigation.Internal
{
	internal class NavigationItemRenderer
	{
		private const int SrcCopyRasterOperation = 0x00CC0020;

		internal NavigationItem _item;

		private Rectangle _dragBounds;

		private Bitmap _dragImage;

		private Bitmap _savedScreenBitmap;

		internal NavigationItemRenderer(NavigationItem item, int width, int height)
		{
			Initialize(item, width, height);
		}

		private void Initialize(NavigationItem navigationBarItem, int width, int height)
		{
			_item = navigationBarItem;
			_dragBounds = new Rectangle(0, 0, width, height);
			_savedScreenBitmap = new Bitmap(width, height);
			_dragImage = new Bitmap(width, height);
		}

		internal void CaptureAndDrawDragImage(Point location)
		{
			_dragBounds.Location = location;
			IntPtr desktopDc = IntPtr.Zero;
			Graphics desktopGraphics = null;
			try
			{
				desktopDc = NativeMethods.GetWindowDC(NativeMethods.GetDesktopWindow());
				desktopGraphics = Graphics.FromHdc(desktopDc);
				Bitmap previousCapture = _savedScreenBitmap;
				_savedScreenBitmap = CaptureScreenArea(desktopDc, location.X, location.Y, _dragBounds.Width, _dragBounds.Height);
				previousCapture?.Dispose();
				desktopGraphics.DrawImageUnscaled(_dragImage, location.X, location.Y);
			}
			finally
			{
				if (desktopGraphics != null)
				{
					desktopGraphics.Dispose();
					desktopGraphics = null;
				}
				NativeMethods.ReleaseDC(NativeMethods.GetDesktopWindow(), desktopDc);
			}
		}

		public Graphics CreateDragImageGraphics()
		{
			return Graphics.FromImage(_dragImage);
		}

		public void RestoreScreenArea()
		{
			IntPtr desktopDc = IntPtr.Zero;
			IntPtr desktopWindow = IntPtr.Zero;
			Graphics desktopGraphics = null;
			try
			{
				if (_savedScreenBitmap != null)
				{
					desktopWindow = NativeMethods.GetDesktopWindow();
					desktopDc = NativeMethods.GetWindowDC(desktopWindow);
					desktopGraphics = Graphics.FromHdc(desktopDc);
					desktopGraphics.DrawImageUnscaled(_savedScreenBitmap, _dragBounds.X, _dragBounds.Y);
				}
			}
			finally
			{
				desktopGraphics?.Dispose();
				NativeMethods.ReleaseDC(desktopWindow, desktopDc);
			}
		}

		private Bitmap CaptureScreenArea(IntPtr hdc, int sourceX, int sourceY, int width, int height)
		{
			Bitmap capturedBitmap = new Bitmap(width, height);
			try
			{
				using (Graphics bitmapGraphics = Graphics.FromImage(capturedBitmap))
				{
					bitmapGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
					IntPtr bitmapDc = bitmapGraphics.GetHdc();
					try
					{
						NativeMethods.BitBlt(bitmapDc, 0, 0, width, height, hdc, sourceX, sourceY, SrcCopyRasterOperation);
					}
					finally
					{
						bitmapGraphics.ReleaseHdc(bitmapDc);
					}
				}
				return capturedBitmap;
			}
			catch
			{
				capturedBitmap.Dispose();
				throw;
			}
		}

		public void DisposeBitmaps()
		{
			if (_dragImage != null)
			{
				_dragImage.Dispose();
				_dragImage = null;
			}
			if (_savedScreenBitmap != null)
			{
				_savedScreenBitmap.Dispose();
				_savedScreenBitmap = null;
			}
		}
	}
}
