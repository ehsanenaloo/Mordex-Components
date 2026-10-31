using System;

namespace Mordex.Windows.Navigation
{
	public class ItemCancelEventArgs : EventArgs
	{
		private NavigationItem _item;

		private bool _cancel;

		public NavigationItem Item => _item;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				if (_cancel != value)
				{
					_cancel = value;
				}
			}
		}

		internal ItemCancelEventArgs(NavigationItem item)
		{
			_item = item;
		}
	}
}
