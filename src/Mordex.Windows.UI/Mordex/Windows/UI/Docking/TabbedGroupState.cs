using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class TabbedGroupState : DockPanelGroupState
	{
		// Vertical drag start point, in group client coordinates, when a tab is dragged out of the strip.
		private const int TabDragStartY = 10;

		internal DockTabStripLayout _tabStrip;

		private DockTabItem _pressedTab;

		private Rectangle _pressedTabBounds;

		private DockPanelBase _hotPanel;

		internal override Size MinimumSize
		{
			get
			{
				int minWidth = -1;
				int minHeight = -1;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					Size minimumSize = item.GetMinimumSize();
					if (minHeight < minimumSize.Height)
					{
						minHeight = minimumSize.Height;
					}
					if (minWidth < minimumSize.Width)
					{
						minWidth = minimumSize.Width;
					}
				}
				if (_tabStrip != null)
				{
					minHeight += _tabStrip.GetBounds().Height;
				}
				return new Size(minWidth, minHeight);
			}
		}

		internal override Size MaximumSize
		{
			get
			{
				int maxWidth = 0;
				int maxHeight = 0;
				foreach (DockPanelBase item in GetVisiblePanels())
				{
					Size maximumSize = item.GetMaximumSize();
					if (maxHeight > maximumSize.Height || maxHeight == 0)
					{
						maxHeight = maximumSize.Height;
					}
					if (maxWidth > maximumSize.Width || maxWidth == 0)
					{
						maxWidth = maximumSize.Width;
					}
				}
				if (maxHeight > 0 && _tabStrip != null)
				{
					maxHeight += _tabStrip.GetBounds().Height;
				}
				return new Size(maxWidth, maxHeight);
			}
		}

		internal TabbedGroupState(MxDockPanelGroup panelGroup)
			: base(panelGroup)
		{
		}

		internal override void RefreshGroupLayout()
		{
			if (GetVisiblePanels().Count > 0 && (GetSelectedPanel() == null || GetSelectedPanel().Closed || GetSelectedPanel().ParentGroup != _panelGroup))
			{
				SetSelectedPanel((DockPanelBase)GetVisiblePanels()[0]);
			}
			if (GetSelectedPanel() != null)
			{
				GetSelectedPanel().BringToFront();
			}
			if (ContentBounds.IsEmpty)
			{
				return;
			}
			int tabStripHeight = 0;
			if (GetVisiblePanels().Count > 1 && _panelGroup.GetAutoHidePanel() == null)
			{
				_tabStrip = new DockTabStripLayout();
				for (int i = 0; i < GetVisiblePanels().Count; i++)
				{
					DockTabItem tab = new DockTabItem((DockPanelBase)GetVisiblePanels()[i], _panelGroup);
					if (tab.GetPanel() == _hotPanel)
					{
						tab._isHot = true;
					}
					switch (_panelGroup.GetEffectiveTabDisplay())
					{
					case TabDisplayMode.Image:
						tab.SetShowImage(true);
						tab.SetShowText(false);
						break;
					case TabDisplayMode.Text:
						tab.SetShowImage(false);
						tab.SetShowText(true);
						break;
					case TabDisplayMode.ImageAndTextOnSelected:
						tab.SetShowImage(true);
						if (tab.GetPanel() != _panelGroup.SelectedPanel)
						{
							tab.SetShowText(false);
						}
						break;
					}
					_tabStrip.AddItem(tab);
				}
				Graphics graphics = _panelGroup.CreateGraphics();
				Size size = _tabStrip.LayoutItems(graphics, _panelGroup.Font, 1, 0);
				graphics.Dispose();
				tabStripHeight = size.Height + 1;
				Rectangle displayRectangle = ContentBounds;
				if (_panelGroup.GetEffectiveTabAlignment() == TabAlignment.Bottom)
				{
					displayRectangle.Y = displayRectangle.Bottom - tabStripHeight;
				}
				displayRectangle.Height = tabStripHeight;
				_tabStrip.SetBounds(displayRectangle);
				_tabStrip.ShrinkToFit(size, displayRectangle.Size);
			}
			else
			{
				_tabStrip = null;
			}
			Rectangle panelBounds = ContentBounds;
			panelBounds.Height -= tabStripHeight;
			if (_panelGroup.GetEffectiveTabAlignment() == TabAlignment.Top)
			{
				panelBounds.Y += tabStripHeight;
			}
			foreach (DockPanelBase item in GetVisiblePanels())
			{
				item.Bounds = panelBounds;
			}
		}

		internal override BorderEdges GetChildPanelEdges(DockPanelBase panel)
		{
			if (!panel.AutoHide)
			{
				if (_panelGroup.HasPanelBorder())
				{
					return BorderEdges.None;
				}
				if (_tabStrip == null)
				{
					return base.GetChildPanelEdges(panel);
				}
				if (_panelGroup.GetEffectiveTabAlignment() == TabAlignment.Top)
				{
					return BorderEdges.Left | BorderEdges.Right | BorderEdges.Bottom;
				}
				return BorderEdges.Left | BorderEdges.Top | BorderEdges.Right;
			}
			return base.GetChildPanelEdges(panel);
		}

		internal DockTabItem HitTestTab(int x, int y)
		{
			if (_tabStrip != null)
			{
				foreach (DockTabItem item in (IEnumerable)_tabStrip)
				{
					Rectangle rectangle = item.GetBounds();
					rectangle.Offset(_tabStrip.GetBounds().Location);
					if (rectangle.Contains(x, y))
					{
						return item;
					}
				}
			}
			return null;
		}

		internal override PanelDropInfo GetDropInfo(DockPanelBase panel, Point screenPoint)
		{
			if (_tabStrip == null)
			{
				return null;
			}
			Point pt = _panelGroup.PointToClient(screenPoint);
			if (_tabStrip.GetBounds().Contains(pt))
			{
				PanelDropInfo panelDropInfo = new PanelDropInfo();
				panelDropInfo._groupStyle = PanelGroupStyle.Tab;
				panelDropInfo._createsGroup = false;
				panelDropInfo._targetPanel = _panelGroup;
				panelDropInfo._dropBounds = GetSelectedPanel().Bounds;
				panelDropInfo._dropBounds.Location = _panelGroup.PointToScreen(panelDropInfo._dropBounds.Location);
				panelDropInfo._tabHeaderBounds = Rectangle.Empty;
				if (pt.X <= _tabStrip.GetEdgePadding())
				{
					panelDropInfo._insertIndex = 0;
					panelDropInfo._tabHeaderBounds = _tabStrip.GetItem(0).GetBounds();
				}
				else
				{
					DockTabItem hitTab = HitTestTab(pt.X, pt.Y);
					if (hitTab != null)
					{
						panelDropInfo._insertIndex = hitTab.GetPanel().GetIndexInParent();
						if (panel.ParentGroup == _panelGroup && panel.GetIndexInParent() < panelDropInfo._insertIndex)
						{
							panelDropInfo._insertIndex++;
						}
						panelDropInfo._tabHeaderBounds = hitTab.GetBounds();
					}
					else
					{
						panelDropInfo._insertIndex = _panelGroup.Panels.Count;
						Rectangle rectangle = _tabStrip.GetItem(_tabStrip.GetCount() - 1).GetBounds();
						if (panel.ParentGroup != _panelGroup)
						{
							rectangle.X = rectangle.Right;
						}
						panelDropInfo._tabHeaderBounds = rectangle;
					}
				}
				panelDropInfo._tabHeaderBounds.Offset(_tabStrip.GetBounds().Location);
				panelDropInfo._tabHeaderBounds.Location = _panelGroup.PointToScreen(panelDropInfo._tabHeaderBounds.Location);
				if (panelDropInfo._tabHeaderBounds.Width > panelDropInfo._dropBounds.Width)
				{
					panelDropInfo._tabHeaderBounds.Width = panelDropInfo._dropBounds.Width;
				}
				if (panelDropInfo._tabHeaderBounds.Right > panelDropInfo._dropBounds.Right)
				{
					panelDropInfo._tabHeaderBounds.X = panelDropInfo._dropBounds.Right - panelDropInfo._tabHeaderBounds.Width;
				}
				return panelDropInfo;
			}
			return null;
		}

		internal override Rectangle GetNewTabBounds()
		{
			if (_tabStrip != null)
			{
				Rectangle result = _tabStrip.GetItem(_tabStrip.GetCount() - 1).GetBounds();
				result.X = result.Right;
				if (result.Right > _tabStrip.GetBounds().Right)
				{
					result.Width /= 2;
					result.X = _tabStrip.GetBounds().Right - result.Width;
				}
				result.Offset(_tabStrip.GetBounds().Location);
				if (_panelGroup != null)
				{
					result.Location = _panelGroup.PointToScreen(result.Location);
				}
				return result;
			}
			return Rectangle.Empty;
		}

		internal override bool ProcessMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				DockTabItem hitTab = HitTestTab(eventArgs.X, eventArgs.Y);
				if (hitTab != null && hitTab.GetPanel() != null && hitTab.GetPanel().Enabled)
				{
					SetSelectedPanel(hitTab.GetPanel());
					GetSelectedPanel().Activate();
					if (!_panelGroup.StaticGroup)
					{
						_pressedTab = hitTab;
						_pressedTabBounds = _pressedTab.GetBounds();
						_pressedTabBounds.Offset(_tabStrip.GetBounds().Location);
					}
				}
			}
			return true;
		}

		internal override bool ProcessMouseMove(MouseEventArgs eventArgs)
		{
			if (_pressedTab != null)
			{
				if (!_pressedTabBounds.Contains(eventArgs.X, eventArgs.Y))
				{
					DockPanelBase draggedPanel = _pressedTab.GetPanel();
					_pressedTab = null;
					if (draggedPanel.CanDragPanel())
					{
						_panelGroup.PanelManager.StartPanelDrag(draggedPanel, eventArgs.X, TabDragStartY);
					}
				}
			}
			else if (eventArgs.Button == MouseButtons.None)
			{
				DockTabItem hitTab = HitTestTab(eventArgs.X, eventArgs.Y);
				if (hitTab == null)
				{
					if (_hotPanel != null)
					{
						_hotPanel = null;
						_panelGroup.RecalculateLayout();
					}
				}
				else if (_hotPanel != hitTab.GetPanel())
				{
					_hotPanel = hitTab.GetPanel();
					_panelGroup.RecalculateLayout();
				}
			}
			return true;
		}

		internal override bool ProcessMouseUp(MouseEventArgs eventArgs)
		{
			_pressedTab = null;
			return true;
		}

		internal override void ProcessPaint(PaintEventArgs eventArgs)
		{
			if (_tabStrip == null)
			{
				return;
			}
			DockManagerRendererBase renderer = _panelGroup.PanelManager.GetPanelRenderer();
			if (_panelGroup.SelectedPanel != null)
			{
				PanelDockStyle stripEdge = (_panelGroup.GetEffectiveTabAlignment() == TabAlignment.Top) ? PanelDockStyle.Top : PanelDockStyle.Bottom;
				renderer.DrawTabStrip(eventArgs.Graphics, _panelGroup, _tabStrip.GetBounds(), _panelGroup.PanelManager.GetTabStripStyle(), _panelGroup.SelectedPanel.GetPanelBorderColor(), stripEdge);
			}
			DockTabItem selectedTab = null;
			foreach (DockTabItem item in (IEnumerable)_tabStrip)
			{
				if (GetSelectedPanel() == item.GetPanel())
				{
					selectedTab = item;
				}
				else
				{
					PaintTab(eventArgs.Graphics, renderer, item);
				}
			}
			if (selectedTab != null)
			{
				PaintTab(eventArgs.Graphics, renderer, selectedTab);
			}
		}

		private void PaintTab(Graphics graphics, DockManagerRendererBase renderer, DockTabItem tab)
		{
			Rectangle bounds = tab.GetBounds();
			bounds.Offset(_tabStrip.GetBounds().Location);
			ButtonState buttonState = ButtonState.Normal;
			if (GetSelectedPanel() == tab.GetPanel())
			{
				buttonState = ((!tab._isHot) ? ButtonState.Selected : ButtonState.Pressed);
			}
			else if (tab._isHot)
			{
				buttonState = ButtonState.Hot;
			}
			if (!tab.GetPanel().Enabled)
			{
				buttonState = ButtonState.Disabled;
			}
			FormatStyle formatStyle = tab.GetPanel().GetTabFormatStyle(buttonState);
			renderer.DrawTabHorizontal(graphics, tab.GetPanel(), _panelGroup.GetEffectiveTabAlignment(), formatStyle, buttonState, bounds);
		}

		internal override void OnSelectedPanelChanged(bool activate)
		{
			if (GetSelectedPanel() != null)
			{
				GetSelectedPanel().BringToFront();
			}
			_panelGroup.Invalidate();
			RefreshGroupLayout();
			base.OnSelectedPanelChanged(activate);
		}

		internal override bool ShouldDragGroup()
		{
			return true;
		}

		internal override void ProcessChildDropInfo(PanelDropInfo dropInfo)
		{
			if (dropInfo._createsGroup)
			{
				if (dropInfo._groupStyle != PanelGroupStyle.VerticalTiles && dropInfo._groupStyle != PanelGroupStyle.HorizontalTiles)
				{
					dropInfo._createsGroup = false;
					dropInfo._insertIndex = GetVisiblePanels().Count;
				}
				dropInfo._targetPanel = _panelGroup;
			}
		}

		internal override bool ProcessDoubleClick()
		{
			if (!_panelGroup.StaticGroup)
			{
				Point point = _panelGroup.PointToClient(Cursor.Position);
				DockTabItem hitTab = HitTestTab(point.X, point.Y);
				if (hitTab != null && hitTab.GetPanel() != null && hitTab.GetPanel().GetEffectiveCaptionDoubleClickAction() == CaptionDoubleClickAction.ToggleDockState)
				{
					if (hitTab.GetPanel().IsFloating())
					{
						hitTab.GetPanel().DockPanel();
					}
					else
					{
						hitTab.GetPanel().UndockPanel();
					}
				}
			}
			return false;
		}

		internal override bool ProcessCloseButtonClick(DockPanelBase panel)
		{
			if (_panelGroup.IsFloating())
			{
				_panelGroup.Closed = true;
				return true;
			}
			return false;
		}

		internal override bool ProcessAutoHideButtonClick(DockPanelBase panel)
		{
			if (_panelGroup.AutoHide == panel.AutoHide)
			{
				_panelGroup.AutoHide = !_panelGroup.AutoHide;
			}
			else
			{
				panel.AutoHide = !panel.AutoHide;
			}
			if (_panelGroup.PanelManager != null)
			{
				_panelGroup.PanelManager.RestoreDockLayout();
			}
			_panelGroup.SelectedPanel = panel;
			_panelGroup.RecalculateLayout();
			if (!panel.AutoHide)
			{
				panel.Activate();
			}
			return true;
		}

		internal override int GetAccessibleChildCount()
		{
			int childCount = 0;
			if (GetSelectedPanel() != null)
			{
				childCount++;
			}
			if (_tabStrip != null)
			{
				childCount += _tabStrip.GetCount();
			}
			return childCount;
		}

		internal override AccessibleObject GetAccessibleChild(int index)
		{
			List<AccessibleObject> list = new List<AccessibleObject>();
			if (GetSelectedPanel() != null)
			{
				list.Add(GetSelectedPanel().AccessibilityObject);
			}
			if (_tabStrip != null)
			{
				foreach (DockTabItem item in (IEnumerable)_tabStrip)
				{
					list.Add(item.GetAccessibleObject());
				}
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
