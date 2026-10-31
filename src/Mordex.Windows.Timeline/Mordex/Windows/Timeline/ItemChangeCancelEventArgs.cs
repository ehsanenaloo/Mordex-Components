using System;

namespace Mordex.Windows.Timeline
{
	public class ItemChangeCancelEventArgs : EventArgs
	{
		private TimelineItem _item;

		private ItemChange _propertyChanged;

		private bool _cancel;

		private object _proposedValue;

		public TimelineItem Item => _item;

		public ItemChange PropertyChanged => _propertyChanged;

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

		public object ProposedValue
		{
			get
			{
				return _proposedValue;
			}
			set
			{
				_proposedValue = value;
			}
		}

		internal ItemChangeCancelEventArgs(TimelineItem item, ItemChange propertyChanged, object newValue)
		{
			_item = item;
			_propertyChanged = propertyChanged;
			_proposedValue = newValue;
		}
	}
}
