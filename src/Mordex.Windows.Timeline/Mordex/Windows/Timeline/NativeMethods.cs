using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Mordex.Windows.Timeline
{
	[SuppressUnmanagedCodeSecurity]
	internal class NativeMethods
	{
		internal struct NativePaintStruct
		{
			public IntPtr _intPtr;

			public int Erase;

			public Rectangle PaintRect;

			public int Restore;

			public int IncUpdate;

			public int ReservedA;

			public int ReservedB;

			public int ReservedC;

			public int ReservedD;

			public int ReservedE;

			public int ReservedF;

			public int ReservedG;

			public int ReservedH;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct NativeBlendFunction
		{
			public byte _byteValue;

			public byte _nativeMethodsData2Byte;

			public byte SourceConstantAlpha;

			public byte AlphaFormat;
		}

		internal struct NativeWindowRect
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class NativeRect
		{
			public int _left;

			public int _top;

			public int _right;

			public int _bottom;

			internal NativeRect(int left, int top, int right, int bottom)
			{
				_left = left;
				_top = top;
				_right = right;
				_bottom = bottom;
			}

			internal NativeRect()
			{
			}
		}

		internal struct NativeSize
		{
			public int Width;

			public int Height;
		}

		internal struct NativePoint
		{
			public int X;

			public int Y;
		}

		[DllImport("gdi32", EntryPoint = "DeleteDC")]
		public static extern bool DeleteDC(IntPtr intPtr);

		[DllImport("gdi32", EntryPoint = "BitBlt")]
		public static extern bool BitBlt(IntPtr intPtr, int destinationX, int integerArgument, int width, int height, IntPtr intPtrArgument, int sourceX, int sourceY, int rasterOperation);

		[DllImport("user32", EntryPoint = "SetParent")]
		public static extern IntPtr SetParent(IntPtr intPtr, IntPtr intPtrArgument);

		[DllImport("user32", EntryPoint = "GetWindowDC")]
		public static extern IntPtr GetWindowDC(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "BeginPaint")]
		public static extern IntPtr BeginPaint(IntPtr intPtr, ref NativePaintStruct nativeMethodsData);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "EndPaint")]
		public static extern bool EndPaint(IntPtr intPtr, ref NativePaintStruct nativeMethodsData);

		[DllImport("user32", EntryPoint = "GetDesktopWindow")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32", EntryPoint = "ReleaseDC")]
		public static extern int ReleaseDC(IntPtr intPtr, IntPtr intPtrArgument);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDC")]
		public static extern IntPtr GetDC(IntPtr intPtr);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowWindow")]
		public static extern int ShowWindow(IntPtr intPtr, short shortInteger);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowCaret")]
		public static extern bool ShowCaret(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "LoadCursor")]
		internal static extern IntPtr LoadCursor(IntPtr intPtr, uint unsignedInteger);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetCursor")]
		public static extern IntPtr SetCursor(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "HideCaret")]
		public static extern bool HideCaret(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "UpdateLayeredWindow")]
		public static extern bool UpdateLayeredWindow(IntPtr intPtr, IntPtr intPtrArgument, ref NativePoint destinationPoint, ref NativeSize windowSize, IntPtr sourceDc, ref NativePoint nativeMethodsData5Argument, int colorKey, ref NativeBlendFunction blendFunction, int integerArgument);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr intPtr, IntPtr intPtrArgument, int x, int integerArgument, int width, int height, int flags);

		[DllImport("user32", EntryPoint = "DrawFocusRect")]
		public static extern int DrawFocusRect(IntPtr intPtr, NativeRect rect);

		[DllImport("user32", EntryPoint = "GetWindowRect")]
		public static extern int GetWindowRect(IntPtr intPtr, ref NativeWindowRect windowRect);

		[DllImport("user32", EntryPoint = "SendMessage")]
		public static extern int SendMessage(IntPtr intPtr, int message, IntPtr intPtrArgument, IntPtr lParam);

		[DllImport("uxtheme.dll", EntryPoint = "IsAppThemed")]
		public static extern bool IsAppThemed();

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeAppProperties")]
		public static extern int GetThemeAppProperties();

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenThemeData", ExactSpelling = true)]
		public static extern IntPtr OpenThemeData(IntPtr intPtr, string text);

		[DllImport("uxtheme.dll", EntryPoint = "CloseThemeData")]
		public static extern IntPtr CloseThemeData(ref IntPtr intPtr);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeBackground")]
		public static extern IntPtr DrawThemeBackground(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect bounds, NativeRect timelineState6Argument);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "DrawThemeText")]
		public static extern IntPtr DrawThemeText(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, string text, int charCount, int textFlags, int extraTextFlags, NativeRect bounds);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeEdge")]
		public static extern IntPtr DrawThemeEdge(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect destinationRect, int edge, int flags, NativeRect timelineState6Argument);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr intPtr, IntPtr intPtrArgument);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr intPtr);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeBackgroundContentRect")]
		public static extern IntPtr GetThemeBackgroundContentRect(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect boundingRect, NativeRect timelineState6Argument);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeIcon")]
		public static extern IntPtr DrawThemeIcon(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect bounds, IntPtr imageList, int imageIndex);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeColor")]
		public static extern IntPtr GetThemeColor(IntPtr intPtr, int partId, int integerArgument, int propertyId, ref int color);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeSysColor")]
		public static extern int GetThemeSysColor(IntPtr intPtr, int colorId);

		[DllImport("uxtheme.dll", EntryPoint = "IsThemeActive")]
		public static extern bool IsThemeActive();
	}
}
