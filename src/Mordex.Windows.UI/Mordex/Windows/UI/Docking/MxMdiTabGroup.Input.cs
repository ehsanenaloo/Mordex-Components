using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxMdiTabGroup
	{
		protected override void OnClick(EventArgs e)
		{
			Point point = PointToClient(Cursor.Position);
			MdiTabItem uiMdiChildTabLayoutState = HitTestTab(point.X, point.Y);
			if (uiMdiChildTabLayoutState != null && uiMdiChildTabLayoutState.GetChildTab() != null && PanelManager != null)
			{
				PanelManager.RaiseMdiTabClick(uiMdiChildTabLayoutState.GetChildTab());
			}
			base.OnClick(e);
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			Point point = PointToClient(Cursor.Position);
			MdiTabItem uiMdiChildTabLayoutState = HitTestTab(point.X, point.Y);
			if (uiMdiChildTabLayoutState != null && uiMdiChildTabLayoutState.GetChildTab() != null && PanelManager != null)
			{
				PanelManager.RaiseMdiTabDoubleClick(uiMdiChildTabLayoutState.GetChildTab());
			}
			base.OnDoubleClick(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			MdiTabItem uiMdiChildTabLayoutState = HitTestTab(e.X, e.Y);
			if (uiMdiChildTabLayoutState != null && PanelManager != null && uiMdiChildTabLayoutState.GetChildTab() != null)
			{
				PanelManager.RaiseMdiTabMouseUp(uiMdiChildTabLayoutState.GetChildTab(), e);
			}
			if (_pressedTabLayout != null && PanelManager != null && _pressedTabLayout.HitTest(false, e.X, e.Y))
			{
				if (_pressedTabLayout.GetButtonType() == TabButtonType.Close)
				{
					if (PanelManager.RaiseMdiTabClosing(SelectedTab))
					{
						GetSelectedForm().Close();
						if (_groupLayout != null && _groupLayout.GetCount() > 0 && _tabGroupHost != null && _tabGroupHost.GetForm() != null && !ContainsForm(_tabGroupHost.GetForm().ActiveMdiChild))
						{
							_groupLayout.GetTab(_groupLayout.GetCount() - 1).GetChildTab().Form.Activate();
						}
					}
				}
				else if (_pressedTabLayout.GetButtonType() == TabButtonType.ActiveFiles)
				{
					_groupLayout.ShowActiveFilesMenu();
				}
			}
			SetPressedTabLayout(null);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			SetHoveredTabState(null);
			MdiTabItem uiMdiChildTabLayoutState = HitTestTab(e.X, e.Y);
			if (e.Button == MouseButtons.Left && uiMdiChildTabLayoutState != null && uiMdiChildTabLayoutState.GetIsVisible())
			{
				SetPressedTabLayout(uiMdiChildTabLayoutState);
				if (PanelManager != null && uiMdiChildTabLayoutState.GetChildTab() != null)
				{
					PanelManager.RaiseMdiTabMouseDown(uiMdiChildTabLayoutState.GetChildTab(), e);
					uiMdiChildTabLayoutState = null;
				}
			}
			if (uiMdiChildTabLayoutState != null && uiMdiChildTabLayoutState.GetIsVisible() && PanelManager != null && uiMdiChildTabLayoutState.GetChildTab() != null)
			{
				PanelManager.RaiseMdiTabMouseDown(uiMdiChildTabLayoutState.GetChildTab(), e);
			}
			base.OnMouseDown(e);
		}

		internal MdiTabItem GetHoveredTabState()
		{
			return _groupLayout.GetHotItem();
		}

		internal void SetHoveredTabState(MdiTabItem uiMdiChildTabLayoutState)
		{
			if (GetHoveredTabState() != uiMdiChildTabLayoutState)
			{
				_groupLayout.SetHotItem(uiMdiChildTabLayoutState);
				UpdateToolTip(uiMdiChildTabLayoutState);
				Invalidate();
			}
			if (GetHoveredTabState() == null)
			{
				StopHoverTimer();
			}
			else
			{
				StartHoverTimer();
			}
		}

		private void StartHoverTimer()
		{
			if (_hoverTimer == null)
			{
				_hoverTimer = new Timer();
				_hoverTimer.Interval = HoverTimerIntervalMs;
				_hoverTimer.Tick += OnHoverTimerTick;
				_hoverTimer.Enabled = true;
			}
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
			Point pt = PointToClient(Cursor.Position);
			if (!base.ClientRectangle.Contains(pt))
			{
				SetHoveredTabState(null);
			}
		}

		private void StartScrollRepeatTimer()
		{
			if (_scrollRepeatTimer == null)
			{
				_scrollRepeatTimer = new Timer();
				_scrollRepeatTimer.Interval = ScrollRepeatDelayMs;
				_scrollRepeatTimer.Tick += OnScrollRepeatTimerTick;
				_scrollRepeatTimer.Enabled = true;
			}
		}

		private void StopScrollRepeatTimer()
		{
			if (_scrollRepeatTimer != null)
			{
				Timer timer = _scrollRepeatTimer;
				_scrollRepeatTimer = null;
				timer.Stop();
				timer.Tick -= OnScrollRepeatTimerTick;
				timer.Dispose();
			}
		}

		private void OnScrollRepeatTimerTick(object sender, EventArgs eventArgs)
		{
			if (!base.Capture || _pressedTabLayout == null)
			{
				StopScrollRepeatTimer();
				return;
			}
			if (_pressedTabLayout._isPressed && !_pressedTabLayout._isDisabled)
			{
				if (_pressedTabLayout.GetButtonType() == TabButtonType.ScrollLeft)
				{
					MdiTabStripLayout uiMdiTabGroupLayoutState = _groupLayout;
					uiMdiTabGroupLayoutState.SetFirstVisibleIndex(uiMdiTabGroupLayoutState.GetFirstVisibleIndex() - 1);
					Invalidate();
				}
				else if (_pressedTabLayout.GetButtonType() == TabButtonType.ScrollRight)
				{
					MdiTabStripLayout tabLayout = _groupLayout;
					tabLayout.SetFirstVisibleIndex(tabLayout.GetFirstVisibleIndex() + 1);
					Invalidate();
				}
			}
			if (_scrollRepeatTimer != null)
			{
				_scrollRepeatTimer.Interval = ScrollRepeatIntervalMs;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			_groupLayout.GetHotItem();
			Control control = RenderingHelper.GetTopLevelControl(this);
			if (control != null && control.ContainsFocus && e.Button == MouseButtons.None)
			{
				SetHoveredTabState(HitTestTab(e.X, e.Y));
				if (GetHoveredTabState() != null && GetHoveredTabState()._closeButton != null && GetHoveredTabState()._closeButton.GetBounds().Contains(e.X, e.Y))
				{
					GetHoveredTabState()._closeButton._isHot = true;
					Invalidate();
				}
			}
			else
			{
				SetHoveredTabState(null);
			}
			if (_pressedTabLayout != null)
			{
				if (_pressedTabLayout.GetButtonType() == TabButtonType.ChildForm)
				{
					if (_dockMessageFilter != null)
					{
						UpdateDropTarget();
					}
					else
					{
						BeginTabDrag();
					}
				}
				else
				{
					bool field_ = _pressedTabLayout._isPressed;
					_pressedTabLayout._isPressed = _pressedTabLayout.HitTest(false, e.X, e.Y);
					if (_pressedTabLayout._isPressed != field_)
					{
						Invalidate();
					}
				}
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			SetHoveredTabState(null);
			SetPressedTabLayout(null);
			base.OnMouseLeave(e);
		}
	}
}
