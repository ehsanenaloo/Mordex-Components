using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		void ISchedulerWorkingHoursOwner.InvalidateHour()
		{
			_isHourRangeCacheStale = true;
			if (_view == SchedulerView.DayView)
			{
				InvalidateAppointmentLayout();
			}
		}

		private void SetAllDayViewBounds(SchedulerAppointmentView appointmentView, int width, bool startsInRange, bool booleanArgument, int left)
		{
			int top = _actualRowHeight + base.ClientRectangle.Top + 1;
			if (IsMultiOwnerActive() && _showOwnersHeader)
			{
				top += _actualRowHeight;
			}
			top -= _allDayScrollOffset * _actualRowHeight;
			_lastAllDayRowTop = FindFreeAllDayRowTop(left, top);
			appointmentView._bounds = new Rectangle(left, _lastAllDayRowTop, width, _actualRowHeight);
			if (_appointmentEditor != null && _appointmentEditor._app == appointmentView.GetAppointment())
			{
				using (Graphics graphics = CreateGraphics())
				{
					appointmentView.RenderContent(graphics, _appointmentEditor);
				}
			}
			appointmentView.UpdateFirstSegment(startsInRange);
			appointmentView.UpdateLastSegment(booleanArgument);
			if (!startsInRange)
			{
				appointmentView.SetContinuedFromPrevious(true);
			}
			if (!booleanArgument)
			{
				appointmentView.SetContinuedToNext(true);
			}
			if (appointmentView._bounds.Right >= _dayAreaLeft)
			{
				appointmentView._isShown = true;
			}
		}

		internal int GetDayOfWeekNumber(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case DayOfWeek.Monday:
				return 2;
			case DayOfWeek.Tuesday:
				return 3;
			case DayOfWeek.Wednesday:
				return 4;
			case DayOfWeek.Thursday:
				return 5;
			case DayOfWeek.Friday:
				return 6;
			case DayOfWeek.Saturday:
				return 7;
			default:
				return 1;
			}
		}

		internal int GetSchedulerWeekdayNumber(SchedulerDayOfWeek schedulerDayOfWeek)
		{
			switch (schedulerDayOfWeek)
			{
			case SchedulerDayOfWeek.Monday:
				return 2;
			case SchedulerDayOfWeek.Tuesday:
				return 3;
			case SchedulerDayOfWeek.Wednesday:
				return 4;
			case SchedulerDayOfWeek.Thursday:
				return 5;
			case SchedulerDayOfWeek.Friday:
				return 6;
			case SchedulerDayOfWeek.Saturday:
				return 7;
			default:
				return 1;
			}
		}

		private bool LayoutAllDayArea(Graphics graphics)
		{
			if (_allDayAreaControl != null)
			{
				_allDayAreaControl._hasHiddenAppointments = false;
			}
			foreach (SchedulerAppointmentView item in _allDayAppointmentViews)
			{
				item._isShown = false;
				item._bounds = Rectangle.Empty;
				item.StopTimerAndHideToolTip();
			}
			if (!_showAllDayArea)
			{
				_backgroundAppointments = null;
				if (_allDayAppointments == null)
				{
					_allDayAppointments = new ArrayList();
				}
				else
				{
					_allDayAppointments.Clear();
				}
				return false;
			}
			if (_isAppointmentCacheStale)
			{
				DateRange dateRange = new DateRange(_dayLayouts.GetLayoutAt(0).GetDate(), _dayLayouts.GetLayoutAt(_dayLayouts.Count - 1).GetDate());
				if (!IsMultiOwnerActive() && CurrentOwner != null)
				{
					_allDayAppointments = _publicAppointments.GetAppointmentsForOwner(dateRange, false, _currentOwner);
				}
				else
				{
					_allDayAppointments = _publicAppointments.GetVisibleAppointmentsInRange(dateRange, false);
				}
			}
			else
			{
				SchedulerComparer comparer = new SchedulerComparer();
				_allDayAppointments.Sort(comparer);
			}
			foreach (SchedulerAppointment appointment in _allDayAppointments)
			{
				if (appointment.GetDurationInHours() < 24)
				{
					continue;
				}
				SchedulerAppointmentView appointmentView = GetAllDayAppointmentView(appointment);
				if (appointmentView != null)
				{
					LayoutAllDayAppointment(appointmentView);
					if (appointmentView._bounds.Left + 1 < _dayAreaLeft - _allDayRowGap || appointmentView._bounds.Left + 1 > base.ClientRectangle.Right)
					{
						appointmentView._bounds = Rectangle.Empty;
					}
					if (_allDayAreaControl != null)
					{
						_allDayAreaControl.Invalidate();
					}
				}
			}
			if (_allDayAreaControl != null && ShouldShowAllDayAreaScrollBar())
			{
				_allDayAreaControl._isLayoutStale = true;
				_allDayAreaControl.Invalidate();
			}
			RemoveUnusedAllDayRenderers();
			_usedAllDayViewCount = 0;
			int requiredHeight = CalculateAllDayAreaHeight() + 2;
			if (_dayAreaTop < requiredHeight)
			{
				_dayAreaTop = requiredHeight;
				if (_allDayAreaControl != null)
				{
					_allDayAreaControl.Height = _dayAreaTop;
					RenderDayView(graphics);
					return true;
				}
			}
			return false;
		}

		private void SetWorkWeekDates(DateTime dateTimeArgument)
		{
			int firstDayIndex = GetSchedulerWeekdayNumber(FirstDayOfWeek);
			firstDayIndex--;
			int dayOfWeek = (int)dateTimeArgument.DayOfWeek;
			int daysSinceWeekStart = 6;
			daysSinceWeekStart = ((dayOfWeek > firstDayIndex) ? (dayOfWeek - firstDayIndex) : ((dayOfWeek != firstDayIndex) ? (7 - (firstDayIndex - dayOfWeek)) : 0));
			if (dateTimeArgument.Date.AddDays(-daysSinceWeekStart) < GetWeekStart(_minDate))
			{
				Date = dateTimeArgument.AddDays(1.0);
				return;
			}
			dateTimeArgument = dateTimeArgument.Date.AddDays(-daysSinceWeekStart);
			DateTime dateTime = dateTimeArgument.AddDays(7.0);
			_dates.ClearWithoutNotify();
			while (dateTimeArgument < dateTime)
			{
				if ((SchedulerRenderer.ToSchedulerDayOfWeek(dateTimeArgument.DayOfWeek) & _workWeek) != 0 && !_dates.Contains(dateTimeArgument.Date))
				{
					_dates.AddWithoutNotify(dateTimeArgument);
				}
				dateTimeArgument = dateTimeArgument.AddDays(1.0);
			}
			if (_dates[_dates.Count - 1].Date > GetWeekEnd(_maxDate) && _dates[0].AddDays(-7.0) > GetWeekStart(Date))
			{
				Date = Date.AddDays(-1.0);
				return;
			}
			_date = _dates[0];
			_isHourRangeCacheStale = true;
			OnDatesChanged(EventArgs.Empty);
		}

		private void LayoutCompressedWeekendSpan(SchedulerAppointmentView appointmentView, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			int nextCellIndex = schedulerHourRangeCollectionLayoutState.GetDayIndex();
			int width = schedulerHourRangeCollectionLayoutState._bounds.Width;
			DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate();
			int lastColumnIndex = 6;
			if (_firstDayOfWeek == SchedulerDayOfWeek.Monday)
			{
				lastColumnIndex++;
			}
			bool isFirstCell = true;
			if (appointmentView.GetAppointment().StartTime.Date.AddDays(1.0) < appointmentView.GetAppointment().EndTime)
			{
				int columnIndex = schedulerHourRangeCollectionLayoutState.GetColumnIndex();
				do
				{
					if (columnIndex > lastColumnIndex)
					{
						appointmentView.UpdateLastSegment(false);
						if (nextCellIndex >= _dayLayouts.Count)
						{
							break;
						}
						SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = _dayLayouts.GetLayoutAt(nextCellIndex);
						int left = schedulerHourRangeCollectionLayoutStateArgument._bounds.Left;
						int nextTop = schedulerHourRangeCollectionLayoutStateArgument._bounds.Top + _actualRowHeight + 4;
						SchedulerAppointmentView schedulerRenderer4Argument = GetMonthAppointmentView(left, nextTop, schedulerHourRangeCollectionLayoutStateArgument, appointmentView.GetAppointment(), MonthFont, false);
						if (schedulerRenderer4Argument != null)
						{
							Rectangle bounds = _dayLayouts.GetLayoutAt(nextCellIndex)._bounds;
							schedulerRenderer4Argument._bounds = new Rectangle(bounds.Left + 4, schedulerRenderer4Argument._rowTop, 0, _actualRowHeight);
							schedulerRenderer4Argument.UpdateFirstSegment(false);
							LayoutCompressedWeekendSpan(schedulerRenderer4Argument, _dayLayouts.GetLayoutAt(nextCellIndex));
						}
						break;
					}
					if (nextCellIndex >= _dayLayouts.Count)
					{
						break;
					}
					if (dateTime.DayOfWeek == DayOfWeek.Sunday || dateTime.DayOfWeek == DayOfWeek.Saturday)
					{
						SchedulerHourRangeCollectionLayoutState nextCell = _dayLayouts.GetLayoutAt(nextCellIndex);
						if (dateTime.DayOfWeek == DayOfWeek.Saturday)
						{
							if (appointmentView._rowTop + _actualRowHeight <= nextCell._bounds.Bottom)
							{
								appointmentView._bounds.Width += width;
							}
							else
							{
								int saturdayLeft = nextCell._bounds.Left;
								int saturdayTop = nextCell._bounds.Top + _actualRowHeight + 4;
								SchedulerAppointmentView saturdayView = ((!isFirstCell) ? GetMonthAppointmentView(saturdayLeft, saturdayTop, nextCell, appointmentView.GetAppointment(), MonthFont, true) : appointmentView);
								if (saturdayView != null)
								{
									saturdayView._bounds = new Rectangle(nextCell._bounds.Left + 4, saturdayView._rowTop, width - 8, _actualRowHeight);
									saturdayView.UpdateFirstSegment(false);
									appointmentView.UpdateLastSegment(false);
									if (nextCell.GetDate().AddDays(1.0) < appointmentView.GetAppointment().EndTime)
									{
										saturdayView.UpdateLastSegment(false);
									}
									if (nextCell.GetDate() == appointmentView.GetAppointment().StartTime.Date)
									{
										saturdayView.UpdateFirstSegment(true);
									}
									if (_appointmentEditor != null && _appointmentEditor._app == saturdayView.GetAppointment())
									{
										using (Graphics graphics = CreateGraphics())
										{
											saturdayView.RenderContent(graphics, _appointmentEditor);
										}
									}
								}
								else
								{
									appointmentView.UpdateLastSegment(false);
									appointmentView.SetContinuedToNext(true);
								}
							}
						}
						else
						{
							int sundayLeft = nextCell._bounds.Left;
							int sundayTop = nextCell._bounds.Top + _actualRowHeight + 4;
							SchedulerAppointmentView sundayView = ((!isFirstCell) ? GetMonthAppointmentView(sundayLeft, sundayTop, nextCell, appointmentView.GetAppointment(), MonthFont, true) : appointmentView);
							if (sundayView != null)
							{
								sundayView._bounds = new Rectangle(nextCell._bounds.Left + 4, sundayView._rowTop, width - 8, _actualRowHeight);
								sundayView.UpdateFirstSegment(false);
								appointmentView.UpdateLastSegment(false);
								if (nextCell.GetDate().AddDays(1.0) < appointmentView.GetAppointment().EndTime)
								{
									sundayView.UpdateLastSegment(false);
								}
								if (nextCell.GetDate() == appointmentView.GetAppointment().StartTime.Date)
								{
									sundayView.UpdateFirstSegment(true);
								}
								if (_appointmentEditor != null && _appointmentEditor._app == sundayView.GetAppointment())
								{
									using (Graphics sundayGraphics = CreateGraphics())
									{
										sundayView.RenderContent(sundayGraphics, _appointmentEditor);
									}
								}
							}
							else
							{
								appointmentView.UpdateLastSegment(false);
								appointmentView.SetContinuedToNext(true);
							}
						}
					}
					else if (dateTime.DayOfWeek == DayOfWeek.Monday && !isFirstCell)
					{
						SchedulerHourRangeCollectionLayoutState mondayCell = _dayLayouts.GetLayoutAt(nextCellIndex);
						int mondayLeft = mondayCell._bounds.Left;
						int mondayTop = mondayCell._bounds.Top + _actualRowHeight + 4;
						SchedulerAppointmentView mondayView = GetMonthAppointmentView(mondayLeft, mondayTop, mondayCell, appointmentView.GetAppointment(), MonthFont, true);
						if (mondayView != null)
						{
							mondayView._bounds = new Rectangle(mondayCell._bounds.Left + 4, mondayView._rowTop, 0, _actualRowHeight);
							mondayView.UpdateFirstSegment(false);
							appointmentView.UpdateLastSegment(false);
							appointmentView.SetContinuedToNext(false);
							LayoutCompressedWeekendSpan(mondayView, mondayCell);
							break;
						}
						appointmentView.UpdateLastSegment(false);
						appointmentView.SetContinuedToNext(true);
					}
					else
					{
						appointmentView._bounds.Width += width;
					}
					isFirstCell = false;
					columnIndex++;
					nextCellIndex++;
					dateTime = dateTime.AddDays(1.0);
				}
				while (dateTime < appointmentView.GetAppointment().EndTime);
			}
			else
			{
				appointmentView._bounds.Width += width;
			}
			appointmentView._bounds.Width -= 8;
		}

		private bool HasDateMissingFromView(ArrayList arrayList)
		{
			if (_schedulerSchedulerAppointment._startTime == _dragOriginalStart)
			{
				foreach (DateTime item in arrayList)
				{
					if (!_dates.Contains(item))
					{
						return true;
					}
				}
			}
			return false;
		}

		private DateTime GetDateTimeAtPoint(Point pt)
		{
			Rectangle empty = Rectangle.Empty;
			try
			{
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					if (_view == SchedulerView.DayView)
					{
						empty = item._bounds;
						empty.Width += empty.Left;
						empty.X = 0;
						if (_allDayAreaControl.Bounds.Contains(pt) && empty.Contains(pt.X, item._bounds.Top))
						{
							return item.GetDate().Date;
						}
						if (!empty.Contains(pt.X, item._bounds.Top))
						{
							continue;
						}
						foreach (SchedulerAppointmentOwnerLayoutState timeSlot in item.GetTimeSlots())
						{
							empty = timeSlot._bounds;
							empty.Width += empty.Left;
							empty.X = 0;
							if (empty.Contains(pt.X, pt.Y))
							{
								return item.GetDate().Date.Add(timeSlot.GetDate().TimeOfDay);
							}
						}
					}
					else if (item._bounds.Contains(pt.X, pt.Y))
					{
						return item.GetDate().Date;
					}
				}
				return DateTime.MinValue;
			}
			catch
			{
				return DateTime.MinValue;
			}
		}

		private SchedulerHourRangeCollectionLayoutState GetDayCellAtPoint(Point point)
		{
			_ = Rectangle.Empty;
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item._bounds.Contains(point.X, point.Y))
				{
					return item;
				}
			}
			return null;
		}

		private TimeSpan ClampToVisibleHours(TimeSpan timeSpan)
		{
			if ((int)timeSpan.TotalHours < DayStartHour)
			{
				return new TimeSpan(DayStartHour, 0, 0);
			}
			if ((int)timeSpan.TotalHours > DayEndHour)
			{
				return new TimeSpan(DayEndHour - 1, 60 - _minuteInterval, 0);
			}
			return timeSpan;
		}

		private void InvalidateDayCell(DateTime dateTime)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item.GetDate() == dateTime)
				{
					Invalidate(item._bounds);
				}
			}
		}

		internal SchedulerHourRangeCollectionLayoutState FindDayCellByDate(DateTime dateTime)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item.GetDate() == dateTime.Date)
				{
					return item;
				}
			}
			return null;
		}

		private int CalculateAllDayAreaHeight()
		{
			if (!_showAllDayArea)
			{
				if (IsMultiOwnerActive() && _showOwnersHeader)
				{
					return _actualRowHeight * 2;
				}
				return _actualRowHeight;
			}
			int allDayBottom = _actualRowHeight * 2 + 2;
			int collapsedBottom = _actualRowHeight * 2 + 2;
			if (IsMultiOwnerActive() && _showOwnersHeader)
			{
				allDayBottom += _actualRowHeight;
				collapsedBottom += _actualRowHeight;
			}
			collapsedBottom -= _allDayScrollOffset * (_actualRowHeight + _allDayRowGap);
			int collapsedRowCount = 0;
			int shownRowCount = 0;
			foreach (SchedulerAppointmentView item in _allDayAppointmentViews)
			{
				if (item._bounds.Bottom > collapsedBottom || (item._bounds.Bottom == collapsedBottom && collapsedRowCount == 0))
				{
					collapsedRowCount++;
					collapsedBottom = item._bounds.Bottom;
				}
				if (item._bounds.Bottom > allDayBottom || (item._bounds.Bottom == allDayBottom && shownRowCount == 0))
				{
					if (_maxAllDayRows > 0 && shownRowCount >= _maxAllDayRows)
					{
						item._isShown = false;
						continue;
					}
					item._isShown = true;
					allDayBottom = item._bounds.Bottom;
					shownRowCount++;
				}
			}
			_allDayRowCount = collapsedRowCount;
			return allDayBottom;
		}


		private DateTime GetWeekEnd(DateTime dateTime)
		{
			if (_view != SchedulerView.DayView)
			{
				return GetWeekStart(dateTime).AddDays(6.0);
			}
			return dateTime;
		}

		private DateTime GetWeekStart(DateTime dateTime)
		{
			if (_view != SchedulerView.DayView)
			{
				int dayOfWeekIndex = GetDayOfWeekNumber(dateTime.DayOfWeek);
				int firstDayIndex;
				if ((_compressWeekEnds || _view == SchedulerView.WeekView) && _view != SchedulerView.DayView && _schedulerFirstDayOfWeek == SchedulerDayOfWeek.Sunday)
				{
					_firstDayOfWeek = SchedulerDayOfWeek.Monday;
					dayOfWeekIndex = GetDayOfWeekNumber(dateTime.DayOfWeek);
					firstDayIndex = 2;
				}
				else
				{
					firstDayIndex = GetSchedulerWeekdayNumber(_schedulerFirstDayOfWeek);
				}
				int daysFromWeekStart = 7;
				daysFromWeekStart = ((dayOfWeekIndex > firstDayIndex) ? (dayOfWeekIndex - firstDayIndex) : ((dayOfWeekIndex != firstDayIndex) ? (7 - (firstDayIndex - dayOfWeekIndex)) : 0));
				return dateTime.Subtract(new TimeSpan(daysFromWeekStart, 0, 0, 0));
			}
			if (dateTime.Date < _minDate.Date)
			{
				return _minDate;
			}
			return dateTime;
		}

		private void RebuildDates()
		{
			if (_isWorkWeekView)
			{
				SetWorkWeekDates(_date);
				return;
			}
			int dayCount = 7;
			DateRange dateRange = DateRange;
			_dates.ClearWithoutNotify();
			if (_view == SchedulerView.MonthView)
			{
				dayCount = _weeks * 7;
			}
			_date = GetWeekStart(_date);
			DateTime dateTime = _date;
			if (_date.AddDays(dayCount - 1) > GetWeekEnd(_maxDate) && _date.AddDays(-7.0) >= GetWeekStart(_date))
			{
				Date = _date.AddDays(-7.0);
				return;
			}
			if (_date < GetWeekStart(_minDate))
			{
				Date = GetWeekStart(_minDate);
				return;
			}
			for (int i = 0; i < dayCount; i++)
			{
				Dates.AddWithoutNotify(dateTime);
				dateTime = dateTime.AddDays(1.0);
			}
			DateRange newDateRange = DateRange;
			EnsureSelectionInRange();
			if (newDateRange.End != dateRange.End || newDateRange.Start != dateRange.Start)
			{
				OnDatesChanged(EventArgs.Empty);
				_isHourRangeCacheStale = true;
				_isAppointmentCacheStale = true;
			}
		}

		private SchedulerHourRangeCollectionLayoutState FindDateStyleOverride(DateTime dateTime, int lowIndex, int integerArgument)
		{
			if (lowIndex > integerArgument)
			{
				return null;
			}
			int middleIndex = (integerArgument + lowIndex) / 2;
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = _dateStyleOverrides.GetLayoutAt(middleIndex);
			int comparison = schedulerHourRangeCollectionLayoutState.GetDate().CompareTo(dateTime);
			if (comparison == 0)
			{
				return schedulerHourRangeCollectionLayoutState;
			}
			if (comparison > 0)
			{
				return FindDateStyleOverride(dateTime, lowIndex, middleIndex - 1);
			}
			return FindDateStyleOverride(dateTime, middleIndex + 1, integerArgument);
		}

		public void SetDateBackColor(DateTime date, Color backColor)
		{
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(date.Date, 0, _dateStyleOverrides.Count - 1);
			if (schedulerHourRangeCollectionLayoutState == null)
			{
				schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(date.Date);
				schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(new SchedulerFormatStyle(this, PredefinedStyle.None));
				schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle().BackColor = backColor;
				_dateStyleOverrides.InsertLayoutSorted(schedulerHourRangeCollectionLayoutState, 0, _dateStyleOverrides.Count - 1, _dateStyleOverrides.Count);
				InvalidateAppointmentLayout();
			}
			else
			{
				if (schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle() == null)
				{
					schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(new SchedulerFormatStyle(this, PredefinedStyle.None));
				}
				schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle().BackColor = backColor;
				Invalidate();
			}
		}

		public void SetDateForeColor(DateTime date, Color foreColor)
		{
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(date.Date, 0, _dateStyleOverrides.Count - 1);
			if (schedulerHourRangeCollectionLayoutState == null)
			{
				schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(date.Date);
				schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(new SchedulerFormatStyle(this, PredefinedStyle.None));
				schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle().ForeColor = foreColor;
				_dateStyleOverrides.InsertLayoutSorted(schedulerHourRangeCollectionLayoutState, 0, _dateStyleOverrides.Count - 1, _dateStyleOverrides.Count);
				InvalidateAppointmentLayout();
			}
			else
			{
				if (schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle() == null)
				{
					schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(new SchedulerFormatStyle(this, PredefinedStyle.None));
				}
				schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle().ForeColor = foreColor;
				Invalidate();
			}
		}

		public void SetDateFormatStyle(DateTime date, SchedulerFormatStyle style)
		{
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(date.Date, 0, _dateStyleOverrides.Count - 1);
			style._owner = this;
			if (schedulerHourRangeCollectionLayoutState == null)
			{
				schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(date.Date);
				schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(style);
				_dateStyleOverrides.InsertLayoutSorted(schedulerHourRangeCollectionLayoutState, 0, _dateStyleOverrides.Count - 1, _dateStyleOverrides.Count);
				InvalidateAppointmentLayout();
			}
			else
			{
				schedulerHourRangeCollectionLayoutState.SetSchedulerFormatStyle(style);
				Invalidate();
			}
		}

		private void SetDayNameHeaderTexts(Graphics graphics)
		{
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			bool fitsFullNames = true;
			foreach (SchedulerAppointmentOwnerLayoutState item in _headerLayouts)
			{
				string text = ((!_compressWeekEnds || item.GetDayOfWeek() != DayOfWeek.Saturday) ? currentInfo.GetDayName(item.GetDayOfWeek()) : (currentInfo.GetDayName(item.GetDayOfWeek()) + "/" + currentInfo.GetDayName(DayOfWeek.Sunday)));
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				int nameWidth = (int)textLayoutRenderer.RenderContent(graphics, text, MonthFont, 0).Width;
				textLayoutRenderer.ResetFormat();
				if (nameWidth >= item._bounds.Width)
				{
					fitsFullNames = false;
					break;
				}
				item.SetHeaderText(text);
			}
			if (fitsFullNames)
			{
				return;
			}
			foreach (SchedulerAppointmentOwnerLayoutState abbreviatedHeader in _headerLayouts)
			{
				string text = ((!_compressWeekEnds || abbreviatedHeader.GetDayOfWeek() != DayOfWeek.Saturday) ? currentInfo.GetAbbreviatedDayName(abbreviatedHeader.GetDayOfWeek()) : (currentInfo.GetAbbreviatedDayName(abbreviatedHeader.GetDayOfWeek()) + "/" + currentInfo.GetAbbreviatedDayName(DayOfWeek.Sunday)));
				abbreviatedHeader.SetHeaderText(text);
			}
		}

		private void CreateWeekDayHeaders(Graphics graphics)
		{
			bool fitsLongDate = true;
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				string text = item.GetDate().ToLongDateString();
				Rectangle bounds = item._bounds;
				bounds.Height = _actualRowHeight;
				if (bounds.Right > GetClientRight())
				{
					bounds.Width--;
				}
				SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(bounds, text);
				schedulerAppointmentOwnerLayoutState.UpdateDate(item.GetDate());
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				int textWidth = (int)textLayoutRenderer.RenderContent(graphics, text, GetViewFont(schedulerFormatStyle), 0).Width;
				textLayoutRenderer.ResetFormat();
				if (textWidth >= schedulerAppointmentOwnerLayoutState._bounds.Width)
				{
					fitsLongDate = false;
				}
				schedulerAppointmentOwnerLayoutState.UpdateDate(item.GetDate());
				_headerLayouts.Add(schedulerAppointmentOwnerLayoutState);
			}
			if (_customDateFormat != "")
			{
				foreach (SchedulerAppointmentOwnerLayoutState customFormatHeader in _headerLayouts)
				{
					string text = customFormatHeader.GetDate().Date.ToString(_customDateFormat, Application.CurrentCulture);
					customFormatHeader.SetHeaderText(text);
				}
			}
			else if (!fitsLongDate)
			{
				foreach (SchedulerAppointmentOwnerLayoutState shortDateHeader in _headerLayouts)
				{
					string text = shortDateHeader.GetDate().Date.ToShortDateString();
					shortDateHeader.SetHeaderText(text);
				}
			}
			schedulerFormatStyle.DisposeResources();
		}

		internal SchedulerHourRangeCollection GetHourRangesForOwner(DateRange dateRange, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			if (schedulerAppointmentOwner != null)
			{
				return WorkingHourSchema.GetCombinedHourRanges(dateRange, schedulerAppointmentOwner.WorkingHourSchema);
			}
			return WorkingHourSchema.GetHourRangesInRange(dateRange);
		}

		private SchedulerHourRangeCollection FindCachedHourRanges(ArrayList arrayList, DateTime dateTime, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in arrayList)
			{
				if (item.GetDate() == dateTime && item.GetOwner() == schedulerAppointmentOwner)
				{
					return item.GetSchedulerHourRangeCollection();
				}
			}
			return new SchedulerHourRangeCollection(this);
		}

		internal string GetStartTimeMemberName()
		{
			if (_usesInternalTable)
			{
				return "___StartTimeMember";
			}
			return _startTimeField;
		}

		internal string GetEndTimeMemberName()
		{
			if (_usesInternalTable)
			{
				return "___EndTimeMember";
			}
			return _endTimeField;
		}

		private void CreateTimeNavigatorFont()
		{
			FontStyle fontStyle = FontStyle.Regular;
			string name = DayFont.FontFamily.Name;
			if (_timeNavigatorFormatStyle.FontUnderline == TriState.True)
			{
				fontStyle |= FontStyle.Underline;
			}
			if (_timeNavigatorFormatStyle.FontBold == TriState.True)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (_timeNavigatorFormatStyle.FontItalic == TriState.True)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (_timeNavigatorFormatStyle.FontStrikeout == TriState.True)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			FontFamily fontFamily = new FontFamily(name);
			if (!fontFamily.IsStyleAvailable(fontStyle))
			{
				if (!fontFamily.IsStyleAvailable(FontStyle.Bold))
				{
					if (fontFamily.IsStyleAvailable(FontStyle.Italic))
					{
						fontStyle = FontStyle.Italic;
					}
					else if (fontFamily.IsStyleAvailable(FontStyle.Strikeout))
					{
						fontStyle = FontStyle.Strikeout;
					}
					else
					{
						if (!fontFamily.IsStyleAvailable(FontStyle.Underline))
						{
							if (_baseFont != null)
							{
								_baseFont.Dispose();
								_baseFont = null;
							}
							_baseFont = (Font)DayFont.Clone();
							return;
						}
						fontStyle = FontStyle.Underline;
					}
				}
				else
				{
					fontStyle = FontStyle.Bold;
				}
			}
			else
			{
				fontStyle = FontStyle.Regular;
			}
			if (_baseFont != null)
			{
				_baseFont.Dispose();
				_baseFont = null;
			}
			if (_interval != Interval.SixtyMinutes)
			{
				int scaledFontSize = (int)DayFont.SizeInPoints * 2;
				_baseFont = new Font(name, scaledFontSize, fontStyle);
			}
			else
			{
				_baseFont = new Font(name, DayFont.SizeInPoints, fontStyle);
			}
		}

		public DateTime GetDateAt()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return GetDateAt(position.X, position.Y);
		}

		public DateTime GetDateAt(Point pt)
		{
			return GetDateAt(pt.X, pt.Y);
		}

		public DateTime GetDateTimeAt()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return GetDateTimeAt(position.X, position.Y);
		}

		public DateTime GetDateTimeAt(Point pt)
		{
			return GetDateTimeAt(pt.X, pt.Y);
		}

		public TimeSpan GetTimeAt(Point pt)
		{
			return GetTimeAt(pt.X, pt.Y);
		}

		public TimeSpan GetTimeAt()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return GetTimeAt(position.X, position.Y);
		}

		public TimeSpan GetTimeAt(int x, int y)
		{
			return FindDaySlotLayoutAt(x, y)?.GetDate().TimeOfDay ?? TimeSpan.Zero;
		}

		public DateTime GetDateAt(int x, int y)
		{
			if (_view == SchedulerView.DayView)
			{
				Rectangle empty = Rectangle.Empty;
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					empty = item._bounds;
					empty.Width += empty.Left;
					empty.X = 0;
					if (empty.Contains(x, item._bounds.Top))
					{
						return item.GetDate().Date;
					}
				}
			}
			else
			{
				foreach (SchedulerHourRangeCollectionLayoutState dayLayout in _dayLayouts)
				{
					if (dayLayout._bounds.Contains(x, y))
					{
						return dayLayout.GetDate().Date;
					}
				}
			}
			if (Dates.Count > 0)
			{
				return _dates[0].Date;
			}
			return DateTime.Today.Date;
		}

		public DateTime GetDateTimeAt(int x, int y)
		{
			DateTime dateAt = GetDateAt(x, y);
			if (_view == SchedulerView.DayView)
			{
				TimeSpan timeAt = GetTimeAt(x, y);
				return dateAt.Add(timeAt);
			}
			return dateAt;
		}

		private int GetDayStartSlotIndex()
		{
			return DayStartHour * 60 / _minuteInterval;
		}

		internal void ExtendVisibleTimeToDayStart(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (_hiddenAppointmentMode == HiddenAppointmentMode.Manual)
			{
				OnHiddenAppointmentClick(new HiddenAppointmentsEventArgs(schedulerHourRangeCollectionLayoutState.GetDate(), View));
			}
			else if (_view == SchedulerView.DayView)
			{
				TimeSpan timeSpan = FirstVisibleTime;
				foreach (SchedulerAppointmentView item in _appointmentViews)
				{
					if (item.GetAppointment().StartTime.TimeOfDay < timeSpan && item.GetAppointment().Owner == schedulerHourRangeCollectionLayoutState.GetOwner() && item.GetAppointment().StartTime.Date <= schedulerHourRangeCollectionLayoutState.GetDate() && item.GetAppointment().EndTime > schedulerHourRangeCollectionLayoutState.GetDate())
					{
						if (item.GetAppointment().StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate())
						{
							timeSpan = new TimeSpan(0, 0, 0, 0);
							break;
						}
						timeSpan = item.GetAppointment().StartTime.TimeOfDay;
					}
				}
				FirstVisibleTime = timeSpan;
			}
			else
			{
				ShowDayViewForRange(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetDate().AddDays(1.0));
			}
		}

		internal void ExtendVisibleTimeToDayEnd(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (_hiddenAppointmentMode == HiddenAppointmentMode.Manual)
			{
				OnHiddenAppointmentClick(new HiddenAppointmentsEventArgs(schedulerHourRangeCollectionLayoutState.GetDate(), View));
			}
			else if (_view == SchedulerView.DayView)
			{
				TimeSpan timeSpan = LastVisibleTime;
				foreach (SchedulerAppointmentView item in _appointmentViews)
				{
					if (item.GetAppointment().StartTime.TimeOfDay > timeSpan && item.GetAppointment().Owner == schedulerHourRangeCollectionLayoutState.GetOwner() && item.GetAppointment().StartTime.Date <= schedulerHourRangeCollectionLayoutState.GetDate() && item.GetAppointment().EndTime > schedulerHourRangeCollectionLayoutState.GetDate())
					{
						if (item.GetAppointment().EndTime.Date > schedulerHourRangeCollectionLayoutState.GetDate())
						{
							timeSpan = new TimeSpan(1, 0, 0, 0);
							break;
						}
						timeSpan = item.GetAppointment().EndTime.TimeOfDay;
					}
				}
				int visibleSlotCount = schedulerHourRangeCollectionLayoutState._bounds.Height / _actualRowHeight;
				int minutes = visibleSlotCount * _minuteInterval;
				if ((int)timeSpan.TotalMinutes % _minuteInterval != 0)
				{
					timeSpan = new TimeSpan(0, (int)timeSpan.TotalMinutes / _minuteInterval * _minuteInterval + _minuteInterval, 0);
				}
				FirstVisibleTime = timeSpan.Subtract(new TimeSpan(0, minutes, 0));
			}
			else
			{
				ShowDayViewForRange(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetDate().AddDays(1.0));
			}
		}

		internal void ShowDayViewForRange(DateTime dateTimeArgument, DateTime rangeEnd)
		{
			DateTime dateTime = dateTimeArgument;
			_isUpdatingDates = true;
			View = SchedulerView.DayView;
			_dates.ClearWithoutNotify();
			while (dateTime < rangeEnd)
			{
				Dates.AddWithoutNotify(dateTime);
				dateTime = dateTime.AddDays(1.0);
			}
			_isUpdatingDates = false;
			RefreshAfterDatesChange();
		}

		private DateTime GetDateInMonthAt(Point pt)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item._bounds.Contains(pt) && item.GetIsDateInRange())
				{
					return item.GetDate().Date;
				}
			}
			return DateTime.MinValue;
		}

		private bool CanAddAppointmentAt(DateTime startTime, DateTime endTime)
		{
			if (_addNewMode != AddNewMode.Automatic)
			{
				return false;
			}
			if (!_allowDragInNonWorkingTime && _view == SchedulerView.DayView && !IsWorkingTime(startTime, endTime, CurrentOwner) && (startTime.TimeOfDay != TimeSpan.Zero || endTime.TimeOfDay != TimeSpan.Zero || startTime == endTime))
			{
				return false;
			}
			return true;
		}

		private void OnRepaintTimerTick(object sender, EventArgs eventArgs)
		{
			Invalidate();
		}

		internal SchedulerTimeSlots TimeSlots => _timeSlots ??= new SchedulerTimeSlots(this);

		internal bool IsWorkingTimeSlot(SchedulerHourRangeCollection schedulerHourRangeCollection, DateTime start, TimeSpan timeSpan)
		{
			return TimeSlots.IsWorkingTimeSlot(schedulerHourRangeCollection, start, timeSpan);
		}

		public bool IsWorkingTime(DateTime startTime, DateTime endTime, SchedulerAppointmentOwner owner)
		{
			return TimeSlots.IsWorkingTime(startTime, endTime, owner);
		}

		public bool IsWorkingTime(DateTime startTime, DateTime endTime)
		{
			return IsWorkingTime(startTime, endTime, null);
		}

		public DateTime FindFreeTimeSlot(DateTime proposedDate, TimeSpan duration, bool onlyWorkingTime)
		{
			return FindFreeTimeSlot(proposedDate, duration, onlyWorkingTime, null);
		}

		/// <summary>
		/// Returns the first start at or after <paramref name="proposedDate"/>, on or before the <see cref="MaxDate"/> day,
		/// where no appointment of the owner overlaps <paramref name="duration"/>. Returns <see cref="DateTime.MaxValue"/>
		/// when there is no such start. When <c>onlyWorkingTime</c> is true and the working hour rules never repeat with
		/// a short period, the search covers 400 years after the last rule with a date; a start later than that is not
		/// found, and <see cref="DateTime.MaxValue"/> is returned.
		/// </summary>
		public DateTime FindFreeTimeSlot(DateTime proposedDate, TimeSpan duration, bool onlyWorkingTime, SchedulerAppointmentOwner owner)
		{
			if (!base.Created)
			{
				CreateControl();
			}
			return TimeSlots.FindFreeTimeSlot(proposedDate, duration, onlyWorkingTime, owner);
		}

		private void SetDayViewDates(DateTime dateTime, bool isFirstDate)
		{
			if (_view != SchedulerView.DayView)
			{
				return;
			}
			if (_dates.Count == 1)
			{
				Date = dateTime;
				return;
			}
			int count = _dates.Count;
			_dates.ClearWithoutNotify();
			if (isFirstDate)
			{
				for (int i = 0; i < count; i++)
				{
					_dates.AddWithoutNotify(dateTime.AddDays(i));
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					_dates.AddWithoutNotify(dateTime.AddDays(-j));
				}
			}
			_isUpdatingDates = true;
			_isHourRangeCacheStale = true;
			OnDatesChanged(EventArgs.Empty);
			_isUpdatingDates = false;
			EnsureSelectionInRange();
			_isAppointmentCacheStale = true;
			InvalidateAppointmentLayout();
		}

		private void RemoveUnusedAllDayRenderers()
		{
			if (_usedAllDayViewCount >= _allDayAppointmentViews.Count)
			{
				return;
			}
			int rendererIndex;
			for (rendererIndex = _usedAllDayViewCount; rendererIndex < _allDayAppointmentViews.Count; rendererIndex++)
			{
				SchedulerAppointmentView renderer = (SchedulerAppointmentView)_allDayAppointmentViews[rendererIndex];
				renderer.StopTimerAndHideToolTip();
				renderer.ReleaseEffectiveFormatStyle();
				renderer = null;
				_allDayAppointmentViews.RemoveAt(rendererIndex);
				rendererIndex--;
				if (_allDayAreaControl != null)
				{
					_allDayAreaControl.Invalidate();
				}
			}
		}

		internal void RefreshAfterDatesChange()
		{
			if (_isUpdatingDates)
			{
				return;
			}
			_isUpdatingDates = true;
			if (View != SchedulerView.DayView)
			{
				_isAppointmentCacheStale = true;
				_isFullLayoutStale = true;
				if (View == SchedulerView.WorkWeek)
				{
					_isAppointmentCacheStale = true;
					_isFullLayoutStale = true;
					SetWorkWeekDates(_date);
				}
				else
				{
					RebuildDates();
					EnsureSelectionInRange();
				}
				return;
			}
			EventArgs e = new EventArgs();
			_isAppointmentCacheStale = true;
			_isFullLayoutStale = true;
			if (_allDayAreaControl != null)
			{
				_allDayAreaControl._isLayoutStale = true;
				_allDayAreaControl.Invalidate();
			}
			EnsureSelectionInRange();
			_isHourRangeCacheStale = true;
			if (_dates.Count > 0)
			{
				_date = _dates[0];
			}
			OnDatesChanged(e);
			_isUpdatingDates = false;
		}

		protected virtual void OnQueryHourRange(QueryHourRangeEventArgs e)
		{
			if (base.Events[_queryHourRangeEventKey] is QueryHourRangeEventHandler queryHourRangeEventHandler)
			{
				queryHourRangeEventHandler(this, e);
			}
		}

		protected internal virtual void OnMaxDateChanged(EventArgs e)
		{
			if (base.Events[_maxDateChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected internal virtual void OnMinDateChanged(EventArgs e)
		{
			if (base.Events[_minDateChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnDatesChanged(EventArgs e)
		{
			if (base.Events[_datesChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnIntervalChanged(EventArgs e)
		{
			if (base.Events[_intervalChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
