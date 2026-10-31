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
		internal FormatStyleLayoutState HitTestElement(int x, int y)
		{
			FormatStyleLayoutState element = HitTestTab(x, y);
			if (element != null)
			{
				return element;
			}
			if (_closeButton != null && _closeButton._bounds.Contains(x, y))
			{
				return _closeButton;
			}
			if (_scrollBackButton != null && _scrollBackButton._bounds.Contains(x, y))
			{
				return _scrollBackButton;
			}
			if (_scrollForwardButton != null && _scrollForwardButton._bounds.Contains(x, y))
			{
				return _scrollForwardButton;
			}
			return null;
		}

		internal TabPageRenderer HitTestTab(int x, int y)
		{
			if (_pageRenderers.Count > 0)
			{
				foreach (TabPageRenderer tabRenderer in _pageRenderers)
				{
					if (tabRenderer._bounds.Contains(x, y))
					{
						return tabRenderer;
					}
				}
			}
			return null;
		}

		public MxTabPage GetTabAt(int x, int y)
		{
			return HitTestTab(x, y)?.GetTabPage();
		}

		public MxTabPage GetTabAt(Point pt)
		{
			return GetTabAt(pt.X, pt.Y);
		}

		public MxTabPage GetTabAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetTabAt(point.X, point.Y);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			TabCloseMouseUp(e);
			if (_scrollBackButton != null)
			{
				_scrollBackButton.MouseDown = false;
			}
			if (_scrollForwardButton != null)
			{
				_scrollForwardButton.MouseDown = false;
			}
			if (_closeButton != null && _closeButton.MouseDown)
			{
				_closeButton.MouseDown = false;
				if (_closeButton._bounds.Contains(e.X, e.Y) && SelectedTab != null)
				{
					SelectedTab.TabVisible = false;
				}
			}
			SetCloseButtonPressed(false);
			SetScrollBackPressed(false);
			SetScrollForwardPressed(false);
		}

		internal void RaiseClosingTab(TabCancelEventArgs eventArgs)
		{
			OnClosingTab(eventArgs);
		}

		internal void RaiseTabClosed(MxTabPage page)
		{
			TabEventArgs e = new TabEventArgs(page);
			OnTabClosed(e);
		}

		private void FocusNextTab()
		{
			int startIndex = _inputFocusTab._index + 1;
			MxTabPage page = FindNextEnabledTab(startIndex, false);
			if (page != null)
			{
				InputFocusTab = page;
				SelectedTab = InputFocusTab;
			}
		}

		private void SelectNextTab()
		{
			int startIndex = _selectedTab._index + 1;
			MxTabPage page = FindNextEnabledTab(startIndex, false);
			if (page != null)
			{
				InputFocusTab = page;
				SelectedTab = InputFocusTab;
				return;
			}
			for (int i = 0; i < _selectedTab.Index; i++)
			{
				page = TabPages[i];
				if (page.TabVisible && page.Enabled)
				{
					InputFocusTab = page;
					SelectedTab = InputFocusTab;
					break;
				}
			}
		}

		private void SelectPreviousTab()
		{
			int startIndex = _selectedTab._index - 1;
			MxTabPage page = FindPreviousEnabledTab(startIndex);
			if (page != null)
			{
				InputFocusTab = page;
				SelectedTab = InputFocusTab;
				return;
			}
			for (int index = _tabPages.Count - 1; index >= 0; index--)
			{
				page = TabPages[index];
				if (page.TabVisible && page.Enabled)
				{
					InputFocusTab = page;
					SelectedTab = InputFocusTab;
					break;
				}
			}
		}

		private void FocusPreviousTab()
		{
			int startIndex = _inputFocusTab._index - 1;
			MxTabPage page = FindPreviousEnabledTab(startIndex);
			if (page != null)
			{
				InputFocusTab = page;
				SelectedTab = InputFocusTab;
			}
		}

		// Moves the focus to the first usable tab of the row above (-1) or below (+1)
		// that ends after the start of the focused tab.
		private void FocusTabInAdjacentRow(int rowOffset)
		{
			if (!MultiLine)
			{
				return;
			}
			TabPageRenderer focusedTab = _pageRenderers.FindTabPageItem(_inputFocusTab);
			if (focusedTab == null)
			{
				return;
			}
			int targetRow = focusedTab._rowIndex + rowOffset;
			if (targetRow < 0 || targetRow >= _tabRows.Count)
			{
				return;
			}
			Rectangle focusedBounds = focusedTab._bounds;
			foreach (TabPageRenderer candidate in (ArrayList)_tabRows[targetRow])
			{
				bool endsAfterFocused = _isHorizontalStrip ? candidate._bounds.Right >= focusedBounds.X : candidate._bounds.Bottom >= focusedBounds.Y;
				MxTabPage candidatePage = candidate.GetTabPage();
				if (endsAfterFocused && candidatePage != null && candidatePage.TabVisible && candidatePage.Enabled)
				{
					InputFocusTab = candidatePage;
					SelectedTab = InputFocusTab;
					break;
				}
			}
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible)
			{
				foreach (MxTabPage item in _tabPages)
				{
					if (item.Enabled && item.TabVisible && Control.IsMnemonic(charCode, item.Text))
					{
						if (!item.Selected)
						{
							SelectedTab = item;
						}
						if (FocusOnClick)
						{
							item.Focus();
						}
						else
						{
							item.SelectNextControl(item, forward: true, tabStopOnly: true, nested: true, wrap: true);
						}
						return true;
					}
				}
			}
			return base.ProcessMnemonic(charCode);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_selectedTab != null && ShowTabs)
			{
				switch (keyData)
				{
				case Keys.Tab | Keys.Shift | Keys.Control:
					SelectPreviousTab();
					return true;
				case Keys.Tab | Keys.Control:
					SelectNextTab();
					return true;
				}
			}
			if (_inputFocusTab != null)
			{
				switch (keyData)
				{
				case Keys.Return:
					if (_inputFocusTab.Enabled && SelectedTab != _inputFocusTab)
					{
						SelectedTab = _inputFocusTab;
						return true;
					}
					break;
				case Keys.Up:
					switch (TabStripAlignment)
					{
					case TabStripAlignment.Top:
						FocusTabInAdjacentRow(-1);
						break;
					case TabStripAlignment.Bottom:
						FocusTabInAdjacentRow(1);
						break;
					case TabStripAlignment.Left:
					case TabStripAlignment.Right:
						FocusPreviousTab();
						break;
					}
					return true;
				case Keys.Left:
					switch (TabStripAlignment)
					{
					case TabStripAlignment.Top:
					case TabStripAlignment.Bottom:
						FocusPreviousTab();
						break;
					case TabStripAlignment.Left:
						FocusTabInAdjacentRow(-1);
						break;
					case TabStripAlignment.Right:
						FocusTabInAdjacentRow(1);
						break;
					}
					return true;
				case Keys.Down:
					switch (TabStripAlignment)
					{
					case TabStripAlignment.Top:
						FocusTabInAdjacentRow(1);
						break;
					case TabStripAlignment.Bottom:
						FocusTabInAdjacentRow(-1);
						break;
					case TabStripAlignment.Left:
					case TabStripAlignment.Right:
						FocusNextTab();
						break;
					}
					return true;
				case Keys.Right:
					switch (TabStripAlignment)
					{
					case TabStripAlignment.Top:
					case TabStripAlignment.Bottom:
						FocusNextTab();
						break;
					case TabStripAlignment.Left:
						FocusTabInAdjacentRow(1);
						break;
					case TabStripAlignment.Right:
						FocusTabInAdjacentRow(-1);
						break;
					}
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			RenderingHelper.SetMouseCapture(this, false);
			InputFocusTab = null;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			InputFocusTab = SelectedTab;
			Invalidate();
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new TabControlAccessibleObject(this);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (_tabStripBounds.Contains(e.X, e.Y) && TabCloseMouseDown(e))
			{
				return;
			}
			if (!_tabStripBounds.Contains(e.X, e.Y) || HitTestStripButtons(new Point(e.X, e.Y), true, e.Button == MouseButtons.Left, true) || e.Button != MouseButtons.Left)
			{
				return;
			}
			TabPageRenderer clickedTab = HitTestTab(e.X, e.Y);
			if (clickedTab == null || !clickedTab.GetTabPage().Enabled || !((FormatStyleLayoutState)clickedTab).IsVisible())
			{
				return;
			}
			if (_focusOnClick)
			{
				RenderingHelper.TryFocusControl(this);
				if (!Focused && !base.DesignMode)
				{
					return;
				}
				InputFocusTab = clickedTab.GetTabPage();
			}
			SelectedTab = clickedTab.GetTabPage();
		}

		internal void SetScrollBackPressed(bool isPressed)
		{
			if (_isScrollBackPressed == isPressed)
			{
				return;
			}
			_isScrollBackPressed = isPressed;
			if (_scrollBackButton != null)
			{
				_scrollBackButton.SetPressed(isPressed);
			}
			if (isPressed)
			{
				ScrollBack();
				StartScrollTimer();
			}
			else
			{
				StopScrollTimer();
			}
			if (_scrollBackButton != null)
			{
				try
				{
					AccessibilityNotifyClients(AccessibleEvents.Focus, _pageRenderers.Count);
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// Accessibility notification is best effort and must not break hot tracking.
				}
			}
		}

		internal void SetScrollForwardPressed(bool isPressed)
		{
			if (_isScrollForwardPressed == isPressed)
			{
				return;
			}
			_isScrollForwardPressed = isPressed;
			if (_scrollForwardButton != null)
			{
				_scrollForwardButton.SetPressed(isPressed);
			}
			if (isPressed)
			{
				ScrollForward();
				StartScrollTimer();
			}
			else
			{
				StopScrollTimer();
			}
			if (_scrollForwardButton != null)
			{
				try
				{
					AccessibilityNotifyClients(AccessibleEvents.Focus, _pageRenderers.Count + 1);
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// Accessibility notification is best effort and must not break hot tracking.
				}
			}
		}

		internal void SetCloseButtonPressed(bool isPressed)
		{
			if (_isCloseButtonPressed != isPressed)
			{
				_isCloseButtonPressed = isPressed;
				if (_closeButton != null)
				{
					_closeButton.SetPressed(isPressed);
				}
			}
		}

		internal void SetScrollForwardHot(bool isHot)
		{
			if (_isScrollForwardHot != isHot)
			{
				_isScrollForwardHot = isHot;
				if (_scrollForwardButton != null)
				{
					_scrollForwardButton.SetHot(isHot);
				}
			}
		}

		internal void SetScrollBackHot(bool isHot)
		{
			if (_isScrollBackHot != isHot)
			{
				_isScrollBackHot = isHot;
				if (_scrollBackButton != null)
				{
					_scrollBackButton.SetHot(isHot);
				}
			}
		}

		internal void SetCloseButtonHot(bool isHot)
		{
			if (_isCloseButtonHot != isHot)
			{
				_isCloseButtonHot = isHot;
				if (_closeButton != null)
				{
					_closeButton.SetHot(isHot);
				}
			}
		}

		internal void UpdateCloseButtonEnabled()
		{
			if (_closeButton != null && _selectedTab != null)
			{
				_closeButton.SetEnabled(_selectedTab.AllowClose);
			}
		}

		internal void ScrollBack()
		{
			if (_scrollBackButton != null && _scrollBackButton.IsEnabled() && _firstVisibleTabIndex > 0)
			{
				_firstVisibleTabIndex--;
				InvalidateLayout();
			}
		}

		internal void ScrollForward()
		{
			if (_scrollForwardButton != null && _scrollForwardButton.IsEnabled() && _firstVisibleTabIndex < _pageRenderers.Count - 1)
			{
				_firstVisibleTabIndex++;
				InvalidateLayout();
			}
		}

		private bool HitTestStripButtons(Point pt, bool isHot, bool isPressed, bool isNewPress)
		{
			if (_scrollBackButton != null)
			{
				if (_scrollBackButton._bounds.Contains(pt))
				{
					SetScrollForwardHot(false);
					SetCloseButtonHot(false);
					if (_scrollBackButton.IsEnabled())
					{
						SetScrollBackHot(isHot);
						if (isPressed && (_scrollBackButton.MouseDown || isNewPress))
						{
							_scrollBackButton.MouseDown = true;
							SetScrollBackPressed(isPressed);
						}
					}
					return true;
				}
				SetScrollBackHot(false);
				SetScrollBackPressed(false);
			}
			if (_scrollForwardButton != null)
			{
				if (_scrollForwardButton._bounds.Contains(pt))
				{
					SetCloseButtonHot(false);
					if (_scrollForwardButton.IsEnabled())
					{
						SetScrollForwardHot(isHot);
						if (isPressed && (_scrollForwardButton.MouseDown || isNewPress))
						{
							_scrollForwardButton.MouseDown = true;
							SetScrollForwardPressed(isPressed);
						}
					}
					return true;
				}
				SetScrollForwardHot(false);
				SetScrollForwardPressed(false);
			}
			if (_closeButton != null)
			{
				if (_closeButton._bounds.Contains(pt))
				{
					if (_closeButton.IsEnabled())
					{
						SetCloseButtonHot(isHot);
						if (isPressed && (_closeButton.MouseDown || isNewPress))
						{
							_closeButton.MouseDown = true;
							SetCloseButtonPressed(isPressed);
						}
					}
					return true;
				}
				SetCloseButtonHot(false);
				SetCloseButtonPressed(false);
			}
			return false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!_tabStripBounds.Contains(e.X, e.Y))
			{
				TabCloseMouseMove(e);
				return;
			}
			TabCloseMouseMove(e);
			Point point = new Point(e.X, e.Y);
			if (e.Button != MouseButtons.Left || !HitTestStripButtons(point, true, true, false))
			{
				if (HitTestStripButtons(point, true, false, false))
				{
					SetHotTab(null);
					return;
				}
				TabPageRenderer hoveredTab = HitTestTab(e.X, e.Y);
				SetHotTab(hoveredTab);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			TabCloseMouseLeave();
			SetScrollBackHot(false);
			SetScrollForwardHot(false);
			SetScrollBackPressed(false);
			SetScrollForwardPressed(false);
			SetHotTab(null);
			SetCloseButtonHot(false);
			SetCloseButtonPressed(false);
		}

		internal void DisposeToolTip()
		{
			if (_themedToolTip != null)
			{
				_themedToolTip.HideAndDisposeTimers();
				_themedToolTip = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		private void UpdateToolTip(TabPageRenderer hotTab)
		{
			if (hotTab == null || hotTab.GetTabPage() == null || (hotTab.GetTabPage().ToolTipText != null && hotTab.GetTabPage().ToolTipText.Length == 0))
			{
				DisposeToolTip();
				return;
			}
			Form form = null;
			form = FindForm();
			if (form == null || !form.ContainsFocus)
			{
				DisposeToolTip();
			}
			else
			{
				if (!ShowToolTips)
				{
					return;
				}
				string toolTipText = hotTab.GetTabPage().ToolTipText;
				if (VisualStyleSupport.IsToolTipHookSupported())
				{
					if (_themedToolTip == null)
					{
						_themedToolTip = new WindowMessageHandler(Font, UseCompatibleTextRendering);
						_themedToolTip.RequestShow(toolTipText, this);
					}
					else
					{
						_themedToolTip.UpdateTipText(toolTipText);
					}
					return;
				}
				if (_toolTip == null)
				{
					_toolTip = new Mordex.Windows.Common.ThemedToolTip();
					_toolTip.ShowAlways = true;
				}
				_toolTip.SetToolTip(this, toolTipText);
			}
		}

		internal void SetHotTab(TabPageRenderer hotTab)
		{
			if (_hotTabRenderer != hotTab)
			{
				if (_hotTabRenderer != null)
				{
					_hotTabRenderer.SetHot(false);
				}
				_hotTabRenderer = hotTab;
				UpdateToolTip(_hotTabRenderer);
				if (_hotTabRenderer != null && _hotTabRenderer.GetTabPage().Enabled)
				{
					_hotTabRenderer.SetHot(true);
				}
			}
		}
	}
}
