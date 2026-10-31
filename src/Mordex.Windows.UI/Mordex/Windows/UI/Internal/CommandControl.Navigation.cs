using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
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
		private bool ContainsScreenPoint(NativeMethods.NativePoint screenPoint)
		{
			Rectangle bounds = base.Bounds;
			if (!_isItemMouseDown && _isExpanding)
			{
				bounds.Height += ExpandHitExtraHeight;
			}
			if (bounds.Contains(screenPoint._x, screenPoint._y))
			{
				return true;
			}
			if (_owner is CommandControl)
			{
				return ((CommandControl)_owner).ContainsScreenPoint(screenPoint);
			}
			return false;
		}

		internal CommandControl FindMenuAt(Point point)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Point pt = PointToClient(point);
			if (clientRectangle.Contains(pt))
			{
				return this;
			}
			if (ChildMenu != null)
			{
				return ChildMenu.FindMenuAt(point);
			}
			return null;
		}

		private bool HandleLeftKey()
		{
			NativeMethods.NativeMsg c = default(NativeMethods.NativeMsg);
			NativeMethods.TryGetMessage(ref c);
			if (_overflowItems != null && _overflowItems.Count > 0)
			{
				SelectPreviousItem();
				return true;
			}
			if (_owner is MxCommandBar)
			{
				MxCommandBar uiCommandBar = FindOwnerCommandBar();
				if (uiCommandBar.TryCloseChildMenu(CommandCloseReason.Keyboard))
				{
					uiCommandBar.SetChildMenu(null);
					if (uiCommandBar.IsHorizontalLayout())
					{
						uiCommandBar.FocusItem(_parentItem, false, true);
					}
					else
					{
						uiCommandBar.FocusItem(_parentItem, false, false);
					}
				}
			}
			else if (_owner is CommandControl)
			{
				((CommandControl)_owner).TryCloseChildMenu(CommandCloseReason.Keyboard);
			}
			return true;
		}

		private bool HandleUpKey()
		{
			if (_isOverflowMenu && _selectedItem != null && _selectedItem.GetCommand().IsParent && ((FormatStyleLayoutState)_selectedItem).IsEnabled())
			{
				NativeMethods.NativeMsg c = default(NativeMethods.NativeMsg);
				NativeMethods.TryGetMessage(ref c);
				OpenChildMenu(_selectedItem, true);
				return true;
			}
			return SelectPreviousItem();
		}

		private bool SelectPreviousItem()
		{
			int startIndex = -1;
			if (_selectedItem != null)
			{
				startIndex = _selectedItem._index - 1;
			}
			if (_layoutItems.Count == 0)
			{
				return false;
			}
			if (startIndex < 0)
			{
				startIndex = _layoutItems.Count - 1;
			}
			int lastVisibleIndex = _layoutItems.Count - 1 - _visibleItemCount;
			for (int index = startIndex; index >= 0; index--)
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_layoutItems.FindItemByIndex(index);
				if (!uiCommandLayoutState.GetCommand().IsSeparator())
				{
					if (!((FormatStyleLayoutState)uiCommandLayoutState).IsVisible())
					{
						SelectItem(null);
						_pendingSelectionIndex = startIndex;
						if (startIndex < _firstVisibleIndex)
						{
							ScrollUp();
						}
						else
						{
							_alignItemsToTop = false;
							if (startIndex <= lastVisibleIndex)
							{
								_firstVisibleIndex = startIndex;
							}
							else
							{
								_firstVisibleIndex = lastVisibleIndex + 1;
							}
							InvalidateLayout();
						}
					}
					else
					{
						SelectItem(uiCommandLayoutState);
					}
					return true;
				}
			}
			return false;
		}

		private bool SelectNextItem()
		{
			int startIndex = 0;
			if (_selectedItem != null)
			{
				startIndex = _selectedItem._index + 1;
			}
			bool hasExpandBar = false;
			int itemCount = _layoutItems.Count;
			if (itemCount <= 0)
			{
				return false;
			}
			CommandLayoutState uiCommandLayoutState = (CommandLayoutState)_layoutItems.FindItemByIndex(itemCount - 1);
			if (uiCommandLayoutState.GetCommand().IsSeparator())
			{
				itemCount--;
			}
			if (_expandBar != null)
			{
				itemCount++;
				hasExpandBar = true;
			}
			if (startIndex > itemCount - 1)
			{
				startIndex = 0;
			}
			for (int i = startIndex; i < itemCount; i++)
			{
				if (startIndex == itemCount - 1 && hasExpandBar)
				{
					_selectedItem = null;
					_selectFirstItem = true;
					if (_owner != null)
					{
						_owner.ShowInfrequentItems = true;
					}
					InvalidateLayout();
					return true;
				}
				CommandLayoutState candidateItem = (CommandLayoutState)_layoutItems.FindItemByIndex(i);
				if (candidateItem.GetCommand().IsSeparator())
				{
					continue;
				}
				if (!((FormatStyleLayoutState)candidateItem).IsVisible())
				{
					SelectItem(null);
					_pendingSelectionIndex = startIndex;
					if (startIndex < _firstVisibleIndex)
					{
						_alignItemsToTop = true;
						_firstVisibleIndex = startIndex;
						InvalidateLayout();
					}
					else
					{
						ScrollDown();
					}
				}
				else
				{
					SelectItem(candidateItem);
				}
				return true;
			}
			return false;
		}

		private bool HandleDownKey()
		{
			if (_isOverflowMenu && _selectedItem != null && _selectedItem.GetCommand().IsParent && ((FormatStyleLayoutState)_selectedItem).IsEnabled())
			{
				NativeMethods.NativeMsg c = default(NativeMethods.NativeMsg);
				NativeMethods.TryGetMessage(ref c);
				OpenChildMenu(_selectedItem, true);
				return true;
			}
			return SelectNextItem();
		}

		private bool HandleRightKey()
		{
			if (ChildMenu == null)
			{
				NativeMethods.NativeMsg c = default(NativeMethods.NativeMsg);
				NativeMethods.TryGetMessage(ref c);
				if (_overflowItems != null && _overflowItems.Count > 0)
				{
					SelectNextItem();
					return true;
				}
				if (GetSelectedItem() != null && _selectedItem.GetCommand().IsParent && ((FormatStyleLayoutState)_selectedItem).IsEnabled())
				{
					OpenChildMenu(_selectedItem, true);
					return true;
				}
				MxCommandBar uiCommandBar = FindOwnerCommandBar();
				if (uiCommandBar != null)
				{
					if (!uiCommandBar.IsHorizontalLayout())
					{
						return false;
					}
					if (uiCommandBar.TryCloseChildMenu(CommandCloseReason.Keyboard))
					{
						uiCommandBar.FocusItem(_parentItem, true, false);
					}
				}
				return true;
			}
			return false;
		}

		private bool IsHandleInControls(IntPtr handle, ControlCollection controlCollection)
		{
			foreach (Control item in controlCollection)
			{
				if (item.Handle == handle)
				{
					return true;
				}
				if (NativeMethods.IsChild(item.Handle, handle))
				{
					return true;
				}
				Control handleControl = Control.FromHandle(handle);
				if (handleControl != null)
				{
					for (handleControl = RenderingHelper.GetParentControl(handleControl); handleControl != null; handleControl = RenderingHelper.GetParentControl(handleControl))
					{
						if (handleControl == item)
						{
							return true;
						}
					}
				}
				if (item is IDropListOwner)
				{
					if (((IDropListOwner)item).DropWindowHandle == handle)
					{
						return true;
					}
					if (NativeMethods.IsChild(((IDropListOwner)item).DropWindowHandle, handle))
					{
						return true;
					}
				}
			}
			return false;
		}

		private MxCommand FindCommandForControl(Control control, ref bool isDropWindow)
		{
			foreach (CommandLayoutState item in _layoutItems)
			{
				MxCommand uiCommand = item.GetCommand();
				if (uiCommand == null || !uiCommand.IsControlEditable() || uiCommand.Control == null)
				{
					continue;
				}
				if (uiCommand.Control == control)
				{
					return uiCommand;
				}
				if (NativeMethods.IsChild(uiCommand.Control.Handle, control.Handle))
				{
					return uiCommand;
				}
				if (uiCommand.Control is IDropListOwner)
				{
					IDropListOwner dropElement = (IDropListOwner)uiCommand.Control;
					if (dropElement.DropWindowHandle == control.Handle)
					{
						isDropWindow = true;
						return uiCommand;
					}
					if (NativeMethods.IsChild(dropElement.DropWindowHandle, control.Handle))
					{
						return uiCommand;
					}
				}
				for (Control ancestor = RenderingHelper.GetParentControl(control); ancestor != null; ancestor = RenderingHelper.GetParentControl(ancestor))
				{
					if (ancestor == uiCommand.Control)
					{
						return uiCommand;
					}
				}
			}
			if (_parentItem != null && _parentItem.GetCommand() != null && (_parentItem.GetCommand().CommandType == CommandType.ControlPopup || _parentItem.GetCommand().CommandType == CommandType.ColorPickerCommand))
			{
				Control popupControl = _parentItem.GetCommand().Control;
				if (popupControl != null)
				{
					if (popupControl == control)
					{
						return _parentItem.GetCommand();
					}
					if (NativeMethods.IsChild(popupControl.Handle, control.Handle))
					{
						return _parentItem.GetCommand();
					}
					if (popupControl is IDropListOwner)
					{
						IDropListOwner popupDropElement = (IDropListOwner)popupControl;
						if (popupDropElement.DropWindowHandle == control.Handle)
						{
							isDropWindow = true;
							return _parentItem.GetCommand();
						}
						if (NativeMethods.IsChild(popupDropElement.DropWindowHandle, control.Handle))
						{
							return _parentItem.GetCommand();
						}
					}
					for (Control popupAncestor = RenderingHelper.GetParentControl(control); popupAncestor != null; popupAncestor = RenderingHelper.GetParentControl(popupAncestor))
					{
						if (popupAncestor == popupControl)
						{
							return _parentItem.GetCommand();
						}
					}
				}
			}
			return null;
		}

		private bool IsOwnerMenuHandle(IntPtr handle)
		{
			if (_owner is CommandControl)
			{
				for (CommandControl uiCommandControl = (CommandControl)_owner; uiCommandControl != null; uiCommandControl = ((!(uiCommandControl._owner is CommandControl)) ? null : ((CommandControl)uiCommandControl._owner)))
				{
					if (uiCommandControl.Handle == handle)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			return base.ProcessMnemonic(charCode);
		}

		// Closes the menu and leaves the private message loop when the queue ends (WM_QUIT) or fails.
		private void EndModalLoop()
		{
			TryCloseMenu(CommandCloseReason.AppFocusChange);
			_isClosing = true;
		}

		private void RunMessageLoop()
		{
			NativeMethods.NativeMsg c = default(NativeMethods.NativeMsg);
			bool skipDispatch = false;
			NativeMethods.NativePoint cursorPoint = default(NativeMethods.NativePoint);
			bool closedOnFocusChange = false;
			bool redirectKeyToMenu = false;
			while (!_isClosing)
			{
				if (!NativeMethods.WaitMessage())
				{
					EndModalLoop();
					break;
				}
				while (!_isClosing && NativeMethods.PeekMessage(ref c, IntPtr.Zero, 0u, 0u, 0u))
				{
					bool discardMessage = false;
					switch (c._message)
					{
					case WmMouseMove:
					{
						if ((GetCommandManagerOwner() != null && _isTearingOff) || base.IsDisposed || !(c._hwnd != base.Handle))
						{
							break;
						}
						Point position = Cursor.Position;
						Point pt = PointToClient(position);
						if (base.ClientRectangle.Contains(pt))
						{
							CommandLayoutState uiCommandLayoutState = HitTestItem(pt.X, pt.Y);
							if (uiCommandLayoutState != null && (!((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() || uiCommandLayoutState.GetCommand().IsSeparator()))
							{
								uiCommandLayoutState = null;
							}
							if (uiCommandLayoutState != null && uiCommandLayoutState != GetSelectedItem() && (uiCommandLayoutState.GetCommand() == null || uiCommandLayoutState.GetCommand().IsEnabled))
							{
								SelectItem(uiCommandLayoutState);
								if (uiCommandLayoutState != null && (ChildMenu == null || ChildMenu._parentItem != uiCommandLayoutState))
								{
									StartSubmenuTimer();
								}
							}
							break;
						}
						Control targetControl = Control.FromHandle(c._hwnd);
						if (!(targetControl is CommandControl))
						{
							if (_isMouseInside)
							{
								SelectItem(null);
								_isMouseInside = false;
							}
							if (FindOwnerCommandBar() != targetControl)
							{
								discardMessage = true;
							}
						}
						break;
					}
					case WmNcLButtonUp:
					case WmNcLButtonDblClk:
					case WmNcRButtonUp:
					case WmNcRButtonDblClk:
					case WmNcMButtonUp:
					case WmNcMButtonDblClk:
					case WmNcXButtonUp:
					case WmLButtonUp:
					case WmLButtonDblClk:
					case WmRButtonUp:
					case WmRButtonDblClk:
					case WmMButtonUp:
					case WmMButtonDblClk:
					case WmXButtonUp:
					{
						_tearOffDragStart = Point.Empty;
						Point position = Cursor.Position;
						if (base.Capture)
						{
							RenderingHelper.SetMouseCapture(this, false);
							if (_tearOffBar != null)
							{
								Rectangle bounds = _tearOffBar._bounds;
								bounds.Inflate(TearOffHitMargin, TearOffHitMargin);
								bounds.Location = PointToScreen(bounds.Location);
								if (bounds.Contains(position))
								{
									discardMessage = true;
									break;
								}
							}
						}
						cursorPoint._x = position.X;
						cursorPoint._y = position.Y;
						MxCommandBar uiCommandBar = FindOwnerCommandBar();
						if (uiCommandBar != null && uiCommandBar.Handle == c._hwnd)
						{
							break;
						}
						if (!ContainsScreenPoint(cursorPoint))
						{
							if (!IsHandleInControls(c._hwnd, base.Controls))
							{
								if (_ignoreNextMouseUp)
								{
									_ignoreNextMouseUp = false;
								}
								else if (TryCloseMenu(CommandCloseReason.AppClicked))
								{
									_isClosing = true;
								}
							}
						}
						else
						{
							if (!(c._hwnd != base.Handle))
							{
								break;
							}
							if (_ignoreNextMouseUp)
							{
								_ignoreNextMouseUp = false;
								break;
							}
							Control control = Control.FromHandle(c._hwnd);
							if (control != null)
							{
								if (!NativeMethods.TryGetMessage(ref c))
								{
									EndModalLoop();
									break;
								}
								NativeMethods.TranslateMessage(ref c);
								NativeMethods.DispatchMessage(ref c);
								MxCommand uiCommand = FindCommandForControl(control, ref _isClosing);
								if (uiCommand == null)
								{
									if (!control.IsDisposed && !IsOwnerMenuHandle(control.Handle) && TryCloseMenu(CommandCloseReason.AppFocusChange))
									{
										closedOnFocusChange = true;
										_isClosing = true;
									}
								}
								else if ((uiCommand.CommandType == CommandType.ColorPickerCommand || uiCommand.CommandType == CommandType.ControlPopup) && !uiCommand.IsControlEditable() && TryCloseMenu(CommandCloseReason.AppFocusChange))
								{
									closedOnFocusChange = true;
									_isClosing = true;
								}
							}
							else
							{
								_isClosing = true;
							}
						}
						break;
					}
					case WmNcLButtonDown:
					case WmNcRButtonDown:
					case WmNcMButtonDown:
					case WmNcXButtonDown:
					case WmLButtonDown:
					case WmRButtonDown:
					case WmMButtonDown:
					case WmXButtonDown:
					{
						_ignoreNextMouseUp = false;
						Point position = Cursor.Position;
						cursorPoint._x = position.X;
						cursorPoint._y = position.Y;
						if (!base.Bounds.Contains(cursorPoint._x, cursorPoint._y))
						{
							if (ContainsScreenPoint(cursorPoint))
							{
								break;
							}
							MxCommandBar ownerBar = FindOwnerCommandBar();
							if (ownerBar != null)
							{
								Point point = ownerBar.PointToClient(new Point(cursorPoint._x, cursorPoint._y));
								CommandLayoutState barItem = ownerBar.HitTestItem(point.X, point.Y);
								if (barItem != null && barItem.Equals(_parentItem))
								{
									break;
								}
							}
							if (!IsHandleInControls(c._hwnd, base.Controls))
							{
								_owner.MousePoint = Cursor.Position;
								if (FindOwnerCommandBar() != null)
								{
									FindOwnerCommandBar().CommandManager.CloseActiveBarMenu(CommandCloseReason.AppClicked);
								}
								else if (TryCloseMenu(CommandCloseReason.AppClicked))
								{
									_isClosing = true;
								}
							}
						}
						else
						{
							if (!(c._hwnd == base.Handle))
							{
								break;
							}
							Point clientPoint = PointToClient(position);
							CommandLayoutState clickedItem = null;
							if (base.ClientRectangle.Contains(clientPoint))
							{
								clickedItem = HitTestItem(clientPoint.X, clientPoint.Y);
								if (clickedItem != null && (!((FormatStyleLayoutState)clickedItem).IsEnabled() || clickedItem.GetCommand().IsSeparator()))
								{
									clickedItem = null;
								}
							}
							if (clickedItem != GetSelectedItem())
							{
								SelectItem(clickedItem);
							}
						}
						break;
					}
					case WmSysKeyDown:
						if ((int)c._wParam == (int)Keys.Menu)
						{
							_isClosing = true;
							discardMessage = true;
							break;
						}
						if (IsHandleInControls(c._hwnd, base.Controls))
						{
							break;
						}
						c._message = WmKeyDown;
						goto case WmKeyDown;
					case WmChar:
					case WmSysChar:
						discardMessage = false;
						redirectKeyToMenu = false;
						if (!IsHandleInControls(c._hwnd, base.Controls))
						{
							redirectKeyToMenu = true;
						}
						break;
					case WmKeyDown:
						_isMouseOnly = false;
						redirectKeyToMenu = false;
						if (!IsHandleInControls(c._hwnd, base.Controls))
						{
							discardMessage = true;
							redirectKeyToMenu = true;
							switch ((int)c._wParam)
							{
							case (int)Keys.Up:
								if (Control.ModifierKeys == Keys.Alt)
								{
									discardMessage = false;
									break;
								}
								_owner.MousePoint = Cursor.Position;
								HandleUpKey();
								break;
							case (int)Keys.Tab:
								if (Control.ModifierKeys == Keys.Control && GetCommandManagerOwner() != null)
								{
									NativeMethods.NativeMsg tabMessage = default(NativeMethods.NativeMsg);
									NativeMethods.TryGetMessage(ref tabMessage);
									GetCommandManagerOwner().ActivateNextBar();
									discardMessage = false;
									skipDispatch = true;
								}
								else
								{
									_owner.MousePoint = Cursor.Position;
									HandleDownKey();
								}
								break;
							case (int)Keys.Down:
								if (Control.ModifierKeys == Keys.Control)
								{
									if (_expandBar != null)
									{
										ExpandInfrequentItems();
									}
								}
								else if (Control.ModifierKeys == Keys.Alt)
								{
									discardMessage = false;
								}
								else
								{
									_owner.MousePoint = Cursor.Position;
									HandleDownKey();
								}
								break;
							case (int)Keys.Left:
								_owner.MousePoint = Cursor.Position;
								if (HandleLeftKey())
								{
									discardMessage = false;
									skipDispatch = true;
								}
								break;
							case (int)Keys.Right:
								_owner.MousePoint = Cursor.Position;
								if (HandleRightKey())
								{
									discardMessage = false;
									skipDispatch = true;
								}
								break;
							case (int)Keys.Return:
								if (GetSelectedItem() == null)
								{
									break;
								}
								if (((FormatStyleLayoutState)GetSelectedItem()).IsEnabled())
								{
									if (GetSelectedItem().GetCommand().Control != null && GetSelectedItem().GetCommand().IsControlEditable())
									{
										RenderingHelper.TryFocusControl(GetSelectedItem().GetCommand().Control);
										break;
									}
									if (ChildMenu == null && GetSelectedItem().GetCommand().IsParent && ((FormatStyleLayoutState)GetSelectedItem()).IsEnabled() && !GetSelectedItem().GetCommand().IsSeparator())
									{
										NativeMethods.NativeMsg enterMessage = default(NativeMethods.NativeMsg);
										NativeMethods.TryGetMessage(ref enterMessage);
										discardMessage = false;
										skipDispatch = true;
										OpenChildMenu(GetSelectedItem(), true);
										break;
									}
									NativeMethods.NativeMsg commandMessage = default(NativeMethods.NativeMsg);
									NativeMethods.TryGetMessage(ref commandMessage);
									discardMessage = false;
									skipDispatch = true;
									StopSubmenuTimer();
									GetSelectedItem().GetCommand().PerformClick();
									if (FindOwnerCommandBar() != null && FindOwnerCommandBar().CommandManager != null)
									{
										FindOwnerCommandBar().CommandManager.SetActiveBar(null);
									}
								}
								else if (TryCloseChildMenu(CommandCloseReason.Keyboard))
								{
									_isClosing = true;
									if (FindOwnerCommandBar() != null)
									{
										FindOwnerCommandBar().CommandManager.SetActiveBar(null);
									}
								}
								break;
							case (int)Keys.Escape:
								if (_isTearingOff && GetCommandManagerOwner() != null)
								{
									NativeMethods.ReleaseCapture();
									MxCommandBar floatingBar = null;
									if (_parentItem != null)
									{
										floatingBar = GetCommandManagerOwner().CommandBars.FindBarByBaseCommand(_parentItem.GetCommand());
									}
									if (floatingBar != null)
									{
										GetCommandManagerOwner().CommandBars.Remove(floatingBar);
									}
									base.Visible = true;
									_isTearingOff = false;
								}
								else
								{
									if (!TryCloseChildMenu(CommandCloseReason.Keyboard))
									{
										break;
									}
									if (_owner is MxCommandBar)
									{
										if (FindOwnerCommandBar().TryCloseChildMenu(CommandCloseReason.Keyboard))
										{
											FindOwnerCommandBar().FocusItem(_parentItem, false, false);
											FindOwnerCommandBar()._isMenuActive = false;
											_isClosing = true;
										}
									}
									else if (_owner is CommandControl)
									{
										((CommandControl)_owner).TryCloseChildMenu(CommandCloseReason.Keyboard);
									}
									else if (_owner is MxContextMenu)
									{
										MxContextMenu uiContextMenu = (MxContextMenu)_owner;
										if (uiContextMenu.TryClose(CommandCloseReason.Keyboard))
										{
											_isClosing = true;
										}
									}
								}
								break;
							default:
							{
								discardMessage = false;
								if (FindOwnerContextMenu() == null)
								{
									break;
								}
								int keyData = GetKeyDataWithModifiers(c._wParam);
								Shortcut shortcut = (Shortcut)keyData;
								if (!Enum.IsDefined(typeof(Shortcut), shortcut))
								{
									break;
								}
								Control keyTarget = Control.FromHandle(c._hwnd);
								Message message = new Message
								{
									HWnd = c._hwnd,
									WParam = c._wParam,
									Msg = c._message,
									LParam = c._lParam
								};
								if (FindOwnerContextMenu().ExecuteShortcutCommand(shortcut, FindOwnerContextMenu().Commands, keyTarget, message))
								{
									discardMessage = true;
								}
								break;
							}
							}
						}
						else if ((int)c._wParam == (int)Keys.Return)
						{
							NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, IdcArrow));
							if (GetSelectedItem() != null)
							{
								StopSubmenuTimer();
								NativeMethods.TranslateMessage(ref c);
								NativeMethods.DispatchMessage(ref c);
								GetSelectedItem().GetCommand().PerformClick();
							}
							discardMessage = true;
							_isClosing = true;
						}
						else if ((int)c._wParam == (int)Keys.Escape && base.ContainsFocus)
						{
							RenderingHelper.TryFocusControl(this);
						}
						break;
					case WmKeyUp:
					case WmSysKeyUp:
						if (!IsHandleInControls(c._hwnd, base.Controls))
						{
							discardMessage = true;
						}
						_owner.MousePoint = Cursor.Position;
						break;
					}
					if (discardMessage)
					{
						NativeMethods.NativeMsg discardedMessage = default(NativeMethods.NativeMsg);
						if (!NativeMethods.TryGetMessage(ref discardedMessage, (uint)c._message, (uint)c._message))
						{
							EndModalLoop();
						}
					}
					else if (!skipDispatch)
					{
						if (!NativeMethods.TryGetMessage(ref c))
						{
							EndModalLoop();
						}
						else
						{
							if (c._message == WmSysKeyDown && !IsHandleInControls(c._hwnd, base.Controls))
							{
								c._message = WmKeyDown;
							}
							if ((c._message == WmKeyDown || c._message == WmChar) && redirectKeyToMenu)
							{
								c._hwnd = base.Handle;
							}
							NativeMethods.TranslateMessage(ref c);
							try
							{
								NativeMethods.DispatchMessage(ref c);
							}
							catch (SEHException)
							{
								// A native fault in another window's procedure must not end the menu loop.
							}
						}
					}
					else
					{
						skipDispatch = false;
					}
				}
			}
			if (closedOnFocusChange && FindOwnerCommandBar() != null)
			{
				FindOwnerCommandBar().CommandManager.SetActiveBar(null);
			}
		}

		private int GetKeyDataWithModifiers(IntPtr keyCode)
		{
			int keyData = (int)keyCode;
			if ((Control.ModifierKeys & Keys.Alt) != Keys.Alt)
			{
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					keyData += (int)Keys.Shift;
				}
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
				{
					keyData += (int)Keys.Control;
				}
			}
			return keyData;
		}

		private void SelectByMnemonic(char character)
		{
			character = char.ToLower(character);
			if (ChildMenu != null)
			{
				return;
			}
			for (int i = 0; i < _layoutItems.Count; i++)
			{
				MxCommand uiCommand = ((CommandLayoutState)_layoutItems.FindItemByIndex(i)).GetCommand();
				if (!uiCommand.HasMnemonic() || !uiCommand.IsEnabled || !uiCommand.IsEffectivelyVisible() || !base.Enabled || uiCommand.CommandType == CommandType.Separator)
				{
					continue;
				}
				char c = char.ToLower(uiCommand.Mnemonic);
				if (character == c)
				{
					SelectItem(_layoutItems.FindCommandItem(uiCommand));
					if (uiCommand.IsParent)
					{
						OpenChildMenu(GetSelectedItem(), true);
						break;
					}
					StopSubmenuTimer();
					uiCommand.PerformClick();
					break;
				}
			}
		}
	}
}
