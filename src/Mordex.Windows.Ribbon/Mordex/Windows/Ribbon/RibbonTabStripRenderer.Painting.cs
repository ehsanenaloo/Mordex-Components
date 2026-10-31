using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabStripRenderer
	{
		public void DrawTabStrip(Graphics graphics, Rectangle clipRect)
		{
			if (_stripBounds.IsEmpty)
			{
				if (!IsInGlassCaption() || !_ribbon._isTabStripLayoutDirty)
				{
					return;
				}
				_ribbon.RenderContent(graphics, false);
				if (_stripBounds.IsEmpty)
				{
					return;
				}
			}
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			if (!IsInGlassCaption())
			{
				Brush brush = GdiCache.Brush(_ribbon.GetHeaderBackColor());
				graphics.FillRectangle(brush, _stripBounds);
				if (_ribbon.Dock == DockStyle.Top && _ribbon.IsParentedToForm() && _ribbon.HandlesTitleBar && !_ribbon.IsCaptionBarShown())
				{
					Pen pen = GdiCache.Pen(_ribbon._colorScheme._tabStripTopLine);
					graphics.DrawLine(pen, _stripBounds.X, _stripBounds.Y, _stripBounds.Right, _stripBounds.Y);
				}
			}
			else if (_ribbon._controlBoxButtonRenderer != null)
			{
				_ribbon._controlBoxButtonRenderer.Paint(graphics, textLayoutRenderer, _ribbon._colorScheme, _ribbon.Font, _ribbon.ForeColorDark, Rectangle.Empty);
			}
			if (_minimizeButton._isVisible)
			{
				DrawMinimizeButton(graphics, _ribbon._colorScheme);
			}
			if (_helpButton._isVisible)
			{
				RenderContent(graphics, _ribbon._colorScheme);
			}
			if (_leftScrollButton._isVisible)
			{
				DrawScrollButton2010(graphics, _ribbon._colorScheme, _leftScrollButton, true);
			}
			if (_rightScrollButton._isVisible)
			{
				DrawScrollButton2010(graphics, _ribbon._colorScheme, _rightScrollButton, false);
			}
			Region region = null;
			using Region clip = graphics.Clip;
			if (!_tabsClipBounds.IsEmpty)
			{
				region = (graphics.Clip = new Region(_tabsClipBounds));
			}
			Pen separatorPen = null;
			Brush separatorBrush = null;
			if (_ribbon._headerRenderer != null && _ribbon._headerRenderer._contextualTabRenderers != null && _ribbon._headerRenderer._contextualTabRenderers.Count > 0)
			{
				DrawContextualTabBackgrounds(graphics);
			}
			for (int i = 0; i < _tabRenderers.Count; i++)
			{
				RibbonTabRenderer thisValue = _tabRenderers[i];
				if (!_tabsClipBounds.IsEmpty && thisValue is BackstageMenuTabRenderer)
				{
					graphics.Clip = clip;
				}
				Color foreColor = _ribbon.ForeColorDark;
				if (!thisValue.Selected)
				{
					foreColor = _ribbon.ForeColorLight;
				}
				thisValue.Paint(graphics, textLayoutRenderer, _ribbon._colorScheme, _ribbon.Font, foreColor, clipRect);
				if (_areTabsCompressed && i < _tabRenderers.Count - 1)
				{
					ThemedRibbonPainter.TabSeparator(graphics, _ribbon._colorScheme.Theme, thisValue._bounds);
				}
				if (!_tabsClipBounds.IsEmpty && thisValue is BackstageMenuTabRenderer)
				{
					graphics.Clip = region;
				}
			}
			separatorPen?.Dispose();
			separatorBrush?.Dispose();
			if (region != null)
			{
				graphics.Clip = clip;
				region.Dispose();
			}
			if (_mdiControlBox != null)
			{
				_mdiControlBox.PaintOffice2010Buttons(graphics);
			}
		}

		private void DrawContextualTabBackgrounds(Graphics graphics)
		{
			foreach (RibbonContextualTabRenderer item in _ribbon._headerRenderer._contextualTabRenderers)
			{
				Rectangle rect = new Rectangle(item._bounds.X, _stripBounds.Y, item._bounds.Width, _stripBounds.Height + 2);
				if (rect.Width <= 0 || rect.Height <= 0 || !item._isVisible)
				{
					continue;
				}
				Rectangle bounds = item._bounds;
				bounds.Y = _stripBounds.Y;
				bounds.Height = _stripBounds.Height;
				DrawContextualTabBackground2010(graphics, bounds, item, _ribbon._colorScheme);
			}
		}

		private void DrawContextualTabBackground2010(Graphics graphics, Rectangle rectangle, RibbonContextualTabRenderer ribbonContextualTabRenderer, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.ContextualBand(graphics, colorScheme.Theme, rectangle, ribbonContextualTabRenderer.GetContextualTab().BackColor, false);
			return;
		}

		private void RenderContent(Graphics graphics, RibbonColorScheme colorScheme)
		{
			if (_helpButton._isEnabled)
			{
				if (IsHelpButtonPressed() && IsHelpButtonHot())
				{
					RibbonChromePainter.DrawBackground(graphics, _helpButton._bounds, colorScheme, true);
				}
				else if (IsHelpButtonHot())
				{
					RibbonChromePainter.DrawHotButtonBackground(graphics, _helpButton._bounds, colorScheme, true);
				}
			}
			Image image = _ribbon.HelpButton.Image;
			RibbonDrawingUtils.HasButtonImage(_ribbon.HelpButton, CommandSizeStyle.Small, _ribbon.ImageList, _ribbon.LargeImageList);
			if (image != null)
			{
				int imageWidth = 16;
				int imageHeight = 16;
				CompositingQuality compositingQuality = graphics.CompositingQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				Rectangle rectangle = new Rectangle(_helpButton._bounds.X + (_helpButton._bounds.Width - imageWidth) / 2, _helpButton._bounds.Y + (_helpButton._bounds.Height - imageHeight) / 2, imageWidth, imageHeight);
				RibbonDrawingUtils.DrawButtonImage(graphics, _ribbon.HelpButton, _ribbon.LargeImageList, _ribbon.ImageList, rectangle, CommandSizeStyle.Small, IsInGlassCaption(), colorScheme);
				graphics.CompositingQuality = compositingQuality;
			}
		}

		private void DrawMinimizeButton(Graphics graphics, RibbonColorScheme colorScheme)
		{
			if (!_minimizeButton._isEnabled)
			{
				return;
			}
			if (IsMinimizeButtonPressed() && IsMinimizeButtonHot())
			{
				RibbonChromePainter.DrawBackground(graphics, _minimizeButton._bounds, colorScheme, true);
			}
			else if (IsMinimizeButtonHot())
			{
				RibbonChromePainter.DrawHotButtonBackground(graphics, _minimizeButton._bounds, colorScheme, true);
			}
			ThemedRibbonPainter.Chevron(graphics, colorScheme.Theme, _minimizeButton._bounds, !_ribbon.Minimized, true);
			return;
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
			if (bounds.Height <= 0 || bounds.Width <= 0)
			{
				return;
			}
			bounds.Height--;
			bounds.Width--;
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(bounds.X, bounds.Y, bounds.Right - 4, bounds.Y);
			graphicsPath.AddArc(bounds.Right - 7, bounds.Y, 6, 6, -90f, 90f);
			graphicsPath.AddLine(bounds.Right - 1, bounds.Y + 4, bounds.Right - 1, bounds.Bottom - 4);
			graphicsPath.AddArc(bounds.Right - 7, bounds.Bottom - 7, 6, 6, 90f, -90f);
			graphicsPath.AddLine(bounds.Right - 4, bounds.Bottom - 1, bounds.X, bounds.Bottom - 1);
			graphicsPath.AddLine(bounds.X, bounds.Bottom - 1, bounds.X, bounds.Y);
			using Region clip = graphics.Clip;
			Region region = (graphics.Clip = new Region(graphicsPath));
			Rectangle rect = bounds;
			rect.Height = (int)((double)bounds.Height * 0.35);
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
				GdiCache.FillGradient(graphics, lowerBodyBounds, colorScheme._scrollButtonBottomStart, colorScheme._scrollButtonBottomEnd, LinearGradientMode.Vertical);
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
			graphics.DrawLine(pen, bounds.X + 1, bounds.Y + 1, bounds.Right - 3, bounds.Y + 1);
			graphics.DrawLine(pen, bounds.Right - 2, bounds.Y + 3, bounds.Right - 2, bounds.Bottom - 3);
			graphics.DrawLine(pen, bounds.Right - 3, bounds.Bottom - 2, bounds.X + 1, bounds.Bottom - 2);
			if (!button.Hot)
			{
				graphics.DrawLine(Pens.White, bounds.X + 1, bounds.Bottom - 2, bounds.X + 1, bounds.Y + 1);
			}
			Brush outlineBrush = new LinearGradientBrush(outlineBounds, colorScheme._scrollButtonOutlineStart, colorScheme._scrollButtonOutlineEnd, LinearGradientMode.Vertical);
			Pen outlinePen = new Pen(outlineBrush);
			graphics.DrawLine(outlinePen, bounds.X, bounds.Y, bounds.Right - 4, bounds.Y);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawArc(outlinePen, bounds.Right - 7, bounds.Y, 6, 6, -90, 90);
			graphics.DrawLine(outlinePen, bounds.Right - 1, bounds.Y + 2, bounds.Right - 1, bounds.Bottom - 4);
			graphics.DrawArc(outlinePen, bounds.Right - 7, bounds.Bottom - 7, 6, 6, 90, -90);
			graphics.SmoothingMode = smoothingMode;
			Brush bottomBorderBrush = new LinearGradientBrush(bounds, colorScheme._groupBorderTop, colorScheme._scrollButtonOutlineEnd, LinearGradientMode.Vertical);
			Pen bottomBorderPen = new Pen(bottomBorderBrush);
			graphics.DrawLine(bottomBorderPen, bounds.Right - 3, bounds.Bottom - 1, bounds.X, bounds.Bottom - 1);
			graphics.DrawLine(bottomBorderPen, bounds.X, bounds.Bottom - 2, bounds.X, bounds.Y + 1);
			Pen shadowPen = GdiCache.Pen(Color.FromArgb(130, colorScheme._scrollButtonOutlineEnd));
			graphics.DrawLine(shadowPen, bounds.X, bounds.Bottom, bounds.Right - 5, bounds.Bottom);
			graphics.DrawArc(shadowPen, bounds.Right - 7, bounds.Bottom - 6, 6, 6, 90, -90);
			shadowPen = GdiCache.Pen(Color.FromArgb(50, colorScheme._scrollButtonOutlineEnd));
			graphics.DrawLine(shadowPen, bounds.X, bounds.Bottom + 1, bounds.Right - 5, bounds.Bottom + 1);
			graphics.DrawArc(shadowPen, bounds.Right - 7, bounds.Bottom - 5, 6, 6, 90, -90);
			Brush shadowBrush = new LinearGradientBrush(new Point(bounds.Right, bounds.Y + 4), new Point(bounds.Right, bounds.Bottom - 3), Color.FromArgb(50, colorScheme._scrollButtonOutlineEnd), Color.FromArgb(80, colorScheme._scrollButtonOutlineEnd));
			shadowPen = new Pen(shadowBrush);
			graphics.DrawLine(shadowPen, bounds.Right, bounds.Y + 4, bounds.Right, bounds.Bottom - 3);
			graphics.SmoothingMode = smoothingMode;
			Rectangle arrowBounds = new Rectangle(button._bounds.X + (button._bounds.Width - 3) / 2, button._bounds.Y + (button._bounds.Height - 5) / 2, 3, 5);
			Color arrowEndColor = colorScheme._scrollButtonArrowEnd;
			Color arrowStartColor = colorScheme._scrollButtonArrowStart;
			arrowBounds.X--;
			Brush arrowBrush = new LinearGradientBrush(arrowBounds, arrowStartColor, arrowEndColor, LinearGradientMode.Horizontal);
			arrowBounds.X++;
			Pen arrowPen = new Pen(arrowBrush);
			graphics.DrawLine(arrowPen, arrowBounds.X, arrowBounds.Y, arrowBounds.X, arrowBounds.Y + 4);
			graphics.DrawLine(arrowPen, arrowBounds.X + 1, arrowBounds.Y + 1, arrowBounds.X + 1, arrowBounds.Y + 3);
			graphics.DrawLine(arrowPen, arrowBounds.X + 2, arrowBounds.Y + 2, arrowBounds.X + 2, (float)arrowBounds.Y + 2.5f);
			pen.Dispose();
			outlinePen.Dispose();
			bottomBorderPen.Dispose();
			innerHighlightBrush.Dispose();
			outlineBrush.Dispose();
			bottomBorderBrush.Dispose();
			shadowPen.Dispose();
			shadowBrush.Dispose();
			arrowPen.Dispose();
			arrowBrush.Dispose();
		}

	}
}
