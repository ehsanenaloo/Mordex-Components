using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline
{
	public class TimelineGroupHeaderTotalCollection : MordexGroupTotalCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "GroupHeaderTotal";

		public MxTimeline Timeline => GetTimelineTable()?.GetTimeline();

		public TimelineGroupHeaderTotal this[int index] => (TimelineGroupHeaderTotal)BaseGet(index);

		public TimelineGroupHeaderTotal this[string key]
		{
			get
			{
				TimelineGroupHeaderTotal timelineGroupHeaderTotal = (TimelineGroupHeaderTotal)BaseGet(key);
				if (timelineGroupHeaderTotal != null)
				{
					return timelineGroupHeaderTotal;
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' in this collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int index = 0;
			for (PropertyValue propertyValue = info["GroupHeaderTotal" + index]; propertyValue != null; propertyValue = info["GroupHeaderTotal" + index])
			{
				TimelineGroupHeaderTotal timelineGroupHeaderTotal = new TimelineGroupHeaderTotal();
				base.List.Add(timelineGroupHeaderTotal);
				timelineGroupHeaderTotal.SetParentCollection(this);
				propertyValue.DeserializeObject(timelineGroupHeaderTotal);
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
					TimelineGroupHeaderTotal value = (TimelineGroupHeaderTotal)enumerator.Current;
					info.AddValue("GroupHeaderTotal" + index, value);
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

		internal TimelineGroupHeaderTotalCollection(TimelineMordexTableBase table)
			: base(table)
		{
		}

		private TimelineMordexTableBase GetTimelineTable()
		{
			return base.BaseTable as TimelineMordexTableBase;
		}

		public void Add(TimelineGroupHeaderTotal groupHeaderTotal)
		{
			BaseAdd(groupHeaderTotal);
		}

		public void AddRange(TimelineGroupHeaderTotal[] groupHeaderTotals)
		{
			for (int i = 0; i < groupHeaderTotals.Length; i++)
			{
				BaseAdd(groupHeaderTotals[i]);
			}
		}

		public void Remove(TimelineGroupHeaderTotal groupTotal)
		{
			BaseRemove(groupTotal);
		}

		public int IndexOf(TimelineGroupHeaderTotal groupHeaderTotal)
		{
			return BaseIndexOf(groupHeaderTotal);
		}
	}
}
