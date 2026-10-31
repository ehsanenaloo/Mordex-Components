using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Mordex.Windows.DataGrid
{
	[SuppressUnmanagedCodeSecurity]
	internal class NativeMethods
	{
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
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct NativeMethodsData
		{
			public byte _byteValue;

			public byte _nativeMethodsDataByte;

			public byte _sourceConstantAlpha;

			public byte _alphaFormat;
		}

		internal struct NativePoint
		{
			public int _x;

			public int _nativeMethodsData2Integer;
		}

		internal struct NativeSize
		{
			public int _width;

			public int _nativeMethodsData3Integer;
		}

		internal struct NativeRectStruct
		{
			public int _left;

			public int _nativeMethodsData4Integer;

			public int _right;

			public int _bottom;
		}

		internal struct NativePaintStruct
		{
		}

		public struct NativeLogBrush
		{
			public uint _brushStyle;

			public uint _nativeMethodsData6UnsignedInteger;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "IsWindowVisible")]
		public static extern bool IsWindowVisible(IntPtr intPtr);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetProp")]
		internal static extern IntPtr GetProp(IntPtr intPtr, string text);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "SetProp")]
		internal static extern int SetProp(IntPtr intPtr, string text, IntPtr otherIntPtr);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "RemoveProp")]
		internal static extern IntPtr RemoveProp(IntPtr intPtr, string text);

		[DllImport("user32", EntryPoint = "ScrollDC")]
		public static extern bool ScrollDC(IntPtr intPtr, int value, int otherValue, ref NativeRectStruct nativeMethodsData, ref NativeRectStruct nativeMethodsData4Argument, IntPtr otherIntPtr, ref NativeRectStruct nativeMethodsData4Argument2);

		[DllImport("user32", EntryPoint = "ValidateRect")]
		internal static extern bool ValidateRect(IntPtr intPtr, ref NativeRectStruct nativeMethodsData);

		[DllImport("gdi32", EntryPoint = "DeleteDC")]
		internal static extern bool DeleteDC(IntPtr intPtr);

		[DllImport("gdi32", EntryPoint = "BitBlt")]
		internal static extern bool BitBlt(IntPtr intPtr, int value, int otherValue, int nextValue, int previousValue, IntPtr otherIntPtr, int firstValue, int secondValue, int thirdValue);

		[DllImport("GDI32.DLL", CharSet = CharSet.Auto, EntryPoint = "StretchBlt", ExactSpelling = true, SetLastError = true)]
		public static extern bool StretchBlt(IntPtr intPtr, int value, int otherValue, int nextValue, int previousValue, IntPtr otherIntPtr, int firstValue, int secondValue, int thirdValue, int fourthValue, int fifthValue);

		[DllImport("user32", EntryPoint = "SetParent")]
		internal static extern IntPtr SetParent(IntPtr intPtr, IntPtr otherIntPtr);

		[DllImport("user32", EntryPoint = "GetWindowDC")]
		internal static extern IntPtr GetWindowDC(IntPtr intPtr);

		[DllImport("user32", EntryPoint = "GetDesktopWindow")]
		internal static extern IntPtr GetDesktopWindow();

		[DllImport("user32", EntryPoint = "ReleaseDC")]
		internal static extern int ReleaseDC(IntPtr intPtr, IntPtr otherIntPtr);

		[DllImport("user32", EntryPoint = "GetWindowRect")]
		internal static extern int GetWindowRect(IntPtr intPtr, NativeRect state);

		[DllImport("user32", EntryPoint = "SendMessage")]
		internal static extern int SendMessage(IntPtr intPtr, int value, IntPtr otherIntPtr, IntPtr nextIntPtr);

		[DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
		internal static extern IntPtr CreateCompatibleBitmap(IntPtr intPtr, int value, int otherValue);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenThemeData", ExactSpelling = true)]
		internal static extern IntPtr OpenThemeData(IntPtr intPtr, string text);

		[DllImport("uxtheme.dll", EntryPoint = "CloseThemeData")]
		internal static extern IntPtr CloseThemeData(ref IntPtr intPtr);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeBackground")]
		internal static extern IntPtr DrawThemeBackground(IntPtr intPtr, IntPtr otherIntPtr, int value, int otherValue, NativeRect state, NativeRect gridState28Argument);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		internal static extern IntPtr SelectObject(IntPtr intPtr, IntPtr otherIntPtr);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		internal static extern IntPtr DeleteObject(IntPtr intPtr);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeColor")]
		internal static extern IntPtr GetThemeColor(IntPtr intPtr, int value, int otherValue, int nextValue, ref int previousValue);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeSysColor")]
		internal static extern int GetThemeSysColor(IntPtr intPtr, int value);

		[DllImport("Gdi32.dll", EntryPoint = "CreateBrushIndirect")]
		public static extern IntPtr CreateBrushIndirect(ref NativeLogBrush nativeMethodsData);

		[DllImport("Gdi32.dll", EntryPoint = "PatBlt")]
		public static extern bool PatBlt(IntPtr intPtr, int value, int otherValue, int nextValue, int previousValue, uint unsignedInteger);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		internal static extern IntPtr SetWindowPos(IntPtr intPtr, IntPtr otherIntPtr, int value, int otherValue, int nextValue, int previousValue, int firstValue);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetDC")]
		internal static extern IntPtr GetDC(IntPtr intPtr);

		[DllImport("Gdi32.dll", EntryPoint = "GetDeviceCaps")]
		public static extern IntPtr GetDeviceCaps(IntPtr intPtr, int value);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowWindow")]
		internal static extern int ShowWindow(IntPtr intPtr, int value);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "UpdateLayeredWindow")]
		internal static extern bool UpdateLayeredWindow(IntPtr intPtr, IntPtr otherIntPtr, ref NativeSize otherNativeMethodsData, ref NativePoint nextNativeMethodsData, IntPtr nextIntPtr, ref NativeSize nativeMethodsData3Argument, int value, ref NativeMethodsData nativeMethodsData, int otherValue);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "BeginPaint")]
		internal static extern IntPtr BeginPaint(IntPtr intPtr, ref NativePaintStruct nativeMethodsData);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetWindowRect")]
		internal static extern bool GetWindowRectStruct(IntPtr intPtr, ref NativeRectStruct nativeMethodsData);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "EndPaint")]
		internal static extern bool EndPaint(IntPtr intPtr, ref NativePaintStruct nativeMethodsData);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ShowCaret")]
		internal static extern bool ShowCaret(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "LoadCursor")]
		internal static extern IntPtr LoadCursor(IntPtr intPtr, uint unsignedInteger);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "CreateCompatibleDC")]
		internal static extern IntPtr CreateCompatibleDC(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetCursor")]
		internal static extern IntPtr SetCursor(IntPtr intPtr);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "HideCaret")]
		internal static extern bool HideCaret(IntPtr intPtr);
	}
}
