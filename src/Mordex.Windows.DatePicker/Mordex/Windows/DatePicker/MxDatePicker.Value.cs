using System;
using System.Globalization;
using System.Text;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private void RefreshNullBinding()
		{
			if (IsNullDate)
			{
				BindableValue = _nullValue;
			}
		}

		private void SyncTextBoxFromValue()
		{
			if (_editStyle == EditStyle.Free && _textBox != null && !_textBox.IsDisposed)
			{
				if (_isNullDate)
				{
					_textBox.Text = "";
				}
				else
				{
					_textBox.Text = DatePickerHelper.FormatDate(_value, GetFormatPattern());
				}
			}
		}

		public void EndEdit()
		{
			if (_editStyle == EditStyle.Masked)
			{
				if (MaskedValidation == MaskedValidation.OnChange)
				{
					if (_activePortion != null && !_isNullDate)
					{
						Value = _activePortion.ApplyTypedValue(_value);
						if (_reformatOnEndEdit && _activePortion != null)
						{
							_activePortion.SetIsActive(false);
							_activePortion.LoadFromDate(_value);
							_activePortion.SetIsActive(true);
						}
					}
				}
				else
				{
					CommitMaskedText();
				}
			}
			else
			{
				SyncValueFromTextBox();
			}
		}

		private void ApplyValue(DateTime value)
		{
			_value = value;
			if (_editStyle == EditStyle.Masked)
			{
				DateFieldPortion activePortion = _activePortion;
				activePortion?.SetHasTypedValue(false);
				SetActivePortion(null);
				SetActivePortion(activePortion);
				RequestLayout();
				OnTextChanged(EventArgs.Empty);
			}
			else
			{
				SyncTextBoxFromValue();
			}
			Invalidate();
			OnValueChanged(EventArgs.Empty);
			OnBindableValueChanged(EventArgs.Empty);
		}

		private void CommitMaskedText()
		{
			StringBuilder combinedText = new StringBuilder();
			foreach (DateFieldPortion portion in _portions)
			{
				if (portion.GetPortionType() == PortionType.Year)
				{
					string portionText = portion.FormatText(_value, _isNullDate);
					if (portionText.Length <= 0)
					{
						combinedText.Append(0);
						continue;
					}
					if (!portion.GetHasTypedValue() && !portion.GetIsActive() && portionText.Length < 4)
					{
						portionText = DatePickerHelper.FormatDate(_value, "yyyy");
					}
					if (int.TryParse(portionText, NumberStyles.None, CultureInfo.InvariantCulture, out int typedYear))
					{
						combinedText.Append(DatePickerHelper.GetDisplayFormat().Calendar.ToFourDigitYear(typedYear));
					}
				}
				else
				{
					combinedText.Append(portion.FormatText(_value, _isNullDate));
				}
				portion.SetHasTypedValue(false);
			}
			string pattern = GetFormatPattern();
			int firstYearIndex = pattern.IndexOf('y');
			if (firstYearIndex >= 0 && pattern.LastIndexOf('y') - firstYearIndex + 1 == 2)
			{
				pattern = pattern.Replace("yy", "yyyy");
			}
			DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
			string text = combinedText.ToString();
			bool parseFailed = false;
			if (!DateTime.TryParseExact(text, pattern, format, DateTimeStyles.None, out DateTime result) && !DateTime.TryParse(text, format, DateTimeStyles.None, out result))
			{
				parseFailed = true;
				result = _value;
			}
			if (parseFailed)
			{
				if (!_isNullDate && Nullable)
				{
					_isNullDate = true;
					OnBindableValueChanged(EventArgs.Empty);
				}
			}
			else if (result != _value)
			{
				Value = result;
				_isNullDate = false;
			}
			else if (_isNullDate)
			{
				_isNullDate = false;
				OnBindableValueChanged(EventArgs.Empty);
			}
			Invalidate();
		}

		private void SyncValueFromTextBox()
		{
			bool isNull = false;
			DateTime parsedDate = _value;
			if (_textBox != null && !_textBox.IsDisposed && _editStyle == EditStyle.Free)
			{
				DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
				if (_textBox.Text == "")
				{
					isNull = true;
				}
				else if (DateTime.TryParseExact(_textBox.Text, GetFormatPattern(), format, DateTimeStyles.None, out DateTime exactDate))
				{
					parsedDate = exactDate;
				}
				else if (DateTime.TryParse(_textBox.Text, format, DateTimeStyles.None, out DateTime looseDate))
				{
					parsedDate = looseDate;
				}
				else
				{
					isNull = true;
				}
			}
			if (isNull == _isNullDate && !(parsedDate != _value) && (!_isNullDate || _textBox.Text.Length <= 0))
			{
				return;
			}
			ParseEventArgs e = new ParseEventArgs(Text, parsedDate, isNull);
			OnParse(e);
			if (e.IsNullDate)
			{
				if (!_isNullDate)
				{
					_isNullDate = true;
					OnBindableValueChanged(EventArgs.Empty);
				}
			}
			else if (e.Value != _value)
			{
				Value = e.Value;
				_isNullDate = false;
			}
			else if (_isNullDate)
			{
				_isNullDate = false;
				OnBindableValueChanged(EventArgs.Empty);
			}
		}

		private string GetFormatPattern()
		{
			switch (_dateFormat)
			{
			case DateFormat.Custom:
				return _customFormat;
			case DateFormat.DateTime:
				return DatePickerHelper.GetDisplayFormat().ShortDatePattern + " " + DatePickerHelper.GetDisplayFormat().LongTimePattern;
			case DateFormat.Long:
				return DatePickerHelper.GetDisplayFormat().LongDatePattern;
			case DateFormat.Short:
				return DatePickerHelper.GetDisplayFormat().ShortDatePattern;
			case DateFormat.Time:
				return DatePickerHelper.GetDisplayFormat().LongTimePattern;
			default:
				return "";
			}
		}

		private void ClampValueToRange()
		{
			if (_value < MinDate)
			{
				if (!IsNullDate)
				{
					Value = MinDate;
				}
				else
				{
					_value = MinDate;
				}
			}
			else if (_value > MaxDate)
			{
				if (!IsNullDate)
				{
					Value = MaxDate;
				}
				else
				{
					_value = MaxDate;
				}
			}
		}
	}
}
