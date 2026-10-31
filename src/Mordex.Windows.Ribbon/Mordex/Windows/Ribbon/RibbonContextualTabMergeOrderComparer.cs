using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonContextualTabMergeOrderComparer : IComparer
	{
		internal RibbonContextualTabMergeOrderComparer()
		{
		}

		public int Compare(object left, object right)
		{
			if (left == null || right == null || left == right)
			{
				return 0;
			}
			RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)left;
			RibbonContextualTab otherTab = (RibbonContextualTab)right;
			int result = ribbonContextualTab.MergeOrder.CompareTo(otherTab.MergeOrder);
			if (result == 0)
			{
				bool isMerged = ribbonContextualTab.IsMerged();
				bool value = otherTab.IsMerged();
				result = isMerged.CompareTo(value);
				if (result == 0)
				{
					result = ribbonContextualTab.GetIndexInOwner().CompareTo(otherTab.GetIndexInOwner());
				}
			}
			return result;
		}
	}
}
