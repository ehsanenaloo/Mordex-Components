using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	public partial class MxSchedulerPrintDocument
	{
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

		private void CopyDayFormatStyle(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, DateTime dateTime)
		{
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = _scheduler.FindDayCellByDate(dateTime);
			if (schedulerHourRangeCollectionLayoutStateArgument != null)
			{
				schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(schedulerHourRangeCollectionLayoutStateArgument.GetSchedulerFormatStyle());
			}
		}

		private void LayoutWeekView(Graphics graphics)
		{
			UpdateRowHeight(graphics);
			int currentTop = _marginBounds.Top;
			int availableHeight = _marginBounds.Height;
			if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
			{
				currentTop += _rowHeight;
				availableHeight -= _rowHeight;
			}
			int dayHeight = availableHeight / 3;
			int columnWidth = _marginBounds.Width / 2;
			int widthRemainder = _marginBounds.Width % 2;
			int heightRemainder = availableHeight % 3;
			widthRemainder++;
			heightRemainder++;
			Rectangle empty = Rectangle.Empty;
			int weekendDayHeight = dayHeight / 2;
			int weekendHeightRemainder = dayHeight % 2;
			int columnNumber = 1;
			int currentLeft = _marginBounds.Left;
			_gridBounds = _marginBounds;
			_dayLayouts = new SchedulerHourRangeLayoutCollection();
			_headerLayouts = new SchedulerOwnerLayoutCollection();
			int layoutPage = 1;
			foreach (SchedulerHourRangeCollectionLayoutState item in _scheduler._dayLayouts)
			{
				if (columnNumber == 2)
				{
					columnWidth += widthRemainder;
					weekendHeightRemainder += widthRemainder;
					columnNumber = 0;
				}
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item.GetDate());
				schedulerHourRangeCollectionLayoutState.SetColumnIndex(layoutPage);
				CopyDayFormatStyle(schedulerHourRangeCollectionLayoutState, item.GetDate());
				int dayCellHeight = dayHeight;
				if (schedulerHourRangeCollectionLayoutState.GetDate().DayOfWeek == DayOfWeek.Saturday)
				{
					empty = (schedulerHourRangeCollectionLayoutState._bounds = new Rectangle(currentLeft, currentTop, columnWidth, weekendDayHeight));
					dayCellHeight = weekendDayHeight;
				}
				else if (schedulerHourRangeCollectionLayoutState.GetDate().DayOfWeek != DayOfWeek.Sunday)
				{
					empty = (schedulerHourRangeCollectionLayoutState._bounds = new Rectangle(currentLeft, currentTop, columnWidth, dayHeight));
				}
				else
				{
					empty = (schedulerHourRangeCollectionLayoutState._bounds = new Rectangle(currentLeft, currentTop, columnWidth, weekendDayHeight + weekendHeightRemainder));
					dayCellHeight = weekendDayHeight + weekendHeightRemainder;
				}
				if (empty.Bottom + dayCellHeight > _marginBounds.Bottom)
				{
					schedulerHourRangeCollectionLayoutState._bounds.Height += heightRemainder;
					if (schedulerHourRangeCollectionLayoutState.GetDate().DayOfWeek != DayOfWeek.Saturday)
					{
						currentLeft += columnWidth;
						currentTop = _marginBounds.Top;
						if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
						{
							currentTop += _rowHeight;
						}
						columnNumber++;
						if (_layoutPages != LayoutPages.OnePage)
						{
							layoutPage++;
						}
					}
				}
				else
				{
					currentTop += dayCellHeight;
				}
				_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutState);
			}
			LayoutDateHeaders(graphics);
			LayoutMonthAppointments();
		}

		private void LayoutDateHeaders(Graphics graphics)
		{
			bool canUseLongDate = true;
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(PredefinedStyle.Header);
			schedulerFormatStyle.ApplyTheme();
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				string text = item.GetDate().ToLongDateString();
				Rectangle bounds = item._bounds;
				bounds.Height = _rowHeight;
				if (item.GetColumnIndex() == 2)
				{
					bounds.Width--;
				}
				SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(bounds, text);
				schedulerAppointmentOwnerLayoutState.SetColumnIndex(item.GetColumnIndex());
				schedulerAppointmentOwnerLayoutState.UpdateDate(item.GetDate());
				int textWidth = (int)graphics.MeasureString(text, GetFormatStyleFont(schedulerFormatStyle), 0).Width;
				if (textWidth >= schedulerAppointmentOwnerLayoutState._bounds.Width)
				{
					canUseLongDate = false;
				}
				_headerLayouts.Add(schedulerAppointmentOwnerLayoutState);
			}
			if (_scheduler.CustomDateFormat != "")
			{
				foreach (SchedulerAppointmentOwnerLayoutState customFormatHeader in _headerLayouts)
				{
					string text;
					try
					{
						text = customFormatHeader.GetDate().Date.ToString(_scheduler.CustomDateFormat, Application.CurrentCulture);
					}
					catch
					{
						text = "";
					}
					customFormatHeader.SetHeaderText(text);
				}
			}
			else if (!canUseLongDate)
			{
				foreach (SchedulerAppointmentOwnerLayoutState shortDateHeader in _headerLayouts)
				{
					string text = shortDateHeader.GetDate().Date.ToShortDateString();
					shortDateHeader.SetHeaderText(text);
				}
			}
			schedulerFormatStyle.DisposeResources();
		}

		private string GetMonthHeaderText()
		{
			DateTime dateTime = _scheduler.Dates[0];
			DateTime lastDate = _scheduler.Dates[_scheduler.Dates.Count - 1];
			if (lastDate.Month == dateTime.Month)
			{
				return dateTime.ToString("MMMM yyyy", Application.CurrentCulture);
			}
			switch (_monthViewHeaderNaming)
			{
			case MonthViewHeaderNaming.Range:
				if (lastDate.Month != dateTime.Month)
				{
					return dateTime.ToString("MMMM", Application.CurrentCulture) + "-" + lastDate.ToString("MMMM yyyy", Application.CurrentCulture);
				}
				return dateTime.ToString("MMMM yyyy", Application.CurrentCulture);
			case MonthViewHeaderNaming.MainMonth:
			{
				int count = _scheduler.Dates.Count;
				int daysInFirstMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month) - _scheduler.Dates[0].Day;
				if (count - daysInFirstMonth > daysInFirstMonth)
				{
					return lastDate.ToString("MMMM yyyy", Application.CurrentCulture);
				}
				return dateTime.ToString("MMMM yyyy", Application.CurrentCulture);
			}
			default:
			{
				lastDate = lastDate.AddDays(1.0);
				if (dateTime.Day == 1)
				{
					return dateTime.ToString("MMMM yyyy", Application.CurrentCulture);
				}
				DateTime nextMonthDate = dateTime.AddMonths(1);
				if (nextMonthDate.Month != lastDate.Month)
				{
					return nextMonthDate.ToString("MMMM yyyy", Application.CurrentCulture);
				}
				goto case MonthViewHeaderNaming.Range;
			}
			}
		}

		private void InitDayViewPrint(Graphics graphics)
		{
			_interval = _scheduler.Interval;
			UpdateTimeColumnWidth(graphics);
			_rowHeight = 0;
			int endMinutes = EndTime.Minutes;
			if (EndTime.TotalMinutes % 30.0 != 0.0)
			{
				if (endMinutes > 30)
				{
					endMinutes = 60 - EndTime.Minutes;
				}
				else if (endMinutes < 30)
				{
					endMinutes = 30 - EndTime.Minutes;
				}
				EndTime = EndTime.Add(new TimeSpan(0, endMinutes, 0));
			}
			_dayLayouts = new SchedulerHourRangeLayoutCollection();
		}

		private DateCollection BuildMonthViewDates(DateTime dateTimeArgument)
		{
			DateCollection dateCollection = new DateCollection(_scheduler);
			DateTime dateTime = dateTimeArgument;
			if (_scheduler.View == SchedulerView.MonthView)
			{
				int dayOfWeekIndex = _scheduler.GetDayOfWeekNumber(dateTimeArgument.DayOfWeek);
				int firstDayIndex;
				if (_isWeekendCompressed && _scheduler.FirstDayOfWeek == SchedulerDayOfWeek.Sunday)
				{
					_schedulerDayOfWeek = SchedulerDayOfWeek.Monday;
					dayOfWeekIndex = _scheduler.GetDayOfWeekNumber(dateTimeArgument.DayOfWeek);
					firstDayIndex = 2;
				}
				else
				{
					firstDayIndex = _scheduler.GetSchedulerWeekdayNumber(_scheduler.FirstDayOfWeek);
				}
				int daysToSubtract = 7;
				daysToSubtract = ((dayOfWeekIndex > firstDayIndex) ? (dayOfWeekIndex - firstDayIndex) : ((dayOfWeekIndex != firstDayIndex) ? (7 - (firstDayIndex - dayOfWeekIndex)) : 0));
				dateTime = dateTimeArgument.Subtract(new TimeSpan(daysToSubtract, 0, 0, 0));
			}
			int dayCount = 7;
			dayCount = _scheduler.Weeks * 7;
			dateTimeArgument = dateTime;
			dateTime = dateTimeArgument;
			for (int i = 0; i < dayCount; i++)
			{
				dateCollection.AddWithoutNotify(dateTime);
				dateTime = dateTime.AddDays(1.0);
			}
			return dateCollection;
		}

		private void LayoutMonthView(Graphics graphics)
		{
			UpdateRowHeight(graphics);
			_schedulerDayOfWeek = _scheduler.FirstDayOfWeek;
			int headerHeight = _rowHeight;
			int gridHeight = _marginBounds.Height - headerHeight;
			int currentLeft = _marginBounds.Left;
			int currentTop = _marginBounds.Top + headerHeight;
			if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
			{
				currentTop += _rowHeight;
				gridHeight -= _rowHeight;
			}
			int columnCount = 7;
			if (_isWeekendCompressed)
			{
				columnCount--;
			}
			int pageCount = 1;
			if (_layoutPages == LayoutPages.TwoPages)
			{
				pageCount = 2;
			}
			int widthRemainder = _marginBounds.Width / pageCount % (columnCount / pageCount);
			int heightRemainder = gridHeight % _scheduler.Weeks;
			heightRemainder++;
			int weekRowHeight = gridHeight / _scheduler.Weeks;
			int dayWidth = _marginBounds.Width / columnCount;
			Rectangle empty = Rectangle.Empty;
			int weekendRowHeight = weekRowHeight / 2;
			int weekendHeightRemainder = weekRowHeight % 2;
			int weekRowNumber = 1;
			string text = string.Empty;
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			bool isSaturday = false;
			bool isSunday = false;
			int columnNumber = 1;
			int headerCount = 0;
			_headerLayouts = new SchedulerOwnerLayoutCollection();
			_gridBounds = new Rectangle(_marginBounds.Left, _marginBounds.Top + _rowHeight, _marginBounds.Width, _marginBounds.Height - _rowHeight);
			_dayLayouts = new SchedulerHourRangeLayoutCollection();
			DateCollection dateCollection = ((_isWeekendCompressed == _scheduler.CompressWeekEnds) ? _scheduler.Dates : BuildMonthViewDates(_scheduler.Date));
			foreach (DateTime item in dateCollection)
			{
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item);
				CopyDayFormatStyle(schedulerHourRangeCollectionLayoutState, item);
				if (text == string.Empty || item.Day == 1)
				{
					text = currentInfo.GetMonthName(item.Month) + " " + item.Day;
					int captionWidth = (int)graphics.MeasureString(text, GetViewFont(), 0).Width;
					if (captionWidth >= dayWidth)
					{
						text = currentInfo.GetAbbreviatedMonthName(item.Month) + " " + item.Day;
					}
				}
				else
				{
					text = item.Day.ToString(Application.CurrentCulture);
				}
				int dayHeight = weekRowHeight;
				if (_isWeekendCompressed)
				{
					if (item.DayOfWeek == DayOfWeek.Saturday)
					{
						dayHeight = weekendRowHeight;
						isSaturday = true;
					}
					else if (item.DayOfWeek == DayOfWeek.Sunday)
					{
						dayHeight = weekendRowHeight + weekendHeightRemainder;
						isSunday = true;
					}
				}
				if (weekRowNumber == _scheduler.Weeks && !isSaturday)
				{
					dayHeight += heightRemainder;
				}
				empty = (schedulerHourRangeCollectionLayoutState._bounds = new Rectangle(currentLeft, currentTop, dayWidth, dayHeight));
				schedulerHourRangeCollectionLayoutState.UpdateCaption(text);
				schedulerHourRangeCollectionLayoutState.SetColumnIndex(columnNumber);
				if (empty.Right + dayWidth > _marginBounds.Right)
				{
					schedulerHourRangeCollectionLayoutState._bounds.Width += widthRemainder;
					if (isSunday)
					{
						currentTop += dayHeight;
						currentLeft = _marginBounds.Left;
						weekRowNumber++;
						columnNumber = 1;
					}
					else if (!isSaturday)
					{
						currentLeft = _marginBounds.Left;
						currentTop += dayHeight;
						weekRowNumber++;
						columnNumber = 1;
					}
					else
					{
						currentTop += dayHeight;
					}
				}
				else
				{
					currentLeft += dayWidth;
					columnNumber++;
					if (_isWeekendCompressed)
					{
						if (isSaturday)
						{
							currentTop += dayHeight;
							currentLeft -= dayWidth;
							columnNumber--;
						}
						else if (isSunday)
						{
							currentTop -= dayHeight - weekendHeightRemainder;
							if (weekRowNumber == _scheduler.Weeks)
							{
								currentTop += heightRemainder;
							}
						}
					}
				}
				if (pageCount > 1 && columnNumber == 4)
				{
					schedulerHourRangeCollectionLayoutState._bounds.Width += widthRemainder;
					currentLeft += widthRemainder;
				}
				if (headerCount < 7)
				{
					bool shouldAddHeader = false;
					DayOfWeek dayOfWeek = DayOfWeek.Monday;
					if (isSaturday)
					{
						shouldAddHeader = true;
						dayOfWeek = item.DayOfWeek;
					}
					else if (!isSunday)
					{
						shouldAddHeader = true;
						dayOfWeek = item.DayOfWeek;
					}
					if (shouldAddHeader)
					{
						int headerTop = _marginBounds.Top;
						if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
						{
							headerTop += _rowHeight;
						}
						SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left, headerTop, schedulerHourRangeCollectionLayoutState._bounds.Width, _rowHeight), "");
						schedulerAppointmentOwnerLayoutState.SetDayOfWeek(dayOfWeek);
						schedulerAppointmentOwnerLayoutState.SetColumnIndex(schedulerHourRangeCollectionLayoutState.GetColumnIndex());
						if (headerCount == 6)
						{
							schedulerAppointmentOwnerLayoutState._bounds.Width--;
						}
						_headerLayouts.Add(schedulerAppointmentOwnerLayoutState);
					}
					headerCount++;
				}
				isSaturday = false;
				isSunday = false;
				_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutState);
			}
			SetDayNameHeaderTexts(graphics);
			BuildMonthAppointmentViews();
		}

		private void SetDayNameHeaderTexts(Graphics graphics)
		{
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			bool canUseFullDayNames = true;
			foreach (SchedulerAppointmentOwnerLayoutState item in _headerLayouts)
			{
				string text = ((!_isWeekendCompressed || item.GetDayOfWeek() != DayOfWeek.Saturday) ? currentInfo.GetDayName(item.GetDayOfWeek()) : (currentInfo.GetDayName(item.GetDayOfWeek()) + "/" + currentInfo.GetDayName(DayOfWeek.Sunday)));
				int textWidth = (int)graphics.MeasureString(text, GetViewFont(), 0).Width;
				if (textWidth >= item._bounds.Width)
				{
					canUseFullDayNames = false;
					break;
				}
				item.SetHeaderText(text);
			}
			if (canUseFullDayNames)
			{
				return;
			}
			foreach (SchedulerAppointmentOwnerLayoutState shortNameHeader in _headerLayouts)
			{
				string text = ((!_isWeekendCompressed || shortNameHeader.GetDayOfWeek() != DayOfWeek.Saturday) ? currentInfo.GetAbbreviatedDayName(shortNameHeader.GetDayOfWeek()) : (currentInfo.GetAbbreviatedDayName(shortNameHeader.GetDayOfWeek()) + "/" + currentInfo.GetAbbreviatedDayName(DayOfWeek.Sunday)));
				shortNameHeader.SetHeaderText(text);
			}
		}

		private int FindFreeAllDayTop(int x, int integerArgument)
		{
			foreach (SchedulerAppointmentView item in _allDayRenderers)
			{
				if (item._bounds.Contains(x, integerArgument))
				{
					integerArgument += _rowHeight;
					return FindFreeAllDayTop(x, integerArgument);
				}
			}
			return integerArgument;
		}

		internal SchedulerAppointmentView CreateAllDayRenderer(SchedulerAppointment appointment)
		{
			SchedulerAppointmentView appointmentRenderer = null;
			appointmentRenderer = new SchedulerAppointmentView(appointment, _rowHeight, (int)_interval, allDay: true, SchedulerView.MonthView, appointment.StartTime, _scheduler, useCompatibleTextRendering: true);
			appointmentRenderer._appointmentFont = PrintDayFont;
			if (appointment.Selected)
			{
				appointmentRenderer.UpdateLayout(AppointmentState.Focus);
			}
			else
			{
				appointmentRenderer.UpdateLayout(AppointmentState.Normal);
			}
			appointmentRenderer._isShown = false;
			appointmentRenderer._bounds = Rectangle.Empty;
			appointmentRenderer._isMultiDay = true;
			appointmentRenderer._showTimeAsClocks = true;
			_allDayRenderers.Add(appointmentRenderer);
			return appointmentRenderer;
		}
	}
}
