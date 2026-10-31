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
		private void UpdateRowHeight(Graphics graphics)
		{
			Font font = GetViewFont();
			_rowHeight = (int)font.GetHeight(graphics) + 3;
		}

		private SchedulerAppointmentOwnerLayoutState FindDateHeaderLayout(SchedulerAllDayArea headerControl, DateTime dateTime)
		{
			foreach (SchedulerAppointmentOwnerLayoutState item in headerControl._dateHeaders)
			{
				if (item.GetDate().Date == dateTime)
				{
					return item;
				}
			}
			return null;
		}

		private void LayoutCompressedMonthAppointment(SchedulerAppointmentView appointmentRenderer, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			int dayIndex = schedulerHourRangeCollectionLayoutState.GetDayIndex();
			int width = schedulerHourRangeCollectionLayoutState._bounds.Width;
			DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate();
			int lastColumn = 6;
			if (_scheduler.FirstDayOfWeek == SchedulerDayOfWeek.Monday)
			{
				lastColumn++;
			}
			bool isFirstDay = true;
			if (appointmentRenderer.GetAppointment().StartTime.Date != appointmentRenderer.GetAppointment().EndTime.Date)
			{
				int columnNumber = schedulerHourRangeCollectionLayoutState.GetColumnIndex();
				do
				{
					if (columnNumber > lastColumn)
					{
						appointmentRenderer.UpdateLastSegment(false);
						if (dayIndex < _dayLayouts.Count)
						{
							SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = _dayLayouts.GetLayoutAt(dayIndex);
							int left = schedulerHourRangeCollectionLayoutStateArgument._bounds.Left;
							int nextDayTop = schedulerHourRangeCollectionLayoutStateArgument._bounds.Top + _rowHeight + 4;
							SchedulerAppointmentView schedulerRenderer4Argument = CreateAppointmentRenderer(left, nextDayTop, schedulerHourRangeCollectionLayoutStateArgument, appointmentRenderer.GetAppointment(), GetViewFont(), false);
							if (schedulerRenderer4Argument != null)
							{
								Rectangle bounds = _dayLayouts.GetLayoutAt(dayIndex)._bounds;
								schedulerRenderer4Argument._bounds = new Rectangle(bounds.Left + 4, schedulerRenderer4Argument._rowTop, 0, _rowHeight);
								schedulerRenderer4Argument.UpdateFirstSegment(false);
								LayoutCompressedMonthAppointment(schedulerRenderer4Argument, _dayLayouts.GetLayoutAt(dayIndex));
							}
						}
						break;
					}
					if (dayIndex >= _dayLayouts.Count)
					{
						break;
					}
					if ((dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday) && !isFirstDay)
					{
						SchedulerHourRangeCollectionLayoutState weekendDayLayout = _dayLayouts.GetLayoutAt(dayIndex);
						if (dateTime.DayOfWeek == DayOfWeek.Saturday)
						{
							if (appointmentRenderer._rowTop + _rowHeight <= weekendDayLayout._bounds.Bottom)
							{
								appointmentRenderer._bounds.Width += width;
							}
							else
							{
								weekendDayLayout.SetHasHiddenAppointments(true);
								appointmentRenderer.UpdateLastSegment(false);
								appointmentRenderer.SetContinuedToNext(true);
							}
						}
						else
						{
							int weekendLeft = weekendDayLayout._bounds.Left;
							int weekendTop = weekendDayLayout._bounds.Top + _rowHeight + 4;
							SchedulerAppointmentView weekendRenderer = CreateAppointmentRenderer(weekendLeft, weekendTop, weekendDayLayout, appointmentRenderer.GetAppointment(), MonthFont, true);
							if (weekendRenderer != null)
							{
								weekendRenderer._bounds = new Rectangle(weekendDayLayout._bounds.Left + 4, weekendRenderer._rowTop, width - 8, _rowHeight);
								weekendRenderer.UpdateFirstSegment(false);
								appointmentRenderer.UpdateLastSegment(false);
								if (weekendDayLayout.GetDate().AddDays(1.0) < appointmentRenderer.GetAppointment().EndTime.Date)
								{
									weekendRenderer.UpdateLastSegment(false);
								}
								if (weekendDayLayout.GetDate() == appointmentRenderer.GetAppointment().StartTime.Date)
								{
									weekendRenderer.UpdateFirstSegment(true);
								}
							}
							else
							{
								appointmentRenderer.UpdateLastSegment(false);
								appointmentRenderer.SetContinuedToNext(true);
							}
						}
					}
					else if (dateTime.DayOfWeek == DayOfWeek.Monday && !isFirstDay)
					{
						SchedulerHourRangeCollectionLayoutState mondayLayout = _dayLayouts.GetLayoutAt(dayIndex);
						int mondayLeft = mondayLayout._bounds.Left;
						int mondayTop = mondayLayout._bounds.Top + _rowHeight + 4;
						SchedulerAppointmentView mondayRenderer = CreateAppointmentRenderer(mondayLeft, mondayTop, mondayLayout, appointmentRenderer.GetAppointment(), GetViewFont(), true);
						if (mondayRenderer != null)
						{
							mondayRenderer._bounds = new Rectangle(mondayLayout._bounds.Left + 4, appointmentRenderer._rowTop, 0, _rowHeight);
							mondayRenderer.UpdateFirstSegment(false);
							appointmentRenderer.UpdateLastSegment(false);
							appointmentRenderer.SetContinuedToNext(false);
							LayoutCompressedMonthAppointment(mondayRenderer, mondayLayout);
							break;
						}
						appointmentRenderer.UpdateLastSegment(false);
						appointmentRenderer.SetContinuedToNext(true);
					}
					else
					{
						appointmentRenderer._bounds.Width += width;
					}
					isFirstDay = false;
					columnNumber++;
					dayIndex++;
					dateTime = dateTime.AddDays(1.0);
				}
				while (dateTime < appointmentRenderer.GetAppointment().EndTime);
			}
			else
			{
				appointmentRenderer._bounds.Width += width;
			}
			appointmentRenderer._bounds.Width -= 8;
		}

		private void LayoutMonthAppointment(SchedulerAppointmentView appointmentRenderer, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (_scheduler.View == SchedulerView.MonthView && _isWeekendCompressed)
			{
				LayoutCompressedMonthAppointment(appointmentRenderer, schedulerHourRangeCollectionLayoutState);
				return;
			}
			int dayIndex = schedulerHourRangeCollectionLayoutState.GetDayIndex();
			int width = schedulerHourRangeCollectionLayoutState._bounds.Width;
			DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate();
			int lastColumn = 7;
			if (appointmentRenderer.GetAppointment().StartTime.Date != appointmentRenderer.GetAppointment().EndTime.Date)
			{
				int columnNumber = schedulerHourRangeCollectionLayoutState.GetColumnIndex();
				do
				{
					if (columnNumber > lastColumn)
					{
						if (dayIndex < _dayLayouts.Count)
						{
							appointmentRenderer.UpdateLastSegment(false);
							SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = _dayLayouts.GetLayoutAt(dayIndex);
							int left = schedulerHourRangeCollectionLayoutStateArgument._bounds.Left;
							int nextDayTop = schedulerHourRangeCollectionLayoutStateArgument._bounds.Top + _rowHeight + 4;
							SchedulerAppointmentView schedulerRenderer4Argument = CreateAppointmentRenderer(left, nextDayTop, schedulerHourRangeCollectionLayoutStateArgument, appointmentRenderer.GetAppointment(), GetViewFont(), false);
							if (schedulerRenderer4Argument != null)
							{
								Rectangle bounds = _dayLayouts.GetLayoutAt(dayIndex)._bounds;
								schedulerRenderer4Argument._bounds = new Rectangle(bounds.Left + 4, schedulerRenderer4Argument._rowTop, 0, _rowHeight);
								schedulerRenderer4Argument.UpdateFirstSegment(false);
								LayoutMonthAppointment(schedulerRenderer4Argument, _dayLayouts.GetLayoutAt(dayIndex));
							}
						}
						break;
					}
					columnNumber++;
					dayIndex++;
					dateTime = dateTime.AddDays(1.0);
					Rectangle field_ = appointmentRenderer._bounds;
					field_.Width += width;
					appointmentRenderer._bounds = field_;
				}
				while (dateTime < appointmentRenderer.GetAppointment().EndTime);
			}
			else
			{
				appointmentRenderer._bounds.Width = width;
			}
			appointmentRenderer._bounds.Width -= 8;
		}

		private void LayoutOwnerAllDayAppointment(SchedulerAppointmentView appointmentRenderer, DateTime dateTimeArgument, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			int dayWidth = _dayColumnWidth;
			int appointmentWidth = 0;
			int appointmentLeft = _dayColumnsLeft + 5;
			SchedulerAppointment schedulerAppointment = appointmentRenderer.GetAppointment();
			DateTime endTime = schedulerAppointment.EndTime;
			bool isFirstDay = false;
			bool isLastDay = false;
			foreach (SchedulerHourRangeCollectionLayoutState item in _scheduler._dayLayouts)
			{
				if (DayViewPrintMode == DayViewPrintMode.PagePerDay && schedulerHourRangeCollectionLayoutState != null && item != schedulerHourRangeCollectionLayoutState)
				{
					continue;
				}
				if (item.GetOwner().ValueEquals(schedulerAppointment.Owner))
				{
					DateTime dateTime = item.GetDate();
					isFirstDay = ((dateTime == schedulerAppointment.StartTime.Date) ? true : false);
					isLastDay = ((dateTime <= endTime && dateTime.AddDays(1.0) >= endTime) ? true : false);
					if (dateTimeArgument <= dateTime && endTime > dateTime)
					{
						appointmentWidth += dayWidth;
						if (appointmentWidth > 0)
						{
							PositionAllDayAppointment(appointmentRenderer, appointmentWidth, isFirstDay, isLastDay, appointmentLeft);
							SchedulerAppointmentView schedulerRenderer4Argument = CreateAllDayRenderer(schedulerAppointment);
							if (schedulerRenderer4Argument != null && item.GetDate() < _scheduler.Dates[_scheduler.Dates.Count - 1].Date && item.GetDate() < schedulerAppointment._endTime.Date)
							{
								LayoutOwnerAllDayAppointment(schedulerRenderer4Argument, item.GetDate().AddDays(1.0), schedulerHourRangeCollectionLayoutState);
							}
							break;
						}
					}
					else
					{
						appointmentLeft += dayWidth + 5;
					}
				}
				else
				{
					appointmentLeft += dayWidth + 5;
				}
			}
		}

		private void LayoutAllDayAppointment(SchedulerAppointmentView appointmentRenderer, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (_scheduler.IsMultiOwnerActive())
			{
				LayoutOwnerAllDayAppointment(appointmentRenderer, appointmentRenderer.GetAppointment()._startTime.Date, schedulerHourRangeCollectionLayoutState);
				return;
			}
			int dayWidth = _dayColumnWidth;
			int appointmentWidth = 0;
			int appointmentLeft = _dayColumnsLeft + 5;
			SchedulerAppointment schedulerAppointment = appointmentRenderer.GetAppointment();
			_ = schedulerAppointment.EndTime;
			_ = schedulerAppointment.StartTime.Date;
			if (_dayViewPrintMode == DayViewPrintMode.PagePerView)
			{
				foreach (DateTime date in _scheduler.Dates)
				{
					if (schedulerAppointment.StartTime.Date <= date && schedulerAppointment.EndTime > date)
					{
						appointmentWidth += dayWidth;
					}
					else if (schedulerAppointment.StartTime.Date > date)
					{
						appointmentLeft += dayWidth;
					}
				}
			}
			else
			{
				appointmentWidth += dayWidth;
			}
			appointmentWidth -= 10;
			int searchTop = _rowHeight + _marginBounds.Top + 5;
			_allDayAppointmentTop = FindFreeAllDayTop(appointmentLeft, searchTop);
			appointmentRenderer._bounds = new Rectangle(appointmentLeft, _allDayAppointmentTop, appointmentWidth, _rowHeight - 2);
			if (_allDayAppointmentTop > _timeGridTop - _rowHeight)
			{
				_timeGridTop = _allDayAppointmentTop + _rowHeight;
				_extraAllDayRowCount++;
			}
		}

		private void PlaceAppointmentInLane(SchedulerAppointment[] schedulerAppointmentArray, SchedulerAppointment schedulerAppointment, DateTime dateTime, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			int durationMinutes = (int)schedulerAppointment.GetDurationOnDate(dateTime).TotalMinutes;
			int startMinutes = (int)schedulerAppointment.GetStartTimeOnDate(dateTime).TotalMinutes;
			int startSlot = startMinutes / _minutesPerSlot;
			int visibleStartOffset = (int)StartTime.TotalMinutes / _minutesPerSlot * _rowHeight;
			int slotCount = (startMinutes % _minutesPerSlot + durationMinutes) / _minutesPerSlot;
			int gridTop = _timeGridTop;
			if ((startMinutes % _minutesPerSlot + durationMinutes) % _minutesPerSlot != 0)
			{
				slotCount++;
			}
			if (slotCount == 0)
			{
				slotCount++;
			}
			if (startSlot + slotCount > schedulerAppointmentArray.Length)
			{
				slotCount = schedulerAppointmentArray.Length - startSlot;
			}
			if (AreSlotsFree(schedulerAppointmentArray, schedulerAppointment, dateTime))
			{
				for (int i = startSlot; i < startSlot + slotCount; i++)
				{
					schedulerAppointmentArray.SetValue(schedulerAppointment, i);
				}
				schedulerAppointment._laneIndex = _appointmentLanes.IndexOf(schedulerAppointmentArray);
				schedulerAppointment.SetLayoutTop(startSlot * _rowHeight - visibleStartOffset + gridTop);
				schedulerAppointment.SetLayoutHeight(slotCount * _rowHeight);
				AddDayAppointmentView(schedulerAppointment, dateTime, startSlot, slotCount, schedulerHourRangeCollectionLayoutState);
			}
			else
			{
				int columnIndex = _appointmentLanes.IndexOf(schedulerAppointmentArray);
				if (columnIndex == _appointmentLanes.Count - 1)
				{
					SchedulerAppointment[] newColumn = new SchedulerAppointment[schedulerAppointmentArray.Length];
					_appointmentLanes.Add(newColumn);
					PlaceAppointmentInLane(newColumn, schedulerAppointment, dateTime, schedulerHourRangeCollectionLayoutState);
				}
				else
				{
					PlaceAppointmentInLane((SchedulerAppointment[])_appointmentLanes[columnIndex + 1], schedulerAppointment, dateTime, schedulerHourRangeCollectionLayoutState);
				}
			}
		}

		private void LayoutDayAppointments(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			try
			{
				int slotsPerDay = (int)_interval * 24;
				_appointmentRenderers = new ArrayList();
				_minutesPerSlot = 60 / (int)_interval;
				_appointmentLanes = new ArrayList();
				_currentDayLane = new SchedulerAppointment[slotsPerDay];
				_appointmentLanes.Add(_currentDayLane);
				ArrayList arrayList = (_scheduler.IsMultiOwnerActive() ? _scheduler._appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetOwner()).GetAppointments() : ((_scheduler.CurrentOwner != null) ? _scheduler._appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutState.GetDate(), _scheduler.CurrentOwner).GetAppointments() : _scheduler._appointmentsByOwnerDate.FindOwnerStateByDate(schedulerHourRangeCollectionLayoutState.GetDate()).GetAppointments()));
				foreach (SchedulerAppointment item in arrayList)
				{
					if ((!_scheduler.ShowAllDayArea || item.GetDurationInHours() < 24) && item.StartTime.Date <= schedulerHourRangeCollectionLayoutState.GetDate())
					{
						PlaceAppointmentInLane(_currentDayLane, item, schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState);
					}
				}
				foreach (SchedulerAppointmentView renderer in _appointmentRenderers)
				{
					if (!(renderer._date != schedulerHourRangeCollectionLayoutState.GetDate()) && renderer.GetSchedulerAppointmentOwner() == schedulerHourRangeCollectionLayoutState.GetOwner())
					{
						SchedulerAppointment appointment = renderer.GetAppointment();
						int left = 0;
						int width = 0;
						int lastColumnIndex = appointment._laneIndex;
						for (int i = appointment._laneIndex + 1; i < _appointmentLanes.Count && AreSlotsFree((SchedulerAppointment[])_appointmentLanes[i], appointment, schedulerHourRangeCollectionLayoutState.GetDate()); i++)
						{
							lastColumnIndex = i;
						}
						width = schedulerHourRangeCollectionLayoutState._bounds.Width + 5;
						left = schedulerHourRangeCollectionLayoutState._bounds.Left - 5;
						int columnWidth = width / _appointmentLanes.Count;
						width = columnWidth * (lastColumnIndex - appointment._laneIndex + 1) - 5;
						left += columnWidth * appointment._laneIndex;
						renderer._bounds = new Rectangle(left, appointment.GetLayoutTop(), width, appointment.GetLayoutHeight());
						schedulerHourRangeCollectionLayoutState.SetLaneCount(_appointmentLanes.Count);
					}
				}
			}
			catch
			{
			}
		}

		private bool LayoutAllDayAppointments(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			_extraAllDayRowCount = 0;
			if (_allDayRenderers != null && _allDayRenderers.Count > 0)
			{
				int rendererIndex;
				for (rendererIndex = 0; rendererIndex < _allDayRenderers.Count; rendererIndex++)
				{
					SchedulerAppointmentView staleRenderer = (SchedulerAppointmentView)_allDayRenderers[rendererIndex];
					_allDayRenderers.Remove(staleRenderer);
					staleRenderer.StopTimerAndHideToolTip();
					staleRenderer = null;
					rendererIndex--;
				}
			}
			_allDayRenderers = new ArrayList();
			foreach (SchedulerAppointment item in _scheduler._allDayAppointments)
			{
				if (item.GetDurationInHours() >= 24 && (!_scheduler.IsMultiOwnerActive() || schedulerHourRangeCollectionLayoutState == null || schedulerHourRangeCollectionLayoutState.GetOwner().ValueEquals(item.Owner)) && (schedulerHourRangeCollectionLayoutState == null || (item.StartTime.Date <= schedulerHourRangeCollectionLayoutState.GetDate() && item.EndTime > schedulerHourRangeCollectionLayoutState.GetDate())))
				{
					SchedulerAppointmentView schedulerRenderer4Argument = CreateAllDayRenderer(item);
					if (schedulerRenderer4Argument != null)
					{
						LayoutAllDayAppointment(schedulerRenderer4Argument, schedulerHourRangeCollectionLayoutState);
					}
				}
			}
			if (_extraAllDayRowCount > 0)
			{
				LayoutDayColumn(graphics, schedulerHourRangeCollectionLayoutState);
				return true;
			}
			return false;
		}

		private void LayoutMonthAppointments()
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				item.SetHasHiddenAppointments(false);
			}
			_appointmentRenderers = new ArrayList();
			foreach (SchedulerAppointment appointment in _scheduler._visibleAppointments)
			{
				foreach (SchedulerHourRangeCollectionLayoutState dayLayout in _dayLayouts)
				{
					DateTime dateTime = appointment.EndTime;
					if (dateTime > appointment.StartTime && dateTime.TimeOfDay.TotalHours == 0.0)
					{
						dateTime = dateTime.Subtract(new TimeSpan(24, 0, 0));
					}
					if (!(dayLayout.GetDate() >= appointment._startTime.Date) || !(dayLayout.GetDate() <= dateTime))
					{
						continue;
					}
					int left = dayLayout._bounds.Left;
					int top = dayLayout._bounds.Top + _rowHeight + 4;
					SchedulerAppointmentView appointmentRenderer = CreateAppointmentRenderer(left, top, dayLayout, appointment, GetViewFont(), false);
					if (appointmentRenderer != null)
					{
						appointmentRenderer._bounds = new Rectangle(dayLayout._bounds.Left + 4, appointmentRenderer._rowTop, dayLayout._bounds.Width - 8, _rowHeight);
						if (dayLayout.GetDate() != appointment.StartTime.Date)
						{
							appointmentRenderer.UpdateFirstSegment(false);
						}
						if (dayLayout.GetDate() != dateTime.Date)
						{
							appointmentRenderer.UpdateLastSegment(false);
						}
					}
				}
			}
		}
	}
}
