using System.Collections;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandPositionMergeOrderComparer : IComparer
	{
		private const int NoPosition = -1;

		private MxCommandCollection _ownerCollection;

		internal CommandPositionMergeOrderComparer(MxCommandCollection ownerCollection)
		{
			_ownerCollection = ownerCollection;
		}

		public int Compare(object firstObject, object secondObject)
		{
			if (firstObject == null || secondObject == null || firstObject == secondObject)
			{
				return 0;
			}
			MxCommand firstCommand = (MxCommand)firstObject;
			MxCommand secondCommand = (MxCommand)secondObject;
			int firstPosition = firstCommand.GetPosition();
			int secondPosition = secondCommand.GetPosition();
			int result = 0;
			if (firstPosition != NoPosition)
			{
				if (secondPosition != NoPosition)
				{
					result = firstPosition.CompareTo(secondPosition);
				}
				else
				{
					int value = secondCommand.GetLayoutIndex();
					result = firstPosition.CompareTo(value);
					if (result == 0)
					{
						return 1;
					}
				}
			}
			else if (secondPosition != NoPosition)
			{
				result = firstCommand.GetLayoutIndex().CompareTo(secondPosition);
				if (result == 0)
				{
					return 1;
				}
			}
			if (result == 0)
			{
				result = firstCommand.MergeOrder.CompareTo(secondCommand.MergeOrder);
				if (result == 0)
				{
					bool isFirstMerged = firstCommand.IsMergedCommand;
					bool isSecondMerged = secondCommand.IsMergedCommand;
					result = isFirstMerged.CompareTo(isSecondMerged);
					if (result == 0)
					{
						result = firstCommand.GetLayoutIndex().CompareTo(secondCommand.GetLayoutIndex());
					}
				}
			}
			return result;
		}
	}
}
