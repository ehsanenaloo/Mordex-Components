using System.Collections;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandAlignmentComparer : IComparer
	{
		internal CommandAlignmentComparer()
		{
		}

		public int Compare(object value, object secondValue)
		{
			if (value == null || secondValue == null)
			{
				return 0;
			}
			MxCommand uiCommand = (MxCommand)value;
			MxCommand otherCommand = (MxCommand)secondValue;
			CommandAlignment commandAlignment = uiCommand.GetEffectiveAlignment();
			CommandAlignment otherAlignment = otherCommand.GetEffectiveAlignment();
			if (commandAlignment == otherAlignment)
			{
				return uiCommand.GetLayoutIndex().CompareTo(otherCommand.GetLayoutIndex());
			}
			if (commandAlignment == CommandAlignment.Near)
			{
				return -1;
			}
			return 1;
		}
	}
}
