using System.Collections;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandPositionComparer : IComparer
	{
		private const int NoPosition = -1;

		private MxCommandCollection _ownerCollection;

		internal CommandPositionComparer(MxCommandCollection ownerCollection)
		{
			_ownerCollection = ownerCollection;
		}

		public int Compare(object firstObject, object secondObject)
		{
			if (firstObject == null || secondObject == null)
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
				}
			}
			else if (secondPosition != NoPosition)
			{
				result = firstCommand.GetLayoutIndex().CompareTo(secondPosition);
			}
			if (result == 0)
			{
				if (firstCommand.GetListIndex() >= 0 && secondCommand.GetListIndex() >= 0)
				{
					result = firstCommand.GetListIndex().CompareTo(secondCommand.GetListIndex());
				}
				if (result == 0)
				{
					result = firstCommand.GetLayoutIndex().CompareTo(secondCommand.GetLayoutIndex());
				}
			}
			return result;
		}
	}
}
