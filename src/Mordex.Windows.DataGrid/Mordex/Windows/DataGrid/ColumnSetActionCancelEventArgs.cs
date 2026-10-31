using System;

namespace Mordex.Windows.DataGrid
{
	public class ColumnSetActionCancelEventArgs : EventArgs
	{
		private readonly GridColumnSet _columnSet;

		private bool _cancel;

		public GridColumnSet ColumnSet => _columnSet;

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

		internal ColumnSetActionCancelEventArgs(GridColumnSet columnSet, bool cancel)
		{
			_columnSet = columnSet;
			_cancel = cancel;
		}
	}
}
