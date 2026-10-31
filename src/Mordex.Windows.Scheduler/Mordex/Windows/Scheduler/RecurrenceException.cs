using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class RecurrenceException : IMordexXmlLayoutsSupport
	{
		internal bool _shouldDeserializeAppointment = true;

		private Guid _recurrencePatternID = Guid.Empty;

		internal RecurrencePattern _pattern;

		private SchedulerAppointment _appointment;

		private DateTime _originalDate = DateTime.MinValue;

		internal bool _deleted;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public RecurrencePattern RecurrencePattern => _pattern;

		public SchedulerAppointment Appointment
		{
			get
			{
				return _appointment;
			}
			set
			{
				_appointment = value;
			}
		}

		public DateTime OriginalDate => _originalDate;

		public bool Deleted
		{
			get
			{
				return _deleted;
			}
			set
			{
				if (_deleted != value)
				{
					_deleted = value;
					if (_appointment != null)
					{
						_appointment.UpdateRecurrenceLayout();
					}
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_recurrencePatternID = (Guid)LayoutReader.GetPropertyValue(info, "RecurrencePatternID", typeof(Guid), Guid.Empty);
			_deleted = (bool)LayoutReader.GetPropertyValue(info, "Deleted", typeof(bool), false);
			_originalDate = (DateTime)LayoutReader.GetPropertyValue(info, "OriginalDate", typeof(DateTime), DateTime.MinValue);
			if (!_shouldDeserializeAppointment)
			{
				return;
			}
			PropertyValue propertyValue = info["Appointment"];
			if (propertyValue != null)
			{
				if (_appointment == null)
				{
					_appointment = new SchedulerAppointment();
					propertyValue.DeserializeObject(_appointment);
					_appointment.SetRecurrenceException(this);
					_appointment._recurrenceState = RecurrenceState.Exception;
				}
				else
				{
					propertyValue.DeserializeObject(_appointment);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_deleted)
			{
				info.AddValue("Deleted", _deleted);
			}
			if (_originalDate != DateTime.MinValue)
			{
				info.AddValue("OriginalDate", _originalDate);
			}
			if (!GetRecurrencePatternId().Equals(Guid.Empty))
			{
				info.AddValue("RecurrencePatternID", GetRecurrencePatternId());
			}
			if (_appointment != null)
			{
				info.AddValue("Appointment", _appointment);
			}
		}

		internal RecurrenceException()
		{
		}

		internal RecurrenceException(SchedulerAppointment appointment, RecurrencePattern pattern, DateTime originalDate)
		{
			_originalDate = originalDate;
			_pattern = pattern;
			_appointment = appointment;
		}

		internal Guid GetRecurrencePatternId()
		{
			if (RecurrencePattern != null)
			{
				return RecurrencePattern.GetId();
			}
			return _recurrencePatternID;
		}

		internal string ToLayoutString()
		{
			PropertyValue root = new PropertyValue("RecurrenceException", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
			return mordexLayoutWriter.ToString();
		}
	}
}
