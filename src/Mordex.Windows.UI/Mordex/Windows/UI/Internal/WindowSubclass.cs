using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mordex.Windows.UI.Internal
{
	internal class WindowSubclass
	{
		private const int GWLP_WNDPROC = -4;

		private const int WM_NCDESTROY = 0x82;

		// Hooks that could not be removed because another window procedure was installed on top of them.
		// They must stay alive and keep forwarding messages until the window is destroyed.
		private static readonly List<Hook> _detachedHooks = new List<Hook>();

		private IntPtr _windowHandle;

		private Hook _hook;

		internal WindowSubclass()
		{
			_windowHandle = IntPtr.Zero;
		}

		internal IntPtr GetWindowHandle()
		{
			return _windowHandle;
		}

		internal void AttachToWindow(IntPtr windowHandle)
		{
			if (_windowHandle.Equals(windowHandle))
			{
				return;
			}
			Detach();
			if (windowHandle == IntPtr.Zero)
			{
				return;
			}
			Hook hook = new Hook(this, windowHandle);
			IntPtr originalProc = NativeMethods.SetWindowLongPtr(windowHandle, GWLP_WNDPROC, hook.CallbackPointer);
			if (originalProc == IntPtr.Zero)
			{
				return;
			}
			hook.OriginalProc = originalProc;
			_hook = hook;
			_windowHandle = windowHandle;
		}

		internal void Detach()
		{
			Hook hook = _hook;
			_hook = null;
			_windowHandle = IntPtr.Zero;
			if (hook == null)
			{
				return;
			}
			hook.Owner = null;
			if (!NativeMethods.IsWindow(hook.Handle))
			{
				return;
			}
			if (NativeMethods.GetWindowLongPtr(hook.Handle, GWLP_WNDPROC) == hook.CallbackPointer)
			{
				NativeMethods.SetWindowLongPtr(hook.Handle, GWLP_WNDPROC, hook.OriginalProc);
				return;
			}
			lock (_detachedHooks)
			{
				PruneDetachedHooks();
				_detachedHooks.Add(hook);
			}
		}

		// WM_NCDESTROY may not reach a detached hook when the procedure chain changed again before the window was
		// destroyed, so hooks of windows that no longer exist are removed here too. Call it inside the lock.
		private static void PruneDetachedHooks()
		{
			_detachedHooks.RemoveAll(hook => !NativeMethods.IsWindow(hook.Handle));
		}

		protected virtual IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr wParam, IntPtr lParam)
		{
			Hook hook = _hook;
			if (hook == null)
			{
				return IntPtr.Zero;
			}
			return NativeMethods.CallWindowProc(hook.OriginalProc, hWnd, iMsg, wParam, lParam);
		}

		private sealed class Hook
		{
			private readonly NativeMethods.UICallback _callback;

			internal readonly IntPtr Handle;

			internal readonly IntPtr CallbackPointer;

			internal IntPtr OriginalProc;

			internal WindowSubclass Owner;

			internal Hook(WindowSubclass owner, IntPtr handle)
			{
				Owner = owner;
				Handle = handle;
				_callback = Invoke;
				CallbackPointer = Marshal.GetFunctionPointerForDelegate(_callback);
			}

			private IntPtr Invoke(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
			{
				WindowSubclass owner = Owner;
				if (owner != null)
				{
					return owner.WndProc(hWnd, msg, wParam, lParam);
				}
				IntPtr result = NativeMethods.CallWindowProc(OriginalProc, hWnd, msg, wParam, lParam);
				if (msg == WM_NCDESTROY)
				{
					lock (_detachedHooks)
					{
						_detachedHooks.Remove(this);
					}
				}
				return result;
			}
		}
	}
}
