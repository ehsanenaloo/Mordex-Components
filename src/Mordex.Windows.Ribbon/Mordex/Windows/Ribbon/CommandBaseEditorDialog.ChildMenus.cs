using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		internal void ShowWithChildMenus()
		{
			Show();
			BringToFront();
			if (GetChildMenu() != null)
			{
				GetChildMenu().Show();
			}
		}

		internal void HideWithChildMenus()
		{
			Hide();
			if (GetChildMenu() != null)
			{
				GetChildMenu().Hide();
			}
		}

		internal void ShowAt(Point screenLocation)
		{
			try
			{
				AccessibilityNotifyClients(AccessibleEvents.SystemMenuPopupStart, 0);
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
			{
				// Accessibility clients are optional; a failed notification must not block the popup.
			}
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			if (_parentCommand is MdiListCommand)
			{
				RebuildMdiList();
			}
			if (_sourceItem != null)
			{
				_anchorScreenBounds = new Rectangle(screenLocation, _sourceItem._bounds.Size);
			}
			else
			{
				_anchorScreenBounds = new Rectangle(screenLocation, _anchorSize);
			}
			_adjustedAnchorBounds = _anchorScreenBounds;
			using (Graphics graphics = CreateGraphics())
			{
				RebuildMenuAndBounds(graphics);
			}
			if (_layoutItems != null && _layoutItems.Count == 0 && base.Controls.Count == 0)
			{
				ClearOwnerChildMenu();
				return;
			}
			NativeMethods.ShowWindow(base.Handle, 4);
			if (Control.MouseButtons != MouseButtons.None)
			{
				_ignoreNextMouseUp = true;
			}
			if (_activateOnShow)
			{
				NativeMethods.SetForegroundWindow(base.Handle);
				_activateOnShow = false;
			}
			if (_shadowWindow == null && !_isDesignMode && ThemeAllowsShadow && RibbonDrawingUtils.IsHighColorDisplay())
			{
				_shadowWindow = new RibbonPopupShadowWindow(_popupColorScheme._popupShadowColor);
				Point location = base.Location;
				Size size = new Size(base.Bounds.Width + 4, base.Bounds.Height + 4);
				Point contentLocation = PointToClient(_adjustedAnchorBounds.Location);
				_shadowWindow._excludedBounds = new Rectangle(contentLocation, _adjustedAnchorBounds.Size);
				_shadowWindow.ShowShadow(new Rectangle(location, size));
			}
			OnDropDown(new CommandEventArgs(GetParentMenuCommand()));
			if (_sourceItem != null && _sourceItem.GetCommand() is DropDownCommand)
			{
				((DropDownCommand)_sourceItem.GetCommand()).NotifyDropDownOpened();
			}
			if (!_isDesignMode)
			{
				NativeMethods.ReleaseCapture();
				RunMessageLoop();
				if (!base.IsDisposed)
				{
					ClearOwnerChildMenu();
				}
				return;
			}
			base.Owner = _owner.ParentForm;
			if (base.Owner == null)
			{
				base.TopMost = true;
			}
			if (!base.IsDisposed && !base.Disposing)
			{
				Show();
			}
		}

		internal bool TryCloseChildMenu(CommandCloseReason commandCloseReason)
		{
			if (_childMenu != null)
			{
				if (_childMenu._sourceItem != null)
				{
					if (_childMenu._sourceItem.GetCommand() is DropDownCommand)
					{
						DropDownCommand dropDownCommand = _childMenu._sourceItem._commandBaseRendererCommand as DropDownCommand;
						if (!dropDownCommand.CanCloseDropDown(commandCloseReason))
						{
							return false;
						}
					}
				}
				else if (_owner is MxRibbonContextMenu)
				{
					if (((MxRibbonContextMenu)_owner).TryClose(commandCloseReason))
					{
						SetChildMenu(null);
						return true;
					}
					return false;
				}
				SetChildMenu(null);
				return true;
			}
			return true;
		}

		public void ClearOwnerChildMenu()
		{
			if (_owner is MxRibbon)
			{
				((MxRibbon)_owner).SetKeyTipMode(false);
				_owner.ChildMenu = null;
			}
			else if (_owner is CommandBaseEditorDialog)
			{
				((CommandBaseEditorDialog)_owner).ClearOwnerChildMenu();
			}
			else if (_owner is MxRibbonContextMenu)
			{
				_owner.ChildMenu = null;
			}
			else if (_owner is MxRibbonStatusBar)
			{
				((MxRibbonStatusBar)_owner).SetChildMenu(null);
			}
		}

		public bool TryCloseOwnerChildMenu(CommandCloseReason commandCloseReason)
		{
			if (_owner is CommandBaseEditorDialog)
			{
				return ((CommandBaseEditorDialog)_owner).TryCloseChildMenu(commandCloseReason);
			}
			if (_owner is MxRibbon)
			{
				return ((MxRibbon)_owner).TryCloseParentControl(commandCloseReason);
			}
			if (_owner is MxRibbonContextMenu)
			{
				return ((MxRibbonContextMenu)_owner).TryClose(commandCloseReason);
			}
			if (_owner is MxRibbonStatusBar)
			{
				return ((MxRibbonStatusBar)_owner).TryCloseChildMenu(commandCloseReason);
			}
			return true;
		}

		public bool TryCloseOwnerMenuChain(CommandCloseReason commandCloseReason)
		{
			if (_owner is CommandBaseEditorDialog)
			{
				return ((CommandBaseEditorDialog)_owner).TryCloseOwnerMenuChain(commandCloseReason);
			}
			if (_owner is MxRibbon)
			{
				return ((MxRibbon)_owner).TryCloseParentControl(commandCloseReason);
			}
			if (_owner is MxRibbonContextMenu)
			{
				return ((MxRibbonContextMenu)_owner).TryClose(commandCloseReason);
			}
			if (_owner is MxRibbonStatusBar)
			{
				return ((MxRibbonStatusBar)_owner).TryCloseChildMenu(commandCloseReason);
			}
			return true;
		}

		public void CloseChildMenuFor(DropDownCommand dropDownCommand)
		{
			if (GetChildMenu() != null)
			{
				if (GetChildMenu().GetParentMenuCommand() == dropDownCommand)
				{
					TryCloseChildMenu(CommandCloseReason.CloseCalled);
				}
				else
				{
					GetChildMenu().CloseChildMenuFor(dropDownCommand);
				}
			}
		}

		public virtual void OpenDropDown(DropDownCommand cmd)
		{
			if (cmd.GetParentContainer() is DropDownCommand && cmd.GetParentContainer() != GetParentMenuCommand())
			{
				OpenDropDown(cmd.GetParentContainer() as DropDownCommand);
				if (GetChildMenu() != null)
				{
					GetChildMenu().OpenDropDown(cmd);
				}
				return;
			}
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				if (item.GetCommand() == cmd && item.HasChildren)
				{
					ShowChildMenu(item, selectItem: true);
					break;
				}
			}
		}

		private bool IsScreenPointInMenuChain(NativeMethods.NativePoint screenPoint)
		{
			if (base.Bounds.Contains(screenPoint._x, screenPoint._y))
			{
				return true;
			}
			if (_owner is CommandBaseEditorDialog)
			{
				return ((CommandBaseEditorDialog)_owner).IsScreenPointInMenuChain(screenPoint);
			}
			return false;
		}

		private void ActivateItem(CommandBaseRenderer item)
		{
			StopHoverTimer();
			if (item.GetCommand() is ContainerControlCommand)
			{
				Control control = ((ContainerControlCommand)item.GetCommand()).Control;
				if (control != null)
				{
					RibbonDrawingUtils.TryFocusControl(control);
					RibbonDrawingUtils.SetCapture(control, true);
				}
				if (item.GetCommand().Enabled)
				{
					item.GetCommand().InvokeOnClick();
				}
			}
			else if (item.HasChildren)
			{
				if (item is SmallMenuButtonRenderer && item is SmallMenuButtonRenderer menuButton && menuButton._isMainAreaPressed)
				{
					TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
					if (item.GetCommand().Enabled)
					{
						item.GetCommand().InvokeOnClick();
					}
					return;
				}
				if (item.GetCommand().IsEnabled && item.GetCommand() is ButtonCommand)
				{
					ButtonCommand buttonCommand = (ButtonCommand)item.GetCommand();
					if (buttonCommand.CheckOnClick || buttonCommand.ActAsOptionButton)
					{
						buttonCommand.Checked = true;
					}
				}
				ShowChildMenu(item, selectItem: true);
			}
			else if (item.GetCommand().CanBeClicked)
			{
				TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
				if (item.GetCommand().Enabled)
				{
					item.GetCommand().InvokeOnClick();
				}
			}
		}

		private bool ContainsWindowHandle(IntPtr handle, Control.ControlCollection controlCollection)
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
				Control windowControl = Control.FromHandle(handle);
				if (windowControl == null)
				{
					continue;
				}
				Form form = windowControl.FindForm();
				if (form != null && form.Owner == this)
				{
					return true;
				}
				for (windowControl = RibbonDrawingUtils.GetParentControl(windowControl); windowControl != null; windowControl = RibbonDrawingUtils.GetParentControl(windowControl))
				{
					if (windowControl == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsParentMenuHandle(IntPtr windowHandle)
		{
			if (_owner is CommandBaseEditorDialog)
			{
				for (CommandBaseEditorDialog commandBaseEditorDialog = (CommandBaseEditorDialog)_owner; commandBaseEditorDialog != null; commandBaseEditorDialog = ((!(commandBaseEditorDialog._owner is CommandBaseEditorDialog)) ? null : ((CommandBaseEditorDialog)commandBaseEditorDialog._owner)))
				{
					if (commandBaseEditorDialog.Handle == windowHandle)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void StartHoverTimer()
		{
			if (_hoverTimer == null)
			{
				_hoverTimer = new Timer();
				_hoverTimer.Interval = 400;
				_hoverTimer.Tick += OnHoverTimerTick;
			}
			_hoverTimer.Start();
		}

		internal void StopHoverTimer()
		{
			if (_hoverTimer == null)
			{
				return;
			}
			Timer timer = _hoverTimer;
			_hoverTimer = null;
			timer.Stop();
			timer.Tick -= OnHoverTimerTick;
			timer.Dispose();
		}

		private void StartChildCloseTimer(int interval)
		{
			if (_childCloseTimer == null)
			{
				_childCloseTimer = new Timer();
				_childCloseTimer.Tick += OnTimerTick;
				_childCloseTimer.Interval = interval;
				_childCloseTimer.Start();
			}
		}

		private void StopChildCloseTimer()
		{
			if (_childCloseTimer == null)
			{
				return;
			}
			Timer timer = _childCloseTimer;
			_childCloseTimer = null;
			timer.Stop();
			timer.Tick -= OnTimerTick;
			timer.Dispose();
		}

		internal virtual void ShowChildMenu(CommandBaseRenderer item, bool selectItem)
		{
			if ((GetChildMenu() == null || GetChildMenu()._sourceItem.GetCommand() != item.GetCommand()) && TryCloseChildMenu(CommandCloseReason.CommandFocused) && item.GetCommand() is DropDownCommand dropDownCommand && dropDownCommand.CanOpenDropDown())
			{
				CommandBaseEditorDialog commandBaseEditorDialog = new CommandBaseEditorDialog(this, dropDownCommand.Commands, item, item.GetCommand() as DropDownCommand);
				commandBaseEditorDialog._commandSizeStyle = _commandSizeStyle;
				if (_popupMenuType != PopupMenuType.RibbonGroupCommands)
				{
					commandBaseEditorDialog._opensVertically = false;
					commandBaseEditorDialog._isFlipped = false;
				}
				commandBaseEditorDialog._selectFirstItem = selectItem;
				commandBaseEditorDialog.ShowAt(PointToScreen(item._bounds.Location));
			}
		}

		internal void InvalidateWithChildMenus()
		{
			Invalidate();
			if (GetChildMenu() != null)
			{
				GetChildMenu().InvalidateWithChildMenus();
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			StopChildCloseTimer();
			if (_childMenu != null)
			{
				Point pt = _childMenu.PointToClient(Cursor.Position);
				if (!_childMenu.ClientRectangle.Contains(pt))
				{
					TryCloseChildMenu(CommandCloseReason.CommandFocused);
				}
			}
		}

		private void OnHoverTimerTick(object sender, EventArgs eventArgs)
		{
			StopHoverTimer();
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			if (_focusedItem != null && _focusedItem.HasChildren)
			{
				try
				{
					ShowChildMenu(_focusedItem, selectItem: false);
					return;
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException || ex is Win32Exception)
				{
					// The popup can close while the child menu opens; ignore that race.
					return;
				}
			}
			TryCloseChildMenu(CommandCloseReason.CommandFocused);
		}

		private void OnChildMenuDropDown(object sender, CommandEventArgs eventArgs)
		{
			OnDropDown(eventArgs);
		}

		protected virtual void OnDropDown(CommandEventArgs eventArgs)
		{
			if (base.Events[s_dropDownEventKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}
	}
}
