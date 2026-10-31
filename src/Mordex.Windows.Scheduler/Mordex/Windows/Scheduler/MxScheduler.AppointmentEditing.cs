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
		internal SchedulerAppointment BuildAppointment(DateTime start, DateTime end, string text)
		{
			return CreateAppointmentInstance(start, end, text);
		}

		protected virtual SchedulerAppointment CreateAppointmentInstance(DateTime start, DateTime end, string text)
		{
			return new SchedulerAppointment(start, end, text);
		}

		internal void CancelAppointmentEdit(SchedulerAppointment schedulerAppointment)
		{
			if (_appointmentEditor != null)
			{
				schedulerAppointment._isEditingText = false;
				ToggleControlEventHandlers(_appointmentEditor, true);
				_isEditingAppointment = false;
				_appointmentEditor.Dispose();
				_appointmentEditor = null;
				if (schedulerAppointment == _newAppointmentBeingEdited)
				{
					Appointments.Remove(_newAppointmentBeingEdited);
				}
				_newAppointmentBeingEdited = null;
				InvalidateAppointmentCacheAndLayout();
				Invalidate(invalidateChildren: true);
				SchedulerRenderer.FocusControl(this);
			}
		}

		internal void SetHoveredAppointment(SchedulerAppointment hoveredAppointment)
		{
			if (_hoveredAppointment != hoveredAppointment)
			{
				if (GetHoveredAppointmentView() != null)
				{
					GetHoveredAppointmentView().HandleMouseLeave(EventArgs.Empty);
				}
				_hoveredAppointment = hoveredAppointment;
			}
		}

		private void AddAppointmentFromDataSource(int index)
		{
			SchedulerAppointment schedulerAppointment = null;
			if (_schedulerPropertyDescriptor != null)
			{
				object owner = null;
				DateTime dateTime = DateTime.MinValue;
				DateTime endTime = DateTime.MinValue;
				string text = string.Empty;
				object value = _schedulerPropertyDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
				string description = "";
				if (value != null && value != DBNull.Value)
				{
					dateTime = (DateTime)value;
				}
				if (_endTimeDescriptor != null)
				{
					object endValue = _endTimeDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
					if (endValue != null && endValue != DBNull.Value)
					{
						endTime = (DateTime)endValue;
					}
				}
				if (_textDescriptor != null)
				{
					object textValue = _textDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
					if (textValue != null && textValue != DBNull.Value)
					{
						text = textValue.ToString();
					}
				}
				if (_descriptionDescriptor != null)
				{
					object descriptionValue = _descriptionDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
					if (descriptionValue != null && descriptionValue != DBNull.Value)
					{
						description = descriptionValue.ToString();
					}
				}
				if (_propertyDescriptor != null)
				{
					owner = _propertyDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
				}
				schedulerAppointment = ((!(endTime != DateTime.MinValue)) ? BuildAppointment(dateTime, dateTime, text) : BuildAppointment(dateTime, endTime, text));
				if (owner != DBNull.Value)
				{
					schedulerAppointment.Owner = owner;
				}
				schedulerAppointment.Description = description;
				schedulerAppointment._notifiesScheduler = false;
				if (_recurrenceInfoDescriptor != null)
				{
					object recurrenceValue = _recurrenceInfoDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
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
				schedulerAppointment._notifiesScheduler = true;
				if (_appointmentSettingsDescriptor != null)
				{
					object settingsValue = _appointmentSettingsDescriptor.GetValue(_schedulerCurrencyManager.List[index]);
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
			}
			if (schedulerAppointment != null)
			{
				schedulerAppointment._dataRow = _schedulerCurrencyManager.List[index];
				schedulerAppointment._scheduler = this;
				_addAction = AddAction.DataSourceNotification;
				if (index < _appointments.Count - 1)
				{
					_isAppointmentCacheStale = true;
					_publicAppointments.Insert(index, schedulerAppointment);
				}
				else
				{
					_isAppointmentCacheStale = true;
					_publicAppointments.Add(schedulerAppointment);
				}
				schedulerAppointment._dataRow = null;
				schedulerAppointment._scheduler = null;
			}
		}

		private void RemoveAppointmentByDataIndex(int dataIndex)
		{
			SchedulerAppointment schedulerAppointment = FindAppointmentByDataIndex(dataIndex);
			if (schedulerAppointment != null)
			{
				if (schedulerAppointment.IsEditing)
				{
					EndEdit();
				}
				_isAppointmentCacheStale = true;
				_publicAppointments.RemoveWithoutNotify(schedulerAppointment);
				_appointments.RemoveWithoutNotify(schedulerAppointment);
				HandleAppointmentRemoved(schedulerAppointment);
				_isAppointmentLayoutStale = true;
				Invalidate();
			}
		}

		private void SetItemValue(object component, string name, object value)
		{
			if (_schedulerCurrencyManager != null)
			{
				if (_isReadingAppointment)
				{
					throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
				}
				if (value == null && component is DataRowView)
				{
					value = DBNull.Value;
				}
				PropertyDescriptorCollection itemProperties = _schedulerCurrencyManager.GetItemProperties();
				itemProperties.Find(name, ignoreCase: true)?.SetValue(component, value);
			}
		}

		internal void AddDayAppointmentView(SchedulerAppointment appointment, DateTime date, int startSlot, int integerArgument, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, bool isFullyVisible)
		{
			bool isNewView = false;
			if (GetNextViewIndex() >= _appointmentViews.Count)
			{
				isNewView = true;
			}
			if (schedulerHourRangeCollectionLayoutState != null)
			{
				if (isNewView)
				{
					SchedulerAppointmentView newView = new SchedulerAppointmentView(appointment, _actualRowHeight, (int)_interval, allDay: false, _view, date, this, UseCompatibleTextRendering);
					newView._appointmentFont = DayFont;
					newView._isShown = false;
					newView.SetStartTimeOfDay(new TimeSpan(0, startSlot * _minuteInterval, 0));
					newView.SetSchedulerRenderer4TimeSpan(new TimeSpan(0, (startSlot + integerArgument) * _minuteInterval, 0));
					newView._isWithinDay = isFullyVisible;
					if (IsMultiOwnerActive())
					{
						newView.SetSchedulerAppointmentOwner(schedulerHourRangeCollectionLayoutState.GetOwner());
					}
					else
					{
						newView.SetSchedulerAppointmentOwner(null);
					}
					if (appointment.Selected)
					{
						newView.UpdateLayout(AppointmentState.Focus);
					}
					else
					{
						AppointmentState appointmentState = AppointmentState.Normal;
						if (appointment.IsRecurring)
						{
							foreach (SchedulerAppointment selectedAppointment in SelectedAppointments)
							{
								if (selectedAppointment.MasterAppointment == appointment.MasterAppointment && selectedAppointment.StartTime == appointment.StartTime)
								{
									appointmentState = AppointmentState.Focus;
								}
							}
						}
						newView.UpdateLayout(appointmentState);
					}
					_appointmentViews.Add(newView);
				}
				else
				{
					SchedulerAppointmentView schedulerRenderer4Argument = (SchedulerAppointmentView)_appointmentViews[GetNextViewIndex()];
					schedulerRenderer4Argument._appointmentFont = DayFont;
					schedulerRenderer4Argument._imageList = GetImageList();
					schedulerRenderer4Argument.UpdateView(_view);
					schedulerRenderer4Argument.UpdateAppointment(appointment);
					schedulerRenderer4Argument._bounds = Rectangle.Empty;
					schedulerRenderer4Argument.UpdateSlotsPerHour(60 / (int)_interval);
					schedulerRenderer4Argument._date = date;
					schedulerRenderer4Argument._cellWidth = schedulerHourRangeCollectionLayoutState._bounds.Width;
					schedulerRenderer4Argument._rowHeight = _actualRowHeight;
					schedulerRenderer4Argument.UpdateFirstSegment(true);
					schedulerRenderer4Argument.UpdateLastSegment(true);
					schedulerRenderer4Argument.SetStartTimeOfDay(new TimeSpan(0, startSlot * _minuteInterval, 0));
					schedulerRenderer4Argument.SetSchedulerRenderer4TimeSpan(new TimeSpan(0, (startSlot + integerArgument) * _minuteInterval, 0));
					schedulerRenderer4Argument._backColor = Color.Empty;
					schedulerRenderer4Argument._schedulerRenderer4Color = Color.Empty;
					schedulerRenderer4Argument._isWithinDay = isFullyVisible;
					if (IsMultiOwnerActive())
					{
						schedulerRenderer4Argument.SetSchedulerAppointmentOwner(schedulerHourRangeCollectionLayoutState.GetOwner());
					}
					else
					{
						schedulerRenderer4Argument.SetSchedulerAppointmentOwner(null);
					}
					if (appointment.Selected)
					{
						schedulerRenderer4Argument.UpdateLayout(AppointmentState.Focus);
					}
					else
					{
						AppointmentState reusedViewState = AppointmentState.Normal;
						if (appointment.IsRecurring)
						{
							for (int i = 0; i < SelectedAppointments.Count; i++)
							{
								SchedulerAppointment selectedForReuse = SelectedAppointments[i];
								if (selectedForReuse.MasterAppointment == appointment.MasterAppointment && selectedForReuse.StartTime == appointment.StartTime && selectedForReuse.RecurrenceState == appointment.RecurrenceState && (selectedForReuse.RecurrenceState != RecurrenceState.Exception || selectedForReuse.GetRecurrenceException() == appointment.GetRecurrenceException()))
								{
									reusedViewState = AppointmentState.Focus;
									SelectedAppointments.Replace(i, schedulerRenderer4Argument.GetAppointment());
								}
							}
						}
						schedulerRenderer4Argument.UpdateLayout(reusedViewState);
					}
					schedulerRenderer4Argument._isShown = false;
				}
			}
			SetNextViewIndex(GetNextViewIndex() + 1);
		}

		private void MoveSelectedAppointments(TimeSpan timeSpan, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			_dragAnchorTime = _dragAnchorTime.Add(timeSpan);
			ArrayList arrayList = new ArrayList();
			foreach (SchedulerAppointment item in _schedulerSelectedAppointments)
			{
				arrayList.Add(item);
			}
			foreach (SchedulerAppointment selectedAppointment in arrayList)
			{
				DateTime dateTime = selectedAppointment.StartTime.Add(timeSpan);
				DateTime droppedEnd = dateTime.Add(selectedAppointment.Duration);
				DroppingAppointmentEventArgs e = new DroppingAppointmentEventArgs(newOwner: (schedulerAppointmentOwner == null) ? selectedAppointment.Owner : schedulerAppointmentOwner.Value, app: selectedAppointment, newStart: dateTime, newEnd: droppedEnd);
				if (!_isAppointmentDragActive)
				{
					bool wasLayoutDirty = _isAppointmentLayoutStale;
					_isAppointmentLayoutStale = false;
					OnDroppingAppointment(e);
					if (wasLayoutDirty)
					{
						_isAppointmentLayoutStale = wasLayoutDirty;
					}
				}
				if (!e.Cancel)
				{
					if (IsMultiOwnerActive())
					{
						object field_ = selectedAppointment._ownerValue;
						selectedAppointment._ownerValue = schedulerAppointmentOwner.Value;
						selectedAppointment.MoveWithOwner(dateTime, droppedEnd, schedulerAppointmentOwner.Value);
						if (selectedAppointment.DataRow != null)
						{
							selectedAppointment._isBeingMoved = true;
							selectedAppointment._ownerValue = field_;
							selectedAppointment.Owner = schedulerAppointmentOwner.Value;
							selectedAppointment._isBeingMoved = false;
						}
						if (!schedulerAppointmentOwner.ValueEquals(selectedAppointment._ownerValue))
						{
							_isAppointmentCacheStale = true;
						}
					}
					else
					{
						selectedAppointment.Move(dateTime, droppedEnd);
					}
					if (selectedAppointment._startTime != dateTime)
					{
						selectedAppointment.RestoreOriginalTimes(true);
						UpdateLayout(selectedAppointment._parentCollection, selectedAppointment, EditType.Change);
					}
					if (!_isAppointmentDragActive)
					{
						AppointmentEventArgs droppedArgs = new AppointmentEventArgs(selectedAppointment);
						OnAppointmentDropped(droppedArgs);
					}
				}
				else
				{
					selectedAppointment.RestoreOriginalTimes(true);
					if (selectedAppointment._parentCollection != null)
					{
						UpdateLayout(selectedAppointment._parentCollection, selectedAppointment, EditType.Change);
					}
				}
				_isAppointmentCacheStale = true;
			}
		}

		private void CancelAppointmentDrag()
		{
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
				_hasCopiesInCollection = false;
			}
			else
			{
				ArrayList arrayList = new ArrayList();
				foreach (SchedulerAppointment selectedAppointment in _schedulerSelectedAppointments)
				{
					arrayList.Add(selectedAppointment);
				}
				foreach (SchedulerAppointment snapshotAppointment in arrayList)
				{
					snapshotAppointment.RestoreOriginalTimes(true);
				}
			}
			if (IsMultiOwnerActive())
			{
				EnsureVisible(_schedulerSchedulerAppointment.StartTime, FindVisibleOwnerByValue(_schedulerSchedulerAppointment.Owner));
			}
			else
			{
				EnsureVisible(_schedulerSchedulerAppointment.StartTime);
			}
			_isAppointmentLayoutStale = true;
			_isAppointmentCacheStale = true;
			Invalidate();
		}

		private void CommitAppointmentDrag()
		{
			if (_dragType != DragType.Move)
			{
				AppointmentChange appointmentChange = AppointmentChange.EndTime;
				_schedulerSchedulerAppointment._isExceptionCreatedDuringDrag = false;
				_ = _schedulerSchedulerAppointment._endTime;
				if (_dragType == DragType.ResizeLeft)
				{
					appointmentChange = AppointmentChange.StartTime;
					_ = _schedulerSchedulerAppointment._startTime;
				}
				if (_schedulerSchedulerAppointment.StartTime != _dragOriginalStart || _schedulerSchedulerAppointment.EndTime != _dragOriginalEnd)
				{
					DateTime dateTime = _schedulerSchedulerAppointment._startTime;
					DateTime originalEndTime = _schedulerSchedulerAppointment._endTime;
					_schedulerSchedulerAppointment._startTime = _dragOriginalStart;
					_schedulerSchedulerAppointment._endTime = _dragOriginalEnd;
					_schedulerSchedulerAppointment._duration = _timeSpan;
					bool isCancelled = false;
					switch (appointmentChange)
					{
					case AppointmentChange.StartTime:
					{
						AppointmentChangeCancelEventArgs startChangeArgs = new AppointmentChangeCancelEventArgs(_schedulerSchedulerAppointment, AppointmentChange.StartTime, dateTime);
						RaiseAppointmentChanging(startChangeArgs);
						dateTime = (DateTime)startChangeArgs.ProposedValue;
						isCancelled = startChangeArgs.Cancel;
						break;
					}
					case AppointmentChange.EndTime:
					{
						AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(_schedulerSchedulerAppointment, AppointmentChange.EndTime, originalEndTime);
						RaiseAppointmentChanging(e);
						originalEndTime = (DateTime)e.ProposedValue;
						isCancelled = e.Cancel;
						break;
					}
					}
					if (isCancelled)
					{
						UpdateLayout(_schedulerSchedulerAppointment._parentCollection, _schedulerSchedulerAppointment, EditType.Change);
						_isAppointmentCacheStale = true;
						InvalidateAppointmentLayout();
					}
					else
					{
						_schedulerSchedulerAppointment._startTime = dateTime;
						_schedulerSchedulerAppointment._endTime = originalEndTime;
						_isAppointmentCacheStale = true;
						UpdateLayout(_schedulerSchedulerAppointment._parentCollection, _schedulerSchedulerAppointment, EditType.Change);
						NotifyAppointmentChanged(_schedulerSchedulerAppointment, appointmentChange);
					}
				}
				return;
			}
			ArrayList arrayList = new ArrayList();
			foreach (SchedulerAppointment item in _schedulerSelectedAppointments)
			{
				arrayList.Add(item);
			}
			if (_hasCopiesInCollection)
			{
				_shouldRaiseChangeEvents = false;
				foreach (SchedulerAppointment copyToRemove in _draggedAppointmentCopies)
				{
					_publicAppointments.RemoveWithoutNotify(copyToRemove);
					_appointments.RemoveWithoutNotify(copyToRemove);
				}
				_shouldRaiseChangeEvents = true;
				foreach (SchedulerAppointment copyToAdd in _draggedAppointmentCopies)
				{
					_publicAppointments.Add(copyToAdd);
				}
			}
			else if (_draggedAppointmentCopies != null)
			{
				foreach (SchedulerAppointment copyToRestore in _draggedAppointmentCopies)
				{
					copyToRestore.SetSchedulerAppointmentValue();
				}
			}
			if (IsMultiOwnerActive())
			{
				try
				{
					if (!_schedulerSchedulerAppointmentOwner.ValueEquals(_schedulerSchedulerAppointment._ownerValue))
					{
						foreach (SchedulerAppointment movedAppointment in arrayList)
						{
							_isAppointmentCacheStale = true;
							UpdateLayout(movedAppointment._parentCollection, movedAppointment, EditType.Change);
						}
					}
				}
				catch
				{
				}
			}
			if (_schedulerSchedulerAppointment.StartTime != _dragOriginalStart || _schedulerSchedulerAppointment.EndTime != _dragOriginalEnd || _hasCopiesInCollection)
			{
				DateTime field_ = _schedulerSchedulerAppointment._startTime;
				TimeSpan timeOffset = _dragOriginalStart.Subtract(_schedulerSchedulerAppointment._startTime);
				SchedulerAppointmentOwner schedulerAppointmentOwner = FindVisibleOwnerByValue(_schedulerSchedulerAppointment.Owner);
				foreach (SchedulerAppointment selectedAppointment in arrayList)
				{
					DateTime dateTimeArgument = selectedAppointment.StartTime.Add(timeOffset);
					DateTime newEndTime = dateTimeArgument.Add(selectedAppointment.Duration);
					selectedAppointment._startTime = dateTimeArgument;
					selectedAppointment._endTime = newEndTime;
					if (_schedulerSchedulerAppointmentOwner != null)
					{
						selectedAppointment._ownerValue = _schedulerSchedulerAppointmentOwner.Value;
					}
				}
				timeOffset = field_.Subtract(_dragOriginalStart);
				if (_view != SchedulerView.DayView)
				{
					MoveSelectedAppointments(timeOffset, CurrentOwner);
				}
				else
				{
					MoveSelectedAppointments(timeOffset, schedulerAppointmentOwner);
				}
			}
			else
			{
				if (!IsMultiOwnerActive() || _schedulerSchedulerAppointmentOwner == null || _schedulerSchedulerAppointmentOwner.ValueEquals(_schedulerSchedulerAppointment._ownerValue))
				{
					return;
				}
				object owner = _schedulerSchedulerAppointment.Owner;
				foreach (SchedulerAppointment droppedAppointment in arrayList)
				{
					droppedAppointment._ownerValue = _schedulerSchedulerAppointmentOwner.Value;
					DroppingAppointmentEventArgs droppingArgs = new DroppingAppointmentEventArgs(droppedAppointment, droppedAppointment.StartTime, droppedAppointment.EndTime, owner);
					bool wasLayoutDirty = _isAppointmentLayoutStale;
					_isAppointmentLayoutStale = false;
					OnDroppingAppointment(droppingArgs);
					if (wasLayoutDirty)
					{
						_isAppointmentLayoutStale = wasLayoutDirty;
					}
					if (!droppingArgs.Cancel)
					{
						if (droppedAppointment.DataRow != null)
						{
							droppedAppointment.Owner = owner;
							if (_schedulerSchedulerAppointmentOwner.ValueEquals(droppedAppointment.Owner))
							{
								_isAppointmentCacheStale = true;
								UpdateLayout(droppedAppointment._parentCollection, droppedAppointment, EditType.Change);
							}
						}
						AppointmentEventArgs droppedArgs = new AppointmentEventArgs(droppedAppointment);
						OnAppointmentDropped(droppedArgs);
					}
					else
					{
						_isAppointmentCacheStale = true;
						UpdateLayout(droppedAppointment._parentCollection, droppedAppointment, EditType.Change);
					}
				}
			}
		}

		private void RemoveUnusedAppointmentRenderers()
		{
			if (GetNextViewIndex() < _appointmentViews.Count)
			{
				int rendererIndex;
				for (rendererIndex = GetNextViewIndex(); rendererIndex < _appointmentViews.Count; rendererIndex++)
				{
					SchedulerAppointmentView renderer = (SchedulerAppointmentView)_appointmentViews[rendererIndex];
					renderer.StopTimerAndHideToolTip();
					renderer.ReleaseEffectiveFormatStyle();
					renderer = null;
					_appointmentViews.RemoveAt(rendererIndex);
					rendererIndex--;
				}
			}
		}

		internal void RemoveFromAppointments(SchedulerAppointment appointment)
		{
			if (_appointments.Contains(appointment))
			{
				_appointments.Remove(appointment);
			}
		}

		internal void AddToAppointments(SchedulerAppointment appointment)
		{
			if (!_appointments.Contains(appointment))
			{
				_appointments.Add(appointment);
			}
		}

		private void CopyAppointmentFields(SchedulerAppointment sourceApp, SchedulerAppointment copyApp)
		{
			copyApp.UpdateSchedulerAppointmentValue(Fields);
			foreach (SchedulerField field in Fields)
			{
				if (!IsBuiltInField(field) && field.IncludeInCopy)
				{
					copyApp.SetValue(field, sourceApp.GetValue(field));
				}
			}
			AppointmentCopiedEventArgs e = new AppointmentCopiedEventArgs(sourceApp, copyApp);
			OnAppointmentCopied(e);
		}
	}
}
