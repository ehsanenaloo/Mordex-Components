using System;
using System.Runtime.InteropServices;

namespace Mordex.Windows.DatePicker
{
	internal static class NativeMethods
	{
		internal const int WsExToolWindow = 0x80;

		internal const int ColorHighlight = 13;

		internal const int ColorButtonFace = 15;

		internal const int ThemePartPushButton = 1;

		internal const int ThemeStateNormal = 1;

		internal const int ThemePropertyBorderColorHint = 3822;

		internal const int ThemePropertyAccentColorHint = 3823;

		private const int SwpNoSize = 0x1;

		private const int SwpNoMove = 0x2;

		private const int SwpShowWindow = 0x40;

		private static readonly IntPtr HwndTopMost = new IntPtr(-1);

		[DllImport("user32", EntryPoint = "SetWindowPos")]
		private static extern bool SetWindowPos(IntPtr handle, IntPtr insertAfter, int x, int y, int width, int height, int flags);

		[DllImport("user32", EntryPoint = "SetParent")]
		internal static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode, EntryPoint = "OpenThemeData", ExactSpelling = true)]
		internal static extern IntPtr OpenThemeData(IntPtr windowHandle, string classList);

		[DllImport("uxtheme.dll", EntryPoint = "CloseThemeData")]
		internal static extern int CloseThemeData(IntPtr themeHandle);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeColor")]
		internal static extern int GetThemeColor(IntPtr themeHandle, int partId, int stateId, int propertyId, out int color);

		[DllImport("uxtheme.dll", EntryPoint = "GetThemeSysColor")]
		internal static extern int GetThemeSysColor(IntPtr themeHandle, int colorId);

		// Shows the window above all other windows without moving, resizing or activating it.
		internal static void ShowTopMost(IntPtr handle)
		{
			SetWindowPos(handle, HwndTopMost, 0, 0, 0, 0, SwpNoSize | SwpNoMove | SwpShowWindow);
		}
	}
}
