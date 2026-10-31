using System;

namespace Mordex.Windows.DataGrid
{
	public class ParseEditValueEventArgs : EventArgs
	{
		private readonly GridRow _row;

		private readonly GridColumn _column;

		private Exception _parseException;

		private object _parsedValue;

		private object _convertedValue;

		public GridRow Row => _row;

		public GridColumn Column => _column;

		public Exception ParseException
		{
			get
			{
				return _parseException;
			}
			set
			{
				_parseException = value;
			}
		}

		public object Value => _parsedValue;

		public object ConvertedValue
		{
			get
			{
				return _convertedValue;
			}
			set
			{
				if (_convertedValue != value)
				{
					_convertedValue = value;
					_parseException = null;
				}
			}
		}

		internal ParseEditValueEventArgs(GridRow row, GridColumn column, object proposedValue, object value)
		{
			_row = row;
			_column = column;
			_convertedValue = proposedValue;
			_parsedValue = value;
		}
	}
}
