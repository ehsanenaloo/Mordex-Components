using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView
	{
		private void OnDragScrollTimerTick(object sender, EventArgs eventArgs)
		{
			FirstMonth = _firstMonth.AddMonths(_scrollMonthStep);
			_scrollTimer.Interval = 200;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			UpdateWorkingHoursSchemaLayoutState(null);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			try
			{
				base.OnMouseUp(e);
				WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState;
				try
				{
					workingHoursSchemaLayoutState = FindMonthHeaderAt(e.X, e.Y, true);
				}
				catch
				{
					StopScrollTimer();
					_isSelecting = false;
					_isWeekRangeSelection = false;
					return;
				}
				StopScrollTimer();
				_isSelecting = false;
				if (workingHoursSchemaLayoutState == null)
				{
					CommitSelection();
					if (_calendarSelectedDates.Length > 0)
					{
						DateTime dateTime = _calendarSelectedDates[0];
						if (dateTime != DateTime.MinValue)
						{
							ScrollToShowDate(dateTime);
						}
					}
				}
				_isWeekRangeSelection = false;
			}
			catch
			{
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState;
			WorkingHoursSchemaLayoutState workingHoursSchemaLayoutStateArgument;
			try
			{
				workingHoursSchemaLayoutState = FindMonthAt(e.X, e.Y);
				workingHoursSchemaLayoutStateArgument = FindMonthHeaderAt(e.X, e.Y, false);
			}
			catch
			{
				return;
			}
			if (workingHoursSchemaLayoutStateArgument != null)
			{
				if (workingHoursSchemaLayoutStateArgument != _workingHoursSchemaLayoutState)
				{
					UpdateWorkingHoursSchemaLayoutState(workingHoursSchemaLayoutStateArgument);
				}
			}
			else
			{
				UpdateWorkingHoursSchemaLayoutState(null);
			}
			try
			{
				bool canSelectMultipleDates = false;
				if (SelectionStyle == CalendarSelectionStyle.Free || _selectionStyle == CalendarSelectionStyle.Schedule)
				{
					canSelectMultipleDates = true;
				}
				if (workingHoursSchemaLayoutState != null && canSelectMultipleDates)
				{
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellAt(workingHoursSchemaLayoutState.GetRightMarginBounds().Left - 2, e.Y, false);
					if (schedulerFormatStyleLayoutState != null && schedulerFormatStyleLayoutState.IsWithinDateLimits())
					{
						_calendarDateRange = new DateRange(schedulerFormatStyleLayoutState.GetDate().AddDays(-6.0), schedulerFormatStyleLayoutState.GetDate());
						_isOverWeekGutter = true;
					}
				}
				else
				{
					_isOverWeekGutter = false;
				}
				if (!_isSelecting)
				{
					return;
				}
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = FindDayCellAt(e.X, e.Y, true);
				if (schedulerFormatStyleLayoutStateArgument != null && schedulerFormatStyleLayoutStateArgument.GetDate() != _selectionAnchorDate && schedulerFormatStyleLayoutStateArgument.IsWithinDateLimits())
				{
					switch (SelectionStyle)
					{
					case CalendarSelectionStyle.Range:
						SelectionRange = new DateRange(_selectionAnchorDate, schedulerFormatStyleLayoutStateArgument.GetDate());
						break;
					case CalendarSelectionStyle.Free:
						if (Control.ModifierKeys == Keys.Control)
						{
							SelectDateRange(new DateRange(_selectionAnchorDate, schedulerFormatStyleLayoutStateArgument.GetDate()), false);
						}
						else
						{
							SelectDateRange(new DateRange(_selectionAnchorDate, schedulerFormatStyleLayoutStateArgument.GetDate()), true);
						}
						break;
					case CalendarSelectionStyle.Schedule:
						if (!_isWeekRangeSelection)
						{
							SelectScheduleRange(new DateRange(_selectionAnchorDate, schedulerFormatStyleLayoutStateArgument.GetDate()));
						}
						else if (schedulerFormatStyleLayoutStateArgument.GetDate() > _anchorWeekRange.Start)
						{
							SelectScheduleRange(new DateRange(_anchorWeekRange.Start, GetWeekEnd(schedulerFormatStyleLayoutStateArgument.GetDate())));
						}
						else
						{
							SelectScheduleRange(new DateRange(GetWeekStart(schedulerFormatStyleLayoutStateArgument.GetDate()), _anchorWeekRange.End));
						}
						break;
					}
				}
				else
				{
					if (!_isOverWeekGutter || !canSelectMultipleDates || _activeSelectionRange.IsIncomplete() || !(_calendarDateRange.Start != _activeSelectionRange.Start) || !(_calendarDateRange.End != _activeSelectionRange.End))
					{
						return;
					}
					if (_calendarDateRange.Start < _activeSelectionRange.Start)
					{
						if (SelectionStyle == CalendarSelectionStyle.Free)
						{
							SelectDateRange(new DateRange(_calendarDateRange.Start, _activeSelectionRange.End), false);
						}
						else
						{
							SelectScheduleRange(new DateRange(_calendarDateRange.Start, _activeSelectionRange.End));
						}
					}
					else if (SelectionStyle == CalendarSelectionStyle.Free)
					{
						SelectDateRange(new DateRange(_activeSelectionRange.Start, _calendarDateRange.End), false);
					}
					else
					{
						SelectScheduleRange(new DateRange(_activeSelectionRange.Start, _calendarDateRange.End));
					}
				}
			}
			catch
			{
			}
		}

		public CalendarHitTest HitTest()
		{
			Point point = PointToClient(Cursor.Position);
			return HitTest(point.X, point.Y);
		}

		public CalendarHitTest HitTest(Point pt)
		{
			return HitTest(pt.X, pt.Y);
		}

		public CalendarHitTest HitTest(int x, int y)
		{
			for (int i = 0; i < 2; i++)
			{
				if (_rectangleArray[i].Contains(x, y))
				{
					return CalendarHitTest.ScrollButton;
				}
			}
			WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = FindMonthHeaderAt(x, y, true);
			if (workingHoursSchemaLayoutState != null)
			{
				return CalendarHitTest.Header;
			}
			SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellAt(x, y, true);
			if (schedulerFormatStyleLayoutState != null)
			{
				return CalendarHitTest.Day;
			}
			workingHoursSchemaLayoutState = FindMonthAt(x, y);
			if (workingHoursSchemaLayoutState != null)
			{
				return CalendarHitTest.WeekNumber;
			}
			return CalendarHitTest.None;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			try
			{
				if (e.Button != MouseButtons.Left)
				{
					return;
				}
				if (_focusOnClick)
				{
					SchedulerRenderer.FocusControl(this);
				}
				for (int i = 0; i < 2; i++)
				{
					if (_rectangleArray[i].Contains(e.X, e.Y))
					{
						_ = _scrollChange;
						if (i == 0)
						{
							_scrollMonthStep = -_scrollChange;
						}
						else
						{
							_scrollMonthStep = _scrollChange;
						}
						FirstMonth = _firstMonth.AddMonths(_scrollMonthStep);
						UpdateTimer(false, SystemInformation.DoubleClickTime);
						return;
					}
				}
				WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = FindMonthHeaderAt(e.X, e.Y, true);
				if (workingHoursSchemaLayoutState != null && _useMonthSelector)
				{
					UpdateCalendarWorkingHoursSchemaLayoutState(workingHoursSchemaLayoutState);
					Update();
					_clickedMonthIndex = workingHoursSchemaLayoutState.GetMonthIndex();
					Rectangle rectangle = workingHoursSchemaLayoutState.GetHeaderBounds();
					rectangle.Width -= _borderSize.Width / 2;
					rectangle.Inflate(-12, -1);
					Point location = rectangle.Location;
					location = PointToScreen(location);
					_calendarControl.BackColor = GetBackgroundColor();
					Color foreColor = DaysFormatStyle.ForeColor;
					if (foreColor.IsEmpty)
					{
						foreColor = GetTextColor();
					}
					_calendarControl.ForeColor = foreColor;
					_calendarControl.Font = Font;
					_calendarControl.SetHighlightedMonth(new DateTime(workingHoursSchemaLayoutState.GetYear(), workingHoursSchemaLayoutState.GetMonth(), 1));
					_calendarControl._schedulerFormatStyle = _schedulerFormatStyle;
					_calendarControl._borderColor = GetThemeColors()._borderColor;
					_themeColors.DisposeResources();
					_themeColors = null;
					_calendarControl.Drop(location, rectangle.Size, (DateTime)_calendarControl.GetHighlightedMonth());
					return;
				}
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellAt(e.X, e.Y, true);
				if (schedulerFormatStyleLayoutState != null && schedulerFormatStyleLayoutState.IsWithinDateLimits())
				{
					_shouldIgnoreSelectionLimit = true;
					SetSelectionCursorDate(schedulerFormatStyleLayoutState.GetDate());
					_shouldIgnoreSelectionLimit = false;
					_isWeekSelection = false;
					switch (_selectionStyle)
					{
					case CalendarSelectionStyle.Single:
						CurrentDate = schedulerFormatStyleLayoutState.GetDate();
						break;
					case CalendarSelectionStyle.Range:
						if (Control.ModifierKeys == Keys.Shift)
						{
							_isSelecting = true;
							SelectionRange = new DateRange(schedulerFormatStyleLayoutState.GetDate(), _selectionAnchorDate);
						}
						else
						{
							_selectionAnchorDate = schedulerFormatStyleLayoutState.GetDate();
							SelectionRange = new DateRange(schedulerFormatStyleLayoutState.GetDate(), schedulerFormatStyleLayoutState.GetDate());
							_isSelecting = true;
						}
						break;
					case CalendarSelectionStyle.Free:
						if (!_activeSelectionRange.IsIncomplete())
						{
							_activeSelectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
						}
						if (Control.ModifierKeys == Keys.Control)
						{
							_isSelecting = true;
							if (_selectedDateList.Contains(schedulerFormatStyleLayoutState.GetDate()))
							{
								_selectionAnchorDate = schedulerFormatStyleLayoutState.GetDate();
								schedulerFormatStyleLayoutState.SetSelected(false);
								_selectedDateList.Remove(schedulerFormatStyleLayoutState.GetDate());
							}
							else if (_selectedDateList.Count < _maxSelectionCount)
							{
								_selectionAnchorDate = schedulerFormatStyleLayoutState.GetDate();
								schedulerFormatStyleLayoutState.SetSelected(true);
								_selectedDateList.Add(schedulerFormatStyleLayoutState.GetDate());
							}
							else
							{
								SetSelectionCursorDate(_selectionAnchorDate);
							}
							Invalidate(schedulerFormatStyleLayoutState.GetBounds());
						}
						else if (Control.ModifierKeys == Keys.Shift)
						{
							ClearSelectedDays();
							SelectDateRange(new DateRange(_selectionAnchorDate, schedulerFormatStyleLayoutState.GetDate()), true);
						}
						else
						{
							ClearSelectedDays();
							_selectionAnchorDate = schedulerFormatStyleLayoutState.GetDate();
							schedulerFormatStyleLayoutState.SetSelected(true);
							_selectedDateList.Add(schedulerFormatStyleLayoutState.GetDate());
							Invalidate(schedulerFormatStyleLayoutState.GetBounds());
							_isSelecting = true;
						}
						break;
					case CalendarSelectionStyle.Schedule:
						if (!_activeSelectionRange.IsIncomplete())
						{
							_activeSelectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
						}
						if (Control.ModifierKeys == Keys.Control)
						{
							if (_selectedDateList.Count <= 14 && _selectedDateList.Count > 1 && _selectedDateList.Contains(schedulerFormatStyleLayoutState.GetDate()))
							{
								schedulerFormatStyleLayoutState.SetSelected(false);
								_selectedDateList.Remove(schedulerFormatStyleLayoutState.GetDate());
								Invalidate(schedulerFormatStyleLayoutState.GetBounds());
							}
							else if (_selectedDateList.Count < 14)
							{
								_selectionAnchorDate = schedulerFormatStyleLayoutState.GetDate();
								_isSelecting = true;
								schedulerFormatStyleLayoutState.SetSelected(true);
								if (!_selectedDateList.Contains(schedulerFormatStyleLayoutState.GetDate()))
								{
									_selectedDateList.Add(schedulerFormatStyleLayoutState.GetDate());
								}
								Invalidate(schedulerFormatStyleLayoutState.GetBounds());
							}
						}
						else if (Control.ModifierKeys == Keys.Shift && _selectionAnchorDate != DateTime.MinValue)
						{
							int dayCount = 1;
							SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = schedulerFormatStyleLayoutState;
							if (schedulerFormatStyleLayoutState.GetDate() > _selectionAnchorDate)
							{
								dayCount = GetInclusiveDayCount(_selectionAnchorDate, schedulerFormatStyleLayoutState.GetDate());
								if (dayCount > 14)
								{
									schedulerFormatStyleLayoutStateArgument = FindDayCellByDate(schedulerFormatStyleLayoutState.GetDate().AddDays(14 - dayCount));
								}
								DateRange dateRange = new DateRange(_selectionAnchorDate, schedulerFormatStyleLayoutStateArgument.GetDate());
								if (dateRange.Start != _activeSelectionRange.Start || dateRange.End != _activeSelectionRange.End)
								{
									ClearSelectedDays();
									SelectDateRange(dateRange, true);
								}
							}
							else
							{
								dayCount = GetInclusiveDayCount(schedulerFormatStyleLayoutState.GetDate(), _selectionAnchorDate);
								if (dayCount > 14)
								{
									schedulerFormatStyleLayoutStateArgument = FindDayCellByDate(schedulerFormatStyleLayoutState.GetDate().AddDays(dayCount - 14));
								}
								DateRange dateRange = new DateRange(schedulerFormatStyleLayoutStateArgument.GetDate(), _selectionAnchorDate);
								if (dateRange.Start != _activeSelectionRange.Start || dateRange.End != _activeSelectionRange.End)
								{
									ClearSelectedDays();
									SelectDateRange(dateRange, true);
								}
							}
						}
						else
						{
							ClearSelectedDays();
							_isWeekRangeSelection = false;
							_selectionAnchorDate = schedulerFormatStyleLayoutState.GetDate();
							schedulerFormatStyleLayoutState.SetSelected(true);
							_selectedDateList.Add(schedulerFormatStyleLayoutState.GetDate());
							Invalidate(schedulerFormatStyleLayoutState.GetBounds());
							_isSelecting = true;
						}
						break;
					}
				}
				else if (_isOverWeekGutter)
				{
					_selectionCursorDate = _calendarDateRange.Start;
					StartWeekSelection();
				}
			}
			catch
			{
			}
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			if (_schedulerFormatStyleLayoutState != null)
			{
				Rectangle rc = _schedulerFormatStyleLayoutState.GetBounds();
				rc.Inflate(1, 1);
				Invalidate(rc);
				_schedulerFormatStyleLayoutState = null;
			}
			StopScrollTimer();
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);
			if (_scheduler == null || _scheduler._dragType != DragType.Move || (!e.Data.GetDataPresent(typeof(SchedulerAppointment)) && !e.Data.GetDataPresent(typeof(AppointmentDragHelper))))
			{
				return;
			}
			Point p = new Point(e.X, e.Y);
			p = PointToClient(p);
			SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellAt(p.X, p.Y, true);
			if (_schedulerFormatStyleLayoutState != null && _schedulerFormatStyleLayoutState != schedulerFormatStyleLayoutState)
			{
				Rectangle rc = _schedulerFormatStyleLayoutState.GetBounds();
				rc.Inflate(1, 1);
				Invalidate(rc);
				Update();
			}
			if (schedulerFormatStyleLayoutState != null && schedulerFormatStyleLayoutState.IsVisible() && schedulerFormatStyleLayoutState.IsWithinDateLimits())
			{
				Graphics graphics = CreateGraphics();
				if (_scheduler._isCopyDrag && Control.ModifierKeys == Keys.Control)
				{
					e.Effect = DragDropEffects.Copy;
				}
				else
				{
					e.Effect = DragDropEffects.Move;
				}
				_schedulerFormatStyleLayoutState = schedulerFormatStyleLayoutState;
				Pen pen = new Pen(SelectedFormatStyle.BackColor);
				SchedulerRenderer.DrawBorder(graphics, pen, schedulerFormatStyleLayoutState.GetBounds());
				pen.Dispose();
				pen = null;
				graphics.Dispose();
				graphics = null;
			}
			else
			{
				for (int i = 0; i < 2; i++)
				{
					if (_rectangleArray[i].Contains(p.X, p.Y))
					{
						_ = _scrollChange;
						if (i == 0)
						{
							_scrollMonthStep = -_scrollChange;
						}
						else
						{
							_scrollMonthStep = _scrollChange;
						}
						if (_scrollTimer == null)
						{
							UpdateTimer(true, 200);
						}
						e.Effect = DragDropEffects.None;
						_scheduler._hasDragChangedDates = true;
						return;
					}
				}
				e.Effect = DragDropEffects.None;
			}
			StopScrollTimer();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			if (_calendarControl._isDroppedDown)
			{
				_calendarControl.UnDrop(false);
			}
			base.OnLostFocus(e);
			Invalidate();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);
			if (_scheduler != null && _scheduler._dragType == DragType.Move && (e.Data.GetDataPresent(typeof(SchedulerAppointment)) || e.Data.GetDataPresent(typeof(AppointmentDragHelper))) && _schedulerFormatStyleLayoutState != null)
			{
				ArrayList arrayList = new ArrayList();
				foreach (SchedulerAppointment selectedAppointment in _scheduler.SelectedAppointments)
				{
					arrayList.Add(selectedAppointment);
				}
				_scheduler._isDropAccepted = true;
				SchedulerAppointment schedulerAppointment = null;
				if (e.Data.GetDataPresent(typeof(SchedulerAppointment)))
				{
					schedulerAppointment = (SchedulerAppointment)e.Data.GetData(typeof(SchedulerAppointment));
				}
				else if (e.Data.GetDataPresent(typeof(AppointmentDragHelper)))
				{
					schedulerAppointment = ((AppointmentDragHelper)e.Data.GetData(typeof(AppointmentDragHelper))).Appointment;
				}
				if (schedulerAppointment != null && (schedulerAppointment.StartTime.Date != _schedulerFormatStyleLayoutState.GetDate() || _scheduler._isCopyDrag))
				{
					int dayShift = (int)_schedulerFormatStyleLayoutState.GetDate().Subtract(schedulerAppointment.StartTime.Date).TotalDays;
					if (!_scheduler._isCopyDrag || Control.ModifierKeys != Keys.Control)
					{
						foreach (SchedulerAppointment item in arrayList)
						{
							bool isNewToScheduler = !_scheduler.Appointments.Contains(item);
							DateTime startTime = item.StartTime.AddDays(dayShift);
							DateTime endTime = startTime.Add(item.Duration);
							item.Move(startTime, endTime);
							if (item._recurrenceState != RecurrenceState.Exception && isNewToScheduler)
							{
								_scheduler.Appointments.Add(item);
							}
						}
						_scheduler.InvalidateAppointmentLayout();
					}
					else if (_scheduler._draggedAppointmentCopies != null)
					{
						foreach (SchedulerAppointment copiedAppointment in _scheduler._draggedAppointmentCopies)
						{
							DateTime copyStartTime = copiedAppointment.StartTime.AddDays(dayShift);
							DateTime copyEndTime = copyStartTime.Add(copiedAppointment.Duration);
							copiedAppointment.Move(copyStartTime, copyEndTime);
							if (copiedAppointment._recurrenceState != RecurrenceState.Exception && !_scheduler.Appointments.Contains(copiedAppointment))
							{
								_scheduler.Appointments.Add(copiedAppointment);
							}
						}
						_scheduler._hasCopiesInCollection = true;
					}
					if (!_scheduler.Dates.Contains(_schedulerFormatStyleLayoutState.GetDate()))
					{
						if (!_scheduler.IsMultiOwnerActive())
						{
							_scheduler.EnsureVisible(_schedulerFormatStyleLayoutState.GetDate().Date.Add(_scheduler._schedulerSchedulerAppointment.StartTime.TimeOfDay));
						}
						else
						{
							SchedulerAppointmentOwner owner = _scheduler.FindOwnerByValue(_scheduler._schedulerSchedulerAppointment.Owner);
							_scheduler.EnsureVisible(_schedulerFormatStyleLayoutState.GetDate().Date.Add(_scheduler._schedulerSchedulerAppointment.StartTime.TimeOfDay), owner);
						}
					}
				}
				_scheduler._isAppointmentCacheStale = true;
				_scheduler.InvalidateAppointmentLayout();
				Rectangle rc = _schedulerFormatStyleLayoutState.GetBounds();
				rc.Inflate(1, 1);
				Invalidate(rc);
			}
			StopScrollTimer();
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Left || keyData == Keys.Right)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_selectionStyle == CalendarSelectionStyle.Single || _selectionCursorDate == DateTime.MinValue)
			{
				return base.ProcessDialogKey(keyData);
			}
			int dayOffset = 0;
			if (_calendarControl._isDroppedDown && keyData == Keys.Escape)
			{
				_calendarControl.UnDrop(true);
				return true;
			}
			switch (keyData)
			{
			case Keys.Left | Keys.Shift:
				dayOffset = -1;
				break;
			case Keys.Right | Keys.Shift:
				dayOffset = 1;
				break;
			case Keys.Up | Keys.Shift:
				dayOffset = -7;
				break;
			case Keys.Down | Keys.Shift:
				dayOffset = 7;
				break;
			}
			if (dayOffset != 0)
			{
				if (_selectionStyle == CalendarSelectionStyle.Range)
				{
					SetSelectionCursorDate(AddDaysClamped(_selectionCursorDate, dayOffset));
					if (_selectionAnchorDate < _selectionCursorDate)
					{
						SelectionRange = new DateRange(_selectionAnchorDate, _selectionCursorDate);
					}
					else
					{
						SelectionRange = new DateRange(_selectionCursorDate, _selectionAnchorDate);
					}
				}
				else
				{
					if (_isWeekSelection && Math.Abs(dayOffset) == 7)
					{
						int maxSelectedCount = 42;
						if (_selectionStyle == CalendarSelectionStyle.Free)
						{
							maxSelectedCount = MaxSelectionCount;
						}
						if (dayOffset < 0)
						{
							if (_selectedDateList.Count >= maxSelectedCount && _activeSelectionRange.End == _anchorWeekRange.End)
							{
								return true;
							}
						}
						else if (_selectedDateList.Count >= maxSelectedCount && _activeSelectionRange.Start == _anchorWeekRange.Start)
						{
							return true;
						}
						ClearSelectedDays();
						SetSelectionCursorDate(AddDaysClamped(_selectionCursorDate, dayOffset));
						_calendarDateRange = new DateRange(GetWeekStart(_selectionCursorDate), GetWeekEnd(_selectionCursorDate));
						StartWeekSelection();
						_isSelecting = false;
						CommitSelection();
						return true;
					}
					SetSelectionCursorDate(AddDaysClamped(_selectionCursorDate, dayOffset));
					_isWeekSelection = false;
					ClearSelectedDays();
					if (_selectionAnchorDate < _selectionCursorDate)
					{
						SelectDateRange(new DateRange(_selectionAnchorDate, _selectionCursorDate), false);
					}
					else
					{
						SelectDateRange(new DateRange(_selectionCursorDate, _selectionAnchorDate), false);
					}
				}
				CommitSelection();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			int dayOffset = 0;
			switch (e.KeyData)
			{
			case Keys.Left:
				dayOffset = -1;
				break;
			case Keys.Right:
				dayOffset = 1;
				break;
			case Keys.Up:
				dayOffset = -7;
				break;
			case Keys.Down:
				dayOffset = 7;
				break;
			}
			if (_selectionStyle != CalendarSelectionStyle.Single && dayOffset != 0)
			{
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(_selectionAnchorDate);
				if (schedulerFormatStyleLayoutState == null)
				{
					return;
				}
				if (dayOffset > 0)
				{
					if (_selectionAnchorDate.AddDays(dayOffset) > MaxDate)
					{
						_selectionAnchorDate = MaxDate;
					}
					else
					{
						_selectionAnchorDate = _selectionAnchorDate.AddDays(dayOffset);
					}
				}
				else if (_selectionAnchorDate.AddDays(dayOffset) < MinDate)
				{
					_selectionAnchorDate = MinDate;
				}
				else
				{
					_selectionAnchorDate = _selectionAnchorDate.AddDays(dayOffset);
				}
				SetSelectionCursorDate(_selectionCursorDate.AddDays(dayOffset));
				ScrollToShowDate(_selectionAnchorDate);
				_anchorWeekRange = new DateRange(GetWeekStart(_selectionCursorDate), GetWeekEnd(_selectionCursorDate));
				if (_selectionStyle == CalendarSelectionStyle.Range)
				{
					RemoveRangeFromSelection(_selectionRange);
				}
				else
				{
					if (_isWeekSelection)
					{
						if (Math.Abs(dayOffset) == 7)
						{
							ClearSelectedDays();
							DateRange dateRange = new DateRange(_activeSelectionRange.Start.AddDays(dayOffset), _activeSelectionRange.End.AddDays(dayOffset));
							SelectScheduleRange(dateRange);
							CommitSelection();
						}
						return;
					}
					ClearSelectedDays();
				}
				schedulerFormatStyleLayoutState.SetSelected(false);
				Invalidate(schedulerFormatStyleLayoutState.GetBounds());
				if (SelectionStyle == CalendarSelectionStyle.Range)
				{
					SelectionRange = new DateRange(_selectionAnchorDate, _selectionAnchorDate);
				}
				else
				{
					_selectedDateList.Add(_selectionAnchorDate);
					schedulerFormatStyleLayoutState = FindDayCellByDate(_selectionAnchorDate);
					if (schedulerFormatStyleLayoutState != null)
					{
						schedulerFormatStyleLayoutState.SetSelected(true);
						Invalidate(schedulerFormatStyleLayoutState.GetBounds());
					}
				}
				SetSelectionCursorDate(_selectionAnchorDate);
				CommitSelection();
			}
			else
			{
				if (dayOffset == 0)
				{
					return;
				}
				if (dayOffset > 0)
				{
					if (CurrentDate.AddDays(dayOffset) > MaxDate)
					{
						CurrentDate = MaxDate;
					}
					else
					{
						CurrentDate = _currentDate.AddDays(dayOffset);
					}
				}
				else if (CurrentDate.AddDays(dayOffset) < MinDate)
				{
					CurrentDate = MinDate;
				}
				else
				{
					CurrentDate = _currentDate.AddDays(dayOffset);
				}
			}
		}
	}
}
