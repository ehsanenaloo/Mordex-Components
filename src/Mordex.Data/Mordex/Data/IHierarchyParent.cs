namespace Mordex.Data
{
	internal interface IHierarchyParent
	{
		MordexFieldBaseState SelfReferencingManager { get; }

		HierarchyNodeCollection ChildRows { get; }

		void AddNode(HierarchyNode node, bool notify);

		void RemoveNode(HierarchyNode node);
	}
}
