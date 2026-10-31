using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class SpinButtonRenderer : ButtonControlRenderer
	{
		internal SpinButtonRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (_drawingStyle.IsThemedLook)
			{
				if (item._type != LayoutItemType.CellUpDownBackground && !item._isHot && !item._isPressed)
				{
					return;
				}
			}
			else if (_drawingStyle._isThemed)
			{
				switch (item._type)
				{
				case LayoutItemType.CellUpDownBackground:
					return;
				case LayoutItemType.CellDownButton:
					if (GridHelper.IsWindows7RendererEnabled())
					{
						ButtonThemeRenderer.DrawThemedBackground(graphics, rect, _owner.RightToLeft, !item.IsDisabled(), item._isPressed, item._isHot, false);
						ControlStyle.DrawVerticalTriangle(graphics, rect.Width, rect, 1, item.IsDisabled() ? SystemPens.GrayText : SystemPens.ControlText);
					}
					else
					{
						VisualStyleDrawer.DrawSpinDownButton(graphics, rect, item._isHot, item._isPressed, !item.IsDisabled());
					}
					return;
				case LayoutItemType.CellUpButton:
					if (GridHelper.IsWindows7RendererEnabled())
					{
						ButtonThemeRenderer.DrawThemedBackground(graphics, rect, _owner.RightToLeft, !item.IsDisabled(), item._isPressed, item._isHot, false);
						ControlStyle.DrawVerticalTriangle(graphics, rect.Width, rect, -1, item.IsDisabled() ? SystemPens.GrayText : SystemPens.ControlText);
					}
					else
					{
						VisualStyleDrawer.DrawSpinUpButton(graphics, rect, item._isHot, item._isPressed, !item.IsDisabled());
					}
					return;
				}
			}
			base.PaintBackground(graphics, item, style, rect, selected, focused);
		}
	}
}
