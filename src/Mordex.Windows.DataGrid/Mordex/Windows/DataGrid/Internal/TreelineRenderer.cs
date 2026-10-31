using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class TreelineRenderer : CellRendererBase
	{
		internal Color _treelineColor;

		internal bool _treelines;

		private Pen _hatchPen;

		internal TreelineRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle, Color treelineColor, bool treelines)
			: base(owner, drawingStyle)
		{
			if (drawingStyle._isThemed)
			{
				_treelineColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.Gridline);
			}
			else
			{
				_treelineColor = treelineColor;
			}
			_treelines = treelines;
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
		}

		internal Pen GetHatchPen()
		{
			if (_hatchPen == null)
			{
				Brush brush = new HatchBrush(HatchStyle.Percent50, _treelineColor, Color.Transparent);
				_hatchPen = new Pen(brush);
				brush.Dispose();
			}
			return _hatchPen;
		}

		internal void DrawTreelines(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, Rectangle rectangle, Rectangle otherRectangle, ExpandBoxStyle expandBoxStyle)
		{
			if (gridGroupLayoutState._type == LayoutItemType.SelfReferencingExpandBox || expandBoxStyle == ExpandBoxStyle.Arrows)
			{
				return;
			}
			if (otherRectangle.IsEmpty)
			{
				otherRectangle = new Rectangle(0, 0, 8, 8);
				otherRectangle.X = rectangle.X + (rectangle.Width - otherRectangle.Width) / 2;
				otherRectangle.Y = rectangle.Y + (rectangle.Height - otherRectangle.Height) / 2;
				otherRectangle.X += otherRectangle.Width / 2;
				otherRectangle.Y += otherRectangle.Height / 2;
				otherRectangle.Width = 0;
				otherRectangle.Height = 0;
			}
			if (!_treelines)
			{
				return;
			}
			Pen pen = GetHatchPen();
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorLeft) != BorderFlags.None)
			{
				if (!gridGroupLayoutState.IsRightToLeft(_owner.RightToLeft))
				{
					graphics.DrawLine(pen, rectangle.Left, otherRectangle.Top + otherRectangle.Height / 2, otherRectangle.Left - 1, otherRectangle.Top + otherRectangle.Height / 2);
				}
				else
				{
					graphics.DrawLine(pen, otherRectangle.Right, otherRectangle.Top + otherRectangle.Height / 2, rectangle.Right, otherRectangle.Top + otherRectangle.Height / 2);
				}
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorTop) != BorderFlags.None)
			{
				graphics.DrawLine(pen, otherRectangle.Left + otherRectangle.Width / 2, rectangle.Top, otherRectangle.Left + otherRectangle.Width / 2, otherRectangle.Top - 1);
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorRight) != BorderFlags.None)
			{
				if (!gridGroupLayoutState.IsRightToLeft(_owner.RightToLeft))
				{
					graphics.DrawLine(pen, otherRectangle.Right, otherRectangle.Top + otherRectangle.Height / 2, rectangle.Right, otherRectangle.Top + otherRectangle.Height / 2);
				}
				else
				{
					graphics.DrawLine(pen, rectangle.Left, otherRectangle.Top + otherRectangle.Height / 2, otherRectangle.Left - 1, otherRectangle.Top + otherRectangle.Height / 2);
				}
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorBottom) != BorderFlags.None)
			{
				graphics.DrawLine(pen, otherRectangle.Left + otherRectangle.Width / 2, otherRectangle.Bottom, otherRectangle.Left + otherRectangle.Width / 2, rectangle.Bottom);
			}
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			MxDataGrid dataGrid = null;
			if (item.GetOwnerTable() != null)
			{
				dataGrid = item.GetOwnerTable().DataGrid;
			}
			else if (item._gridGroupLayoutStateOwner != null && item._gridGroupLayoutStateOwner.GridRow != null)
			{
				dataGrid = item._gridGroupLayoutStateOwner.GridRow.DataGrid;
			}
			ExpandBoxStyle expandBoxStyle = ExpandBoxStyle.Classic;
			if (dataGrid != null)
			{
				expandBoxStyle = dataGrid.HierarchicalExpandBoxStyle;
				if (expandBoxStyle == ExpandBoxStyle.UseDefault)
				{
					expandBoxStyle = ExpandBoxStyle.Classic;
				}
			}
			DrawTreelines(graphics, item, rect, Rectangle.Empty, expandBoxStyle);
		}
	}
}
