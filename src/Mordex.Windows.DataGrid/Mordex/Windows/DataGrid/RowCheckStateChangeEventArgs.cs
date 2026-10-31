using System;

namespace Mordex.Windows.DataGrid
{
	public class RowCheckStateChangeEventArgs : EventArgs
	{
		private readonly GridRow _row;

		private readonly CheckStateChangeType _changeType;

		private readonly RowCheckState _oldCheckState;

		private readonly RowCheckState _checkState;

		public RowCheckState CheckState => _checkState;

		public RowCheckState OldCheckState => _oldCheckState;

		public GridRow Row => _row;

		public CheckStateChangeType ChangeType => _changeType;

		internal RowCheckStateChangeEventArgs(GridRow row, CheckStateChangeType changeType, RowCheckState oldCheckState, RowCheckState checkState)
		{
			_changeType = changeType;
			_row = row;
			_oldCheckState = oldCheckState;
			_checkState = checkState;
		}
	}
}
