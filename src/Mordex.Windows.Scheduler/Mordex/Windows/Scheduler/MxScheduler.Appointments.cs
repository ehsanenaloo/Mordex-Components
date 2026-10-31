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
		internal void SortVisibleAppointments()
		{
			if (_view != SchedulerView.DayView)
			{
				SchedulerStartTimeComparer comparer = new SchedulerStartTimeComparer();
				_visibleAppointments.Sort(comparer);
				return;
			}
			foreach (SchedulerAppointmentOwnerState item in _appointmentsByOwnerDate)
			{
				SchedulerComparer dayComparer = new SchedulerComparer();
				item.GetAppointments().Sort(dayComparer);
			}
		}

		private void SelectFirstOrLastAppointment(bool selectFirst)
		{
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = null;
			SchedulerAppointmentView appointmentView = null;
			DateTime minValue = DateTime.MinValue;
			if (selectFirst)
			{
				for (int i = 0; i < _dayLayouts.Count; i++)
				{
					schedulerHourRangeCollectionLayoutState = _dayLayouts.GetLayoutAt(i);
					if (_view == SchedulerView.DayView)
					{
						if (_multiOwner)
						{
							_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetOwner()).GetAppointments();
						}
						else
						{
							_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDate(schedulerHourRangeCollectionLayoutState.GetDate()).GetAppointments();
						}
					}
					if (_visibleAppointments.Count > 0)
					{
						appointmentView = FindAppointmentView((SchedulerAppointment)_visibleAppointments[0]);
					}
					if (appointmentView != null)
					{
						if (_multiOwner)
						{
							CurrentOwner = FindVisibleOwnerByValue(appointmentView.GetAppointment().Owner);
						}
						minValue = schedulerHourRangeCollectionLayoutState.GetDate();
						if (appointmentView.GetAppointment().StartTime > minValue)
						{
							minValue = appointmentView.GetAppointment().StartTime;
						}
						SelectDateRange(minValue, minValue.AddMinutes(_minuteInterval));
						break;
					}
				}
			}
			else
			{
				for (int index = _dayLayouts.Count - 1; index >= 0; index--)
				{
					schedulerHourRangeCollectionLayoutState = _dayLayouts.GetLayoutAt(index);
					if (_multiOwner)
					{
						_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetOwner()).GetAppointments();
					}
					else
					{
						_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDate(schedulerHourRangeCollectionLayoutState.GetDate()).GetAppointments();
					}
					if (_visibleAppointments.Count > 0)
					{
						appointmentView = FindAppointmentView((SchedulerAppointment)_visibleAppointments[0]);
					}
					if (appointmentView != null)
					{
						if (_multiOwner)
						{
							CurrentOwner = FindVisibleOwnerByValue(appointmentView.GetAppointment().Owner);
						}
						minValue = schedulerHourRangeCollectionLayoutState.GetDate();
						if (appointmentView.GetAppointment().StartTime > minValue)
						{
							minValue = appointmentView.GetAppointment().StartTime;
						}
						SelectDateRange(minValue, minValue.AddMinutes(_minuteInterval));
						break;
					}
				}
			}
			if (IsMultiOwnerActive())
			{
				EnsureVisible(_selectedDays.Start, CurrentOwner);
			}
			else
			{
				EnsureVisible(_selectedDays.Start);
			}
			_schedulerOwner = null;
			Invalidate();
		}

		private bool SelectAdjacentAppointment(bool forward)
		{
			bool hasAppointments = false;
			SchedulerAppointment schedulerAppointment = null;
			DateTime dateTime = _selectedDays.Start;
			if (_schedulerOwner != null)
			{
				schedulerAppointment = _schedulerOwner.GetAppointment();
				dateTime = schedulerAppointment.StartTime;
				if (!TryCommitEditor())
				{
					return true;
				}
			}
			SchedulerAppointmentView appointmentView = null;
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = null;
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = null;
			if (IsMultiOwnerActive())
			{
				schedulerHourRangeCollectionLayoutState = FindDayCellByDateAndOwner(dateTime, _currentOwner);
				if (schedulerHourRangeCollectionLayoutState == null)
				{
					SelectFirstOrLastAppointment(forward);
					return true;
				}
				_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetOwner()).GetAppointments();
			}
			else
			{
				schedulerHourRangeCollectionLayoutState = FindDayCellByDate(dateTime);
				if (schedulerHourRangeCollectionLayoutState == null)
				{
					SelectFirstOrLastAppointment(forward);
					return true;
				}
				_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDate(schedulerHourRangeCollectionLayoutState.GetDate()).GetAppointments();
			}
			schedulerHourRangeCollectionLayoutStateArgument = schedulerHourRangeCollectionLayoutState;
			if (_visibleAppointments.Count > 0)
			{
				appointmentView = FindAppointmentView(GetAdjacentAppointment(schedulerAppointment, forward, dateTime));
				hasAppointments = true;
			}
			if (appointmentView == null)
			{
				if (forward)
				{
					for (int i = schedulerHourRangeCollectionLayoutState.GetDayIndex() + 1; i < _dayLayouts.Count; i++)
					{
						schedulerHourRangeCollectionLayoutStateArgument = _dayLayouts.GetLayoutAt(i);
						if (_multiOwner)
						{
							_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutStateArgument.GetDate(), schedulerHourRangeCollectionLayoutStateArgument.GetOwner()).GetAppointments();
						}
						else
						{
							_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDate(schedulerHourRangeCollectionLayoutStateArgument.GetDate()).GetAppointments();
						}
						if (_visibleAppointments.Count > 0)
						{
							appointmentView = FindAppointmentView((SchedulerAppointment)_visibleAppointments[0]);
							hasAppointments = true;
						}
						if (appointmentView != null)
						{
							if (_multiOwner)
							{
								CurrentOwner = FindVisibleOwnerByValue(appointmentView.GetAppointment().Owner);
							}
							break;
						}
					}
				}
				else
				{
					for (int index = schedulerHourRangeCollectionLayoutState.GetDayIndex() - 1; index >= 0; index--)
					{
						schedulerHourRangeCollectionLayoutStateArgument = _dayLayouts.GetLayoutAt(index);
						if (_multiOwner)
						{
							_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutStateArgument.GetDate(), schedulerHourRangeCollectionLayoutStateArgument.GetOwner()).GetAppointments();
						}
						else
						{
							_visibleAppointments = _appointmentsByOwnerDate.FindOwnerStateByDate(schedulerHourRangeCollectionLayoutStateArgument.GetDate()).GetAppointments();
						}
						if (_visibleAppointments.Count > 0)
						{
							appointmentView = FindAppointmentView((SchedulerAppointment)_visibleAppointments[_visibleAppointments.Count - 1]);
							hasAppointments = true;
						}
						if (appointmentView != null)
						{
							if (_multiOwner)
							{
								CurrentOwner = FindVisibleOwnerByValue(appointmentView.GetAppointment().Owner);
							}
							break;
						}
					}
				}
			}
			if (appointmentView != null)
			{
				_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
				SchedulerAppointment selectedAppointment = (_currentAppointment = appointmentView.GetAppointment());
				selectedAppointment.Selected = true;
				if (selectedAppointment.Duration.TotalDays > 1.0)
				{
					EnsureVisible(schedulerHourRangeCollectionLayoutStateArgument.GetDate(), schedulerHourRangeCollectionLayoutStateArgument.GetOwner());
				}
				else
				{
					EnsureVisible(selectedAppointment);
				}
				ShowEditor(appointmentView, selectedAppointment.Text);
			}
			else
			{
				_schedulerSelectedAppointments.Clear();
				_currentAppointment = null;
				_schedulerOwner = null;
				if (hasAppointments)
				{
					SchedulerRenderer.FocusControl(this);
				}
			}
			return hasAppointments;
		}

		private void OnControlItemChanged(object sender, ItemChangedEventArgs eventArgs)
		{
			if (base.InvokeRequired)
			{
				ItemChangedCallback method = OnCurrencyManagerItemChanged;
				Invoke(method, eventArgs);
				Thread.Sleep(0);
			}
			else
			{
				OnCurrencyManagerItemChanged(eventArgs);
			}
		}

		private void OnCurrencyManagerItemChanged(ItemChangedEventArgs eventArgs)
		{
			if (eventArgs.Index != -1)
			{
				if (!SupportsListChangeNotification() && _shouldHandleItemChanged)
				{
					RefreshAppointmentFromDataSource(eventArgs.Index);
				}
			}
			else if (SupportsListChangeNotification())
			{
				if (GetCurrencyManager().List != _iList)
				{
					_publicAppointments.UpdateSchedulerAppointmentCollection();
					_appointments.UpdateSchedulerAppointmentCollection();
					_isReadingAppointment = true;
					ReloadAppointmentsFromDataSource();
					_isReadingAppointment = false;
				}
			}
			else
			{
				_publicAppointments.UpdateSchedulerAppointmentCollection();
				_appointments.UpdateSchedulerAppointmentCollection();
				_isReadingAppointment = true;
				ReloadAppointmentsFromDataSource();
				_isReadingAppointment = false;
			}
		}

		internal void ReloadAppointmentsFromDataSource()
		{
			_recurrenceAppointmentCollection.Clear();
			_appointments.UpdateSchedulerAppointmentCollection();
			_publicAppointments.UpdateSchedulerAppointmentCollection();
			_currentAppointment = null;
			_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
			_nextReminderAppointment = null;
			_nextReminderTime = DateTime.MaxValue;
			if (_schedulerCurrencyManager == null)
			{
				SetCurrencyManager(null);
				return;
			}
			try
			{
				UpdatePropertyDescriptors();
				ArrayList arrayList = null;
				if (_recurrenceInfoDescriptor != null)
				{
					arrayList = new ArrayList();
				}
				for (int i = 0; i < _schedulerCurrencyManager.Count; i++)
				{
					if (_schedulerPropertyDescriptor == null)
					{
						continue;
					}
					DateTime dateTime = DateTime.MinValue;
					try
					{
						dateTime = (DateTime)_schedulerPropertyDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
					}
					catch
					{
					}
					DateTime endTime = DateTime.MinValue;
					object owner = null;
					string text = "";
					string description = "";
					if (_textDescriptor != null)
					{
						object value = _textDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
						if (value != null && value.GetType() == typeof(string))
						{
							text = (string)value;
						}
						else if (!(value is DBNull) && value != null)
						{
							text = value.ToString();
						}
					}
					if (_descriptionDescriptor != null)
					{
						object descriptionValue = _descriptionDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
						if (descriptionValue != null && descriptionValue.GetType() == typeof(string))
						{
							description = (string)descriptionValue;
						}
						else if (!(descriptionValue is DBNull) && descriptionValue != null)
						{
							description = descriptionValue.ToString();
						}
					}
					if (_endTimeDescriptor != null)
					{
						object endValue = _endTimeDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
						endTime = ((endValue == null || endValue.GetType() != typeof(DateTime)) ? dateTime : ((DateTime)endValue));
					}
					if (_propertyDescriptor != null)
					{
						object ownerValue = _propertyDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
						owner = ((ownerValue != DBNull.Value) ? ownerValue : null);
					}
					SchedulerAppointment schedulerAppointment = ((!(endTime != DateTime.MinValue) || !(endTime >= dateTime)) ? BuildAppointment(dateTime, dateTime, text) : BuildAppointment(dateTime, endTime, text));
					if (owner != null)
					{
						schedulerAppointment.Owner = owner;
					}
					schedulerAppointment.Description = description;
					schedulerAppointment._notifiesScheduler = false;
					if (_recurrenceInfoDescriptor != null)
					{
						object recurrenceValue = _recurrenceInfoDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
						if (recurrenceValue != null && recurrenceValue.GetType() == typeof(string))
						{
							string recurrenceText = (string)recurrenceValue;
							if (recurrenceText.Length > 0)
							{
								schedulerAppointment.DeferPatternDeserialization();
								try
								{
									schedulerAppointment.RecurrencePatternInfo = recurrenceText;
								}
								catch
								{
									schedulerAppointment.RecurrencePattern = null;
								}
								if (schedulerAppointment._recurrenceState == RecurrenceState.Master)
								{
									RecurrenceAppointments.AddIfRecurring(schedulerAppointment);
								}
							}
						}
					}
					schedulerAppointment._notifiesScheduler = true;
					if (_appointmentSettingsDescriptor != null)
					{
						object settingsValue = _appointmentSettingsDescriptor.GetValue(_schedulerCurrencyManager.List[i]);
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
					_appointments.AddUnchecked(schedulerAppointment);
					if (schedulerAppointment._recurrenceState == RecurrenceState.Exception)
					{
						schedulerAppointment._index = i;
						arrayList.Add(schedulerAppointment);
					}
				}
				if (arrayList != null && arrayList.Count > 0)
				{
					foreach (SchedulerAppointment item in arrayList)
					{
						if (item.GetRecurrenceException() != null)
						{
							SchedulerAppointment masterAppointment = RecurrenceAppointments.FindByRecurrenceId(item.GetRecurrenceException().GetRecurrencePatternId());
							if (masterAppointment != null)
							{
								item.GetRecurrenceException()._pattern = masterAppointment.RecurrencePattern;
								if (!masterAppointment.RecurrencePattern.Exceptions.Contains(item.GetRecurrenceException()))
								{
									masterAppointment.RecurrencePattern.Exceptions.AddException(item.GetRecurrenceException());
								}
								item.SetMasterAppointment(masterAppointment);
								continue;
							}
						}
						item.SetRecurrenceException(null);
						item._recurrenceState = RecurrenceState.NotRecurring;
					}
				}
				foreach (SchedulerAppointment recurringMaster in _recurrenceAppointmentCollection)
				{
					recurringMaster.ApplyDeferredPatternDeserialization();
				}
				for (int j = 0; j < _appointments.Count; j++)
				{
					if (j >= 0 && j < _appointments.Count)
					{
						SchedulerAppointment app = _appointments[j];
						AppointmentEventArgs e = new AppointmentEventArgs(app);
						OnAppointmentRead(e);
					}
				}
				RebuildFilteredAppointments();
				UpdateSchedule();
				_isAppointmentCacheStale = true;
				_isFullLayoutStale = true;
				UpdateLayout(null, null, EditType.None);
			}
			catch
			{
				SetCurrencyManager(null);
				_isReadingAppointment = false;
				throw;
			}
		}

		private void ShowHiddenAppointmentButton(DateTime date, int direction, Rectangle rectangle, HiddenAppointmentMode mode)
		{
			bool isNewButton = false;
			if (_usedMoreButtonCount >= _moreButtons.Count)
			{
				isNewButton = true;
			}
			SchedulerHiddenAppointmentButton moreButton = null;
			if (isNewButton)
			{
				moreButton = new SchedulerHiddenAppointmentButton(this, date, direction, mode);
				_moreButtons.Add(moreButton);
				base.Controls.Add(moreButton);
			}
			else
			{
				moreButton = (SchedulerHiddenAppointmentButton)_moreButtons[_usedMoreButtonCount];
				if (moreButton == null)
				{
					return;
				}
				moreButton._date = date;
				moreButton.UpdateDirection(direction);
				moreButton._mode = mode;
			}
			moreButton.Bounds = rectangle;
			moreButton.BringToFront();
			_usedMoreButtonCount++;
		}

		internal void ClearDataSourceRows()
		{
			if (_schedulerCurrencyManager != null)
			{
				int rowIndex;
				for (rowIndex = 0; rowIndex < _schedulerCurrencyManager.Count; rowIndex++)
				{
					_schedulerCurrencyManager.RemoveAt(rowIndex);
					rowIndex--;
				}
			}
			RaiseAppointmentsReset(EventArgs.Empty);
		}

		private void ScrollDayViewRows()
		{
			int scrollOffset = 0;
			if (_firstVisibleSlot == null)
			{
				return;
			}
			int targetSlotIndex = GetVerticalScrollBarControl().GetScrollValue() + GetDayStartSlotIndex();
			if (_firstVisibleSlot.GetIndex() == targetSlotIndex || _dayLayouts.Count <= 0)
			{
				return;
			}
			if (_firstVisibleSlot.GetIndex() < targetSlotIndex)
			{
				scrollOffset = _firstVisibleSlot._bounds.Height * (targetSlotIndex - _firstVisibleSlot.GetIndex());
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					foreach (SchedulerAppointmentOwnerLayoutState slotToShiftUp in item.GetTimeSlots())
					{
						slotToShiftUp._bounds.Y -= scrollOffset;
					}
				}
			}
			else
			{
				scrollOffset = _firstVisibleSlot._bounds.Height * (_firstVisibleSlot.GetIndex() - targetSlotIndex);
				foreach (SchedulerHourRangeCollectionLayoutState dayLayout in _dayLayouts)
				{
					foreach (SchedulerAppointmentOwnerLayoutState slotToShiftDown in dayLayout.GetTimeSlots())
					{
						slotToShiftDown._bounds.Y += scrollOffset;
					}
				}
			}
			_firstVisibleSlot = _dayLayouts.GetLayoutAt(0).GetTimeSlots().GetLayoutAt(targetSlotIndex);
			_firstVisibleSlotTime = new TimeSpan(0, _firstVisibleSlot.GetIndex() * _minuteInterval, 0);
			_isAppointmentLayoutStale = true;
			if (_allDayAreaControl != null)
			{
				_allDayAreaControl.BringToFront();
			}
			OnScroll(EventArgs.Empty);
			Invalidate();
		}

		internal void StartAppointmentDrag(SchedulerAppointment currentAppointment, DateTime dateTimeArgument, Point point)
		{
			_isDropAccepted = true;
			if (_dragType == DragType.Move)
			{
				if ((_allowAppointmentDrag & AllowAppointmentDrag.Move) == 0)
				{
					return;
				}
			}
			else if ((_allowAppointmentDrag & AllowAppointmentDrag.Resize) == 0)
			{
				return;
			}
			if (!_schedulerSelectedAppointments.Contains(currentAppointment))
			{
				_schedulerSelectedAppointments.Add(currentAppointment);
			}
			AppointmentDragType appointmentDragType = AppointmentDragType.Move;
			if (_dragType == DragType.ResizeLeft)
			{
				appointmentDragType = AppointmentDragType.StartTimeChange;
				if (!IsDragTimeAllowed(_schedulerSchedulerAppointment, appointmentDragType))
				{
					return;
				}
			}
			else if (_dragType == DragType.ResizeRight)
			{
				appointmentDragType = AppointmentDragType.EndTimeChange;
				if (!IsDragTimeAllowed(_schedulerSchedulerAppointment, appointmentDragType))
				{
					return;
				}
			}
			for (int i = 0; i < _schedulerSelectedAppointments.Count; i++)
			{
				SchedulerAppointment schedulerAppointment = _schedulerSelectedAppointments[i];
				AppointmentDragEventArgs e = new AppointmentDragEventArgs(schedulerAppointment, appointmentDragType);
				OnAppointmentDrag(e);
				if (e.Cancel)
				{
					_schedulerSelectedAppointments.Remove(schedulerAppointment);
					i--;
				}
			}
			if (_schedulerSelectedAppointments.Count == 0)
			{
				_schedulerSchedulerAppointment = null;
				return;
			}
			_hasDragChangedDates = false;
			ArrayList arrayList = new ArrayList(_dates);
			_dragOriginalStart = currentAppointment.StartTime;
			_dragOriginalEnd = currentAppointment.EndTime;
			_timeSpan = currentAppointment.Duration;
			_schedulerSchedulerAppointmentOwner = FindVisibleOwnerByValue(currentAppointment._ownerValue);
			_mouseDownPoint = Point.Empty;
			_dragAnchorTime = currentAppointment._startTime;
			_dragInitialAnchorTime = _dragAnchorTime;
			_isCopyDrag = false;
			SchedulerRenderer.FocusControl(this);
			_isColumnSplitterActive = false;
			_isAppointmentDragActive = true;
			_shouldRaiseChangeEvents = false;
			bool allowDrop = AllowDrop;
			_schedulerTimeSpan = new TimeSpan(0, 0, 0);
			SchedulerRenderer.SetAllowDrop(this, true);
			if (_dragType == DragType.Move)
			{
				if (_view == SchedulerView.DayView)
				{
					point = PointToClient(point);
					if (currentAppointment._duration.TotalDays < 1.0)
					{
						DateTime dateTime = GetDateTimeAtPoint(point);
						if (dateTime != DateTime.MinValue)
						{
							_schedulerTimeSpan = currentAppointment._startTime.Subtract(dateTime);
						}
					}
				}
				if (Control.ModifierKeys == Keys.Control && (_allowAppointmentDrag & AllowAppointmentDrag.Copy) != AllowAppointmentDrag.None)
				{
					_draggedAppointmentCopies = new ArrayList();
					foreach (SchedulerAppointment item in _schedulerSelectedAppointments)
					{
						item.SaveOriginalTimes(_schedulerSchedulerAppointment._startTime);
						SchedulerAppointment appointmentCopy = item.CreateDragCopy();
						AppointmentCopiedCancelEventArgs copyArgs = new AppointmentCopiedCancelEventArgs(item, appointmentCopy);
						OnCopyingAppointment(copyArgs);
						if (!copyArgs.Cancel)
						{
							if (item == _schedulerSchedulerAppointment)
							{
								_schedulerSchedulerAppointment = appointmentCopy;
							}
							_draggedAppointmentCopies.Add(appointmentCopy);
							_publicAppointments.AddValidated(appointmentCopy);
							CopyAppointmentFields(item, appointmentCopy);
							_isCopyDrag = true;
							_hasCopiesInCollection = true;
						}
					}
					if (_isCopyDrag)
					{
						_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
						foreach (SchedulerAppointment copiedAppointment in _draggedAppointmentCopies)
						{
							_schedulerSelectedAppointments.Add(copiedAppointment);
						}
						_isAppointmentCacheStale = true;
						_isAppointmentLayoutStale = true;
						Invalidate();
					}
				}
				else
				{
					foreach (SchedulerAppointment draggedAppointment in _schedulerSelectedAppointments)
					{
						draggedAppointment.SaveOriginalTimes(_schedulerSchedulerAppointment._startTime);
					}
				}
				AppointmentDragHelper data = new AppointmentDragHelper(_schedulerSchedulerAppointment);
				DoDragDrop(data, DragDropEffects.All);
			}
			else
			{
				_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
				_currentAppointment = currentAppointment;
				UpdateSchedulerSelectedAppointments(currentAppointment);
				currentAppointment.SaveOriginalTimes(_schedulerSchedulerAppointment._startTime);
				AppointmentDragHelper resizeDragData = new AppointmentDragHelper(_schedulerSchedulerAppointment);
				DoDragDrop(resizeDragData, DragDropEffects.Move);
			}
			if (GetHoveredAppointmentView() != null)
			{
				GetHoveredAppointmentView()._isMouseDown = false;
			}
			if (!_isDropAccepted)
			{
				_shouldRaiseChangeEvents = false;
				ArrayList selectedSnapshot = new ArrayList();
				foreach (SchedulerAppointment selectedAppointment in _schedulerSelectedAppointments)
				{
					selectedSnapshot.Add(selectedAppointment);
				}
				foreach (SchedulerAppointment snapshotAppointment in selectedSnapshot)
				{
					snapshotAppointment.RestoreOriginalTimes(true);
				}
				_shouldRaiseChangeEvents = true;
			}
			SchedulerRenderer.SetAllowDrop(this, allowDrop);
			_mouseDownPoint = Point.Empty;
			_isAppointmentDragActive = false;
			_shouldRaiseChangeEvents = true;
			StopAutoScrollTimer();
			CommitAppointmentDrag();
			if (IsMultiOwnerActive() && _schedulerSchedulerAppointment.Owner != null)
			{
				_shouldClearSelectionOnOwnerChange = false;
				CurrentOwner = FindVisibleOwnerByValue(_schedulerSchedulerAppointment.Owner);
				_shouldClearSelectionOnOwnerChange = true;
			}
			if (_hasDragChangedDates && HasDateMissingFromView(arrayList))
			{
				_dates.ClearWithoutNotify();
				foreach (DateTime originalDate in arrayList)
				{
					_dates.AddWithoutNotify(originalDate);
				}
				RefreshAfterDatesChange();
				InvalidateAppointmentLayout();
			}
			if (_dragType == DragType.Move)
			{
				_currentAppointment = null;
				_schedulerSelectedAppointments.Clear();
			}
			_hasCopiesInCollection = false;
			_isFullLayoutStale = true;
			if (_view == SchedulerView.DayView)
			{
				if ((int)_schedulerSchedulerAppointment._duration.TotalDays < 1)
				{
					_selectionAnchor = _schedulerSchedulerAppointment._startTime;
					_selectionCursor = _selectionAnchor;
					UpdateSelectedDays(new DateRange(_selectionAnchor, _schedulerSchedulerAppointment._endTime));
				}
				else
				{
					_selectionAnchor = _schedulerSchedulerAppointment._startTime.Date.Add(_firstVisibleSlotTime);
					_selectionCursor = _selectionAnchor;
					UpdateSelectedDays(new DateRange(_selectionAnchor, _selectionAnchor.AddMinutes(_minuteInterval)));
				}
			}
			else
			{
				_selectionAnchor = _schedulerSchedulerAppointment._startTime.Date;
				_selectionCursor = _selectionAnchor;
				if (_schedulerSchedulerAppointment._endTime.TimeOfDay.TotalMinutes > 0.0)
				{
					UpdateSelectedDays(new DateRange(_selectionAnchor, _schedulerSchedulerAppointment._endTime.Date.AddDays(1.0)));
				}
				else
				{
					UpdateSelectedDays(new DateRange(_selectionAnchor, _schedulerSchedulerAppointment._endTime.Date));
				}
			}
			EnsureSelectionInRange();
			if (_view == SchedulerView.DayView)
			{
				RenderCurrentView(null);
			}
			Invalidate();
			_isCopyDrag = false;
			_draggedAppointmentCopies = null;
			_hasRemovedCopies = false;
			_schedulerSchedulerAppointment = null;
		}

		private void DisposeUnusedHiddenAppointmentButtons()
		{
			if (_usedMoreButtonCount < _moreButtons.Count)
			{
				int buttonIndex;
				for (buttonIndex = _usedMoreButtonCount; buttonIndex < _moreButtons.Count; buttonIndex++)
				{
					SchedulerHiddenAppointmentButton hiddenAppointmentButton = (SchedulerHiddenAppointmentButton)_moreButtons[buttonIndex];
					hiddenAppointmentButton.Dispose();
					hiddenAppointmentButton = null;
					_moreButtons.RemoveAt(buttonIndex);
					buttonIndex--;
				}
			}
		}

		protected virtual void OnAppointmentRead(AppointmentEventArgs e)
		{
			if (base.Events[_appointmentReadEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentAdded(AppointmentEventArgs e)
		{
			if (base.Events[_appointmentAddedEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		protected virtual void OnRecurrentAppointmentRemoved(AppointmentEventArgs e)
		{
			if (base.Events[_recurrentAppointmentRemovedEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentRemoved(AppointmentEventArgs e)
		{
			if (base.Events[_appointmentRemovedEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentChanging(AppointmentChangeCancelEventArgs e)
		{
			if (base.Events[_appointmentChangingEventKey] is AppointmentChangeCancelEventHandler appointmentChangeCancelEventHandler)
			{
				appointmentChangeCancelEventHandler(this, e);
			}
		}

		internal void NotifyAppointmentChanged(SchedulerAppointment app, AppointmentChange propertyChanged)
		{
			if (_isAppointmentDragActive || !_shouldRaiseChangeEvents)
			{
				return;
			}
			AppointmentChangeEventArgs e = new AppointmentChangeEventArgs(app, propertyChanged);
			OnAppointmentChanged(e);
			if (e.Appointment.Reminder.Enabled)
			{
				switch (e.PropertyChanged)
				{
				case AppointmentChange.StartTime:
				case AppointmentChange.StartTimeEndTime:
				case AppointmentChange.ReminderInterval:
				case AppointmentChange.ReminderEnabled:
				case AppointmentChange.RecurrencePattern:
					RecalculateReminder(app);
					break;
				case AppointmentChange.EndTime:
				case AppointmentChange.Description:
				case AppointmentChange.Owner:
					break;
				}
			}
			else if (e.PropertyChanged == AppointmentChange.ReminderEnabled)
			{
				RecalculateReminder(app);
			}
		}

		protected virtual void OnAppointmentChanged(AppointmentChangeEventArgs e)
		{
			if (base.Events[_appointmentChangedEventKey] is AppointmentChangeEventHandler appointmentChangeEventHandler)
			{
				appointmentChangeEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentsLoaded(EventArgs e)
		{
			if (base.Events[_appointmentsLoadedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnAddingAppointment(AddAppointmentEventArgs e)
		{
			if (base.Events[_addingAppointmentEventKey] is AddAppointmentEventHandler addAppointmentEventHandler)
			{
				addAppointmentEventHandler(this, e);
			}
		}

		protected virtual void OnCopyingAppointment(AppointmentCopiedCancelEventArgs e)
		{
			if (base.Events[_copyingAppointmentEventKey] is AppointmentCopiedCancelEventHandler appointmentCopiedCancelEventHandler)
			{
				appointmentCopiedCancelEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentCopied(AppointmentCopiedEventArgs e)
		{
			if (base.Events[_appointmentCopiedEventKey] is AppointmentCopiedEventHandler appointmentCopiedEventHandler)
			{
				appointmentCopiedEventHandler(this, e);
			}
		}

		internal void RaiseAppointmentDoubleClick(SchedulerAppointment app)
		{
			AppointmentEventArgs e = new AppointmentEventArgs(app);
			OnAppointmentDoubleClick(e);
		}

		protected virtual void OnAppointmentDoubleClick(AppointmentEventArgs e)
		{
			if (base.Events[_appointmentDoubleClickEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		internal void RaiseHiddenAppointmentClick(DateTime date)
		{
			HiddenAppointmentsEventArgs e = new HiddenAppointmentsEventArgs(date, _view);
			OnHiddenAppointmentClick(e);
		}

		protected virtual void OnHiddenAppointmentClick(HiddenAppointmentsEventArgs e)
		{
			if (base.Events[_hiddenAppointmentClickEventKey] is HiddenAppointmentsEventHandler hiddenAppointmentsEventHandler)
			{
				hiddenAppointmentsEventHandler(this, e);
			}
		}

		protected virtual void OnValidatingAppointmentText(ValidatingAppointmentTextEventArgs e)
		{
			if (base.Events[_validatingAppointmentTextEventKey] is ValidatingAppointmentTextEventHandler validatingAppointmentTextEventHandler)
			{
				validatingAppointmentTextEventHandler(this, e);
			}
		}

		internal bool RaiseEditingAppointment(SchedulerAppointment app, string text)
		{
			if (!_allowEdit)
			{
				return false;
			}
			AppointmentCancelEventArgs e = new AppointmentCancelEventArgs(app);
			OnEditingAppointment(e);
			return !e.Cancel;
		}

		protected virtual void OnEditingAppointment(AppointmentCancelEventArgs e)
		{
			if (base.Events[_editingAppointmentEventKey] is AppointmentCancelEventHandler appointmentCancelEventHandler)
			{
				appointmentCancelEventHandler(this, e);
			}
		}

		protected virtual void OnDroppingAppointment(DroppingAppointmentEventArgs e)
		{
			if (base.Events[_droppingAppointmentEventKey] is DroppingAppointmentEventHandler droppingAppointmentEventHandler)
			{
				droppingAppointmentEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentDropped(AppointmentEventArgs e)
		{
			if (base.Events[_appointmentDroppedEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentDrag(AppointmentDragEventArgs e)
		{
			if (base.Events[_appointmentDragEventKey] is AppointmentDragEventHandler appointmentDragEventHandler)
			{
				appointmentDragEventHandler(this, e);
			}
		}

		protected virtual void OnAppointmentEdited(AppointmentEventArgs e)
		{
			if (base.Events[_appointmentEditedEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		protected virtual void OnRemovingAppointment(AppointmentCancelEventArgs e)
		{
			if (base.Events[_removingAppointmentEventKey] is AppointmentCancelEventHandler appointmentCancelEventHandler)
			{
				appointmentCancelEventHandler(this, e);
			}
		}

		protected virtual void OnRemovingRecurrentAppointment(RemovingRecurrentAppointmentEventArgs e)
		{
			if (base.Events[_removingRecurrentAppointmentEventKey] is RemovingRecurrentAppointmentEventHandler removingRecurrentAppointmentEventHandler)
			{
				removingRecurrentAppointmentEventHandler(this, e);
			}
		}

		protected virtual void OnSelectedAppointmentsChanged(EventArgs e)
		{
			if (base.Events[_selectedAppointmentsChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal void RaiseAppointmentRead(AppointmentEventArgs eventArgs)
		{
			OnAppointmentRead(eventArgs);
		}
	}
}
