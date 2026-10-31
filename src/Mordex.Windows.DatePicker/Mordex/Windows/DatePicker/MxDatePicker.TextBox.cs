using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private void ApplyEditStyle()
		{
			if (_editStyle == EditStyle.Masked)
			{
				RemoveTextBox();
			}
			else
			{
				CreateTextBox();
			}
		}

		private void RemoveTextBox()
		{
			if (_textBox != null)
			{
				DetachTextBoxEvents();
				if (_textBox.ContainsFocus)
				{
					TextBox textBox = _textBox;
					_textBox = null;
					this.Focus();
					textBox.Dispose();
				}
				else
				{
					_textBox.Dispose();
					_textBox = null;
				}
				base.ContextMenuStrip = _contextMenuStrip;
			}
		}

		private void CreateTextBox()
		{
			if (_textBox == null)
			{
				_textBox = new TextBox();
				_textBox.Width = 0;
				_textBox.Visible = !base.DesignMode && base.Enabled;
				_textBox.BackColor = GetEditBackColor();
				_textBox.ForeColor = GetEditForeColor();
				Mordex.Windows.Common.ThemeWindow.ApplyNativeLook(_textBox, Mordex.Windows.Common.MxThemeManager.DefaultTheme);
				_textBox.ReadOnly = _readOnly;
				_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
				base.ContextMenuStrip = null;
				_textBox.ContextMenuStrip = _contextMenuStrip;
				SyncTextBoxFromValue();
				AttachTextBoxEvents();
				base.Controls.Add(_textBox);
				RequestLayout();
			}
		}

		private void AttachTextBoxEvents()
		{
			_textBox.GotFocus += OnTextBoxGotFocus;
			_textBox.LostFocus += OnTextBoxLostFocus;
			_textBox.KeyDown += OnTextBoxKeyDown;
			_textBox.KeyUp += OnTextBoxKeyUp;
			_textBox.KeyPress += OnTextBoxKeyPress;
			_textBox.MouseDown += OnTextBoxMouseDown;
			_textBox.MouseUp += OnTextBoxMouseUp;
			_textBox.MouseMove += OnTextBoxMouseMove;
			_textBox.TextChanged += OnTextBoxTextChanged;
			_textBox.Click += OnTextBoxClick;
			_textBox.DoubleClick += OnTextBoxDoubleClick;
		}

		private void DetachTextBoxEvents()
		{
			_textBox.GotFocus -= OnTextBoxGotFocus;
			_textBox.LostFocus -= OnTextBoxLostFocus;
			_textBox.KeyDown -= OnTextBoxKeyDown;
			_textBox.KeyUp -= OnTextBoxKeyUp;
			_textBox.KeyPress -= OnTextBoxKeyPress;
			_textBox.MouseDown -= OnTextBoxMouseDown;
			_textBox.MouseUp -= OnTextBoxMouseUp;
			_textBox.MouseMove -= OnTextBoxMouseMove;
			_textBox.TextChanged -= OnTextBoxTextChanged;
			_textBox.Click -= OnTextBoxClick;
			_textBox.DoubleClick -= OnTextBoxDoubleClick;
		}

		// Runs the action while the text box events are forwarded to the control, so the handlers can ignore them.
		private void ForwardTextBoxEvent(Action forward)
		{
			_forwardingTextBoxEvent = true;
			try
			{
				forward();
			}
			finally
			{
				_forwardingTextBoxEvent = false;
			}
		}

		private MouseEventArgs ToControlMouseEvent(MouseEventArgs textBoxEvent)
		{
			Point position = PointToClient(Cursor.Position);
			return new MouseEventArgs(textBoxEvent.Button, textBoxEvent.Clicks, position.X, position.Y, textBoxEvent.Delta);
		}

		private void OnTextBoxClick(object sender, EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
		}

		private void OnTextBoxDoubleClick(object sender, EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
		}

		private void OnTextBoxTextChanged(object sender, EventArgs eventArgs)
		{
			ForwardTextBoxEvent(() => OnTextChanged(eventArgs));
		}

		private void OnTextBoxKeyDown(object sender, KeyEventArgs eventArgs)
		{
			ForwardTextBoxEvent(() =>
			{
				OnKeyDown(eventArgs);
				if (!eventArgs.Handled && _droppedDown && _calendar.HandleDialogKey(eventArgs.KeyData))
				{
					eventArgs.Handled = true;
				}
			});
		}

		private void OnTextBoxKeyUp(object sender, KeyEventArgs eventArgs)
		{
			ForwardTextBoxEvent(() => OnKeyUp(eventArgs));
		}

		private void OnTextBoxKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			ForwardTextBoxEvent(() => OnKeyPress(eventArgs));
		}

		private void OnTextBoxMouseDown(object sender, MouseEventArgs eventArgs)
		{
			ForwardTextBoxEvent(() => OnMouseDown(ToControlMouseEvent(eventArgs)));
		}

		private void OnTextBoxMouseUp(object sender, MouseEventArgs eventArgs)
		{
			ForwardTextBoxEvent(() => OnMouseUp(ToControlMouseEvent(eventArgs)));
		}

		private void OnTextBoxMouseMove(object sender, MouseEventArgs eventArgs)
		{
			ForwardTextBoxEvent(() => OnMouseMove(ToControlMouseEvent(eventArgs)));
		}

		private void OnTextBoxGotFocus(object sender, EventArgs eventArgs)
		{
			_textBox.SelectAll();
			if (!_focusingTextBox)
			{
				OnGotFocus(eventArgs);
			}
		}

		private void OnTextBoxLostFocus(object sender, EventArgs eventArgs)
		{
			SyncValueFromTextBox();
			SyncTextBoxFromValue();
			if (!base.ContainsFocus)
			{
				OnLostFocus(eventArgs);
			}
		}
	}
}
