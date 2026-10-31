namespace Mordex.Data
{
	public interface IMordexSelfReferencingSettings
	{
		string ChildListMember { get; set; }

		SelfReferencingAggregationMode AggregationMode { get; set; }

		HierarchicalGroupMode HierarchicalGroupMode { get; set; }

		string ChildMember { get; set; }

		string ParentMember { get; set; }

		object ParentRootValue { get; set; }

		ParentRootMode ParentRootMode { get; set; }

		bool TreatOrphanRowsAsRoot { get; set; }
	}
}
