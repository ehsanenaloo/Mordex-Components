using System.Drawing;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class RowHeaderCellRenderer : HeaderCellRendererBase
	{
		internal RowHeaderCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		// The row mark (current row, new row, filter, total, being edited) is painted after the text at the far side, where the
		// text leaves 16 pixels. The error image is painted closer in, so a grid that shows errors draws the text at once.
		internal override Rectangle GetTextFreeArea(GridGroupLayoutState item, Rectangle bounds, Rectangle gridlineFreeArea)
		{
			Rectangle area = base.GetTextFreeArea(item, bounds, gridlineFreeArea);
			MxDataGrid dataGrid = item._gridGroupLayoutStateOwner?.GridRow?.DataGrid;
			if (area.IsEmpty || dataGrid == null || dataGrid.ShowErrors)
			{
				return Rectangle.Empty;
			}
			area.Width -= 16;
			if (_owner.RightToLeft)
			{
				area.X += 16;
			}
			return area.Width > 0 ? area : Rectangle.Empty;
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (!IsEmptyBounds(rect) && item != null && item.GetOwnerTable() != null)
			{
				if (_owner.RightToLeft)
				{
					rect.X += 16;
				}
				rect.Width -= 16;
				if (rect.Width >= 0)
				{
					base.PaintForeground(graphics, item, style, rect, selected: false, focused: false);
				}
			}
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
			base.PaintAddornments(graphics, item, style, rect);
			if (item._gridGroupLayoutStateOwner == null || item._gridGroupLayoutStateOwner.GridRow == null)
			{
				return;
			}
			MxDataGrid dataGrid = item._gridGroupLayoutStateOwner.GridRow.DataGrid;
			if (dataGrid == null)
			{
				return;
			}
			bool condition = dataGrid.Row == item._gridGroupLayoutStateOwner.GridRow.Position;
			if (item._gridGroupLayoutStateOwner.GridRow.GetLogicalRow() is DataMordexRow)
			{
				condition = false;
			}
			bool otherCondition = item._gridGroupLayoutStateOwner.GridRow.RowType == RowType.FilterRow;
			bool nextCondition = item._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord;
			bool previousCondition = item._gridGroupLayoutStateOwner.GridRow.RowType == RowType.TotalRow;
			bool firstCondition = false;
			if (condition && !otherCondition && item._gridGroupLayoutStateOwner.GridRow.HasChangedCells())
			{
				firstCondition = true;
			}
			Color color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderBorder);
			if (!_drawingStyle.IsThemedLook && !_drawingStyle._isThemed && style != null && !style.ForeColor.IsEmpty)
			{
				color = style.ForeColor;
			}
			if (firstCondition)
			{
				if (!_owner.IsPrinting())
				{
					CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetEditingImage(), Color.Magenta), rect, color, _owner.RightToLeft, ImageHorizontalAlignment.Far, _owner.RightToLeft);
				}
			}
			else if (otherCondition)
			{
				CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetFilterImage(), Color.Magenta), rect, color, _owner.RightToLeft, ImageHorizontalAlignment.Far, _owner.RightToLeft);
			}
			else if (previousCondition)
			{
				CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetSumImage(), Color.Magenta), rect, color, false, ImageHorizontalAlignment.Far, _owner.RightToLeft);
			}
			else if (condition)
			{
				if (!_owner.IsPrinting())
				{
					CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetCurrentRowImage(), Color.Magenta), rect, color, _owner.RightToLeft, ImageHorizontalAlignment.Far, _owner.RightToLeft);
				}
			}
			else if (nextCondition)
			{
				CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetNewXpImage(), Color.Magenta), rect, color, _owner.RightToLeft, ImageHorizontalAlignment.Far, _owner.RightToLeft);
			}
			if (dataGrid.ShowErrors && item._gridGroupLayoutStateOwner.GridRow.GetTextOrEmpty().Length > 0)
			{
				Image image = dataGrid.GetErrorImage();
				if (image != null)
				{
					Rectangle rectangle = rect;
					rectangle.Inflate(-2, -2);
					GridHelper.DrawImageAndText(graphics, null, "", image, rectangle, ImageHorizontalAlignment.Far, ImageVerticalAlignment.Center, null, null, item.IsRightToLeft(_owner.RightToLeft), -1, null, _owner.IsPrinting());
				}
			}
		}
	}
}
