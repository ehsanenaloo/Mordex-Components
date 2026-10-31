using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	internal class GridDropDownState : ControlStyleContainerControl
	{
		internal GridDropDown _gridDropDownStateDropControl;

		private bool _isAutoCompleteEnabled = true;

		private bool _isSyncingText;

		private object _gridDropDownStateValue;

		private bool _isChangeSuppressed;

		internal GridDropDownState(GridDropDown dropDownList)
		{
			_gridDropDownStateDropControl = dropDownList;
		}

		public GridDropDown GetDropDownList()
		{
			return _gridDropDownStateDropControl;
		}

		public void SetDropDownList(GridDropDown gridDropDownStateDropControl)
		{
			_gridDropDownStateDropControl = gridDropDownStateDropControl;
		}

		protected internal override Control GetDropControl()
		{
			return _gridDropDownStateDropControl;
		}

		protected override void DropListControl(bool setCapture)
		{
			if (_gridDropDownStateDropControl != null)
			{
				_gridDropDownStateDropControl.EnsureGraphicsReady();
			}
			base.DropListControl(setCapture);
			if (_gridDropDownStateDropControl != null)
			{
				Rectangle rectangle = GetVirtualClientRectangle();
				rectangle.Location = PointToScreen(rectangle.Location);
				_gridDropDownStateDropControl.SelectRowByValue(_gridDropDownStateValue);
				if (_gridDropDownStateValue != null && _gridDropDownStateDropControl.GetSelectedValue() == null)
				{
					_gridDropDownStateDropControl.SelectByPrefix(_gridDropDownStateValue as string);
				}
				_gridDropDownStateDropControl.ScrollSelectedRowToTop();
				_gridDropDownStateDropControl.AssignControlStyle(GetControlStyle());
				_gridDropDownStateDropControl.ShowDropDown(rectangle, this);
				Cursor = Cursors.Arrow;
				GridHelper.SetCapture(_gridDropDownStateDropControl, setCapture);
			}
		}

		protected override void OnDropHide(bool cancel)
		{
			if (_gridDropDownStateDropControl == null)
			{
				return;
			}
			_gridDropDownStateDropControl.HideDropDown(cancel);
			if (!cancel)
			{
				try
				{
					SetCurrentValue(_gridDropDownStateDropControl.GetSelectedValue());
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				if (_textBox != null)
				{
					SelectRange(Text.Length, 0);
					GridHelper.SelectAndFocus(_textBox);
				}
			}
			Cursor = Cursors.Default;
			base.OnDropHide(cancel);
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			if (GetIsDroppedDown() && _gridDropDownStateDropControl != null)
			{
				GridHelper.SetCapture(_gridDropDownStateDropControl, true);
			}
		}

		protected override void OnMouseMove(MouseEventArgs me)
		{
			base.OnMouseMove(me);
			if (GetIsDroppedDown() && _gridDropDownStateDropControl != null)
			{
				Point mousePosition = Control.MousePosition;
				if (_gridDropDownStateDropControl.Bounds.Contains(mousePosition.X, mousePosition.Y))
				{
					GridHelper.SetCapture(_gridDropDownStateDropControl, true);
				}
			}
		}

		protected override void OnTextChanged(EventArgs eventArgs)
		{
			base.OnTextChanged(eventArgs);
			if (_gridDropDownStateDropControl != null)
			{
				if (!_isSyncingText)
				{
					if (_textBox is MaskEdit && ((MaskEdit)_textBox)._isTypingForward)
					{
						return;
					}
					if (Text.Length == 0)
					{
						_gridDropDownStateValue = null;
					}
					else
					{
						_gridDropDownStateValue = _gridDropDownStateDropControl.FindValueByDisplayText(Text);
					}
				}
			}
			else
			{
				_gridDropDownStateValue = Text;
			}
			if (!_isChangeSuppressed && !_isSyncingText)
			{
				OnChange(EventArgs.Empty);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (GetIsDroppedDown() && _gridDropDownStateDropControl != null && _gridDropDownStateDropControl.HandleNavigationKey(keyData))
			{
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		public void SetAutoCompleteEnabled(bool gridDropDownStateBoolean)
		{
			_isAutoCompleteEnabled = gridDropDownStateBoolean;
		}

		protected override void OnKeyPressed(EventArgs eventArgs)
		{
			base.OnKeyPressed(eventArgs);
			AutoCompleteText();
		}

		protected override void OnKeyPress(KeyPressEventArgs eventArgs)
		{
			base.OnKeyPress(eventArgs);
			if (!eventArgs.Handled)
			{
				InternalProcessChar(eventArgs.KeyChar);
			}
		}

		protected override void OnKeyDown(KeyEventArgs eventArgs)
		{
			base.OnKeyDown(eventArgs);
			if (!eventArgs.Handled && eventArgs.KeyCode == Keys.Delete && GetEditStyle() == EditStyle.DropDownList)
			{
				SetCurrentValue(null);
			}
		}

		protected override void InternalProcessChar(char keyChar)
		{
			if (_gridDropDownStateDropControl != null && GetEditStyle() == EditStyle.DropDownList && char.IsLetterOrDigit(keyChar))
			{
				string text = keyChar.ToString(GridHelper.GetCurrentCulture());
				object nextByFirstChar = _gridDropDownStateDropControl.FindNextByFirstChar(GetCurrentValue(), text);
				if (nextByFirstChar != null)
				{
					SetCurrentValue(nextByFirstChar);
				}
			}
		}

		private void AutoCompleteText()
		{
			if (_gridDropDownStateDropControl == null)
			{
				return;
			}
			if (_isAutoCompleteEnabled)
			{
				if (Text.Length > 0)
				{
					string text = Text;
					if (_textBox != null && GetEditStyle() == EditStyle.Combo)
					{
						text = _textBox.Text;
					}
					object selectByPrefix = _gridDropDownStateDropControl.SelectByPrefix(text);
					if (selectByPrefix != null)
					{
						string displayTextForValue = _gridDropDownStateDropControl.GetDisplayTextForValue(selectByPrefix, "", false);
						SetCurrentValue(selectByPrefix);
						_isSyncingText = true;
						Text = text + displayTextForValue.Substring(text.Length);
						SelectRange(Text.Length, text.Length);
						_isSyncingText = false;
					}
					else if (!(_gridDropDownStateValue is string) || (string)_gridDropDownStateValue != text)
					{
						_gridDropDownStateValue = text;
						OnChange(EventArgs.Empty);
					}
				}
				else
				{
					_gridDropDownStateValue = null;
				}
			}
			else
			{
				object valueByDisplayText = _gridDropDownStateDropControl.FindValueByDisplayText(Text);
				if (_gridDropDownStateValue != valueByDisplayText)
				{
					_gridDropDownStateValue = valueByDisplayText;
					OnChange(EventArgs.Empty);
				}
			}
		}

		internal void SyncValueFromText()
		{
			if (_gridDropDownStateDropControl != null)
			{
				if (!_isSyncingText)
				{
					if (Text.Length == 0)
					{
						_gridDropDownStateValue = null;
					}
					else
					{
						_gridDropDownStateValue = _gridDropDownStateDropControl.FindValueByDisplayText(Text);
					}
				}
			}
			else
			{
				_gridDropDownStateValue = Text;
			}
		}

		internal void SetValueFromText(string text)
		{
			SetCurrentValue(_gridDropDownStateDropControl.ParseValueText(text));
		}

		public object GetCurrentValue()
		{
			return _gridDropDownStateValue;
		}

		public void SetCurrentValue(object gridDropDownStateValue)
		{
			if (gridDropDownStateValue == _gridDropDownStateValue)
			{
				return;
			}
			_gridDropDownStateValue = gridDropDownStateValue;
			_isSyncingText = true;
			if (_gridDropDownStateDropControl != null && _gridDropDownStateDropControl.ReplaceValues)
			{
				Text = _gridDropDownStateDropControl.GetDisplayTextForValue(_gridDropDownStateValue, "", false);
				if (GetIsImageSet() && _gridDropDownStateDropControl.ImageColumn != null)
				{
					SetImage(_gridDropDownStateDropControl.GetImageForValue(_gridDropDownStateValue));
				}
			}
			else if (GridHelper.IsNullOrDBNull(gridDropDownStateValue))
			{
				Text = "";
			}
			else
			{
				Text = GridHelper.ToSafeString(gridDropDownStateValue);
			}
			_isSyncingText = false;
			OnChange(EventArgs.Empty);
		}

		internal void SetValueSilently(object value)
		{
			_isChangeSuppressed = true;
			SetCurrentValue(value);
			_isChangeSuppressed = false;
		}

		public bool IsTextNotInList()
		{
			if (GetEditStyle() != EditStyle.DropDownList && _gridDropDownStateDropControl != null && _gridDropDownStateDropControl.ReplaceValues && Text.Length > 0 && !_gridDropDownStateDropControl.IsDisplayTextInList(Text))
			{
				return true;
			}
			return false;
		}

		protected override bool OnDoMouseWheel(int delta)
		{
			if (_gridDropDownStateDropControl != null && GetIsDroppedDown())
			{
				((ScrollableBase)_gridDropDownStateDropControl).ScrollByWheelDelta(delta);
				return true;
			}
			return false;
		}
	}
}
