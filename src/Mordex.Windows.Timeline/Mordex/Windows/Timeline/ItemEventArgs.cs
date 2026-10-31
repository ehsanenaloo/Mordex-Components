using System;

namespace Mordex.Windows.Timeline
{
	public class ItemEventArgs : EventArgs
	{
		private TimelineItem _item;

		public TimelineItem Item => _item;

		internal ItemEventArgs(TimelineItem item)
		{
			_item = item;
		}
	}
}
