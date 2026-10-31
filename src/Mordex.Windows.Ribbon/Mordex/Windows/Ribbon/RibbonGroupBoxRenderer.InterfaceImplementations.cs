using System.Collections.Generic;
using System.Drawing;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonGroupBoxRenderer
	{
		bool ISupportsDesignerMouse.DesignerMouseIn
		{
			get
			{
				return IsDesignerMouseIn();
			}
			set
			{
				SetDesignerMouseIn(value);
			}
		}

		bool IKeyboardNavigable.SelectLastItem()
		{
			for (int index = _childRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _childRenderers[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (thisValue is IKeyboardNavigable && ((IKeyboardNavigable)thisValue).SelectLastItem())
					{
						return true;
					}
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			return false;
		}

		bool IKeyboardNavigable.SelectFirstItem()
		{
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (!(item.GetCommand() is SeparatorCommand))
				{
					if (item is IKeyboardNavigable && ((IKeyboardNavigable)item).SelectFirstItem())
					{
						return true;
					}
					GetRenderHost().ActiveInputItem = item;
					return true;
				}
			}
			return false;
		}

		bool IKeyboardNavigable.MoveNext(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				if (((IKeyboardNavigable)this).SelectFirstItem())
				{
					return true;
				}
				if (_dialogLauncher != null)
				{
					GetRenderHost().ActiveInputItem = _dialogLauncher;
					return true;
				}
				return false;
			}
			RibbonCommandColumn thisValue = _commandColumns[selectedItem._columnIndex];
			if (thisValue.TryMoveNext(selectedItem))
			{
				return true;
			}
			int rowIndex = 0;
			int nextColumnIndex = selectedItem._columnIndex + 1;
			return TryFocusItemInColumn(nextColumnIndex, rowIndex, true);
		}

		bool IKeyboardNavigable.MovePrevious(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				if (((IKeyboardNavigable)this).SelectLastItem())
				{
					return true;
				}
				if (_dialogLauncher != null)
				{
					GetRenderHost().ActiveInputItem = _dialogLauncher;
					return true;
				}
				return false;
			}
			RibbonCommandColumn thisValue = _commandColumns[selectedItem._columnIndex];
			if (thisValue.TryMovePrevious(selectedItem))
			{
				return true;
			}
			int rowIndex = 0;
			int previousColumnIndex = selectedItem._columnIndex - 1;
			return TryFocusItemInColumn(previousColumnIndex, rowIndex, false);
		}

		bool IKeyboardNavigable.MoveRight(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			RibbonCommandColumn thisValue = _commandColumns[selectedItem._columnIndex];
			if (thisValue.TryMoveRight(selectedItem))
			{
				return true;
			}
			int rowIndex = selectedItem._rowInColumn;
			int nextColumnIndex = selectedItem._columnIndex + 1;
			if (selectedItem.CommandType == CommandType.LargeButton)
			{
				rowIndex = GetRenderHost().MaxItemRows / 2;
			}
			return TryFocusItemInColumn(nextColumnIndex, rowIndex, true);
		}

		bool IKeyboardNavigable.MoveLeft(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectLastItem();
			}
			RibbonCommandColumn thisValue = _commandColumns[selectedItem._columnIndex];
			if (thisValue.TryMoveLeft(selectedItem))
			{
				return true;
			}
			int rowIndex = selectedItem._rowInColumn;
			int previousColumnIndex = selectedItem._columnIndex - 1;
			if (selectedItem.CommandType == CommandType.LargeButton)
			{
				rowIndex = GetRenderHost().MaxItemRows / 2;
			}
			return TryFocusItemInColumn(previousColumnIndex, rowIndex, false);
		}

		bool IKeyboardNavigable.MoveDown(CommandBaseRenderer selectedItem)
		{
			int columnIndex = -1;
			if (selectedItem == null)
			{
				columnIndex = 0;
			}
			else
			{
				if (selectedItem == _dialogLauncher)
				{
					if (_commandBaseRendererRibbon._quickAccessToolbarRenderer != null && _commandBaseRendererRibbon.QuickAccessPosition == QuickAccessPosition.BelowRibbon)
					{
						return _commandBaseRendererRibbon._quickAccessToolbarRenderer.TrySelectItemAtX(_bounds.X + _bounds.Width / 2);
					}
					return false;
				}
				if (selectedItem._layoutItemOwner != this)
				{
					if (selectedItem._layoutItemOwner is IKeyboardNavigable)
					{
						return ((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveDown(selectedItem);
					}
					selectedItem = selectedItem._layoutItemOwner;
					while (selectedItem != null && selectedItem._layoutItemOwner != this)
					{
						selectedItem = selectedItem._layoutItemOwner;
					}
				}
				if (selectedItem == null)
				{
					return false;
				}
				columnIndex = selectedItem._columnIndex;
			}
			RibbonCommandColumn column = null;
			if (columnIndex >= 0 && _commandColumns.Count > columnIndex)
			{
				column = _commandColumns[columnIndex];
			}
			if (column != null)
			{
				bool handled = column.TryMoveDown(selectedItem);
				if (!handled)
				{
					if (_dialogLauncher != null)
					{
						_commandBaseRendererRibbon.SetKeyboardFocusRenderer(_dialogLauncher);
						handled = true;
					}
					else if (_commandBaseRendererRibbon.QuickAccessPosition == QuickAccessPosition.BelowRibbon && _commandBaseRendererRibbon._quickAccessToolbarRenderer != null)
					{
						handled = _commandBaseRendererRibbon._quickAccessToolbarRenderer.TrySelectItemAtX(_bounds.X + _bounds.Width / 2);
					}
				}
				return handled;
			}
			return false;
		}

		bool IKeyboardNavigable.MoveUp(CommandBaseRenderer selectedItem)
		{
			int columnIndex = -1;
			if (selectedItem == null)
			{
				columnIndex = 0;
				if (_dialogLauncher != null)
				{
					_commandBaseRendererRibbon.SetKeyboardFocusRenderer(_dialogLauncher);
					return true;
				}
			}
			else
			{
				if (selectedItem._layoutItemOwner != this)
				{
					if (selectedItem._layoutItemOwner is IKeyboardNavigable)
					{
						return ((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveUp(selectedItem);
					}
					selectedItem = selectedItem._layoutItemOwner;
					while (selectedItem != null && selectedItem._layoutItemOwner != this)
					{
						selectedItem = selectedItem._layoutItemOwner;
					}
				}
				if (selectedItem == null)
				{
					return false;
				}
				if (selectedItem == _dialogLauncher)
				{
					selectedItem = null;
					columnIndex = 0;
				}
				else
				{
					columnIndex = selectedItem._columnIndex;
				}
			}
			RibbonCommandColumn column = null;
			if (columnIndex >= 0)
			{
				column = _commandColumns[columnIndex];
			}
			return column?.TryMoveUp(selectedItem) ?? false;
		}

		CommandBase IKeyboardNavigable.GetCommandAt(int x, int y)
		{
			if (_bounds.Contains(x, y))
			{
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					if (item._bounds.Contains(x, y))
					{
						if (item is IKeyboardNavigable)
						{
							return ((IKeyboardNavigable)item).GetCommandAt(x, y);
						}
						return item.GetCommand();
					}
				}
				return GetRibbonGroup();
			}
			return null;
		}

		CommandBaseRenderer IKeyboardNavigable.GetCommandLayoutItem(CommandBase command)
		{
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (item.GetCommand() == command)
				{
					return item;
				}
				if (item is IKeyboardNavigable)
				{
					CommandBaseRenderer commandLayoutItem = ((IKeyboardNavigable)item).GetCommandLayoutItem(command);
					if (commandLayoutItem != null)
					{
						return commandLayoutItem;
					}
				}
			}
			return null;
		}

		void IKeyboardNavigable.AddKeyTips(Graphics graphics, TextLayoutRenderer textRenderer, Font font, IList<RibbonKeyTipEntry> keyTips, bool showToolTip)
		{
			bool useCompatibleTextRendering = GetRenderHost().UseCompatibleTextRendering;
			if (_dialogLauncher != null && GetRibbonGroup().DialogButtonKeyTip.Length > 0)
			{
				SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(font, useCompatibleTextRendering, GetRibbonGroup().DialogButtonKeyTip, GetRibbonGroup().Ribbon._colorScheme);
				superTipSettingsWindowMessageHandler.SetCompactMode(true);
				superTipSettingsWindowMessageHandler.SetSticky(true);
				superTipSettingsWindowMessageHandler._isKeyTipEnabled = GetRibbonGroup().IsEnabled;
				if (!_isCollapsed && showToolTip)
				{
					superTipSettingsWindowMessageHandler.ShowAt(GetRenderHost().ParentControl, GetRenderHost().PointToScreen(_dialogLauncher.GetKeyTipLocation(graphics, textRenderer, font, GetRibbonGroup().DialogButtonKeyTip)));
				}
				RibbonKeyTipEntry item = new RibbonKeyTipEntry(GetRibbonGroup().DialogButtonKeyTip, GetRibbonGroup(), _dialogLauncher, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
				keyTips.Add(item);
			}
			foreach (CommandBaseRenderer itemRenderer in _childRenderers)
			{
				if (itemRenderer is IKeyboardNavigable)
				{
					((IKeyboardNavigable)itemRenderer).AddKeyTips(graphics, textRenderer, font, keyTips, !_isCollapsed && showToolTip);
				}
				else
				{
					if (!(itemRenderer.GetCommand() is IKeyTipProvider))
					{
						continue;
					}
					IKeyTipProvider keyTipProvider = (IKeyTipProvider)itemRenderer.GetCommand();
					if (keyTipProvider.HasKeyTip)
					{
						SuperTipSettingsWindowMessageHandler itemKeyTipWindow = new SuperTipSettingsWindowMessageHandler(font, useCompatibleTextRendering, keyTipProvider.KeyTip, GetRibbonGroup().Ribbon._colorScheme);
						itemKeyTipWindow.SetCompactMode(true);
						itemKeyTipWindow.SetSticky(true);
						itemKeyTipWindow._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
						if (!_isCollapsed && showToolTip)
						{
							itemKeyTipWindow.ShowAt(GetRenderHost().ParentControl, GetRenderHost().ParentControl.PointToScreen(itemRenderer.GetKeyTipLocation(graphics, textRenderer, font, keyTipProvider.KeyTip)));
						}
						RibbonKeyTipEntry itemEntry = new RibbonKeyTipEntry(keyTipProvider.KeyTip, itemRenderer.GetCommand(), itemRenderer, itemKeyTipWindow, itemKeyTipWindow._isKeyTipEnabled);
						keyTips.Add(itemEntry);
					}
				}
			}
			if (!_isCollapsed)
			{
				return;
			}
			IKeyTipProvider groupKeyTipProvider = GetRibbonGroup();
			if (groupKeyTipProvider.HasKeyTip)
			{
				SuperTipSettingsWindowMessageHandler groupKeyTipWindow = new SuperTipSettingsWindowMessageHandler(font, useCompatibleTextRendering, groupKeyTipProvider.KeyTip, GetRibbonGroup().Ribbon._colorScheme);
				groupKeyTipWindow.SetCompactMode(true);
				groupKeyTipWindow.SetSticky(true);
				groupKeyTipWindow._isKeyTipEnabled = GetRibbonGroup().IsEnabled;
				if (showToolTip)
				{
					groupKeyTipWindow.ShowAt(GetRenderHost().ParentControl, GetRenderHost().PointToScreen(GetKeyTipLocation(graphics, textRenderer, font, groupKeyTipProvider.KeyTip)));
				}
				RibbonKeyTipEntry groupEntry = new RibbonKeyTipEntry(groupKeyTipProvider.KeyTip, GetRibbonGroup(), this, groupKeyTipWindow, groupKeyTipWindow._isKeyTipEnabled);
				keyTips.Add(groupEntry);
			}
		}

		void IKeyboardNavigable.RemoveControls()
		{
			base.RemoveHostedControls();
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			ShowCollapsedPopup();
		}
	}
}
