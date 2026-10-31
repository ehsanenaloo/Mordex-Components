using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>Applies the kind of a theme to the frame of a window: the dark title bar of Windows 10 and 11.</summary>
	public static class ThemeWindow
	{
		private const int UseImmersiveDarkMode = 20;

		private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, MxTheme> NativeLooks = new System.Runtime.CompilerServices.ConditionalWeakTable<Control, MxTheme>();

		// The title bar that each form should have. Every new handle of the form gets it.
		private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Form, TitleBar> TitleBars = new System.Runtime.CompilerServices.ConditionalWeakTable<Form, TitleBar>();

		private sealed class TitleBar
		{
			internal bool Dark;
		}

		// The theme whose kind the native parts of a control were last given; null when none or when the control got its own look back.
		internal static MxTheme NativeLookOf(Control control)
		{
			return NativeLooks.TryGetValue(control, out MxTheme theme) ? theme : null;
		}

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr window, string subAppName, string subIdList);

		/// <summary>
		/// Sets the title bar of the form to dark or light to match the theme. The form keeps the latest setting: a handle
		/// made later, also when the form makes its handle again, gets it. Returns false when Windows has no such setting.
		/// </summary>
		public static bool Apply(Form form, MxTheme theme)
		{
			if (form == null)
			{
				throw new ArgumentNullException(nameof(form));
			}
			if (theme == null)
			{
				throw new ArgumentNullException(nameof(theme));
			}
			if (!TitleBars.TryGetValue(form, out TitleBar titleBar))
			{
				titleBar = new TitleBar();
				TitleBars.Add(form, titleBar);
				form.HandleCreated += OnTitleBarHandleCreated;
			}
			titleBar.Dark = theme.Kind == ThemeKind.Dark;
			if (!form.IsHandleCreated)
			{
				return true;
			}
			return Set(form.Handle, titleBar.Dark);
		}

		private static void OnTitleBarHandleCreated(object sender, EventArgs e)
		{
			Form form = (Form)sender;
			if (TitleBars.TryGetValue(form, out TitleBar titleBar))
			{
				Set(form.Handle, titleBar.Dark);
			}
		}

		/// <summary>
		/// Gives the scroll bars and the other native parts of a standard Windows control (a text box, a list, a tree, a list
		/// view, a scrolling panel) the dark or the light look of Windows that matches the theme. Windows draws these parts
		/// itself, so their colors are the colors of Windows and not the colors of the theme; only the kind of the theme is
		/// used. Pass a null theme to give the control its own look back. Returns false when Windows has no such setting.
		/// </summary>
		public static bool ApplyNativeLook(Control control, MxTheme theme)
		{
			if (control == null)
			{
				throw new ArgumentNullException(nameof(control));
			}
			if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
			{
				return false;
			}
			bool pending = NativeLooks.TryGetValue(control, out _) && !control.IsHandleCreated;
			NativeLooks.Remove(control);
			if (theme != null)
			{
				NativeLooks.Add(control, theme);
			}
			if (!control.IsHandleCreated)
			{
				// One handler is enough: it reads the latest theme when the handle comes.
				if (!pending)
				{
					EventHandler apply = null;
					apply = (sender, e) =>
					{
						control.HandleCreated -= apply;
						SetNative(control, NativeLookOf(control));
					};
					control.HandleCreated += apply;
				}
				return true;
			}
			return SetNative(control, theme);
		}

		private static bool SetNative(Control control, MxTheme theme)
		{
			bool dark = theme != null && theme.Kind == ThemeKind.Dark;
			// The dark Explorer look leaves the face of a combo box light; the dark look of the file dialogs has a dark face.
			string darkName = control is ComboBox ? "DarkMode_CFD" : "DarkMode_Explorer";
			string name = theme == null ? null : dark ? darkName : "Explorer";
			bool done = SetWindowTheme(control.Handle, name, null) == 0;
			if (control is ListView)
			{
				// The column header is a window of its own. An owner drawn list draws the columns, but the header draws the
				// part right of the last column with its own theme, which stays light unless the header gets the dark one.
				IntPtr header = SendMessage(control.Handle, ListViewGetHeader, IntPtr.Zero, IntPtr.Zero);
				if (header != IntPtr.Zero)
				{
					SetWindowTheme(header, dark ? "DarkMode_ItemsView" : null, null);
				}
			}
			return done;
		}

		private const int ListViewGetHeader = 0x101F;

		[DllImport("user32.dll", EntryPoint = "SendMessageW")]
		private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

		private static bool Set(IntPtr handle, bool dark)
		{
			if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
			{
				return false;
			}
			int value = dark ? 1 : 0;
			return DwmSetWindowAttribute(handle, UseImmersiveDarkMode, ref value, sizeof(int)) == 0;
		}
	}
}
