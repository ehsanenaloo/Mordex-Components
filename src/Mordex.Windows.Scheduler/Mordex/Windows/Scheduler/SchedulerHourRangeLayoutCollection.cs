using System;
using System.Collections;
using System.Reflection;

namespace Mordex.Windows.Scheduler
{
	[DefaultMember("Item")]
	internal class SchedulerHourRangeLayoutCollection : ICollection, IEnumerable
	{
		private ArrayList _layouts;

		public int Count => _layouts.Count;

		public object SyncRoot => _layouts.SyncRoot;

		public bool IsSynchronized => _layouts.IsSynchronized;

		internal SchedulerHourRangeLayoutCollection()
		{
			_layouts = new ArrayList();
		}

		public void AddLayout(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			schedulerHourRangeCollectionLayoutState.UpdateDayIndex(_layouts.Count);
			_layouts.Add(schedulerHourRangeCollectionLayoutState);
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

		public SchedulerHourRangeCollectionLayoutState GetLayoutAt(int index)
		{
			return (SchedulerHourRangeCollectionLayoutState)_layouts[index];
		}

		public SchedulerHourRangeCollectionLayoutState FindLayoutByDate(DateTime dateTime)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _layouts)
			{
				if (item.GetDate() == dateTime.Date)
				{
					return item;
				}
			}
			return null;
		}

		public SchedulerHourRangeCollectionLayoutState FindLayout(DateTime dateTime, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			foreach (SchedulerHourRangeCollectionLayoutState item in _layouts)
			{
				if (item.GetDate() == dateTime.Date && item.GetOwner() == schedulerAppointmentOwner)
				{
					return item;
				}
			}
			return null;
		}

		internal int InsertLayoutSorted(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, int index, int highIndex, int indexArgument)
		{
			if (index > highIndex)
			{
				_layouts.Insert(index, schedulerHourRangeCollectionLayoutState);
				return index;
			}
			indexArgument = (highIndex + index) / 2;
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = (SchedulerHourRangeCollectionLayoutState)_layouts[indexArgument];
			int comparison = schedulerHourRangeCollectionLayoutStateArgument.GetDate().CompareTo(schedulerHourRangeCollectionLayoutState.GetDate());
			if (comparison == 0)
			{
				_layouts.Insert(indexArgument, schedulerHourRangeCollectionLayoutState);
				return -1;
			}
			if (comparison > 0)
			{
				return InsertLayoutSorted(schedulerHourRangeCollectionLayoutState, index, indexArgument - 1, indexArgument);
			}
			return InsertLayoutSorted(schedulerHourRangeCollectionLayoutState, indexArgument + 1, highIndex, indexArgument);
		}
	}
}
