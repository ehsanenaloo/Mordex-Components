using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabStripRenderer
	{
		private void InvalidateRegion(Rectangle rc)
		{
			if (IsInGlassCaption())
			{
				_ribbon.RefreshWindowFrame(true);
			}
			else
			{
				_ribbon.Invalidate(rc);
			}
		}

		public void MarkTabRenderersDirty()
		{
			_areTabRenderersDirty = true;
		}

		public Rectangle GetContextualTabBounds(RibbonContextualTab ribbonContextualTab)
		{
			int leftEdge = -1;
			int rightEdge = 0;
			if (_tabRenderers != null)
			{
				foreach (RibbonTabRenderer item in _tabRenderers)
				{
					if (ribbonContextualTab.Tabs.Contains(item.GetTab()))
					{
						if (leftEdge == -1 || item._bounds.Left < leftEdge)
						{
							leftEdge = item._bounds.Left;
						}
						if (item._bounds.Right > rightEdge)
						{
							rightEdge = item._bounds.Right;
						}
					}
				}
			}
			if (rightEdge <= 0)
			{
				return Rectangle.Empty;
			}
			return new Rectangle(leftEdge, 0, rightEdge - leftEdge, 0);
		}

		internal Rectangle GetVisibleTabBounds(RibbonTabRenderer ribbonTabRenderer)
		{
			Rectangle bounds = ribbonTabRenderer._bounds;
			if (_leftScrollButton._isVisible && bounds.X < _leftScrollButton._bounds.Right)
			{
				int width = bounds.Width - (_leftScrollButton._bounds.Right - bounds.X);
				bounds.X = _leftScrollButton._bounds.Right;
				bounds.Width = width;
			}
			if (_rightScrollButton._isVisible && bounds.Right > _rightScrollButton._bounds.X)
			{
				int rightClippedWidth = bounds.Width - (bounds.Right - _rightScrollButton._bounds.X);
				bounds.Width = rightClippedWidth;
			}
			if (bounds.Right > _minimizeButton._bounds.X && !_minimizeButton._bounds.IsEmpty)
			{
				int buttonClippedWidth = bounds.Width - (bounds.Right - _minimizeButton._bounds.X);
				bounds.Width = buttonClippedWidth;
			}
			return bounds;
		}

		public void RebuildTabRenderers()
		{
			_areTabRenderersDirty = false;
			if (_ribbon != null)
			{
				_ribbon.Controls.Clear();
			}
			StopItemAnimations();
			_tabRenderers = new List<RibbonTabRenderer>();
			if (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.BackstageMenu || _ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxTab)
			{
				_backstageTabRenderer = new BackstageMenuTabRenderer(_ribbon.BackstageMenu);
				_tabRenderers.Add(_backstageTabRenderer);
			}
			foreach (RibbonTab tab in _ribbon.Tabs)
			{
				if (tab.Visible)
				{
					_tabRenderers.Add(new RibbonTabRenderer(tab, _ribbon));
				}
			}
			foreach (RibbonContextualTab item in _ribbon.GetContextualTabList())
			{
				foreach (RibbonTab contextualChildTab in item.Tabs)
				{
					if (contextualChildTab.Visible)
					{
						_tabRenderers.Add(new RibbonTabRenderer(contextualChildTab, _ribbon, contextualTab: true));
					}
				}
			}
			if (_ribbon.ShouldShowAddNewItems())
			{
				_tabRenderers.Add(new AddNewTabRenderer(_ribbon));
			}
		}

		public void LayoutTabStrip(Graphics graphics, Rectangle stripBounds, int unusedOffset)
		{
			if (_areTabRenderersDirty)
			{
				RebuildTabRenderers();
			}
			if (stripBounds.Width <= 0 || stripBounds.Height <= 0)
			{
				_stripBounds = Rectangle.Empty;
				_tabsBounds = Rectangle.Empty;
				return;
			}
			_stripBounds = stripBounds;
			int availableWidth = stripBounds.Width - _defaultFirstTabOffset;
			int buttonSize = _ribbon.GetSmallItemHeight();
			int rightEdge = stripBounds.Right;
			if (_ribbon.IsMdiChildMaximized())
			{
				if (_mdiControlBox == null)
				{
					_mdiControlBox = new RibbonMdiControlBox(_ribbon);
				}
				int mdiBoxWidth = buttonSize * 3;
				Rectangle rectangle = new Rectangle(stripBounds.Right - mdiBoxWidth - 1, stripBounds.Y + (stripBounds.Height - buttonSize) / 2, mdiBoxWidth, buttonSize);
				_mdiControlBox.LayoutButtons(rectangle, 0);
				rightEdge -= mdiBoxWidth;
				availableWidth -= mdiBoxWidth;
			}
			else
			{
				_mdiControlBox = null;
			}
			if (_ribbon.HelpButton.Visible)
			{
				Rectangle helpButtonBounds = new Rectangle(rightEdge - buttonSize - 1, stripBounds.Y + (stripBounds.Height - buttonSize) / 2, buttonSize, buttonSize);
				_helpButton._isVisible = true;
				_helpButton._bounds = helpButtonBounds;
				_helpButton._isEnabled = _ribbon.HelpButton.Enabled;
				availableWidth -= buttonSize;
				rightEdge -= buttonSize;
			}
			else
			{
				_helpButton._isVisible = false;
			}
			if (_ribbon.AllowMinimize)
			{
				Rectangle minimizeButtonBounds = new Rectangle(rightEdge - buttonSize - 1, stripBounds.Y + (stripBounds.Height - buttonSize) / 2, buttonSize, buttonSize);
				_minimizeButton._isVisible = true;
				_minimizeButton._bounds = minimizeButtonBounds;
				_minimizeButton._isEnabled = !_ribbon.IsBackstageOpen();
				availableWidth -= buttonSize;
			}
			else
			{
				_minimizeButton._isVisible = false;
			}
			_tabsBounds = new Rectangle(stripBounds.X, stripBounds.Y, availableWidth, stripBounds.Height);
			if (_tabRenderers.Count == 0)
			{
				return;
			}
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			int totalTabsWidth = 0;
			RibbonContextualTab ribbonContextualTab = null;
			int contextualGapsWidth = 0;
			int backstageTabWidth = 0;
			foreach (RibbonTabRenderer item in _tabRenderers)
			{
				if (!item._isVisible)
				{
					continue;
				}
				item.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
				totalTabsWidth += item._fullItemWidth;
				if (item is BackstageMenuTabRenderer)
				{
					backstageTabWidth = item._fullItemWidth;
				}
				if (item.GetTab() != null && item.GetTab().GetTabOwner() is RibbonContextualTab)
				{
					if (ribbonContextualTab != null && item.GetTab().GetTabOwner() != ribbonContextualTab)
					{
						totalTabsWidth += 2;
						contextualGapsWidth += 2;
					}
					ribbonContextualTab = item.GetTab().GetTabOwner() as RibbonContextualTab;
				}
			}
			int firstTabOffset = Math.Max(_ribbon.FirstTabOffset, backstageTabWidth + 2);
			availableWidth += _defaultFirstTabOffset - firstTabOffset;
			_tabsBounds = new Rectangle(stripBounds.X + firstTabOffset, stripBounds.Y, availableWidth, stripBounds.Height);
			int freeSpace = _tabsBounds.Width - totalTabsWidth;
			_areTabsCompressed = false;
			if (freeSpace > 0)
			{
				int extraPerTab = freeSpace / _tabRenderers.Count;
				if (extraPerTab > 11)
				{
					extraPerTab = 11;
				}
				foreach (RibbonTabRenderer stretchTab in _tabRenderers)
				{
					stretchTab._extraWidth = extraPerTab;
				}
				if (extraPerTab < 11)
				{
					freeSpace -= extraPerTab * _tabRenderers.Count;
					for (int i = 0; i < freeSpace; i++)
					{
						_tabRenderers[i]._extraWidth++;
					}
				}
			}
			else if (totalTabsWidth > _tabsBounds.Width)
			{
				totalTabsWidth -= contextualGapsWidth;
				contextualGapsWidth = 0;
				_areTabsCompressed = true;
				int targetTabWidth = (_tabsBounds.Width - GetScrollOffset()) / _tabRenderers.Count;
				if (targetTabWidth < _ribbon.TabMinWidth)
				{
					targetTabWidth = _ribbon.TabMinWidth;
				}
				int spareWidth = 0;
				ArrayList arrayList = new ArrayList();
				foreach (RibbonTabRenderer measureTab in _tabRenderers)
				{
					measureTab._extraWidth = 0;
					if (measureTab._fullItemWidth > targetTabWidth)
					{
						arrayList.Add(measureTab);
					}
					else
					{
						spareWidth += targetTabWidth - measureTab._fullItemWidth;
					}
				}
				if (spareWidth > 0 && arrayList.Count > 0)
				{
					foreach (RibbonTabRenderer oversizedTab in arrayList)
					{
						oversizedTab.SetLayoutWidth(targetTabWidth);
					}
					while (spareWidth > 0)
					{
						for (int j = 0; j < arrayList.Count; j++)
						{
							RibbonTabRenderer ribbonTabRenderer = (RibbonTabRenderer)arrayList[j];
							if (ribbonTabRenderer._fullItemWidth > ribbonTabRenderer.GetLayoutWidth())
							{
								ribbonTabRenderer.SetLayoutWidth(ribbonTabRenderer.GetLayoutWidth() + 1);
								spareWidth--;
								if (spareWidth <= 0)
								{
									break;
								}
							}
							else
							{
								arrayList.Remove(ribbonTabRenderer);
								j--;
							}
						}
						if (arrayList.Count == 0)
						{
							spareWidth = 0;
						}
					}
				}
			}
			int tabLeft = _tabsBounds.X;
			ribbonContextualTab = null;
			foreach (RibbonTabRenderer layoutTab in _tabRenderers)
			{
				if (contextualGapsWidth > 0 && layoutTab.GetTab() != null && layoutTab.GetTab().GetTabOwner() is RibbonContextualTab)
				{
					if (ribbonContextualTab != null && layoutTab.GetTab().GetTabOwner() != ribbonContextualTab)
					{
						tabLeft += 2;
					}
					ribbonContextualTab = layoutTab.GetTab().GetTabOwner() as RibbonContextualTab;
				}
				if (layoutTab is BackstageMenuTabRenderer)
				{
					layoutTab.ArrangeItem(graphics, textRenderer, stripBounds.X, _tabsBounds.Y + 2, layoutTab.GetLayoutWidth(), _tabsBounds.Height - 2);
					continue;
				}
				layoutTab.ArrangeItem(graphics, textRenderer, tabLeft + 2, _tabsBounds.Y + 2, layoutTab.GetLayoutWidth() - 5, _tabsBounds.Height - 2);
				tabLeft += layoutTab._bounds.Width + 5;
			}
			_totalTabsWidth = tabLeft - 5;
			if (_tabsBounds.Right < tabLeft - 5)
			{
				LayoutTabsAtScrollOffset();
				return;
			}
			_tabsClipBounds = Rectangle.Empty;
			_leftScrollButton._isVisible = false;
			_rightScrollButton._isVisible = false;
			SetRightScrollHot(false);
			SetLeftScrollHot(false);
			SetMinimizeButtonHot(false);
			SetHelpButtonHot(false);
		}

		public void LayoutTabsAtScrollOffset()
		{
			int scrollOffset = GetScrollOffset();
			if (GetScrollOffset() < 0)
			{
				int minOffset = _tabsBounds.Right - _totalTabsWidth;
				if (minOffset > GetScrollOffset())
				{
					scrollOffset = minOffset;
				}
			}
			int tabLeft = _tabsBounds.X + scrollOffset;
			foreach (RibbonTabRenderer item in _tabRenderers)
			{
				if (!(item is BackstageMenuTabRenderer))
				{
					item.MoveToX(tabLeft);
					tabLeft += item._bounds.Width + 5;
				}
			}
			if (_tabsBounds.Right < _totalTabsWidth)
			{
				LayoutScrollButtons(_tabsBounds);
				return;
			}
			_tabsClipBounds = Rectangle.Empty;
			_leftScrollButton._isVisible = false;
			_rightScrollButton._isVisible = false;
			SetRightScrollHot(false);
			SetLeftScrollHot(false);
			SetMinimizeButtonHot(false);
			SetHelpButtonHot(false);
			StopTimer();
		}

		private void LayoutScrollButtons(Rectangle rectangle)
		{
			int visibleLeft = _tabsBounds.X;
			int width = _tabsBounds.Width;
			if (GetScrollOffset() < 0)
			{
				_leftScrollButton._isVisible = true;
				_leftScrollButton._bounds = new Rectangle(rectangle.X + 1, rectangle.Y + 1, 13, rectangle.Height - 2);
				visibleLeft = _leftScrollButton._bounds.Right;
			}
			else
			{
				_leftScrollButton._isVisible = false;
				SetLeftScrollHot(false);
				if (_isLeftScrollPressed)
				{
					StopTimer();
				}
			}
			if (_tabRenderers.Count > 0 && _tabRenderers[_tabRenderers.Count - 1]._bounds.Right > rectangle.Right)
			{
				_rightScrollButton._isVisible = true;
				int x = rectangle.Right - 1 - 13;
				_rightScrollButton._bounds = new Rectangle(x, rectangle.Y + 1, 13, rectangle.Height - 2);
				width = _rightScrollButton._bounds.Left - visibleLeft;
			}
			else
			{
				_rightScrollButton._isVisible = false;
				SetRightScrollHot(false);
				if (_isRightScrollPressed)
				{
					StopTimer();
				}
			}
			_tabsClipBounds = new Rectangle(visibleLeft, rectangle.Y, width, rectangle.Height + 1);
		}

		public RibbonTabRenderer GetTabAt(int pointX, int pointY)
		{
			if (_tabRenderers != null)
			{
				foreach (RibbonTabRenderer item in _tabRenderers)
				{
					if (item._bounds.Contains(pointX, pointY))
					{
						return item;
					}
				}
			}
			return null;
		}

		public bool IsInGlassCaption()
		{
			if (_ribbon != null && _ribbon.ShowCaptionBar && _ribbon._headerRenderer.GetFormHandle() != IntPtr.Zero)
			{
				return _ribbon.IsGlassCaptionActive();
			}
			return false;
		}
	}
}
