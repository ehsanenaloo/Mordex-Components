using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	internal partial class ColorSchemeWindowMessageHandler : NativeWindow
	{
		private const int WmPaint = 15;

		private const int SwShowNoActivate = 4;

		private const int WsPopup = int.MinValue;

		private const int WsExTopmostToolWindow = 0x88;

		private const int WsExLayered = 0x80000;

		private const int UlwAlpha = 2;

		private const int ShadowSize = 4;

		private const int ShadowGradientWidth = 5;

		private const int ShadowStartOffset = 8;

		private const int BitsPerPixel = 32;

		private const int BytesPerPixel = 4;

		private const int CornerSizeLarge = 24;

		private const int CornerLengthLarge = 92;
		internal bool _bottomCornerOnRight;

		internal bool _drawBottomShadow = true;

		internal bool _drawRightShadow = true;

		private bool _useThemedShadow;


		internal bool _drawRightShadowTopCorner = true;

		internal bool _drawBothBottomCorners = true;

		internal Rectangle _cutoutBounds = Rectangle.Empty;

		internal Rectangle _windowBounds = Rectangle.Empty;

		private Size _surfaceSize;

		private IntPtr _surfaceDeviceContext;

		private IntPtr _surfaceBitmap;

		private IntPtr _surfaceOldBitmap;

		// Wraps the pixels of _surfaceBitmap, so drawing into it fills the layered window bitmap directly.
		private Bitmap _surface;

		private string _surfaceKey;

		public void CreateShadowWindow(Rectangle bounds, bool useThemedShadow)
		{
			CreateParams createParams = new CreateParams();
			_useThemedShadow = useThemedShadow;
			_windowBounds = bounds;
			_surfaceKey = null;
			createParams.X = bounds.X;
			createParams.Y = bounds.Y;
			createParams.Height = bounds.Height;
			createParams.Width = bounds.Width;
			createParams.Parent = IntPtr.Zero;
			createParams.Style = WsPopup;
			createParams.ExStyle = WsExTopmostToolWindow;
			createParams.ExStyle += WsExLayered;
			CreateHandle(createParams);
			NativeMethods.ShowWindow(base.Handle, SwShowNoActivate);
			UpdateLayeredSurface();
		}

		private void UpdateLayeredSurface()
		{
			Rectangle bounds = _windowBounds;
			if (base.Handle == IntPtr.Zero || !EnsureSurface(bounds.Size))
			{
				return;
			}
			// The style and color scheme change only in ProcessRectangle, which clears the key.
			string key = string.Join(",", _cutoutBounds, _drawBottomShadow, _drawRightShadow, _drawRightShadowTopCorner, _drawBothBottomCorners, _bottomCornerOnRight, ThemedShadowColor().ToArgb());
			if (key != _surfaceKey)
			{
				using (Graphics graphics = Graphics.FromImage(_surface))
				{
					graphics.Clear(Color.Transparent);
					Rectangle rectangle = new Rectangle(Point.Empty, bounds.Size);
					if (!_cutoutBounds.IsEmpty)
					{
						graphics.ExcludeClip(_cutoutBounds);
					}
					if (_drawBottomShadow)
					{
						DrawBottomShadow(graphics, rectangle);
					}
					if (_drawRightShadow)
					{
						DrawRightShadow(graphics, rectangle);
					}
				}
				_surfaceKey = key;
			}
			NativeMethods.NativeSize b = default(NativeMethods.NativeSize);
			b._width = bounds.Width;
			b._height = bounds.Height;
			NativeMethods.NativePoint k = default(NativeMethods.NativePoint);
			k._x = bounds.X;
			k._y = bounds.Y;
			NativeMethods.NativePoint sourcePoint = default(NativeMethods.NativePoint);
			NativeMethods.NativeBlendFunction g = new NativeMethods.NativeBlendFunction
			{
				_blendOp = 0,
				_sourceConstantAlpha = byte.MaxValue,
				_blendFlags = 0,
				_alphaFormat = 1
			};
			NativeMethods.UpdateLayeredWindow(base.Handle, IntPtr.Zero, ref k, ref b, _surfaceDeviceContext, ref sourcePoint, 0, ref g, UlwAlpha);
		}

		private bool EnsureSurface(Size size)
		{
			if (_surface != null && _surfaceSize == size)
			{
				return true;
			}
			ReleaseSurface();
			if (size.Width <= 0 || size.Height <= 0)
			{
				return false;
			}
			_surfaceDeviceContext = NativeMethods.CreateCompatibleDC(IntPtr.Zero);
			if (_surfaceDeviceContext == IntPtr.Zero)
			{
				return false;
			}
			// A negative height makes a top-down bitmap, which matches the row order GDI+ uses.
			NativeMethods.BitmapInfoHeader header = new NativeMethods.BitmapInfoHeader
			{
				StructSize = Marshal.SizeOf<NativeMethods.BitmapInfoHeader>(),
				Width = size.Width,
				Height = -size.Height,
				Planes = 1,
				BitCount = BitsPerPixel
			};
			_surfaceBitmap = NativeMethods.CreateDIBSection(_surfaceDeviceContext, ref header, 0u, out IntPtr bits, IntPtr.Zero, 0u);
			if (_surfaceBitmap == IntPtr.Zero || bits == IntPtr.Zero)
			{
				ReleaseSurface();
				return false;
			}
			_surfaceOldBitmap = NativeMethods.SelectObject(_surfaceDeviceContext, _surfaceBitmap);
			_surface = new Bitmap(size.Width, size.Height, size.Width * BytesPerPixel, PixelFormat.Format32bppPArgb, bits);
			_surfaceSize = size;
			_surfaceKey = null;
			return true;
		}

		private void ReleaseSurface()
		{
			if (_surface != null)
			{
				_surface.Dispose();
				_surface = null;
			}
			if (_surfaceDeviceContext != IntPtr.Zero)
			{
				if (_surfaceOldBitmap != IntPtr.Zero)
				{
					NativeMethods.SelectObject(_surfaceDeviceContext, _surfaceOldBitmap);
				}
				NativeMethods.DeleteDC(_surfaceDeviceContext);
			}
			if (_surfaceBitmap != IntPtr.Zero)
			{
				NativeMethods.DeleteObject(_surfaceBitmap);
			}
			_surfaceDeviceContext = IntPtr.Zero;
			_surfaceBitmap = IntPtr.Zero;
			_surfaceOldBitmap = IntPtr.Zero;
			_surfaceSize = Size.Empty;
			_surfaceKey = null;
		}

		protected override void OnHandleChange()
		{
			base.OnHandleChange();
			if (base.Handle == IntPtr.Zero)
			{
				ReleaseSurface();
			}
		}

		protected void ProcessWindowMessage(ref Message message)
		{
			NativeMethods.NativePaintStruct h = default(NativeMethods.NativePaintStruct);
			IntPtr intPtr = NativeMethods.BeginPaint(message.HWnd, ref h);
			try
			{
				NativeMethods.NativeRect i = default(NativeMethods.NativeRect);
				NativeMethods.GetWindowRect(message.HWnd, ref i);
				Rectangle rectangle = new Rectangle(0, 0, i._right - i._left, i._bottom - i._top);
				using (Graphics graphics = RenderingHelper.CreateGraphicsFromHdc(intPtr))
				{
					if (_drawBottomShadow)
					{
						DrawBottomShadow(graphics, rectangle);
					}
					if (_drawRightShadow)
					{
						DrawRightShadow(graphics, rectangle);
					}
				}
			}
			finally
			{
				NativeMethods.EndPaint(message.HWnd, ref h);
			}
		}

		private void DrawRightShadow(Graphics graphics, Rectangle rectangle)
		{
			DrawThemedRightShadow(graphics, rectangle);
		}

		// Draws the shadow again after a flag or the cut-out changed.
		internal void Redraw()
		{
			UpdateLayeredSurface();
		}

		internal void SetWindowBounds(Rectangle bounds)
		{
			_windowBounds = bounds;
			UpdateLayeredSurface();
		}

		private void DrawBottomShadow(Graphics graphics, Rectangle rectangle)
		{
			DrawThemedBottomShadow(graphics, rectangle);
		}

		protected override void WndProc(ref Message message)
		{
			int msg = message.Msg;
			if (msg == WmPaint)
			{
				ProcessWindowMessage(ref message);
			}
			else
			{
				base.WndProc(ref message);
			}
		}
	}
}
