using System.Collections;

namespace Mordex.Data
{
	public interface IMordexFieldCollection : IEnumerable
	{
		IMordexTable Table { get; }

		IMordexField this[int index] { get; }

		IMordexField this[string key] { get; }

		int Count { get; }

		int Add(IMordexField field);

		void Insert(int index, IMordexField field);

		void Clear();

		void RemoveAt(int index);

		void Remove(IMordexField field);

		void Remove(string key);
	}
}
