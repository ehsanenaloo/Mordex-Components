using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	/// <summary>Finds the layout item, row, column or card of a grid under a client point.</summary>
	internal sealed class GridHitTester
	{
		private readonly MxDataGrid _grid;

		internal GridHitTester(MxDataGrid grid)
		{
			_grid = grid;
		}

		private GridGroupLayoutState FindLayoutItemInRows(RowLayoutList rows, int x, int y)
		{
			if (rows != null)
			{
				foreach (MxRowLayoutState item in rows)
				{
					if (y < item.GetLocation().Y || y > item.GetLocation().Y + item._rowHeight)
					{
						continue;
					}
					for (int i = 0; i < item._arrayList.Count; i++)
					{
						GridGroupLayoutState gridGroupLayoutState = (GridGroupLayoutState)item._arrayList[i];
						if (!gridGroupLayoutState._isVisible)
						{
							continue;
						}
						Rectangle rectangle = _grid.AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
						if (gridGroupLayoutState._type == LayoutItemType.PreviewRow)
						{
							if (_grid.RightToLeft == RightToLeft.Yes)
							{
								if (rectangle.X > _grid.ClientRectangle.X)
								{
									rectangle.X = _grid.ClientRectangle.X;
								}
							}
							else if (rectangle.X < _grid.ClientRectangle.X)
							{
								rectangle.X = _grid.ClientRectangle.X;
							}
						}
						if (!rectangle.Contains(x, y))
						{
							continue;
						}
						if (gridGroupLayoutState._children != null)
						{
							GridGroupLayoutState[] gridGroupLayoutStateArray = gridGroupLayoutState._children;
							foreach (GridGroupLayoutState gridGroupLayoutStateArrayItem in gridGroupLayoutStateArray)
							{
								if (gridGroupLayoutStateArrayItem._isVisible && _grid.AdjustRectangleToViewport(gridGroupLayoutStateArrayItem.GetAbsoluteItemRect(), gridGroupLayoutStateArrayItem._isHorizontallyScrolled).Contains(x, y))
								{
									return gridGroupLayoutStateArrayItem;
								}
							}
						}
						return gridGroupLayoutState;
					}
				}
			}
			return null;
		}

		internal GridColumn FindColumnAtX(GridTable gridTable, int x)
		{
			if (_grid.View == View.TableView)
			{
				if (gridTable == null)
				{
					gridTable = _grid.RootTable;
				}
				if (gridTable == null)
				{
					return null;
				}
				GridColumn gridColumn = FindColumnAtXInRows(_grid._headerRowLayouts, gridTable, x);
				if (gridColumn != null)
				{
					return gridColumn;
				}
				gridColumn = FindColumnAtXInRows(_grid._footerRowLayouts, gridTable, x);
				if (gridColumn != null)
				{
					return gridColumn;
				}
				return FindColumnAtXInRows(_grid._bodyRowLayouts, gridTable, x);
			}
			return null;
		}

		private GridColumn FindColumnAtXInRows(RowLayoutList rows, GridTable gridTable, int x)
		{
			if (rows != null)
			{
				foreach (MxRowLayoutState item in rows)
				{
					for (int i = 0; i < item._arrayList.Count; i++)
					{
						GridGroupLayoutState gridGroupLayoutState = (GridGroupLayoutState)item._arrayList[i];
						if (gridGroupLayoutState._isVisible && gridGroupLayoutState._column != null && gridGroupLayoutState._column.Table == gridTable && (gridGroupLayoutState._type == LayoutItemType.ColumnHeader || gridGroupLayoutState._type == LayoutItemType.Cell || gridGroupLayoutState._type == LayoutItemType.EmptyCell || gridGroupLayoutState._type == LayoutItemType.FilterRowCell || gridGroupLayoutState._type == LayoutItemType.GroupHeaderCell || gridGroupLayoutState._type == LayoutItemType.GroupTotalCell || gridGroupLayoutState._type == LayoutItemType.TotalRowCell))
						{
							Rectangle rectangle = _grid.AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
							if (rectangle.X <= x && rectangle.Right >= x)
							{
								return gridGroupLayoutState._column;
							}
						}
					}
				}
			}
			return null;
		}

		internal GridGroupLayoutState FindLayoutItemAtPoint(int x, int y)
		{
			if (_grid.View == View.TableView)
			{
				GridGroupLayoutState gridGroupLayoutState = FindLayoutItemInRows(_grid._headerRowLayouts, x, y);
				if (gridGroupLayoutState != null)
				{
					return gridGroupLayoutState;
				}
				gridGroupLayoutState = FindLayoutItemInRows(_grid._footerRowLayouts, x, y);
				if (gridGroupLayoutState != null)
				{
					return gridGroupLayoutState;
				}
				gridGroupLayoutState = FindLayoutItemInRows(_grid._bodyRowLayouts, x, y);
				if (gridGroupLayoutState != null)
				{
					return gridGroupLayoutState;
				}
			}
			else if (_grid._cardRowLayouts != null)
			{
				foreach (GridTableLayoutState item in _grid._cardRowLayouts)
				{
					if (!_grid.AdjustRectangleToViewport(item.GetBounds(), false).Contains(x, y))
					{
						continue;
					}
					for (int i = item._arrayList.Count - 1; i >= 0; i--)
					{
						GridGroupLayoutState arrayListItem = (GridGroupLayoutState)item._arrayList[i];
						if (arrayListItem._isVisible && _grid.AdjustRectangleToViewport(arrayListItem.GetAbsoluteItemRect(), arrayListItem._isHorizontallyScrolled).Contains(x, y))
						{
							if (arrayListItem._children != null)
							{
								GridGroupLayoutState[] gridGroupLayoutStateArray = arrayListItem._children;
								foreach (GridGroupLayoutState gridGroupLayoutStateArrayItem in gridGroupLayoutStateArray)
								{
									if (gridGroupLayoutStateArrayItem._isVisible && _grid.AdjustRectangleToViewport(gridGroupLayoutStateArrayItem.GetAbsoluteItemRect(), gridGroupLayoutStateArrayItem._isHorizontallyScrolled).Contains(x, y))
									{
										return gridGroupLayoutStateArrayItem;
									}
								}
							}
							return arrayListItem;
						}
					}
				}
			}
			return GridGroupLayoutState.CreateEmptyItem();
		}

		internal MxRowLayoutState FindRowLayoutAtPoint(Point point)
		{
			if (!_grid.ClientRectangle.Contains(point))
			{
				return null;
			}
			if (_grid._headerRowLayouts != null)
			{
				foreach (MxRowLayoutState item in _grid._headerRowLayouts)
				{
					if (point.Y >= item.GetLocation().Y && point.Y < item.GetLocation().Y + item._rowHeight)
					{
						return item;
					}
				}
			}
			if (_grid._bodyRowLayouts != null)
			{
				foreach (MxRowLayoutState bodyRowLayout in _grid._bodyRowLayouts)
				{
					if (point.Y >= bodyRowLayout.GetLocation().Y && point.Y < bodyRowLayout.GetLocation().Y + bodyRowLayout._rowHeight)
					{
						return bodyRowLayout;
					}
				}
			}
			return null;
		}

		internal GridTableLayoutState FindTableLayoutAtPoint(Point point)
		{
			if (!_grid.ClientRectangle.Contains(point))
			{
				return null;
			}
			if (_grid._cardRowLayouts != null)
			{
				foreach (GridTableLayoutState item in _grid._cardRowLayouts)
				{
					if (item.GetBounds().Contains(point))
					{
						return item;
					}
				}
			}
			return null;
		}

		internal GridGroupLayoutState FindLinkCellAtPoint(int x, int y)
		{
			GridGroupLayoutState gridGroupLayoutState = FindLayoutItemAtPoint(x, y);
			if (gridGroupLayoutState != null && (gridGroupLayoutState._type == LayoutItemType.Cell || gridGroupLayoutState._type == LayoutItemType.CardCell))
			{
				GridCell gridCell = gridGroupLayoutState.GetCell();
				if (gridCell != null && gridCell.CellDisplayType == ColumnType.Link)
				{
					return gridGroupLayoutState;
				}
			}
			return null;
		}

		internal GridGroupLayoutState FindResizableHeaderAtPoint(int x, int y)
		{
			Point point = new Point(x, y);
			MxRowLayoutState mordexRowLayoutState = FindRowLayoutAtPoint(point);
			if (mordexRowLayoutState != null && mordexRowLayoutState._rowType == LayoutRowType.ColumnHeader)
			{
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					Rectangle rectangle = _grid.AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					if ((item._type != LayoutItemType.ColumnHeader && item._type != LayoutItemType.ColumnSetHeader) || !((_grid.RightToLeft != RightToLeft.No) ? new Rectangle(rectangle.Left - 3, rectangle.Top, 5, rectangle.Height) : new Rectangle(rectangle.Right - 3, rectangle.Top, 5, rectangle.Height)).Contains(point))
					{
						continue;
					}
					if (item._type == LayoutItemType.ColumnHeader)
					{
						if (item._column.AllowSize)
						{
							return item;
						}
						return null;
					}
					if (item._type == LayoutItemType.ColumnSetHeader)
					{
						if (item._columnSet.AllowSize)
						{
							return item;
						}
						return null;
					}
				}
			}
			return null;
		}

		internal int FindCardResizeIndexAtPoint(int x, int y)
		{
			if (_grid._cardBounds != null)
			{
				for (int i = 0; i < _grid._cardBounds.Length; i++)
				{
					if (_grid.AdjustRectangleToViewport(_grid._cardBounds[i], false).Contains(x, y))
					{
						return i;
					}
				}
			}
			return -1;
		}

		internal int FindExpandBoxRowAtPoint(int x, int y)
		{
			if (_grid._bodyRowLayouts == null)
			{
				return -1;
			}
			GridGroupLayoutState gridGroupLayoutState = FindLayoutItemAtPoint(x, y);
			if (gridGroupLayoutState._type == LayoutItemType.ExpandBox || gridGroupLayoutState._type == LayoutItemType.GroupExpandBox || gridGroupLayoutState._type == LayoutItemType.SelfReferencingExpandBox)
			{
				return gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.Position;
			}
			return -1;
		}

		internal int FindCardExpandBoxAtPoint(int x, int y)
		{
			if (_grid._cardRowLayouts == null)
			{
				return -1;
			}
			foreach (GridTableLayoutState item in _grid._cardRowLayouts)
			{
				foreach (GridGroupLayoutState arrayListItem in item._arrayList)
				{
					if (arrayListItem._type == LayoutItemType.CardExpandBox && _grid.AdjustRectangleToViewport(arrayListItem.GetAbsoluteItemRect(), arrayListItem._isHorizontallyScrolled).Contains(x, y))
					{
						return item.GetRowPosition();
					}
				}
			}
			return -1;
		}
	}
}
