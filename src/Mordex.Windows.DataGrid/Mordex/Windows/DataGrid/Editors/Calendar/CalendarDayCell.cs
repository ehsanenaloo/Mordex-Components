using System;
using System.Drawing;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal class CalendarDayCell
	{
		private DateTime _dateTime;

		private bool _isAdjacentMonth;

		private bool _calendarLayoutState2Boolean;

		private Rectangle _bounds;

		private string _dayText;

		private bool _isBold;

		private bool _isSelected;

		internal CalendarDayCell()
		{
		}

		internal DateTime GetDate()
		{
			return _dateTime;
		}

		internal void SetDate(DateTime dateTime)
		{
			_dateTime = dateTime;
		}

		internal bool GetIsAdjacentMonth()
		{
			return _isAdjacentMonth;
		}

		internal void SetIsAdjacentMonth(bool condition)
		{
			_isAdjacentMonth = condition;
		}

		internal bool GetIsVisible()
		{
			return _calendarLayoutState2Boolean;
		}

		internal void SetIsVisible(bool calendarLayoutState2Boolean)
		{
			_calendarLayoutState2Boolean = calendarLayoutState2Boolean;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle rectangle)
		{
			_bounds = rectangle;
		}

		internal string GetDayText()
		{
			return _dayText;
		}

		internal void SetDayText(string text)
		{
			_dayText = text;
		}

		internal bool GetIsBold()
		{
			return _isBold;
		}

		internal void SetIsBold(bool calendarLayoutState2Boolean2)
		{
			_isBold = calendarLayoutState2Boolean2;
		}

		internal bool GetIsSelected()
		{
			return _isSelected;
		}

		internal void SetIsSelected(bool calendarLayoutState2Boolean3)
		{
			_isSelected = calendarLayoutState2Boolean3;
		}
	}
}
