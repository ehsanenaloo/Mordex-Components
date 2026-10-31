using System;

namespace Mordex.Windows.Navigation
{
	public class GroupCancelEventArgs : EventArgs
	{
		private NavigationGroup _group;

		private bool _cancel;

		public NavigationGroup Group => _group;

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

		internal GroupCancelEventArgs(NavigationGroup group)
		{
			_group = group;
		}
	}
}
