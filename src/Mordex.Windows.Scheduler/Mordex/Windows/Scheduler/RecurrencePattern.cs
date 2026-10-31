using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class RecurrencePattern : RecurrencePatternBase, IMordexXmlLayoutsSupport
	{
		internal bool _shouldSerializeExceptions = true;

		internal bool _isDeserializationDeferred;

		private PropertyValue _propertyValue;

		private Guid _id = Guid.Empty;

		private RecurrenceExceptionCollection _exceptions;

		internal SchedulerAppointment _owner;

		private DateTime _reminderDismissTime = DateTime.MinValue;

		internal DateTime _reminderFiredTime = DateTime.MinValue;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public MxScheduler Scheduler
		{
			get
			{
				if (MasterAppointment != null)
				{
					return MasterAppointment.Scheduler;
				}
				return null;
			}
		}

		public RecurrenceExceptionCollection Exceptions => _exceptions;

		public SchedulerAppointment MasterAppointment => _owner;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			DeserializeRecurrencePatternBaseState(info);
			_id = (Guid)LayoutReader.GetPropertyValue(info, "Id", typeof(Guid), Guid.Empty);
			_reminderDismissTime = (DateTime)LayoutReader.GetPropertyValue(info, "ReminderDismissTime", typeof(DateTime), DateTime.MinValue);
			_propertyValue = info["Exceptions"];
			if (!_isDeserializationDeferred)
			{
				DeserializeExceptions();
			}
		}

		internal void DeserializeExceptions()
		{
			if (_propertyValue != null)
			{
				if (_exceptions == null)
				{
					_exceptions = new RecurrenceExceptionCollection(this);
				}
				_propertyValue.DeserializeObject(_exceptions);
				_propertyValue = null;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			SerializeRecurrencePatternBaseState(info);
			if (_reminderDismissTime != DateTime.MinValue)
			{
				info.AddValue("ReminderDismissTime", _reminderDismissTime);
			}
			if (!_id.Equals(Guid.Empty))
			{
				info.AddValue("Id", _id);
			}
			if (_shouldSerializeExceptions && _exceptions.Count > 0)
			{
				info.AddValue("Exceptions", _exceptions);
			}
		}

		public static RecurrencePattern FromXMLString(string xml)
		{
			RecurrencePattern recurrencePattern = new RecurrencePattern();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xml);
			mordexLayoutReader["RecurrencePattern"]?.DeserializeObject(recurrencePattern);
			return recurrencePattern;
		}

		public RecurrencePattern()
		{
			_exceptions = new RecurrenceExceptionCollection(this);
			_id = Guid.NewGuid();
		}

		internal RecurrencePattern(Guid id)
		{
			_id = id;
		}

		internal Guid GetId()
		{
			return _id;
		}

		public DateTime GetFirstOccurrenceDate()
		{
			return new SchedulerRecurrenceExpander(this).GetFirstOccurrenceDate(base.PatternStartDate);
		}

		public SchedulerAppointment[] GetOccurrence(DateTime date)
		{
			DateRange range = new DateRange(date.Date, date.Date.AddDays(1.0));
			return GetOccurrences(range);
		}

		public SchedulerAppointment[] GetOccurrences(DateRange range)
		{
			return new SchedulerRecurrenceExpander(this).GetOccurrences(range);
		}

		public SchedulerAppointment FindNextOccurrence(DateTime date)
		{
			SchedulerRecurrenceExpander expander = new SchedulerRecurrenceExpander(this);
			SchedulerAppointment schedulerAppointment = expander.FindNextOccurrence(date.Date);
			if (schedulerAppointment != null && schedulerAppointment.StartTime < date)
			{
				return expander.FindNextOccurrence(date.Date.AddDays(1.0));
			}
			return schedulerAppointment;
		}

		public SchedulerAppointment FindPreviousOccurrence(DateTime date)
		{
			SchedulerRecurrenceExpander expander = new SchedulerRecurrenceExpander(this);
			SchedulerAppointment schedulerAppointment = expander.FindPreviousOccurrence(date.Date);
			if (schedulerAppointment != null && schedulerAppointment.StartTime > date)
			{
				return expander.FindPreviousOccurrence(date.Date.AddDays(-1.0));
			}
			return schedulerAppointment;
		}

		internal override void InvalidateOwner()
		{
			if (_isEditing)
			{
				return;
			}
			Exceptions.Clear();
			if (Scheduler != null && MasterAppointment != null)
			{
				DateTime dateTime = new SchedulerRecurrenceExpander(this).GetFirstOccurrenceDate(MasterAppointment.StartTime.Date);
				if (MasterAppointment.StartTime.Date != dateTime)
				{
					MasterAppointment.Move(dateTime.Add(base.StartTime), dateTime.Add(base.EndTime));
				}
				MasterAppointment.RaiseAppointmentChanged(AppointmentChange.RecurrencePattern);
				Scheduler.UpdateRecurrenceCollection(MasterAppointment, true);
			}
		}

		internal DateTime GetEffectiveReminderDismissTime()
		{
			if (_reminderFiredTime != DateTime.MinValue)
			{
				return _reminderFiredTime;
			}
			return _reminderDismissTime;
		}

		internal void UpdateReminderDismissTime(DateTime reminderDismissTime)
		{
			if (_reminderDismissTime != reminderDismissTime)
			{
				_reminderDismissTime = reminderDismissTime;
				if (Scheduler != null)
				{
					Scheduler.WriteRecurrenceToDataSource(MasterAppointment);
				}
			}
		}

		public new string ToString()
		{
			PropertyValue root = new PropertyValue("RecurrencePattern", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
			return mordexLayoutWriter.ToString();
		}
	}
}
