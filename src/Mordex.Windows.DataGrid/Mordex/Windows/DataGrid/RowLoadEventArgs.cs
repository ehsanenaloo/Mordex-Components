using System;

namespace Mordex.Windows.DataGrid
{
	public class RowLoadEventArgs : EventArgs
	{
		private readonly GridRow _row;

		public GridRow Row => _row;

		internal RowLoadEventArgs(GridRow row)
		{
			_row = row;
		}
	}
}
