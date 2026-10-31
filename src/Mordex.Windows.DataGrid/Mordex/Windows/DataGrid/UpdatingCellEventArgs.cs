using System;

namespace Mordex.Windows.DataGrid
{
	public class UpdatingCellEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private bool _cancel;

		private object _newCellValue;

		private readonly object _initialValue;

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
				return _newCellValue;
			}
			set
			{
				_newCellValue = value;
			}
		}

		public object InitialValue => _initialValue;

		internal UpdatingCellEventArgs(GridColumn column, object initialValue, object value)
		{
			_column = column;
			_initialValue = initialValue;
			_newCellValue = value;
		}
	}
}
