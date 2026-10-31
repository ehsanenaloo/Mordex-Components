using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			SetFirstVisibleMonth(_firstVisibleMonth.AddMonths(_monthScrollStep));
			_scrollTimer.Interval = 200;
		}

		private void UpdateCalendar()
		{
			_scrollTimer = new Timer();
			_scrollTimer.Interval = 200;
			_scrollTimer.Tick += OnTimerTick;
			_todayButton = new TimelineCalendarButton(this, 0);
			_noneButton = new TimelineCalendarButton(this, 0);
			_monthLayoutTemplate = new TimelineLayoutState();
			UpdateDayNames();
			_firstVisibleMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			if (_calendarFont == null)
			{
				_calendarFont = new Font(base.Font, FontStyle.Bold);
			}
			_todayRectColor = Color.Empty;
			_headerFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.Header);
			_daysFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
			_selectedFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.Selected);
			_monthBackgroundStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
			_trailingFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.Trailing);
			_dayHeadersFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
			_calendarControl = new CalendarControl(!ShouldUseCompatibleTextRendering());
			_calendarControl._owner = this;
			_calendarControl.Visible = false;
			_calendarControl.Location = new Point(7, 34);
			_calendarControl.TabIndex = 0;
			_calendarControl.Size = new Size(0, 0);
			TabStop = false;
		}

		private void OnCalendarTimerTick(object sender, EventArgs eventArgs)
		{
			if (Visible)
			{
				TimelineUtilities.SetControlCapture(this, true);
			}
			if (_calendarTimer != null)
			{
				_calendarTimer.Dispose();
				_calendarTimer = null;
			}
		}

		internal TimelineCalendarDay GetDayCellAt(int x, int integerArgument, bool snapToMonth)
		{
			try
			{
				int rowIndex = integerArgument / _monthLayoutTemplate.GetTotalSize().Height;
				int columnIndex = x / _monthLayoutTemplate.GetTotalSize().Width;
				if (rowIndex > _monthRowCount - 1 || columnIndex > _monthColumnCount - 1)
				{
					return null;
				}
				TimelineLayoutState timelineLayoutState = _timelineLayoutStateArray[rowIndex, columnIndex];
				return timelineLayoutState.GetCellAtPoint(x, integerArgument, snapToMonth);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void UpdateTimelineLayoutState(TimelineLayoutState timelineLayoutState)
		{
			if (_timelineLayoutState != null)
			{
				_timelineLayoutState.SetHeaderHot(false);
				Invalidate(_timelineLayoutState.GetHeaderBounds());
			}
			_timelineLayoutState = timelineLayoutState;
			if (_timelineLayoutState != null)
			{
				_timelineLayoutState.SetHeaderHot(true);
				Invalidate(_timelineLayoutState.GetHeaderBounds());
			}
		}

		private void UpdateCalendarTimelineLayoutState(TimelineLayoutState calendarTimelineLayoutState)
		{
			if (_calendarTimelineLayoutState != null)
			{
				_calendarTimelineLayoutState.SetHeaderPressed(false);
				Invalidate(_calendarTimelineLayoutState.GetHeaderBounds());
			}
			_calendarTimelineLayoutState = calendarTimelineLayoutState;
			if (_calendarTimelineLayoutState != null)
			{
				_calendarTimelineLayoutState.SetHeaderPressed(true);
				Invalidate(_calendarTimelineLayoutState.GetHeaderBounds());
			}
		}

		private Brush GetMonthBodyBrush(TimelineLayoutState timelineLayoutState, Rectangle rectangle, TimelineFormatStyle timelineFormatStyle)
		{
			rectangle.Inflate(1, 0);
			return timelineFormatStyle.GetBackBrush(rectangle);
		}

		private TimelineLayoutState FindMonthByHeaderAt(int x, int integerArgument, bool excludeEdges)
		{
			for (int i = 0; i < _monthRowCount; i++)
			{
				for (int j = 0; j < _monthColumnCount; j++)
				{
					TimelineLayoutState timelineLayoutState = _timelineLayoutStateArray[i, j];
					Rectangle rectangle = timelineLayoutState.GetHeaderBounds();
					if (excludeEdges)
					{
						rectangle.Inflate(-12, -1);
					}
					if (rectangle.Contains(x, integerArgument))
					{
						return timelineLayoutState;
					}
				}
			}
			return null;
		}

		private TimelineLayoutState FindMonthByWeekNumberArea(int x, int integerArgument)
		{
			try
			{
				for (int i = 0; i < _monthRowCount; i++)
				{
					for (int j = 0; j < _monthColumnCount; j++)
					{
						TimelineLayoutState timelineLayoutState = _timelineLayoutStateArray[i, j];
						if (timelineLayoutState.GetLeftMarginBounds().Contains(x, integerArgument))
						{
							return timelineLayoutState;
						}
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		private Font GetWeekNumberFont()
		{
			if (_weekNumberFont == null)
			{
				_weekNumberFont = new Font(Font.FontFamily, Font.SizeInPoints - 2f);
			}
			return _weekNumberFont;
		}

		internal CalendarWeekRule GetWeekRule()
		{
			return timeline.WeekRule;
		}

		private Brush GetDayBackgroundBrush(TimelineCalendarDay dayCell, TimelineFormatStyle timelineFormatStyle, TimelineFormatStyle timelineFormatStyleArgument, TimelineFormatStyle trailingStyle, bool isSelected)
		{
			if (isSelected)
			{
				return timelineFormatStyleArgument.GetBackBrush(dayCell.GetBounds());
			}
			if (dayCell.IsOutsideMonth())
			{
				return trailingStyle.GetBackBrush(dayCell.GetBounds());
			}
			return timelineFormatStyle.GetBackBrush(dayCell.GetBounds());
		}

		private Font GetDayFont(TimelineCalendarDay dayCell, TimelineFormatStyle timelineFormatStyle, TimelineFormatStyle timelineFormatStyleArgument, TimelineFormatStyle trailingStyle, bool isSelected)
		{
			if (isSelected)
			{
				return timelineFormatStyleArgument.ResolveFont(base.Font);
			}
			if (dayCell.IsOutsideMonth())
			{
				return trailingStyle.ResolveFont(base.Font);
			}
			return timelineFormatStyle.ResolveFont(base.Font);
		}

		private Brush GetDayTextBrush(TimelineCalendarDay dayCell, TimelineFormatStyle timelineFormatStyle, TimelineFormatStyle timelineFormatStyleArgument, TimelineFormatStyle trailingStyle, bool isSelected)
		{
			if (!dayCell.IsInRange())
			{
				return GdiCache.Brush(SystemColors.GrayText);
			}
			if (isSelected)
			{
				return timelineFormatStyleArgument.GetForeColorBrush(GetEffectiveForeColor());
			}
			if (dayCell.IsOutsideMonth())
			{
				return trailingStyle.GetForeColorBrush(GetEffectiveForeColor());
			}
			return timelineFormatStyle.GetForeColorBrush(GetEffectiveForeColor());
		}

		internal void UpdateCalendarControlTextRendering()
		{
			if (_calendarControl != null)
			{
				_calendarControl._useCompatibleTextRendering = ShouldUseCompatibleTextRendering();
			}
		}

		private System.DayOfWeek ToSystemDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case DayOfWeek.Sunday:
				return System.DayOfWeek.Sunday;
			case DayOfWeek.Monday:
				return System.DayOfWeek.Monday;
			case DayOfWeek.Tuesday:
				return System.DayOfWeek.Tuesday;
			case DayOfWeek.Wednesday:
				return System.DayOfWeek.Wednesday;
			case DayOfWeek.Thursday:
				return System.DayOfWeek.Thursday;
			case DayOfWeek.Friday:
				return System.DayOfWeek.Friday;
			case DayOfWeek.Saturday:
				return System.DayOfWeek.Saturday;
			default:
				return DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
			}
		}

		private void UpdateDayNames()
		{
			bool useShortestNames = ShouldUseShortestDayNames();
			int dayIndex = (int)ToSystemDayOfWeek(GetDayOfWeek());
			for (int i = 0; i < 7; i++)
			{
				if (dayIndex == 7)
				{
					dayIndex = 0;
				}
				string text = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName((System.DayOfWeek)dayIndex);
				if (_dayOfWeekAbbreviation == DayOfWeekAbbreviation.UseFirstLetter)
				{
					text = (useShortestNames ? DateTimeFormatInfo.CurrentInfo.GetShortestDayName((System.DayOfWeek)dayIndex) : text.Substring(0, 1));
				}
				_textArray[i] = text;
				dayIndex++;
			}
		}

		internal DayOfWeek GetDayOfWeek()
		{
			return _dayOfWeek;
		}

		internal void UpdateDayOfWeek(DayOfWeek dayOfWeek)
		{
			if (_dayOfWeek != dayOfWeek)
			{
				_dayOfWeek = dayOfWeek;
				UpdateDayNames();
			}
		}

		private DateTime CreateDateTime(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1);
		}

		internal DateTime GetFirstVisibleMonth()
		{
			return _firstVisibleMonth;
		}

		internal void SetFirstVisibleMonth(DateTime dateTimeArgument)
		{
			try
			{
				if (!(_firstVisibleMonth != dateTimeArgument))
				{
					return;
				}
				if (dateTimeArgument.Day != 1)
				{
					dateTimeArgument = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
				}
				int monthDifference = dateTimeArgument.Month - _firstVisibleMonth.Month;
				int yearDifference = dateTimeArgument.Year - _firstVisibleMonth.Year;
				monthDifference += yearDifference * 12;
				DateTime dateTime = dateTimeArgument;
				if (CreateDateTime(_minDate) > dateTimeArgument)
				{
					SetFirstVisibleMonth(_minDate);
					return;
				}
				if (_timelineLayoutStateArray.Length > 1)
				{
					dateTime = dateTimeArgument.AddMonths(_timelineLayoutStateArray.Length - 1);
				}
				dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
				if (GetScrollLimitDate() <= dateTime)
				{
					if (_timelineLayoutStateArray.Length == 0)
					{
						SetFirstVisibleMonth(GetScrollLimitDate().AddMonths(-1));
					}
					else
					{
						SetFirstVisibleMonth(GetScrollLimitDate().AddMonths(-_timelineLayoutStateArray.Length));
					}
				}
				else
				{
					_firstVisibleMonth = dateTimeArgument;
					UpdateTimelineLayoutStateArray(false);
					UpdateLayout(false);
					ApplyBoldedDates();
				}
			}
			catch
			{
			}
		}

		internal void UpdateTimelineLayoutStateArray(bool forceRebuild)
		{
			Point point = new Point(_borderSize.Width / 2, _borderSize.Height / 2);
			int monthIndex = 0;
			if (_monthLayoutTemplate.GetTotalSize().Width == 0 || _monthLayoutTemplate.GetTotalSize().Height == 0)
			{
				UpdateCellSizes();
			}
			if (!forceRebuild && _timelineLayoutStateArray.GetUpperBound(0) + 1 == _monthRowCount && _timelineLayoutStateArray.GetUpperBound(1) + 1 == _monthColumnCount)
			{
				return;
			}
			_timelineLayoutStateArray = new TimelineLayoutState[_monthRowCount, _monthColumnCount];
			for (int i = 0; i < _monthRowCount; i++)
			{
				for (int j = 0; j < _monthColumnCount; j++)
				{
					TimelineLayoutState timelineLayoutState = new TimelineLayoutState();
					timelineLayoutState.SetMonthIndex(monthIndex);
					timelineLayoutState.SetHeaderBounds(_monthLayoutTemplate.GetHeaderBounds());
					timelineLayoutState.LayoutGrid(_dayCellWidth, _dayCellHeight, _weekNumberColumnWidth, point, _borderSize);
					_timelineLayoutStateArray[i, j] = timelineLayoutState;
					point.X += _monthLayoutTemplate.GetTotalSize().Width;
					monthIndex++;
				}
				point.X = _borderSize.Width / 2;
				point.Y += _monthLayoutTemplate.GetTotalSize().Height;
			}
			UpdateLayout(true);
			int left = _contentBounds.Left;
			int top = _contentBounds.Top;
			int x = _contentBounds.Right - _headerHeight - 1;
			if (_contentBounds.Right <= 0)
			{
				x = left + _monthLayoutTemplate.GetTotalSize().Width - _headerHeight - 1;
			}
			ref Rectangle reference = ref _rectangleArray[0];
			reference = new Rectangle(left, top, _headerHeight - 1, _headerHeight - 1);
			ref Rectangle nextButtonRect = ref _rectangleArray[1];
			nextButtonRect = new Rectangle(x, top, _headerHeight - 1, _headerHeight - 1);
			Invalidate();
		}

		private void UpdateDateRange(DateRange dateRange)
		{
			if (_dateRange.Start != dateRange.Start || _dateRange.End != dateRange.End)
			{
				_dateRange = dateRange;
			}
		}

		private void ApplyBoldedDates()
		{
			try
			{
				for (int i = 0; i < _boldedDates.Length; i++)
				{
					DateTime dateTime = _boldedDates[i];
					GetMonthLayoutForDate(dateTime)?.GetDayCellForDate(dateTime)?.SetBold(true);
				}
				for (int i = 0; i < _annuallyBoldedDates.Length; i++)
				{
					DateTime dateTime = _annuallyBoldedDates[i];
					for (int j = 0; j <= _timelineLayoutStateArray.GetUpperBound(0); j++)
					{
						for (int k = 0; k <= _timelineLayoutStateArray.GetUpperBound(1); k++)
						{
							TimelineLayoutState timelineLayoutStateArgument = _timelineLayoutStateArray[j, k];
							int year = timelineLayoutStateArgument.GetYear();
							if (dateTime.Day > DateTime.DaysInMonth(year, dateTime.Month))
							{
								continue;
							}
							DateTime annualDate = new DateTime(year, dateTime.Month, dateTime.Day);
							if (annualDate >= timelineLayoutStateArgument.GetDateRange().Start && annualDate <= timelineLayoutStateArgument.GetDateRange().End)
							{
								TimelineCalendarDay timelineLayoutState2Argument = timelineLayoutStateArgument.GetDayCellForDate(annualDate);
								if (timelineLayoutState2Argument != null)
								{
									timelineLayoutState2Argument.SetBold(true);
								}
								else
								{
									timelineLayoutState2Argument = null;
								}
							}
						}
					}
				}
				if (timeline != null && _boldDatesWithItems)
				{
					MarkDatesWithItems();
				}
			}
			catch
			{
			}
		}

		private TimelineLayoutState GetMonthLayoutForDate(DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
			{
				return null;
			}
			if (dateTime < _dateRange.Start || dateTime > _dateRange.End)
			{
				return null;
			}
			for (int i = 0; i < _monthRowCount; i++)
			{
				for (int j = 0; j < _monthColumnCount; j++)
				{
					TimelineLayoutState timelineLayoutState = _timelineLayoutStateArray[i, j];
					if (dateTime >= timelineLayoutState.GetDateRange().Start && dateTime <= timelineLayoutState.GetDateRange().End)
					{
						return timelineLayoutState;
					}
				}
			}
			return null;
		}

		public DateTime GetDateAt(int x, int y)
		{
			return GetDayCellAt(x, y, true)?.GetDate().Date ?? DateTime.MinValue;
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

		private TimelineCalendarDay GetDayCellForDate(DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
			{
				return null;
			}
			if (dateTime < _dateRange.Start || dateTime > _dateRange.End)
			{
				return null;
			}
			return GetMonthLayoutForDate(dateTime)?.GetDayCellForDate(dateTime);
		}

		private void InvalidateDate(DateTime dateTime)
		{
			TimelineCalendarDay dayCell = GetDayCellForDate(dateTime);
			if (dayCell != null)
			{
				Invalidate(dayCell.GetBounds());
			}
		}

		internal DateTime GetMaxDate()
		{
			return _maxDate;
		}

		internal void UpdateMaxDate(DateTime dateTimeArgument)
		{
			if (_maxDate != dateTimeArgument.Date)
			{
				if (dateTimeArgument.Date < _minDate)
				{
					throw new ArgumentOutOfRangeException("The MaxDate must be greater or equal than the MinDate");
				}
				if (dateTimeArgument.Date > new DateTime(9998, 12, 31))
				{
					throw new ArgumentOutOfRangeException(dateTimeArgument.ToShortDateString() + " is not a valid value for 'MaxDate'.  'MaxDate' must be lower than or equal to 12/31/9998");
				}
				_maxDate = dateTimeArgument.Date;
				_calendarControl._maxDate = _maxDate;
				DateTime dateTime = _firstVisibleMonth.AddMonths(1);
				if (_timelineLayoutStateArray.Length > 1)
				{
					dateTime = _firstVisibleMonth.AddMonths(_timelineLayoutStateArray.Length - 1);
				}
				dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
				if (_maxDate < dateTime)
				{
					SetFirstVisibleMonth(_maxDate.AddMonths(-(_timelineLayoutStateArray.Length - 1)));
				}
			}
		}

		internal void UpdateMinDate(DateTime dateTime)
		{
			if (_minDate != dateTime.Date)
			{
				if (dateTime.Date > _maxDate)
				{
					throw new ArgumentOutOfRangeException("The MinDate must be lower or equal than the MaxDate");
				}
				if (dateTime.Date < new DateTime(1, 2, 1))
				{
					throw new ArgumentOutOfRangeException(dateTime.ToShortDateString() + " is not a valid value for 'MinDate'.  'MinDate' must be greater than or equal to 2/1/0001");
				}
				_minDate = dateTime.Date;
				_calendarControl._minDate = _minDate;
				if (_minDate > GetFirstVisibleMonth())
				{
					SetFirstVisibleMonth(_minDate);
				}
			}
		}

		internal void ApplyPickedMonth(object value)
		{
			UpdateCalendarTimelineLayoutState(null);
			if (value != null && (DateTime)value != DateTime.MinValue)
			{
				SetFirstVisibleMonth(((DateTime)value).AddMonths(-_clickedMonthIndex));
			}
			_scrollTimer.Enabled = false;
			_scrollTimer.Interval = 200;
			if (_isDroppedDown)
			{
				TimelineUtilities.SetControlCapture(this, true);
			}
		}
	}
}
