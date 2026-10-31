using System;
using System.Drawing;
using System.Globalization;

namespace Mordex.Windows.DatePicker
{
	internal class MonthLayout
	{
		// A month page shows six weeks of seven days.
		internal const int GridRows = 6;

		internal const int GridColumns = 7;

		private string _headerFormat = "MMMM yyyy";

		private DateTime _gridStartDate;

		private int _monthIndex;

		private int _month;

		private bool _isHot;

		private bool _isPressed;

		private int _year;

		private DateTime _minDate;

		private DateTime _maxDate;

		private Rectangle _headerBounds;

		private CalendarCell[] _weekdayCells;

		private CalendarCell[,] _dayCells;

		private Size _monthSize;

		private DateRange _visibleRange;

		private Rectangle _leftMarginBounds;

		private Rectangle _rightMarginBounds;

		private Size _cellSize;

		internal MonthLayout(string headerFormat)
		{
			_headerFormat = headerFormat;
			_weekdayCells = new CalendarCell[GridColumns];
			_dayCells = new CalendarCell[GridRows, GridColumns];
			for (int i = 0; i < GridColumns; i++)
			{
				_weekdayCells[i] = new CalendarCell();
				for (int j = 0; j < GridRows; j++)
				{
					_dayCells[j, i] = new CalendarCell();
				}
			}
		}

		internal int GetMonthIndex()
		{
			return _monthIndex;
		}

		internal void SetMonthIndex(int monthIndex)
		{
			_monthIndex = monthIndex;
		}

		internal int GetMonth()
		{
			return _month;
		}

		internal void SetMonth(int month)
		{
			_month = month;
		}

		public bool GetIsHot()
		{
			return _isHot;
		}

		public void SetIsHot(bool isHot)
		{
			_isHot = isHot;
		}

		public bool GetIsPressed()
		{
			return _isPressed;
		}

		public void SetIsPressed(bool isPressed)
		{
			_isPressed = isPressed;
		}

		internal int GetYear()
		{
			return _year;
		}

		internal void SetYear(int year)
		{
			_year = year;
		}

		internal DateTime GetMinDate()
		{
			return _minDate;
		}

		internal void SetMinDate(DateTime minDate)
		{
			_minDate = minDate;
		}

		internal DateTime GetMaxDate()
		{
			return _maxDate;
		}

		internal void SetMaxDate(DateTime maxDate)
		{
			_maxDate = maxDate;
		}

		internal string GetHeaderText()
		{
			return DatePickerHelper.FormatDate(new DateTime(_year, _month, 1), _headerFormat);
		}

		internal Rectangle GetHeaderBounds()
		{
			return _headerBounds;
		}

		internal void SetHeaderBounds(Rectangle bounds)
		{
			_headerBounds = bounds;
		}

		internal CalendarCell GetWeekdayCell(int column)
		{
			return _weekdayCells[column];
		}

		internal CalendarCell GetDayCell(int row, int column)
		{
			return _dayCells[row, column];
		}

		internal Size GetMonthSize()
		{
			return _monthSize;
		}

		internal void SetMonthSize(Size monthSize)
		{
			_monthSize = monthSize;
		}

		internal void ArrangeCells(int cellWidth, int cellHeight, int marginWidth, Point origin)
		{
			_cellSize = new Size(cellWidth, cellHeight);
			_headerBounds.Offset(origin);
			int bottom = _headerBounds.Bottom;
			int cellLeft = marginWidth + origin.X;
			for (int i = 0; i < GridColumns; i++)
			{
				_weekdayCells[i].SetBounds(new Rectangle(cellLeft, bottom, cellWidth, cellHeight + 1));
				cellLeft += cellWidth;
			}
			bottom++;
			for (int j = 0; j < GridRows; j++)
			{
				bottom += cellHeight;
				cellLeft = origin.X + marginWidth;
				for (int i = 0; i < GridColumns; i++)
				{
					_dayCells[j, i].SetBounds(new Rectangle(cellLeft, bottom, cellWidth, cellHeight));
					cellLeft += cellWidth;
				}
			}
			SetMonthSize(new Size(_dayCells[GridRows - 1, GridColumns - 1].GetBounds().Right + marginWidth, _dayCells[GridRows - 1, GridColumns - 1].GetBounds().Bottom));
			_leftMarginBounds = new Rectangle(_headerBounds.Left, _headerBounds.Bottom, marginWidth, GetMonthSize().Height - _headerBounds.Bottom);
			_rightMarginBounds = new Rectangle(_weekdayCells[GridColumns - 1].GetBounds().Right, _headerBounds.Bottom, marginWidth, _leftMarginBounds.Height);
		}

		internal void FillDays(bool showDaysBefore, bool showDaysAfter, CalendarDayOfWeek firstDayOfWeek, DateTimeFormatInfo format)
		{
			DateTime firstVisibleDate = DateTime.MinValue;
			DateTime lastVisibleDate = DateTime.MinValue;
			DateTime monthStart = new DateTime(_year, _month, 1);
			int firstDayIndex = (firstDayOfWeek != CalendarDayOfWeek.Default) ? (int)firstDayOfWeek : (int)format.FirstDayOfWeek;
			int daysBeforeMonth = ((int)monthStart.DayOfWeek - firstDayIndex + GridColumns) % GridColumns;
			if (daysBeforeMonth == 0 && showDaysBefore)
			{
				daysBeforeMonth = GridColumns;
			}
			_gridStartDate = monthStart.AddDays(-daysBeforeMonth);
			DateTime cellDate = _gridStartDate;
			for (int row = 0; row < GridRows; row++)
			{
				for (int column = 0; column < GridColumns; column++)
				{
					CalendarCell cell = _dayCells[row, column];
					cell.SetDate(cellDate);
					cell.SetIsSelectable(cellDate >= _minDate && cellDate <= _maxDate);
					cell.SetIsBold(false);
					bool isShown = true;
					if (cellDate.Month != _month)
					{
						isShown = cellDate < monthStart ? showDaysBefore : showDaysAfter;
					}
					cell.SetIsVisible(isShown);
					cell.SetIsAdjacentMonth(isShown && cellDate.Month != _month);
					if (isShown)
					{
						cell.SetDayText(cellDate.Day.ToString(format));
						if (firstVisibleDate == DateTime.MinValue)
						{
							firstVisibleDate = cellDate;
						}
						lastVisibleDate = cellDate;
					}
					cellDate = cellDate.AddDays(1.0);
				}
			}
			_visibleRange = new DateRange(firstVisibleDate, lastVisibleDate);
		}

		internal DateRange GetVisibleRange()
		{
			return _visibleRange;
		}

		internal Rectangle GetLeftMarginBounds()
		{
			return _leftMarginBounds;
		}

		internal CalendarCell GetCellForDate(DateTime dateTime)
		{
			if (dateTime >= _visibleRange._start && dateTime <= _visibleRange._end)
			{
				int dayOffset = (dateTime.Date - _gridStartDate.Date).Days;
				if (dayOffset >= 0 && dayOffset < GridRows * GridColumns)
				{
					return _dayCells[dayOffset / GridColumns, dayOffset % GridColumns];
				}
			}
			return null;
		}

		internal CalendarCell FindCellAt(int x, int y, bool snapToVisible)
		{
			Rectangle firstBounds = _dayCells[0, 0].GetBounds();
			Rectangle lastBounds = _dayCells[GridRows - 1, GridColumns - 1].GetBounds();
			if (_cellSize.Width <= 0 || _cellSize.Height <= 0 || x < firstBounds.Left || x >= lastBounds.Right || y < firstBounds.Top || y >= lastBounds.Bottom)
			{
				return null;
			}
			int row = (y - firstBounds.Top) / _cellSize.Height;
			int column = (x - firstBounds.Left) / _cellSize.Width;
			CalendarCell cell = _dayCells[row, column];
			if (cell.GetIsVisible() || !snapToVisible)
			{
				return cell;
			}
			// Hidden cells are the trailing days. Move toward the visible days of the same row.
			int step = row == 0 ? 1 : -1;
			for (int scanColumn = column; scanColumn >= 0 && scanColumn < GridColumns; scanColumn += step)
			{
				if (_dayCells[row, scanColumn].GetIsVisible())
				{
					return _dayCells[row, scanColumn];
				}
			}
			return null;
		}
	}
}
