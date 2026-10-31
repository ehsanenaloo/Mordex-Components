using System;
using System.Collections;
using System.Reflection;

namespace Mordex.Windows.Scheduler
{
	[DefaultMember("Item")]
	internal class SchedulerOwnerLayoutCollection : ICollection, IEnumerable
	{
		private ArrayList _layouts;

		public int Count => _layouts.Count;

		public object SyncRoot => _layouts.SyncRoot;

		public bool IsSynchronized => _layouts.IsSynchronized;

		internal SchedulerOwnerLayoutCollection()
		{
			_layouts = new ArrayList();
		}

		public void Add(SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState)
		{
			_layouts.Add(schedulerAppointmentOwnerLayoutState);
		}

		public void Clear()
		{
			_layouts.Clear();
		}

		public void CopyTo(Array destination, int index)
		{
			_layouts.CopyTo(destination, index);
		}

		public IEnumerator GetEnumerator()
		{
			return _layouts.GetEnumerator();
		}

		public SchedulerAppointmentOwnerLayoutState GetLayoutAt(int index)
		{
			return (SchedulerAppointmentOwnerLayoutState)_layouts[index];
		}

		public void Sort()
		{
			SchedulerOwnerLayoutComparer comparer = new SchedulerOwnerLayoutComparer();
			_layouts.Sort(comparer);
		}
	}
}
