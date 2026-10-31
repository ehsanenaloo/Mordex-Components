using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class CardCaptionButtonRenderer : CellRendererBase
	{
		internal CardCaptionButtonRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
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
			Rectangle rectangle = new Rectangle(0, 0, rect.Height - 4, rect.Height - 4);
			rectangle.X = rect.X + (rect.Width - rectangle.Width) / 2;
			rectangle.Y = rect.Y + (rect.Height - rectangle.Height) / 2;
			Color color = style.ForeColor;
			if (!_drawingStyle.IsThemedLook)
			{
				if (_drawingStyle._isThemed)
				{
					DrawBackground(graphics, rect, selected);
					color = (selected ? ((!item._isHot) ? _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaptionText) : _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionGradient)) : ((!item._isHot) ? _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionText) : Color.FromArgb(124, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionText))));
				}
				else if (item._isHot)
				{
					CellRendererBase.RenderContent(graphics, rectangle, Appearance.RaisedLight, style.GetEffectiveBackColor());
					rectangle.Inflate(-1, -1);
				}
			}
			else if (item._isHot)
			{
				color = Color.Black;
				Brush brush = new LinearGradientBrush(Rectangle.Inflate(rectangle, 0, 1), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElement), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient), 90f);
				graphics.FillRectangle(brush, rectangle);
				brush.Dispose();
				Pen pen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementBorder));
				GridHelper.DrawFlatBorder(graphics, pen, rectangle);
			}
			else if (_drawingStyle._isThemed)
			{
				color = ((!selected) ? _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionText) : _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaptionText));
			}
			if (!RowStateHelper.GetRowTag(item._gridGroupLayoutStateOwner.GridRow.GetLogicalRow()).GetIsExpanded())
			{
				CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetChevronDownImage(), Color.Magenta), rectangle, color, false, ImageHorizontalAlignment.Center, _owner.RightToLeft);
			}
			else
			{
				CellRendererBase.DrawRecoloredImage(graphics, GridHelper.MakeBitmapTransparent(ResourceImages.GetChevronUpImage(), Color.Magenta), rectangle, color, false, ImageHorizontalAlignment.Center, _owner.RightToLeft);
			}
		}

		private void DrawBackground(Graphics graphics, Rectangle bounds, bool condition)
		{
			bounds.Inflate(-2, -2);
			bounds.Height--;
			if (bounds.Width != bounds.Height)
			{
				if (bounds.Width > bounds.Height)
				{
					bounds.Width = bounds.Height;
				}
				else
				{
					bounds.Height = bounds.Width;
				}
			}
			Brush brush;
			Pen pen;
			if (condition)
			{
				brush = GdiCache.Brush(Color.FromArgb(192, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaption)));
				pen = GdiCache.Pen(Color.FromArgb(124, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaption)));
			}
			else
			{
				brush = GdiCache.Brush(Color.FromArgb(192, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaption)));
				pen = GdiCache.Pen(Color.FromArgb(124, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionText)));
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.FillEllipse(brush, bounds);
			graphics.DrawEllipse(pen, bounds);
			graphics.SmoothingMode = SmoothingMode.Default;
		}
	}
}
