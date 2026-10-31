using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline
{
	internal class TimelineFormatStyleControl : Control
	{
		internal TextBox _textBoxControl;

		private Container _container = new Container();

		public bool _hasLeftBorder = true;

		public bool _hasRightBorder = true;

		private TimelineItem _item;

		private MxTimeline _timeline;

		public TimelineFormatStyleControl(MxTimeline timeline, TimelineItem item, bool useDefaultMenu)
		{
			InitializeControls();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			_item = item;
			_timeline = timeline;
			_textBoxControl.Bounds = base.ClientRectangle;
			_textBoxControl.Text = item.Text;
			if (!useDefaultMenu)
			{
				_textBoxControl.ContextMenuStrip = new ContextMenuStrip();
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Pen pen = _timeline.GetBorderPen();
			if (_hasLeftBorder)
			{
				eventArgs.Graphics.DrawLine(pen, base.ClientRectangle.Left, base.ClientRectangle.Top, base.ClientRectangle.Left, base.ClientRectangle.Bottom);
			}
			if (_hasRightBorder)
			{
				eventArgs.Graphics.DrawLine(pen, base.ClientRectangle.Right - 1, base.ClientRectangle.Top, base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom);
			}
			eventArgs.Graphics.DrawLine(pen, base.ClientRectangle.Left, base.ClientRectangle.Top, base.ClientRectangle.Right, base.ClientRectangle.Top);
			eventArgs.Graphics.DrawLine(pen, base.ClientRectangle.Left, base.ClientRectangle.Bottom - 1, base.ClientRectangle.Right, base.ClientRectangle.Bottom - 1);
			pen.Dispose();
			pen = null;
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-1, -1);
			_textBoxControl.Bounds = clientRectangle;
			_textBoxControl.SelectionStart = _textBoxControl.Text.Length;
			_textBoxControl.ScrollToCaret();
			TimelineUtilities.FocusControl(_textBoxControl);
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
				_timeline.TryCommitItemEdit();
				return true;
			case Keys.Escape:
				_timeline.DisposeItemEditor();
				return true;
			default:
				return base.ProcessDialogKey(keyData);
			}
		}

		private void InitializeControls()
		{
			_textBoxControl = new TextBox();
			SuspendLayout();
			_textBoxControl.AutoSize = false;
			_textBoxControl.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_textBoxControl.Location = new Point(17, 17);
			_textBoxControl.Name = "textBox1";
			_textBoxControl.Size = new Size(0, 0);
			_textBoxControl.TabIndex = 0;
			_textBoxControl.Text = "";
			_textBoxControl.WordWrap = false;
			base.Controls.AddRange(new Control[1] { _textBoxControl });
			ResumeLayout(performLayout: false);
		}
	}
}
