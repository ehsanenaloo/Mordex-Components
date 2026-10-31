using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	[ToolboxItem(false)]
	public class MxAutoHideStrip : Control
	{
		// How long the pointer must rest on a tab before its panel slides out.
		private const int HoverShowDelayMs = 500;

		private const int GroupSpacing = 10;

		private const int TabStartOffset = 2;

		private MxDockManager _dockManager;

		private AutoHideStripDock _autoHideDock;

		private ArrayList _panels;

		private ArrayList _tabPanels;

		private Timer _hoverTimer;

		private DockPanelBasePanel _popupPanel;

		private DockPanelBase _hotPanel;

		private int _thickness;

		private int _edgePadding;

		private ArrayList _tabGroups;

		private Rectangle _tabAreaBounds;

		private Rectangle _nearCornerBounds;

		private Rectangle _farCornerBounds;

		[DefaultValue(AutoHideStripDock.Left)]
		[Localizable(false)]
		public AutoHideStripDock AutoHideDock => _autoHideDock;

		public DockPanelBase[] Panels
		{
			get
			{
				if (_tabPanels == null)
				{
					_tabPanels = new ArrayList();
				}
				DockPanelBase[] panelsCopy = new DockPanelBase[_tabPanels.Count];
				_tabPanels.CopyTo(panelsCopy);
				return panelsCopy;
			}
		}

		internal MxAutoHideStrip(MxDockManager panelManager, AutoHideStripDock autoHideDock)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			_dockManager = panelManager;
			_autoHideDock = autoHideDock;
			switch (_autoHideDock)
			{
			case AutoHideStripDock.Bottom:
				Dock = DockStyle.Bottom;
				break;
			case AutoHideStripDock.Left:
				Dock = DockStyle.Left;
				break;
			case AutoHideStripDock.Right:
				Dock = DockStyle.Right;
				break;
			case AutoHideStripDock.Top:
				Dock = DockStyle.Top;
				break;
			}
			_panels = new ArrayList();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopHoverTimer();
				try
				{
					if (_panels != null)
					{
						foreach (DockPanelBase item in _panels)
						{
							item.Dispose();
						}
					}
				}
				catch
				{
					// Dispose must not throw. A panel that fails to dispose must not keep this strip from releasing its handle.
				}
			}
			base.Dispose(disposing);
		}

		private void StartHoverTimer()
		{
			StopHoverTimer();
			_hoverTimer = new Timer();
			_hoverTimer.Interval = HoverShowDelayMs;
			_hoverTimer.Tick += OnHoverTimerTick;
			_hoverTimer.Start();
		}

		private void StopHoverTimer()
		{
			if (_hoverTimer != null)
			{
				Timer timer = _hoverTimer;
				_hoverTimer = null;
				timer.Stop();
				timer.Tick -= OnHoverTimerTick;
				timer.Dispose();
			}
		}

		private void OnHoverTimerTick(object sender, EventArgs eventArgs)
		{
			if (_hotPanel != null)
			{
				ShowPopup(_hotPanel, false);
			}
		}

		internal void ClearPopupPanel()
		{
			_popupPanel = null;
		}

		internal void HidePopup()
		{
			if (_popupPanel != null)
			{
				_popupPanel.SlideClose(false);
			}
		}

		internal DockPanelBase GetPopupTarget(DockPanelBase panel)
		{
			if (panel != null && panel.ParentGroup != null && panel.ParentGroup.AutoHide && panel.ParentGroup.StaticGroup && panel.ParentGroup.IsTabGroup() && panel.ParentGroup.SelectedPanel == panel)
			{
				return panel.ParentGroup;
			}
			return panel;
		}

		internal void ShowPopup(DockPanelBase panel, bool isActivation)
		{
			if (_dockManager != null)
			{
				PanelActionCancelEventArgs e = new PanelActionCancelEventArgs(panel, cancel: false);
				_dockManager.RaisePanelAutoHideActiveChanging(e);
				if (e.Cancel)
				{
					return;
				}
			}
			if (panel != null && panel.ParentGroup != null && panel.ParentGroup.AutoHide)
			{
				if (panel.ParentGroup.SelectedPanel != panel && panel.ParentGroup.IsTabGroup())
				{
					if (_popupPanel != null && _popupPanel.GetPanel() == panel.ParentGroup)
					{
						if (panel.ParentGroup.SelectedPanel.Focused)
						{
							panel.ParentGroup.SelectedPanel = panel;
							panel.Activate();
						}
						else
						{
							panel.ParentGroup.SelectedPanel = panel;
						}
					}
					else
					{
						panel.ParentGroup.SelectedPanel = panel;
					}
				}
				else
				{
					panel.ParentGroup.SelectedPanel = panel;
				}
			}
			panel = GetPopupTarget(panel);
			if (_popupPanel == null || _popupPanel.GetPanel() != panel)
			{
				_dockManager.HideAutoHidePanels();
			}
			if (_popupPanel != null)
			{
				return;
			}
			foreach (DockTabStripLayout group in _tabGroups)
			{
				foreach (DockTabItem tab in (IEnumerable)group)
				{
					if (tab.GetPanel().ParentGroup == panel.ParentGroup)
					{
						tab.GetPanel().SetIsAutoHideTabSelected(false);
					}
				}
			}
			panel.SetIsAutoHideTabSelected(true);
			UpdateLayout();
			if (panel.ParentGroup != null && panel.ParentGroup.AutoHide && panel.ParentGroup.SelectedPanel != panel && panel.ParentGroup.IsTabGroup())
			{
				panel.ParentGroup.SelectedPanel = panel;
			}
			_popupPanel = new DockPanelBasePanel(_dockManager, panel, this);
			_popupPanel.SlideOpen(true, isActivation);
		}

		internal void ClosePopup()
		{
			HidePopup();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			SetHotPanel(null);
			if (e.Button == MouseButtons.Left)
			{
				DockTabItem tab = HitTestTab(e.X, e.Y);
				if (tab != null && tab.GetPanel().Enabled)
				{
					tab.GetPanel().Activate();
					if (!tab.GetPanel().ContainsFocus)
					{
						tab.GetPanel().Focus();
					}
				}
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			Form form = RenderingHelper.FindForm(this);
			if (e.Button == MouseButtons.None && form != null && (form.ContainsFocus || form.ActiveControl == _dockManager.ContainerControl))
			{
				DockPanelBase panel = PanelFromPoint(e.X, e.Y);
				if (panel != null && !panel.Enabled)
				{
					panel = null;
				}
				SetHotPanel(panel);
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			SetHotPanel(null);
			base.OnMouseLeave(e);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate();
		}

		private void RebuildTabGroups()
		{
			_tabPanels = new ArrayList();
			foreach (DockPanelBase panel in _panels)
			{
				if (panel.Closed || !MxDockManager.IsSinglePanel(panel))
				{
					continue;
				}
				if (panel is MxDockPanelGroup)
				{
					MxDockPanelGroup panelGroup = (MxDockPanelGroup)panel;
					if (panelGroup.IsTabGroup())
					{
						foreach (DockPanelBase childPanel in (IEnumerable)panelGroup.Panels)
						{
							if (!childPanel.Closed)
							{
								_tabPanels.Add(childPanel);
							}
						}
					}
					else
					{
						_tabPanels.Add(panel);
					}
				}
				else
				{
					_tabPanels.Add(panel);
				}
			}
			_tabGroups = new ArrayList();
			for (int panelIndex = _tabPanels.Count - 1; panelIndex >= 0; panelIndex--)
			{
				DockPanelBase movingPanel = (DockPanelBase)_tabPanels[panelIndex];
				if (movingPanel.ParentGroup != null)
				{
					for (int previousIndex = panelIndex - 1; previousIndex >= 0; previousIndex--)
					{
						DockPanelBase earlierPanel = (DockPanelBase)_tabPanels[previousIndex];
						if (movingPanel.ParentGroup == earlierPanel.ParentGroup)
						{
							int insertIndex = previousIndex + 1;
							if (insertIndex != panelIndex)
							{
								_tabPanels.RemoveAt(panelIndex);
								_tabPanels.Insert(insertIndex, movingPanel);
								panelIndex++;
							}
							break;
						}
					}
				}
			}
			DockTabStripLayout currentGroup = null;
			for (int i = 0; i < _tabPanels.Count; i++)
			{
				DockPanelBase tabPanel = (DockPanelBase)_tabPanels[i];
				if (currentGroup == null || tabPanel.ParentGroup == null || currentGroup.GetItem(0).GetPanel().ParentGroup != tabPanel.ParentGroup)
				{
					currentGroup = new DockTabStripLayout();
					currentGroup.SetEdgePadding(0);
					_tabGroups.Add(currentGroup);
				}
				DockTabItem newTab = new DockTabItem(tabPanel, this);
				if (tabPanel == GetHotPanel())
				{
					newTab._isHot = true;
				}
				currentGroup.AddItem(newTab);
			}
			foreach (DockTabStripLayout group in _tabGroups)
			{
				bool hasVisibleTab = false;
				foreach (DockTabItem tab in (IEnumerable)group)
				{
					tab.GetPanel().SetIsAutoHideTabSelected(false);
				}
				if (hasVisibleTab)
				{
					continue;
				}
				foreach (DockTabItem candidateTab in (IEnumerable)group)
				{
					if (candidateTab.GetPanel().ParentGroup != null && candidateTab.GetPanel() == candidateTab.GetPanel().ParentGroup._groupState.GetSelectedPanel())
					{
						candidateTab.GetPanel().SetIsAutoHideTabSelected(true);
						hasVisibleTab = true;
					}
				}
				if (!hasVisibleTab)
				{
					group.GetItem(group.GetCount() - 1).GetPanel().SetIsAutoHideTabSelected(true);
				}
			}
		}

		private DockPanelBase GetHotPanel()
		{
			return _hotPanel;
		}

		private void SetHotPanel(DockPanelBase hotPanel)
		{
			if (_hotPanel != hotPanel)
			{
				if (_dockManager != null && !_dockManager.ShowAutoHidePanelsOnClick && _hotPanel != null)
				{
					StopHoverTimer();
					Invalidate();
				}
				_hotPanel = hotPanel;
				if (_dockManager != null && !_dockManager.ShowAutoHidePanelsOnClick && _hotPanel != null)
				{
					Invalidate();
					StartHoverTimer();
				}
				UpdateLayout();
			}
		}

		public bool IsVisible()
		{
			if (_tabPanels != null && _tabPanels.Count > 0)
			{
				return true;
			}
			return false;
		}

		internal void UpdateLayout()
		{
			if (base.IsDisposed)
			{
				return;
			}
			try
			{
				RebuildTabGroups();
				bool isVertical = true;
				_thickness = 0;
				_tabAreaBounds = base.ClientRectangle;
				int tabSpacing = 0;
				switch (_autoHideDock)
				{
				case AutoHideStripDock.Left:
					_edgePadding = _dockManager.PanelPadding.Left;
					_nearCornerBounds = base.ClientRectangle;
					_nearCornerBounds.Height = _dockManager.AutoHideTopStrip.Height;
					_tabAreaBounds.Y += _nearCornerBounds.Height;
					_tabAreaBounds.Height -= _nearCornerBounds.Height;
					_farCornerBounds = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Bottom - _dockManager.AutoHideBottomStrip.Height, base.ClientRectangle.Width, _dockManager.AutoHideBottomStrip.Height);
					_tabAreaBounds.Height -= _farCornerBounds.Height;
					break;
				case AutoHideStripDock.Right:
					_edgePadding = _dockManager.PanelPadding.Right;
					_farCornerBounds = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Bottom - _dockManager.AutoHideBottomStrip.Height, base.ClientRectangle.Width, _dockManager.AutoHideBottomStrip.Height);
					_tabAreaBounds.Height -= _farCornerBounds.Height;
					break;
				case AutoHideStripDock.Top:
					isVertical = false;
					_edgePadding = _dockManager.PanelPadding.Top;
					_farCornerBounds = new Rectangle(base.ClientRectangle.Right - _dockManager.AutoHideRightStrip.Width, base.ClientRectangle.Top, _dockManager.AutoHideRightStrip.Width, base.ClientRectangle.Height);
					_tabAreaBounds.Width -= _farCornerBounds.Width;
					break;
				case AutoHideStripDock.Bottom:
					isVertical = false;
					_edgePadding = _dockManager.PanelPadding.Bottom;
					break;
				}
				if (_tabPanels.Count > 0)
				{
					using Graphics graphics = CreateGraphics();
					if (!isVertical)
					{
						int nextX = DisplayRectangle.X + TabStartOffset;
						int y = DisplayRectangle.Y;
						y = ((_autoHideDock != AutoHideStripDock.Bottom) ? (y - 1) : (y + (_edgePadding + 1)));
						int tabHeight = 0;
						foreach (DockTabStripLayout group in _tabGroups)
						{
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								tab.SetShowText(true);
							}
							group.LayoutItems(graphics, Font, 1, tabSpacing);
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								if (tab.GetItemSize().Height > tabHeight)
								{
									tabHeight = tab.GetItemSize().Height;
								}
								switch (tab.GetPanel().GetEffectiveAutoHideTabDisplay())
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
									if (!tab.GetPanel().GetIsAutoHideTabSelected())
									{
										tab.SetShowText(false);
									}
									break;
								}
							}
							group.LayoutItems(graphics, Font, 1, tabSpacing);
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								tab.SetLocation(new Point(nextX, y));
								if (tab.GetItemSize().Height > _thickness)
								{
									_thickness = tabHeight;
								}
								nextX += tab.GetBounds().Width - 1 + tabSpacing;
							}
							nextX += GroupSpacing;
						}
						foreach (DockTabStripLayout group in _tabGroups)
						{
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								tab.SetItemSize(new Size(tab.GetItemSize().Width, tabHeight));
								if (tab.GetPanel().ContainsFocus)
								{
									tab._isHot = false;
									tab._isSelected = true;
									continue;
								}
								tab._isSelected = false;
								if (tab.GetPanel() == _hotPanel)
								{
									tab._isHot = true;
								}
								else
								{
									tab._isHot = false;
								}
							}
						}
					}
					else
					{
						int x = DisplayRectangle.X;
						int nextY = DisplayRectangle.Y + TabStartOffset;
						if (_autoHideDock == AutoHideStripDock.Left)
						{
							nextY += _dockManager.AutoHideTopStrip.Height;
							x--;
						}
						else
						{
							x += _edgePadding + 1;
						}
						int tabWidth = 0;
						foreach (DockTabStripLayout group in _tabGroups)
						{
							group.SetItemOrientation(ItemOrientation.Vertical);
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								tab.SetShowText(true);
							}
							group.LayoutItems(graphics, Font, 1, tabSpacing);
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								if (tab.GetItemSize().Width > tabWidth)
								{
									tabWidth = tab.GetItemSize().Width;
								}
								switch (tab.GetPanel().GetEffectiveAutoHideTabDisplay())
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
									if (!tab.GetPanel().GetIsAutoHideTabSelected())
									{
										tab.SetShowText(false);
									}
									break;
								}
							}
							group.LayoutItems(graphics, Font, 1, tabSpacing);
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								tab.SetLocation(new Point(x, nextY));
								if (tabWidth > _thickness)
								{
									_thickness = tabWidth;
								}
								nextY += tab.GetBounds().Height - 1 + tabSpacing;
							}
							nextY += GroupSpacing;
						}
						foreach (DockTabStripLayout group in _tabGroups)
						{
							foreach (DockTabItem tab in (IEnumerable)group)
							{
								tab.SetItemSize(new Size(tabWidth, tab.GetItemSize().Height));
								if (tab.GetPanel().ContainsFocus || (_popupPanel != null && tab._isSelected))
								{
									tab._isHot = false;
									tab._isSelected = true;
									continue;
								}
								tab._isSelected = false;
								if (tab.GetPanel() == _hotPanel)
								{
									tab._isHot = true;
								}
								else
								{
									tab._isHot = false;
								}
							}
						}
					}
				}
				if (isVertical)
				{
					_thickness += _edgePadding;
					base.Width = _thickness;
				}
				else
				{
					_thickness += _edgePadding;
					base.Height = _thickness;
				}
				if (_popupPanel != null)
				{
					_popupPanel.Bounds = _dockManager.GetAutoHidePanelBounds(_autoHideDock, _popupPanel.GetPanel());
				}
				Invalidate();
			}
			catch
			{
				// Runs from layout events. An error here must not reach the message loop; the next layout pass retries.
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				DockManagerRendererBase managerRenderer = _dockManager.GetPanelRenderer();
				if (_tabPanels == null || _tabPanels.Count == 0)
				{
					managerRenderer.DrawBackground(e.Graphics, this, base.ClientRectangle);
				}
				else
				{
					if (_autoHideDock == AutoHideStripDock.Left)
					{
						if (_dockManager.AutoHideTopStrip.IsVisible())
						{
							managerRenderer.DrawAutoHideStripCorner(e.Graphics, this, _nearCornerBounds, ContentAlignment.TopLeft);
						}
						else
						{
							managerRenderer.DrawBackground(e.Graphics, this, _nearCornerBounds);
						}
						if (_dockManager.AutoHideBottomStrip.IsVisible())
						{
							managerRenderer.DrawAutoHideStripCorner(e.Graphics, this, _farCornerBounds, ContentAlignment.BottomLeft);
						}
						else
						{
							managerRenderer.DrawBackground(e.Graphics, this, _farCornerBounds);
						}
					}
					else if (_autoHideDock == AutoHideStripDock.Top)
					{
						if (_dockManager.AutoHideRightStrip.IsVisible())
						{
							managerRenderer.DrawAutoHideStripCorner(e.Graphics, this, _farCornerBounds, ContentAlignment.TopRight);
						}
						else
						{
							managerRenderer.DrawBackground(e.Graphics, this, _farCornerBounds);
						}
					}
					else if (_autoHideDock == AutoHideStripDock.Right)
					{
						if (_dockManager.AutoHideBottomStrip.IsVisible())
						{
							managerRenderer.DrawAutoHideStripCorner(e.Graphics, this, _farCornerBounds, ContentAlignment.BottomRight);
						}
						else
						{
							managerRenderer.DrawBackground(e.Graphics, this, _farCornerBounds);
						}
					}
					Rectangle bounds = _tabAreaBounds;
					managerRenderer.DrawAutoHideStripBackground(e.Graphics, this, bounds, _autoHideDock);
				}
				if (_tabGroups == null)
				{
					return;
				}
				foreach (DockTabStripLayout group in _tabGroups)
				{
					foreach (DockTabItem tab in (IEnumerable)group)
					{
						DrawTab(e.Graphics, managerRenderer, tab);
					}
				}
			}
			catch
			{
				// A paint error must not reach the message loop, or it repeats on every repaint.
			}
		}

		internal int GetTabCount()
		{
			int count = 0;
			if (_tabGroups != null)
			{
				foreach (DockTabStripLayout group in _tabGroups)
				{
					foreach (DockTabItem tab in (IEnumerable)group)
					{
						_ = tab;
						count++;
					}
				}
			}
			return count;
		}

		internal AccessibleObject GetTabAccessibleObject(int index)
		{
			List<AccessibleObject> list = new List<AccessibleObject>();
			if (_tabGroups != null)
			{
				foreach (DockTabStripLayout group in _tabGroups)
				{
					foreach (DockTabItem tab in (IEnumerable)group)
					{
						list.Add(tab.GetAccessibleObject());
					}
				}
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}

		private void DrawTab(Graphics graphics, DockManagerRendererBase managerRenderer, DockTabItem tab)
		{
			tab.GetBounds();
			ButtonState buttonState = ButtonState.Normal;
			if (tab._isSelected)
			{
				buttonState = ButtonState.Selected;
			}
			else if (tab._isHot)
			{
				buttonState = ButtonState.Hot;
			}
			FormatStyle uiFormatStyle = tab.GetPanel().GetAutoHideTabFormatStyle(buttonState);
			Font font = tab.GetPanel().GetCaptionFont(uiFormatStyle);
			if (_autoHideDock == AutoHideStripDock.Top || _autoHideDock == AutoHideStripDock.Bottom)
			{
				managerRenderer.DrawAutoHideTabHorizontal(graphics, tab.GetPanel(), uiFormatStyle, font, buttonState, tab.GetBounds(), tab.GetShowText(), tab.GetShowImage(), _autoHideDock == AutoHideStripDock.Top);
			}
			else
			{
				managerRenderer.DrawAutoHideTabVertical(graphics, tab.GetPanel(), uiFormatStyle, font, buttonState, tab.GetBounds(), tab.GetShowText(), tab.GetShowImage(), _autoHideDock == AutoHideStripDock.Left);
			}
			uiFormatStyle.DisposeResources();
		}

		public DockPanelBase PanelFromPoint(int x, int y)
		{
			return HitTestTab(x, y)?.GetPanel();
		}

		private DockTabItem HitTestTab(int x, int y)
		{
			if (_tabGroups != null)
			{
				foreach (DockTabStripLayout group in _tabGroups)
				{
					foreach (DockTabItem tab in (IEnumerable)group)
					{
						if (tab.GetBounds().Contains(x, y))
						{
							return tab;
						}
					}
				}
			}
			return null;
		}

		protected override void OnResize(EventArgs e)
		{
			switch (_autoHideDock)
			{
			case AutoHideStripDock.Left:
			case AutoHideStripDock.Right:
				if (base.Width != _thickness)
				{
					base.Width = _thickness;
					return;
				}
				break;
			case AutoHideStripDock.Top:
			case AutoHideStripDock.Bottom:
				if (base.Height != _thickness)
				{
					base.Height = _thickness;
					return;
				}
				break;
			}
			if (_dockManager != null)
			{
				_dockManager.LayoutAutoHideStrips();
			}
			base.OnResize(e);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new AutoHideStripAccessibleObject(this);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (_dockManager != null)
			{
				_dockManager.LayoutAutoHideStrips();
			}
		}

		internal void RemovePanel(DockPanelBase panel)
		{
			if (_panels.Contains(panel))
			{
				_panels.Remove(panel);
				panel.SetAutoHideStrip(null);
				if (_popupPanel != null)
				{
					_popupPanel.CloseNow();
				}
				StopHoverTimer();
				UpdateLayout();
			}
		}

		internal void RemoveForeignPanels()
		{
			ArrayList snapshot = new ArrayList();
			foreach (DockPanelBase panel in _panels)
			{
				snapshot.Add(panel);
			}
			foreach (DockPanelBase candidate in snapshot)
			{
				if (candidate.PanelManager != _dockManager)
				{
					_panels.Remove(candidate);
				}
			}
			UpdateLayout();
		}

		internal void AddPanel(DockPanelBase panel)
		{
			if (!_panels.Contains(panel))
			{
				_panels.Add(panel);
				panel.SetAutoHideStrip(this);
				UpdateLayout();
			}
		}
	}
}
