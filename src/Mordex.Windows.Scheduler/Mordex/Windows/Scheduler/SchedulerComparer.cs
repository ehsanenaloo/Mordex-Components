using System;
using System.Collections;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerComparer : IComparer
	{
		internal SchedulerComparer()
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
			DateTime date = schedulerAppointment.StartTime.Date;
			DateTime otherDate = otherAppointment.StartTime.Date;
			int comparison = date.CompareTo(otherDate);
			if (comparison == 0)
			{
				TimeSpan timeOfDay = schedulerAppointment.StartTime.TimeOfDay;
				TimeSpan otherTimeOfDay = otherAppointment.StartTime.TimeOfDay;
				comparison = timeOfDay.CompareTo(otherTimeOfDay);
				if (comparison == 0)
				{
					timeOfDay = schedulerAppointment.Duration;
					otherTimeOfDay = otherAppointment.Duration;
					comparison = -timeOfDay.CompareTo(otherTimeOfDay);
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
