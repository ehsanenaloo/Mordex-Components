using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		private void SetBackstageMenuScrollableBase(BackstageMenuScrollableBase backstageMenuScrollableBase)
		{
			if (_backstageMenuScrollableBase == backstageMenuScrollableBase)
			{
				return;
			}
			if (_backstageMenuScrollableBase != null)
			{
				if (_backstageMenuScrollableBase.ContainsFocus)
				{
					if (GetHostForm() != null)
					{
						GetHostForm().Focus();
					}
					if (_backstageMenuScrollableBase.ContainsFocus)
					{
						Focus();
					}
				}
				_backstageMenuScrollableBase.Dispose();
				_backstageMenuScrollableBase = null;
			}
			if (GetHostForm() != null && backstageMenuScrollableBase != null)
			{
				GetHostForm().AutoScroll = false;
				_formAutoScroll = GetHostForm().AutoScroll;
			}
			_backstageMenuScrollableBase = backstageMenuScrollableBase;
			if (GetHostForm() != null && backstageMenuScrollableBase == null)
			{
				GetHostForm().AutoScroll = _formAutoScroll;
			}
			Invalidate();
		}

		internal Padding GetGroupsAreaPadding()
		{
			return new Padding(3, 3, 3, 3);
		}

		internal Padding GetContentBorderPadding()
		{
			if (!_minimized)
			{
				return new Padding(0, 0, 0, 3);
			}
			if (_quickAccessPosition == QuickAccessPosition.AboveRibbon && !IsGlassCaptionActive())
			{
				return new Padding(0, 0, 0, 2);
			}
			return new Padding(0, 0, 0, 0);
		}

		internal Padding GetGroupBoxPadding()
		{
			return new Padding(2, 0, 5, 0);
		}

		internal bool HasVisibleTab()
		{
			foreach (RibbonTab tab in Tabs)
			{
				if (tab.Visible)
				{
					return true;
				}
			}
			foreach (RibbonContextualTab item in GetContextualTabList())
			{
				if (!item.Visible)
				{
					continue;
				}
				foreach (MxRibbon contextualChildTab in item.Tabs)
				{
					if (contextualChildTab.Visible)
					{
						return true;
					}
				}
			}
			return false;
		}

		internal Rectangle GetContentAreaBounds()
		{
			return _contentAreaBounds;
		}

		internal int GetContentAreaWidth()
		{
			if (_contentAreaWidth <= 0)
			{
				return base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right;
			}
			return _contentAreaWidth;
		}

		internal void SetPushedRenderer(CommandBaseRenderer ribbonCommandBaseRenderer)
		{
			if (_pressedRenderer != ribbonCommandBaseRenderer)
			{
				if (_pressedRenderer != null)
				{
					_pressedRenderer.Pushed = false;
				}
				_pressedRenderer = ribbonCommandBaseRenderer;
				if (_pressedRenderer != null)
				{
					_pressedRenderer.Pushed = true;
				}
				Invalidate();
			}
		}

		internal void SetHotRenderer(CommandBaseRenderer commandBaseRenderer)
		{
			if (_hoveredRenderer == commandBaseRenderer)
			{
				return;
			}
			if (_hoveredRenderer != null)
			{
				_hoveredRenderer.StartPressAnimation();
				_hoveredRenderer.Hot = false;
				if (_hoveredRenderer.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)_hoveredRenderer.GetCommand()).InvokeMouseLeave();
				}
			}
			if (commandBaseRenderer != null)
			{
				if (commandBaseRenderer.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)commandBaseRenderer.GetCommand()).InvokeMouseEnter();
				}
				if (!commandBaseRenderer.Focusable)
				{
					commandBaseRenderer = null;
				}
			}
			_hoveredRenderer = commandBaseRenderer;
			if (_hoveredRenderer != null)
			{
				_hoveredRenderer.Hot = true;
			}
			ShowSuperTip(_hoveredRenderer);
		}

		internal int GetVisibleGroupCount()
		{
			if (_tabContentRenderer != null)
			{
				return _tabContentRenderer._groupRenderers.Count;
			}
			return 0;
		}

		public RibbonTab GetTabAt(int x, int y)
		{
			if (_tabStripRenderer != null)
			{
				RibbonTabRenderer ribbonTabRenderer = _tabStripRenderer.GetTabAt(x, y);
				if (ribbonTabRenderer != null)
				{
					return ribbonTabRenderer.GetTab();
				}
			}
			return null;
		}

		public RibbonTab GetTabAt(Point pt)
		{
			return GetTabAt(pt.X, pt.Y);
		}

		public RibbonTab GetTabAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetTabAt(point.X, point.Y);
		}

		public CommandBase GetCommandAt(Point pt)
		{
			return GetCommandAt(pt.X, pt.Y);
		}

		public CommandBase GetCommandAt()
		{
			return GetCommandAt(PointToClient(Cursor.Position));
		}

		public CommandBase GetCommandAt(int x, int y)
		{
			if (_tabContentRenderer != null)
			{
				CommandBase commandBase = _tabContentRenderer.FindCommandAt(x, y);
				if (commandBase == null && _quickAccessToolbarRenderer != null)
				{
					CommandBaseRenderer itemAt = _quickAccessToolbarRenderer.GetItemAt(new Point(x, y));
					if (itemAt != null)
					{
						return itemAt.GetCommand();
					}
				}
				return commandBase;
			}
			return null;
		}

		public CommandCollection GetAllCommands()
		{
			Hashtable hashtable = new Hashtable();
			CollectCommands(QuickAccessCommands, hashtable);
			CollectCommands(QuickAccessCustomizeList, hashtable);
			CollectCommands(ControlBoxMenu.LeftCommands, hashtable);
			CollectCommands(ControlBoxMenu.RightCommands, hashtable);
			CollectCommands(ControlBoxMenu.BottomCommands, hashtable);
			foreach (RibbonTab item in _tabs)
			{
				CollectGroupCommands(item.Groups, hashtable);
			}
			if (_tabs._mergedTabs != null)
			{
				foreach (RibbonTab hiddenTab in _tabs._mergedTabs)
				{
					CollectGroupCommands(hiddenTab.Groups, hashtable);
				}
			}
			foreach (RibbonContextualTab contextualTab in _contextualTabs)
			{
				foreach (RibbonTab tab in contextualTab.Tabs)
				{
					CollectGroupCommands(tab.Groups, hashtable);
				}
			}
			CommandCollection commandCollection = new CommandCollection(null, isQuickAccessCollection: false);
			int commandCount = 0;
			foreach (CommandBase value in hashtable.Values)
			{
				commandCollection.Add(value);
				commandCount++;
			}
			return commandCollection;
		}

		private void CollectCommands(CommandCollection commandCollection, Hashtable hashtable)
		{
			foreach (CommandBase item in commandCollection)
			{
				if (!hashtable.ContainsKey(item))
				{
					hashtable.Add(item, item);
					if (item is DropDownCommand)
					{
						CollectCommands(((DropDownCommand)item).Commands, hashtable);
					}
					if (item is Gallery)
					{
						CollectGalleryItems(((Gallery)item).GalleryItems, hashtable);
					}
					if (item is CommandBox)
					{
						CollectCommands(((CommandBox)item).Commands, hashtable);
					}
					if (item is ButtonGroup)
					{
						CollectCommands(((ButtonGroup)item).Commands, hashtable);
					}
				}
			}
		}

		private void CollectGalleryItems(GalleryItemCollection galleryItemCollection, Hashtable hashtable)
		{
			foreach (CommandBase item in galleryItemCollection)
			{
				if (!hashtable.ContainsKey(item))
				{
					hashtable.Add(item, item);
				}
			}
		}

		private void CollectGroupCommands(RibbonGroupCollection ribbonGroupCollection, Hashtable hashtable)
		{
			foreach (RibbonGroup item in ribbonGroupCollection)
			{
				if (!hashtable.ContainsKey(item))
				{
					hashtable.Add(item, item);
					CollectCommands(item.Commands, hashtable);
				}
			}
		}

		internal Rectangle GetTabBounds(RibbonTab ribbonTab)
		{
			if (!base.IsHandleCreated)
			{
				return Rectangle.Empty;
			}
			if (_isTabStripLayoutDirty)
			{
				RenderRibbonImmediately();
				RenderTabContentImmediately();
			}
			return _tabStripRenderer.FindTabRenderer(ribbonTab)?._bounds ?? Rectangle.Empty;
		}

		internal Rectangle GetCommandRendererBounds(CommandBase commandBase)
		{
			CommandBaseRenderer commandBaseRenderer = FindCommandRenderer(commandBase);
			if (commandBaseRenderer != null)
			{
				Rectangle bounds = commandBaseRenderer._bounds;
				if (commandBaseRenderer is GalleryRenderer)
				{
					bounds.Inflate(0, -3);
				}
				return bounds;
			}
			return Rectangle.Empty;
		}

		internal CommandBaseRenderer FindCommandRenderer(CommandBase commandBase)
		{
			if (!base.IsHandleCreated)
			{
				return null;
			}
			if (_isTabStripLayoutDirty)
			{
				RenderRibbonImmediately();
			}
			if (_isTabContentDirty)
			{
				RenderTabContentImmediately();
			}
			CommandBaseRenderer commandBaseRenderer = _tabContentRenderer.FindRendererForCommand(commandBase);
			if (commandBaseRenderer == null && GetPopupMenu() != null)
			{
				commandBaseRenderer = GetPopupMenu().FindRendererForCommand(commandBase);
			}
			return commandBaseRenderer;
		}

		internal void SyncActiveContextualTab(RibbonContextualTab item)
		{
			if (!item.Active && !base.DesignMode)
			{
				if (_contextualTabList.Contains(item))
				{
					_contextualTabList.Remove(item);
					_tabStripRenderer.MarkTabRenderersDirty();
					_headerRenderer.InvalidateLayout();
					RequestTabStripLayout(false);
					if (item.Tabs.Contains(_selectedTab))
					{
						SelectFirstVisibleTab();
					}
				}
			}
			else
			{
				if (!ContextualTabs.Contains(item))
				{
					return;
				}
				if (!_contextualTabList.Contains(item))
				{
					_contextualTabList.Add(item);
				}
				_tabStripRenderer.MarkTabRenderersDirty();
				if (_headerRenderer != null)
				{
					_headerRenderer.InvalidateLayout();
				}
				if (_selectedTab == null)
				{
					SelectFirstVisibleTab();
				}
				RequestTabStripLayout(false);
				InvalidateTabContent();
			}
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.Refresh(update: false);
			}
		}

		internal HitTest GetHitTest(int x, int y)
		{
			if (!_controlBoxButtonBounds.IsEmpty && _controlBoxButtonBounds.Contains(x, y))
			{
				return HitTest.ControlBoxButton;
			}
			if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer._customizeButton != null && _quickAccessToolbarRenderer._customizeButton._bounds.Contains(x, y))
			{
				return HitTest.CustomizeButton;
			}
			HitTest hitTest = _tabStripRenderer.GetHitTest(x, y);
			if (hitTest == HitTest.None)
			{
				hitTest = _tabContentRenderer.GetHitTest(x, y);
			}
			return hitTest;
		}

		internal int GetBackstageHitArea(Point screenPoint)
		{
			if (GetBackstageMenuScrollableBase() != null)
			{
				Point point = GetBackstageMenuScrollableBase().PointToClient(screenPoint);
				return GetBackstageMenuScrollableBase().GetHitRegion(point);
			}
			return 0;
		}

		internal void RecalculateLayout(bool isFormMaximized)
		{
			if (_isDisposing || _isMergeInProgress)
			{
				return;
			}
			try
			{
				if (IsCaptionBarShown())
				{
					int captionHeight = SystemInformation.CaptionHeight;
					if (_form != null && (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
					{
						captionHeight = SystemInformation.ToolWindowCaptionHeight;
					}
					if (captionHeight < MinCaptionHeight)
					{
						captionHeight = MinCaptionHeight;
					}
					_captionHeight = captionHeight;
					if (IsParentedToForm() && HandlesTitleBar)
					{
						_captionHeight += 4;
					}
				}
				else
				{
					_captionHeight = 0;
				}
				_smallItemHeight = _smallImageSize.Height;
				int textHeight = Font.Height;
				if (!base.IsDisposed && base.IsHandleCreated)
				{
					textHeight = TextRenderer.MeasureText("W", Font, Size.Empty).Height;
					if (!RibbonDrawingUtils.IsClearTypeFont(Font))
					{
						textHeight++;
					}
				}
				if (textHeight + 1 > _smallItemHeight)
				{
					_smallItemHeight = textHeight + 1;
				}
				_smallItemHeight += 6;
				if (_tabStripHeight != -1)
				{
					_tabStripRowHeight = _tabStripHeight;
				}
				else
				{
					_tabStripRowHeight = _smallItemHeight + 3;
					if (IsGlassCaptionActive())
					{
						_tabStripRowHeight++;
					}
				}
				if (IsTabStripHidden())
				{
					_tabStripRowHeight = 0;
				}
				if (GetHostForm() != null && GetHostForm().FormBorderStyle == FormBorderStyle.None)
				{
					_captionHeight = 0;
				}
				else if ((GetHostForm() == null || _ribbonWindowMessageHandler == null) && IsTabStripHidden())
				{
					_captionHeight = 0;
				}
				_groupLabelHeight = Font.Height;
				if (_groupLabelHeight < 14)
				{
					_groupLabelHeight = 14;
				}
				if (!_minimized)
				{
					int contentHeight = _smallItemHeight * ItemRows + _groupLabelHeight + GetGroupsAreaPadding().Top + GetGroupsAreaPadding().Bottom;
					contentHeight += GetGroupBoxPadding().Top + GetGroupBoxPadding().Bottom;
					if (_selectedTab != null && (_tabs.Count > 0 || (_selectedTab != null && _selectedTab.GetTabOwner() is RibbonContextualTab)))
					{
						_layoutHeight = _tabStripRowHeight + contentHeight + GetContentBorderPadding().Bottom;
						_contentAreaBounds = new Rectangle(GetContentBorderPadding().Left, _tabStripRowHeight + _captionHeight, base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right, contentHeight + GetContentBorderPadding().Bottom);
					}
					else
					{
						_layoutHeight = _tabStripRowHeight;
						_contentAreaBounds = new Rectangle(GetContentBorderPadding().Left, _tabStripRowHeight + _captionHeight, 0, 0);
					}
					if (IsGlassCaptionActive())
					{
						_contentAreaBounds.Y--;
						_layoutHeight--;
					}
				}
				else
				{
					_layoutHeight = _tabStripRowHeight + GetContentBorderPadding().Bottom;
					_contentAreaBounds = new Rectangle(GetContentBorderPadding().Left, _tabStripRowHeight + _captionHeight, 0, 0);
				}
				if (_headerRenderer != null && IsCaptionBarShown())
				{
					Rectangle rectangle = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, _captionHeight);
					if (IsGlassCaptionActive() && _ribbonWindowMessageHandler != null)
					{
						rectangle.Width = GetCaptionAreaWidth();
					}
					_headerRenderer.SetArea(rectangle);
				}
				if (_quickAccessPosition == QuickAccessPosition.BelowRibbon)
				{
					_layoutHeight += QuickAccessBarHeight;
					if (_quickAccessToolbarRenderer == null)
					{
						_quickAccessToolbarRenderer = new QuickAccessToolbarRenderer(this);
					}
				}
				_layoutHeight += _captionHeight;
				base.Height = _layoutHeight;
			}
			catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
			{
				// Layout can run while the control or its form is being created or closed.
			}
		}

		internal Rectangle GetContextualTabBounds(Graphics graphics, RibbonContextualTab ribbonContextualTab)
		{
			if (_tabStripRenderer != null)
			{
				if (_isTabStripLayoutDirty)
				{
					RenderContent(graphics, false);
					RenderTabContentImmediately();
				}
				return _tabStripRenderer.GetContextualTabBounds(ribbonContextualTab);
			}
			return Rectangle.Empty;
		}

		private void BuildKeyTips(RibbonTab owner)
		{
			_keyTipOwner = owner;
			_keyTips = new List<RibbonKeyTipEntry>();
			using Graphics graphics = CreateGraphics();
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			foreach (RibbonGroupBoxRenderer item in _tabContentRenderer._groupRenderers)
			{
				Rectangle bounds = (item as CommandBaseRenderer)._bounds;
				bool showToolTip = bounds.Left >= base.ClientRectangle.X && bounds.Right <= base.ClientRectangle.Right;
				((IKeyboardNavigable)item).AddKeyTips(graphics, textRenderer, Font, _keyTips, showToolTip);
			}
		}

		internal void NotifyGallerySelectionChanged(Gallery gallery, GalleryItem galleryItem)
		{
			if (_tabContentRenderer != null)
			{
				_tabContentRenderer.ScrollGalleryItemIntoView(gallery, galleryItem);
			}
		}

		internal Size GetControlBoxButtonSize()
		{
			if (GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton && !IsTabStripHidden())
			{
				return new Size(36, 36);
			}
			return new Size(16, 16);
		}
	}
}
