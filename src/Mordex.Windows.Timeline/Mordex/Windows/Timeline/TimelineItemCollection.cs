using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline
{
	public class TimelineItemCollection : IList, ICollection, IEnumerable, IMordexXmlLayoutsSupport
	{
		private MxTimeline _timeline;

		internal readonly TimelineItemIndex _index;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		public MxTimeline Timeline => _timeline;

		public TimelineItem this[int index] => TimelineDataSourceManager.GetTimelineItem(_timeline.GetTimelineDataSourceManager().NonGroupedRowManager[index]);

		public TimelineItem this[string key]
		{
			get
			{
				for (int i = 0; i < Count; i++)
				{
					TimelineItem timelineItem = this[i];
					if (string.Compare(timelineItem.Key, key, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						return timelineItem;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' in this collection.");
			}
		}

		public int Count => _timeline.GetTimelineDataSourceManager().NonGroupedRowManager.Count;

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		bool IList.IsReadOnly => false;

		bool IList.IsFixedSize => false;

		object ICollection.SyncRoot => ((ICollection)_timeline.GetTimelineDataSourceManager().NonGroupedRootRow).SyncRoot;

		bool ICollection.IsSynchronized => ((ICollection)_timeline.GetTimelineDataSourceManager().NonGroupedRootRow).IsSynchronized;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int itemIndex = 0;
			for (PropertyValue propertyValue = info["Item" + itemIndex]; propertyValue != null; propertyValue = info["Item" + itemIndex])
			{
				TimelineItem timelineItem = new TimelineItem();
				timelineItem._row = _timeline.GetTimelineDataSourceManager().RootRow.NewRow;
				_timeline.DeserializeItem(timelineItem, propertyValue.GetLayoutReader());
				timelineItem._owner = this;
				propertyValue.DeserializeObject(timelineItem);
				itemIndex++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			MordexRowCollection rows = _timeline.GetTimelineDataSourceManager().NonGroupedRootRow.Rows;
			int itemIndex = 0;
			foreach (MordexRow item in (IEnumerable)rows)
			{
				TimelineItem value = TimelineDataSourceManager.GetTimelineItem(item);
				info.AddValue("Item" + itemIndex, value);
				itemIndex++;
			}
		}

		internal TimelineItemCollection(MxTimeline timeline)
		{
			_timeline = timeline;
			_index = new TimelineItemIndex(timeline);
		}

		public void Add(TimelineItem item)
		{
			if (item._owner != null)
			{
				string text = "";
				text = ((item._owner != this) ? "In order to add a TimelineItem to this collection, you must first remove it from it´s current parent collection." : "This TimelineItem already belongs to this collection.");
				throw new InvalidOperationException(text);
			}
			item._row = _timeline.GetTimelineDataSourceManager().RootRow.NewRow;
			item._row.BeginEdit();
			if (!CanAddItem(item))
			{
				item._row.CancelEdit();
				item._row = null;
			}
			else
			{
				_timeline.SaveItemToRow(item);
				item._owner = this;
				NotifyItemAdded(item);
			}
		}

		public int IndexOf(TimelineItem item)
		{
			MordexRowManager nonGroupedRowManager = _timeline.GetTimelineDataSourceManager().NonGroupedRowManager;
			for (int i = 0; i < nonGroupedRowManager.Count; i++)
			{
				MordexDataRow mordexDataRow = nonGroupedRowManager[i] as MordexDataRow;
				TimelineItemState timelineItemState = (TimelineItemState)mordexDataRow.DataHolder.Tag;
				if (timelineItemState._timelineItem == item)
				{
					return i;
				}
			}
			return -1;
		}

		public void CopyTo(Array destination, int index)
		{
			for (int i = 0; i < Count; i++)
			{
				destination.SetValue(this[i], index + i);
			}
		}

		private IList AsList()
		{
			return this;
		}

		private bool CanAddItem(TimelineItem timelineItem)
		{
			if (_timeline == null)
			{
				return true;
			}
			return _timeline.RaiseAddingItem(timelineItem);
		}

		private bool CanRemoveItem(TimelineItem timelineItem)
		{
			if (_timeline == null)
			{
				return true;
			}
			return _timeline.RaiseRemovingItem(timelineItem);
		}

		private void NotifyItemAdded(TimelineItem timelineItem)
		{
			if (_timeline != null)
			{
				_timeline.RaiseItemAdded(timelineItem);
			}
		}

		private void NotifyItemRemoved(TimelineItem timelineItem)
		{
			if (_timeline != null)
			{
				_timeline.RaiseItemRemoved(timelineItem);
			}
		}

		internal ArrayList GetItemsInRange(DateRange dateRange)
		{
			if (!_timeline.Created)
			{
				_timeline.CreateControl();
			}
			return _index.GetItemsInRange(dateRange);
		}

		public void Clear()
		{
			int index;
			for (index = 0; index < AsList().Count; index++)
			{
				TimelineItem item = (TimelineItem)AsList()[index];
				Remove(item);
				index--;
			}
		}

		public void Remove(TimelineItem item)
		{
			if (item._owner == this && item._row != null && CanRemoveItem(item))
			{
				item._row.Delete();
				NotifyItemRemoved(item);
			}
		}

		public void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		public bool Contains(TimelineItem item)
		{
			if (item._owner == this)
			{
				return true;
			}
			return false;
		}

		int IList.Add(object item)
		{
			Add((TimelineItem)item);
			return Count;
		}

		bool IList.Contains(object item)
		{
			foreach (MordexDataRow dataRow in (IEnumerable)_timeline.GetTimelineDataSourceManager().NonGroupedRowManager)
			{
				TimelineItemState timelineItemState = (TimelineItemState)dataRow.DataHolder.Tag;
				if (timelineItemState._timelineItem == item)
				{
					return true;
				}
			}
			return false;
		}

		int IList.IndexOf(object item)
		{
			return IndexOf((TimelineItem)item);
		}

		void IList.Insert(int index, object item)
		{
			throw new NotImplementedException();
		}

		void IList.Remove(object item)
		{
			if (item is TimelineItem timelineItem)
			{
				Remove(timelineItem);
			}
		}

		public IEnumerator GetEnumerator()
		{
			ArrayList arrayList = new ArrayList();
			foreach (MordexDataRow item in (IEnumerable)_timeline.GetTimelineDataSourceManager().NonGroupedRowManager)
			{
				TimelineItemState timelineItemState = (TimelineItemState)item.DataHolder.Tag;
				arrayList.Add(timelineItemState._timelineItem);
			}
			return arrayList.GetEnumerator();
		}
	}
}
