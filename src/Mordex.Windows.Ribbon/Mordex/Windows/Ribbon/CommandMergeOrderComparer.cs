using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class CommandMergeOrderComparer : IComparer
	{
		private IList _commands;

		internal CommandMergeOrderComparer(IList commands)
		{
			_commands = commands;
		}

		public int Compare(object left, object right)
		{
			if (left == null || right == null || left == right)
			{
				return 0;
			}
			CommandBase commandBase = (CommandBase)left;
			CommandBase otherCommand = (CommandBase)right;
			int result = commandBase.MergeOrder.CompareTo(otherCommand.MergeOrder);
			if (result == 0)
			{
				bool isMerged = commandBase.IsMerged();
				bool value = otherCommand.IsMerged();
				result = isMerged.CompareTo(value);
				if (result == 0)
				{
					int index = _commands.IndexOf(commandBase);
					int otherIndex = _commands.IndexOf(otherCommand);
					result = index.CompareTo(otherIndex);
				}
			}
			return result;
		}
	}
}
