using System;
using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class NativeHeaderDragFeedback : HeaderDragFeedback
	{
		private float _floatValue;

		private bool _gridRenderer23Boolean;

		private Rectangle _gridRenderer23Rectangle;

		private Rectangle _rect;

		private Bitmap _gridRenderer23Bitmap;

		private IntPtr _headerBitmapHandle;

		private Point _gridRenderer23Point;

		private IntPtr _gridRenderer23IntPtr;

		private IntPtr _headerHbitmap;

		private IntPtr _scratchBitmapHandle;

		public NativeHeaderDragFeedback(Graphics graphics, GridGroupLayoutState item, int width, int height)
		{
			UpdateFloatValue();
			InitializeBuffers(graphics, item, width, height);
		}

		private void UpdateFloatValue()
		{
			int value = 10;
			int otherValue = 117;
			IntPtr intPtr = NativeMethods.GetDC(IntPtr.Zero);
			int nextValue = NativeMethods.GetDeviceCaps(intPtr, value).ToInt32();
			int previousValue = NativeMethods.GetDeviceCaps(intPtr, otherValue).ToInt32();
			NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
			_floatValue = (float)previousValue / (float)nextValue;
		}

		private void InitializeBuffers(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, int width, int height)
		{
			if (width > Timing.MaxDragImageSize)
			{
				width = Timing.MaxDragImageSize;
			}
			if (height > Timing.MaxDragImageSize)
			{
				height = Timing.MaxDragImageSize;
			}
			_gridGroupLayoutState = gridGroupLayoutState;
			_gridRenderer23Rectangle = new Rectangle(0, 0, width, height);
			IntPtr intPtr = NativeMethods.GetWindowDC(IntPtr.Zero);
			_gridRenderer23Bitmap = new Bitmap(width, height);
			_headerBitmapHandle = NativeMethods.CreateCompatibleBitmap(intPtr, (int)((float)width * _floatValue), (int)((float)height * _floatValue));
			NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
		}

		internal override void Move()
		{
			IntPtr intPtr = IntPtr.Zero;
			IntPtr compatibleDC = IntPtr.Zero;
			IntPtr otherCompatibleDC = IntPtr.Zero;
			IntPtr selectObject = IntPtr.Zero;
			if (_headerHbitmap == IntPtr.Zero)
			{
				_headerHbitmap = _gridRenderer23Bitmap.GetHbitmap();
			}
			IntPtr hbitmap = _headerHbitmap;
			try
			{
				int x = Cursor.Position.X;
				int y = Cursor.Position.Y;
				intPtr = NativeMethods.GetWindowDC(IntPtr.Zero);
				Point point = new Point(x - _gridRenderer3Point.X, y - _gridRenderer3Point.Y);
				compatibleDC = NativeMethods.CreateCompatibleDC(intPtr);
				selectObject = NativeMethods.SelectObject(compatibleDC, _headerBitmapHandle);
				int value;
				int otherValue;
				if (_gridRenderer23Boolean)
				{
					value = (int)((float)_gridRenderer23Point.X * _floatValue) - (int)((float)point.X * _floatValue);
					otherValue = (int)((float)_gridRenderer23Point.Y * _floatValue) - (int)((float)point.Y * _floatValue);
				}
				else
				{
					value = 0;
					otherValue = 0;
					NativeMethods.BitBlt(compatibleDC, 0, 0, (int)((float)_gridRenderer23Rectangle.Width * _floatValue), (int)((float)_gridRenderer23Rectangle.Height * _floatValue), intPtr, (int)((float)point.X * _floatValue), (int)((float)point.Y * _floatValue), WindowMessages.SourceCopy);
				}
				int scaledWidth = (int)((float)_gridRenderer23Rectangle.Width * _floatValue);
				int scaledHeight = (int)((float)_gridRenderer23Rectangle.Height * _floatValue);
				if (_scratchBitmapHandle == IntPtr.Zero)
				{
					_scratchBitmapHandle = NativeMethods.CreateCompatibleBitmap(intPtr, scaledWidth, scaledHeight);
				}
				otherCompatibleDC = NativeMethods.CreateCompatibleDC(intPtr);
				NativeMethods.SelectObject(otherCompatibleDC, _scratchBitmapHandle);
				NativeMethods.BitBlt(otherCompatibleDC, 0, 0, scaledWidth, scaledHeight, intPtr, (int)((float)point.X * _floatValue), (int)((float)point.Y * _floatValue), WindowMessages.SourceCopy);
				NativeMethods.BitBlt(otherCompatibleDC, value, otherValue, (int)((float)_gridRenderer23Rectangle.Width * _floatValue), (int)((float)_gridRenderer23Rectangle.Height * _floatValue), compatibleDC, 0, 0, WindowMessages.SourceCopy);
				StretchBlitToTarget(intPtr, hbitmap, (int)((float)point.X * _floatValue), (int)((float)point.Y * _floatValue), _gridRenderer23Rectangle.Width, _gridRenderer23Rectangle.Height);
				StretchBlitToTarget(compatibleDC, hbitmap, -value, -otherValue, _gridRenderer23Rectangle.Width, _gridRenderer23Rectangle.Height);
				if (_gridRenderer23Boolean)
				{
					NativeMethods.BitBlt(intPtr, (int)((float)_gridRenderer23Point.X * _floatValue), (int)((float)_gridRenderer23Point.Y * _floatValue), (int)((float)_gridRenderer23Rectangle.Width * _floatValue), (int)((float)_gridRenderer23Rectangle.Height * _floatValue), compatibleDC, 0, 0, WindowMessages.SourceCopy);
				}
				NativeMethods.BitBlt(compatibleDC, 0, 0, (int)((float)_gridRenderer23Rectangle.Width * _floatValue), (int)((float)_gridRenderer23Rectangle.Height * _floatValue), otherCompatibleDC, 0, 0, WindowMessages.SourceCopy);
				_gridRenderer23Point = point;
				_gridRenderer23Boolean = true;
			}
			finally
			{
				if (compatibleDC != IntPtr.Zero)
				{
					NativeMethods.SelectObject(compatibleDC, selectObject);
					NativeMethods.DeleteDC(compatibleDC);
				}
				if (otherCompatibleDC != IntPtr.Zero)
				{
					NativeMethods.DeleteDC(otherCompatibleDC);
				}
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.ReleaseDC(IntPtr.Zero, intPtr);
				}
			}
		}

		private void BlitToTarget(IntPtr otherIntPtr, IntPtr nextIntPtr, int value, int otherValue, int nextValue, int previousValue)
		{
			IntPtr intPtr = NativeMethods.CreateCompatibleDC(otherIntPtr);
			IntPtr selectObject = NativeMethods.SelectObject(intPtr, nextIntPtr);
			NativeMethods.BitBlt(otherIntPtr, value, otherValue, nextValue, previousValue, intPtr, 0, 0, WindowMessages.SourceCopy);
			NativeMethods.SelectObject(intPtr, selectObject);
			NativeMethods.DeleteDC(intPtr);
		}

		private void StretchBlitToTarget(IntPtr otherIntPtr, IntPtr nextIntPtr, int value, int otherValue, int nextValue, int previousValue)
		{
			IntPtr intPtr = NativeMethods.CreateCompatibleDC(otherIntPtr);
			IntPtr selectObject = NativeMethods.SelectObject(intPtr, nextIntPtr);
			NativeMethods.StretchBlt(otherIntPtr, value, otherValue, (int)((float)nextValue * _floatValue), (int)((float)previousValue * _floatValue), intPtr, 0, 0, nextValue, previousValue, WindowMessages.SourceCopy);
			NativeMethods.SelectObject(intPtr, selectObject);
			NativeMethods.DeleteDC(intPtr);
		}

		public override Graphics CreateFeedbackGraphics()
		{
			if (_headerHbitmap != IntPtr.Zero)
			{
				NativeMethods.DeleteObject(_headerHbitmap);
				_headerHbitmap = IntPtr.Zero;
			}
			Graphics graphics = Graphics.FromImage(_gridRenderer23Bitmap);
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			return graphics;
		}

		public override void EraseHeader()
		{
			IntPtr intPtr = IntPtr.Zero;
			IntPtr desktopWindow = IntPtr.Zero;
			Graphics graphics = null;
			try
			{
				if (_gridRenderer23Boolean && _headerBitmapHandle != IntPtr.Zero)
				{
					desktopWindow = NativeMethods.GetDesktopWindow();
					intPtr = NativeMethods.GetWindowDC(desktopWindow);
					BlitToTarget(intPtr, _headerBitmapHandle, (int)((float)_gridRenderer23Point.X * _floatValue), (int)((float)_gridRenderer23Point.Y * _floatValue), (int)((float)_gridRenderer23Rectangle.Width * _floatValue), (int)((float)_gridRenderer23Rectangle.Height * _floatValue));
				}
				_gridRenderer23Boolean = false;
			}
			finally
			{
				graphics?.Dispose();
				NativeMethods.ReleaseDC(desktopWindow, intPtr);
			}
		}

		public override void EraseArrows()
		{
			if (_gridRenderer23IntPtr == IntPtr.Zero)
			{
				return;
			}
			IntPtr intPtr = IntPtr.Zero;
			IntPtr desktopWindow = IntPtr.Zero;
			Graphics graphics = null;
			try
			{
				EraseHeader();
				desktopWindow = NativeMethods.GetDesktopWindow();
				intPtr = NativeMethods.GetWindowDC(desktopWindow);
				graphics = Graphics.FromHdc(intPtr);
				BlitToTarget(intPtr, _gridRenderer23IntPtr, _rect.X, _rect.Y, _rect.Width, _rect.Height);
				Move();
			}
			finally
			{
				_rect = Rectangle.Empty;
				if (_gridRenderer23IntPtr != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(_gridRenderer23IntPtr);
					_gridRenderer23IntPtr = IntPtr.Zero;
				}
				if (graphics != null)
				{
					graphics.Dispose();
					graphics = null;
				}
				NativeMethods.ReleaseDC(desktopWindow, intPtr);
			}
		}

		private void DrawRedArrows(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = GdiCache.Pen(Color.Red);
			for (int i = 0; i < 4; i++)
			{
				graphics.DrawLine(pen, 3, i, 6, i);
			}
			graphics.DrawLine(pen, 0, 4, 9, 4);
			graphics.DrawLine(pen, 1, 5, 8, 5);
			graphics.DrawLine(pen, 2, 6, 7, 6);
			graphics.DrawLine(pen, 3, 7, 6, 7);
			graphics.DrawLine(pen, 4, 8, 5, 8);
			for (int i = 1; i < 5; i++)
			{
				graphics.DrawLine(pen, 3, rectangle.Height - i, 6, rectangle.Height - i);
			}
			graphics.DrawLine(pen, 0, rectangle.Height - 5, 9, rectangle.Height - 5);
			graphics.DrawLine(pen, 1, rectangle.Height - 6, 8, rectangle.Height - 6);
			graphics.DrawLine(pen, 2, rectangle.Height - 7, 7, rectangle.Height - 7);
			graphics.DrawLine(pen, 3, rectangle.Height - 8, 6, rectangle.Height - 8);
			graphics.DrawLine(pen, 4, rectangle.Height - 9, 5, rectangle.Height - 9);
		}

		public override void DrawArrows(Rectangle rect)
		{
			rect.X = (int)((float)rect.X * _floatValue);
			rect.Y = (int)((float)rect.Y * _floatValue);
			rect.Height = (int)((float)rect.Height * _floatValue);
			rect.Width = 0;
			rect.Inflate(5, 10);
			if (_gridRenderer23IntPtr != IntPtr.Zero && _rect == rect)
			{
				return;
			}
			EraseArrows();
			_rect = rect;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr windowDC = IntPtr.Zero;
			IntPtr zero = IntPtr.Zero;
			Graphics graphics = null;
			Bitmap bitmap = null;
			try
			{
				EraseHeader();
				intPtr = NativeMethods.GetDesktopWindow();
				windowDC = NativeMethods.GetWindowDC(intPtr);
				if (_gridRenderer23IntPtr != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(_gridRenderer23IntPtr);
					_gridRenderer23IntPtr = IntPtr.Zero;
				}
				_gridRenderer23IntPtr = CaptureScreenRegion(windowDC, rect.Left, rect.Top, rect.Width, rect.Height);
				zero = CaptureScreenRegion(windowDC, rect.Left, rect.Top, rect.Width, rect.Height);
				bitmap = new Bitmap(_rect.Width, _rect.Height);
				graphics = Graphics.FromImage(bitmap);
				graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				IntPtr hdc = graphics.GetHdc();
				BlitToTarget(hdc, zero, 0, 0, rect.Width, rect.Height);
				graphics.ReleaseHdc(hdc);
				DrawRedArrows(graphics, _rect);
				NativeMethods.DeleteObject(zero);
				zero = IntPtr.Zero;
				zero = bitmap.GetHbitmap();
				BlitToTarget(windowDC, zero, rect.X, rect.Y, rect.Width, rect.Height);
				Move();
			}
			catch (Exception ex) when (ex is ExternalException || ex is ArgumentException || ex is InvalidOperationException)
			{
				// The drag arrows are cosmetic; a failed screen capture only hides them.
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					NativeMethods.DeleteObject(zero);
				}
				NativeMethods.ReleaseDC(intPtr, windowDC);
				if (graphics != null)
				{
					graphics.Dispose();
					graphics = null;
				}
				if (bitmap != null)
				{
					bitmap.Dispose();
					bitmap = null;
				}
			}
		}

		private IntPtr CaptureScreenRegion(IntPtr otherIntPtr, int value, int otherValue, int nextValue, int previousValue)
		{
			IntPtr zero = IntPtr.Zero;
			_ = IntPtr.Zero;
			zero = NativeMethods.CreateCompatibleBitmap(otherIntPtr, nextValue, previousValue);
			IntPtr intPtr = NativeMethods.CreateCompatibleDC(otherIntPtr);
			IntPtr selectObject = NativeMethods.SelectObject(intPtr, zero);
			NativeMethods.BitBlt(intPtr, 0, 0, nextValue, previousValue, otherIntPtr, value, otherValue, WindowMessages.SourceCopy);
			NativeMethods.SelectObject(intPtr, selectObject);
			NativeMethods.DeleteDC(intPtr);
			return zero;
		}

		public override void Dispose()
		{
			if (_gridRenderer23Bitmap != null)
			{
				_gridRenderer23Bitmap.Dispose();
				_gridRenderer23Bitmap = null;
			}
			if (_headerBitmapHandle != IntPtr.Zero)
			{
				NativeMethods.DeleteObject(_headerBitmapHandle);
				_headerBitmapHandle = IntPtr.Zero;
			}
			if (_headerHbitmap != IntPtr.Zero)
			{
				NativeMethods.DeleteObject(_headerHbitmap);
				_headerHbitmap = IntPtr.Zero;
			}
			if (_scratchBitmapHandle != IntPtr.Zero)
			{
				NativeMethods.DeleteObject(_scratchBitmapHandle);
				_scratchBitmapHandle = IntPtr.Zero;
			}
			if (_gridRenderer23IntPtr != IntPtr.Zero)
			{
				NativeMethods.DeleteObject(_gridRenderer23IntPtr);
				_gridRenderer23IntPtr = IntPtr.Zero;
			}
		}
	}
}
