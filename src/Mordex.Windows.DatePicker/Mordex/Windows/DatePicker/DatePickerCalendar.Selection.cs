using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar
	{
		internal DateTime GetSelectedDate()
		{
			return _selectedDate;
		}

		internal void SetSelectedDate(DateTime date)
		{
			_isNullDateSelected = false;
			if (date.Date < _minDate.Date)
			{
				date = _minDate.Date;
			}
			else if (date.Date > _maxDate.Date)
			{
				date = _maxDate.Date;
			}
			if (!(_selectedDate != date.Date))
			{
				return;
			}
			_selectedDate = date.Date;
			if (_followSelection)
			{
				if (_selectedDate < _firstMonth)
				{
					FirstMonth = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
				}
				else
				{
					DateTime lastVisibleMonth = DatePickerHelper.AddMonthsClamped(_firstMonth, Math.Max(_monthLayouts.Length - 1, 0));
					DateTime lastVisibleDay = new DateTime(lastVisibleMonth.Year, lastVisibleMonth.Month, DateTime.DaysInMonth(lastVisibleMonth.Year, lastVisibleMonth.Month));
					if (_selectedDate > lastVisibleDay)
					{
						_isAdjustingFirstMonth = true;
						FirstMonth = DatePickerHelper.AddMonthsClamped(new DateTime(_selectedDate.Year, _selectedDate.Month, 1), -(_monthLayouts.Length - 1));
						_isAdjustingFirstMonth = false;
					}
				}
			}
			Invalidate();
		}

		public DateTime GetDateAt(int x, int y)
		{
			return GetCellAt(x, y, true)?.GetDate().Date ?? DateTime.MinValue;
		}

		public DateTime GetDateAt(Point pt)
		{
			return GetDateAt(pt.X, pt.Y);
		}

		public DateTime GetDateAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetDateAt(point.X, point.Y);
		}

		internal DateTime GetMaxDate()
		{
			return _maxDate;
		}

		internal void SetMaxDate(DateTime maxDate)
		{
			if (_maxDate != maxDate)
			{
				if (maxDate < _minDate)
				{
					throw new ArgumentOutOfRangeException("The MaxDate must be greater or equal than the MinDate");
				}
				if (maxDate > new DateTime(9998, 12, 31))
				{
					throw new ArgumentOutOfRangeException(maxDate.ToShortDateString() + " is not a valid value for 'MaxDate'.  'MaxDate' must be lower than or equal to 12/31/9998");
				}
				_maxDate = maxDate;
				_calendarControl._maxMonth = _maxDate;
				DateTime lastVisibleMonth = DatePickerHelper.AddMonthsClamped(_firstMonth, Math.Max(_monthLayouts.Length - 1, 0));
				DateTime lastVisibleDay = new DateTime(lastVisibleMonth.Year, lastVisibleMonth.Month, DateTime.DaysInMonth(lastVisibleMonth.Year, lastVisibleMonth.Month));
				if (_maxDate < lastVisibleDay)
				{
					FirstMonth = DatePickerHelper.AddMonthsClamped(_maxDate, -Math.Max(_monthLayouts.Length - 1, 0));
				}
			}
		}

		internal DateTime GetMinDate()
		{
			return _minDate;
		}

		internal void SetMinDate(DateTime minDate)
		{
			if (_minDate != minDate)
			{
				if (minDate > _maxDate)
				{
					throw new ArgumentOutOfRangeException("The MinDate must be lower or equal than the MaxDate");
				}
				if (minDate < new DateTime(1, 2, 1))
				{
					throw new ArgumentOutOfRangeException(minDate.ToShortDateString() + " is not a valid value for 'MinDate'.  'MinDate' must be greater than or equal to 2/1/0001");
				}
				_minDate = minDate;
				_calendarControl._minMonth = _minDate;
				if (_minDate > FirstMonth)
				{
					FirstMonth = _minDate;
				}
			}
		}
	}
}
