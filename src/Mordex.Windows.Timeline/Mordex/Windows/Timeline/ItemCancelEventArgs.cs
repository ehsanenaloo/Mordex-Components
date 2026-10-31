using System;

namespace Mordex.Windows.Timeline
{
	public class ItemCancelEventArgs : EventArgs
	{
		private TimelineItem _item;

		private bool _cancel;

		public TimelineItem Item => _item;

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

		internal ItemCancelEventArgs(TimelineItem item)
		{
			_item = item;
		}
	}
}
