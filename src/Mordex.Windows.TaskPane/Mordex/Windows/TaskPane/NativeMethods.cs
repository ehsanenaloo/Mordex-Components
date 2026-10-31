using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace Mordex.Windows.TaskPane
{
	[SuppressUnmanagedCodeSecurity]
	internal class NativeMethods
	{
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

		internal NativeMethods()
		{
		}

		[DllImport("uxtheme.dll", EntryPoint = "IsAppThemed")]
		public static extern bool IsAppThemed();

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeAppProperties")]
		public static extern int GetThemeAppProperties();

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

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern bool DeleteObject(IntPtr gdiObject);

		[DllImport("user32.dll", EntryPoint = "GetIconInfo")]
		public static extern bool GetIconInfo(IntPtr icon, out NativeIconInfo iconInfo);

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
	}
}
