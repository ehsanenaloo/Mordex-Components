using System;
using System.Collections;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerStartTimeComparer : IComparer
	{
		internal SchedulerStartTimeComparer()
		{
		}

		public int Compare(object value, object valueArgument)
		{
			if (value == null || valueArgument == null)
			{
				return 0;
			}
			if (value == valueArgument)
			{
				return 0;
			}
			SchedulerAppointment schedulerAppointment = (SchedulerAppointment)value;
			SchedulerAppointment otherAppointment = (SchedulerAppointment)valueArgument;
			DateTime startTime = schedulerAppointment.StartTime;
			DateTime otherStartTime = otherAppointment.StartTime;
			int comparison = startTime.CompareTo(otherStartTime);
			if (comparison == 0)
			{
				TimeSpan duration = schedulerAppointment.Duration;
				TimeSpan otherDuration = otherAppointment.Duration;
				comparison = -duration.CompareTo(otherDuration);
				if (comparison == 0)
				{
					startTime = schedulerAppointment.StartTime;
					otherStartTime = otherAppointment.StartTime;
					comparison = startTime.CompareTo(otherStartTime);
				}
				if (comparison == 0)
				{
					string text = schedulerAppointment.Text;
					string otherText = otherAppointment.Text;
					comparison = text.CompareTo(otherText);
				}
				if (comparison == 0)
				{
					int index = schedulerAppointment.Index;
					int otherIndex = otherAppointment.Index;
					if (schedulerAppointment.MasterAppointment != null)
					{
						index = schedulerAppointment.MasterAppointment.Index;
					}
					if (otherAppointment.MasterAppointment != null)
					{
						otherIndex = otherAppointment.MasterAppointment.Index;
					}
					comparison = index.CompareTo(otherIndex);
				}
				if (comparison == 0)
				{
					if (schedulerAppointment.GetRecurrenceException() != null)
					{
						if (otherAppointment.GetRecurrenceException() != null)
						{
							DateTime originalDate = schedulerAppointment.GetRecurrenceException().OriginalDate;
							DateTime otherOriginalDate = otherAppointment.GetRecurrenceException().OriginalDate;
							comparison = originalDate.CompareTo(otherOriginalDate);
						}
						return -1;
					}
					if (otherAppointment.GetRecurrenceException() != null)
					{
						return 1;
					}
				}
			}
			return comparison;
		}
	}
}
