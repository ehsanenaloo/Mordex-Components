using System.Collections;

namespace Mordex.Data
{
	internal class NodeRowIndexComparer : IComparer
	{
		internal NodeRowIndexComparer()
		{
		}

		int IComparer.Compare(object value, object other)
		{
			HierarchyNode node = value as HierarchyNode;
			HierarchyNode otherNode = other as HierarchyNode;
			if (node != null && otherNode != null)
			{
				return node.GetDataRowIndex().CompareTo(otherNode.GetDataRowIndex());
			}
			return 0;
		}
	}
}
