using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonCommandColumn
	{
		public IList<CommandBaseRenderer> _items;

		private ICommandRenderHost _owner;

		public CommandType _commandsType;

		private bool _isFullyShrunk;

		private int _columnWidth;

		public RibbonCommandColumn(ICommandRenderHost owner, CommandType commandsType)
		{
			_items = new List<CommandBaseRenderer>();
			_owner = owner;
			_commandsType = commandsType;
		}

		public int GetColumnWidth()
		{
			return _columnWidth;
		}

		public void SetColumnWidth(int ribbonState18Integer)
		{
			_columnWidth = ribbonState18Integer;
		}

		public bool IsFullyShrunk()
		{
			if (_commandsType == CommandType.LargeButton && _isFullyShrunk && _items.Count > 1)
			{
				int groupedItemCount = _items.Count / 3 * 3;
				for (int i = 1; i <= groupedItemCount; i++)
				{
					RibbonButtonRenderer button = (RibbonButtonRenderer)_items[_items.Count - i];
					if (button.SizeStyle == CommandSizeStyle.Small && button._hasImageContent && button._hasTextContent)
					{
						return false;
					}
				}
			}
			return _isFullyShrunk;
		}

		public void SetFullyShrunk(bool isFullyShrunk)
		{
			_isFullyShrunk = isFullyShrunk;
		}

		public Rectangle GetBounds()
		{
			if (_items.Count > 0)
			{
				CommandBaseRenderer thisValue = _items[0];
				int left = thisValue._bounds.Left;
				int top = thisValue._bounds.Top;
				int right = thisValue._bounds.Right;
				int bottom = thisValue._bounds.Bottom;
				for (int i = 1; i < _items.Count; i++)
				{
					thisValue = _items[i];
					Rectangle bounds = thisValue._bounds;
					if (bounds.Top < top)
					{
						top = bounds.Top;
					}
					if (bounds.Left < left)
					{
						left = bounds.Left;
					}
					if (bounds.Bottom > bottom)
					{
						bottom = bounds.Bottom;
					}
					if (bounds.Right > right)
					{
						right = bounds.Right;
					}
				}
				return new Rectangle(left, top, right - left, bottom - top);
			}
			return Rectangle.Empty;
		}

		public IList<CommandBaseRenderer> GetItems()
		{
			return _items;
		}

		public bool ContainsCommand(CommandBase commandBase)
		{
			foreach (CommandBaseRenderer item in _items)
			{
				if (item.GetCommand() == commandBase)
				{
					return true;
				}
			}
			return false;
		}

		public bool CanShrink(bool forceShrink)
		{
			if (IsFullyShrunk())
			{
				return false;
			}
			switch (_commandsType)
			{
			case CommandType.ButtonGroup:
				return false;
			case CommandType.SmallButton:
				foreach (CommandBaseRenderer item in _items)
				{
					if (item.CommandType == CommandType.ContainerControl)
					{
						return false;
					}
					if (!item._hasImageContent || !item._hasTextContent)
					{
						return false;
					}
				}
				return true;
			case CommandType.LargeButton:
				if (_items.Count > 1)
				{
					return true;
				}
				return false;
			case CommandType.Box:
				return ((CommandBoxRenderer)_items[0]).CanShrinkColumns();
			case CommandType.Gallery:
				return ((GalleryRenderer)_items[0]).CanShrink(forceShrink);
			case CommandType.ContainerControl:
				return false;
			default:
				return false;
			}
		}

		public void Shrink()
		{
			switch (_commandsType)
			{
			case CommandType.ButtonGroup:
				if (_items[0] is ButtonGroupRenderer buttonGroup)
				{
					buttonGroup.SetHasRightSeparator(true);
				}
				break;
			case CommandType.SmallButton:
				foreach (RibbonButtonRenderer item in _items)
				{
					item.ShowText = false;
				}
				break;
			case CommandType.LargeButton:
			{
				int groupedItemCount = _items.Count / 3 * 3;
				for (int i = 1; i <= groupedItemCount; i++)
				{
					RibbonButtonRenderer button = (RibbonButtonRenderer)_items[_items.Count - i];
					if (button.SizeStyle == CommandSizeStyle.Large)
					{
						button.SizeStyle = CommandSizeStyle.Small;
					}
					else
					{
						button.ShowText = false;
					}
				}
				break;
			}
			case CommandType.Gallery:
				if (_items[0] is GalleryRenderer galleryRenderer)
				{
					galleryRenderer.Columns--;
					galleryRenderer._hasReducedColumns = true;
					SetFullyShrunk(galleryRenderer.IsCollapsedToButton());
					return;
				}
				break;
			}
			SetFullyShrunk(true);
		}

		internal bool TryMovePrevious(CommandBaseRenderer item)
		{
			int startIndex = 0;
			if (item != null)
			{
				startIndex = _items.IndexOf(item) - 1;
			}
			CommandBaseRenderer commandBaseRenderer = null;
			for (int index = startIndex; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _items[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (item == null && thisValue is IKeyboardNavigable)
					{
						return ((IKeyboardNavigable)thisValue).MovePrevious(item);
					}
					commandBaseRenderer = thisValue;
					break;
				}
			}
			if (commandBaseRenderer != null)
			{
				if (commandBaseRenderer is IKeyboardNavigable)
				{
					return ((IKeyboardNavigable)commandBaseRenderer).MovePrevious(null);
				}
				_owner.ActiveInputItem = commandBaseRenderer;
				return true;
			}
			return false;
		}

		internal bool TryMoveNext(CommandBaseRenderer item)
		{
			int startIndex = 0;
			if (item != null)
			{
				startIndex = _items.IndexOf(item) + 1;
			}
			CommandBaseRenderer commandBaseRenderer = null;
			for (int i = startIndex; i < _items.Count; i++)
			{
				CommandBaseRenderer thisValue = _items[i];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (item == null && thisValue is IKeyboardNavigable)
					{
						return ((IKeyboardNavigable)thisValue).MoveNext(item);
					}
					commandBaseRenderer = thisValue;
					break;
				}
			}
			if (commandBaseRenderer != null)
			{
				if (commandBaseRenderer is IKeyboardNavigable)
				{
					return ((IKeyboardNavigable)commandBaseRenderer).MoveNext(null);
				}
				_owner.ActiveInputItem = commandBaseRenderer;
				return true;
			}
			return false;
		}

		internal bool TryMoveRight(CommandBaseRenderer item)
		{
			int startIndex = 0;
			if (item != null)
			{
				startIndex = _items.IndexOf(item) + 1;
			}
			CommandBaseRenderer commandBaseRenderer = null;
			for (int i = startIndex; i < _items.Count; i++)
			{
				CommandBaseRenderer thisValue = _items[i];
				if (thisValue.GetCommand() is SeparatorCommand)
				{
					continue;
				}
				if (item == null)
				{
					if (thisValue is IKeyboardNavigable)
					{
						return ((IKeyboardNavigable)thisValue).MoveRight(item);
					}
					commandBaseRenderer = thisValue;
					break;
				}
				if (thisValue._bounds.X > item._bounds.X)
				{
					if (item._rowInColumn == thisValue._rowInColumn)
					{
						commandBaseRenderer = thisValue;
						break;
					}
					if (thisValue._rowInColumn < item._rowInColumn)
					{
						commandBaseRenderer = thisValue;
					}
					else if (commandBaseRenderer == null || commandBaseRenderer._rowInColumn > thisValue._rowInColumn)
					{
						commandBaseRenderer = thisValue;
					}
				}
			}
			if (commandBaseRenderer != null)
			{
				if (commandBaseRenderer is IKeyboardNavigable)
				{
					return ((IKeyboardNavigable)commandBaseRenderer).MoveRight(null);
				}
				_owner.ActiveInputItem = commandBaseRenderer;
				return true;
			}
			return false;
		}

		internal bool TryMoveLeft(CommandBaseRenderer item)
		{
			int startIndex = 0;
			if (item != null)
			{
				startIndex = _items.IndexOf(item) - 1;
			}
			CommandBaseRenderer commandBaseRenderer = null;
			for (int index = startIndex; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _items[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (item == null)
					{
						if (thisValue is IKeyboardNavigable)
						{
							return ((IKeyboardNavigable)thisValue).MoveLeft(item);
						}
						commandBaseRenderer = thisValue;
						break;
					}
					if (thisValue._bounds.X < item._bounds.X)
					{
						if (item._rowInColumn == thisValue._rowInColumn)
						{
							commandBaseRenderer = thisValue;
							break;
						}
						if (thisValue._rowInColumn < item._rowInColumn)
						{
							commandBaseRenderer = thisValue;
						}
						commandBaseRenderer = thisValue;
					}
				}
			}
			if (commandBaseRenderer != null)
			{
				if (commandBaseRenderer is IKeyboardNavigable)
				{
					return ((IKeyboardNavigable)commandBaseRenderer).MoveLeft(null);
				}
				_owner.ActiveInputItem = commandBaseRenderer;
				return true;
			}
			return false;
		}

		internal bool TryMoveDown(CommandBaseRenderer item)
		{
			int startIndex = 0;
			if (item != null)
			{
				startIndex = _items.IndexOf(item) + 1;
			}
			for (int i = startIndex; i < _items.Count; i++)
			{
				CommandBaseRenderer thisValue = _items[i];
				if (item != null && item._rowInColumn >= thisValue._rowInColumn)
				{
					break;
				}
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (thisValue is IKeyboardNavigable && ((IKeyboardNavigable)thisValue).SelectFirstItem())
					{
						return true;
					}
					_owner.ActiveInputItem = thisValue;
					return true;
				}
			}
			return false;
		}

		internal bool TryMoveUp(CommandBaseRenderer item)
		{
			int startIndex = 0;
			startIndex = ((item == null) ? (_items.Count - 1) : (_items.IndexOf(item) - 1));
			for (int index = startIndex; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _items[index];
				if (item != null && item._rowInColumn <= thisValue._rowInColumn)
				{
					break;
				}
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (thisValue is IKeyboardNavigable && ((IKeyboardNavigable)thisValue).SelectLastItem())
					{
						return true;
					}
					_owner.ActiveInputItem = thisValue;
					return true;
				}
			}
			return false;
		}

		public void SpreadLastGroupVertically(int availableHeight, int itemsSpace)
		{
			int usedHeight = 0;
			for (int i = 0; i < _items.Count; i++)
			{
				CommandBaseRenderer thisValue = _items[i];
				usedHeight = ((usedHeight + thisValue._bounds.Height <= availableHeight) ? (usedHeight + thisValue._bounds.Height) : thisValue._bounds.Height);
			}
			int lastGroupStart = _items.Count - _items.Count % _owner.MaxItemRows;
			if (usedHeight < availableHeight)
			{
				availableHeight -= (_items.Count - lastGroupStart - 1) * itemsSpace;
				int extraSpacing = (availableHeight - usedHeight) / (_items.Count - lastGroupStart + 1);
				for (int j = lastGroupStart; j < _items.Count; j++)
				{
					CommandBaseRenderer shiftedItem = _items[j];
					shiftedItem.MoveY(shiftedItem._bounds.Y + extraSpacing * (j - lastGroupStart + 1));
				}
			}
		}
	}
}
