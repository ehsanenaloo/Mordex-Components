using System.Collections;
using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridTableLayoutState : ILayoutElement
	{
		internal ArrayList _arrayList;

		internal GridTable _ownerTable;

		internal GridGroupLayoutState _gridGroupLayoutState;

		internal int _headerHeight;

		internal int _cardColumnIndex;

		internal Size _contentSize;

		internal LayoutRowType _rowType;

		internal Point _location;

		internal GridRow _gridRow;

		private MxDataGrid _dataGrid;

		IList ILayoutElement.Items => _arrayList;

		public GridTable LayoutTable => _ownerTable;

		public GridRow GridRow => _gridRow;

		internal GridTableLayoutState(LayoutRowType rowType, GridTable table)
		{
			_rowType = rowType;
			_ownerTable = table;
			_arrayList = new ArrayList();
		}

		public int GetRowPosition()
		{
			if (_gridRow != null)
			{
				return _gridRow.Position;
			}
			return -1;
		}

		internal int GetLogicalRowIndex()
		{
			return _gridRow.GetPosition();
		}

		internal Rectangle GetBounds()
		{
			return new Rectangle(_location, new Size(_contentSize.Width, _contentSize.Height));
		}

		internal Rectangle GetBoundsBelowHeader()
		{
			return new Rectangle(_location.X, _location.Y + _headerHeight, _contentSize.Width, _contentSize.Height - _headerHeight);
		}

		public Rectangle GetItemRect(Rectangle rect)
		{
			rect.Offset(_location);
			return rect;
		}

		internal int GetHeight()
		{
			return _contentSize.Height;
		}

		internal int GetWidth()
		{
			return _contentSize.Width;
		}

		public void DisposeDataGrid()
		{
			if (_dataGrid != null)
			{
				_dataGrid.Dispose();
				_dataGrid = null;
			}
		}
	}
}
