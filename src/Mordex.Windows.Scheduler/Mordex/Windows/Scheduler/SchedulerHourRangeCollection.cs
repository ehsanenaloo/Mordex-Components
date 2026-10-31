using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerHourRangeCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private ISchedulerWorkingHoursOwner _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "HourRange";

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

		public SchedulerHourRange this[int index] => (SchedulerHourRange)base.List[index];

		public SchedulerHourRange this[string key]
		{
			get
			{
				foreach (SchedulerHourRange item in base.List)
				{
					if (item.Key == key)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["HourRange" + index]; propertyValue != null; propertyValue = info["HourRange" + index])
			{
				SchedulerHourRange schedulerHourRange = new SchedulerHourRange();
				base.InnerList.Add(schedulerHourRange);
				schedulerHourRange.SetOwner(_owner);
				schedulerHourRange._index = index;
				index++;
			}
			index = 0;
			for (PropertyValue propertyValue = info["HourRange" + index]; propertyValue != null; propertyValue = info["HourRange" + index])
			{
				propertyValue.DeserializeObject(this[index]);
				index++;
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
					SchedulerHourRange value = (SchedulerHourRange)enumerator.Current;
					info.AddValue("HourRange" + index, value);
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

		private SchedulerHourRangeCollection()
		{
		}

		internal SchedulerHourRangeCollection(ISchedulerWorkingHoursOwner owner)
		{
			_owner = owner;
		}

		public void Add(SchedulerHourRange hourRange)
		{
			if (_owner != null)
			{
				hourRange.SetOwner(_owner);
			}
			base.List.Add(hourRange);
			InvalidateOwnerHour();
		}

		public void Insert(int index, SchedulerHourRange hourRange)
		{
			if (index > base.List.Count)
			{
				index = base.List.Count;
			}
			if (_owner != null)
			{
				hourRange.SetOwner(_owner);
			}
			base.List.Insert(index, hourRange);
			InvalidateOwnerHour();
		}

		public void AddRange(SchedulerHourRange[] hourRanges)
		{
			foreach (SchedulerHourRange schedulerHourRange in hourRanges)
			{
				if (_owner != null)
				{
					schedulerHourRange.SetOwner(_owner);
				}
				base.List.Add(schedulerHourRange);
			}
			InvalidateOwnerHour();
		}

		public void Remove(SchedulerHourRange hourRange)
		{
			hourRange.SetOwner(null);
			base.List.Remove(hourRange);
			InvalidateOwnerHour();
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			InvalidateOwnerHour();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			SchedulerHourRange schedulerHourRange = (SchedulerHourRange)value;
			schedulerHourRange.SetOwner(null);
			schedulerHourRange._index = -1;
			InvalidateOwnerHour();
		}

		// The last range that contains the time wins.
		internal SchedulerHourRange FindRangeForTime(TimeSpan timeSpan)
		{
			SchedulerHourRange result = null;
			foreach (SchedulerHourRange item in base.List)
			{
				if (timeSpan >= item.StartTime && timeSpan < item.EndTime)
				{
					result = item;
				}
			}
			return result;
		}

		internal SchedulerFormatStyle GetFormatStyleForTime(TimeSpan timeSpan, bool isAlternating, SchedulerFormatStyle schedulerFormatStyle, SchedulerFormatStyle closedFormatStyle)
		{
			SchedulerHourRange range = FindRangeForTime(timeSpan);
			if (range == null)
			{
				return null;
			}
			SchedulerFormatStyle baseStyle = range.IsWorkingTime ? schedulerFormatStyle : closedFormatStyle;
			return (!isAlternating) ? baseStyle.MergeWith(range.FormatStyle) : baseStyle.MergeWith(range.AlternatingFormatStyle);
		}

		internal void InvalidateOwnerHour()
		{
			if (_owner != null)
			{
				_owner.InvalidateHour();
			}
		}
	}
}
