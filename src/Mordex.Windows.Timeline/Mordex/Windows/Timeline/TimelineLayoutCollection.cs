using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class TimelineLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private TimelineLayout _parentLayout;

		private MxTimeline _timeline;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public TimelineLayout this[int index] => (TimelineLayout)base.List[index];

		public TimelineLayout this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TimelineLayout timelineLayout = (TimelineLayout)enumerator.Current;
						if (string.Compare(timelineLayout.Key, key, ignoreCase: true) == 0)
						{
							return timelineLayout;
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
				TimelineLayout timelineLayout = new TimelineLayout();
				Add(timelineLayout);
				propertyValue.DeserializeObject(timelineLayout);
				index++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineLayout layout = (TimelineLayout)enumerator.Current;
					if (layout._isCurrentLayout)
					{
						GetOwnerTimeline().CurrentLayout = layout;
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
					TimelineLayout value = (TimelineLayout)enumerator.Current;
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

		internal TimelineLayoutCollection(MxTimeline timeline)
		{
			_timeline = timeline;
		}

		internal TimelineLayoutCollection(TimelineLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal TimelineLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxTimeline GetTimeline()
		{
			if (_parentLayout == null)
			{
				return _timeline;
			}
			return _parentLayout.GetTimeline();
		}

		internal MxTimeline GetOwnerTimeline()
		{
			if (_timeline != null)
			{
				return _timeline;
			}
			if (_parentLayout != null)
			{
				return _parentLayout.GetTimeline();
			}
			return null;
		}

		public void Add(TimelineLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(TimelineLayout[] layouts)
		{
			foreach (TimelineLayout layout in layouts)
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
			if (value.GetType() != typeof(TimelineLayout))
			{
				throw new ArgumentException();
			}
			TimelineLayout timelineLayout = (TimelineLayout)value;
			if (timelineLayout.GetTimelineLayoutCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !timelineLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			((TimelineLayout)value).SetTimelineLayoutCollection(this);
			((TimelineLayout)value).UpdateTimelineLayout();
		}

		public void Remove(TimelineLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxTimeline timeline = GetTimeline();
			if (timeline != null && timeline.CurrentLayout == (TimelineLayout)value)
			{
				timeline.CurrentLayout = null;
			}
			((TimelineLayout)value).SetTimelineLayoutCollection(null);
		}

		internal TimelineLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineLayout timelineLayout = (TimelineLayout)enumerator.Current;
					TimelineLayout currentLayout = timelineLayout.FindCurrentLayout();
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

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineLayout timelineLayout = (TimelineLayout)enumerator.Current;
					timelineLayout.SetTimelineLayoutCollection(null);
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

		public int IndexOf(TimelineLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(TimelineLayout layout)
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
					TimelineLayout timelineLayout = (TimelineLayout)enumerator.Current;
					if (string.Compare(timelineLayout.Key, layoutKey, ignoreCase: true) == 0)
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
