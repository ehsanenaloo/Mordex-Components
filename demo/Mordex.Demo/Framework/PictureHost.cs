using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Demo
{
	/// <summary>
	/// A window that holds a demo page while a picture of the page is made. It never becomes the active window, so no
	/// control in the page gets the keyboard focus and the picture never shows a focused row, tab or button.
	/// </summary>
	/// <remarks>
	/// Showing a window without activation is not enough: Windows can activate it later, after a click, with Alt+Tab or
	/// when another window closes. WS_EX_NOACTIVATE keeps Windows from activating it and WS_EX_TOOLWINDOW takes it out of
	/// Alt+Tab.
	/// </remarks>
	public sealed class PictureHost : Form
	{
		private const int WsExToolWindow = 0x00000080;
		private const int WsExNoActivate = 0x08000000;
		private const int WmMouseActivate = 0x0021;
		private const int MaNoActivate = 3;

		public PictureHost(Point location, Size size)
		{
			StartPosition = FormStartPosition.Manual;
			Location = location;
			Size = size;
			ShowInTaskbar = false;
		}

		protected override bool ShowWithoutActivation => true;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams p = base.CreateParams;
				p.ExStyle |= WsExNoActivate | WsExToolWindow;
				return p;
			}
		}

		/// <summary>
		/// Takes the keyboard focus away from the page, for a focus that came in some other way. Returns true when the
		/// window was active or a control in it had the focus.
		/// </summary>
		public bool DropFocus()
		{
			bool focused = ActiveForm == this || (ContainsFocus && !Focused);
			if (focused || ActiveControl != null)
			{
				ActiveControl = null;
			}
			return focused;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmMouseActivate)
			{
				m.Result = new IntPtr(MaNoActivate);
				return;
			}
			base.WndProc(ref m);
		}
	}
}
