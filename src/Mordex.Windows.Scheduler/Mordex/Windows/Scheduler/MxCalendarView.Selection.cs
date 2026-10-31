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
		private void StartWeekSelection()
		{
			_isSelecting = true;
			_isWeekSelection = true;
			if (SelectionStyle == CalendarSelectionStyle.Free && Control.ModifierKeys == Keys.Control)
			{
				_selectionAnchorDate = _calendarDateRange.Start;
				_anchorWeekRange = _calendarDateRange;
				if (IsRangeFullySelected(_calendarDateRange))
				{
					RemoveRangeFromSelection(_calendarDateRange);
				}
				else if (_selectedDateList.Count <= _maxSelectionCount - 7)
				{
					SelectDateRange(_calendarDateRange, false);
				}
				return;
			}
			if (_anchorWeekRange.IsIncomplete())
			{
				_anchorWeekRange = new DateRange(_selectionAnchorDate, _selectionAnchorDate);
			}
			if (!(_calendarDateRange.Start != _activeSelectionRange.Start) && !(_calendarDateRange.End != _activeSelectionRange.End))
			{
				return;
			}
			if (Control.ModifierKeys == Keys.Shift)
			{
				if (_anchorWeekRange.Start < _calendarDateRange.Start)
				{
					SelectScheduleRange(new DateRange(_anchorWeekRange.Start, _calendarDateRange.End));
				}
				else
				{
					SelectScheduleRange(new DateRange(_calendarDateRange.Start, _anchorWeekRange.End));
				}
			}
			else
			{
				ClearSelectedDays();
				_selectionAnchorDate = _calendarDateRange.Start;
				_anchorWeekRange = _calendarDateRange;
				SelectDateRange(_calendarDateRange, false);
			}
		}

		private void UpdateCalendarSelectedDates()
		{
			ArrayList arrayList = new ArrayList();
			DateTime[] originalDates = _calendarSelectedDates;
			foreach (DateTime dateTime in originalDates)
			{
				arrayList.Add(dateTime);
			}
			arrayList.Sort();
			DateTime nextDate = (DateTime)arrayList[0];
			DateTime[] contiguousDates = new DateTime[arrayList.Count];
			for (int j = 0; j < _calendarSelectedDates.Length; j++)
			{
				contiguousDates.SetValue(nextDate, j);
				nextDate = nextDate.AddDays(1.0);
			}
			_calendarSelectedDates = contiguousDates;
		}

		private void UpdateSelectionRange(bool shouldUpdateBoldDates)
		{
			try
			{
				for (int i = 0; i < _boldedDates.Length; i++)
				{
					DateTime dateTime = _boldedDates[i];
					FindMonthByDate(dateTime)?.GetDayCellForDate(dateTime)?.SetBold(true);
				}
				for (int i = 0; i < _annuallyBoldedDates.Length; i++)
				{
					DateTime dateTime = _annuallyBoldedDates[i];
					for (int j = 0; j <= _workingHoursSchemaLayoutStateArray.GetUpperBound(0); j++)
					{
						for (int k = 0; k <= _workingHoursSchemaLayoutStateArray.GetUpperBound(1); k++)
						{
							WorkingHoursSchemaLayoutState workingHoursSchemaLayoutStateArgument = _workingHoursSchemaLayoutStateArray[j, k];
							DateTime dateInYear = new DateTime(workingHoursSchemaLayoutStateArgument.GetYear(), dateTime.Month, dateTime.Day);
							if (dateInYear > workingHoursSchemaLayoutStateArgument.GetDateRange().Start && dateInYear < workingHoursSchemaLayoutStateArgument.GetDateRange().End)
							{
								SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = workingHoursSchemaLayoutStateArgument.GetDayCellForDate(dateInYear);
								if (schedulerFormatStyleLayoutStateArgument != null)
								{
									schedulerFormatStyleLayoutStateArgument.SetBold(true);
								}
								else
								{
									schedulerFormatStyleLayoutStateArgument = null;
								}
							}
						}
					}
				}
				if (_selectionStyle == CalendarSelectionStyle.Range)
				{
					DateRange selectionRange = _selectionRange;
					_selectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
					SelectionRange = selectionRange;
				}
				else if (_selectionStyle != CalendarSelectionStyle.Single)
				{
					if (_calendarSelectedDates == null)
					{
						return;
					}
					ClearSelectedDays();
					DateTime[] currentSelection = _calendarSelectedDates;
					foreach (DateTime selectedDate in currentSelection)
					{
						_selectedDateList.Add(selectedDate);
						FindDayCellByDate(selectedDate)?.SetSelected(true);
					}
				}
				if (shouldUpdateBoldDates && _scheduler != null && _boldDatesMode != BoldDatesMode.None)
				{
					UpdateDateCollection(false);
				}
			}
			catch
			{
			}
		}

		private void SetSelectionCursorDate(DateTime calendarDateTime)
		{
			if (!(_selectionCursorDate != calendarDateTime))
			{
				return;
			}
			int maxDayCount = _maxSelectionCount;
			if (_selectionAnchorDate == DateTime.MinValue)
			{
				_selectionCursorDate = calendarDateTime;
				return;
			}
			if (_selectionStyle == CalendarSelectionStyle.Schedule)
			{
				maxDayCount = ((!_isWeekSelection) ? 14 : 42);
			}
			if (!_shouldIgnoreSelectionLimit)
			{
				int dayCount = GetInclusiveDayCount(calendarDateTime, _selectionAnchorDate);
				if (dayCount > maxDayCount)
				{
					calendarDateTime = ((!(_selectionAnchorDate > calendarDateTime)) ? _selectionAnchorDate.AddDays(maxDayCount - 1) : _selectionAnchorDate.AddDays(-maxDayCount + 1));
				}
			}
			_selectionCursorDate = calendarDateTime;
		}

		private void CommitSelection()
		{
			ArrayList arrayList = new ArrayList();
			if (_selectionStyle == CalendarSelectionStyle.Free || _selectionStyle == CalendarSelectionStyle.Schedule)
			{
				_selectedDates = new DateTime[_selectedDateList.Count];
				foreach (DateTime item in _selectedDateList)
				{
					arrayList.Add(item);
				}
				arrayList.Sort();
				for (int i = 0; i < arrayList.Count; i++)
				{
					DateTime sortedDate = (DateTime)arrayList[i];
					_selectedDates.SetValue(sortedDate, i);
				}
			}
			else if (_selectionStyle == CalendarSelectionStyle.Range)
			{
				int dayCount = GetInclusiveDayCount(_selectionRange.Start, _selectionRange.End);
				_selectedDates = new DateTime[dayCount];
				DateTime day = _selectionRange.Start;
				int dayIndex = 0;
				while (day <= _selectionRange.End)
				{
					_selectedDates.SetValue(day, dayIndex);
					arrayList.Add(day);
					day = day.AddDays(1.0);
					dayIndex++;
				}
			}
			else
			{
				_selectedDates = new DateTime[1];
				_selectedDates.SetValue(CurrentDate, 0);
				arrayList.Add(CurrentDate);
			}
			if (!HasSameSelectedDates(arrayList))
			{
				_calendarSelectedDates = _selectedDates;
				EventArgs e = new EventArgs();
				OnSelectionChanged(e);
			}
		}

		private bool AreSelectedDatesContiguous()
		{
			if (_calendarSelectedDates == null || _calendarSelectedDates.Length == 0)
			{
				return false;
			}
			DateTime dateTime = _calendarSelectedDates[0];
			for (int i = 1; i < _calendarSelectedDates.Length; i++)
			{
				DateTime currentDate = _calendarSelectedDates[i];
				if (currentDate != dateTime.AddDays(1.0))
				{
					return false;
				}
				dateTime = dateTime.AddDays(1.0);
			}
			return true;
		}

		private bool SelectionMatchesSchedulerDates()
		{
			if (_scheduler._isWorkWeekView)
			{
				if (_selectedDateList.Count == 1)
				{
					return true;
				}
				if (_selectedDateList.Count == _scheduler.Dates.Count)
				{
					foreach (DateTime item in _selectedDateList)
					{
						if (!_scheduler.Dates.Contains(item))
						{
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}

		private void ApplySelectionToScheduler()
		{
			if (_isSyncingFromScheduler || !_scheduler.Created || _scheduler.Dates.Count == 0)
			{
				return;
			}
			if (!_scheduler.TryCommitEditor())
			{
				_isUpdatingScheduler = false;
				_isSyncingFromScheduler = true;
				SyncSelectionFromScheduler();
				_isSyncingFromScheduler = false;
				return;
			}
			_isUpdatingScheduler = true;
			bool isDaySelection = false;
			if (_selectedDateList.Count % 7 != 0)
			{
				isDaySelection = true;
			}
			else if (_scheduler.View == SchedulerView.DayView)
			{
				isDaySelection = true;
			}
			else if (!AreSelectedDatesContiguous())
			{
				isDaySelection = true;
			}
			else if (_scheduler.View == SchedulerView.WorkWeek)
			{
				isDaySelection = true;
			}
			if (_selectedDateList.Count <= 14 && !_isWeekSelection && isDaySelection && !_isWeekRangeSelection)
			{
				if (SelectionMatchesSchedulerDates())
				{
					_scheduler.Date = (DateTime)_selectedDateList[0];
				}
				else
				{
					_scheduler.View = SchedulerView.DayView;
					_scheduler.Dates.ClearWithoutNotify();
					foreach (DateTime item in _selectedDateList)
					{
						_scheduler.Dates.AddWithoutNotify(item);
					}
					_scheduler._date = _scheduler.Dates[0];
					_scheduler.RefreshAfterDatesChange();
					_scheduler.InvalidateAppointmentLayout();
				}
			}
			else
			{
				ArrayList arrayList = new ArrayList();
				DateTime[] currentSelection = _calendarSelectedDates;
				foreach (DateTime selectedDate in currentSelection)
				{
					arrayList.Add(selectedDate);
				}
				int weekCount = arrayList.Count / 7;
				if (weekCount > 1)
				{
					_scheduler.Weeks = weekCount;
					_scheduler.View = SchedulerView.MonthView;
				}
				else
				{
					_scheduler.View = SchedulerView.WeekView;
				}
				arrayList.Sort();
				DateTime date = (DateTime)arrayList[0];
				if (date.DayOfWeek == DayOfWeek.Sunday)
				{
					date = date.AddDays(1.0);
				}
				else if (date.DayOfWeek == DayOfWeek.Monday && _scheduler.FirstDayOfWeek == SchedulerDayOfWeek.Sunday)
				{
					date = date.AddDays(6.0);
				}
				_scheduler.Date = date;
				_activeSelectionRange = new DateRange((DateTime)arrayList[0], (DateTime)arrayList[arrayList.Count - 1]);
			}
			_isUpdatingScheduler = false;
		}

		private void RebuildSelectionFromDates()
		{
			if (_selectionStyle == CalendarSelectionStyle.Free || _selectionStyle == CalendarSelectionStyle.Schedule)
			{
				Region region = new Region(Rectangle.Empty);
				ClearSelectedDays();
				_activeSelectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
				for (int i = 0; i < _calendarSelectedDates.Length; i++)
				{
					DateTime dateTime = _calendarSelectedDates[i];
					_selectedDateList.Add(dateTime);
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(dateTime);
					if (schedulerFormatStyleLayoutState != null)
					{
						schedulerFormatStyleLayoutState.SetSelected(true);
						region.Union(schedulerFormatStyleLayoutState.GetBounds());
					}
				}
				if (_calendarSelectedDates.Length > 0)
				{
					_selectionAnchorDate = _calendarSelectedDates[0];
				}
				Invalidate(region);
				region.Dispose();
				region = null;
			}
			else if (_selectionStyle == CalendarSelectionStyle.Range)
			{
				ArrayList arrayList = new ArrayList();
				DateTime[] currentSelection = _calendarSelectedDates;
				foreach (DateTime selectedDate in currentSelection)
				{
					arrayList.Add(selectedDate);
				}
				arrayList.Sort();
				DateRange selectionRange = new DateRange((DateTime)arrayList[0], (DateTime)arrayList[arrayList.Count - 1]);
				SelectionRange = selectionRange;
			}
			else if (_calendarSelectedDates.Length > 0)
			{
				CurrentDate = _calendarSelectedDates[0];
			}
		}

		private void ClearSelectedDays()
		{
			Region region = new Region(Rectangle.Empty);
			int index;
			for (index = 0; index < _selectedDateList.Count; index++)
			{
				DateTime dateTime = (DateTime)_selectedDateList[index];
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(dateTime);
				if (schedulerFormatStyleLayoutState != null)
				{
					schedulerFormatStyleLayoutState.SetSelected(false);
					region.Union(schedulerFormatStyleLayoutState.GetBounds());
				}
				_selectedDateList.Remove(dateTime);
				index--;
			}
			Invalidate(region);
			region.Dispose();
			region = null;
		}

		private bool IsRangeFullySelected(DateRange dateRange)
		{
			DateTime start = dateRange.Start;
			while (start <= dateRange.End)
			{
				if (!_selectedDateList.Contains(start))
				{
					return false;
				}
			}
			return true;
		}

		private void RemoveRangeFromSelection(DateRange dateRange)
		{
			Region region = new Region(Rectangle.Empty);
			DateTime dateTime = dateRange.Start;
			while (dateTime <= dateRange.End)
			{
				if (_selectedDateList.Contains(dateTime))
				{
					_selectedDateList.Remove(dateTime);
				}
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(dateTime);
				if (schedulerFormatStyleLayoutState != null)
				{
					schedulerFormatStyleLayoutState.SetSelected(false);
					region.Union(schedulerFormatStyleLayoutState.GetBounds());
				}
				dateTime = dateTime.AddDays(1.0);
			}
			if (dateRange.Start == _activeSelectionRange.Start && dateRange.End == _activeSelectionRange.End)
			{
				_activeSelectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
			}
			Invalidate(region);
			region.Dispose();
			region = null;
		}

		private void SelectScheduleRange(DateRange newRange)
		{
			Region region = new Region(Rectangle.Empty);
			if (newRange.Start == _activeSelectionRange.Start && newRange.End == _activeSelectionRange.End)
			{
				return;
			}
			if (!_activeSelectionRange.IsIncomplete())
			{
				RemoveRangeFromSelection(_activeSelectionRange);
			}
			_activeSelectionRange = newRange;
			if (_activeSelectionRange.Start > _activeSelectionRange.End)
			{
				DateTime start = _activeSelectionRange.Start;
				_activeSelectionRange.Start = _activeSelectionRange.End;
				_activeSelectionRange.End = start;
			}
			int maxDayCount = 42;
			if (_selectionStyle == CalendarSelectionStyle.Free)
			{
				maxDayCount = _maxSelectionCount;
			}
			int rangeDayCount = GetInclusiveDayCount(_activeSelectionRange.Start, _activeSelectionRange.End);
			if (rangeDayCount > 8 && !_isWeekRangeSelection)
			{
				if (Control.ModifierKeys == Keys.Control && _selectedDateList.Count > 0)
				{
					maxDayCount = 14 - _selectedDateList.Count;
				}
				else
				{
					_isWeekRangeSelection = true;
					_anchorWeekRange = new DateRange(GetWeekStart(_selectionAnchorDate), GetWeekEnd(_selectionAnchorDate));
					_activeSelectionRange.Start = GetWeekStart(_activeSelectionRange.Start);
					_activeSelectionRange.End = GetWeekEnd(_activeSelectionRange.End);
				}
			}
			rangeDayCount = GetInclusiveDayCount(_activeSelectionRange.Start, _activeSelectionRange.End);
			if (rangeDayCount > maxDayCount)
			{
				if (_isWeekRangeSelection)
				{
					if (_anchorWeekRange.IsIncomplete())
					{
						_activeSelectionRange.End = _activeSelectionRange.Start.AddDays(maxDayCount);
					}
					else if (_anchorWeekRange.Start > _activeSelectionRange.Start)
					{
						_activeSelectionRange.Start = _anchorWeekRange.End.AddDays(-maxDayCount + 1);
						_activeSelectionRange.End = _anchorWeekRange.End;
					}
					else
					{
						_activeSelectionRange.End = _anchorWeekRange.Start.AddDays(maxDayCount - 1);
						_activeSelectionRange.Start = _anchorWeekRange.Start;
					}
				}
				else if (_selectionAnchorDate == DateTime.MinValue)
				{
					_activeSelectionRange.End = _activeSelectionRange.Start.AddDays(maxDayCount);
				}
				else if (_selectionAnchorDate > _activeSelectionRange.Start)
				{
					_activeSelectionRange.Start = _selectionAnchorDate.AddDays(-maxDayCount + 1);
				}
				else
				{
					_activeSelectionRange.End = _selectionAnchorDate.AddDays(maxDayCount - 1);
				}
			}
			if (!_activeSelectionRange.IsIncomplete())
			{
				DateTime start = _activeSelectionRange.Start;
				while (start <= _activeSelectionRange.End)
				{
					if (!_selectedDateList.Contains(start))
					{
						_selectedDateList.Add(start);
					}
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(start);
					if (schedulerFormatStyleLayoutState != null)
					{
						schedulerFormatStyleLayoutState.SetSelected(true);
						region.Union(schedulerFormatStyleLayoutState.GetBounds());
						schedulerFormatStyleLayoutState = null;
					}
					start = start.AddDays(1.0);
				}
			}
			Invalidate(region);
			region.Dispose();
			region = null;
		}

		private void SelectDateRange(DateRange newRange, bool shouldReplaceSelection)
		{
			Region region = new Region(Rectangle.Empty);
			int availableDayCount = 42;
			if (_selectionStyle == CalendarSelectionStyle.Free)
			{
				availableDayCount = _maxSelectionCount;
			}
			if (newRange.Start == _activeSelectionRange.Start && newRange.End == _activeSelectionRange.End && _selectedDateList.Count > 0)
			{
				return;
			}
			if (shouldReplaceSelection)
			{
				if (!_activeSelectionRange.IsIncomplete())
				{
					RemoveRangeFromSelection(_activeSelectionRange);
				}
			}
			else if (_selectedDateList.Count >= availableDayCount)
			{
				return;
			}
			_activeSelectionRange = newRange;
			if (_activeSelectionRange.Start > _activeSelectionRange.End)
			{
				DateTime start = _activeSelectionRange.Start;
				_activeSelectionRange.Start = _activeSelectionRange.End;
				_activeSelectionRange.End = start;
			}
			int rangeDayCount = GetInclusiveDayCount(_activeSelectionRange.Start, _activeSelectionRange.End);
			availableDayCount -= _selectedDateList.Count;
			if (availableDayCount > 0 && rangeDayCount > availableDayCount)
			{
				if (_selectionAnchorDate == DateTime.MinValue)
				{
					_activeSelectionRange.End = _activeSelectionRange.Start.AddDays(availableDayCount);
				}
				else if (_selectionAnchorDate > _activeSelectionRange.Start)
				{
					_activeSelectionRange.Start = _selectionAnchorDate.AddDays(-availableDayCount + 1);
				}
				else
				{
					_activeSelectionRange.End = _selectionAnchorDate.AddDays(availableDayCount - 1);
				}
			}
			if (_activeSelectionRange.Start == DateTime.MinValue)
			{
				_activeSelectionRange.Start = _activeSelectionRange.End;
			}
			if (_activeSelectionRange.End == DateTime.MinValue)
			{
				_activeSelectionRange.End = _activeSelectionRange.Start;
			}
			if (!_activeSelectionRange.IsIncomplete())
			{
				DateTime start = _activeSelectionRange.Start;
				while (start <= _activeSelectionRange.End)
				{
					if (!_selectedDateList.Contains(start))
					{
						_selectedDateList.Add(start);
					}
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(start);
					if (schedulerFormatStyleLayoutState != null)
					{
						schedulerFormatStyleLayoutState.SetSelected(true);
						region.Union(schedulerFormatStyleLayoutState.GetBounds());
						schedulerFormatStyleLayoutState = null;
					}
					start = start.AddDays(1.0);
				}
			}
			Invalidate(region);
			region.Dispose();
			region = null;
		}

		private void SyncSelectionFromScheduler()
		{
			if (_selectionStyle != CalendarSelectionStyle.Schedule || _isUpdatingScheduler)
			{
				return;
			}
			DateTime[] schedulerDates = new DateTime[_scheduler.Dates.Count];
			int dateIndex = 0;
			_isWeekSelection = false;
			foreach (DateTime date in _scheduler.Dates)
			{
				schedulerDates.SetValue(date, dateIndex);
				dateIndex++;
			}
			SelectedDates = schedulerDates;
			if (schedulerDates.Length > 0)
			{
				ScrollToShowDate(schedulerDates[0]);
			}
		}

		protected virtual void OnCurrentDateChanged(EventArgs e)
		{
			if (base.Events[_currentDateChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnSelectionChanged(EventArgs e)
		{
			EventHandler eventHandler = base.Events[_selectionChangedKey] as EventHandler;
			if (_selectionStyle == CalendarSelectionStyle.Schedule && _scheduler != null)
			{
				ApplySelectionToScheduler();
				if (_selectionStyle == CalendarSelectionStyle.Schedule && !_isUpdatingScheduler)
				{
					DateTime[] schedulerDates = new DateTime[_scheduler.Dates.Count];
					int dateIndex = 0;
					_isWeekSelection = false;
					foreach (DateTime date in _scheduler.Dates)
					{
						schedulerDates.SetValue(date, dateIndex);
						dateIndex++;
					}
					if (schedulerDates.Length > 0)
					{
						ScrollToShowDate(schedulerDates[0]);
					}
					UpdateLayout(schedulerDates);
					Update();
				}
			}
			eventHandler?.Invoke(this, e);
		}
	}
}
