using System;

namespace Mordex.Windows.DataGrid
{
	public class GroupsChangingEventArgs : EventArgs
	{
		private readonly GridGroup _group;

		private readonly GroupChangeAction _action;

		private int _proposedPosition;

		private bool _cancel;

		private readonly GridTable _targetTable;

		public GridGroup Group => _group;

		public GroupChangeAction Action => _action;

		public int ProposedPosition
		{
			get
			{
				return _proposedPosition;
			}
			set
			{
				_proposedPosition = value;
			}
		}

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

		public GridTable Table => _targetTable;

		internal GroupsChangingEventArgs(GridGroup group, GridTable table, GroupChangeAction action, int proposedPosition)
		{
			_group = group;
			_targetTable = table;
			_action = action;
			_proposedPosition = proposedPosition;
		}
	}
}
