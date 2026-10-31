using System.Collections;

namespace Mordex.Data
{
	public interface IMordexGroupCollection : IEnumerable
	{
		IMordexTable Table { get; }

		IMordexGroup this[int index] { get; }

		int Count { get; }

		void Clear();

		int Add(IMordexGroup group);

		IMordexGroup Add(IMordexField field, SortOrder sortOrder);

		void Remove(IMordexGroup group);

		void RemoveAt(int index);
	}
}
