using Mordex.Data;

namespace Mordex.Windows.FilterBuilder
{
	internal static class FilterBuilderTexts
	{
		internal static string LogicalOperatorText(LogicalOperator logicalOperator, bool negated, BuiltInTextList texts)
		{
			switch (logicalOperator)
			{
			case LogicalOperator.Or:
				return texts[negated ? BuiltInText.OrNotOperator : BuiltInText.OrOperator];
			case LogicalOperator.Xor:
				return texts[negated ? BuiltInText.XorNotOperator : BuiltInText.XorOperator];
			default:
				return texts[negated ? BuiltInText.AndNotOperator : BuiltInText.AndOperator];
			}
		}

		internal static string ConditionOperatorText(ConditionOperator conditionOperator, BuiltInTextList texts)
		{
			switch (conditionOperator)
			{
			case ConditionOperator.BeginsWith: return texts[BuiltInText.BeginsWith];
			case ConditionOperator.Between: return texts[BuiltInText.Between];
			case ConditionOperator.Contains: return texts[BuiltInText.Contains];
			case ConditionOperator.EndsWith: return texts[BuiltInText.EndsWith];
			case ConditionOperator.Equal: return texts[BuiltInText.Equal];
			case ConditionOperator.GreaterThan: return texts[BuiltInText.GreaterThan];
			case ConditionOperator.GreaterThanOrEqualTo: return texts[BuiltInText.GreaterThanOrEqualTo];
			case ConditionOperator.In: return texts[BuiltInText.In];
			case ConditionOperator.IsEmpty: return texts[BuiltInText.IsEmpty];
			case ConditionOperator.IsNull: return texts[BuiltInText.IsNull];
			case ConditionOperator.LessThan: return texts[BuiltInText.LessThan];
			case ConditionOperator.LessThanOrEqualTo: return texts[BuiltInText.LessThanOrEqualTo];
			case ConditionOperator.NotBetween: return texts[BuiltInText.NotBetween];
			case ConditionOperator.NotContains: return texts[BuiltInText.NotContains];
			case ConditionOperator.NotEqual: return texts[BuiltInText.NotEqual];
			case ConditionOperator.NotIn: return texts[BuiltInText.NotIn];
			case ConditionOperator.NotIsEmpty: return texts[BuiltInText.NotIsEmpty];
			case ConditionOperator.NotIsNull: return texts[BuiltInText.NotIsNull];
			default: return "";
			}
		}
	}
}
