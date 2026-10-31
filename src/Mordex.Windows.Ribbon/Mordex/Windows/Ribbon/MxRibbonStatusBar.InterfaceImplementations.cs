using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		MxRibbon ICommandContainer.Ribbon => null;

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu => null;

		CommandType ICommandContainer.CommandType => CommandType.StatusBar;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Default;

		bool ICommandContainer.IsEnabled => base.Enabled;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Horizontal;

		CommandCollection ICommandContainer.Commands => _leftPanelCommands;

		AccessibleObject ICommandRenderHost.AccessibilityObject => base.AccessibilityObject;

		int ICommandRenderHost.SmallItemHeight
		{
			get
			{
				int itemHeight = _imageSize.Height;
				if (Font.Height + 1 > itemHeight)
				{
					itemHeight = Font.Height + 1;
				}
				return itemHeight + 6;
			}
		}

		Size ICommandRenderHost.SmallImageSize => ImageSize;

		Size ICommandRenderHost.LargeImageSize => ImageSize;

		Font ICommandRenderHost.Font => Font;

		RibbonColorScheme ICommandRenderHost.ColorScheme => _colorScheme;


		CommandBaseRenderer ICommandRenderHost.FocusItem
		{
			get
			{
				return GetHotRenderer();
			}
			set
			{
				SetHotRenderer(value);
			}
		}

		CommandBaseRenderer ICommandRenderHost.MouseDownItem
		{
			get
			{
				return GetPushedRenderer();
			}
			set
			{
				SetPushedRenderer(value);
			}
		}

		int ICommandRenderHost.MaxItemRows => 1;

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

		bool ICommandRenderHost.IsInDesignMode => base.DesignMode;

		Control ICommandRenderHost.ParentControl => this;

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

		CommandCollection ICommandRenderHost.Commands => _leftPanelCommands;

		Form ICommandRenderHost.ParentForm => RibbonDrawingUtils.FindParentForm(this);

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

		Control IRibbonHitTestSource.HostControl => this;

		RibbonColorScheme IRibbonHitTestSource.ColorScheme => _colorScheme;

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			if (LeftPanelCommands.Contains(command))
			{
				LeftPanelCommands.RemoveFromInnerListAndClearOwner(command);
			}
			if (RightPanelCommands.Contains(command))
			{
				RightPanelCommands.RemoveFromInnerListAndClearOwner(command);
			}
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
			if (GetChildMenu() != null)
			{
				GetChildMenu().RefreshForCommand(command, false);
			}
			RebuildPanelLayouts();
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (base.Container != null && command.Container != null)
			{
				command.Container.Remove(command);
			}
			if (command is ContainerControlCommand)
			{
				Control control = ((ContainerControlCommand)command).Control;
				if (control != null && base.Controls.Contains(control))
				{
					base.Controls.Remove(control);
				}
			}
			if (GetChildMenu() != null)
			{
				GetChildMenu().RefreshForCommand(command, true);
			}
			RebuildPanelLayouts();
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
			if (LeftPanelCommands.Contains(eventArgs.Command))
			{
				LeftPanelCommands.Remove(eventArgs.Command);
			}
			if (RightPanelCommands.Contains(eventArgs.Command))
			{
				RightPanelCommands.Remove(eventArgs.Command);
			}
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			switch (commandType)
			{
			case CommandType.Box:
			case CommandType.RibbonGroup:
			case CommandType.GalleryItem:
			case CommandType.Gallery:
				return false;
			default:
				return true;
			}
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
			if (_leftPanelCommands.Contains(eventArgs.Command) && eventArgs.Command is ButtonCommand buttonCommand && buttonCommand.Checked && buttonCommand.ActAsOptionButton)
			{
				foreach (CommandBase item in _leftPanelCommands)
				{
					if (item is ButtonCommand && item != eventArgs.Command && ((ButtonCommand)item).ActAsOptionButton)
					{
						((ButtonCommand)item).Checked = false;
					}
				}
			}
			if (!_rightPanelCommands.Contains(eventArgs.Command) || !(eventArgs.Command is ButtonCommand rightButtonCommand) || !rightButtonCommand.Checked || !rightButtonCommand.ActAsOptionButton)
			{
				return;
			}
			foreach (CommandBase rightItem in _rightPanelCommands)
			{
				if (rightItem is ButtonCommand && rightItem != eventArgs.Command && ((ButtonCommand)rightItem).ActAsOptionButton)
				{
					((ButtonCommand)rightItem).Checked = false;
				}
			}
		}

		ImageList ICommandRenderHost.GetImageList(CommandBase command)
		{
			return ImageList;
		}

		ImageList ICommandRenderHost.GetLargeImageList(CommandBase command)
		{
			return null;
		}

		Point ICommandRenderHost.PointToScreen(Point clientPoint)
		{
			return PointToScreen(clientPoint);
		}

		void ICommandRenderHost.ShowQuickCustomizeMenu(CommandBase command)
		{
		}

		Point IRibbonHitTestSource.PointToClient(Point screenPoint)
		{
			return PointToClient(screenPoint);
		}

		Rectangle IRibbonHitTestSource.GetCommandBounds(CommandBase command)
		{
			return FindCommandRenderer(command)?._bounds ?? Rectangle.Empty;
		}

		Rectangle IRibbonHitTestSource.GetBackstageBarBounds()
		{
			return Rectangle.Empty;
		}

		CommandBase IRibbonHitTestSource.GetCommandAt(Point pt)
		{
			return FindCommandAt(pt);
		}

		AccessibleObject ICommandContainer.AccessibilityObject
		{
			get { return base.AccessibilityObject; }
		}
	}
}
