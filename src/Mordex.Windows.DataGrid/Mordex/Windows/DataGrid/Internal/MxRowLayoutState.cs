using System;
using System.Collections;
using System.Drawing;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class MxRowLayoutState : ILayoutElement
	{
		internal bool _showTreeLines;

		internal MordexRow _mordexRow;

		internal ArrayList _arrayList;

		internal GridTable _owner;

		internal bool _isFirstRow;

		internal int _rowHeight;

		internal int _nextItemX;

		internal int _top;

		internal int _rightEdge;

		internal int _rowPosition;

		internal LayoutRowType _rowType;

		private Point _location;

		internal GridRow _gridRow;

		private Rectangle _itemRect;

		IList ILayoutElement.Items => _arrayList;

		public GridTable LayoutTable => _owner;

		public GridRow GridRow => _gridRow;

		internal MxRowLayoutState(LayoutRowType rowType, GridTable table)
		{
			_rowType = rowType;
			_rowPosition = -1;
			_owner = table;
			_arrayList = new ArrayList();
		}

		internal int GetRowPosition()
		{
			if (_rowType == LayoutRowType.EmptyRow)
			{
				return _rowPosition;
			}
			try
			{
				return _gridRow.GetPosition();
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return 0;
			}
		}

		public Rectangle GetItemRect(Rectangle rect)
		{
			rect.Offset(_location);
			return rect;
		}

		internal Point GetLocation()
		{
			return _location;
		}

		internal void SetLocation(Point point)
		{
			if (_location != point)
			{
				_location = point;
				_itemRect = Rectangle.Empty;
			}
		}

		public GridGroupLayoutState FindCellItemForColumn(GridColumn gridColumn)
		{
			foreach (GridGroupLayoutState item in _arrayList)
			{
				if (item._column == gridColumn && (item._type == LayoutItemType.Cell || item._type == LayoutItemType.FilterRowCell || item._type == LayoutItemType.GroupTotalCell || item._type == LayoutItemType.TotalRowCell))
				{
					return item;
				}
			}
			return null;
		}

		internal Rectangle GetContentBounds()
		{
			if (!_itemRect.IsEmpty)
			{
				return _itemRect;
			}
			int left = -1;
			int right = -1;
			int top = -1;
			int bottom = -1;
			foreach (GridGroupLayoutState item in _arrayList)
			{
				switch (item._type)
				{
				case LayoutItemType.Cell:
				case LayoutItemType.PreviewRow:
				case LayoutItemType.TotalRowCell:
				case LayoutItemType.GroupHeaderRow:
				case LayoutItemType.GroupTotalCell:
				case LayoutItemType.GroupExpandBox:
				case LayoutItemType.EmptyCell:
				case LayoutItemType.FilterRowCell:
					if (top > item._rect.Top || top == -1)
					{
						top = item._rect.Top;
					}
					if (left > item._rect.Left || left == -1)
					{
						left = item._rect.Left;
					}
					if (right < item._rect.Right)
					{
						right = item._rect.Right;
					}
					if (bottom < item._rect.Bottom || bottom == -1)
					{
						bottom = item._rect.Bottom;
					}
					break;
				}
			}
			if (_gridRow != null && (GridRow.DataGrid.GridLines == GridLines.Horizontal || GridRow.DataGrid.GridLines == GridLines.None))
			{
				right++;
			}
			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);
			_itemRect = GetItemRect(rectangle);
			return _itemRect;
		}

		internal Rectangle GetCellsBounds()
		{
			int left = -1;
			int right = -1;
			int top = -1;
			int bottom = -1;
			foreach (GridGroupLayoutState item in _arrayList)
			{
				switch (item._type)
				{
				case LayoutItemType.Cell:
				case LayoutItemType.PreviewRow:
				case LayoutItemType.ColumnHeader:
				case LayoutItemType.TotalRowCell:
				case LayoutItemType.GroupHeaderRow:
				case LayoutItemType.GroupTotalCell:
				case LayoutItemType.RowHeader:
				case LayoutItemType.GroupExpandBox:
				case LayoutItemType.ColumnSetHeader:
				case LayoutItemType.EmptyCell:
				case LayoutItemType.FilterRowCell:
				case LayoutItemType.TableHeader:
					if (top > item._rect.Top || top == -1)
					{
						top = item._rect.Top;
					}
					if (left > item._rect.Left || left == -1)
					{
						left = item._rect.Left;
					}
					if (right < item._rect.Right)
					{
						right = item._rect.Right;
					}
					if (bottom < item._rect.Bottom || bottom == -1)
					{
						bottom = item._rect.Bottom;
					}
					break;
				}
			}
			if (_gridRow != null && (GridRow.DataGrid.GridLines == GridLines.Horizontal || GridRow.DataGrid.GridLines == GridLines.None))
			{
				right++;
			}
			Rectangle rectangle = new Rectangle(left, top, right - left, bottom - top);
			return GetItemRect(rectangle);
		}

		internal int GetRightEdgeBeforeColumn(int value)
		{
			int right = -1;
			foreach (GridGroupLayoutState item in _arrayList)
			{
				switch (item._type)
				{
				case LayoutItemType.RowHeader:
				case LayoutItemType.ExpandBox:
				case LayoutItemType.GroupIndent:
				case LayoutItemType.Connector:
					if (right < item._rect.Right)
					{
						right = item._rect.Right;
					}
					break;
				case LayoutItemType.Cell:
				case LayoutItemType.ColumnHeader:
					if (item._column._position < value && right < item._rect.Right)
					{
						right = item._rect.Right;
					}
					break;
				}
			}
			if (right == -1)
			{
				right = 0;
			}
			return right + _location.X;
		}

		public void ReleaseItems()
		{
			_gridRow = null;
			_owner = null;
			if (_arrayList == null)
			{
				return;
			}
			foreach (GridGroupLayoutState item in _arrayList)
			{
				item.ClearReferences();
			}
			_arrayList.Clear();
			_arrayList = null;
		}
	}
}
