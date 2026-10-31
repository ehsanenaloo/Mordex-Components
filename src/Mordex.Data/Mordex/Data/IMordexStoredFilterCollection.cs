using System.Collections;

namespace Mordex.Data
{
	public interface IMordexStoredFilterCollection : IEnumerable
	{
		IMordexTable Table { get; }

		IMordexStoredFilter this[int index] { get; }

		IMordexStoredFilter this[string key] { get; }

		int Count { get; }

		void Clear();

		int Add(IMordexStoredFilter storedFilter);

		void Remove(IMordexStoredFilter storedFilter);

		void Remove(string key);

		void RemoveAt(int index);
	}
}
