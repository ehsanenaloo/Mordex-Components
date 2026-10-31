using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class MaskEdit
	{
		internal static string FormatWithMask(object item, string text)
		{
			if (text == null || text.Length == 0)
			{
				return string.Concat(item);
			}
			if (_maskEdit == null)
			{
				_maskEdit = new MaskEdit(checkLicense: false);
			}
			_maskEdit.Text = "";
			_maskEdit.Mask = text;
			_maskEdit.Text = string.Concat(item);
			return _maskEdit.GetDisplayText(false);
		}

		private bool IsTextValidForMask(string text, bool condition)
		{
			if (!IsFormattedTextValid(text, condition) && !TryFillSlotsFromRawText(text, condition))
			{
				return false;
			}
			return true;
		}

		private void UpdateMaskEdit()
		{
			if (_mask.Length == 0)
			{
				_hasMask = false;
				_maskSlots = new MaskEditData[0];
				return;
			}
			_isMaskDataSynced = true;
			_hasMask = true;
			_maskSlots = new MaskEditData[CountMaskSlots(_mask)];
			int slot = 0;
			bool escaped = false;
			foreach (char character in _mask)
			{
				if (escaped)
				{
					SetLiteral(ref _maskSlots[slot++], character.ToString());
					escaped = false;
				}
				else if (character == '\\')
				{
					escaped = true;
				}
				else if (character == '!')
				{
					_isMaskDataSynced = false;
				}
				else
				{
					ApplyMaskCharacter(ref _maskSlots[slot++], character);
				}
			}
		}

		// '\' escapes the next character and '!' is a flag, so neither takes a slot.
		private static int CountMaskSlots(string mask)
		{
			int count = 0;
			bool escaped = false;
			foreach (char character in mask)
			{
				if (escaped)
				{
					count++;
					escaped = false;
				}
				else if (character == '\\')
				{
					escaped = true;
				}
				else if (character != '!')
				{
					count++;
				}
			}
			return count;
		}

		private static void SetLiteral(ref MaskEditData slot, string text)
		{
			slot._isLiteral = true;
			slot._rawText = text;
		}

		private static void ApplyMaskCharacter(ref MaskEditData slot, char character)
		{
			CultureInfo culture = GridHelper.GetCurrentCulture();
			switch (character)
			{
			case '0':
				slot._isTextValid = true;
				slot._allowsDigit = true;
				break;
			case '9':
			case '#':
				slot._allowsDigit = true;
				slot._allowsSign = true;
				slot._allowsSpace = true;
				break;
			case 'L':
				slot._isTextValid = true;
				slot._allowsLetter = true;
				break;
			case '?':
				slot._allowsLetter = true;
				break;
			case 'A':
				slot._isTextValid = true;
				slot._allowsDigit = true;
				slot._allowsLetter = true;
				break;
			case 'a':
				slot._allowsDigit = true;
				slot._allowsLetter = true;
				break;
			case '&':
				slot._isTextValid = true;
				slot._allowsAnyChar = true;
				break;
			case 'C':
				slot._allowsAnyChar = true;
				break;
			case '.':
				SetLiteral(ref slot, culture.NumberFormat.NumberDecimalSeparator);
				break;
			case ',':
				SetLiteral(ref slot, culture.NumberFormat.NumberGroupSeparator);
				break;
			case ':':
				SetLiteral(ref slot, culture.DateTimeFormat.TimeSeparator);
				break;
			case '/':
				SetLiteral(ref slot, culture.DateTimeFormat.DateSeparator);
				break;
			default:
				SetLiteral(ref slot, character.ToString());
				break;
			}
		}

		internal void ShowFullMaskAndSelect()
		{
			string text = GetDisplayText(true);
			SelectAll();
			SelectRange(text.Length, 0);
		}

		internal bool HandleMaskedChar(char character)
		{
			bool result = false;
			_isBackspacePressed = character == '\b';
			if (character != '\r' && character != '\b')
			{
				_lastTextLength = 1;
			}
			else
			{
				_lastTextLength = -1;
			}
			if (_hasMask)
			{
				if (character == '\b')
				{
					if (SelectionLength == 0)
					{
						int value = base.SelectionStart - 1;
						if (value >= 0)
						{
							ClearSlot(value);
							base.Modified = true;
							ApplyText(GetDisplayText(true));
							if (value > 0 && _maskSlots != null && _maskSlots.Length > value)
							{
								int otherValue = value;
								while (_maskSlots[otherValue - 1]._isLiteral)
								{
									otherValue--;
									if (otherValue < 1)
									{
										break;
									}
								}
								value = otherValue;
							}
							SetCaretPosition(value);
						}
					}
					else
					{
						base.Modified = true;
						int selectionStart = base.SelectionStart;
						ClearSlots(base.SelectionStart, SelectionLength);
						ApplyText(GetDisplayText(Focused));
						SetCaretPosition(selectionStart);
					}
					result = true;
				}
				else
				{
					int otherSelectionStart = base.SelectionStart;
					if (otherSelectionStart >= 0 && otherSelectionStart < _maskSlots.Length)
					{
						if (SelectionLength > 0)
						{
							base.Modified = true;
							ClearSlots(base.SelectionStart, SelectionLength);
							ApplyText(GetDisplayText(Focused));
							SetCaretPosition(otherSelectionStart);
							SelectionLength = 0;
						}
						if (_maskSlots[otherSelectionStart]._isLiteral)
						{
							otherSelectionStart = GetNextSlotIndex(otherSelectionStart);
						}
						if (TrySetSlotChar(otherSelectionStart, character))
						{
							base.Modified = true;
							ApplyText(GetDisplayText(true));
							SetCaretPosition(GetNextSlotIndex(otherSelectionStart));
							SelectionLength = 0;
						}
					}
					result = true;
				}
			}
			else if (_numeric && !char.IsDigit(character) && character != '\b')
			{
				result = true;
			}
			if (base.TextAlign == HorizontalAlignment.Left && SelectedText.Length > 0)
			{
				_lastTextLength++;
			}
			return result;
		}

		private void PasteFromClipboard()
		{
			if (base.ReadOnly)
			{
				return;
			}
			try
			{
				IDataObject dataObject = Clipboard.GetDataObject();
				if (dataObject == null || !dataObject.GetDataPresent(typeof(string)))
				{
					return;
				}
				string text = (string)dataObject.GetData(typeof(string));
				if (!Multiline)
				{
					if (text.Length > 1 && text.Substring(text.Length - 2, 2) == "\r\n")
					{
						text = text.Substring(0, text.Length - 2);
					}
					text = text.Replace("\n", " ");
				}
				if (_hasMask)
				{
					for (int i = 0; i < text.Length; i++)
					{
						HandleMaskedChar(text[i]);
					}
					return;
				}
				if (_numeric)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int j = 0; j < text.Length; j++)
					{
						if (!HandleMaskedChar(text[j]))
						{
							stringBuilder.Append(text[j]);
						}
					}
					text = stringBuilder.ToString();
				}
				string otherText = Text;
				int value = 0;
				otherText = ((SelectionLength <= 0) ? otherText.Insert(base.SelectionStart, text) : (otherText.Substring(0, base.SelectionStart) + text + otherText.Substring(base.SelectionStart + SelectionLength)));
				if (MaxLength > 0 && otherText.Length > MaxLength)
				{
					otherText = otherText.Substring(0, MaxLength);
				}
				value = Math.Min(base.SelectionStart + text.Length, otherText.Length);
				Text = otherText;
				SetCaretPosition(value);
				SelectionLength = 0;
				ScrollToCaret();
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}
	}
}
