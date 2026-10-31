using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		internal Image GetTabImage(MxTabPage page)
		{
			if (page.Image != null)
			{
				return page.Image;
			}
			if (ImageList != null)
			{
				try
				{
					if (page.ImageKey.Length > 0 && ImageList.Images.ContainsKey(page.ImageKey))
					{
						return ImageList.Images[page.ImageKey];
					}
					if (page.ImageIndex >= 0 && page.ImageIndex < ImageList.Images.Count)
					{
						return ImageList.Images[page.ImageIndex];
					}
				}
				catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
				{
					// The image list can change while a tab paints; draw the tab without an image.
					return null;
				}
			}
			return null;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (TabCloseButtonsShown != _tabCloseLayout)
			{
				_isLayoutDirty = true;
			}
			if (_isLayoutDirty)
			{
				LayoutTabs(e.Graphics);
			}
			if (IsThemed)
			{
				PaintThemedControl(e.Graphics);
				base.OnPaint(e);
				return;
			}
			if ((VisualStyleSupport.IsThemingUsable(false)))
			{
				SetThemeRendered(true);
				if (_themePainter == null)
				{
					_themePainter = GetThemePainter();
				}
			}
			else
			{
				SetThemeRendered(false);
			}
			StateStyles stateStyles = CreateTabStateStyles(false);
			if (_themePainter == null)
			{
				PaintStripBackground(e.Graphics, e.ClipRectangle);
			}
			if (!HasVisibleTabs())
			{
				if (!_isThemeRendered)
				{
					FormatStyle panelFormatStyle = PanelFormatStyle;
					RenderingHelper.DrawFormatStyleBackground(e.Graphics, panelFormatStyle, e.ClipRectangle, base.ClientRectangle);
					panelFormatStyle.DisposeResources();
				}
				PaintPageBorder(e.Graphics, base.ClientRectangle, stateStyles);
				stateStyles.DisposeFormatStyleResources();
				_themePainter = null;
				return;
			}
			PaintPageBorder(e.Graphics, _pageDisplayBounds, stateStyles);
			if (!_tabStripBounds.IsEmpty)
			{
				Rectangle rectangle = _tabStripBounds;
				rectangle.Inflate(2, 2);
				if (e.Graphics.ClipBounds.IntersectsWith(rectangle))
				{
					PaintTabs(e.Graphics, stateStyles);
					PaintStripButtons(e.Graphics);
				}
			}
			stateStyles.DisposeFormatStyleResources();
			_themePainter = null;
			base.OnPaint(e);
		}

		private bool ShouldDrawText(TabPageRenderer tab)
		{
			if (_tabDisplay == TabDisplay.Image)
			{
				return false;
			}
			if (_tabDisplay == TabDisplay.ImageAndTextOnSelected)
			{
				return tab.IsPressed();
			}
			return true;
		}

		private int GetTabState(TabPageRenderer tab)
		{
			if (tab.IsPressed())
			{
				return TabStateSelected;
			}
			if (!((FormatStyleLayoutState)tab).IsEnabled())
			{
				return TabStateDisabled;
			}
			if (tab.IsHot())
			{
				return TabStateHot;
			}
			return TabStateNormal;
		}

		private TextLayoutRenderer CreateTabTextRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.SetNoPadding(true);
			if (!_isTextHorizontal)
			{
				textLayoutRenderer.SetVertical(true);
			}
			textLayoutRenderer.SetShowHotkeyPrefix(true);
			return textLayoutRenderer;
		}

		private void PaintThemedTabs(Graphics graphics, StateStyles baseStyles)
		{
			if (_themePainter == null)
			{
				_themePainter = GetThemePainter();
			}
			Rectangle tabBounds = Rectangle.Empty;
			TabPageRenderer selectedTabRenderer = null;
			TextLayoutRenderer textLayoutRenderer = CreateTabTextRenderer();
			StateStyles tabStateStyles = null;
			FormatStyle tabFormatStyle = null;
			foreach (TabPageRenderer item in _pageRenderers)
			{
				tabBounds = item._bounds;
				if (((FormatStyleLayoutState)item).IsVisible() && !Rectangle.Intersect(_tabStripBounds, tabBounds).IsEmpty)
				{
					int tabState = GetTabState(item);
					if (item.IsPressed())
					{
						selectedTabRenderer = item;
						continue;
					}
					_themePainter.DrawAlignedTabThemeBackground(graphics, tabBounds, _tabStripAlignment, TabPartDefault, tabState, false);
					tabStateStyles = baseStyles.CreateMergedStateStyles(item.GetTabPage().StateStyles);
					tabFormatStyle = CreateTabFormatStyle(tabStateStyles, ((FormatStyleLayoutState)item).IsEnabled(), item.IsHot(), item.IsPressed());
					PaintTabContent(graphics, item, tabFormatStyle.ResolveFont(Font), textLayoutRenderer, tabFormatStyle.GetForeBrush(), false, false, SystemColors.Menu);
					tabStateStyles.DisposeFormatStyleResources();
					tabFormatStyle.DisposeResources();
				}
			}
			if (selectedTabRenderer == null)
			{
				return;
			}
			bool isTrimmed = false;
			int selectedState = GetTabState(selectedTabRenderer);
			tabBounds = selectedTabRenderer._bounds;
			int tabPart = TabPartDefault;
			if (_isHorizontalStrip)
			{
				if (tabBounds.X == base.ClientRectangle.X)
				{
					tabPart = TabPartFirst;
				}
				if (tabBounds.Right == base.ClientRectangle.Right)
				{
					if (tabPart == TabPartFirst)
					{
						tabBounds.Width -= 2;
						isTrimmed = true;
					}
					else
					{
						tabPart = TabPartLast;
					}
				}
			}
			else
			{
				if (tabBounds.Y == base.ClientRectangle.Y)
				{
					tabPart = TabPartFirst;
				}
				if (tabBounds.Bottom == base.ClientRectangle.Bottom)
				{
					if (tabPart == TabPartFirst)
					{
						tabBounds.Height -= 2;
						isTrimmed = true;
					}
					else
					{
						tabPart = TabPartLast;
					}
				}
			}
			_themePainter.DrawAlignedTabThemeBackground(graphics, tabBounds, _tabStripAlignment, tabPart, selectedState, isTrimmed);
			tabStateStyles = baseStyles.CreateMergedStateStyles(selectedTabRenderer.GetTabPage().StateStyles);
			tabFormatStyle = CreateTabFormatStyle(tabStateStyles, ((FormatStyleLayoutState)selectedTabRenderer).IsEnabled(), selectedTabRenderer.IsHot(), true);
			PaintTabContent(graphics, selectedTabRenderer, tabFormatStyle.ResolveFont(Font), textLayoutRenderer, tabFormatStyle.GetForeBrush(), false, true, SystemColors.Menu);
			tabFormatStyle.DisposeResources();
			tabStateStyles.DisposeFormatStyleResources();
		}

		private void PaintTabContent(Graphics graphics, TabPageRenderer tab, Font font, TextLayoutRenderer textLayoutRenderer, Brush brush, bool isFlat, bool isSelected, Color backColor)
		{
			if (!tab._imageBounds.IsEmpty && _tabDisplay != TabDisplay.Text)
			{
				Size imageSize = Size.Empty;
				Rectangle imageBounds = tab._imageBounds;
				if (!_imageSize.IsEmpty)
				{
					imageSize = _imageSize;
				}
				else if (tab.GetTabPage().Icon != null)
				{
					imageSize = tab.GetTabPage().Icon.Size;
				}
				else if (tab._itemImage != null)
				{
					imageSize = tab._itemImage.Size;
				}
				imageBounds.X += (imageBounds.Width - imageSize.Width) / 2;
				imageBounds.Y += (imageBounds.Height - imageSize.Height) / 2;
				if (_tabStripAlignment == TabStripAlignment.Bottom)
				{
					imageBounds.Y--;
				}
				if (_tabStripAlignment == TabStripAlignment.Left)
				{
					imageBounds.X--;
				}
				imageBounds.Size = imageSize;
				int imageIndex = -1;
				if (tab.GetTabPage().Image == null && ImageList != null)
				{
					if (tab.GetTabPage().ImageKey.Length > 0 && ImageList.Images.ContainsKey(tab.GetTabPage().ImageKey))
					{
						imageIndex = ImageList.Images.IndexOfKey(tab.GetTabPage().ImageKey);
					}
					else if (tab.GetTabPage().ImageIndex >= 0 && tab.GetTabPage().ImageIndex < ImageList.Images.Count)
					{
						imageIndex = tab.GetTabPage().ImageIndex;
					}
				}
				RenderingHelper.DrawStateImage(graphics, tab.GetTabPage().Icon, tab._itemImage, imageIndex, ImageList, imageBounds, ((FormatStyleLayoutState)tab).IsEnabled(), tab.IsHot(), isSelected, backColor, true, null);
			}
			if (ShouldDrawText(tab) && brush != null && !tab._textBounds.IsEmpty)
			{
				Rectangle textBounds = tab._textBounds;
				textLayoutRenderer.DrawTextWithBrush(graphics, tab._caption, textBounds, font, brush);
			}
			if (!_showFocusRectangle || !Focused || tab.GetTabPage() != InputFocusTab)
			{
				return;
			}
			Rectangle focusBounds = tab._bounds;
			focusBounds.Inflate(-3, -3);
			Pen pen = ((brush == null) ? new Pen(GetEffectiveForeColor()) : new Pen(brush));
			pen.DashStyle = DashStyle.Dot;
			RenderingHelper.DrawRectangleOutline(graphics, pen, focusBounds);
			pen.Dispose();
			pen = null;
		}

		private void PaintStripButtons(Graphics graphics)
		{
			if (_closeButton != null)
			{
				if (_isThemeRendered)
				{
					_themePainter.DrawCloseButtonTheme(graphics, _closeButton._bounds, _closeButton.IsHot(), _closeButton.IsPressed(), base.Enabled && _closeButton.IsEnabled());
				}
				else
				{
					PaintStripButton(graphics, _closeButton._bounds, true, -1, _isTextHorizontal, _closeButton.IsHot(), _closeButton.IsPressed(), base.Enabled && _closeButton.IsEnabled());
				}
			}
			if (_scrollBackButton == null)
			{
				return;
			}
			if (_isThemeRendered)
			{
				if (_isHorizontalStrip)
				{
					_themePainter.DrawScrollLeftArrowTheme(graphics, _scrollBackButton._bounds, _scrollBackButton.IsHot(), _scrollBackButton.IsPressed(), base.Enabled && _scrollBackButton.IsEnabled());
					_themePainter.DrawScrollRightArrowTheme(graphics, _scrollForwardButton._bounds, _scrollForwardButton.IsHot(), _scrollForwardButton.IsPressed(), base.Enabled && _scrollForwardButton.IsEnabled());
				}
				else
				{
					_themePainter.DrawScrollUpArrowTheme(graphics, _scrollBackButton._bounds, _scrollBackButton.IsHot(), _scrollBackButton.IsPressed(), base.Enabled && _scrollBackButton.IsEnabled());
					_themePainter.DrawScrollDownArrowTheme(graphics, _scrollForwardButton._bounds, _scrollForwardButton.IsHot(), _scrollForwardButton.IsPressed(), base.Enabled && _scrollForwardButton.IsEnabled());
				}
				return;
			}
			PaintStripButton(graphics, _scrollBackButton._bounds, false, -1, _isHorizontalStrip, _scrollBackButton.IsHot(), _scrollBackButton.IsPressed(), base.Enabled && _scrollBackButton.IsEnabled());
			PaintStripButton(graphics, _scrollForwardButton._bounds, false, 1, _isHorizontalStrip, _scrollForwardButton.IsHot(), _scrollForwardButton.IsPressed(), base.Enabled && _scrollForwardButton.IsEnabled());
		}

		private void PaintStripButton(Graphics graphics, Rectangle buttonBounds, bool isCloseButton, int direction, bool isHorizontal, bool isHot, bool isPressed, bool isEnabled)
		{
			StateStyles buttonStateStyles = CreateTabStateStyles(true);
			FormatStyle buttonFormatStyle = null;
			if (_themePainter != null)
			{
				if (isHot || isPressed)
				{
					_themePainter.DrawButtonBackground(graphics, buttonBounds, Color.Empty, isHot, isPressed, isEnabled, false, true, _isHorizontalStrip);
				}
			}
			else
			{
				buttonFormatStyle = buttonStateStyles.FormatStyle;
				if (isHot && isEnabled)
				{
					buttonFormatStyle = ((!isPressed) ? buttonStateStyles.HotFormatStyle : buttonStateStyles.PressedFormatStyle);
				}
				if (buttonFormatStyle.BackColor.IsEmpty)
				{
					buttonFormatStyle.BackColor = GetEffectiveBackColor();
					if (buttonFormatStyle.BackColor.Equals(Color.Transparent))
					{
						buttonFormatStyle.BackColor = SystemColors.Control;
					}
				}
				Brush brush = buttonFormatStyle.GetBackgroundBrush(buttonBounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, buttonBounds);
				}
				buttonFormatStyle.DisposeResources();
			}
			int glyphHeight = buttonBounds.Height + 1;
			int glyphWidth = buttonBounds.Width + 1;
			Color color = RenderingHelper.GetSchemeColor(ColorName.Control);
			buttonBounds = ((isPressed && isEnabled) ? (RenderingHelper.DrawColoredBorder(graphics, color, buttonBounds, BorderStyle.SunkenLight3D)) : (RenderingHelper.DrawColoredBorder(graphics, color, buttonBounds, BorderStyle.RaisedLight3D)));
			Color glyphColor = ((!isEnabled) ? RenderingHelper.GetSchemeColor(ColorName.GrayText) : (RenderingHelper.GetSchemeColor(ColorName.ControlText)));
			if (isCloseButton)
			{
				Image closeGlyph = RenderingHelper.MakeMagentaTransparent(WindowsResources.CloseImage());
				if (closeGlyph != null)
				{
					RenderingHelper.DrawRecoloredImage(graphics, closeGlyph, buttonBounds, glyphColor);
				}
				return;
			}
			if (isHorizontal)
			{
				Pen horizontalGlyphPen = GdiCache.Pen(glyphColor);
				RenderingHelper.DrawSideTriangle(graphics, glyphHeight, buttonBounds, direction, horizontalGlyphPen);
				return;
			}
			Pen verticalGlyphPen = GdiCache.Pen(glyphColor);
			RenderingHelper.DrawUpDownTriangle(graphics, glyphWidth, buttonBounds, direction, verticalGlyphPen);
		}

		internal StateStyles CreateTabStateStyles(bool isForButtons)
		{
			StateStyles stateStyles = new StateStyles();
			stateStyles.FormatStyle.ForeColor = GetEffectiveForeColor();
			return stateStyles.CreateMergedStateStyles(TabsStateStyles);
		}

		internal FormatStyle CreateTabFormatStyle(StateStyles stateStyles, bool isEnabled, bool isHot, bool isSelected)
		{
			if (!base.Enabled)
			{
				isEnabled = false;
			}
			FormatStyle formatStyle = new FormatStyle(stateStyles.FormatStyle);
			if (!isEnabled)
			{
				formatStyle = formatStyle.MergeStyle(stateStyles.DisabledFormatStyle);
			}
			else
			{
				if (isSelected)
				{
					formatStyle = formatStyle.MergeStyle(stateStyles.SelectedFormatStyle);
				}
				if (isHot)
				{
					formatStyle = formatStyle.MergeStyle(stateStyles.HotFormatStyle);
				}
				if (isSelected && isHot)
				{
					formatStyle = formatStyle.MergeStyle(stateStyles.PressedFormatStyle);
				}
			}
			if (formatStyle.ForeColor.IsEmpty)
			{
				formatStyle.ForeColor = GetEffectiveForeColor();
			}
			return formatStyle;
		}

		private void PaintTabJoinLine(Graphics graphics, Rectangle tabBounds, bool isThin)
		{
			Pen pen = null;
			pen = GdiCache.Pen(GetEffectiveBackColor());
			int edgeOffset = 2;
			int endInset = 2;
			if (isThin)
			{
				edgeOffset = 1;
				endInset = 1;
			}
			switch (_tabStripAlignment)
			{
			case TabStripAlignment.Top:
				graphics.DrawLine(pen, tabBounds.Left + endInset, tabBounds.Bottom - edgeOffset, tabBounds.Right - endInset - 1, tabBounds.Bottom - edgeOffset);
				break;
			case TabStripAlignment.Left:
				graphics.DrawLine(pen, tabBounds.Right - edgeOffset, tabBounds.Top + endInset, tabBounds.Right - edgeOffset, tabBounds.Bottom - endInset - 1);
				break;
			case TabStripAlignment.Bottom:
				if (!isThin)
				{
					edgeOffset--;
				}
				graphics.DrawLine(pen, tabBounds.Left + endInset, tabBounds.Top + edgeOffset, tabBounds.Right - endInset - 1, tabBounds.Top + edgeOffset);
				break;
			case TabStripAlignment.Right:
				if (!isThin)
				{
					edgeOffset--;
				}
				graphics.DrawLine(pen, tabBounds.Left + edgeOffset, tabBounds.Top + endInset, tabBounds.Left + edgeOffset, tabBounds.Bottom - endInset - 1);
				break;
			}
			pen = null;
		}

		private void PaintStripBackground(Graphics graphics, Rectangle clipBounds)
		{
			if (!(BackColor != Color.Transparent))
			{
				return;
			}
			FormatStyle stripFormatStyle = new FormatStyle(TabStripFormatStyle);
			if (_themePainter == null && stripFormatStyle.BackColor.IsEmpty)
			{
				stripFormatStyle.BackColor = GetEffectiveBackColor();
			}
			Rectangle stripBounds = base.ClientRectangle;
			switch (TabStripAlignment)
			{
			case TabStripAlignment.Top:
				stripBounds.Height = _tabStripBounds.Bottom;
				break;
			case TabStripAlignment.Bottom:
				stripBounds.Y = _tabStripBounds.Top;
				stripBounds.Height = base.ClientRectangle.Bottom - _tabStripBounds.Top;
				break;
			case TabStripAlignment.Left:
				stripBounds.Width = _tabStripBounds.Right;
				break;
			case TabStripAlignment.Right:
				stripBounds.X = _tabStripBounds.Left;
				stripBounds.Width = base.ClientRectangle.Right - _tabStripBounds.Left;
				break;
			}
			RenderingHelper.DrawFormatStyleBackground(graphics, stripFormatStyle, clipBounds, stripBounds);
		}

		private void PaintTabs(Graphics graphics, StateStyles baseStyles)
		{
			if (_isThemeRendered)
			{
				PaintThemedTabs(graphics, baseStyles);
				return;
			}
			Rectangle stripClipBounds = _tabStripBounds;
			stripClipBounds.Inflate(2, 2);
			Region region = new Region(stripClipBounds);
			Region clip = graphics.Clip;
			graphics.Clip = region;
			TextLayoutRenderer textLayoutRenderer = CreateTabTextRenderer();
			Rectangle tabBounds = Rectangle.Empty;
			Rectangle selectedBounds = Rectangle.Empty;
			FormatStyle tabFormatStyle = null;
			FormatStyle selectedFormatStyle = null;
			Brush noBrush = null;
			Brush textBrush = null;
			StateStyles tabStateStyles = null;
			Color stripBackColor = GetEffectiveBackColor();
			if (stripBackColor == Color.Transparent)
			{
				stripBackColor = GetAncestorBackColor();
				if (stripBackColor == SystemColors.Control)
				{
					stripBackColor = RenderingHelper.GetSchemeColor(ColorName.Control);
				}
			}
			for (int i = 0; i < _pageRenderers.Count; i++)
			{
				TabPageRenderer tab = (TabPageRenderer)_pageRenderers.FindItemByIndex(i);
				tabBounds = tab._bounds;
				if (!((FormatStyleLayoutState)tab).IsVisible() || Rectangle.Intersect(_tabStripBounds, tabBounds).IsEmpty)
				{
					continue;
				}
				tabStateStyles = baseStyles.CreateMergedStateStyles(tab.GetTabPage().StateStyles);
				tabFormatStyle = CreateTabFormatStyle(tabStateStyles, ((FormatStyleLayoutState)tab).IsEnabled(), tab.IsHot(), tab.IsPressed());
				textBrush = tabFormatStyle.GetForeBrush();
				Rectangle tabShapeBounds = RenderingHelper.DrawTabBorder(graphics, _tabStripAlignment, tabFormatStyle.BackColor, stripBackColor, tabBounds, base.ClientRectangle, false, _isHorizontalStrip, noBrush);
				RenderingHelper.DrawFormatStyleBackground(graphics, tabFormatStyle, stripClipBounds, tabShapeBounds);
				if (tab.IsPressed())
				{
					selectedBounds = tabBounds;
					selectedFormatStyle = tabFormatStyle;
				}
				PaintTabContent(graphics, tab, tabFormatStyle.ResolveFont(Font), textLayoutRenderer, textBrush, false, tab.IsPressed(), tabFormatStyle.BackColor);
				tabStateStyles.DisposeFormatStyleResources();
				tabFormatStyle.DisposeResources();
			}
			if (selectedBounds != Rectangle.Empty)
			{
				RenderingHelper.DrawTabBorder(graphics, _tabStripAlignment, selectedFormatStyle.BackColor, stripBackColor, selectedBounds, base.ClientRectangle, true, _isHorizontalStrip, selectedFormatStyle.GetBackgroundBrush(selectedBounds));
				if (selectedFormatStyle.BackColor.IsEmpty)
				{
					PaintTabJoinLine(graphics, selectedBounds, false);
				}
				selectedFormatStyle.DisposeResources();
			}
			graphics.Clip = clip;
			clip.Dispose();
			region.Dispose();
			region = null;
			textLayoutRenderer.ResetCache();
		}

		// Returns the first parent color that is not transparent, or Transparent when there is none.
		private Color GetAncestorBackColor()
		{
			Color color = Color.Transparent;
			for (Control control = RenderingHelper.GetParentControl(this); control != null; control = RenderingHelper.GetParentControl(control))
			{
				color = control.BackColor;
				if (color != Color.Transparent)
				{
					break;
				}
			}
			return color;
		}

		private void PaintPageBorder(Graphics graphics, Rectangle pageBounds, StateStyles baseStyles)
		{
			if (_isThemeRendered)
			{
				if (_themePainter == null)
				{
					_themePainter = GetThemePainter();
				}
				Bitmap bitmap = null;
				RenderingHelper.DrawThemedTabBackground(graphics, ref bitmap, pageBounds, pageBounds, _tabStripAlignment, VisualStyleElement.Tab.Pane.Normal);
				if (bitmap != null)
				{
					bitmap.Dispose();
					bitmap = null;
				}
			}
			else
			{
				if (_pageBorder == PageBorder.None)
				{
					return;
				}
				TabPageRenderer selectedTabRenderer = _pageRenderers.FindTabPageItem(_selectedTab);
				FormatStyle pageFormatStyle = null;
				StateStyles pageStateStyles = baseStyles;
				if (selectedTabRenderer != null)
				{
					pageStateStyles = pageStateStyles.CreateMergedStateStyles(selectedTabRenderer.GetTabPage().StateStyles);
					pageFormatStyle = CreateTabFormatStyle(pageStateStyles, ((FormatStyleLayoutState)selectedTabRenderer).IsEnabled(), false, false);
				}
				else
				{
					pageFormatStyle = CreateTabFormatStyle(pageStateStyles, base.Enabled, false, false);
				}
				BorderStyle borderStyle = BorderStyle.Raised;
				Color pageBackColor = pageFormatStyle.BackColor;
				if (pageBackColor.IsEmpty)
				{
					pageBackColor = GetEffectiveBackColor();
					if (pageBackColor == Color.Transparent)
					{
						pageBackColor = GetAncestorBackColor();
					}
				}
				RenderingHelper.DrawColoredBorder(graphics, pageBackColor, pageBounds, borderStyle);
			}
		}
	}
}
