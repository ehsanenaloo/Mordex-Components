using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	// Paints the panels, tabs, strips and caption buttons from the default theme. Colors come from the color table of
	// the themed look, so a color the developer set on a style still wins. It builds no brush, pen or path.
	internal class ThemedDockRenderer : DockManagerRendererBase
	{
		private const int ContentGap = 4;

		private const int SeparatorInset = 3;

		private const int GripWidth = 35;

		private const int GripDotCount = 9;

		private const int GripDotSpacing = 4;

		private const int AccentWidth = 2;

		internal ThemedDockRenderer(MxDockManager manager)
			: base(manager, createScheme: false)
		{
			_colorSchemeAppearance = new MxColorSchemeAppearanceState();
		}

		private ResolvedTheme Theme
		{
			get
			{
				Control owner = GetDockManager().ContainerControl;
				return MxThemeManager.ResolveDefault(owner == null ? 96 : owner.DeviceDpi);
			}
		}

		private TextLayoutRenderer CreateLayout()
		{
			TextLayoutRenderer layout = new TextLayoutRenderer(!GetDockManager().UseCompatibleTextRendering);
			Control owner = GetDockManager().ContainerControl;
			if (owner != null)
			{
				layout.SetRightToLeft(owner.RightToLeft);
			}
			return layout;
		}

		private static void Fill(Graphics graphics, Color color, Rectangle bounds)
		{
			if (!color.IsEmpty && color.A > 0 && bounds.Width > 0 && bounds.Height > 0)
			{
				graphics.FillRectangle(GdiCache.Brush(color), bounds);
			}
		}

		private static readonly ThemeRole[] BandRoles = { ThemeRole.Caption, ThemeRole.CaptionInactive, ThemeRole.Header, ThemeRole.TabActive, ThemeRole.TabInactive, ThemeRole.TabHot, ThemeRole.SurfaceHot, ThemeRole.ToolbarBack };

		// A color that is a role of the theme takes the face style of the theme (a gradient); any other color stays flat.
		private void FillStyled(Graphics graphics, Color color, Rectangle bounds)
		{
			ResolvedTheme theme = Theme;
			if (!theme.Style.CaptionEnd.IsEmpty && !color.IsEmpty && color.A == 255)
			{
				int wanted = color.ToArgb();
				if (theme.Color(ThemeRole.Caption).ToArgb() == wanted && theme.FillCaption(graphics, bounds, ThemeRole.Caption))
				{
					return;
				}
				if (theme.Color(ThemeRole.CaptionInactive).ToArgb() == wanted && theme.FillCaption(graphics, bounds, ThemeRole.CaptionInactive))
				{
					return;
				}
			}
			if (theme.Style.FaceGradient != ThemeGradient.None && !color.IsEmpty && color.A == 255 && bounds.Width > 0 && bounds.Height > 0)
			{
				int argb = color.ToArgb();
				foreach (ThemeRole role in BandRoles)
				{
					if (theme.Color(role).ToArgb() == argb)
					{
						theme.FillBand(graphics, bounds, role);
						return;
					}
				}
			}
			Fill(graphics, color, bounds);
		}

		private static bool IsHot(ButtonState state)
		{
			return state == ButtonState.Hot || state == ButtonState.Pressed;
		}

		internal override void DrawInnerAreaBackground(Graphics graphics, Control sender, Rectangle rect, PanelInnerAreaStyle style)
		{
			MxDockManager manager = GetDockManager();
			switch (style)
			{
			case PanelInnerAreaStyle.AppWorkspace:
				Fill(graphics, manager.GetDefaultColor(PanelColors.InnerWorkspace), rect);
				break;
			case PanelInnerAreaStyle.ContainerPanel:
				Fill(graphics, manager.GetDefaultColor(PanelColors.InnerContainer), rect);
				break;
			case PanelInnerAreaStyle.Window:
				Fill(graphics, manager.GetDefaultColor(PanelColors.InnerWindow), rect);
				break;
			}
		}

		public override void DrawFloatingBorder(Graphics graphics, DockPanelBase panel, Rectangle borderRect)
		{
			ResolvedTheme theme = Theme;
			MxFloatingWindow window = panel?.FloatingWindow;
			bool active = window != null && window.IsWindowActive;
			Color outer = active ? theme.Color(ThemeRole.Accent) : GetColorSchemeAppearance().GetColor(PanelColors.FloatingWindowBorder);
			graphics.DrawRectangle(GdiCache.Pen(outer), borderRect.X, borderRect.Y, borderRect.Width - 1, borderRect.Height - 1);
			graphics.DrawRectangle(theme.Pen(ThemeRole.Border), borderRect.X + 1, borderRect.Y + 1, borderRect.Width - 3, borderRect.Height - 3);
			if (window != null && window.HotEdge != ContentAlignment.MiddleCenter)
			{
				DrawHotEdge(graphics, theme, borderRect, window.HotEdge, window.CornerSize);
			}
		}

		// The resize edge under the pointer shows in the accent color; a corner lights both of its sides.
		private static void DrawHotEdge(Graphics graphics, ResolvedTheme theme, Rectangle bounds, ContentAlignment edge, int corner)
		{
			int line = theme.Scaled(3);
			Brush brush = theme.Brush(ThemeRole.Accent);
			bool left = edge == ContentAlignment.TopLeft || edge == ContentAlignment.MiddleLeft || edge == ContentAlignment.BottomLeft;
			bool right = edge == ContentAlignment.TopRight || edge == ContentAlignment.MiddleRight || edge == ContentAlignment.BottomRight;
			bool top = edge == ContentAlignment.TopLeft || edge == ContentAlignment.TopCenter || edge == ContentAlignment.TopRight;
			bool bottom = edge == ContentAlignment.BottomLeft || edge == ContentAlignment.BottomCenter || edge == ContentAlignment.BottomRight;
			bool isCorner = (left || right) && (top || bottom);
			int alongX = isCorner ? corner : bounds.Width;
			int alongY = isCorner ? corner : bounds.Height;
			if (top)
			{
				graphics.FillRectangle(brush, left || !isCorner ? bounds.X : bounds.Right - alongX, bounds.Y, alongX, line);
			}
			if (bottom)
			{
				graphics.FillRectangle(brush, left || !isCorner ? bounds.X : bounds.Right - alongX, bounds.Bottom - line, alongX, line);
			}
			if (left)
			{
				graphics.FillRectangle(brush, bounds.X, top || !isCorner ? bounds.Y : bounds.Bottom - alongY, line, alongY);
			}
			if (right)
			{
				graphics.FillRectangle(brush, bounds.Right - line, top || !isCorner ? bounds.Y : bounds.Bottom - alongY, line, alongY);
			}
		}

		public override void DrawBackground(Graphics graphics, Control sender, Rectangle rect)
		{
			Fill(graphics, GetDockManager().BackColorSplitter, rect);
		}

		public override void DrawPanelButton(Graphics graphics, PanelCaptionStyle titleStyle, DockPanelBase panel, PanelButtonType buttonType, Rectangle buttonBounds, ButtonState state, Font font, FormatStyle captionFormatStyle, Image buttonImage)
		{
			ResolvedTheme theme = Theme;
			bool pressed = (state & ButtonState.Pressed) == ButtonState.Pressed;
			bool hot = (state & ButtonState.Hot) == ButtonState.Hot;
			bool disabled = state == ButtonState.Disabled;
			Color fore = disabled ? theme.Color(ThemeRole.TextDisabled) : captionFormatStyle.ForeColor;
			if ((pressed || hot) && buttonBounds.Width > 0 && buttonBounds.Height > 0)
			{
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				theme.FillRounded(graphics, buttonBounds, pressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, theme.Radius(ThemeElement.Button));
				graphics.SmoothingMode = smoothing;
			}
			switch (buttonType)
			{
			case PanelButtonType.ScrollLeft:
				buttonBounds.Y++;
				RenderingHelper.DrawHorizontalArrow(graphics, buttonBounds.Height, buttonBounds, -1, GdiCache.Pen(fore), !disabled);
				buttonBounds.Y--;
				break;
			case PanelButtonType.ScrollRight:
				buttonBounds.Y++;
				RenderingHelper.DrawHorizontalArrow(graphics, buttonBounds.Height, buttonBounds, 1, GdiCache.Pen(fore), !disabled);
				buttonBounds.Y--;
				break;
			default:
				if (buttonImage != null)
				{
					Rectangle glyph = buttonBounds;
					glyph.X = glyph.Right - glyph.Height;
					glyph.Width = glyph.Height;
					RenderingHelper.DrawRecoloredImage(graphics, buttonImage, glyph, fore);
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
		}

		public override void DrawTabStrip(Graphics graphics, Control sender, Rectangle bounds, FormatStyle formatStyle, Color borderColor, PanelDockStyle dockStyle)
		{
			FillStyled(graphics, formatStyle.BackColor, bounds);
			Pen pen = GdiCache.Pen(borderColor);
			if (dockStyle == PanelDockStyle.Top)
			{
				graphics.DrawLine(pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			}
			else
			{
				graphics.DrawLine(pen, bounds.X, bounds.Top, bounds.Right - 1, bounds.Top);
			}
		}

		public override void DrawTabHorizontalBackground(Graphics graphics, DockPanelBase panel, TabAlignment alignment, FormatStyle formatStyle, ButtonState state, Rectangle bounds)
		{
			ResolvedTheme theme = Theme;
			bool selected = state == ButtonState.Selected || state == ButtonState.Pressed;
			Color back = formatStyle.BackColor;
			if (back.IsEmpty && IsHot(state))
			{
				back = theme.Color(ThemeRole.TabHot);
			}
			if (theme.DrawStripTab(graphics, bounds, selected, IsHot(state), alignment == TabAlignment.Bottom))
			{
				return;
			}
			FillStyled(graphics, back, bounds);
			if (selected)
			{
				int line = theme.Scaled(AccentWidth);
				Rectangle accent = alignment == TabAlignment.Bottom ? new Rectangle(bounds.X, bounds.Y, bounds.Width, line) : new Rectangle(bounds.X, bounds.Bottom - line, bounds.Width, line);
				graphics.FillRectangle(theme.Brush(ThemeRole.Accent), accent);
			}
		}

		public override void DrawTabHorizontalForeground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, ButtonState state, Rectangle bounds)
		{
			Font font = panel.GetCaptionFont(formatStyle);
			int contentX = bounds.X;
			CommandImageSource image = panel.GetImageSource();
			if (!image.IsEmpty())
			{
				Size size = GetDockManager().GetPanelImageSize(image);
				contentX += ContentGap;
				DrawCommandImage(graphics, image, contentX, bounds.Y + (bounds.Height - size.Height) / 2, size, panel.Enabled);
				contentX += size.Width;
			}
			contentX += ContentGap;
			if (!string.IsNullOrEmpty(panel.Text) && !formatStyle.ForeColor.IsEmpty)
			{
				Rectangle label = Rectangle.FromLTRB(contentX, bounds.Y, bounds.Right, bounds.Bottom);
				if (label.Width > 0)
				{
					TextLayoutRenderer layout = CreateLayout();
					layout.SetLineAlignment(StringAlignment.Center);
					layout.SetStringTrimming(StringTrimming.EllipsisCharacter);
					layout.SetWordWrap(false);
					layout.DrawTextWithBrush(graphics, panel.Text, label, font, GdiCache.Brush(formatStyle.ForeColor));
					layout.ResetCache();
				}
			}
		}

		public override void DrawMdiTabBackground(Graphics graphics, MxMdiTabGroup tabControl, FormatStyle formatStyle, ButtonState state, Rectangle bounds, bool isSelectedTab)
		{
			ResolvedTheme theme = Theme;
			Color back = formatStyle.BackColor;
			if (back.IsEmpty && IsHot(state))
			{
				back = theme.Color(ThemeRole.TabHot);
			}
			FillStyled(graphics, back, bounds);
			if (isSelectedTab)
			{
				graphics.FillRectangle(theme.Brush(ThemeRole.Accent), bounds.X, bounds.Y, bounds.Width, theme.Scaled(AccentWidth));
			}
			else
			{
				Pen separator = GdiCache.Pen(tabControl.PanelManager.TabbedMdiSettings.BorderColor);
				graphics.DrawLine(separator, bounds.Right - 1, bounds.Y + SeparatorInset, bounds.Right - 1, bounds.Bottom - SeparatorInset);
			}
		}

		public override void DrawMdiTabForeground(Graphics graphics, MxMdiTabGroup tabControl, MdiChildTab tab, FormatStyle formatStyle, ButtonState state, Rectangle bounds, Rectangle clipRect)
		{
			Font font = formatStyle.ResolveFont(tabControl.Font);
			int contentX = bounds.X + 2;
			CommandImageSource image = tab.CreateImageSource();
			Size size = tab.GetImageSize();
			if (!image.IsEmpty())
			{
				Rectangle imageBounds = new Rectangle(contentX, bounds.Y + (bounds.Height - size.Height) / 2 + 2, size.Width, size.Height);
				if (image._icon != null)
				{
					if (imageBounds.Right <= clipRect.Right)
					{
						RenderingHelper.DrawIcon(graphics, image._icon, imageBounds, true, Theme.Color(ThemeRole.Surface));
					}
				}
				else
				{
					image.DrawImage(graphics, imageBounds, true);
				}
				contentX += size.Width + 2;
			}
			string text = tab.Text;
			if (!string.IsNullOrEmpty(text) && !formatStyle.ForeColor.IsEmpty)
			{
				Rectangle label = Rectangle.FromLTRB(contentX, bounds.Y + 1, bounds.Right, bounds.Bottom);
				if (label.Width > 0)
				{
					TextLayoutRenderer layout = CreateLayout();
					layout.SetStringAlignment(StringAlignment.Near);
					layout.SetLineAlignment(StringAlignment.Center);
					layout.SetStringTrimming(tabControl.PanelManager.TabbedMdiSettings.TabCaptionTrimming);
					layout.SetWordWrap(false);
					layout.DrawTextWithBrush(graphics, text, label, font, GdiCache.Brush(formatStyle.ForeColor));
					layout.ResetCache();
				}
			}
		}

		public override void DrawMdiBackground(Graphics graphics, Control sender, Rectangle bounds, FormatStyle style)
		{
			Fill(graphics, style.BackColor, bounds);
		}

		public override void DrawPanelCaptionBackground(Graphics graphics, PanelCaptionStyle captionStyle, DockPanelBase panel, Rectangle captionBounds, FormatStyle captionFormatStyle)
		{
			FillStyled(graphics, captionFormatStyle.BackColor, captionBounds);
			if (panel != null && panel.Enabled && panel.IsCaptionActive())
			{
				ResolvedTheme theme = Theme;
				int line = theme.Scaled(AccentWidth);
				graphics.FillRectangle(theme.Brush(ThemeRole.Accent), captionBounds.X, captionBounds.Bottom - line, captionBounds.Width, line);
			}
		}

		public override void DrawPanelCaptionForeground(Graphics graphics, PanelCaptionStyle titleStyle, DockPanelBase panel, Rectangle textBounds, Font font, FormatStyle captionFormatStyle, FormatStyle infoTextStyle)
		{
			TextLayoutRenderer layout = CreateLayout();
			PanelTextAlignment alignment = panel.GetEffectiveTextAlignment();
			layout.SetStringAlignment(ToAlignment(alignment));
			layout.SetStringTrimming(StringTrimming.EllipsisCharacter);
			layout.SetLineAlignment(StringAlignment.Center);
			layout.SetWordWrap(false);
			string text = panel.Text;
			PanelCaptionDisplayMode mode = panel.GetEffectiveCaptionDisplayMode();
			if (mode == PanelCaptionDisplayMode.Image || mode == PanelCaptionDisplayMode.ImageAndText)
			{
				CommandImageSource image = panel.GetImageSource();
				if (!image.IsEmpty())
				{
					Size size = GetDockManager().GetPanelImageSize(image);
					if (panel.GetEffectiveImageAlignment() == PanelImageAlignment.Near)
					{
						DrawCommandImage(graphics, image, textBounds.X, textBounds.Top + (textBounds.Height - size.Height) / 2, size, panel.Enabled);
						textBounds.X += size.Width + ContentGap;
						textBounds.Width -= size.Width + ContentGap;
					}
					else
					{
						DrawCommandImage(graphics, image, textBounds.Right - size.Width - ContentGap, textBounds.Top + (textBounds.Height - size.Height) / 2, size, panel.Enabled);
						textBounds.Width -= size.Width + ContentGap;
					}
				}
			}
			if (mode == PanelCaptionDisplayMode.ImageAndText || mode == PanelCaptionDisplayMode.Text)
			{
				if (font != null && text.Length > 0 && !captionFormatStyle.ForeColor.IsEmpty)
				{
					layout.DrawTextWithBrush(graphics, text, textBounds, font, GdiCache.Brush(captionFormatStyle.ForeColor));
				}
				if (panel.InfoText.Length > 0 && alignment != PanelTextAlignment.Center)
				{
					int used = (int)layout.MeasureText(graphics, text, font, 0).Width + ContentGap;
					if (used < textBounds.Width)
					{
						if (alignment == PanelTextAlignment.Near)
						{
							textBounds.X += used;
						}
						textBounds.Width -= used + (alignment == PanelTextAlignment.Near ? ContentGap : 0);
					}
					else
					{
						textBounds = Rectangle.Empty;
					}
				}
				if (panel.InfoText.Length > 0 && textBounds.Width > 0 && infoTextStyle != null && !infoTextStyle.ForeColor.IsEmpty)
				{
					layout.SetStringAlignment(ToAlignment(panel.GetEffectiveInfoTextAlignment()));
					layout.DrawTextWithBrush(graphics, panel.InfoText, textBounds, infoTextStyle.ResolveFont(font), GdiCache.Brush(infoTextStyle.ForeColor));
				}
			}
			layout.ResetCache();
		}

		private static StringAlignment ToAlignment(PanelTextAlignment alignment)
		{
			switch (alignment)
			{
			case PanelTextAlignment.Center:
				return StringAlignment.Center;
			case PanelTextAlignment.Far:
				return StringAlignment.Far;
			default:
				return StringAlignment.Near;
			}
		}

		public override void DrawResizeBarBackground(Graphics graphics, DockPanelBase panel, Rectangle bounds)
		{
			Fill(graphics, GetDockManager().BackColorResizeBar, bounds);
		}

		public override void DrawResizeBarGrabHandle(Graphics graphics, DockPanelBase panel, Rectangle bounds)
		{
			Brush dots = Theme.Brush(ThemeRole.TextMuted);
			int x = bounds.Left + (bounds.Width - GripWidth) / 2;
			for (int i = 0; i < GripDotCount; i++)
			{
				graphics.FillRectangle(dots, x, bounds.Top + 2, 2, 2);
				x += GripDotSpacing;
			}
		}

		public override void DrawGrabItem(Graphics graphics, Rectangle bounds)
		{
			Brush dots = Theme.Brush(ThemeRole.TextMuted);
			Rectangle area = bounds;
			area.X += (area.Width - 4) / 2;
			area.Inflate(0, -5);
			while (area.Height > 4)
			{
				graphics.FillRectangle(dots, area.Left + 1, area.Y + 1, 2, 2);
				area.Y += 4;
				area.Height -= 4;
			}
		}

		public override void DrawBarGroupButtonBackground(Graphics graphics, DockPanelBase panel, Rectangle bounds, ButtonState state, FormatStyle formatStyle)
		{
			Color back = formatStyle.BackColor;
			if (back.IsEmpty && IsHot(state))
			{
				back = Theme.Color(ThemeRole.SurfaceHot);
			}
			FillStyled(graphics, back, bounds);
		}

		public override void DrawBarGroupButtonForeground(Graphics graphics, DockPanelBase panel, Rectangle bounds, string text, CommandImageSource image, ButtonState state, FormatStyle formatStyle, Font font)
		{
			TextLayoutRenderer layout = CreateLayout();
			layout.SetWordWrap(false);
			layout.SetStringTrimming(StringTrimming.EllipsisCharacter);
			layout.SetStringAlignment(StringAlignment.Near);
			layout.SetLineAlignment(StringAlignment.Center);
			int inset = panel.CanDragPanel() ? 12 : 8;
			Rectangle content = new Rectangle(bounds.X + inset, bounds.Y, bounds.Width - inset, bounds.Height);
			Rectangle label = content;
			if (image != null)
			{
				Size size = GetDockManager().GetLargeImageSizeFor(image);
				DrawCommandImage(graphics, image, content.X, content.Y + (content.Height - size.Height) / 2, size, panel.Enabled);
				label = Rectangle.FromLTRB(content.X + size.Width + 8, bounds.Top, bounds.Right, bounds.Bottom);
			}
			if (!formatStyle.ForeColor.IsEmpty && label.Width > 0)
			{
				layout.DrawTextWithBrush(graphics, panel.Text, label, font, GdiCache.Brush(formatStyle.ForeColor));
			}
			layout.ResetCache();
		}

		internal override void DrawAutoHideStripBackground(Graphics graphics, Control sender, Rectangle bounds, AutoHideStripDock stripDock)
		{
			FillStyled(graphics, GetDockManager().BackColorAutoHideStrip, bounds);
		}

		internal override void DrawAutoHideStripCorner(Graphics graphics, Control sender, Rectangle bounds, ContentAlignment cornerAlignment)
		{
			Fill(graphics, GetDockManager().BackColorAutoHideStrip, bounds);
		}

		private void DrawAutoHideTabBackground(Graphics graphics, FormatStyle formatStyle, ButtonState state, Rectangle bounds, ThemeRole accentSide, bool horizontal, bool outerFirst)
		{
			ResolvedTheme theme = Theme;
			Color back = formatStyle.BackColor;
			if (back.IsEmpty && IsHot(state))
			{
				back = theme.Color(ThemeRole.TabHot);
			}
			if (theme.DrawStripTab(graphics, bounds, state == ButtonState.Selected || state == ButtonState.Pressed, IsHot(state), outerFirst, !horizontal))
			{
				return;
			}
			Fill(graphics, back, bounds);
			if (state == ButtonState.Selected || state == ButtonState.Pressed)
			{
				int line = theme.Scaled(AccentWidth);
				Rectangle accent;
				if (horizontal)
				{
					accent = outerFirst ? new Rectangle(bounds.X, bounds.Y, bounds.Width, line) : new Rectangle(bounds.X, bounds.Bottom - line, bounds.Width, line);
				}
				else
				{
					accent = outerFirst ? new Rectangle(bounds.X, bounds.Y, line, bounds.Height) : new Rectangle(bounds.Right - line, bounds.Y, line, bounds.Height);
				}
				graphics.FillRectangle(theme.Brush(accentSide), accent);
			}
		}

		internal override void DrawAutoHideTabHorizontalBackground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, ButtonState state, Rectangle bounds, bool isTopStrip)
		{
			DrawAutoHideTabBackground(graphics, formatStyle, state, bounds, ThemeRole.Accent, horizontal: true, outerFirst: !isTopStrip);
		}

		internal override void DrawAutoHideTabVerticalBackground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, ButtonState state, Rectangle bounds, bool isLeftStrip)
		{
			DrawAutoHideTabBackground(graphics, formatStyle, state, bounds, ThemeRole.Accent, horizontal: false, outerFirst: !isLeftStrip);
		}

		internal override void DrawAutoHideTabHorizontalForeground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, ButtonState state, Rectangle bounds, bool drawText, bool drawImage)
		{
			int contentX = bounds.X;
			if (drawImage)
			{
				CommandImageSource image = panel.GetImageSource();
				if (!image.IsEmpty())
				{
					Size size = GetDockManager().GetPanelImageSize(image);
					contentX += ContentGap;
					DrawCommandImage(graphics, image, contentX, bounds.Y + (bounds.Height - size.Height) / 2, size, panel.Enabled);
					contentX += size.Width;
				}
			}
			contentX += ContentGap;
			DrawAutoHideText(graphics, panel, formatStyle, font, Rectangle.FromLTRB(contentX, bounds.Y, bounds.Right, bounds.Bottom), drawText, vertical: false);
		}

		internal override void DrawAutoHideTabVerticalForeground(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, ButtonState state, Rectangle bounds, bool drawText, bool drawImage)
		{
			int contentY = bounds.Y;
			if (drawImage)
			{
				CommandImageSource image = panel.GetImageSource();
				if (!image.IsEmpty())
				{
					Size size = GetDockManager().GetPanelImageSize(image);
					contentY += ContentGap;
					DrawCommandImage(graphics, image, bounds.X + (bounds.Width - size.Width) / 2, contentY, size, panel.Enabled);
					contentY += size.Height;
				}
			}
			contentY += ContentGap;
			DrawAutoHideText(graphics, panel, formatStyle, font, Rectangle.FromLTRB(bounds.X, contentY, bounds.Right, bounds.Bottom), drawText, vertical: true);
		}

		private void DrawAutoHideText(Graphics graphics, DockPanelBase panel, FormatStyle formatStyle, Font font, Rectangle label, bool drawText, bool vertical)
		{
			if (!drawText || string.IsNullOrEmpty(panel.Text) || formatStyle.ForeColor.IsEmpty || label.Width <= 0 || label.Height <= 0)
			{
				return;
			}
			TextLayoutRenderer layout = CreateLayout();
			layout.SetLineAlignment(StringAlignment.Center);
			layout.SetStringAlignment(StringAlignment.Near);
			layout.SetStringTrimming(StringTrimming.EllipsisCharacter);
			layout.SetVertical(vertical);
			layout.SetWordWrap(false);
			layout.DrawTextWithBrush(graphics, panel.Text, label, font, GdiCache.Brush(formatStyle.ForeColor));
			layout.ResetCache();
		}
	}
}
