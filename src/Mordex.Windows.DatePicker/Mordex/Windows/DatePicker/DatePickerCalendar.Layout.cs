using System;
using System.Drawing;
using System.Globalization;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar
	{
		private const int ButtonTextPadding = 8;

		private const int ButtonRowExtraHeight = 21;

		private const int ButtonTopMargin = 7;

		private const int ButtonHeightExtra = 8;

		private const int ButtonGap = 18;

		internal void RebuildLayout()
		{
			UpdateBorderSize();
			ComputeDroppedControlSize(_columns, _rows);
			MeasureCells();
			UpdateMonthLayouts(true);
			UpdateMonthDates(true);
			Invalidate();
		}

		private BorderStyle GetEffectiveBorderStyle()
		{
			if (BorderStyle == BorderStyle.None)
			{
				return BorderStyle;
			}
			return BorderStyle.Flat;
		}

		internal void MeasureCells()
		{
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-_borderSize.Width / 2, -_borderSize.Height / 2);
			if (HeaderFont == null || _dayHeaderFont == null)
			{
				return;
			}
			_monthLayoutTemplate = new MonthLayout(HeaderFormat);
			using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UsesCompatibleTextRendering()))
			using (Graphics graphics = CreateGraphics())
			{
				textRenderer.NoPadding = true;
				SizeF headerSize = textRenderer.MeasureText(graphics, "88", HeaderFont, 0);
				_headerHeight = (int)headerSize.Height + 6;
				SizeF daySize = textRenderer.MeasureText(graphics, "88", _dayHeaderFont, 0);
				_dayCellWidth = (int)daySize.Width + 2;
				_dayCellHeight = (int)daySize.Height + 3;
				for (int column = 0; column < DatePickerHelper.DaysPerWeek; column++)
				{
					SizeF nameSize = textRenderer.MeasureText(graphics, _dayHeaderTexts[column], _dayHeaderFont, 0);
					if ((int)nameSize.Width + 2 > _dayCellWidth)
					{
						_dayCellWidth = (int)nameSize.Width + 2;
					}
				}
			}
			if (!UsesCompatibleTextRendering())
			{
				_dayCellWidth += 2;
			}
			_weekColumnWidth = _dayCellWidth;
			Rectangle monthBounds = new Rectangle(0, 0, _weekColumnWidth * 2 + _dayCellWidth * DatePickerHelper.DaysPerWeek, _headerHeight);
			_monthLayoutTemplate.SetHeaderBounds(monthBounds);
			_monthLayoutTemplate.ArrangeCells(_dayCellWidth, _dayCellHeight, _weekColumnWidth, new Point(0, 0));
			ComputeDroppedControlSize(_columns, _rows);
			Invalidate();
		}

		private DayOfWeek ToDayOfWeek(CalendarDayOfWeek calendarDayOfWeek)
		{
			switch (calendarDayOfWeek)
			{
			case CalendarDayOfWeek.Sunday:
				return DayOfWeek.Sunday;
			case CalendarDayOfWeek.Monday:
				return DayOfWeek.Monday;
			case CalendarDayOfWeek.Tuesday:
				return DayOfWeek.Tuesday;
			case CalendarDayOfWeek.Wednesday:
				return DayOfWeek.Wednesday;
			case CalendarDayOfWeek.Thursday:
				return DayOfWeek.Thursday;
			case CalendarDayOfWeek.Friday:
				return DayOfWeek.Friday;
			case CalendarDayOfWeek.Saturday:
				return DayOfWeek.Saturday;
			default:
				return DatePickerHelper.GetDisplayFormat().FirstDayOfWeek;
			}
		}

		private static bool IsArabicCulture()
		{
			return CultureInfo.CurrentCulture.Name.StartsWith("ar", StringComparison.InvariantCultureIgnoreCase);
		}

		// Chinese and Hebrew day names have no useful first letter, so the shortest name is used instead.
		private static bool UsesShortestDayNames()
		{
			string name = CultureInfo.CurrentCulture.Name;
			return string.Equals(name, "zh-CHS", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "zh-MO", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "zh-CN", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "zh-SG", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "zh-TW", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(name, "he-IL", StringComparison.OrdinalIgnoreCase);
		}

		// Rebuilds the cached day names, cell sizes and month grids when the thread culture changed since they were built.
		internal void EnsureCulture()
		{
			if (!DatePickerHelper.IsCultureStale(_cultureStamp))
			{
				return;
			}
			UpdateDayHeaderTexts();
			MeasureCells();
			ComputeDroppedControlSize(_columns, _rows);
			UpdateMonthLayouts(true);
		}

		internal void UpdateDayHeaderTexts()
		{
			_cultureStamp = CultureInfo.CurrentCulture;
			DateTimeFormatInfo format = DatePickerHelper.GetDisplayFormat();
			bool useShortestNames = UsesShortestDayNames();
			bool isArabic = IsArabicCulture();
			int firstDayIndex = (int)ToDayOfWeek(_firstDayOfWeek);
			for (int i = 0; i < DatePickerHelper.DaysPerWeek; i++)
			{
				DayOfWeek day = (DayOfWeek)((firstDayIndex + i) % DatePickerHelper.DaysPerWeek);
				string text = format.GetAbbreviatedDayName(day);
				if (_dayOfWeekAbbreviation == DayOfWeekAbbreviation.UseFirstLetter && text.Length > 0)
				{
					text = useShortestNames ? format.GetShortestDayName(day) : (isArabic ? text.Substring(text.Length - 1, 1) : text.Substring(0, 1));
				}
				_dayHeaderTexts[i] = text;
			}
		}

		private void UpdateBorderSize()
		{
			_borderSize = Size.Empty;
			int border = GetEffectiveBorderStyle() != BorderStyle.None ? 2 : 0;
			_borderSize = new Size(border + 6, border + 6);
		}

		private DateTime GetRangeEndExclusive()
		{
			int monthSpan = _maxDate.Month - _minDate.Month + (_maxDate.Year - _minDate.Year) * 12;
			if (monthSpan < _monthLayouts.Length - 1)
			{
				return GetMonthStart(_minDate).AddMonths(_monthLayouts.Length);
			}
			return _maxDate.AddMonths(1);
		}

		private DateTime GetMonthStart(DateTime date)
		{
			return new DateTime(date.Year, date.Month, 1);
		}

		internal void UpdateMonthLayouts(bool forceRebuild)
		{
			Point point = new Point(_borderSize.Width / 2, _borderSize.Height / 2);
			int monthIndex = 0;
			if (_monthLayoutTemplate.GetMonthSize().Width == 0 || _monthLayoutTemplate.GetMonthSize().Height == 0)
			{
				MeasureCells();
			}
			if (!forceRebuild && _monthLayouts.GetUpperBound(0) + 1 == _rows && _monthLayouts.GetUpperBound(1) + 1 == _columns)
			{
				return;
			}
			_monthLayouts = new MonthLayout[_rows, _columns];
			for (int i = 0; i < _rows; i++)
			{
				for (int j = 0; j < _columns; j++)
				{
					MonthLayout monthLayout = new MonthLayout(HeaderFormat);
					monthLayout.SetMonthIndex(monthIndex);
					monthLayout.SetHeaderBounds(_monthLayoutTemplate.GetHeaderBounds());
					monthLayout.ArrangeCells(_dayCellWidth, _dayCellHeight, _weekColumnWidth, point);
					_monthLayouts[i, j] = monthLayout;
					point.X += _monthLayoutTemplate.GetMonthSize().Width;
					monthIndex++;
				}
				point.X = _borderSize.Width / 2;
				point.Y += _monthLayoutTemplate.GetMonthSize().Height;
			}
			UpdateMonthDates(true);
			int left = _contentBounds.Left;
			int top = _contentBounds.Top;
			int nextArrowLeft = _contentBounds.Right - _headerHeight - 1;
			if (_contentBounds.Right <= 0)
			{
				nextArrowLeft = left + _monthLayoutTemplate.GetMonthSize().Width - _headerHeight - 1;
			}
			ref Rectangle previousArrowBounds = ref _scrollArrowBounds[0];
			previousArrowBounds = new Rectangle(left, top, _headerHeight - 1, _headerHeight - 1);
			ref Rectangle nextArrowBounds = ref _scrollArrowBounds[1];
			nextArrowBounds = new Rectangle(nextArrowLeft, top, _headerHeight - 1, _headerHeight - 1);
			Invalidate();
		}

		private void UpdateMonthDates(bool applyBoldedDates)
		{
			DateTime month = _firstMonth;
			for (int i = 0; i < _rows; i++)
			{
				for (int j = 0; j < _columns; j++)
				{
					MonthLayout monthLayout = _monthLayouts[i, j];
					monthLayout.SetMonth(month.Month);
					monthLayout.SetYear(month.Year);
					monthLayout.SetMinDate(_minDate);
					monthLayout.SetMaxDate(_maxDate);
					bool showDaysBefore = _showTrailingDays && i == 0 && j == 0;
					bool showDaysAfter = _showTrailingDays && ((i == _rows - 1) & (j == _columns - 1));
					monthLayout.FillDays(showDaysBefore, showDaysAfter, _firstDayOfWeek, DatePickerHelper.GetDisplayFormat());
					month = DatePickerHelper.AddMonthsClamped(month, 1);
				}
			}
			if (_monthLayouts.Length > 0)
			{
				SetVisibleRange(new DateRange(_monthLayouts[0, 0].GetVisibleRange()._start, _monthLayouts[_rows - 1, _columns - 1].GetVisibleRange()._end));
			}
			if (applyBoldedDates)
			{
				ApplyBoldedDates();
			}
			Invalidate();
		}

		private void SetVisibleRange(DateRange range)
		{
			if (_visibleRange._start != range._start || _visibleRange._end != range._end)
			{
				_visibleRange = range;
			}
		}

		private void ApplyBoldedDates()
		{
			for (int i = 0; i < _boldedDates.Length; i++)
			{
				DateTime boldedDate = _boldedDates[i];
				FindMonthLayoutContaining(boldedDate)?.GetCellForDate(boldedDate)?.SetIsBold(true);
			}
			foreach (DateTime annualDate in _annuallyBoldedDates)
			{
				foreach (MonthLayout monthLayout in _monthLayouts)
				{
					// The page can show days of the previous and next year around January and December.
					for (int yearOffset = -1; yearOffset <= 1; yearOffset++)
					{
						int year = monthLayout.GetYear() + yearOffset;
						if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
						{
							continue;
						}
						DateTime dateInYear = new DateTime(year, annualDate.Month, Math.Min(annualDate.Day, DateTime.DaysInMonth(year, annualDate.Month)));
						monthLayout.GetCellForDate(dateInYear)?.SetIsBold(true);
					}
				}
			}
		}

		private MonthLayout FindMonthLayoutContaining(DateTime date)
		{
			if (date == DateTime.MinValue)
			{
				return null;
			}
			if (date < _visibleRange._start || date > _visibleRange._end)
			{
				return null;
			}
			for (int i = 0; i < _rows; i++)
			{
				for (int j = 0; j < _columns; j++)
				{
					MonthLayout monthLayout = _monthLayouts[i, j];
					if (date >= monthLayout.GetVisibleRange()._start && date <= monthLayout.GetVisibleRange()._end)
					{
						return monthLayout;
					}
				}
			}
			return null;
		}

		protected internal void ComputeDroppedControlSize(int columns, int rows)
		{
			_rows = rows;
			_columns = columns;
			Size size = _monthLayoutTemplate.GetMonthSize();
			int lastRowHeight = size.Height;
			int contentWidth = size.Width * columns + _borderSize.Width / 2;
			int gridHeight = size.Height * rows + _borderSize.Height / 2;
			if (_showTodayButton || _showNoneButton)
			{
				int fontHeight;
				int todayButtonWidth = 0;
				int noneButtonWidth = 0;
				int minimumButtonWidth = size.Width / 3;
				using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UsesCompatibleTextRendering()))
				using (Graphics graphics = CreateGraphics())
				{
					textRenderer.Alignment = StringAlignment.Center;
					fontHeight = (int)HeaderFont.GetHeight(graphics);
					if (_showTodayButton)
					{
						todayButtonWidth = Math.Max((int)textRenderer.MeasureText(graphics, _todayButtonText, HeaderFont, 0).Width + ButtonTextPadding, minimumButtonWidth);
					}
					if (_showNoneButton)
					{
						noneButtonWidth = Math.Max((int)textRenderer.MeasureText(graphics, _noneButtonText, HeaderFont, 0).Width + ButtonTextPadding, minimumButtonWidth);
					}
				}
				lastRowHeight += fontHeight + ButtonRowExtraHeight;
				_buttonRowBounds = new Rectangle(0, size.Height, size.Width, fontHeight + ButtonRowExtraHeight);
				if (todayButtonWidth > noneButtonWidth)
				{
					noneButtonWidth = todayButtonWidth;
				}
				else if (_showTodayButton)
				{
					todayButtonWidth = noneButtonWidth;
				}
				int availableWidth = size.Width - 3;
				if (todayButtonWidth + noneButtonWidth > availableWidth)
				{
					if (_noneButtonText.Length > 0 && _todayButtonText.Length > 0)
					{
						todayButtonWidth = availableWidth / 2;
						noneButtonWidth = todayButtonWidth;
					}
					else if (_noneButtonText.Length > 0)
					{
						noneButtonWidth = availableWidth;
					}
					else
					{
						todayButtonWidth = availableWidth;
					}
				}
				int buttonTop = gridHeight + ButtonTopMargin;
				int buttonHeight = fontHeight + ButtonHeightExtra;
				if (_showNoneButton)
				{
					if (_showTodayButton && _todayButtonText.Length > 0)
					{
						int buttonsLeft = (contentWidth - (ButtonGap + todayButtonWidth + noneButtonWidth)) / 2;
						_todayButton._bounds = new Rectangle(buttonsLeft, buttonTop, todayButtonWidth, buttonHeight);
						_noneButton._bounds = _todayButton._bounds;
						_noneButton._bounds.Offset(ButtonGap + todayButtonWidth, 0);
					}
					else
					{
						int noneLeft = (contentWidth - noneButtonWidth) / 2;
						_noneButton._bounds = new Rectangle(noneLeft, buttonTop, noneButtonWidth, buttonHeight);
						_todayButton._bounds = Rectangle.Empty;
					}
				}
				else
				{
					int todayLeft = (contentWidth - todayButtonWidth) / 2;
					_todayButton._bounds = new Rectangle(todayLeft, buttonTop, todayButtonWidth, buttonHeight);
					_noneButton._bounds = Rectangle.Empty;
				}
			}
			else
			{
				_buttonRowBounds = Rectangle.Empty;
				_todayButton._bounds = Rectangle.Empty;
				_noneButton._bounds = Rectangle.Empty;
			}
			Size = new Size(size.Width * columns + _borderSize.Width, size.Height * (rows - 1) + lastRowHeight + _borderSize.Width);
		}
	}
}
