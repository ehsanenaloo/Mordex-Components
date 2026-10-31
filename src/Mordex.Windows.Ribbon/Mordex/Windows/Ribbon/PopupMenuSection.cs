using System.Collections.Generic;

namespace Mordex.Windows.Ribbon
{
	internal class PopupMenuSection
	{
		public IList<CommandBaseRenderer> _items;

		private CommandType _commandType;

		public PopupMenuSection(CommandType commandType)
		{
			if (commandType == CommandType.ContainerControl)
			{
				commandType = CommandType.SmallButton;
			}
			_commandType = commandType;
			_items = new List<CommandBaseRenderer>();
		}

		public CommandType GetCommandType()
		{
			return _commandType;
		}

		public bool CanContain(CommandType commandType)
		{
			if (commandType == CommandType.ContainerControl)
			{
				commandType = CommandType.SmallButton;
			}
			if (commandType != _commandType)
			{
				if (commandType == CommandType.Separator)
				{
					return true;
				}
				return false;
			}
			return true;
		}

		public void AddItem(CommandBaseRenderer item)
		{
			_items.Add(item);
		}

		public int GetTotalHeight()
		{
			int totalHeight = 0;
			foreach (CommandBaseRenderer item in _items)
			{
				totalHeight += item._bounds.Height;
			}
			return totalHeight;
		}
	}
}
