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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		protected override void OnGotFocus(EventArgs e)
		{
			if (!_hasFocus)
			{
				base.OnGotFocus(e);
				Invalidate();
				_hasFocus = true;
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			if (!base.ContainsFocus)
			{
				base.OnLostFocus(e);
				Invalidate();
				_hasFocus = false;
			}
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			base.OnDragOver(e);
			if (_isAppointmentDragActive && _dragType == DragType.Move)
			{
				if (_isCopyDrag && Control.ModifierKeys == Keys.Control)
				{
					e.Effect = DragDropEffects.Copy;
				}
				else
				{
					e.Effect = DragDropEffects.Move;
				}
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Left || keyData == Keys.Right)
			{
				return !_isOwnerListActive;
			}
			return base.IsInputKey(keyData);
		}

		protected override bool IsInputChar(char charCode)
		{
			if (char.IsLetterOrDigit(charCode))
			{
				return true;
			}
			return base.IsInputChar(charCode);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled)
			{
				return;
			}
			if (e.KeyData == Keys.Delete && _allowDelete && !_isEditingAppointment)
			{
				for (int i = 0; i < _schedulerSelectedAppointments.Count; i++)
				{
					SchedulerAppointment schedulerAppointment = _schedulerSelectedAppointments[i];
					schedulerAppointment.Delete();
					if (!_schedulerSelectedAppointments.Contains(schedulerAppointment))
					{
						i--;
					}
				}
				_schedulerSelectedAppointments.Clear();
				_schedulerOwner = null;
				_currentAppointment = null;
				Invalidate();
			}
			else
			{
				if (!(_selectionAnchor != DateTime.MinValue) || _isEditingAppointment)
				{
					return;
				}
				TimeSpan timeSpan = TimeSpan.Zero;
				switch (e.KeyData)
				{
				case Keys.Left:
					if (_view == SchedulerView.WeekView)
					{
						SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDayCellByDate(_selectionCursor.Date);
						if (schedulerHourRangeCollectionLayoutState != null)
						{
							timeSpan = GetDateAcrossWeekRow(true, schedulerHourRangeCollectionLayoutState).Date.Subtract(_selectionCursor.Date);
							break;
						}
						return;
					}
					if (_view == SchedulerView.DayView)
					{
						SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = null;
						schedulerHourRangeCollectionLayoutStateArgument = ((!IsMultiOwnerActive()) ? FindDayCellByDate(_selectionCursor.Date) : FindDayCellByDateAndOwner(_selectionCursor.Date, CurrentOwner));
						if (schedulerHourRangeCollectionLayoutStateArgument != null && schedulerHourRangeCollectionLayoutStateArgument.GetDayIndex() > 0)
						{
							int previousColumnIndex = schedulerHourRangeCollectionLayoutStateArgument.GetDayIndex() - 1;
							SchedulerHourRangeCollectionLayoutState previousColumn = _dayLayouts.GetLayoutAt(previousColumnIndex);
							if (previousColumn != null)
							{
								if (IsMultiOwnerActive())
								{
									CurrentOwner = previousColumn.GetOwner();
								}
								_selectionAnchor = previousColumn.GetDate().Add(_selectionAnchor.TimeOfDay);
								_selectionCursor = _selectionAnchor;
								UpdateSelectedDays(new DateRange(_selectionAnchor, _selectionAnchor.AddMinutes(_minuteInterval)));
								Invalidate();
								if (GetHorizontalScrollBarControl().GetScrollValue() > previousColumnIndex)
								{
									GetHorizontalScrollBarControl().UpdateScrollValue(previousColumnIndex);
								}
								return;
							}
						}
					}
					timeSpan = new TimeSpan(-1, 0, 0, 0);
					break;
				case Keys.Right:
					if (_view == SchedulerView.WeekView)
					{
						SchedulerHourRangeCollectionLayoutState currentCellRight = FindDayCellByDate(_selectionCursor.Date);
						if (currentCellRight != null)
						{
							timeSpan = GetDateAcrossWeekRow(false, currentCellRight).Date.Subtract(_selectionCursor.Date);
							break;
						}
						return;
					}
					if (_view == SchedulerView.DayView)
					{
						SchedulerHourRangeCollectionLayoutState currentColumnRight = null;
						currentColumnRight = ((!IsMultiOwnerActive()) ? FindDayCellByDate(_selectionCursor.Date) : FindDayCellByDateAndOwner(_selectionCursor.Date, CurrentOwner));
						if (currentColumnRight != null && currentColumnRight.GetDayIndex() < _dayLayouts.Count - 1)
						{
							int nextColumnIndex = currentColumnRight.GetDayIndex() + 1;
							SchedulerHourRangeCollectionLayoutState nextColumn = _dayLayouts.GetLayoutAt(nextColumnIndex);
							if (nextColumn != null)
							{
								if (IsMultiOwnerActive())
								{
									CurrentOwner = nextColumn.GetOwner();
								}
								_selectionAnchor = nextColumn.GetDate().Add(_selectionAnchor.TimeOfDay);
								_selectionCursor = _selectionAnchor;
								UpdateSelectedDays(new DateRange(_selectionAnchor, _selectionAnchor.AddMinutes(_minuteInterval)));
								Invalidate();
								if (GetHorizontalScrollBarControl().GetScrollValue() + DayColumns - 1 < nextColumnIndex)
								{
									GetHorizontalScrollBarControl().UpdateScrollValue(nextColumnIndex - DayColumns + 1);
								}
								return;
							}
						}
					}
					timeSpan = new TimeSpan(1, 0, 0, 0);
					break;
				case Keys.Up:
					timeSpan = ((_view != SchedulerView.MonthView) ? ((_view != SchedulerView.WeekView) ? new TimeSpan(0, -_minuteInterval, 0) : new TimeSpan(-1, 0, 0, 0)) : new TimeSpan(-7, 0, 0, 0));
					break;
				case Keys.Down:
					timeSpan = ((_view != SchedulerView.MonthView) ? ((_view != SchedulerView.WeekView) ? new TimeSpan(0, _minuteInterval, 0) : new TimeSpan(1, 0, 0, 0)) : new TimeSpan(7, 0, 0, 0));
					break;
				}
				MoveSelection(timeSpan, e.KeyData);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_isOwnerListActive)
			{
				if (_schedulerFormatStyleScrollableBase.HandleDialogKey(keyData))
				{
					return true;
				}
			}
			else
			{
				if (keyData == Keys.F4 && IsMultiOwnerActive() && _showOwnersHeader && _view != SchedulerView.DayView)
				{
					ShowOwnerListPopup();
					return true;
				}
				switch (keyData)
				{
				case Keys.Prior:
					if (_view == SchedulerView.DayView)
					{
						StyledScrollBar scrollBarPageUp = GetVerticalScrollBarControl();
						scrollBarPageUp.UpdateScrollValue(scrollBarPageUp.GetScrollValue() - GetVerticalScrollBarControl().GetLargeChange());
					}
					else
					{
						int pageUpPosition = GetVerticalScrollBarControl().GetScrollValue() - GetVerticalScrollBarControl().GetLargeChange();
						if (pageUpPosition == _verticalScrollPosition)
						{
							_verticalScrollPosition++;
						}
						ScrollToPosition(pageUpPosition);
						GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
					}
					return true;
				case Keys.Next:
					if (_view == SchedulerView.DayView)
					{
						StyledScrollBar scrollBarStyleControl2Argument = GetVerticalScrollBarControl();
						scrollBarStyleControl2Argument.UpdateScrollValue(scrollBarStyleControl2Argument.GetScrollValue() + GetVerticalScrollBarControl().GetLargeChange());
					}
					else
					{
						int pageDownPosition = GetVerticalScrollBarControl().GetScrollValue() + GetVerticalScrollBarControl().GetLargeChange();
						if (pageDownPosition == _verticalScrollPosition)
						{
							_verticalScrollPosition--;
						}
						ScrollToPosition(pageDownPosition);
						GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
					}
					return true;
				case Keys.End:
				{
					DateTime endDate = _selectedDays.Start.Date;
					DateTime endSelectionStart = DateTime.MinValue;
					if (_view == SchedulerView.DayView)
					{
						endSelectionStart = endDate.Add(_workEndTime.Subtract(new TimeSpan(0, _minuteInterval, 0)));
						UpdateSelectedDays(new DateRange(endSelectionStart, endSelectionStart.AddMinutes(_minuteInterval)));
						EnsureVisible(endSelectionStart);
					}
					else if (_view == SchedulerView.WeekView)
					{
						endSelectionStart = _date.Date.AddDays(6.0);
						UpdateSelectedDays(new DateRange(endSelectionStart, endSelectionStart.AddDays(1.0)));
					}
					else
					{
						endSelectionStart = GetWeekEnd(_selectedDays.Start.Date);
						UpdateSelectedDays(new DateRange(endSelectionStart, endSelectionStart.AddDays(1.0)));
					}
					if (endSelectionStart != DateTime.MinValue)
					{
						_selectionCursor = endSelectionStart;
						_selectionAnchor = endSelectionStart;
						Invalidate();
					}
					return true;
				}
				case Keys.Home:
				{
					DateTime date = _selectedDays.Start.Date;
					DateTime minValue = DateTime.MinValue;
					if (_view == SchedulerView.DayView)
					{
						minValue = date.Add(_workStartTime);
						UpdateSelectedDays(new DateRange(minValue, minValue.AddMinutes(_minuteInterval)));
						EnsureVisible(minValue);
					}
					else if (_view == SchedulerView.WeekView)
					{
						minValue = _date.Date;
						UpdateSelectedDays(new DateRange(minValue, minValue.AddDays(1.0)));
					}
					else
					{
						minValue = GetWeekStart(_selectedDays.Start.Date);
						UpdateSelectedDays(new DateRange(minValue, minValue.AddDays(1.0)));
					}
					if (minValue != DateTime.MinValue)
					{
						_selectionCursor = minValue;
						_selectionAnchor = minValue;
						Invalidate();
					}
					return true;
				}
				}
				bool shouldExtendSelection = false;
				if (!_isAppointmentDragActive)
				{
					TimeSpan timeSpan = TimeSpan.Zero;
					if ((keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift)) && _acceptsTab)
					{
						if (TrySelectNextAppointment(keyData))
						{
							return true;
						}
						return base.ProcessDialogKey(keyData);
					}
					if (keyData == Keys.F2)
					{
						if (_schedulerOwner != null && !_isEditingAppointment)
						{
							_schedulerOwner.StartEditTimerIfAllowed();
						}
						return true;
					}
					if (!_multiSelect || _selectionCursor == DateTime.MinValue)
					{
						return base.ProcessDialogKey(keyData);
					}
					_isExtendingSelection = true;
					switch (keyData)
					{
					case Keys.Left | Keys.Shift:
						if (_view == SchedulerView.WeekView)
						{
							SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDayCellByDate(_selectionCursor.Date);
							if (schedulerHourRangeCollectionLayoutState == null)
							{
								return true;
							}
							timeSpan = GetDateAcrossWeekRow(true, schedulerHourRangeCollectionLayoutState).Date.Subtract(_selectionCursor.Date);
						}
						else
						{
							timeSpan = new TimeSpan(-1, 0, 0, 0);
							if (_view == SchedulerView.DayView)
							{
								_ = _selectedDays.End.AddDays(-1.0).Date;
								if (_selectionAnchor == _selectedDays.End)
								{
									_ = _selectedDays.Start.AddDays(-1.0).Date;
								}
								if (!_dates.Contains(_selectionCursor.AddDays(-1.0).Date))
								{
									_isExtendingSelection = false;
									MovePrevious();
									return true;
								}
							}
						}
						shouldExtendSelection = true;
						break;
					case Keys.Right | Keys.Shift:
						if (_view == SchedulerView.WeekView)
						{
							SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = FindDayCellByDate(_selectionCursor.Date);
							if (schedulerHourRangeCollectionLayoutStateArgument == null)
							{
								return true;
							}
							timeSpan = GetDateAcrossWeekRow(false, schedulerHourRangeCollectionLayoutStateArgument).Date.Subtract(_selectionCursor.Date);
						}
						else
						{
							timeSpan = new TimeSpan(1, 0, 0, 0);
							if (_view == SchedulerView.DayView && !_dates.Contains(_selectionCursor.AddDays(1.0).Date))
							{
								_isExtendingSelection = false;
								MoveNext();
								return true;
							}
						}
						shouldExtendSelection = true;
						break;
					case Keys.Up | Keys.Shift:
						if (_view == SchedulerView.DayView)
						{
							timeSpan = new TimeSpan(0, -_minuteInterval, 0);
							if (_selectedDays.Start.Hour == DayStartHour && _selectedDays.Start.Minute == 0)
							{
								return true;
							}
							if (_selectionCursor.Date != _selectionCursor.Add(timeSpan).Date && _selectionCursor == _selectedDays.Start)
							{
								return true;
							}
						}
						else
						{
							timeSpan = new TimeSpan(-7, 0, 0, 0);
						}
						shouldExtendSelection = true;
						break;
					case Keys.Down | Keys.Shift:
						if (_view == SchedulerView.DayView)
						{
							timeSpan = new TimeSpan(0, _minuteInterval, 0);
							int endHour = (int)_selectedDays.End.TimeOfDay.TotalHours;
							if (endHour == 0 || endHour >= DayEndHour)
							{
								return true;
							}
						}
						else
						{
							timeSpan = new TimeSpan(7, 0, 0, 0);
						}
						shouldExtendSelection = true;
						break;
					}
					_isExtendingSelection = false;
					if (shouldExtendSelection)
					{
						if (CanMoveSelectionCursor(timeSpan) && timeSpan != TimeSpan.Zero)
						{
							_selectionCursor = _selectionCursor.Add(timeSpan);
							if (_selectionCursor < _selectionAnchor)
							{
								if (!_dates.Contains(_selectionAnchor.Date))
								{
									_selectionAnchor = _selectionCursor.Date;
								}
								if (_view == SchedulerView.DayView)
								{
									UpdateSelectedDays(new DateRange(_selectionCursor, _selectionAnchor.AddMinutes(_minuteInterval)));
								}
								else
								{
									UpdateSelectedDays(new DateRange(_selectionCursor, _selectionAnchor.AddDays(1.0)));
								}
							}
							else if (_selectionCursor > _selectionAnchor)
							{
								if (!_dates.Contains(_selectionAnchor.Date))
								{
									_selectionAnchor = _selectionCursor.Date.AddDays(1.0);
								}
								if (_view == SchedulerView.DayView)
								{
									if (_selectionCursor == _selectedDays.End)
									{
										_selectionCursor = _selectionCursor.Add(timeSpan);
									}
									UpdateSelectedDays(new DateRange(_selectedDays.Start, _selectionCursor));
								}
								else
								{
									UpdateSelectedDays(new DateRange(_selectedDays.Start, _selectionCursor.AddDays(1.0)));
								}
							}
							else if (_selectionCursor == _selectionAnchor && _view == SchedulerView.DayView)
							{
								if (timeSpan < TimeSpan.Zero && (Keys.Up | Keys.Shift) == keyData)
								{
									_selectionCursor = _selectionCursor.Add(timeSpan);
									UpdateSelectedDays(new DateRange(_selectionCursor, _selectedDays.End));
								}
								else
								{
									UpdateSelectedDays(new DateRange(_selectionCursor, _selectionCursor.AddMinutes(_minuteInterval)));
								}
							}
							else
							{
								UpdateSelectedDays(new DateRange(_selectionCursor, _selectionCursor.AddDays(1.0)));
							}
							ScrollSelectionIntoView(keyData);
							EnsureSelectionInRange();
							Invalidate();
						}
						return true;
					}
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			StopAutoScrollTimer();
			_isSelectingWithMouse = false;
			_dragStartScreenPoint = Point.Empty;
			if (GetPressedWeekHeader() != null && _pressedWeekHeader._bounds.Contains(e.X, e.Y))
			{
				if (!_pressedWeekHeader._dateRange.IsIncomplete())
				{
					ShowDayViewForRange(_pressedWeekHeader._dateRange.Start, _pressedWeekHeader._dateRange.End.AddDays(1.0));
				}
				else
				{
					ShowDayViewForRange(_pressedWeekHeader.GetDate().Date, _pressedWeekHeader.GetDate().Date.AddDays(1.0));
				}
			}
			SetPressedWeekHeader(null);
			foreach (SchedulerAppointmentView item in _appointmentViews)
			{
				if (item._bounds.Contains(e.X, e.Y))
				{
					item.HandleMouseUp(e);
					base.OnMouseUp(e);
					return;
				}
			}
			if (_view == SchedulerView.DayView)
			{
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					if (allDayView._bounds.Contains(e.X, e.Y))
					{
						SetHoveredAppointment(allDayView.GetAppointment());
						SetHoveredDate(allDayView._date.Date);
						allDayView.HandleMouseUp(e);
						base.OnMouseUp(e);
						return;
					}
				}
			}
			if (GetHoveredAppointmentView() != null)
			{
				GetHoveredAppointmentView()._isMouseDown = false;
			}
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left && GetHoveredAppointmentView() != null)
			{
				if (GetHoveredAppointmentView()._isMouseDown)
				{
					GetHoveredAppointmentView().HandleMouseMove(e);
					return;
				}
			}
			else
			{
				if (_dayLayouts != null)
				{
					if (_allDayAreaControl != null)
					{
						foreach (SchedulerHourRangeCollectionLayoutState item in _allDayAreaControl._dayCells)
						{
							if (item.HandleMouseMove(e, this))
							{
								return;
							}
						}
					}
					foreach (SchedulerHourRangeCollectionLayoutState dayCell in _dayLayouts)
					{
						if (dayCell._bounds.Contains(e.X, e.Y))
						{
							if (dayCell.HandleMouseMove(e, this))
							{
								return;
							}
							break;
						}
					}
					if (_weekHeaderLayouts != null && IsViewNavigationEnabled())
					{
						foreach (SchedulerAppointmentOwnerLayoutState headerCell in _weekHeaderLayouts)
						{
							if (headerCell._bounds.Contains(e.X, e.Y))
							{
								UpdateSchedulerSchedulerAppointmentOwnerLayoutState(headerCell);
								UpdateSchedulerAppointmentOwnerLayoutState(null);
								return;
							}
						}
					}
					UpdateSchedulerSchedulerAppointmentOwnerLayoutState(null);
					UpdateSchedulerAppointmentOwnerLayoutState(null);
				}
				foreach (SchedulerAppointmentView appointmentView in _appointmentViews)
				{
					if (appointmentView._bounds.Contains(e.X, e.Y))
					{
						SetHoveredAppointment(appointmentView.GetAppointment());
						SetHoveredDate(appointmentView._date.Date);
						appointmentView.HandleMouseMove(e);
						return;
					}
				}
			}
			if (_view == SchedulerView.DayView)
			{
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					if (allDayView._bounds.Contains(e.X, e.Y))
					{
						SetHoveredAppointment(allDayView.GetAppointment());
						SetHoveredDate(allDayView._date.Date);
						allDayView.HandleMouseMove(e);
						return;
					}
				}
			}
			SetHoveredAppointment(null);
			SetHoveredDate(DateTime.MinValue);
			if (_isAppointmentDragActive)
			{
				return;
			}
			Point point = new Point(e.X, e.Y);
			if (e.Button == MouseButtons.Left && _dragStartScreenPoint != Point.Empty && _isColumnSplitterActive && _schedulerSelectedAppointments.Count == 0)
			{
				Point position = Cursor.Position;
				if (IsOutsideDragThreshold(position.X, position.Y))
				{
					_isColumnSplitterActive = true;
					bool allowDrop = AllowDrop;
					SchedulerRenderer.SetAllowDrop(this, true);
					_mouseDownPoint = Point.Empty;
					DoDragDrop(DayColumns, DragDropEffects.All);
					if (_dayResizeColumnWidth != -1)
					{
						DayColumns = (GetClientRight() - (_dayAreaLeft - 4)) / _dayResizeColumnWidth;
					}
					SchedulerRenderer.SetAllowDrop(this, allowDrop);
					Invalidate(invalidateChildren: true);
					_isColumnSplitterActive = false;
					_dragStartScreenPoint = Point.Empty;
				}
				return;
			}
			if (_allowDayResize && _dayLayouts.Count > 1 && _view == SchedulerView.DayView && _schedulerSelectedAppointments.Count == 0)
			{
				Rectangle empty = Rectangle.Empty;
				if (GetAppointmentAt(e.X, e.Y) == null)
				{
					foreach (SchedulerHourRangeCollectionLayoutState dayColumn in _dayLayouts)
					{
						if (new Rectangle(dayColumn._bounds.X - 5, _dayAreaTop, 5, dayColumn._bounds.Height).Contains(e.X, e.Y))
						{
							_isColumnSplitterActive = true;
							SetCursor(Cursors.VSplit);
							return;
						}
					}
				}
			}
			_isColumnSplitterActive = false;
			SetCursor(null);
			if (e.Button == MouseButtons.Left && _multiSelect && _selectionAnchor != DateTime.MinValue && !_shouldIgnoreMouseSelection && !_isEditingAppointment && _isSelectingWithMouse)
			{
				DateTime minValue = DateTime.MinValue;
				if (_view == SchedulerView.DayView)
				{
					UpdateDragAutoScroll(point);
					DateTime dateTime = GetDateTimeAtPoint(point);
					if (!(dateTime != DateTime.MinValue))
					{
						return;
					}
					minValue = dateTime;
				}
				else
				{
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = GetDayCellAtPoint(point);
					if (schedulerHourRangeCollectionLayoutState == null || !schedulerHourRangeCollectionLayoutState.GetIsDateInRange())
					{
						return;
					}
					DateTime date = schedulerHourRangeCollectionLayoutState.GetDate().Date;
					if (date.Equals(DateTime.MinValue))
					{
						return;
					}
					minValue = date;
				}
				if (!(minValue != DateTime.MinValue))
				{
					return;
				}
				if (minValue < _selectionAnchor)
				{
					if (minValue != _selectedDays.Start)
					{
						InvalidateSelectedDays();
						DateTime backwardStart = minValue;
						DateTime backwardEnd = backwardStart;
						backwardEnd = ((_view != SchedulerView.DayView) ? _selectionAnchor.AddDays(1.0) : ((_allDayAreaControl == null || !_allDayAreaControl.Bounds.Contains(e.X, e.Y)) ? _selectionAnchor.AddMinutes(_minuteInterval) : _selectionAnchor.AddDays(1.0)));
						SelectDateRange(backwardStart, backwardEnd);
						InvalidateSelectedDays();
					}
				}
				else if (minValue != _selectedDays.Start || minValue != _selectedDays.End)
				{
					InvalidateSelectedDays();
					DateTime forwardStart = _selectionAnchor;
					DateTime forwardEnd = _selectionAnchor;
					forwardEnd = ((_view != SchedulerView.DayView) ? minValue.AddDays(1.0) : ((_allDayAreaControl == null || !_allDayAreaControl.Bounds.Contains(e.X, e.Y)) ? minValue.AddMinutes(_minuteInterval) : minValue.AddDays(1.0)));
					SelectDateRange(forwardStart, forwardEnd);
					InvalidateSelectedDays();
				}
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			UpdateSchedulerAppointmentOwnerLayoutState(null);
			UpdateSchedulerSchedulerAppointmentOwnerLayoutState(null);
			if (!base.ClientRectangle.Contains(position))
			{
				base.OnMouseLeave(e);
				SetHoveredAppointment(null);
				SetHoveredDate(DateTime.MinValue);
				SetCursor(null);
			}
			SetHotHeader(null);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			UpdateSchedulerSchedulerAppointmentOwnerLayoutState(null);
			UpdateSchedulerAppointmentOwnerLayoutState(null);
			HandleMouseWheelDelta(e.Delta);
		}

		private void HandleMouseWheelDelta(int delta)
		{
			if (!base.Created || base.ClientRectangle.Width <= 0 || base.ClientRectangle.Height <= 0)
			{
				return;
			}
			base.ScrollByWheelDelta(delta);
			if (!_isOwnerListVisible)
			{
				if (_view == SchedulerView.DayView)
				{
					return;
				}
				if (delta < 0)
				{
					if (IsVerticalScrollRequired())
					{
						ScrollToPosition(_verticalScrollPosition + 1);
					}
				}
				else if (delta > 0 && IsVerticalScrollRequired())
				{
					ScrollToPosition(_verticalScrollPosition - 1);
				}
			}
			else if (_schedulerFormatStyleScrollableBase != null)
			{
				_schedulerFormatStyleScrollableBase.ScrollByWheelDelta(delta);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			SchedulerRenderer.FocusControl(this);
			base.OnMouseDown(e);
			if (!TryCommitEditor())
			{
				return;
			}
			if (e.Button == MouseButtons.Left)
			{
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					if (item.HandleMouseDown(e, this))
					{
						return;
					}
				}
				if (_allDayAreaControl != null)
				{
					foreach (SchedulerHourRangeCollectionLayoutState allDayCell in _allDayAreaControl._dayCells)
					{
						if (allDayCell.HandleMouseDown(e, this))
						{
							return;
						}
					}
				}
				if (_weekHeaderLayouts != null && IsViewNavigationEnabled())
				{
					foreach (SchedulerAppointmentOwnerLayoutState headerCell in _weekHeaderLayouts)
					{
						if (headerCell._bounds.Contains(e.X, e.Y))
						{
							SetPressedWeekHeader(headerCell);
							return;
						}
					}
				}
				_isLastButtonLeft = true;
				_isSelectingWithMouse = true;
			}
			else
			{
				_isLastButtonLeft = false;
			}
			_shouldIgnoreMouseSelection = false;
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			Point point = (_mouseDownPoint = new Point(e.X, e.Y));
			if (_view == SchedulerView.DayView)
			{
				if (_isColumnSplitterActive)
				{
					_dragStartScreenPoint = Cursor.Position;
				}
				else
				{
					_dragStartScreenPoint = Point.Empty;
				}
				if (_allDayAreaControl == null || !_allDayAreaControl.Bounds.Contains(point))
				{
					DateTime dateTime = GetDateTimeAtPoint(point);
					if (!dateTime.Equals(DateTime.MinValue))
					{
						if (Control.ModifierKeys == Keys.Shift && _selectionAnchor != DateTime.MinValue)
						{
							InvalidateSelectedDays();
							if (dateTime > _selectionAnchor)
							{
								UpdateSelectedDays(new DateRange(_selectionAnchor, dateTime.AddMinutes(_minuteInterval)));
							}
							else
							{
								UpdateSelectedDays(new DateRange(dateTime, _selectionAnchor.AddMinutes(_minuteInterval)));
							}
							InvalidateSelectedDays();
						}
						else
						{
							InvalidateSelectedDays();
							SelectDateRange(dateTime, dateTime.AddMinutes(_minuteInterval));
							_selectionAnchor = dateTime;
							_selectionCursor = dateTime;
							InvalidateSelectedDays();
						}
					}
				}
				else
				{
					DateTime allDayDate = DateTime.MinValue;
					if (_allDayAreaControl != null)
					{
						if (!_showAllDayArea)
						{
							foreach (SchedulerAppointmentOwnerLayoutState allDayHeaderCell in _allDayAreaControl._dateHeaders)
							{
								if (allDayHeaderCell._bounds.Contains(e.X, e.Y))
								{
									allDayDate = allDayHeaderCell.GetDate();
									break;
								}
							}
						}
						else
						{
							foreach (SchedulerHourRangeCollectionLayoutState allDayDateCell in _allDayAreaControl._dayCells)
							{
								if (allDayDateCell._bounds.Contains(e.X, allDayDateCell._bounds.Top))
								{
									allDayDate = allDayDateCell.GetDate().Date;
									break;
								}
							}
						}
					}
					if (!allDayDate.Equals(DateTime.MinValue))
					{
						if (Control.ModifierKeys == Keys.Shift && _selectionAnchor != DateTime.MinValue)
						{
							InvalidateSelectedDays();
							if (allDayDate > _selectionAnchor)
							{
								UpdateSelectedDays(new DateRange(_selectionAnchor, allDayDate.AddDays(1.0)));
							}
							else
							{
								UpdateSelectedDays(new DateRange(allDayDate, _selectionAnchor.AddMinutes(_minuteInterval)));
							}
							InvalidateSelectedDays();
						}
						else
						{
							InvalidateSelectedDays();
							SelectDateRange(allDayDate, allDayDate.AddDays(1.0));
							_selectionAnchor = allDayDate;
							_selectionCursor = allDayDate;
							InvalidateSelectedDays();
						}
					}
				}
				if (IsMultiOwnerActive())
				{
					SchedulerAppointmentOwner ownerAtPoint = FindOwnerAtPoint(e.X, e.Y);
					if (ownerAtPoint != null)
					{
						CurrentOwner = ownerAtPoint;
					}
				}
			}
			else
			{
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = GetDayCellAtPoint(point);
				if (schedulerHourRangeCollectionLayoutState == null || !schedulerHourRangeCollectionLayoutState.GetIsDateInRange())
				{
					if (_ownerHeaderBounds.Contains(e.X, e.Y))
					{
						ShowOwnerListPopup();
					}
					return;
				}
				DateTime date = schedulerHourRangeCollectionLayoutState.GetDate().Date;
				if (!date.Equals(DateTime.MinValue))
				{
					if (Control.ModifierKeys == Keys.Shift && _selectionAnchor != DateTime.MinValue)
					{
						InvalidateSelectedDays();
						if (date > _selectionAnchor)
						{
							UpdateSelectedDays(new DateRange(_selectionAnchor, date.AddDays(1.0)));
						}
						else
						{
							UpdateSelectedDays(new DateRange(date, _selectionAnchor.AddDays(1.0)));
						}
						InvalidateSelectedDays();
					}
					else
					{
						InvalidateSelectedDays();
						SelectDateRange(date, date.AddDays(1.0));
						_selectionAnchor = date;
						_selectionCursor = date;
						InvalidateSelectedDays();
					}
				}
			}
			bool isNothingHit = true;
			if (GetHoveredAppointmentView() != null)
			{
				GetHoveredAppointmentView()._isMouseDown = false;
			}
			if (_appointmentViews != null)
			{
				foreach (SchedulerAppointmentView appointmentView in _appointmentViews)
				{
					if (appointmentView._bounds.Contains(e.X, e.Y))
					{
						SetHoveredAppointment(appointmentView.GetAppointment());
						SetHoveredDate(appointmentView._date.Date);
						appointmentView.HandleMouseDown(e);
						isNothingHit = false;
						_isColumnSplitterActive = false;
						break;
					}
				}
			}
			if (_view == SchedulerView.DayView && _allDayAreaControl != null && _allDayAreaControl.Bounds.Contains(e.X, e.Y) && _allDayAppointmentViews != null)
			{
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					if (allDayView._bounds.Contains(e.X, e.Y))
					{
						SetHoveredAppointment(allDayView.GetAppointment());
						SetHoveredDate(allDayView._date.Date);
						allDayView.HandleMouseDown(e);
						isNothingHit = false;
						break;
					}
				}
			}
			if (isNothingHit)
			{
				SetHoveredAppointment(null);
				SetHoveredDate(DateTime.MinValue);
				_currentAppointment = null;
				_schedulerSelectedAppointments.Clear();
				if (_schedulerOwner != null)
				{
					_schedulerOwner.CloseEditorIfOpen();
					_schedulerOwner = null;
				}
			}
		}

		public HitTest HitTest()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return HitTest(position.X, position.Y);
		}

		public HitTest HitTest(Point pt)
		{
			return HitTest(pt.X, pt.Y);
		}

		public HitTest HitTest(int x, int y)
		{
			if (base.ClientRectangle.Contains(x, y))
			{
				if (GetAppointmentAt(x, y) != null)
				{
					return Mordex.Windows.Scheduler.HitTest.Appointment;
				}
				if (_view == SchedulerView.DayView)
				{
					if (_allDayAreaControl != null)
					{
						if (_allDayAreaControl.Bounds.Contains(x, y))
						{
							if (_allDayAreaControl.FindHeaderAt(new Point(x, y)) != null)
							{
								return Mordex.Windows.Scheduler.HitTest.Header;
							}
							foreach (SchedulerHourRangeCollectionLayoutState item in _allDayAreaControl._dayCells)
							{
								if (item._bounds.Contains(x, y))
								{
									return Mordex.Windows.Scheduler.HitTest.AllDayArea;
								}
							}
						}
						Rectangle rectangle = new Rectangle(base.ClientRectangle.Left, _allDayAreaControl.Bottom, _dayAreaLeft - 5, GetClientBottom() - _allDayAreaControl.Bottom);
						if (rectangle.Contains(x, y))
						{
							return Mordex.Windows.Scheduler.HitTest.TimeNavigator;
						}
					}
				}
				else
				{
					if (GetHeaderLayoutAt(x, y) != null)
					{
						return Mordex.Windows.Scheduler.HitTest.Header;
					}
					if (_weekHeaderLayouts != null)
					{
						foreach (SchedulerAppointmentOwnerLayoutState weekHeader in _weekHeaderLayouts)
						{
							if (weekHeader._bounds.Contains(x, y))
							{
								return Mordex.Windows.Scheduler.HitTest.Header;
							}
						}
					}
				}
				return Mordex.Windows.Scheduler.HitTest.Schedule;
			}
			return Mordex.Windows.Scheduler.HitTest.None;
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			if (!_isLastButtonLeft)
			{
				return;
			}
			Point position = Cursor.Position;
			position = PointToClient(position);
			foreach (SchedulerAppointmentView item in _appointmentViews)
			{
				if (item._bounds.Contains(position.X, position.Y))
				{
					item.HandleDoubleClick(e);
					return;
				}
			}
			foreach (SchedulerAppointmentView allDayRenderer in _allDayAppointmentViews)
			{
				Point point = _allDayAreaControl.PointToClient(Cursor.Position);
				if (allDayRenderer._bounds.Contains(point.X, point.Y))
				{
					allDayRenderer.HandleDoubleClick(e);
					return;
				}
			}
			if (GetAppointmentAt(position) == null && _addNewMode == AddNewMode.Automatic)
			{
				DateTime dateTime = GetDateTimeAtPoint(position);
				if (!(dateTime != DateTime.MinValue))
				{
					base.OnDoubleClick(e);
					return;
				}
				DateTime newEndTime;
				if (_view == SchedulerView.DayView && _allDayAreaControl != null && _allDayAreaControl.Bounds.Contains(position))
				{
					if (position.X < _allDayAreaControl.GetLeftPanelWidth())
					{
						base.OnDoubleClick(e);
						return;
					}
					newEndTime = dateTime.Date.AddDays(1.0);
				}
				else
				{
					newEndTime = dateTime.AddMinutes(_minuteInterval);
				}
				if (!CanAddAppointmentAt(dateTime, newEndTime))
				{
					return;
				}
				SchedulerAppointment schedulerAppointment = BuildAppointment(dateTime, newEndTime, "");
				if (CurrentOwner != null)
				{
					schedulerAppointment.Owner = CurrentOwner.Value;
				}
				_addAction = AddAction.DoubleClick;
				_publicAppointments.Add(schedulerAppointment);
			}
			base.OnDoubleClick(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (e.Handled || _isEditingAppointment || !(_selectedDays.Start != DateTime.MinValue) || !IsTextCharacter(e.KeyChar) || !CanAddAppointmentAt(_selectedDays.Start, _selectedDays.End))
			{
				return;
			}
			SchedulerAppointment schedulerAppointment = BuildAppointment(_selectedDays.Start, _selectedDays.End, "");
			if (CurrentOwner != null)
			{
				schedulerAppointment.Owner = CurrentOwner.Value;
			}
			_addAction = AddAction.Typing;
			_publicAppointments.Add(schedulerAppointment);
			if (!_publicAppointments.Contains(schedulerAppointment))
			{
				return;
			}
			if (schedulerAppointment.GetDurationInHours() < 24)
			{
				EnsureVisible(_selectedDays.Start);
			}
			_newAppointmentBeingEdited = schedulerAppointment;
			_isAppointmentCacheStale = true;
			UpdateSelectionForClick(schedulerAppointment);
			if (_view == SchedulerView.DayView)
			{
				LayoutAllDayArea(null);
				LayoutDayViewAppointments();
			}
			else if (_view == SchedulerView.WeekView)
			{
				LayoutWeekViewAppointments();
			}
			else
			{
				LayoutMonthViewAppointments();
			}
			SchedulerAppointmentView appointmentRenderer = FindAppointmentView(schedulerAppointment);
			if (appointmentRenderer != null)
			{
				if (e.KeyChar.ToString() != "\r")
				{
					ShowEditor(appointmentRenderer, e.KeyChar.ToString());
				}
				else
				{
					ShowEditor(appointmentRenderer, "");
				}
			}
			Invalidate();
		}

		protected override void OnDragLeave(EventArgs e)
		{
			Point pt = PointToClient(Cursor.Position);
			if (base.ClientRectangle.Contains(pt))
			{
				return;
			}
			base.OnDragLeave(e);
			if (!_isAppointmentDragActive)
			{
				return;
			}
			if (_isCopyDrag)
			{
				_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
				foreach (SchedulerAppointment item in _draggedAppointmentCopies)
				{
					item.MoveWithOwner(item._owner.StartTime, item._owner.EndTime, item._owner.Owner);
					_publicAppointments.Remove(item);
					if (item == _schedulerSchedulerAppointment)
					{
						_schedulerSchedulerAppointment = item._owner;
					}
					_schedulerSelectedAppointments.Add(item._owner);
				}
				_hasRemovedCopies = true;
				_hasCopiesInCollection = false;
			}
			else
			{
				foreach (SchedulerAppointment selectedAppointment in _schedulerSelectedAppointments)
				{
					selectedAppointment.RestoreOriginalTimes(false);
				}
			}
			_dragAnchorTime = _dragInitialAnchorTime;
			_isAppointmentLayoutStale = true;
			_isAppointmentCacheStale = true;
			Invalidate();
		}

		internal void HandleAppointmentAdded(SchedulerAppointment app)
		{
			if (_isAppointmentDragActive || !_shouldRaiseChangeEvents)
			{
				return;
			}
			if (app.HasActiveReminder())
			{
				if (app.MasterAppointment != null || app.RecurrenceState == RecurrenceState.Master)
				{
					SchedulerAppointment schedulerAppointment = null;
					schedulerAppointment = ((app._recurrenceState != RecurrenceState.Master) ? app.MasterAppointment.GetNextReminderAppointment() : app.GetNextReminderAppointment());
					if (schedulerAppointment != null)
					{
						UpdateNextReminder(schedulerAppointment);
					}
				}
				else
				{
					UpdateNextReminder(app);
				}
			}
			AppointmentEventArgs e = new AppointmentEventArgs(app);
			OnAppointmentAdded(e);
		}

		internal void HandleAppointmentRemoved(SchedulerAppointment app)
		{
			if (!_isAppointmentDragActive)
			{
				if (app == _nextReminderAppointment)
				{
					ResetReminders();
				}
				if (!app._isRemovalSilent)
				{
					AppointmentEventArgs e = new AppointmentEventArgs(app);
					OnAppointmentRemoved(e);
				}
			}
		}
	}
}
