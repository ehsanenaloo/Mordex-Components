using System;

namespace Mordex.Windows.DataGrid
{
	public class DropDownHideEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private readonly bool _valueSelected;

		public GridColumn Column => _column;

		public bool ValueSelected => _valueSelected;

		internal DropDownHideEventArgs(GridColumn column, bool valueSelected)
		{
			_column = column;
			_valueSelected = valueSelected;
		}
	}
}
