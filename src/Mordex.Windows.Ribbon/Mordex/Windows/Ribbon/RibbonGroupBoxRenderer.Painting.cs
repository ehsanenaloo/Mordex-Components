using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonGroupBoxRenderer
	{
		public bool DrawsBackground()
		{
			return _drawsBackground;
		}

		private void RenderOffice2010Caption(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor)
		{
			Rectangle rectangle = _captionBarBounds;
			rectangle.Y--;
			if (rectangle.Width > 0 && rectangle.Height > 0 && _captionBarBounds.Width > 0 && _captionBarBounds.Height > 0)
			{
				textRenderer.SetStringAlignment(StringAlignment.Center);
				textRenderer.SetLineAlignment(StringAlignment.Far);
				textRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
				// A caption color of the theme applies only while the ribbon keeps the dark text color of the theme.
				Color captionColor = !colorScheme._groupCaptionText.IsEmpty && foreColor == colorScheme._foreColorDark ? colorScheme._groupCaptionText : foreColor;
				textRenderer.RenderContent(graphics, GetCaptionText(), _textBounds, GetRibbonGroup().ForegroundStyle, font, captionColor, colorScheme._disabledTextColor, _commandBaseRendererCommand.IsEnabled);
				if (GetRibbonGroup().DialogButtonVisible)
				{
					_dialogLauncher.Paint(graphics, textRenderer, colorScheme, font, foreColor, Rectangle.Empty);
				}
			}
		}

		public void DrawPopupOpenBackground(Graphics graphics, RibbonColorScheme colorScheme)
		{
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(_bounds.X, _bounds.Y, 6, 6, -180f, 90f);
			graphicsPath.AddLine(_bounds.X + 2, _bounds.Y, _bounds.Right - 3, _bounds.Y);
			graphicsPath.AddArc(_bounds.Right - 7, _bounds.Y, 6, 6, -90f, 90f);
			graphicsPath.AddLine(_bounds.Right - 1, _bounds.Y + 2, _bounds.Right - 1, _bounds.Bottom - 3);
			graphicsPath.AddArc(_bounds.Right - 7, _bounds.Bottom - 7, 6, 6, 0f, 90f);
			graphicsPath.AddLine(_bounds.Right - 3, _bounds.Bottom - 1, _bounds.X + 2, _bounds.Bottom - 1);
			graphicsPath.AddArc(_bounds.X, _bounds.Bottom - 7, 6, 6, 90f, 90f);
			graphicsPath.AddLine(_bounds.X, _bounds.Bottom - 3, _bounds.X, _bounds.Y + 2);
			graphics.SmoothingMode = SmoothingMode.None;
			Rectangle rectangle = _bounds;
			rectangle.Inflate(-1, -1);
			int topHeight = Convert.ToInt32(Math.Round((float)_bounds.Height * 0.2f)) - 1;
			rectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, topHeight + 1);
			Rectangle rect = rectangle;
			rect.Y--;
			rect.Height += 2;
			Brush brush = new LinearGradientBrush(rect, colorScheme._groupFillTopStart, colorScheme._groupFillTopEnd, LinearGradientMode.Vertical);
			rectangle.Height++;
			graphics.FillRectangle(brush, rectangle);
			brush.Dispose();
			rectangle = Rectangle.FromLTRB(rectangle.X, rectangle.Y + topHeight, rectangle.Right, _bounds.Bottom - 2);
			rect = rectangle;
			brush = new LinearGradientBrush(rect, colorScheme._groupFillBottomStart, colorScheme._groupFillBottomEnd, LinearGradientMode.Vertical);
			rectangle.Y++;
			graphics.FillRectangle(brush, rectangle);
			brush.Dispose();
			rect = _bounds;
			rect.Inflate(-1, 0);
			rect.Height = 4;
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, colorScheme._groupFillGlow, Color.FromArgb(0, colorScheme._groupFillGlow), LinearGradientMode.Vertical);
			Blend blend = new Blend();
			blend.Positions = new float[5] { 0f, 0.25f, 0.5f, 0.75f, 1f };
			blend.Factors = new float[5] { 0f, 0.3f, 0.66f, 0.9f, 1f };
			linearGradientBrush.Blend = blend;
			rect.Height--;
			rect.Y++;
			graphics.FillRectangle(linearGradientBrush, rect);
			linearGradientBrush.Dispose();
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			rect = new Rectangle(_bounds.X, _bounds.Top - 1, _bounds.Width, _bounds.Height + 2);
			LinearGradientBrush borderBrush = new LinearGradientBrush(rect, colorScheme._groupOutlineStart, colorScheme._groupOutlineEnd, LinearGradientMode.Vertical);
			using (Pen pen = new Pen(borderBrush))
			{
				graphics.DrawPath(pen, graphicsPath);
				graphicsPath.Dispose();
			}
			borderBrush.Dispose();
			borderBrush = new LinearGradientBrush(rect, Color.FromArgb(40, Color.White), Color.FromArgb(8, Color.White), LinearGradientMode.Vertical);
			borderBrush.SetSigmaBellShape(0.2f, 0.5f);
			using (Pen highlightPen = new Pen(borderBrush))
			{
				graphics.DrawLine(highlightPen, _bounds.X + 1, _bounds.Y + 2, _bounds.X + 1, _bounds.Bottom - 3);
			}
			Rectangle bottomHighlightRect = new Rectangle(_bounds.X + 1, _bounds.Bottom - 2, _bounds.Width - 2, 2);
			graphics.FillRectangle(borderBrush, bottomHighlightRect);
			borderBrush.Dispose();
			graphics.SmoothingMode = smoothingMode;
		}

		private void DrawThemedGroupBackground(Graphics graphics, ResolvedTheme theme)
		{
			Padding padding = ((_commandBaseRendererRibbon != null) ? _commandBaseRendererRibbon.GetGroupsAreaPadding() : Padding.Empty);
			Rectangle area = new Rectangle(_bounds.X, _bounds.Y - padding.Top, _bounds.Width, _bounds.Height + padding.Top + padding.Bottom - 1);
			bool open = IsPopupOpen();
			bool hot = Hot || _fadeAlpha != 255;
			if (_isCollapsed && (open || hot))
			{
				ThemedRibbonPainter.CollapsedGroup(graphics, theme, Rectangle.Inflate(area, -1, 0), hot, open);
				return;
			}
			if (!ThemedRibbonPainter.GroupBox(graphics, theme, area, _captionBarBounds))
			{
				ThemedRibbonPainter.GroupSeparator(graphics, theme, area);
			}
		}

		private void DrawOffice2010GroupBackground(Graphics graphics, RibbonColorScheme colorScheme)
		{
			DrawThemedGroupBackground(graphics, colorScheme.Theme);
			return;
		}

		private void DrawOffice2010CollapsedIconBackground(Graphics graphics, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.CollapsedIcon(graphics, colorScheme.Theme, _collapsedIconBounds);
			return;
		}

		private void PaintOffice2010(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color color, Rectangle clipRectangle)
		{
			if (_isCollapsed)
			{
				DrawOffice2010GroupBackground(graphics, colorScheme);
				textRenderer.SetWrapEnabled(true);
				textRenderer.SetStringAlignment(StringAlignment.Center);
				textRenderer.SetLineAlignment(StringAlignment.Near);
				color = _commandBaseRendererRibbon.ForeColorDark;
				textRenderer.RenderContent(graphics, GetCaptionText(), _textBounds, GetRibbonGroup().ForegroundStyle, font, color, colorScheme._disabledTextColor, _commandBaseRendererCommand.IsEnabled);
				Rectangle rectangle = _lastWordBounds;
				rectangle.Offset(0, 1);
				RibbonChromePainter.DrawVerticalArrow(graphics, rectangle, 1, _isEnabled, colorScheme._glyphDisabledColor, colorScheme);
				if (!_imageBounds.IsEmpty)
				{
					DrawOffice2010CollapsedIconBackground(graphics, colorScheme);
					RibbonDrawingUtils.DrawImageFromSources(graphics, GetRibbonGroup().Icon, GetRibbonGroup().Image, GetRibbonGroup().ImageIndex, GetRibbonGroup().ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, GetRibbonGroup().IsEnabled, false, colorScheme);
				}
				return;
			}
			if (DrawsBackground())
			{
				DrawOffice2010GroupBackground(graphics, colorScheme);
			}
			RenderOffice2010Caption(graphics, textRenderer, colorScheme, font, color);
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				Rectangle itemClip = Rectangle.Intersect(clipRectangle, item._bounds);
				if (itemClip.Width > 0 && itemClip.Height > 0)
				{
					item.Paint(graphics, textRenderer, colorScheme, font, _commandBaseRendererRibbon.ForeColorDark, clipRectangle);
				}
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			PaintOffice2010(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
			return;
		}
	}
}
