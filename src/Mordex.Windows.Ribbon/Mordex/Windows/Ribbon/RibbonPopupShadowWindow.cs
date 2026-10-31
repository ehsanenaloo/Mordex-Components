using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonPopupShadowWindow : NativeWindow
	{
		private const int UlwAlpha = 2;

		private Color _shadowColor;

		private Bitmap _bottomRightCornerBitmap;

		private Bitmap _bottomLeftCornerBitmap;

		private Bitmap _topRightCornerBitmap;

		internal Rectangle _excludedBounds = Rectangle.Empty;

		internal Rectangle _shadowBounds = Rectangle.Empty;

		public RibbonPopupShadowWindow(Color shadowColor)
		{
			_shadowColor = shadowColor;
		}

		public void ShowShadow(Rectangle ribbonWindowMessageHandler3Rectangle)
		{
			CreateParams createParams = new CreateParams();
			_shadowBounds = ribbonWindowMessageHandler3Rectangle;
			createParams.X = ribbonWindowMessageHandler3Rectangle.X;
			createParams.Y = ribbonWindowMessageHandler3Rectangle.Y;
			createParams.Height = ribbonWindowMessageHandler3Rectangle.Height;
			createParams.Width = ribbonWindowMessageHandler3Rectangle.Width;
			createParams.Parent = IntPtr.Zero;
			createParams.Style = int.MinValue;
			createParams.ExStyle = 136;
			createParams.ExStyle += 524288;
			CreateHandle(createParams);
			NativeMethods.ShowWindow(base.Handle, 4);
			RenderLayeredShadow();
		}

		private void RenderLayeredShadow()
		{
			using (Bitmap bitmap = new Bitmap(_shadowBounds.Width, _shadowBounds.Height, PixelFormat.Format32bppArgb))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.Clear(Color.Transparent);
					Rectangle rectangle = new Rectangle(0, 0, _shadowBounds.Width, _shadowBounds.Height);
					if (!_excludedBounds.IsEmpty)
					{
						graphics.ExcludeClip(_excludedBounds);
					}
					DrawShadow(graphics, rectangle);
				}
				IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
				IntPtr memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
				IntPtr hbitmap = bitmap.GetHbitmap(Color.FromArgb(0));
				IntPtr oldObject = NativeMethods.SelectObject(memoryDc, hbitmap);
				try
				{
					NativeMethods.NativeSize h = default(NativeMethods.NativeSize);
					h._width = _shadowBounds.Width;
					h._height = _shadowBounds.Height;
					NativeMethods.NativePoint m = default(NativeMethods.NativePoint);
					m._x = _shadowBounds.X;
					m._y = _shadowBounds.Y;
					NativeMethods.NativePoint sourceOrigin = default(NativeMethods.NativePoint);
					sourceOrigin._x = 0;
					sourceOrigin._y = 0;
					NativeMethods.BlendFunction g = new NativeMethods.BlendFunction
					{
						_blendOp = 0,
						_sourceConstantAlpha = byte.MaxValue,
						_blendFlags = 0,
						_alphaFormat = 1
					};
					NativeMethods.UpdateLayeredWindow(base.Handle, screenDc, ref m, ref h, memoryDc, ref sourceOrigin, 0, ref g, UlwAlpha);
				}
				finally
				{
					NativeMethods.SelectObject(memoryDc, oldObject);
					NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
					NativeMethods.DeleteObject(hbitmap);
					NativeMethods.DeleteDC(memoryDc);
				}
			}
		}

		internal void DisposeShadow()
		{
			if (base.Handle != IntPtr.Zero)
			{
				DestroyHandle();
			}
			_bottomRightCornerBitmap?.Dispose();
			_bottomRightCornerBitmap = null;
			_bottomLeftCornerBitmap?.Dispose();
			_bottomLeftCornerBitmap = null;
			_topRightCornerBitmap?.Dispose();
			_topRightCornerBitmap = null;
		}

		protected void PaintShadow(ref Message message)
		{
			NativeMethods.PaintStruct c = default(NativeMethods.PaintStruct);
			IntPtr paintDc = NativeMethods.BeginPaint(message.HWnd, ref c);
			try
			{
				NativeMethods.NativeRect i = default(NativeMethods.NativeRect);
				NativeMethods.GetWindowRect(message.HWnd, ref i);
				Rectangle rectangle = new Rectangle(0, 0, i._right - i._left, i._bottom - i._top);
				using (Graphics graphics = RibbonDrawingUtils.CreateGraphicsFromHdc(paintDc))
				using (Bitmap bitmap = new Bitmap(_shadowBounds.Width, _shadowBounds.Height))
				{
					using (Graphics bufferGraphics = Graphics.FromImage(bitmap))
					{
						DrawShadow(bufferGraphics, rectangle);
					}
					graphics.DrawImageUnscaled(bitmap, 0, 0);
				}
			}
			finally
			{
				NativeMethods.EndPaint(message.HWnd, ref c);
			}
		}

		private void DrawShadow(Graphics graphics, Rectangle windowBounds)
		{
			windowBounds.Width -= 4;
			windowBounds.Height -= 4;
			Pen pen = new Pen(Color.FromArgb(63, _shadowColor));
			Pen mediumPen = new Pen(Color.FromArgb(47, _shadowColor));
			Pen lightPen = new Pen(Color.FromArgb(19, _shadowColor));
			if (windowBounds.Height >= 5)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(windowBounds.Right, windowBounds.Top + 4, 4, 4), image: GetTopRightCornerImage());
				if (windowBounds.Height >= 8)
				{
					Rectangle rectangle = new Rectangle(windowBounds.Right - 1, windowBounds.Top + 8, 6, windowBounds.Height - 9);
					int lineX = windowBounds.Right;
					for (int i = 0; i < 4; i++)
					{
						Pen verticalLinePen;
						switch (i)
						{
						case 2:
							verticalLinePen = mediumPen;
							break;
						case 3:
							verticalLinePen = lightPen;
							break;
						default:
							verticalLinePen = pen;
							break;
						}
						graphics.DrawLine(verticalLinePen, lineX, windowBounds.Y + 8, lineX, windowBounds.Bottom - 1);
						lineX++;
					}
				}
			}
			if (windowBounds.Width >= 5)
			{
				graphics.DrawImageUnscaled(rect: new Rectangle(windowBounds.Left + 4, windowBounds.Bottom, 4, 4), image: GetBottomLeftCornerImage());
				int lineY = windowBounds.Bottom;
				for (int j = 0; j < 4; j++)
				{
					Pen horizontalLinePen;
					switch (j)
					{
					case 2:
						horizontalLinePen = mediumPen;
						break;
					case 3:
						horizontalLinePen = lightPen;
						break;
					default:
						horizontalLinePen = pen;
						break;
					}
					graphics.DrawLine(horizontalLinePen, windowBounds.X + 8, lineY, windowBounds.Right - 1, lineY);
					lineY++;
				}
			}
			mediumPen.Dispose();
			pen.Dispose();
			lightPen.Dispose();
			graphics.DrawImageUnscaled(rect: new Rectangle(windowBounds.Right, windowBounds.Bottom, 5, 5), image: GetBottomRightCornerImage());
		}

		private Image GetTopRightCornerImage()
		{
			if (_topRightCornerBitmap == null)
			{
				CreateTopRightCornerBitmap();
			}
			return _topRightCornerBitmap;
		}

		private Image GetBottomLeftCornerImage()
		{
			if (_bottomLeftCornerBitmap == null)
			{
				CreateBottomLeftCornerBitmap();
			}
			return _bottomLeftCornerBitmap;
		}

		private Image GetBottomRightCornerImage()
		{
			if (_bottomRightCornerBitmap == null)
			{
				CreateBottomRightCornerBitmap();
			}
			return _bottomRightCornerBitmap;
		}

		private void CreateBottomLeftCornerBitmap()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int maxAlpha = 63;
			for (int i = 0; i < 4; i++)
			{
				int columnAlpha = i * maxAlpha / 3;
				for (int j = 0; j < 4; j++)
				{
					int pixelAlpha = (4 - j) * columnAlpha / 4;
					int alpha = pixelAlpha;
					Color color = Color.FromArgb(alpha, _shadowColor);
					bitmap.SetPixel(i, j, color);
				}
			}
			_bottomLeftCornerBitmap = bitmap;
		}

		private void CreateTopRightCornerBitmap()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			int maxAlpha = 63;
			for (int i = 0; i < 4; i++)
			{
				int columnAlpha = i * maxAlpha / 3;
				for (int row = 3; row >= 0; row--)
				{
					int pixelAlpha = (4 - row) * columnAlpha / 4;
					int alpha = pixelAlpha;
					Color color = Color.FromArgb(alpha, _shadowColor);
					bitmap.SetPixel(row, i, color);
				}
			}
			_topRightCornerBitmap = bitmap;
		}

		private void CreateBottomRightCornerBitmap()
		{
			Bitmap bitmap = new Bitmap(4, 4);
			for (int i = 0; i < 4; i++)
			{
				int peakAlpha = 63;
				switch (i)
				{
				case 1:
					peakAlpha = 72;
					break;
				case 2:
					peakAlpha = 47;
					break;
				case 3:
					peakAlpha = 19;
					break;
				default:
					peakAlpha = 72;
					break;
				}
				for (int row = 3; row >= 0; row--)
				{
					int scaledAlpha = (4 - row) * peakAlpha / 4;
					int clampedAlpha = scaledAlpha;
					if (clampedAlpha > 63)
					{
						clampedAlpha = 63;
					}
					Color color = Color.FromArgb(clampedAlpha, _shadowColor);
					bitmap.SetPixel(row, i, color);
				}
			}
			_bottomRightCornerBitmap = bitmap;
		}

		internal void ResizeShadow(Rectangle ribbonWindowMessageHandler3Rectangle)
		{
			_shadowBounds = ribbonWindowMessageHandler3Rectangle;
			RenderLayeredShadow();
		}

		protected override void WndProc(ref Message message)
		{
			int msg = message.Msg;
			if (msg == 15)
			{
				PaintShadow(ref message);
			}
			else
			{
				base.WndProc(ref message);
			}
		}
	}
}
