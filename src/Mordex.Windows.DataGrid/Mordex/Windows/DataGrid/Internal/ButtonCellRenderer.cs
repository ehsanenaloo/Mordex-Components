using System.Drawing;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ButtonCellRenderer : CellRendererBase
	{
		internal ButtonCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			bool enabled = true;
			if (item._gridGroupLayoutStateOwner is MxRowLayoutState)
			{
				MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)item._gridGroupLayoutStateOwner;
				if (mordexRowLayoutState._rowType == LayoutRowType.EmptyRow || (mordexRowLayoutState._rowType == LayoutRowType.NewRow && mordexRowLayoutState.GridRow != null && !mordexRowLayoutState.GridRow.Selected))
				{
					return;
				}
				GridRow gridRow = mordexRowLayoutState._gridRow;
			}
			if (item.GetCell() != null)
			{
				enabled = item.GetCell().Enabled;
			}
			object cellValue = item.GetCellValue();
			RowCheckState rowCheckState = RowCheckState.Unchecked;
			if (cellValue is RowCheckState)
			{
				rowCheckState = (RowCheckState)cellValue;
			}
			TriState triState = TriState.Empty;
			switch (rowCheckState)
			{
			case RowCheckState.Unchecked:
				triState = TriState.False;
				break;
			case RowCheckState.Checked:
				triState = TriState.True;
				break;
			}
			if (_drawingStyle.IsThemedLook)
			{
				GridColumnControl.DrawGradientCheckBox(graphics, rect, item._column, _drawingStyle.GetResolvedAppearance(), triState, true, item._isHot, item._isPressed, true, enabled);
			}
			else if (_drawingStyle._isThemed)
			{
				GridColumnControl.DrawClassicCheckBox(graphics, rect, item._column, triState, item._isHot, item._isPressed, true, true, enabled);
			}
			else
			{
				GridColumnControl.DrawBackground(graphics, rect, item._column, triState, true, true, enabled);
			}
		}
	}
}
