using System;
using System.Collections;
using System.Collections.Generic;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class RecurrenceExceptionCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private RecurrencePattern _pattern;

		private readonly Dictionary<DateTime, RecurrenceException> _byOriginalDate = new Dictionary<DateTime, RecurrenceException>();

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "RecurrenceException";

		public MxScheduler Scheduler
		{
			get
			{
				if (_pattern != null)
				{
					return _pattern.Scheduler;
				}
				return null;
			}
		}

		public RecurrencePattern RecurrencePattern => _pattern;

		public RecurrenceException this[int index] => (RecurrenceException)base.List[index];

		public RecurrenceException this[DateTime date]
		{
			get
			{
				_byOriginalDate.TryGetValue(date, out RecurrenceException exception);
				return exception;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int exceptionIndex = 0;
			PropertyValue propertyValue = info["RecurrenceException" + exceptionIndex];
			List<RecurrenceException> staleExceptions = new List<RecurrenceException>();
			foreach (RecurrenceException item in base.List)
			{
				staleExceptions.Add(item);
			}
			while (propertyValue != null)
			{
				RecurrenceException ex = new RecurrenceException();
				ex._pattern = _pattern;
				ex._shouldDeserializeAppointment = false;
				propertyValue.DeserializeObject(ex);
				if (_pattern != null)
				{
					RecurrenceException existingException = this[ex.OriginalDate];
					if (existingException == null)
					{
						ex._shouldDeserializeAppointment = true;
						propertyValue.DeserializeObject(ex);
						base.List.Add(ex);
					}
					else
					{
						staleExceptions.Remove(existingException);
						propertyValue.DeserializeObject(existingException);
					}
				}
				else
				{
					ex._shouldDeserializeAppointment = true;
					propertyValue.DeserializeObject(ex);
					base.List.Add(ex);
				}
				ex._shouldDeserializeAppointment = true;
				exceptionIndex++;
				propertyValue = info["RecurrenceException" + exceptionIndex];
			}
			foreach (RecurrenceException staleException in staleExceptions)
			{
				if (Contains(staleException))
				{
					base.List.Remove(staleException);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int exceptionIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RecurrenceException value = (RecurrenceException)enumerator.Current;
					info.AddValue("RecurrenceException" + exceptionIndex, value);
					exceptionIndex++;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		internal RecurrenceExceptionCollection(RecurrencePattern pattern)
		{
			_pattern = pattern;
		}

		internal void AddException(RecurrenceException recurrenceException)
		{
			recurrenceException.Appointment._recurrenceState = RecurrenceState.Exception;
			base.List.Add(recurrenceException);
		}

		internal void RemoveException(RecurrenceException recurrenceException)
		{
			if (base.List.Contains(recurrenceException))
			{
				base.List.Remove(recurrenceException);
			}
		}

		public void Remove(SchedulerAppointment exception)
		{
			if (exception.GetRecurrenceException() != null && base.List.Contains(exception.GetRecurrenceException()))
			{
				base.List.Remove(exception.GetRecurrenceException());
			}
		}

		internal RecurrenceExceptionCollection GetExceptionsInRange(DateRange dateRange)
		{
			RecurrenceExceptionCollection recurrenceExceptionCollection = new RecurrenceExceptionCollection(null);
			foreach (RecurrenceException item in base.List)
			{
				SchedulerAppointment appointment = item.Appointment;
				if (appointment.EndTime >= dateRange.Start && appointment.StartTime <= dateRange.End)
				{
					recurrenceExceptionCollection.List.Add(item);
				}
			}
			return recurrenceExceptionCollection;
		}

		public new void Clear()
		{
			for (int index = base.List.Count - 1; index >= 0; index--)
			{
				RemoveAt(index);
			}
		}

		public bool Contains(RecurrenceException exception)
		{
			return base.List.Contains(exception);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			RecurrenceException exception = (RecurrenceException)value;
			_byOriginalDate[exception.OriginalDate] = exception;
		}

		protected override void OnClearComplete()
		{
			_byOriginalDate.Clear();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			RecurrenceException ex = (RecurrenceException)value;
			if (_byOriginalDate.TryGetValue(ex.OriginalDate, out RecurrenceException indexed) && indexed == ex)
			{
				_byOriginalDate.Remove(ex.OriginalDate);
			}
			if (Scheduler != null)
			{
				Scheduler.RemoveFromAppointments(ex.Appointment);
			}
		}
	}
}
