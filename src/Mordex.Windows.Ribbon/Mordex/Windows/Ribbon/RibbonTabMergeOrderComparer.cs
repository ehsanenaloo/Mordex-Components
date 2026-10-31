using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonTabMergeOrderComparer : IComparer
	{
		internal RibbonTabMergeOrderComparer()
		{
		}

		public int Compare(object left, object right)
		{
			if (left == null || right == null || left == right)
			{
				return 0;
			}
			RibbonTab ribbonTab = (RibbonTab)left;
			RibbonTab otherTab = (RibbonTab)right;
			int result = ribbonTab.MergeOrder.CompareTo(otherTab.MergeOrder);
			if (result == 0)
			{
				bool isMerged = ribbonTab.IsMerged();
				bool value = otherTab.IsMerged();
				result = isMerged.CompareTo(value);
				if (result == 0)
				{
					result = ribbonTab.Index.CompareTo(otherTab.Index);
				}
			}
			return result;
		}
	}
}
