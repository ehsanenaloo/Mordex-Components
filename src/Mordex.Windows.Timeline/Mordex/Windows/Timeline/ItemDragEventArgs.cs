using System;

namespace Mordex.Windows.Timeline
{
	public class ItemDragEventArgs : EventArgs
	{
		private TimelineItem _item;

		private ItemDragType _dragType;

		private bool _cancel;

		public TimelineItem Item => _item;

		public ItemDragType DragType => _dragType;

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

		internal ItemDragEventArgs(TimelineItem item, ItemDragType dragType)
		{
			_item = item;
			_dragType = dragType;
		}
	}
}
