using System;

namespace Mordex.Windows.Timeline
{
	public class ValidatingItemTextEventArgs : EventArgs
	{
		private string _newValue = "";

		private TimelineItem _item;

		private bool _cancel;

		public string ProposedValue => _newValue;

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

		internal ValidatingItemTextEventArgs(TimelineItem item, string newValue)
		{
			_item = item;
			_newValue = newValue;
		}
	}
}
