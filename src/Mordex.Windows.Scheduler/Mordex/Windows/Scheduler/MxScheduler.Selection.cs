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
		void IDropListOwner.OnDropListHide(bool cancel)
		{
			CloseOwnerList(cancel);
		}

		private void UpdateCurrentTimeTimer()
		{
			if (_highlightCurrentTime == TriState.True || _highlightCurrentTime == TriState.Empty)
			{
				if (_schedulerTimer == null)
				{
					_schedulerTimer = new System.Windows.Forms.Timer();
					_schedulerTimer.Tick += OnRepaintTimerTick;
					_schedulerTimer.Interval = 600000000;
					_schedulerTimer.Enabled = true;
				}
			}
			else if (_schedulerTimer != null)
			{
				try
				{
					_schedulerTimer.Enabled = false;
					_schedulerTimer.Tick -= OnRepaintTimerTick;
					_schedulerTimer.Dispose();
					_schedulerTimer = null;
				}
				catch
				{
				}
			}
		}

		private void OnControlDragOver(object sender, DragEventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			DragEventArgs drgevent = new DragEventArgs(eventArgs.Data, eventArgs.KeyState, position.X, position.Y, eventArgs.AllowedEffect, eventArgs.Effect);
			OnDragOver(drgevent);
		}

		private void OnControlDragDrop(object sender, DragEventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			DragEventArgs drgevent = new DragEventArgs(eventArgs.Data, eventArgs.KeyState, position.X, position.Y, eventArgs.AllowedEffect, eventArgs.Effect);
			OnDragDrop(drgevent);
		}

		private void OnControlDragLeave(object sender, EventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			if (!base.ClientRectangle.Contains(position))
			{
				OnDragLeave(eventArgs);
			}
		}

		private void OnControlKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			OnKeyPress(eventArgs);
		}

		internal SchedulerAppointmentOwnerLayoutState GetPressedWeekHeader()
		{
			return _pressedWeekHeader;
		}

		internal void SetPressedWeekHeader(SchedulerAppointmentOwnerLayoutState pressedWeekHeader)
		{
			if (_pressedWeekHeader != pressedWeekHeader)
			{
				if (_pressedWeekHeader != null)
				{
					_pressedWeekHeader.SetPressed(false);
				}
				_pressedWeekHeader = pressedWeekHeader;
				if (_pressedWeekHeader != null)
				{
					_pressedWeekHeader.SetPressed(true);
				}
				Invalidate(invalidateChildren: true);
			}
		}

		private void MoveSelection(TimeSpan timeSpan, Keys keys)
		{
			if (!(timeSpan != TimeSpan.Zero))
			{
				return;
			}
			DateTime dateTime;
			DateTime end;
			if (_view == SchedulerView.DayView)
			{
				if (keys == Keys.Up || keys == Keys.Down)
				{
					if (!(_selectionAnchor.Date == _selectionAnchor.Add(timeSpan).Date))
					{
						return;
					}
					dateTime = _selectionAnchor.Add(timeSpan);
					end = dateTime.AddMinutes(_minuteInterval);
					if ((int)dateTime.TimeOfDay.TotalHours >= DayEndHour || (int)dateTime.TimeOfDay.TotalHours < DayStartHour)
					{
						return;
					}
				}
				else
				{
					dateTime = _selectionAnchor.Add(timeSpan);
					end = dateTime.AddMinutes(_minuteInterval);
					if (keys == Keys.Left)
					{
						if (!_dates.Contains(end.Date))
						{
							MovePrevious();
							if (View == SchedulerView.WorkWeek)
							{
								try
								{
									TimeSpan value = end.Subtract(dateTime);
									dateTime = _dates[_dates.Count - 1].Date.Add(dateTime.TimeOfDay);
									end = dateTime.Add(value);
								}
								catch
								{
								}
							}
						}
					}
					else if (!_dates.Contains(dateTime.Date))
					{
						MoveNext();
						if (View == SchedulerView.WorkWeek)
						{
							TimeSpan selectionSpanNext = end.Subtract(dateTime);
							dateTime = _date.Date.Add(dateTime.TimeOfDay);
							end = dateTime.Add(selectionSpanNext);
						}
					}
				}
			}
			else
			{
				dateTime = _selectionAnchor.Add(timeSpan);
				end = dateTime.AddDays(1.0);
			}
			if (!(dateTime < _minDate) && !(end.Date > _maxDate))
			{
				_selectionAnchor = dateTime;
				_selectionCursor = dateTime;
				UpdateSelectedDays(new DateRange(dateTime, end));
				ScrollSelectionIntoView(keys);
				Invalidate();
			}
		}

		private void ScrollSelectionIntoView(Keys keys)
		{
			if (_view != SchedulerView.DayView)
			{
				switch (keys)
				{
				case Keys.Left:
				case Keys.Up:
				case Keys.Left | Keys.Shift:
				case Keys.Up | Keys.Shift:
					if (_selectionCursor.Date < DateRange.Start.Date)
					{
						ScrollToPosition(_verticalScrollPosition - 1);
						GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
					}
					break;
				case Keys.Right:
				case Keys.Down:
				case Keys.Right | Keys.Shift:
				case Keys.Down | Keys.Shift:
					if (_selectionCursor > DateRange.End)
					{
						ScrollToPosition(_verticalScrollPosition + 1);
						GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
					}
					break;
				}
				return;
			}
			TimeSpan timeSpan = _firstVisibleSlotTime.Add(new TimeSpan(0, GetVerticalScrollBarControl().GetLargeChange() * _minuteInterval, 0));
			if (_selectedDays.End.TimeOfDay > timeSpan)
			{
				StyledScrollBar scrollBarDown = GetVerticalScrollBarControl();
				scrollBarDown.UpdateScrollValue(scrollBarDown.GetScrollValue() + 1);
			}
			else if (_selectedDays.End.TimeOfDay == TimeSpan.Zero)
			{
				if (_selectedDays.End > _selectedDays.Start)
				{
					StyledScrollBar scrollBarStyleControl2Argument = GetVerticalScrollBarControl();
					scrollBarStyleControl2Argument.UpdateScrollValue(scrollBarStyleControl2Argument.GetScrollValue() + 1);
				}
			}
			else if (_selectedDays.Start.TimeOfDay < _firstVisibleSlotTime)
			{
				StyledScrollBar scrollBarUp = GetVerticalScrollBarControl();
				scrollBarUp.UpdateScrollValue(scrollBarUp.GetScrollValue() - 1);
			}
			if (_dayColumns <= 1)
			{
				return;
			}
			int columnsPerDate = 1;
			if (IsMultiOwnerActive())
			{
				columnsPerDate = _visibleOwners.Count;
			}
			int selectedDayIndex = 0;
			bool isDateFound = false;
			foreach (DateTime item in _dates)
			{
				if (item.Date == _selectedDays.Start.Date)
				{
					isDateFound = true;
					break;
				}
				selectedDayIndex++;
			}
			if (!isDateFound)
			{
				return;
			}
			int ownerIndex = GetHorizontalScrollBarControl().GetScrollValue();
			if (ownerIndex > 0)
			{
				if (_visibleOwners.Count > 0)
				{
					while (ownerIndex >= _visibleOwners.Count)
					{
						ownerIndex -= _visibleOwners.Count;
					}
				}
				else
				{
					ownerIndex--;
				}
			}
			GetHorizontalScrollBarControl().UpdateScrollValue(columnsPerDate * selectedDayIndex + ownerIndex);
		}

		private void EnsureSelectionInRange()
		{
			if (_selectedDays.Start < _minDate)
			{
				SelectDateRange(_minDate, SelectedDays.End);
			}
			if (_selectedDays.End.Date > _maxDate)
			{
				SelectDateRange(SelectedDays.Start, _maxDate);
			}
			if (_dates.Count <= 0 || _isExtendingSelection)
			{
				return;
			}
			if (!_dates.Contains(_selectionAnchor.Date))
			{
				if (_view != SchedulerView.DayView)
				{
					if (_dates[0] >= _minDate)
					{
						_selectionAnchor = _dates[0];
					}
					else
					{
						_selectionAnchor = _minDate.Date;
					}
					UpdateSelectedDays(new DateRange(_selectionAnchor, _selectionAnchor.AddDays(1.0)));
				}
				else
				{
					_selectionAnchor = _dates[0].Add(_selectionAnchor.TimeOfDay);
					UpdateSelectedDays(new DateRange(_selectionAnchor, _selectionAnchor.AddMinutes(_minuteInterval)));
				}
				_selectionCursor = _selectionAnchor;
				return;
			}
			DateTime dateTime = _selectedDays.Start;
			DateTime selectionEnd = SelectedDays.End;
			if (!_dates.Contains(_selectedDays.Start.Date))
			{
				dateTime = _dates[0];
			}
			if (!_dates.Contains(_selectedDays.End.Date))
			{
				selectionEnd = _dates[_dates.Count - 1].AddDays(1.0);
			}
			SelectDateRange(dateTime, selectionEnd);
		}

		internal void UpdateSelectionForClick(SchedulerAppointment currentAppointment)
		{
			try
			{
				if (Control.ModifierKeys == Keys.Control)
				{
					if (!_multiSelect)
					{
						_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
					}
					else if (IsMultiOwnerActive() && CurrentOwner != null && !CurrentOwner.ValueEquals(currentAppointment.Owner))
					{
						_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
					}
					if (_currentAppointment == null)
					{
						_currentAppointment = currentAppointment;
					}
					currentAppointment.Selected = !currentAppointment.Selected;
				}
				else if (Control.ModifierKeys == Keys.Shift)
				{
					if (_currentAppointment == null || !_multiSelect || _currentAppointment.RecurrenceState == RecurrenceState.Master)
					{
						_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
						_currentAppointment = currentAppointment;
						UpdateSchedulerSelectedAppointments(currentAppointment);
					}
					else
					{
						DateTime endTime = _currentAppointment.EndTime;
						DateTime startTime = _currentAppointment.StartTime;
						int index = _currentAppointment.Index;
						int clickedIndex = currentAppointment.Index;
						if (currentAppointment.EndTime > endTime)
						{
							endTime = currentAppointment.EndTime;
						}
						if (currentAppointment.StartTime < startTime)
						{
							startTime = currentAppointment.StartTime;
						}
						if (index > clickedIndex)
						{
							index = clickedIndex;
							clickedIndex = _currentAppointment.Index;
						}
						new DateRange(startTime, endTime);
						ArrayList arrayList = (IsMultiOwnerActive() ? GetOwnerAppointmentsBetween(_currentAppointment, currentAppointment, CurrentOwner.Value) : GetAppointmentsBetween(_currentAppointment, currentAppointment));
						SchedulerAppointment schedulerAppointment = _currentAppointment;
						_currentAppointment = schedulerAppointment;
						if (arrayList.Count > 0)
						{
							_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
							foreach (SchedulerAppointment item in arrayList)
							{
								UpdateSchedulerSelectedAppointments(item);
							}
						}
						else
						{
							_schedulerSelectedAppointments.Clear();
						}
					}
				}
				else if (!_schedulerSelectedAppointments.Contains(currentAppointment))
				{
					if (_schedulerSelectedAppointments != null)
					{
						_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
					}
					_currentAppointment = currentAppointment;
					UpdateSchedulerSelectedAppointments(currentAppointment);
				}
			}
			catch
			{
			}
		}

		private void SelectDateRange(DateTime start, DateTime end)
		{
			UpdateSelectedDays(new DateRange(start, end));
		}

		private void UpdateSelectedDays(DateRange selectedDays)
		{
			if (_view == SchedulerView.DayView && _allDayAreaControl != null)
			{
				_allDayAreaControl.Invalidate();
			}
			if (_selectedDays.Start != selectedDays.Start || _selectedDays.End != selectedDays.End)
			{
				_selectedDays = selectedDays;
				OnSelectedDaysChanged(EventArgs.Empty);
			}
		}

		private void InvalidateSelectedDays()
		{
			if (IsMultiOwnerActive())
			{
				Invalidate();
			}
			else if (_selectedDays.Start != DateTime.MinValue)
			{
				DateTime dateTime = _selectedDays.Start.Date;
				do
				{
					InvalidateDayCell(dateTime);
					dateTime = dateTime.AddDays(1.0);
				}
				while (dateTime <= _selectedDays.End);
			}
		}

		internal void SetHoveredDate(DateTime hoveredDate)
		{
			_hoveredAppointmentDate = hoveredDate;
		}

		private void ApplyCurrentLayout()
		{
			_schedulerLayout = _currentLayout;
			SchedulerLayout schedulerLayout = null;
			if (_currentLayout != null)
			{
				schedulerLayout = _currentLayout.GetMergedLayout();
			}
			if (schedulerLayout == null || schedulerLayout.LayoutString == "")
			{
				DeserializeSchedulerState(new LayoutReader(), (PersistenceMode)0);
				BindDataSource(null, new BindingMemberInfo(""), true);
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(schedulerLayout);
				mordexLayoutReader["SchedulerLayoutData"]?.DeserializeObject(this);
				BindDataSource(_currentLayout.DataSource, new BindingMemberInfo(_currentLayout.DataMember), true);
				if (!_isInitializing)
				{
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		private void SetHotHeader(SchedulerAppointmentOwnerLayoutState hotHeader)
		{
			if (hotHeader != _hotHeader)
			{
				if (_hotHeader != null)
				{
					_hotHeader.SetHovered(false);
					DrawDateHeader(null, _hotHeader, _hotHeader.IsHovered(), _hotHeader._bounds, true);
				}
				_hotHeader = hotHeader;
				if (hotHeader != null)
				{
					_hotHeader.SetHovered(true);
					DrawDateHeader(null, _hotHeader, _hotHeader.IsHovered(), _hotHeader._bounds, true);
				}
			}
		}

		private bool IsDateSelected(DateTime dateTime)
		{
			if (dateTime >= _selectedDays.Start.Date && dateTime < _selectedDays.End)
			{
				return true;
			}
			return false;
		}

		private void AlignSelectionToSlots()
		{
			TimeSpan timeOfDay = _selectedDays.Start.TimeOfDay;
			int startMinutes = timeOfDay.Hours * 60 + timeOfDay.Minutes / _minuteInterval * _minuteInterval;
			DateTime dateTime = _selectedDays.Start.Date.AddMinutes(startMinutes);
			timeOfDay = _selectedDays.End.TimeOfDay;
			int endMinutes = timeOfDay.Hours * 60 + timeOfDay.Minutes / _minuteInterval * _minuteInterval;
			if (endMinutes <= startMinutes)
			{
				endMinutes += _minuteInterval;
			}
			SelectDateRange(dateTime, _selectedDays.End.Date.AddMinutes(endMinutes));
			_selectionAnchor = _selectedDays.Start;
			_selectionCursor = _selectionAnchor;
		}

		private bool UpdateDragAutoScroll(Point pt)
		{
			bool shouldScroll = false;
			int interval = 50;
			if (_view != SchedulerView.DayView)
			{
				if (_showOwnersHeader && IsMultiOwnerActive() && (_allowAppointmentDrag & AllowAppointmentDrag.OwnerChange) != AllowAppointmentDrag.None && _ownerHeaderBounds.Contains(pt))
				{
					shouldScroll = true;
					interval = 500;
				}
			}
			else if (base.Bounds.Contains(pt) && (_allDayAreaControl == null || !_allDayAreaControl.Bounds.Contains(pt)))
			{
				if (pt.Y + _actualRowHeight > GetClientBottom())
				{
					if (LastVisibleTime < new TimeSpan(DayEndHour, 0, 0) && (_mouseDownPoint.IsEmpty || pt.Y - _mouseDownPoint.Y > 5))
					{
						_isEdgeScrollVertical = true;
						_isEdgeScrollForward = true;
						shouldScroll = true;
					}
				}
				else if (pt.Y - _actualRowHeight <= _dayAreaTop)
				{
					if ((_mouseDownPoint.IsEmpty || _mouseDownPoint.Y - pt.Y > 5) && FirstVisibleTime > new TimeSpan(DayStartHour, 0, 0))
					{
						_isEdgeScrollVertical = true;
						_isEdgeScrollForward = false;
						shouldScroll = true;
					}
				}
				else if (DayColumns > 0 && _dayColumns < _dayLayouts.Count)
				{
					if (pt.X - _actualRowHeight <= _dayAreaLeft)
					{
						if (_mouseDownPoint.IsEmpty || _mouseDownPoint.X - pt.X > 5)
						{
							_isEdgeScrollVertical = false;
							_isEdgeScrollForward = false;
							shouldScroll = true;
						}
					}
					else if (pt.X + _actualRowHeight > GetClientRight() && (_mouseDownPoint.IsEmpty || pt.X - _mouseDownPoint.X > 5))
					{
						_isEdgeScrollVertical = false;
						_isEdgeScrollForward = true;
						shouldScroll = true;
					}
				}
			}
			if (shouldScroll)
			{
				_mouseDownPoint = Point.Empty;
				if (_edgeScrollTimer == null)
				{
					_edgeScrollTimer = new System.Windows.Forms.Timer();
					_edgeScrollTimer.Interval = interval;
					_edgeScrollTimer.Tick += OnAutoScrollTimerTick;
					_edgeScrollTimer.Start();
				}
				return true;
			}
			StopAutoScrollTimer();
			_isEdgeScrollVertical = false;
			_isEdgeScrollForward = false;
			if (_isOwnerListVisible && !_schedulerFormatStyleScrollableBase.Bounds.Contains(Cursor.Position))
			{
				_schedulerFormatStyleScrollableBase.DisposeResources();
				_schedulerFormatStyleScrollableBase.HideDropList(true);
			}
			return false;
		}

		private void UpdateDragPosition(Point pt, SchedulerAppointmentOwner schedulerAppointmentOwnerArgument, bool shouldRefreshLayout)
		{
			if (_allDayAreaControl.Bounds.Contains(pt))
			{
				ApplyAllDayAreaDrag(Cursor.Position);
				return;
			}
			DateTime dateTime = GetDateTimeAtPoint(pt);
			if (dateTime.Equals(DateTime.MinValue))
			{
				return;
			}
			_selectionAnchor = dateTime;
			if (_dragType == DragType.ResizeLeft)
			{
				if (!IsMultiOwnerActive() || schedulerAppointmentOwnerArgument == null || schedulerAppointmentOwnerArgument.ValueEquals(_schedulerSchedulerAppointment.Owner))
				{
					dateTime = ClampResizedStart(dateTime, _schedulerSchedulerAppointment);
					_schedulerSchedulerAppointment.StartTime = dateTime;
				}
				return;
			}
			if (_dragType == DragType.ResizeRight)
			{
				if (!IsMultiOwnerActive() || schedulerAppointmentOwnerArgument == null || schedulerAppointmentOwnerArgument.ValueEquals(_schedulerSchedulerAppointment.Owner))
				{
					dateTime = ClampResizedEnd(dateTime.AddMinutes(_minuteInterval), _schedulerSchedulerAppointment);
					_schedulerSchedulerAppointment.EndTime = dateTime;
				}
				return;
			}
			SchedulerAppointmentOwner schedulerAppointmentOwner = null;
			if (IsMultiOwnerActive())
			{
				schedulerAppointmentOwner = FindOwnerAtPoint(pt.X, pt.Y);
			}
			TimeSpan timeSpan = _schedulerSchedulerAppointment._timeSpan;
			if (timeSpan.TotalHours >= 24.0 && ShowAllDayArea)
			{
				timeSpan = new TimeSpan(0, _minuteInterval, 0);
			}
			bool isMoveAllowed = true;
			dateTime = GetDraggedStartTime(dateTime, _schedulerTimeSpan, timeSpan, schedulerAppointmentOwner, ref isMoveAllowed);
			if (!isMoveAllowed)
			{
				foreach (SchedulerAppointment selectedAppointment in _schedulerSelectedAppointments)
				{
					selectedAppointment.RestoreOriginalTimes(false);
				}
				_dragAnchorTime = _dragInitialAnchorTime;
				_isAppointmentLayoutStale = true;
				_isAppointmentCacheStale = true;
				Invalidate();
			}
			else
			{
				if (!(dateTime != _dragAnchorTime) && (schedulerAppointmentOwner == null || schedulerAppointmentOwner.Value == null || schedulerAppointmentOwner.Equals(_schedulerSchedulerAppointment.Owner)))
				{
					return;
				}
				TimeSpan value = dateTime.Subtract(_dragAnchorTime);
				_dragAnchorTime = _dragAnchorTime.Add(value);
				List<SchedulerAppointment> selectedSnapshot = new List<SchedulerAppointment>();
				foreach (SchedulerAppointment appointmentToCopy in _schedulerSelectedAppointments)
				{
					selectedSnapshot.Add(appointmentToCopy);
				}
				bool hasChanged = false;
				foreach (SchedulerAppointment movedAppointment in selectedSnapshot)
				{
					if (IsMultiOwnerActive() && schedulerAppointmentOwner != null && (schedulerAppointmentOwner.Value != movedAppointment.Owner || (schedulerAppointmentOwner.Value != null && !schedulerAppointmentOwner.Value.Equals(movedAppointment._ownerValue))))
					{
						hasChanged = true;
						movedAppointment.Owner = schedulerAppointmentOwner.Value;
					}
					DateTime newStartTime = movedAppointment.StartTime.Add(value);
					TimeSpan movedDuration = ((!(movedAppointment._timeSpan.TotalHours >= 24.0) || !ShowAllDayArea) ? movedAppointment._timeSpan : new TimeSpan(0, _minuteInterval, 0));
					if (movedAppointment.GetDurationInHours() >= 24 && movedAppointment._timeSpan.TotalHours < 24.0)
					{
						hasChanged = true;
						movedAppointment.ApplyDragTime(newStartTime, movedDuration, true);
					}
					else if (movedAppointment._startTime != newStartTime || movedAppointment._endTime != newStartTime.Add(movedDuration))
					{
						hasChanged = true;
						movedAppointment.ApplyDragTime(newStartTime, movedDuration, false);
					}
				}
				if (shouldRefreshLayout && hasChanged)
				{
					_isAppointmentCacheStale = true;
					_isAppointmentLayoutStale = true;
				}
				Invalidate(invalidateChildren: true);
			}
		}

		internal bool IsDragTimeAllowed(SchedulerAppointment schedulerAppointment, AppointmentDragType appointmentDragType)
		{
			if (!_allowDragInNonWorkingTime)
			{
				SchedulerAppointmentOwner schedulerAppointmentOwner = null;
				if (IsMultiOwnerActive())
				{
					schedulerAppointmentOwner = _owners.FindOwnerByValue(schedulerAppointment.Owner);
				}
				// Each time is checked with the hour ranges of its own day, as a resize clamps it.
				SchedulerDayHourRanges days = new SchedulerDayHourRanges(this, schedulerAppointmentOwner);
				switch (appointmentDragType)
				{
				case AppointmentDragType.StartTimeChange:
					if (!IsWorkingTimeSlot(days.For(schedulerAppointment.StartTime), schedulerAppointment.StartTime, TimeSpan.Zero))
					{
						return false;
					}
					break;
				case AppointmentDragType.EndTimeChange:
					// The end time is not part of the appointment, so the time just before it is checked.
					DateTime lastTime = schedulerAppointment.EndTime > schedulerAppointment.StartTime ? schedulerAppointment.EndTime.AddTicks(-1) : schedulerAppointment.EndTime;
					if (!IsWorkingTimeSlot(days.For(lastTime), lastTime, TimeSpan.Zero))
					{
						return false;
					}
					break;
				}
			}
			return true;
		}

		private bool IsOutsideDragThreshold(int x, int integerArgument)
		{
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(_dragStartScreenPoint.X - dragSize.Width, _dragStartScreenPoint.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(x, integerArgument))
			{
				return true;
			}
			return false;
		}

		private DateTime GetDraggedStartTime(DateTime dateTimeArgument, TimeSpan timeSpanArgument, TimeSpan duration, SchedulerAppointmentOwner schedulerAppointmentOwner, ref bool isAllowed)
		{
			DateTime dateTime = dateTimeArgument.Add(timeSpanArgument);
			if (_view == SchedulerView.DayView && dateTime.TimeOfDay.TotalMinutes % (double)_minuteInterval != 0.0)
			{
				int slotIndex = (int)(dateTime.TimeOfDay.TotalMinutes / (double)_minuteInterval);
				dateTime = dateTime.Date.AddMinutes(slotIndex * _minuteInterval);
			}
			if (!_allowDragInNonWorkingTime)
			{
				// The span is checked piece by piece with the rules of the day of each piece, and only for the time
				// the appointment covers.
				bool fits = duration <= DateTime.MaxValue - dateTime
					&& TimeSlots.FirstNonWorkingSlot(new SchedulerDayHourRanges(this, schedulerAppointmentOwner), dateTime, duration) == null;
				if (!fits)
				{
					if (timeSpanArgument != TimeSpan.Zero)
					{
						return GetDraggedStartTime(dateTimeArgument, TimeSpan.Zero, duration, schedulerAppointmentOwner, ref isAllowed);
					}
					isAllowed = false;
					return _dragAnchorTime;
				}
			}
			isAllowed = true;
			return dateTime;
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
		{
			base.OnQueryContinueDrag(e);
			if (_isColumnSplitterActive)
			{
				if (e.EscapePressed)
				{
					_dayResizeColumnWidth = -1;
					e.Action = DragAction.Cancel;
				}
				else
				{
					Point point = PointToClient(Cursor.Position);
					UpdateColumnResizeGuide(point.X, point.Y);
					SetCursor(Cursors.VSplit);
				}
				return;
			}
			if (e.EscapePressed)
			{
				SetCursor(null);
				e.Action = DragAction.Cancel;
				CancelAppointmentDrag();
				return;
			}
			if (e.Action != DragAction.Continue)
			{
				if (e.Action == DragAction.Drop)
				{
					Point position = Cursor.Position;
					position = PointToClient(position);
					if (!base.ClientRectangle.Contains(position))
					{
						_isDropAccepted = false;
						_isAppointmentDragActive = false;
						return;
					}
					_isDropAccepted = true;
				}
				SetCursor(null);
			}
			Point dragPosition = Cursor.Position;
			dragPosition = PointToClient(dragPosition);
			bool refreshLayout = true;
			if (UpdateDragAutoScroll(dragPosition))
			{
				if (_view != SchedulerView.DayView)
				{
					return;
				}
				refreshLayout = false;
			}
			SchedulerAppointmentOwner schedulerAppointmentOwner = FindOwnerAtPoint(dragPosition.X, dragPosition.Y);
			if (schedulerAppointmentOwner != null && (_allowAppointmentDrag & AllowAppointmentDrag.OwnerChange) == 0 && !schedulerAppointmentOwner.ValueEquals(_schedulerSchedulerAppointment.Owner))
			{
				return;
			}
			if (base.ClientRectangle.Contains(dragPosition))
			{
				if (_view == SchedulerView.DayView)
				{
					UpdateDragPosition(dragPosition, schedulerAppointmentOwner, refreshLayout);
				}
				else
				{
					DateTime dateTime = GetDateInMonthAt(dragPosition);
					if (dateTime.Equals(DateTime.MinValue))
					{
						return;
					}
					_selectionAnchor = dateTime.Date;
					if (_dragType == DragType.Move)
					{
						dateTime = _selectionAnchor.Add(_dragOriginalStart.TimeOfDay);
					}
					ApplyDragToDate(dateTime, schedulerAppointmentOwner);
				}
			}
			if (!_isCopyDrag || !base.ClientRectangle.Contains(dragPosition.X, dragPosition.Y))
			{
				return;
			}
			if (Control.ModifierKeys != Keys.Control && !_hasRemovedCopies)
			{
				_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
				foreach (SchedulerAppointment item in _draggedAppointmentCopies)
				{
					_publicAppointments.Remove(item);
					item._owner.MoveWithOwner(item.StartTime, item.EndTime, item.Owner);
					if (item == _schedulerSchedulerAppointment)
					{
						_schedulerSchedulerAppointment = item._owner;
					}
					_schedulerSelectedAppointments.Add(item._owner);
				}
				_isAppointmentCacheStale = true;
				_isAppointmentLayoutStale = true;
				Invalidate();
				_hasRemovedCopies = true;
				_hasCopiesInCollection = false;
			}
			else
			{
				if (!_hasRemovedCopies || Control.ModifierKeys != Keys.Control)
				{
					return;
				}
				_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
				for (int i = 0; i < _draggedAppointmentCopies.Count; i++)
				{
					SchedulerAppointment copiedAppointment = (SchedulerAppointment)_draggedAppointmentCopies[i];
					copiedAppointment.MoveWithOwner(copiedAppointment._owner.StartTime, copiedAppointment._owner.EndTime, copiedAppointment._owner.Owner);
					copiedAppointment._owner.RestoreOriginalTimes(false);
					if (copiedAppointment._owner == _schedulerSchedulerAppointment)
					{
						_schedulerSchedulerAppointment = copiedAppointment;
					}
					_publicAppointments.AddValidated(copiedAppointment);
					SelectedAppointments.Add(copiedAppointment);
				}
				_isAppointmentCacheStale = true;
				_isAppointmentLayoutStale = true;
				Invalidate();
				_hasRemovedCopies = false;
				_hasCopiesInCollection = true;
			}
		}

		private void ApplyDragToDate(DateTime dateTimeArgument, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			if (_dragType == DragType.ResizeRight)
			{
				DateTime dateTime = dateTimeArgument.AddDays(1.0);
				if (dateTime != _schedulerSchedulerAppointment.EndTime.Date)
				{
					if (dateTime < _schedulerSchedulerAppointment.StartTime)
					{
						_schedulerSchedulerAppointment.EndTime = _schedulerSchedulerAppointment.StartTime;
					}
					else
					{
						_schedulerSchedulerAppointment.EndTime = dateTime;
					}
					if (_view == SchedulerView.MonthView)
					{
						LayoutMonthViewAppointments();
					}
					else
					{
						LayoutWeekViewAppointments();
					}
				}
			}
			else if (_dragType == DragType.ResizeLeft)
			{
				DateTime newStartDate = dateTimeArgument;
				if (newStartDate != _schedulerSchedulerAppointment.StartTime.Date)
				{
					if (newStartDate > _schedulerSchedulerAppointment.EndTime)
					{
						_schedulerSchedulerAppointment.StartTime = _schedulerSchedulerAppointment.EndTime;
					}
					else
					{
						_schedulerSchedulerAppointment.StartTime = newStartDate;
					}
					if (_view == SchedulerView.MonthView)
					{
						LayoutMonthViewAppointments();
					}
					else
					{
						LayoutWeekViewAppointments();
					}
				}
			}
			else
			{
				MoveSelectionToDate(dateTimeArgument, schedulerAppointmentOwner);
			}
		}

		private void MoveSelectionToDate(DateTime dateTimeArgument, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			DateTime dateTime = dateTimeArgument;
			if (!IsMultiOwnerActive())
			{
				if (dateTime.Date != _dragAnchorTime.Date)
				{
					TimeSpan timeSpan = dateTime.Subtract(_dragAnchorTime);
					MoveSelectedAppointments(timeSpan, schedulerAppointmentOwner);
					Invalidate();
				}
			}
			else if (dateTime.Date != _dragAnchorTime.Date || _schedulerSchedulerAppointment.Owner != schedulerAppointmentOwner.Value)
			{
				TimeSpan moveOffset = dateTime.Subtract(_dragAnchorTime);
				MoveSelectedAppointments(moveOffset, schedulerAppointmentOwner);
			}
		}

		private void ApplyAllDayAreaDrag(Point pt)
		{
			pt = _allDayAreaControl.PointToClient(pt);
			if (!IsMultiOwnerActive())
			{
				foreach (SchedulerHourRangeCollectionLayoutState item in _allDayAreaControl._dayCells)
				{
					if (item._bounds.Contains(pt))
					{
						if (_dragType == DragType.ResizeRight)
						{
							_schedulerSchedulerAppointment.EndTime = item.GetDate().AddHours(DayEndHour);
						}
						else if (_dragType == DragType.ResizeLeft)
						{
							_schedulerSchedulerAppointment.StartTime = item.GetDate().AddHours(DayStartHour);
						}
						else if (_schedulerSchedulerAppointment._startTime != item.GetDate() || _schedulerSchedulerAppointment.GetDurationInHours() < 24)
						{
							int dayShift = 0;
							int durationInDays = 0;
							_dragAnchorTime = item.GetDate().Date;
							foreach (SchedulerAppointment selectedAppointment in _schedulerSelectedAppointments)
							{
								dayShift = (int)selectedAppointment._savedStartTime.Date.Subtract(_schedulerSchedulerAppointment._savedStartTime.Date).TotalDays;
								DateTime startTime = item.GetDate().AddDays(dayShift);
								durationInDays = (int)selectedAppointment._duration.TotalDays;
								if (selectedAppointment._duration.TotalDays % 1.0 != 0.0)
								{
									durationInDays++;
								}
								DateTime endTime = startTime.AddDays(durationInDays);
								selectedAppointment.Move(startTime, endTime);
								_isAppointmentCacheStale = true;
								_isAppointmentLayoutStale = true;
								Invalidate();
							}
						}
					}
				}
				return;
			}
			foreach (SchedulerHourRangeCollectionLayoutState allDayLayout in _allDayAreaControl._dayCells)
			{
				if (!allDayLayout._bounds.Contains(pt))
				{
					continue;
				}
				SchedulerAppointmentOwner schedulerAppointmentOwner = FindOwnerAtPoint(pt.X + base.ClientRectangle.Left + 1, _dayAreaTop + 10);
				if (schedulerAppointmentOwner == null)
				{
					continue;
				}
				if (_dragType == DragType.ResizeRight)
				{
					if (schedulerAppointmentOwner.ValueEquals(_schedulerSchedulerAppointment.Owner))
					{
						_schedulerSchedulerAppointment.EndTime = allDayLayout.GetDate().AddDays(1.0);
					}
				}
				else if (_dragType == DragType.ResizeLeft)
				{
					if (schedulerAppointmentOwner.ValueEquals(_schedulerSchedulerAppointment.Owner))
					{
						_schedulerSchedulerAppointment.StartTime = allDayLayout.GetDate();
					}
				}
				else
				{
					if (!(_schedulerSchedulerAppointment._startTime != allDayLayout.GetDate()) && _schedulerSchedulerAppointment.GetDurationInHours() >= 24 && schedulerAppointmentOwner.ValueEquals(_schedulerSchedulerAppointment.Owner))
					{
						continue;
					}
					int ownerDayShift = 0;
					int ownerDurationInDays = 0;
					_dragAnchorTime = allDayLayout.GetDate().Date;
					foreach (SchedulerAppointment reassignedAppointment in _schedulerSelectedAppointments)
					{
						ownerDayShift = (int)reassignedAppointment._savedStartTime.Date.Subtract(_schedulerSchedulerAppointment._savedStartTime.Date).TotalDays;
						reassignedAppointment.Owner = schedulerAppointmentOwner.Value;
						DateTime movedStartTime = allDayLayout.GetDate().AddDays(ownerDayShift);
						ownerDurationInDays = (int)reassignedAppointment._duration.TotalDays;
						if (reassignedAppointment._duration.TotalDays % 1.0 != 0.0)
						{
							ownerDurationInDays++;
						}
						DateTime movedEndTime = movedStartTime.AddDays(ownerDurationInDays);
						reassignedAppointment.Move(movedStartTime, movedEndTime);
						_isAppointmentCacheStale = true;
						_isAppointmentLayoutStale = true;
						Invalidate();
					}
				}
			}
		}

		protected virtual void OnSelectedDaysChanged(EventArgs e)
		{
			if (base.Events[_selectedDaysChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[_currentLayoutChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[_currentLayoutChangingEventKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}
	}
}
