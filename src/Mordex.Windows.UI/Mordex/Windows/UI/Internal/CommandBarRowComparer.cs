using System.Collections;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandBarRowComparer : IComparer
	{
		internal CommandBarRowComparer()
		{
		}

		public int Compare(object value, object secondValue)
		{
			if (value == null || secondValue == null)
			{
				return 0;
			}
			CommandBarState uiCommandBarState = (CommandBarState)value;
			CommandBarState secondState = (CommandBarState)secondValue;
			int rowIndex = uiCommandBarState._rowIndex;
			int secondRowIndex = secondState._rowIndex;
			return rowIndex.CompareTo(secondRowIndex);
		}
	}
}
