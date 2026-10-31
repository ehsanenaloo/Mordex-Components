using System.Collections;

namespace Mordex.Data
{
	public interface IMordexGroupTotalCollection : IEnumerable
	{
		IMordexTable Table { get; }

		IMordexGroupTotal this[int index] { get; }

		IMordexGroupTotal this[string key] { get; }

		int Count { get; }

		int Add(IMordexGroupTotal groupTotal);

		void Remove(IMordexGroupTotal groupTotal);

		void Remove(string key);

		void RemoveAt(int index);

		void Clear();
	}
}
