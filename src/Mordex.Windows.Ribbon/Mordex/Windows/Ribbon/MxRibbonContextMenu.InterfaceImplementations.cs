using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonContextMenu
	{
		MxRibbon ICommandContainer.Ribbon => null;

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu => this;

		CommandType ICommandContainer.CommandType => CommandType.ContextMenu;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Small;

		bool ICommandContainer.IsEnabled => true;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Vertical;

		CommandCollection ICommandContainer.Commands => _commandCollection;

		AccessibleObject ICommandContainer.AccessibilityObject => null;

		AccessibleObject ICommandRenderHost.AccessibilityObject => GetAccessibleObject();

		Control ICommandRenderHost.ParentControl
		{
			get
			{
				CommandBaseEditorDialog childMenu = _childMenu;
				if (childMenu != null)
				{
					while (childMenu.GetChildMenu() != null)
					{
						childMenu = childMenu.GetChildMenu();
					}
				}
				return childMenu;
			}
		}

		int ICommandRenderHost.SmallItemHeight
		{
			get
			{
				int itemHeight = SmallImageSize.Height;
				Font font = ((ICommandRenderHost)this).Font;
				if (font.Height + 3 > itemHeight)
				{
					itemHeight = font.Height + 3;
				}
				return itemHeight + 6;
			}
		}

		Size ICommandRenderHost.SmallImageSize
		{
			get
			{
				if (SmallImageSize.IsEmpty)
				{
					if (_smallImageList != null)
					{
						return _smallImageList.ImageSize;
					}
					return new Size(16, 16);
				}
				return SmallImageSize;
			}
		}

		Size ICommandRenderHost.LargeImageSize
		{
			get
			{
				if (LargeImageSize.IsEmpty)
				{
					if (LargeImageList != null)
					{
						return LargeImageList.ImageSize;
					}
					return new Size(32, 32);
				}
				return LargeImageSize;
			}
		}

		Font ICommandRenderHost.Font
		{
			get
			{
				if (_control != null)
				{
					Form form = RibbonDrawingUtils.FindParentForm(_control);
					if (form != null)
					{
						return form.Font;
					}
					return _control.Font;
				}
				return Control.DefaultFont;
			}
		}

		RibbonColorScheme ICommandRenderHost.ColorScheme => _colorScheme;


		CommandBaseRenderer ICommandRenderHost.FocusItem
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		CommandBaseRenderer ICommandRenderHost.MouseDownItem
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		int ICommandRenderHost.MaxItemRows => 3;

		CommandBaseRenderer ICommandRenderHost.ActiveInputItem
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		ISite ICommandRenderHost.Site => Site;

		bool ICommandRenderHost.UseCompatibleTextRendering => UseCompatibleTextRendering;

		RightToLeft ICommandRenderHost.RightToLeft => RightToLeft.No;

		bool ICommandRenderHost.IsInDesignMode => base.DesignMode;

		CommandBaseEditorDialog ICommandRenderHost.ChildMenu
		{
			get
			{
				return _childMenu;
			}
			set
			{
				SetChildMenu(value);
			}
		}

		CommandCollection ICommandRenderHost.Commands => _commandCollection;

		Form ICommandRenderHost.ParentForm
		{
			get
			{
				if (_control != null)
				{
					return RibbonDrawingUtils.FindParentForm(_control);
				}
				return null;
			}
		}

		CommandBaseRenderer ICommandRenderHost.SelectedItem
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		bool ICommandRenderHost.ShowToolTips => ShowToolTips;

		MxRibbon ICommandRenderHost.Ribbon => null;

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
			Commands.Insert(index, command);
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			Commands.RemoveFromInnerListAndClearOwner(command);
		}

		void ICommandContainer.CommandAdded(CommandBase command)
		{
			if (base.Container != null && command.Container == null)
			{
				base.Container.Add(command);
				if (string.IsNullOrEmpty(command.Key))
				{
					command.Key = command.Name;
				}
			}
			RefreshChildMenuLayout();
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (base.Container != null && command.Container != null)
			{
				command.Container.Remove(command);
			}
			RefreshChildMenuLayout();
		}

		void ICommandContainer.CommandClicked(CommandEventArgs eventArgs)
		{
			if (GetChildMenu() != null)
			{
				GetChildMenu().TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
			}
			OnCommandClick(eventArgs);
		}

		bool ICommandContainer.CommandDropDownClosing(DropDownClosingEventArgs eventArgs)
		{
			OnCommandDropDownClosing(eventArgs);
			return eventArgs.Cancel;
		}

		void ICommandContainer.CommandDropDownClosed(CommandEventArgs eventArgs)
		{
			OnCommandDropDownClosed(eventArgs);
		}

		void ICommandContainer.CommandDropDownOpened(CommandEventArgs eventArgs)
		{
			OnCommandDropDownOpened(eventArgs);
		}

		bool ICommandContainer.CommandDropDownOpening(CommandCancelEventArgs eventArgs)
		{
			OnCommandDropDownOpening(eventArgs);
			return eventArgs.Cancel;
		}

		void ICommandContainer.CommandDisposed(CommandEventArgs eventArgs)
		{
			if (Commands.Contains(eventArgs.Command))
			{
				Commands.Remove(eventArgs.Command);
			}
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			switch (commandType)
			{
			case CommandType.RibbonGroup:
			case CommandType.GalleryItem:
			case CommandType.StatusBarPanel:
				return false;
			default:
				return true;
			}
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
			if (!(eventArgs.Command is ButtonCommand buttonCommand) || !buttonCommand.Checked || !buttonCommand.ActAsOptionButton)
			{
				return;
			}
			foreach (CommandBase command in Commands)
			{
				if (command is ButtonCommand && command != eventArgs.Command && ((ButtonCommand)command).ActAsOptionButton)
				{
					((ButtonCommand)command).Checked = false;
				}
			}
		}

		bool IExtenderProvider.CanExtend(object target)
		{
			if (target is Control)
			{
				return true;
			}
			if (target is NotifyIcon)
			{
				return true;
			}
			return false;
		}

		ImageList ICommandRenderHost.GetImageList(CommandBase command)
		{
			return ImageList;
		}

		ImageList ICommandRenderHost.GetLargeImageList(CommandBase commmand)
		{
			return LargeImageList;
		}

		Point ICommandRenderHost.PointToScreen(Point clientPoint)
		{
			CommandBaseEditorDialog childMenu = _childMenu;
			if (childMenu != null)
			{
				while (childMenu.GetChildMenu() != null)
				{
					childMenu = childMenu.GetChildMenu();
				}
				return childMenu.PointToScreen(clientPoint);
			}
			return clientPoint;
		}

		void ICommandRenderHost.ShowQuickCustomizeMenu(CommandBase command)
		{
		}

		bool IMessageFilter.PreFilterMessage(ref Message msg)
		{
			if (msg.Msg == NativeMethods.WmKeyDown)
			{
				int keyData = GetShortcutKeyCode(msg.WParam);
				Shortcut shortcut = (Shortcut)keyData;
				if (Enum.IsDefined(typeof(Shortcut), shortcut))
				{
					Control control = null;
					control = Control.FromHandle(msg.HWnd);
					if (control != null && IsWithinEnabledTarget(control) && TryHandleShortcut(shortcut, null, msg))
					{
						return true;
					}
				}
			}
			else if (msg.Msg == NativeMethods.WmSysKeyDown)
			{
				Shortcut sysKeyShortcut;
				if (NativeMethods.ToInt32Unchecked(msg.WParam) == 121)
				{
					int sysKeyData = GetShortcutKeyCode(msg.WParam);
					sysKeyShortcut = (Shortcut)sysKeyData;
				}
				else
				{
					sysKeyShortcut = (Shortcut)(262144 + NativeMethods.ToInt32Unchecked(msg.WParam));
				}
				if (Enum.IsDefined(typeof(Shortcut), sysKeyShortcut))
				{
					Control sysKeyControl = null;
					sysKeyControl = Control.FromHandle(msg.HWnd);
					if (sysKeyControl != null && IsWithinEnabledTarget(sysKeyControl) && TryHandleShortcut(sysKeyShortcut, null, msg))
					{
						return true;
					}
				}
			}
			else if (msg.Msg == NativeMethods.WmRButtonUp && _menuTargets != null && _menuTargets.Count > 0)
			{
				Control clickedControl = Control.FromHandle(msg.HWnd);
				if (clickedControl != null)
				{
					return TryShowForControl(clickedControl);
				}
			}
			return false;
		}
	}
}
