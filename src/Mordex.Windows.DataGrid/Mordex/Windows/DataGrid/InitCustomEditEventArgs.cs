using System;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid
{
	public class InitCustomEditEventArgs : EventArgs
	{
		private readonly char _editChar;

		private readonly GridRow _row;

		private readonly GridColumn _column;

		private Control _customEditControl;

		private readonly GridFormatStyle _formatStyle;

		private readonly object _cellValue;

		public char EditChar => _editChar;

		public GridRow Row => _row;

		public GridColumn Column => _column;

		public Control EditControl
		{
			get
			{
				return _customEditControl;
			}
			set
			{
				_customEditControl = value;
			}
		}

		public GridFormatStyle FormatStyle => _formatStyle;

		public object Value => _cellValue;

		internal InitCustomEditEventArgs(GridRow row, GridColumn column, GridFormatStyle style, object value, char editChar)
		{
			_row = row;
			_column = column;
			_formatStyle = style;
			_cellValue = value;
			_editChar = editChar;
		}
	}
}
