using System;

namespace Mordex.Data
{
	public interface ISupportFilterBuilder
	{
		IMordexTable RootTable { get; }

		event EventHandler RootTableChanged;

		IMordexCondition CreateCondition();
	}
}
