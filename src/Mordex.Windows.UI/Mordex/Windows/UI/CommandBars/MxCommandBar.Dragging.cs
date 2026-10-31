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
		internal void SetDropIndex(int dropIndex)
		{
			if (_dropIndex != dropIndex)
			{
				if (_dropIndex >= 0)
				{
					DrawDropMarker();
				}
				_dropIndex = dropIndex;
				if (_dropIndex >= 0)
				{
					DrawDropMarker();
				}
			}
		}

		internal bool ContainsScreenPoint(Point point)
		{
			if (!base.Visible)
			{
				return false;
			}
			Point pt = PointToClient(point);
			if (base.ClientRectangle.Contains(pt))
			{
				return true;
			}
			if (GetChildMenu() != null)
			{
				return GetChildMenu().FindMenuAt(point) != null;
			}
			return false;
		}

		internal void ClearDropMarker()
		{
			SetDropIndex(-1);
			if (GetChildMenu() != null)
			{
				GetChildMenu().ClearInsertMarks();
			}
		}

		private int GetVisibleIndex(MxCommand uiCommand)
		{
			int index = 0;
			foreach (MxCommand item in _commands)
			{
				if (item == uiCommand)
				{
					return index;
				}
				if (item.IsVisible)
				{
					index++;
				}
			}
			return -1;
		}

		private int GetDropIndex(CommandLayoutState uiCommandLayoutState, Point point, ref bool insertAfter, bool useAbsoluteIndex)
		{
			int index = -1;
			if (uiCommandLayoutState != null)
			{
				if (_isHorizontal)
				{
					if (point.X > uiCommandLayoutState._bounds.Left + uiCommandLayoutState._bounds.Width / 2)
					{
						insertAfter = true;
					}
					else
					{
						insertAfter = false;
					}
				}
				else if (point.Y > uiCommandLayoutState._bounds.Top + uiCommandLayoutState._bounds.Height / 2)
				{
					insertAfter = true;
				}
				else
				{
					insertAfter = false;
				}
				if (uiCommandLayoutState.GetCommand().IsSeparator())
				{
					index = uiCommandLayoutState._index;
					if (uiCommandLayoutState.GetCommand() != null)
					{
						index = ((!useAbsoluteIndex) ? GetVisibleIndex(uiCommandLayoutState.GetCommand()) : Commands.IndexOf(uiCommandLayoutState.GetCommand()));
					}
					if (insertAfter)
					{
						index++;
						insertAfter = false;
					}
					else
					{
						index--;
						insertAfter = true;
					}
				}
				else
				{
					index = uiCommandLayoutState._index;
					if (uiCommandLayoutState.GetCommand() != null)
					{
						index = ((!useAbsoluteIndex) ? GetVisibleIndex(uiCommandLayoutState.GetCommand()) : Commands.IndexOf(uiCommandLayoutState.GetCommand()));
					}
				}
			}
			else if (_gripItem != null)
			{
				if (_gripItem._bounds.Contains(point.X, point.Y))
				{
					index = 0;
					insertAfter = false;
				}
			}
			else
			{
				Rectangle clientRectangle = base.ClientRectangle;
				if (_isHorizontal)
				{
					clientRectangle.Width = DropAreaLength;
				}
				else
				{
					clientRectangle.Height = DropAreaLength;
				}
				if (clientRectangle.Contains(point.X, point.Y))
				{
					index = 0;
					insertAfter = false;
				}
			}
			if (_customizeItem != null && index < 0 && _customizeItem._bounds.Contains(point.X, point.Y))
			{
				index = Commands.Count - 1;
				insertAfter = true;
			}
			return index;
		}

		internal void DragOverCommand(MxCommand command, Point screenPoint, MxCommandCollection uiCommandCollection, bool isCopy)
		{
			if (GetChildMenu() != null)
			{
				CommandControl uiCommandControl = GetChildMenu().FindMenuAt(screenPoint);
				if (uiCommandControl != null)
				{
					uiCommandControl.DragOverCommand(command, screenPoint, uiCommandCollection, isCopy);
					return;
				}
			}
			bool isOwnCommand = false;
			if (uiCommandCollection != null)
			{
				isOwnCommand = Commands.Contains(command);
			}
			Point point = PointToClient(screenPoint);
			CommandLayoutState uiCommandLayoutState = HitTestItem(point.X, point.Y);
			bool isAfter = false;
			int dropIndex = GetDropIndex(uiCommandLayoutState, point, ref isAfter, false);
			if (_isDropAfterItem != isAfter)
			{
				if (_dropIndex >= 0)
				{
					DrawDropMarker();
					_dropIndex = -1;
				}
				_isDropAfterItem = isAfter;
			}
			if (isOwnCommand && !isCopy)
			{
				CommandLayoutState draggedItem = _layoutItems.FindCommandItem(command);
				if (draggedItem != null)
				{
					if (draggedItem._index == dropIndex || (draggedItem._index - 1 == dropIndex && isAfter) || (draggedItem._index + 1 == dropIndex && !isAfter))
					{
						dropIndex = -1;
					}
					if (draggedItem == uiCommandLayoutState)
					{
						uiCommandLayoutState = null;
					}
				}
			}
			SetDropIndex(dropIndex);
			if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand().IsParent && command != uiCommandLayoutState.GetCommand().BaseCommand && command.BaseCommand != uiCommandLayoutState.GetCommand().BaseCommand)
			{
				if (uiCommandLayoutState.GetCommand().AllowCustomizeCommands != InheritableBoolean.False && (GetChildMenu() == null || GetChildMenu()._parentItem != uiCommandLayoutState))
				{
					PopupSettings baseSettings = BuildPopupSettings(uiCommandLayoutState.GetCommand());
					CommandControl subMenu = new CommandControl(uiCommandLayoutState.GetCommand().Commands, uiCommandLayoutState, this, baseSettings, holdMessages: false);
					SetChildMenu(subMenu);
					subMenu.ShowAt(PointToScreen(uiCommandLayoutState._bounds.Location));
					Invalidate();
				}
			}
			else if (GetChildMenu() != null)
			{
				SetChildMenu(null);
			}
		}

		internal void DropCommand(MxCommand command, Point screenPoint, MxCommandCollection uiCommandCollection, bool isCopy)
		{
			if (GetChildMenu() != null)
			{
				CommandControl uiCommandControl = GetChildMenu().FindMenuAt(screenPoint);
				if (uiCommandControl != null)
				{
					uiCommandControl.DropCommand(command, screenPoint, uiCommandCollection, isCopy);
					return;
				}
			}
			MxCommand uiCommand = command;
			if (isCopy)
			{
				uiCommand = command.Clone();
				if (command.IsMergedCommand)
				{
					uiCommand._mergeType = CommandMergeType.Add;
				}
			}
			Point point = PointToClient(screenPoint);
			CommandLayoutState uiCommandLayoutState = HitTestItem(point.X, point.Y);
			bool isAfter = false;
			int dropIndex = GetDropIndex(uiCommandLayoutState, point, ref isAfter, true);
			bool isOwnCommand = false;
			if (uiCommandCollection != null)
			{
				isOwnCommand = Commands.Contains(command);
			}
			int oldIndex = -1;
			if (isOwnCommand && !isCopy)
			{
				CommandLayoutState draggedItem = _layoutItems.FindCommandItem(command);
				if (draggedItem != null)
				{
					if (draggedItem._index == dropIndex || (draggedItem._index - 1 == dropIndex && isAfter) || (draggedItem._index + 1 == dropIndex && !isAfter))
					{
						SetDropIndex(-1);
						SetChildMenu(null);
						return;
					}
					oldIndex = draggedItem._index;
				}
			}
			if (uiCommandCollection != null && !isCopy)
			{
				if (command.IsMergedCommand)
				{
					if (uiCommandCollection == _commands && dropIndex >= 0)
					{
						if (_commands._mergedCommands != null && _commands._mergedCommands.Contains(command))
						{
							_commands._mergedCommands.Remove(command);
						}
						_commands.Remove(command);
					}
					else
					{
						uiCommand = command.Clone();
						uiCommand._mergeType = CommandMergeType.Add;
						InheritableBoolean visible = command._visible;
						command.Visible = InheritableBoolean.False;
						uiCommand._visible = visible;
						uiCommand.SetMergeTarget(null);
					}
				}
				if (uiCommandCollection.Contains(command))
				{
					if (uiCommandCollection._owner is MxCommandBar)
					{
						if (!((MxCommandBar)uiCommandCollection._owner).GetIsMergedCommandBar())
						{
							uiCommandCollection.Remove(command);
						}
					}
					else
					{
						uiCommandCollection.Remove(command);
					}
				}
			}
			if (dropIndex >= 0)
			{
				if (isAfter)
				{
					dropIndex++;
				}
				if (oldIndex >= 0 && oldIndex < dropIndex)
				{
					dropIndex--;
				}
				if (uiCommand.BaseCommand == null)
				{
					uiCommand = new MxCommand(command);
					if (command.CommandManager != null && command.CommandManager != CommandManager)
					{
						uiCommand.MergeType = CommandMergeType.Add;
						uiCommand.UpdateMergeState(command.CommandManager.Id, true);
					}
				}
				Commands.Insert(dropIndex, uiCommand);
				uiCommand.UpdatePosition(dropIndex);
				RecalculateLayout(true);
				SetCustomizeSelectedCommand(uiCommand);
			}
			else
			{
				CommandManager.SetActiveElement(null);
			}
			SetDropIndex(-1);
			SetChildMenu(null);
		}

		internal void SelectCustomizeCommandAt(Point screenPoint)
		{
			if (GetChildMenu() != null)
			{
				CommandControl uiCommandControl = GetChildMenu().FindMenuAt(screenPoint);
				if (uiCommandControl != null)
				{
					uiCommandControl.SelectCustomizeCommandAt(screenPoint);
					return;
				}
				SetChildMenu(null);
			}
			Point point = PointToClient(screenPoint);
			CommandLayoutState uiCommandLayoutState = HitTestItem(point.X, point.Y);
			if (uiCommandLayoutState != null && !uiCommandLayoutState.GetCommand().IsSeparator())
			{
				bool isParent = uiCommandLayoutState.HasDropDown();
				uiCommandLayoutState.SetHasDropDown(false);
				SetCustomizeSelectedCommand(uiCommandLayoutState.GetCommand());
				uiCommandLayoutState.SetHasDropDown(isParent);
				Invalidate();
				Update();
				CommandManager.ShowCommandMenu(Point.Empty, 0);
			}
		}

		internal CommandLayoutState BeginCustomizeDrag(Point point)
		{
			Point ptOffset = PointToClient(point);
			if (_gripItem != null && _gripItem._bounds.Contains(ptOffset.X, ptOffset.Y))
			{
				_messageFilter = CommandBarMessageFilter.StartMove(this, ptOffset, null);
				return null;
			}
			if (GetChildMenu() != null)
			{
				CommandControl uiCommandControl = GetChildMenu().FindMenuAt(point);
				if (uiCommandControl != null)
				{
					return uiCommandControl.BeginCommandDrag(point);
				}
			}
			CommandLayoutState uiCommandLayoutState = HitTestItem(ptOffset.X, ptOffset.Y);
			if (uiCommandLayoutState != null && !uiCommandLayoutState.GetCommand().IsSeparator())
			{
				CommandManager.SetActiveElement(this);
				SetCustomizeSelectedCommand(uiCommandLayoutState.GetCommand());
				Rectangle bounds = uiCommandLayoutState._bounds;
				bounds.Location = PointToScreen(bounds.Location);
				Invalidate();
				CommandDragState uiCommandCollectionLayoutState = new CommandDragState(_commandManager, uiCommandLayoutState.GetCommand(), Commands, bounds);
				uiCommandCollectionLayoutState.RunDragLoop();
			}
			return null;
		}

		internal MxCommand GetCustomizeSelectedCommand()
		{
			return _customizeSelectedCommand;
		}

		internal void SetCustomizeSelectedCommand(MxCommand command)
		{
			_customizeSelectedCommand = command;
			if (CommandManager != null)
			{
				if (_customizeSelectedCommand != null)
				{
					CommandManager.SetActiveElement(this);
					if (GetChildMenu() == null)
					{
						CommandLayoutState uiCommandLayoutState = _layoutItems.FindCommandItem(_customizeSelectedCommand);
						if (uiCommandLayoutState != null && uiCommandLayoutState.HasDropDown())
						{
							if (uiCommandLayoutState.GetCommand() != null && uiCommandLayoutState.GetCommand().AllowCustomizeCommands != InheritableBoolean.False)
							{
								PopupSettings baseSettings = BuildPopupSettings(_customizeSelectedCommand);
								CommandControl uiCommandControl = new CommandControl(_customizeSelectedCommand.Commands, uiCommandLayoutState, this, baseSettings, holdMessages: false);
								SetChildMenu(uiCommandControl);
								CommandManager.SetActiveElement(uiCommandControl);
								uiCommandControl.ShowAt(PointToScreen(uiCommandLayoutState._bounds.Location));
							}
						}
						else
						{
							SetChildMenu(null);
						}
					}
					else
					{
						SetChildMenu(null);
					}
				}
				else if (CommandManager.GetActiveElement() != GetChildMenu())
				{
					SetChildMenu(null);
				}
				CommandManager.SetSelectedCommand(command);
			}
			Invalidate();
		}

		internal CommandLayoutState GetCustomizeSelectedItem()
		{
			if (_customizeSelectedCommand != null)
			{
				return _layoutItems.FindCommandItem(_customizeSelectedCommand);
			}
			return null;
		}
	}
}
