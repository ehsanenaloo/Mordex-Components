using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonHeaderRenderer
	{
		private MxRibbon _ribbon;

		private Rectangle _area;

		internal IList<RibbonContextualTabRenderer> _contextualTabRenderers;

		private bool _areContextualTabsStale;

		internal bool _isLayoutDirty;

		private Size _borderSize;

		private Rectangle _applicationTextBounds;

		private Rectangle _documentTextBounds;

		private IntPtr _formHandle;

		private RibbonLayoutItem _hotItem;

		private RibbonLayoutItem _pushedItem;

		private RibbonLayoutItem _pushedCaptionButton;

		private RibbonLayoutItem _hotCaptionButton;

		private HeaderQuickAccessToolbarRenderer _quickAccessToolbar;

		private CommandBaseRenderer _controlBoxButtonRenderer;

		internal Rectangle _iconBounds;

		internal Font _captionFont;

		private Rectangle _headerBounds;

		public RibbonHeaderRenderer(MxRibbon ribbon)
		{
			_ribbon = ribbon;
			_areContextualTabsStale = true;
			_isLayoutDirty = true;
			_formHandle = IntPtr.Zero;
			_borderSize = Size.Empty;
		}

		private RibbonLayoutItem GetCloseButton()
		{
			if (_ribbon.HasWindowMessageHandler())
			{
				return _ribbon._ribbonWindowMessageHandler._closeButton;
			}
			return null;
		}

		private RibbonLayoutItem GetMinimizeButton()
		{
			if (_ribbon.HasWindowMessageHandler())
			{
				return _ribbon._ribbonWindowMessageHandler._minimizeButton;
			}
			return null;
		}

		private RibbonLayoutItem GetHelpButton()
		{
			if (_ribbon.HasWindowMessageHandler())
			{
				return _ribbon._ribbonWindowMessageHandler._helpButton;
			}
			return null;
		}

		private RibbonLayoutItem GetMaximizeButton()
		{
			if (_ribbon.HasWindowMessageHandler())
			{
				return _ribbon._ribbonWindowMessageHandler._maximizeButton;
			}
			return null;
		}

		public IntPtr GetFormHandle()
		{
			return _formHandle;
		}

		public void SetFormHandle(IntPtr formHandle)
		{
			_formHandle = formHandle;
		}

		public Rectangle GetArea()
		{
			return _area;
		}

		public void SetArea(Rectangle rectangle)
		{
			if (_area != rectangle)
			{
				_area = rectangle;
				_headerBounds = _area;
				_isLayoutDirty = true;
			}
		}

		public int GetClampedBorderHeight()
		{
			int height = GetBorderSize().Height;
			if (height > 4)
			{
				height = 4;
			}
			return height;
		}

		public Size GetBorderSize()
		{
			return _borderSize;
		}

		public void SetBorderSize(Size size)
		{
			if (_borderSize != size)
			{
				_borderSize = size;
				_isLayoutDirty = true;
			}
		}

		public string GetDocumentTitlePrefix()
		{
			string text = "";
			if (_ribbon != null && _ribbon.GetHostForm() != null)
			{
				text = _ribbon.DocumentName;
				if (text.Length > 0 && _ribbon.ApplicationName.Length > 0)
				{
					text += " - ";
				}
			}
			return text;
		}

		public string GetApplicationName()
		{
			string result = "";
			if (_ribbon != null && _ribbon.GetHostForm() != null)
			{
				result = _ribbon.ApplicationName;
			}
			return result;
		}

		public string GetCaptionText()
		{
			string result = "";
			if (_ribbon != null && _ribbon.GetHostForm() != null)
			{
				result = _ribbon.DocumentName;
				if (result.Length > 0 && _ribbon.ApplicationName.Length > 0)
				{
					result += " - ";
				}
				result += _ribbon.ApplicationName;
			}
			return result;
		}

		public RibbonColorScheme GetColorScheme()
		{
			if (_ribbon != null)
			{
				return _ribbon._colorScheme;
			}
			return null;
		}

		public RibbonLayoutItem GetHotItem()
		{
			return _hotItem;
		}

		public void SetHotItem(RibbonLayoutItem hotItem)
		{
			if (_hotItem != hotItem)
			{
				if (_hotItem != null)
				{
					_hotItem.Hot = false;
				}
				_hotItem = hotItem;
				if (_hotItem != null)
				{
					_hotItem.Hot = true;
				}
				RefreshHeader(true);
			}
		}

		public void SetPushedItem(RibbonLayoutItem pushedItem)
		{
			if (_pushedItem != pushedItem)
			{
				if (_pushedItem != null)
				{
					_pushedItem.Pushed = false;
				}
				_pushedItem = pushedItem;
				if (_pushedItem != null)
				{
					_pushedItem.Pushed = true;
				}
				RefreshHeader(true);
			}
		}

		internal void SetPushedCaptionButton(RibbonLayoutItem button)
		{
			if (_pushedCaptionButton != button)
			{
				if (_pushedCaptionButton != null)
				{
					_pushedCaptionButton.Pushed = false;
				}
				_pushedCaptionButton = button;
				if (_pushedCaptionButton != null)
				{
					SetHotCaptionButton(null);
					_pushedCaptionButton.Pushed = true;
				}
				RefreshHeader(true);
			}
		}

		internal void SetHotCaptionButton(RibbonLayoutItem button)
		{
			if (_hotCaptionButton != button)
			{
				if (_hotCaptionButton != null)
				{
					_hotCaptionButton.Hot = false;
				}
				_hotCaptionButton = button;
				if (_hotCaptionButton != null)
				{
					_hotCaptionButton.Hot = true;
				}
				RefreshHeader(true);
			}
		}

		public void RebuildContextualTabs()
		{
			_contextualTabRenderers = new List<RibbonContextualTabRenderer>();
			if (_ribbon.Visible && !_ribbon.IsTabStripHidden())
			{
				foreach (RibbonContextualTab item in _ribbon.GetContextualTabList())
				{
					_contextualTabRenderers.Add(new RibbonContextualTabRenderer(item, _ribbon));
				}
			}
			_areContextualTabsStale = false;
		}

		internal void InvalidateLayout()
		{
			_areContextualTabsStale = true;
			_isLayoutDirty = true;
		}

		internal void RequestLayoutAndRepaint()
		{
			_isLayoutDirty = true;
			InvalidateHeader();
		}

		internal void ResetCaptionFont()
		{
			_captionFont?.Dispose();
			_captionFont = null;
		}

		public Font GetCaptionFont()
		{
			if (_captionFont != null)
			{
				return _captionFont;
			}
			if (_ribbon.TitleBarFont != null)
			{
				_captionFont = new Font(_ribbon.TitleBarFont.FontFamily, SystemFonts.CaptionFont.Size, _ribbon.TitleBarFont.Style);
				return _captionFont;
			}
			return SystemFonts.CaptionFont;
		}

		public bool HandleMouseMove(Point pt)
		{
			if (_controlBoxButtonRenderer != null && _controlBoxButtonRenderer._bounds.Contains(pt))
			{
				if (_quickAccessToolbar != null)
				{
					_quickAccessToolbar.SetHoveredItem(null);
				}
				SetHotItem(_controlBoxButtonRenderer);
				return true;
			}
			if (_iconBounds.Contains(pt))
			{
				return true;
			}
			SetHotItem(null);
			if (_quickAccessToolbar != null && _quickAccessToolbar.HandleMouseMove(pt))
			{
				return true;
			}
			return false;
		}

		public bool HandleRightMouseDown(Point pt)
		{
			if (_ribbon != null)
			{
				_ribbon.ShowSuperTip(null);
			}
			if (_iconBounds.Contains(pt))
			{
				if (_ribbon != null && _ribbon.GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton)
				{
					return true;
				}
				return false;
			}
			if (_controlBoxButtonRenderer != null && _controlBoxButtonRenderer._bounds.Contains(pt))
			{
				return true;
			}
			if (_quickAccessToolbar != null && _quickAccessToolbar.HandleMouseDown(pt, MouseButtons.Right))
			{
				return true;
			}
			return false;
		}

		public bool HandleDoubleClick(Point pt)
		{
			if (_controlBoxButtonRenderer != null && _controlBoxButtonRenderer._bounds.Contains(pt))
			{
				if (!_ribbon.IsInDesignMode() && _ribbon.ControlBoxDoubleClickAction == ControlBoxDoubleClickAction.CloseForm && _ribbon.GetHostForm() != null)
				{
					_ribbon.GetHostForm().Close();
				}
				return true;
			}
			Rectangle rectangle = _iconBounds;
			rectangle.Inflate(1, 1);
			if (rectangle.Contains(pt))
			{
				if (_ribbon != null && _ribbon.GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public bool HandleMouseDown(Point pt)
		{
			if (_ribbon != null)
			{
				_ribbon.ShowSuperTip(null);
			}
			if (_iconBounds.Contains(pt))
			{
				if (_ribbon != null && _ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton)
				{
					return true;
				}
				return false;
			}
			if (_controlBoxButtonRenderer != null && _controlBoxButtonRenderer._bounds.Contains(pt))
			{
				SetPushedItem(_controlBoxButtonRenderer);
				_ribbon.ShowControlBoxMenu();
				return true;
			}
			if (_quickAccessToolbar != null && _quickAccessToolbar.HandleMouseDown(pt, MouseButtons.Left))
			{
				return true;
			}
			if (_contextualTabRenderers != null)
			{
				foreach (RibbonContextualTabRenderer item in _contextualTabRenderers)
				{
					if (item._bounds.Contains(pt))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool HandleMouseUp(Point pt, MouseButtons mouseButtons)
		{
			SetPushedItem(null);
			if (_ribbon == null)
			{
				return false;
			}
			if (mouseButtons == MouseButtons.Right && _controlBoxButtonRenderer != null && _controlBoxButtonRenderer._bounds.Contains(pt))
			{
				_ribbon.BuildRibbonContextMenu(null, false);
				return true;
			}
			if (_quickAccessToolbar != null && _quickAccessToolbar.HandleMouseUp(pt, mouseButtons))
			{
				_ribbon.SetPushedRenderer(null);
				return true;
			}
			if (mouseButtons == MouseButtons.Left)
			{
				if (_contextualTabRenderers != null)
				{
					foreach (RibbonContextualTabRenderer item in _contextualTabRenderers)
					{
						if (!item._bounds.Contains(pt))
						{
							continue;
						}
						foreach (RibbonTab tab in item.GetContextualTab().Tabs)
						{
							if (tab.Visible && tab.Enabled)
							{
								_ribbon.SelectedTab = tab;
								if (_ribbon.Minimized)
								{
									_ribbon._tabStripRenderer.ShowMinimizedRibbonPopup();
								}
								if (_ribbon.IsInDesignMode())
								{
									_ribbon.DesignerSelectComponent(item.GetContextualTab());
								}
								return true;
							}
						}
					}
				}
				if (_controlBoxButtonRenderer != null && _controlBoxButtonRenderer._bounds.Contains(pt))
				{
					return true;
				}
			}
			return false;
		}

		public void ClearHotAndPushedState()
		{
			SetHotCaptionButton(null);
			SetPushedCaptionButton(null);
			if (GetHotItem() == _controlBoxButtonRenderer)
			{
				Rectangle ribbonRectangle = _ribbon._controlBoxButtonBounds;
				if (!ribbonRectangle.Contains(_ribbon.PointToClient(Cursor.Position)))
				{
					SetHotItem(null);
				}
			}
			else
			{
				SetHotItem(null);
			}
			if (_quickAccessToolbar != null)
			{
				_quickAccessToolbar.SetHoveredItem(null);
			}
		}

		public void DrawHeader(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (_areContextualTabsStale)
			{
				RebuildContextualTabs();
			}
			if (_ribbon.Visible && _ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton && !_ribbon.IsTabStripHidden())
			{
				if (_ribbon._controlBoxButtonRenderer == null)
				{
					_ribbon._controlBoxButtonRenderer = new ControlBoxButtonRenderer(_ribbon);
				}
			}
			else
			{
				_ribbon._controlBoxButtonRenderer = null;
			}
			_controlBoxButtonRenderer = _ribbon._controlBoxButtonRenderer;
			_isLayoutDirty = false;
			int yOffset = 0;
			if (!_ribbon.IsCaptionBarShown())
			{
				_ribbon._controlBoxButtonBounds = new Rectangle(4, _headerBounds.Y + GetClampedBorderHeight() - _headerBounds.Height + 4, _ribbon.GetControlBoxButtonSize().Width, _ribbon.GetControlBoxButtonSize().Height);
			}
			else
			{
				if (_ribbon.IsFormMaximized())
				{
					yOffset += 4;
				}
				int leftMargin = 4;
				if (!_ribbon.GetFrameBorderSize().IsEmpty)
				{
					_ribbon._controlBoxButtonBounds = new Rectangle(_headerBounds.X + leftMargin, _headerBounds.Y + Math.Min(8, _ribbon.GetFrameBorderSize().Height) + yOffset, _ribbon.GetControlBoxButtonSize().Width, _ribbon.GetControlBoxButtonSize().Height);
				}
				else
				{
					_ribbon._controlBoxButtonBounds = new Rectangle(_headerBounds.X + leftMargin, _headerBounds.Y + 4, _ribbon.GetControlBoxButtonSize().Width, _ribbon.GetControlBoxButtonSize().Height);
				}
				if (_ribbon.IsFormMaximized())
				{
					yOffset -= 2;
				}
			}
			if (_formHandle == IntPtr.Zero)
			{
				_borderSize = Size.Empty;
			}
			else
			{
				_borderSize = _ribbon.GetFrameBorderSize();
			}
			int buttonWidth = _ribbon.GetControlBoxButtonSize().Width;
			int contentLeft = ((_ribbon.GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton) ? 1 : 4);
			if (_ribbon.IsGlassCaptionActive())
			{
				contentLeft += _borderSize.Width;
			}
			if (_controlBoxButtonRenderer != null)
			{
				if (_ribbon.IsFormMaximized())
				{
					_controlBoxButtonRenderer._bounds = new Rectangle(_headerBounds.X + contentLeft, _headerBounds.Y + 10 + yOffset, _ribbon.GetControlBoxButtonSize().Width, _ribbon.GetControlBoxButtonSize().Height);
				}
				else
				{
					_controlBoxButtonRenderer._bounds = new Rectangle(_headerBounds.X + contentLeft, _headerBounds.Y + 8 + yOffset, _ribbon.GetControlBoxButtonSize().Width, _ribbon.GetControlBoxButtonSize().Height);
				}
				_iconBounds = Rectangle.Empty;
			}
			else
			{
				int iconTop = _headerBounds.Y + (_headerBounds.Height - 4 - _ribbon.GetControlBoxButtonSize().Height) / 2 + 2;
				if (_ribbon.IsGlassCaptionActive())
				{
					iconTop = ((!_ribbon.IsFormMaximized()) ? (iconTop + 2) : (iconTop + 4));
				}
				bool hasCaptionPadding = false;
				if (_ribbon != null && _ribbon.Dock == DockStyle.Top && _ribbon.IsParentedToForm() && _ribbon.HandlesTitleBar && _ribbon.IsCaptionBarShown())
				{
					contentLeft += 6;
					hasCaptionPadding = true;
				}
				_iconBounds = new Rectangle(_headerBounds.X + contentLeft, iconTop, _ribbon.GetControlBoxButtonSize().Width, _ribbon.GetControlBoxButtonSize().Height);
				if (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton && _ribbon.QuickAccessCommands.Count == 0)
				{
					buttonWidth = _headerBounds.Height - 8;
				}
				if (hasCaptionPadding)
				{
					contentLeft += 3;
				}
			}
			int availableWidth = _headerBounds.Width - buttonWidth - 12;
			int tabsLeft = _headerBounds.X + buttonWidth;
			tabsLeft = ((_ribbon.GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton) ? (tabsLeft + (contentLeft + 4)) : (tabsLeft + (contentLeft + 6)));
			if (GetMinimizeButton() != null)
			{
				availableWidth -= GetMinimizeButton()._bounds.Width;
			}
			if (GetMaximizeButton() != null)
			{
				availableWidth -= GetMaximizeButton()._bounds.Width;
			}
			if (GetCloseButton() != null)
			{
				availableWidth -= GetCloseButton()._bounds.Width;
			}
			if (GetHelpButton() != null)
			{
				availableWidth -= GetHelpButton()._bounds.Width;
			}
			if (_ribbon.Visible && !_ribbon.IsTabStripHidden() && _ribbon.QuickAccessPosition == QuickAccessPosition.AboveRibbon)
			{
				if (_quickAccessToolbar == null)
				{
					_quickAccessToolbar = new HeaderQuickAccessToolbarRenderer(_ribbon, this);
					_ribbon._quickAccessToolbarRenderer = _quickAccessToolbar;
				}
				if (_borderSize.Height > 0)
				{
					_quickAccessToolbar.ArrangeToolbar(graphics, tabsLeft, _headerBounds.Y + _borderSize.Height + yOffset);
				}
				else
				{
					_quickAccessToolbar.ArrangeToolbar(graphics, tabsLeft, _headerBounds.Y + 1 + yOffset);
				}
				if (_quickAccessToolbar._rectangle.Width > 0)
				{
					int remainingWidth = availableWidth - (_quickAccessToolbar._rectangle.Width + 20);
					while (remainingWidth < 30 && _quickAccessToolbar.HasVisibleItems())
					{
						_quickAccessToolbar.Shrink(graphics);
						remainingWidth = ((_quickAccessToolbar._rectangle.Width <= 0) ? (availableWidth - 10) : (availableWidth - (_quickAccessToolbar._rectangle.Width + 20)));
					}
					availableWidth = remainingWidth;
					tabsLeft = ((_quickAccessToolbar._rectangle.Width <= 0) ? (tabsLeft + 10) : (tabsLeft + (_quickAccessToolbar._rectangle.Width + 20)));
				}
			}
			else
			{
				_quickAccessToolbar = null;
			}
			Rectangle rectangle = new Rectangle(tabsLeft, _headerBounds.Y + yOffset, availableWidth, _headerBounds.Height);
			int firstTabLeft = -1;
			int lastTabRight = -1;
			foreach (RibbonContextualTabRenderer item in _contextualTabRenderers)
			{
				Rectangle field_ = _ribbon.GetContextualTabBounds(graphics, item.GetContextualTab());
				if (field_.IsEmpty)
				{
					item._isVisible = false;
					continue;
				}
				if (field_.X < tabsLeft)
				{
					if (_quickAccessToolbar != null && _quickAccessToolbar._rectangle.Width > 0)
					{
						int remainingTabWidth = field_.X - (_quickAccessToolbar._rectangle.Width + 20);
						while (remainingTabWidth < 30 && _quickAccessToolbar.HasVisibleItems())
						{
							_quickAccessToolbar.Shrink(graphics);
							remainingTabWidth = ((_quickAccessToolbar._rectangle.Width <= 0) ? (field_.X - 10) : (field_.X - (_quickAccessToolbar._rectangle.Width + 20)));
						}
						availableWidth = remainingTabWidth;
						tabsLeft = ((_quickAccessToolbar._rectangle.Width <= 0) ? (tabsLeft + 10) : (tabsLeft + (_quickAccessToolbar._rectangle.Width + 20)));
					}
					else
					{
						int clippedWidth = tabsLeft - field_.X - 10;
						field_.X = tabsLeft - 10;
						field_.Width -= clippedWidth;
						item._isLeftEdgeVisible = false;
					}
				}
				else
				{
					item._isLeftEdgeVisible = true;
				}
				item._contextualGroupBounds = field_;
				if (field_.Right > rectangle.Right)
				{
					field_.Width = rectangle.Right - field_.X;
				}
				if (field_.Width <= 0)
				{
					item._bounds = Rectangle.Empty;
					item._isVisible = false;
					continue;
				}
				item._isVisible = true;
				if (firstTabLeft == -1 || field_.X < firstTabLeft)
				{
					firstTabLeft = field_.X;
				}
				if (lastTabRight == -1 || field_.Right > lastTabRight)
				{
					lastTabRight = field_.Right;
				}
				item.ArrangeItem(graphics, textRenderer, field_.Left, _headerBounds.Top, field_.Width, _headerBounds.Height);
			}
			Rectangle empty = Rectangle.Empty;
			textRenderer.SetStringAlignment(StringAlignment.Center);
			if (firstTabLeft != -1)
			{
				string text = GetDocumentTitlePrefix() + GetApplicationName();
				int titleTextWidth = (int)textRenderer.MeasureText(graphics, text, GetCaptionFont(), 0, 0).Width;
				Rectangle leftTitleArea = new Rectangle(rectangle.Left, rectangle.Top, firstTabLeft - rectangle.Left, _headerBounds.Height);
				if ((rectangle.Right - titleTextWidth) / 2 > lastTabRight)
				{
					empty = new Rectangle(lastTabRight, rectangle.Top, rectangle.Right - lastTabRight, rectangle.Height);
				}
				else if (leftTitleArea.Width < titleTextWidth)
				{
					Rectangle rightTitleArea = new Rectangle(lastTabRight, _headerBounds.Top + _borderSize.Height, rectangle.Right - lastTabRight, _headerBounds.Height - _borderSize.Height);
					empty = ((rightTitleArea.Width <= titleTextWidth) ? leftTitleArea : rightTitleArea);
				}
				else
				{
					empty = leftTitleArea;
				}
			}
			else
			{
				empty = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
			}
			if (empty.IsEmpty)
			{
				return;
			}
			int documentWidth = (int)textRenderer.MeasureText(graphics, GetDocumentTitlePrefix(), GetCaptionFont(), 0, 0).Width;
			int applicationWidth = 0;
			if (GetApplicationName().Length > 0)
			{
				applicationWidth = (int)textRenderer.MeasureText(graphics, GetApplicationName(), GetCaptionFont(), 0, 0).Width;
			}
			if (documentWidth + applicationWidth > empty.Width)
			{
				int ellipsisWidth = (int)textRenderer.MeasureText(graphics, "W", GetCaptionFont(), 0, 0).Width;
				applicationWidth = ellipsisWidth;
				if (empty.Width - documentWidth <= ellipsisWidth)
				{
					applicationWidth = ellipsisWidth;
					documentWidth = empty.Width - applicationWidth;
				}
				else
				{
					applicationWidth = empty.Width - documentWidth;
				}
				_documentTextBounds = new Rectangle(empty.X, empty.Y, documentWidth, empty.Height);
				_applicationTextBounds = new Rectangle(_documentTextBounds.Right, empty.Y, applicationWidth, empty.Height);
			}
			else
			{
				if (_ribbon.Visible && !_ribbon.IsTabStripHidden())
				{
					_documentTextBounds = new Rectangle(empty.X + (empty.Width - documentWidth - applicationWidth) / 2, empty.Y, documentWidth, empty.Height);
				}
				else
				{
					_documentTextBounds = new Rectangle(empty.X, empty.Y, documentWidth, empty.Height);
				}
				_applicationTextBounds = new Rectangle(_documentTextBounds.Right, empty.Y, applicationWidth, empty.Height);
			}
		}

		private void DrawBackground(Graphics graphics, Rectangle rectangle)
		{
			Color activeBorderColor = GetColorScheme()._activeWindowBorder;
			Rectangle rect = new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height - 1);
			GdiCache.FillGradient(graphics, rect, GetColorScheme()._activeCaptionGradientFirst, GetColorScheme()._activeCaptionGradientSecond, LinearGradientMode.Vertical);
			Pen pen = GdiCache.Pen(activeBorderColor);
			graphics.DrawLine(pen, rectangle.X, rectangle.Y, rectangle.Right - 1, rectangle.Y);
			pen = GdiCache.Pen(GetColorScheme()._activeWindowInnerBorder);
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.Right - 1, rectangle.Y + 1);
		}

		private void DrawInactiveFlatBackground(Graphics graphics, Rectangle rectangle)
		{
			Color inactiveBorderColor = GetColorScheme()._inactiveWindowBorder;
			Rectangle rect = new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height - 1);
			GdiCache.FillGradient(graphics, rect, GetColorScheme()._inactiveCaptionGradientFirst, GetColorScheme()._inactiveCaptionGradientSecond, LinearGradientMode.Vertical);
			Pen pen = GdiCache.Pen(inactiveBorderColor);
			graphics.DrawLine(pen, rectangle.X, rectangle.Y, rectangle.Right - 1, rectangle.Y);
			pen = GdiCache.Pen(GetColorScheme()._inactiveWindowInnerBorder);
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.Right - 1, rectangle.Y + 1);
		}

		private void DrawActiveHeaderBackground(Graphics graphics, Rectangle bounds)
		{
			DrawBackground(graphics, bounds);
			return;
		}

		private void DrawInactiveHeaderBackground(Graphics graphics, Rectangle bounds)
		{
			DrawInactiveFlatBackground(graphics, bounds);
			return;
		}

		private void RenderContent(Graphics graphics, Rectangle headerBounds, bool drawBackground)
		{
			Rectangle rectangle = headerBounds;
			if (_ribbon.IsFormActive())
			{
				if (drawBackground)
				{
					DrawActiveHeaderBackground(graphics, rectangle);
				}
			}
			else if (drawBackground)
			{
				DrawInactiveHeaderBackground(graphics, rectangle);
			}
			Pen pen = GdiCache.Pen(GetColorScheme()._captionBottomLine);
			graphics.DrawLine(pen, headerBounds.X, headerBounds.Bottom - 1, headerBounds.Right, headerBounds.Bottom - 1);
			DrawCaptionButtons(graphics, null);
		}

		internal void PaintContextualTabs(Graphics graphics)
		{
			if (_ribbon != null)
			{
				bool useTextRenderer = true;
				if (_ribbon.IsGlassCaptionActive() && _ribbon.IsParentedToForm() && _ribbon.IsFormMaximized())
				{
					useTextRenderer = false;
				}
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
				textLayoutRenderer.SetNoPadding(true);
				PaintContextualTabRenderers(graphics, textLayoutRenderer);
			}
		}

		private void PaintContextualTabRenderers(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (_contextualTabRenderers == null)
			{
				return;
			}
			foreach (RibbonContextualTabRenderer item in _contextualTabRenderers)
			{
				if (item._isVisible)
				{
					item.Paint(graphics, textRenderer, _ribbon._colorScheme, _ribbon.Font, Color.White, Rectangle.Empty);
				}
			}
		}

		public void DrawHeaderBackground(Graphics graphics, bool isActive, bool drawContextualTabs)
		{
			if (GetArea().Width == 0 || GetArea().Height == 0)
			{
				return;
			}
			Region clip = null;
			try
			{
				if (_formHandle == IntPtr.Zero || (_ribbon.GetHostForm() != null && _ribbon.GetHostForm().FormBorderStyle == FormBorderStyle.None))
				{
					Brush brush = GdiCache.Brush(_ribbon.GetHeaderBackColor());
					graphics.FillRectangle(brush, GetArea());
				}
				else
				{
					clip = graphics.Clip;
					using (Region region = new Region(GetArea()))
					{
						graphics.Clip = region;
					}
					if (!_ribbon.IsGlassCaptionActive())
					{
						RenderContent(graphics, GetArea(), true);
					}
				}
				DrawHeaderContent(graphics, isActive, drawContextualTabs);
			}
			finally
			{
				if (clip != null)
				{
					graphics.Clip = clip;
					clip.Dispose();
				}
			}
		}

		private void DrawHeaderContent(Graphics graphics, bool isActive, bool drawContextualTabs)
		{
			bool useTextRenderer = true;
			if (_ribbon.IsGlassCaptionActive() && _ribbon.IsParentedToForm() && _ribbon.IsFormMaximized())
			{
				useTextRenderer = false;
			}
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
			textLayoutRenderer.SetNoPadding(true);
			if (_isLayoutDirty)
			{
				DrawHeader(graphics, textLayoutRenderer);
			}
			if (_ribbon.Visible && !_ribbon.IsTabStripHidden())
			{
				if (_controlBoxButtonRenderer != null)
				{
					if (_ribbon != null)
					{
						Rectangle ribbonRectangle = _ribbon._controlBoxButtonBounds;
						ribbonRectangle.Inflate(6, 6);
						_ribbon.Invalidate(ribbonRectangle);
					}
					_controlBoxButtonRenderer.Paint(graphics, textLayoutRenderer, _ribbon._colorScheme, _ribbon.Font, _ribbon.ForeColorDark, Rectangle.Empty);
				}
				if (_quickAccessToolbar != null)
				{
					_quickAccessToolbar.OnPaint(graphics, _ribbon._colorScheme, Rectangle.Empty);
				}
				if (drawContextualTabs)
				{
					PaintContextualTabRenderers(graphics, textLayoutRenderer);
				}
			}
			if (_formHandle != IntPtr.Zero)
			{
				if (!_iconBounds.IsEmpty)
				{
					Rectangle rectangle = _iconBounds;
					if (_ribbon.GetHostForm() != null && _ribbon.GetHostForm().ShowIcon)
					{
						RibbonDrawingUtils.DrawIcon(graphics, _ribbon.ResolveControlBoxIcon(), rectangle, true, SystemColors.Window, _ribbon.IsGlassCaptionActive() && _ribbon.IsParentedToForm());
					}
				}
				if (GetCaptionText().Length > 0)
				{
					Color color = _ribbon._colorScheme._activeCaptionDocumentText;
					Color applicationColor = _ribbon._colorScheme._activeCaptionApplicationText;
					if (!isActive)
					{
						color = _ribbon._colorScheme._inactiveCaptionDocumentText;
						applicationColor = _ribbon._colorScheme._inactiveCaptionApplicationText;
					}
					if (_ribbon.IsGlassCaptionActive() && _ribbon.IsParentedToForm())
					{
						if (GetDocumentTitlePrefix().Length > 0 && GetApplicationName().Length > 0)
						{
							Rectangle combinedTextBounds = _documentTextBounds;
							combinedTextBounds.Width += _applicationTextBounds.Width;
							string text = GetDocumentTitlePrefix() + GetApplicationName();
							RibbonDrawingUtils.DrawText(graphics, text, GetCaptionFont(), combinedTextBounds, 10, _ribbon.IsFormMaximized());
						}
						else
						{
							if (GetDocumentTitlePrefix().Length > 0)
							{
								Rectangle documentBounds = _documentTextBounds;
								string documentText = GetDocumentTitlePrefix();
								RibbonDrawingUtils.DrawText(graphics, documentText, GetCaptionFont(), documentBounds, 10, _ribbon.IsFormMaximized());
							}
							if (GetApplicationName().Length > 0)
							{
								Rectangle applicationBounds = _applicationTextBounds;
								RibbonDrawingUtils.DrawText(graphics, GetApplicationName(), GetCaptionFont(), applicationBounds, 10, _ribbon.IsFormMaximized());
							}
						}
					}
					else
					{
						textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
						textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
						textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
						if (GetDocumentTitlePrefix().Length > 0 && _documentTextBounds.Width > 0)
						{
							Brush documentBrush = GdiCache.Brush(color);
							textLayoutRenderer.DrawTextWithBrush(graphics, GetDocumentTitlePrefix(), _documentTextBounds, GetCaptionFont(), documentBrush);
						}
						if (GetApplicationName().Length > 0 && _applicationTextBounds.Width > 0)
						{
							Brush applicationBrush = GdiCache.Brush(applicationColor);
							textLayoutRenderer.DrawTextWithBrush(graphics, GetApplicationName(), _applicationTextBounds, GetCaptionFont(), applicationBrush);
						}
					}
				}
			}
		}

		internal void RefreshHeader(bool updateNow)
		{
			if (_formHandle != IntPtr.Zero && _ribbon != null && _ribbon.IsGlassCaptionActive())
			{
				_ribbon.RefreshWindowFrame(updateNow);
			}
			else
			{
				_ribbon.Invalidate(GetArea());
			}
		}

		internal void InvalidateHeader()
		{
			if (_formHandle != IntPtr.Zero)
			{
				_ribbon.InvalidateWindowFrame();
			}
			else
			{
				_ribbon.Invalidate(GetArea());
			}
		}

		internal void RepaintCaptionButton(RibbonLayoutItem button)
		{
			if (_ribbon != null)
			{
				_ribbon.Invalidate();
				using (Graphics graphics = _ribbon.CreateGraphics())
				{
					DrawCaptionButtons(graphics, button);
				}
			}
		}

		internal void DrawCaptionButtons(Graphics graphics, RibbonLayoutItem button)
		{
			if ((button == null || button == GetCloseButton()) && GetCloseButton() != null && GetCloseButton()._isVisible)
			{
				DrawCloseButton(graphics);
			}
			if ((button == null || button == GetMaximizeButton()) && GetMaximizeButton() != null && GetMaximizeButton()._isVisible)
			{
				DrawMaximizeButton(graphics);
			}
			if ((button == null || button == GetMinimizeButton()) && GetMinimizeButton() != null && GetMinimizeButton()._isVisible)
			{
				DrawMinimizeButton(graphics);
			}
			if ((button == null || button == GetHelpButton()) && GetHelpButton() != null && GetHelpButton()._isVisible)
			{
				DrawHelpButton(graphics);
			}
		}

		private void DrawHelpButton(Graphics graphics)
		{
			if (GetHelpButton()._bounds.Width > 0 && GetHelpButton()._bounds.Height > 0)
			{
				Rectangle bounds = GetHelpButton()._bounds;
				if (!_ribbon.IsTabStripHidden())
				{
					bounds.Offset(-_ribbon.GetFrameBorderSize().Width, 0);
				}
				RibbonChromePainter.DrawCaptionButtonBackgroundFlat(graphics, bounds, GetColorScheme(), GetHelpButton().Hot, GetHelpButton().Pushed);
				RibbonChromePainter.DrawHelpGlyphFlat(graphics, bounds, GetColorScheme(), _ribbon.IsFormActive(), GetHelpButton().Hot, GetHelpButton().Pushed);
			}
		}

		private void DrawMaximizeButton(Graphics graphics)
		{
			if (GetMaximizeButton()._bounds.Width <= 0 || GetMaximizeButton()._bounds.Height <= 0)
			{
				return;
			}
			Rectangle bounds = GetMaximizeButton()._bounds;
			if (!_ribbon.IsTabStripHidden())
			{
				bounds.Offset(-_ribbon.GetFrameBorderSize().Width, 0);
			}
			RibbonChromePainter.DrawCaptionButtonBackgroundFlat(graphics, bounds, GetColorScheme(), GetMaximizeButton().Hot, GetMaximizeButton().Pushed);
			if (_ribbon.GetHostForm().WindowState == FormWindowState.Maximized)
			{
				RibbonChromePainter.DrawRestoreGlyphFlat(graphics, bounds, GetColorScheme(), _ribbon.IsFormActive(), GetMaximizeButton().Hot, GetMaximizeButton().Pushed);
			}
			else
			{
				RibbonChromePainter.DrawMaximizeGlyphFlat(graphics, bounds, GetColorScheme(), _ribbon.IsFormActive(), GetMaximizeButton().Hot, GetMaximizeButton().Pushed, GetMaximizeButton()._isEnabled);
			}
		}

		private void DrawMinimizeButton(Graphics graphics)
		{
			if (GetMinimizeButton()._bounds.Width > 0 && GetMinimizeButton()._bounds.Height > 0)
			{
				Rectangle bounds = GetMinimizeButton()._bounds;
				if (!_ribbon.IsTabStripHidden())
				{
					bounds.Offset(-_ribbon.GetFrameBorderSize().Width, 0);
				}
				RibbonChromePainter.DrawCaptionButtonBackgroundFlat(graphics, bounds, GetColorScheme(), GetMinimizeButton().Hot, GetMinimizeButton().Pushed);
				RibbonChromePainter.DrawMinimizeGlyphFlat(graphics, bounds, GetColorScheme(), _ribbon.IsFormActive(), GetMinimizeButton().Hot, GetMinimizeButton().Pushed, GetMinimizeButton()._isEnabled);
			}
		}

		private void DrawCloseButton(Graphics graphics)
		{
			if (GetCloseButton()._bounds.Width > 0 && GetCloseButton()._bounds.Height > 0)
			{
				Rectangle bounds = GetCloseButton()._bounds;
				if (!_ribbon.IsTabStripHidden())
				{
					bounds.Offset(-_ribbon.GetFrameBorderSize().Width, 0);
				}
				RibbonChromePainter.DrawCloseButtonFlat(graphics, bounds, GetColorScheme(), _ribbon.IsFormActive(), GetCloseButton().Hot, GetCloseButton().Pushed);
			}
		}
	}
}
