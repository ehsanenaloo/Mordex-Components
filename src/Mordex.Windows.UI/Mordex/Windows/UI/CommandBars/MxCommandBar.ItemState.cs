using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		internal bool TryCloseChildMenu(CommandCloseReason commandCloseReason)
		{
			if (_childMenu != null && _childMenu._parentItem != null && _childMenu._parentItem.GetCommand() != null)
			{
				MxCommand uiCommand = _childMenu._parentItem.GetCommand();
				if (uiCommand.ConfirmClose(commandCloseReason))
				{
					SetChildMenu(null);
					return true;
				}
				return false;
			}
			return true;
		}

		internal CommandControl GetChildMenu()
		{
			return _childMenu;
		}

		internal void SetChildMenu(CommandControl uiCommandControl)
		{
			if (_childMenu == uiCommandControl)
			{
				return;
			}
			if (_childMenu != null)
			{
				_childMenu.Dispose();
				if (uiCommandControl != null)
				{
					Form form = RenderingHelper.FindForm(this);
					if (form != null)
					{
						form.Invalidate();
						form.Update();
					}
				}
			}
			_childMenu = uiCommandControl;
			ShowCommandToolTip(null, null);
			if (_childMenu != null && CommandManager != null)
			{
				CommandManager.ResetActiveControl(false);
				CommandManager.SetActiveCommandControl(null);
				CommandManager.SetActiveBar(this);
			}
		}

		internal bool HasMouseMoved()
		{
			if (!_mousePoint.IsEmpty)
			{
				Point position = Cursor.Position;
				if (_mousePoint.X == position.X && _mousePoint.Y == position.Y)
				{
					return false;
				}
			}
			return true;
		}

		internal bool ShouldShowInfrequentItems()
		{
			if (CommandManager != null && CommandManager.AlwaysShowFullMenus)
			{
				return true;
			}
			if (CommandManager != null && CommandManager._isCustomizing)
			{
				return true;
			}
			return _showInfrequentItems;
		}

		internal void SetShowInfrequentItems(bool showInfrequentItems)
		{
			if (_showInfrequentItems != showInfrequentItems)
			{
				_showInfrequentItems = showInfrequentItems;
			}
		}

		internal CommandLayoutState GetPressedItem()
		{
			return _pressedItem;
		}

		internal void SetPressedItem(CommandLayoutState uiCommandLayoutState)
		{
			if (_pressedItem != uiCommandLayoutState)
			{
				if (_pressedItem != null)
				{
					_pressedItem.MouseDown = false;
					_pressedItem.SetPressed(false);
					_pressedItem.SetHot(false);
				}
				_pressedItem = uiCommandLayoutState;
				if (_pressedItem != null)
				{
					_pressedItem.MouseDown = true;
				}
			}
		}

		internal CommandLayoutState GetSelectedItem()
		{
			return _selectedItem;
		}

		internal void SetSelectedItem(CommandLayoutState uiCommandBarUICommandLayoutState)
		{
			if (_selectedItem == uiCommandBarUICommandLayoutState)
			{
				return;
			}
			if (_selectedItem != null)
			{
				_selectedItem.SetPressed(false);
				_selectedItem._isDropDownPressed = false;
				if (_selectedItemShadow != null)
				{
					_selectedItemShadow.DestroyHandle();
					_selectedItemShadow = null;
				}
			}
			_selectedItem = uiCommandBarUICommandLayoutState;
			if (_selectedItem != null)
			{
				_selectedItem.SetPressed(false);
				_selectedItem.SetHot(false);
			}
			Invalidate();
		}

		internal void HideToolTip()
		{
			_toolTipText = "";
			_toolTipItem = null;
			if (_toolTipWindow != null)
			{
				_toolTipWindow.RemoveShownHandler(OnToolTipWindowShown);
				_toolTipWindow.HideAndDisposeTimers();
				_toolTipWindow = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		internal void UpdateItemToolTip(CommandLayoutState uiCommandLayoutState)
		{
			if (uiCommandLayoutState == null)
			{
				if (_toolTipText.Length <= 0)
				{
					HideToolTip();
				}
			}
			else if (!base.ContainsFocus)
			{
				ShowCommandToolTip(uiCommandLayoutState.GetCommand(), uiCommandLayoutState);
			}
		}

		internal void ShowToolTipText(string uiCommandBarText)
		{
			if (uiCommandBarText == null || uiCommandBarText.Length <= 0)
			{
				HideToolTip();
			}
			else
			{
				if (string.Compare(uiCommandBarText, _toolTipText, ignoreCase: true) == 0)
				{
					return;
				}
				_toolTipText = uiCommandBarText;
				if (!ShouldShowToolTips())
				{
					return;
				}
				if (VisualStyleSupport.IsToolTipHookSupported())
				{
					if (_toolTipWindow == null)
					{
						_toolTipWindow = new WindowMessageHandler(Font, ShouldUseCompatibleTextRendering());
						_toolTipWindow.RequestShow(_toolTipText, this);
					}
					else
					{
						_toolTipWindow.UpdateTipText(_toolTipText);
					}
				}
				else
				{
					if (_toolTip == null)
					{
						_toolTip = new Mordex.Windows.Common.ThemedToolTip();
					}
					_toolTip.SetToolTip(this, _toolTipText);
				}
			}
		}

		internal void RefreshToolTipText()
		{
			if (_toolTipItem != null && _toolTipItem.GetCommand() != null)
			{
				_toolTipText = _toolTipItem.GetCommand().ToolTipText;
				if (CommandManager.ShowShortcutInToolTips && _toolTipItem.GetCommand().Shortcut != Shortcut.None)
				{
					_toolTipText = _toolTipText + " (" + RenderingHelper.GetShortcutText(_toolTipItem.GetCommand().Shortcut) + ")";
				}
				if (_toolTipWindow != null)
				{
					_toolTipWindow.UpdateTipText(_toolTipText);
				}
				else if (_toolTip != null)
				{
					_toolTip.SetToolTip(this, _toolTipText);
				}
			}
		}

		private void OnToolTipWindowShown(object sender, EventArgs eventArgs)
		{
			if (_toolTipItem == null)
			{
				return;
			}
			_toolTipItem._accessibleRole = AccessibleRole.ToolTip;
			try
			{
				if (_toolTipItem == _customizeItem)
				{
					AccessibilityNotifyClients(AccessibleEvents.Show, _layoutItems.Count);
				}
				else
				{
					AccessibilityNotifyClients(AccessibleEvents.Show, _toolTipItem._index);
				}
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// Accessibility notification is best effort and must not stop the tooltip.
			}
		}

		internal void ShowCommandToolTip(MxCommand uiCommand, CommandLayoutState item)
		{
			if (item == null || uiCommand == null || (uiCommand.ToolTipText != null && uiCommand.ToolTipText.Length == 0) || _childMenu != null)
			{
				HideToolTip();
			}
			else
			{
				if (!ShouldShowToolTips() || item.Equals(_toolTipItem))
				{
					return;
				}
				int toolTipDelay = -1;
				if (_toolTipWindow != null || _toolTip != null)
				{
					toolTipDelay = 0;
					HideToolTip();
				}
				_toolTipItem = item;
				string text = uiCommand.ToolTipText;
				if (CommandManager.ShowShortcutInToolTips && uiCommand.Shortcut != Shortcut.None)
				{
					text = text + " (" + RenderingHelper.GetShortcutText(uiCommand.Shortcut) + ")";
				}
				if (VisualStyleSupport.IsToolTipHookSupported())
				{
					if (_toolTipWindow == null)
					{
						_toolTipWindow = new WindowMessageHandler(Font, ShouldUseCompatibleTextRendering());
						if (toolTipDelay >= 0)
						{
							_toolTipWindow.SetShowDelayMs(0);
						}
						_toolTipWindow.AddShownHandler(OnToolTipWindowShown);
						_toolTipWindow.RequestShow(text, this);
					}
					else
					{
						_toolTipWindow.UpdateTipText(text);
					}
					return;
				}
				if (_toolTip == null)
				{
					_toolTip = new Mordex.Windows.Common.ThemedToolTip();
					if (toolTipDelay >= 0)
					{
						_toolTip.InitialDelay = toolTipDelay;
						_toolTip.ReshowDelay = toolTipDelay;
					}
				}
				_toolTip.SetToolTip(this, text);
			}
		}

		internal CommandLayoutState GetHotItem()
		{
			return _hotItem;
		}

		internal void SetHotItem(CommandLayoutState item)
		{
			if (_hotItem == item || (_childMenu != null && (item == null || (_hotItem != null && _hotItem.GetCommand() != null && item.GetCommand() != null && item.GetCommand().Key == _hotItem.GetCommand().Key && item.GetCommand().GetLayoutIndex() == _hotItem.GetCommand().GetLayoutIndex()))) || (item == null && CommandManager != null && CommandManager.GetActiveBar() == this))
			{
				return;
			}
			if (_hotItem != null)
			{
				_hotItem.SetDropDownHot(false);
				if (_hotItem._hostsControl)
				{
					DeactivateHostedCombo(_hotItem);
				}
				_hotItem.SetHot(false);
				if (_hotItem.GetCommand() != null)
				{
					_hotItem.GetCommand().NotifyMouseLeave();
				}
			}
			_hotItem = item;
			if (_hotItem == null)
			{
				return;
			}
			if (_hotItem.GetCommand() != null && !_hotItem.GetCommand().CanBeHotTracked())
			{
				_hotItem.GetCommand().NotifyMouseHover();
				return;
			}
			if (_hotItem != GetSelectedItem() && _hotItem.GetCommand() != null && (_hotItem.GetCommand().IsButtonType() || !_isHorizontal))
			{
				_hotItem.SetHot(true);
			}
			_hotItem.GetCommand().NotifyMouseHover();
			if (!_isMenuActive)
			{
				return;
			}
			if (_hotItem.GetCommand().IsParent)
			{
				if (GetChildMenu() == null || GetChildMenu()._parentItem != _hotItem)
				{
					OpenChildMenu(_hotItem, false);
				}
			}
			else
			{
				TryCloseChildMenu(CommandCloseReason.CommandFocused);
			}
		}

		private void OpenChildMenu(CommandLayoutState parentItem, bool selectFirstItem)
		{
			if (TryCloseChildMenu(CommandCloseReason.CommandFocused))
			{
				PopupSettings baseSettings = BuildPopupSettings(parentItem.GetCommand());
				CommandControl uiCommandControl = new CommandControl(parentItem.GetCommand().Commands, parentItem, this, baseSettings, holdMessages: true);
				uiCommandControl._dropsDown = _isHorizontal;
				uiCommandControl._selectFirstItem = _isKeyboardNavigation;
				_isKeyboardNavigation = selectFirstItem;
				SetChildMenu(uiCommandControl);
				uiCommandControl.ShowAt(PointToScreen(parentItem._bounds.Location));
			}
		}
	}
}
