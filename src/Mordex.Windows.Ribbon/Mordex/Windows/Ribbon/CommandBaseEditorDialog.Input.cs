using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		private CommandBaseRenderer GetLastSelectableItem()
		{
			for (int index = _layoutItems.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer commandBaseRenderer = (CommandBaseRenderer)_layoutItems[index];
				if (commandBaseRenderer.CommandType != CommandType.Separator)
				{
					return commandBaseRenderer;
				}
			}
			return null;
		}

		public virtual void SelectPrevious()
		{
			if (_layoutItems == null)
			{
				return;
			}
			int startIndex = -1;
			if (_focusedItem != null)
			{
				startIndex = _layoutItems.IndexOf(_focusedItem) - 1;
			}
			int count = _layoutItems.Count;
			if (count <= 0)
			{
				return;
			}
			if (startIndex < 0)
			{
				startIndex = _layoutItems.Count - 1;
			}
			for (int index = startIndex; index >= 0; index--)
			{
				CommandBaseRenderer commandBaseRenderer = (CommandBaseRenderer)_layoutItems[index];
				if (commandBaseRenderer.CommandType == CommandType.Separator)
				{
					if (startIndex != 0)
					{
						continue;
					}
					commandBaseRenderer = GetLastSelectableItem();
					if (commandBaseRenderer == null)
					{
						break;
					}
				}
				else if (commandBaseRenderer.CommandType == CommandType.Gallery && !((GalleryMenuRenderer)commandBaseRenderer).TrySelectLastItem())
				{
					continue;
				}
				FocusItem = commandBaseRenderer;
				if (commandBaseRenderer._bounds.Bottom > _contentBounds.Bottom)
				{
					SetScrollOffset(GetScrollOffset() + (commandBaseRenderer._bounds.Bottom - _contentBounds.Bottom));
				}
				else if (commandBaseRenderer._bounds.Top < _contentBounds.Top)
				{
					SetScrollOffset(GetScrollOffset() - (_contentBounds.Top - commandBaseRenderer._bounds.Top));
				}
				break;
			}
		}

		public virtual void SelectNext()
		{
			if (_layoutItems == null)
			{
				return;
			}
			int startIndex = 0;
			if (_focusedItem != null)
			{
				startIndex = _layoutItems.IndexOf(_focusedItem) + 1;
			}
			int endIndex = _layoutItems.Count;
			if (endIndex <= 0)
			{
				return;
			}
			CommandBaseRenderer commandBaseRenderer = (CommandBaseRenderer)_layoutItems[endIndex - 1];
			if (commandBaseRenderer.CommandType == CommandType.Separator)
			{
				endIndex--;
			}
			if (startIndex > endIndex - 1)
			{
				FocusItem = null;
				_selectFirstItem = true;
				SetScrollOffset(0);
				MarkLayoutDirty();
				return;
			}
			for (int i = startIndex; i < endIndex; i++)
			{
				CommandBaseRenderer candidate = (CommandBaseRenderer)_layoutItems[i];
				if (candidate.CommandType != CommandType.Separator)
				{
					if (candidate.CommandType != CommandType.Gallery || ((GalleryMenuRenderer)candidate).TrySelectFirstItem())
					{
						FocusItem = candidate;
						if (candidate._bounds.Bottom > _contentBounds.Bottom)
						{
							SetScrollOffset(GetScrollOffset() + (candidate._bounds.Bottom - _contentBounds.Bottom));
						}
						break;
					}
				}
				else if (startIndex == endIndex - 1)
				{
					FocusItem = null;
					_selectFirstItem = true;
					SetScrollOffset(0);
					MarkLayoutDirty();
				}
			}
		}

		public virtual void HandleKeyUp()
		{
			if (_focusedItem is GalleryMenuRenderer)
			{
				((GalleryMenuRenderer)_focusedItem).MoveSelectionUp();
			}
			else
			{
				SelectPrevious();
			}
		}

		public virtual void HandleKeyDown()
		{
			if (_focusedItem is GalleryMenuRenderer)
			{
				((GalleryMenuRenderer)_focusedItem).MoveSelectionDown();
			}
			else
			{
				SelectNext();
			}
		}

		public virtual void HandleKeyLeft()
		{
			if (_focusedItem != null && _focusedItem is GalleryMenuRenderer)
			{
				((GalleryMenuRenderer)_focusedItem).MoveSelectionLeft();
			}
			else if (_owner is CommandBaseEditorDialog)
			{
				TryCloseOwnerChildMenu(CommandCloseReason.Keyboard);
			}
		}

		public virtual bool HandleKeyRight(NativeMethods.NativeMessage msg)
		{
			if (_focusedItem != null && _childMenu == null)
			{
				if (_focusedItem is GalleryMenuRenderer)
				{
					((GalleryMenuRenderer)_focusedItem).MoveSelectionRight();
					return true;
				}
				if (_focusedItem._isEnabled && _focusedItem.HasChildren)
				{
					NativeMethods.NativeMessage n = default(NativeMethods.NativeMessage);
					NativeMethods.GetMessage(ref n, IntPtr.Zero, 0u, 0u);
					ShowChildMenu(_focusedItem, selectItem: true);
					return true;
				}
			}
			return false;
		}

		private void ActivateItemByMnemonic(char character)
		{
			character = char.ToLower(character);
			if (GetChildMenu() != null || _layoutItems == null)
			{
				return;
			}
			for (int i = 0; i < _layoutItems.Count; i++)
			{
				CommandBase commandBase = ((CommandBaseRenderer)_layoutItems[i]).GetCommand();
				if (commandBase != null && commandBase.HasMnemonic && commandBase.IsEnabled && commandBase.Visible && commandBase is ButtonCommand)
				{
					char c = char.ToLower(((ButtonCommand)commandBase).Mnemonic);
					if (character == c)
					{
						FocusItem = (CommandBaseRenderer)_layoutItems[i];
						if (FocusItem.HasChildren)
						{
							ShowChildMenu(FocusItem, selectItem: true);
							break;
						}
						StopHoverTimer();
						TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
						commandBase.InvokeOnClick();
						break;
					}
				}
				else if (_layoutItems[i] is ControlBoxPanelRenderer && ((ControlBoxPanelRenderer)_layoutItems[i]).TryActivateMnemonic(character))
				{
					break;
				}
			}
		}

		protected virtual void OnWMMouseDown(NativeMethods.NativeMessage msg)
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			NativeMethods.NativePoint m = default(NativeMethods.NativePoint);
			Point position = Cursor.Position;
			m._x = position.X;
			m._y = position.Y;
			if (!base.Bounds.Contains(m._x, m._y))
			{
				if (!IsScreenPointInMenuChain(m))
				{
					if (!ContainsWindowHandle(msg._windowHandle, base.Controls))
					{
						bool hasRibbonControls = GetRibbon() != null && base.Controls.Count > 0;
						TryCloseOwnerMenuChain(CommandCloseReason.AppClicked);
						if (hasRibbonControls && GetRibbon() != null)
						{
							GetRibbon().Update();
						}
					}
				}
				else
				{
					FindAncestorMenuAt(m)?.TryCloseChildMenu(CommandCloseReason.AppClicked);
				}
			}
			else
			{
				if (!(msg._windowHandle == base.Handle))
				{
					return;
				}
				Point pt = PointToClient(position);
				CommandBaseRenderer commandBaseRenderer = null;
				if (base.ClientRectangle.Contains(pt))
				{
					commandBaseRenderer = FindRendererAt(pt.X, pt.Y);
					if (commandBaseRenderer != null && (!commandBaseRenderer._isEnabled || commandBaseRenderer.CommandType == CommandType.Separator))
					{
						commandBaseRenderer = null;
					}
				}
				if (commandBaseRenderer != FocusItem)
				{
					FocusItem = commandBaseRenderer;
				}
			}
		}

		protected virtual void OnWMMouseUp(NativeMethods.NativeMessage msg)
		{
			NativeMethods.NativePoint m = default(NativeMethods.NativePoint);
			Point position = Cursor.Position;
			m._x = position.X;
			m._y = position.Y;
			if (!IsScreenPointInMenuChain(m))
			{
				if (ContainsWindowHandle(msg._windowHandle, base.Controls))
				{
					Control control = null;
					control = Control.FromHandle(msg._windowHandle);
					if (control != null)
					{
						CommandBase parentCommand = GetParentCommand(control, ref _exitMessageLoop);
						if (parentCommand != null)
						{
							return;
						}
						Form form = control.FindForm();
						if (form != null && form.Owner == this)
						{
							return;
						}
					}
				}
				if (_ignoreNextMouseUp || _isResizing)
				{
					_ignoreNextMouseUp = false;
					return;
				}
				if (ContainsWindowHandle(msg._windowHandle, base.Controls))
				{
					Control releasedControl = null;
					releasedControl = Control.FromHandle(msg._windowHandle);
					if (releasedControl != null)
					{
						CommandBase releasedControlOwner = GetParentCommand(releasedControl, ref _exitMessageLoop);
						if (releasedControlOwner != null)
						{
							return;
						}
						Form releasedControlForm = releasedControl.FindForm();
						if (releasedControlForm != null && releasedControlForm.Owner == this)
						{
							return;
						}
					}
				}
				if (TryCloseOwnerChildMenu(CommandCloseReason.AppClicked))
				{
					_exitMessageLoop = true;
				}
			}
			else
			{
				if (!(msg._windowHandle != base.Handle))
				{
					return;
				}
				if (_ignoreNextMouseUp)
				{
					_ignoreNextMouseUp = false;
					return;
				}
				Control outsideControl = null;
				outsideControl = Control.FromHandle(msg._windowHandle);
				if (outsideControl != null)
				{
					ContainerControlCommand outsideControlOwner = GetParentCommand(outsideControl, ref _exitMessageLoop);
					if (outsideControlOwner != null || IsParentMenuHandle(outsideControl.Handle))
					{
						return;
					}
					Form outsideControlForm = outsideControl.FindForm();
					if (outsideControlForm != null && outsideControlForm.Owner == this)
					{
						return;
					}
					while (outsideControl != null)
					{
						if (base.Controls.Contains(outsideControl))
						{
							return;
						}
						outsideControl = outsideControl.Parent;
					}
					_exitMessageLoop = true;
				}
				else
				{
					_exitMessageLoop = true;
				}
			}
		}

		private void RunMessageLoop()
		{
			NativeMethods.NativeMessage msg = default(NativeMethods.NativeMessage);
			bool enterHandled = false;
			NativeMethods.NativePoint m = default(NativeMethods.NativePoint);
			bool redirectToPopup = false;
			while (!_exitMessageLoop)
			{
				if (!NativeMethods.WaitMessage())
				{
					// Stop instead of spinning when the wait fails.
					break;
				}
				while (!_exitMessageLoop && NativeMethods.PeekMessage(ref msg, IntPtr.Zero, 0u, 0u, 0u))
				{
					bool swallowMessage = false;
					_ = _contentBounds.Width;
					_ = _contentBounds.Height;
					switch (msg._message)
					{
					case 512:
						if (!base.IsDisposed && msg._windowHandle != base.Handle)
						{
							Point position = Cursor.Position;
							m._x = position.X;
							m._y = position.Y;
							if (_isMouseInside)
							{
								OnMouseLeave(EventArgs.Empty);
							}
							if (!IsScreenPointInMenuChain(m))
							{
								if (_hoveredControlHandle != IntPtr.Zero)
								{
									NativeMethods.SendMessage(_hoveredControlHandle, NativeMethods.WmMouseLeave, IntPtr.Zero, IntPtr.Zero);
								}
								swallowMessage = true;
							}
						}
						else
						{
							_isMouseInside = true;
						}
						break;
					case 162:
					case 163:
					case 165:
					case 166:
					case 168:
					case 169:
					case 172:
					case 514:
					case 515:
					case 517:
					case 518:
					case 520:
					case 521:
					case 524:
						if (base.Capture && !_isResizing)
						{
							RibbonDrawingUtils.SetCapture(this, false);
						}
						else if (_pressedScrollBar == null || !(_pressedScrollBar.Handle == msg._windowHandle))
						{
							OnWMMouseUp(msg);
						}
						break;
					case 161:
					case 164:
					case 167:
					case 171:
					case 513:
					case 516:
					case 519:
					case 523:
						_ignoreNextMouseUp = false;
						OnWMMouseDown(msg);
						break;
					case 522:
						if (_hasVerticalScrollBar)
						{
							Point position = Cursor.Position;
							m._x = position.X;
							m._y = position.Y;
							if (base.Bounds.Contains(m._x, m._y))
							{
								short wheelDelta = unchecked((short)((msg._wParam.ToInt64() >> 16) & 0xFFFF));
								ScrollByWheel(wheelDelta / WheelDelta);
								swallowMessage = true;
							}
						}
						break;
					case 260:
						if (NativeMethods.ToInt32Unchecked(msg._wParam) == 18)
						{
							_exitMessageLoop = true;
							swallowMessage = true;
							break;
						}
						if (ContainsWindowHandle(msg._windowHandle, base.Controls))
						{
							break;
						}
						msg._message = 256;
						goto case 256;
					case 258:
						swallowMessage = false;
						redirectToPopup = false;
						if (!ContainsWindowHandle(msg._windowHandle, base.Controls))
						{
							redirectToPopup = true;
						}
						break;
					case 256:
						redirectToPopup = false;
						if (!ContainsWindowHandle(msg._windowHandle, base.Controls))
						{
							redirectToPopup = true;
							swallowMessage = true;
							switch (NativeMethods.ToInt32Unchecked(msg._wParam))
							{
							case 38:
								HandleKeyUp();
								break;
							case 9:
								HandleKeyDown();
								break;
							case 40:
								HandleKeyDown();
								break;
							case 37:
								HandleKeyLeft();
								break;
							case 39:
								if (HandleKeyRight(msg))
								{
									swallowMessage = false;
								}
								break;
							case 13:
								if (FocusItem != null && FocusItem._isEnabled)
								{
									NativeMethods.NativeMessage n = default(NativeMethods.NativeMessage);
									NativeMethods.GetMessage(ref n, IntPtr.Zero, 0u, 0u);
									swallowMessage = false;
									enterHandled = true;
									if (FocusItem is GalleryMenuRenderer)
									{
										((GalleryMenuRenderer)FocusItem).InvokeHotItem();
									}
									else
									{
										ActivateItem(FocusItem);
									}
								}
								break;
							case 27:
							{
								if (_isResizing)
								{
									EndResize(true);
									swallowMessage = true;
									break;
								}
								MxRibbon ribbon = GetRibbon();
								if (TryCloseOwnerChildMenu(CommandCloseReason.Keyboard))
								{
									if (ribbon != null && ribbon.IsKeyTipMode() && ribbon.GetKeyTipOwner() != null)
									{
										ribbon.StepBackKeyTipLevel();
									}
									redirectToPopup = false;
								}
								break;
							}
							default:
								swallowMessage = false;
								break;
							}
							if (_exitMessageLoop)
							{
								swallowMessage = false;
							}
						}
						else
						{
							if (NativeMethods.ToInt32Unchecked(msg._wParam) != 13)
							{
								break;
							}
							NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, 32512u));
							if (FocusItem != null)
							{
								StopHoverTimer();
								TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
								if (FocusItem != null && FocusItem.GetCommand().Enabled)
								{
									FocusItem.GetCommand().InvokeOnClick();
								}
							}
							Control control = null;
							control = Control.FromHandle(msg._windowHandle);
							if (control != null)
							{
								CommandBase parentCommand = GetParentCommand(control, ref _exitMessageLoop);
								if (parentCommand != null)
								{
									break;
								}
								Form form = control.FindForm();
								if (form != null && form.Owner == this)
								{
									break;
								}
							}
							_exitMessageLoop = true;
						}
						break;
					case 32:
						if (msg._windowHandle == base.Handle)
						{
							Point position = Cursor.Position;
							if (!base.Bounds.Contains(position))
							{
								swallowMessage = true;
							}
						}
						break;
					case 257:
					case 261:
						if (!ContainsWindowHandle(msg._windowHandle, base.Controls))
						{
							swallowMessage = true;
						}
						break;
					}
					if (swallowMessage)
					{
						NativeMethods.NativeMessage discardedMessage = default(NativeMethods.NativeMessage);
						NativeMethods.GetMessage(ref discardedMessage, IntPtr.Zero, (uint)msg._message, (uint)msg._message);
					}
					else if (!enterHandled)
					{
						if (NativeMethods.GetMessage(ref msg, IntPtr.Zero, 0u, 0u))
						{
							if ((msg._message == 256 || msg._message == 258) && redirectToPopup && !base.IsDisposed)
							{
								msg._windowHandle = base.Handle;
							}
							NativeMethods.TranslateMessage(ref msg);
							try
							{
								NativeMethods.DispatchMessage(ref msg);
							}
							catch (SEHException)
							{
								// A window procedure that fails natively must not break the popup message loop.
							}
						}
					}
					else
					{
						enterHandled = false;
					}
				}
			}
		}

		internal virtual IList<RibbonKeyTipEntry> GetKeyTips(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (GetChildMenu() != null)
			{
				return GetChildMenu().GetKeyTips(graphics, textRenderer);
			}
			IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
			if (GetRibbon() != null)
			{
				GetRibbon().SetKeyTipOwner(GetParentMenuCommand());
				foreach (CommandBaseRenderer itemRenderer in _layoutItems)
				{
					if (itemRenderer.GetCommand() is Gallery || !(itemRenderer.GetCommand() is IKeyTipProvider))
					{
						continue;
					}
					IKeyTipProvider keyTipProvider = (IKeyTipProvider)itemRenderer.GetCommand();
					if (keyTipProvider.HasKeyTip)
					{
						Point keyTipLocation = itemRenderer.GetKeyTipLocation(graphics, textRenderer, Font, keyTipProvider.KeyTip);
						if (keyTipLocation.Y < base.ClientRectangle.Bottom && keyTipLocation.Y >= base.ClientRectangle.Top)
						{
							SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(Font, _owner.UseCompatibleTextRendering, keyTipProvider.KeyTip, _popupColorScheme);
							superTipSettingsWindowMessageHandler.SetCompactMode(true);
							superTipSettingsWindowMessageHandler._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
							superTipSettingsWindowMessageHandler.SetSticky(true);
							superTipSettingsWindowMessageHandler.ShowAt(_owner.ParentControl, PointToScreen(keyTipLocation));
							RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipProvider.KeyTip, itemRenderer.GetCommand(), itemRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
							list.Add(item);
						}
					}
				}
			}
			return list;
		}

		private void ScrollByWheel(int wheelNotches)
		{
			Point point = PointToClient(Cursor.Position);
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(point.X, point.Y);
			if (commandBaseRenderer is GalleryMenuRenderer)
			{
				((GalleryMenuRenderer)commandBaseRenderer).ScrollByWheel(wheelNotches);
			}
			else if (_scrollBar != null)
			{
				if (wheelNotches < 0)
				{
					RibbonControl commandBaseEditorDialogRibbonControl = _scrollBar;
					commandBaseEditorDialogRibbonControl.SetScrollValue(commandBaseEditorDialogRibbonControl.GetScrollValue() + _scrollBar.GetSmallChange());
				}
				else if (wheelNotches > 0)
				{
					RibbonControl upScrollBar = _scrollBar;
					upScrollBar.SetScrollValue(upScrollBar.GetScrollValue() - _scrollBar.GetSmallChange());
				}
				SetScrollOffset(_scrollBar.GetScrollValue());
			}
		}

		private void OnControlClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.Tag != null && eventArgs.Command.Tag is Form)
			{
				Form form = eventArgs.Command.Tag as Form;
				form.Activate();
			}
			_exitMessageLoop = true;
		}

		private void OnScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			SetScrollOffset(eventArgs.NewValue);
		}

		private void OnStateMouseUp(object sender, MouseEventArgs eventArgs)
		{
			SetPressedScrollBar(null);
		}

		private void OnStateMouseDown(object sender, MouseEventArgs eventArgs)
		{
			SetPressedScrollBar(_scrollBar);
		}

		protected void OnControlMouseEnter(object sender, EventArgs eventArgs)
		{
			if (sender is Control control)
			{
				_hoveredControlHandle = control.Handle;
			}
		}

		protected void OnControlMouseLeave(object sender, EventArgs eventArgs)
		{
			_hoveredControlHandle = IntPtr.Zero;
		}

		protected override void WndProc(ref Message message)
		{
			if (_isDesignMode)
			{
				if (message.Msg == WmActivateApp)
				{
					EndInPlaceEdit(false);
					if (base.Owner == null)
					{
						ClearOwnerChildMenu();
					}
				}
				else if (message.Msg == WmMouseActivate)
				{
					message.Result = (IntPtr)MaNoActivate;
					return;
				}
				base.WndProc(ref message);
				return;
			}
			switch (message.Msg)
			{
			case WmChar:
				if (GetRibbon() != null && GetRibbon().IsKeyTipMode())
				{
					GetRibbon().HandleKeyTipChar((char)NativeMethods.ToInt32Unchecked(message.WParam));
				}
				else
				{
					ActivateItemByMnemonic((char)NativeMethods.ToInt32Unchecked(message.WParam));
				}
				break;
			case WmActivateApp:
				if (!_activateOnShow)
				{
					TryCloseOwnerChildMenu(CommandCloseReason.AppFocusChange);
				}
				break;
			case WmMouseActivate:
				message.Result = (IntPtr)MaNoActivate;
				break;
			default:
				base.WndProc(ref message);
				break;
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.X == _lastMousePoint.X && eventArgs.Y == _lastMousePoint.Y)
			{
				return;
			}
			_lastMousePoint = new Point(eventArgs.X, eventArgs.Y);
			_isMouseInside = true;
			if (_sourceItem != null)
			{
				_owner.SelectedItem = _sourceItem;
			}
			if (!_resizeStartPoint.IsEmpty && (_isResizing || IsOutsideDragRectangle(eventArgs.X, eventArgs.Y, _resizeStartPoint)))
			{
				FocusItem = null;
				ResizeByDrag(new Point(eventArgs.X, eventArgs.Y));
			}
			else if (!_dragStartPoint.IsEmpty && IsOutsideDragRectangle(eventArgs.X, eventArgs.Y, _dragStartPoint))
			{
				StartDesignDragDrop();
			}
			else
			{
				if (_pressedScrollBar != null && _pressedScrollBar.Capture)
				{
					return;
				}
				if (_resizeBarBounds.Contains(eventArgs.X, eventArgs.Y))
				{
					FocusItem = null;
					return;
				}
				CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
				if (_isDesignMode)
				{
					if (commandBaseRenderer is RibbonContextMenuRenderer)
					{
						commandBaseRenderer.OnMouseMove(eventArgs);
					}
					SetDesignHoverItem(commandBaseRenderer);
					if (commandBaseRenderer is GalleryMenuRenderer)
					{
						GalleryMenuRenderer galleryMenu = commandBaseRenderer as GalleryMenuRenderer;
						galleryMenu.SetDesignHighlightItem(galleryMenu.GetItemAt(eventArgs.X, eventArgs.Y));
					}
					return;
				}
				commandBaseRenderer?.OnMouseMove(eventArgs);
				if (commandBaseRenderer != null && (!commandBaseRenderer._isEnabled || (commandBaseRenderer.GetCommand() != null && !commandBaseRenderer.GetCommand().IsEnabled)))
				{
					commandBaseRenderer = null;
				}
				if (commandBaseRenderer == FocusItem)
				{
					return;
				}
				_showToolTipOnFocus = true;
				FocusItem = commandBaseRenderer;
				_showToolTipOnFocus = false;
				if (FocusItem != null && commandBaseRenderer != null && commandBaseRenderer.GetCommand() != null && commandBaseRenderer.HasChildren && (GetChildMenu() == null || GetChildMenu()._sourceItem != commandBaseRenderer))
				{
					if (GetRibbon() != null)
					{
						GetRibbon().SetKeyTipMode(false);
					}
					StartHoverTimer();
				}
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			_isMouseInside = false;
			FocusItem = null;
			SetDesignHoverItem(null);
			if (GetRibbon() != null)
			{
				GetRibbon().SetHotRenderer(null);
				GetRibbon().ClearSuperTipCommand();
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (GetRibbon() != null)
			{
				GetRibbon().SetKeyTipMode(false);
			}
			if (_resizeGripBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				_resizeStartSize = base.Size;
				_resizeStartPoint = new Point(eventArgs.X, eventArgs.Y);
			}
			else if (_isDesignMode)
			{
				CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
				if (commandBaseRenderer == null)
				{
					return;
				}
				if (commandBaseRenderer is RibbonContextMenuRenderer)
				{
					commandBaseRenderer.OnMouseDown(eventArgs);
					return;
				}
				if (commandBaseRenderer.CommandType == CommandType.Gallery)
				{
					CommandBaseRenderer galleryItemRenderer = ((GalleryMenuRenderer)commandBaseRenderer).GetItemAt(eventArgs.X, eventArgs.Y);
					if (galleryItemRenderer != null)
					{
						commandBaseRenderer = galleryItemRenderer;
					}
				}
				if (commandBaseRenderer.GetCommand() != null && commandBaseRenderer.GetCommand().GetParentContainer() != null)
				{
					_dragStartPoint = new Point(eventArgs.X, eventArgs.Y);
					SelectCommandInDesigner(commandBaseRenderer.GetCommand());
				}
			}
			else
			{
				CommandBaseRenderer pressedRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
				if (pressedRenderer is ControlBoxPanelRenderer)
				{
					pressedRenderer.OnMouseDown(eventArgs);
				}
				else if (pressedRenderer != null && pressedRenderer._commandBaseRendererCommand != null && pressedRenderer._commandBaseRendererCommand.Enabled)
				{
					pressedRenderer.OnMouseDown(eventArgs);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			_dragStartPoint = Point.Empty;
			if (_isResizing)
			{
				if (eventArgs.Button == MouseButtons.Left)
				{
					EndResize(false);
				}
				return;
			}
			if (_isDesignMode)
			{
				CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
				if (commandBaseRenderer != null)
				{
					if (eventArgs.Button == MouseButtons.Left)
					{
						ForwardMouseUpToItem(commandBaseRenderer, eventArgs);
					}
					else if (eventArgs.Button == MouseButtons.Right)
					{
						ShowDesignerContextMenu(commandBaseRenderer);
					}
				}
				return;
			}
			CommandBaseRenderer releasedRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			if (releasedRenderer is ControlBoxPanelRenderer)
			{
				releasedRenderer.OnMouseUp(eventArgs);
			}
			else
			{
				if (releasedRenderer == null || releasedRenderer._commandBaseRendererCommand == null || !releasedRenderer._commandBaseRendererCommand.Enabled)
				{
					return;
				}
				if (releasedRenderer.CommandType == CommandType.Gallery || eventArgs.Button != MouseButtons.Left)
				{
					if (GetRibbon() != null && eventArgs.Button == MouseButtons.Right && GetRibbon().ShowQuickCustomizeMenu)
					{
						((ICommandRenderHost)GetRibbon()).ShowQuickCustomizeMenu(releasedRenderer.GetCommand());
						ClearOwnerChildMenu();
					}
					releasedRenderer.OnMouseUp(eventArgs);
				}
				else if (releasedRenderer.GetCommand() is IMouseEventInvoker)
				{
					ActivateItem(releasedRenderer);
				}
			}
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			_isMouseInside = true;
			if (_owner is CommandBaseEditorDialog)
			{
				_owner.SelectedItem = _sourceItem;
			}
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			if (drgevent.Data.GetDataPresent(typeof(CommandBaseState)))
			{
				Point point = PointToClient(new Point(drgevent.X, drgevent.Y));
				CommandBaseRenderer commandBaseRenderer = FindRendererAt(point.X, point.Y);
				if (!(commandBaseRenderer is RibbonContextMenuRenderer))
				{
					if (Control.ModifierKeys == Keys.Control)
					{
						drgevent.Effect = DragDropEffects.Copy;
					}
					else
					{
						drgevent.Effect = DragDropEffects.Move;
					}
					FocusItem = commandBaseRenderer;
				}
				else
				{
					drgevent.Effect = DragDropEffects.None;
					FocusItem = null;
				}
			}
			base.OnDragOver(drgevent);
		}

		protected override void OnDragLeave(EventArgs eventArgs)
		{
			FocusItem = null;
			base.OnDragLeave(eventArgs);
		}

		protected override void OnDragDrop(DragEventArgs eventArgs)
		{
			FocusItem = null;
			if (eventArgs.Data.GetDataPresent(typeof(CommandBaseState)))
			{
				CommandBaseState commandBaseState = (CommandBaseState)eventArgs.Data.GetData(typeof(CommandBaseState));
				Point point = PointToClient(new Point(eventArgs.X, eventArgs.Y));
				CommandBaseRenderer commandBaseRenderer = FindRendererAt(point.X, point.Y);
				if (commandBaseRenderer != null && commandBaseRenderer != commandBaseState.GetRenderer())
				{
					int dropIndex = _layoutItems.IndexOf(commandBaseRenderer);
					using (DesignEditScope edit = CommandCollection.BeginDesignEdit(_commands, "Move Command"))
					{
						_commands.MoveCommandTo(dropIndex, commandBaseState.GetRenderer().GetCommand());
						edit.End();
					}
					MarkMenuBoundsDirty();
				}
			}
			base.OnDragDrop(eventArgs);
		}

		protected override void OnControlAdded(ControlEventArgs eventArgs)
		{
			base.OnControlAdded(eventArgs);
			eventArgs.Control.MouseEnter += OnControlMouseEnter;
			eventArgs.Control.MouseLeave += OnControlMouseLeave;
		}

		protected override void OnControlRemoved(ControlEventArgs eventArgs)
		{
			base.OnControlRemoved(eventArgs);
			eventArgs.Control.MouseEnter -= OnControlMouseEnter;
			eventArgs.Control.MouseLeave -= OnControlMouseLeave;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_isDesignMode)
			{
				switch (keyData)
				{
				case Keys.Return:
					if (_ownerItemRenderer != null)
					{
						EndInPlaceEdit(false);
					}
					else if (GetDesignSelectedItem() != null)
					{
						SetInPlaceEditCommand(GetDesignSelectedItem());
					}
					return true;
				case Keys.Escape:
					EndInPlaceEdit(true);
					return true;
				case Keys.F2:
					if (GetDesignSelectedItem() != null)
					{
						SetInPlaceEditCommand(GetDesignSelectedItem());
					}
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		private void OnControlCommandClick(object sender, CommandEventArgs eventArgs)
		{
			string key;
			if (eventArgs.Command.Tag is CommandBase commandBase && (key = eventArgs.Command.Key) != null)
			{
				if (!(key == "Delete"))
				{
					if (!(key == "MoveUp"))
					{
						if (!(key == "MoveDown"))
						{
							if (key == "AddQA" && GetRibbon() != null)
							{
								CommandCollection quickAccessCommands = GetRibbon().QuickAccessCommands;
								using (DesignEditScope edit = CommandCollection.BeginDesignEdit(quickAccessCommands, "Add to Quick Access"))
								{
									quickAccessCommands.Add(commandBase);
									edit.End();
								}
							}
						}
						else
						{
							MoveCommandInOwner(commandBase, commandBase.GetIndexInOwner() + 1);
						}
					}
					else
					{
						MoveCommandInOwner(commandBase, commandBase.GetIndexInOwner() - 1);
					}
				}
				else
				{
					commandBase.Dispose();
				}
			}
			MxRibbonContextMenu ribbonContextMenu = sender as MxRibbonContextMenu;
			ribbonContextMenu.CommandClick -= OnControlCommandClick;
			ribbonContextMenu.Dispose();
		}

		private static void MoveCommandInOwner(CommandBase command, int index)
		{
			CommandCollection owner = command.CommandsOwner;
			if (owner == null)
			{
				return;
			}
			using (DesignEditScope edit = CommandCollection.BeginDesignEdit(owner, "Move Command"))
			{
				owner.MoveCommandTo(index, command);
				edit.End();
			}
		}

		private void ForwardMouseUpToItem(CommandBaseRenderer commandBaseRenderer, MouseEventArgs eventArgs)
		{
			if (commandBaseRenderer is RibbonContextMenuRenderer)
			{
				commandBaseRenderer.OnMouseUp(eventArgs);
			}
			else if (commandBaseRenderer is GalleryMenuRenderer)
			{
				GalleryMenuRenderer galleryMenu = (GalleryMenuRenderer)commandBaseRenderer;
				CommandBaseRenderer galleryItem = galleryMenu.GetItemAt(eventArgs.X, eventArgs.Y);
				if (galleryItem is GalleryState)
				{
					galleryItem.OnMouseUp(eventArgs);
				}
			}
			else if (commandBaseRenderer is SmallMenuButtonRenderer && _allowInPlaceEdit)
			{
				SetInPlaceEditCommand(commandBaseRenderer);
			}
		}
	}
}
