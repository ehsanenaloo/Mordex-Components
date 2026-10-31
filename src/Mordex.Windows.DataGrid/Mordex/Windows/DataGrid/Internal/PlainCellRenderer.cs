using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class PlainCellRenderer : CellRendererBase
	{
		internal PlainCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintGridlines(Graphics graphics, GridGroupLayoutState item, Rectangle rect)
		{
		}

		internal override Brush GetBackBrush(Rectangle rect, GridGroupLayoutState item, GridFormatStyle style, bool selected, bool focused)
		{
			if (_drawingStyle.IsThemedLook && _drawingStyle._isThemed)
			{
				rect.Inflate(1, 1);
				return new LinearGradientBrush(rect, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupByBoxInfo), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupByBoxInfoGradient), 90f);
			}
			if (_drawingStyle._isThemed)
			{
				rect.Inflate(1, 1);
				return new LinearGradientBrush(rect, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupByBoxInfo), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupByBoxInfoGradient), 0f);
			}
			return base.GetBackBrush(rect, item, style, selected, focused);
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			if (_drawingStyle.IsThemedLook)
			{
				Brush otherBackBrush = GetBackBrush(rect, item, style, selected, focused);
				if (otherBackBrush != null)
				{
					graphics.FillRectangle(otherBackBrush, rect);
					otherBackBrush.Dispose();
				}
				PaintBackgroundImage(graphics, rect, style);
			}
			else
			{
				base.PaintBackground(graphics, item, style, rect, selected, focused);
			}
		}

		internal override void PaintBackgroundImage(Graphics graphics, Rectangle rect, GridFormatStyle style)
		{
			if (!_drawingStyle._isThemed)
			{
				base.PaintBackgroundImage(graphics, rect, style);
			}
		}
	}
}
