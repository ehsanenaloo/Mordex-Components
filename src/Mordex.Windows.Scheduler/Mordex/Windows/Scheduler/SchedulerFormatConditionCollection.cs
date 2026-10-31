using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerFormatConditionCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxScheduler _scheduler;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Condition";

		public MxScheduler Scheduler => _scheduler;

		public SchedulerFormatCondition this[int index] => (SchedulerFormatCondition)base.List[index];

		public SchedulerFormatCondition this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						SchedulerFormatCondition schedulerFormatCondition = (SchedulerFormatCondition)enumerator.Current;
						if (string.Compare(schedulerFormatCondition.Key, key, ignoreCase: true) == 0)
						{
							return schedulerFormatCondition;
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
			for (PropertyValue propertyValue = info["Condition" + index]; propertyValue != null; propertyValue = info["Condition" + index])
			{
				SchedulerFormatCondition schedulerFormatCondition = new SchedulerFormatCondition();
				base.InnerList.Add(schedulerFormatCondition);
				schedulerFormatCondition._owner = this;
				schedulerFormatCondition._collectionIndex = index;
				index++;
				propertyValue.DeserializeObject(schedulerFormatCondition);
			}
			InvalidateControl();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerFormatCondition value = (SchedulerFormatCondition)enumerator.Current;
					info.AddValue("Condition" + index, value);
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

		internal SchedulerFormatConditionCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void Add(SchedulerFormatCondition formatCondition)
		{
			formatCondition._owner = this;
			formatCondition._collectionIndex = base.List.Count;
			base.List.Add(formatCondition);
			InvalidateControl();
		}

		public bool Contains(SchedulerFormatCondition condition)
		{
			return base.List.Contains(condition);
		}

		public bool Contains(string conditionKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerFormatCondition schedulerFormatCondition = (SchedulerFormatCondition)enumerator.Current;
					if (string.Compare(schedulerFormatCondition.Key, conditionKey, ignoreCase: true) == 0)
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

		public int IndexOf(SchedulerFormatCondition condition)
		{
			return base.List.IndexOf(condition);
		}

		public void Remove(SchedulerFormatCondition condition)
		{
			base.List.Remove(condition);
		}

		public void AddRange(SchedulerFormatCondition[] conditions)
		{
			for (int i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] != null)
				{
					conditions[i]._collectionIndex = base.List.Count;
					conditions[i]._owner = this;
					base.List.Add(conditions[i]);
				}
			}
			InvalidateControl();
		}

		internal void InvalidateControl()
		{
			_scheduler.InvalidateAppointmentStyles();
			_scheduler.Invalidate();
		}

		protected override void OnClearComplete()
		{
			foreach (SchedulerFormatCondition item in base.List)
			{
				item._collectionIndex = -1;
				item._owner = null;
			}
			InvalidateControl();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			UpdateSchedulerFormatConditionCollection();
			InvalidateControl();
		}

		private void UpdateSchedulerFormatConditionCollection()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._collectionIndex = i;
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			InvalidateControl();
		}
	}
}
