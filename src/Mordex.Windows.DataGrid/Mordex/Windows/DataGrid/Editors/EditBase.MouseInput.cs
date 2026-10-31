using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
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
		protected override void OnMouseDown(MouseEventArgs me)
		{
			if (!_isDragOver)
			{
				try
				{
					Select();
				}
				finally
				{
				}
				GridHelper.SelectAndFocus(this);
				SetHotButton(-1);
				if (base.Capture && base.ContainsFocus && me.Button == MouseButtons.Left)
				{
					SetPressedButton(HitTestButton(me.X, me.Y));
					if (_pressedButtonIndex != -1)
					{
						ProcessMouseInput(_pressedButtonIndex, me);
						return;
					}
					if (TreatControlAreaAsButton)
					{
						ProcessMouseInput(0, me);
					}
					else if (me.Clicks == 2)
					{
						OnDoubleClick(EventArgs.Empty);
					}
				}
			}
			base.OnMouseDown(me);
		}

		protected override void OnMouseMove(MouseEventArgs me)
		{
			base.OnMouseMove(me);
			if (_isDragOver)
			{
				return;
			}
			if (_pressedButtonIndex != -1)
			{
				bool condition = _buttonStates[_pressedButtonIndex]._isPressed;
				_buttonStates[_pressedButtonIndex]._isPressed = _buttonStates[_pressedButtonIndex]._bounds.Contains(me.X, me.Y);
				_buttonStates[_pressedButtonIndex]._isHot = !_buttonStates[_pressedButtonIndex]._isPressed;
				if (condition != _buttonStates[_pressedButtonIndex]._isPressed)
				{
					if (_buttonStates[_pressedButtonIndex]._isPressed)
					{
						ResumeRepeat();
					}
					else
					{
						SuspendRepeat();
					}
					Invalidate();
				}
			}
			if (me.Button == MouseButtons.None)
			{
				SetHotButton(HitTestButton(me.X, me.Y));
			}
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			if (!_isDragOver)
			{
				if (_pressedButtonIndex != -1 && _buttonStates[_pressedButtonIndex]._bounds.Contains(me.X, me.Y) && GetButtonForIndex(_pressedButtonIndex) == EditButton.Button)
				{
					RaiseButtonClick(_pressedButtonIndex);
				}
				StopRepeat();
				SetHotButton(-1);
				SetPressedButton(-1);
			}
		}

		private bool IsMouseOver()
		{
			return _isMouseOver;
		}

		private void SetMouseOver(bool isMouseOver)
		{
			try
			{
				if (isMouseOver && (base.TopLevelControl == null || !base.TopLevelControl.ContainsFocus))
				{
					isMouseOver = false;
				}
				if (_isMouseOver == isMouseOver)
				{
					return;
				}
				_isMouseOver = isMouseOver;
				if (_actAsRibbonEditControl)
				{
					if (_isMouseOver)
					{
						BackColor = Color.White;
					}
					else if (!base.ContainsFocus)
					{
						BackColor = Color.White;
					}
				}
				if (_hoverMode == HoverMode.Highlight || _actAsRibbonEditControl)
				{
					Invalidate();
				}
			}
			finally
			{
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			Point pt = PointToClient(Cursor.Position);
			Control control = null;
			try
			{
				control = GetChildAtPoint(pt);
			}
			finally
			{
			}
			if (control == null)
			{
				base.OnMouseLeave(e);
				SetMouseOver(false);
				_hoverRaised = false;
			}
			SetHotButton(-1);
			SetPressedButton(-1);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			if (!_isMouseOver)
			{
				base.OnMouseEnter(e);
				SetMouseOver(true);
			}
		}

		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);
			_hoverRaised = true;
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			if (_textBox == null || _isDragOver)
			{
				base.OnDragEnter(e);
			}
		}

		protected override void OnDragLeave(EventArgs e)
		{
			if (_textBox == null || _isDragOver)
			{
				base.OnDragLeave(e);
			}
		}

		private int HitTestButton(int value, int otherValue)
		{
			if (!ButtonEnabled)
			{
				return -1;
			}
			if (!_contentBounds.IsEmpty)
			{
				if (_buttonStates[0]._bounds.Contains(value, otherValue))
				{
					return 0;
				}
				if (_buttonType == EditButtonType.UpDownButtons && _buttonStates[1]._bounds.Contains(value, otherValue))
				{
					return 1;
				}
			}
			return -1;
		}

		private void OnEditControlMouseHover(object sender, EventArgs eventArgs)
		{
			if (!_hoverRaised)
			{
				OnMouseHover(eventArgs);
			}
		}

		internal void RaiseClick(EventArgs eventArgs)
		{
			OnClick(eventArgs);
		}

		private void OnEditControlClick(object sender, EventArgs eventArgs)
		{
			OnClick(eventArgs);
		}

		private void OnEditControlDoubleClick(object sender, EventArgs eventArgs)
		{
			OnDoubleClick(eventArgs);
		}

		private void OnEditControlDragEnter(object sender, DragEventArgs eventArgs)
		{
			_isDragOver = true;
			OnDragEnter(eventArgs);
			_isDragOver = false;
		}

		private void OnEditControlDragDrop(object sender, DragEventArgs eventArgs)
		{
			OnDragDrop(eventArgs);
		}

		private void OnEditControlDragLeave(object sender, EventArgs eventArgs)
		{
			_isDragOver = true;
			OnDragLeave(eventArgs);
			_isDragOver = false;
		}

		private void OnEditControlDragOver(object sender, DragEventArgs eventArgs)
		{
			OnDragOver(eventArgs);
		}

		private void OnEditControlMouseDown(object sender, MouseEventArgs eventArgs)
		{
			Point p = _textBox.PointToScreen(new Point(eventArgs.X, eventArgs.Y));
			p = PointToClient(p);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
			_isDragOver = true;
			OnMouseDown(e);
			_isDragOver = false;
		}

		private void OnEditControlMouseClick(object sender, MouseEventArgs eventArgs)
		{
			Point p = _textBox.PointToScreen(new Point(eventArgs.X, eventArgs.Y));
			p = PointToClient(p);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
			_isDragOver = true;
			OnMouseClick(e);
			_isDragOver = false;
		}

		private void OnEditControlMouseDoubleClick(object sender, MouseEventArgs eventArgs)
		{
			Point p = _textBox.PointToScreen(new Point(eventArgs.X, eventArgs.Y));
			p = PointToClient(p);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
			OnMouseDoubleClick(e);
		}

		private void OnEditControlMouseMove(object sender, MouseEventArgs eventArgs)
		{
			Point p = _textBox.PointToScreen(new Point(eventArgs.X, eventArgs.Y));
			p = PointToClient(p);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
			OnMouseMove(e);
		}

		private void OnEditControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			Point p = _textBox.PointToScreen(new Point(eventArgs.X, eventArgs.Y));
			p = PointToClient(p);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
			_isDragOver = true;
			OnMouseUp(e);
			_isDragOver = false;
		}

		private void OnEditControlMouseEnter(object sender, EventArgs eventArgs)
		{
			if (!_isMouseOver)
			{
				OnMouseEnter(eventArgs);
			}
		}

		private void OnEditControlMouseLeave(object sender, EventArgs eventArgs)
		{
			Point pt = PointToClient(Cursor.Position);
			if (!base.ClientRectangle.Contains(pt))
			{
				OnMouseLeave(eventArgs);
			}
		}

		private void OnEditControlMouseWheel(object sender, MouseEventArgs eventArgs)
		{
		}

		public void PerformButtonClick()
		{
			if (ButtonStyle != EditButtonStyle.NoButton)
			{
				OnButtonClick(EventArgs.Empty);
			}
		}

		protected virtual void OnButtonClick(EventArgs e)
		{
			if (base.Events[_editBaseValue] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		private void RaiseButtonClick(int value)
		{
			EditButton editButton = GetButtonForIndex(value);
			OnButtonClick(GetButtonForIndex(value));
			if (editButton == EditButton.Button)
			{
				OnButtonClick(EventArgs.Empty);
			}
		}

		protected virtual void OnButtonClick(EditButton editButton)
		{
		}

		private void ProcessMouseInput(int value, MouseEventArgs eventArgs)
		{
			if (ButtonEnabled)
			{
				EditButton editButton = GetButtonForIndex(value);
				OnButtonMouseDown(editButton);
				switch (editButton)
				{
				case EditButton.DownButton:
					StartRepeatDown();
					break;
				case EditButton.UpButton:
					StartRepeatUp();
					break;
				}
			}
		}

		protected virtual void OnButtonMouseDown(EditButton editButton)
		{
		}

		internal virtual void DoButtonClick(int index)
		{
			RaiseButtonClick(index);
		}
	}
}
