using System;
using System.Collections;

namespace Mordex.Windows.Timeline
{
	public class TimelineSelectedItemCollection : ICollection, IEnumerable
	{
		private ArrayList _items;

		internal MxTimeline _timeline;

		public ITimelineObject this[int index] => (ITimelineObject)_items[index];

		public ITimelineObject this[string key]
		{
			get
			{
				foreach (TimelineItem item in _items)
				{
					if (item.Key == key)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		public int Count => _items.Count;

		public object SyncRoot => _items.SyncRoot;

		public bool IsReadOnly => _items.IsReadOnly;

		public bool IsSynchronized => _items.IsSynchronized;

		internal TimelineSelectedItemCollection(MxTimeline timeline)
		{
			_items = new ArrayList();
			_timeline = timeline;
		}

		public bool Contains(ITimelineObject item)
		{
			return _items.Contains(item);
		}

		public void Clear()
		{
			if (_items.Count <= 0)
			{
				return;
			}
			foreach (ITimelineObject item in _items)
			{
				item.Selected = false;
			}
			_items.Clear();
			UpdateLayout();
		}

		private void UpdateLayout()
		{
			_timeline.Invalidate();
			_timeline.RaiseSelectedItemsChanged();
		}

		public void Remove(ITimelineObject item)
		{
			if (_items.Contains(item))
			{
				item.Selected = false;
				_items.Remove(item);
				UpdateLayout();
			}
		}

		public void Add(ITimelineObject item)
		{
			if (!_items.Contains(item))
			{
				item.Selected = true;
				_items.Add(item);
				UpdateLayout();
			}
		}

		public void CopyTo(Array destination, int index)
		{
			_items.CopyTo(destination, index);
		}

		public IEnumerator GetEnumerator()
		{
			return _items.GetEnumerator();
		}
	}
}
