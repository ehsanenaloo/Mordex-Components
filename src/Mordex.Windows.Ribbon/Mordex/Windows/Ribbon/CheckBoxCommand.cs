using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.Ribbon
{
	public class CheckBoxCommand : ContainerControlCommand
	{
		private MxCheckBox _checkBox;

		[Description("Represents the CheckBox control associated with the command.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MxCheckBox CheckBox
		{
			get
			{
				if (_checkBox == null)
				{
					_checkBox = new MxCheckBox();
					_checkBox.ShowFocusRectangle = false;
					_checkBox.Enabled = Enabled;
					_checkBox.BackColor = Color.Transparent;
					_checkBox.Height = 22;
					_checkBox.AutoSize = false;
					_checkBox.CheckedChanged += OnCheckBoxCheckedChanged;
				}
				return _checkBox;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override Control Control
		{
			get
			{
				return CheckBox;
			}
			set
			{
				throw new InvalidOperationException("It is not possible to set the control for this command. Use a ContainerControlCommand instead");
			}
		}

		public CheckBoxCommand()
		{
		}

		public CheckBoxCommand(string key)
			: base(key)
		{
		}

		public CheckBoxCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public CheckBoxCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}

		protected override void OnBeforePaint()
		{
			base.OnBeforePaint();
			if (CheckBox != null && base.Ribbon != null)
			{
				CheckBox.ForeColor = base.Ribbon._colorScheme._foreColorDark;
			}
		}

		private void OnCheckBoxCheckedChanged(object sender, EventArgs eventArgs)
		{
			base.Checked = _checkBox.Checked;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && CheckBox != null)
			{
				CheckBox.CheckedChanged -= OnCheckBoxCheckedChanged;
				_checkBox.Dispose();
				_checkBox = null;
			}
		}

		protected override void OnCheckedChanged(CommandEventArgs e)
		{
			if (CheckBox != null)
			{
				CheckBox.Checked = base.Checked;
			}
			base.OnCheckedChanged(e);
		}

		internal override bool SearchShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			if (!base.Visible || !Enabled)
			{
				return false;
			}
			if (base.Shortcut == shortcut)
			{
				if (inputControl == null || inputControl.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
				{
					if (!base.CheckOnClick && !base.ActAsOptionButton)
					{
						base.Checked = !base.Checked;
					}
					InvokeOnClick();
				}
				else if (inputControl != null)
				{
					NativeMethods.NativeMessage n = new NativeMethods.NativeMessage
					{
						_windowHandle = msg.HWnd,
						_lParam = msg.LParam,
						_wParam = msg.WParam,
						_message = msg.Msg
					};
					NativeMethods.TranslateMessage(ref n);
					NativeMethods.DispatchMessage(ref n);
				}
				return true;
			}
			return false;
		}
	}
}
