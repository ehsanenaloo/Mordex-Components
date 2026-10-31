using System;

namespace Mordex.Windows.DataGrid
{
	public class GroupsChangedEventArgs : EventArgs
	{
		private readonly GridTable _changedTable;

		public GridTable Table => _changedTable;

		internal GroupsChangedEventArgs(GridTable table)
		{
			_changedTable = table;
		}
	}
}
