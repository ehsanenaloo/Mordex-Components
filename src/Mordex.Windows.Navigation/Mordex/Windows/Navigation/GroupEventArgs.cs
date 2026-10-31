using System;

namespace Mordex.Windows.Navigation
{
	public class GroupEventArgs : EventArgs
	{
		private NavigationGroup _group;

		public NavigationGroup Group => _group;

		public GroupEventArgs(NavigationGroup group)
		{
			_group = group;
		}
	}
}
