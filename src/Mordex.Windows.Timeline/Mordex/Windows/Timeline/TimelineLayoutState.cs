using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline
{
	internal class TimelineLayoutState
	{
		private DateTime _gridStartDate;

		private int _monthIndex;

		private int _month;

		private bool _isHeaderHot;

		private bool _isHeaderPressed;

		private int _year;

		private DateTime _minDate;

		private DateTime _maxDate;

		private Rectangle _headerBounds;

		private TimelineCalendarDay[] _weekdayHeaderCells;

		private TimelineCalendarDay[,] _dayCells;

		private Size _totalSize;

		private DateRange _dateRange;

		private Rectangle _leftMarginBounds;

		private Rectangle _rightMarginBounds;

		private Size _cellSize;

		internal TimelineLayoutState()
		{
			_weekdayHeaderCells = new TimelineCalendarDay[7];
			_dayCells = new TimelineCalendarDay[6, 7];
			for (int i = 0; i < 7; i++)
			{
				_weekdayHeaderCells[i] = new TimelineCalendarDay();
				for (int j = 0; j < 6; j++)
				{
					_dayCells[j, i] = new TimelineCalendarDay();
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

		internal void SetMonth(int timelineLayoutStateInteger)
		{
			_month = timelineLayoutStateInteger;
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

		public void SetHeaderPressed(bool timelineLayoutStateBoolean)
		{
			_isHeaderPressed = timelineLayoutStateBoolean;
		}

		internal int GetYear()
		{
			return _year;
		}

		internal void SetYear(int year)
		{
			_year = year;
		}

		internal void SetMinDate(DateTime timelineLayoutStateDateTime)
		{
			_minDate = timelineLayoutStateDateTime;
		}

		internal void SetMaxDate(DateTime maxDate)
		{
			_maxDate = maxDate;
		}

		internal string GetHeaderText()
		{
			return new DateTime(_year, _month, 1).ToString("MMMM yyyy", Application.CurrentCulture);
		}

		internal Rectangle GetHeaderBounds()
		{
			return _headerBounds;
		}

		internal void SetHeaderBounds(Rectangle rectangle)
		{
			_headerBounds = rectangle;
		}

		internal TimelineCalendarDay GetWeekdayHeaderCell(int column)
		{
			return _weekdayHeaderCells[column];
		}

		internal TimelineCalendarDay GetDayCell(int row, int integerArgument)
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

		internal void LayoutGrid(int width, int height, int widthArgument, Point pos, Size size)
		{
			_cellSize = new Size(width, height);
			_headerBounds.Offset(pos);
			int bottom = _headerBounds.Bottom;
			int cellLeft = widthArgument + pos.X;
			for (int i = 0; i < 7; i++)
			{
				_weekdayHeaderCells[i].SetBounds(new Rectangle(cellLeft, bottom, width, height + 1));
				cellLeft += width;
			}
			bottom++;
			for (int j = 0; j < 6; j++)
			{
				bottom += height;
				cellLeft = pos.X + widthArgument;
				for (int i = 0; i < 7; i++)
				{
					_dayCells[j, i].SetBounds(new Rectangle(cellLeft, bottom, width, height));
					cellLeft += width;
				}
			}
			SetTotalSize(new Size(_dayCells[5, 6].GetBounds().Right + widthArgument, _dayCells[5, 6].GetBounds().Bottom));
			_leftMarginBounds = new Rectangle(_headerBounds.Left, _headerBounds.Bottom, widthArgument, GetTotalSize().Height - _headerBounds.Bottom);
			_rightMarginBounds = new Rectangle(_weekdayHeaderCells[6].GetBounds().Right, _headerBounds.Bottom, widthArgument, _leftMarginBounds.Height);
		}

		internal void PopulateDayCells(bool showPreviousMonthDays, bool booleanArgument, DayOfWeek dayOfWeekArgument)
		{
			DateTime dateTime = DateTime.MinValue;
			DateTime end = DateTime.MinValue;
			_ = DateTimeFormatInfo.CurrentInfo;
			DateTime monthStart = new DateTime(_year, _month, 1);
			int firstDayOfWeekIndex = (int)TimelineUtilities.ToSystemDayOfWeek(dayOfWeekArgument);
			int dayOfWeek = (int)monthStart.DayOfWeek;
			dayOfWeek = ((dayOfWeek > firstDayOfWeekIndex) ? (dayOfWeek - firstDayOfWeekIndex) : ((dayOfWeek < firstDayOfWeekIndex) ? (7 - (firstDayOfWeekIndex - dayOfWeek)) : 0));
			DateTime cellDate = (_gridStartDate = ((dayOfWeek != 0 || !showPreviousMonthDays) ? monthStart.AddDays(-dayOfWeek) : monthStart.AddDays(-7.0)));
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					TimelineCalendarDay dayCell = _dayCells[i, j];
					dayCell.SetDate(cellDate);
					if (cellDate < _minDate || cellDate > _maxDate)
					{
						dayCell.SetInRange(false);
					}
					else
					{
						dayCell.SetInRange(true);
					}
					dayCell.SetVisible(true);
					dayCell.SetBold(false);
					dayCell.SetSelected(false);
					dayCell.SetOutsideMonth(false);
					if (cellDate.Month != _month)
					{
						if (cellDate < monthStart)
						{
							if (showPreviousMonthDays)
							{
								dayCell.SetOutsideMonth(true);
							}
							else
							{
								dayCell.SetVisible(false);
							}
						}
						else if (booleanArgument)
						{
							dayCell.SetOutsideMonth(true);
						}
						else
						{
							dayCell.SetVisible(false);
						}
					}
					if (dayCell.IsVisible())
					{
						dayCell.SetDayText(cellDate.Day.ToString(Application.CurrentCulture));
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

		internal DateRange GetDateRange()
		{
			return _dateRange;
		}

		internal Rectangle GetLeftMarginBounds()
		{
			return _leftMarginBounds;
		}

		internal TimelineCalendarDay GetDayCellForDate(DateTime dateTime)
		{
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

		internal TimelineCalendarDay GetCellAtPoint(int x, int integerArgument, bool snapToVisibleCell)
		{
			TimelineCalendarDay firstCell = _dayCells[0, 0];
			TimelineCalendarDay timelineLayoutState2Argument = _dayCells[5, 6];
			if (integerArgument < firstCell.GetBounds().Top || integerArgument > timelineLayoutState2Argument.GetBounds().Bottom)
			{
				return null;
			}
			if (x < firstCell.GetBounds().Left || x > timelineLayoutState2Argument.GetBounds().Right)
			{
				return null;
			}
			x -= firstCell.GetBounds().Left;
			integerArgument -= firstCell.GetBounds().Top;
			int row = integerArgument / _cellSize.Height;
			int column = x / _cellSize.Width;
			TimelineCalendarDay cell = _dayCells[row, column];
			if (cell.IsVisible() || !snapToVisibleCell)
			{
				return cell;
			}
			int searchColumn = column;
			if (row == 0)
			{
				while (!cell.IsVisible())
				{
					cell = _dayCells[row, searchColumn];
					searchColumn++;
				}
			}
			else
			{
				while (!cell.IsVisible())
				{
					cell = _dayCells[row, searchColumn];
					searchColumn--;
				}
			}
			return cell;
		}
	}
}
