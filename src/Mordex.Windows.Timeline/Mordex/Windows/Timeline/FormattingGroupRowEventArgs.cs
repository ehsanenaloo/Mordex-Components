using System;

namespace Mordex.Windows.Timeline
{
	public class FormattingGroupRowEventArgs : EventArgs
	{
		private TimelineGroupRow _groupRow;

		public TimelineGroupRow GroupRow => _groupRow;

		internal FormattingGroupRowEventArgs(TimelineGroupRow groupRow)
		{
			_groupRow = groupRow;
		}
	}
}
