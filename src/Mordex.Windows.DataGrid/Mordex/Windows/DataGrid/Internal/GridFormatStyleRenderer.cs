using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridFormatStyleRenderer : LayoutStateCellRenderer
	{
		private Image _foregroundImage;

		private Image _gridFormatStyleRendererImage;

		private HeaderCellRendererBase _foregroundRenderer;

		internal GridFormatStyle _gridFormatStyle;

		internal GridTable _sourceTable;

		internal override void OnDispose()
		{
			if (_foregroundImage != null)
			{
				_foregroundImage.Dispose();
			}
			if (_gridFormatStyleRendererImage != null)
			{
				_gridFormatStyleRendererImage.Dispose();
			}
		}

		internal GridFormatStyleRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
			if (_drawingStyle.IsThemedLook && !_owner.IsPrinting())
			{
				_foregroundImage = GroupExpanderRenderer.CreateExpanderButtonImage(_drawingStyle.GetResolvedAppearance(), false);
				_gridFormatStyleRendererImage = GroupExpanderRenderer.CreateExpanderButtonImage(_drawingStyle.GetResolvedAppearance(), true);
			}
			else if (_drawingStyle._isThemed)
			{
				_foregroundRenderer = new HeaderCellRendererBase(owner, drawingStyle);
			}
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			GridRow gridRow = item._gridGroupLayoutStateOwner.GridRow;
			MxDataGrid dataGrid = gridRow.DataGrid;
			rect.Width++;
			ExpandBoxStyle expandBoxStyle = ExpandBoxStyle.Classic;
			Image image = null;
			Image treeExpandGlyph = null;
			if (dataGrid != null)
			{
				expandBoxStyle = dataGrid.GroupExpandBoxStyle;
				if (expandBoxStyle == ExpandBoxStyle.UseDefault)
				{
					expandBoxStyle = ExpandBoxStyle.Classic;
				}
				if (expandBoxStyle == ExpandBoxStyle.UseCustomGlyph)
				{
					treeExpandGlyph = dataGrid.TreeExpandGlyph;
					image = dataGrid.TreeCollapseGlyph;
				}
			}
			if (gridRow.Children <= 0)
			{
				return;
			}
			if (expandBoxStyle == ExpandBoxStyle.UseCustomGlyph && image != null && item._gridGroupLayoutStateOwner.GridRow.Expanded)
			{
				if (rect.Width >= image.Width && rect.Height >= image.Height)
				{
					Rectangle otherRectangle = new Rectangle(rect.X + (rect.Width - image.Width) / 2, rect.Y + (rect.Height - image.Height) / 2, image.Width, image.Height);
					graphics.DrawImageUnscaled(image, otherRectangle);
				}
				else
				{
					graphics.DrawImage(image, rect);
				}
				return;
			}
			if (expandBoxStyle == ExpandBoxStyle.UseCustomGlyph && treeExpandGlyph != null && !item._gridGroupLayoutStateOwner.GridRow.Expanded)
			{
				if (rect.Width >= treeExpandGlyph.Width && rect.Height >= treeExpandGlyph.Height)
				{
					Rectangle nextRectangle = new Rectangle(rect.X + (rect.Width - treeExpandGlyph.Width) / 2, rect.Y + (rect.Height - treeExpandGlyph.Height) / 2, treeExpandGlyph.Width, treeExpandGlyph.Height);
					graphics.DrawImageUnscaled(treeExpandGlyph, nextRectangle);
				}
				else
				{
					graphics.DrawImage(treeExpandGlyph, rect);
				}
				return;
			}
			if (expandBoxStyle == ExpandBoxStyle.Arrows)
			{
				rect.X += 4;
				GroupExpanderRenderer.DrawExpandGlyph(graphics, rect, item.GetMordexRow().Expanded, item._isHot);
				return;
			}
			if (_drawingStyle.IsThemedLook && !_owner.IsPrinting())
			{
				if (item.GetMordexRow().Expanded)
				{
					if (_gridFormatStyleRendererImage != null)
					{
						graphics.DrawImageUnscaled(_gridFormatStyleRendererImage, rect.X + (rect.Width - _gridFormatStyleRendererImage.Width) / 2, rect.Y + (rect.Height - _gridFormatStyleRendererImage.Height) / 2);
					}
				}
				else if (_foregroundImage != null)
				{
					graphics.DrawImageUnscaled(_foregroundImage, rect.X + (rect.Width - _foregroundImage.Width) / 2, rect.Y + (rect.Height - _foregroundImage.Height) / 2);
				}
				return;
			}
			bool expanded = item.GetMordexRow().Expanded;
			Rectangle rectangle = new Rectangle(0, 0, 14, 14);
			rectangle.X = rect.X + (rect.Width - rectangle.Width) / 2;
			rectangle.Y = rect.Y + (rect.Height - rectangle.Height) / 2;
			rect = rectangle;
			if (_drawingStyle._isThemed)
			{
				_foregroundRenderer.PaintBackground(graphics, item, style, rect, selected: false, focused: false);
				_foregroundRenderer.PaintAddornments(graphics, item, style, rect);
			}
			else
			{
				Color color = Color.Empty;
				if (_gridFormatStyle != null)
				{
					color = _gridFormatStyle.GetEffectiveBackColor();
				}
				if (color.IsEmpty)
				{
					color = SystemColors.Control;
				}
				Brush brush = GdiCache.Brush(color);
				graphics.FillRectangle(brush, rect);
				if (_owner.IsPrinting())
				{
					Pen pen = GdiCache.Pen(GridHelper.BlendColors(style.GetEffectiveBackColor(), Color.Black, 0.5f));
					GridHelper.DrawFlatBorder(graphics, pen, rect);
					if (_owner.PrintDocument.PrintCollapsedRows)
					{
						expanded = true;
					}
				}
				else
				{
					CellRendererBase.RenderContent(graphics, rect, Appearance.RaisedLight, color);
				}
			}
			rect.Inflate(-4, -4);
			rect.Width -= 2;
			rect.Height -= 2;
			graphics.DrawLine(Pens.Black, rect.X, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2);
			if (!expanded)
			{
				graphics.DrawLine(Pens.Black, rect.X + rect.Width / 2, rect.Y, rect.X + rect.Width / 2, rect.Bottom);
			}
		}
	}
}
