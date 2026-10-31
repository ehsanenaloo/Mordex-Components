using System;
using System.Collections;
using System.Reflection;

namespace Mordex.Windows.Scheduler
{
	[DefaultMember("Item")]
	internal class SchedulerAppointmentOwnerStateCollection : ICollection, IEnumerable
	{
		private ArrayList _ownerStates;

		public int Count => _ownerStates.Count;

		public bool IsSynchronized => _ownerStates.IsSynchronized;

		public object SyncRoot => _ownerStates.SyncRoot;

		internal SchedulerAppointmentOwnerStateCollection()
		{
			_ownerStates = new ArrayList();
		}

		public void Clear()
		{
			_ownerStates.Clear();
		}

		public void Add(SchedulerAppointmentOwnerState schedulerAppointmentOwnerState)
		{
			_ownerStates.Add(schedulerAppointmentOwnerState);
		}

		public SchedulerAppointmentOwnerState FindOwnerStateByDate(DateTime dateTime)
		{
			foreach (SchedulerAppointmentOwnerState item in _ownerStates)
			{
				if (item._date == dateTime)
				{
					return item;
				}
			}
			return null;
		}

		public SchedulerAppointmentOwnerState FindOwnerStateByDateAndOwner(DateTime dateTime, SchedulerAppointmentOwner owner)
		{
			foreach (SchedulerAppointmentOwnerState item in _ownerStates)
			{
				if (item._date == dateTime && (item.GetUser() == owner || item.GetUser().Equals(owner)))
				{
					return item;
				}
			}
			return null;
		}

		public void CopyTo(Array destination, int index)
		{
			_ownerStates.CopyTo(destination, index);
		}

		public IEnumerator GetEnumerator()
		{
			return _ownerStates.GetEnumerator();
		}
	}
}
