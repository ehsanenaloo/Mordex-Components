using Mordex.Data;
using Mordex.Windows.Common;

namespace Mordex.Windows.FilterBuilder
{
	internal interface IFilterBuilderElement
	{
		MxFilterBuilder FilterBuilder { get; }

		BuiltInTextList BuiltInTextList { get; }

		IMordexCondition FilterCondition { get; }

		int RowHeight { get; }

		void OnChildConditionRemoved(ConditionPanel panel);

		void OnChildConditionAdded(IMordexCondition condition);

		void InvalidateConditions();

		void OnConditionChanged(IMordexCondition condition);
	}
}
