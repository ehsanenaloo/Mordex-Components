using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking.Internal
{
	[DefaultMember("Item")]
	internal class MdiTabStripLayout : IEnumerable
	{
		private const int DefaultEdgePadding = 7;

		private const int ButtonSize = 16;

		private const int ScrollButtonWidth = 11;

		private const int ButtonGap = 3;

		private const int CloseButtonRightOffset = 17;

		private const int TabCloseButtonTop = 2;

		private const int ButtonTop = 1;

		internal bool _showCloseButton;

		internal bool _useActiveFilesMenu;

		internal bool _showCloseButtonOnSelectedTab;

		private MdiTabItem _activeFilesButton;

		private MdiTabItem _closeButton;

		private MxMdiTabGroup _tabGroup;

		internal ArrayList _tabs;

		private bool _hasSlantedTabs;

		private TabScrollButtons _tabScrollButtons;

		private MxContextMenu _activeFilesMenu;

		private MxCommandManager _menuCommandManager;

		internal bool _hasOverflow;

		private int _firstVisibleIndex;

		private Rectangle _tabsBounds;

		private MdiTabItem _scrollLeftButton;

		private MdiTabItem _scrollRightButton;

		private bool _hasButtons;

		private int _edgePadding;

		private MdiTabItem _hotItem;

		private bool _isMenuOpen;

		internal MdiTabStripLayout(MxMdiTabGroup tabControl)
		{
			_showCloseButton = tabControl.PanelManager.TabbedMdiSettings.ShowCloseButton;
			_tabScrollButtons = tabControl.PanelManager.TabbedMdiSettings.ResolveTabScrollButtons();
			_showCloseButtonOnSelectedTab = tabControl.PanelManager.TabbedMdiSettings.ShowCloseButtonOnSelectedTab;
			_useActiveFilesMenu = _tabScrollButtons == TabScrollButtons.UseContextMenu;
			if (_useActiveFilesMenu)
			{
				_tabScrollButtons = TabScrollButtons.Never;
			}
			_tabGroup = tabControl;
			_hasSlantedTabs = false;
			_edgePadding = DefaultEdgePadding;
			_tabs = new ArrayList();
			CreateActiveFilesMenu();
		}

		internal void DisposeResources()
		{
			if (_activeFilesMenu != null)
			{
				_activeFilesMenu.CommandClick -= OnActiveFilesMenuCommandClick;
				_activeFilesMenu.Dispose();
				_activeFilesMenu = null;
			}
			if (_menuCommandManager != null)
			{
				_menuCommandManager.Dispose();
				_menuCommandManager = null;
			}
		}

		internal MdiTabItem GetCloseButton()
		{
			if (_showCloseButton)
			{
				return _closeButton;
			}
			return null;
		}

		internal MdiTabItem GetActiveFilesButton()
		{
			if (_useActiveFilesMenu)
			{
				return _activeFilesButton;
			}
			return null;
		}

		internal Rectangle GetTabsBounds(Rectangle rectangle)
		{
			return _tabsBounds;
		}

		internal void MoveTab(MdiTabItem movedTab, MdiTabItem targetTab)
		{
			int x = 0;
			if (GetCount() > 0)
			{
				x = ((MdiTabItem)_tabs[0]).GetLocation().X;
			}
			_tabs.Remove(movedTab);
			int targetIndex = _tabs.IndexOf(targetTab);
			if (movedTab.GetLocation().X < targetTab.GetLocation().X)
			{
				targetIndex++;
			}
			_tabs.Insert(targetIndex, movedTab);
			foreach (MdiTabItem item in _tabs)
			{
				item.SetLocation(new Point(x, item.GetLocation().Y));
				x += item.GetItemSize().Width + item.GetTabOverlap();
			}
		}

		internal void AddTab(MdiTabItem tab)
		{
			_tabs.Add(tab);
			MxCommand uiCommand = new MxCommand("Command" + _tabs.Count, tab.GetText());
			uiCommand.Tag = tab;
			if (tab.GetChildTab() != null)
			{
				CommandImageSource imageSource = tab.GetChildTab().CreateImageSource();
				if (imageSource != null)
				{
					uiCommand.Icon = imageSource._icon;
				}
			}
			_activeFilesMenu.Commands.Add(uiCommand);
			_activeFilesMenu.Commands.SortCommands();
		}

		internal int GetCount()
		{
			return _tabs.Count;
		}

		internal int GetFirstVisibleIndex()
		{
			if (_firstVisibleIndex >= GetCount())
			{
				_firstVisibleIndex = GetCount() - 1;
			}
			else if (_firstVisibleIndex == -1 && GetCount() > 0)
			{
				_firstVisibleIndex = 0;
			}
			return _firstVisibleIndex;
		}

		internal void SetFirstVisibleIndex(int index)
		{
			int maxIndex = GetMaxFirstVisibleIndex();
			if (index >= maxIndex)
			{
				index = maxIndex;
			}
			if (index < 0)
			{
				index = ((GetCount() == 0) ? (-1) : 0);
			}
			if (_firstVisibleIndex != index)
			{
				_firstVisibleIndex = index;
				UpdateOverflowState();
				PositionCloseButton();
			}
		}

		internal void PositionCloseButton()
		{
			if (_tabGroup != null && _tabGroup.PanelManager != null && _tabGroup.PanelManager.TabbedMdiSettings.ShowCloseButtonOnSelectedTab && _tabGroup.SelectedTab != null && _closeButton != null)
			{
				Rectangle rectangle = GetTabBounds(FindTab(_tabGroup.SelectedTab), false);
				_closeButton.SetLocation(new Point(rectangle.Right - CloseButtonRightOffset, _closeButton.GetLocation().Y));
			}
		}

		private void UpdateOverflowState()
		{
			_hasOverflow = false;
			if (_useActiveFilesMenu)
			{
				if (_tabs.Count <= 1)
				{
					return;
				}
				foreach (MdiTabItem item in _tabs)
				{
					item.SetIsVisible(item.GetBounds().Right < _tabsBounds.Right - 1);
					if (!item.GetIsVisible())
					{
						_hasOverflow = true;
					}
				}
				MdiTabItem firstTab = _tabs[0] as MdiTabItem;
				firstTab.SetIsVisible(true);
				return;
			}
			if (_scrollLeftButton != null)
			{
				_scrollLeftButton._isDisabled = GetScrollOffset() == 0;
			}
			if (_scrollRightButton != null)
			{
				if (GetCount() == 0)
				{
					_scrollRightButton._isDisabled = true;
				}
				else
				{
					Rectangle rectangle = GetTabBounds(GetCount() - 1, false);
					if (rectangle.IsEmpty)
					{
						_scrollRightButton._isDisabled = false;
					}
					else
					{
						_scrollRightButton._isDisabled = rectangle.Right < _tabsBounds.Right;
					}
				}
			}
			if ((_scrollLeftButton != null && !_scrollLeftButton._isDisabled) || (_scrollRightButton != null && !_scrollRightButton._isDisabled))
			{
				_hasOverflow = true;
			}
		}

		internal Rectangle GetTabBounds(int index, bool useBodyBounds)
		{
			if (index < GetFirstVisibleIndex())
			{
				return Rectangle.Empty;
			}
			Rectangle result = ((!useBodyBounds) ? GetTab(index).GetBounds() : GetTab(index).GetBodyBounds());
			int scrollOffset = GetScrollOffset();
			result.Offset(-scrollOffset, 0);
			if (result.X > _tabsBounds.Right)
			{
				return Rectangle.Empty;
			}
			return result;
		}

		internal Rectangle GetTabBounds(MdiTabItem tab, bool useBodyBounds)
		{
			int index = _tabs.IndexOf(tab);
			return GetTabBounds(index, useBodyBounds);
		}

		internal bool EnsureTabVisible(Form form)
		{
			int index = IndexOfForm(form);
			if (index != -1)
			{
				if (index < GetFirstVisibleIndex())
				{
					SetFirstVisibleIndex(index);
					return true;
				}
				Rectangle rectangle = GetTabBounds(index, false);
				if (rectangle.IsEmpty || rectangle.Right > _tabsBounds.Right)
				{
					if (_useActiveFilesMenu)
					{
						MdiTabItem formTab = FindTabByForm(form);
						if (formTab != null)
						{
							_tabGroup.MoveTabToStart(formTab.GetChildTab());
							return true;
						}
					}
					int usedWidth = _edgePadding;
					for (int tabIndex = index; tabIndex >= 0; tabIndex--)
					{
						usedWidth += GetTab(tabIndex).GetItemSize().Width + GetTab(tabIndex).GetTabOverlap();
						if (usedWidth > _tabsBounds.Width)
						{
							if (tabIndex == index)
							{
								SetFirstVisibleIndex(index);
							}
							else
							{
								SetFirstVisibleIndex(tabIndex + 1);
							}
							return true;
						}
					}
					SetFirstVisibleIndex(0);
					return true;
				}
			}
			return false;
		}

		internal int GetMaxFirstVisibleIndex()
		{
			int usedWidth = 0;
			for (int tabIndex = GetCount() - 1; tabIndex >= 0; tabIndex--)
			{
				usedWidth += GetTab(tabIndex).GetItemSize().Width;
				if (usedWidth > _tabsBounds.Width)
				{
					if (tabIndex == GetCount() - 1)
					{
						return tabIndex;
					}
					return tabIndex + 1;
				}
			}
			return 0;
		}

		internal int GetScrollOffset()
		{
			if (GetFirstVisibleIndex() >= 0)
			{
				return GetTab(_firstVisibleIndex).GetLocation().X - _edgePadding;
			}
			return 0;
		}

		internal Size LayoutTabs(Graphics graphics, Font font, int integer, Rectangle bounds, int fixedHeight)
		{
			int x = _edgePadding;
			int tabHeight = 0;
			int maxTabSize = _tabGroup.PanelManager.TabbedMdiSettings.MaxTabSize;
			if (fixedHeight != -1)
			{
				tabHeight = fixedHeight;
			}
			else
			{
				foreach (MdiTabItem item in _tabs)
				{
					int itemHeight = item.MeasureTabHeight(graphics, _hasSlantedTabs, _tabGroup.ResolveTabStyle(item.GetChildTab(), item.GetButtonState(), false), font);
					if (itemHeight > tabHeight)
					{
						tabHeight = itemHeight;
					}
				}
			}
			int buttonWidth = ButtonSize;
			int buttonHeight = ButtonSize;
			foreach (MdiTabItem tab in _tabs)
			{
				tab.SetLocation(new Point(x, 0));
				tab.SetItemSize(tab.MeasureTabSize(graphics, _hasSlantedTabs, _tabGroup.ResolveTabStyle(tab.GetChildTab(), tab.GetButtonState(), false), font, tabHeight, _tabGroup.SelectedTab == tab.GetChildTab()));
				if (maxTabSize > 0 && tab.GetItemSize().Width > maxTabSize)
				{
					tab.SetItemSize(new Size(maxTabSize, tab.GetItemSize().Height));
				}
				if (tab._closeButton != null)
				{
					Rectangle rectangle = GetTabBounds(tab, false);
					tab._closeButton.SetLocation(new Point(rectangle.Right - CloseButtonRightOffset, bounds.Y + TabCloseButtonTop + (tabHeight - buttonHeight) / 2));
					tab._closeButton.SetItemSize(new Size(buttonWidth, buttonHeight));
				}
				x += tab.GetItemSize().Width + tab.GetTabOverlap();
			}
			x += _edgePadding;
			_hasButtons = false;
			int rightEdge = bounds.Right - ButtonGap;
			if (_showCloseButton)
			{
				if (_tabGroup.PanelManager.TabbedMdiSettings.ShowCloseButtonOnSelectedTab && _tabGroup.SelectedTab != null)
				{
					_closeButton = new MdiTabItem(TabButtonType.Close, null, _tabGroup, useCompatibleTextRendering: false, closeButtonOnTab: false);
					Rectangle selectedTabBounds = GetTabBounds(FindTab(_tabGroup.SelectedTab), false);
					_closeButton.SetLocation(new Point(selectedTabBounds.Right - CloseButtonRightOffset, bounds.Y + TabCloseButtonTop + (tabHeight - buttonHeight) / 2));
					_closeButton.SetItemSize(new Size(buttonWidth, buttonHeight));
				}
				else
				{
					rightEdge -= buttonWidth;
					_closeButton = new MdiTabItem(TabButtonType.Close, null, _tabGroup, useCompatibleTextRendering: false, _showCloseButtonOnSelectedTab);
					_closeButton.SetLocation(new Point(rightEdge, bounds.Y + ButtonTop + (tabHeight - buttonHeight) / 2));
					_closeButton.SetItemSize(new Size(buttonWidth, buttonHeight));
					_hasButtons = true;
				}
			}
			else
			{
				_closeButton = null;
			}
			if (_useActiveFilesMenu)
			{
				rightEdge -= buttonWidth;
				_activeFilesButton = new MdiTabItem(TabButtonType.ActiveFiles, null, _tabGroup, useCompatibleTextRendering: false, _showCloseButtonOnSelectedTab);
				_activeFilesButton.SetLocation(new Point(rightEdge, bounds.Y + ButtonTop + (tabHeight - buttonHeight) / 2));
				_activeFilesButton.SetItemSize(new Size(buttonWidth, buttonHeight));
				_hasButtons = true;
			}
			else
			{
				_activeFilesButton = null;
			}
			buttonWidth = ScrollButtonWidth;
			if (_hasButtons)
			{
				rightEdge -= ButtonGap;
			}
			if (_tabScrollButtons == TabScrollButtons.Always || (_tabScrollButtons == TabScrollButtons.Automatic && rightEdge < x))
			{
				rightEdge -= buttonWidth;
				_scrollRightButton = new MdiTabItem(TabButtonType.ScrollRight, null, _tabGroup, useCompatibleTextRendering: false, closeButtonOnTab: false);
				_scrollRightButton.SetLocation(new Point(rightEdge, bounds.Y + ButtonTop + (tabHeight - buttonHeight) / 2));
				_scrollRightButton.SetItemSize(new Size(buttonWidth, buttonHeight));
				rightEdge -= buttonWidth;
				_scrollLeftButton = new MdiTabItem(TabButtonType.ScrollLeft, null, _tabGroup, useCompatibleTextRendering: false, closeButtonOnTab: false);
				_scrollLeftButton.SetLocation(new Point(rightEdge, bounds.Y + ButtonTop + (tabHeight - buttonHeight) / 2));
				_scrollLeftButton.SetItemSize(new Size(buttonWidth, buttonHeight));
				_hasButtons = true;
				rightEdge -= ButtonGap;
			}
			else
			{
				if (rightEdge >= x)
				{
					SetFirstVisibleIndex(0);
				}
				_scrollLeftButton = null;
				_scrollRightButton = null;
			}
			if (!_hasButtons)
			{
				rightEdge = bounds.Right;
			}
			_tabsBounds = Rectangle.FromLTRB(bounds.X, bounds.Y, rightEdge, bounds.Bottom);
			UpdateOverflowState();
			return new Size(x, tabHeight);
		}

		internal MdiTabItem GetScrollLeftButton()
		{
			return _scrollLeftButton;
		}

		internal MdiTabItem GetScrollRightButton()
		{
			return _scrollRightButton;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _tabs.GetEnumerator();
		}

		internal MdiTabItem GetTab(int index)
		{
			return (MdiTabItem)_tabs[index];
		}

		internal MdiTabItem FindTab(MdiChildTab childTab)
		{
			foreach (MdiTabItem item in _tabs)
			{
				if (item.GetChildTab() == childTab)
				{
					return item;
				}
			}
			return null;
		}

		internal MdiTabItem GetHotItem()
		{
			return _hotItem;
		}

		internal void SetHotItem(MdiTabItem hotItem)
		{
			if (_hotItem == hotItem)
			{
				return;
			}
			if (_hotItem != null)
			{
				_hotItem._isHot = false;
				if (_hotItem._closeButton != null)
				{
					_hotItem._closeButton._isHot = false;
				}
			}
			_hotItem = hotItem;
			if (_hotItem != null)
			{
				_hotItem._isHot = true;
			}
		}

		internal MdiTabItem FindTabByForm(Form form)
		{
			foreach (MdiTabItem item in _tabs)
			{
				if (item.GetChildTab().Form == form)
				{
					return item;
				}
			}
			return null;
		}

		internal int IndexOfForm(Form form)
		{
			for (int i = 0; i < _tabs.Count; i++)
			{
				if (GetTab(i).GetChildTab().Form == form)
				{
					return i;
				}
			}
			return -1;
		}

		internal MdiTabItem HitTest(Form form, int x, int y)
		{
			if (_showCloseButton && _closeButton.HitTest(false, x, y))
			{
				return _closeButton;
			}
			if (_useActiveFilesMenu && _activeFilesButton != null && _activeFilesButton.HitTest(false, x, y))
			{
				return _activeFilesButton;
			}
			if (_scrollLeftButton != null && _scrollLeftButton.HitTest(false, x, y))
			{
				return _scrollLeftButton;
			}
			if (_scrollRightButton != null && _scrollRightButton.HitTest(false, x, y))
			{
				return _scrollRightButton;
			}
			int formIndex = IndexOfForm(form);
			if (formIndex != -1)
			{
				MdiTabItem formTab = GetTab(formIndex);
				if (formTab.HitTestBounds(GetTabBounds(formIndex, false), _hasSlantedTabs, x, y))
				{
					return formTab;
				}
			}
			for (int i = 0; i < GetCount(); i++)
			{
				MdiTabItem tab = GetTab(i);
				if (tab.HitTestBounds(GetTabBounds(i, false), _hasSlantedTabs, x, y))
				{
					return tab;
				}
			}
			return null;
		}

		private void CreateActiveFilesMenu()
		{
			_menuCommandManager = new MxCommandManager();
			_activeFilesMenu = new MxContextMenu(_menuCommandManager, "ActiveFilesMenu");
			_activeFilesMenu.CommandClick += OnActiveFilesMenuCommandClick;
		}

		internal bool GetIsMenuOpen()
		{
			return _isMenuOpen;
		}

		internal void SetIsMenuOpen(bool isMenuOpen)
		{
			if (_isMenuOpen != isMenuOpen)
			{
				_isMenuOpen = isMenuOpen;
				_tabGroup.Invalidate(_activeFilesButton.GetBounds());
			}
		}

		internal void ShowActiveFilesMenu()
		{
			Point p = new Point(_activeFilesButton.GetBounds().X, _activeFilesButton.GetBounds().Bottom);
			SetIsMenuOpen(true);
			p = _tabGroup.PointToScreen(p);
			_activeFilesMenu.Show(p);
			SetIsMenuOpen(false);
		}

		private void OnActiveFilesMenuCommandClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.Tag is MdiTabItem uiMdiChildTabLayoutState)
			{
				if (uiMdiChildTabLayoutState.GetBounds().Right > _tabsBounds.Right - 1)
				{
					_tabGroup.MoveTabToStart(uiMdiChildTabLayoutState.GetChildTab());
				}
				_tabGroup.SetPressedTabLayout(uiMdiChildTabLayoutState);
			}
		}
	}
}
