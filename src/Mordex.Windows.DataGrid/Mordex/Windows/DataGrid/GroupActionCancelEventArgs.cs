using System;

namespace Mordex.Windows.DataGrid
{
	public class GroupActionCancelEventArgs : EventArgs
	{
		private readonly GridGroup _group;

		private bool _cancel;

		public GridGroup Group => _group;

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

		internal GroupActionCancelEventArgs(GridGroup group, bool cancel)
		{
			_group = group;
			_cancel = cancel;
		}
	}
}
