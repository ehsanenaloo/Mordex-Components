using System;
using System.Drawing;
using System.Globalization;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal class CalendarLayoutState
	{
		private CalendarValueState _calendarValueState;

		private DateTime _dateTime;

		private int _index;

		private int _month;

		private int _year;

		private GridLayoutState _gridLayoutState;

		private CalendarDayCell[] _calendarLayoutState2Array;

		private CalendarDayCell[,] _calendarLayoutStateCalendarLayoutState2Array;

		private Size _blockSize;

		private Size _calendarLayoutStateSize;

		internal CalendarLayoutState()
		{
			_calendarLayoutState2Array = new CalendarDayCell[7];
			_calendarLayoutStateCalendarLayoutState2Array = new CalendarDayCell[6, 7];
			for (int i = 0; i < 7; i++)
			{
				_calendarLayoutState2Array[i] = new CalendarDayCell();
				for (int j = 0; j < 6; j++)
				{
					_calendarLayoutStateCalendarLayoutState2Array[j, i] = new CalendarDayCell();
				}
			}
			_gridLayoutState._isVisible = true;
			_gridLayoutState._isEnabled = true;
		}

		internal int GetIndex()
		{
			return _index;
		}

		internal void SetIndex(int value)
		{
			_index = value;
		}

		internal int GetMonth()
		{
			return _month;
		}

		internal void SetMonth(int calendarLayoutStateInteger)
		{
			_month = calendarLayoutStateInteger;
		}

		internal int GetYear()
		{
			return _year;
		}

		internal void SetYear(int year)
		{
			_year = year;
		}

		internal string GetMonthTitle()
		{
			return new DateTime(_year, _month, 1).ToString("MMMM yyyy", GridHelper.GetCurrentCulture());
		}

		internal GridLayoutState GetTitleLayout()
		{
			return _gridLayoutState;
		}

		internal Rectangle GetTitleRectangle()
		{
			return _gridLayoutState._bounds;
		}

		internal void SetTitleRectangle(Rectangle rectangle)
		{
			_gridLayoutState._bounds = rectangle;
		}

		internal CalendarDayCell GetWeekdayCell(int value)
		{
			return _calendarLayoutState2Array[value];
		}

		internal CalendarDayCell GetDayCell(int value, int otherValue)
		{
			return _calendarLayoutStateCalendarLayoutState2Array[value, otherValue];
		}

		internal Size GetBlockSize()
		{
			return _blockSize;
		}

		internal void SetBlockSize(Size size)
		{
			_blockSize = size;
		}

		internal void LayoutCells(int width, int height, int value, Point pos)
		{
			_calendarLayoutStateSize = new Size(width, height);
			_gridLayoutState._bounds.Offset(pos);
			int bottom = _gridLayoutState._bounds.Bottom;
			int otherValue = value + pos.X;
			for (int i = 0; i < 7; i++)
			{
				_calendarLayoutState2Array[i].SetBounds(new Rectangle(otherValue, bottom, width, height + 1));
				otherValue += width;
			}
			bottom++;
			for (int j = 0; j < 6; j++)
			{
				bottom += height;
				otherValue = pos.X + value;
				for (int i = 0; i < 7; i++)
				{
					_calendarLayoutStateCalendarLayoutState2Array[j, i].SetBounds(new Rectangle(otherValue, bottom, width, height));
					otherValue += width;
				}
			}
			SetBlockSize(new Size(_calendarLayoutStateCalendarLayoutState2Array[5, 6].GetBounds().Right + value, _calendarLayoutStateCalendarLayoutState2Array[5, 6].GetBounds().Bottom));
		}

		internal void FillDays(bool condition, bool otherCondition, DaysOfWeekEnum daysOfWeekEnum)
		{
			object Start = null;
			object end = null;
			DateTimeFormatInfo dateTimeFormat = GridHelper.GetCurrentCulture().DateTimeFormat;
			DateTime dateTime = new DateTime(_year, _month, 1);
			int otherDaysOfWeekEnum = ((daysOfWeekEnum != DaysOfWeekEnum.Default) ? ((int)daysOfWeekEnum) : ((int)dateTimeFormat.FirstDayOfWeek));
			int dayOfWeek = (int)dateTime.DayOfWeek;
			dayOfWeek -= otherDaysOfWeekEnum;
			if (dayOfWeek < 0)
			{
				dayOfWeek += 7;
			}
			int otherDayOfWeek = -1;
			DateTime addDays;
			if (dateTime > DateTime.MinValue)
			{
				addDays = dateTime.AddDays(-dayOfWeek);
			}
			else
			{
				otherDayOfWeek = dayOfWeek;
				addDays = dateTime;
			}
			_dateTime = addDays;
			bool nextCondition = false;
			for (int i = 0; i < 6; i++)
			{
				if (i > 0)
				{
					otherDayOfWeek = 0;
				}
				for (int j = 0; j < 7; j++)
				{
					CalendarDayCell calendarLayoutStateCalendarLayoutState2ArrayItem = _calendarLayoutStateCalendarLayoutState2Array[i, j];
					calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsVisible(false);
					calendarLayoutStateCalendarLayoutState2ArrayItem.SetDate(addDays);
					calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsVisible(true);
					calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsBold(false);
					calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsSelected(false);
					calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsAdjacentMonth(false);
					if (nextCondition)
					{
						calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsVisible(false);
					}
					else if (addDays.Month != _month)
					{
						if (addDays < dateTime)
						{
							if (condition)
							{
								calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsAdjacentMonth(true);
							}
							else
							{
								calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsVisible(false);
							}
						}
						else if (otherCondition)
						{
							calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsAdjacentMonth(true);
						}
						else
						{
							calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsVisible(false);
						}
					}
					if (calendarLayoutStateCalendarLayoutState2ArrayItem.GetIsVisible())
					{
						calendarLayoutStateCalendarLayoutState2ArrayItem.SetDayText(addDays.Day.ToString(GridHelper.GetCurrentCulture()));
						if (Start == null)
						{
							Start = addDays;
						}
						end = addDays;
					}
					if (otherDayOfWeek > j)
					{
						calendarLayoutStateCalendarLayoutState2ArrayItem.SetIsVisible(false);
						continue;
					}
					try
					{
						addDays = addDays.AddDays(1.0);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						nextCondition = true;
					}
				}
			}
			_calendarValueState = new CalendarValueState(Start, end);
		}

		internal CalendarValueState GetVisibleRange()
		{
			return _calendarValueState;
		}

		internal CalendarDayCell FindDayCellByDate(object value)
		{
			if (value is DateTime || (GridHelper.CompareDates(value, _calendarValueState.GetStart()) >= 0 && GridHelper.CompareDates(value, _calendarValueState.GetEnd()) <= 0))
			{
				DateTime dateTime = (DateTime)value;
				int days;
				if (dateTime.Month == _dateTime.Month)
				{
					days = dateTime.Day - _dateTime.Day;
				}
				else
				{
					DateTime date = dateTime.Date;
					DateTime otherDate = _dateTime.Date;
					days = date.Subtract(otherDate).Days;
				}
				int otherValue = days / 7;
				int nextValue = days % 7;
				return _calendarLayoutStateCalendarLayoutState2Array[otherValue, nextValue];
			}
			return null;
		}

		internal CalendarDayCell FindDayCellAt(int value, int otherValue)
		{
			CalendarDayCell calendarLayoutStateCalendarLayoutState2ArrayItem = _calendarLayoutStateCalendarLayoutState2Array[0, 0];
			CalendarDayCell calendarLayoutState2Argument = _calendarLayoutStateCalendarLayoutState2Array[5, 6];
			if (otherValue < calendarLayoutStateCalendarLayoutState2ArrayItem.GetBounds().Top || otherValue > calendarLayoutState2Argument.GetBounds().Bottom)
			{
				return null;
			}
			if (value < calendarLayoutStateCalendarLayoutState2ArrayItem.GetBounds().Left || value > calendarLayoutState2Argument.GetBounds().Right)
			{
				return null;
			}
			value -= calendarLayoutStateCalendarLayoutState2ArrayItem.GetBounds().Left;
			otherValue -= calendarLayoutStateCalendarLayoutState2ArrayItem.GetBounds().Top;
			int nextValue = otherValue / _calendarLayoutStateSize.Height;
			int previousValue = value / _calendarLayoutStateSize.Width;
			return _calendarLayoutStateCalendarLayoutState2Array[nextValue, previousValue];
		}
	}
}
