using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		MxRibbon ITabOwner.Ribbon => this;

		bool ITabOwner.IsEnabled => base.Enabled;

		MxRibbon ICommandContainer.Ribbon => this;

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu => null;

		CommandType ICommandContainer.CommandType => CommandType.Ribbon;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Default;

		bool ICommandContainer.IsEnabled => base.Enabled;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Horizontal;

		CommandCollection ICommandContainer.Commands => _quickAccessCommands;

		Control IRibbonHitTestSource.HostControl => this;

		RibbonColorScheme IRibbonHitTestSource.ColorScheme => _colorScheme;

		AccessibleObject ICommandRenderHost.AccessibilityObject => base.AccessibilityObject;

		int ICommandRenderHost.SmallItemHeight => GetSmallItemHeight();

		Size ICommandRenderHost.SmallImageSize => SmallImageSize;

		Size ICommandRenderHost.LargeImageSize => LargeImageSize;

		Font ICommandRenderHost.Font => Font;

		RibbonColorScheme ICommandRenderHost.ColorScheme => _colorScheme;


		CommandBaseRenderer ICommandRenderHost.FocusItem
		{
			get
			{
				return GetFocusItem();
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
				return GetMouseDownItem();
			}
			set
			{
				SetPushedRenderer(value);
			}
		}

		int ICommandRenderHost.MaxItemRows => _itemRows;

		CommandBaseRenderer ICommandRenderHost.ActiveInputItem
		{
			get
			{
				return GetKeyboardFocusRenderer();
			}
			set
			{
				SetKeyboardFocusRenderer(value);
			}
		}

		ISite ICommandRenderHost.Site => Site;

		bool ICommandRenderHost.UseCompatibleTextRendering => UseCompatibleTextRendering;

		bool ICommandRenderHost.IsInDesignMode => base.DesignMode;

		Control ICommandRenderHost.ParentControl
		{
			get
			{
				if (_minimized && GetPopupMenu() != null)
				{
					return _popupMenu;
				}
				return this;
			}
		}

		CommandBaseEditorDialog ICommandRenderHost.ChildMenu
		{
			get
			{
				return _popupMenu;
			}
			set
			{
				SetPopupMenu(value);
			}
		}

		CommandCollection ICommandRenderHost.Commands => _quickAccessCommands;

		Form ICommandRenderHost.ParentForm => GetHostForm();

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

		MxRibbon ICommandRenderHost.Ribbon => this;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			DeserializeRibbonState(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			SerializeRibbonState(info);
			if (_layouts.Count > 0)
			{
				info.AddValue("Layouts", _layouts);
			}
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
				if (GetHostForm() == null)
				{
					AttachToForm(RibbonDrawingUtils.FindParentForm(this));
					SetParentedToForm(base.Parent == GetHostForm() && base.Parent != null);
					RefreshInternalCaptionRow();
					RelayoutForFormState();
				}
				else if (_ribbonWindowMessageHandler is RibbonCustomCaptionMessageHandler)
				{
					((RibbonCustomCaptionMessageHandler)_ribbonWindowMessageHandler).AttachFormResizeHandler();
				}
				if (_layoutStream != null)
				{
					LayoutStream = _layoutStream;
					_layoutStream = null;
				}
			}
		}

		void ITabOwner.TabDisposed(RibbonTab tab)
		{
			if (Tabs.Contains(tab))
			{
				Tabs.Remove(tab);
			}
		}

		void ITabOwner.TabAdded(RibbonTab tab)
		{
			if (base.Container != null && tab.Container == null)
			{
				base.Container.Add(tab);
				if (tab.Key.Length == 0)
				{
					tab.Key = tab.Name;
				}
				foreach (RibbonGroup group in tab.Groups)
				{
					if (group.Container == null)
					{
						tab.Container.Add(group);
						if (group.Key.Length == 0)
						{
							group.Key = group.Name;
						}
					}
				}
			}
			else if (tab.Container != null && tab.Key.Length == 0)
			{
				tab.Key = tab.Name;
			}
			RequestTabStripLayout(true);
			if (_selectedTab == null && tab.Visible)
			{
				if (tab.GetTabOwner() is RibbonContextualTab)
				{
					RibbonContextualTab ribbonContextualTab = tab.GetTabOwner() as RibbonContextualTab;
					if (ribbonContextualTab.Active)
					{
						SelectedTab = tab;
					}
				}
				else
				{
					SelectedTab = tab;
				}
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.InvalidateLayout();
			}
			RefreshWindowFrame(true);
		}

		void ITabOwner.TabRemoved(RibbonTab tab)
		{
			if (base.Container != null && tab.Container != null && tab.HasBeenDisposed())
			{
				tab.Container.Remove(tab);
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.InvalidateLayout();
			}
			RequestTabStripLayout(true);
			RefreshWindowFrame(true);
			if (SelectedTab == tab)
			{
				SelectFirstVisibleTab();
			}
		}

		void ITabOwner.RemoveTab(RibbonTab tab)
		{
			Tabs.DetachTab(tab);
		}

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
			QuickAccessCommands.Insert(index, command);
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			if (QuickAccessCommands.Contains(command))
			{
				QuickAccessCommands.RemoveFromInnerListAndClearOwner(command);
			}
			if (QuickAccessCustomizeList.Contains(command))
			{
				QuickAccessCustomizeList.RemoveFromInnerListAndClearOwner(command);
			}
		}

		void ICommandContainer.CommandAdded(CommandBase command)
		{
			if (base.Container != null && command.Container == null)
			{
				base.Container.Add(command);
				if (command.Key.Length == 0)
				{
					command.Key = command.Name;
				}
			}
			else if (command.Container != null && command.Key.Length == 0)
			{
				command.Key = command.Name;
			}
			if (command is BackstageTab)
			{
				BackstageTab backstageTab = (BackstageTab)command;
				if (base.Container != null && backstageTab.TabPage.Container == null)
				{
					base.Container.Add(backstageTab.TabPage);
				}
			}
			if (_tabContentRenderer != null)
			{
				_tabContentRenderer.MarkGroupsStale();
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.RequestLayoutAndRepaint();
			}
			if (_quickAccessToolbarRenderer != null)
			{
				_quickAccessToolbarRenderer.MarkItemsDirty();
			}
			if (_backstageMenuScrollableBase != null)
			{
				_backstageMenuScrollableBase.ApplyLayout();
			}
			if (GetPopupMenu() != null)
			{
				GetPopupMenu().RefreshForCommand(command, false);
			}
			if (_mergeHost != null)
			{
				_mergeHost.InvalidateTabContent();
			}
			InvalidateTabContent();
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (base.Container != null && command.Container != null && command.GetParentContainer() == null && command.HasBeenDisposed())
			{
				base.Container.Remove(command);
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.RequestLayoutAndRepaint();
			}
			if (_quickAccessToolbarRenderer != null)
			{
				_quickAccessToolbarRenderer.MarkItemsDirty();
			}
			if (_tabContentRenderer != null)
			{
				_tabContentRenderer.MarkGroupsStale();
			}
			if (GetPopupMenu() != null)
			{
				GetPopupMenu().RefreshForCommand(command, true);
			}
			if (_mergeHost != null)
			{
				_mergeHost.InvalidateTabContent();
			}
			InvalidateTabContent();
		}

		void ICommandContainer.CommandClicked(CommandEventArgs eventArgs)
		{
			SetKeyTipMode(false);
			if (_tabContentRenderer != null && !base.DesignMode)
			{
				_tabContentRenderer.SetHotGroup(null);
			}
			if (GetPopupMenu() != null && eventArgs.Command.CommandType != CommandType.ContainerControl)
			{
				GetPopupMenu().TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
			}
			if (_mergeHost != null && _mergeHost.GetPopupMenu() != null && eventArgs.Command.CommandType != CommandType.ContainerControl)
			{
				_mergeHost.GetPopupMenu().TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
			}
			if (!IsInDesignMode() && !(eventArgs.Command is BackstageTab))
			{
				CloseBackstage();
			}
			if (!base.DesignMode)
			{
				OnCommandClick(eventArgs);
			}
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
			if (QuickAccessCommands.Contains(eventArgs.Command))
			{
				QuickAccessCommands.Remove(eventArgs.Command);
			}
			if (QuickAccessCustomizeList.Contains(eventArgs.Command))
			{
				QuickAccessCustomizeList.Remove(eventArgs.Command);
			}
			if (ControlBoxMenu.LeftCommands.Contains(eventArgs.Command))
			{
				ControlBoxMenu.LeftCommands.Remove(eventArgs.Command);
			}
			if (ControlBoxMenu.RightCommands.Contains(eventArgs.Command))
			{
				ControlBoxMenu.RightCommands.Remove(eventArgs.Command);
			}
			if (ControlBoxMenu.BottomCommands.Contains(eventArgs.Command))
			{
				ControlBoxMenu.BottomCommands.Remove(eventArgs.Command);
			}
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			switch (commandType)
			{
			case CommandType.ButtonGroup:
			case CommandType.Box:
			case CommandType.GalleryItem:
			case CommandType.StatusBarPanel:
				return false;
			default:
				return true;
			}
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
			if (ControlBoxMenu.LeftCommands.Contains(eventArgs.Command) && eventArgs.Command is ButtonCommand buttonCommand && buttonCommand.Checked && buttonCommand.ActAsOptionButton)
			{
				foreach (CommandBase leftCommand in ControlBoxMenu.LeftCommands)
				{
					if (leftCommand is ButtonCommand && leftCommand != eventArgs.Command && ((ButtonCommand)leftCommand).ActAsOptionButton)
					{
						((ButtonCommand)leftCommand).Checked = false;
					}
				}
			}
			if (ControlBoxMenu.RightCommands.Contains(eventArgs.Command) && eventArgs.Command is ButtonCommand rightOptionButton && rightOptionButton.Checked && rightOptionButton.ActAsOptionButton)
			{
				foreach (CommandBase rightCommand in ControlBoxMenu.RightCommands)
				{
					if (rightCommand is ButtonCommand && rightCommand != eventArgs.Command && ((ButtonCommand)rightCommand).ActAsOptionButton)
					{
						((ButtonCommand)rightCommand).Checked = false;
					}
				}
			}
			if (!ControlBoxMenu.BottomCommands.Contains(eventArgs.Command) || !(eventArgs.Command is ButtonCommand bottomOptionButton) || !bottomOptionButton.Checked || !bottomOptionButton.ActAsOptionButton)
			{
				return;
			}
			foreach (CommandBase bottomCommand in ControlBoxMenu.BottomCommands)
			{
				if (bottomCommand is ButtonCommand && bottomCommand != eventArgs.Command && ((ButtonCommand)bottomCommand).ActAsOptionButton)
				{
					((ButtonCommand)bottomCommand).Checked = false;
				}
			}
		}

		Point IRibbonHitTestSource.PointToClient(Point screenPoint)
		{
			return PointToClient(screenPoint);
		}

		Rectangle IRibbonHitTestSource.GetCommandBounds(CommandBase command)
		{
			return GetCommandRendererBounds(command);
		}

		Rectangle IRibbonHitTestSource.GetBackstageBarBounds()
		{
			if (GetBackstageMenuScrollableBase() != null)
			{
				Rectangle rectangle = GetBackstageMenuScrollableBase().GetScreenBounds();
				return new Rectangle(PointToClient(rectangle.Location), rectangle.Size);
			}
			return Rectangle.Empty;
		}

		CommandBase IRibbonHitTestSource.GetCommandAt(Point pt)
		{
			return GetCommandAt(pt);
		}

		bool IMessageFilter.PreFilterMessage(ref Message msg)
		{
			return PreFilterRibbonMessage(ref msg);
		}

		ImageList ICommandRenderHost.GetImageList(CommandBase command)
		{
			if (command != null && command.IsMerged() && _mergedRibbon != null)
			{
				return _ribbonImageList;
			}
			return ImageList;
		}

		ImageList ICommandRenderHost.GetLargeImageList(CommandBase command)
		{
			if (command.IsMerged() && _mergedRibbon != null)
			{
				return _ribbonLargeImageList;
			}
			return _largeImageList;
		}

		Point ICommandRenderHost.PointToScreen(Point clientPoint)
		{
			return PointToScreen(clientPoint);
		}

		void ICommandRenderHost.ShowQuickCustomizeMenu(CommandBase command)
		{
			BuildRibbonContextMenu(command, _isQuickAccessItemMouseUp);
		}

		AccessibleObject ICommandContainer.AccessibilityObject
		{
			get { return base.AccessibilityObject; }
		}
	}
}
