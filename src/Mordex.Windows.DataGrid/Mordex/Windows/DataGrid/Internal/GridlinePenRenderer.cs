using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridlinePenRenderer
	{
		private Color _lineColor;

		private IGridEnvironment _owner;

		private Pen _cachedPen;

		internal GridlinePenRenderer(Color color, IGridEnvironment owner)
		{
			_owner = owner;
			_lineColor = color;
		}

		private Pen GetOrCreatePen()
		{
			if (_cachedPen == null)
			{
				_cachedPen = new Pen(_lineColor);
			}
			return _cachedPen;
		}

		internal virtual void PaintGridlines(Graphics graphics, GridGroupLayoutState item, Rectangle bounds, bool rightToLeft)
		{
			Pen pen = ((_owner.IsPrinting() || item.GetOwnerTable() == null) ? GetOrCreatePen() : item.GetOwnerTable().GetOrCreateGridLinePen());
			if (item.HasBottomBorder())
			{
				graphics.DrawLine(pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			}
			bool hasRightBorder = false;
			if (_owner.IsPrinting())
			{
				if (item._type == LayoutItemType.GroupExpandBox && item.GetOwnerTable() != null)
				{
					GridLines gridLines = item.GetOwnerTable().GetEffectiveGridLines();
					hasRightBorder = gridLines == GridLines.Both || gridLines == GridLines.Vertical || gridLines == GridLines.RowOutline;
				}
				else if (item._type != LayoutItemType.GroupHeaderRow)
				{
					hasRightBorder = item.HasRightBorder();
				}
			}
			bool otherHasRightBorder = item.HasRightBorder();
			if (item.GetOwnerTable() != null && _owner.IsPrinting())
			{
				bool otherCondition = false;
				if (item._type == LayoutItemType.Cell || item._type == LayoutItemType.EmptyCell)
				{
					otherCondition = true;
				}
				if (otherCondition && item.GetOwnerTable().GetEffectiveGridLines() == GridLines.RowOutline && item._gridGroupLayoutStateOwner is MxRowLayoutState mordexRowLayoutState)
				{
					Rectangle rectangle = item.GetAbsoluteItemRect();
					Rectangle itemRectCore = mordexRowLayoutState.GetContentBounds();
					otherHasRightBorder = rectangle.Right == itemRectCore.Right;
					hasRightBorder = rectangle.Left == itemRectCore.Left;
				}
			}
			if (rightToLeft)
			{
				if (hasRightBorder && item._type != LayoutItemType.Connector && item._type != LayoutItemType.ExpandBox)
				{
					graphics.DrawLine(pen, bounds.Right + 1, bounds.Top, bounds.Right + 1, bounds.Bottom - 1);
				}
				if (otherHasRightBorder)
				{
					graphics.DrawLine(pen, bounds.X, bounds.Top, bounds.X, bounds.Bottom - 1);
				}
			}
			else
			{
				if (hasRightBorder && item._type != LayoutItemType.Connector && item._type != LayoutItemType.ExpandBox)
				{
					graphics.DrawLine(pen, bounds.X - 1, bounds.Top, bounds.X - 1, bounds.Bottom - 1);
				}
				if (otherHasRightBorder)
				{
					graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
				}
			}
		}

		internal void RenderContent(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, Rectangle rectangle, bool condition)
		{
			if (gridGroupLayoutState.HasTopBorder())
			{
				Pen pen = ((_owner.IsPrinting() || gridGroupLayoutState.GetOwnerTable() == null) ? GetOrCreatePen() : gridGroupLayoutState.GetOwnerTable().GetOrCreateGridLinePen());
				if (gridGroupLayoutState._type == LayoutItemType.CardCell)
				{
					graphics.DrawLine(pen, rectangle.X - 1, rectangle.Y - 1, rectangle.Right - 1, rectangle.Y - 1);
				}
				else
				{
					graphics.DrawLine(pen, rectangle.X, rectangle.Y - 1, rectangle.Right - 1, rectangle.Y - 1);
				}
			}
			if (gridGroupLayoutState.HasLeftBorder())
			{
				Pen otherPen = ((_owner.IsPrinting() || gridGroupLayoutState.GetOwnerTable() == null) ? GetOrCreatePen() : gridGroupLayoutState.GetOwnerTable().GetOrCreateGridLinePen());
				if (condition)
				{
					graphics.DrawLine(otherPen, rectangle.Right - 1, rectangle.Y - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				}
				else
				{
					graphics.DrawLine(otherPen, rectangle.X - 1, rectangle.Y - 1, rectangle.X - 1, rectangle.Bottom - 1);
				}
			}
		}

		internal void DisposePen()
		{
			if (_cachedPen != null)
			{
				_cachedPen.Dispose();
				_cachedPen = null;
			}
		}
	}
}
