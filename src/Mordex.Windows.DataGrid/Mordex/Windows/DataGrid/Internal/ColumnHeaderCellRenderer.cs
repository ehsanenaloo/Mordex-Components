using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ColumnHeaderCellRenderer : HeaderCellRendererBase
	{
		internal ColumnHeaderCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override Rectangle GetForegroundRect(Rectangle rect, GridGroupLayoutState item, GridFormatStyle style)
		{
			Rectangle foregroundRect = base.GetForegroundRect(rect, item, style);
			if (item._column != null && item._showSortIndicator)
			{
				SortOrder sortOrder = SortOrder.Unsorted;
				if (item._gridGroup != null)
				{
					sortOrder = item._gridGroup.SortOrder;
				}
				else if (item._column != null)
				{
					switch (item._column.SortIndicator)
					{
					case SortIndicator.Ascending:
						sortOrder = SortOrder.Ascending;
						break;
					case SortIndicator.Descending:
						sortOrder = SortOrder.Descending;
						break;
					case SortIndicator.Automatic:
						sortOrder = item._column.SortOrder;
						break;
					}
				}
				if (sortOrder != SortOrder.Unsorted)
				{
					if (foregroundRect.Width > 17)
					{
						if (item.IsRightToLeft(_owner.RightToLeft))
						{
							foregroundRect.X += 17;
						}
						foregroundRect.Width -= 17;
					}
					else
					{
						foregroundRect.Width = 0;
					}
				}
			}
			return foregroundRect;
		}

		// The sort arrow is painted after the text, next to it, and a pressed header is darkened over its text: those texts are drawn at once.
		internal override Rectangle GetTextFreeArea(GridGroupLayoutState item, Rectangle bounds, Rectangle gridlineFreeArea)
		{
			bool darkened = item._isPressed && !_drawingStyle.IsThemedLook && !_drawingStyle._isThemed;
			if (darkened || (item._showSortIndicator && ShownSortOrder(item) != SortOrder.Unsorted))
			{
				return Rectangle.Empty;
			}
			return base.GetTextFreeArea(item, bounds, gridlineFreeArea);
		}

		private static SortOrder ShownSortOrder(GridGroupLayoutState item)
		{
			if (item._gridGroup != null)
			{
				return item._gridGroup.SortOrder;
			}
			if (item._column == null)
			{
				return SortOrder.Unsorted;
			}
			switch (item._column.SortIndicator)
			{
			case SortIndicator.Ascending:
				return SortOrder.Ascending;
			case SortIndicator.Descending:
				return SortOrder.Descending;
			case SortIndicator.Automatic:
				return item._column.SortOrder;
			default:
				return SortOrder.Unsorted;
			}
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
			SortOrder sortOrder = SortOrder.Unsorted;
			if (item._showSortIndicator)
			{
				if (item._gridGroup != null)
				{
					sortOrder = item._gridGroup.SortOrder;
				}
				else if (item._column != null)
				{
					switch (item._column.SortIndicator)
					{
					case SortIndicator.Ascending:
						sortOrder = SortOrder.Ascending;
						break;
					case SortIndicator.Descending:
						sortOrder = SortOrder.Descending;
						break;
					case SortIndicator.Automatic:
						sortOrder = item._column.SortOrder;
						break;
					}
				}
			}
			Image image = null;
			switch (sortOrder)
			{
			case SortOrder.Ascending:
				image = GridHelper.MakeBitmapTransparent(ResourceImages.GetUpXPImage(), Color.Magenta);
				break;
			case SortOrder.Descending:
				image = GridHelper.MakeBitmapTransparent(ResourceImages.GetDownXpImage(), Color.Magenta);
				break;
			}
			if (image != null)
			{
				Rectangle foregroundRect = base.GetForegroundRect(rect, item, style);
				if (item.IsRightToLeft(_owner.RightToLeft))
				{
					int x = foregroundRect.X;
					if (item._rightMargin < 2)
					{
						x += 2 - item._rightMargin;
					}
					foregroundRect.Width = image.Width;
				}
				else
				{
					int x = foregroundRect.Right - image.Width;
					if (item._rightMargin < 2)
					{
						x -= 2 - item._rightMargin;
					}
					if (x > foregroundRect.X)
					{
						foregroundRect.X = x;
						foregroundRect.Width = image.Width;
					}
				}
				Color color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SortArrow);
				CellRendererBase.DrawRecoloredImage(graphics, image, foregroundRect, color, false, ImageHorizontalAlignment.Center, _owner.RightToLeft);
			}
			base.PaintAddornments(graphics, item, style, rect);
			if (!_drawingStyle.IsThemedLook && !_drawingStyle._isThemed && item._isPressed)
			{
				Brush brush = GdiCache.Brush(Color.FromArgb(122, Color.Black));
				graphics.FillRectangle(brush, rect);
			}
		}
	}
}
