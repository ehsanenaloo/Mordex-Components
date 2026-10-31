using System;
using System.Collections;

namespace Mordex.Windows.Scheduler
{
	public class RecurrenceAppointmentCollection : CollectionBase
	{
		private MxScheduler _scheduler;

		public MxScheduler Scheduler => _scheduler;

		public SchedulerAppointment this[int index] => (SchedulerAppointment)base.List[index];

		internal RecurrenceAppointmentCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		internal void RemoveIfNotRecurring(SchedulerAppointment schedulerAppointment)
		{
			if (schedulerAppointment.RecurrencePattern == null)
			{
				base.List.Remove(schedulerAppointment);
			}
		}

		internal void AddIfRecurring(SchedulerAppointment schedulerAppointment)
		{
			if (schedulerAppointment.RecurrencePattern != null)
			{
				base.List.Add(schedulerAppointment);
			}
		}

		public bool Contains(SchedulerAppointment appointment)
		{
			return base.List.Contains(appointment);
		}

		internal SchedulerAppointment FindByRecurrenceId(Guid guid)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerAppointment schedulerAppointment = (SchedulerAppointment)enumerator.Current;
					if (schedulerAppointment.RecurrencePattern.GetId().Equals(guid))
					{
						return schedulerAppointment;
					}
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
			return null;
		}
	}
}
