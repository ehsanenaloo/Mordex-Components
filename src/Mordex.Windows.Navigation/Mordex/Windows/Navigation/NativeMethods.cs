using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Mordex.Windows.Navigation
{
	[SuppressUnmanagedCodeSecurity]
	internal class NativeMethods
	{
		internal struct NativePoint
		{
			public int _x;

			public int _y;
		}

		internal struct NativeSize
		{
			public int _width;

			public int _height;
		}

		internal struct NativeWindowRect
		{
			public int _left;

			public int _top;

			public int _right;

			public int _bottom;
		}

		internal struct NativePaintStruct
		{
			public IntPtr _hdc;

			public int _erase;

			public Rectangle _paintRect;

			public int _restore;

			public int _incUpdate;

			public int _reserved1;

			public int _reserved2;

			public int _reserved3;

			public int _reserved4;

			public int _reserved5;

			public int _reserved6;

			public int _reserved7;

			public int _reserved8;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct NativeBlendFunction
		{
			public byte _blendOp;

			public byte _blendFlags;

			public byte _sourceConstantAlpha;

			public byte _alphaFormat;
		}

		internal struct NativeIconInfo
		{
			public bool _isIcon;

			public int _hotspotX;

			public int _hotspotY;

			public IntPtr _maskBitmap;

			public IntPtr _colorBitmap;
		}

		[StructLayout(LayoutKind.Sequential)]
		public class NativeRect
		{
			public int _left;

			public int _top;

			public int _right;

			public int _bottom;

			internal static NativeRect FromRectangle(Rectangle bounds)
			{
				return new NativeRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			}

			internal static NativeRect FromRectangleF(RectangleF rectangleF)
			{
				return new NativeRect((int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Right, (int)rectangleF.Bottom);
			}

			internal NativeRect(int left, int top, int right, int bottom)
			{
				_left = left;
				_top = top;
				_right = right;
				_bottom = bottom;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class NativePointClass
		{
			public int _x;

			public int _y;
		}

		internal NativeMethods()
		{
		}

		[DllImport("user32", EntryPoint = "InvertRect")]
		public static extern int InvertRect(IntPtr hdc, NativeRect rect);

		[DllImport("uxtheme.dll", EntryPoint = "IsAppThemed")]
		public static extern bool IsAppThemed();

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeAppProperties")]
		public static extern int GetThemeAppProperties();

		[DllImport("user32", EntryPoint = "GetWindowDC")]
		public static extern IntPtr GetWindowDC(IntPtr hwnd);

		[DllImport("user32", EntryPoint = "GetDesktopWindow")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32", EntryPoint = "ReleaseDC")]
		public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

		[DllImport("gdi32", EntryPoint = "BitBlt")]
		public static extern bool BitBlt(IntPtr destDc, int destX, int destY, int width, int height, IntPtr srcDc, int srcX, int srcY, int rasterOp);

		[DllImport("user32.dll", EntryPoint = "GetIconInfo")]
		public static extern bool GetIconInfo(IntPtr icon, out NativeIconInfo iconInfo);

		[DllImport("user32", EntryPoint = "DrawState")]
		public static extern int DrawState(IntPtr hdc, IntPtr foregroundBrush, int callback, IntPtr userData, int wordData, int x, int y, int width, int height, int flags);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern bool DeleteObject(IntPtr gdiObject);

		[DllImport("user32", EntryPoint = "DestroyIcon")]
		public static extern int DestroyIcon(IntPtr icon);

		[DllImport("gdi32", EntryPoint = "DeleteDC")]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("user32", EntryPoint = "SetParent")]
		public static extern IntPtr SetParent(IntPtr childWindow, IntPtr newParent);

		[DllImport("user32", EntryPoint = "GetWindowRect")]
		public static extern int GetWindowRect(IntPtr hwnd, ref NativeWindowRect rect);

		[DllImport("user32", EntryPoint = "SendMessage")]
		public static extern int SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenThemeData", ExactSpelling = true)]
		public static extern IntPtr OpenThemeData(IntPtr hwnd, string classList);

		[DllImport("uxtheme.dll", EntryPoint = "CloseThemeData")]
		public static extern int CloseThemeData(IntPtr theme);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeBackground")]
		public static extern IntPtr DrawThemeBackground(IntPtr theme, IntPtr hdc, int partId, int stateId, NativeRect rect, NativeRect clipRect);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "DrawThemeText")]
		public static extern IntPtr DrawThemeText(IntPtr theme, IntPtr hdc, int partId, int stateId, string text, int charCount, int textFlags, int textFlags2, NativeRect rect);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeEdge")]
		public static extern IntPtr DrawThemeEdge(IntPtr theme, IntPtr hdc, int partId, int stateId, NativeRect destRect, int edges, int flags, NativeRect contentRect);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr gdiObject);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeBackgroundContentRect")]
		public static extern IntPtr GetThemeBackgroundContentRect(IntPtr theme, IntPtr hdc, int partId, int stateId, NativeRect boundingRect, NativeRect contentRect);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeIcon")]
		public static extern IntPtr DrawThemeIcon(IntPtr theme, IntPtr hdc, int partId, int stateId, NativeRect rect, IntPtr imageList, int imageIndex);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeColor")]
		public static extern IntPtr GetThemeColor(IntPtr theme, int partId, int stateId, int propId, ref int color);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeSysColor")]
		public static extern int GetThemeSysColor(IntPtr theme, int colorId);

		[DllImport("uxtheme.dll", EntryPoint = "IsThemeActive")]
		public static extern bool IsThemeActive();

		[DllImport("gdi32.dll", EntryPoint = "CreateRectRgn")]
		public static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

		[DllImport("gdi32.dll", EntryPoint = "SelectClipRgn")]
		public static extern IntPtr SelectClipRgn(IntPtr hdc, IntPtr region);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDC")]
		public static extern IntPtr GetDC(IntPtr hwnd);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowWindow")]
		public static extern int ShowWindow(IntPtr hwnd, short showCommand);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetCursor")]
		public static extern IntPtr SetCursor(IntPtr cursor);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, int flags);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "LoadCursor")]
		internal static extern IntPtr LoadCursor(IntPtr instance, IntPtr cursorId);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "BeginPaint")]
		public static extern IntPtr BeginPaint(IntPtr hwnd, ref NativePaintStruct paintStruct);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "EndPaint")]
		public static extern bool EndPaint(IntPtr hwnd, ref NativePaintStruct paintStruct);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "UpdateLayeredWindow")]
		public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr destDc, ref NativePoint destinationPoint, ref NativeSize size, IntPtr sourceDc, ref NativePoint sourcePoint, int colorKey, ref NativeBlendFunction blend, int flags);
	}
}
