using System;

namespace Mordex.Windows.Timeline
{
	public class DroppingItemEventArgs : EventArgs
	{
		private DateTime _newStart = DateTime.MinValue;

		private DateTime _newEnd = DateTime.MinValue;

		private TimelineGroupRow _groupRow;

		private TimelineItem _item;

		private bool _cancel;

		public DateTime ProposedStartTime => _newStart;

		public DateTime ProposedEndTime => _newEnd;

		public TimelineGroupRow ProposedGroupRow => _groupRow;

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

		internal DroppingItemEventArgs(TimelineItem item, DateTime newStart, DateTime newEnd, TimelineGroupRow groupRow)
		{
			_item = item;
			_newEnd = newEnd;
			_newStart = newStart;
			_groupRow = groupRow;
		}
	}
}
