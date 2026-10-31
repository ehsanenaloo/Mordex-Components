using System;

namespace Mordex.Windows.DataGrid
{
	public class RowActionCancelEventArgs : EventArgs
	{
		private readonly GridRow _row;

		private bool _cancel;

		public GridRow Row => _row;

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

		internal RowActionCancelEventArgs(GridRow row)
		{
			_row = row;
		}
	}
}
