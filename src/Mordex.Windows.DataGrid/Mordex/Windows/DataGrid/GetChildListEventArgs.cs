using System;
using System.Collections;
using System.Data;

namespace Mordex.Windows.DataGrid
{
	public class GetChildListEventArgs : EventArgs
	{
		private GridRow _parentRow;

		private GridTable _childTable;

		private object _childList;

		public GridRow ParentRow => _parentRow;

		public GridTable ChildTable => _childTable;

		public object ChildList
		{
			get
			{
				return _childList;
			}
			set
			{
				if (value != null && !(value is IList) && !(value is DataTable))
				{
					throw new ArgumentException("ChildList must be a DataTable or a collection that implements IList interface.");
				}
				_childList = value;
			}
		}

		internal GetChildListEventArgs(GridRow parentRow, GridTable childTable)
		{
			_parentRow = parentRow;
			_childTable = childTable;
		}
	}
}
