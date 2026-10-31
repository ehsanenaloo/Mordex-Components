using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabContentRenderer
	{
		public void RenderContent(Graphics graphics, Rectangle rectangle)
		{
			if (!_areGroupsStale && (rectangle.Width == 0 || rectangle.Height == 0) && _ribbon.QuickAccessPosition == QuickAccessPosition.AboveRibbon)
			{
				_renderHost.ParentControl.Controls.Clear();
				_areGroupsStale = true;
				return;
			}
			if (_areGroupsStale)
			{
				RebuildGroupRenderers();
			}
			_bounds = rectangle;
			_groupsBounds = new Rectangle(rectangle.X + _ribbon.GetGroupsAreaPadding().Left, rectangle.Y + _ribbon.GetGroupsAreaPadding().Top, rectangle.Width - _ribbon.GetGroupsAreaPadding().Right - _ribbon.GetGroupsAreaPadding().Left, rectangle.Height - _ribbon.GetGroupsAreaPadding().Top - _ribbon.GetGroupsAreaPadding().Bottom);
			_groupsBounds.Height -= _ribbon.GetContentBorderPadding().Bottom;
			if (_groupsBounds.Height > 0)
			{
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
				_totalGroupsWidth = 0;
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					item.CalculateFullItemWidth(graphics, textLayoutRenderer, _ribbon.Font);
					_totalGroupsWidth += item._fullItemWidth;
				}
				_totalGroupsWidth += (_groupRenderers.Count - 1) * 2;
				LayoutGroupsAtOffset(graphics, textLayoutRenderer, GetScrollOffset());
			}
		}

		public void DrawContent(Graphics graphics, Rectangle clipRectangle)
		{
			if (_bounds.Width <= 0 || _bounds.Height <= 0)
			{
				return;
			}
			DrawContentFrame(graphics)?.Dispose();
			Rectangle rect = _bounds;
			rect.Inflate(-1, -1);
			using (Region clip = graphics.Clip)
			{
				try
				{
					using (Region region = new Region(rect))
					{
						graphics.Clip = region;
					}
					if (_groupRenderers != null)
					{
						using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
						textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
						Color foreColor = _ribbon.ForeColorDark;
						foreach (RibbonGroupBoxRenderer item in _groupRenderers)
						{
							Rectangle rectangle = Rectangle.Intersect(item._bounds, clipRectangle);
							if (rectangle.Width > 0 && rectangle.Height > 0)
							{
								item.Paint(graphics, textLayoutRenderer, _ribbon._colorScheme, _ribbon.Font, foreColor, clipRectangle);
							}
						}
					}
				}
				finally
				{
					graphics.Clip = clip;
				}
			}
			if (_leftScrollButton != null)
			{
				DrawScrollButton2010(graphics, _ribbon._colorScheme, _leftScrollButton, true);
			}
			if (_rightScrollButton != null)
			{
				DrawScrollButton2010(graphics, _ribbon._colorScheme, _rightScrollButton, false);
			}
		}

		private void DrawScrollButton2010(Graphics graphics, RibbonColorScheme colorScheme, RibbonLayoutItem button, bool isLeftButton)
		{
			ThemedRibbonPainter.ScrollButton(graphics, colorScheme.Theme, button._bounds, button.Hot, button.Pushed, button._isEnabled, false);
			RibbonChromePainter.DrawHorizontalArrow(graphics, Rectangle.Inflate(button._bounds, -1, -1), isLeftButton ? -1 : 1, button._isEnabled, colorScheme._darkLine, colorScheme);
			return;
		}

		private void DrawBackground(Graphics graphics, RibbonColorScheme colorScheme, RibbonLayoutItem button)
		{
			Rectangle bounds = button._bounds;
			bounds.Height--;
			bounds.Width--;
			bounds.X++;
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(bounds.Right - 1, bounds.Y, bounds.X + 3, bounds.Y);
			graphicsPath.AddArc(bounds.X, bounds.Y, 6, 6, -180f, 90f);
			graphicsPath.AddLine(bounds.X, bounds.Y + 3, bounds.X, bounds.Bottom - 4);
			graphicsPath.AddArc(bounds.X, bounds.Bottom - 7, 6, 6, 180f, -90f);
			graphicsPath.AddLine(bounds.X + 3, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphicsPath.AddLine(bounds.Right - 1, bounds.Bottom - 1, bounds.Right - 1, bounds.Y);
			using Region clip = graphics.Clip;
			Region region = (graphics.Clip = new Region(graphicsPath));
			Rectangle rect = bounds;
			rect.Height = (int)((double)bounds.Height * 0.2);
			Rectangle lowerBodyBounds = new Rectangle(bounds.X, rect.Bottom, bounds.Width, bounds.Height - rect.Height);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			if (button.Pushed && button.Hot)
			{
				RibbonChromePainter.DrawBackground(graphics, bounds, colorScheme, true);
			}
			else if (button.Hot)
			{
				RibbonChromePainter.DrawHotButtonBackground(graphics, bounds, colorScheme, true);
			}
			else
			{
				GdiCache.FillGradient(graphics, rect, colorScheme._scrollButtonTopStart, colorScheme._scrollButtonTopEnd, LinearGradientMode.Vertical);
				lowerBodyBounds.Y--;
				using (Brush lowerBodyBrush = new LinearGradientBrush(lowerBodyBounds, colorScheme._scrollButtonBottomStart, colorScheme._scrollButtonBottomEnd, LinearGradientMode.Vertical))
				{
					lowerBodyBounds.Y++;
					graphics.FillRectangle(lowerBodyBrush, lowerBodyBounds);
				}
			}
			graphics.Clip = clip;
			region.Dispose();
			Rectangle outlineBounds = bounds;
			outlineBounds.Height = _ribbon.GetContentAreaBounds().Height;
			if (outlineBounds.Height == 0)
			{
				outlineBounds.Height = 69;
			}
			Brush innerHighlightBrush = new LinearGradientBrush(bounds, Color.FromArgb(10, Color.White), colorScheme._innerBorderHighlight, LinearGradientMode.Vertical);
			Pen pen = new Pen(innerHighlightBrush);
			graphics.DrawLine(pen, bounds.X + 3, bounds.Y + 1, bounds.Right - 1, bounds.Y + 1);
			graphics.DrawLine(pen, bounds.X + 1, bounds.Y + 3, bounds.X + 1, bounds.Bottom - 3);
			graphics.DrawLine(pen, bounds.X + 3, bounds.Bottom - 2, bounds.Right - 1, bounds.Bottom - 2);
			if (!button.Hot)
			{
				graphics.DrawLine(Pens.White, bounds.Right - 2, bounds.Bottom - 2, bounds.Right - 2, bounds.Y + 1);
			}
			Brush outlineBrush = new LinearGradientBrush(outlineBounds, colorScheme._scrollButtonOutlineStart, colorScheme._scrollButtonOutlineEnd, LinearGradientMode.Vertical);
			Pen outlinePen = new Pen(outlineBrush);
			graphics.DrawLine(outlinePen, bounds.X + 3, bounds.Y, bounds.Right - 1, bounds.Y);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawArc(outlinePen, bounds.X, bounds.Y, 6, 6, -180, 90);
			graphics.DrawLine(outlinePen, bounds.X, bounds.Y + 2, bounds.X, bounds.Bottom - 4);
			graphics.DrawArc(outlinePen, bounds.X, bounds.Bottom - 7, 6, 6, 180, -90);
			graphics.SmoothingMode = smoothingMode;
			Brush bottomBorderBrush = new LinearGradientBrush(bounds, colorScheme._groupBorderTop, colorScheme._scrollButtonOutlineEnd, LinearGradientMode.Vertical);
			Pen bottomBorderPen = new Pen(bottomBorderBrush);
			graphics.DrawLine(bottomBorderPen, bounds.X + 2, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(bottomBorderPen, bounds.Right - 1, bounds.Bottom - 2, bounds.Right - 1, bounds.Y + 1);
			Pen shadowPen = GdiCache.Pen(Color.FromArgb(130, colorScheme._scrollButtonOutlineEnd));
			graphics.DrawLine(shadowPen, bounds.X + 4, bounds.Bottom, bounds.Right - 1, bounds.Bottom);
			graphics.DrawArc(shadowPen, bounds.X, bounds.Bottom - 6, 6, 6, 180, -90);
			shadowPen = GdiCache.Pen(Color.FromArgb(50, colorScheme._scrollButtonOutlineEnd));
			graphics.DrawLine(shadowPen, bounds.X + 3, bounds.Bottom + 1, bounds.Right - 1, bounds.Bottom + 1);
			graphics.DrawArc(shadowPen, bounds.X - 1, bounds.Bottom - 6, 6, 6, 180, -90);
			Brush shadowBrush = new LinearGradientBrush(new Point(bounds.X - 1, bounds.Y + 4), new Point(bounds.X - 1, bounds.Bottom - 3), Color.FromArgb(50, colorScheme._scrollButtonOutlineEnd), Color.FromArgb(80, colorScheme._scrollButtonOutlineEnd));
			shadowPen = new Pen(shadowBrush);
			graphics.DrawLine(shadowPen, bounds.X - 1, bounds.Y + 4, bounds.X - 1, bounds.Bottom - 4);
			graphics.SmoothingMode = smoothingMode;
			Rectangle arrowBounds = new Rectangle(button._bounds.X + (button._bounds.Width - 3) / 2, button._bounds.Y + (button._bounds.Height - 5) / 2, 3, 5);
			Color arrowEndColor = colorScheme._scrollButtonArrowEnd;
			Color arrowStartColor = colorScheme._scrollButtonArrowStart;
			arrowBounds.X--;
			Brush arrowBrush = new LinearGradientBrush(arrowBounds, arrowEndColor, arrowStartColor, LinearGradientMode.Horizontal);
			arrowBounds.X++;
			Pen arrowPen = new Pen(arrowBrush);
			graphics.DrawLine(arrowPen, arrowBounds.X + 2, arrowBounds.Y, arrowBounds.X + 2, arrowBounds.Y + 4);
			graphics.DrawLine(arrowPen, arrowBounds.X + 1, arrowBounds.Y + 1, arrowBounds.X + 1, arrowBounds.Y + 3);
			graphics.DrawLine(arrowPen, arrowBounds.X, arrowBounds.Y + 2, arrowBounds.X, (float)arrowBounds.Y + 2.5f);
			shadowPen.Dispose();
			shadowBrush.Dispose();
			arrowBrush.Dispose();
			arrowPen.Dispose();
			pen.Dispose();
			outlinePen.Dispose();
			bottomBorderPen.Dispose();
			innerHighlightBrush.Dispose();
			outlineBrush.Dispose();
			bottomBorderBrush.Dispose();
		}

		private GraphicsPath DrawContentFrame2010(Graphics graphics)
		{
			Rectangle selectedTabBounds = Rectangle.Empty;
			if (!_ribbon.Minimized)
			{
				RibbonTabRenderer selectedRenderer = (!_ribbon.IsBackstageOpen()) ? _ribbon._tabStripRenderer.FindTabRenderer(_ribbon.SelectedTab) : _ribbon._tabStripRenderer.GetTabRenderers()[0];
				if (selectedRenderer != null)
				{
					selectedTabBounds = _ribbon._tabStripRenderer.GetVisibleTabBounds(selectedRenderer);
				}
			}
			Color lineColor = _ribbon.IsContextualTabSelected() ? ((RibbonContextualTab)_ribbon.SelectedTab.GetTabOwner()).BackColor : Color.Empty;
			ThemedRibbonPainter.ContentFrame(graphics, _ribbon._colorScheme.Theme, _bounds, _ribbon.GetContentBorderPadding().Bottom, selectedTabBounds, lineColor);
			return null;
		}

		private GraphicsPath DrawContentFrame(Graphics graphics)
		{
			return DrawContentFrame2010(graphics);
		}

	}
}
