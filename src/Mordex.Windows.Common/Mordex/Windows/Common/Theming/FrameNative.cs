using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Mordex.Windows.Common
{
	internal static class FrameNative
	{
		internal const int WmSize = 0x0005;
		internal const int WmActivate = 0x0006;
		internal const int WmCancelMode = 0x001F;
		internal const int WmSetText = 0x000C;
		internal const int WmWindowPosChanged = 0x0047;
		internal const int WmStyleChanged = 0x007D;
		internal const int WmSetIcon = 0x0080;
		internal const int WmGetMinMaxInfo = 0x0024;
		internal const int WmNcCalcSize = 0x0083;
		internal const int WmNcHitTest = 0x0084;
		internal const int WmNcPaint = 0x0085;
		internal const int WmNcActivate = 0x0086;
		internal const int WmNcUahDrawCaption = 0x00AE;
		internal const int WmNcUahDrawFrame = 0x00AF;
		internal const int WmNcMouseMove = 0x00A0;
		internal const int WmNcLButtonDown = 0x00A1;
		internal const int WmNcLButtonDblClk = 0x00A3;
		internal const int WmMouseMove = 0x0200;
		internal const int WmLButtonUp = 0x0202;
		internal const int WmCaptureChanged = 0x0215;
		internal const int WmSysCommand = 0x0112;
		internal const int WmNcMouseLeave = 0x02A2;
		internal const int WmPrint = 0x0317;
		internal const int WmDwmNcRenderingChanged = 0x031F;
		internal const int WmDpiChanged = 0x02E0;
		internal const long PrfNonClient = 0x2;

		internal const int HtNowhere = 0;
		internal const int HtClient = 1;
		internal const int HtCaption = 2;
		internal const int HtSysMenu = 3;
		internal const int HtMenu = 5;
		internal const int WmNcLButtonUp = 0x00A2;
		internal const int HtMinButton = 8;
		internal const int HtMaxButton = 9;
		internal const int HtLeft = 10;
		internal const int HtRight = 11;
		internal const int HtTop = 12;
		internal const int HtTopLeft = 13;
		internal const int HtTopRight = 14;
		internal const int HtBottom = 15;
		internal const int HtBottomLeft = 16;
		internal const int HtBottomRight = 17;
		internal const int HtBorder = 18;
		internal const int HtClose = 20;

		internal const int ScMinimize = 0xF020;
		internal const int ScMaximize = 0xF030;
		internal const int ScClose = 0xF060;
		internal const int ScRestore = 0xF120;

		internal const int CaptionColor = 35;
		internal const int TextColor = 36;
		internal const int BorderColor = 34;
		private const int CornerPreference = 33;
		private const int NcRenderingPolicy = 2;

		internal const long WsExLayoutRtl = 0x00400000;
		private const int GwlStyle = -16;
		private const int GwlExStyle = -20;
		private const long WsVisible = 0x10000000;

		private const int SmCxFrame = 32;
		private const int SmCxPaddedBorder = 92;

		[StructLayout(LayoutKind.Sequential)]
		internal struct Rect
		{
			public int Left, Top, Right, Bottom;

			internal Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);

			internal static Rect From(Rectangle r) => new Rect { Left = r.Left, Top = r.Top, Right = r.Right, Bottom = r.Bottom };
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TrackMouse
		{
			public int Size;
			public int Flags;
			public IntPtr Window;
			public int HoverTime;
		}

		[DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rect rect);
		[DllImport("user32.dll")] internal static extern IntPtr GetWindowDC(IntPtr window);
		[DllImport("user32.dll")] internal static extern int ReleaseDC(IntPtr window, IntPtr dc);
		[DllImport("user32.dll")] internal static extern bool IsZoomed(IntPtr window);
		[DllImport("user32.dll")] internal static extern bool IsIconic(IntPtr window);
		[DllImport("user32.dll")] internal static extern uint GetDpiForWindow(IntPtr window);
		[DllImport("user32.dll")] private static extern int GetSystemMetricsForDpi(int index, uint dpi);
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);
		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern IntPtr GetProp(IntPtr window, string name);
		[DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int cx, int cy, uint flags);
		[DllImport("user32.dll")] private static extern bool RedrawWindow(IntPtr window, IntPtr update, IntPtr region, uint flags);
		[DllImport("user32.dll")] internal static extern int SetWindowRgn(IntPtr window, IntPtr region, bool redraw);
		[DllImport("user32.dll")] internal static extern IntPtr SetCapture(IntPtr window);
		[DllImport("user32.dll")] internal static extern bool ReleaseCapture();
		[DllImport("user32.dll")] internal static extern IntPtr GetCapture();
		[DllImport("user32.dll")] internal static extern bool ClientToScreen(IntPtr window, ref Point point);
		[DllImport("user32.dll", EntryPoint = "PostMessageW")] internal static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")] internal static extern IntPtr GetSystemMenu(IntPtr window, bool revert);
		[DllImport("user32.dll")] private static extern int TrackPopupMenuEx(IntPtr menu, uint flags, int x, int y, IntPtr window, IntPtr parameters);
		[DllImport("user32.dll")] private static extern bool TrackMouseEvent(ref TrackMouse track);
		[DllImport("gdi32.dll")] internal static extern bool SetViewportOrgEx(IntPtr dc, int x, int y, out Point previous);
		[DllImport("gdi32.dll")] private static extern int SetLayout(IntPtr dc, uint layout);
		[DllImport("gdi32.dll")] private static extern int ExcludeClipRect(IntPtr dc, int left, int top, int right, int bottom);
		[DllImport("gdi32.dll")] private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);
		[DllImport("gdi32.dll")] private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);
		[DllImport("gdi32.dll")] private static extern int CombineRgn(IntPtr dest, IntPtr source1, IntPtr source2, int mode);
		[DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr obj);
		[DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

		[StructLayout(LayoutKind.Sequential)]
		private struct MonitorInfo
		{
			public int Size;
			public Rect Monitor;
			public Rect Work;
			public int Flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MinMaxInfo
		{
			public Point Reserved;
			public Point MaxSize;
			public Point MaxPosition;
			public Point MinTrack;
			public Point MaxTrack;
		}

		[DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
		[DllImport("user32.dll")] private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

		/// <summary>
		/// Sets the size of the maximized window to the work area of its monitor plus the overhang on each side. Windows works the
		/// size out from the caption style that the frame took away, so its own answer is too tall.
		/// </summary>
		internal static void FitMaximizedBounds(IntPtr window, IntPtr lParam, Size maximumSize, int overhang)
		{
			if (!maximumSize.IsEmpty)
			{
				return;
			}
			MonitorInfo info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
			if (!GetMonitorInfo(MonitorFromWindow(window, 2), ref info))
			{
				return;
			}
			MinMaxInfo minMax = Marshal.PtrToStructure<MinMaxInfo>(lParam);
			minMax.MaxPosition = new Point(info.Work.Left - info.Monitor.Left - overhang, info.Work.Top - info.Monitor.Top - overhang);
			minMax.MaxSize = new Point(info.Work.Right - info.Work.Left + overhang * 2, info.Work.Bottom - info.Work.Top + overhang * 2);
			if (minMax.MaxTrack.X < minMax.MaxSize.X)
			{
				minMax.MaxTrack.X = minMax.MaxSize.X;
			}
			if (minMax.MaxTrack.Y < minMax.MaxSize.Y)
			{
				minMax.MaxTrack.Y = minMax.MaxSize.Y;
			}
			Marshal.StructureToPtr(minMax, lParam, false);
		}

		internal static Rectangle GetWindowRectangle(IntPtr window)
		{
			GetWindowRect(window, out Rect rect);
			return rect.ToRectangle();
		}

		internal static Size GetWindowSize(IntPtr window)
		{
			return GetWindowRectangle(window).Size;
		}

		internal static int GetDpi(IntPtr window)
		{
			uint dpi = GetDpiForWindow(window);
			return dpi == 0 ? 96 : (int)dpi;
		}

		/// <summary>How far a maximized window reaches beyond the screen on each side.</summary>
		internal static int GetOverhang(int dpi)
		{
			return GetSystemMetricsForDpi(SmCxFrame, (uint)dpi) + GetSystemMetricsForDpi(SmCxPaddedBorder, (uint)dpi);
		}

		internal static long GetExStyle(IntPtr window)
		{
			return (long)GetWindowLongPtr(window, GwlExStyle);
		}

		internal static void SetVisibleStyle(IntPtr window, bool visible)
		{
			long style = (long)GetWindowLongPtr(window, GwlStyle);
			style = visible ? style | WsVisible : style & ~WsVisible;
			SetWindowLongPtr(window, GwlStyle, new IntPtr(style));
		}

		internal const long WsCaption = 0x00C00000;

		/// <summary>Takes the caption style from the window, so Windows has no caption of its own to paint, or gives it back.</summary>
		internal static void SetCaptionStyle(IntPtr window, bool caption)
		{
			long style = (long)GetWindowLongPtr(window, GwlStyle);
			style = caption ? style | WsCaption : style & ~WsCaption;
			SetWindowLongPtr(window, GwlStyle, new IntPtr(style));
		}

		internal static bool HasVisibleStyle(IntPtr window)
		{
			return ((long)GetWindowLongPtr(window, GwlStyle) & WsVisible) != 0;
		}

		internal static void FrameChanged(IntPtr window)
		{
			SetWindowPos(window, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020);
		}

		/// <summary>Repaints the frame of the window at once.</summary>
		internal static void RedrawFrame(IntPtr window)
		{
			RedrawWindow(window, IntPtr.Zero, IntPtr.Zero, 0x0400 | 0x0001);
		}

		internal static void SetLayoutLtr(IntPtr dc)
		{
			SetLayout(dc, 0);
		}

		internal static void ExcludeClientFromClip(IntPtr dc, Rectangle client)
		{
			ExcludeClipRect(dc, client.Left, client.Top, client.Right, client.Bottom);
		}

		internal static IntPtr CreateTopRoundRegion(int width, int height, int radius)
		{
			IntPtr round = CreateRoundRectRgn(0, 0, width + 1, height + radius * 2 + 2, radius * 2, radius * 2);
			IntPtr box = CreateRectRgn(0, 0, width, height);
			CombineRgn(round, round, box, 1);
			DeleteObject(box);
			return round;
		}

		internal static void TrackNonClientLeave(IntPtr window)
		{
			TrackMouse track = new TrackMouse { Size = Marshal.SizeOf<TrackMouse>(), Flags = 0x2 | 0x10, Window = window };
			TrackMouseEvent(ref track);
		}

		internal static int TrackPopup(IntPtr menu, bool rightAligned, int x, int y, IntPtr window)
		{
			return TrackPopupMenuEx(menu, 0x0100 | (rightAligned ? 0x0008u : 0u), x, y, window, IntPtr.Zero);
		}

		internal static Rect[] ReadCalcSize(IntPtr lParam)
		{
			Rect[] rects = new Rect[3];
			int size = Marshal.SizeOf<Rect>();
			for (int i = 0; i < 3; i++)
			{
				rects[i] = Marshal.PtrToStructure<Rect>(lParam + i * size);
			}
			return rects;
		}

		internal static void WriteCalcSize(IntPtr lParam, Rect[] rects)
		{
			Marshal.StructureToPtr(rects[0], lParam, false);
		}

		/// <summary>Turns the drawing of the non-client area by Windows off, so the frame paint of the window is the only one.</summary>
		internal static void SetNonClientRendering(IntPtr window, bool paintedByWindow)
		{
			int value = paintedByWindow ? 1 : 0;
			DwmSetWindowAttribute(window, NcRenderingPolicy, ref value, sizeof(int));
		}

		internal static void SetCornerPreference(IntPtr window, int preference)
		{
			if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
			{
				DwmSetWindowAttribute(window, CornerPreference, ref preference, sizeof(int));
			}
		}

		internal static void SetColorAttribute(IntPtr window, int attribute, Color color)
		{
			if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
			{
				int value = color.R | color.G << 8 | color.B << 16;
				DwmSetWindowAttribute(window, attribute, ref value, sizeof(int));
			}
		}

		internal static void SetDefaultColor(IntPtr window, int attribute)
		{
			if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
			{
				int value = unchecked((int)0xFFFFFFFF);
				DwmSetWindowAttribute(window, attribute, ref value, sizeof(int));
			}
		}
	}
}
