using System;
using System.Security;

namespace Mordex.Windows.UI.Design
{
	[SuppressUnmanagedCodeSecurity]
	internal static class NativeMethods
	{
		internal const int WM_MOUSEMOVE = 512;

		internal const int WM_LBUTTONDOWN = 513;

		internal const int WM_LBUTTONUP = 514;

		internal const int WM_SETCURSOR = 32;

		internal const int WM_MOUSELEAVE = 675;
	}
}
