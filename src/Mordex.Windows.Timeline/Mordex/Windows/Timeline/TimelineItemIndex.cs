using System;
using System.Collections;
using System.Collections.Generic;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline
{
	// The items of a timeline ordered by start time, for range queries. Adding, removing and moving an item change the order in
	// place; a reset of the rows (new data, a filter, a regroup) makes the next query build it again.
	internal sealed class TimelineItemIndex
	{
		private struct Entry
		{
			internal TimelineItem Item;

			internal long Start;

			internal long End;
		}

		private readonly MxTimeline _timeline;

		private readonly List<Entry> _entries = new List<Entry>();

		// The start each item has in the list, to find its place again after the item moved.
		private readonly Dictionary<TimelineItem, long> _starts = new Dictionary<TimelineItem, long>(ReferenceEqualityComparer.Instance);

		private MordexRowManager _rows;

		private bool _valid;

		// An item was taken out and the row list has not yet said that it changed.
		private bool _removalPending;

		// The row added last and its item, while no change to another item came after it.
		private MordexRow _addedRow;

		private TimelineItem _addedItem;

		// No item is longer than this, so an item that ends inside a range starts at most this much before it.
		private long _longest;

		// For tests: how many times the order was built from all rows on this thread.
		[ThreadStatic]
		internal static long Rebuilds;

		internal TimelineItemIndex(MxTimeline timeline)
		{
			_timeline = timeline;
		}

		internal void Invalidate()
		{
			_valid = false;
			_removalPending = false;
			ForgetAdded();
		}

		private void ForgetAdded()
		{
			_addedRow = null;
			_addedItem = null;
		}

		// The child rows of a row were loaded again. The parent row says this also when it commits a new row, right after the
		// row of the item was added; the order already holds that item then.
		internal void ChildRowsReset(MordexRow row)
		{
			if (_valid && row != null && _addedRow != null && ReferenceEquals(row, _addedRow.ParentRow))
			{
				ForgetAdded();
				return;
			}
			Invalidate();
		}

		// The row list changed as a whole. When that is only the removal of the item just taken out, the order still holds;
		// anything else (a filter, a regroup, rows shown again) makes the next query build it again.
		internal void RowsChanged(MordexRowManager rows)
		{
			if (_valid && _removalPending && ReferenceEquals(rows, _rows) && rows != null && rows.Count == _entries.Count)
			{
				_removalPending = false;
				return;
			}
			Invalidate();
		}

		// The row of an item was added.
		internal void Add(MordexRow row, TimelineItem item)
		{
			ForgetAdded();
			if (!_valid || item == null)
			{
				return;
			}
			Take(item);
			if (_valid)
			{
				Insert(item);
				_addedRow = row;
				_addedItem = item;
			}
		}

		// The start or the end of an item may have changed. An item that is not in the list stays out.
		internal void Update(TimelineItem item)
		{
			if (!ReferenceEquals(item, _addedItem))
			{
				ForgetAdded();
			}
			if (!_valid || item == null || !_starts.ContainsKey(item))
			{
				return;
			}
			Take(item);
			if (_valid)
			{
				Insert(item);
			}
		}

		// The row of an item is going away.
		internal void Remove(TimelineItem item)
		{
			ForgetAdded();
			if (_valid && item != null && Take(item))
			{
				_removalPending = true;
			}
		}

		private bool Take(TimelineItem item)
		{
			if (!_starts.TryGetValue(item, out long start))
			{
				return false;
			}
			_starts.Remove(item);
			for (int i = LowerBound(start); i < _entries.Count && _entries[i].Start == start; i++)
			{
				if (ReferenceEquals(_entries[i].Item, item))
				{
					_entries.RemoveAt(i);
					return true;
				}
			}
			// The list and the map disagree; build both again on the next query.
			Invalidate();
			return false;
		}

		// The items that end at or after the start of the range and start at or before its end, in TimelineComparer order.
		internal ArrayList GetItemsInRange(DateRange dateRange)
		{
			MordexRowManager rows = _timeline.GetTimelineDataSourceManager().NonGroupedRowManager;
			_removalPending = false;
			ForgetAdded();
			if (!_valid || !ReferenceEquals(rows, _rows) || rows == null || rows.Count != _entries.Count)
			{
				Rebuild(rows);
			}
			long from = dateRange.Start.Ticks;
			long to = dateRange.End.Ticks;
			ArrayList result = new ArrayList();
			int runStart = 0;
			long runKey = long.MinValue;
			for (int i = LowerBound(from - _longest); i < _entries.Count && _entries[i].Start <= to; i++)
			{
				Entry entry = _entries[i];
				if (entry.End < from)
				{
					continue;
				}
				if (entry.Start != runKey)
				{
					SortRun(result, runStart);
					runStart = result.Count;
					runKey = entry.Start;
				}
				result.Add(entry.Item);
			}
			SortRun(result, runStart);
			return result;
		}

		// Items with the same start are ordered by the rest of TimelineComparer: duration, text, index.
		private static void SortRun(ArrayList result, int runStart)
		{
			int count = result.Count - runStart;
			if (count > 1)
			{
				result.Sort(runStart, count, new TimelineComparer());
			}
		}

		private void Rebuild(MordexRowManager rows)
		{
			Rebuilds++;
			_entries.Clear();
			_starts.Clear();
			_longest = 0;
			_rows = rows;
			if (rows != null)
			{
				foreach (MordexRow row in (IEnumerable)rows)
				{
					TimelineItem item = TimelineDataSourceManager.GetTimelineItem(row);
					if (item != null && !_starts.ContainsKey(item))
					{
						Entry entry = MakeEntry(item);
						_entries.Add(entry);
						_starts.Add(item, entry.Start);
						_longest = Math.Max(_longest, entry.End - entry.Start);
					}
				}
			}
			_entries.Sort((a, b) => a.Start.CompareTo(b.Start));
			_valid = true;
		}

		private void Insert(TimelineItem item)
		{
			Entry entry = MakeEntry(item);
			_entries.Insert(LowerBound(entry.Start + 1), entry);
			_starts.Add(item, entry.Start);
			_longest = Math.Max(_longest, entry.End - entry.Start);
		}

		private static Entry MakeEntry(TimelineItem item)
		{
			return new Entry { Item = item, Start = item.StartTime.Ticks, End = item.EndTime.Ticks };
		}

		// The first position whose start is not less than the key.
		private int LowerBound(long key)
		{
			int low = 0;
			int high = _entries.Count;
			while (low < high)
			{
				int middle = low + (high - low) / 2;
				if (_entries[middle].Start < key)
				{
					low = middle + 1;
				}
				else
				{
					high = middle;
				}
			}
			return low;
		}
	}
}
