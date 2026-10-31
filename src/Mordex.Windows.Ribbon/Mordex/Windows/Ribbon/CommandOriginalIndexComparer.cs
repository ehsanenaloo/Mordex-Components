using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class CommandOriginalIndexComparer : IComparer
	{
		private IList _commands;

		internal CommandOriginalIndexComparer(IList commands)
		{
			_commands = commands;
		}

		public int Compare(object value, object otherValue)
		{
			if (value == null || otherValue == null || value == otherValue)
			{
				return 0;
			}
			CommandBase commandBase = (CommandBase)value;
			CommandBase otherCommand = (CommandBase)otherValue;
			int index = commandBase.GetOriginalIndex();
			int otherIndex = otherCommand.GetOriginalIndex();
			if (index == -1)
			{
				index = _commands.IndexOf(commandBase);
			}
			if (otherIndex == -1)
			{
				otherIndex = _commands.IndexOf(otherCommand);
			}
			int result = index.CompareTo(otherIndex);
			if (result == 0)
			{
				index = _commands.IndexOf(commandBase);
				otherIndex = _commands.IndexOf(otherCommand);
				result = index.CompareTo(otherIndex);
			}
			return result;
		}
	}
}
