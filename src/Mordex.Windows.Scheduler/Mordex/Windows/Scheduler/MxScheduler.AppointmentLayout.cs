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
		private void PlaceAppointmentInLane(SchedulerAppointment[] schedulerAppointmentArray, SchedulerAppointment schedulerAppointment, DateTime dateTime, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			int visibleMinutes = (int)schedulerAppointment.GetDurationOnDate(dateTime).TotalMinutes;
			int startMinutes = (int)schedulerAppointment.GetStartTimeOnDate(dateTime).TotalMinutes;
			int slotCount = (startMinutes % _minuteInterval + visibleMinutes) / _minuteInterval;
			int bottom = _dayAreaTop;
			if (_allDayAreaControl != null)
			{
				bottom = _allDayAreaControl.Bottom;
			}
			if ((startMinutes % _minuteInterval + visibleMinutes) % _minuteInterval != 0)
			{
				slotCount++;
			}
			if (slotCount == 0)
			{
				slotCount++;
			}
			int startSlot = startMinutes / _minuteInterval;
			if (startSlot + slotCount > 1440)
			{
				slotCount = 1440 - startSlot;
			}
			if (CanPlaceInLane(schedulerAppointmentArray, schedulerAppointment, dateTime))
			{
				for (int i = startSlot; i < startSlot + slotCount; i++)
				{
					schedulerAppointmentArray.SetValue(schedulerAppointment, i);
				}
				schedulerAppointment._laneIndex = _appointmentLanes.IndexOf(schedulerAppointmentArray);
				schedulerAppointment.SetLayoutTop(startSlot * _actualRowHeight + bottom - _firstVisibleSlot.GetIndex() * _actualRowHeight);
				schedulerAppointment.SetLayoutHeight(slotCount * _actualRowHeight);
				AddDayAppointmentView(schedulerAppointment, dateTime, startSlot, slotCount, schedulerHourRangeCollectionLayoutState, true);
			}
			else
			{
				int laneIndex = _appointmentLanes.IndexOf(schedulerAppointmentArray);
				if (laneIndex == _appointmentLanes.Count - 1)
				{
					SchedulerAppointment[] newLane = new SchedulerAppointment[schedulerAppointmentArray.Length];
					_appointmentLanes.Add(newLane);
					PlaceAppointmentInLane(newLane, schedulerAppointment, dateTime, schedulerHourRangeCollectionLayoutState);
				}
				else
				{
					PlaceAppointmentInLane((SchedulerAppointment[])_appointmentLanes[laneIndex + 1], schedulerAppointment, dateTime, schedulerHourRangeCollectionLayoutState);
				}
			}
		}

		private void LayoutDayViewAppointments()
		{
			try
			{
				if (!base.Created)
				{
					return;
				}
				int slotsPerDay = (int)_interval * 24;
				bool hasAppointmentsBelow = false;
				bool hasAppointmentsAbove = false;
				if (_isAppointmentCacheStale)
				{
					RebuildDayOwnerStates();
					_isAppointmentCacheStale = false;
				}
				else
				{
					SortVisibleAppointments();
				}
				try
				{
					foreach (SchedulerAppointmentView item in _appointmentViews)
					{
						item._bounds = Rectangle.Empty;
						item.StopTimerAndHideToolTip();
					}
				}
				catch
				{
				}
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Y += _dayAreaTop;
				clientRectangle.Height -= _dayAreaTop;
				foreach (SchedulerHourRangeCollectionLayoutState dayLayout in _dayLayouts)
				{
					dayLayout.SetHasHiddenAbove(false);
					dayLayout.SetHasHiddenBelow(false);
					_appointmentLanes = new ArrayList();
					_currentDayLane = new SchedulerAppointment[slotsPerDay];
					_appointmentLanes.Add(_currentDayLane);
					foreach (SchedulerAppointment appointment in _visibleAppointments = (IsMultiOwnerActive() ? _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(dayLayout.GetDate(), dayLayout.GetOwner()).GetAppointments() : ((CurrentOwner != null) ? _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(dayLayout.GetDate(), CurrentOwner).GetAppointments() : _appointmentsByOwnerDate.FindOwnerStateByDate(dayLayout.GetDate()).GetAppointments())))
					{
						if ((!_showAllDayArea || appointment.GetDurationInHours() < 24) && appointment.StartTime.Date <= dayLayout.GetDate())
						{
							PlaceAppointmentInLane(_currentDayLane, appointment, dayLayout.GetDate(), dayLayout);
						}
					}
					foreach (SchedulerAppointmentView appointmentView in _appointmentViews)
					{
						if (appointmentView._date != dayLayout.GetDate() || appointmentView.GetSchedulerAppointmentOwner() != dayLayout.GetOwner())
						{
							continue;
						}
						SchedulerAppointment viewAppointment = appointmentView.GetAppointment();
						int viewLeft = 0;
						int viewWidth = 0;
						int lastLaneIndex = viewAppointment._laneIndex;
						for (int i = viewAppointment._laneIndex + 1; i < _appointmentLanes.Count && CanPlaceInLane((SchedulerAppointment[])_appointmentLanes[i], viewAppointment, dayLayout.GetDate()); i++)
						{
							lastLaneIndex = i;
						}
						viewWidth = dayLayout._bounds.Width + _columnGap;
						viewLeft = dayLayout._bounds.Left - _columnGap;
						int laneWidth = viewWidth / _appointmentLanes.Count;
						if (laneWidth < 10)
						{
							if (lastLaneIndex > viewWidth / 10)
							{
								lastLaneIndex = viewWidth / 10;
							}
							laneWidth = 10;
						}
						viewWidth = laneWidth * (lastLaneIndex - viewAppointment._laneIndex + 1) - _columnGap;
						viewLeft += laneWidth * viewAppointment._laneIndex;
						Rectangle b = new Rectangle(viewLeft, viewAppointment.GetLayoutTop(), viewWidth, viewAppointment.GetLayoutHeight());
						if (viewAppointment.GetLayoutTop() < _dayAreaTop + base.ClientRectangle.Top)
						{
							appointmentView._isStartInView = false;
						}
						else
						{
							appointmentView._isStartInView = true;
						}
						b = Rectangle.Intersect(clientRectangle, b);
						if (b.Right > dayLayout._bounds.Right - _columnGap)
						{
							b.Width = Math.Max(0, dayLayout._bounds.Right - b.X - _columnGap);
						}
						if (b.Height > 0 && b.Width > 0)
						{
							appointmentView._bounds = b;
							if (_appointmentEditor != null && _appointmentEditor._app == appointmentView.GetAppointment())
							{
								using (Graphics graphics = CreateGraphics())
								{
									appointmentView.RenderContent(graphics, _appointmentEditor);
								}
							}
						}
						else
						{
							appointmentView._bounds = Rectangle.Empty;
						}
						if (viewWidth > 0 && viewAppointment.GetLayoutHeight() > 0)
						{
							if (viewAppointment.GetLayoutTop() + 2 >= GetClientBottom())
							{
								dayLayout.SetHasHiddenBelow(true);
								hasAppointmentsBelow = true;
							}
							else if (viewAppointment.GetLayoutTop() + viewAppointment.GetLayoutHeight() <= _dayAreaTop + 5 + base.ClientRectangle.Top)
							{
								dayLayout.SetHasHiddenAbove(true);
								hasAppointmentsAbove = true;
							}
						}
						dayLayout.SetLaneCount(_appointmentLanes.Count);
					}
				}
				if (hasAppointmentsAbove)
				{
					ShowHiddenAppointmentButton(_dates[0], 1, new Rectangle(_dayAreaLeft - 24, _dayAreaTop + 2, 20, 8), HiddenAppointmentMode.Manual);
				}
				if (hasAppointmentsBelow)
				{
					ShowHiddenAppointmentButton(_dates[0], -1, new Rectangle(_dayAreaLeft - 24, GetClientBottom() - 9, 20, 8), HiddenAppointmentMode.Manual);
				}
				RemoveUnusedAppointmentRenderers();
				SetNextViewIndex(0);
				DisposeUnusedHiddenAppointmentButtons();
				_usedMoreButtonCount = 0;
				foreach (SchedulerAppointmentView visibleView in _appointmentViews)
				{
					if (visibleView._bounds.Left > _dayAreaLeft - 6)
					{
						visibleView.UpdateSubBoundsAndInvalidate();
						if (visibleView._bounds.Bottom > clientRectangle.Bottom)
						{
							visibleView._bounds.Height = clientRectangle.Bottom - visibleView._bounds.Top;
							visibleView._isEndInView = false;
						}
						else
						{
							visibleView._isEndInView = true;
						}
						visibleView._isShown = true;
					}
					else
					{
						visibleView._bounds = Rectangle.Empty;
					}
					if (visibleView._bounds.Right > base.ClientRectangle.Right)
					{
						visibleView._bounds = Rectangle.Empty;
					}
				}
				_appointmentLanes = null;
				if (_allDayAreaControl != null)
				{
					_allDayAreaControl.BringToFront();
				}
				_isAppointmentLayoutStale = false;
				if (_view != SchedulerView.DayView)
				{
					return;
				}
			}
			catch
			{
			}
		}

		private void UpdateHiddenAppointmentButtons()
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item.GetHasHiddenAppointments())
				{
					Rectangle bounds = new Rectangle(item._bounds.Right - 21, item._bounds.Bottom - 9, 20, 8);
					ShowHiddenAppointmentButton(item.GetDate(), -1, bounds, _hiddenAppointmentMode);
				}
				else
				{
					item.SetHasHiddenBelow(false);
					item._hiddenBelowButton = null;
				}
			}
			DisposeUnusedHiddenAppointmentButtons();
			_usedMoreButtonCount = 0;
		}

		private void LayoutAllDayAppointmentForOwners(SchedulerAppointmentView appointmentView, DateTime dateTimeArgument)
		{
			int schedulerControl2Integer = _allDayAreaControl._columnWidthWithGap;
			int viewWidth = 0;
			int columnGap = _columnGap;
			int viewLeft = _dayAreaLeft - _columnGap - _firstVisibleColumnIndex * schedulerControl2Integer;
			viewLeft += columnGap;
			SchedulerAppointment schedulerAppointment = appointmentView.GetAppointment();
			DateTime endTime = schedulerAppointment.EndTime;
			bool startsInRange = false;
			bool endsInRange = false;
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item.GetOwner().ValueEquals(schedulerAppointment.Owner))
				{
					DateTime dateTime = item.GetDate();
					startsInRange = ((dateTime == schedulerAppointment.StartTime.Date) ? true : false);
					endsInRange = ((dateTime <= endTime && dateTime.AddDays(1.0) >= endTime) ? true : false);
					if (dateTimeArgument <= dateTime && schedulerAppointment.EndTime > dateTime)
					{
						viewWidth += schedulerControl2Integer;
						if (viewWidth <= 0)
						{
							continue;
						}
						viewWidth -= columnGap * 2;
						if (_columnGap == 0)
						{
							if (startsInRange)
							{
								viewLeft += 5;
								viewWidth -= 5;
							}
							viewWidth = ((!endsInRange) ? (viewWidth - 1) : (viewWidth - 5));
						}
						SetAllDayViewBounds(appointmentView, viewWidth, startsInRange, endsInRange, viewLeft);
						if (item.GetDate() < _dates[_dates.Count - 1].Date && item.GetDate() < schedulerAppointment._endTime.Date)
						{
							SchedulerAppointmentView schedulerRenderer4Argument = GetAllDayAppointmentView(schedulerAppointment);
							LayoutAllDayAppointmentForOwners(schedulerRenderer4Argument, item.GetDate().AddDays(1.0));
						}
						break;
					}
					viewLeft += schedulerControl2Integer;
				}
				else
				{
					viewLeft += schedulerControl2Integer;
				}
			}
		}

		private void LayoutAllDayAppointment(SchedulerAppointmentView appointmentView)
		{
			if (IsMultiOwnerActive())
			{
				LayoutAllDayAppointmentForOwners(appointmentView, appointmentView.GetAppointment()._startTime.Date);
				return;
			}
			int schedulerControl2Integer = _allDayAreaControl._columnWidthWithGap;
			int viewWidth = 0;
			int columnGap = _columnGap;
			int viewLeft = _dayAreaLeft - _columnGap;
			viewLeft += columnGap;
			SchedulerAppointment schedulerAppointment = appointmentView.GetAppointment();
			DateTime endTime = schedulerAppointment.EndTime;
			bool startsInRange = false;
			bool endsInRange = false;
			DateTime date = schedulerAppointment.StartTime.Date;
			DateTime dateTime = _dayLayouts.GetLayoutAt(0).GetDate();
			if (DayColumns < _dayLayouts.Count && _dayColumns > 0 && _firstVisibleColumnIndex > 0)
			{
				dateTime = _dayLayouts.GetLayoutAt(_firstVisibleColumnIndex).GetDate();
			}
			foreach (DateTime item in _allDayAreaControl.GetDateCollection())
			{
				if (item >= dateTime)
				{
					if (schedulerAppointment.StartTime.Date <= item && schedulerAppointment.EndTime > item)
					{
						viewWidth += schedulerControl2Integer;
					}
					else if (schedulerAppointment.StartTime.Date > item)
					{
						viewLeft += schedulerControl2Integer;
					}
					if (item == date)
					{
						startsInRange = true;
					}
					if (item <= endTime && item.AddDays(1.0) >= endTime)
					{
						endsInRange = true;
					}
				}
			}
			viewWidth -= columnGap * 2;
			if (_columnGap == 0)
			{
				if (startsInRange)
				{
					viewLeft += 5;
					viewWidth -= 5;
				}
				viewWidth = ((!endsInRange) ? (viewWidth - 1) : (viewWidth - 5));
			}
			int top = _actualRowHeight + base.ClientRectangle.Top + 1;
			if (IsMultiOwnerActive() && _showOwnersHeader)
			{
				top += _actualRowHeight;
			}
			top -= _allDayScrollOffset * (_actualRowHeight + _allDayRowGap);
			_lastAllDayRowTop = FindFreeAllDayRowTop(viewLeft, top);
			appointmentView._bounds = new Rectangle(viewLeft, _lastAllDayRowTop, viewWidth, _actualRowHeight);
			if (_appointmentEditor != null && _appointmentEditor._app == appointmentView.GetAppointment())
			{
				using (Graphics graphics = CreateGraphics())
				{
					appointmentView.RenderContent(graphics, _appointmentEditor);
				}
			}
			appointmentView.UpdateFirstSegment(startsInRange);
			appointmentView.UpdateLastSegment(endsInRange);
			if (!startsInRange)
			{
				appointmentView.SetContinuedFromPrevious(true);
			}
			if (!endsInRange)
			{
				appointmentView.SetContinuedToNext(true);
			}
			appointmentView._isShown = true;
		}

		private void LayoutWeekViewAppointments()
		{
			if (!base.Created)
			{
				return;
			}
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				item.SetHasHiddenAppointments(false);
			}
			foreach (SchedulerAppointmentView staleView in _appointmentViews)
			{
				staleView._bounds = Rectangle.Empty;
				staleView.StopTimerAndHideToolTip();
			}
			ResetPlacedViewIndex();
			if (_isAppointmentCacheStale)
			{
				DateRange dateRange = new DateRange(_dayLayouts.GetLayoutAt(0).GetDate(), _dayLayouts.GetLayoutAt(_dayLayouts.Count - 1).GetDate().AddDays(1.0));
				if (IsMultiOwnerActive() || CurrentOwner != null)
				{
					_visibleAppointments = _publicAppointments.GetAppointmentsForOwner(dateRange, true, _currentOwner);
				}
				else
				{
					_visibleAppointments = _publicAppointments.GetVisibleAppointmentsInRange(dateRange, true);
				}
				_backgroundAppointments = null;
				_isAppointmentCacheStale = false;
			}
			else
			{
				SortVisibleAppointments();
			}
			foreach (SchedulerAppointment appointment in _visibleAppointments)
			{
				foreach (SchedulerHourRangeCollectionLayoutState dayLayout in _dayLayouts)
				{
					DateTime dateTime = appointment.EndTime;
					if (dateTime > appointment.StartTime && dateTime.TimeOfDay.TotalHours == 0.0)
					{
						dateTime = dateTime.Subtract(new TimeSpan(24, 0, 0));
					}
					if (!(dayLayout.GetDate() >= appointment.StartTime.Date) || !(dayLayout.GetDate() <= dateTime))
					{
						continue;
					}
					int left = dayLayout._bounds.Left;
					int top = dayLayout._bounds.Top + _actualRowHeight + _allDayRowGap;
					SchedulerAppointmentView appointmentView = GetMonthAppointmentView(left, top, dayLayout, appointment, WeekFont, false);
					if (appointmentView == null)
					{
						continue;
					}
					appointmentView._bounds = new Rectangle(dayLayout._bounds.Left + 4, appointmentView._rowTop, dayLayout._bounds.Width - 8, _actualRowHeight);
					if (dayLayout.GetDate() != appointment.StartTime.Date)
					{
						appointmentView.UpdateFirstSegment(false);
					}
					if (dayLayout.GetDate() != dateTime.Date)
					{
						appointmentView.UpdateLastSegment(false);
					}
					if (_appointmentEditor != null && _appointmentEditor._app == appointmentView.GetAppointment())
					{
						using (Graphics graphics = CreateGraphics())
						{
							appointmentView.RenderContent(graphics, _appointmentEditor);
						}
					}
				}
			}
			RemoveUnusedAppointmentRenderers();
			UpdateHiddenAppointmentButtons();
			SetNextViewIndex(0);
			_isAppointmentLayoutStale = false;
		}

		internal void UpdateSchedulerAppointmentOwnerLayoutState(SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState)
		{
			if (_schedulerAppointmentOwnerLayoutState != schedulerAppointmentOwnerLayoutState)
			{
				if (_schedulerAppointmentOwnerLayoutState != null)
				{
					_schedulerAppointmentOwnerLayoutState.SetHovered(false);
				}
				_schedulerAppointmentOwnerLayoutState = schedulerAppointmentOwnerLayoutState;
				if (_schedulerAppointmentOwnerLayoutState != null)
				{
					_schedulerAppointmentOwnerLayoutState.SetHovered(true);
					SetCursor(null);
				}
				Invalidate(invalidateChildren: true);
			}
		}

		internal void UpdateSchedulerSchedulerAppointmentOwnerLayoutState(SchedulerAppointmentOwnerLayoutState schedulerSchedulerAppointmentOwnerLayoutState)
		{
			if (_schedulerSchedulerAppointmentOwnerLayoutState != schedulerSchedulerAppointmentOwnerLayoutState)
			{
				if (_schedulerSchedulerAppointmentOwnerLayoutState != null)
				{
					_schedulerSchedulerAppointmentOwnerLayoutState.SetHovered(false);
				}
				_schedulerSchedulerAppointmentOwnerLayoutState = schedulerSchedulerAppointmentOwnerLayoutState;
				if (_schedulerSchedulerAppointmentOwnerLayoutState != null)
				{
					_schedulerSchedulerAppointmentOwnerLayoutState.SetHovered(true);
					SetCursor(null);
				}
				Invalidate(invalidateChildren: true);
			}
		}

		private void LayoutMonthViewAppointments()
		{
			if (!base.Created)
			{
				return;
			}
			int rowHeight = _actualRowHeight;
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				item.SetHasHiddenAppointments(false);
			}
			foreach (SchedulerAppointmentView staleView in _appointmentViews)
			{
				staleView._bounds = Rectangle.Empty;
				staleView.StopTimerAndHideToolTip();
			}
			ResetPlacedViewIndex();
			if (_isAppointmentCacheStale)
			{
				DateRange dateRange = new DateRange(_dayLayouts.GetLayoutAt(0).GetDate(), _dayLayouts.GetLayoutAt(_dayLayouts.Count - 1).GetDate());
				if (IsMultiOwnerActive() || CurrentOwner != null)
				{
					_visibleAppointments = _publicAppointments.GetAppointmentsForOwner(dateRange, true, _currentOwner);
				}
				else
				{
					_visibleAppointments = _publicAppointments.GetVisibleAppointmentsInRange(dateRange, true);
				}
				_backgroundAppointments = null;
				_isAppointmentCacheStale = false;
			}
			else
			{
				SortVisibleAppointments();
			}
			foreach (SchedulerAppointment appointment in _visibleAppointments)
			{
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = _dayLayouts.FindLayoutByDate(appointment.StartTime.Date);
				if (schedulerHourRangeCollectionLayoutState == null || appointment.StartTime.Date < _dayLayouts.GetLayoutAt(0).GetDate())
				{
					schedulerHourRangeCollectionLayoutState = _dayLayouts.GetLayoutAt(0);
				}
				int left = schedulerHourRangeCollectionLayoutState._bounds.Left;
				int top = schedulerHourRangeCollectionLayoutState._bounds.Top + rowHeight + 4;
				SchedulerAppointmentView appointmentView = GetMonthAppointmentView(left, top, schedulerHourRangeCollectionLayoutState, appointment, MonthFont, false);
				if (appointmentView == null)
				{
					continue;
				}
				appointmentView._bounds = new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left + 4, appointmentView._rowTop, 0, rowHeight);
				if (appointmentView._date == appointment.StartTime.Date)
				{
					appointmentView.UpdateFirstSegment(true);
				}
				else
				{
					appointmentView.SetContinuedFromPrevious(true);
				}
				LayoutMonthAppointmentSpan(appointmentView, schedulerHourRangeCollectionLayoutState);
				if (_appointmentEditor != null && _appointmentEditor._app == appointmentView.GetAppointment())
				{
					using (Graphics graphics = CreateGraphics())
					{
						appointmentView.RenderContent(graphics, _appointmentEditor);
					}
				}
			}
			RemoveUnusedAppointmentRenderers();
			SetNextViewIndex(0);
			UpdateHiddenAppointmentButtons();
			_isAppointmentLayoutStale = false;
		}

		private void LayoutMonthAppointmentSpan(SchedulerAppointmentView appointmentView, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (_view == SchedulerView.MonthView && _compressWeekEnds)
			{
				LayoutCompressedWeekendSpan(appointmentView, schedulerHourRangeCollectionLayoutState);
				return;
			}
			int nextCellIndex = schedulerHourRangeCollectionLayoutState.GetDayIndex();
			int width = schedulerHourRangeCollectionLayoutState._bounds.Width;
			DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate();
			int lastColumnIndex = 7;
			if (appointmentView.GetAppointment().StartTime.Date < DateRange.Start)
			{
				appointmentView.UpdateFirstSegment(false);
			}
			if (appointmentView.GetAppointment().StartTime.Date != appointmentView.GetAppointment().EndTime.Date)
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
							LayoutMonthAppointmentSpan(schedulerRenderer4Argument, _dayLayouts.GetLayoutAt(nextCellIndex));
						}
						break;
					}
					columnIndex++;
					nextCellIndex++;
					dateTime = dateTime.AddDays(1.0);
					Rectangle field_ = appointmentView._bounds;
					field_.Width += width;
					appointmentView._bounds = field_;
					if (_appointmentEditor != null && _appointmentEditor._app == appointmentView.GetAppointment())
					{
						using (Graphics graphics = CreateGraphics())
						{
							appointmentView.RenderContent(graphics, _appointmentEditor);
						}
					}
				}
				while (dateTime < appointmentView.GetAppointment().EndTime);
			}
			else
			{
				appointmentView._bounds.Width = width;
			}
			appointmentView._bounds.Width -= 8;
			if (appointmentView.GetAppointment().EndTime.Date > DateRange.End.Date.AddDays(1.0))
			{
				appointmentView.UpdateLastSegment(false);
			}
		}

		private void UpdateRowHeight()
		{
			if (_rowHeight <= 0)
			{
				Font font = null;
				font = ((_view == SchedulerView.DayView) ? DayFont : ((_view != SchedulerView.WeekView) ? MonthFont : WeekFont));
				if (font != null)
				{
					_defaultRowHeight = (int)font.GetHeight() + 6;
					if (_defaultRowHeight <= 18)
					{
						_defaultRowHeight += 3;
					}
				}
				else
				{
					_defaultRowHeight = 21;
				}
				ImageList imageList = GetImageList();
				if (imageList != null && _defaultRowHeight < imageList.ImageSize.Height + 5)
				{
					_defaultRowHeight = imageList.ImageSize.Height + 5;
				}
			}
			else
			{
				_defaultRowHeight = _rowHeight;
			}
			_actualRowHeight = _defaultRowHeight;
			if (_view != SchedulerView.DayView)
			{
				return;
			}
			int totalSlots = (int)_interval * (DayEndHour - DayStartHour);
			if (_dayAreaTop == 0)
			{
				if (!_showAllDayArea)
				{
					_dayAreaTop = _defaultRowHeight;
					if (IsMultiOwnerActive() && _showOwnersHeader)
					{
						_dayAreaTop += _defaultRowHeight;
					}
				}
				else
				{
					_dayAreaTop = _actualRowHeight * 2 + 2;
					if (IsMultiOwnerActive() && _showOwnersHeader)
					{
						_dayAreaTop += _actualRowHeight;
					}
				}
			}
			if (_actualRowHeight * totalSlots < base.ClientRectangle.Height - _dayAreaTop && _rowHeight <= 0)
			{
				_actualRowHeight = (base.ClientRectangle.Height - _dayAreaTop) / totalSlots;
			}
		}

		private SchedulerAppointmentOwner FindVisibleOwnerByValue(object value)
		{
			foreach (SchedulerAppointmentOwner item in _visibleOwners)
			{
				if (item.ValueEquals(value))
				{
					return item;
				}
			}
			return null;
		}

		private SchedulerAppointment GetAdjacentAppointment(SchedulerAppointment schedulerAppointment, bool isForward, DateTime dateTime)
		{
			if (schedulerAppointment == null)
			{
				if (dateTime != DateTime.MinValue)
				{
					if (isForward)
					{
						foreach (SchedulerAppointment item in _visibleAppointments)
						{
							if (item.StartTime.Date >= dateTime.Date && item.StartTime.TimeOfDay >= dateTime.TimeOfDay)
							{
								return item;
							}
						}
					}
					else
					{
						TimeSpan timeOfDay = dateTime.TimeOfDay;
						SchedulerAppointment bestMatch = null;
						foreach (SchedulerAppointment candidate in _visibleAppointments)
						{
							if (candidate.StartTime.Date >= dateTime.Date && candidate.StartTime.TimeOfDay <= timeOfDay)
							{
								bestMatch = candidate;
								timeOfDay = candidate.StartTime.TimeOfDay;
							}
						}
						if (bestMatch != null)
						{
							return bestMatch;
						}
					}
				}
				if (isForward)
				{
					return (SchedulerAppointment)_visibleAppointments[0];
				}
				return (SchedulerAppointment)_visibleAppointments[_visibleAppointments.Count - 1];
			}
			SchedulerAppointment result = null;
			SchedulerAppointment listAppointment = null;
			for (int i = 0; i < _visibleAppointments.Count; i++)
			{
				listAppointment = (SchedulerAppointment)_visibleAppointments[i];
				if (schedulerAppointment == listAppointment)
				{
					if (!isForward)
					{
						return result;
					}
					if (i <= _visibleAppointments.Count - 2)
					{
						return (SchedulerAppointment)_visibleAppointments[i + 1];
					}
					SelectFirstOrLastAppointment(isForward);
					return null;
				}
				result = listAppointment;
			}
			if (dateTime != DateTime.MinValue)
			{
				SelectFirstOrLastAppointment(isForward);
			}
			return null;
		}

		private void RefreshAppointmentFromDataSource(int dataIndex)
		{
			SchedulerAppointment schedulerAppointment = FindAppointmentByDataIndex(dataIndex);
			if (schedulerAppointment == null || _schedulerPropertyDescriptor == null)
			{
				return;
			}
			DateTime oldStart = schedulerAppointment._startTime;
			DateTime oldEnd = schedulerAppointment._endTime;
			bool wasShownCandidate = schedulerAppointment._recurrenceState == RecurrenceState.NotRecurring && schedulerAppointment.RecurrencePattern == null;
			object owner = schedulerAppointment._ownerValue;
			DateTime dateTime = schedulerAppointment._startTime;
			DateTime endTime = schedulerAppointment._endTime;
			string text = schedulerAppointment._text;
			string description = schedulerAppointment._description;
			object value = _schedulerPropertyDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
			if (value != null && value != DBNull.Value)
			{
				dateTime = (DateTime)value;
			}
			if (_endTimeDescriptor != null)
			{
				object endValue = _endTimeDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
				if (endValue != null && endValue != DBNull.Value)
				{
					endTime = (DateTime)endValue;
				}
			}
			if (_textDescriptor != null)
			{
				object textValue = _textDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
				if (textValue != null && textValue != DBNull.Value)
				{
					text = (string)textValue;
				}
			}
			if (_descriptionDescriptor != null)
			{
				object descriptionValue = _descriptionDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
				if (descriptionValue != null && descriptionValue != DBNull.Value)
				{
					description = (string)descriptionValue;
				}
			}
			if (_propertyDescriptor != null)
			{
				owner = _propertyDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
				if (owner == DBNull.Value)
				{
					owner = null;
				}
			}
			schedulerAppointment._notifiesScheduler = false;
			if (_recurrenceInfoDescriptor != null)
			{
				object recurrenceValue = _recurrenceInfoDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
				if (recurrenceValue != null && recurrenceValue.GetType() == typeof(string))
				{
					string recurrenceText = (string)recurrenceValue;
					if (recurrenceText.Length > 0)
					{
						try
						{
							schedulerAppointment.RecurrencePatternInfo = recurrenceText;
						}
						catch
						{
							schedulerAppointment.RecurrencePattern = null;
						}
					}
				}
			}
			if (_appointmentSettingsDescriptor != null)
			{
				object settingsValue = _appointmentSettingsDescriptor.GetValue(_schedulerCurrencyManager.List[dataIndex]);
				if (settingsValue != null && settingsValue.GetType() == typeof(string))
				{
					string settingsText = (string)settingsValue;
					if (settingsText.Length > 0)
					{
						try
						{
							schedulerAppointment.SettingsInfo = (string)settingsValue;
						}
						catch
						{
						}
					}
				}
			}
			schedulerAppointment._notifiesScheduler = true;
			schedulerAppointment.ApplyFieldValues(dateTime, endTime, text, owner, description);
			if (FilterConditionApplied != null)
			{
				if (schedulerAppointment.Scheduler == null)
				{
					schedulerAppointment._scheduler = this;
				}
				if (!FilterConditionApplied.IsAppointmentAccepted(schedulerAppointment))
				{
					if (_publicAppointments.Contains(schedulerAppointment))
					{
						schedulerAppointment._isRemovalSilent = true;
						_publicAppointments.RemoveWithoutNotify(schedulerAppointment);
						schedulerAppointment._scheduler = this;
						schedulerAppointment._isRemovalSilent = false;
					}
				}
				else if (!_publicAppointments.Contains(schedulerAppointment))
				{
					_publicAppointments.AddUnchecked(schedulerAppointment);
					schedulerAppointment._scheduler = null;
				}
			}
			if (wasShownCandidate && FilterConditionApplied == null && schedulerAppointment._recurrenceState == RecurrenceState.NotRecurring && schedulerAppointment.RecurrencePattern == null && IsOutsideShownDates(oldStart, oldEnd) && IsOutsideShownDates(schedulerAppointment._startTime, schedulerAppointment._endTime))
			{
				return;
			}
			_isAppointmentCacheStale = true;
			_isAppointmentLayoutStale = true;
			Invalidate();
		}

		// True when a span of time lies outside the days on screen, so a change to it cannot alter the display.
		private bool IsOutsideShownDates(DateTime start, DateTime end)
		{
			if (_dayLayouts == null || _dayLayouts.Count == 0)
			{
				return false;
			}
			DateTime first = DateTime.MaxValue;
			DateTime last = DateTime.MinValue;
			foreach (SchedulerHourRangeCollectionLayoutState layout in _dayLayouts)
			{
				DateTime date = layout.GetDate();
				first = date < first ? date : first;
				last = date > last ? date : last;
			}
			return end < first.AddDays(-1.0) || start >= last.AddDays(2.0);
		}

		internal void RebuildFilteredAppointments()
		{
			_publicAppointments.UpdateSchedulerAppointmentCollection();
			foreach (SchedulerAppointment item in _appointments)
			{
				if (item._recurrenceState == RecurrenceState.Exception)
				{
					continue;
				}
				if (item.Scheduler == null)
				{
					item._scheduler = this;
				}
				if (FilterConditionApplied != null)
				{
					if (FilterConditionApplied.IsAppointmentAccepted(item))
					{
						_publicAppointments.AddUnchecked(item);
					}
				}
				else
				{
					_publicAppointments.AddUnchecked(item);
				}
			}
			ResetReminders();
			_isAppointmentCacheStale = true;
			_isFullLayoutStale = true;
			Invalidate();
		}

		internal void UpdateCurrentAppointment(SchedulerAppointment appointment)
		{
			if (_schedulerSelectedAppointments != null && _schedulerSelectedAppointments.Contains(appointment))
			{
				if (_currentAppointment == appointment)
				{
					_currentAppointment = null;
				}
				appointment._selected = false;
				_schedulerSelectedAppointments.Remove(appointment);
			}
		}

		internal void UpdateSchedulerSelectedAppointments(SchedulerAppointment appointment)
		{
			if (_schedulerSelectedAppointments == null)
			{
				_schedulerSelectedAppointments = new SchedulerSelectedAppointments(this);
			}
			_schedulerSelectedAppointments.Add(appointment);
		}

		internal SchedulerAppointmentOwnerLayoutState FindDaySlotLayoutAt(int x, int integerArgument)
		{
			if (_view == SchedulerView.DayView)
			{
				Rectangle empty = Rectangle.Empty;
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					empty = item._bounds;
					empty.Width += empty.Left;
					empty.X = 0;
					if (!empty.Contains(x, integerArgument))
					{
						continue;
					}
					foreach (SchedulerAppointmentOwnerLayoutState slotLayout in item.GetTimeSlots())
					{
						empty = slotLayout._bounds;
						empty.Width += empty.Left;
						empty.X = 0;
						if (empty.Contains(x, integerArgument))
						{
							return slotLayout;
						}
					}
					return null;
				}
				return null;
			}
			return null;
		}

		internal void InvalidateAppointmentLayout()
		{
			InvalidateAppointmentStyles();
			_isFullLayoutStale = true;
			if (_schedulerOwner != null)
			{
				_schedulerOwner.CloseEditorIfOpen();
				_schedulerOwner = null;
			}
			foreach (SchedulerAppointmentView item in _allDayAppointmentViews)
			{
				item._bounds = Rectangle.Empty;
			}
			if (_view == SchedulerView.DayView && _allDayAreaControl != null)
			{
				_allDayAreaControl._isLayoutStale = true;
			}
			Invalidate(invalidateChildren: true);
		}
	}
}
