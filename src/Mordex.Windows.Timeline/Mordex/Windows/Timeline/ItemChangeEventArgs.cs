using System;

namespace Mordex.Windows.Timeline
{
	public class ItemChangeEventArgs : EventArgs
	{
		private TimelineItem _item;

		private ItemChange _propertyChanged;

		public TimelineItem Item => _item;

		public ItemChange PropertyChanged => _propertyChanged;

		internal ItemChangeEventArgs(TimelineItem item, ItemChange propertyChanged)
		{
			_item = item;
			_propertyChanged = propertyChanged;
		}
	}
}
