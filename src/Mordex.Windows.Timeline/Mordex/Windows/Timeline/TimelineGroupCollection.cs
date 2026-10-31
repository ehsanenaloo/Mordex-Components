using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline
{
	public class TimelineGroupCollection : MordexGroupCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Group";

		public TimelineGroup this[int index] => (TimelineGroup)BaseGet(index);

		public TimelineGroup this[string key]
		{
			get
			{
				foreach (TimelineGroup item in base.List)
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
			int index = 0;
			for (PropertyValue propertyValue = info["Group" + index]; propertyValue != null; propertyValue = info["Group" + index])
			{
				TimelineGroup timelineGroup = new TimelineGroup();
				timelineGroup._owner = this;
				propertyValue.DeserializeObject(timelineGroup);
				timelineGroup._owner = null;
				BaseAdd(timelineGroup);
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
					TimelineGroup value = (TimelineGroup)enumerator.Current;
					info.AddValue("Group" + index, value);
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

		internal TimelineGroupCollection(TimelineMordexTableBase table)
			: base(table)
		{
		}

		internal MxTimeline GetTimeline()
		{
			return ((TimelineMordexTableBase)base.BaseTable).GetTimeline();
		}

		public void Add(TimelineGroup group)
		{
			BaseAdd(group);
		}

		public void AddRange(TimelineGroup[] groups)
		{
			for (int i = 0; i < groups.Length; i++)
			{
				BaseAdd(groups[i]);
			}
		}

		public void Insert(int index, TimelineGroup group)
		{
			BaseInsert(index, group);
		}

		public void Remove(TimelineGroup group)
		{
			BaseRemove(group);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public bool Contains(TimelineGroup group)
		{
			return BaseContains(group);
		}

		public bool Contains(TimelineField field)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineGroup timelineGroup = (TimelineGroup)enumerator.Current;
					if (timelineGroup.Field != null && timelineGroup.Field == field)
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
	}
}
