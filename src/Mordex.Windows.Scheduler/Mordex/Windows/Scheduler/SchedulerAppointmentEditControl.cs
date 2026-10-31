using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerAppointmentEditControl : Control
	{
		private Container _container = new Container();

		public TextBox _textBoxControl;

		public MxScheduler _scheduler;

		public SchedulerAppointment _app;

		internal SchedulerAppointmentEditControl(MxScheduler scheduler, SchedulerAppointment app, bool useDefaultMenu)
		{
			InitializeTextBox();
			_scheduler = scheduler;
			_textBoxControl.Font = _scheduler.GetViewFont(null);
			if (!useDefaultMenu)
			{
				_textBoxControl.ContextMenuStrip = new ContextMenuStrip();
			}
			_app = app;
		}

		protected override void Dispose(bool disposing)
		{
			if (_textBoxControl != null)
			{
				_textBoxControl.Dispose();
				_textBoxControl = null;
			}
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Return:
				_scheduler.CommitEdit();
				return true;
			case Keys.Tab:
			case Keys.Tab | Keys.Shift:
				if (_scheduler.AcceptsTab)
				{
					if (_scheduler.TrySelectNextAppointment(keyData))
					{
						return true;
					}
					_scheduler.CommitEdit();
				}
				break;
			}
			if (keyData == Keys.Escape)
			{
				_scheduler.CancelAppointmentEdit(_app);
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		private void InitializeTextBox()
		{
			_container = new Container();
			base.Bounds = Rectangle.Empty;
			_textBoxControl = new TextBox();
			base.Controls.Add(_textBoxControl);
			_textBoxControl.AutoSize = false;
			_textBoxControl.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_textBoxControl.Location = new Point(0, 0);
			_textBoxControl.Multiline = true;
			_textBoxControl.Name = "textBox1";
			_textBoxControl.Size = new Size(0, 0);
			_textBoxControl.TabIndex = 0;
			_textBoxControl.Text = "";
			_textBoxControl.KeyDown += OnTextBoxKeyDown;
			_textBoxControl.KeyPress += OnTextBoxKeyPress;
			_textBoxControl.KeyUp += OnTextBoxKeyUp;
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			_textBoxControl.Bounds = base.ClientRectangle;
			_textBoxControl.SelectionStart = _textBoxControl.Text.Length;
			_textBoxControl.ScrollToCaret();
			SchedulerRenderer.FocusControl(_textBoxControl);
		}

		private void OnTextBoxKeyDown(object sender, KeyEventArgs eventArgs)
		{
			OnKeyDown(eventArgs);
		}

		private void OnTextBoxKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			OnKeyPress(eventArgs);
		}

		private void OnTextBoxKeyUp(object sender, KeyEventArgs eventArgs)
		{
			OnKeyUp(eventArgs);
		}
	}
}
