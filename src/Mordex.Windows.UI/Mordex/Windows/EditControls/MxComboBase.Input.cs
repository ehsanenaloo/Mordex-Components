using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		internal bool IsMouseOver()
		{
			return _isMouseOver;
		}

		internal void SetMouseOver(bool isMouseOver)
		{
			if (_isMouseOver != isMouseOver)
			{
				_isMouseOver = isMouseOver;
				if (_actAsRibbonEditControl && _textBox != null)
				{
					_textBox.BackColor = GetEffectiveBackColor();
				}
				Invalidate();
			}
		}

		private bool IsButtonPressed()
		{
			return _windowsLayoutState._isPressed;
		}

		private void SetButtonPressed(bool isPressed)
		{
			if (isPressed != _windowsLayoutState._isPressed)
			{
				_windowsLayoutState._isPressed = isPressed;
				AccessibilityNotifyClients(AccessibleEvents.StateChange, DropDownButtonChildId);
				Invalidate();
			}
		}

		private bool IsButtonHot()
		{
			return _windowsLayoutState._isHot;
		}

		private void SetButtonHot(bool isHot)
		{
			if (isHot != _windowsLayoutState._isHot)
			{
				_windowsLayoutState._isHot = isHot;
				Invalidate();
			}
		}

		internal AccessibleObject GetOrCreateAccessibleObject()
		{
			if (_textBox != null)
			{
				return _textBox.AccessibilityObject;
			}
			if (_textAccessibleObject == null)
			{
				_textAccessibleObject = new ComboTextAccessibleObject(this);
			}
			return _textAccessibleObject;
		}

		internal AccessibleObject GetDropDownButtonAccessibleObject()
		{
			if (_dropDownButtonAccessibleObject == null)
			{
				_dropDownButtonAccessibleObject = new ComboDropDownButtonAccessibleObject(this);
			}
			return _dropDownButtonAccessibleObject;
		}

		protected override void OnMouseDown(MouseEventArgs me)
		{
			base.OnMouseDown(me);
			SetButtonHot(false);
			RenderingHelper.TryFocusControl(this);
			if (base.ContainsFocus && !_readOnly && me.Button == MouseButtons.Left)
			{
				if (_windowsLayoutState._isButtonVisible && _windowsLayoutState._buttonBounds.Contains(me.X, me.Y))
				{
					_isButtonMouseDown = true;
					SetButtonPressed(true);
					DropListControl(setCapture: false);
				}
				else if (_editStyle == EditStyle.DropDownList)
				{
					DropListControl(setCapture: false);
				}
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			if (_textBox != null && HasText)
			{
				_textBox.Visible = true;
				_isFocusingTextBox = true;
				_textBox.SelectionStart = 0;
				_textBox.SelectionLength = _textBox.Text.Length;
				RenderingHelper.TryFocusControl(_textBox);
				_isFocusingTextBox = false;
				SetStyle(ControlStyles.Selectable, value: false);
			}
			if (_actAsRibbonEditControl && _textBox != null)
			{
				_textBox.BackColor = GetEffectiveBackColor();
			}
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			if (DroppedDown)
			{
				OnDropHide(cancel: false);
			}
			else
			{
				OnAcceptChanges(EventArgs.Empty);
			}
			base.OnValidating(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			SetStyle(ControlStyles.Selectable, value: true);
			base.OnLostFocus(e);
			if (!base.ContainsFocus)
			{
				RenderingHelper.SetMouseCapture(this, false);
				if (_textBox != null)
				{
					_textBox.SelectionStart = 0;
					_textBox.SelectionLength = 0;
				}
				OnAcceptChanges(EventArgs.Empty);
			}
			if (_actAsRibbonEditControl && _textBox != null)
			{
				_textBox.BackColor = GetEffectiveBackColor();
			}
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs me)
		{
			base.OnMouseMove(me);
			Form form = RenderingHelper.FindForm(this);
			if (form != null && form.ContainsFocus)
			{
				if (_isButtonMouseDown && me.Button == MouseButtons.Left)
				{
					SetButtonPressed(_windowsLayoutState._buttonBounds.Contains(me.X, me.Y));
				}
				if (me.Button == MouseButtons.None)
				{
					SetButtonHot(_windowsLayoutState._buttonBounds.Contains(me.X, me.Y));
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			SetButtonPressed(false);
			_isButtonMouseDown = false;
			SetButtonHot(false);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			if (!base.ClientRectangle.Contains(PointToClient(Cursor.Position)))
			{
				SetMouseOver(false);
			}
			SetButtonPressed(false);
			_isButtonMouseDown = false;
			SetButtonHot(false);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			if (!_isMouseOver)
			{
				Form form = RenderingHelper.FindForm(this);
				if (form != null && form.ContainsFocus)
				{
					SetMouseOver(true);
				}
			}
			base.OnMouseEnter(e);
		}

		protected internal virtual void DropListControl(bool setCapture)
		{
			if (_form != null)
			{
				_form.Deactivate -= OnFormDeactivate;
			}
			_form = RenderingHelper.FindForm(this);
			if (_form != null)
			{
				_form.Deactivate += OnFormDeactivate;
			}
			OnDropDown(EventArgs.Empty);
			Invalidate();
			_droppedDown = true;
		}

		private void OnFormDeactivate(object sender, EventArgs eventArgs)
		{
			OnDropHide(cancel: true);
		}

		void IDropListOwner.OnDropListHide(bool cancel)
		{
			OnDropHide(cancel);
		}

		protected internal virtual void OnVisualStyleChanged()
		{
		}

		protected internal virtual void OnDropHide(bool cancel)
		{
			if (_form != null)
			{
				_form.Deactivate -= OnFormDeactivate;
				_form = null;
			}
			_droppedDown = false;
			SetMouseOver(false);
			OnClosed(EventArgs.Empty);
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnDoMouseWheel(int delta)
		{
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (!((HandledMouseEventArgs)e).Handled)
			{
				((HandledMouseEventArgs)e).Handled = true;
				OnDoMouseWheel(e.Delta);
			}
		}

		protected override bool IsInputChar(char charCode)
		{
			if (char.IsLetterOrDigit(charCode))
			{
				return true;
			}
			return base.IsInputChar(charCode);
		}
	}
}
