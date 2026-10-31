using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		internal CommandBaseEditorDialog GetPopupMenu()
		{
			return _popupMenu;
		}

		internal void SetPopupMenu(CommandBaseEditorDialog parentControl)
		{
			if (_popupMenu != parentControl)
			{
				if (_popupMenu != null)
				{
					NativeMethods.AnimateWindow(_popupMenu.Handle, NativeMethods.PopupFadeMilliseconds, NativeMethods.AwHideWithFade);
					_popupMenu.RemoveDropDownHandler(OnPopupDropDownCommand);
					_popupMenu.Dispose();
				}
				_popupMenu = parentControl;
				if (_popupMenu != null)
				{
					_popupMenu.AddDropDownHandler(OnPopupDropDownCommand);
				}
				RefreshWindowFrame(true);
			}
		}

		internal void CloseBackstage()
		{
			if (_backstageMenuScrollableBase != null)
			{
				ShowSuperTip(null);
				CancelEventArgs e = new CancelEventArgs();
				OnBackstageClosing(e);
				if (!e.Cancel)
				{
					SetBackstageMenuScrollableBase(null);
					EventArgs e2 = new EventArgs();
					OnBackstageClosed(e2);
				}
				SetKeyTipMode(false);
				RequestTabStripLayout(false);
			}
		}

		internal void ShowBackstageMenu()
		{
			if (_tabStripRenderer != null)
			{
				if (_isTabStripLayoutDirty)
				{
					RenderRibbonImmediately();
				}
				ToggleBackstageMenu(_tabStripRenderer.FindTabRenderer(BackstageMenu) as BackstageMenuTabRenderer);
			}
		}

		internal void ToggleBackstageMenu(BackstageMenuTabRenderer backstageTabRenderer)
		{
			SetKeyboardFocusRenderer(null);
			if (GetPopupMenu() != null)
			{
				SetPopupMenu(null);
			}
			if (_backstageMenuScrollableBase == null)
			{
				CancelEventArgs e = new CancelEventArgs();
				OnBackstageOpening(e);
				if (!e.Cancel)
				{
					BackstageMenuScrollableBase backstageMenuScrollableBase = new BackstageMenuScrollableBase(this, BackstageMenu);
					Rectangle bounds = backstageTabRenderer._bounds;
					if (IsGlassCaptionActive())
					{
						bounds.Y--;
					}
					if (backstageMenuScrollableBase.TryShowBelow(bounds))
					{
						SetBackstageMenuScrollableBase(backstageMenuScrollableBase);
						EventArgs e2 = new EventArgs();
						OnBackstageOpened(e2);
						if (_isOpeningKeyTipDropDown)
						{
							LoadBackstageKeyTips();
						}
					}
					else
					{
						backstageMenuScrollableBase.Dispose();
					}
				}
				RequestTabStripLayout(false);
			}
			else
			{
				CloseBackstage();
			}
		}

		internal void CloseDropDownPopup(DropDownCommand dropDownCommand)
		{
			if (GetPopupMenu() != null)
			{
				if (GetPopupMenu().GetParentMenuCommand() == dropDownCommand)
				{
					TryCloseParentControl(CommandCloseReason.CloseCalled);
				}
				else
				{
					GetPopupMenu().CloseChildMenuFor(dropDownCommand);
				}
			}
		}

		internal void OpenDropDownCommand(DropDownCommand dropDownCommand)
		{
			if (!dropDownCommand.Enabled || !dropDownCommand.Visible)
			{
				return;
			}
			RibbonTab ribbonTab = null;
			ICommandContainer iAccessibleRibbonElement = dropDownCommand.GetParentContainer();
			while (iAccessibleRibbonElement != null)
			{
				if (iAccessibleRibbonElement is RibbonTab)
				{
					ribbonTab = (RibbonTab)iAccessibleRibbonElement;
					if (ribbonTab != SelectedTab)
					{
						using (Graphics graphics = CreateGraphics())
						{
							SelectedTab = ribbonTab;
							RenderTabContent(graphics);
						}
					}
					break;
				}
				if (iAccessibleRibbonElement is RibbonGroup)
				{
					if (_tabContentRenderer != null)
					{
						RibbonGroupBoxRenderer ribbonRenderer = _tabContentRenderer.FindGroupRenderer(iAccessibleRibbonElement as RibbonGroup);
						if (ribbonRenderer == null)
						{
							ribbonRenderer = _tabContentRenderer.FindGroupRendererForCommand(dropDownCommand);
						}
						if (ribbonRenderer != null && ribbonRenderer._isCollapsed)
						{
							_dropDownCommand = dropDownCommand;
							if (ribbonRenderer.IsPopupOpen() && GetPopupMenu() != null)
							{
								GetPopupMenu().OpenDropDown(_dropDownCommand);
							}
							else
							{
								ribbonRenderer.ShowCollapsedPopup();
							}
							_dropDownCommand = null;
							return;
						}
						iAccessibleRibbonElement = ((ribbonRenderer == null) ? ((CommandBase)iAccessibleRibbonElement).GetParentContainer() : ribbonRenderer.GetRibbonGroup().GetParentContainer());
					}
					else
					{
						iAccessibleRibbonElement = ((CommandBase)iAccessibleRibbonElement).GetParentContainer();
					}
				}
				else
				{
					if (!(iAccessibleRibbonElement is CommandBase))
					{
						break;
					}
					iAccessibleRibbonElement = ((CommandBase)iAccessibleRibbonElement).GetParentContainer();
				}
			}
			if (dropDownCommand.GetParentContainer() is DropDownCommand)
			{
				if (GetPopupMenu() != null && GetPopupMenu().GetParentMenuCommand() == dropDownCommand.GetParentContainer())
				{
					GetPopupMenu().OpenDropDown(dropDownCommand);
					return;
				}
				if (_dropDownCommand == null)
				{
					_dropDownCommand = dropDownCommand;
				}
				OpenDropDownCommand(dropDownCommand.GetParentContainer() as DropDownCommand);
				_dropDownCommand = null;
			}
			else if (iAccessibleRibbonElement == this)
			{
				bool isControlBoxCommand = false;
				if (ControlBoxMenu.LeftCommands.Contains(dropDownCommand) || ControlBoxMenu.RightCommands.Contains(dropDownCommand) || ControlBoxMenu.BottomCommands.Contains(dropDownCommand))
				{
					isControlBoxCommand = true;
				}
				if (!isControlBoxCommand)
				{
					return;
				}
				if (GetPopupMenu() != null)
				{
					GetPopupMenu().OpenDropDown(dropDownCommand);
				}
				else if (_controlBoxButtonRenderer != null)
				{
					if (_dropDownCommand == null)
					{
						_dropDownCommand = dropDownCommand;
					}
					ShowControlBoxMenu();
					_dropDownCommand = null;
				}
			}
			else
			{
				if (_tabContentRenderer._groupRenderers == null)
				{
					return;
				}
				foreach (RibbonGroupBoxRenderer item in _tabContentRenderer._groupRenderers)
				{
					foreach (CommandBaseRenderer commandRenderer in item._childRenderers)
					{
						if (commandRenderer.GetCommand() == dropDownCommand)
						{
							if (commandRenderer is IDropDownItem)
							{
								((IDropDownItem)commandRenderer).DropDown(selectFirstItem: true);
							}
							break;
						}
					}
				}
			}
		}

		internal void ShowSuperTip(CommandBaseRenderer item)
		{
			if (!ShowToolTips)
			{
				return;
			}
			ButtonCommand buttonCommand = null;
			SuperTipSettings superTipSettings = null;
			CommandBase commandBase = null;
			if (item != null)
			{
				commandBase = item.GetCommand();
				if (item._commandBaseRendererCommand is ButtonCommand)
				{
					buttonCommand = item.GetCommand() as ButtonCommand;
					superTipSettings = ((ButtonCommand)item.GetCommand()).SuperTipSettings;
				}
				else if (item.GetCommand() is GalleryItem)
				{
					superTipSettings = ((GalleryItem)item.GetCommand()).SuperTipSettings;
				}
				else if (item.GetCommand() is RibbonGroup)
				{
					superTipSettings = ((RibbonGroup)item.GetCommand()).SuperTipSettings;
				}
				else if (item.GetCommand() is BackstageTab)
				{
					superTipSettings = ((BackstageTab)item.GetCommand()).SuperTipSettings;
				}
				else if (item is RibbonGroupDialogLauncherRenderer && item._layoutItemOwner is RibbonGroupBoxRenderer ribbonRenderer)
				{
					commandBase = ribbonRenderer.GetCommand();
					superTipSettings = ribbonRenderer.GetRibbonGroup().DialogButtonSuperTipSettings;
				}
			}
			if (item == null || commandBase == null || superTipSettings == null || superTipSettings.IsEmpty)
			{
				ClearSuperTipCommand();
			}
			else
			{
				if (commandBase == _superTipCommand)
				{
					return;
				}
				ClearSuperTipCommand();
				_superTipCommand = commandBase;
				string text = "";
				if (ShowShortcutInToolTips && buttonCommand != null && buttonCommand.Shortcut != Shortcut.None)
				{
					text = text + " (" + RibbonDrawingUtils.GetShortcutText(buttonCommand.Shortcut) + ")";
				}
				SuperTipSettings tipSettingsWithShortcut = superTipSettings.Clone();
				tipSettingsWithShortcut.HeaderText += text;
				if (_superTipDelay <= 0)
				{
					_mordexSuperTip.ShowAlways = true;
				}
				else
				{
					_mordexSuperTip.ShowAlways = false;
					_mordexSuperTip.AutoPopDelay = _superTipDelay;
				}
				_mordexSuperTip.BodyWidth = SuperTipWidth;
				Point point = new Point(item._bounds.X, item._ownerBottom + 2);
				if (item.GetCommand() is RibbonGroup)
				{
					point = new Point(item._bounds.X, item._bounds.Bottom + 2);
				}
				if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.ContainsItem(item))
				{
					point = _quickAccessToolbarRenderer.PointToScreen(item, new Point(item._bounds.X, item._bounds.Bottom + 5));
				}
				else if (item.GetRenderHost() != null)
				{
					if (item.GetRenderHost() is CommandBaseEditorDialog)
					{
						point = new Point(item._bounds.X, item._bounds.Bottom + 2);
					}
					else if (item.GetRenderHost() is BackstageMenuScrollableBase)
					{
						point = new Point(item._bounds.X, item._bounds.Bottom + 2);
					}
					point = item.GetRenderHost().PointToScreen(point);
				}
				else
				{
					point = PointToScreen(point);
				}
				_mordexSuperTip.Show(tipSettingsWithShortcut, this, point, _superTipDelay);
			}
		}

		internal void ClearSuperTipCommand()
		{
			if (_mordexSuperTip != null)
			{
				_mordexSuperTip.HideActiveToolTip();
			}
			_superTipCommand = null;
		}

		internal void ShowControlBoxMenu()
		{
			if ((_designer != null && DesignHelper.Designer_Locked(_designer)) || ((_controlBoxButtonRenderer == null || _controlBoxButtonRenderer.IsMenuOpen()) && GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxTab))
			{
				return;
			}
			if (base.DesignMode && _designer != null)
			{
				DesignHelper.Designer_SelectComponent(_designer, this);
			}
			CancelEventArgs e = new CancelEventArgs();
			OnControlBoxOpening(e);
			if (e.Cancel)
			{
				if (_controlBoxButtonRenderer != null)
				{
					_controlBoxButtonRenderer.SetMenuOpen(false);
					_controlBoxButtonRenderer.Hot = false;
					_controlBoxButtonRenderer.Pushed = false;
				}
				return;
			}
			if (_controlBoxButtonRenderer == null && (_tabStripRenderer == null || _tabStripRenderer._backstageTabRenderer == null))
			{
				return;
			}
			ControlBoxMenuRenderer controlBoxMenuRenderer = new ControlBoxMenuRenderer(_controlBoxMenu);
			controlBoxMenuRenderer._commandSizeStyle = CommandSizeStyle.Large;
			OnControlBoxOpened(EventArgs.Empty);
			if (_controlBoxButtonRenderer != null)
			{
				_controlBoxButtonRenderer.SetMenuOpen(true);
				if (IsCaptionBarShown())
				{
					controlBoxMenuRenderer.ShowAt(PointToScreen(new Point(0, _captionHeight)));
				}
				else
				{
					controlBoxMenuRenderer.ShowAt(PointToScreen(new Point(0, 0)));
				}
			}
			else if (_tabStripRenderer != null && _tabStripRenderer._backstageTabRenderer != null)
			{
				int menuTop = 1;
				if (IsCaptionBarShown())
				{
					controlBoxMenuRenderer.ShowAt(PointToScreen(new Point(-1, _captionHeight + menuTop)));
				}
				else
				{
					controlBoxMenuRenderer.ShowAt(PointToScreen(new Point(-1, menuTop)));
				}
			}
			controlBoxMenuRenderer = null;
			if (_controlBoxButtonRenderer != null)
			{
				_controlBoxButtonRenderer.SetMenuOpen(false);
				_controlBoxButtonRenderer.Hot = false;
				_controlBoxButtonRenderer.Pushed = false;
			}
		}

		internal void ShowQuickAccessCustomizeMenuAt(Point point, Size size)
		{
			BuildQuickAccessCustomizeMenu();
			_ribbonContextMenu._anchorSize = size;
			_ribbonContextMenu.Show(this, PointToClient(point));
		}

		internal void BuildRibbonContextMenu(CommandBase targetCommand, bool isInQuickAccess)
		{
			if (!ShowQuickCustomizeMenu)
			{
				return;
			}
			if (targetCommand is GalleryItem)
			{
				targetCommand = targetCommand.GetParentContainer() as CommandBase;
			}
			if (_ribbonContextMenu != null)
			{
				_ribbonContextMenu.CommandClick -= OnRibbonContextMenuCommandClick;
				_ribbonContextMenu.Dispose();
			}
			_ribbonContextMenu = new MxRibbonContextMenu();
			if (targetCommand != null && targetCommand.AllowAddToQuickAccessBar && QuickAccessCommands.CanContainCommand(targetCommand))
			{
				if (isInQuickAccess)
				{
					ButtonCommand buttonCommand = new ButtonCommand("RemoveFromQA");
					buttonCommand.Text = _builtInTexts[RibbonBuiltInText.RemoveFromQA];
					if (buttonCommand.Text != null && buttonCommand.Text.Length > 0)
					{
						buttonCommand.Tag = targetCommand;
						_ribbonContextMenu.Commands.Add(buttonCommand);
					}
				}
				else
				{
					ButtonCommand addToQuickAccessButton = new ButtonCommand("AddToQA");
					addToQuickAccessButton.Text = _builtInTexts[RibbonBuiltInText.AddToQA];
					if (addToQuickAccessButton.Text != null && addToQuickAccessButton.Text.Length > 0)
					{
						addToQuickAccessButton.Tag = targetCommand;
						if (_quickAccessCommands.Contains(targetCommand))
						{
							addToQuickAccessButton.Enabled = false;
						}
						_ribbonContextMenu.Commands.Add(addToQuickAccessButton);
					}
				}
				SeparatorCommand command = new SeparatorCommand();
				_ribbonContextMenu.Commands.Add(command);
			}
			ButtonCommand moreCommandsButton = new ButtonCommand("cmdMoreCommands");
			moreCommandsButton.Text = _builtInTexts[RibbonBuiltInText.CustomizeMoreCommands];
			if (moreCommandsButton.Text != null && moreCommandsButton.Text.Length > 0)
			{
				_ribbonContextMenu.Commands.Add(moreCommandsButton);
			}
			ButtonCommand positionButton = new ButtonCommand("cmdQAPosition");
			if (_quickAccessPosition == QuickAccessPosition.AboveRibbon)
			{
				positionButton.Text = _builtInTexts[RibbonBuiltInText.ShowBelowRibbon];
			}
			else
			{
				positionButton.Text = _builtInTexts[RibbonBuiltInText.ShowAboveRibbon];
			}
			if (positionButton.Text != null && positionButton.Text.Length > 0)
			{
				_ribbonContextMenu.Commands.Add(positionButton);
			}
			_ribbonContextMenu.CommandClick += OnRibbonContextMenuCommandClick;
			_ribbonContextMenu.Show(this);
		}

		private void BuildQuickAccessCustomizeMenu()
		{
			if (_ribbonContextMenu != null)
			{
				_ribbonContextMenu.CommandClick -= OnRibbonContextMenuCommandClick;
				_ribbonContextMenu.Dispose();
			}
			_ribbonContextMenu = new MxRibbonContextMenu();
			SeparatorCommand separatorCommand = new SeparatorCommand("cmdTitle");
			separatorCommand.Title = _builtInTexts[RibbonBuiltInText.CustomizeQA];
			separatorCommand.ForegroundStyle.FontBold = true;
			_ribbonContextMenu.Commands.Add(separatorCommand);
			if (_quickAccessCustomizeList.Count > 0)
			{
				foreach (CommandBase item in _quickAccessCustomizeList)
				{
					string text = item.Key;
					if (item is ButtonCommand)
					{
						text = ((ButtonCommand)item).Text;
					}
					else if (item is RibbonGroup)
					{
						text = ((RibbonGroup)item).Text;
					}
					ButtonCommand buttonCommand = new ButtonCommand();
					buttonCommand.Key = item.Key;
					buttonCommand.Text = text;
					buttonCommand.Tag = item;
					buttonCommand.CheckOnClick = true;
					if (_quickAccessCommands.Contains(item))
					{
						buttonCommand.Checked = true;
					}
					_ribbonContextMenu.Commands.Add(buttonCommand);
				}
				SeparatorCommand command = new SeparatorCommand();
				_ribbonContextMenu.Commands.Add(command);
			}
			ButtonCommand moreCommandsButton = new ButtonCommand("cmdMoreCommands");
			moreCommandsButton.Text = _builtInTexts[RibbonBuiltInText.CustomizeMoreCommands];
			if (moreCommandsButton.Text != null && moreCommandsButton.Text.Length > 0)
			{
				_ribbonContextMenu.Commands.Add(moreCommandsButton);
			}
			ButtonCommand positionButton = new ButtonCommand("cmdQAPosition");
			if (_quickAccessPosition == QuickAccessPosition.AboveRibbon)
			{
				positionButton.Text = _builtInTexts[RibbonBuiltInText.ShowBelowRibbon];
			}
			else
			{
				positionButton.Text = _builtInTexts[RibbonBuiltInText.ShowAboveRibbon];
			}
			if (positionButton.Text != null && positionButton.Text.Length > 0)
			{
				_ribbonContextMenu.Commands.Add(positionButton);
			}
			_ribbonContextMenu.CommandClick += OnRibbonContextMenuCommandClick;
		}
	}
}
