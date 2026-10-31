using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class MaskEdit
	{
		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Left:
			case Keys.Right:
				if (!_isDropHideAllowed)
				{
					return false;
				}
				if (!HasColumnContainerParent())
				{
					return base.IsInputKey(keyData);
				}
				break;
			case Keys.Prior:
			case Keys.Next:
			case Keys.End:
			case Keys.Home:
			case Keys.Up:
			case Keys.Down:
				if (_isDropDownShown)
				{
					return false;
				}
				break;
			}
			switch (keyData)
			{
			case Keys.Return:
				return false;
			case Keys.Escape:
				return false;
			case Keys.Right:
				if (base.SelectionStart == base.Text.Length && base.SelectionLength == 0 && RightToLeft == RightToLeft.No)
				{
					return false;
				}
				if (base.SelectionStart == 0 && base.SelectionLength == 0 && RightToLeft == RightToLeft.Yes)
				{
					return false;
				}
				return base.IsInputKey(keyData);
			case Keys.Left:
				if (base.SelectionStart == 0 && base.SelectionLength == 0 && RightToLeft == RightToLeft.No)
				{
					return false;
				}
				if (base.SelectionStart == base.Text.Length && base.SelectionLength == 0 && RightToLeft == RightToLeft.Yes)
				{
					return false;
				}
				return base.IsInputKey(keyData);
			case Keys.Up:
			case Keys.Down:
				if (base.Multiline && HasColumnContainerParent())
				{
					if (keyData == Keys.Up)
					{
						if (GetLineFromCharIndex(base.SelectionStart) == 0)
						{
							return false;
						}
					}
					else if (GetLineFromCharIndex(base.SelectionStart) == GetLineFromCharIndex(Text.Length))
					{
						return false;
					}
					return base.IsInputKey(keyData);
				}
				if (IsInsideColumnContainer())
				{
					return false;
				}
				return base.IsInputKey(keyData);
			case Keys.Prior:
			case Keys.Next:
				return false;
			default:
				return base.IsInputKey(keyData);
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (!e.Handled && !base.ReadOnly && (!char.IsControl(e.KeyChar) || e.KeyChar == '\b'))
			{
				e.Handled = HandleMaskedChar(e.KeyChar);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (_hasMask && e.KeyCode == Keys.Delete && !base.ReadOnly && !e.Handled)
			{
				int selectionStart = base.SelectionStart;
				ClearSlots(base.SelectionStart, SelectionLength);
				ApplyText(GetDisplayText(Focused));
				SetCaretPosition(selectionStart);
				SelectionLength = 0;
				e.Handled = true;
			}
		}

		internal void AddKeyPressedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _keyPressedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _keyPressedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		internal void RemoveKeyPressedHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _keyPressedHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _keyPressedHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected internal virtual void OnKeyPressed(EventArgs e)
		{
			if (_keyPressedHandler != null)
			{
				_keyPressedHandler(this, e);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == (Keys.I | Keys.Control) && !Multiline)
			{
				return true;
			}
			if (_handlesReturnAndEscape)
			{
				switch (keyData)
				{
				case Keys.Return:
					OnReturnKey(EventArgs.Empty);
					return true;
				case Keys.Escape:
					OnEscapeKey(EventArgs.Empty);
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnReturnKey(EventArgs e)
		{
		}

		internal void AddEscapeKeyHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _escapeKeyHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _escapeKeyHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		internal void RemoveEscapeKeyHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _escapeKeyHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _escapeKeyHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnEscapeKey(EventArgs e)
		{
			if (_escapeKeyHandler != null)
			{
				_escapeKeyHandler(this, e);
			}
		}
	}
}
