using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private SchedulerLayout _parentLayout;

		private MxScheduler _scheduler;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public MxScheduler Scheduler => _scheduler;

		public SchedulerLayout this[int index] => (SchedulerLayout)base.List[index];

		public SchedulerLayout this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						SchedulerLayout schedulerLayout = (SchedulerLayout)enumerator.Current;
						if (string.Compare(schedulerLayout.Key, key, ignoreCase: true) == 0)
						{
							return schedulerLayout;
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
				throw new ArgumentOutOfRangeException();
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["Layout" + index]; propertyValue != null; propertyValue = info["Layout" + index])
			{
				SchedulerLayout schedulerLayout = new SchedulerLayout();
				propertyValue.DeserializeObject(schedulerLayout);
				Add(schedulerLayout);
				index++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerLayout layout = (SchedulerLayout)enumerator.Current;
					if (layout._isCurrentLayout)
					{
						Scheduler.CurrentLayout = layout;
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
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerLayout value = (SchedulerLayout)enumerator.Current;
					info.AddValue("Layout" + index, value);
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

		internal SchedulerLayoutCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		internal SchedulerLayoutCollection(SchedulerLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal SchedulerLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxScheduler GetOwnerScheduler()
		{
			if (_parentLayout == null)
			{
				return _scheduler;
			}
			return _parentLayout.GetScheduler();
		}

		public void Add(SchedulerLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(SchedulerLayout[] layouts)
		{
			foreach (SchedulerLayout layout in layouts)
			{
				Add(layout);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.GetType() != typeof(SchedulerLayout))
			{
				throw new InvalidCastException();
			}
			SchedulerLayout schedulerLayout = (SchedulerLayout)value;
			if (schedulerLayout.GetSchedulerLayoutCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !schedulerLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			SchedulerLayout schedulerLayout = (SchedulerLayout)value;
			schedulerLayout.SetSchedulerLayoutCollection(this);
			schedulerLayout.UpdateSchedulerLayout();
		}

		public void Remove(SchedulerLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxScheduler scheduler = GetOwnerScheduler();
			if (scheduler != null && scheduler.CurrentLayout == (SchedulerLayout)value)
			{
				scheduler.CurrentLayout = null;
			}
			((SchedulerLayout)value).SetSchedulerLayoutCollection(null);
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerLayout schedulerLayout = (SchedulerLayout)enumerator.Current;
					schedulerLayout.SetSchedulerLayoutCollection(null);
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

		public int IndexOf(SchedulerLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(SchedulerLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerLayout schedulerLayout = (SchedulerLayout)enumerator.Current;
					if (string.Compare(schedulerLayout.Key, layoutKey, ignoreCase: true) == 0)
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

		internal SchedulerLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerLayout schedulerLayout = (SchedulerLayout)enumerator.Current;
					SchedulerLayout currentLayout = schedulerLayout.FindCurrentLayout();
					if (currentLayout != null)
					{
						return currentLayout;
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
