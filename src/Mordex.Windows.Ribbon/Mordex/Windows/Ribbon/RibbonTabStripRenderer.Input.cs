using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabStripRenderer
	{
		public void SelectFirstTab()
		{
			if (_tabRenderers.Count <= 0)
			{
				return;
			}
			foreach (RibbonTabRenderer item in GetTabRenderers())
			{
				if (item._isEnabled)
				{
					_ribbon.SelectedTab = item.GetTab();
					_ribbon.SetKeyboardFocusRenderer(item);
				}
			}
		}

		public void SelectLastTab()
		{
			if (_tabRenderers.Count <= 0)
			{
				return;
			}
			for (int tabIndex = GetTabRenderers().Count - 1; tabIndex >= 0; tabIndex--)
			{
				RibbonTabRenderer thisValue = GetTabRenderers()[tabIndex];
				if (thisValue._isEnabled)
				{
					_ribbon.SelectedTab = thisValue.GetTab();
					_ribbon.SetKeyboardFocusRenderer(thisValue);
				}
			}
		}

		public void SelectNextTab(bool wrapAround)
		{
			int nextIndex = 0;
			if (_tabRenderers == null || _tabRenderers.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < _tabRenderers.Count; i++)
			{
				RibbonTabRenderer thisValue = _tabRenderers[i];
				if (thisValue.GetTab() == _ribbon.SelectedTab)
				{
					nextIndex = i + 1;
					break;
				}
			}
			if (nextIndex > _tabRenderers.Count - 1)
			{
				if (!wrapAround)
				{
					return;
				}
				nextIndex = 0;
			}
			_ribbon.SelectedTab = _tabRenderers[nextIndex].GetTab();
		}

		public void SelectPreviousTab(bool wrapAround)
		{
			int previousIndex = 0;
			if (_tabRenderers == null || _tabRenderers.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < _tabRenderers.Count; i++)
			{
				RibbonTabRenderer thisValue = _tabRenderers[i];
				if (thisValue.GetTab() == _ribbon.SelectedTab)
				{
					previousIndex = i - 1;
					break;
				}
			}
			if (previousIndex < 0)
			{
				if (!wrapAround)
				{
					return;
				}
				previousIndex = _tabRenderers.Count - 1;
			}
			_ribbon.SelectedTab = _tabRenderers[previousIndex].GetTab();
		}

		public HitTest GetHitTest(int pointX, int pointY)
		{
			if (_rightScrollButton._isVisible && _rightScrollButton._bounds.Contains(pointX, pointY))
			{
				return HitTest.TabScrollButton;
			}
			if (_leftScrollButton._isVisible && _leftScrollButton._bounds.Contains(pointX, pointY))
			{
				return HitTest.TabScrollButton;
			}
			if (_helpButton._isVisible && _helpButton._bounds.Contains(pointX, pointY))
			{
				return HitTest.HelpButton;
			}
			if (_minimizeButton._isVisible && _minimizeButton._bounds.Contains(pointX, pointY))
			{
				return HitTest.MinimizeRibbonButton;
			}
			RibbonTabRenderer ribbonTabRenderer = GetTabAt(pointX, pointY);
			if (ribbonTabRenderer != null)
			{
				if (ribbonTabRenderer is BackstageMenuTabRenderer)
				{
					return HitTest.BackstageMenuButton;
				}
				return HitTest.RibbonTab;
			}
			return HitTest.None;
		}

		public bool HandleDoubleClick(Point point)
		{
			RibbonTabRenderer ribbonTabRenderer = GetTabAt(point.X, point.Y);
			if (ribbonTabRenderer != null && _ribbon != null && _ribbon.MinimizeOnDoubleClick)
			{
				if (ribbonTabRenderer.GetTab() != null && ribbonTabRenderer.GetTab().IsEffectivelyEnabled())
				{
					if (!_ribbon.Minimized)
					{
						if (_ribbon._wasSelectedTabClicked)
						{
							_ribbon.Minimized = true;
						}
						else if (ribbonTabRenderer.GetTab().IsMarkedSelected())
						{
							_ribbon._activeTabRenderer = ribbonTabRenderer;
						}
					}
					else if (_ribbon.Minimized)
					{
						_ribbon.Minimized = false;
						_ribbon.SetPopupMenu(null);
					}
				}
				return true;
			}
			return false;
		}

		public bool HandleMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				if (_mdiControlBox != null && _mdiControlBox.HandleMouseDown(eventArgs))
				{
					_isMousePressed = true;
					return true;
				}
				if (_rightScrollButton._isVisible && _rightScrollButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					SetRightScrollPressed(true);
					_isMousePressed = true;
					return true;
				}
				if (_leftScrollButton._isVisible && _leftScrollButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					SetLeftScrollPressed(true);
					_isMousePressed = true;
					return true;
				}
				if (_helpButton._isVisible && _helpButton._isEnabled && _helpButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					SetHelpButtonPressed(true);
					_isMousePressed = true;
					_pressedItem = _helpButton;
					return true;
				}
				if (_minimizeButton._isVisible && _minimizeButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					SetMinimizeButtonPressed(true);
					_isMousePressed = true;
					_pressedItem = _minimizeButton;
					return true;
				}
			}
			RibbonTabRenderer ribbonTabRenderer = GetTabAt(eventArgs.X, eventArgs.Y);
			if (ribbonTabRenderer != null)
			{
				_isMousePressed = true;
				_pressedItem = ribbonTabRenderer;
				if (eventArgs.Button == MouseButtons.Left)
				{
					if (ribbonTabRenderer is BackstageMenuTabRenderer)
					{
						if (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.BackstageMenu)
						{
							_ribbon.ToggleBackstageMenu(ribbonTabRenderer as BackstageMenuTabRenderer);
							_isMousePressed = false;
							_pressedItem = null;
						}
						else if (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxTab)
						{
							_ribbon.ShowControlBoxMenu();
							_isMousePressed = false;
							_pressedItem = null;
						}
					}
					else if (ribbonTabRenderer.GetTab() != null && ribbonTabRenderer.GetTab().Enabled)
					{
						_ribbon.CloseBackstage();
						if (_ribbon._activeTabRenderer == ribbonTabRenderer)
						{
							_ribbon._wasSelectedTabClicked = true;
						}
						else
						{
							_ribbon._wasSelectedTabClicked = false;
							if (ribbonTabRenderer.GetTab() == _ribbon.SelectedTab)
							{
								_ribbon._activeTabRenderer = ribbonTabRenderer;
							}
							else
							{
								_ribbon._activeTabRenderer = null;
							}
						}
						bool minimized = _ribbon.Minimized;
						bool wasAlreadySelected = ribbonTabRenderer.GetTab() == _ribbon.SelectedTab;
						_ribbon.SelectedTab = ribbonTabRenderer.GetTab();
						if (minimized == _ribbon.Minimized && minimized)
						{
							SetHotTabRenderer(null);
							if (_ribbon.GetPopupMenu() != null && _ribbon.GetPopupMenu() is MinimizedRibbonPopup)
							{
								if (wasAlreadySelected)
								{
									_ribbon.SetPopupMenu(null);
								}
								else
								{
									_ribbon._tabContentRenderer.MarkGroupsStale();
								}
							}
							else
							{
								ShowMinimizedRibbonPopup();
							}
						}
					}
					else
					{
						_ribbon.CloseBackstage();
					}
				}
			}
			else
			{
				_ribbon.SetPopupMenu(null);
			}
			if (_stripBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				return ribbonTabRenderer != null;
			}
			return false;
		}

		public bool HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (_isMousePressed && _ribbon != null)
			{
				SetRightScrollPressed(false);
				SetLeftScrollPressed(false);
				SetMinimizeButtonPressed(false);
				SetHelpButtonPressed(false);
				_isMousePressed = false;
				if (_mdiControlBox != null)
				{
					_mdiControlBox.HandleMouseUp(eventArgs);
				}
				if (_minimizeButton == _pressedItem && _minimizeButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_ribbon.Minimized = !_ribbon.Minimized;
				}
				if (_helpButton == _pressedItem && _helpButton._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_ribbon.HelpButton.InvokeOnClick();
				}
				RibbonTabRenderer ribbonTabRenderer = GetTabAt(eventArgs.X, eventArgs.Y);
				if (eventArgs.Button == MouseButtons.Right && ribbonTabRenderer == _pressedItem)
				{
					_ribbon.BuildRibbonContextMenu(null, false);
				}
				ribbonTabRenderer?.OnMouseUp(eventArgs);
				_pressedItem = null;
				return true;
			}
			return false;
		}

		public bool HandleMouseMove(Point point)
		{
			if (_stripBounds.Contains(point.X, point.Y))
			{
				if (_mdiControlBox != null && _mdiControlBox.SetHotButtonAt(point))
				{
					SetMinimizeButtonHot(false);
					SetHelpButtonHot(false);
					SetRightScrollHot(false);
					SetLeftScrollHot(false);
					return true;
				}
				if (_rightScrollButton._isVisible)
				{
					Rectangle bounds = _rightScrollButton._bounds;
					bounds.Inflate(1, 1);
					if (bounds.Contains(point.X, point.Y))
					{
						SetHotTabRenderer(null);
						SetMinimizeButtonHot(false);
						SetHelpButtonHot(false);
						SetRightScrollHot(true);
						return true;
					}
					SetRightScrollHot(false);
				}
				if (_leftScrollButton._isVisible)
				{
					Rectangle leftButtonHitBounds = _leftScrollButton._bounds;
					leftButtonHitBounds.Inflate(1, 1);
					if (leftButtonHitBounds.Contains(point.X, point.Y))
					{
						SetHotTabRenderer(null);
						SetMinimizeButtonHot(false);
						SetHelpButtonHot(false);
						SetLeftScrollHot(true);
						return true;
					}
					SetLeftScrollHot(false);
				}
				if (_helpButton._isVisible)
				{
					Rectangle helpButtonHitBounds = _helpButton._bounds;
					helpButtonHitBounds.Inflate(1, 1);
					if (_helpButton._isEnabled && helpButtonHitBounds.Contains(point.X, point.Y))
					{
						SetHotTabRenderer(null);
						SetMinimizeButtonHot(false);
						SetHelpButtonHot(true);
						return true;
					}
					SetHelpButtonHot(false);
				}
				if (_minimizeButton._isVisible)
				{
					Rectangle minimizeButtonHitBounds = _minimizeButton._bounds;
					minimizeButtonHitBounds.Inflate(1, 1);
					if (minimizeButtonHitBounds.Contains(point.X, point.Y))
					{
						SetHotTabRenderer(null);
						SetMinimizeButtonHot(true);
						return true;
					}
					SetMinimizeButtonHot(false);
				}
				RibbonTabRenderer ribbonTabRenderer = GetTabAt(point.X, point.Y);
				if (ribbonTabRenderer != null && ribbonTabRenderer.GetTab() != null && !ribbonTabRenderer.GetTab().Enabled)
				{
					ribbonTabRenderer = null;
				}
				SetHotTabRenderer(ribbonTabRenderer);
				return ribbonTabRenderer != null;
			}
			SetHotTabRenderer(null);
			SetLeftScrollHot(false);
			SetRightScrollHot(false);
			SetMinimizeButtonHot(false);
			SetHelpButtonHot(false);
			if (_mdiControlBox != null)
			{
				_mdiControlBox.SetHotButton(null);
			}
			return false;
		}

		public void ClearHoverState()
		{
			if (_mdiControlBox != null)
			{
				_mdiControlBox.SetHotButton(null);
			}
			SetHotTabRenderer(null);
			SetRightScrollHot(false);
			SetLeftScrollHot(false);
			SetMinimizeButtonHot(false);
			SetHelpButtonHot(false);
			StopTimer();
		}

		internal void ShowMinimizedRibbonPopup()
		{
			_ribbon.SetMinimizedPopupOpen(true);
			_minimizedPopup = new MinimizedRibbonPopup(_ribbon);
			int popupX = _stripBounds.X;
			int popupY = _stripBounds.Bottom;
			if (IsInGlassCaption() && _ribbon != null)
			{
				popupX -= _ribbon.GetFrameBorderSize().Width;
			}
			popupY--;
			_minimizedPopup.ShowAt(_ribbon.PointToScreen(new Point(popupX, popupY)));
			_ribbon.SetMinimizedPopupOpen(false);
			_minimizedPopup = null;
		}
	}
}
