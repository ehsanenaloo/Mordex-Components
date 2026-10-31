using System;

namespace Mordex.Windows.DataGrid
{
	public class RowActionEventArgs : EventArgs
	{
		private readonly GridRow _row;

		public GridRow Row => _row;

		internal RowActionEventArgs(GridRow row)
		{
			_row = row;
		}
	}
}
