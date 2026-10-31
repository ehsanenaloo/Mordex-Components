using System;

namespace Mordex.Windows.Navigation
{
	public class ItemEventArgs : EventArgs
	{
		private NavigationItem _item;

		public NavigationItem Item => _item;

		public ItemEventArgs(NavigationItem item)
		{
			_item = item;
		}
	}
}
