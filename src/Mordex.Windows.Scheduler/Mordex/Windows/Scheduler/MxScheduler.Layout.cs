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
		internal ContextMenuStrip GetContextMenu()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new ContextMenuStrip();
			}
			return _contextMenu;
		}

		void ISchedulerFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			InvalidateAppointmentStyles();
			Invalidate(invalidateChildren: true);
		}

		internal void InvalidateWithChildren()
		{
			InvalidateAppointmentStyles();
			Invalidate(invalidateChildren: true);
		}

		// The appointment views keep their effective style between draws. Anything that can change it (a style property, a
		// format condition, the theme, an appointment, the data, the size or the font) calls this, and each view builds its
		// style again on its next draw.
		internal void InvalidateAppointmentStyles()
		{
			_appointmentStyleVersion++;
		}

		public void EnsureVisible(DateTime date, SchedulerAppointmentOwner owner)
		{
			if (_isFullLayoutStale)
			{
				RenderCurrentView(null);
			}
			if (!_dates.Contains(date.Date) && Date.Date != date.Date)
			{
				Date = date;
				RenderCurrentView(null);
			}
			if (_view == SchedulerView.DayView)
			{
				int targetSlotIndex = ((int)date.TimeOfDay.TotalMinutes - _dayStartHour * 60) / _minuteInterval;
				int visibleSlotCount = (base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight;
				if (_firstVisibleSlot != null)
				{
					int visibleEndSlotIndex = _firstVisibleSlot.GetIndex() + visibleSlotCount;
					if (targetSlotIndex < _firstVisibleSlot.GetIndex() || targetSlotIndex >= visibleEndSlotIndex)
					{
						GetVerticalScrollBarControl().UpdateScrollValue(targetSlotIndex);
					}
				}
				else
				{
					_firstVisibleSlotIndex = targetSlotIndex;
				}
				if (_dayColumns > 0)
				{
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = null;
					schedulerHourRangeCollectionLayoutState = ((!IsMultiOwnerActive() || owner == null) ? FindDayCellByDate(date) : FindDayCellByDateAndOwner(date, owner));
					if (schedulerHourRangeCollectionLayoutState != null && (schedulerHourRangeCollectionLayoutState.GetDayIndex() < _firstVisibleColumnIndex || schedulerHourRangeCollectionLayoutState.GetDayIndex() > _firstVisibleColumnIndex + _dayColumns - 1))
					{
						GetHorizontalScrollBarControl().UpdateScrollValue(schedulerHourRangeCollectionLayoutState.GetDayIndex());
					}
				}
			}
			else if (IsMultiOwnerActive() && owner != null)
			{
				CurrentOwner = owner;
			}
		}

		public void EnsureVisible(SchedulerAppointment appointment)
		{
			if (appointment != null)
			{
				EnsureVisible(appointment.StartTime, FindVisibleOwnerByValue(appointment._ownerValue));
				if (appointment.Duration.TotalDays >= 1.0 && _view == SchedulerView.DayView && _allDayAreaControl != null)
				{
					_allDayAreaControl.ScrollAppointmentIntoView(appointment);
				}
			}
		}

		public void EnsureVisible(DateTime date)
		{
			if (_isFullLayoutStale)
			{
				RenderCurrentView(null);
			}
			if (_dayColumns > 0)
			{
				EnsureVisible(date, _currentOwner);
				return;
			}
			if (!_dates.Contains(date.Date))
			{
				Date = date;
				_isAppointmentCacheStale = true;
			}
			if (_view != SchedulerView.DayView)
			{
				return;
			}
			int targetSlotIndex = ((int)date.TimeOfDay.TotalMinutes - _dayStartHour * 60) / _minuteInterval;
			int visibleSlotCount = (base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight;
			if (_firstVisibleSlot != null)
			{
				int visibleEndSlotIndex = _firstVisibleSlot.GetIndex() + visibleSlotCount;
				if (targetSlotIndex < _firstVisibleSlot.GetIndex() || targetSlotIndex >= visibleEndSlotIndex)
				{
					GetVerticalScrollBarControl().UpdateScrollValue(targetSlotIndex);
				}
			}
			else
			{
				_firstVisibleSlotIndex = targetSlotIndex;
			}
		}

		public void EnsureVisible(TimeSpan time)
		{
			if (_view == SchedulerView.DayView && _dates.Count > 0)
			{
				EnsureVisible(_dates[0].Add(time));
			}
		}

		private SchedulerLayout FindLayoutByKey(SchedulerLayoutCollection schedulerLayoutCollection, string strB)
		{
			foreach (SchedulerLayout item in schedulerLayoutCollection)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return item;
				}
				SchedulerLayout childMatch = FindLayoutByKey(item.ChildLayouts, strB);
				if (childMatch != null)
				{
					return childMatch;
				}
			}
			return null;
		}

		internal TypeCode GetFieldTypeCode(SchedulerField schedulerField)
		{
			try
			{
				if (_schedulerCurrencyManager != null)
				{
					PropertyDescriptorCollection itemProperties = _schedulerCurrencyManager.GetItemProperties();
					if (schedulerField.DataMember.Length > 0)
					{
						PropertyDescriptor propertyDescriptor = itemProperties.Find(schedulerField.DataMember, ignoreCase: true);
						if (propertyDescriptor != null)
						{
							return Type.GetTypeCode(propertyDescriptor.PropertyType);
						}
					}
				}
				return TypeCode.String;
			}
			catch
			{
				return TypeCode.String;
			}
		}

		internal string GetRecurrenceInfoFieldName()
		{
			if (_usesInternalTable)
			{
				return "___RecurrenceInfoMember";
			}
			return _recurrenceInfoField;
		}

		private BorderFlags GetHeaderBorderFlags(Rectangle rectangle)
		{
			BorderFlags borderFlags = BorderFlags.None;
			if (_view == SchedulerView.DayView)
			{
				if (rectangle.Top > base.ClientRectangle.Top)
				{
					borderFlags |= BorderFlags.Top;
				}
				if (rectangle.Left > _dayAreaLeft)
				{
					borderFlags |= BorderFlags.Left;
				}
				borderFlags |= BorderFlags.Bottom;
			}
			else if (_view == SchedulerView.MonthView)
			{
				borderFlags |= (BorderFlags)12;
				if (rectangle.X <= base.ClientRectangle.Left + _actualRowHeight && rectangle.X > base.ClientRectangle.Left + 1)
				{
					borderFlags |= BorderFlags.Left;
				}
				if (rectangle.Y > base.ClientRectangle.Top + 1)
				{
					borderFlags |= BorderFlags.Top;
				}
			}
			else
			{
				borderFlags |= (BorderFlags)12;
			}
			if (GetEffectiveBorderStyle() == BorderStyle.None)
			{
				borderFlags |= BorderFlags.Top;
			}
			return borderFlags;
		}

		// Set while the time slots of a day view are painted, so slots in the same hour range share one merged style.
		private Dictionary<(SchedulerHourRange, bool, SchedulerFormatStyle), SchedulerFormatStyle> _hourRangeStyleCache;

		internal SchedulerFormatStyle GetTimeSlotStyle(SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, SchedulerFormatStyle schedulerFormatStyleArgument, SchedulerFormatStyle alternatingHourStyle, SchedulerFormatStyle workingHourStyle, SchedulerFormatStyle alternatingWorkingHourStyle)
		{
			schedulerAppointmentOwnerLayoutState.ResolveOwner();
			SchedulerFormatStyle schedulerFormatStyle = null;
			bool isWorkingHour = schedulerAppointmentOwnerLayoutState.IsWorkingHour();
			bool isAlternatingHour = false;
			if (HourEvenOdd != HourEvenOdd.None)
			{
				if (HourEvenOdd == HourEvenOdd.Interval)
				{
					if (schedulerAppointmentOwnerLayoutState.GetIndex() % 2 != 0)
					{
						isAlternatingHour = true;
					}
				}
				else if (schedulerAppointmentOwnerLayoutState.GetDate().Hour % 2 != 0)
				{
					isAlternatingHour = true;
				}
			}
			schedulerFormatStyle = (isAlternatingHour ? ((!isWorkingHour) ? alternatingHourStyle : alternatingWorkingHourStyle) : ((!isWorkingHour) ? schedulerFormatStyleArgument : workingHourStyle));
			SchedulerFormatStyle hourRangeStyle = null;
			SchedulerHourRangeCollection hourRanges = schedulerAppointmentOwnerLayoutState.GetSchedulerHourRangeCollectionLayoutState().GetSchedulerHourRangeCollection();
			if (hourRanges != null && hourRanges.Count > 0)
			{
				TimeSpan timeOfDay = schedulerAppointmentOwnerLayoutState.GetDate().TimeOfDay;
				SchedulerFormatStyle openStyle = isAlternatingHour ? alternatingWorkingHourStyle : workingHourStyle;
				SchedulerFormatStyle closedStyle = isAlternatingHour ? alternatingHourStyle : schedulerFormatStyleArgument;
				if (_hourRangeStyleCache == null)
				{
					hourRangeStyle = hourRanges.GetFormatStyleForTime(timeOfDay, isAlternatingHour, openStyle, closedStyle);
				}
				else
				{
					SchedulerHourRange range = hourRanges.FindRangeForTime(timeOfDay);
					if (range != null)
					{
						SchedulerFormatStyle baseStyle = range.IsWorkingTime ? openStyle : closedStyle;
						if (!_hourRangeStyleCache.TryGetValue((range, isAlternatingHour, baseStyle), out hourRangeStyle))
						{
							hourRangeStyle = isAlternatingHour ? baseStyle.MergeWith(range.AlternatingFormatStyle) : baseStyle.MergeWith(range.FormatStyle);
							_hourRangeStyleCache.Add((range, isAlternatingHour, baseStyle), hourRangeStyle);
						}
					}
				}
			}
			if (hourRangeStyle == null)
			{
				return schedulerFormatStyle;
			}
			return hourRangeStyle;
		}

		internal ImageList GetImageList()
		{
			if (_imageSource == ImageSource.ExternalImageList)
			{
				return _imageList;
			}
			return _internalImageList;
		}

		internal void UpdateLayout(SchedulerAppointmentCollection schedulerAppointmentCollection, SchedulerAppointment appointment, EditType editType)
		{
			switch (editType)
			{
			case EditType.Add:
				if (schedulerAppointmentCollection == _publicAppointments)
				{
					if (!_appointments.Contains(appointment))
					{
						if (appointment.RecurrencePattern != null)
						{
							UpdateRecurrenceCollection(appointment, false);
						}
						_appointments.AddValidated(appointment);
						if (appointment.RecurrencePattern != null && appointment.RecurrencePattern.Exceptions.Count > 0)
						{
							foreach (RecurrenceException exception in appointment.RecurrencePattern.Exceptions)
							{
								AddToAppointments(exception.Appointment);
							}
						}
						else if (appointment.RecurrenceState == RecurrenceState.Exception && appointment.RecurrencePattern == null)
						{
							SchedulerAppointment schedulerAppointment = RecurrenceAppointments.FindByRecurrenceId(appointment.GetRecurrenceException().GetRecurrencePatternId());
							if (schedulerAppointment != null)
							{
								appointment.GetRecurrenceException()._pattern = schedulerAppointment.RecurrencePattern;
								RecurrenceException existingException = schedulerAppointment.RecurrencePattern.Exceptions[appointment.GetRecurrenceException().OriginalDate];
								if (existingException != null)
								{
									schedulerAppointment.RecurrencePattern.Exceptions.RemoveException(existingException);
								}
								schedulerAppointment.RecurrencePattern.Exceptions.AddException(appointment.GetRecurrenceException());
								appointment.SetMasterAppointment(schedulerAppointment);
							}
						}
					}
					if (FilterConditionApplied != null && !FilterConditionApplied.IsAppointmentAccepted(appointment))
					{
						appointment._isRemovalSilent = true;
						_publicAppointments.RemoveWithoutNotify(appointment);
						appointment._scheduler = this;
						appointment._isRemovalSilent = false;
					}
					break;
				}
				AddToDataSource(appointment);
				if (appointment.RecurrencePattern != null)
				{
					UpdateRecurrenceCollection(appointment, false);
				}
				if (appointment.IsDataRowEditStarted())
				{
					if (appointment.DataRow is IEditableObject)
					{
						((IEditableObject)appointment.DataRow).EndEdit();
					}
					_shouldHandleItemAdded = true;
					appointment.SetDataRowEditStarted(false);
					appointment._scheduler = null;
					appointment._dataRow = null;
				}
				if (appointment._recurrenceState != RecurrenceState.Master || appointment.RecurrencePattern == null || appointment.RecurrencePattern.Exceptions.Count <= 0)
				{
					break;
				}
				foreach (RecurrenceException recurrenceException in appointment.RecurrencePattern.Exceptions)
				{
					_appointments.Add(recurrenceException.Appointment);
				}
				break;
			case EditType.Change:
				WriteAppointmentToDataSource(appointment);
				break;
			case EditType.Remove:
				if (_appointmentEditor != null && (_appointmentEditor._app.Equals(appointment) || (_appointmentEditor._app.RecurrenceState == RecurrenceState.Occurrence && _appointmentEditor._app.MasterAppointment == appointment)) && !TryCommitEditor())
				{
					return;
				}
				if (appointment.Selected)
				{
					UpdateCurrentAppointment(appointment);
				}
				if (appointment.RecurrencePattern != null && _recurrenceAppointmentCollection.Contains(appointment))
				{
					appointment.RecurrencePattern = null;
					if (_recurrenceAppointmentCollection.Contains(appointment))
					{
						UpdateRecurrenceCollection(appointment, true);
					}
				}
				if (schedulerAppointmentCollection == _publicAppointments)
				{
					if (_nextReminderAppointment == appointment)
					{
						RecalculateReminder(appointment);
					}
					int index = appointment._index;
					if (_shouldHandleRemovals)
					{
						RemoveFromDataSource(appointment);
					}
					appointment._index = index;
					_appointments.RemoveWithoutNotify(appointment);
				}
				else if (_publicAppointments.Contains(appointment))
				{
					_publicAppointments.Remove(appointment);
				}
				else if (appointment._index >= 0 && _shouldHandleRemovals)
				{
					RemoveFromDataSource(appointment);
				}
				break;
			case EditType.Clear:
				SetEditor(null);
				ResetRemindersIfBound();
				SelectedAppointments.Clear();
				break;
			}
			if (appointment != null && appointment.MasterAppointment != null && appointment._recurrenceState == RecurrenceState.Exception)
			{
				WriteRecurrenceToDataSource(appointment.MasterAppointment);
			}
			if (_view == SchedulerView.DayView && editType == EditType.Remove && appointment._duration.TotalDays >= 1.0)
			{
				_isFullLayoutStale = true;
			}
			_isAppointmentLayoutStale = true;
			InvalidateAppointmentStyles();
			Invalidate(invalidateChildren: true);
		}

		internal void InvalidateAppointmentCacheAndLayout()
		{
			InvalidateAppointmentStyles();
			_isAppointmentCacheStale = true;
			_isAppointmentLayoutStale = true;
			Invalidate(invalidateChildren: true);
		}



		private DateTime ClampResizedStart(DateTime dateTime, SchedulerAppointment schedulerAppointment)
		{
			if (dateTime > schedulerAppointment.EndTime.AddMinutes(-_minuteInterval))
			{
				return schedulerAppointment.EndTime.AddMinutes(-_minuteInterval);
			}
			if (!_allowDragInNonWorkingTime)
			{
				SchedulerAppointmentOwner schedulerAppointmentOwner = null;
				if (IsMultiOwnerActive())
				{
					schedulerAppointmentOwner = _owners.FindOwnerByValue(schedulerAppointment.Owner);
				}
				SchedulerDayHourRanges days = new SchedulerDayHourRanges(this, schedulerAppointmentOwner);
				if (!IsWorkingTimeSlot(days.For(dateTime), dateTime, TimeSpan.Zero))
				{
					return ClampResizedStart(dateTime.AddMinutes(_minuteInterval), schedulerAppointment);
				}
			}
			return dateTime;
		}

		private DateTime ClampResizedEnd(DateTime dateTime, SchedulerAppointment schedulerAppointment)
		{
			if (dateTime <= schedulerAppointment.StartTime.AddMinutes(_minuteInterval))
			{
				return schedulerAppointment.StartTime.AddMinutes(_minuteInterval);
			}
			if (!_allowDragInNonWorkingTime)
			{
				SchedulerAppointmentOwner schedulerAppointmentOwner = null;
				if (IsMultiOwnerActive())
				{
					schedulerAppointmentOwner = _owners.FindOwnerByValue(schedulerAppointment.Owner);
				}
				// The end time is not part of the appointment, so the time just before it is checked, as the resize
				// gate does. The end lies after the start here, so one tick back stays within the calendar.
				DateTime lastTime = dateTime.AddTicks(-1);
				SchedulerDayHourRanges days = new SchedulerDayHourRanges(this, schedulerAppointmentOwner);
				if (!IsWorkingTimeSlot(days.For(lastTime), lastTime, TimeSpan.Zero))
				{
					return ClampResizedEnd(dateTime.AddMinutes(-_minuteInterval), schedulerAppointment);
				}
			}
			return dateTime;
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[_layoutLoadEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
