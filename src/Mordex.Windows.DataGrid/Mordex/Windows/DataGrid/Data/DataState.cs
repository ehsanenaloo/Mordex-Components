using System;
using System.ComponentModel;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class DataState
	{
		internal static CustomGroupType ComputeCustomGroupType(Mordex.Data.CustomGroupType customGroupType)
		{
			switch (customGroupType)
			{
			case Mordex.Data.CustomGroupType.CompositeColumns:
				return CustomGroupType.CompositeColumns;
			case Mordex.Data.CustomGroupType.ConditionalGroupRows:
				return CustomGroupType.ConditionalGroupRows;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.CustomGroupType ToDataCustomGroupType(CustomGroupType customGroupType)
		{
			switch (customGroupType)
			{
			case CustomGroupType.ConditionalGroupRows:
				return Mordex.Data.CustomGroupType.ConditionalGroupRows;
			case CustomGroupType.CompositeColumns:
				return Mordex.Data.CustomGroupType.CompositeColumns;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static RowCheckState ComputeRowCheckState(CheckedState checkedState)
		{
			switch (checkedState)
			{
			case CheckedState.Checked:
				return RowCheckState.Checked;
			case CheckedState.Mixed:
				return RowCheckState.Mixed;
			case CheckedState.Unchecked:
				return RowCheckState.Unchecked;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static CheckedState ComputeCheckedState(RowCheckState rowCheckState)
		{
			switch (rowCheckState)
			{
			case RowCheckState.Unchecked:
				return CheckedState.Unchecked;
			case RowCheckState.Mixed:
				return CheckedState.Mixed;
			case RowCheckState.Checked:
				return CheckedState.Checked;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static RowCheckStateBehavior ComputeRowCheckStateBehavior(CheckedBehavior checkedBehavior)
		{
			switch (checkedBehavior)
			{
			case CheckedBehavior.CheckChildRows:
				return RowCheckStateBehavior.CheckStateDependsOnChild;
			case CheckedBehavior.CheckIndividualRows:
				return RowCheckStateBehavior.CheckIndividualRows;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static CheckedBehavior ComputeCheckedBehavior(RowCheckStateBehavior rowCheckStateBehavior)
		{
			switch (rowCheckStateBehavior)
			{
			case RowCheckStateBehavior.CheckIndividualRows:
				return CheckedBehavior.CheckIndividualRows;
			case RowCheckStateBehavior.CheckStateDependsOnChild:
				return CheckedBehavior.CheckChildRows;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static BoundMode ToGridBoundMode(Mordex.Data.BoundMode boundMode)
		{
			switch (boundMode)
			{
			case Mordex.Data.BoundMode.Bound:
				return BoundMode.Bound;
			case Mordex.Data.BoundMode.Unbound:
				return BoundMode.Unbound;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.BoundMode ComputeBoundMode(BoundMode boundMode)
		{
			switch (boundMode)
			{
			case BoundMode.Bound:
				return Mordex.Data.BoundMode.Bound;
			case BoundMode.Unbound:
				return Mordex.Data.BoundMode.Unbound;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static ColumnBoundMode ToGridColumnBoundMode(Mordex.Data.ColumnBoundMode columnBoundMode)
		{
			switch (columnBoundMode)
			{
			case Mordex.Data.ColumnBoundMode.Bound:
				return ColumnBoundMode.Bound;
			case Mordex.Data.ColumnBoundMode.UnboundFetch:
				return ColumnBoundMode.UnboundFetch;
			case Mordex.Data.ColumnBoundMode.UnboundKeepValues:
				return ColumnBoundMode.Unbound;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.ColumnBoundMode ComputeColumnBoundMode(ColumnBoundMode columnBoundMode)
		{
			switch (columnBoundMode)
			{
			case ColumnBoundMode.Bound:
				return Mordex.Data.ColumnBoundMode.Bound;
			case ColumnBoundMode.Unbound:
				return Mordex.Data.ColumnBoundMode.UnboundKeepValues;
			case ColumnBoundMode.UnboundFetch:
				return Mordex.Data.ColumnBoundMode.UnboundFetch;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static ColumnCompareTarget ComputeColumnCompareTarget(CompareTarget compareTarget)
		{
			switch (compareTarget)
			{
			case CompareTarget.Text:
				return ColumnCompareTarget.Text;
			case CompareTarget.Value:
				return ColumnCompareTarget.Value;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static CompareTarget ComputeCompareTarget(ColumnCompareTarget columnCompareTarget)
		{
			switch (columnCompareTarget)
			{
			case ColumnCompareTarget.Text:
				return CompareTarget.Text;
			case ColumnCompareTarget.Value:
				return CompareTarget.Value;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static InheritableBoolean ToGridInheritableBoolean(Mordex.Data.InheritableBoolean inheritableBoolean)
		{
			switch (inheritableBoolean)
			{
			case Mordex.Data.InheritableBoolean.False:
				return InheritableBoolean.False;
			case Mordex.Data.InheritableBoolean.True:
				return InheritableBoolean.True;
			default:
				return InheritableBoolean.Default;
			}
		}

		internal static Mordex.Data.InheritableBoolean ToDataInheritableBoolean(InheritableBoolean inheritableBoolean)
		{
			switch (inheritableBoolean)
			{
			case InheritableBoolean.False:
				return Mordex.Data.InheritableBoolean.False;
			case InheritableBoolean.True:
				return Mordex.Data.InheritableBoolean.True;
			default:
				return Mordex.Data.InheritableBoolean.Default;
			}
		}

		internal static GroupTotals ComputeGroupTotals(Mordex.Data.GroupTotals groupTotals)
		{
			switch (groupTotals)
			{
			case Mordex.Data.GroupTotals.Always:
				return GroupTotals.Always;
			case Mordex.Data.GroupTotals.ExpandedRow:
				return GroupTotals.ExpandedGroup;
			case Mordex.Data.GroupTotals.Never:
				return GroupTotals.Never;
			default:
				return GroupTotals.Default;
			}
		}

		internal static Mordex.Data.GroupTotals ToDataGroupTotals(GroupTotals groupTotals)
		{
			switch (groupTotals)
			{
			case GroupTotals.Always:
				return Mordex.Data.GroupTotals.Always;
			case GroupTotals.ExpandedGroup:
				return Mordex.Data.GroupTotals.ExpandedRow;
			case GroupTotals.Never:
				return Mordex.Data.GroupTotals.Never;
			default:
				return Mordex.Data.GroupTotals.Default;
			}
		}

		internal static NewRowPosition ComputeNewRowPosition(Mordex.Data.NewRowPosition newRowPosition)
		{
			switch (newRowPosition)
			{
			case Mordex.Data.NewRowPosition.BottomRow:
				return NewRowPosition.BottomRow;
			case Mordex.Data.NewRowPosition.TopRow:
				return NewRowPosition.TopRow;
			default:
				return NewRowPosition.Default;
			}
		}

		internal static Mordex.Data.NewRowPosition ToDataNewRowPosition(NewRowPosition newRowPosition)
		{
			switch (newRowPosition)
			{
			case NewRowPosition.BottomRow:
				return Mordex.Data.NewRowPosition.BottomRow;
			case NewRowPosition.TopRow:
				return Mordex.Data.NewRowPosition.TopRow;
			default:
				return Mordex.Data.NewRowPosition.Default;
			}
		}

		internal static SortOrder ComputeSortOrder(Mordex.Data.SortOrder sortOrder)
		{
			switch (sortOrder)
			{
			case Mordex.Data.SortOrder.Ascending:
				return SortOrder.Ascending;
			case Mordex.Data.SortOrder.Descending:
				return SortOrder.Descending;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.SortOrder ToDataSortOrder(SortOrder sortOrder)
		{
			switch (sortOrder)
			{
			case SortOrder.Ascending:
				return Mordex.Data.SortOrder.Ascending;
			case SortOrder.Descending:
				return Mordex.Data.SortOrder.Descending;
			default:
				throw new NotImplementedException();
			}
		}

		internal static ConditionOperator ComputeConditionOperator(Mordex.Data.ConditionOperator conditionOperator)
		{
			switch (conditionOperator)
			{
			case Mordex.Data.ConditionOperator.BeginsWith:
				return ConditionOperator.BeginsWith;
			case Mordex.Data.ConditionOperator.Between:
				return ConditionOperator.Between;
			case Mordex.Data.ConditionOperator.Contains:
				return ConditionOperator.Contains;
			case Mordex.Data.ConditionOperator.EndsWith:
				return ConditionOperator.EndsWith;
			case Mordex.Data.ConditionOperator.Equal:
				return ConditionOperator.Equal;
			case Mordex.Data.ConditionOperator.GreaterThan:
				return ConditionOperator.GreaterThan;
			case Mordex.Data.ConditionOperator.GreaterThanOrEqualTo:
				return ConditionOperator.GreaterThanOrEqualTo;
			case Mordex.Data.ConditionOperator.In:
				return ConditionOperator.In;
			case Mordex.Data.ConditionOperator.IsEmpty:
				return ConditionOperator.IsEmpty;
			case Mordex.Data.ConditionOperator.IsNull:
				return ConditionOperator.IsNull;
			case Mordex.Data.ConditionOperator.LessThan:
				return ConditionOperator.LessThan;
			case Mordex.Data.ConditionOperator.LessThanOrEqualTo:
				return ConditionOperator.LessThanOrEqualTo;
			case Mordex.Data.ConditionOperator.NotBetween:
				return ConditionOperator.NotBetween;
			case Mordex.Data.ConditionOperator.NotContains:
				return ConditionOperator.NotContains;
			case Mordex.Data.ConditionOperator.NotEqual:
				return ConditionOperator.NotEqual;
			case Mordex.Data.ConditionOperator.NotIn:
				return ConditionOperator.NotIn;
			case Mordex.Data.ConditionOperator.NotIsEmpty:
				return ConditionOperator.NotIsEmpty;
			case Mordex.Data.ConditionOperator.NotIsNull:
				return ConditionOperator.NotIsNull;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.ConditionOperator ToDataConditionOperator(ConditionOperator conditionOperator)
		{
			switch (conditionOperator)
			{
			case ConditionOperator.BeginsWith:
				return Mordex.Data.ConditionOperator.BeginsWith;
			case ConditionOperator.Between:
				return Mordex.Data.ConditionOperator.Between;
			case ConditionOperator.Contains:
				return Mordex.Data.ConditionOperator.Contains;
			case ConditionOperator.EndsWith:
				return Mordex.Data.ConditionOperator.EndsWith;
			case ConditionOperator.Equal:
				return Mordex.Data.ConditionOperator.Equal;
			case ConditionOperator.GreaterThan:
				return Mordex.Data.ConditionOperator.GreaterThan;
			case ConditionOperator.GreaterThanOrEqualTo:
				return Mordex.Data.ConditionOperator.GreaterThanOrEqualTo;
			case ConditionOperator.In:
				return Mordex.Data.ConditionOperator.In;
			case ConditionOperator.IsEmpty:
				return Mordex.Data.ConditionOperator.IsEmpty;
			case ConditionOperator.IsNull:
				return Mordex.Data.ConditionOperator.IsNull;
			case ConditionOperator.LessThan:
				return Mordex.Data.ConditionOperator.LessThan;
			case ConditionOperator.LessThanOrEqualTo:
				return Mordex.Data.ConditionOperator.LessThanOrEqualTo;
			case ConditionOperator.NotBetween:
				return Mordex.Data.ConditionOperator.NotBetween;
			case ConditionOperator.NotContains:
				return Mordex.Data.ConditionOperator.NotContains;
			case ConditionOperator.NotEqual:
				return Mordex.Data.ConditionOperator.NotEqual;
			case ConditionOperator.NotIn:
				return Mordex.Data.ConditionOperator.NotIn;
			case ConditionOperator.NotIsEmpty:
				return Mordex.Data.ConditionOperator.NotIsEmpty;
			case ConditionOperator.NotIsNull:
				return Mordex.Data.ConditionOperator.NotIsNull;
			default:
				throw new NotImplementedException();
			}
		}

		internal static LogicalOperator ComputeLogicalOperator(Mordex.Data.LogicalOperator logicalOperator)
		{
			switch (logicalOperator)
			{
			case Mordex.Data.LogicalOperator.And:
				return LogicalOperator.And;
			case Mordex.Data.LogicalOperator.None:
				return LogicalOperator.None;
			case Mordex.Data.LogicalOperator.Or:
				return LogicalOperator.Or;
			case Mordex.Data.LogicalOperator.Xor:
				return LogicalOperator.Xor;
			default:
				throw new NotImplementedException();
			}
		}

		internal static Mordex.Data.LogicalOperator ToDataLogicalOperator(LogicalOperator logicalOperator)
		{
			switch (logicalOperator)
			{
			case LogicalOperator.And:
				return Mordex.Data.LogicalOperator.And;
			case LogicalOperator.None:
				return Mordex.Data.LogicalOperator.None;
			case LogicalOperator.Or:
				return Mordex.Data.LogicalOperator.Or;
			case LogicalOperator.Xor:
				return Mordex.Data.LogicalOperator.Xor;
			default:
				throw new NotImplementedException();
			}
		}

		internal static bool ResolveInheritableBoolean(InheritableBoolean inheritableBoolean, bool otherInheritableBoolean)
		{
			switch (inheritableBoolean)
			{
			case InheritableBoolean.True:
				return true;
			case InheritableBoolean.False:
				return false;
			default:
				return otherInheritableBoolean;
			}
		}

		internal static InheritableBoolean ComputeInheritableBoolean(bool condition)
		{
			if (condition)
			{
				return InheritableBoolean.True;
			}
			return InheritableBoolean.False;
		}

		internal static Mordex.Data.AggregateFunction ToDataAggregateFunction(AggregateFunction aggregateFunction)
		{
			switch (aggregateFunction)
			{
			case AggregateFunction.Average:
				return Mordex.Data.AggregateFunction.Average;
			case AggregateFunction.Count:
				return Mordex.Data.AggregateFunction.Count;
			case AggregateFunction.Max:
				return Mordex.Data.AggregateFunction.Max;
			case AggregateFunction.Min:
				return Mordex.Data.AggregateFunction.Min;
			case AggregateFunction.None:
				return Mordex.Data.AggregateFunction.None;
			case AggregateFunction.StdDeviation:
				return Mordex.Data.AggregateFunction.StdDeviation;
			case AggregateFunction.Sum:
				return Mordex.Data.AggregateFunction.Sum;
			case AggregateFunction.ValueCount:
				return Mordex.Data.AggregateFunction.ValueCount;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static AggregateFunction ComputeAggregateFunction(Mordex.Data.AggregateFunction aggregateFunction)
		{
			switch (aggregateFunction)
			{
			case Mordex.Data.AggregateFunction.Average:
				return AggregateFunction.Average;
			case Mordex.Data.AggregateFunction.Count:
				return AggregateFunction.Count;
			case Mordex.Data.AggregateFunction.Max:
				return AggregateFunction.Max;
			case Mordex.Data.AggregateFunction.Min:
				return AggregateFunction.Min;
			case Mordex.Data.AggregateFunction.None:
				return AggregateFunction.None;
			case Mordex.Data.AggregateFunction.StdDeviation:
				return AggregateFunction.StdDeviation;
			case Mordex.Data.AggregateFunction.Sum:
				return AggregateFunction.Sum;
			case Mordex.Data.AggregateFunction.ValueCount:
				return AggregateFunction.ValueCount;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.GroupInterval ComputeGroupInterval(GroupInterval groupInterval)
		{
			switch (groupInterval)
			{
			case GroupInterval.Alphabetical:
				return Mordex.Data.GroupInterval.Alphabetical;
			case GroupInterval.Date:
				return Mordex.Data.GroupInterval.Date;
			case GroupInterval.Default:
				return Mordex.Data.GroupInterval.Default;
			case GroupInterval.Hour:
				return Mordex.Data.GroupInterval.Hour;
			case GroupInterval.Minute:
				return Mordex.Data.GroupInterval.Minute;
			case GroupInterval.Month:
				return Mordex.Data.GroupInterval.Month;
			case GroupInterval.OutlookDates:
				return Mordex.Data.GroupInterval.OutlookDates;
			case GroupInterval.Quarter:
				return Mordex.Data.GroupInterval.Quarter;
			case GroupInterval.Second:
				return Mordex.Data.GroupInterval.Second;
			case GroupInterval.Text:
				return Mordex.Data.GroupInterval.Text;
			case GroupInterval.Value:
				return Mordex.Data.GroupInterval.Value;
			case GroupInterval.Year:
				return Mordex.Data.GroupInterval.Year;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static GroupInterval ToGridGroupInterval(Mordex.Data.GroupInterval groupInterval)
		{
			switch (groupInterval)
			{
			case Mordex.Data.GroupInterval.Alphabetical:
				return GroupInterval.Alphabetical;
			case Mordex.Data.GroupInterval.Date:
				return GroupInterval.Date;
			case Mordex.Data.GroupInterval.Default:
				return GroupInterval.Default;
			case Mordex.Data.GroupInterval.Hour:
				return GroupInterval.Hour;
			case Mordex.Data.GroupInterval.Minute:
				return GroupInterval.Minute;
			case Mordex.Data.GroupInterval.Month:
				return GroupInterval.Month;
			case Mordex.Data.GroupInterval.OutlookDates:
				return GroupInterval.OutlookDates;
			case Mordex.Data.GroupInterval.Quarter:
				return GroupInterval.Quarter;
			case Mordex.Data.GroupInterval.Second:
				return GroupInterval.Second;
			case Mordex.Data.GroupInterval.Text:
				return GroupInterval.Text;
			case Mordex.Data.GroupInterval.Value:
				return GroupInterval.Value;
			case Mordex.Data.GroupInterval.Year:
				return GroupInterval.Year;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.FormatMode ToDataFormatMode(FormatMode formatMode)
		{
			switch (formatMode)
			{
			case FormatMode.UseDefault:
				return Mordex.Data.FormatMode.UseDefault;
			case FormatMode.UseIFormattable:
				return Mordex.Data.FormatMode.UseIFormattable;
			case FormatMode.UseInputMask:
				return Mordex.Data.FormatMode.UseInputMask;
			case FormatMode.UseStringFormat:
				return Mordex.Data.FormatMode.UseStringFormat;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static FormatMode ToGridFormatMode(Mordex.Data.FormatMode formatMode)
		{
			switch (formatMode)
			{
			case Mordex.Data.FormatMode.UseDefault:
				return FormatMode.UseDefault;
			case Mordex.Data.FormatMode.UseIFormattable:
				return FormatMode.UseIFormattable;
			case Mordex.Data.FormatMode.UseInputMask:
				return FormatMode.UseInputMask;
			case Mordex.Data.FormatMode.UseStringFormat:
				return FormatMode.UseStringFormat;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.GroupMode ComputeGroupMode(GroupMode groupMode)
		{
			switch (groupMode)
			{
			case GroupMode.Collapsed:
				return Mordex.Data.GroupMode.Collapsed;
			case GroupMode.Expanded:
				return Mordex.Data.GroupMode.Expanded;
			case GroupMode.Default:
				return Mordex.Data.GroupMode.Default;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static GroupMode ToGridGroupMode(Mordex.Data.GroupMode groupMode)
		{
			switch (groupMode)
			{
			case Mordex.Data.GroupMode.Collapsed:
				return GroupMode.Collapsed;
			case Mordex.Data.GroupMode.Expanded:
				return GroupMode.Expanded;
			case Mordex.Data.GroupMode.Default:
				return GroupMode.Default;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static SelfReferencingAggregationMode ComputeSelfReferencingAggregationMode(Mordex.Data.SelfReferencingAggregationMode selfReferencingAggregationMode)
		{
			switch (selfReferencingAggregationMode)
			{
			case Mordex.Data.SelfReferencingAggregationMode.AllRows:
				return SelfReferencingAggregationMode.AllRows;
			case Mordex.Data.SelfReferencingAggregationMode.RootRows:
				return SelfReferencingAggregationMode.RootRows;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.SelfReferencingAggregationMode ToDataSelfReferencingAggregationMode(SelfReferencingAggregationMode selfReferencingAggregationMode)
		{
			switch (selfReferencingAggregationMode)
			{
			case SelfReferencingAggregationMode.AllRows:
				return Mordex.Data.SelfReferencingAggregationMode.AllRows;
			case SelfReferencingAggregationMode.RootRows:
				return Mordex.Data.SelfReferencingAggregationMode.RootRows;
			default:
				throw new NotImplementedException();
			}
		}

		internal static HierarchicalGroupMode ComputeHierarchicalGroupMode(Mordex.Data.HierarchicalGroupMode hierarchicalGroupMode)
		{
			switch (hierarchicalGroupMode)
			{
			case Mordex.Data.HierarchicalGroupMode.AllRows:
				return HierarchicalGroupMode.AllRows;
			case Mordex.Data.HierarchicalGroupMode.ParentRows:
				return HierarchicalGroupMode.ParentRows;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.HierarchicalGroupMode ToDataHierarchicalGroupMode(HierarchicalGroupMode hierarchicalGroupMode)
		{
			switch (hierarchicalGroupMode)
			{
			case HierarchicalGroupMode.AllRows:
				return Mordex.Data.HierarchicalGroupMode.AllRows;
			case HierarchicalGroupMode.ParentRows:
				return Mordex.Data.HierarchicalGroupMode.ParentRows;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static HierarchicalMode ComputeHierarchicalMode(Mordex.Data.HierarchicalMode hierarchicalMode)
		{
			switch (hierarchicalMode)
			{
			case Mordex.Data.HierarchicalMode.SelfReferencing:
				return HierarchicalMode.SelfReferencing;
			case Mordex.Data.HierarchicalMode.SelfReferencingChildList:
				return HierarchicalMode.SelfReferencingChildList;
			case Mordex.Data.HierarchicalMode.UseChildTables:
				return HierarchicalMode.UseChildTables;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.HierarchicalMode ToDataHierarchicalMode(HierarchicalMode hierarchicalMode)
		{
			switch (hierarchicalMode)
			{
			case HierarchicalMode.SelfReferencing:
				return Mordex.Data.HierarchicalMode.SelfReferencing;
			case HierarchicalMode.SelfReferencingChildList:
				return Mordex.Data.HierarchicalMode.SelfReferencingChildList;
			case HierarchicalMode.UseChildTables:
				return Mordex.Data.HierarchicalMode.UseChildTables;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static ParentRootMode ComputeParentRootMode(Mordex.Data.ParentRootMode parentRootMode)
		{
			switch (parentRootMode)
			{
			case Mordex.Data.ParentRootMode.NullValue:
				return ParentRootMode.NullValue;
			case Mordex.Data.ParentRootMode.SelfReferencingNode:
				return ParentRootMode.SelfReferencingNode;
			case Mordex.Data.ParentRootMode.UseParentRootValue:
				return ParentRootMode.UseParentRootValue;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.ParentRootMode ToDataParentRootMode(ParentRootMode parentRootMode)
		{
			switch (parentRootMode)
			{
			case ParentRootMode.NullValue:
				return Mordex.Data.ParentRootMode.NullValue;
			case ParentRootMode.SelfReferencingNode:
				return Mordex.Data.ParentRootMode.SelfReferencingNode;
			case ParentRootMode.UseParentRootValue:
				return Mordex.Data.ParentRootMode.UseParentRootValue;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.GroupTotalVisibility ComputeGroupTotalVisibility(GroupTotalVisibility groupTotalVisibility)
		{
			switch (groupTotalVisibility)
			{
			case GroupTotalVisibility.Default:
				return Mordex.Data.GroupTotalVisibility.Default;
			case GroupTotalVisibility.Hidden:
				return Mordex.Data.GroupTotalVisibility.Hidden;
			case GroupTotalVisibility.Visible:
				return Mordex.Data.GroupTotalVisibility.Visible;
			case GroupTotalVisibility.VisibleWhenExpanded:
				return Mordex.Data.GroupTotalVisibility.VisibleWhenExpanded;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static GroupTotalVisibility ToGridGroupTotalVisibility(Mordex.Data.GroupTotalVisibility groupTotalVisibility)
		{
			switch (groupTotalVisibility)
			{
			case Mordex.Data.GroupTotalVisibility.Default:
				return GroupTotalVisibility.Default;
			case Mordex.Data.GroupTotalVisibility.Hidden:
				return GroupTotalVisibility.Hidden;
			case Mordex.Data.GroupTotalVisibility.Visible:
				return GroupTotalVisibility.Visible;
			case Mordex.Data.GroupTotalVisibility.VisibleWhenExpanded:
				return GroupTotalVisibility.VisibleWhenExpanded;
			default:
				throw new InvalidEnumArgumentException();
			}
		}
	}
}
