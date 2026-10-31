using System;
using System.Drawing;
using System.Globalization;

namespace Mordex.Windows.DatePicker
{
	internal class DateFieldPortion
	{
		private const int MonthsPerYear = 12;

		private const int HoursPerDay = 24;

		private const int HoursPerHalfDay = 12;

		private const int MinutesPerHour = 60;

		private const int SecondsPerMinute = 60;

		public Rectangle _bounds;

		private readonly bool _useTextRenderer;

		private PortionType _portionType;

		private string _displayText = "";

		private string _formatPattern = "";

		private DateTime _currentValue;

		private MaskedValidation _maskedValidation;

		private bool _isActive;

		private bool _hasTypedValue;

		private string _typedText = "";

		private int _fieldValue;

		internal DateFieldPortion(bool literal, string text, MaskedValidation maskedValidation, bool useCompatibleTextRendering)
		{
			_useTextRenderer = !useCompatibleTextRendering;
			_maskedValidation = maskedValidation;
			if (literal || text.Length == 0)
			{
				_portionType = PortionType.Literal;
				_displayText = text;
			}
			else
			{
				_formatPattern = text;
				if (text.StartsWith("MMMM", StringComparison.Ordinal))
				{
					_portionType = PortionType.MonthName;
				}
				else if (text.StartsWith("MMM", StringComparison.Ordinal))
				{
					_portionType = PortionType.MonthAbbreviated;
				}
				else if (text.StartsWith("M", StringComparison.Ordinal))
				{
					_portionType = PortionType.Month;
				}
				else if (text.StartsWith("ddd", StringComparison.Ordinal))
				{
					_portionType = PortionType.DayOfWeek;
				}
				else if (text.StartsWith("d", StringComparison.Ordinal))
				{
					_portionType = PortionType.Day;
				}
				else if (text.StartsWith("y", StringComparison.Ordinal))
				{
					_portionType = PortionType.Year;
				}
				else if (text.StartsWith("h", StringComparison.Ordinal))
				{
					_portionType = PortionType.Hour12;
				}
				else if (text.StartsWith("H", StringComparison.Ordinal))
				{
					_portionType = PortionType.Hour24;
				}
				else if (text.StartsWith("m", StringComparison.Ordinal))
				{
					_portionType = PortionType.Minute;
				}
				else if (text.StartsWith("s", StringComparison.Ordinal))
				{
					_portionType = PortionType.Second;
				}
				else if (text.StartsWith("t", StringComparison.Ordinal))
				{
					_portionType = PortionType.AMPM;
				}
				else if (text.StartsWith("/", StringComparison.Ordinal))
				{
					_portionType = PortionType.DateSeparator;
					_displayText = DatePickerHelper.GetDisplayFormat().DateSeparator;
				}
				else if (text.StartsWith(":", StringComparison.Ordinal))
				{
					_displayText = DatePickerHelper.GetDisplayFormat().TimeSeparator;
					_portionType = PortionType.TimeSeparator;
				}
			}
		}

		public bool IsTypedTextComplete()
		{
			switch (_portionType)
			{
			case PortionType.AMPM:
				DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
				return string.Equals(_typedText, format.AMDesignator, StringComparison.CurrentCultureIgnoreCase) || string.Equals(_typedText, format.PMDesignator, StringComparison.CurrentCultureIgnoreCase);
			case PortionType.Year:
				if (_typedText.Length == 4)
				{
					return true;
				}
				return false;
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				if (_typedText.Length == _displayText.Length)
				{
					return true;
				}
				if (_typedText.Length == 2)
				{
					if (char.IsLetter(_typedText, 0))
					{
						return false;
					}
					if (char.IsLetter(_typedText, 1))
					{
						return false;
					}
					return true;
				}
				return false;
			default:
				if (_typedText.Length == 2)
				{
					return true;
				}
				return false;
			}
		}

		public void LayoutEndingAt(Graphics graphics, Font font, int x, int y, int height, DateTime dateTime)
		{
			LayoutStartingAt(graphics, font, x, y, height, dateTime);
			_bounds.X -= _bounds.Width;
		}

		public void LayoutStartingAt(Graphics graphics, Font font, int x, int y, int height, DateTime dateTime)
		{
			float textWidth;
			using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(_useTextRenderer))
			{
				textRenderer.NoPadding = true;
				textWidth = MeasureWidestText(textRenderer, graphics, font);
			}
			_bounds = new Rectangle(x, y, (int)Math.Round(textWidth, 0), height);
		}

		private float MeasureWidestText(TextLayoutRenderer textRenderer, Graphics graphics, Font font)
		{
			DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
			switch (_portionType)
			{
			case PortionType.AMPM:
				return Math.Max(MeasureText(textRenderer, graphics, font, FormatText(new DateTime(2000, 1, 1, 1, 0, 0), false)), MeasureText(textRenderer, graphics, font, FormatText(new DateTime(2000, 1, 1, 13, 0, 0), false)));
			case PortionType.Month:
			case PortionType.Day:
			case PortionType.Hour12:
			case PortionType.Hour24:
			case PortionType.Minute:
			case PortionType.Second:
				return MeasureText(textRenderer, graphics, font, "00");
			case PortionType.Year:
				return MeasureText(textRenderer, graphics, font, _isActive ? "0000" : FormatText(DateTime.Now, false));
			case PortionType.DayOfWeek:
				return MeasureWidest(textRenderer, graphics, font, _formatPattern == "ddd" ? format.AbbreviatedDayNames : format.DayNames);
			case PortionType.MonthName:
				return MeasureWidest(textRenderer, graphics, font, format.MonthNames);
			case PortionType.MonthAbbreviated:
				return MeasureWidest(textRenderer, graphics, font, format.AbbreviatedMonthNames);
			case PortionType.DateSeparator:
			case PortionType.TimeSeparator:
			case PortionType.Literal:
				return MeasureText(textRenderer, graphics, font, _displayText);
			default:
				return 0f;
			}
		}

		private static float MeasureText(TextLayoutRenderer textRenderer, Graphics graphics, Font font, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0f;
			}
			return textRenderer.MeasureText(graphics, text, font, 0).Width;
		}

		private static float MeasureWidest(TextLayoutRenderer textRenderer, Graphics graphics, Font font, string[] texts)
		{
			float widest = 0f;
			foreach (string text in texts)
			{
				widest = Math.Max(widest, MeasureText(textRenderer, graphics, font, text));
			}
			return widest;
		}

		public PortionType GetPortionType()
		{
			return _portionType;
		}

		public bool IsEditable()
		{
			switch (_portionType)
			{
			case PortionType.DayOfWeek:
			case PortionType.DateSeparator:
			case PortionType.TimeSeparator:
			case PortionType.Literal:
				return false;
			default:
				return true;
			}
		}

		public DateTime GetIncremented(DateTime dateTime, FieldIncrements increments, DateTime maxDate)
		{
			int year = dateTime.Year;
			int month = dateTime.Month;
			int day = dateTime.Day;
			int hour = dateTime.Hour;
			int minute = dateTime.Minute;
			int second = dateTime.Second;
			switch (_portionType)
			{
			case PortionType.AMPM:
				hour = ((hour >= HoursPerHalfDay) ? (hour - HoursPerHalfDay) : (hour + HoursPerHalfDay));
				break;
			case PortionType.Day:
				day = WrapDay(day + increments._dayIncrement, DateTime.DaysInMonth(year, month));
				break;
			case PortionType.Hour12:
				hour += increments._hourIncrement;
				if (hour >= HoursPerDay)
				{
					hour -= HoursPerDay;
				}
				break;
			case PortionType.Hour24:
				hour += increments._hourIncrement;
				if (hour >= HoursPerDay)
				{
					hour -= HoursPerDay;
				}
				break;
			case PortionType.Minute:
				minute += increments._minuteIncrement;
				if (minute >= MinutesPerHour)
				{
					minute -= MinutesPerHour;
				}
				break;
			case PortionType.Month:
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				month += increments._monthIncrement;
				if (month > MonthsPerYear)
				{
					month -= MonthsPerYear;
				}
				break;
			case PortionType.Second:
				second += increments._secondIncrement;
				if (second >= SecondsPerMinute)
				{
					second -= SecondsPerMinute;
				}
				break;
			case PortionType.Year:
				year = (int)Math.Min((long)year + increments._yearIncrement, maxDate.Year);
				_fieldValue = year;
				break;
			}
			if (day > DateTime.DaysInMonth(year, month))
			{
				day = DateTime.DaysInMonth(year, month);
			}
			return new DateTime(year, month, day, hour, minute, second);
		}

		public DateTime GetDecremented(DateTime dateTime, FieldIncrements increments, DateTime minDate)
		{
			int year = dateTime.Year;
			int month = dateTime.Month;
			int day = dateTime.Day;
			int hour = dateTime.Hour;
			int minute = dateTime.Minute;
			int second = dateTime.Second;
			switch (_portionType)
			{
			case PortionType.AMPM:
				hour = ((hour >= HoursPerHalfDay) ? (hour - HoursPerHalfDay) : (hour + HoursPerHalfDay));
				break;
			case PortionType.Day:
				day = WrapDay(day - increments._dayIncrement, DateTime.DaysInMonth(year, month));
				break;
			case PortionType.Hour12:
				if (hour < HoursPerHalfDay)
				{
					hour -= increments._hourIncrement;
					if (hour < 0)
					{
						hour += HoursPerDay;
					}
				}
				else
				{
					hour -= increments._hourIncrement;
					if (hour < 0)
					{
						hour += HoursPerHalfDay;
					}
				}
				break;
			case PortionType.Hour24:
				hour -= increments._hourIncrement;
				if (hour < 0)
				{
					hour += HoursPerDay;
				}
				break;
			case PortionType.Minute:
				minute -= increments._minuteIncrement;
				if (minute < 0)
				{
					minute += MinutesPerHour;
				}
				break;
			case PortionType.Month:
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				month -= increments._monthIncrement;
				if (month < 1)
				{
					month += MonthsPerYear;
				}
				break;
			case PortionType.Second:
				second -= increments._secondIncrement;
				if (second < 0)
				{
					second += SecondsPerMinute;
				}
				break;
			case PortionType.Year:
				year = (int)Math.Max((long)year - increments._yearIncrement, minDate.Year);
				_fieldValue = year;
				break;
			}
			if (day > DateTime.DaysInMonth(year, month))
			{
				day = DateTime.DaysInMonth(year, month);
			}
			return new DateTime(year, month, day, hour, minute, second);
		}

		// Keeps a day number inside 1..daysInMonth, wrapping around at both ends.
		private static int WrapDay(int day, int daysInMonth)
		{
			return ((day - 1) % daysInMonth + daysInMonth) % daysInMonth + 1;
		}

		public string GetFormatPattern()
		{
			return _formatPattern;
		}

		public string FormatText(DateTime dateTime, bool isNullDate)
		{
			if (_hasTypedValue && (_isActive || _maskedValidation == MaskedValidation.OnLeave))
			{
				return _displayText;
			}
			if (isNullDate && _portionType != PortionType.Literal && _portionType != PortionType.DateSeparator && _portionType != PortionType.TimeSeparator)
			{
				return "";
			}
			switch (_portionType)
			{
			case PortionType.DateSeparator:
			case PortionType.TimeSeparator:
			case PortionType.Literal:
				return _displayText;
			case PortionType.Year:
				if (_isActive)
				{
					return _fieldValue.ToString(DatePickerHelper.GetDisplayCulture());
				}
				break;
			}
			if (_formatPattern.Length == 1)
			{
				return dateTime.ToString("%" + _formatPattern, DatePickerHelper.GetDisplayCulture());
			}
			return dateTime.ToString(_formatPattern, DatePickerHelper.GetDisplayCulture());
		}

		public void LoadFromDate(DateTime dateTime)
		{
			_typedText = "";
			switch (_portionType)
			{
			case PortionType.AMPM:
				if (dateTime.Hour < HoursPerHalfDay)
				{
					_fieldValue = 0;
				}
				else
				{
					_fieldValue = 1;
				}
				break;
			case PortionType.Day:
				_fieldValue = dateTime.Day;
				break;
			case PortionType.Hour12:
				_fieldValue = dateTime.Hour;
				if (_fieldValue > HoursPerHalfDay)
				{
					_fieldValue -= HoursPerHalfDay;
				}
				break;
			case PortionType.Hour24:
				_fieldValue = dateTime.Hour;
				break;
			case PortionType.Minute:
				_fieldValue = dateTime.Minute;
				break;
			case PortionType.Month:
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				_fieldValue = dateTime.Month;
				break;
			case PortionType.Second:
				_fieldValue = dateTime.Second;
				break;
			case PortionType.Year:
				_fieldValue = dateTime.Year;
				if (!_hasTypedValue)
				{
					_typedText = _fieldValue.ToString(DatePickerHelper.GetDisplayCulture());
					_displayText = _typedText;
				}
				break;
			}
			_currentValue = dateTime;
		}

		public bool GetIsActive()
		{
			return _isActive;
		}

		public void SetIsActive(bool isActive)
		{
			if (IsEditable() && _isActive != isActive)
			{
				_isActive = isActive;
				if (_maskedValidation == MaskedValidation.OnChange)
				{
					_hasTypedValue = false;
				}
			}
		}

		public DateTime ApplyTypedValue(DateTime dateTime)
		{
			if (_hasTypedValue)
			{
				DateTime result = dateTime;
				switch (_portionType)
				{
				case PortionType.AMPM:
					if (_fieldValue == 0)
					{
						if (dateTime.Hour > 11)
						{
							result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour - HoursPerHalfDay, dateTime.Minute, dateTime.Second);
						}
					}
					else if (dateTime.Hour < HoursPerHalfDay)
					{
						result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + HoursPerHalfDay, dateTime.Minute, dateTime.Second);
					}
					break;
				case PortionType.Day:
					if (_fieldValue > 0 && _fieldValue <= DateTime.DaysInMonth(dateTime.Year, dateTime.Month))
					{
						result = new DateTime(dateTime.Year, dateTime.Month, _fieldValue, dateTime.Hour, dateTime.Minute, dateTime.Second);
					}
					break;
				case PortionType.Hour12:
					if (_fieldValue > 0 && _fieldValue < 13)
					{
						result = ((dateTime.Hour <= 11) ? new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, _fieldValue, dateTime.Minute, dateTime.Second) : ((_fieldValue != 12) ? new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, _fieldValue + 12, dateTime.Minute, dateTime.Second) : new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 12, dateTime.Minute, dateTime.Second)));
					}
					break;
				case PortionType.Hour24:
					if (_fieldValue >= 0 && _fieldValue < HoursPerDay)
					{
						result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, _fieldValue, dateTime.Minute, dateTime.Second);
					}
					break;
				case PortionType.Minute:
					if (_fieldValue >= 0 && _fieldValue < 60)
					{
						result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, _fieldValue, dateTime.Second);
					}
					break;
				case PortionType.Month:
				case PortionType.MonthAbbreviated:
				case PortionType.MonthName:
					if (_fieldValue > 0 && _fieldValue <= MonthsPerYear)
					{
						int day = Math.Min(dateTime.Day, DateTime.DaysInMonth(dateTime.Year, _fieldValue));
						result = new DateTime(dateTime.Year, _fieldValue, day, dateTime.Hour, dateTime.Minute, dateTime.Second);
					}
					break;
				case PortionType.Second:
					if (_fieldValue >= 0 && _fieldValue < 60)
					{
						result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, _fieldValue);
					}
					break;
				case PortionType.Year:
				{
					int year = dateTime.Year;
					if (int.TryParse(_typedText, NumberStyles.None, CultureInfo.InvariantCulture, out int typedYear))
					{
						_fieldValue = DatePickerHelper.GetDisplayFormat().Calendar.ToFourDigitYear(typedYear);
						year = _fieldValue;
					}
					int yearDay = Math.Min(dateTime.Day, DateTime.DaysInMonth(year, dateTime.Month));
					result = new DateTime(year, dateTime.Month, yearDay, dateTime.Hour, dateTime.Minute, dateTime.Second);
					break;
				}
				}
				return result;
			}
			return dateTime;
		}

		public bool GetHasTypedValue()
		{
			return _hasTypedValue;
		}

		public void SetHasTypedValue(bool hasTypedValue)
		{
			_hasTypedValue = hasTypedValue;
		}

		public bool TryAcceptChar(char character, DateTime dateTime)
		{
			bool accepted = false;
			switch (_portionType)
			{
			case PortionType.AMPM:
				accepted = TryAcceptMeridiem(character, dateTime);
				break;
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				accepted = TryAcceptDigit(character, dateTime);
				if (accepted && _fieldValue >= 1 && _fieldValue <= MonthsPerYear)
				{
					DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
					_displayText = _portionType == PortionType.MonthName ? format.MonthNames[_fieldValue - 1] : format.AbbreviatedMonthNames[_fieldValue - 1];
				}
				else
				{
					accepted = TryMatchMonthName(character, dateTime);
				}
				break;
			default:
				accepted = TryAcceptDigit(character, dateTime);
				break;
			}
			if (accepted)
			{
				_hasTypedValue = true;
			}
			return accepted;
		}

		private bool TryMatchMonthName(char character, DateTime dateTime)
		{
			string prefix = _typedText + character;
			prefix = prefix.ToUpper();
			string[] monthNames = ((_portionType != PortionType.MonthAbbreviated) ? DatePickerHelper.GetDisplayFormat().MonthNames : DatePickerHelper.GetDisplayFormat().AbbreviatedMonthNames);
			int startIndex = _fieldValue;
			if (startIndex < 0)
			{
				startIndex = 0;
			}
			for (int i = startIndex; i < monthNames.Length; i++)
			{
				if (monthNames[i].StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
				{
					_fieldValue = i + 1;
					_displayText = monthNames[i];
					_typedText = prefix;
					return true;
				}
			}
			for (int j = 0; j < startIndex; j++)
			{
				if (monthNames[j].StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
				{
					_fieldValue = j + 1;
					_displayText = monthNames[j];
					_typedText = prefix;
					return true;
				}
			}
			if (_typedText == "")
			{
				return false;
			}
			_typedText = "";
			return TryMatchMonthName(character, dateTime);
		}

		private bool TryAcceptDigit(char character, DateTime dateTime)
		{
			if (char.IsDigit(character))
			{
				string digits = _typedText + character;
				if (!TrySetNumber(digits))
				{
					digits = character.ToString();
					return TrySetNumber(digits);
				}
				return true;
			}
			return false;
		}

		private bool TrySetNumber(string digits)
		{
			if (digits == "0")
			{
				_typedText = "0";
				_displayText = "0";
				return true;
			}
			if (int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int number) && IsValueInRange(number))
			{
				_fieldValue = number;
				_typedText = digits;
				_displayText = _typedText;
				return true;
			}
			return false;
		}

		private bool IsValueInRange(int value)
		{
			switch (_portionType)
			{
			case PortionType.Day:
				if (_maskedValidation == MaskedValidation.OnLeave)
				{
					if (value > 0 && value <= 31)
					{
						return true;
					}
					return false;
				}
				if (value > 0 && value <= DateTime.DaysInMonth(_currentValue.Year, _currentValue.Month))
				{
					return true;
				}
				return false;
			case PortionType.Hour12:
				if (value > 0 && value < 13)
				{
					return true;
				}
				return false;
			case PortionType.Hour24:
				if (value >= 0 && value < 24)
				{
					return true;
				}
				return false;
			case PortionType.Minute:
			case PortionType.Second:
				if (value >= 0 && value <= 59)
				{
					return true;
				}
				return false;
			case PortionType.Month:
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				if (value > 0 && value < 13)
				{
					return true;
				}
				return false;
			case PortionType.Year:
				if (value > 0 && value < 10000)
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		private bool TryAcceptMeridiem(char character, DateTime dateTime)
		{
			string value = _typedText + char.ToUpper(character);
			if (DatePickerHelper.GetDisplayFormat().AMDesignator.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
			{
				_fieldValue = 0;
				_typedText = value;
				_displayText = DatePickerHelper.GetDisplayFormat().AMDesignator;
				return true;
			}
			if (DatePickerHelper.GetDisplayFormat().PMDesignator.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
			{
				_fieldValue = 1;
				_typedText = value;
				_displayText = DatePickerHelper.GetDisplayFormat().PMDesignator;
				return true;
			}
			if (_typedText.Length > 0)
			{
				value = new string(char.ToUpper(character), 1);
				if (DatePickerHelper.GetDisplayFormat().AMDesignator.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
				{
					_fieldValue = 0;
					_typedText = value;
					_displayText = DatePickerHelper.GetDisplayFormat().AMDesignator;
					return true;
				}
				if (DatePickerHelper.GetDisplayFormat().PMDesignator.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
				{
					_fieldValue = 1;
					_typedText = value;
					_displayText = DatePickerHelper.GetDisplayFormat().PMDesignator;
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
