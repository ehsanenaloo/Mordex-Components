using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerStoredFilterCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxScheduler _scheduler;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "StoredFilter";

		public MxScheduler Scheduler => _scheduler;

		public SchedulerStoredFilter this[int index] => (SchedulerStoredFilter)base.List[index];

		public SchedulerStoredFilter this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						SchedulerStoredFilter schedulerStoredFilter = (SchedulerStoredFilter)enumerator.Current;
						if (string.Compare(schedulerStoredFilter.Key, key, ignoreCase: true) == 0)
						{
							return schedulerStoredFilter;
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
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int index = 0;
			for (PropertyValue propertyValue = info["StoredFilter" + index]; propertyValue != null; propertyValue = info["StoredFilter" + index])
			{
				SchedulerStoredFilter schedulerStoredFilter = new SchedulerStoredFilter();
				base.List.Add(schedulerStoredFilter);
				index++;
				propertyValue.DeserializeObject(schedulerStoredFilter);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerStoredFilter value = (SchedulerStoredFilter)enumerator.Current;
					info.AddValue("StoredFilter" + index, value);
					index++;
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

		internal SchedulerStoredFilterCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public int IndexOf(SchedulerStoredFilter storedFilter)
		{
			return base.List.IndexOf(storedFilter);
		}

		public void Add(SchedulerStoredFilter storedFilter)
		{
			base.List.Add(storedFilter);
		}

		public void AddRange(SchedulerStoredFilter[] storedFilters)
		{
			foreach (SchedulerStoredFilter value in storedFilters)
			{
				base.List.Add(value);
			}
		}

		public void Remove(SchedulerStoredFilter storedFilter)
		{
			base.List.Remove(storedFilter);
		}

		public bool Contains(SchedulerStoredFilter storedFilter)
		{
			return base.List.Contains(storedFilter);
		}

		public bool Contains(string storedFilterKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerStoredFilter schedulerStoredFilter = (SchedulerStoredFilter)enumerator.Current;
					if (string.Compare(schedulerStoredFilter.Key, storedFilterKey, ignoreCase: true) == 0)
					{
						return true;
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
			return false;
		}

		protected override void OnInsert(int index, object value)
		{
			if (!(value is SchedulerStoredFilter))
			{
				throw new ArgumentException();
			}
			if (((SchedulerStoredFilter)value).GetSchedulerStoredFilterCollection() != null)
			{
				throw new InvalidOperationException("Filter already belongs to a collection.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			((SchedulerStoredFilter)value).SetSchedulerStoredFilterCollection(this);
			((SchedulerStoredFilter)value)._index = base.List.Count - 1;
		}

		protected override void OnClear()
		{
			bool hasCurrentFilter = false;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerStoredFilter schedulerStoredFilter = (SchedulerStoredFilter)enumerator.Current;
					if (schedulerStoredFilter.GetIsCurrent() && Scheduler.FilterCondition == schedulerStoredFilter)
					{
						hasCurrentFilter = true;
					}
					schedulerStoredFilter.SetSchedulerStoredFilterCollection(null);
					schedulerStoredFilter._index = -1;
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
			if (hasCurrentFilter)
			{
				Scheduler.FilterCondition = null;
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			SchedulerStoredFilter schedulerStoredFilter = (SchedulerStoredFilter)value;
			if (schedulerStoredFilter.GetIsCurrent() && Scheduler.FilterCondition == schedulerStoredFilter)
			{
				Scheduler.FilterCondition = null;
			}
			schedulerStoredFilter.SetSchedulerStoredFilterCollection(null);
			schedulerStoredFilter._index = -1;
			UpdateSchedulerStoredFilterCollection();
		}

		private void UpdateSchedulerStoredFilterCollection()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._index = i;
			}
		}
	}
}
