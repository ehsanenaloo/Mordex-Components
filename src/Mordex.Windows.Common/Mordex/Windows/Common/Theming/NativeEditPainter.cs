using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// Gives a standard text box or combo box the frame of the older Windows looks (blue-gray in Windows XP Blue, olive or
	/// silver in the other schemes) and a Luna drop-down button for the combo box. It listens to the window messages of the
	/// control and paints over the frame after Windows has painted the control. The frame is lit when the mouse is over the
	/// control or the control has the focus.
	/// </summary>
	internal sealed class NativeEditPainter : NativeWindow
	{
		private const int WmPaint = 0x000F;

		private const int WmNcPaint = 0x0085;

		private const uint RedrawInvalidate = 0x0001;

		private const uint RedrawFrame = 0x0400;

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr window);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr window);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr window, IntPtr dc);

		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr window, IntPtr update, IntPtr region, uint flags);

		private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, NativeEditPainter> Painters = new System.Runtime.CompilerServices.ConditionalWeakTable<Control, NativeEditPainter>();

		/// <summary>The painter of a control, or null.</summary>
		internal static NativeEditPainter Find(Control control)
		{
			return Painters.TryGetValue(control, out NativeEditPainter painter) ? painter : null;
		}

		private readonly Control _control;

		private bool _hot;

		internal NativeEditPainter(Control control)
		{
			_control = control;
			Painters.AddOrUpdate(control, this);
			if (control.IsHandleCreated)
			{
				AssignHandle(control.Handle);
			}
			control.HandleCreated += OnHandleCreated;
			control.HandleDestroyed += OnHandleDestroyed;
			control.Enter += OnStateChanged;
			control.Leave += OnStateChanged;
		}

		internal bool IsHot => _hot;

		internal bool IsLit => _control.Enabled && (_hot || _control.ContainsFocus);

		internal void Detach()
		{
			_control.HandleCreated -= OnHandleCreated;
			_control.HandleDestroyed -= OnHandleDestroyed;
			_control.Enter -= OnStateChanged;
			_control.Leave -= OnStateChanged;
			Painters.Remove(_control);
			ReleaseHandle();
		}

		private void OnHandleCreated(object sender, EventArgs e)
		{
			AssignHandle(_control.Handle);
		}

		private void OnHandleDestroyed(object sender, EventArgs e)
		{
			ReleaseHandle();
		}

		private void OnStateChanged(object sender, EventArgs e)
		{
			Redraw();
		}

		private void Redraw()
		{
			if (_control.IsHandleCreated)
			{
				RedrawWindow(_control.Handle, IntPtr.Zero, IntPtr.Zero, RedrawFrame | RedrawInvalidate);
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 0x0200 && !_hot)
			{
				_hot = true;
				Redraw();
			}
			else if (m.Msg == 0x02A3 && _hot)
			{
				_hot = false;
				Redraw();
			}
			base.WndProc(ref m);
			if (m.Msg == WmNcPaint && _control is TextBox)
			{
				IntPtr dc = GetWindowDC(Handle);
				if (dc != IntPtr.Zero)
				{
					try
					{
						using (Graphics g = Graphics.FromHdc(dc))
						{
							DrawFrame(g, new Rectangle(0, 0, _control.Width, _control.Height));
						}
					}
					finally
					{
						ReleaseDC(Handle, dc);
					}
				}
			}
			else if (m.Msg == WmPaint && _control is ComboBox combo)
			{
				IntPtr dc = GetDC(Handle);
				if (dc != IntPtr.Zero)
				{
					try
					{
						using (Graphics g = Graphics.FromHdc(dc))
						{
							DrawCombo(g, combo);
						}
					}
					finally
					{
						ReleaseDC(Handle, dc);
					}
				}
			}
		}

		/// <summary>The color of the frame in the current state.</summary>
		internal static Color FrameColor(ResolvedTheme theme, bool enabled, bool lit)
		{
			return theme.Color(!enabled ? ThemeRole.BorderDisabled : lit ? ThemeRole.BorderHot : ThemeRole.Border);
		}

		private void DrawFrame(Graphics g, Rectangle bounds)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(_control.DeviceDpi);
			g.DrawRectangle(GdiCache.Pen(FrameColor(theme, _control.Enabled, IsLit)), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
		}

		private void DrawCombo(Graphics g, ComboBox combo)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(combo.DeviceDpi);
			Rectangle bounds = combo.ClientRectangle;
			int width = SystemInformation.VerticalScrollBarWidth;
			Rectangle button = new Rectangle(bounds.Right - width, bounds.Y, width, bounds.Height);
			g.FillRectangle(GdiCache.Brush(combo.Enabled ? combo.BackColor : theme.Color(ThemeRole.Surface)), new Rectangle(button.X - 1, button.Y + 1, button.Width, button.Height - 2));
			theme.DrawDropButton(g, button, combo.Enabled, combo.Enabled && _hot, combo.DroppedDown);
			DrawFrame(g, bounds);
		}
	}
}
