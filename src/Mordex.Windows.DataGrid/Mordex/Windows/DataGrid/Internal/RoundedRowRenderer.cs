using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class RoundedRowRenderer : CellRendererBase
	{
		internal RoundedRowRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void PaintBackground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			rect.Width--;
			if (!_drawingStyle._isThemed)
			{
				base.PaintBackground(graphics, item, style, rect, selected, focused);
				return;
			}
			Brush backBrush = GetBackBrush(rect, item, style, selected, focused);
			rect.Height--;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddPolygon(new Point[7]
			{
				new Point(rect.Left, rect.Bottom),
				new Point(rect.Left, rect.Top + 2),
				new Point(rect.Left + 2, rect.Top),
				new Point(rect.Right - 2, rect.Top),
				new Point(rect.Right, rect.Top + 2),
				new Point(rect.Right, rect.Bottom),
				new Point(rect.Left, rect.Bottom)
			});
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.FillPath(backBrush, graphicsPath);
			Pen otherPen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.Gridline));
			graphics.DrawPath(otherPen, graphicsPath);
			graphics.SmoothingMode = SmoothingMode.Default;
			backBrush.Dispose();
			graphicsPath.Dispose();
		}

		internal override Brush GetBackBrush(Rectangle rect, GridGroupLayoutState item, GridFormatStyle style, bool selected, bool focused)
		{
			Color empty = Color.Empty;
			Color gridColor = Color.Empty;
			if (_drawingStyle.IsThemedLook)
			{
				if (_drawingStyle._isThemed)
				{
					if (selected)
					{
						empty = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaption);
						gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaptionGradient);
					}
					else
					{
						empty = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaption);
						gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionGradient);
					}
				}
				else
				{
					if (selected && focused)
					{
						return base.GetBackBrush(rect, item, style, selected, focused);
					}
					empty = style.BackColor;
					gridColor = style.BackColorGradient;
					if (gridColor.IsEmpty)
					{
						gridColor = GridHelper.BlendColors(empty, Color.Black, 0.25f);
					}
				}
				rect.Inflate(1, 1);
				return new LinearGradientBrush(rect, empty, gridColor, LinearGradientMode.Horizontal);
			}
			if (_drawingStyle._isThemed)
			{
				rect.Inflate(1, 1);
				if (selected)
				{
					empty = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaption);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaptionGradient);
				}
				else
				{
					empty = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaption);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionGradient);
				}
				return new LinearGradientBrush(rect, empty, gridColor, 0f);
			}
			return base.GetBackBrush(rect, item, style, selected, focused);
		}

		internal override Brush GetForeBrush(GridFormatStyle style, GridGroupLayoutState item, bool selected, bool focused)
		{
			if (_drawingStyle._isThemed)
			{
				if (selected)
				{
					return new SolidBrush(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.SelectedCardCaptionText));
				}
				return new SolidBrush(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.CardCaptionText));
			}
			return base.GetForeBrush(style, item, selected, focused);
		}
	}
}
