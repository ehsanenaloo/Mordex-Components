using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal partial class CommandControl
	{
		internal void OnCommandChanged(object sender, EventArgs eventArgs)
		{
			InvalidateLayout();
		}

		public Rectangle GetCommandScreenBounds(MxCommand uiCommand)
		{
			if (_layoutItems != null)
			{
				foreach (CommandLayoutState item in _layoutItems)
				{
					if (item.GetCommand() == uiCommand)
					{
						return new Rectangle(PointToScreen(item._bounds.Location), item._bounds.Size);
					}
				}
			}
			if (_childMenu != null)
			{
				return _childMenu.GetCommandScreenBounds(uiCommand);
			}
			return Rectangle.Empty;
		}

		internal CommandLayoutState HitTestItem(int x, int y)
		{
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (((FormatStyleLayoutState)item).IsVisible() && item._bounds.Contains(x, y))
				{
					return item;
				}
			}
			if (_isOverflowMenu && _overflowItems != null && _overflowItems.Count > 0)
			{
				foreach (CommandLayoutState overflowItem in _overflowItems)
				{
					if (overflowItem._bounds.Contains(x, y))
					{
						return overflowItem;
					}
				}
			}
			return null;
		}

		public CommandLayoutState HitTestItemRow(int y)
		{
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (((FormatStyleLayoutState)item).IsVisible() && item._bounds.Contains(item._bounds.X, y))
				{
					return item;
				}
			}
			return null;
		}

		private FormatStyleLayoutState HitTestButton(int x, int y)
		{
			if (_expandBar != null && _expandBar._bounds.Contains(x, y))
			{
				return _expandBar;
			}
			if (_scrollDownButton != null && _scrollDownButton._bounds.Contains(x, y))
			{
				return _scrollDownButton;
			}
			if (_scrollUpButton != null && _scrollUpButton._bounds.Contains(x, y))
			{
				return _scrollUpButton;
			}
			if (_tearOffBar != null && _tearOffBar._bounds.Contains(x, y))
			{
				return _tearOffBar;
			}
			return null;
		}

		internal void SetHasCheckMargin(bool hasCheckMargin)
		{
			_hasCheckMargin = hasCheckMargin;
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			if (_shadowWindow != null)
			{
				_shadowWindow.SetWindowBounds(new Rectangle(_menuLocation, new Size(base.Bounds.Width + ShadowSize, base.Bounds.Height + ShadowSize)));
			}
			base.OnResize(eventArgs);
		}

		private void OnControlMouseMove(object sender, MouseEventArgs eventArgs)
		{
			OnMouseMove(RenderingHelper.TranslateMouseEvent((Control)sender, this, eventArgs));
		}

		private void OnControlLostFocus(object sender, EventArgs eventArgs)
		{
			if (sender is MxComboBase)
			{
				((MxComboBase)sender).OnAcceptChanges(EventArgs.Empty);
			}
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
			if (!base.Visible)
			{
				if (_shadowWindow != null)
				{
					_shadowWindow.DestroyHandle();
					_shadowWindow = null;
				}
			}
			else if (_shadowWindow == null && _holdMessages && VisualStyleSupport.SupportsLayeredWindows() && ColorSchemeWindowMessageHandler.IsShadowAllowed())
			{
				_shadowWindow = new ColorSchemeWindowMessageHandler();
				_shadowWindow.CreateShadowWindow(new Rectangle(base.Location, new Size(base.Bounds.Width + ShadowSize, base.Bounds.Height + ShadowSize)), _canUseThemes);
			}
		}

		private void StartTearOff()
		{
			foreach (Control control in base.Controls)
			{
				control.Resize -= OnControlResize;
				control.MouseMove -= OnControlMouseMove;
				control.LostFocus -= OnControlLostFocus;
			}
			MxCommandBar uiCommandBar = GetCommandManagerOwner().CommandBars.FindBarByBaseCommand(_parentItem.GetCommand().BaseCommand);
			if (uiCommandBar == null)
			{
				string key = GetCommandManagerOwner().GetUniqueCommandBarName(_parentItem.GetCommand().Key);
				uiCommandBar = new MxCommandBar(key);
				uiCommandBar.SetSourceCommand(_parentItem.GetCommand().BaseCommand);
				GetCommandManagerOwner().CommandBars.Add(uiCommandBar);
			}
			if (uiCommandBar != null && GetCommandManagerOwner().CommandBars.Contains(uiCommandBar))
			{
				_parentItem.GetCommand().BaseCommand.FloatingBar = uiCommandBar;
				uiCommandBar.SetClosed(false);
				Point point = (uiCommandBar.FloatingLocation = Cursor.Position);
				if (uiCommandBar.GetFloatingWindow() == null)
				{
					uiCommandBar._forceDockStyleUpdate = true;
					uiCommandBar.DockStyle = BarsDockStyle.Floating;
				}
				Rectangle bounds = _tearOffBar._bounds;
				bounds.Inflate(TearOffHitMargin, TearOffHitMargin);
				bounds.Location = PointToScreen(bounds.Location);
				base.Visible = false;
				uiCommandBar.GetFloatingWindow()._resizeMode = ResizeMode.Floating;
				RenderingHelper.SetMouseCapture(uiCommandBar.GetFloatingWindow(), true);
				uiCommandBar.GetFloatingWindow()._dragStart = point;
				uiCommandBar.GetFloatingWindow()._dragMessageFilter = CommandBarMessageFilter.StartWindowDrag(uiCommandBar, ResizeMode.Floating, point, _tearOffDragStart);
				uiCommandBar.GetFloatingWindow()._dragMessageFilter._floatingGrabOffset.X = (int)((float)_tearOffDragStart.X / (float)base.Width * (float)uiCommandBar.FloatingSize.Width);
				uiCommandBar.GetFloatingWindow()._dragMessageFilter._dropZoneBounds = bounds;
				uiCommandBar.GetFloatingWindow()._dragMessageFilter._tearOffControl = this;
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if ((FindOwnerCommandBar() != null && !FindOwnerCommandBar().HasMouseMoved()) || _isExpanding || (FindOwnerCommandBar() != null && GetCommandManagerOwner()._isCustomizing))
			{
				return;
			}
			if (eventArgs.Button == MouseButtons.Left && !_tearOffDragStart.IsEmpty)
			{
				if (IsOutsideTearOffBar(eventArgs.X, eventArgs.Y))
				{
					SetTearingOff(true);
					StartTearOff();
				}
				return;
			}
			if (_parentItem != null)
			{
				_owner.SelectedCommand = _parentItem.GetCommand();
			}
			FormatStyleLayoutState uiFormatStyleLayoutState = HitTestButton(eventArgs.X, eventArgs.Y);
			SetHotButton(uiFormatStyleLayoutState);
			if (uiFormatStyleLayoutState != null)
			{
				SelectItem(null);
				SetOpenItem(null);
				return;
			}
			CommandLayoutState uiCommandLayoutState = HitTestItem(eventArgs.X, eventArgs.Y);
			if (uiCommandLayoutState == GetSelectedItem())
			{
				return;
			}
			if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand() != null && (!uiCommandLayoutState.GetCommand().IsEnabled || uiCommandLayoutState.GetCommand().IsSeparator() || uiCommandLayoutState.GetCommand().CommandType == CommandType.Label))
			{
				SelectItem(null);
				StopSubmenuTimer();
				uiCommandLayoutState.GetCommand().NotifyMouseHover();
				_selectedItem = uiCommandLayoutState;
			}
			else
			{
				SelectItem(uiCommandLayoutState);
				if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand() != null && uiCommandLayoutState.GetCommand().IsParent && uiCommandLayoutState.GetCommand().IsEnabled && (ChildMenu == null || ChildMenu._parentItem != uiCommandLayoutState))
				{
					StartSubmenuTimer();
				}
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new CommandMenuAccessibleObject(this);
		}

		protected override void OnDoubleClick(EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
			Point point = PointToClient(Cursor.Position);
			CommandLayoutState uiCommandLayoutState = HitTestItem(point.X, point.Y);
			if (uiCommandLayoutState == null || !((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() || !uiCommandLayoutState.GetCommand().IsParent)
			{
				return;
			}
			foreach (MxCommand command in uiCommandLayoutState.GetCommand().Commands)
			{
				if (command.IsEnabled && command.IsVisible && command.IsDefaultItem())
				{
					StopSubmenuTimer();
					command.PerformClick();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			_tearOffDragStart = Point.Empty;
			if (eventArgs.Button != MouseButtons.Left || !_isItemMouseDown)
			{
				return;
			}
			CommandLayoutState uiCommandLayoutState = HitTestItem(eventArgs.X, eventArgs.Y);
			if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand().CanBeClicked() && (!uiCommandLayoutState.GetCommand().IsParent || (_overflowItems != null && _overflowItems.ContainsItem(uiCommandLayoutState))))
			{
				if (uiCommandLayoutState.GetCommand().CommandManager != null && FindOwnerCommandBar() != null)
				{
					FindOwnerCommandBar()._isInvokingShortcut = true;
				}
				StopSubmenuTimer();
				uiCommandLayoutState.GetCommand().PerformClick();
				if (FindOwnerCommandBar() != null)
				{
					FindOwnerCommandBar()._isInvokingShortcut = false;
				}
				if (GetCommandManagerOwner() != null)
				{
					GetCommandManagerOwner().SetActiveBar(null);
				}
				if (uiCommandLayoutState.GetCommand().IsParent)
				{
					CloseMenuChain();
				}
			}
		}

		internal bool IsOutsideTearOffBar(int x, int y)
		{
			Rectangle bounds = _tearOffBar._bounds;
			bounds.Inflate(TearOffHitMargin, TearOffHitMargin);
			if (!bounds.Contains(x, y))
			{
				return true;
			}
			return false;
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			StopSubmenuTimer();
			if (GetCommandManagerOwner() != null && GetCommandManagerOwner()._isCustomizing)
			{
				return;
			}
			FormatStyleLayoutState uiFormatStyleLayoutState = HitTestButton(eventArgs.X, eventArgs.Y);
			if (eventArgs.Button == MouseButtons.Left && uiFormatStyleLayoutState != null)
			{
				_isItemMouseDown = false;
				if (uiFormatStyleLayoutState._caption == "InfrequentItemsBar")
				{
					StopHoverTimer();
					ExpandInfrequentItems();
				}
				if (uiFormatStyleLayoutState._caption == "FloatingBar")
				{
					_tearOffDragStart = new Point(eventArgs.X, eventArgs.Y);
				}
			}
			else
			{
				_isItemMouseDown = true;
				CommandLayoutState uiCommandLayoutState = HitTestItem(eventArgs.X, eventArgs.Y);
				if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand().IsParent && ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() && !uiCommandLayoutState.GetCommand().IsSeparator())
				{
					OpenChildMenu(uiCommandLayoutState, false);
				}
			}
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			_isMouseInside = true;
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			_isMouseInside = false;
			if (GetHotButton() != null && !_isTearingOff)
			{
				SetHotButton(null);
			}
			SelectItem(null);
		}
	}
}
