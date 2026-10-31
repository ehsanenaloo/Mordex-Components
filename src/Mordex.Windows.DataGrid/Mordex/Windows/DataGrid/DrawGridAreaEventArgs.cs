using System;
using System.Drawing;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class DrawGridAreaEventArgs : EventArgs
	{
		internal CellRendererBase _itemRenderer;

		internal bool _isBackgroundPainted;

		internal bool _isFocused;

		private readonly MxDataGrid _dataGrid;

		private readonly GridGroupLayoutState _layoutState;

		private readonly Graphics _graphics;

		private bool _handled;

		private readonly OwnerDrawnArea _area;

		private readonly Rectangle _bounds;

		private readonly Font _font;

		private GridFormatStyle _formatStyle;

		private readonly StringFormat _stringFormat;

		public GridTable Table
		{
			get
			{
				if (_layoutState == null)
				{
					return null;
				}
				return _layoutState.GetOwnerTable();
			}
		}

		public GridColumnSet ColumnSet
		{
			get
			{
				if (_layoutState == null)
				{
					return null;
				}
				return _layoutState._columnSet;
			}
		}

		public GridColumn Column
		{
			get
			{
				if (_layoutState == null)
				{
					return null;
				}
				return _layoutState._column;
			}
		}

		public GridRow Row
		{
			get
			{
				if (_layoutState == null || _layoutState._gridGroupLayoutStateOwner == null)
				{
					return null;
				}
				return _layoutState._gridGroupLayoutStateOwner.GridRow;
			}
		}

		public Graphics Graphics => _graphics;

		public bool Handled
		{
			get
			{
				return _handled;
			}
			set
			{
				_handled = value;
			}
		}

		public OwnerDrawnArea GridArea => _area;

		public Rectangle Bounds => _bounds;

		public Font Font => _font;

		public Color BackColor
		{
			get
			{
				if (_formatStyle != null)
				{
					return _formatStyle.GetEffectiveBackColor();
				}
				return _dataGrid.BackColor;
			}
		}

		public Brush BackBrush
		{
			get
			{
				if (_formatStyle != null)
				{
					Brush brush = _formatStyle.GetOrCreateBackBrush(_bounds);
					if (brush == null)
					{
						return new SolidBrush(Color.Transparent);
					}
					return brush;
				}
				return new SolidBrush(_dataGrid.BackColor);
			}
		}

		public Color ForeColor
		{
			get
			{
				if (_formatStyle != null)
				{
					return _formatStyle.ForeColor;
				}
				return Color.Empty;
			}
		}

		public Brush ForeBrush
		{
			get
			{
				if (_formatStyle != null)
				{
					return _formatStyle.GetForeBrush();
				}
				return null;
			}
		}

		public StringFormat StringFormat => _stringFormat;

		internal DrawGridAreaEventArgs(Graphics graphics, Rectangle bounds, Font font, GridFormatStyle style, StringFormat stringFormat, GridGroupLayoutState item, MxDataGrid dataGrid)
		{
			switch (item._type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.CardCell:
				if (item._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord)
				{
					if (dataGrid.View == View.TableView)
					{
						_area = OwnerDrawnArea.NewRowCell;
					}
					else
					{
						_area = OwnerDrawnArea.NewCardCell;
					}
				}
				else if (dataGrid.View == View.TableView)
				{
					_area = OwnerDrawnArea.Cell;
				}
				else
				{
					_area = OwnerDrawnArea.CardCell;
				}
				break;
			case LayoutItemType.GroupTotalCell:
				_area = OwnerDrawnArea.GroupTotalCell;
				break;
			case LayoutItemType.TotalRowCell:
				_area = OwnerDrawnArea.TotalRowCell;
				break;
			case LayoutItemType.GroupByBoxInfo:
				_area = OwnerDrawnArea.GroupByBoxInfoText;
				break;
			case LayoutItemType.PreviewRow:
				_area = OwnerDrawnArea.PreviewRow;
				break;
			case LayoutItemType.ColumnHeader:
				_area = OwnerDrawnArea.ColumnHeader;
				break;
			case LayoutItemType.CardHeader:
				_area = OwnerDrawnArea.CardColumnHeader;
				break;
			case LayoutItemType.CardCaption:
				_area = OwnerDrawnArea.CardCaption;
				break;
			case LayoutItemType.GroupHeaderRow:
				_area = OwnerDrawnArea.GroupRow;
				break;
			case LayoutItemType.RowHeader:
				_area = OwnerDrawnArea.RowHeader;
				break;
			case LayoutItemType.GroupByBoxBackground:
				_area = OwnerDrawnArea.GroupByBoxBackground;
				break;
			case LayoutItemType.NewRowSeparator:
				_area = OwnerDrawnArea.NewRowSeparator;
				break;
			case LayoutItemType.TotalRowSeparator:
				_area = OwnerDrawnArea.TotalRowSeparator;
				break;
			case LayoutItemType.ColumnSetHeader:
				_area = OwnerDrawnArea.ColumnSetHeader;
				break;
			}
			_layoutState = item;
			_graphics = graphics;
			_bounds = bounds;
			_font = font;
			_formatStyle = style;
			_stringFormat = stringFormat;
			_dataGrid = dataGrid;
		}

		internal DrawGridAreaEventArgs(Graphics graphics, Rectangle bounds, GridFormatStyle style, MxDataGrid dataGrid, OwnerDrawnArea area)
		{
			_area = area;
			_graphics = graphics;
			_bounds = bounds;
			_formatStyle = style;
			_dataGrid = dataGrid;
		}

		public void PaintBackground()
		{
			switch (_area)
			{
			case OwnerDrawnArea.GroupByBoxBackground:
				_dataGrid.DrawGroupByBoxBackground(_graphics, true);
				return;
			case OwnerDrawnArea.ControlBackground:
				_dataGrid.DrawControlBackground(_graphics, _bounds, _bounds, true, true);
				return;
			}
			if (_itemRenderer != null)
			{
				_itemRenderer.PaintBackground(_graphics, _layoutState, _formatStyle, _bounds, _isBackgroundPainted, _isFocused);
			}
		}
	}
}
