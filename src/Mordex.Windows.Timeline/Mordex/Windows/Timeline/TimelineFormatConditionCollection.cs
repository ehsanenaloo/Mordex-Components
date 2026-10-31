using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class TimelineFormatConditionCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxTimeline _timeline;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Condition";

		public TimelineFormatCondition this[int index] => (TimelineFormatCondition)base.InnerList[index];

		public TimelineFormatCondition this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TimelineFormatCondition timelineFormatCondition = (TimelineFormatCondition)enumerator.Current;
						if (string.Compare(timelineFormatCondition.Key, key, ignoreCase: true) == 0)
						{
							return timelineFormatCondition;
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
			for (PropertyValue propertyValue = info["Condition" + index]; propertyValue != null; propertyValue = info["Condition" + index])
			{
				TimelineFormatCondition timelineFormatCondition = new TimelineFormatCondition();
				base.InnerList.Add(timelineFormatCondition);
				timelineFormatCondition.SetFormatConditionCollection(this);
				timelineFormatCondition._index = index;
				index++;
				propertyValue.DeserializeObject(timelineFormatCondition);
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
					TimelineFormatCondition value = (TimelineFormatCondition)enumerator.Current;
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

		internal TimelineFormatConditionCollection(MxTimeline timeline)
		{
			_timeline = timeline;
		}

		public void Add(TimelineFormatCondition condition)
		{
			if (condition == null)
			{
				throw new ArgumentNullException();
			}
			if (condition.GetTimelineFormatConditionCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (!condition.IsValid(_timeline))
			{
				throw new ArgumentException();
			}
			condition._index = base.List.Count;
			base.InnerList.Add(condition);
			condition.SetFormatConditionCollection(this);
			InvalidateControl();
		}

		public void AddRange(TimelineFormatCondition[] conditions)
		{
			for (int i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] != null)
				{
					base.InnerList.Add(conditions[i]);
					conditions[i]._index = base.Count - 1;
					conditions[i].SetFormatConditionCollection(this);
				}
			}
			InvalidateControl();
		}

		internal void InvalidateControl()
		{
			_timeline.Invalidate();
		}

		public new void RemoveAt(int index)
		{
			TimelineFormatCondition condition = this[index];
			Remove(condition);
		}

		public void Remove(TimelineFormatCondition condition)
		{
			if (condition == null)
			{
				throw new ArgumentNullException();
			}
			base.InnerList.Remove(condition);
			condition.SetFormatConditionCollection(null);
			UpdateTimelineFormatConditionCollection();
			InvalidateControl();
		}

		private void UpdateTimelineFormatConditionCollection()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._index = i;
			}
		}

		protected override void OnClear()
		{
			base.OnClear();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineFormatCondition timelineFormatCondition = (TimelineFormatCondition)enumerator.Current;
					timelineFormatCondition.SetFormatConditionCollection(null);
					timelineFormatCondition._index = -1;
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

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			InvalidateControl();
		}
	}
}
