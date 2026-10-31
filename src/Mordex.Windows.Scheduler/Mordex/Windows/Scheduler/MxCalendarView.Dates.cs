using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView
	{
		void ISchedulerWorkingHoursOwner.InvalidateHour()
		{
			RefreshMonthLayouts(false);
			UpdateSelectionRange(false);
			Invalidate();
		}

		private void UpdateCalendar()
		{
			_iContainer = new Container();
			_calendarControl = new CalendarControl(!UseCompatibleTextRendering);
			_calendarControl.SetMonthFormat(HeaderFormat);
			_calendarControl._owner = this;
			BackColor = SystemColors.Window;
			Text = "Calendar";
			_calendarControl.Visible = false;
			_calendarControl.SetValueSelectedCallback(ApplyPickedMonth);
			base.Controls.Add(_calendarControl);
		}

		private void UpdateTimer(bool isDragScroll, int interval)
		{
			if (_scrollTimer == null)
			{
				_scrollTimer = new Timer();
				if (isDragScroll)
				{
					_scrollTimer.Tick += OnDragScrollTimerTick;
				}
				else
				{
					_scrollTimer.Tick += OnTimerTick;
				}
			}
			_scrollTimer.Interval = interval;
			_scrollTimer.Enabled = true;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (!base.Capture)
			{
				_scrollTimer.Enabled = false;
				return;
			}
			FirstMonth = _firstMonth.AddMonths(_scrollMonthStep);
			_scrollTimer.Interval = 200;
		}

		private SchedulerDayOfWeek ToSchedulerDayOfWeek(CalendarDayOfWeek calendarDayOfWeek)
		{
			switch (ToSystemDayOfWeek(calendarDayOfWeek))
			{
			case DayOfWeek.Monday:
				return SchedulerDayOfWeek.Monday;
			case DayOfWeek.Tuesday:
				return SchedulerDayOfWeek.Tuesday;
			case DayOfWeek.Wednesday:
				return SchedulerDayOfWeek.Wednesday;
			case DayOfWeek.Thursday:
				return SchedulerDayOfWeek.Thursday;
			case DayOfWeek.Friday:
				return SchedulerDayOfWeek.Friday;
			case DayOfWeek.Saturday:
				return SchedulerDayOfWeek.Saturday;
			default:
				return SchedulerDayOfWeek.Sunday;
			}
		}

		private CalendarDayOfWeek ToCalendarDayOfWeek(SchedulerDayOfWeek schedulerDayOfWeek)
		{
			switch (schedulerDayOfWeek)
			{
			case SchedulerDayOfWeek.Monday:
				return CalendarDayOfWeek.Monday;
			case SchedulerDayOfWeek.Tuesday:
				return CalendarDayOfWeek.Tuesday;
			case SchedulerDayOfWeek.Wednesday:
				return CalendarDayOfWeek.Wednesday;
			case SchedulerDayOfWeek.Thursday:
				return CalendarDayOfWeek.Thursday;
			case SchedulerDayOfWeek.Friday:
				return CalendarDayOfWeek.Friday;
			case SchedulerDayOfWeek.Saturday:
				return CalendarDayOfWeek.Saturday;
			default:
				return CalendarDayOfWeek.Sunday;
			}
		}

		private DateTime GetWeekStart(DateTime dateTime)
		{
			int firstDayIndex = (int)ToSystemDayOfWeek(FirstDayOfWeek);
			int dayOfWeek = (int)dateTime.DayOfWeek;
			int daysSinceWeekStart = 6;
			daysSinceWeekStart = ((dayOfWeek > firstDayIndex) ? (dayOfWeek - firstDayIndex) : ((dayOfWeek != firstDayIndex) ? (7 - (firstDayIndex - dayOfWeek)) : 0));
			return dateTime.AddDays(-daysSinceWeekStart);
		}

		private DateTime GetWeekEnd(DateTime dateTime)
		{
			int firstDayIndex = (int)ToSystemDayOfWeek(FirstDayOfWeek);
			int dayOfWeek = (int)dateTime.DayOfWeek;
			int daysUntilWeekEnd = 6;
			daysUntilWeekEnd = ((dayOfWeek > firstDayIndex) ? (6 - (dayOfWeek - firstDayIndex)) : ((dayOfWeek != firstDayIndex) ? (firstDayIndex - dayOfWeek - 1) : 6));
			return dateTime.AddDays(daysUntilWeekEnd);
		}

		private void UpdateWorkingHoursSchemaLayoutState(WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState)
		{
			if (_workingHoursSchemaLayoutState != null)
			{
				_workingHoursSchemaLayoutState.SetHeaderHot(false);
				Invalidate(_workingHoursSchemaLayoutState.GetHeaderBounds());
			}
			_workingHoursSchemaLayoutState = workingHoursSchemaLayoutState;
			if (_workingHoursSchemaLayoutState != null)
			{
				_workingHoursSchemaLayoutState.SetHeaderHot(true);
				Invalidate(_workingHoursSchemaLayoutState.GetHeaderBounds());
			}
		}

		private void UpdateCalendarWorkingHoursSchemaLayoutState(WorkingHoursSchemaLayoutState calendarWorkingHoursSchemaLayoutState)
		{
			if (_calendarWorkingHoursSchemaLayoutState != null)
			{
				_calendarWorkingHoursSchemaLayoutState.SetHeaderPressed(false);
				Invalidate(_calendarWorkingHoursSchemaLayoutState.GetHeaderBounds());
			}
			_calendarWorkingHoursSchemaLayoutState = calendarWorkingHoursSchemaLayoutState;
			if (_calendarWorkingHoursSchemaLayoutState != null)
			{
				_calendarWorkingHoursSchemaLayoutState.SetHeaderPressed(true);
				Invalidate(_calendarWorkingHoursSchemaLayoutState.GetHeaderBounds());
			}
		}

		private Brush GetMonthBackgroundBrush(WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState, Rectangle rectangle, SchedulerFormatStyle schedulerFormatStyle, SchedulerFormatStyle schedulerFormatStyleArgument)
		{
			rectangle.Inflate(1, 0);
			if (workingHoursSchemaLayoutState.GetMonth() % 2 != 0)
			{
				return schedulerFormatStyleArgument.GetBackBrush(rectangle);
			}
			return schedulerFormatStyle.GetBackBrush(rectangle);
		}

		private WorkingHoursSchemaLayoutState FindMonthHeaderAt(int x, int integerArgument, bool shouldNarrowBounds)
		{
			int monthCount = _monthRowCount * _monthColumnCount;
			if (monthCount == _workingHoursSchemaLayoutStateArray.LongLength && _workingHoursSchemaLayoutStateArray != null && _workingHoursSchemaLayoutStateArray.Length > 0)
			{
				for (int i = 0; i < _monthRowCount; i++)
				{
					for (int j = 0; j < _monthColumnCount; j++)
					{
						WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = _workingHoursSchemaLayoutStateArray[i, j];
						Rectangle rectangle = workingHoursSchemaLayoutState.GetHeaderBounds();
						if (shouldNarrowBounds)
						{
							rectangle.Inflate(-12, -1);
						}
						if (rectangle.Contains(x, integerArgument))
						{
							return workingHoursSchemaLayoutState;
						}
					}
				}
			}
			return null;
		}

		private WorkingHoursSchemaLayoutState FindMonthWithDayAt(int x, int integerArgument)
		{
			try
			{
				for (int i = 0; i < _monthRowCount; i++)
				{
					for (int j = 0; j < _monthColumnCount; j++)
					{
						WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = _workingHoursSchemaLayoutStateArray[i, j];
						if (workingHoursSchemaLayoutState.GetCellAtPoint(x, integerArgument, false) != null)
						{
							return workingHoursSchemaLayoutState;
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

		private WorkingHoursSchemaLayoutState FindMonthAt(int x, int integerArgument)
		{
			try
			{
				int monthCount = _monthRowCount * _monthColumnCount;
				if (_workingHoursSchemaLayoutStateArray != null && _workingHoursSchemaLayoutStateArray.LongLength == monthCount)
				{
					for (int i = 0; i < _monthRowCount; i++)
					{
						for (int j = 0; j < _monthColumnCount; j++)
						{
							WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = _workingHoursSchemaLayoutStateArray[i, j];
							if (workingHoursSchemaLayoutState.GetLeftMarginBounds().Contains(x, integerArgument))
							{
								return workingHoursSchemaLayoutState;
							}
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

		private void MeasureMonthLayout()
		{
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-_borderSize.Width / 2, -_borderSize.Height / 2);
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateIsPaddingDisabled(true);
			try
			{
				if (HeaderFont == null || _calendarFont == null)
				{
					return;
				}
				_monthTemplate = new WorkingHoursSchemaLayoutState(_workingHoursSchema, HeaderFormat);
				Graphics graphics;
				try
				{
					graphics = CreateGraphics();
				}
				catch
				{
					return;
				}
				SizeF sizeF = textLayoutRenderer.RenderContent(graphics, "88", HeaderFont, 0);
				_headerHeight = new Size((int)sizeF.Width, (int)sizeF.Height).Height + 6;
				sizeF = textLayoutRenderer.RenderContent(graphics, "88", _calendarFont, 0);
				Size size = new Size((int)sizeF.Width, (int)sizeF.Height);
				_dayCellWidth = size.Width + 2;
				_dayCellHeight = size.Height + 3;
				for (int i = 0; i < 7; i++)
				{
					sizeF = textLayoutRenderer.RenderContent(graphics, _textArray[i], base.Font, 0);
					size = new Size((int)sizeF.Width, (int)sizeF.Height);
					if (size.Width + 2 > _dayCellWidth)
					{
						_dayCellWidth = size.Width + 2;
					}
				}
				_sideColumnWidth = _dayCellWidth;
				int monthWidth = _sideColumnWidth * 2 + _dayCellWidth * 7;
				int monthHeight = _headerHeight + 1 + _dayCellHeight * 7;
				int monthsLeft = (_contentBounds.Width - _monthColumnCount * monthWidth) / 2;
				if (monthsLeft < _contentBounds.Left)
				{
					monthsLeft = _contentBounds.Left;
				}
				int monthsTop = (_contentBounds.Height - _monthRowCount * monthHeight - _dayCellHeight) / 2;
				if (monthsTop < _contentBounds.Top)
				{
					monthsTop = _contentBounds.Top;
				}
				Rectangle rectangle = new Rectangle(monthsLeft, monthsTop, monthWidth, _headerHeight);
				_monthTemplate.SetHeaderBounds(rectangle);
				_monthTemplate.LayoutGridWithLeftOffset(_dayCellWidth, _dayCellHeight, monthsLeft, _sideColumnWidth, new Point(0, 0));
				graphics.Dispose();
				graphics = null;
			}
			catch (Exception ex)
			{
				ReportPaintError(ex);
			}
			textLayoutRenderer.ResetFormat();
		}

		private DayOfWeek ToSystemDayOfWeek(CalendarDayOfWeek calendarDayOfWeek)
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
				return DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
			}
		}

		private bool UsesShortestDayNames()
		{
			string name = CultureInfo.CurrentCulture.Name;
			if (string.Compare(name, "zh-CHS", ignoreCase: true) == 0 || string.Compare(name, "zh-MO", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "zh-CN", ignoreCase: true) == 0 || string.Compare(name, "zh-SG", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "zh-TW", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "he-IL", ignoreCase: true) == 0)
			{
				return true;
			}
			return false;
		}

		private void UpdateWeekdayNames()
		{
			bool useShortestNames = UsesShortestDayNames();
			bool isArabicCulture = IsArabicCulture();
			int dayIndex = (int)ToSystemDayOfWeek(_firstDayOfWeek);
			for (int i = 0; i < 7; i++)
			{
				if (dayIndex == 7)
				{
					dayIndex = 0;
				}
				string text = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName((DayOfWeek)dayIndex);
				if (_dayOfWeekAbbreviation == DayOfWeekAbbreviation.UseFirstLetter)
				{
					text = (useShortestNames ? DateTimeFormatInfo.CurrentInfo.GetShortestDayName((DayOfWeek)dayIndex) : (isArabicCulture ? text.Substring(text.Length - 1, 1) : text.Substring(0, 1)));
				}
				_textArray[i] = text;
				dayIndex++;
			}
		}

		private void UpdateDateCollection(bool shouldInvalidate)
		{
			ClearAppointmentBoldDates();
			if (BoldDatesMode == BoldDatesMode.None || !_scheduler.Created)
			{
				return;
			}
			_appointmentBoldDates = new DateCollection(null);
			if (_scheduler.Appointments.Count == 0)
			{
				Invalidate();
				return;
			}
			ArrayList arrayList = _scheduler.Appointments.GetVisibleAppointmentsOverlappingRange(_dateRange);
			DateTime dateTime = _dateRange.Start;
			foreach (SchedulerAppointment item in arrayList)
			{
				if (!ShouldBoldForOwner(item._ownerValue) || item.EndTime <= dateTime)
				{
					continue;
				}
				dateTime = item.StartTime.Date;
				while (item.StartTime.Date <= dateTime && (item.EndTime > dateTime || (item.EndTime == dateTime && item.Duration == TimeSpan.Zero)))
				{
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(dateTime);
					if (schedulerFormatStyleLayoutState != null)
					{
						schedulerFormatStyleLayoutState.SetBold(true);
						_appointmentBoldDates.AddWithoutNotify(dateTime);
					}
					dateTime = dateTime.AddDays(1.0);
				}
				dateTime = item.EndTime.Date;
			}
			if (shouldInvalidate)
			{
				Invalidate();
			}
		}

		private void UpdateMonthGrid()
		{
			_contentBounds = base.ClientRectangle;
			Size size = _monthTemplate.GetTotalSize();
			if ((size.Width == 0) | (size.Height == 0))
			{
				return;
			}
			int previousRowCount = _monthRowCount;
			int previousColumnCount = _monthColumnCount;
			_monthColumnCount = (_contentBounds.Width - _borderSize.Width) / size.Width;
			if (_monthColumnCount == 0)
			{
				_monthColumnCount = 1;
			}
			_monthRowCount = (_contentBounds.Height - _borderSize.Height) / size.Height;
			if (_monthRowCount == 0)
			{
				_monthRowCount = 1;
			}
			if (previousColumnCount != _monthColumnCount || previousRowCount != _monthRowCount)
			{
				Invalidate();
			}
			_contentBounds.Inflate(-_borderSize.Width / 2, -_borderSize.Height / 2);
			if (!_isInitializing)
			{
				if (AutoSize)
				{
					base.Size = new Size(size.Width * _monthColumnCount + _borderSize.Width * 2, size.Height * _monthRowCount + _borderSize.Height * 2);
					return;
				}
				MeasureMonthLayout();
				UpdateWorkingHoursSchemaLayoutStateArray(true);
			}
		}

		private DateTime AddMonthsClamped(DateTime dateTime, int monthCount)
		{
			int year = dateTime.Year;
			if (Math.Abs(monthCount) > 12)
			{
				year = dateTime.Year + monthCount / 12;
				monthCount %= 12;
			}
			int month = dateTime.Month + monthCount;
			if (month > 12)
			{
				year += month / 12;
				month -= 12 * (month / 12);
			}
			else if (month == 0)
			{
				year--;
				month = 12;
			}
			else if (month < 0)
			{
				year += month / 12;
				if (month % 12 != 0)
				{
					year--;
				}
				month = 12 + month;
			}
			if (month == 0)
			{
				month++;
			}
			int daysInMonth = DateTime.DaysInMonth(year, month);
			if (daysInMonth < dateTime.Day)
			{
				return new DateTime(year, month, daysInMonth);
			}
			return new DateTime(year, month, dateTime.Day);
		}

		private ArrayList GetWeekDates(DateTime dateTimeArgument, int dayCount)
		{
			dateTimeArgument = GetWeekStart(dateTimeArgument);
			DateTime dateTime = dateTimeArgument.AddDays(dayCount);
			ArrayList arrayList = new ArrayList();
			if (_scheduler.View != SchedulerView.WorkWeek)
			{
				while (dateTimeArgument < dateTime)
				{
					arrayList.Add(dateTimeArgument);
					dateTimeArgument = dateTimeArgument.AddDays(1.0);
				}
			}
			else
			{
				arrayList.Add(dateTimeArgument);
			}
			return arrayList;
		}

		private DateTime GetFirstUnavailableMonth()
		{
			int monthSpan = _maxDate.Month - _minDate.Month + (_maxDate.Year - _minDate.Year) * 12;
			if (monthSpan < _workingHoursSchemaLayoutStateArray.Length - 1)
			{
				return CreateDateTime(_minDate).AddMonths(_workingHoursSchemaLayoutStateArray.Length);
			}
			return _maxDate.AddMonths(1);
		}

		private DateTime CreateDateTime(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1);
		}

		private void UpdateWorkingHoursSchemaLayoutStateArray(bool shouldRebuild)
		{
			Point point = new Point(_borderSize.Width / 2, _borderSize.Height / 2);
			int monthIndex = 0;
			if (_monthTemplate.GetTotalSize().Width == 0 || _monthTemplate.GetTotalSize().Height == 0)
			{
				MeasureMonthLayout();
			}
			if (!shouldRebuild && _workingHoursSchemaLayoutStateArray.GetUpperBound(0) + 1 == _monthRowCount && _workingHoursSchemaLayoutStateArray.GetUpperBound(1) + 1 == _monthColumnCount)
			{
				return;
			}
			_workingHoursSchemaLayoutStateArray = new WorkingHoursSchemaLayoutState[_monthRowCount, _monthColumnCount];
			for (int i = 0; i < _monthRowCount; i++)
			{
				for (int j = 0; j < _monthColumnCount; j++)
				{
					WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = new WorkingHoursSchemaLayoutState(_workingHoursSchema, HeaderFormat);
					workingHoursSchemaLayoutState.SetMonthIndex(monthIndex);
					workingHoursSchemaLayoutState.SetHeaderBounds(_monthTemplate.GetHeaderBounds());
					workingHoursSchemaLayoutState.LayoutGridWithLeftOffset(_dayCellWidth, _dayCellHeight, _monthTemplate.GetHeaderBounds().Left, _sideColumnWidth, point);
					_workingHoursSchemaLayoutStateArray[i, j] = workingHoursSchemaLayoutState;
					point.X += _monthTemplate.GetTotalSize().Width;
					monthIndex++;
				}
				point.X = _borderSize.Width / 2;
				point.Y += _monthTemplate.GetTotalSize().Height;
			}
			RefreshMonthLayouts(true);
			int top;
			int x;
			int left;
			if (_workingHoursSchemaLayoutStateArray.Length > 0)
			{
				top = _workingHoursSchemaLayoutStateArray[0, 0].GetHeaderBounds().Top;
				x = _workingHoursSchemaLayoutStateArray[0, _monthColumnCount - 1].GetHeaderBounds().Right - _headerHeight - 1;
				left = _workingHoursSchemaLayoutStateArray[0, 0].GetHeaderBounds().Left;
			}
			else
			{
				left = _contentBounds.Left;
				x = _contentBounds.Right - _headerHeight - 1;
				top = _contentBounds.Top;
			}
			if (_contentBounds.Right <= 0)
			{
				x = left + _monthTemplate.GetTotalSize().Width - _headerHeight - 1;
			}
			ref Rectangle reference = ref _rectangleArray[0];
			reference = new Rectangle(left, top, _headerHeight - 1, _headerHeight - 1);
			ref Rectangle nextButtonBounds = ref _rectangleArray[1];
			nextButtonBounds = new Rectangle(x, top, _headerHeight - 1, _headerHeight - 1);
			Invalidate();
		}

		private void RefreshMonthLayouts(bool shouldRefreshSelection)
		{
			DateTime dateTime = _firstMonth;
			try
			{
				for (int i = 0; i < _monthRowCount; i++)
				{
					for (int j = 0; j < _monthColumnCount; j++)
					{
						WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = _workingHoursSchemaLayoutStateArray[i, j];
						workingHoursSchemaLayoutState.SetMonth(dateTime.Month);
						workingHoursSchemaLayoutState.SetYear(dateTime.Year);
						workingHoursSchemaLayoutState.SetMinDate(_minDate);
						workingHoursSchemaLayoutState.SetMaxDate(_maxDate);
						bool showLeadingDays = _showTrailingDays && i == 0 && j == 0;
						bool showTrailingDays = _showTrailingDays && ((i == _monthRowCount - 1) & (j == _monthColumnCount - 1));
						workingHoursSchemaLayoutState.BuildDayCells(showLeadingDays, showTrailingDays, _firstDayOfWeek);
						dateTime = dateTime.AddMonths(1);
					}
				}
				if (_workingHoursSchemaLayoutStateArray.Length > 0)
				{
					UpdateDateRange(new DateRange(_workingHoursSchemaLayoutStateArray[0, 0].GetDateRange().Start, _workingHoursSchemaLayoutStateArray[_monthRowCount - 1, _monthColumnCount - 1].GetDateRange().End));
				}
			}
			catch
			{
			}
			if (shouldRefreshSelection)
			{
				UpdateSelectionRange(true);
			}
		}

		private void UpdateDateRange(DateRange dateRange)
		{
			if (_dateRange.Start != dateRange.Start || _dateRange.End != dateRange.End)
			{
				_dateRange = dateRange;
				OnDatesChanged(EventArgs.Empty);
			}
		}

		internal void ApplyPickedMonth(object value)
		{
			UpdateCalendarWorkingHoursSchemaLayoutState(null);
			DateTime dateTime = (DateTime)value;
			if (dateTime != DateTime.MinValue)
			{
				FirstMonth = dateTime.AddMonths(-_clickedMonthIndex);
			}
			StopScrollTimer();
		}

		private void RemoveBoldDates(DateTime[] dateTimeArray, bool isAnnual)
		{
			if (!isAnnual)
			{
				if (dateTimeArray.Length > 0)
				{
					foreach (DateTime dateTime in dateTimeArray)
					{
						FindMonthByDate(dateTime)?.GetDayCellForDate(dateTime)?.SetBold(false);
					}
				}
				return;
			}
			for (int j = 0; j < dateTimeArray.Length; j++)
			{
				DateTime dateToUnbold = dateTimeArray[j];
				for (int k = 0; k <= _workingHoursSchemaLayoutStateArray.GetUpperBound(0); k++)
				{
					for (int l = 0; l <= _workingHoursSchemaLayoutStateArray.GetUpperBound(1); l++)
					{
						WorkingHoursSchemaLayoutState workingHoursSchemaLayoutStateArgument = _workingHoursSchemaLayoutStateArray[k, l];
						DateTime dateInYear = new DateTime(workingHoursSchemaLayoutStateArgument.GetYear(), dateToUnbold.Month, dateToUnbold.Day);
						if (dateInYear > workingHoursSchemaLayoutStateArgument.GetDateRange().Start && dateInYear < workingHoursSchemaLayoutStateArgument.GetDateRange().End)
						{
							SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = workingHoursSchemaLayoutStateArgument.GetDayCellForDate(dateInYear);
							if (schedulerFormatStyleLayoutStateArgument != null)
							{
								schedulerFormatStyleLayoutStateArgument.SetBold(false);
							}
							else
							{
								schedulerFormatStyleLayoutStateArgument = null;
							}
						}
					}
				}
			}
		}

		private WorkingHoursSchemaLayoutState FindMonthByDate(DateTime dateTime)
		{
			dateTime = dateTime.Date;
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
					WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = _workingHoursSchemaLayoutStateArray[i, j];
					if (dateTime >= workingHoursSchemaLayoutState.GetDateRange().Start && dateTime <= workingHoursSchemaLayoutState.GetDateRange().End)
					{
						return workingHoursSchemaLayoutState;
					}
				}
			}
			return null;
		}

		public DateTime GetDateAt(int x, int y)
		{
			return FindDayCellAt(x, y, true)?.GetDate().Date ?? DateTime.MinValue;
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

		private SchedulerFormatStyleLayoutState FindDayCellByDate(DateTime dateTime)
		{
			if (dateTime == DateTime.MinValue)
			{
				return null;
			}
			if (dateTime < _dateRange.Start || dateTime > _dateRange.End)
			{
				return null;
			}
			return FindMonthByDate(dateTime)?.GetDayCellForDate(dateTime);
		}

		private void InvalidateDay(DateTime dateTime)
		{
			SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(dateTime);
			if (schedulerFormatStyleLayoutState != null)
			{
				Invalidate(schedulerFormatStyleLayoutState.GetBounds());
			}
		}

		private DateTime AddDaysClamped(DateTime dateTime, int dayCount)
		{
			if (dayCount > 0)
			{
				dateTime = ((!(dateTime.AddDays(dayCount) > MaxDate)) ? dateTime.AddDays(dayCount) : MaxDate);
			}
			else if (dayCount < 0)
			{
				dateTime = ((!(dateTime.AddDays(dayCount) < MinDate)) ? dateTime.AddDays(dayCount) : MinDate);
			}
			return dateTime;
		}

		private void RequestBoldDatesRefresh()
		{
			_shouldRefreshBoldDates = true;
			Invalidate();
		}

		private void OnSchedulerDatesChanged(object sender, EventArgs eventArgs)
		{
			SyncSelectionFromScheduler();
		}

		private void OnSchedulerMinDateChanged(object sender, EventArgs eventArgs)
		{
			if (_selectionStyle == CalendarSelectionStyle.Schedule)
			{
				MinDate = _scheduler.MinDate;
			}
		}

		private void OnSchedulerMaxDateChanged(object sender, EventArgs eventArgs)
		{
			if (_selectionStyle == CalendarSelectionStyle.Schedule)
			{
				MaxDate = _scheduler.MaxDate;
			}
		}

		protected virtual void OnDatesChanged(EventArgs e)
		{
			if (base.Events[_datesChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
