using System;

namespace Mordex.Windows.DataGrid
{
	public class ColumnActionEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		public GridColumn Column => _column;

		internal ColumnActionEventArgs(GridColumn column)
		{
			_column = column;
		}
	}
}
