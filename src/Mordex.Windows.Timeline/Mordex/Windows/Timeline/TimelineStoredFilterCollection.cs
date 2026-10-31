using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline
{
	public class TimelineStoredFilterCollection : MordexStoredFilterCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "StoredFilter";

		public TimelineStoredFilter this[int index] => (TimelineStoredFilter)BaseGet(index);

		public TimelineStoredFilter this[string key]
		{
			get
			{
				TimelineStoredFilter timelineStoredFilter = (TimelineStoredFilter)BaseGet(key);
				if (timelineStoredFilter == null)
				{
					throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
				}
				return timelineStoredFilter;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int index = 0;
			for (PropertyValue propertyValue = info["StoredFilter" + index]; propertyValue != null; propertyValue = info["StoredFilter" + index])
			{
				TimelineStoredFilter timelineStoredFilter = new TimelineStoredFilter();
				base.List.Add(timelineStoredFilter);
				index++;
				propertyValue.DeserializeObject(timelineStoredFilter);
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
					TimelineStoredFilter value = (TimelineStoredFilter)enumerator.Current;
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

		internal TimelineStoredFilterCollection(TimelineMordexTableBase table)
			: base(table)
		{
		}

		private TimelineMordexTableBase GetTimelineTable()
		{
			return base.BaseTable as TimelineMordexTableBase;
		}

		internal MxTimeline GetTimeline()
		{
			return GetTimelineTable()?.GetTimeline();
		}

		public int IndexOf(TimelineStoredFilter storedFilter)
		{
			return BaseIndexOf(storedFilter);
		}

		public void Add(TimelineStoredFilter storedFilter)
		{
			BaseAdd(storedFilter);
		}

		public void AddRange(TimelineStoredFilter[] storedFilters)
		{
			foreach (TimelineStoredFilter storedFilter in storedFilters)
			{
				BaseAdd(storedFilter);
			}
		}

		public void Remove(TimelineStoredFilter storedFilter)
		{
			BaseRemove(storedFilter);
		}
	}
}
