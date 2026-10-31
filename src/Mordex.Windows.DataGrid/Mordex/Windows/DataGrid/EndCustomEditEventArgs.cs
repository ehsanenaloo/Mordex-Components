using System;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid
{
	public class EndCustomEditEventArgs : EventArgs
	{
		private Control _editControl;

		private readonly GridRow _row;

		private readonly GridColumn _column;

		private object _editedValue;

		private bool _dataChanged;

		private bool _cancelUpdate;

		public Control EditControl => _editControl;

		public GridRow Row => _row;

		public GridColumn Column => _column;

		public object Value
		{
			get
			{
				return _editedValue;
			}
			set
			{
				if (_editedValue != value)
				{
					_editedValue = value;
					_dataChanged = true;
				}
			}
		}

		public bool DataChanged
		{
			get
			{
				return _dataChanged;
			}
			set
			{
				_dataChanged = value;
			}
		}

		public bool CancelUpdate
		{
			get
			{
				return _cancelUpdate;
			}
			set
			{
				_cancelUpdate = value;
			}
		}

		internal EndCustomEditEventArgs(GridRow row, GridColumn column, Control editControl, object value)
		{
			_row = row;
			_column = column;
			_editedValue = value;
			_editControl = editControl;
		}
	}
}
