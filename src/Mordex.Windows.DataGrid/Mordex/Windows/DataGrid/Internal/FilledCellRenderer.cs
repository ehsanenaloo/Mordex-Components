using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class FilledCellRenderer : CellRendererBase
	{
		internal FilledCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override bool TextMayBeDeferred(GridFormatStyle style)
		{
			return style.Appearance == Appearance.Flat || style.Appearance == Appearance.Empty;
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			GridRow gridRow = item._gridGroupLayoutStateOwner.GridRow;
			base.PaintForeground(graphics, item, style, rect, selected, focused);
		}
	}
}
