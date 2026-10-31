using System.Collections;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerOwnerLayoutComparer : IComparer
	{
		internal SchedulerOwnerLayoutComparer()
		{
		}

		public int Compare(object valueArgument, object secondValue)
		{
			if (valueArgument == null || secondValue == null)
			{
				return 0;
			}
			SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = (SchedulerAppointmentOwnerLayoutState)valueArgument;
			SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutStateArgument = (SchedulerAppointmentOwnerLayoutState)secondValue;
			int firstIndex = schedulerAppointmentOwnerLayoutState.GetIndex();
			int value = schedulerAppointmentOwnerLayoutStateArgument.GetIndex();
			return firstIndex.CompareTo(value);
		}
	}
}
