using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerAppointment : ICloneable, IMordexXmlLayoutsSupport, ISchedulerFormatStyleOwner
	{
		private bool _includesCoreFields = true;

		internal bool _isRemovalSilent;

		internal bool _isEditingText;

		internal bool _isBeingMoved;

		internal SchedulerAppointment _owner;

		internal bool _selected;

		private SchedulerFormatStyle _formatStyle;

		internal RecurrenceState _recurrenceState;

		private bool _isPatternDeserializationDeferred;

		internal bool _notifiesScheduler = true;

		private SchedulerAppointment _masterAppointment;

		private RecurrencePattern _recurrencePattern;

		internal MxScheduler _scheduler;

		private bool _isDataRowEditStarted;

		private Reminder _reminder;

		internal int _layoutTop;

		internal int _layoutHeight;

		internal int _laneIndex;

		internal SchedulerAppointmentCollection _parentCollection;

		internal string _description = string.Empty;

		internal string _text = string.Empty;

		private Color _borderColor = Color.Blue;

		internal TimeSpan _duration = TimeSpan.Zero;

		private int _indexValue = -1;

		internal static int s_indexVersion;

		// Counts writes, so a cached map from data index to appointment can tell when it is out of date.
		internal int _index
		{
			get { return _indexValue; }
			set
			{
				_indexValue = value;
				s_indexVersion++;
			}
		}

		internal int _collectionIndex;

		private string _key = "";

		internal object _dataRow;

		private string _firstImageKey = "";

		private string _secondImageKey = "";

		private string _thirdImageKey = "";

		private string _fourthImageKey = "";

		private int _firstImageIndex = -1;

		private int _secondImageIndex = -1;

		private bool _visible = true;

		private string _prefix = "";

		private int _thirdImageIndex = -1;

		private int _fourthImageIndex = -1;

		private bool _allDayEvent;

		private object _tag;

		internal DateTime _savedStartTime;

		private DateTime _savedEndTime;

		internal TimeSpan _timeSpan;

		internal object _value;

		private TimeSpan _dragStartOffset;

		internal bool _isExceptionCreatedDuringDrag;

		private DateTime _endTimeValue = DateTime.MinValue;

		// Every write is reported to the owning collection, which keeps a cached time index (see SchedulerAppointmentCollection).
		internal DateTime _endTime
		{
			get { return _endTimeValue; }
			set
			{
				_endTimeValue = value;
				_parentCollection?.NoteTimeChange();
			}
		}

		internal object _ownerValue;

		private DateTime _startTimeValue = DateTime.MinValue;

		internal DateTime _startTime
		{
			get { return _startTimeValue; }
			set
			{
				_startTimeValue = value;
				_parentCollection?.NoteTimeChange();
			}
		}

		private object _fieldValues;

		internal bool _isDeletedOccurrence;

		private RecurrenceException _recurrenceException;

		[Browsable(false)]
		public bool IsEditing
		{
			get
			{
				if (Scheduler != null && _scheduler.GetEditor() != null && Scheduler.GetEditor()._app == this)
				{
					return true;
				}
				return false;
			}
		}

		public Reminder Reminder
		{
			get
			{
				return _reminder;
			}
			set
			{
				if (value == null)
				{
					value = new Reminder(this);
				}
				if (_reminder != value)
				{
					if (_recurrenceState == RecurrenceState.Occurrence)
					{
						EnsureRecurrenceException();
					}
					_reminder = value;
					_reminder._appointment = this;
					WriteChangesToDataSource(true, true);
				}
			}
		}

		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (_key != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_key = value;
					RefreshAppearance(true);
				}
			}
		}

		public object DataRow
		{
			get
			{
				if (Scheduler != null && _dataRow != null)
				{
					return _dataRow;
				}
				if (_recurrenceState == RecurrenceState.Occurrence && MasterAppointment != null)
				{
					return MasterAppointment.DataRow;
				}
				if (_index >= 0 && Scheduler != null && Scheduler.GetCurrencyManager() != null && _index < Scheduler.GetCurrencyManager().Count)
				{
					return Scheduler.GetCurrencyManager().List[_index];
				}
				return null;
			}
		}

		void ISchedulerFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			if (_recurrenceState == RecurrenceState.Occurrence)
			{
				EnsureRecurrenceException();
			}
			if (Scheduler != null)
			{
				RefreshAppearance(true);
				if (_recurrenceState == RecurrenceState.Master)
				{
					Scheduler.InvalidateAppointmentCacheAndLayout();
				}
			}
		}

		public SchedulerAppointment(DateTime appointmentStart, DateTime appointmentEnd)
		{
			if (appointmentEnd < appointmentStart)
			{
				throw new InvalidOperationException();
			}
			_startTime = appointmentStart;
			_endTime = appointmentEnd;
			_duration = new TimeSpan(_endTime.Ticks - _startTime.Ticks);
			_reminder = new Reminder(this);
		}

		public SchedulerAppointment()
		{
			_reminder = new Reminder(this);
			_startTime = DateTime.Today;
			_endTime = DateTime.Today;
		}

		public SchedulerAppointment(DateTime appointmentStart, DateTime appointmentEnd, string text)
		{
			if (appointmentEnd < appointmentStart)
			{
				throw new InvalidOperationException("The start time must be bigger than the end time");
			}
			_startTime = appointmentStart;
			_endTime = appointmentEnd;
			_duration = new TimeSpan(_endTime.Ticks - _startTime.Ticks);
			_text = text;
			_reminder = new Reminder(this);
		}

		public SchedulerAppointment(DateTime appointmentStart)
		{
			_startTime = appointmentStart;
			_reminder = new Reminder(this);
		}

		public SchedulerAppointment(DateTime appointmentStart, string text)
		{
			_startTime = appointmentStart;
			_endTime = _startTime;
			_text = text;
			_reminder = new Reminder(this);
		}

		public object Clone()
		{
			SchedulerAppointment schedulerAppointment = null;
			schedulerAppointment = ((Scheduler != null) ? Scheduler.BuildAppointment(_startTime, _endTime, _text) : new SchedulerAppointment(_startTime, _endTime));
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
			if (_recurrencePattern != null)
			{
				schedulerAppointment.RecurrencePatternInfo = RecurrencePatternInfo;
			}
			if (_formatStyle != null)
			{
				schedulerAppointment._formatStyle = _formatStyle.CloneAsCustomStyle();
				schedulerAppointment._formatStyle._owner = schedulerAppointment;
			}
			schedulerAppointment._tag = _tag;
			schedulerAppointment._ownerValue = _ownerValue;
			return schedulerAppointment;
		}

		internal object CloneWithoutRecurrence()
		{
			SchedulerAppointment schedulerAppointment = null;
			schedulerAppointment = ((Scheduler != null) ? Scheduler.BuildAppointment(_startTime, _endTime, _text) : new SchedulerAppointment(_startTime, _endTime));
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
			return schedulerAppointment;
		}

		internal SchedulerAppointment CreateDragCopy()
		{
			SchedulerAppointment schedulerAppointment = (SchedulerAppointment)CloneWithoutRecurrence();
			schedulerAppointment._owner = this;
			schedulerAppointment._timeSpan = _timeSpan;
			return schedulerAppointment;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			if (_formatStyle != null && !_formatStyle.HasOnlyDefaultValues())
			{
				_formatStyle._owner = null;
				_formatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle != null)
			{
				return !_formatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		private bool HasCustomSettings()
		{
			if (_formatStyle != null && !_formatStyle.HasOnlyDefaultValues())
			{
				return true;
			}
			if (_borderColor != Color.Blue)
			{
				return true;
			}
			if (_firstImageIndex != -1 || _firstImageKey.Length > 0)
			{
				return true;
			}
			if (_secondImageIndex != -1 || _secondImageKey.Length > 0)
			{
				return true;
			}
			if (_thirdImageIndex != -1 || _thirdImageKey.Length > 0)
			{
				return true;
			}
			if (_fourthImageIndex != -1 || _fourthImageKey.Length > 0)
			{
				return true;
			}
			if (_prefix.Length > 0)
			{
				return true;
			}
			if (_key != null && _key.Length > 0)
			{
				return true;
			}
			if (AllDayEvent)
			{
				return true;
			}
			if (_reminder.HasNonDefaultSettings())
			{
				return true;
			}
			return false;
		}

		internal void DeferPatternDeserialization()
		{
			_isPatternDeserializationDeferred = true;
		}

		internal void SetMasterAppointment(SchedulerAppointment schedulerAppointment)
		{
			_masterAppointment = schedulerAppointment;
			if (_masterAppointment != null)
			{
				_scheduler = _masterAppointment.Scheduler;
			}
		}

		internal bool IsDataRowEditStarted()
		{
			return _isDataRowEditStarted;
		}

		internal void SetDataRowEditStarted(bool isStarted)
		{
			_isDataRowEditStarted = isStarted;
		}

		internal bool HasActiveReminder()
		{
			if (Reminder != null && Reminder.Enabled && Reminder.GetActive())
			{
				return true;
			}
			if (RecurrenceState == RecurrenceState.Master)
			{
				if (Reminder != null && !Reminder.GetActive())
				{
					return false;
				}
				if (RecurrencePattern != null && RecurrencePattern.Exceptions.Count > 0)
				{
					foreach (RecurrenceException exception in RecurrencePattern.Exceptions)
					{
						if (exception.Appointment.Reminder != null && exception.Appointment.Reminder.Enabled && exception.Appointment.Reminder.GetActive())
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		internal SchedulerAppointment GetNextReminderAppointment()
		{
			return new SchedulerRecurrenceExpander(RecurrencePattern).FindNextReminderAppointment();
		}

		internal DateTime GetReminderTimeOrMax()
		{
			if (Reminder != null)
			{
				return Reminder.GetReminderDateTime();
			}
			return DateTime.MaxValue;
		}

		internal int GetLayoutTop()
		{
			return _layoutTop;
		}

		internal int GetLayoutHeight()
		{
			return _layoutHeight;
		}

		internal void UpdateSchedulerAppointment()
		{
			if (Scheduler != null)
			{
				_scheduler.UpdateLayout(_parentCollection, this, EditType.Change);
			}
		}

		private void InvalidateLayout()
		{
			if (Scheduler != null)
			{
				_scheduler.InvalidateAppointmentLayout();
			}
		}

		internal void UpdateRecurrenceLayout()
		{
			if (Scheduler != null)
			{
				_scheduler.UpdateRecurrenceCollection(this, true);
			}
		}

		internal int GetDurationInHours()
		{
			return Duration.Days * 24 + Duration.Hours;
		}

		public Rectangle GetBounds()
		{
			if (Scheduler != null)
			{
				SchedulerAppointmentView appointmentRenderer = Scheduler.FindAppointmentView(this);
				if (appointmentRenderer != null)
				{
					return appointmentRenderer._bounds;
				}
			}
			return Rectangle.Empty;
		}

		internal TimeSpan GetStartTimeOnDate(DateTime dateTime)
		{
			if (_startTime.Date < dateTime && _endTime >= dateTime)
			{
				return new TimeSpan(0, 0, 0);
			}
			return _startTime.TimeOfDay;
		}

		internal TimeSpan GetDurationOnDate(DateTime dateTime)
		{
			if (_startTime.Date == dateTime)
			{
				if (_endTime.Date == dateTime)
				{
					return Duration;
				}
				return new TimeSpan(24, 0, 0).Subtract(_startTime.TimeOfDay);
			}
			if (_endTime.Date == dateTime)
			{
				return _endTime.TimeOfDay;
			}
			return new TimeSpan(24, 0, 0);
		}

		internal void ApplyFieldValues(DateTime startTime, DateTime endTime, string text, object ownerValue, string description)
		{
			_startTime = startTime;
			if (endTime != DateTime.MinValue)
			{
				if (endTime < startTime)
				{
					endTime = startTime;
				}
				_endTime = endTime;
			}
			_text = text;
			_ownerValue = ownerValue;
			_description = description;
		}

		internal void MoveWithOwner(DateTime dateTime, DateTime dateTimeArgument, object value)
		{
			ApplyMove(dateTime, dateTimeArgument, value, true);
		}

		internal void SaveOriginalTimes(DateTime dateTime)
		{
			_savedStartTime = _startTime;
			_savedEndTime = _endTime;
			_timeSpan = _duration;
			_value = _ownerValue;
			_dragStartOffset = _startTime.TimeOfDay.Subtract(dateTime.TimeOfDay);
			if (MasterAppointment != null)
			{
				MasterAppointment._timeSpan = _duration;
				MasterAppointment._savedEndTime = _endTime;
				MasterAppointment._value = _ownerValue;
				MasterAppointment._savedStartTime = _startTime;
				MasterAppointment._dragStartOffset = _dragStartOffset;
			}
		}

		internal void RestoreOriginalTimes(bool updateException)
		{
			_startTime = _savedStartTime;
			_endTime = _savedEndTime;
			_duration = _timeSpan;
			_ownerValue = _value;
			if (updateException && _recurrenceState == RecurrenceState.Exception && _isExceptionCreatedDuringDrag)
			{
				_isExceptionCreatedDuringDrag = false;
				RecurrencePattern.Exceptions.RemoveException(GetRecurrenceException());
			}
		}

		internal void ApplyDragTime(DateTime startTime, TimeSpan duration, bool addTimeOffset)
		{
			if (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException())
			{
				if (addTimeOffset)
				{
					_startTime = startTime.Add(_dragStartOffset);
				}
				else
				{
					_startTime = startTime;
				}
				_duration = duration;
				_endTime = _startTime.Add(duration);
			}
		}

		internal void RaiseAppointmentChanging(AppointmentChangeCancelEventArgs eventArgs)
		{
			if (Scheduler != null)
			{
				_scheduler.RaiseAppointmentChanging(eventArgs);
			}
		}

		internal void RaiseAppointmentChanged(AppointmentChange appointmentChange)
		{
			if (Scheduler != null)
			{
				_scheduler.NotifyAppointmentChanged(this, appointmentChange);
			}
		}

		public object GetValue(SchedulerField field)
		{
			if (_fieldValues != null)
			{
				return ((object[])_fieldValues)[field.Index];
			}
			if (_recurrenceState == RecurrenceState.Occurrence && MasterAppointment != null)
			{
				return _masterAppointment.GetValue(field);
			}
			if (Scheduler != null)
			{
				return Scheduler.GetFieldValue(this, field);
			}
			return null;
		}

		public object GetValue(string fieldKey)
		{
			if (Scheduler != null)
			{
				if (_fieldValues != null)
				{
					return ((object[])_fieldValues)[Scheduler.Fields[fieldKey].Index];
				}
				if (_recurrenceState == RecurrenceState.Occurrence && MasterAppointment != null)
				{
					return _masterAppointment.GetValue(fieldKey);
				}
				return Scheduler.GetMemberValue(this, fieldKey);
			}
			return null;
		}

		public object GetValue(int fieldIndex)
		{
			if (_fieldValues != null)
			{
				return ((object[])_fieldValues)[fieldIndex];
			}
			if (_recurrenceState == RecurrenceState.Occurrence && MasterAppointment != null)
			{
				return _masterAppointment.GetValue(fieldIndex);
			}
			if (Scheduler != null)
			{
				return Scheduler.GetFieldValueByIndex(this, fieldIndex);
			}
			return null;
		}

		internal void UpdateSchedulerAppointmentValue(SchedulerFieldCollection schedulerFieldCollection)
		{
			if (schedulerFieldCollection != null)
			{
				_fieldValues = new object[schedulerFieldCollection.Count];
			}
		}

		internal void SetSchedulerAppointmentValue()
		{
			_fieldValues = null;
		}

		internal bool HasFieldValues()
		{
			return _fieldValues != null;
		}

		internal bool EnsureRecurrenceException()
		{
			if (_recurrenceException == null)
			{
				RecurrenceException ex = new RecurrenceException(this, RecurrencePattern, StartTime.Date);
				ex._deleted = _isDeletedOccurrence;
				bool isRejected = false;
				if (Scheduler != null)
				{
					_recurrenceException = ex;
					if (RecurrencePattern != null)
					{
						RecurrencePattern.Exceptions.AddException(_recurrenceException);
					}
					isRejected = Scheduler.IsRecurrenceExceptionCancelled(this);
				}
				if (isRejected)
				{
					_recurrenceState = RecurrenceState.Occurrence;
					RecurrencePattern.Exceptions.RemoveException(_recurrenceException);
					_recurrenceException = null;
					return false;
				}
				if (Scheduler != null)
				{
					if (Scheduler._isAppointmentDragActive)
					{
						_isExceptionCreatedDuringDrag = true;
					}
					Scheduler.RaiseRecurrenceExceptionCreated(this);
				}
			}
			return true;
		}

		internal RecurrenceException GetRecurrenceException()
		{
			return _recurrenceException;
		}

		internal void SetRecurrenceException(RecurrenceException recurrenceException)
		{
			_recurrenceException = recurrenceException;
		}
	}
}
