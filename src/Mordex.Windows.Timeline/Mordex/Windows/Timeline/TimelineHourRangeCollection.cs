using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class TimelineHourRangeCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private ITimelineWorkingHoursOwner _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "HourRange";

		public MxTimeline Timeline
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Timeline;
				}
				return null;
			}
		}

		public TimelineHourRange this[int index] => (TimelineHourRange)base.List[index];

		public TimelineHourRange this[string key]
		{
			get
			{
				foreach (TimelineHourRange item in base.List)
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
				TimelineHourRange timelineHourRange = new TimelineHourRange();
				base.InnerList.Add(timelineHourRange);
				timelineHourRange._owner = _owner;
				timelineHourRange._index = index;
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
					TimelineHourRange value = (TimelineHourRange)enumerator.Current;
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

		private TimelineHourRangeCollection()
		{
		}

		internal TimelineHourRangeCollection(ITimelineWorkingHoursOwner owner)
		{
			_owner = owner;
		}

		public void Add(TimelineHourRange hourRange)
		{
			hourRange._owner = _owner;
			base.List.Add(hourRange);
			InvalidateTimeline();
		}

		public void AddRange(TimelineHourRange[] hourRanges)
		{
			foreach (TimelineHourRange timelineHourRange in hourRanges)
			{
				timelineHourRange._owner = _owner;
				base.List.Add(timelineHourRange);
			}
			InvalidateTimeline();
		}

		public void Remove(TimelineHourRange hourRange)
		{
			hourRange._owner = null;
			base.List.Remove(hourRange);
			InvalidateTimeline();
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			InvalidateTimeline();
		}

		internal void InvalidateTimeline()
		{
			if (Timeline != null)
			{
				Timeline.Invalidate();
			}
		}
	}
}
