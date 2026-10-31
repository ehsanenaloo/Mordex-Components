using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		private void OnControlMouseEnter(object sender, EventArgs eventArgs)
		{
			if (sender is MxComboBase)
			{
				MxComboBase uiComboBase = sender as MxComboBase;
				if (CommandManager == null || !CommandManager.CanActivateBar(this) || CommandManager._isCustomizing)
				{
					uiComboBase.SetMouseOver(false);
					return;
				}
			}
			if (CommandManager != null && (GetParentForm() == null || GetParentForm().ContainsFocus))
			{
				Point point = PointToClient(Cursor.Position);
				CommandLayoutState uiCommandLayoutState = HitTestItem(point.X, point.Y);
				UpdateItemToolTip(uiCommandLayoutState);
				SetHotItem(uiCommandLayoutState);
			}
		}

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			HideToolTip();
			if (CommandManager != null)
			{
				if (CommandManager.GetActiveBar() != this)
				{
					_commandManager.CloseActiveBarMenu(CommandCloseReason.AppClicked);
				}
				CommandManager.SetActiveCommandControl(sender as Control);
			}
		}

		private void OnControlLostFocus(object sender, EventArgs eventArgs)
		{
			Control control = sender as Control;
			if (!control.ContainsFocus && CommandManager != null)
			{
				CommandManager.SetActiveCommandControl(null);
			}
		}

		private void OnControlMouseLeave(object sender, EventArgs eventArgs)
		{
			if (CommandManager == null || CommandManager._isCustomizing)
			{
				return;
			}
			Point point = PointToClient(Cursor.Position);
			CommandLayoutState uiCommandLayoutState = HitTestItem(point.X, point.Y);
			UpdateItemToolTip(uiCommandLayoutState);
			if (CommandManager.GetActiveBar() == this && (uiCommandLayoutState == null || uiCommandLayoutState.GetCommand().Control == sender))
			{
				if (sender is MxComboBase)
				{
					MxComboBase uiComboBase = sender as MxComboBase;
					uiComboBase.SetMouseOver(true);
				}
			}
			else if (CommandManager.CanActivateBar(this))
			{
				SetHotItem(uiCommandLayoutState);
			}
		}

		private void OnControlResize(object sender, EventArgs eventArgs)
		{
			if (CommandManager != null)
			{
				InvalidateLayout();
			}
		}

		internal void EndGripDrag()
		{
			if (_messageFilter != null)
			{
				_gripMouseDownPoint = Point.Empty;
				_messageFilter = null;
			}
			if (_gripItem != null)
			{
				_gripItem.MouseDown = false;
			}
			_gripMouseDownPoint = Point.Empty;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (_messageFilter != null)
			{
				_messageFilter.EndDrag();
			}
			else
			{
				if (_mdiButtons != null)
				{
					_mdiButtons.HandleMouseUp(e);
				}
				CommandLayoutState uiCommandLayoutState = HitTestItem(e.X, e.Y);
				if (uiCommandLayoutState != null && uiCommandLayoutState.MouseDown)
				{
					TryCloseChildMenu(CommandCloseReason.CommandClicked);
					if (uiCommandLayoutState != null && (!uiCommandLayoutState._hostsControl || !_isHorizontal))
					{
						uiCommandLayoutState.SetHot(false);
						uiCommandLayoutState.SetPressed(false);
						uiCommandLayoutState.GetCommand().PerformClick();
					}
				}
				if (_gripItem != null)
				{
					_gripItem.MouseDown = false;
				}
				_gripMouseDownPoint = Point.Empty;
			}
			SetPressedItem(null);
			if (GetHotItem() != null)
			{
				if (_hotItem.GetCommand() != null)
				{
					if (_hotItem.GetCommand().IsButtonType())
					{
						_hotItem.SetHot(true);
					}
				}
				else
				{
					_hotItem.SetHot(true);
				}
			}
			if (GetChildMenu() == null)
			{
				_isMenuActive = false;
			}
		}

		internal void ShowCustomizeMenu(CommandLayoutState parentItem, Point point)
		{
			BuildCustomizeMenu(parentItem);
			if (TryCloseChildMenu(CommandCloseReason.CommandFocused))
			{
				PopupSettings baseSettings = BuildPopupSettings(_customizeCommand);
				CommandControl uiCommandControl = new CommandControl(_customizeCommand.Commands, parentItem, this, baseSettings, holdMessages: true);
				uiCommandControl._isOverflowMenu = true;
				uiCommandControl._dropsDown = _isHorizontal;
				SetChildMenu(uiCommandControl);
				uiCommandControl.ShowAt(point);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			CommandLayoutState toolTipItem = _toolTipItem;
			HideToolTip();
			_toolTipItem = toolTipItem;
			if (!HasMouseMoved())
			{
				_mousePoint = Point.Empty;
				return;
			}
			if (CommandManager != null)
			{
				if (CommandManager._isCustomizing)
				{
					return;
				}
				if (base.ContainsFocus)
				{
					CommandManager.ResetActiveControl(true);
				}
			}
			if (e.Button == MouseButtons.Right)
			{
				if (CommandManager.ShowQuickCustomizeMenu)
				{
					CommandManager.ShowToolbarsMenu(this);
				}
				return;
			}
			CommandLayoutState uiCommandLayoutState = null;
			if (GetChildMenu() != null)
			{
				uiCommandLayoutState = GetChildMenu()._parentItem;
			}
			CommandManager.CloseActiveBarMenu(CommandCloseReason.AppClicked);
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			if (_gripItem != null && _gripItem._bounds.Contains(e.X, e.Y))
			{
				_gripMouseDownPoint = new Point(e.X, e.Y);
				_gripItem.MouseDown = true;
				return;
			}
			if (_mdiButtons != null)
			{
				_mdiButtons.HandleMouseDown(e);
			}
			if (_customizeItem != null && _customizeItem._bounds.Contains(e.X, e.Y))
			{
				ShowCustomizeMenu(_customizeItem, PointToScreen(_customizeItem._bounds.Location));
				return;
			}
			CommandLayoutState hitItem = HitTestItem(e.X, e.Y);
			if (hitItem == null)
			{
				return;
			}
			if (hitItem.GetCommand().CanBeClicked() && (_commandBarType != CommandBarType.Menu || !hitItem.GetCommand().IsParent) && !hitItem._dropDownBounds.Contains(e.X, e.Y) && (!hitItem.GetCommand().IsParent || hitItem.GetCommand().GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton))
			{
				SetPressedItem(hitItem);
				SetHotItem(hitItem);
				hitItem.SetPressed(true);
			}
			if (!hitItem.GetCommand().IsParent || !((FormatStyleLayoutState)hitItem).IsEnabled() || hitItem == uiCommandLayoutState)
			{
				return;
			}
			if (_commandBarType != CommandBarType.Menu && hitItem.GetCommand().GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton)
			{
				if (!hitItem._dropDownBounds.Contains(e.X, e.Y))
				{
					return;
				}
				hitItem._isDropDownPressed = true;
			}
			else
			{
				_isMenuActive = true;
			}
			if (hitItem != uiCommandLayoutState)
			{
				OpenChildMenu(hitItem, false);
			}
		}

		private void StartChildMenuCloseTimer()
		{
			if (_childMenuCloseTimer == null)
			{
				_childMenuCloseTimer = new Timer();
				_childMenuCloseTimer.Tick += OnChildMenuCloseTimerTick;
			}
			_childMenuCloseTimer.Interval = ChildMenuCloseDelay;
			_childMenuCloseTimer.Start();
		}

		private void StopChildMenuCloseTimer()
		{
			Timer timer = _childMenuCloseTimer;
			if (timer != null)
			{
				_childMenuCloseTimer = null;
				timer.Stop();
				timer.Tick -= OnChildMenuCloseTimerTick;
				timer.Dispose();
			}
		}

		private void OnChildMenuCloseTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				StopChildMenuCloseTimer();
				if (GetChildMenu() == null || GetChildMenu().Bounds.Contains(Cursor.Position) || !TryCloseChildMenu(CommandCloseReason.CommandFocused))
				{
					return;
				}
				CommandManager.SetActiveBar(null);
				SetChildMenu(null);
				Point pt = PointToClient(Cursor.Position);
				if (!base.ClientRectangle.Contains(pt))
				{
					SetHotItem(null);
					if (_customizeItem != null)
					{
						_customizeItem.SetHot(false);
					}
				}
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// The bar or its manager can be torn down between ticks; closing the popup is best effort.
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			Point pt = PointToClient(Cursor.Position);
			CommandLayoutState uiCommandLayoutState = HitTestItem(pt.X, pt.Y);
			if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand().IsParent)
			{
				foreach (MxCommand command in uiCommandLayoutState.GetCommand().Commands)
				{
					if (command.IsEnabled && command.IsVisible && command.IsDefaultItem())
					{
						command.PerformClick();
					}
				}
				return;
			}
			if (!_isFullShown && _gripItem != null && _gripItem._bounds.Contains(pt))
			{
				if (_isHorizontal)
				{
					base.Width = GetFullLength();
				}
				else
				{
					base.Height = GetFullLength();
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!HasMouseMoved())
			{
				return;
			}
			if (_messageFilter != null)
			{
				_messageFilter.UpdateDrag(Cursor.Position);
			}
			else if (_gripItem != null && _gripItem.MouseDown && !_gripMouseDownPoint.IsEmpty)
			{
				if (IsOutsideDragThreshold(e.X, e.Y) && _messageFilter == null)
				{
					_messageFilter = CommandBarMessageFilter.StartMove(this, _gripMouseDownPoint, SaveDockedPosition);
				}
			}
			else
			{
				if (CommandManager == null || !CommandManager.CanActivateBar(this))
				{
					return;
				}
				if (GetFloatingWindow() == null && GetParentForm() != null && !GetParentForm().ContainsFocus && !GetParentForm().IsMdiChild)
				{
					Form form = RenderingHelper.FindForm(this);
					if (form == null || !form.ContainsFocus)
					{
						return;
					}
				}
				if (CommandManager._isCustomizing)
				{
					return;
				}
				if (_mdiButtons != null && _mdiButtons.HandleMouseMove(e))
				{
					ShowToolTipText(_mdiButtons.GetHoveredButtonToolTipText());
				}
				if (_customizeItem != null && (_customizeItem.MouseDown || e.Button == MouseButtons.None))
				{
					if (_customizeItem._bounds.Contains(e.X, e.Y))
					{
						SelectCustomizeItem();
						ShowCommandToolTip(GetCustomizeCommand(), _customizeItem);
						return;
					}
					_customizeItem.SetHot(false);
				}
				CommandLayoutState uiCommandLayoutState = HitTestItem(e.X, e.Y);
				UpdateItemToolTip(uiCommandLayoutState);
				if (uiCommandLayoutState != null && _commandBarType != CommandBarType.Menu)
				{
					if (uiCommandLayoutState != _selectedItem && GetChildMenu() != null)
					{
						StartChildMenuCloseTimer();
					}
					else
					{
						StopChildMenuCloseTimer();
					}
				}
				if (e.Button == MouseButtons.Left)
				{
					if (uiCommandLayoutState != null && uiCommandLayoutState.MouseDown)
					{
						uiCommandLayoutState.SetPressed(true);
					}
					else if (GetPressedItem() != null)
					{
						GetPressedItem().SetPressed(false);
						GetPressedItem().SetHot(true);
					}
					else
					{
						SetHotItem(uiCommandLayoutState);
					}
					return;
				}
				SetHotItem(uiCommandLayoutState);
			}
		}
	}
}
