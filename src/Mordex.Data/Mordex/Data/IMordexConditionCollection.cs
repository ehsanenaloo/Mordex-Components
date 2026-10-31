using System.Collections;

namespace Mordex.Data
{
	public interface IMordexConditionCollection : IEnumerable
	{
		IMordexCondition this[int index] { get; }

		int Count { get; }
	}
}
