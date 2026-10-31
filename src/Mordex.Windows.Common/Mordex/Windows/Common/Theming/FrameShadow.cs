using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// The soft shadow around a window with a custom frame. Windows draws its shadow only while it draws the frame itself, and
	/// the custom frame turns that drawing off, so four layered windows (one per side) lie just behind the window and show the
	/// shadow. They take no clicks and no focus, are not in the task bar, and are hidden while the window is hidden, minimized
	/// or maximized.
	/// </summary>
	internal sealed class FrameShadow : IDisposable
	{
		// The width of the shadow and the darkness at the edge of the window, at 96 DPI.
		internal const int Spread = 12;
		internal const int EdgeAlpha = 56;

		private const int WsPopup = unchecked((int)0x80000000);
		private const int WsExLayered = 0x00080000;
		private const int WsExTransparent = 0x00000020;
		private const int WsExToolWindow = 0x00000080;
		private const int WsExNoActivate = 0x08000000;
		private const uint SwpNoSize = 0x0001;
		private const uint SwpNoMove = 0x0002;
		private const uint SwpNoActivate = 0x0010;
		private const uint SwpShowWindow = 0x0040;
		private const uint SwpHideWindow = 0x0080;
		private const uint SwpNoOwnerZOrder = 0x0200;

		[DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int cx, int cy, uint flags);
		[DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr window);

		private readonly Side[] _sides = new Side[4];
		private Rectangle _window;
		private int _dpi;
		private bool _shown;

		/// <summary>True while the shadow is shown.</summary>
		internal bool IsShown => _shown;

		/// <summary>The shadow windows, for tests.</summary>
		internal IntPtr[] Handles
		{
			get
			{
				IntPtr[] handles = new IntPtr[_sides.Length];
				for (int i = 0; i < _sides.Length; i++)
				{
					handles[i] = _sides[i]?.Handle ?? IntPtr.Zero;
				}
				return handles;
			}
		}

		/// <summary>Places the shadow around the window and just behind it, or hides it when the window shows no shadow now.</summary>
		internal void Update(IntPtr window)
		{
			bool show = window != IntPtr.Zero && IsWindowVisible(window) && !FrameNative.IsIconic(window) && !FrameNative.IsZoomed(window);
			if (!show)
			{
				Hide();
				return;
			}
			Rectangle bounds = FrameNative.GetWindowRectangle(window);
			int dpi = FrameNative.GetDpi(window);
			bool resized = bounds.Size != _window.Size || dpi != _dpi;
			_window = bounds;
			_dpi = dpi;
			int spread = Math.Max(1, (int)Math.Round(Spread * dpi / 96f));
			Rectangle outer = Rectangle.Inflate(bounds, spread, spread);
			Rectangle[] parts =
			{
				new Rectangle(outer.X, outer.Y, outer.Width, spread),
				new Rectangle(outer.X, bounds.Bottom, outer.Width, spread),
				new Rectangle(outer.X, bounds.Y, spread, bounds.Height),
				new Rectangle(bounds.Right, bounds.Y, spread, bounds.Height)
			};
			for (int i = 0; i < parts.Length; i++)
			{
				Side side = _sides[i] ?? (_sides[i] = new Side());
				side.EnsureHandle();
				if (resized || !side.Painted)
				{
					side.Paint(parts[i], bounds, spread);
				}
				// Just behind the window in the z order, so the window covers the inner edge of the shadow.
				SetWindowPos(side.Handle, window, parts[i].X, parts[i].Y, 0, 0, SwpNoSize | SwpNoActivate | SwpNoOwnerZOrder | SwpShowWindow);
			}
			_shown = true;
		}

		internal void Hide()
		{
			if (!_shown)
			{
				return;
			}
			_shown = false;
			foreach (Side side in _sides)
			{
				if (side != null && side.Handle != IntPtr.Zero)
				{
					SetWindowPos(side.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpNoSize | SwpNoMove | SwpNoActivate | SwpHideWindow | 0x0004);
				}
			}
		}

		public void Dispose()
		{
			_shown = false;
			for (int i = 0; i < _sides.Length; i++)
			{
				_sides[i]?.DestroyHandle();
				_sides[i] = null;
			}
		}

		// The alpha of the shadow at a point outside the window: dark at the edge, fading to nothing at the spread.
		internal static byte AlphaAt(Rectangle window, int spread, int x, int y)
		{
			int dx = x < window.X ? window.X - x : x >= window.Right ? x - window.Right + 1 : 0;
			int dy = y < window.Y ? window.Y - y : y >= window.Bottom ? y - window.Bottom + 1 : 0;
			if (dx == 0 && dy == 0)
			{
				return 0;
			}
			double distance = Math.Sqrt(dx * dx + dy * dy);
			if (distance >= spread)
			{
				return 0;
			}
			double t = 1 - distance / spread;
			return (byte)Math.Round(EdgeAlpha * t * t);
		}

		private sealed class Side : NativeWindow
		{
			internal bool Painted { get; private set; }

			internal void EnsureHandle()
			{
				if (Handle != IntPtr.Zero)
				{
					return;
				}
				CreateHandle(new CreateParams
				{
					Style = WsPopup,
					ExStyle = WsExLayered | WsExTransparent | WsExToolWindow | WsExNoActivate,
					Caption = string.Empty
				});
				Painted = false;
			}

			// Draws the part of the shadow that this window holds, in screen coordinates.
			internal void Paint(Rectangle part, Rectangle window, int spread)
			{
				if (part.Width <= 0 || part.Height <= 0)
				{
					return;
				}
				int[] pixels = new int[part.Width * part.Height];
				for (int y = 0; y < part.Height; y++)
				{
					for (int x = 0; x < part.Width; x++)
					{
						// Black with alpha: the premultiplied color is black as well.
						pixels[y * part.Width + x] = AlphaAt(window, spread, part.X + x, part.Y + y) << 24;
					}
				}
				using (Bitmap bitmap = new Bitmap(part.Width, part.Height, PixelFormat.Format32bppArgb))
				{
					BitmapData data = bitmap.LockBits(new Rectangle(0, 0, part.Width, part.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					try
					{
						for (int y = 0; y < part.Height; y++)
						{
							Marshal.Copy(pixels, y * part.Width, data.Scan0 + y * data.Stride, part.Width);
						}
					}
					finally
					{
						bitmap.UnlockBits(data);
					}
					IntPtr screen = NativeMethods.GetDC(IntPtr.Zero);
					IntPtr memory = NativeMethods.CreateCompatibleDC(screen);
					IntPtr hbitmap = IntPtr.Zero;
					IntPtr previous = IntPtr.Zero;
					try
					{
						hbitmap = bitmap.GetHbitmap(Color.FromArgb(0));
						previous = NativeMethods.SelectObject(memory, hbitmap);
						NativeMethods.Point position = new NativeMethods.Point { X = part.X, Y = part.Y };
						NativeMethods.Point source = default;
						NativeMethods.Size size = new NativeMethods.Size { Width = part.Width, Height = part.Height };
						NativeMethods.BlendFunction blend = new NativeMethods.BlendFunction { SourceConstantAlpha = 255, AlphaFormat = 1 };
						NativeMethods.UpdateLayeredWindow(Handle, screen, ref position, ref size, memory, ref source, 0, ref blend, NativeMethods.UlwAlpha);
						Painted = true;
					}
					finally
					{
						if (previous != IntPtr.Zero)
						{
							NativeMethods.SelectObject(memory, previous);
						}
						if (hbitmap != IntPtr.Zero)
						{
							NativeMethods.DeleteObject(hbitmap);
						}
						NativeMethods.DeleteDC(memory);
						NativeMethods.ReleaseDC(IntPtr.Zero, screen);
					}
				}
			}

			protected override void WndProc(ref Message m)
			{
				const int WmNcHitTest = 0x0084;
				const int WmMouseActivate = 0x0021;
				if (m.Msg == WmNcHitTest)
				{
					m.Result = new IntPtr(-1);
					return;
				}
				if (m.Msg == WmMouseActivate)
				{
					m.Result = new IntPtr(3);
					return;
				}
				base.WndProc(ref m);
			}
		}
	}
}
