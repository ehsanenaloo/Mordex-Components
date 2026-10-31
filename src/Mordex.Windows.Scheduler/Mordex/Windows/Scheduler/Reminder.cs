using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class Reminder : IMordexXmlLayoutsSupport
	{
		internal SchedulerAppointment _appointment;

		private bool _active = true;

		private DateTime _snoozeTime = DateTime.MaxValue;

		internal bool _enabled;

		private bool _snoozed;

		private TimeSpan _snoozedInterval = new TimeSpan(0, 5, 0);

		private TimeSpan _reminderInterval = new TimeSpan(0, 15, 0);

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public SchedulerAppointment Appointment => _appointment;

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled == value)
				{
					return;
				}
				AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(_appointment, AppointmentChange.ReminderEnabled, value);
				_appointment.RaiseAppointmentChanging(e);
				if (!e.Cancel)
				{
					_enabled = value;
					bool shouldCreateException = true;
					if (!value && _appointment.IsRecurring)
					{
						shouldCreateException = ShouldCreateException();
					}
					if (_appointment._recurrenceState != RecurrenceState.Occurrence)
					{
						shouldCreateException = false;
					}
					OnReminderChanged(shouldCreateException, false);
					_snoozed = false;
					_appointment.RaiseAppointmentChanged(AppointmentChange.ReminderEnabled);
				}
			}
		}

		public bool Snoozed => _snoozed;

		public TimeSpan SnoozedInterval
		{
			get
			{
				return _snoozedInterval;
			}
			set
			{
				if (_snoozedInterval != value)
				{
					_snoozedInterval = value;
					OnReminderChanged(true, true);
				}
			}
		}

		public TimeSpan ReminderInterval
		{
			get
			{
				return _reminderInterval;
			}
			set
			{
				if (_reminderInterval != value)
				{
					AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(_appointment, AppointmentChange.ReminderInterval, value);
					_appointment.RaiseAppointmentChanging(e);
					if (!e.Cancel)
					{
						value = (TimeSpan)e.ProposedValue;
						_reminderInterval = value;
						OnReminderChanged(true, false);
						_appointment.RaiseAppointmentChanged(AppointmentChange.ReminderInterval);
					}
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), false);
			_reminderInterval = (TimeSpan)LayoutReader.GetPropertyValue(info, "ReminderInterval", typeof(TimeSpan), new TimeSpan(0, 15, 0));
			_snoozedInterval = (TimeSpan)LayoutReader.GetPropertyValue(info, "SnoozedInterval", typeof(TimeSpan), new TimeSpan(0, 5, 0));
			_active = (bool)LayoutReader.GetPropertyValue(info, "Active", typeof(bool), true);
			if (_active)
			{
				_snoozed = (bool)LayoutReader.GetPropertyValue(info, "Snoozed", typeof(bool), false);
				_snoozeTime = (DateTime)LayoutReader.GetPropertyValue(info, "SnoozeTime", typeof(DateTime), DateTime.MaxValue);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_enabled)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (_reminderInterval != new TimeSpan(0, 15, 0))
			{
				info.AddValue("ReminderInterval", _reminderInterval);
			}
			if (_snoozedInterval != new TimeSpan(0, 5, 0))
			{
				info.AddValue("SnoozedInterval", _snoozedInterval);
			}
			if (!_active)
			{
				info.AddValue("Active", _active);
			}
			if (_active)
			{
				if (_snoozed)
				{
					info.AddValue("Snoozed", _snoozed);
				}
				if (_snoozeTime != DateTime.MaxValue)
				{
					info.AddValue("SnoozeTime", _snoozeTime);
				}
			}
		}

		public Reminder(SchedulerAppointment appointment)
		{
			if (appointment == null)
			{
				throw new ArgumentNullException();
			}
			_appointment = appointment;
		}

		private void OnReminderChanged(bool shouldCreateException, bool booleanArgument)
		{
			_appointment.WriteChangesToDataSource(shouldCreateException, booleanArgument);
		}

		internal bool HasNonDefaultSettings()
		{
			if (_enabled)
			{
				return true;
			}
			if (_reminderInterval != new TimeSpan(0, 15, 0))
			{
				return true;
			}
			if (_snoozedInterval != new TimeSpan(0, 5, 0))
			{
				return true;
			}
			if (!_active)
			{
				return true;
			}
			if (_active)
			{
				if (_snoozed)
				{
					return true;
				}
				if (_snoozeTime != DateTime.MaxValue)
				{
					return true;
				}
			}
			return false;
		}

		internal Reminder CloneDetached()
		{
			Reminder reminder = (Reminder)MemberwiseClone();
			reminder._appointment = null;
			return reminder;
		}

		public void Snooze()
		{
			_snoozed = true;
			_snoozeTime = DateTime.Now.Add(_snoozedInterval);
			OnReminderChanged(true, true);
		}

		internal bool GetActive()
		{
			return _active;
		}

		internal void UpdateActive(bool active)
		{
			if (_active != active)
			{
				_active = active;
			}
		}

		public DateTime GetReminderDateTime()
		{
			if (_snoozed)
			{
				return _snoozeTime;
			}
			return _appointment.StartTime.Subtract(_reminderInterval);
		}

		private bool ShouldCreateException()
		{
			DateTime dateTime = _appointment.StartTime.Subtract(ReminderInterval);
			if (_appointment.RecurrencePattern != null && dateTime <= DateTime.Now && dateTime >= _appointment.RecurrencePattern.GetEffectiveReminderDismissTime())
			{
				_appointment.RecurrencePattern.UpdateReminderDismissTime(DateTime.Now);
				return false;
			}
			return true;
		}
	}
}
