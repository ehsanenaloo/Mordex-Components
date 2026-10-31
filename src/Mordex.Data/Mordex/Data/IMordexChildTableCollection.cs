using System.Collections;

namespace Mordex.Data
{
	public interface IMordexChildTableCollection : IEnumerable
	{
		IMordexTable this[int index] { get; }

		IMordexTable this[string key] { get; }

		int Count { get; }

		IMordexTable ParentTable { get; }

		int Add(IMordexTable table);

		void Clear();

		void Remove(IMordexTable table);

		void Remove(string key);

		void RemoveAt(int index);
	}
}
