using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms.VisualStyles;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GroupExpanderRenderer : TreelineRenderer
	{
		private Image _gridRenderer15Image;

		private Image _expandedImage;

		internal override void OnDispose()
		{
			if (_gridRenderer15Image != null)
			{
				_gridRenderer15Image.Dispose();
			}
			if (_expandedImage != null)
			{
				_expandedImage.Dispose();
			}
		}

		internal GroupExpanderRenderer(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle, Color treelineColor, bool treelines)
			: base(owner, drawingStyle, treelineColor, treelines)
		{
			if (_drawingStyle._isThemed || _drawingStyle.IsThemedLook)
			{
				_gridRenderer15Image = CreateExpanderButtonImage(_drawingStyle.GetResolvedAppearance(), false);
				_expandedImage = CreateExpanderButtonImage(_drawingStyle.GetResolvedAppearance(), true);
			}
		}

		internal static void DrawExpandGlyph(Graphics graphics, Rectangle otherRectangle, bool condition, bool otherCondition)
		{
			if (otherCondition)
			{
				if (!condition)
				{
					GridRenderer.DrawHotCollapsedGlyph(graphics, otherRectangle);
				}
				else
				{
					GridRenderer.DrawHotExpandedGlyph(graphics, otherRectangle);
				}
				return;
			}
			Rectangle rectangle = new Rectangle(0, 0, 8, 8);
			rectangle.X = otherRectangle.X + (otherRectangle.Width - rectangle.Width) / 2;
			rectangle.Y = otherRectangle.Y + (otherRectangle.Height - rectangle.Height) / 2 - 1;
			otherRectangle = rectangle;
			GraphicsPath graphicsPath = new GraphicsPath();
			if (!condition)
			{
				graphicsPath.StartFigure();
				graphicsPath.AddLine(new Point(otherRectangle.X, otherRectangle.Y), new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Y + otherRectangle.Height / 2));
				graphicsPath.AddLine(new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Y + otherRectangle.Height / 2), new Point(otherRectangle.X, otherRectangle.Bottom));
				graphicsPath.AddLine(new Point(otherRectangle.X, otherRectangle.Bottom), new Point(otherRectangle.X, otherRectangle.Y));
				graphicsPath.CloseFigure();
				SmoothingMode smoothingMode = graphics.SmoothingMode;
				if (otherCondition)
				{
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					Pen pen = GdiCache.Pen(Color.Orange, 1.5f);
					graphics.DrawPath(pen, graphicsPath);
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
				}
				else
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.DrawPath(Pens.Gray, graphicsPath);
				}
				graphics.SmoothingMode = smoothingMode;
			}
			else
			{
				otherRectangle.Y--;
				graphicsPath.StartFigure();
				graphicsPath.AddLine(new Point(otherRectangle.X, otherRectangle.Bottom), new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Bottom));
				graphicsPath.AddLine(new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Bottom), new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Y + otherRectangle.Height / 2 - 1));
				graphicsPath.AddLine(new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Y + otherRectangle.Height / 2 - 1), new Point(otherRectangle.X, otherRectangle.Bottom));
				graphicsPath.CloseFigure();
				SmoothingMode otherSmoothingMode = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Point[] points = new Point[3]
				{
					new Point(otherRectangle.X - 1, otherRectangle.Bottom),
					new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Bottom),
					new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Y + otherRectangle.Height / 2 - 2)
				};
				Brush brush = GdiCache.Brush(Color.FromArgb(80, 80, 80));
				graphics.FillPolygon(brush, points);
				brush = null;
				points = new Point[6]
				{
					new Point(otherRectangle.X + 1, otherRectangle.Bottom),
					new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Bottom),
					new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Bottom - 1),
					new Point(otherRectangle.X + otherRectangle.Width / 2 + 1, otherRectangle.Y + otherRectangle.Height / 2),
					new Point(otherRectangle.X + otherRectangle.Width / 2, otherRectangle.Y + otherRectangle.Height / 2),
					new Point(otherRectangle.X + 1, otherRectangle.Bottom - 1)
				};
				graphics.DrawPolygon(Pens.Black, points);
				graphics.SmoothingMode = otherSmoothingMode;
			}
		}

		internal static Image CreateExpanderButtonImage(GridColorTable colorSchemeAppearanceState, bool condition)
		{
			Bitmap bitmap = new Bitmap(14, 14);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			graphics.Clear(Color.Transparent);
			Rectangle rectangle = new Rectangle(1, 1, 11, 11);
			if (colorSchemeAppearanceState.IsThemedLook)
			{
				graphics.FillRectangle(GdiCache.Brush(colorSchemeAppearanceState.GetGridColor(GridColorKey.WindowColor)), rectangle);
				GridHelper.DrawFlatBorder(graphics, GdiCache.Pen(colorSchemeAppearanceState.GetGridColor(GridColorKey.ControlDark)), rectangle);
				Pen glyphPen = GdiCache.Pen(colorSchemeAppearanceState.GetGridColor(GridColorKey.WindowTextColor));
				graphics.DrawLine(glyphPen, 3, 6, 9, 6);
				if (!condition)
				{
					graphics.DrawLine(glyphPen, 6, 3, 6, 9);
				}
				graphics.Dispose();
				return bitmap;
			}
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangle, Color.White, colorSchemeAppearanceState.GetGridColor(GridColorKey.HeaderBorder), LinearGradientMode.ForwardDiagonal);
			Blend blend = new Blend(3);
			blend.Positions = new float[3] { 0f, 0.3f, 1f };
			blend.Factors = new float[3] { 0f, 0.1f, 1f };
			linearGradientBrush.Blend = blend;
			graphics.FillRectangle(linearGradientBrush, rectangle);
			linearGradientBrush.Dispose();
			Pen pen = new Pen(GridHelper.BlendColors(colorSchemeAppearanceState.GetGridColor(GridColorKey.Header), Color.White, 0.5f));
			GridHelper.DrawFlatBorder(graphics, pen, rectangle);
			pen.Dispose();
			pen = new Pen(GridHelper.BlendColors(colorSchemeAppearanceState.GetGridColor(GridColorKey.ControlBorder), Color.White, 0.75f));
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.X, rectangle.Bottom - 2);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
			pen.Dispose();
			pen = new Pen(GridHelper.BlendColors(colorSchemeAppearanceState.GetGridColor(GridColorKey.ControlBorder), Color.Black, 0.75f));
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Y + 1, rectangle.Right - 1, rectangle.Bottom - 2);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
			rectangle.Inflate(-3, -3);
			rectangle.Width--;
			rectangle.Height--;
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + rectangle.Height / 2, rectangle.Right, rectangle.Y + rectangle.Height / 2);
			if (!condition)
			{
				graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2, rectangle.Y, rectangle.X + rectangle.Width / 2, rectangle.Bottom);
			}
			pen.Dispose();
			graphics.Dispose();
			return bitmap;
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			if (IsEmptyBounds(rect))
			{
				return;
			}
			MxDataGrid dataGrid = item._gridGroupLayoutStateOwner.GridRow.DataGrid;
			ExpandBoxStyle expandBoxStyle = ExpandBoxStyle.Classic;
			Image image = null;
			Image treeExpandGlyph = null;
			if (dataGrid != null)
			{
				expandBoxStyle = dataGrid.HierarchicalExpandBoxStyle;
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
			DrawTreelines(graphics, item, rect, Rectangle.Empty, expandBoxStyle);
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
				DrawExpandGlyph(graphics, rect, item._gridGroupLayoutStateOwner.GridRow.Expanded, item._isHot);
				return;
			}
			if (_drawingStyle.IsThemedLook || _drawingStyle._isThemed)
			{
				Image gridRenderer15Image = null;
				gridRenderer15Image = ((!item._gridGroupLayoutStateOwner.GridRow.Expanded) ? _gridRenderer15Image : _expandedImage);
				if (gridRenderer15Image != null)
				{
					Rectangle previousRectangle = new Rectangle(rect.X + (rect.Width - gridRenderer15Image.Width) / 2, rect.Y + (rect.Height - gridRenderer15Image.Height) / 2, gridRenderer15Image.Width, gridRenderer15Image.Height);
					graphics.DrawImageUnscaled(gridRenderer15Image, previousRectangle);
				}
				return;
			}
			if (_drawingStyle._isThemed)
			{
				DrawThemedTreeGlyph(graphics, rect, item._gridGroupLayoutStateOwner.GridRow.Expanded);
				return;
			}
			bool expanded = item._gridGroupLayoutStateOwner.GridRow.Expanded;
			if (_owner.IsPrinting() && _owner.PrintDocument.PrintCollapsedRows)
			{
				expanded = true;
			}
			Rectangle rectangle = new Rectangle(0, 0, 9, 9);
			rectangle.X = rect.X + (rect.Width - rectangle.Width) / 2 + 1;
			rectangle.Y = rect.Y + (rect.Height - rectangle.Height) / 2 + 1;
			rect = rectangle;
			graphics.FillRectangle(SystemBrushes.Window, rect);
			Pen pen = GdiCache.Pen(_treelineColor);
			GridHelper.DrawFlatBorder(graphics, pen, rect);
			rect.Inflate(-2, -2);
			rect.Width--;
			rect.Height--;
			graphics.DrawLine(SystemPens.WindowText, rect.X, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2);
			if (!expanded)
			{
				graphics.DrawLine(SystemPens.WindowText, rect.X + rect.Width / 2, rect.Y, rect.X + rect.Width / 2, rect.Bottom);
			}
		}

		private void DrawThemedTreeGlyph(Graphics dc, Rectangle bounds, bool condition)
		{
			if (condition)
			{
				if (!VisualStyleDrawer.TrySetElement(VisualStyleElement.TreeView.Glyph.Opened))
				{
					return;
				}
			}
			else if (!VisualStyleDrawer.TrySetElement(VisualStyleElement.TreeView.Glyph.Closed))
			{
				return;
			}
			VisualStyleDrawer._visualStyleRenderer.DrawBackground(dc, bounds);
		}
	}
}
