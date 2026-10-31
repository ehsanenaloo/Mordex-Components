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
		internal void BindCommands()
		{
			if (_isInitializing || _commandManager == null)
			{
				return;
			}
			ResolveBaseCommands();
			foreach (MxCommand item in _commands)
			{
				if (item.BaseCommand == null)
				{
					throw new InvalidOperationException("The commands in the CommandCollection must have a BaseCommand located in the UICommandManager");
				}
			}
		}

		private void ResolveBaseCommands()
		{
			for (int i = 0; i < _commands.Count; i++)
			{
				MxCommand uiCommand = _commands[i];
				if (uiCommand.BaseCommand != null)
				{
					if (uiCommand.BaseCommand.CommandManager == null && CommandManager != null)
					{
						CommandManager.Commands.Add(uiCommand.BaseCommand);
					}
				}
				else if (uiCommand.CommandType == CommandType.Separator)
				{
					if (CommandManager != null)
					{
						uiCommand.SetBaseCommand(CommandManager.GetSeparatorCommand());
					}
				}
				else if (uiCommand.Key.Length > 0 && CommandManager != null)
				{
					MxCommand baseCommand = CommandManager.Commands.FindByKey(uiCommand.Key);
					if (baseCommand != null)
					{
						uiCommand.SetBaseCommand(baseCommand);
					}
				}
			}
		}

		internal bool IsEditableInDesigner()
		{
			if (base.Container == null)
			{
				return true;
			}
			IInheritanceService inheritanceService = (IInheritanceService)GetService(typeof(IInheritanceService));
			if (inheritanceService != null)
			{
				if (inheritanceService.GetInheritanceAttribute(this) == InheritanceAttribute.InheritedReadOnly)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		internal bool IsNotInheritedInDesigner()
		{
			if (base.Container == null)
			{
				return true;
			}
			IInheritanceService inheritanceService = (IInheritanceService)GetService(typeof(IInheritanceService));
			if (inheritanceService != null)
			{
				if (inheritanceService.GetInheritanceAttribute(this) == InheritanceAttribute.NotInherited)
				{
					return true;
				}
				return false;
			}
			return false;
		}

		internal void CopyFromBaseBar(MxCommandBar uiCommandBar, MxCommandManager uiCommandManager)
		{
			_isLoadingLayout = true;
			_isMergedCommandBar = uiCommandBar._isMergedCommandBar;
			_textAlign = uiCommandBar._textAlign;
			_view = uiCommandBar._view;
			_allowCustomize = uiCommandBar._allowCustomize;
			_showCustomizeButton = uiCommandBar._showCustomizeButton;
			_animation = uiCommandBar._animation;
			_allowDelete = uiCommandBar._allowDelete;
			_allowRename = uiCommandBar._allowRename;
			_allowMerge = uiCommandBar._allowMerge;
			_commandBarType = uiCommandBar._commandBarType;
			SetParentID(uiCommandBar._parentID);
			if (_commands != uiCommandBar._commands)
			{
				RemoveHostedControls();
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					MxCommand mergedCommand = uiCommandBar.Commands.FindByKey(uiCommand.Key);
					if (mergedCommand != null)
					{
						if (!mergedCommand.IsSeparator())
						{
							MxCommand managerCommand = uiCommandManager.Commands.FindByKey(uiCommand.Key);
							if (managerCommand != null)
							{
								mergedCommand.SetBaseCommand(managerCommand);
							}
							else
							{
								mergedCommand.SetBaseCommand(uiCommand.BaseCommand);
							}
							_commands.ReplaceCommand(uiCommand, mergedCommand);
						}
						else
						{
							uiCommand.UpdateMergeState(CommandManager.Id, true);
						}
					}
					else if (uiCommand.IsSeparator())
					{
						uiCommand.UpdateMergeState(CommandManager.Id, true);
					}
				}
				_commands.AssignCommandOrder();
				_commands._mergedCommands = uiCommandBar._commands._mergedCommands;
				if (_commands._mergedCommands != null)
				{
					_commands.MergeCommands(null, uiCommandManager);
				}
				else
				{
					_commands.SortCommands();
				}
			}
			_commandsStateStyles = uiCommandBar._commandsStateStyles;
			if (_commandsStateStyles != null)
			{
				_commandsStateStyles._owner = this;
			}
			_commandsStyle = uiCommandBar._commandsStyle;
			_lockCommandBar = uiCommandBar._lockCommandBar;
			_isClosed = uiCommandBar.IsClosed();
			_floatingLocation = uiCommandBar._floatingLocation;
			_floatingSize = uiCommandBar._floatingSize;
			_formatStyle = uiCommandBar._formatStyle;
			if (_formatStyle != null)
			{
				_formatStyle._owner = this;
			}
			_forceDockStyleUpdate = true;
			_rowIndex = -1;
			DockStyle = uiCommandBar._dockStyle;
			_position = uiCommandBar._position;
			SetMergedLocation(uiCommandBar.GetMergedLocation());
			if (Location != uiCommandBar.GetMergedLocation())
			{
				Location = uiCommandBar.GetMergedLocation();
			}
			RowIndex = uiCommandBar.MergeRowOrder;
			if (CommandManager != null)
			{
				RemoveDockedPlaceholder();
			}
			_dockedDockStyle = uiCommandBar._dockedDockStyle;
			_dockedRowIndex = uiCommandBar._dockedRowIndex;
			_dockedLocation = uiCommandBar._dockedLocation;
			ReserveDockedRow();
			_position = uiCommandBar._position;
			_popupSettings = uiCommandBar._popupSettings;
			if (_popupSettings != null)
			{
				_popupSettings._owner = this;
			}
			_popupView = uiCommandBar._popupView;
			_showToolTips = uiCommandBar._showToolTips;
			_commandBarWrappable = uiCommandBar._commandBarWrappable;
			_fullRow = uiCommandBar._fullRow;
			_isLoadingLayout = false;
			base.Visible = uiCommandBar.Visible;
			InvalidateLayout();
		}

		private void ReserveDockedRow()
		{
			if (_dockedRowIndex == -1 || CommandManager == null)
			{
				return;
			}
			switch (_dockedDockStyle)
			{
			case BarsDockStyle.Top:
				CommandManager.TopRebar.AddBarState(this, _dockedRowIndex);
				break;
			case BarsDockStyle.Left:
				CommandManager.LeftRebar.AddBarState(this, _dockedRowIndex);
				break;
			case BarsDockStyle.Right:
				CommandManager.RightRebar.AddBarState(this, _dockedRowIndex);
				break;
			case BarsDockStyle.Bottom:
				CommandManager.BottomRebar.AddBarState(this, _dockedRowIndex);
				break;
			}
		}

		internal int GetBarIndex()
		{
			if (CommandManager != null)
			{
				return CommandManager.CommandBars.IndexOf(this);
			}
			return _detachedIndex;
		}
	}
}
