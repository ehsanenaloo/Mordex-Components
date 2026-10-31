using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonContextualTabOriginalIndexComparer : IComparer
	{
		internal RibbonContextualTabOriginalIndexComparer()
		{
		}

		public int Compare(object value, object otherValue)
		{
			if (value == null || otherValue == null || value == otherValue)
			{
				return 0;
			}
			RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)value;
			RibbonContextualTab otherTab = (RibbonContextualTab)otherValue;
			int result = ribbonContextualTab.GetOriginalIndex().CompareTo(otherTab.GetOriginalIndex());
			if (result == 0)
			{
				result = ribbonContextualTab.GetIndexInOwner().CompareTo(otherTab.GetIndexInOwner());
			}
			return result;
		}
	}
}
