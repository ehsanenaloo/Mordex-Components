using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	internal class WorkingHoursSchemaLayoutState
	{
		private string _headerFormat;

		private WorkingHoursSchema _schema;

		private DateRange _dateRange;

		private DateTime _gridStartDate;

		private int _monthIndex;

		private int _month;

		private bool _isHeaderHot;

		private bool _isHeaderPressed;

		private int _year;

		private DateTime _minDate;

		private DateTime _maxDate;

		private Rectangle _headerBounds;

		private SchedulerFormatStyleLayoutState[] _weekdayHeaderCells;

		private SchedulerFormatStyleLayoutState[,] _dayCells;

		private Size _totalSize;

		private Rectangle _leftMarginBounds;

		private Rectangle _rightMarginBounds;

		private Size _cellSize;

		internal WorkingHoursSchemaLayoutState(WorkingHoursSchema schema, string headerFormat)
		{
			_headerFormat = headerFormat;
			_weekdayHeaderCells = new SchedulerFormatStyleLayoutState[7];
			_dayCells = new SchedulerFormatStyleLayoutState[6, 7];
			for (int i = 0; i < 7; i++)
			{
				_weekdayHeaderCells[i] = new SchedulerFormatStyleLayoutState();
				for (int j = 0; j < 6; j++)
				{
					_dayCells[j, i] = new SchedulerFormatStyleLayoutState();
				}
			}
			_schema = schema;
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

		internal void SetMonth(int workingHoursSchemaLayoutStateInteger)
		{
			_month = workingHoursSchemaLayoutStateInteger;
		}

		public bool IsHeaderHot()
		{
			return _isHeaderHot;
		}

		public void SetHeaderHot(bool isHeaderHot)
		{
			_isHeaderHot = isHeaderHot;
		}

		public bool IsHeaderPressed()
		{
			return _isHeaderPressed;
		}

		public void SetHeaderPressed(bool workingHoursSchemaLayoutStateBoolean)
		{
			_isHeaderPressed = workingHoursSchemaLayoutStateBoolean;
		}

		internal int GetYear()
		{
			return _year;
		}

		internal void SetYear(int year)
		{
			_year = year;
		}

		internal void SetMinDate(DateTime workingHoursSchemaLayoutStateDateTime)
		{
			_minDate = workingHoursSchemaLayoutStateDateTime;
		}

		internal void SetMaxDate(DateTime maxDate)
		{
			_maxDate = maxDate;
		}

		internal string GetHeaderText()
		{
			return new DateTime(_year, _month, 1).ToString(_headerFormat, Application.CurrentCulture);
		}

		internal Rectangle GetHeaderBounds()
		{
			return _headerBounds;
		}

		internal void SetHeaderBounds(Rectangle rectangle)
		{
			_headerBounds = rectangle;
		}

		internal SchedulerFormatStyleLayoutState GetWeekdayHeaderCell(int column)
		{
			return _weekdayHeaderCells[column];
		}

		internal SchedulerFormatStyleLayoutState GetDayCell(int row, int integerArgument)
		{
			return _dayCells[row, integerArgument];
		}

		internal Size GetTotalSize()
		{
			return _totalSize;
		}

		internal void SetTotalSize(Size size)
		{
			_totalSize = size;
		}

		internal void LayoutGrid(int width, int height, int widthArgument, Point pos)
		{
			_cellSize = new Size(width, height);
			_headerBounds.Offset(pos);
			int bottom = _headerBounds.Bottom;
			int cellLeft = widthArgument + pos.X + _headerBounds.Left;
			for (int i = 0; i < 7; i++)
			{
				_weekdayHeaderCells[i].SetBounds(new Rectangle(cellLeft, bottom, width, height + 1));
				cellLeft += width;
			}
			bottom++;
			for (int j = 0; j < 6; j++)
			{
				bottom += height;
				cellLeft = pos.X + widthArgument + _headerBounds.Left;
				for (int i = 0; i < 7; i++)
				{
					_dayCells[j, i].SetBounds(new Rectangle(cellLeft, bottom, width, height));
					cellLeft += width;
				}
			}
			SetTotalSize(new Size(_dayCells[5, 6].GetBounds().Right + widthArgument, _dayCells[5, 6].GetBounds().Bottom));
			_leftMarginBounds = new Rectangle(pos.X + _headerBounds.Left, _headerBounds.Bottom, widthArgument, GetTotalSize().Height - _headerBounds.Height);
			_rightMarginBounds = new Rectangle(_weekdayHeaderCells[6].GetBounds().Right, _headerBounds.Bottom, widthArgument, _leftMarginBounds.Height);
		}

		internal void LayoutGridWithLeftOffset(int width, int height, int leftOffset, int widthArgument, Point pos)
		{
			_cellSize = new Size(width, height);
			_headerBounds.Offset(pos);
			int bottom = _headerBounds.Bottom;
			int cellLeft = leftOffset + widthArgument + pos.X;
			for (int i = 0; i < 7; i++)
			{
				_weekdayHeaderCells[i].SetBounds(new Rectangle(cellLeft, bottom, width, height + 1));
				cellLeft += width;
			}
			bottom++;
			int contentHeight = GetHeaderBounds().Height + 1;
			for (int j = 0; j < 6; j++)
			{
				bottom += height;
				contentHeight += height;
				cellLeft = pos.X + widthArgument + leftOffset;
				for (int i = 0; i < 7; i++)
				{
					_dayCells[j, i].SetBounds(new Rectangle(cellLeft, bottom, width, height));
					cellLeft += width;
				}
			}
			SetTotalSize(new Size(GetHeaderBounds().Width, contentHeight + height));
			_leftMarginBounds = new Rectangle(_headerBounds.Left, _headerBounds.Bottom, widthArgument, contentHeight - _headerBounds.Height);
			_rightMarginBounds = new Rectangle(_weekdayHeaderCells[6].GetBounds().Right, _headerBounds.Bottom, widthArgument, _leftMarginBounds.Height);
		}

		internal void BuildDayCells(bool showPreviousMonthDays, bool booleanArgument, CalendarDayOfWeek calendarDayOfWeek)
		{
			DateTime dateTime = DateTime.MinValue;
			DateTime end = DateTime.MinValue;
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			DateTime monthStart = new DateTime(_year, _month, 1);
			int firstDayOfWeekIndex = ((calendarDayOfWeek != CalendarDayOfWeek.Default) ? ((int)calendarDayOfWeek) : ((int)currentInfo.FirstDayOfWeek));
			int dayOfWeek = (int)monthStart.DayOfWeek;
			dayOfWeek = ((dayOfWeek > firstDayOfWeekIndex) ? (dayOfWeek - firstDayOfWeekIndex) : ((dayOfWeek < firstDayOfWeekIndex) ? (7 - (firstDayOfWeekIndex - dayOfWeek)) : 0));
			DateTime cellDate = (_gridStartDate = ((dayOfWeek != 0 || !showPreviousMonthDays) ? monthStart.AddDays(-dayOfWeek) : monthStart.AddDays(-7.0)));
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = _dayCells[i, j];
					schedulerFormatStyleLayoutState.SetDate(cellDate);
					if (cellDate < _minDate || cellDate > _maxDate)
					{
						schedulerFormatStyleLayoutState.SetWithinDateLimits(false);
					}
					else
					{
						schedulerFormatStyleLayoutState.SetWithinDateLimits(true);
					}
					ApplyWorkingHoursStyle(schedulerFormatStyleLayoutState, cellDate);
					schedulerFormatStyleLayoutState.SetVisible(true);
					schedulerFormatStyleLayoutState.SetBold(false);
					schedulerFormatStyleLayoutState.SetSelected(false);
					schedulerFormatStyleLayoutState.SetOutsideMonth(false);
					if (cellDate.Month != _month)
					{
						if (cellDate < monthStart)
						{
							if (showPreviousMonthDays)
							{
								schedulerFormatStyleLayoutState.SetOutsideMonth(true);
							}
							else
							{
								schedulerFormatStyleLayoutState.SetVisible(false);
							}
						}
						else if (booleanArgument)
						{
							schedulerFormatStyleLayoutState.SetOutsideMonth(true);
						}
						else
						{
							schedulerFormatStyleLayoutState.SetVisible(false);
						}
					}
					if (schedulerFormatStyleLayoutState.IsVisible())
					{
						schedulerFormatStyleLayoutState.SetText(cellDate.Day.ToString(Application.CurrentCulture));
						if (dateTime == DateTime.MinValue)
						{
							dateTime = cellDate;
						}
						end = cellDate;
					}
					cellDate = cellDate.AddDays(1.0);
				}
			}
			_dateRange = new DateRange(dateTime, end);
		}

		internal Rectangle GetLeftMarginBounds()
		{
			return _leftMarginBounds;
		}

		internal Rectangle GetRightMarginBounds()
		{
			return _rightMarginBounds;
		}

		private void ApplyWorkingHoursStyle(SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState, DateTime start)
		{
			if (schedulerFormatStyleLayoutState.IsWithinDateLimits() && _schema != null)
			{
				SchedulerHourRange schedulerHourRange = _schema.FindHourRange(new DateRange(start, start.AddDays(1.0)));
				if (schedulerHourRange != null)
				{
					schedulerFormatStyleLayoutState.SetSchedulerFormatStyle(schedulerHourRange.FormatStyle);
				}
				else
				{
					schedulerFormatStyleLayoutState.SetSchedulerFormatStyle(null);
				}
			}
			else
			{
				schedulerFormatStyleLayoutState.SetSchedulerFormatStyle(null);
			}
		}

		internal DateRange GetDateRange()
		{
			return _dateRange;
		}

		internal SchedulerFormatStyleLayoutState GetDayCellForDate(DateTime dateTime)
		{
			dateTime = dateTime.Date;
			if (dateTime >= _dateRange.Start && dateTime <= _dateRange.End)
			{
				int dayOffset;
				if (dateTime.Month == _gridStartDate.Month)
				{
					dayOffset = dateTime.Day - _gridStartDate.Day;
				}
				else
				{
					DateTime date = dateTime.Date;
					DateTime gridStartDate = _gridStartDate.Date;
					dayOffset = date.Subtract(gridStartDate).Days;
				}
				int row = dayOffset / 7;
				int column = dayOffset % 7;
				return _dayCells[row, column];
			}
			return null;
		}

		internal SchedulerFormatStyleLayoutState GetCellAtPoint(int x, int integerArgument, bool snapToVisibleCell)
		{
			try
			{
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = _dayCells[0, 0];
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = _dayCells[5, 6];
				if (integerArgument < schedulerFormatStyleLayoutState.GetBounds().Top || integerArgument > schedulerFormatStyleLayoutStateArgument.GetBounds().Bottom)
				{
					return null;
				}
				if (x < schedulerFormatStyleLayoutState.GetBounds().Left || x > schedulerFormatStyleLayoutStateArgument.GetBounds().Right)
				{
					return null;
				}
				x -= schedulerFormatStyleLayoutState.GetBounds().Left;
				integerArgument -= schedulerFormatStyleLayoutState.GetBounds().Top;
				int row = integerArgument / _cellSize.Height;
				int column = x / _cellSize.Width;
				if (row > 5)
				{
					row = 5;
				}
				if (column > 6)
				{
					column = 6;
				}
				SchedulerFormatStyleLayoutState cell = _dayCells[row, column];
				if (cell.IsVisible() || !snapToVisibleCell)
				{
					return cell;
				}
				int searchColumn = column;
				if (row == 0)
				{
					while (!cell.IsVisible())
					{
						if (searchColumn > 6)
						{
							return null;
						}
						cell = _dayCells[row, searchColumn];
						searchColumn++;
					}
				}
				else
				{
					while (!cell.IsVisible())
					{
						if (searchColumn < 0)
						{
							return null;
						}
						cell = _dayCells[row, searchColumn];
						searchColumn--;
					}
				}
				return cell;
			}
			catch
			{
				return GetCellAtPoint(x, integerArgument, snapToVisibleCell);
			}
		}
	}
}
