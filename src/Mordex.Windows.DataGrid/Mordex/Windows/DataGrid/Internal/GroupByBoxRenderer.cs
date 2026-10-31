using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GroupByBoxRenderer : CellRendererBase
	{
		internal GroupByBoxRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
		}

		internal override void PaintGridlines(Graphics graphics, GridGroupLayoutState item, Rectangle rect)
		{
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (!IsEmptyBounds(rect))
			{
				if (_drawingStyle._isThemed)
				{
					Brush brush = new LinearGradientBrush(Rectangle.Inflate(rect, 1, 1), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupByBox), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.GroupByBoxGradient), 90f);
					graphics.FillRectangle(brush, rect);
					brush.Dispose();
				}
				else
				{
					base.PaintBackground(graphics, item, style, rect, selected: false, focused: false);
				}
			}
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
		}
	}
}
