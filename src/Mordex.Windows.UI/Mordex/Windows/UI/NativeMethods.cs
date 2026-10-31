using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Mordex.Windows.UI
{
	[SuppressUnmanagedCodeSecurity]
	internal class NativeMethods
	{
		private const int LogFontFaceNameLength = 32;

		internal struct NativeSize
		{
			public int _width;

			public int _height;
		}

		internal struct NativeMsg
		{
			public IntPtr _hwnd;

			public int _message;

			public IntPtr _wParam;

			public IntPtr _lParam;

			public int _time;

			public int _pointX;

			public int _pointY;
		}

		internal struct NativeIconInfo
		{
			public bool _isIcon;

			public int _hotspotX;

			public int _hotspotY;

			public IntPtr _maskBitmap;

			public IntPtr _colorBitmap;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal class NativeLogFont
		{

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = LogFontFaceNameLength)]
			public string _faceName;
		}

		internal struct NativeLogBrush
		{
			public uint _style;

			public uint _color;

			public IntPtr _hatch;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct NativeBlendFunction
		{
			public byte _blendOp;

			public byte _blendFlags;

			public byte _sourceConstantAlpha;

			public byte _alphaFormat;
		}

		internal struct NativePaintStruct
		{
			public IntPtr _dc;

			public int _erase;

			public Rectangle _paintRect;

			public int _restore;

			public int _incUpdate;

			public int _reservedA;

			public int _reservedB;

			public int _reservedC;

			public int _reservedD;

			public int _reservedE;

			public int _reservedF;

			public int _reservedG;

			public int _reservedH;
		}

		internal struct NativeRect
		{
			public int _left;

			public int _top;

			public int _right;

			public int _bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class NativeRectClass
		{
			public int _left;

			public int _top;

			public int _right;

			public int _bottom;

			internal static NativeRectClass FromRectangle(Rectangle rectangle)
			{
				return new NativeRectClass(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
			}

			internal NativeRectClass(int left, int top, int right, int bottom)
			{
				_left = left;
				_top = top;
				_right = right;
				_bottom = bottom;
			}
		}

		internal struct NativePoint
		{
			public int _x;

			public int _y;
		}

		internal delegate IntPtr UICallback(IntPtr hwnd, int Msg, IntPtr wParam, IntPtr lParam);

		public static NativeRect ToNativeRect(Rectangle rectangle)
		{
			NativeRect result = default(NativeRect);
			result._left = rectangle.Left;
			result._top = rectangle.Top;
			result._bottom = rectangle.Bottom;
			result._right = rectangle.Right;
			return result;
		}

		[DllImport("Gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr dc, IntPtr gdiObject);

		[DllImport("Gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr gdiObject);

		[DllImport("Gdi32.dll", EntryPoint = "SelectClipRgn")]
		public static extern int SelectClipRgn(IntPtr dc, IntPtr region);

		[DllImport("Gdi32.dll", EntryPoint = "PatBlt")]
		public static extern bool PatBlt(IntPtr dc, int x, int y, int width, int height, uint rop);

		[DllImport("Gdi32.dll", EntryPoint = "GetClipBox")]
		public static extern int GetClipBox(IntPtr dc, ref NativeRect clipBox);

		[DllImport("Gdi32.dll", EntryPoint = "CreateBrushIndirect")]
		public static extern IntPtr CreateBrushIndirect(ref NativeLogBrush brush);

		[DllImport("Gdi32.dll", EntryPoint = "CombineRgn")]
		public static extern int CombineRgn(IntPtr destRegion, IntPtr sourceRegion1, IntPtr sourceRegion2, int mode);

		[DllImport("Gdi32.dll", EntryPoint = "CreateRectRgnIndirect")]
		public static extern IntPtr CreateRectRgnIndirect(ref NativeRect rect);

		[DllImport("User32.dll", EntryPoint = "BringWindowToTop")]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("User32.dll", EntryPoint = "SetWindowPos")]
		public static extern int SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

		[DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("User32.dll", EntryPoint = "GetDC")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
		public static extern IntPtr GetDeviceCaps(IntPtr dc, int index);

		[DllImport("User32", EntryPoint = "ReleaseDC")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr dc);

		[DllImport("User32.dll", EntryPoint = "ShowWindow")]
		public static extern int ShowWindow(IntPtr hWnd, int command);

		[DllImport("Gdi32", EntryPoint = "DeleteDC")]
		public static extern bool DeleteDC(IntPtr dc);

		[DllImport("User32", EntryPoint = "GetWindowRect")]
		public static extern bool GetWindowRect(IntPtr hWnd, ref NativeRect rect);

		[DllImport("User32", EntryPoint = "SetParent")]
		public static extern IntPtr SetParent(IntPtr hWnd, IntPtr newParent);

		[DllImport("User32.dll", EntryPoint = "LoadCursor")]
		internal static extern IntPtr LoadCursor(IntPtr instance, uint cursorId);

		[DllImport("Gdi32.dll", EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr dc);

		[DllImport("User32.dll", EntryPoint = "SetCursor")]
		public static extern IntPtr SetCursor(IntPtr cursor);

		[DllImport("User32.dll", EntryPoint = "GetCaretPos")]
		public static extern bool GetCaretPos(ref NativePoint point);

		[DllImport("user32.dll", EntryPoint = "PeekMessageW", ExactSpelling = true)]
		public static extern bool PeekMessage(ref NativeMsg msg, IntPtr hWnd, uint filterMin, uint filterMax, uint removeFlags);

		[DllImport("user32.dll", EntryPoint = "TranslateMessage", ExactSpelling = true)]
		public static extern bool TranslateMessage(ref NativeMsg msg);

		[DllImport("user32.dll", EntryPoint = "IsDialogMessageW", ExactSpelling = true)]
		public static extern bool IsDialogMessage(IntPtr hDlg, ref NativeMsg msg);

		[DllImport("user32.dll", EntryPoint = "DispatchMessageW", ExactSpelling = true)]
		public static extern IntPtr DispatchMessage(ref NativeMsg msg);

		// Returns a positive value for a message, 0 for WM_QUIT and -1 on error.
		[DllImport("user32.dll", EntryPoint = "GetMessageW", ExactSpelling = true)]
		public static extern int GetMessage(ref NativeMsg msg, IntPtr hWnd, uint filterMin, uint filterMax);

		[DllImport("user32.dll", EntryPoint = "WaitMessage", ExactSpelling = true)]
		public static extern bool WaitMessage();

		// Removes the next message from the queue for a private modal loop. Returns false when the loop must end:
		// on WM_QUIT the quit request is posted again so the outer message loop still sees it, and -1 is an error.
		internal static bool TryGetMessage(ref NativeMsg msg)
		{
			return TryGetMessage(ref msg, 0u, 0u);
		}

		internal static bool TryGetMessage(ref NativeMsg msg, uint filterMin, uint filterMax)
		{
			int result = GetMessage(ref msg, IntPtr.Zero, filterMin, filterMax);
			if (result > 0)
			{
				return true;
			}
			if (result == 0)
			{
				PostQuitMessage(unchecked((int)msg._wParam.ToInt64()));
			}
			return false;
		}

		[DllImport("User32.dll", EntryPoint = "BeginPaint")]
		public static extern IntPtr BeginPaint(IntPtr hWnd, ref NativePaintStruct paintStruct);

		[DllImport("User32.dll", EntryPoint = "EndPaint")]
		public static extern bool EndPaint(IntPtr hWnd, ref NativePaintStruct paintStruct);

		[DllImport("User32.dll", EntryPoint = "UpdateLayeredWindow")]
		public static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr destDc, ref NativePoint destPoint, ref NativeSize size, IntPtr sourceDc, ref NativePoint sourcePoint, int colorKey, ref NativeBlendFunction blend, int flags);

		[DllImport("User32.dll", EntryPoint = "AnimateWindow")]
		public static extern bool AnimateWindow(IntPtr hWnd, uint durationMs, uint flags);

		[DllImport("User32.dll", EntryPoint = "IsChild")]
		public static extern bool IsChild(IntPtr parent, IntPtr window);

		[DllImport("User32.dll", EntryPoint = "IsWindow")]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
		public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

		[DllImport("User32.dll", EntryPoint = "ReleaseCapture")]
		public static extern bool ReleaseCapture();

		[DllImport("User32.dll", EntryPoint = "WindowFromPoint")]
		public static extern IntPtr WindowFromPoint(NativePoint point);

		[DllImport("User32.dll", EntryPoint = "GetKeyboardState")]
		public static extern bool GetKeyboardState(byte[] keyState);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenThemeData", ExactSpelling = true)]
		public static extern IntPtr OpenThemeData(IntPtr hWnd, string classList);

		[DllImport("uxtheme.dll", EntryPoint = "CloseThemeData", ExactSpelling = true)]
		public static extern int CloseThemeData(IntPtr hTheme);

		[DllImport("uxtheme.dll", EntryPoint = "DrawThemeBackground")]
		public static extern IntPtr DrawThemeBackground(IntPtr theme, IntPtr dc, int partId, int stateId, NativeRectClass rect, NativeRectClass clipRect);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeColor")]
		public static extern IntPtr GetThemeColor(IntPtr theme, int partId, int stateId, int propertyId, ref int color);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeSysColor")]
		public static extern int GetThemeSysColor(IntPtr theme, int colorId);

		[DllImport("user32.dll", EntryPoint = "EnableMenuItem")]
		public static extern IntPtr EnableMenuItem(IntPtr menu, int itemId, int flags);

		[DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr revert);

		[DllImport("user32.dll", EntryPoint = "GetIconInfo")]
		public static extern bool GetIconInfo(IntPtr icon, out NativeIconInfo iconInfo);

		// GetWindowLongPtrW and SetWindowLongPtrW exist only in 64-bit user32; 32-bit code must use the LONG versions.
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr GetWindowLongPtr64Bit(IntPtr hWnd, int index);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongW", ExactSpelling = true, SetLastError = true)]
		private static extern int GetWindowLong32Bit(IntPtr hWnd, int index);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr SetWindowLongPtr64Bit(IntPtr hWnd, int index, IntPtr newValue);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongW", ExactSpelling = true, SetLastError = true)]
		private static extern int SetWindowLong32Bit(IntPtr hWnd, int index, int newValue);

		internal static IntPtr GetWindowLongPtr(IntPtr hWnd, int index)
		{
			return IntPtr.Size == 8 ? GetWindowLongPtr64Bit(hWnd, index) : new IntPtr(GetWindowLong32Bit(hWnd, index));
		}

		internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int index, IntPtr newValue)
		{
			return IntPtr.Size == 8 ? SetWindowLongPtr64Bit(hWnd, index, newValue) : new IntPtr(SetWindowLong32Bit(hWnd, index, newValue.ToInt32()));
		}

		[DllImport("user32.dll", EntryPoint = "CallWindowProcW", ExactSpelling = true)]
		public static extern IntPtr CallWindowProc(IntPtr previousProc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", EntryPoint = "GetWindowDC")]
		public static extern IntPtr GetWindowDC(IntPtr hWnd);

		[StructLayout(LayoutKind.Sequential)]
		internal struct BitmapInfoHeader
		{
			public int StructSize;

			public int Width;

			public int Height;

			public short Planes;

			public short BitCount;

			public int Compression;

			public int SizeImage;

			public int XPelsPerMeter;

			public int YPelsPerMeter;

			public int ClrUsed;

			public int ClrImportant;
		}

		[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", ExactSpelling = true)]
		internal static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfoHeader bitmapInfo, uint usage, out IntPtr bits, IntPtr section, uint offset);

		[DllImport("user32.dll", EntryPoint = "PostQuitMessage", ExactSpelling = true)]
		public static extern void PostQuitMessage(int exitCode);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetPropW", ExactSpelling = true)]
		internal static extern IntPtr SetPropW(IntPtr hWnd, string name, IntPtr data);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RemovePropW", ExactSpelling = true)]
		internal static extern IntPtr RemovePropW(IntPtr hWnd, string name);

		[DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled")]
		public static extern void DwmIsCompositionEnabled(ref bool isEnabled);
	}
}
