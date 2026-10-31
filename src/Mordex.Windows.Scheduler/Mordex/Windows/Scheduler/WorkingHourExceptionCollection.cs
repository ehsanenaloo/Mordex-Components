using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class WorkingHourExceptionCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private ISchedulerWorkingHoursOwner _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Exception";

		public MxScheduler Scheduler
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Scheduler;
				}
				return null;
			}
		}

		public WorkingHourException this[int index] => (WorkingHourException)base.List[index];

		public WorkingHourException this[string key]
		{
			get
			{
				foreach (WorkingHourException item in base.List)
				{
					if (item.Key == key)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' in this collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int exceptionIndex = 0;
			for (PropertyValue propertyValue = info["Exception" + exceptionIndex]; propertyValue != null; propertyValue = info["Exception" + exceptionIndex])
			{
				WorkingHourException ex = new WorkingHourException();
				base.InnerList.Add(ex);
				ex._owner = this;
				exceptionIndex++;
			}
			exceptionIndex = 0;
			for (PropertyValue propertyValue = info["Exception" + exceptionIndex]; propertyValue != null; propertyValue = info["Exception" + exceptionIndex])
			{
				propertyValue.DeserializeObject(this[exceptionIndex]);
				exceptionIndex++;
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
					WorkingHourException value = (WorkingHourException)enumerator.Current;
					info.AddValue("Exception" + exceptionIndex, value);
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

		internal WorkingHourExceptionCollection(ISchedulerWorkingHoursOwner owner)
		{
			_owner = owner;
		}

		public void AddRange(WorkingHourException[] exceptions)
		{
			foreach (WorkingHourException ex in exceptions)
			{
				if (ex == null)
				{
					throw new ArgumentNullException();
				}
				if (ex._owner != null)
				{
					if (ex.Scheduler != Scheduler)
					{
						throw new InvalidOperationException("The exception day can only belog to one Schedule");
					}
					throw new InvalidOperationException("The exception day can only be added once");
				}
				base.InnerList.Add(ex);
				ex._owner = this;
			}
			UpdateWorkingHourExceptionCollection();
		}

		public void Add(WorkingHourException exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException();
			}
			if (exception._owner != null)
			{
				if (exception.Scheduler != Scheduler)
				{
					throw new InvalidOperationException("The exception day can only belog to one Schedule");
				}
				throw new InvalidOperationException("The exception day can only be added once");
			}
			base.List.Add(exception);
		}

		public bool Contains(WorkingHourException exception)
		{
			return base.List.Contains(exception);
		}

		public void Remove(WorkingHourException exception)
		{
			base.List.Remove(exception);
		}

		public new void Clear()
		{
			for (int index = base.List.Count - 1; index >= 0; index--)
			{
				WorkingHourException ex = this[index];
				base.InnerList.Remove(ex);
				ex._owner = null;
			}
			UpdateWorkingHourExceptionCollection();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			WorkingHourException ex = (WorkingHourException)value;
			ex._owner = null;
			UpdateWorkingHourExceptionCollection();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			WorkingHourException ex = (WorkingHourException)value;
			ex._owner = this;
			UpdateWorkingHourExceptionCollection();
		}

		internal void UpdateWorkingHourExceptionCollection()
		{
			if (Scheduler != null)
			{
				Scheduler._isHourRangeCacheStale = true;
				Scheduler.InvalidateAppointmentLayout();
			}
			else if (_owner != null)
			{
				_owner.InvalidateHour();
			}
		}
	}
}
