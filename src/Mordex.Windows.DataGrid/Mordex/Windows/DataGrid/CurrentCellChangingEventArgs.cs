using System;

namespace Mordex.Windows.DataGrid
{
	public class CurrentCellChangingEventArgs : EventArgs
	{
		private readonly GridRow _row;

		private readonly GridColumn _column;

		private bool _cancel;

		public GridRow Row => _row;

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

		internal CurrentCellChangingEventArgs(GridRow row, GridColumn column, bool cancel)
		{
			_row = row;
			_column = column;
			_cancel = cancel;
		}
	}
}
