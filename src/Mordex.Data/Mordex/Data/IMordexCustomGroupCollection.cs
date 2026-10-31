using System.Collections;

namespace Mordex.Data
{
	public interface IMordexCustomGroupCollection : IEnumerable
	{
		IMordexTable Table { get; }

		IMordexCustomGroup this[int index] { get; }

		IMordexCustomGroup this[string key] { get; }

		int Count { get; }

		void Clear();

		int Add(IMordexCustomGroup customGroup);

		void RemoveAt(int index);

		void Remove(IMordexCustomGroup customGroup);

		void Remove(string key);

		void Insert(int index, IMordexCustomGroup customGroup);
	}
}
