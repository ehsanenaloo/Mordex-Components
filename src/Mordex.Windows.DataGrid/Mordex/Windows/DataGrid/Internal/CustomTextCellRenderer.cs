using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class CustomTextCellRenderer : CellRendererBase
	{
		internal CustomTextCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
			_gridRenderer5Boolean = false;
		}

		internal override TextLayoutRenderer GetTextRenderer(GridGroupLayoutState item, GridFormatStyle style)
		{
			TextLayoutRenderer textLayoutRenderer = item.ComputeTextLayoutRenderer(style, _drawingStyle.GetUseTextRenderer());
			textLayoutRenderer.SetWordWrap(true);
			if (_owner.RightToLeft)
			{
				textLayoutRenderer.UpdateRightToLeft(RightToLeft.Yes);
			}
			return textLayoutRenderer;
		}
	}
}
