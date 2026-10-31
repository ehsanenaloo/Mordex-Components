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
		private void DeactivateHostedCombo(CommandLayoutState uiCommandLayoutState)
		{
			if (uiCommandLayoutState.GetCommand().Control != null && uiCommandLayoutState.GetCommand().Control is MxComboBase)
			{
				MxComboBase uiComboBase = uiCommandLayoutState.GetCommand().Control as MxComboBase;
				uiComboBase.SetMouseOver(false);
			}
		}

		private void FocusHostedControl(CommandLayoutState uiCommandLayoutState)
		{
			if (uiCommandLayoutState._hostsControl && uiCommandLayoutState.GetCommand().Control != null)
			{
				if (uiCommandLayoutState.GetCommand().Control is MxComboBase)
				{
					MxComboBase uiComboBase = uiCommandLayoutState.GetCommand().Control as MxComboBase;
					uiComboBase.SetMouseOver(true);
				}
				uiCommandLayoutState.GetCommand().Control.Focus();
			}
			else if (base.ContainsFocus)
			{
				CommandManager.ResetActiveControl(false);
			}
		}

		internal void HandleLeftKey()
		{
			if (GetChildMenu() == null)
			{
				_mousePoint = Cursor.Position;
				if (IsHorizontalLayout())
				{
					SelectPreviousItem();
				}
				else
				{
					OpenHotItem();
				}
			}
		}

		internal void SelectNextItem()
		{
			ShowCommandToolTip(null, null);
			CommandManager.SetActiveBar(this);
			_mousePoint = Cursor.Position;
			int startIndex = 0;
			if (_hotItem != null)
			{
				startIndex = _hotItem._index + 1;
			}
			if (startIndex >= _layoutItems.Count && _customizeItem == null)
			{
				startIndex = 0;
			}
			if (_customizeItem != null && ((FormatStyleLayoutState)_customizeItem).IsHot())
			{
				_customizeItem.SetHot(false);
			}
			for (int i = startIndex; i < _layoutItems.Count; i++)
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_layoutItems.FindItemByIndex(i);
				if (((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() && !uiCommandLayoutState.GetCommand().IsSeparator() && ((FormatStyleLayoutState)uiCommandLayoutState).IsVisible())
				{
					bool canSelect = true;
					if (uiCommandLayoutState._hostsControl)
					{
						canSelect = GetHost() == null || !GetHost().ContainsFocus;
						FocusHostedControl(uiCommandLayoutState);
						canSelect = GetHost() == null || !GetHost().ContainsFocus;
					}
					_isKeyboardNavigation = true;
					if (canSelect)
					{
						CommandManager.ResetActiveControl(true);
					}
					SetHotItem(uiCommandLayoutState);
					return;
				}
			}
			if (_customizeItem != null)
			{
				SelectCustomizeItem();
			}
		}

		private void SelectCustomizeItem()
		{
			if (_hotItem != null)
			{
				if (_hotItem._hostsControl)
				{
					DeactivateHostedCombo(_hotItem);
				}
				else if (GetChildMenu() != null)
				{
					_customizeItem.SetHot(true);
					StartChildMenuCloseTimer();
					return;
				}
				if (TryCloseChildMenu(CommandCloseReason.CommandFocused))
				{
					_hotItem.SetHot(false);
					if (_hotItem.GetCommand() != null)
					{
						_hotItem.GetCommand().NotifyMouseLeave();
					}
					_hotItem = null;
					if (base.ContainsFocus)
					{
						CommandManager.ResetActiveControl(false);
					}
				}
			}
			_customizeItem.SetHot(true);
		}

		internal void FocusItem(CommandLayoutState item, bool selectNext, bool selectPrevious)
		{
			if (item != null && item.GetCommand() != null)
			{
				item = _layoutItems.FindCommandItem(item.GetCommand());
			}
			if (_layoutItems.ContainsItem(item))
			{
				_mousePoint = Cursor.Position;
				_hotItem = item;
				_hotItem.SetHot(true);
			}
			else if (item != null && _customizeItem == item)
			{
				SelectCustomizeItem();
			}
			if (selectNext)
			{
				SelectNextItem();
			}
			else if (selectPrevious)
			{
				SelectPreviousItem();
			}
		}

		internal void SelectPreviousItem()
		{
			ShowCommandToolTip(null, null);
			_mousePoint = Cursor.Position;
			CommandManager.SetActiveBar(this);
			int startIndex = 0;
			if (_hotItem != null)
			{
				startIndex = _hotItem._index - 1;
			}
			else if (_customizeItem != null && ((FormatStyleLayoutState)_customizeItem).IsHot())
			{
				startIndex = _layoutItems.Count - 1;
			}
			if (startIndex < 0)
			{
				startIndex = ((_customizeItem == null) ? (_layoutItems.Count - 1) : (-1));
			}
			if (_customizeItem != null && ((FormatStyleLayoutState)_customizeItem).IsHot())
			{
				_customizeItem.SetHot(false);
			}
			for (int index = startIndex; index >= 0; index--)
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_layoutItems.FindItemByIndex(index);
				if (((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() && !uiCommandLayoutState.GetCommand().IsSeparator() && ((FormatStyleLayoutState)uiCommandLayoutState).IsVisible())
				{
					bool canSelect = true;
					if (uiCommandLayoutState._hostsControl)
					{
						canSelect = GetHost() == null || !GetHost().ContainsFocus;
						FocusHostedControl(uiCommandLayoutState);
					}
					_isKeyboardNavigation = true;
					if (canSelect)
					{
						CommandManager.ResetActiveControl(true);
					}
					SetHotItem(uiCommandLayoutState);
					return;
				}
			}
			if (_customizeItem != null)
			{
				SelectCustomizeItem();
			}
		}

		private void OpenHotItem()
		{
			if (GetHotItem() != null)
			{
				MxCommand uiCommand = GetHotItem().GetCommand();
				if (uiCommand.IsEnabled && uiCommand.Commands.Count > 0)
				{
					_isKeyboardNavigation = true;
					_isMenuActive = true;
					OpenChildMenu(GetHotItem(), true);
				}
				else if (_hotItem.GetCommand().Control is MxComboBase)
				{
					MxComboBase uiComboBase = _hotItem.GetCommand().Control as MxComboBase;
					RenderingHelper.TryFocusControl(uiComboBase);
					uiComboBase.DroppedDown = true;
				}
			}
			else if (_customizeItem != null && ((FormatStyleLayoutState)_customizeItem).IsHot())
			{
				ShowCustomizeMenu(_customizeItem, PointToScreen(_customizeItem._bounds.Location));
			}
		}

		internal void HandleUpKey()
		{
			if (GetChildMenu() == null)
			{
				_mousePoint = Cursor.Position;
				if (!IsHorizontalLayout())
				{
					_isMenuActive = false;
					SelectPreviousItem();
					_isMenuActive = true;
				}
				else
				{
					OpenHotItem();
				}
			}
		}

		internal void HandleDownKey()
		{
			if (GetChildMenu() == null)
			{
				_mousePoint = Cursor.Position;
				if (IsHorizontalLayout())
				{
					OpenHotItem();
					return;
				}
				_isMenuActive = false;
				SelectNextItem();
				_isMenuActive = true;
			}
		}

		internal void HandleRightKey()
		{
			if (GetChildMenu() == null)
			{
				_mousePoint = Cursor.Position;
				if (IsHorizontalLayout())
				{
					SelectNextItem();
				}
				else
				{
					OpenHotItem();
				}
			}
		}

		internal bool HandleMnemonic(char character)
		{
			character = char.ToLower(character);
			if (GetChildMenu() == null)
			{
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					if (!uiCommand.HasMnemonic() || !uiCommand.IsEnabled || !uiCommand.IsEffectivelyVisible() || !base.Enabled || uiCommand.CommandType == CommandType.Separator)
					{
						continue;
					}
					CommandStyle commandStyle = GetEffectiveCommandStyle(uiCommand);
					if (commandStyle == CommandStyle.Image)
					{
						continue;
					}
					char c = char.ToLower(uiCommand.Mnemonic);
					if (character == c)
					{
						SetHotItem(_layoutItems.FindCommandItem(uiCommand));
						if (!uiCommand.IsParent)
						{
							Update();
							uiCommand.PerformClick();
							SetHotItem(null);
							return true;
						}
						if (GetHotItem() != null)
						{
							_isMenuActive = true;
							OpenChildMenu(GetHotItem(), true);
							_mousePoint = Cursor.Position;
							return true;
						}
					}
				}
			}
			return false;
		}

		internal bool InvokeShortcut(Shortcut shortcut, MxCommandCollection uiCommandCollection, Control control, Message msg)
		{
			if (!base.Enabled || !base.Visible)
			{
				return false;
			}
			if (_mdiSystemMenu != null)
			{
				foreach (MxCommand command in _mdiSystemMenu.Commands)
				{
					if (command.IsEnabled && command.IsEffectivelyVisible() && command.Shortcut == shortcut)
					{
						if (control == null || control.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
						{
							_isInvokingShortcut = true;
							command.PerformClick();
							_isInvokingShortcut = false;
						}
						else if (control != null)
						{
							NativeMethods.NativeMsg c = new NativeMethods.NativeMsg
							{
								_hwnd = msg.HWnd,
								_lParam = msg.LParam,
								_wParam = msg.WParam,
								_message = msg.Msg
							};
							NativeMethods.TranslateMessage(ref c);
							NativeMethods.DispatchMessage(ref c);
						}
						return true;
					}
				}
			}
			foreach (MxCommand item in uiCommandCollection)
			{
				if (!item.IsEnabled || !item.IsEffectivelyVisible())
				{
					continue;
				}
				if (item.Shortcut == shortcut)
				{
					if (control == null || control.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
					{
						_isInvokingShortcut = true;
						item.PerformClick();
						_isInvokingShortcut = false;
					}
					else if (control != null)
					{
						NativeMethods.NativeMsg nativeMessage = new NativeMethods.NativeMsg
						{
							_hwnd = msg.HWnd,
							_lParam = msg.LParam,
							_wParam = msg.WParam,
							_message = msg.Msg
						};
						NativeMethods.TranslateMessage(ref nativeMessage);
						NativeMethods.DispatchMessage(ref nativeMessage);
					}
					return true;
				}
				if (item.Commands.Count > 0 && InvokeShortcut(shortcut, item.Commands, control, msg))
				{
					return true;
				}
			}
			return false;
		}

		internal bool FocusFirstItem()
		{
			for (int i = 0; i < _commands.Count; i++)
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_layoutItems.FindItemByIndex(i);
				if (((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() && uiCommandLayoutState.GetCommand().IsButtonType())
				{
					_mousePoint = Cursor.Position;
					SetHotItem(uiCommandLayoutState);
					return true;
				}
			}
			return false;
		}

		private void ExpandCommand(MxCommand uiCommand)
		{
			CommandLayoutState uiCommandLayoutState = _layoutItems.FindCommandItem(uiCommand);
			if (uiCommandLayoutState != null)
			{
				OpenChildMenu(uiCommandLayoutState, false);
				return;
			}
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (!item.GetCommand().IsSeparator() && ContainsCommandKey(uiCommand.Key, item.GetCommand()))
				{
					uiCommandLayoutState = item;
					break;
				}
			}
			if (uiCommandLayoutState != null && TryCloseChildMenu(CommandCloseReason.CommandFocused))
			{
				PopupSettings baseSettings = BuildPopupSettings(uiCommandLayoutState.GetCommand());
				CommandControl uiCommandControl = new CommandControl(uiCommandLayoutState.GetCommand().Commands, uiCommandLayoutState, this, baseSettings, holdMessages: true);
				uiCommandControl._dropsDown = _isHorizontal;
				SetChildMenu(uiCommandControl);
				GetChildMenu().ShowCommandPath(uiCommand, PointToScreen(uiCommandLayoutState._bounds.Location));
			}
		}

		internal bool ContainsCommandKey(string strA, MxCommand uiCommand)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommand command in uiCommand.Commands)
			{
				if (string.Compare(strA, command.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
				if (ContainsCommandKey(strA, command))
				{
					return true;
				}
			}
			return false;
		}
	}
}
