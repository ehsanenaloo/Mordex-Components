using System.ComponentModel;
using Mordex.Data;

namespace Mordex.Windows.Timeline.Data
{
	internal class DataState
	{
		internal static Mordex.Data.SortOrder ToDataSortOrder(SortOrder sortOrder)
		{
			switch (sortOrder)
			{
			case SortOrder.Ascending:
				return Mordex.Data.SortOrder.Ascending;
			case SortOrder.Descending:
				return Mordex.Data.SortOrder.Descending;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static SortOrder ToTimelineSortOrder(Mordex.Data.SortOrder sortOrder)
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

		internal static FormatMode ToTimelineFormatMode(Mordex.Data.FormatMode formatMode)
		{
			switch (formatMode)
			{
			case Mordex.Data.FormatMode.UseIFormattable:
				return FormatMode.UseIFormattable;
			case Mordex.Data.FormatMode.UseStringFormat:
			case Mordex.Data.FormatMode.UseInputMask:
				return FormatMode.UseStringFormat;
			case Mordex.Data.FormatMode.UseDefault:
				return FormatMode.UseDefault;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static Mordex.Data.FormatMode ToDataFormatMode(FormatMode formatMode)
		{
			switch (formatMode)
			{
			case FormatMode.UseStringFormat:
				return Mordex.Data.FormatMode.UseStringFormat;
			case FormatMode.UseIFormattable:
				return Mordex.Data.FormatMode.UseIFormattable;
			case FormatMode.UseDefault:
				return Mordex.Data.FormatMode.UseDefault;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static GroupInterval ToTimelineGroupInterval(Mordex.Data.GroupInterval groupInterval)
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

		internal static Mordex.Data.GroupInterval ToDataGroupInterval(GroupInterval groupInterval)
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

		internal static ConditionOperator ToTimelineConditionOperator(Mordex.Data.ConditionOperator conditionOperator)
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
				throw new InvalidEnumArgumentException();
			}
		}

		internal static LogicalOperator ToTimelineLogicalOperator(Mordex.Data.LogicalOperator logicalOperator)
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
				throw new InvalidEnumArgumentException();
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
				throw new InvalidEnumArgumentException();
			}
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

		internal static AggregateFunction ToTimelineAggregateFunction(Mordex.Data.AggregateFunction aggregateFunction)
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

		internal static FieldCompareTarget ToTimelineFieldCompareTarget(CompareTarget compareTarget)
		{
			switch (compareTarget)
			{
			case CompareTarget.Text:
				return FieldCompareTarget.Text;
			case CompareTarget.Value:
				return FieldCompareTarget.Value;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		internal static CompareTarget ToDataCompareTarget(FieldCompareTarget fieldCompareTarget)
		{
			switch (fieldCompareTarget)
			{
			case FieldCompareTarget.Text:
				return CompareTarget.Text;
			case FieldCompareTarget.Value:
				return CompareTarget.Value;
			default:
				throw new InvalidEnumArgumentException();
			}
		}
	}
}
