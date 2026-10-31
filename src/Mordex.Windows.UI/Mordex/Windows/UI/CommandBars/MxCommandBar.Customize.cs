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
		internal bool ShouldShowAddRemoveButton()
		{
			if (_showAddRemoveButton == InheritableBoolean.Default)
			{
				if (CommandManager.ShowAddRemoveButton == InheritableBoolean.Default)
				{
					return true;
				}
				if (CommandManager.ShowAddRemoveButton == InheritableBoolean.False)
				{
					return false;
				}
			}
			else if (_showAddRemoveButton == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool ShouldShowCustomizeButton()
		{
			if (_showCustomizeButton == InheritableBoolean.Default)
			{
				if (CommandManager.ShowCustomizeButton == InheritableBoolean.Default)
				{
					if (_commandBarType == CommandBarType.ToolBar)
					{
						return true;
					}
				}
				else if (CommandManager.ShowCustomizeButton == InheritableBoolean.True)
				{
					return true;
				}
			}
			else if (_showCustomizeButton == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool CanCustomize()
		{
			if (_allowCustomize == InheritableBoolean.Default)
			{
				if (CommandManager.AllowCustomize == InheritableBoolean.Default)
				{
					return true;
				}
				if (CommandManager.AllowCustomize == InheritableBoolean.True)
				{
					return true;
				}
			}
			else if (_allowCustomize == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool CanClose()
		{
			if (_allowClose == InheritableBoolean.Default)
			{
				if (CommandManager.AllowClose == InheritableBoolean.Default)
				{
					return true;
				}
				if (CommandManager.AllowClose == InheritableBoolean.True)
				{
					return true;
				}
			}
			else if (_allowClose == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal MxCommand GetCustomizeCommand()
		{
			if (_customizeCommand == null)
			{
				_customizeCommand = new MxCommand();
			}
			if (CommandManager != null)
			{
				_customizeCommand.ToolTipText = CommandManager.BuiltInTexts[CommandsBuiltInText.CustomizeButtonToolTip];
				_customizeCommand.Text = _customizeCommand.ToolTipText;
			}
			return _customizeCommand;
		}

		private void BuildCustomizeMenu(CommandLayoutState uiCommandLayoutState)
		{
			_customizeCommand = new MxCommand();
			if (CommandManager != null)
			{
				_customizeCommand.ToolTipText = CommandManager.BuiltInTexts[CommandsBuiltInText.CustomizeButtonToolTip];
			}
			MxCommand uiCommand = new MxCommand("AddorRemove", "&Add or Remove Buttons");
			if (!ShouldShowAddRemoveButton())
			{
				uiCommand.IsVisible = false;
			}
			uiCommand.Text = CommandManager.BuiltInTexts[CommandsBuiltInText.AddRemoveButtons];
			_customizeCommand.Commands.Add(uiCommand);
			if (GetHostRow() != null)
			{
				foreach (MxCommandBar item in GetHostRow().GetBars())
				{
					MxCommand barCommand = new MxCommand(item.Key, item.Text);
					barCommand.SetAddRemoveButtonsItem(true);
					AddToggleItems(barCommand, item);
					if (!item.CanCustomize())
					{
						barCommand.Enabled = InheritableBoolean.False;
					}
					uiCommand.Commands.Add(barCommand);
				}
			}
			else
			{
				MxCommand thisBarCommand = new MxCommand(Key, Text);
				thisBarCommand.SetAddRemoveButtonsItem(true);
				AddToggleItems(thisBarCommand, this);
				uiCommand.Commands.Add(thisBarCommand);
			}
			MxCommand customizeCommand = new MxCommand("Customize", "&Customize...");
			customizeCommand.SetBuiltInId("Customize");
			customizeCommand.Text = CommandManager.BuiltInTexts[CommandsBuiltInText.Customize];
			MxCommand customizeItem = new MxCommand(customizeCommand);
			customizeItem.Click += OnCustomizeMenuClick;
			uiCommand.Commands.Add(customizeItem);
			if (!CanCustomize() || CommandManager.AllowCustomize == InheritableBoolean.False)
			{
				customizeCommand.Visible = InheritableBoolean.False;
			}
			uiCommandLayoutState.SetCommand(customizeCommand);
		}

		private void AddToggleItems(MxCommand uiCommand, MxCommandBar uiCommandBar)
		{
			foreach (MxCommand command in uiCommandBar.Commands)
			{
				if (!command.IsHiddenByMerge() && !command.IsSeparator())
				{
					MxCommand toggleCommand = new MxCommand();
					toggleCommand.Image = command.Image;
					toggleCommand.HotImageIndex = command.HotImageIndex;
					toggleCommand.HotImageKey = command.HotImageKey;
					toggleCommand.ImageIndex = command.ImageIndex;
					toggleCommand.ImageKey = command.ImageKey;
					toggleCommand.SelectedImageIndex = command.SelectedImageIndex;
					toggleCommand.SelectedImageKey = command.SelectedImageKey;
					toggleCommand.LargeHotImageIndex = command.LargeHotImageIndex;
					toggleCommand.LargeHotImageKey = command.LargeHotImageKey;
					toggleCommand.LargeImageIndex = command.LargeImageIndex;
					toggleCommand.LargeImageKey = command.LargeImageKey;
					toggleCommand.LargeSelectedImageIndex = command.LargeSelectedImageIndex;
					toggleCommand.LargeSelectedImageKey = command.LargeSelectedImageKey;
					toggleCommand.Icon = command.Icon;
					toggleCommand.Text = command.Text;
					toggleCommand.Shortcut = command.Shortcut;
					toggleCommand.IsChecked = command.IsEffectivelyVisible();
					toggleCommand.SetBuiltInId("CommandBarItem");
					toggleCommand.Tag = command;
					toggleCommand.UpdateMergeState(command._guid, command.IsMergedCommand);
					toggleCommand.Click += OnCustomizeMenuClick;
					uiCommand.Commands.Add(toggleCommand);
				}
			}
		}

		private void OnCustomizeMenuClick(object sender, CommandEventArgs eventArgs)
		{
			if (eventArgs.Command.GetBuiltInId() == "CommandBarItem")
			{
				MxCommand uiCommand = (MxCommand)eventArgs.Command.Tag;
				uiCommand.SetVisible(!uiCommand.IsEffectivelyVisible());
				eventArgs.Command.IsChecked = !eventArgs.Command.IsChecked;
			}
			else if (eventArgs.Command.GetBuiltInId() == "Reset")
			{
				SetChildMenu(null);
			}
			else if (eventArgs.Command.GetBuiltInId() == "Customize")
			{
				SetChildMenu(null);
				CommandManager.ShowCustomizeDialog();
			}
			else
			{
				SetChildMenu(null);
			}
		}

		internal bool CanMove()
		{
			if (LockCommandBar == InheritableBoolean.False)
			{
				return true;
			}
			if (LockCommandBar == InheritableBoolean.True)
			{
				return false;
			}
			return !CommandManager.LockCommandBars;
		}
	}
}
