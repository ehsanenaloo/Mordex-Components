using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		internal MxCommand FindCommandByShortcut(Shortcut shortcut)
		{
			return FindCommandByShortcut(shortcut, Commands);
		}

		private MxCommand FindCommandByShortcut(Shortcut shortcut, MxCommandCollection uiCommandCollection)
		{
			foreach (MxCommand item in uiCommandCollection)
			{
				if (item.Shortcut == shortcut)
				{
					return item;
				}
				MxCommand childMatch = FindCommandByShortcut(shortcut, item.Commands);
				if (childMatch != null)
				{
					return childMatch;
				}
			}
			return null;
		}

		internal Control GetActiveCommandControl()
		{
			return _activeCommandControl;
		}

		internal void SetActiveCommandControl(Control control)
		{
			if (_activeCommandControl != control)
			{
				_activeCommandControl = control;
			}
		}

		internal MxCommandBar GetActiveBar()
		{
			return _activeBar;
		}

		internal void SetActiveBar(MxCommandBar newActiveBar)
		{
			if (_activeBar == newActiveBar)
			{
				return;
			}
			if (_activeBar != null)
			{
				MxCommandBar uiCommandBar = _activeBar;
				_activeBar = null;
				uiCommandBar._isMenuActive = false;
				if (uiCommandBar._customizeItem != null)
				{
					uiCommandBar._customizeItem.SetHot(false);
				}
				uiCommandBar.SetChildMenu(null);
				uiCommandBar.SetHotItem(null);
			}
			_activeBar = newActiveBar;
		}

		internal void CloseActiveBarMenu(CommandCloseReason commandCloseReason)
		{
			if (_activeBar != null)
			{
				if (!_activeBar.TryCloseChildMenu(commandCloseReason))
				{
					return;
				}
				MxCommandBar uiCommandBar = _activeBar;
				_activeBar = null;
				uiCommandBar._isMenuActive = false;
				if (uiCommandBar._customizeItem != null)
				{
					uiCommandBar._customizeItem.SetHot(false);
				}
				uiCommandBar.SetChildMenu(null);
				uiCommandBar.SetHotItem(null);
			}
			_activeBar = null;
		}

		internal void RefreshToolTips()
		{
			foreach (MxCommandBar item in _commandBars)
			{
				item.RefreshToolTipText();
			}
		}

		internal bool CanActivateBar(MxCommandBar uiCommandBar)
		{
			if (_activeCommandControl != null)
			{
				return false;
			}
			if (_activeBar != null && _activeBar != uiCommandBar)
			{
				return false;
			}
			return true;
		}

		internal bool IsInDesignMode()
		{
			return base.DesignMode;
		}

		internal void AttachContextMenu(MxContextMenu component)
		{
			component._commandManager = this;
			if (base.DesignMode && base.Container != null && component.Container == null)
			{
				base.Container.Add(component);
			}
			component.BindBaseCommands();
		}

		internal void DetachContextMenu(MxContextMenu component)
		{
			if (base.DesignMode && base.Container != null && component.Container != null)
			{
				component.Container.Remove(component);
			}
			component._commandManager = null;
		}

		private MxCommandBar FindMenuBar()
		{
			foreach (MxCommandBar commandBar in CommandBars)
			{
				if (commandBar.Visible && commandBar.Enabled && commandBar.CommandBarType == CommandBarType.Menu)
				{
					return commandBar;
				}
			}
			return null;
		}

		internal bool ActivateNextBar()
		{
			if (GetActiveBar() == null || CommandBars.Count == 1)
			{
				return false;
			}
			int activeIndex = CommandBars.IndexOf(GetActiveBar());
			if (activeIndex >= 0)
			{
				for (int i = activeIndex + 1; i < CommandBars.Count; i++)
				{
					MxCommandBar uiCommandBar = CommandBars[i];
					if (uiCommandBar.Visible)
					{
						uiCommandBar._mousePoint = Cursor.Position;
						SetActiveBar(uiCommandBar);
						uiCommandBar.SelectNextItem();
						return true;
					}
				}
				for (int previousIndex = activeIndex - 1; previousIndex >= 0; previousIndex--)
				{
					MxCommandBar previousBar = CommandBars[previousIndex];
					if (previousBar.Visible)
					{
						previousBar._mousePoint = Cursor.Position;
						SetActiveBar(previousBar);
						previousBar.SelectNextItem();
						return true;
					}
				}
			}
			return false;
		}

		private int GetShortcutKeys(IntPtr intPtr)
		{
			int keys = (int)intPtr;
			if ((Control.ModifierKeys & Keys.Alt) != Keys.Alt)
			{
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					keys += (int)Keys.Shift;
				}
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
				{
					keys += (int)Keys.Control;
				}
			}
			return keys;
		}

		private bool ProcessMnemonic(char character)
		{
			if (GetActiveBar() != null && GetActiveBar().HandleMnemonic(character))
			{
				return true;
			}
			foreach (MxCommandBar commandBar in CommandBars)
			{
				if (commandBar.Enabled && commandBar.Visible && commandBar.HandleMnemonic(character))
				{
					return true;
				}
			}
			return false;
		}

		private bool ProcessShortcut(Shortcut shortcut, Control control, Message msg)
		{
			foreach (MxCommandBar commandBar in CommandBars)
			{
				if (commandBar.InvokeShortcut(shortcut, commandBar.Commands, control, msg))
				{
					return true;
				}
			}
			if (_shortcutBehavior == ShortcutBehavior.Always)
			{
				foreach (MxCommand command in Commands)
				{
					if (command.IsVisible && command.IsEnabled && command.Shortcut == shortcut)
					{
						if (control == null || control.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
						{
							command.PerformClick();
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
			return false;
		}

		private MxCommandBar GetFocusedBar()
		{
			if (GetActiveBar() == null)
			{
				foreach (MxCommandBar commandBar in CommandBars)
				{
					if (commandBar.ContainsFocus)
					{
						return commandBar;
					}
				}
			}
			return GetActiveBar();
		}

		private bool FilterMessage(ref Message msg)
		{
			bool result = false;
			if (_containerControl == null || !_containerControl.Enabled || !_containerControl.Visible)
			{
				return false;
			}
			try
			{
				if (GetForm() != Form.ActiveForm && (Form.ActiveForm == null || Form.ActiveForm.ActiveMdiChild == null || Form.ActiveForm.ActiveMdiChild != GetForm()))
				{
					return false;
				}
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// A message filter must never throw into the message loop.
				return false;
			}
			switch (msg.Msg)
			{
			case WmActivateApp:
				if (GetActiveBar() != null)
				{
					CloseActiveBarMenu(CommandCloseReason.AppFocusChange);
				}
				break;
			case WmRButtonUp:
			{
				if (GetEditTargetControls() == null)
				{
					break;
				}
				Control rightClickControl = Control.FromHandle(msg.HWnd);
				if (rightClickControl != null && TryShowEditContextMenu(rightClickControl))
				{
					result = true;
				}
				break;
			}
			case WmNcLButtonDown:
			case WmNcRButtonDown:
			case WmNcMButtonDown:
			case WmLButtonDown:
			case WmRButtonDown:
			case WmMButtonDown:
			case WmXButtonDown:
				if (GetActiveBar() != null)
				{
					CloseActiveBarMenu(CommandCloseReason.AppClicked);
				}
				ResetActiveControl(false);
				return false;
			case WmKeyDown:
			{
				int keyData = GetShortcutKeys(msg.WParam);
				MxCommandBar uiCommandBar = GetFocusedBar();
				switch (keyData)
				{
				case (int)(Keys.Control | Keys.Tab):
					if (uiCommandBar != null)
					{
						if (GetActiveCommandControl() == null)
						{
							ActivateNextBar();
						}
						else
						{
							_containerControl.SelectNextControl(GetActiveCommandControl(), forward: true, tabStopOnly: true, nested: true, wrap: true);
						}
						result = true;
					}
					break;
				case (int)Keys.Escape:
					if (GetActiveBar() != null)
					{
						CloseActiveBarMenu(CommandCloseReason.Keyboard);
						result = true;
					}
					break;
				case (int)Keys.Apps:
				{
					if (GetEditTargetControls() == null)
					{
						break;
					}
					Control appsKeyControl = Control.FromHandle(msg.HWnd);
					if (appsKeyControl != null && TryShowEditContextMenu(appsKeyControl))
					{
						result = true;
					}
					break;
				}
				case (int)Keys.Left:
					if (GetActiveBar() != null)
					{
						GetActiveBar().HandleLeftKey();
						result = true;
					}
					break;
				case (int)Keys.Tab:
					if (uiCommandBar != null)
					{
						if (!uiCommandBar.TabStop)
						{
							uiCommandBar.HandleRightKey();
							result = true;
						}
						else
						{
							SetActiveBar(null);
						}
					}
					break;
				case (int)Keys.Right:
					if (GetActiveBar() != null)
					{
						GetActiveBar().HandleRightKey();
						result = true;
					}
					break;
				case (int)Keys.Enter:
					if (GetActiveBar() == null)
					{
						break;
					}
					if (GetActiveBar().GetHotItem() != null && ((FormatStyleLayoutState)GetActiveBar().GetHotItem()).IsEnabled())
					{
						if (GetActiveBar().GetHotItem()._hostsControl)
						{
							if (GetActiveBar().GetHotItem().GetCommand() != null && GetActiveBar().GetHotItem().GetCommand().Control != null)
							{
								RenderingHelper.TryFocusControl(GetActiveBar().GetHotItem().GetCommand().Control);
							}
						}
						else if (GetActiveBar().GetHotItem().GetCommand() != null && GetActiveBar().GetHotItem().GetCommand().IsParent)
						{
							if (GetActiveBar().IsHorizontalLayout())
							{
								GetActiveBar().HandleDownKey();
							}
							else
							{
								GetActiveBar().HandleLeftKey();
							}
						}
						else if (GetActiveBar().GetHotItem().GetCommand() != null)
						{
							GetActiveBar().GetHotItem().GetCommand().PerformClick();
						}
					}
					result = true;
					break;
				case (int)Keys.Down:
					if (GetActiveBar() != null)
					{
						GetActiveBar().HandleDownKey();
						result = true;
					}
					break;
				case (int)Keys.Up:
					if (GetActiveBar() != null)
					{
						GetActiveBar().HandleUpKey();
						result = true;
					}
					break;
				default:
				{
					Shortcut keyShortcut = (Shortcut)keyData;
					if (!Enum.IsDefined(typeof(Shortcut), keyShortcut))
					{
						break;
					}
					Control targetControl = Control.FromHandle(msg.HWnd);
					if (_contextMenusByComponent != null && _contextMenusByComponent.Count > 0 && targetControl != null && _contextMenusByComponent.ContainsKey(targetControl))
					{
						if (targetControl.PreProcessControlMessage(ref msg) != PreProcessControlState.MessageNotNeeded)
						{
							break;
						}
						MxContextMenu targetMenu = (MxContextMenu)_contextMenusByComponent[targetControl];
						if (targetMenu != null && targetMenu.ExecuteShortcutCommand(keyShortcut, targetMenu.Commands, targetControl, msg))
						{
							result = true;
							break;
						}
					}
					if (GetActiveBar() != null && GetActiveBar().GetChildMenu() == null)
					{
						if (!GetActiveBar().InvokeShortcut(keyShortcut, GetActiveBar().Commands, targetControl, msg))
						{
							if (ProcessShortcut(keyShortcut, targetControl, msg))
							{
								result = true;
							}
						}
						else
						{
							result = true;
						}
					}
					else
					{
						if (_commandBars == null)
						{
							break;
						}
						Control sourceControl = Control.FromHandle(msg.HWnd);
						if (ProcessShortcut(keyShortcut, sourceControl, msg))
						{
							result = true;
						}
					}
					break;
				}
				}
				break;
			}
			case WmChar:
				if (GetActiveBar() != null)
				{
					ProcessMnemonic((char)(int)msg.WParam);
					result = true;
				}
				break;
			case WmKeyUp:
				if ((int)msg.WParam == (int)Keys.Apps && GetActiveBar() != null && GetActiveBar().GetChildMenu() == null)
				{
					Point point = Point.Empty;
					if (GetActiveBar().GetHotItem() != null)
					{
						point = GetActiveBar().GetHotItem()._bounds.Location;
					}
					ShowToolbarsMenu(GetActiveBar(), point);
					SetActiveBar(null);
					result = true;
				}
				break;
			case WmSysKeyUp:
				if (Control.ModifierKeys == Keys.Control || ((int)msg.WParam != (int)Keys.Menu && (int)msg.WParam != (int)Keys.F10) || (_isF10Handled && (int)msg.WParam == (int)Keys.F10))
				{
					break;
				}
				if (GetActiveBar() != null)
				{
					SetActiveBar(null);
				}
				else if (_canActivateMenuOnAltUp)
				{
					MxCommandBar menuBar = FindMenuBar();
					if (menuBar != null && menuBar.FocusFirstItem())
					{
						SetActiveBar(menuBar);
					}
				}
				else
				{
					_canActivateMenuOnAltUp = true;
				}
				result = true;
				break;
			case WmSysChar:
			{
				if (GetActiveBar() != null)
				{
					break;
				}
				bool isAltCombination = false;
				Shortcut sysCharShortcut;
				if ((int)msg.WParam == (int)Keys.F10)
				{
					int f10Keys = GetShortcutKeys(msg.WParam);
					sysCharShortcut = (Shortcut)f10Keys;
				}
				else
				{
					isAltCombination = true;
					sysCharShortcut = (Shortcut)((int)Keys.Alt + (int)msg.WParam);
				}
				if (Enum.IsDefined(typeof(Shortcut), sysCharShortcut) && (!isAltCombination || !char.IsLetterOrDigit((char)(int)msg.WParam)))
				{
					break;
				}
				MxCommandBar mnemonicMenuBar = FindMenuBar();
				int lParamBits = msg.LParam.ToInt32();
				if ((lParamBits & SysCharSkipFlag) == SysCharSkipFlag)
				{
					break;
				}
				char c = (char)(int)msg.WParam;
				if (!char.IsLetterOrDigit(c))
				{
					break;
				}
				if (mnemonicMenuBar != null && mnemonicMenuBar.HandleMnemonic(c))
				{
					result = true;
				}
				else
				{
					if (_commandBars == null)
					{
						break;
					}
					foreach (MxCommandBar commandBar in CommandBars)
					{
						if (commandBar.Visible && commandBar.Enabled && commandBar.HandleMnemonic(c))
						{
							result = true;
							break;
						}
					}
				}
				break;
			}
			case WmSysKeyDown:
			{
				_isF10Handled = false;
				if (GetActiveBar() != null)
				{
					if ((int)msg.WParam != (int)Keys.Menu && (int)msg.WParam != (int)Keys.F10)
					{
						Shortcut shortcut = (Shortcut)((int)Keys.Alt + (int)msg.WParam);
						if (!Enum.IsDefined(typeof(Shortcut), shortcut))
						{
							ProcessMnemonic((char)(int)msg.WParam);
						}
						result = true;
					}
					else
					{
						_canActivateMenuOnAltUp = false;
						result = true;
						SetActiveBar(null);
					}
					break;
				}
				Shortcut sysKeyShortcut;
				if ((int)msg.WParam == (int)Keys.F10)
				{
					int sysKeys = GetShortcutKeys(msg.WParam);
					sysKeyShortcut = (Shortcut)sysKeys;
				}
				else
				{
					sysKeyShortcut = (Shortcut)((int)Keys.Alt + (int)msg.WParam);
				}
				if (!Enum.IsDefined(typeof(Shortcut), sysKeyShortcut))
				{
					break;
				}
				Control control = Control.FromHandle(msg.HWnd);
				if (_contextMenusByComponent != null && _contextMenusByComponent.Count > 0 && control != null && _contextMenusByComponent.ContainsKey(control))
				{
					MxContextMenu uiContextMenu = (MxContextMenu)_contextMenusByComponent[control];
					if (uiContextMenu != null && uiContextMenu.ExecuteShortcutCommand(sysKeyShortcut, uiContextMenu.Commands, control, msg))
					{
						if (sysKeyShortcut == Shortcut.F10)
						{
							_isF10Handled = true;
						}
						result = true;
						break;
					}
				}
				if (GetActiveBar() != null && GetActiveBar().GetChildMenu() == null)
				{
					if (!GetActiveBar().InvokeShortcut(sysKeyShortcut, GetActiveBar().Commands, control, msg))
					{
						if (ProcessShortcut(sysKeyShortcut, control, msg))
						{
							if (sysKeyShortcut == Shortcut.F10)
							{
								_isF10Handled = true;
							}
							result = true;
						}
					}
					else
					{
						result = true;
					}
				}
				else if (_commandBars != null && ProcessShortcut(sysKeyShortcut, control, msg))
				{
					if (sysKeyShortcut == Shortcut.F10)
					{
						_isF10Handled = true;
					}
					result = true;
				}
				break;
			}
			}
			return result;
		}

		bool IMessageFilter.PreFilterMessage(ref Message msg)
		{
			return FilterMessage(ref msg);
		}
	}
}
