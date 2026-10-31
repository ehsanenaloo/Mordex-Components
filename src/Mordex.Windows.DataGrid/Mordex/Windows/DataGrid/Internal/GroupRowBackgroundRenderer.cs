using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GroupRowBackgroundRenderer : CellRendererBase
	{
		internal Rectangle _gradientBounds;

		internal GroupRowBackgroundRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
			_gradientBounds = new Rectangle(0, 0, 200, 200);
		}

		internal override Brush GetBackBrush(Rectangle rect, GridGroupLayoutState item, GridFormatStyle style, bool selected, bool focused)
		{
			if (selected && focused)
			{
				return base.GetBackBrush(rect, item, style, selected, focused);
			}
			Rectangle bounds = rect;
			MxDataGrid dataGrid = null;
			if (item != null && item._gridGroupLayoutStateOwner != null && item._gridGroupLayoutStateOwner.GridRow != null)
			{
				dataGrid = item._gridGroupLayoutStateOwner.GridRow.DataGrid;
			}
			if (_drawingStyle.IsThemedLook || _drawingStyle._isThemed)
			{
				Color color;
				Color gridColor;
				if (_drawingStyle._isThemed)
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupRow);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupRowGradient);
				}
				else
				{
					color = style.BackColor;
					if (style.BackColorGradient.IsEmpty)
					{
						gridColor = style.BackColor;
						color = GridHelper.BlendColors(Color.White, gridColor, 0.75f);
					}
					else
					{
						gridColor = style.BackColorGradient;
					}
				}
				rect = Rectangle.Inflate(_gradientBounds, 1, 1);
				return new LinearGradientBrush(rect, color, gridColor, 0f);
			}
			return base.GetBackBrush(rect, item, style, selected, focused);
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
		}

		internal override bool TextMayBeDeferred(GridFormatStyle style)
		{
			return GetType() == typeof(GroupRowBackgroundRenderer) || GetType() == typeof(LayoutStateCellRenderer);
		}
	}
}
