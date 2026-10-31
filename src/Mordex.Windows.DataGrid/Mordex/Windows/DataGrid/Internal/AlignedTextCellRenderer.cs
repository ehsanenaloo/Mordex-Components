using System.Drawing;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class AlignedTextCellRenderer : CellRendererBase
	{
		private TextAlignment _textAlignment;

		internal AlignedTextCellRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
			_alignsText = true;
			_textAlignment = TextAlignment.Center;
		}

		internal void SetTextAlignment(TextAlignment textAlignment)
		{
			_textAlignment = textAlignment;
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect) || (item._gridGroupLayoutStateOwner is MxRowLayoutState mordexRowLayoutState && (mordexRowLayoutState._rowType == LayoutRowType.EmptyRow || (mordexRowLayoutState._rowType == LayoutRowType.NewRow && !selected) || (mordexRowLayoutState._rowType == LayoutRowType.FilterRow && item._column != null && item._column.ActAsSelector))))
			{
				return;
			}
			object cellValue = item.GetCellValue();
			bool condition = false;
			TriState triState = TriState.Empty;
			if (item._column != null)
			{
				if (cellValue is RowCheckState)
				{
					switch ((RowCheckState)cellValue)
					{
					case RowCheckState.Checked:
						triState = TriState.True;
						break;
					case RowCheckState.Unchecked:
						triState = TriState.False;
						break;
					}
					condition = true;
				}
				else if (GridColumnControl.AreValuesEqual(item._column.CheckBoxTrueValue, cellValue))
				{
					triState = TriState.True;
				}
				else if (GridColumnControl.AreValuesEqual(item._column.CheckBoxFalseValue, cellValue))
				{
					triState = TriState.False;
				}
			}
			bool enabled = true;
			if (item._gridGroupLayoutStateOwner != null)
			{
				GridCell gridCell = null;
				if (item._gridGroupLayoutStateOwner.GridRow != null)
				{
					gridCell = item._gridGroupLayoutStateOwner.GridRow.Cells[item._column];
				}
				if (gridCell != null)
				{
					enabled = gridCell.Enabled;
					if (gridCell.GetErrorText().Length > 0)
					{
						Image image = null;
						if (gridCell.HasErrorIcon())
						{
							image = gridCell.Image;
						}
						if (image != null)
						{
							GridHelper.DrawImageAndText(graphics, null, "", image, rect, ImageHorizontalAlignment.Near, ImageVerticalAlignment.Center, null, null, item.IsRightToLeft(_owner.RightToLeft), -1, null, _owner.IsPrinting());
							if (_textAlignment == TextAlignment.Near)
							{
								rect.X += image.Width + 4;
								rect.Width -= image.Width + 4;
							}
						}
					}
				}
			}
			if (_drawingStyle.IsThemedLook)
			{
				GridColumnControl.DrawGradientCheckBox(graphics, rect, item._column, _drawingStyle.GetResolvedAppearance(), triState, _textAlignment == TextAlignment.Center, item._isHot, item._isPressed, condition, enabled);
			}
			else if (_drawingStyle._isThemed)
			{
				GridColumnControl.DrawClassicCheckBox(graphics, rect, item._column, triState, item._isHot, item._isPressed, _textAlignment == TextAlignment.Center, condition, enabled);
			}
			else
			{
				GridColumnControl.DrawBackground(graphics, rect, item._column, triState, _textAlignment == TextAlignment.Center, condition, enabled);
			}
		}
	}
}
