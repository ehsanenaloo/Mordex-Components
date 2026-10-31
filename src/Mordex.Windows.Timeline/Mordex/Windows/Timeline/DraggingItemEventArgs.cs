using System;

namespace Mordex.Windows.Timeline
{
	public class DraggingItemEventArgs : EventArgs
	{
		private ItemDragType _dragType;

		private TimelineItem _item;

		private DateTime _start = DateTime.MinValue;

		private DateTime _end = DateTime.MinValue;

		private TimelineGroupRow _row;

		private bool _cancel;

		public ItemDragType DragType => _dragType;

		public TimelineItem Item => _item;

		public DateTime StartTime => _start;

		public DateTime EndTime => _end;

		public TimelineGroupRow GroupRow => _row;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		internal DraggingItemEventArgs(TimelineItem item, ItemDragType dragType, DateTime start, DateTime end, TimelineGroupRow row)
		{
			_item = item;
			_dragType = dragType;
			_start = start;
			_end = end;
			_row = row;
		}
	}
}
