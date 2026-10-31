using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridGroupLayoutState
	{
		internal object _cellValue;

		private ButtonStyle _buttonStyle;

		internal GridGroupLayoutState[] _children;

		private bool _hasRendered;

		internal GridGroupLayoutState _owner;

		private bool _hasSize;

		private Size _cachedSize;

		internal bool _isHot;

		internal bool _isPressed;

		internal int _rowHeight;

		internal GridGroup _gridGroup;

		internal bool _isHorizontallyScrolled = true;

		internal BorderFlags _borderFlags;

		internal BorderFlags _gridGroupLayoutStateBorderFlags;

		internal ILayoutElement _gridGroupLayoutStateOwner;

		internal GridColumn _column;

		internal Rectangle _rect;

		internal int _leftMargin;

		internal int _topMargin;

		internal int _rightMargin;

		internal int _bottomMargin;

		internal bool _isVisible = true;

		private bool _isDisabled;

		internal bool _isRibbonStyle;

		internal Image _ownImage;

		internal LayoutItemType _type;

		private MordexRow _mordexRow;

		internal GridColumnSet _columnSet;

		internal bool _showSortIndicator = true;

		internal bool _drawBottomGridline = true;

		internal GridGroupLayoutState(LayoutItemType type, ILayoutElement owner, GridColumn column)
			: this(type, owner, column, Rectangle.Empty)
		{
		}

		internal GridGroupLayoutState(LayoutItemType type, ILayoutElement owner, GridColumn column, Rectangle bounds)
		{
			_column = column;
			_gridGroupLayoutStateOwner = owner;
			_rect = bounds;
			_type = type;
			switch (type)
			{
			case LayoutItemType.Connector:
				_rightMargin = -1;
				break;
			case LayoutItemType.RowHeader:
				_gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom;
				break;
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.ColumnSetHeader:
			case LayoutItemType.EmptyHeader:
				_gridGroupLayoutStateBorderFlags = BorderFlags.Bottom | BorderFlags.ConnectorRight;
				break;
			case LayoutItemType.TableHeader:
				_gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom;
				break;
			case LayoutItemType.NewRowSeparator:
			case LayoutItemType.TotalRowSeparator:
				_gridGroupLayoutStateBorderFlags = BorderFlags.Right | BorderFlags.Bottom | BorderFlags.ConnectorTop;
				break;
			}
		}
	}
}
