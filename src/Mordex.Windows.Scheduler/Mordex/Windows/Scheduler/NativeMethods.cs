using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Mordex.Windows.Scheduler
{
	[SuppressUnmanagedCodeSecurity]
	internal class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct NativeBlendFunction
		{
			public byte _byteValue;

			public byte _nativeMethodsDataByte;

			public byte SourceConstantAlpha;

			public byte AlphaFormat;
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

		internal struct NativeWindowRect
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

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

		[DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
		public static extern IntPtr GetDeviceCaps(IntPtr intPtr, int index);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr intPtr, IntPtr intPtrArgument, int x, int integerArgument, int width, int height, int flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDC")]
		public static extern IntPtr GetDC(IntPtr intPtr);

		[DllImport("user32", EntryPoint = "ReleaseDC")]
		public static extern int ReleaseDC(IntPtr intPtr, IntPtr intPtrArgument);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowWindow")]
		public static extern int ShowWindow(IntPtr intPtr, short shortInteger);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "UpdateLayeredWindow")]
		public static extern bool UpdateLayeredWindow(IntPtr intPtr, IntPtr intPtrArgument, ref NativePoint destinationPoint, ref NativeSize windowSize, IntPtr sourceDc, ref NativePoint nativeMethodsData3Argument, int colorKey, ref NativeBlendFunction nativeMethodsData, int integerArgument);

		[DllImport("gdi32", EntryPoint = "DeleteDC")]
		public static extern bool DeleteDC(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "BeginPaint")]
		public static extern IntPtr BeginPaint(IntPtr intPtr, ref NativePaintStruct paintStruct);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetWindowRect")]
		public static extern bool GetWindowRect(IntPtr intPtr, ref NativeWindowRect windowRect);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "EndPaint")]
		public static extern bool EndPaint(IntPtr intPtr, ref NativePaintStruct paintStruct);

		[DllImport("user32", EntryPoint = "SetParent")]
		public static extern IntPtr SetParent(IntPtr intPtr, IntPtr intPtrArgument);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowCaret")]
		public static extern bool ShowCaret(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "LoadCursor")]
		internal static extern IntPtr LoadCursor(IntPtr intPtr, uint unsignedInteger);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetCursor")]
		public static extern IntPtr SetCursor(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "HideCaret")]
		public static extern bool HideCaret(IntPtr intPtr);


		[DllImport("uxtheme.dll", EntryPoint = "GetThemeAppProperties")]
		public static extern int GetThemeAppProperties();



		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeBackground")]
		public static extern IntPtr DrawThemeBackground(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect bounds, NativeRect schedulerState7Argument);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "DrawThemeText")]
		public static extern IntPtr DrawThemeText(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, string text, int charCount, int textFlags, int extraTextFlags, NativeRect bounds);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeEdge")]
		public static extern IntPtr DrawThemeEdge(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect destinationRect, int edge, int flags, NativeRect schedulerState7Argument);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr intPtr, IntPtr intPtrArgument);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr intPtr);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeBackgroundContentRect")]
		public static extern IntPtr GetThemeBackgroundContentRect(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect boundingRect, NativeRect schedulerState7Argument);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeIcon")]
		public static extern IntPtr DrawThemeIcon(IntPtr intPtr, IntPtr intPtrArgument, int partId, int integerArgument, NativeRect bounds, IntPtr imageList, int imageIndex);



	}
}
