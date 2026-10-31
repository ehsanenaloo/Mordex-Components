using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockManagerRendererBase
	{
		private const int ContentGap = 4;

		private const int CaptionTextPadding = 6;

		private const int CaptionImagePadding = 7;

		private const int SeparatorInset = 3;

		private const int GripWidth = 35;

		private const int GripDotCount = 9;

		private const int GripDotSpacing = 4;

		private const int SmallButtonImageSize = 16;

		private const int ButtonTextInset = 8;

		private const int ButtonTextInsetIndented = 12;

		private const int ButtonImageGap = 8;

		private static readonly Color GripShadowColor = Color.FromArgb(40, 50, 71);

		private static readonly Color GripHighlightColor = Color.FromArgb(249, 249, 251);

		private static readonly Color GripDotColor = Color.FromArgb(97, 116, 142);

		protected MxColorSchemeAppearanceState _colorSchemeAppearance;

		private MxDockManager _dockManager;

		private bool _useCompatibleTextRendering;

		internal DockManagerRendererBase(MxDockManager manager)
			: this(manager, createScheme: true)
		{
		}

		protected DockManagerRendererBase(MxDockManager manager, bool createScheme)
		{
			_dockManager = manager;
			_useCompatibleTextRendering = _dockManager.UseCompatibleTextRendering;
			if (createScheme)
			{
				_colorSchemeAppearance = new MxColorSchemeAppearanceState();
			}
		}

		internal MxColorSchemeAppearanceState GetColorSchemeAppearance()
		{
			return _colorSchemeAppearance;
		}

		internal virtual void DrawInnerAreaBackground(Graphics graphics, Control sender, Rectangle rect, PanelInnerAreaStyle style)
		{
			Brush brush = null;
			switch (style)
			{
			case PanelInnerAreaStyle.AppWorkspace:
				brush = new SolidBrush(_dockManager.GetDefaultColor(PanelColors.InnerWorkspace));
				break;
			case PanelInnerAreaStyle.ContainerPanel:
				brush = new SolidBrush(_dockManager.GetDefaultColor(PanelColors.InnerContainer));
				break;
			case PanelInnerAreaStyle.Window:
				brush = new SolidBrush(_dockManager.GetDefaultColor(PanelColors.InnerWindow));
				break;
			}
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
				brush.Dispose();
				brush = null;
			}
		}

		public virtual void DrawFloatingBorder(Graphics graphics, DockPanelBase panel, Rectangle borderRect)
		{
			Rectangle rectangle = borderRect;
			rectangle.Offset(1, 1);
			rectangle.Width--;
			rectangle.Height--;
			RenderingHelper.DrawColoredBorder(graphics, _colorSchemeAppearance.GetColor(PanelColors.FloatingWindowBorder), rectangle, BorderStyle.Raised);
		}

		public virtual int GetCaptionSize(DockPanelBase panel, Font font, FormatStyle captionStyle)
		{
			int captionSize = -1;
			try
			{
				captionSize = font.Height;
				captionSize += CaptionTextPadding;
				CommandImageSource imageRenderer = panel.GetImageSource();
				int height = _dockManager.GetPanelImageSize(imageRenderer).Height;
				height += CaptionImagePadding;
				if (height > captionSize)
				{
					captionSize = height;
				}
			}
			catch
			{
			}
			return captionSize;
		}

		public virtual void DrawPanelButton(Graphics graphics, PanelCaptionStyle titleStyle, DockPanelBase panel, PanelButtonType buttonType, Rectangle buttonBounds, ButtonState state, Font font, FormatStyle captionFormatStyle, Image buttonImage)
		{
			bool isPressed = (state & ButtonState.Pressed) == ButtonState.Pressed;
			bool isHot = (state & ButtonState.Hot) == ButtonState.Hot;
			if (isPressed || (isHot && buttonBounds.Width > 0 && buttonBounds.Height > 0))
			{
				if (!captionFormatStyle.BackColorGradient.IsEmpty)
				{
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(buttonBounds, 1, 1), captionFormatStyle.BackColor, captionFormatStyle.BackColorGradient, LinearGradientMode.Vertical);
					graphics.FillRectangle(linearGradientBrush, buttonBounds);
					linearGradientBrush.Dispose();
					linearGradientBrush = null;
				}
				else
				{
					Brush brush = GdiCache.Brush(captionFormatStyle.BackColor);
					graphics.FillRectangle(brush, buttonBounds);
					brush = null;
				}
			}
			switch (buttonType)
			{
			case PanelButtonType.ScrollLeft:
			{
				Pen leftArrowPen = GdiCache.Pen(captionFormatStyle.ForeColor);
				buttonBounds.Y++;
				RenderingHelper.DrawHorizontalArrow(graphics, buttonBounds.Height, buttonBounds, -1, leftArrowPen, state != ButtonState.Disabled);
				buttonBounds.Y--;
				break;
			}
			case PanelButtonType.ScrollRight:
			{
				Pen rightArrowPen = GdiCache.Pen(captionFormatStyle.ForeColor);
				buttonBounds.Y++;
				RenderingHelper.DrawHorizontalArrow(graphics, buttonBounds.Height, buttonBounds, 1, rightArrowPen, state != ButtonState.Disabled);
				buttonBounds.Y--;
				break;
			}
			default:
				if (buttonImage != null)
				{
					Rectangle rectangle = buttonBounds;
					rectangle.X = rectangle.Right - rectangle.Height;
					rectangle.Width = rectangle.Height;
					RenderingHelper.DrawRecoloredImage(graphics, buttonImage, rectangle, captionFormatStyle.ForeColor);
				}
				break;
			}
			if (buttonType == PanelButtonType.CaptionButton)
			{
				FormatStyle infoTextStyle = null;
				if (panel != null && panel.InfoText.Length > 0)
				{
					infoTextStyle = panel.GetInfoTextFormatStyle(panel.IsFloatingRoot());
				}
				DrawPanelCaptionForeground(graphics, titleStyle, panel, buttonBounds, font, captionFormatStyle, infoTextStyle);
			}
			if (state != ButtonState.Normal && state != ButtonState.Disabled)
			{
				Pen hotBorderPen = GdiCache.Pen(_dockManager.GetDefaultColor(PanelColors.HotElementBorder));
				RenderingHelper.DrawRectangleOutline(graphics, hotBorderPen, buttonBounds);
			}
		}

		public virtual void DrawTabStrip(Graphics graphics, Control sender, Rectangle bounds, FormatStyle formatStyle, Color borderColor, PanelDockStyle dockStyle)
		{
			RenderingHelper.DrawFormatStyleBackground(graphics, formatStyle, bounds, bounds);
			Pen pen = GdiCache.Pen(borderColor);
			if (dockStyle == PanelDockStyle.Top)
			{
				graphics.DrawLine(pen, new Point(bounds.X, bounds.Bottom - 1), new Point(bounds.Right - 1, bounds.Bottom - 1));
			}
			else
			{
				graphics.DrawLine(pen, bounds.Location, new Point(bounds.Right - 1, bounds.Top));
			}
		}

		public virtual void DrawTabHorizontal(Graphics graphics, DockPanelBase panel, TabAlignment alignment, FormatStyle formatStyle, ButtonState state, Rectangle bounds)
		{
			if (alignment == TabAlignment.Top)
			{
				bounds.Offset(0, 1);
			}
			DrawTabHorizontalBackground(graphics, panel, alignment, formatStyle, state, bounds);
			DrawTabHorizontalForeground(graphics, panel, formatStyle, state, bounds);
		}

		public virtual void DrawMdiTabBackground(Graphics graphics, MxMdiTabGroup tabControl, FormatStyle formatStyle, ButtonState state, Rectangle bounds, bool isSelectedTab)
		{
			Color borderColor = _dockManager.TabbedMdiSettings.BorderColor;
			if (state == ButtonState.Selected || state == ButtonState.Pressed)
			{
				bounds.Inflate(1, 0);
				bounds.Height++;
				RenderingHelper.DrawFormatStyleBackground(graphics, formatStyle, bounds, bounds);
				graphics.DrawLine(SystemPens.ControlLightLight, new Point(bounds.X, bounds.Top), new Point(bounds.X, bounds.Bottom - 1));
				graphics.DrawLine(SystemPens.ControlLightLight, new Point(bounds.X, bounds.Top), new Point(bounds.Right, bounds.Top));
				Pen selectedBorderPen = GdiCache.Pen(borderColor);
				graphics.DrawLine(selectedBorderPen, new Point(bounds.Right, bounds.Top), new Point(bounds.Right, bounds.Bottom - 1));
			}
			else
			{
				RenderingHelper.DrawFormatStyleBackground(graphics, formatStyle, bounds, bounds);
				Pen separatorPen = GdiCache.Pen(borderColor);
				graphics.DrawLine(separatorPen, new Point(bounds.Right - 1, bounds.Y + SeparatorInset), new Point(bounds.Right - 1, bounds.Bottom - SeparatorInset));
			}
		}

		public virtual void DrawMdiTabForeground(Graphics graphics, MxMdiTabGroup tabControl, MdiChildTab tab, FormatStyle formatStyle, ButtonState state, Rectangle bounds, Rectangle clipRect)
		{
			string text = tab.Text;
			Font font = formatStyle.ResolveFont(tabControl.Font);
			int contentX = bounds.X + 2;
			CommandImageSource imageRenderer = tab.CreateImageSource();
			Size size = tab.GetImageSize();
			if (!imageRenderer.IsEmpty())
			{
				if (imageRenderer._icon != null)
				{
					Rectangle rectangle = new Rectangle(contentX, bounds.Y + (bounds.Height - size.Height) / 2 + 2, size.Width, size.Height);
					if (rectangle.Right <= clipRect.Right)
					{
						RenderingHelper.DrawIcon(graphics, imageRenderer._icon, rectangle, true, RenderingHelper.GetSchemeColor(ColorName.Control));
					}
					contentX += size.Width + 2;
				}
				else
				{
					imageRenderer.DrawImage(graphics, new Rectangle(contentX, bounds.Y + (bounds.Height - size.Height) / 2 + 2, size.Width, size.Height), true);
					contentX += size.Width + 2;
				}
			}
			if (text != null && text.Length > 0 && formatStyle.GetForeBrush() != null)
			{
				Rectangle labelBounds = Rectangle.FromLTRB(contentX, bounds.Y + 1, bounds.Right, bounds.Bottom);
				if (labelBounds.Width > 0)
				{
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
					textLayoutRenderer.SetStringTrimming(tabControl.PanelManager.TabbedMdiSettings.TabCaptionTrimming);
					textLayoutRenderer.SetWordWrap(false);
					textLayoutRenderer.DrawTextWithBrush(graphics, text, labelBounds, font, formatStyle.GetForeBrush());
					textLayoutRenderer.ResetCache();
				}
			}
		}

		public virtual void DrawMdiBackground(Graphics graphics, Control sender, Rectangle bounds, FormatStyle style)
		{
			RenderingHelper.DrawFormatStyleBackground(graphics, style, bounds, bounds);
		}

		public virtual void DrawTabHorizontalBackground(Graphics graphics, DockPanelBase panel, TabAlignment alignment, FormatStyle formatStyle, ButtonState state, Rectangle bounds)
		{
			Color color = panel.GetPanelBorderColor();
			if (state == ButtonState.Selected || state == ButtonState.Pressed)
			{
				bounds.Inflate(1, 0);
				RenderingHelper.DrawFormatStyleBackground(graphics, formatStyle, bounds, bounds);
				Pen pen = GdiCache.Pen(color);
				graphics.DrawLine(pen, new Point(bounds.X, bounds.Bottom), new Point(bounds.Right, bounds.Bottom));
				graphics.DrawLine(pen, new Point(bounds.Right, bounds.Bottom), new Point(bounds.Right, bounds.Y));
				if (alignment == TabAlignment.Bottom)
				{
					graphics.DrawLine(SystemPens.ControlLightLight, bounds.Location, new Point(bounds.X, bounds.Bottom));
				}
				else
				{
					graphics.DrawLine(SystemPens.ControlLightLight, bounds.Location, new Point(bounds.Right, bounds.Top));
				}
			}
			else
			{
				if (alignment != TabAlignment.Top)
				{
					bounds.Y++;
				}
				else
				{
					bounds.Y--;
				}
				RenderingHelper.DrawFormatStyleBackground(graphics, formatStyle, bounds, bounds);
				DrawSeparator(graphics, color, bounds);
			}
		}

		internal virtual void DrawSeparator(Graphics graphics, Color borderColor, Rectangle bounds)
		{
			Pen pen = GdiCache.Pen(borderColor);
			graphics.DrawLine(pen, new Point(bounds.Right - 1, bounds.Y + SeparatorInset), new Point(bounds.Right - 1, bounds.Bottom - SeparatorInset));
		}

		public virtual void DrawTabHorizontalForeground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, ButtonState state, Rectangle bounds)
		{
			Font font = panel.GetCaptionFont(formatStyle);
			int contentX = bounds.X;
			CommandImageSource imageRenderer = panel.GetImageSource();
			if (!imageRenderer.IsEmpty())
			{
				Size size = _dockManager.GetPanelImageSize(imageRenderer);
				contentX += ContentGap;
				DrawCommandImage(graphics, imageRenderer, contentX, bounds.Y + (bounds.Height - size.Height) / 2, size, panel.Enabled);
				contentX += size.Width;
			}
			contentX += ContentGap;
			if (panel.Text != null && panel.Text.Length > 0 && formatStyle.GetForeBrush() != null)
			{
				Rectangle rectangle = Rectangle.FromLTRB(contentX, bounds.Y, bounds.Right, bounds.Bottom);
				if (rectangle.Width > 0)
				{
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
					textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
					textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
					textLayoutRenderer.SetWordWrap(false);
					textLayoutRenderer.DrawTextWithBrush(graphics, panel.Text, rectangle, font, formatStyle.GetForeBrush());
					textLayoutRenderer.ResetCache();
				}
			}
		}

		public virtual void DrawBackground(Graphics graphics, Control sender, Rectangle rect)
		{
			if (_dockManager != null)
			{
				Brush brush = new SolidBrush(_dockManager.BackColorSplitter);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
					brush.Dispose();
				}
			}
		}

		public virtual void DrawPanelCaptionBackground(Graphics graphics, PanelCaptionStyle captionStyle, DockPanelBase panel, Rectangle captionBounds, FormatStyle captionFormatStyle)
		{
			DrawFormatStyleBackground(graphics, captionBounds, captionFormatStyle);
		}

		internal static void DrawFormatStyleBackground(Graphics graphics, Rectangle rectangle, FormatStyle uiFormatStyle)
		{
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, rectangle, rectangle);
		}

		public virtual void DrawPanelCaptionForeground(Graphics graphics, PanelCaptionStyle titleStyle, DockPanelBase panel, Rectangle textBounds, Font font, FormatStyle captionFormatStyle, FormatStyle infoTextStyle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			if (panel != null)
			{
				switch (panel.GetEffectiveTextAlignment())
				{
				case PanelTextAlignment.Center:
					textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
					break;
				case PanelTextAlignment.Near:
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					break;
				case PanelTextAlignment.Far:
					textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
					break;
				}
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			}
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetWordWrap(false);
			string text = panel.Text;
			PanelCaptionDisplayMode panelCaptionDisplayMode = PanelCaptionDisplayMode.Text;
			panelCaptionDisplayMode = panel.GetEffectiveCaptionDisplayMode();
			if (panelCaptionDisplayMode == PanelCaptionDisplayMode.Image || panelCaptionDisplayMode == PanelCaptionDisplayMode.ImageAndText)
			{
				CommandImageSource imageRenderer = panel.GetImageSource();
				if (!imageRenderer.IsEmpty())
				{
					Size size = _dockManager.GetPanelImageSize(imageRenderer);
					if (panel.GetEffectiveImageAlignment() == PanelImageAlignment.Near)
					{
						DrawCommandImage(graphics, imageRenderer, textBounds.X, textBounds.Top + (textBounds.Height - size.Height) / 2, size, panel.Enabled);
						textBounds.X += size.Width + ContentGap;
						textBounds.Width -= size.Width + ContentGap;
					}
					else
					{
						DrawCommandImage(graphics, imageRenderer, textBounds.Right - size.Width - ContentGap, textBounds.Top + (textBounds.Height - size.Height) / 2, size, panel.Enabled);
						textBounds.Width -= size.Width + ContentGap;
					}
				}
			}
			if (panelCaptionDisplayMode == PanelCaptionDisplayMode.ImageAndText || panelCaptionDisplayMode == PanelCaptionDisplayMode.Text)
			{
				if (font != null && text.Length > 0 && captionFormatStyle.GetForeBrush() != null)
				{
					textLayoutRenderer.DrawTextWithBrush(graphics, text, textBounds, font, captionFormatStyle.GetForeBrush());
				}
				if (panel.InfoText.Length > 0)
				{
					if (panel.GetEffectiveTextAlignment() == PanelTextAlignment.Near)
					{
						SizeF nearTextSize = textLayoutRenderer.MeasureText(graphics, text, font, 0);
						nearTextSize.Width += ContentGap;
						if (nearTextSize.Width < (float)textBounds.Width)
						{
							textBounds.X += (int)nearTextSize.Width;
							textBounds.Width -= (int)nearTextSize.Width + ContentGap;
						}
						else
						{
							textBounds = Rectangle.Empty;
						}
					}
					else if (panel.GetEffectiveTextAlignment() == PanelTextAlignment.Far)
					{
						SizeF farTextSize = textLayoutRenderer.MeasureText(graphics, text, font, 0);
						farTextSize.Width += ContentGap;
						if (farTextSize.Width < (float)textBounds.Width)
						{
							textBounds.Width -= (int)farTextSize.Width;
						}
						else
						{
							textBounds = Rectangle.Empty;
						}
					}
				}
				if (panel.InfoText.Length > 0 && textBounds.Width > 0)
				{
					switch (panel.GetEffectiveInfoTextAlignment())
					{
					case PanelTextAlignment.Center:
						textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
						break;
					case PanelTextAlignment.Near:
						textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
						break;
					case PanelTextAlignment.Far:
						textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
						break;
					}
					if (infoTextStyle != null)
					{
						Font infoFont = infoTextStyle.ResolveFont(font);
						if (infoFont != null && infoTextStyle != null && infoTextStyle.GetForeBrush() != null)
						{
							textLayoutRenderer.DrawTextWithBrush(graphics, panel.InfoText, textBounds, infoFont, infoTextStyle.GetForeBrush());
						}
					}
				}
			}
			textLayoutRenderer.ResetCache();
		}

		internal static void DrawCommandImage(Graphics graphics, CommandImageSource imageRenderer, int x, int y, Size size, bool isEnabled)
		{
			if (imageRenderer._icon != null)
			{
				RenderingHelper.DrawIcon(graphics, imageRenderer._icon, new Rectangle(x, y, size.Width, size.Height), isEnabled, SystemColors.Control);
			}
			else
			{
				imageRenderer.DrawImage(graphics, new Rectangle(x, y, size.Width, size.Height), isEnabled);
			}
		}

		internal static void DrawOutlineIfVisible(Graphics graphics, Rectangle rectangle, Color color)
		{
			if (!color.IsEmpty && color.A > 0)
			{
				Pen pen = GdiCache.Pen(color);
				RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
			}
		}

		public virtual void DrawCaptionBorder(Graphics graphics, DockPanelBase panel, Rectangle captionBounds)
		{
			Color color = panel.GetCaptionBorderColor();
			DrawOutlineIfVisible(graphics, captionBounds, color);
		}

		public virtual void DrawPanelBorder(Graphics graphics, DockPanelBase panel, Rectangle borderRect)
		{
			if (panel == null)
			{
				DrawBorder(graphics, borderRect, _dockManager.DefaultPanelSettings.BorderPanelColor);
			}
			else
			{
				DrawBorder(graphics, borderRect, panel.GetPanelBorderColor());
			}
		}

		internal static void DrawBorder(Graphics graphics, Rectangle rectangle, Color color)
		{
			Pen pen = GdiCache.Pen(color);
			RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
		}

		public MxDockManager GetDockManager()
		{
			return _dockManager;
		}

		public virtual void DrawResizeBarBackground(Graphics graphics, DockPanelBase panel, Rectangle bounds)
		{
			Brush brush = GdiCache.Brush(GetDockManager().BackColorResizeBar);
			graphics.FillRectangle(brush, bounds);
		}

		public virtual void DrawResizeBarGrabHandle(Graphics graphics, DockPanelBase panel, Rectangle bounds)
		{
			int gripX = bounds.Left + (bounds.Width - GripWidth) / 2;
			Brush shadowBrush = GdiCache.Brush(GripShadowColor);
			Brush highlightBrush = GdiCache.Brush(GripHighlightColor);
			Brush dotBrush = GdiCache.Brush(GripDotColor);
			for (int i = 0; i < GripDotCount; i++)
			{
				Rectangle rect = new Rectangle(gripX, bounds.Top + 2, 2, 2);
				graphics.FillRectangle(shadowBrush, rect);
				rect.Offset(1, 1);
				graphics.FillRectangle(highlightBrush, rect);
				rect.Size = new Size(1, 1);
				graphics.FillRectangle(dotBrush, rect);
				gripX += GripDotSpacing;
			}
			shadowBrush = null;
			highlightBrush = null;
			dotBrush = null;
		}

		public virtual void DrawResizeBar(Graphics graphics, DockPanelBase panel, Rectangle bounds)
		{
			DrawResizeBarBackground(graphics, panel, bounds);
			DrawResizeBarGrabHandle(graphics, panel, bounds);
		}

		public virtual void DrawBarGroupSmallButton(Graphics graphics, DockPanelBase panel, Rectangle bounds, ButtonState state, CommandImageSource image, FormatStyle formatStyle)
		{
			DrawBarGroupSmallButtonBackground(graphics, panel, bounds, state, formatStyle);
			DrawBarGroupSmallButtonForeground(graphics, panel, bounds, image, state, formatStyle);
		}

		public virtual void DrawBarGroupSmallButtonBackground(Graphics graphics, DockPanelBase panel, Rectangle bounds, ButtonState state, FormatStyle formatStyle)
		{
			if (state != ButtonState.Normal)
			{
				DrawBarGroupButtonBackground(graphics, panel, bounds, state, formatStyle);
			}
		}

		public virtual void DrawBarGroupSmallButtonForeground(Graphics graphics, DockPanelBase panel, Rectangle bounds, CommandImageSource imageHolder, ButtonState state, FormatStyle formatStyle)
		{
			Rectangle rectangle = bounds;
			if (imageHolder != null)
			{
				Size size = new Size(SmallButtonImageSize, SmallButtonImageSize);
				rectangle.Y += (rectangle.Height - size.Height) / 2;
				rectangle.X += (rectangle.Width - size.Width) / 2;
				DrawCommandImage(graphics, imageHolder, rectangle.X, rectangle.Y, size, panel.Enabled);
			}
		}

		public virtual void DrawGrabItem(Graphics graphics, Rectangle bounds)
		{
			Brush brush = null;
			Brush windowBrush = null;
			Color color = RenderingHelper.BlendColors(RenderingHelper.GetSchemeColor(ColorName.ControlDark), RenderingHelper.GetSchemeColor(ColorName.Window), 0.75f);
			brush = GdiCache.Brush(color);
			windowBrush = GdiCache.Brush(RenderingHelper.GetSchemeColor(ColorName.Window));
			Rectangle rectangle = bounds;
			rectangle.X += (rectangle.Width - 4) / 2;
			rectangle.Inflate(0, -5);
			while (rectangle.Height > 4)
			{
				Rectangle rect = new Rectangle(rectangle.Left + 2, rectangle.Y + 1, 2, 2);
				graphics.FillRectangle(windowBrush, rect);
				rect.Offset(-1, -1);
				graphics.FillRectangle(brush, rect);
				rectangle.Y += 4;
				rectangle.Height -= 4;
			}
			brush = null;
			windowBrush = null;
		}

		public virtual void DrawBarGroupButton(Graphics graphics, DockPanelBase panel, Rectangle bounds, ButtonState state, CommandImageSource image, string text, FormatStyle formatStyle, Font font)
		{
			if (!panel.Enabled)
			{
				state = ButtonState.Disabled;
			}
			DrawBarGroupButtonBackground(graphics, panel, bounds, state, formatStyle);
			DrawBarGroupButtonForeground(graphics, panel, bounds, text, image, state, formatStyle, font);
			DrawBarGroupButtonBorder(graphics, panel, bounds, state);
		}

		public virtual void DrawBarGroupButtonBackground(Graphics graphics, DockPanelBase panel, Rectangle bounds, ButtonState state, FormatStyle formatStyle)
		{
			Brush brush = formatStyle.GetBackgroundBrush(bounds);
			if (brush != null)
			{
				graphics.FillRectangle(brush, bounds);
			}
		}

		public virtual void DrawBarGroupButtonForeground(Graphics graphics, DockPanelBase panel, Rectangle bounds, string text, CommandImageSource image, ButtonState state, FormatStyle formatStyle, Font font)
		{
			Brush brush = formatStyle.GetForeBrush();
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			Rectangle rectangle = bounds;
			int textInset = ButtonTextInset;
			if (panel.CanDragPanel())
			{
				textInset = ButtonTextInsetIndented;
			}
			rectangle.X += textInset;
			rectangle.Width -= textInset;
			Rectangle contentBounds = rectangle;
			if (image != null)
			{
				Size size = _dockManager.GetLargeImageSizeFor(image);
				DrawCommandImage(graphics, image, contentBounds.X, contentBounds.Y + (contentBounds.Height - size.Height) / 2, size, panel.Enabled);
				rectangle = Rectangle.FromLTRB(contentBounds.X + size.Width + ButtonImageGap, bounds.Top, bounds.Right, bounds.Bottom);
			}
			if (brush != null && rectangle.Width > 0)
			{
				textLayoutRenderer.DrawTextWithBrush(graphics, panel.Text, rectangle, font, brush);
			}
			textLayoutRenderer.ResetCache();
		}

		public virtual void DrawBarGroupButtonBorder(Graphics graphics, DockPanelBase panel, Rectangle bounds, ButtonState state)
		{
			Color color = ((panel.ParentGroup == null) ? panel.GetPanelBorderColor() : panel.ParentGroup.GetPanelBorderColor());
			Pen pen = GdiCache.Pen(color);
			RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
		}

		internal virtual void DrawBarGroupChevron(Graphics graphics, Rectangle bounds, ButtonState state, FormatStyle formatStyle)
		{
			{
				Image image = RenderingHelper.MakeMagentaTransparent(WindowsResources.ChevronOutlook());
				Rectangle rect = new Rectangle(bounds.X + (bounds.Width - image.Width) / 2, bounds.Top + (bounds.Height - image.Height) / 2, image.Width, image.Height);
				graphics.DrawImage(image, rect);
			}
		}

		internal virtual void DrawAutoHideStripBackground(Graphics graphics, Control sender, Rectangle bounds, AutoHideStripDock stripDock)
		{
			Brush brush = new SolidBrush(_dockManager.BackColorAutoHideStrip);
			if (brush != null)
			{
				graphics.FillRectangle(brush, bounds);
				brush.Dispose();
			}
		}

		internal virtual void DrawAutoHideStripCorner(Graphics graphics, Control sender, Rectangle bounds, ContentAlignment cornerAlignment)
		{
			Brush brush = new SolidBrush(_dockManager.BackColorAutoHideStrip);
			if (brush != null)
			{
				graphics.FillRectangle(brush, bounds);
				brush.Dispose();
			}
		}

		internal virtual void DrawAutoHideTabHorizontal(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, ButtonState state, Rectangle bounds, bool drawText, bool drawImage, bool isTopStrip)
		{
			DrawAutoHideTabHorizontalBackground(graphics, panel, formatStyle, state, bounds, isTopStrip);
			DrawAutoHideTabHorizontalForeground(graphics, panel, formatStyle, font, state, bounds, drawText, drawImage);
		}

		internal virtual void DrawAutoHideTabHorizontalBackground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, ButtonState state, Rectangle bounds, bool isTopStrip)
		{
			Color borderPanelColor = _dockManager.DefaultPanelSettings.BorderPanelColor;
			Brush brush = formatStyle.GetBackgroundBrush(bounds);
			if (brush != null)
			{
				graphics.FillRectangle(brush, bounds);
				brush.Dispose();
			}
			Pen pen = GdiCache.Pen(borderPanelColor);
			RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
			graphics.DrawLine(pen, new Point(bounds.Right - 1, bounds.Y + SeparatorInset), new Point(bounds.Right - 1, bounds.Bottom - SeparatorInset));
		}

		internal virtual void DrawAutoHideTabHorizontalForeground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, ButtonState state, Rectangle bounds, bool drawText, bool drawImage)
		{
			int contentX = bounds.X;
			if (drawImage)
			{
				CommandImageSource imageRenderer = panel.GetImageSource();
				Size size = _dockManager.GetPanelImageSize(imageRenderer);
				if (!imageRenderer.IsEmpty())
				{
					contentX += ContentGap;
					DrawCommandImage(graphics, imageRenderer, contentX, bounds.Y + (bounds.Height - size.Height) / 2, size, panel.Enabled);
					contentX += size.Width;
				}
			}
			contentX += ContentGap;
			if (drawText && panel.Text != null && panel.Text.Length > 0 && formatStyle.GetForeBrush() != null)
			{
				Rectangle rectangle = Rectangle.FromLTRB(contentX, bounds.Y, bounds.Right, bounds.Bottom);
				if (rectangle.Width > 0)
				{
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
					textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
					textLayoutRenderer.SetWordWrap(false);
					textLayoutRenderer.DrawTextWithBrush(graphics, panel.Text, rectangle, font, formatStyle.GetForeBrush());
					textLayoutRenderer.ResetCache();
				}
			}
		}

		internal virtual void DrawAutoHideTabVertical(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, ButtonState state, Rectangle bounds, bool drawText, bool drawImage, bool isLeftStrip)
		{
			DrawAutoHideTabVerticalBackground(graphics, panel, formatStyle, state, bounds, isLeftStrip);
			DrawAutoHideTabVerticalForeground(graphics, panel, formatStyle, font, state, bounds, drawText, drawImage);
		}

		internal virtual void DrawAutoHideTabVerticalBackground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, ButtonState state, Rectangle bounds, bool isLeftStrip)
		{
			Color borderPanelColor = _dockManager.DefaultPanelSettings.BorderPanelColor;
			RenderingHelper.DrawFormatStyleBackground(graphics, formatStyle, bounds, bounds);
			Pen pen = GdiCache.Pen(borderPanelColor);
			RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
			graphics.DrawLine(pen, new Point(bounds.Right - 1, bounds.Y + SeparatorInset), new Point(bounds.Right - 1, bounds.Bottom - SeparatorInset));
		}

		internal virtual void DrawAutoHideTabVerticalForeground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, ButtonState state, Rectangle bounds, bool drawText, bool drawImage)
		{
			int contentY = bounds.Y;
			if (drawImage)
			{
				CommandImageSource imageRenderer = panel.GetImageSource();
				if (!imageRenderer.IsEmpty())
				{
					Size size = _dockManager.GetPanelImageSize(imageRenderer);
					contentY += ContentGap;
					DrawCommandImage(graphics, imageRenderer, bounds.X + (bounds.Width - size.Width) / 2, contentY, size, panel.Enabled);
					contentY += size.Height;
				}
			}
			contentY += ContentGap;
			if (drawText && panel.Text != null && panel.Text.Length > 0 && formatStyle.GetForeBrush() != null)
			{
				Rectangle rectangle = Rectangle.FromLTRB(bounds.X, contentY, bounds.Right, bounds.Bottom);
				if (rectangle.Height > 0)
				{
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
					textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
					textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					textLayoutRenderer.SetVertical(true);
					textLayoutRenderer.SetWordWrap(false);
					textLayoutRenderer.DrawTextWithBrush(graphics, panel.Text, rectangle, font, formatStyle.GetForeBrush());
					textLayoutRenderer.ResetCache();
				}
			}
		}
	}
}
