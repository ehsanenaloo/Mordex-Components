using System;

namespace Mordex.Windows.DataGrid
{
	public class GroupActionEventArgs : EventArgs
	{
		private readonly GridGroup _group;

		public GridGroup Group => _group;

		internal GroupActionEventArgs(GridGroup group)
		{
			_group = group;
		}
	}
}
