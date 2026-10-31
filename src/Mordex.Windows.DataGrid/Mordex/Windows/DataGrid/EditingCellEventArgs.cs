using System;

namespace Mordex.Windows.DataGrid
{
	public class EditingCellEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private bool _cancel;

		private object _cellValue;

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

		public object Value
		{
			get
			{
				return _cellValue;
			}
			set
			{
				_cellValue = value;
			}
		}

		internal EditingCellEventArgs(GridColumn column, object value)
		{
			_column = column;
			_cellValue = value;
		}
	}
}
