using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerAppointment
	{
		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected != value)
				{
					if (Scheduler != null && _selected)
					{
						_scheduler.UpdateCurrentAppointment(this);
					}
					_selected = value;
					if (Scheduler != null && _selected)
					{
						_scheduler.UpdateSchedulerSelectedAppointments(this);
					}
				}
			}
		}

		public RecurrenceState RecurrenceState => _recurrenceState;

		public bool IsRecurring
		{
			get
			{
				if (_recurrenceState != RecurrenceState.NotRecurring)
				{
					return true;
				}
				return false;
			}
		}

		public string SettingsInfo
		{
			get
			{
				if (HasCustomSettings())
				{
					PropertyValue root = new PropertyValue("AppointmentLayoutInfo", this);
					_includesCoreFields = false;
					LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
					string result = mordexLayoutWriter.ToString();
					_includesCoreFields = true;
					return result;
				}
				return "";
			}
			set
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
				PropertyValue propertyValue = mordexLayoutReader["AppointmentLayoutInfo"];
				if (propertyValue != null)
				{
					_includesCoreFields = false;
					propertyValue.DeserializeObject(this);
					_includesCoreFields = true;
				}
			}
		}

		public string RecurrencePatternInfo
		{
			get
			{
				switch (_recurrenceState)
				{
				case RecurrenceState.Master:
					return RecurrencePattern.ToString();
				case RecurrenceState.Exception:
					if (_recurrenceException != null)
					{
						return GetRecurrenceException().ToLayoutString();
					}
					return "";
				default:
					return "";
				}
			}
			set
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
				PropertyValue propertyValue = mordexLayoutReader["RecurrencePattern"];
				if (propertyValue != null)
				{
					RecurrencePattern recurrencePattern = _recurrencePattern;
					if (recurrencePattern == null)
					{
						recurrencePattern = new RecurrencePattern();
					}
					recurrencePattern._isDeserializationDeferred = _isPatternDeserializationDeferred;
					recurrencePattern._owner = this;
					propertyValue.DeserializeObject(recurrencePattern);
					RecurrencePattern = recurrencePattern;
					return;
				}
				propertyValue = mordexLayoutReader["RecurrenceException"];
				if (propertyValue != null)
				{
					_isPatternDeserializationDeferred = false;
					_recurrenceException = new RecurrenceException();
					_recurrenceException.Appointment = this;
					_recurrenceException._shouldDeserializeAppointment = false;
					propertyValue.DeserializeObject(_recurrenceException);
					_recurrenceException._shouldDeserializeAppointment = true;
					_recurrenceState = RecurrenceState.Exception;
				}
				else
				{
					_recurrenceState = RecurrenceState.NotRecurring;
				}
			}
		}

		public SchedulerAppointment MasterAppointment => _masterAppointment;

		public RecurrencePattern RecurrencePattern
		{
			get
			{
				if (IsRecurring)
				{
					if (MasterAppointment != null)
					{
						return MasterAppointment.RecurrencePattern;
					}
					if (GetRecurrenceException() != null && _recurrencePattern == null)
					{
						return GetRecurrenceException().RecurrencePattern;
					}
				}
				return _recurrencePattern;
			}
			set
			{
				if (RecurrencePattern == value)
				{
					return;
				}
				if (MasterAppointment != null)
				{
					MasterAppointment.RecurrencePattern = value;
					return;
				}
				if (value != null && value.MasterAppointment != null && value.MasterAppointment != this)
				{
					throw new InvalidOperationException("The RecurrencePattern can not belog to two different appointments");
				}
				if (_recurrencePattern != null)
				{
					_recurrencePattern.Exceptions.Clear();
				}
				if (value == null)
				{
					_recurrenceState = RecurrenceState.NotRecurring;
				}
				else
				{
					_recurrenceState = RecurrenceState.Master;
				}
				_recurrencePattern = value;
				if (_recurrencePattern != null)
				{
					if (_reminder != null && _recurrencePattern.GetEffectiveReminderDismissTime() == DateTime.MinValue)
					{
						_recurrencePattern.UpdateReminderDismissTime(DateTime.Today);
					}
					if (_recurrencePattern.PatternStartDate == DateTime.MinValue)
					{
						_recurrencePattern.PatternStartDate = StartTime.Date;
					}
					if (_recurrencePattern.DayOfWeek == (SchedulerDayOfWeek)0)
					{
						_recurrencePattern.DayOfWeek = SchedulerRenderer.ToSchedulerDayOfWeek(StartTime.Date.DayOfWeek);
					}
					if (_recurrencePattern.StartTime == TimeSpan.Zero && RecurrencePattern.EndTime == TimeSpan.Zero)
					{
						_recurrencePattern.BeginEdit();
						_recurrencePattern.StartTime = StartTime.TimeOfDay;
						_recurrencePattern.Duration = EndTime.Subtract(StartTime);
						_recurrencePattern.EndEdit();
					}
					_recurrencePattern._owner = this;
				}
				UpdateRecurrenceLayout();
				if (_scheduler != null && _notifiesScheduler)
				{
					_scheduler.NotifyAppointmentChanged(this, AppointmentChange.RecurrencePattern);
				}
				if (_recurrencePattern == null || _recurrencePattern.Exceptions.Count <= 0 || Scheduler == null)
				{
					return;
				}
				foreach (RecurrenceException exception in _recurrencePattern.Exceptions)
				{
					Scheduler.AddToAppointments(exception.Appointment);
				}
			}
		}

		[Browsable(false)]
		public MxScheduler Scheduler
		{
			get
			{
				if (_parentCollection != null)
				{
					_scheduler = _parentCollection.Scheduler;
					return _scheduler;
				}
				if (_scheduler != null)
				{
					return _scheduler;
				}
				if (_masterAppointment != null)
				{
					return _masterAppointment.Scheduler;
				}
				return null;
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (!(_description != value))
				{
					return;
				}
				if (Scheduler != null)
				{
					if (!Scheduler.ValidateAppointmentChange(AppointmentChange.Description))
					{
						return;
					}
					AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(this, AppointmentChange.Description, value);
					_scheduler.RaiseAppointmentChanging(e);
					if (!e.Cancel)
					{
						value = (string)e.ProposedValue;
						_description = value;
						if (Scheduler.IsFieldBound(AppointmentChange.Description))
						{
							UpdateSchedulerAppointment();
						}
						else
						{
							InvalidateLayout();
						}
						_scheduler.NotifyAppointmentChanged(this, AppointmentChange.Description);
					}
				}
				else
				{
					_description = value;
				}
			}
		}

		public TimeSpan Duration
		{
			get
			{
				try
				{
					if (_endTime <= _startTime)
					{
						return TimeSpan.Zero;
					}
					_duration = _endTime.Subtract(_startTime);
					return _duration;
				}
				catch
				{
					return TimeSpan.Zero;
				}
			}
			set
			{
				if (_duration != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_duration = value;
					EndTime = _startTime.AddTicks(_duration.Ticks);
					RefreshAppearance(false);
				}
			}
		}

		public int Index => _collectionIndex;

		[DefaultValue("")]
		public string Prefix
		{
			get
			{
				return _prefix;
			}
			set
			{
				if (_prefix != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_prefix = value;
					RefreshAppearance(true);
				}
			}
		}

		public bool AllDayEvent
		{
			get
			{
				if (_allDayEvent && (_startTime.TimeOfDay != TimeSpan.Zero || _endTime.TimeOfDay != TimeSpan.Zero))
				{
					return false;
				}
				return _allDayEvent;
			}
			set
			{
				if ((AllDayEvent == value && !value) || (_recurrenceState == RecurrenceState.Occurrence && !EnsureRecurrenceException()))
				{
					return;
				}
				_allDayEvent = value;
				if (_allDayEvent)
				{
					_startTime = _startTime.Date;
					if (EndTime.TimeOfDay != TimeSpan.Zero)
					{
						EndTime = _endTime.Date.AddDays(1.0);
					}
					else if (EndTime <= _startTime)
					{
						EndTime = _startTime.Date.AddDays(1.0);
					}
				}
				else if (Scheduler != null)
				{
					EndTime = _startTime.AddMinutes(_scheduler.MinuteInterval);
				}
				else
				{
					EndTime = _startTime.AddMinutes(30.0);
				}
				RefreshAppearance(true);
			}
		}

		[Browsable(false)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				if (_tag != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_tag = value;
				}
			}
		}

		public DateTime EndTime
		{
			get
			{
				return _endTime;
			}
			set
			{
				if (!(_endTime != value))
				{
					return;
				}
				if (value < _startTime && !MxScheduler._isUpdatingTimeRange)
				{
					throw new InvalidOperationException("The end time must be bigger than the start time");
				}
				if (Scheduler != null && !Scheduler.ValidateAppointmentChange(AppointmentChange.EndTime))
				{
					return;
				}
				if (_recurrenceState == RecurrenceState.Occurrence)
				{
					DateTime dateTime = _endTime;
					_endTime = value;
					if (!EnsureRecurrenceException())
					{
						_endTime = dateTime;
						return;
					}
					if (Scheduler == null)
					{
						return;
					}
					_endTime = dateTime;
				}
				AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(this, AppointmentChange.EndTime, value);
				RaiseAppointmentChanging(e);
				if (e.Cancel)
				{
					return;
				}
				value = (DateTime)e.ProposedValue;
				if (value < _startTime && !MxScheduler._isUpdatingTimeRange)
				{
					throw new InvalidOperationException();
				}
				if ((value.Date != _endTime.Date || _endTime.Date.TimeOfDay == TimeSpan.Zero) && Scheduler != null)
				{
					Scheduler._isAppointmentCacheStale = true;
				}
				if (_recurrenceState == RecurrenceState.Master)
				{
					RecurrencePattern.Exceptions.Clear();
					if (Scheduler != null)
					{
						Scheduler._isAppointmentCacheStale = true;
					}
				}
				_endTime = value;
				_duration = new TimeSpan(_endTime.Ticks - _startTime.Ticks);
				if (Scheduler != null && Scheduler.IsFieldBound(AppointmentChange.EndTime))
				{
					UpdateSchedulerAppointment();
				}
				else
				{
					InvalidateLayout();
				}
				RaiseAppointmentChanged(AppointmentChange.EndTime);
			}
		}

		public object Owner
		{
			get
			{
				return _ownerValue;
			}
			set
			{
				if (((_ownerValue == value || _ownerValue != null) && (_ownerValue == null || _ownerValue.Equals(value))) || (Scheduler != null && !Scheduler.ValidateAppointmentChange(AppointmentChange.Owner)))
				{
					return;
				}
				if (_recurrenceState == RecurrenceState.Occurrence)
				{
					object previousOwner = _ownerValue;
					_ownerValue = value;
					if (!EnsureRecurrenceException())
					{
						_ownerValue = previousOwner;
						return;
					}
					if (Scheduler == null)
					{
						return;
					}
					_ownerValue = previousOwner;
				}
				AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(this, AppointmentChange.Owner, value);
				RaiseAppointmentChanging(e);
				if (e.Cancel)
				{
					return;
				}
				value = e.ProposedValue;
				_ownerValue = value;
				if (Scheduler != null)
				{
					_scheduler._isAppointmentCacheStale = true;
					if (Scheduler.IsFieldBound(AppointmentChange.Owner))
					{
						UpdateSchedulerAppointment();
					}
					else
					{
						InvalidateLayout();
					}
					RaiseAppointmentChanged(AppointmentChange.Owner);
				}
			}
		}

		public DateTime StartTime
		{
			get
			{
				return _startTime;
			}
			set
			{
				if (!(_startTime != value))
				{
					return;
				}
				if (value > _endTime && !MxScheduler._isUpdatingTimeRange)
				{
					throw new InvalidOperationException("The end time must be bigger than the start time");
				}
				if (Scheduler != null && !Scheduler.ValidateAppointmentChange(AppointmentChange.StartTime))
				{
					return;
				}
				if (_recurrenceState == RecurrenceState.Occurrence)
				{
					DateTime dateTime = _startTime;
					if (!EnsureRecurrenceException())
					{
						return;
					}
					_startTime = value;
					if (Scheduler == null)
					{
						return;
					}
					_startTime = dateTime;
				}
				AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(this, AppointmentChange.StartTime, value);
				RaiseAppointmentChanging(e);
				if (e.Cancel)
				{
					return;
				}
				value = (DateTime)e.ProposedValue;
				if (value > _endTime && !MxScheduler._isUpdatingTimeRange)
				{
					throw new InvalidOperationException();
				}
				if (value.Date != _startTime.Date && Scheduler != null)
				{
					Scheduler._isAppointmentCacheStale = true;
				}
				if (_recurrenceState == RecurrenceState.Master)
				{
					RecurrencePattern.Exceptions.Clear();
					if (Scheduler != null)
					{
						Scheduler._isAppointmentCacheStale = true;
					}
				}
				_startTime = value;
				_duration = new TimeSpan(_endTime.Ticks - _startTime.Ticks);
				if (Scheduler != null && Scheduler.IsFieldBound(AppointmentChange.StartTime))
				{
					UpdateSchedulerAppointment();
				}
				else
				{
					InvalidateLayout();
				}
				RaiseAppointmentChanged(AppointmentChange.StartTime);
			}
		}
	}
}
