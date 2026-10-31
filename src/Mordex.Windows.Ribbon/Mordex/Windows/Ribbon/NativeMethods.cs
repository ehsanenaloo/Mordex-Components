#pragma warning disable CS0649 // Fields of native interop structs are filled by native calls.
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

namespace Mordex.Windows.Ribbon
{
	internal class NativeMethods
	{
		internal const uint PmRemove = 1u;

		internal const int WmSize = 5;

		internal const int WmActivate = 6;

		internal const int WmSetRedraw = 11;

		internal const int WmSetText = 12;

		internal const int WmPaint = 15;

		internal const int WmEraseBackground = 20;

		internal const int WmActivateApp = 28;

		internal const int WmSetCursor = 32;

		internal const int WmMouseActivate = 33;

		internal const uint SwpRefreshFrame = 51u;

		internal const int WmWindowPosChanging = 70;

		internal const int WmWindowPosChanged = 71;

		internal const int WmSetIcon = 128;

		internal const int WmNcCalcSize = 131;

		internal const int WmNcHitTest = 132;

		internal const int WmNcPaint = 133;

		internal const int WmNcActivate = 134;

		internal const int WmNcMouseMove = 160;

		internal const int WmNcLButtonDown = 161;

		internal const int WmNcLButtonUp = 162;

		internal const int WmNcLButtonDoubleClick = 163;

		internal const int WmNcRButtonDown = 164;

		internal const int WmNcRButtonUp = 165;

		internal const int WmNcRButtonDoubleClick = 166;

		internal const int WmNcMButtonDown = 167;

		internal const uint PopupFadeMilliseconds = 200u;

		internal const int WmKeyDown = 256;

		internal const int WmSysKeyDown = 260;

		internal const int WmSysCommand = 274;

		internal const uint WmMouseFirst = 512u;

		internal const int WmMouseMove = 512;

		internal const int WmLButtonDown = 513;

		internal const int WmLButtonUp = 514;

		internal const int WmLButtonDoubleClick = 515;

		internal const int WmRButtonDown = 516;

		internal const int WmRButtonUp = 517;

		internal const int WmRButtonDoubleClick = 518;

		internal const int WmMButtonDown = 519;

		internal const uint WmMouseLast = 525u;

		internal const int WmNcMouseLeave = 674;

		internal const int WmMouseLeave = 675;

		internal const int RedrawFrameInvalidate = 1025;

		internal const int RedrawFrameNoChildren = 1088;

		internal const uint AwHideWithFade = 589824u;

		internal const int BITSPIXEL = 12;

		internal const int SRCCOPY = 0x00CC0020;

		internal const uint DTT_TEXTCOLOR = 0x1;

		internal const uint DTT_GLOWSIZE = 0x800;

		internal const uint DTT_COMPOSITED = 0x2000;

		// DT_CENTER | DT_VCENTER | DT_SINGLELINE | DT_NOPREFIX
		internal const int CaptionTextFormat = 0x825;

		// Message parameters are 64 bit on x64 but carry packed 32 bit values; keep the low half instead of throwing.
		internal static int ToInt32Unchecked(IntPtr value)
		{
			return unchecked((int)value.ToInt64());
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal class LogFont
		{
			public int _height;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string _faceName;
		}

		internal struct PaintStruct
		{
			public IntPtr _deviceContext;

			public int _erase;

			public Rectangle _rectangle;

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

		internal struct Margins
		{
			public int _leftWidth;

			public int _rightWidth;

			public int _topHeight;

			public int _bottomHeight;

			public Margins(Padding padding)
			{
				_leftWidth = padding.Left;
				_rightWidth = padding.Right;
				_topHeight = padding.Top;
				_bottomHeight = padding.Bottom;
			}
		}

		internal struct TrackMouseEventData
		{
			[MarshalAs(UnmanagedType.U4)]
			public int _size;

			[MarshalAs(UnmanagedType.U4)]
			public int _flags;

			public IntPtr _windowHandle;

			[MarshalAs(UnmanagedType.U4)]
			public int _hoverTime;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct BufferedPaintParams
		{
			public int _size;

			public int _flags;

			public NativeRect _exclude;

			public BlendFunction _blendFunction;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct BlendFunction
		{
			public byte _blendOp;

			public byte _blendFlags;

			public byte _sourceConstantAlpha;

			public byte _alphaFormat;
		}

		internal struct NativeSize
		{
			public int _width;

			public int _height;
		}

		internal struct NativeRect
		{
			public int _left;

			public int _top;

			public int _right;

			public int _bottom;
		}

		internal struct IconInfo
		{
			public bool _isIcon;

			public int _hotspotX;

			public int _hotspotY;

			public IntPtr _maskBitmap;

			public IntPtr _colorBitmap;
		}

		internal struct WindowPos
		{
			internal IntPtr _windowHandle;

			internal int _x;

			internal int _y;

			internal int _width;

			internal int _height;

			internal uint _flags;
		}

		internal struct NcCalcSizeParams
		{
			public NativeRect _newWindowRect;

			public NativeRect _oldWindowRect;

			public NativeRect _oldClientRect;

			public WindowPos _windowPos;
		}

		internal struct NativePoint
		{
			public int _x;

			public int _y;
		}

		internal struct NativeMessage
		{
			public IntPtr _windowHandle;

			public int _message;

			public IntPtr _wParam;

			public IntPtr _lParam;

			public int _time;

			public int _pointX;

			public int _pointY;
		}

		internal struct NativeShadowOffset
		{
			public int _x;

			public int _y;
		}

		internal struct ThemeTextOptions
		{
			public uint _size;

			public uint _flags;

			public uint _textColor;

			public uint _borderColor;

			public uint _shadowColor;

			public int _shadowType;

			public NativeShadowOffset _shadowOffset;

			public int _borderSize;

			public int _fontPropId;

			public int _colorPropId;

			public int _stateId;

			public int _applyOverlay;

			public int _glowSize;

			public IntPtr _drawTextCallback;

			public int _callbackParam;
		}

		internal struct BitmapInfo
		{
			public BitmapInfoHeader _header;

			public RgbQuad _colors;
		}

		internal struct BitmapInfoHeader
		{
			public int _size;

			public int _width;

			public int _height;

			public short _planes;

			public short _bitCount;

			public int _compression;

			public int _sizeImage;

			public int _xPelsPerMeter;

			public int _yPelsPerMeter;

			public int _colorsUsed;

			public int _colorsImportant;
		}

		internal struct RgbQuad
		{
			public byte _blue;

			public byte _green;

			public byte _red;

			public byte _reserved;
		}

		public enum SystemMetric
		{
			SM_ARRANGE = 56,
			SM_CLEANBOOT = 67,
			SM_CMONITORS = 80,
			SM_CMOUSEBUTTONS = 43,
			SM_CXBORDER = 5,
			SM_CXCURSOR = 13,
			SM_CXDLGFRAME = 7,
			SM_CXDOUBLECLK = 36,
			SM_CXDRAG = 68,
			SM_CXEDGE = 45,
			SM_CXFIXEDFRAME = 7,
			SM_CXFOCUSBORDER = 83,
			SM_CXFRAME = 32,
			SM_CXFULLSCREEN = 16,
			SM_CXHSCROLL = 21,
			SM_CXHTHUMB = 10,
			SM_CXICON = 11,
			SM_CXICONSPACING = 38,
			SM_CXMAXIMIZED = 61,
			SM_CXMAXTRACK = 59,
			SM_CXMENUCHECK = 71,
			SM_CXMENUSIZE = 54,
			SM_CXMIN = 28,
			SM_CXMINIMIZED = 57,
			SM_CXMINSPACING = 47,
			SM_CXMINTRACK = 34,
			SM_CXPADDEDBORDER = 92,
			SM_CXSCREEN = 0,
			SM_CXSIZE = 30,
			SM_CXSIZEFRAME = 32,
			SM_CXSMICON = 49,
			SM_CXSMSIZE = 52,
			SM_CXVIRTUALSCREEN = 78,
			SM_CXVSCROLL = 2,
			SM_CYBORDER = 6,
			SM_CYCAPTION = 4,
			SM_CYCURSOR = 14,
			SM_CYDLGFRAME = 8,
			SM_CYDOUBLECLK = 37,
			SM_CYDRAG = 69,
			SM_CYEDGE = 46,
			SM_CYFIXEDFRAME = 8,
			SM_CYFOCUSBORDER = 84,
			SM_CYFRAME = 33,
			SM_CYFULLSCREEN = 17,
			SM_CYHSCROLL = 3,
			SM_CYICON = 12,
			SM_CYICONSPACING = 39,
			SM_CYKANJIWINDOW = 18,
			SM_CYMAXIMIZED = 62,
			SM_CYMAXTRACK = 60,
			SM_CYMENU = 15,
			SM_CYMENUCHECK = 72,
			SM_CYMENUSIZE = 55,
			SM_CYMIN = 29,
			SM_CYMINIMIZED = 58,
			SM_CYMINSPACING = 48,
			SM_CYMINTRACK = 35,
			SM_CYSCREEN = 1,
			SM_CYSIZE = 31,
			SM_CYSIZEFRAME = 33,
			SM_CYSMCAPTION = 51,
			SM_CYSMICON = 50,
			SM_CYSMSIZE = 53,
			SM_CYVIRTUALSCREEN = 79,
			SM_CYVSCROLL = 20,
			SM_CYVTHUMB = 9,
			SM_DBCSENABLED = 42,
			SM_DEBUG = 22,
			SM_DIGITIZER = 94,
			SM_IMMENABLED = 82,
			SM_MAXIMUMTOUCHES = 95,
			SM_MEDIACENTER = 87,
			SM_MENUDROPALIGNMENT = 40,
			SM_MIDEASTENABLED = 74,
			SM_MOUSEPRESENT = 19,
			SM_MOUSEHORIZONTALWHEELPRESENT = 91,
			SM_MOUSEWHEELPRESENT = 75,
			SM_NETWORK = 63,
			SM_PENWINDOWS = 41,
			SM_REMOTECONTROL = 8193,
			SM_REMOTESESSION = 4096,
			SM_SAMEDISPLAYFORMAT = 81,
			SM_SECURE = 44,
			SM_SERVER_R2_INSTALLED = 89,
			SM_SHOWSOUNDS = 70,
			SM_SHUTTINGDOWN = 8192,
			SM_SLOWMACHINE = 73,
			SM_STARTER = 88,
			SM_SWAPBUTTON = 23,
			SM_TABLETPC = 86,
			SM_XVIRTUALSCREEN = 76,
			SM_YVIRTUALSCREEN = 77
		}

		[Flags]
		internal enum DCX
		{
			DCX_CACHE = 2,
			DCX_CLIPCHILDREN = 8,
			DCX_CLIPSIBLINGS = 0x10,
			DCX_EXCLUDERGN = 0x40,
			DCX_EXCLUDEUPDATE = 0x100,
			DCX_INTERSECTRGN = 0x80,
			DCX_INTERSECTUPDATE = 0x200,
			DCX_LOCKWINDOWUPDATE = 0x400,
			DCX_NORECOMPUTE = 0x100000,
			DCX_NORESETATTRS = 4,
			DCX_PARENTCLIP = 0x20,
			DCX_VALIDATE = 0x200000,
			DCX_WINDOW = 1
		}

		[DllImport("User32.dll", EntryPoint = "BeginPaint")]
		public static extern IntPtr BeginPaint(IntPtr windowHandle, ref PaintStruct paintStruct);

		[DllImport("User32.dll", EntryPoint = "EndPaint")]
		public static extern bool EndPaint(IntPtr windowHandle, ref PaintStruct paintStruct);

		[DllImport("user32.dll", EntryPoint = "GetIconInfo")]
		public static extern bool GetIconInfo(IntPtr iconHandle, out IconInfo iconInfo);

		[DllImport("Gdi32.dll", EntryPoint = "GetDeviceCaps")]
		public static extern IntPtr GetDeviceCaps(IntPtr deviceContext, int index);

		[DllImport("Gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern bool DeleteObject(IntPtr gdiObject);

		[DllImport("Gdi32", EntryPoint = "DeleteDC")]
		public static extern bool DeleteDC(IntPtr deviceContext);

		[DllImport("Gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr deviceContext, IntPtr gdiObject);

		[DllImport("Gdi32.dll", EntryPoint = "CreateCompatibleDC", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr deviceContext);

		[DllImport("Gdi32.dll", EntryPoint = "CreateCompatibleBitmap", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr deviceContext, int width, int height);

		[DllImport("User32.dll", EntryPoint = "PeekMessage")]
		public static extern bool PeekMessage(ref NativeMessage message, IntPtr windowHandle, uint filterMin, uint filterMax, uint removeFlags);

		[DllImport("User32.dll", EntryPoint = "TranslateMessage")]
		public static extern bool TranslateMessage(ref NativeMessage message);

		[DllImport("User32.dll", EntryPoint = "WaitMessage")]
		public static extern bool WaitMessage();

		[DllImport("User32.dll", EntryPoint = "GetMessage")]
		public static extern bool GetMessage(ref NativeMessage message, IntPtr windowHandle, uint filterMin, uint filterMax);

		[DllImport("User32.dll", EntryPoint = "DispatchMessage")]
		public static extern bool DispatchMessage(ref NativeMessage message);

		[DllImport("User32.dll", EntryPoint = "ReleaseCapture")]
		public static extern bool ReleaseCapture();

		[DllImport("User32.dll", EntryPoint = "ShowWindow")]
		public static extern bool ShowWindow(IntPtr windowHandle, int command);

		[DllImport("User32.dll", EntryPoint = "IsChild")]
		public static extern bool IsChild(IntPtr parentHandle, IntPtr childHandle);

		[DllImport("user32.dll", EntryPoint = "GetDC", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetDC(IntPtr windowHandle);

		[DllImport("user32.dll", EntryPoint = "GetWindowDC", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetWindowDC(IntPtr windowHandle);

		[DllImport("user32.dll", EntryPoint = "GetDCEx", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetDCEx(IntPtr windowHandle, IntPtr clipRegion, uint flags);

		[DllImport("User32.dll", EntryPoint = "SetCursor")]
		public static extern IntPtr SetCursor(IntPtr cursorHandle);

		[DllImport("User32.dll", EntryPoint = "LoadCursor")]
		internal static extern IntPtr LoadCursor(IntPtr instanceHandle, uint cursorId);

		[DllImport("User32", EntryPoint = "ReleaseDC")]
		public static extern int ReleaseDC(IntPtr windowHandle, IntPtr deviceContext);

		[DllImport("User32", EntryPoint = "GetWindowRect")]
		public static extern bool GetWindowRect(IntPtr windowHandle, ref NativeRect rect);

		[DllImport("User32.dll", EntryPoint = "SendMessage")]
		public static extern IntPtr SendMessage(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam);

		[DllImport("Gdi32", EntryPoint = "BitBlt")]
		public static extern bool BitBlt(IntPtr destDc, int destX, int destY, int width, int height, IntPtr sourceDc, int sourceX, int sourceY, int rasterOp);

		[DllImport("User32.dll", EntryPoint = "RedrawWindow", ExactSpelling = true, SetLastError = true)]
		public static extern bool RedrawWindow(IntPtr windowHandle, IntPtr updateRect, IntPtr updateRegion, int flags);

		[DllImport("User32.dll", EntryPoint = "SetWindowPos", ExactSpelling = true, SetLastError = true)]
		public static extern int SetWindowPos(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

		[DllImport("User32.dll", EntryPoint = "UpdateLayeredWindow")]
		public static extern bool UpdateLayeredWindow(IntPtr windowHandle, IntPtr destDc, ref NativePoint destPosition, ref NativeSize size, IntPtr sourceDc, ref NativePoint sourcePosition, int colorKey, ref BlendFunction blend, int flags);

		[DllImport("User32.dll", EntryPoint = "SetForegroundWindow", ExactSpelling = true, SetLastError = true)]
		public static extern bool SetForegroundWindow(IntPtr windowHandle);

		[DllImport("User32.dll", EntryPoint = "TrackMouseEvent")]
		public static extern bool TrackMouseEvent([In][Out][MarshalAs(UnmanagedType.Struct)] ref TrackMouseEventData trackData);

		[DllImport("User32.dll", EntryPoint = "AnimateWindow")]
		public static extern bool AnimateWindow(IntPtr windowHandle, uint duration, uint flags);

		[DllImport("User32.dll", EntryPoint = "GetCaretPos")]
		public static extern bool GetCaretPos(ref NativePoint position);

		[DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
		public static extern int GetSystemMetrics(SystemMetric systemMetric);

		[DllImport("UxTheme.dll", EntryPoint = "DrawThemeTextEx")]
		public static extern int DrawThemeTextEx(IntPtr theme, IntPtr deviceContext, int partId, int stateId, IntPtr textPointer, int textLength, int textFlags, ref NativeRect rect, ref ThemeTextOptions options);

		[DllImport("Gdi32.dll", EntryPoint = "CreateDIBSection", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr CreateDIBSection(IntPtr deviceContext, ref BitmapInfo bitmapInfo, uint usage, int bitsPointer, IntPtr sectionHandle, uint offset);

		[DllImport("Gdi32.dll", EntryPoint = "SaveDC")]
		public static extern int SaveDC(IntPtr deviceContext);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetPropW", ExactSpelling = true)]
		internal static extern IntPtr SetPropW(IntPtr windowHandle, string text, IntPtr data);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetPropW", ExactSpelling = true)]
		internal static extern IntPtr GetPropW(IntPtr windowHandle, string text);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RemovePropW", ExactSpelling = true)]
		internal static extern IntPtr RemovePropW(IntPtr windowHandle, string text);

		[DllImport("dwmapi.dll", EntryPoint = "DwmDefWindowProc")]
		public static extern int DwmDefWindowProc(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam, ref IntPtr result);

		[DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled")]
		public static extern void DwmIsCompositionEnabled(ref bool isEnabled);

		[DllImport("dwmapi.dll", EntryPoint = "DwmExtendFrameIntoClientArea")]
		public static extern void DwmExtendFrameIntoClientArea(IntPtr windowHandle, ref Margins margins);

	}
}
