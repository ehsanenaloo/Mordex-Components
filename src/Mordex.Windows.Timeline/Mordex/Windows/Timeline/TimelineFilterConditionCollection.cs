using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class TimelineFilterConditionCollection : MordexConditionCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "FilterCondition";

		public TimelineFilterCondition this[int index] => (TimelineFilterCondition)BaseGet(index);

		public TimelineFilterCondition this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TimelineFilterCondition timelineFilterCondition = (TimelineFilterCondition)enumerator.Current;
						if (string.Compare(timelineFilterCondition.Key, key, ignoreCase: true) == 0)
						{
							return timelineFilterCondition;
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
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' in this collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int index = 0;
			for (PropertyValue propertyValue = info["FilterCondition" + index]; propertyValue != null; propertyValue = info["FilterCondition" + index])
			{
				TimelineFilterCondition timelineFilterCondition = new TimelineFilterCondition();
				if (owner != null)
				{
					timelineFilterCondition.SetConditionOwner(GetOwner().GetBaseOwner());
				}
				propertyValue.DeserializeObject(timelineFilterCondition);
				BaseAdd(timelineFilterCondition);
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
					TimelineFilterCondition value = (TimelineFilterCondition)enumerator.Current;
					info.AddValue("FilterCondition" + index, value);
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

		internal TimelineFilterConditionCollection(TimelineFilterCondition owner)
			: base(owner)
		{
		}

		internal TimelineFilterCondition GetOwner()
		{
			return (TimelineFilterCondition)owner;
		}

		public int IndexOf(TimelineFilterCondition condition)
		{
			return ((IList)this).IndexOf((object)condition);
		}
	}
}
