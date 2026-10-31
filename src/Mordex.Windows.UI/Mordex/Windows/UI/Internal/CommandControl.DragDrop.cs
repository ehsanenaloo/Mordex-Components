using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
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
		internal void SetInsertMarkIndex(int index)
		{
			if (_insertMarkIndex != index)
			{
				Graphics graphics = CreateGraphics();
				if (_insertMarkIndex >= 0)
				{
					DrawInsertMark(graphics);
				}
				_insertMarkIndex = index;
				if (_insertMarkIndex >= 0)
				{
					DrawInsertMark(graphics);
				}
				graphics.Dispose();
				graphics = null;
			}
		}

		internal void DropCommand(MxCommand command, Point screenPoint, MxCommandCollection uiCommandCollection, bool isCopy)
		{
			Point point = PointToClient(screenPoint);
			CommandLayoutState uiCommandLayoutState = HitTestItemRow(point.Y);
			int insertIndex = -1;
			bool insertAfter = false;
			if (uiCommandLayoutState != null)
			{
				insertAfter = ((point.Y > uiCommandLayoutState._bounds.Top + uiCommandLayoutState._bounds.Height / 2) ? true : false);
				if (uiCommandLayoutState.GetCommand().IsSeparator())
				{
					if (insertAfter)
					{
						point = PointToScreen(new Point(uiCommandLayoutState._bounds.X, uiCommandLayoutState._bounds.Bottom + 1));
						DropCommand(command, point, uiCommandCollection, isCopy);
					}
					else
					{
						point = PointToScreen(new Point(uiCommandLayoutState._bounds.X, uiCommandLayoutState._bounds.Top - 1));
						DropCommand(command, point, uiCommandCollection, isCopy);
					}
					return;
				}
				insertIndex = uiCommandLayoutState._index;
				if (uiCommandLayoutState.GetCommand() != null)
				{
					insertIndex = Commands.IndexOf(uiCommandLayoutState.GetCommand());
				}
			}
			else if (_tearOffBar != null && _tearOffBar._bounds.Contains(point.X, point.Y))
			{
				insertIndex = 0;
				insertAfter = false;
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
			bool isOwnCommand = false;
			if (uiCommandCollection != null)
			{
				isOwnCommand = Commands.Contains(command);
			}
			int oldIndex = -1;
			bool isOnOwnerBar = false;
			if (!isCopy && FindOwnerCommandBar() != null && FindOwnerCommandBar().Commands.Contains(command))
			{
				isOnOwnerBar = true;
			}
			if (isOwnCommand && !isCopy)
			{
				CommandLayoutState draggedItem = _layoutItems.FindCommandItem(command);
				if (draggedItem != null)
				{
					if (draggedItem._index == insertIndex || (draggedItem._index - 1 == insertIndex && insertAfter) || (draggedItem._index + 1 == insertIndex && !insertAfter))
					{
						SetInsertMarkIndex(-1);
						ChildMenu = null;
						return;
					}
					oldIndex = draggedItem._index;
				}
			}
			if (uiCommandCollection != null && !isCopy)
			{
				if (command.IsMergedCommand)
				{
					if (uiCommandCollection == _commands && insertIndex > 0)
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
					uiCommandCollection.Remove(command);
				}
			}
			if (insertIndex >= 0)
			{
				if (insertAfter)
				{
					insertIndex++;
				}
				if (oldIndex >= 0 && oldIndex < insertIndex)
				{
					insertIndex--;
				}
				if (uiCommand.BaseCommand == null)
				{
					uiCommand = new MxCommand(command);
					if (command.CommandManager != null && command.CommandManager != GetCommandManagerOwner())
					{
						uiCommand.MergeType = CommandMergeType.Add;
						uiCommand.UpdateMergeState(command.CommandManager.Id, true);
					}
				}
				Commands.Insert(insertIndex, uiCommand);
				uiCommand.UpdatePosition(insertIndex);
				Graphics graphics = CreateGraphics();
				LayoutMenu(graphics);
				graphics.Dispose();
				graphics = null;
				SetCustomizeCommand(uiCommand);
			}
			SetInsertMarkIndex(-1);
			ChildMenu = null;
			if (isOnOwnerBar && FindOwnerCommandBar() != null)
			{
				FindOwnerCommandBar().SetChildMenu(null);
			}
		}

		internal void ClearInsertMarks()
		{
			SetInsertMarkIndex(-1);
			if (ChildMenu != null)
			{
				ChildMenu.ClearInsertMarks();
			}
		}

		internal void DragOverCommand(MxCommand command, Point screenPoint, MxCommandCollection uiCommandCollection, bool isCopy)
		{
			if (FindOwnerCommandBar() != null)
			{
				FindOwnerCommandBar().SetDropIndex(-1);
			}
			if (_owner is CommandControl)
			{
				CommandControl uiCommandControl = (CommandControl)_owner;
				uiCommandControl.SetInsertMarkIndex(-1);
			}
			Point point = PointToClient(screenPoint);
			CommandLayoutState uiCommandLayoutState = HitTestItemRow(point.Y);
			int insertIndex = -1;
			bool insertAfter = false;
			bool isOwnCommand = false;
			if (uiCommandCollection != null)
			{
				isOwnCommand = Commands.Contains(command);
			}
			if (uiCommandLayoutState != null)
			{
				insertAfter = ((point.Y > uiCommandLayoutState._bounds.Top + uiCommandLayoutState._bounds.Height / 2) ? true : false);
				if (uiCommandLayoutState.GetCommand().IsSeparator())
				{
					if (insertAfter)
					{
						point = PointToScreen(new Point(uiCommandLayoutState._bounds.X, uiCommandLayoutState._bounds.Bottom + 1));
						DragOverCommand(command, point, uiCommandCollection, isCopy);
					}
					else
					{
						point = PointToScreen(new Point(uiCommandLayoutState._bounds.X, uiCommandLayoutState._bounds.Top - 1));
						DragOverCommand(command, point, uiCommandCollection, isCopy);
					}
					return;
				}
				insertIndex = uiCommandLayoutState._index;
			}
			else if (_tearOffBar != null && _tearOffBar._bounds.Contains(point.X, point.Y))
			{
				insertIndex = 0;
				insertAfter = false;
			}
			if (_insertMarkAfter != insertAfter)
			{
				if (_insertMarkIndex >= 0)
				{
					Graphics graphics = CreateGraphics();
					DrawInsertMark(graphics);
					_insertMarkIndex = -1;
					graphics.Dispose();
					graphics = null;
				}
				_insertMarkAfter = insertAfter;
			}
			if (isOwnCommand && !isCopy)
			{
				CommandLayoutState draggedItem = _layoutItems.FindCommandItem(command);
				if (draggedItem != null)
				{
					if (draggedItem._index == insertIndex || (draggedItem._index - 1 == insertIndex && insertAfter) || (draggedItem._index + 1 == insertIndex && !insertAfter))
					{
						insertIndex = -1;
					}
					if (draggedItem == uiCommandLayoutState)
					{
						uiCommandLayoutState = null;
					}
				}
			}
			SetInsertMarkIndex(insertIndex);
			if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand().IsParent && command != uiCommandLayoutState.GetCommand().BaseCommand && command.BaseCommand != uiCommandLayoutState.GetCommand().BaseCommand)
			{
				OpenChildMenu(uiCommandLayoutState, false);
			}
			else
			{
				ChildMenu = null;
			}
		}

		internal void SelectCustomizeCommandAt(Point point)
		{
			if (FindOwnerCommandBar() != null)
			{
				FindOwnerCommandBar().SetCustomizeSelectedCommand(null);
			}
			if (ChildMenu != null)
			{
				ChildMenu = null;
			}
			CommandLayoutState uiCommandLayoutState = HitTestItemRow(PointToClient(point).Y);
			if (uiCommandLayoutState != null && !uiCommandLayoutState.GetCommand().IsSeparator())
			{
				_customizeCommand = null;
				SetCustomizeCommand(uiCommandLayoutState.GetCommand());
				ChildMenu = null;
				GetCommandManagerOwner().ShowCommandMenu(Point.Empty, 0);
			}
		}

		internal CommandLayoutState BeginCommandDrag(Point point)
		{
			if (_owner is CommandControl)
			{
				CommandControl uiCommandControl = (CommandControl)_owner;
				uiCommandControl.SetCustomizeCommand(null);
			}
			if (ChildMenu != null)
			{
				_childMenu.SetCustomizeCommand(null);
			}
			CommandLayoutState uiCommandLayoutState = HitTestItemRow(PointToClient(point).Y);
			if (uiCommandLayoutState != null && !uiCommandLayoutState.GetCommand().IsSeparator())
			{
				SetCustomizeCommand(uiCommandLayoutState.GetCommand());
				Rectangle bounds = uiCommandLayoutState._bounds;
				bounds.Location = PointToScreen(bounds.Location);
				CommandDragState uiCommandCollectionLayoutState = new CommandDragState(FindOwnerCommandBar().CommandManager, uiCommandLayoutState.GetCommand(), Commands, bounds);
				uiCommandCollectionLayoutState.RunDragLoop();
				return uiCommandLayoutState;
			}
			return null;
		}

		public MxCommand GetCustomizeCommand()
		{
			return _customizeCommand;
		}

		public void SetCustomizeCommand(MxCommand uiCommandControlUICommand)
		{
			if (_customizeCommand != uiCommandControlUICommand)
			{
				_customizeCommand = uiCommandControlUICommand;
				if (_customizeCommand != null)
				{
					if (FindOwnerCommandBar().CommandManager != null)
					{
						FindOwnerCommandBar().CommandManager.SetActiveElement(this);
					}
					if (_customizeCommand.IsParent && !base.IsDisposed)
					{
						OpenChildMenu(GetCustomizeItem(), false);
					}
					else
					{
						ChildMenu = null;
					}
				}
				GetCommandManagerOwner().SetSelectedCommand(uiCommandControlUICommand);
				Invalidate();
			}
			else if (_customizeCommand != null)
			{
				if (_childMenu != null)
				{
					ChildMenu = null;
				}
				else if (_customizeCommand.IsParent)
				{
					OpenChildMenu(GetCustomizeItem(), false);
				}
			}
		}

		internal CommandLayoutState GetCustomizeItem()
		{
			if (_customizeCommand != null)
			{
				return _layoutItems.FindCommandItem(_customizeCommand);
			}
			return null;
		}
	}
}
