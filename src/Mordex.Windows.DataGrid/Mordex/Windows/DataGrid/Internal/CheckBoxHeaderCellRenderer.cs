using System.Drawing;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class CheckBoxHeaderCellRenderer : ColumnHeaderCellRenderer
	{
		private MxColorSchemeAppearanceState _checkStyle;

		internal CheckBoxHeaderCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle, MxColorSchemeAppearanceState checkStyle)
			: base(owner, drawingStyle)
		{
			_checkStyle = checkStyle;
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			if (!item._column.UseHeaderSelector)
			{
				base.PaintForeground(graphics, item, style, rect, selected, focused);
				return;
			}
			TriState triState = TriState.False;
			object cellValue = item.GetCellValue();
			if (cellValue is RowCheckState)
			{
				switch ((RowCheckState)cellValue)
				{
				case RowCheckState.Checked:
					triState = TriState.True;
					break;
				case RowCheckState.Mixed:
					triState = TriState.Empty;
					break;
				case RowCheckState.Unchecked:
					triState = TriState.False;
					break;
				}
			}
			if (_checkStyle.IsThemedLook)
			{
				GridColumnControl.DrawGradientCheckBox(graphics, rect, null, _drawingStyle.GetResolvedAppearance(), triState, true, item._isHot, item._isPressed, true, true);
			}
			else if (_checkStyle._isThemed)
			{
				GridColumnControl.DrawClassicCheckBox(graphics, rect, null, triState, item._isHot, item._isPressed, true, true, true);
			}
			else
			{
				GridColumnControl.DrawBackground(graphics, rect, null, triState, true, true, true);
			}
		}
	}
}
