using System.Collections;

namespace Mordex.Windows.DataGrid
{
	internal class FieldChooserItemComparer : IComparer
	{
		private int _objectType;

		private HeaderOrder _order;

		internal FieldChooserItemComparer(int objectType, HeaderOrder order)
		{
			_objectType = objectType;
			_order = order;
		}

		int IComparer.Compare(object value, object valueArgument)
		{
			if (_objectType == 0)
			{
				GridColumn gridColumn = (GridColumn)value;
				GridColumn column = (GridColumn)valueArgument;
				switch (_order)
				{
				case HeaderOrder.Alphabetical:
					return string.Compare(gridColumn.Caption, column.Caption, ignoreCase: true);
				case HeaderOrder.Index:
					return gridColumn.Index.CompareTo(column.Index);
				case HeaderOrder.Position:
					return gridColumn.Position.CompareTo(column.Position);
				}
			}
			else
			{
				GridColumnSet gridColumnSet = (GridColumnSet)value;
				GridColumnSet columnSet = (GridColumnSet)valueArgument;
				switch (_order)
				{
				case HeaderOrder.Alphabetical:
					return string.Compare(gridColumnSet.Caption, columnSet.Caption, ignoreCase: true);
				case HeaderOrder.Index:
					return gridColumnSet.Index.CompareTo(columnSet.Index);
				case HeaderOrder.Position:
					return gridColumnSet.Position.CompareTo(columnSet.Position);
				}
			}
			return 0;
		}
	}
}
