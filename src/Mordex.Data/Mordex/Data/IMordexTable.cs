using System;

namespace Mordex.Data
{
	public interface IMordexTable
	{
		IFilterCondition FilterCondition { get; set; }

		GroupMode GroupMode { get; set; }

		bool FilterParentRows { get; set; }

		string Key { get; set; }

		string DataMember { get; set; }

		string TableCaption { get; set; }

		IMordexTable ParentTable { get; }

		IMordexSelfReferencingSettings SelfReferencingSettings { get; }

		bool IsAuxiliaryTable { get; }

		HierarchicalMode HierarchicalMode { get; set; }

		bool IsFiltered { get; }

		bool HasChildTables { get; }

		DataSourceManager DataSourceManager { get; set; }

		InheritableBoolean DynamicFiltering { get; set; }

		InheritableBoolean AllowChildTableGroups { get; set; }

		GroupTotals GroupTotalRows { get; set; }

		InheritableBoolean TotalRow { get; set; }

		IMordexGroupCollection Groups { get; }

		IMordexGroupTotalCollection GroupTotals { get; }

		IMordexSortFieldCollection SortFields { get; }

		IMordexChildTableCollection ChildTables { get; }

		IMordexFieldCollection Fields { get; }

		IMordexStoredFilterCollection StoredFilters { get; }

		NewRowPosition NewRowPosition { get; set; }

		InheritableBoolean AllowAddNew { get; set; }

		string[] DataKeyMembers { get; set; }

		event EventHandler ColumnsChanged;

		event EventHandler FilterConditionChanged;

		void Initialize(string dataMember);

		void Initialize();

		IMordexCondition CreateCondition();
	}
}
