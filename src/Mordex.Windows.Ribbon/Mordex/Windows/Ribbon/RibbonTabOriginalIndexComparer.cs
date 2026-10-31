using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonTabOriginalIndexComparer : IComparer
	{
		internal RibbonTabOriginalIndexComparer()
		{
		}

		public int Compare(object value, object otherValue)
		{
			if (value == null || otherValue == null || value == otherValue)
			{
				return 0;
			}
			RibbonTab ribbonTab = (RibbonTab)value;
			RibbonTab otherTab = (RibbonTab)otherValue;
			int result = ribbonTab.GetIndexInOwner().CompareTo(otherTab.GetIndexInOwner());
			if (result == 0)
			{
				result = ribbonTab.Index.CompareTo(otherTab.Index);
			}
			return result;
		}
	}
}
