using System;

namespace Mordex.Windows.DataGrid
{
	public class ColumnActionCancelEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private bool _cancel;

		public GridColumn Column => _column;

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

		internal ColumnActionCancelEventArgs(GridColumn column, bool cancel)
		{
			_column = column;
			_cancel = cancel;
		}
	}
}
