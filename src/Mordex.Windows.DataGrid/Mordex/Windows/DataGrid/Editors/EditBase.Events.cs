using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class EditBase
	{
		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			RecalculateLayout();
		}

		private void HandleControlStyleChanged(object sender, EventArgs eventArgs)
		{
			((IGridStyleSource)this).OnControlStyleChanged();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			RecalculateLayout();
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			RecalculateLayout();
			base.OnRightToLeftChanged(e);
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (_textBox != null && _textBox.Text != base.Text)
			{
				_textBox.Text = base.Text;
			}
			Invalidate();
			base.OnTextChanged(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			RecalculateLayout();
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);
			if (_textBox != null)
			{
				if (!base.Enabled && BackColor.Equals(SystemColors.Window))
				{
					_textBox.BackColor = SystemColors.Control;
				}
				else
				{
					_textBox.BackColor = BackColor;
				}
			}
		}

		protected override void OnForeColorChanged(EventArgs e)
		{
			base.OnForeColorChanged(e);
			if (_textBox != null)
			{
				_textBox.ForeColor = ForeColor;
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			if (EditControl != null)
			{
				EditControl.Select();
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
		}

		protected override void OnLeave(EventArgs e)
		{
			if (EditControl == null)
			{
				base.OnLostFocus(e);
			}
			DisposeResources();
			base.OnLeave(e);
			Invalidate();
		}

		protected override void OnEnter(EventArgs e)
		{
			if (EditControl == null)
			{
				base.OnGotFocus(e);
			}
			else if (!EditControl.ContainsFocus)
			{
				GridHelper.SelectAndFocus(EditControl);
			}
			base.OnEnter(e);
			if (_actAsRibbonEditControl)
			{
				BackColor = Color.White;
			}
			Invalidate();
		}

		protected virtual void OnTextBoxTextChanged(EventArgs e)
		{
			base.Text = _textBox.Text;
		}

		void IGridStyleSource.OnControlStyleChanged()
		{
			Invalidate();
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			if (base.Events[_readOnlyChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			if (_officeStyle != null)
			{
				_officeStyle.GetLayoutState().ResetCachedStyles();
				_officeStyle.ReleaseLayoutState();
			}
			RecalculateLayout();
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_repeatHadCapture && !base.Capture)
			{
				DisposeResources();
			}
			else if (_repeatDirection >= 0)
			{
				if (_repeatDirection == 1)
				{
					OnButtonClick(EditButton.UpButton);
				}
				else
				{
					OnButtonClick(EditButton.DownButton);
				}
				_timer.Interval = Timing.AutoRepeatInterval;
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			NativeMethods.RemoveProp(base.Handle, "PROP_ISEDITBASE");
			StopListeningToThemes();
			base.OnHandleDestroyed(e);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			NativeMethods.SetProp(base.Handle, "PROP_ISEDITBASE", new IntPtr(1));
			UpdateThemeListening();
		}
	}
}
