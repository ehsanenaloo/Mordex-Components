using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private bool GetIsMouseOver()
		{
			return _isMouseOver;
		}

		private void SetIsMouseOver(bool isMouseOver)
		{
			if (_isMouseOver == isMouseOver)
			{
				return;
			}
			_isMouseOver = isMouseOver;
			if (HoverMode == HoverMode.Highlight || _actAsRibbonEditControl)
			{
				if (_actAsRibbonEditControl && _textBox != null)
				{
					_textBox.BackColor = GetEditBackColor();
				}
				Invalidate();
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
			case Keys.Delete:
				return !_droppedDown;
			default:
				return false;
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

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled || _activePortion == null || _forwardingTextBoxEvent)
			{
				return;
			}
			switch (e.KeyCode)
			{
			case Keys.Delete:
				if (!ReadOnly && _nullable)
				{
					IsNullDate = true;
				}
				break;
			case Keys.Up:
				if (!_readOnly)
				{
					if (_isNullDate)
					{
						IsNullDate = false;
						Value = DateTime.Today;
					}
					else
					{
						Value = _activePortion.GetIncremented(_value, _fieldIncrements, MaxDate);
					}
				}
				break;
			case Keys.Down:
				if (!ReadOnly)
				{
					if (_isNullDate)
					{
						IsNullDate = false;
						Value = DateTime.Today;
					}
					else
					{
						Value = _activePortion.GetDecremented(_value, _fieldIncrements, MinDate);
					}
				}
				break;
			case Keys.Left:
				if (RightToLeft == RightToLeft.No)
				{
					ActivatePreviousPortion();
				}
				else
				{
					ActivateNextPortion();
				}
				break;
			case Keys.Right:
				if (RightToLeft == RightToLeft.No)
				{
					ActivateNextPortion();
				}
				else
				{
					ActivatePreviousPortion();
				}
				break;
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (ReadOnly || e.Handled || _forwardingTextBoxEvent)
			{
				return;
			}
			if (ShowCheckBox && e.KeyChar.Equals(' '))
			{
				Checked = !Checked;
			}
			switch (e.KeyChar)
			{
			case ' ':
				if (!_showCheckBox)
				{
					ActivateNextPortion();
				}
				return;
			case ',':
			case '-':
			case '.':
			case '/':
			case ':':
				ActivateNextPortion();
				return;
			}
			if (_activePortion == null)
			{
				return;
			}
			if (_isNullDate && MaskedValidation == MaskedValidation.OnChange)
			{
				if (_activePortion.TryAcceptChar(e.KeyChar, DateTime.Today) && MaskedValidation == MaskedValidation.OnChange)
				{
					if (IsNullDate)
					{
						_value = DateTime.Today;
					}
					IsNullDate = false;
					Invalidate();
				}
				if (!_isNullDate && _activePortion != null && _activePortion.IsTypedTextComplete())
				{
					if (_activePortion != GetLastEditablePortion())
					{
						ActivateNextPortion();
					}
					else if (MaskedValidation == MaskedValidation.OnChange)
					{
						SetActivePortion(null);
						SetActivePortion(GetLastEditablePortion());
					}
				}
				return;
			}
			if (_activePortion != null && _activePortion.TryAcceptChar(e.KeyChar, _value))
			{
				Invalidate();
			}
			if (_activePortion != null && _activePortion.IsTypedTextComplete())
			{
				if (_activePortion != GetLastEditablePortion())
				{
					ActivateNextPortion();
				}
				else if (MaskedValidation == MaskedValidation.OnChange)
				{
					SetActivePortion(null);
					SetActivePortion(GetLastEditablePortion());
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (_forwardingTextBoxEvent)
			{
				return;
			}
			SetHotButton(null);
			if (_textBox != null)
			{
				_textBox.Focus();
			}
			else
			{
				this.Focus();
			}
			if (base.ContainsFocus && e.Button == MouseButtons.Left && (ReadOnly || !PressButtonAt(e.X, e.Y)))
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Inflate(-_borderWidth, -_borderWidth);
				if (clientRectangle.Contains(e.X, e.Y))
				{
					SetActivePortion(GetPortionAt(e.X, e.Y));
				}
			}
		}

		private void SetPressedButton(PickerButton button)
		{
			if (_pressedButton != button)
			{
				if (_pressedButton != null)
				{
					_pressedButton.SetIsPressed(false);
				}
				_pressedButton = button;
				if (_pressedButton != null)
				{
					_pressedButton.SetIsPressed(true);
				}
				HandlePressedButtonChanged();
			}
		}

		private void HandlePressedButtonChanged()
		{
			if (!base.Enabled)
			{
				_spinRepeatTimer.Enabled = false;
				return;
			}
			if (_pressedButton == _dropDownButton)
			{
				if (IsDropDownEnabled())
				{
					OpenDropDown(true);
				}
			}
			else if (_pressedButton == _checkBoxButton)
			{
				ToggleChecked();
			}
			else if (_pressedButton != null)
			{
				StepValueFromSpinButton(_pressedButton);
			}
			else
			{
				_spinRepeatTimer.Enabled = false;
			}
		}

		private void ToggleChecked()
		{
			Checked = !Checked;
		}

		private void StepValueFromSpinButton(PickerButton spinButton)
		{
			if (_activePortion != null)
			{
				if (_isNullDate)
				{
					IsNullDate = false;
					Value = DateTime.Today;
				}
				else if (spinButton == _upButton)
				{
					Value = _activePortion.GetIncremented(_value, _fieldIncrements, MaxDate);
				}
				else
				{
					Value = _activePortion.GetDecremented(_value, _fieldIncrements, MinDate);
				}
				_spinRepeatTimer.Interval = SpinFirstRepeatDelay;
				_spinRepeatTimer.Enabled = true;
			}
		}

		private bool PressButtonAt(int x, int y)
		{
			PickerButton button = GetButtonAt(x, y);
			SetPressedButton(button);
			return button != null;
		}

		private PickerButton GetButtonAt(int x, int y)
		{
			if (_dropDownButton.ContainsPoint(x, y))
			{
				return _dropDownButton;
			}
			if (_upButton.ContainsPoint(x, y))
			{
				return _upButton;
			}
			if (_downButton.ContainsPoint(x, y))
			{
				return _downButton;
			}
			if (_checkBoxButton.ContainsPoint(x, y))
			{
				return _checkBoxButton;
			}
			return null;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (!_forwardingTextBoxEvent)
			{
				SetPressedButton(null);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!_forwardingTextBoxEvent)
			{
				if (_pressedButton != null)
				{
					_pressedButton.SetIsPressed(_pressedButton.ContainsPoint(e.X, e.Y));
				}
				if (e.Button == MouseButtons.None)
				{
					SetHotButton(GetButtonAt(e.X, e.Y));
				}
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			SetIsMouseOver(true);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHotButton(null);
			if (!base.ClientRectangle.Contains(PointToClient(Cursor.Position)))
			{
				SetIsMouseOver(false);
			}
		}

		private void SetHotButton(PickerButton button)
		{
			if (_hotButton != button)
			{
				if (_hotButton != null)
				{
					_hotButton.SetIsHot(false);
				}
				_hotButton = button;
				if (_hotButton != null)
				{
					_hotButton.SetIsHot(true);
				}
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (_textBox != null)
			{
				_focusingTextBox = true;
				_textBox.Focus();
				_focusingTextBox = false;
				if (_actAsRibbonEditControl)
				{
					_textBox.BackColor = GetEditBackColor();
				}
			}
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			EndEdit();
			base.OnLostFocus(e);
			SetPressedButton(null);
			if (_droppedDown)
			{
				if (_calendar._calendarControl._isShown)
				{
					_calendar._calendarControl.Dismiss();
				}
				_calendar.CloseDropList(true);
			}
			if (_actAsRibbonEditControl && _textBox != null)
			{
				_textBox.BackColor = GetEditBackColor();
			}
			Invalidate();
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			ActivateFirstPortion();
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			if (EditStyle == EditStyle.Masked)
			{
				if (MaskedValidation == MaskedValidation.OnLeave)
				{
					CommitMaskedText();
					SetActivePortion(null);
				}
				else
				{
					SetActivePortion(null);
					BindableValue = BindableValue;
					if (!IsNullDate)
					{
						Value = Value;
					}
				}
			}
			else
			{
				SyncValueFromTextBox();
				SyncTextBoxFromValue();
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_droppedDown)
			{
				if (_calendar.HandleDialogKey(keyData))
				{
					return true;
				}
			}
			else
			{
				switch (keyData)
				{
				case Keys.Return:
					if (_textBox != null && _editStyle == EditStyle.Free)
					{
						SyncValueFromTextBox();
						SyncTextBoxFromValue();
					}
					else if (_maskedValidation == MaskedValidation.OnLeave)
					{
						CommitMaskedText();
					}
					else
					{
						EndEdit();
					}
					break;
				case Keys.V | Keys.Control:
				{
					if (ReadOnly)
					{
						return true;
					}
					IDataObject pasteData = TryGetClipboardData();
					if (pasteData != null)
					{
						if (pasteData.GetDataPresent(typeof(DateTime)))
						{
							Value = (DateTime)pasteData.GetData(typeof(DateTime));
							IsNullDate = false;
							return true;
						}
						if (pasteData.GetDataPresent(typeof(string)))
						{
							string text = (string)pasteData.GetData(typeof(string));
							if (DateTime.TryParse(text, DatePickerHelper.GetDisplayFormat(), DateTimeStyles.None, out DateTime pastedDate))
							{
								Value = pastedDate;
								IsNullDate = false;
							}
						}
					}
					return true;
				}
				case Keys.C | Keys.Control:
					if (BindableValue != DBNull.Value)
					{
						TrySetClipboardValue();
					}
					return true;
				case Keys.X | Keys.Control:
					if (BindableValue != DBNull.Value)
					{
						if (TrySetClipboardValue() && !ReadOnly && _nullable)
						{
							BindableValue = DBNull.Value;
						}
					}
					return true;
				case Keys.F4:
				case Keys.Down | Keys.Alt:
					if (ShowDropDown && !ReadOnly)
					{
						if (_droppedDown)
						{
							CloseDropDown(false);
						}
						else
						{
							OpenDropDown(true);
						}
						return true;
					}
					break;
				case Keys.Escape:
					if (_droppedDown)
					{
						CloseDropDown(false);
						return true;
					}
					break;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheel(e.Delta);
		}

		private void ScrollByWheel(int delta)
		{
			if (_droppedDown)
			{
				_calendar.FirstMonth = DatePickerHelper.AddMonthsClamped(_calendar.FirstMonth, delta < 0 ? 1 : -1);
			}
			else if (!ReadOnly && _activePortion != null)
			{
				if (delta < 0)
				{
					Value = _activePortion.GetDecremented(_value, _fieldIncrements, MinDate);
				}
				else
				{
					Value = _activePortion.GetIncremented(_value, _fieldIncrements, MaxDate);
				}
			}
		}

		private const int ClipboardRetryCount = 5;

		private const int ClipboardRetryDelay = 100;

		// Another program can hold the clipboard open for a moment; the copy is retried and then given up.
		private bool TrySetClipboardValue()
		{
			DataObject data = new DataObject();
			data.SetData(typeof(DateTime), _value);
			data.SetData(typeof(string), DatePickerHelper.FormatDate(_value, GetFormatPattern()));
			try
			{
				Clipboard.SetDataObject(data, true, ClipboardRetryCount, ClipboardRetryDelay);
				return true;
			}
			catch (ExternalException)
			{
				return false;
			}
		}

		private static IDataObject TryGetClipboardData()
		{
			try
			{
				return Clipboard.GetDataObject();
			}
			catch (ExternalException)
			{
				return null;
			}
		}
	}
}
