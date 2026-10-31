using System;
using System.Runtime.InteropServices;

namespace Mordex.Windows.Common
{
	internal static class NativeMethods
	{
		internal const int SwHide = 0;

		internal const int SwShowNoActivate = 4;

		internal const int BitsPerPixelCaps = 12;

		internal const uint IdcArrow = 32512u;

		// SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOOWNERZORDER
		internal const uint SetWindowPosKeepSizeNoActivate = 0x211u;

		internal const int UlwAlpha = 2;

		internal const int WsPopup = unchecked((int)0x80000000);

		internal const int WsExTopMost = 0x8;

		internal const int WsExToolWindow = 0x80;

		internal const int WsExLayered = 0x80000;

		internal const int HtTransparent = -1;

		internal const int MaNoActivate = 3;

		internal const int WmPaint = 0x000F;

		internal const int WmActivate = 0x0006;

		internal const int WmActivateApp = 0x001C;

		internal const int WmSetCursor = 0x0020;

		internal const int WmMouseActivate = 0x0021;

		internal const int WmNcHitTest = 0x0084;

		internal const int WmHideSuperTip = 0x0401;

		[StructLayout(LayoutKind.Sequential)]
		internal struct Point
		{
			public int X;

			public int Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct Size
		{
			public int Width;

			public int Height;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct Rect
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct BlendFunction
		{
			public byte BlendOp;

			public byte BlendFlags;

			public byte SourceConstantAlpha;

			public byte AlphaFormat;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct PaintStruct
		{
			public IntPtr Hdc;

			public int Erase;

			public Rect PaintRect;

			public int Restore;

			public int IncUpdate;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] Reserved;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

		[DllImport("user32.dll")]
		internal static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BlendFunction pblend, int dwFlags);

		[DllImport("user32.dll")]
		internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

		[DllImport("gdi32.dll")]
		internal static extern int GetDeviceCaps(IntPtr hdc, int index);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ShowWindow(IntPtr hWnd, int command);

		[DllImport("user32.dll")]
		internal static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("user32.dll")]
		internal static extern IntPtr LoadCursor(IntPtr hInstance, uint cursorName);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll")]
		internal static extern IntPtr BeginPaint(IntPtr hWnd, out PaintStruct paint);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool EndPaint(IntPtr hWnd, ref PaintStruct paint);

		[DllImport("user32.dll")]
		internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
	}
}
