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
		private void DrawAppointmentStrips(Graphics graphics, Rectangle rectangleArgument)
		{
			if (IsMultiOwnerActive())
			{
				DrawOwnerAppointmentStrips(graphics, rectangleArgument);
				return;
			}
			float appointmentTop = base.ClientRectangle.Top;
			ArrayList arrayList = _allDayAppointments;
			if (!ShowAllDayArea && _dayLayouts.Count > 1)
			{
				if (_backgroundAppointments == null)
				{
					DateRange dateRange = new DateRange(_dayLayouts.GetLayoutAt(0).GetDate(), _dayLayouts.GetLayoutAt(_dayLayouts.Count - 1).GetDate());
					_backgroundAppointments = (IsMultiOwnerActive() || CurrentOwner == null) ? _publicAppointments.GetVisibleAppointmentsInRange(dateRange, false) : _publicAppointments.GetAppointmentsForOwner(dateRange, false, _currentOwner);
				}
				arrayList = _backgroundAppointments;
			}
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				if (item._bounds.Left < _dayAreaLeft)
				{
					continue;
				}
				Rectangle a = new Rectangle(item._bounds.Left - 4, item._bounds.Top, 4, item._bounds.Height);
				Rectangle rectangle = Rectangle.Intersect(a, rectangleArgument);
				appointmentTop = item._bounds.Top;
				if (rectangle.IsEmpty)
				{
					continue;
				}
				Brush brush = GetStripBackgroundBrush();
				if (brush != null)
				{
					graphics.FillRectangle(brush, a.Left, a.Top, a.Width, a.Height);
				}
				DateTime dateTime = item.GetDate().Add(_firstVisibleSlotTime);
				foreach (SchedulerAppointment backgroundAppointment in arrayList)
				{
					if (backgroundAppointment.StartTime.Date > item.GetDate() || dateTime.Date > item.GetDate())
					{
						break;
					}
					if (backgroundAppointment.EndTime > dateTime && !backgroundAppointment._isBeingMoved)
					{
						float durationInSlots = (float)backgroundAppointment.Duration.TotalMinutes / (float)_minuteInterval;
						if (backgroundAppointment.StartTime < dateTime)
						{
							durationInSlots -= (float)dateTime.Subtract(backgroundAppointment.StartTime).TotalMinutes / (float)_minuteInterval;
						}
						float height = durationInSlots * (float)_actualRowHeight;
						if (backgroundAppointment.StartTime > dateTime)
						{
							appointmentTop += (float)backgroundAppointment.StartTime.Subtract(dateTime).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight;
						}
						RectangleF rect = new RectangleF(a.Left, appointmentTop, a.Width, height);
						if (rect.Bottom > (float)base.Bottom)
						{
							rect.Height = (float)base.Bottom - rect.Top;
						}
						brush = GetAppointmentBorderBrush(backgroundAppointment);
						if (brush != null)
						{
							graphics.FillRectangle(brush, rect);
						}
						appointmentTop = (float)backgroundAppointment.EndTime.TimeOfDay.Subtract(_firstVisibleSlotTime).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight + (float)_dayAreaTop + (float)base.ClientRectangle.Top;
						dateTime = backgroundAppointment.EndTime;
					}
				}
				foreach (SchedulerAppointment visibleAppointment in _visibleAppointments)
				{
					if (visibleAppointment.StartTime.Date > item.GetDate() || dateTime.Date > item.GetDate())
					{
						break;
					}
					if (visibleAppointment.EndTime > dateTime && !visibleAppointment._isBeingMoved)
					{
						float visibleDurationInSlots = (float)visibleAppointment.Duration.TotalMinutes / (float)_minuteInterval;
						if (visibleAppointment.StartTime < dateTime)
						{
							visibleDurationInSlots -= (float)dateTime.Subtract(visibleAppointment.StartTime).TotalMinutes / (float)_minuteInterval;
						}
						float visibleBarHeight = visibleDurationInSlots * (float)_actualRowHeight;
						if (visibleAppointment.StartTime > dateTime)
						{
							appointmentTop += (float)visibleAppointment.StartTime.Subtract(dateTime).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight;
						}
						RectangleF visibleBarRect = new RectangleF(a.Left, appointmentTop, a.Width, visibleBarHeight);
						brush = GetAppointmentBorderBrush(visibleAppointment);
						if (brush != null)
						{
							graphics.FillRectangle(brush, visibleBarRect);
						}
						appointmentTop = (float)visibleAppointment.EndTime.TimeOfDay.Subtract(_firstVisibleSlotTime).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight + (float)_dayAreaTop + (float)base.ClientRectangle.Top;
						dateTime = visibleAppointment.EndTime;
					}
				}
			}
			if (_lightLightPen != null)
			{
				graphics.DrawLine(_lightLightPen, _dayAreaLeft - 5, _allDayAreaControl.Bottom, _dayAreaLeft - 5, base.Bottom);
			}
		}

		private void DrawOwnerAppointmentStrips(Graphics graphics, Rectangle rectangleArgument)
		{
			foreach (SchedulerAppointmentOwner item in _visibleOwners)
			{
				float appointmentTop = base.ClientRectangle.Top;
				foreach (DateTime day in _dates)
				{
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDayCellByDateAndOwner(day, item);
					if (schedulerHourRangeCollectionLayoutState == null || schedulerHourRangeCollectionLayoutState._bounds.Left < _dayAreaLeft)
					{
						continue;
					}
					Rectangle a = new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left - 4, schedulerHourRangeCollectionLayoutState._bounds.Top, 4, schedulerHourRangeCollectionLayoutState._bounds.Height);
					Rectangle rectangle = Rectangle.Intersect(a, rectangleArgument);
					appointmentTop = schedulerHourRangeCollectionLayoutState._bounds.Top;
					if (rectangle.IsEmpty)
					{
						continue;
					}
					Brush brush = GetStripBackgroundBrush();
					if (brush != null)
					{
						graphics.FillRectangle(brush, a.Left, a.Top, a.Width, a.Height);
					}
					DateTime coveredUntil = schedulerHourRangeCollectionLayoutState.GetDate().Add(_firstVisibleSlotTime);
					foreach (SchedulerAppointment backgroundAppointment in _allDayAppointments)
					{
						if ((IsMultiOwnerActive() && !schedulerHourRangeCollectionLayoutState.GetOwner().ValueEquals(backgroundAppointment.Owner)) || backgroundAppointment.StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate() || coveredUntil.Date > schedulerHourRangeCollectionLayoutState.GetDate())
						{
							break;
						}
						if (backgroundAppointment.EndTime > coveredUntil && !backgroundAppointment._isBeingMoved)
						{
							float durationInSlots = (float)backgroundAppointment.Duration.TotalMinutes / (float)_minuteInterval;
							if (backgroundAppointment.StartTime < coveredUntil)
							{
								durationInSlots -= (float)coveredUntil.Subtract(backgroundAppointment.StartTime).TotalMinutes / (float)_minuteInterval;
							}
							float height = durationInSlots * (float)_actualRowHeight;
							if (backgroundAppointment.StartTime > coveredUntil)
							{
								appointmentTop += (float)backgroundAppointment.StartTime.Subtract(coveredUntil).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight;
							}
							RectangleF rect = new RectangleF(a.Left, appointmentTop, a.Width, height);
							brush = GetAppointmentBorderBrush(backgroundAppointment);
							if (brush != null)
							{
								graphics.FillRectangle(brush, rect);
							}
							appointmentTop = (float)backgroundAppointment.EndTime.TimeOfDay.Subtract(_firstVisibleSlotTime).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight + (float)_dayAreaTop + (float)base.ClientRectangle.Top;
							coveredUntil = backgroundAppointment.EndTime;
						}
					}
					SchedulerAppointmentOwnerState schedulerAppointmentOwnerState = _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(day, item);
					ArrayList arrayList = ((schedulerAppointmentOwnerState == null) ? new ArrayList() : schedulerAppointmentOwnerState.GetAppointments());
					foreach (SchedulerAppointment ownerAppointment in arrayList)
					{
						if ((IsMultiOwnerActive() && !schedulerHourRangeCollectionLayoutState.GetOwner().ValueEquals(ownerAppointment.Owner)) || ownerAppointment.StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate() || coveredUntil.Date > schedulerHourRangeCollectionLayoutState.GetDate())
						{
							break;
						}
						if (ownerAppointment.EndTime > coveredUntil && !ownerAppointment._isBeingMoved)
						{
							float ownerDurationInSlots = (float)ownerAppointment.Duration.TotalMinutes / (float)_minuteInterval;
							if (ownerAppointment.StartTime < coveredUntil)
							{
								ownerDurationInSlots -= (float)coveredUntil.Subtract(ownerAppointment.StartTime).TotalMinutes / (float)_minuteInterval;
							}
							float ownerBarHeight = ownerDurationInSlots * (float)_actualRowHeight;
							if (ownerAppointment.StartTime > coveredUntil)
							{
								appointmentTop += (float)ownerAppointment.StartTime.Subtract(coveredUntil).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight;
							}
							RectangleF ownerBarRect = new RectangleF(a.Left, appointmentTop, a.Width, ownerBarHeight);
							if (ownerBarRect.Bottom > (float)base.Bottom)
							{
								ownerBarRect.Height = (float)base.Bottom - ownerBarRect.Top;
							}
							brush = GetAppointmentBorderBrush(ownerAppointment);
							if (brush != null)
							{
								graphics.FillRectangle(brush, ownerBarRect);
							}
							appointmentTop = (float)ownerAppointment.EndTime.TimeOfDay.Subtract(_firstVisibleSlotTime).TotalMinutes / (float)_minuteInterval * (float)_actualRowHeight + (float)_dayAreaTop + (float)base.ClientRectangle.Top;
							coveredUntil = ownerAppointment.EndTime;
						}
					}
				}
			}
			if (_lightLightPen != null)
			{
				graphics.DrawLine(_lightLightPen, _dayAreaLeft - 5, _allDayAreaControl.Bottom, _dayAreaLeft - 5, base.Bottom);
			}
		}

		private void DrawAppointmentViews(PaintEventArgs eventArgs)
		{
			if (_view == SchedulerView.DayView)
			{
				foreach (SchedulerAppointmentView item in _appointmentViews)
				{
					if (item.GetAppointment().Selected)
					{
						continue;
					}
					Rectangle rectangle = Rectangle.Intersect(eventArgs.ClipRectangle, item._bounds);
					if (!rectangle.IsEmpty && rectangle.Height > 1)
					{
						if (!IsAreaDrawnByHandler(eventArgs.Graphics, OwnerDrawnArea.Appointments, item._bounds, item.GetAppointment()))
						{
							item.DrawAppointment(eventArgs.Graphics);
						}
						else if (item.GetAppointment().Description != "")
						{
							item._hasHiddenText = true;
						}
					}
				}
				foreach (SchedulerAppointmentView selectedRenderer in _appointmentViews)
				{
					if (!selectedRenderer.GetAppointment().Selected)
					{
						continue;
					}
					Rectangle selectedVisibleBounds = Rectangle.Intersect(eventArgs.ClipRectangle, selectedRenderer._bounds);
					if (!selectedVisibleBounds.IsEmpty && selectedVisibleBounds.Height > 1)
					{
						if (!IsAreaDrawnByHandler(eventArgs.Graphics, OwnerDrawnArea.Appointments, selectedRenderer._bounds, selectedRenderer.GetAppointment()))
						{
							selectedRenderer.DrawAppointment(eventArgs.Graphics);
						}
					}
				}
			}
			else
			{
				foreach (SchedulerAppointmentView renderer in _appointmentViews)
				{
					if (!Rectangle.Intersect(eventArgs.ClipRectangle, renderer._bounds).IsEmpty)
					{
						if (!IsAreaDrawnByHandler(eventArgs.Graphics, OwnerDrawnArea.Appointments, renderer._bounds, renderer.GetAppointment()))
						{
							renderer.DrawAppointment(eventArgs.Graphics);
						}
					}
				}
			}
		}
	}
}
