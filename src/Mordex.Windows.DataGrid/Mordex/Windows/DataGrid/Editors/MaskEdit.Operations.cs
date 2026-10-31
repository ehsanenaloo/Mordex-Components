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
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ContextMenuStrip = new ContextMenuStrip();
			}
			base.Dispose(disposing);
		}

		protected override void OnEnter(EventArgs e)
		{
			if (_hasMask)
			{
				_isTextChangeEnabled = false;
				ApplyText(GetDisplayText(true));
				_isTextChangeEnabled = true;
			}
			if (!HasColumnContainerParent() && _isFirstEnter)
			{
				SelectAll();
			}
			base.OnEnter(e);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			Select(base.SelectionStart, SelectionLength);
			base.OnGotFocus(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			if (_hasMask)
			{
				_isTextChangeEnabled = false;
				ApplyText(GetDisplayText(false));
				_isTextChangeEnabled = true;
			}
			base.OnLeave(e);
		}

		private bool IsFormattedTextValid(string text, bool condition)
		{
			if (text.Length > _maskSlots.Length)
			{
				return false;
			}
			ClearSlots(0, _maskSlots.Length);
			int startIndex = 0;
			if (_isMaskDataSynced)
			{
				for (int i = 0; i < _maskSlots.Length; i++)
				{
					if (text.Length <= startIndex)
					{
						continue;
					}
					if (_maskSlots[i]._isLiteral)
					{
						if (text.Substring(startIndex, 1) != _maskSlots[i]._rawText)
						{
							return false;
						}
						startIndex++;
					}
					else if (!IsCharAllowedInSlot(i, text[startIndex]))
					{
						if (_maskSlots[i]._isTextValid)
						{
							if (condition && char.IsSeparator(text[startIndex]))
							{
								return true;
							}
							return false;
						}
					}
					else
					{
						_maskSlots[i]._rawText = text.Substring(startIndex, 1);
						startIndex++;
					}
				}
			}
			else
			{
				startIndex = text.Length - 1;
				for (int value = _maskSlots.Length - 1; value >= 0; value--)
				{
					if (startIndex >= 0)
					{
						if (_maskSlots[value]._isLiteral)
						{
							if (text.Substring(startIndex, 1) != _maskSlots[value]._rawText)
							{
								return false;
							}
							startIndex--;
						}
						else if (!IsCharAllowedInSlot(value, text[startIndex]))
						{
							if (_maskSlots[value]._isTextValid)
							{
								if (condition && char.IsSeparator(text[startIndex]))
								{
									return true;
								}
								return false;
							}
						}
						else
						{
							_maskSlots[value]._rawText = text.Substring(startIndex, 1);
							startIndex--;
						}
					}
				}
			}
			return AreFilledSlotsValid(condition);
		}

		private bool AreFilledSlotsValid(bool condition)
		{
			if (_hasMask)
			{
				for (int i = 0; i < _maskSlots.Length; i++)
				{
					if (!_maskSlots[i]._isLiteral)
					{
						if (_maskSlots[i]._isTextValid && _maskSlots[i]._rawText != null && _maskSlots[i]._rawText.Length == 0)
						{
							return false;
						}
						if (_maskSlots[i]._rawText != null && _maskSlots[i]._rawText.Length > 0 && !IsCharAllowedInSlot(i, _maskSlots[i]._rawText[0]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public bool IsTextValid()
		{
			if (_mask.Length > 0)
			{
				if (!_hasMask)
				{
					return false;
				}
				bool includeLiterals = _includeLiterals;
				IncludeLiterals = false;
				string text = Text;
				IncludeLiterals = includeLiterals;
				if (text.Length == 0)
				{
					return true;
				}
				return AreFilledSlotsValid(true);
			}
			return true;
		}

		private bool TryFillSlotsFromRawText(string text, bool condition)
		{
			ClearSlots(0, _maskSlots.Length);
			if (_isMaskDataSynced)
			{
				int startIndex = 0;
				for (int i = 0; i < _maskSlots.Length; i++)
				{
					if (text.Length > startIndex && !_maskSlots[i]._isLiteral)
					{
						if (!IsCharAllowedInSlot(i, text[startIndex]))
						{
							return false;
						}
						_maskSlots[i]._rawText = text.Substring(startIndex, 1);
						startIndex++;
					}
				}
				if (startIndex < text.Length)
				{
					return false;
				}
			}
			else
			{
				int otherStartIndex = text.Length - 1;
				for (int value = _maskSlots.Length - 1; value >= 0; value--)
				{
					if (otherStartIndex >= 0 && !_maskSlots[value]._isLiteral)
					{
						if (!IsCharAllowedInSlot(value, text[otherStartIndex]))
						{
							return false;
						}
						_maskSlots[value]._rawText = text.Substring(otherStartIndex, 1);
						otherStartIndex--;
					}
				}
				if (otherStartIndex != -1)
				{
					return false;
				}
			}
			return AreFilledSlotsValid(condition);
		}

		private string GetDisplayText(bool condition)
		{
			if (_hasMask)
			{
				char value = _promptChar;
				if (!condition)
				{
					value = ' ';
				}
				StringBuilder stringBuilder = new StringBuilder(_maskSlots.Length);
				for (int i = 0; i < _maskSlots.Length; i++)
				{
					if (_maskSlots[i]._rawText == null || _maskSlots[i]._rawText.Length == 0)
					{
						stringBuilder.Append(value);
					}
					else
					{
						stringBuilder.Append(_maskSlots[i]._rawText);
					}
				}
				if (IsWindowsNT4OrOlder())
				{
					stringBuilder.Append(" ");
				}
				return stringBuilder.ToString();
			}
			return base.Text;
		}

		private bool IsWindowsNT4OrOlder()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				return false;
			}
			if (Environment.OSVersion.Version.Major > 4)
			{
				return false;
			}
			return true;
		}

		private string BuildValueText()
		{
			if (_hasMask)
			{
				bool condition = true;
				StringBuilder stringBuilder = new StringBuilder(_maskSlots.Length);
				for (int i = 0; i < _maskSlots.Length; i++)
				{
					if (_maskSlots[i]._rawText != null && _maskSlots[i]._rawText.Length != 0 && (_includeLiterals || !_maskSlots[i]._isLiteral))
					{
						stringBuilder.Append(_maskSlots[i]._rawText);
						if (!_maskSlots[i]._isLiteral)
						{
							condition = false;
						}
					}
				}
				if (condition)
				{
					return "";
				}
				switch (base.CharacterCasing)
				{
				case CharacterCasing.Upper:
					return stringBuilder.ToString().ToUpper();
				case CharacterCasing.Lower:
					return stringBuilder.ToString().ToLower();
				default:
					return stringBuilder.ToString();
				}
			}
			return base.Text;
		}

		internal string GetValueText()
		{
			return BuildValueText();
		}

		private int GetNextSlotIndex(int nextSlotIndex)
		{
			nextSlotIndex++;
			while (nextSlotIndex < _maskSlots.Length && _maskSlots[nextSlotIndex]._isLiteral)
			{
				nextSlotIndex++;
			}
			return nextSlotIndex;
		}

		private bool TrySetSlotChar(int value, char character)
		{
			if (_hasMask)
			{
				if (value >= 0 && value < _maskSlots.Length)
				{
					if (_maskSlots[value]._isLiteral)
					{
						return TrySetSlotChar(GetNextSlotIndex(value), character);
					}
					if (IsCharAllowedInSlot(value, character))
					{
						_maskSlots[value]._rawText = character.ToString(GridHelper.GetCurrentCulture());
						return true;
					}
					return false;
				}
				return false;
			}
			return false;
		}

		private void ClearSlot(int value)
		{
			if (_hasMask && value >= 0 && value < _maskSlots.Length && !_maskSlots[value]._isLiteral)
			{
				_maskSlots[value]._rawText = "";
			}
		}

		private void ClearSlots(int value, int otherValue)
		{
			if (otherValue <= 0)
			{
				otherValue = 1;
			}
			for (int i = value; i < value + otherValue; i++)
			{
				ClearSlot(i);
			}
		}

		private bool IsCharAllowedInSlot(int value, char character)
		{
			if (_maskSlots[value]._allowsAnyChar)
			{
				return true;
			}
			if (_maskSlots[value]._allowsDigit && char.GetUnicodeCategory(character) == UnicodeCategory.DecimalDigitNumber)
			{
				return true;
			}
			if (_maskSlots[value]._allowsLetter && (char.GetUnicodeCategory(character) == UnicodeCategory.LowercaseLetter || char.GetUnicodeCategory(character) == UnicodeCategory.UppercaseLetter))
			{
				return true;
			}
			if (_maskSlots[value]._allowsSpace && char.GetUnicodeCategory(character) == UnicodeCategory.SpaceSeparator)
			{
				return true;
			}
			if (_maskSlots[value]._allowsSign && (character.ToString(GridHelper.GetCurrentCulture()) == "+" || character.ToString(GridHelper.GetCurrentCulture()) == "-"))
			{
				return true;
			}
			return false;
		}

		internal bool IsInsideColumnContainer()
		{
			return HasColumnContainerParent();
		}

		private bool HasColumnContainerParent()
		{
			for (Control control = GridHelper.GetParentSafe(this); control != null; control = GridHelper.GetParentSafe(control))
			{
				if (control is GridColumnContainerControl)
				{
					return true;
				}
			}
			return false;
		}

		private void ApplyText(string text)
		{
			_isHandlingTextChange = true;
			bool modified = base.Modified;
			if (!_isInternalTextChange)
			{
				_isInternalTextChange = true;
				base.Text = text;
				_isInternalTextChange = false;
				base.Modified = modified;
				if (_isTextChangeEnabled)
				{
					base.OnTextChanged(EventArgs.Empty);
				}
			}
			else
			{
				base.Text = text;
				base.Modified = modified;
			}
			_isHandlingTextChange = false;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (!_isTextChangeEnabled)
			{
				return;
			}
			_isTypingForward = _lastTextLength > 0;
			if (!_isInternalTextChange)
			{
				base.OnTextChanged(e);
			}
			if (_lastTextLength > 0)
			{
				_lastTextLength--;
				if (_lastTextLength == 0)
				{
					OnKeyPressed(e);
				}
			}
			else if (!_isHandlingTextChange)
			{
				bool modified = base.Modified;
				if (_hasMask)
				{
					ApplyText(GetDisplayText(Focused));
				}
				else
				{
					Text = base.Text;
				}
				base.Modified = modified;
			}
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
			case WindowMessages.Cut:
				if (!CutSelection())
				{
					base.WndProc(ref m);
				}
				break;
			case WindowMessages.Paste:
				PasteFromClipboard();
				break;
			case WindowMessages.Copy:
				if (_hasMask)
				{
					try
					{
						if (_includeLiterals)
						{
							Clipboard.SetDataObject(GetDisplayText(false), true, ClipboardRetryCount, ClipboardRetryDelay);
						}
						else
						{
							Clipboard.SetDataObject(UnmaskedText, true, ClipboardRetryCount, ClipboardRetryDelay);
						}
						break;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						break;
					}
				}
				base.WndProc(ref m);
				break;
			default:
				base.WndProc(ref m);
				break;
			}
		}

		private bool CutSelection()
		{
			try
			{
				if (SelectionLength > 0 && _hasMask)
				{
					int selectionStart = base.SelectionStart;
					StringBuilder text = new StringBuilder();
					for (int i = base.SelectionStart; i < base.SelectionStart + SelectionLength; i++)
					{
						if (i < _maskSlots.Length)
						{
							if (_maskSlots[i]._rawText != null && _maskSlots[i]._rawText.Length != 0)
							{
								text.Append(_maskSlots[i]._rawText);
							}
							else
							{
								text.Append(_promptChar);
							}
							ClearSlot(i);
						}
					}
					Clipboard.SetDataObject(text.ToString(), true, ClipboardRetryCount, ClipboardRetryDelay);
					ApplyText(GetDisplayText(true));
					SetCaretPosition(selectionStart);
					SelectionLength = 0;
					return true;
				}
				return false;
			}
			catch (ExternalException)
			{
				// The clipboard is locked by another program; the cut is skipped.
				return true;
			}
		}

		private void SetCaretPosition(int value)
		{
			PlaceCaret(value);
		}

		private void PlaceCaret(int selectionStart)
		{
			SelectionLength = 0;
			base.SelectionStart = selectionStart;
		}

		internal void SelectRange(int value, int otherValue)
		{
			NativeMethods.SendMessage(base.Handle, WindowMessages.EditSetSelection, (IntPtr)value, (IntPtr)otherValue);
			ScrollToCaret();
		}

		protected override void OnTextAlignChanged(EventArgs e)
		{
			RecreateHandle();
			base.OnTextAlignChanged(e);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new MaskEditAccessibleObject(this);
		}
	}
}
