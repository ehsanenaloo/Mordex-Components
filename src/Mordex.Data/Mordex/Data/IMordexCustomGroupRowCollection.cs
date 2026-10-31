using System.Collections;

namespace Mordex.Data
{
	public interface IMordexCustomGroupRowCollection : IEnumerable
	{
		int Count { get; }

		IMordexCustomGroupRow this[int index] { get; }

		int Add(IMordexCustomGroupRow customGroupRow);

		void Clear();

		void Remove(IMordexCustomGroupRow customGroupRow);

		void RemoveAt(int index);

		void Insert(int index, IMordexCustomGroupRow customGroupRow);
	}
}
