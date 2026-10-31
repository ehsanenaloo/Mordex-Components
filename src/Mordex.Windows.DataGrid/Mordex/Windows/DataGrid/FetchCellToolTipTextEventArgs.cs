using System;

namespace Mordex.Windows.DataGrid
{
	public class FetchCellToolTipTextEventArgs : EventArgs
	{
		private string _toolTipText;

		private readonly GridColumn _column;

		private readonly GridRow _row;

		public GridColumn Column => _column;

		public GridRow Row => _row;

		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
			}
		}

		internal FetchCellToolTipTextEventArgs(GridRow row, GridColumn column)
		{
			_toolTipText = "";
			_column = column;
			_row = row;
		}
	}
}
