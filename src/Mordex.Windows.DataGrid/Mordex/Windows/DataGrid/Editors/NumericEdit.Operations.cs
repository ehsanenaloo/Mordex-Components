using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class NumericEdit
	{
		internal bool IsTextInvalid()
		{
			try
			{
				ParseText(base.Text);
				return false;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return true;
			}
		}

		protected override void OnMultilineChanged(EventArgs e)
		{
			Multiline = false;
		}

		private int GetEffectiveDecimalDigits()
		{
			if (_decimalDigits == -1)
			{
				if (_formatStringOverride == "")
				{
					switch (_valueType)
					{
					case NumericEditValueType.SByte:
					case NumericEditValueType.Byte:
					case NumericEditValueType.Int16:
					case NumericEditValueType.UInt16:
					case NumericEditValueType.Int32:
					case NumericEditValueType.UInt32:
					case NumericEditValueType.Int64:
					case NumericEditValueType.UInt64:
						return 0;
					default:
						switch (_formatMask)
						{
						case NumericEditFormatMask.Currency:
							return GetNumberFormat().CurrencyDecimalDigits;
						case NumericEditFormatMask.General:
						case NumericEditFormatMask.Number:
							return GetNumberFormat().NumberDecimalDigits;
						case NumericEditFormatMask.Percent:
							return GetNumberFormat().PercentDecimalDigits;
						default:
							return -1;
						}
					}
				}
				return -1;
			}
			return _decimalDigits;
		}

		internal void SetEmptyTextAsString(bool condition)
		{
			_emptyTextAsString = condition;
		}

		private void UpdateNumericEdit()
		{
			_isUpdatingDisplay = true;
			if (_currentValue == null || _currentValue == DBNull.Value)
			{
				base.Text = "";
			}
			else if (_formatStringOverride == "")
			{
				string text = "";
				switch (_formatMask)
				{
				case NumericEditFormatMask.Currency:
					text = "C";
					text += GetEffectiveDecimalDigits();
					break;
				case NumericEditFormatMask.General:
				{
					// "G" with a digit count means significant digits and switches to scientific notation.
					int decimalDigits = GetEffectiveDecimalDigits();
					text = (decimalDigits > 0) ? ("0." + new string('#', decimalDigits)) : "0";
					break;
				}
				case NumericEditFormatMask.Number:
					text = "N";
					text += GetEffectiveDecimalDigits();
					break;
				case NumericEditFormatMask.Percent:
					text = "P";
					text += GetEffectiveDecimalDigits();
					break;
				}
				try
				{
					if (_currentValue is IFormattable)
					{
						base.Text = ((IFormattable)_currentValue).ToString(text, GetNumberFormat());
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					base.Text = GridHelper.ToSafeString(_currentValue);
				}
			}
			else
			{
				string otherText = ((IFormattable)_currentValue).ToString(_formatStringOverride, GetNumberFormat());
				if (otherText != base.Text)
				{
					base.Text = otherText;
				}
			}
			_isUpdatingDisplay = false;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Return:
				return false;
			case Keys.Escape:
				return false;
			case Keys.Right:
				if (base.SelectionStart == base.Text.Length && base.SelectionLength == 0 && IsInsideColumnContainer())
				{
					return false;
				}
				return base.IsInputKey(keyData);
			case Keys.Left:
				if (base.SelectionStart == 0 && base.SelectionLength == 0 && IsInsideColumnContainer())
				{
					return false;
				}
				return base.IsInputKey(keyData);
			case Keys.Prior:
			case Keys.Next:
			case Keys.Up:
			case Keys.Down:
				return false;
			case Keys.End:
			case Keys.Home:
				return true;
			default:
				return base.IsInputKey(keyData);
			}
		}

		private object ParseText(string text)
		{
			if (text == "")
			{
				if (_emptyTextAsString)
				{
					return "";
				}
				if (_nullBehavior == NumericEditNullBehavior.AllowDBNull)
				{
					return DBNull.Value;
				}
				if (_nullBehavior == NumericEditNullBehavior.AllowNull)
				{
					return null;
				}
			}
			ArrayList arrayList = ComputeArrayList(text);
			text = "";
			foreach (NumberToken item in arrayList)
			{
				if (item._category != NumberCategory.Space)
				{
					text += item._slot;
				}
			}
			bool condition = false;
			NumberStyles numberStyles;
			switch (_formatMask)
			{
			case NumericEditFormatMask.Currency:
				numberStyles = NumberStyles.Currency;
				break;
			case NumericEditFormatMask.Percent:
			{
				numberStyles = NumberStyles.Number;
				int startIndex = text.IndexOf(GetNumberFormat().PercentSymbol);
				if (startIndex != -1)
				{
					text = text.Remove(startIndex, GetNumberFormat().PercentSymbol.Length);
				}
				if (_editMode != NumericEditMode.Value)
				{
					condition = true;
				}
				break;
			}
			default:
				numberStyles = NumberStyles.Number;
				if (_formatStringOverride != null && _formatStringOverride.Length > 0 && _formatStringOverride.Contains(NumberFormatInfo.CurrentInfo.CurrencySymbol))
				{
					numberStyles |= NumberStyles.AllowCurrencySymbol;
				}
				break;
			}
			object parseText = 0;
			if (text.Length == 0)
			{
				text = "0";
			}
			else
			{
				if (text == NumberFormatInfo.CurrentInfo.NegativeSign || text == NumberFormatInfo.CurrentInfo.PositiveSign)
				{
					return ParseText("");
				}
				if (text == NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator)
				{
					text = "0";
				}
			}
			if (condition && _valueType != NumericEditValueType.Decimal && _valueType != NumericEditValueType.Double && _valueType != NumericEditValueType.Single)
			{
				decimal percent = decimal.Parse(text, numberStyles, GetNumberFormat()) / 100m;
				return Convert.ChangeType(percent, GetValueType(), GetNumberFormat());
			}
			switch (_valueType)
			{
			case NumericEditValueType.Byte:
				parseText = byte.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.Decimal:
				parseText = decimal.Parse(text, numberStyles, GetNumberFormat());
				if (condition)
				{
					parseText = (decimal)parseText / 100m;
				}
				break;
			case NumericEditValueType.Double:
				parseText = double.Parse(text, numberStyles, GetNumberFormat());
				if (condition)
				{
					parseText = (double)parseText / 100.0;
				}
				break;
			case NumericEditValueType.Int16:
				parseText = short.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.Int32:
				parseText = int.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.Int64:
				parseText = long.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.SByte:
				parseText = sbyte.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.Single:
				parseText = float.Parse(text, numberStyles, GetNumberFormat());
				if (condition)
				{
					parseText = (float)parseText / 100f;
				}
				break;
			case NumericEditValueType.UInt16:
				parseText = ushort.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.UInt32:
				parseText = uint.Parse(text, numberStyles, GetNumberFormat());
				break;
			case NumericEditValueType.UInt64:
				parseText = ulong.Parse(text, numberStyles, GetNumberFormat());
				break;
			}
			return parseText;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled)
			{
				if (e.KeyCode == Keys.Delete)
				{
					_isProgrammaticChange = true;
				}
				else if (e.KeyCode == Keys.Decimal)
				{
					_isDecimalKeyDown = true;
				}
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			_isDecimalKeyDown = false;
		}

		internal void SetTextFromCharacter(char character)
		{
			if (IsNumberTextValid(string.Concat(character)))
			{
				_isProgrammaticChange = true;
				base.Text = string.Concat(character);
				base.SelectionStart = base.Text.Length;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (_selectAllOnMouseDown)
			{
				_selectAllOnMouseDown = false;
				SelectAll();
			}
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			if (Control.MouseButtons != MouseButtons.None)
			{
				_selectAllOnMouseDown = true;
			}
			if (_editMode == NumericEditMode.Value)
			{
				_isProgrammaticChange = true;
				if (_currentValue == null || _currentValue == DBNull.Value)
				{
					base.Text = "";
				}
				else
				{
					base.Text = GridHelper.ToSafeString(Value);
				}
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (_selectAllOnFirstFocus)
			{
				SelectAll();
				_selectAllOnFirstFocus = false;
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			if (!_validateOnLeave)
			{
				return;
			}
			bool modified = base.Modified;
			try
			{
				if (base.Modified)
				{
					Value = ParseText(base.Text);
				}
				UpdateNumericEdit();
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				if (_nullBehavior == NumericEditNullBehavior.AllowDBNull)
				{
					Value = DBNull.Value;
				}
				else if (NullBehavior == NumericEditNullBehavior.AllowNull)
				{
					Value = null;
				}
				else
				{
					Value = 0;
				}
			}
			SelectAll();
			base.Modified = modified;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (ValidateAndCommitText())
			{
				base.OnTextChanged(e);
			}
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == WindowMessages.Paste)
			{
				PasteFromClipboard();
			}
			else
			{
				base.WndProc(ref m);
			}
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
				if (dataObject != null && dataObject.GetDataPresent(typeof(string)))
				{
					string text = (string)dataObject.GetData(typeof(string));
					string otherText = Text;
					int selectionStart = 0;
					otherText = ((SelectionLength <= 0) ? otherText.Insert(base.SelectionStart, text) : (otherText.Substring(0, base.SelectionStart) + text + otherText.Substring(base.SelectionStart + SelectionLength)));
					if (MaxLength > 0 && otherText.Length > MaxLength)
					{
						otherText = otherText.Substring(0, MaxLength);
					}
					selectionStart = Math.Min(base.SelectionStart + text.Length, otherText.Length);
					if (IsNumberTextValid(otherText))
					{
						_isProgrammaticChange = true;
						base.Text = otherText;
						_isProgrammaticChange = false;
						base.SelectionStart = selectionStart;
						SelectionLength = 0;
					}
					ScrollToCaret();
				}
			}
			catch (ExternalException)
			{
				// The clipboard is locked by another program; the paste is skipped.
			}
		}

		private bool ValidateAndCommitText()
		{
			if (!_isProgrammaticChange && !_isUpdatingDisplay)
			{
				if (!IsNumberTextValid(base.Text))
				{
					_isProgrammaticChange = true;
					base.Text = _committedText;
					_isProgrammaticChange = false;
					return false;
				}
				try
				{
					object parseText = ParseText(base.Text);
					if (parseText != _currentValue)
					{
						_currentValue = parseText;
						OnValueChanged(EventArgs.Empty);
					}
					_committedText = base.Text;
					return true;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					_isProgrammaticChange = true;
					base.Text = _committedText;
					_isProgrammaticChange = false;
					return false;
				}
			}
			if (!_isUpdatingDisplay)
			{
				try
				{
					object otherParseText = ParseText(base.Text);
					if (GridHelper.CompareValues(otherParseText, _currentValue, null, true) != 0)
					{
						_currentValue = otherParseText;
						OnValueChanged(EventArgs.Empty);
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
			_isProgrammaticChange = false;
			_committedText = base.Text;
			return true;
		}

		internal void ReplaceTextWithChar(char character)
		{
			base.Text = character.ToString();
			base.SelectionStart = 1;
		}

		internal void SetAllowNegative(bool allowNegative)
		{
			_allowNegative = allowNegative;
		}

		private bool AreParenthesesAllowed()
		{
			if (_allowParentheses == TriState.Empty)
			{
				return _formatMask == NumericEditFormatMask.Currency;
			}
			if (_allowParentheses == TriState.True)
			{
				return true;
			}
			return false;
		}

		private NumberFormatInfo GetNumberFormat()
		{
			return Application.CurrentCulture.NumberFormat;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (e.Handled)
			{
				return;
			}
			if (char.IsControl(e.KeyChar))
			{
				if (e.KeyChar == '\b')
				{
					_isProgrammaticChange = true;
				}
				else if (e.KeyChar == '\u007f')
				{
					e.Handled = true;
				}
				return;
			}
			string text = string.Concat(e.KeyChar);
			if (_isDecimalKeyDown && text == ".")
			{
				text = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			}
			string otherText = GetTextBeforeCaret() + text + GetTextAfterSelection();
			ArrayList arrayList = ComputeArrayList(otherText);
			int selectionStart = base.SelectionStart;
			int value = -1;
			int otherValue = 0;
			int nextValue = 0;
			bool condition = false;
			foreach (NumberToken item in arrayList)
			{
				if (item._category == NumberCategory.InvalidData)
				{
					e.Handled = true;
					return;
				}
				if (item._category == NumberCategory.DecimalSepator)
				{
					condition = true;
				}
				if (condition && item._category == NumberCategory.DigitGroup && item._slot.Length > GetEffectiveDecimalDigits() && GetEffectiveDecimalDigits() != -1)
				{
					e.Handled = true;
					return;
				}
				if (selectionStart == otherValue)
				{
					value = nextValue;
				}
				otherValue += item._slot.Length;
				nextValue++;
			}
			if (value > 0 && ((NumberToken)arrayList[value - 1])._isNegative)
			{
				e.Handled = true;
			}
			_isProgrammaticChange = true;
			if (!e.Handled && _isDecimalKeyDown && text != string.Concat(e.KeyChar))
			{
				SelectedText = text;
				e.Handled = true;
			}
		}

		private string GetTextBeforeCaret()
		{
			if (base.SelectionStart == 0)
			{
				return "";
			}
			return base.Text.Substring(0, base.SelectionStart);
		}

		private string GetTextAfterSelection()
		{
			int startIndex = base.SelectionStart + SelectionLength;
			if (startIndex < base.Text.Length)
			{
				return base.Text.Substring(startIndex);
			}
			return "";
		}

		private bool IsNumberTextValid(string text)
		{
			bool condition = false;
			ArrayList arrayList = ComputeArrayList(text);
			foreach (NumberToken item in arrayList)
			{
				if (item._category == NumberCategory.InvalidData)
				{
					return false;
				}
				if (item._category == NumberCategory.DecimalSepator)
				{
					condition = true;
				}
				if (condition && item._category == NumberCategory.DigitGroup && item._slot.Length > GetEffectiveDecimalDigits() && GetEffectiveDecimalDigits() != -1)
				{
					return false;
				}
			}
			return true;
		}

		private NumberToken ParseToken(string otherText, int startIndex, NumberCategory nc)
		{
			if (otherText.Length == 0)
			{
				return null;
			}
			string text = otherText.Substring(startIndex);
			switch (nc)
			{
			case NumberCategory.OpenParentheses:
				if (text.StartsWith("("))
				{
					return new NumberToken(nc, "(");
				}
				return null;
			case NumberCategory.CloseParentheses:
				if (text.StartsWith(")"))
				{
					return new NumberToken(nc, ")");
				}
				return null;
			case NumberCategory.CurrencySymbol:
			{
				string currencySymbol = GetNumberFormat().CurrencySymbol;
				if (text.Substring(0, currencySymbol.Length) == currencySymbol)
				{
					return new NumberToken(NumberCategory.CurrencySymbol, GetNumberFormat().CurrencySymbol);
				}
				for (int length = currencySymbol.Length - 1; length > 0; length--)
				{
					if (text.StartsWith(currencySymbol.Substring(0, length)))
					{
						NumberToken a = new NumberToken(nc, currencySymbol.Substring(0, length));
						a._isNegative = true;
						return a;
					}
				}
				return null;
			}
			case NumberCategory.DecimalSepator:
			{
				string decimalSeparator = GetDecimalSeparator();
				if (text.StartsWith(decimalSeparator))
				{
					return new NumberToken(NumberCategory.DecimalSepator, decimalSeparator);
				}
				for (int otherLength = decimalSeparator.Length - 1; otherLength > 0; otherLength--)
				{
					if (text.StartsWith(decimalSeparator.Substring(0, otherLength)))
					{
						NumberToken a3 = new NumberToken(nc, decimalSeparator.Substring(0, otherLength));
						a3._isNegative = true;
						return a3;
					}
				}
				return null;
			}
			case NumberCategory.GroupSeparator:
			{
				string groupSeparator = GetGroupSeparator();
				if (text.StartsWith(groupSeparator))
				{
					return new NumberToken(NumberCategory.GroupSeparator, groupSeparator);
				}
				for (int nextLength = groupSeparator.Length - 1; nextLength > 0; nextLength--)
				{
					if (text.StartsWith(groupSeparator.Substring(0, nextLength)))
					{
						NumberToken a2 = new NumberToken(nc, groupSeparator.Substring(0, nextLength));
						a2._isNegative = true;
						return a2;
					}
				}
				return null;
			}
			case NumberCategory.Sign:
			{
				string positiveSign = GetNumberFormat().PositiveSign;
				if (text.StartsWith(positiveSign))
				{
					return new NumberToken(NumberCategory.Sign, positiveSign);
				}
				for (int previousLength = positiveSign.Length - 1; previousLength > 0; previousLength--)
				{
					if (text.StartsWith(positiveSign.Substring(0, previousLength)))
					{
						NumberToken a4 = new NumberToken(nc, positiveSign.Substring(0, previousLength));
						a4._isNegative = true;
						return a4;
					}
				}
				positiveSign = GetNumberFormat().NegativeSign;
				if (text.StartsWith(positiveSign))
				{
					return new NumberToken(NumberCategory.Sign, positiveSign);
				}
				for (int firstLength = positiveSign.Length - 1; firstLength > 0; firstLength--)
				{
					if (text.StartsWith(positiveSign.Substring(0, firstLength)))
					{
						NumberToken a5 = new NumberToken(nc, positiveSign.Substring(0, firstLength));
						a5._isNegative = true;
						return a5;
					}
				}
				return null;
			}
			case NumberCategory.PercentSymbol:
			{
				string percentSymbol = GetNumberFormat().PercentSymbol;
				if (text.StartsWith(percentSymbol))
				{
					return new NumberToken(NumberCategory.PercentSymbol, percentSymbol);
				}
				for (int secondLength = percentSymbol.Length - 1; secondLength > 0; secondLength--)
				{
					if (text.StartsWith(percentSymbol.Substring(0, secondLength)))
					{
						NumberToken a6 = new NumberToken(nc, percentSymbol.Substring(0, secondLength));
						a6._isNegative = true;
						return a6;
					}
				}
				return null;
			}
			case NumberCategory.Space:
				if (char.IsWhiteSpace(otherText[startIndex]))
				{
					return new NumberToken(NumberCategory.Space, string.Concat(otherText[startIndex]));
				}
				return null;
			default:
			{
				int value = 0;
				string slot = "";
				while (char.IsDigit(text[value]))
				{
					slot += text[value];
					value++;
					if (value >= text.Length)
					{
						break;
					}
				}
				if (slot.Length > 0)
				{
					return new NumberToken(NumberCategory.DigitGroup, slot);
				}
				return null;
			}
			}
		}

		private string GetDecimalSeparator()
		{
			if (_formatMask == NumericEditFormatMask.Currency)
			{
				return GetNumberFormat().CurrencyDecimalSeparator;
			}
			return GetNumberFormat().NumberDecimalSeparator;
		}

		private string GetGroupSeparator()
		{
			if (_formatMask == NumericEditFormatMask.Currency)
			{
				return GetNumberFormat().CurrencyGroupSeparator;
			}
			return GetNumberFormat().NumberGroupSeparator;
		}

		protected override void OnAcceptsTabChanged(EventArgs e)
		{
			AcceptsTab = false;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new NumericEditAccessibleObject(this);
		}
	}
}
