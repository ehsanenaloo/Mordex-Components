using System.Collections;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandMergeOrderComparer : IComparer
	{
		private MxCommandCollection _ownerCollection;

		internal CommandMergeOrderComparer(MxCommandCollection ownerCollection)
		{
			_ownerCollection = ownerCollection;
		}

		public int Compare(object value, object secondValue)
		{
			if (value == null || secondValue == null || value == secondValue)
			{
				return 0;
			}
			MxCommand firstCommand = (MxCommand)value;
			MxCommand secondCommand = (MxCommand)secondValue;
			int result = firstCommand.MergeOrder.CompareTo(secondCommand.MergeOrder);
			if (result == 0)
			{
				result = firstCommand.IsMergedCommand.CompareTo(secondCommand.IsMergedCommand);
				if (result == 0)
				{
					result = firstCommand.GetLayoutIndex().CompareTo(secondCommand.GetLayoutIndex());
				}
			}
			return result;
		}
	}
}
