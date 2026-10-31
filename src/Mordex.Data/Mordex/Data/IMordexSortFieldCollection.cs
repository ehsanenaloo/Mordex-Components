using System.Collections;

namespace Mordex.Data
{
	public interface IMordexSortFieldCollection : IEnumerable
	{
		IMordexTable Table { get; }

		int Count { get; }

		IMordexSortField this[int index] { get; }

		int Add(IMordexSortField sortField);

		void Clear();

		void Remove(IMordexSortField sortField);

		void RemoveAt(int index);
	}
}
