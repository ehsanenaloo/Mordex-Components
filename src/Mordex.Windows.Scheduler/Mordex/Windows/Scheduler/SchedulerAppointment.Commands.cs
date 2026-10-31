using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerAppointment
	{
		internal SchedulerAppointment CreateOccurrence(DateTime startTime, DateTime schedulerAppointmentDateTime)
		{
			SchedulerAppointment schedulerAppointment = CreateRecurrentInstance(startTime, schedulerAppointmentDateTime);
			schedulerAppointment.Move(startTime, schedulerAppointmentDateTime);
			schedulerAppointment._allDayEvent = _allDayEvent;
			schedulerAppointment._borderColor = _borderColor;
			schedulerAppointment._description = _description;
			schedulerAppointment._firstImageIndex = _firstImageIndex;
			schedulerAppointment._secondImageIndex = _secondImageIndex;
			schedulerAppointment._thirdImageIndex = _thirdImageIndex;
			schedulerAppointment._fourthImageIndex = _fourthImageIndex;
			schedulerAppointment._firstImageKey = _firstImageKey;
			schedulerAppointment._secondImageKey = _secondImageKey;
			schedulerAppointment._thirdImageKey = _thirdImageKey;
			schedulerAppointment._fourthImageKey = _fourthImageKey;
			schedulerAppointment._text = _text;
			schedulerAppointment._reminder = _reminder.CloneDetached();
			schedulerAppointment._reminder._appointment = schedulerAppointment;
			schedulerAppointment._visible = _visible;
			if (_formatStyle != null)
			{
				schedulerAppointment._formatStyle = _formatStyle.CloneAsCustomStyle();
				schedulerAppointment._formatStyle._owner = schedulerAppointment;
			}
			schedulerAppointment._prefix = _prefix;
			schedulerAppointment._tag = _tag;
			schedulerAppointment._ownerValue = _ownerValue;
			if (schedulerAppointment._reminder != null && schedulerAppointment._reminder.Enabled)
			{
				DateTime reminderDateTime = schedulerAppointment.Reminder.GetReminderDateTime();
				if (reminderDateTime <= RecurrencePattern.GetEffectiveReminderDismissTime())
				{
					schedulerAppointment._reminder._enabled = false;
				}
			}
			schedulerAppointment._recurrenceState = RecurrenceState.Occurrence;
			schedulerAppointment._timeSpan = _timeSpan;
			schedulerAppointment._savedEndTime = schedulerAppointmentDateTime;
			schedulerAppointment._value = _ownerValue;
			schedulerAppointment._savedStartTime = _startTime;
			schedulerAppointment._dragStartOffset = _dragStartOffset;
			return schedulerAppointment;
		}

		protected virtual SchedulerAppointment CreateRecurrentInstance(DateTime startTime, DateTime endTime)
		{
			if (Scheduler != null)
			{
				return Scheduler.BuildAppointment(startTime, endTime, "");
			}
			return new SchedulerAppointment(startTime, endTime);
		}

		internal void ApplyDeferredPatternDeserialization()
		{
			if (_isPatternDeserializationDeferred)
			{
				_isPatternDeserializationDeferred = false;
				if (_recurrencePattern != null)
				{
					_recurrencePattern.DeserializeExceptions();
				}
			}
		}

		internal void SetLayoutTop(int top)
		{
			_layoutTop = top;
		}

		internal void SetLayoutHeight(int schedulerAppointmentInteger)
		{
			_layoutHeight = schedulerAppointmentInteger;
		}

		private void RefreshAppearance(bool syncDataSource)
		{
			if (Scheduler != null)
			{
				_scheduler.InvalidateWithChildren();
				if (syncDataSource)
				{
					Scheduler.WriteSettingsToDataSource(this);
				}
			}
		}

		internal void ApplyMove(DateTime startTime, DateTime endTime, object ownerValue, bool shouldSetOwner)
		{
			if (endTime < startTime)
			{
				throw new InvalidOperationException();
			}
			if (_startTime == startTime && _endTime == endTime && _ownerValue == ownerValue)
			{
				return;
			}
			AppointmentChange appointmentChange = AppointmentChange.StartTimeEndTime;
			TimeSpan value = endTime.Subtract(startTime);
			AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(this, appointmentChange, startTime);
			_isBeingMoved = true;
			RaiseAppointmentChanging(e);
			if (!e.Cancel && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
			{
				if (shouldSetOwner)
				{
					_ownerValue = ownerValue;
				}
				startTime = (DateTime)e.ProposedValue;
				endTime = startTime.Add(value);
				if ((startTime.Date != _startTime.Date || _startTime.TimeOfDay == TimeSpan.Zero || endTime.Date != _endTime.Date || _endTime.TimeOfDay == TimeSpan.Zero) && Scheduler != null)
				{
					Scheduler._isAppointmentCacheStale = true;
				}
				_startTime = startTime;
				_endTime = endTime;
				if (Duration.TotalDays >= 1.0 && startTime.TimeOfDay == TimeSpan.Zero && endTime.TimeOfDay == TimeSpan.Zero)
				{
					AllDayEvent = true;
				}
				if (Scheduler != null)
				{
					if (Scheduler.IsFieldBound(AppointmentChange.StartTime) || Scheduler.IsFieldBound(AppointmentChange.EndTime) || Scheduler.IsFieldBound(AppointmentChange.Owner))
					{
						UpdateSchedulerAppointment();
					}
					else
					{
						InvalidateLayout();
					}
				}
				RaiseAppointmentChanged(appointmentChange);
			}
			_isBeingMoved = false;
		}

		public void Move(DateTime startTime, DateTime endTime)
		{
			ApplyMove(startTime, endTime, _ownerValue, false);
		}

		public void SetValue(SchedulerField field, object value)
		{
			if (_fieldValues != null)
			{
				((object[])_fieldValues)[field.Index] = value;
			}
			else if (Scheduler != null && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
			{
				Scheduler.SetFieldValue(this, field, value);
				Scheduler.UpdateLayout(_parentCollection, this, EditType.Change);
			}
		}

		public void SetValue(int fieldIndex, object value)
		{
			if (_fieldValues != null)
			{
				((object[])_fieldValues)[fieldIndex] = value;
			}
			else if (Scheduler != null && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
			{
				Scheduler.SetFieldValueByIndex(this, fieldIndex, value);
				Scheduler.UpdateLayout(_parentCollection, this, EditType.Change);
			}
		}

		public void SetValue(string fieldKey, object value)
		{
			if (_fieldValues != null)
			{
				((object[])_fieldValues)[Scheduler.Fields[fieldKey].Index] = value;
				return;
			}
			if (Scheduler != null)
			{
				if (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException())
				{
					if (!IsDataRowEditStarted() && DataRow is IEditableObject)
					{
						((IEditableObject)DataRow).BeginEdit();
					}
					Scheduler.SetFieldValueByKey(this, fieldKey, value);
					Scheduler.UpdateLayout(_parentCollection, this, EditType.Change);
					if (!IsDataRowEditStarted() && DataRow is IEditableObject)
					{
						((IEditableObject)DataRow).EndEdit();
					}
				}
				return;
			}
			throw new Exception("The appointment must be added to the Appointment collection of the Schedule before using SetValue method");
		}

		public void Delete()
		{
			if (_recurrenceState == RecurrenceState.Exception || RecurrenceState == RecurrenceState.Occurrence)
			{
				if (Scheduler != null)
				{
					Scheduler.DeleteRecurrenceOccurrence(this);
				}
				return;
			}
			if (_recurrenceState == RecurrenceState.Master && _recurrencePattern != null)
			{
				RecurrencePattern.Exceptions.Clear();
				if (Scheduler != null)
				{
					_scheduler._isAppointmentCacheStale = true;
				}
			}
			if (Scheduler != null)
			{
				if (Scheduler.Appointments.Contains(this))
				{
					Scheduler.Appointments.Remove(this);
				}
				else if (_parentCollection != null && !_parentCollection._isPublic && Scheduler.CanRemoveAppointment(this))
				{
					_parentCollection.Remove(this);
				}
			}
		}
	}
}
