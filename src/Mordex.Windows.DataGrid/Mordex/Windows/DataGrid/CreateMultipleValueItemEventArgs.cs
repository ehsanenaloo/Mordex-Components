using System;

namespace Mordex.Windows.DataGrid
{
	public class CreateMultipleValueItemEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private object _itemValue;

		private object _newItem;

		public GridColumn Column => _column;

		public object ItemValue
		{
			get
			{
				return _itemValue;
			}
			set
			{
				_itemValue = value;
			}
		}

		public object Item
		{
			get
			{
				return _newItem;
			}
			set
			{
				_newItem = value;
			}
		}

		internal CreateMultipleValueItemEventArgs(GridColumn column, object itemValue)
		{
			_column = column;
			_itemValue = itemValue;
		}
	}
}
