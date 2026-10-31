using System.Collections;
using System.Drawing;

namespace Mordex.Data
{
	public interface IMordexValueItemCollection : IEnumerable
	{
		IMordexValueItem this[int index] { get; }

		IMordexValueItem this[object value] { get; }

		int Count { get; }

		bool ReplaceValues { get; set; }

		IMordexValueItem Add(object value, string text);

		IMordexValueItem Add(object value, string text, int imageIndex);

		IMordexValueItem Add(object value, string text, Image image);

		int Add(IMordexValueItem item);

		void Insert(int index, IMordexValueItem item);

		bool Contains(object value);

		void Remove(IMordexValueItem item);

		void RemoveAt(int index);

		void Clear();

		void Sort();
	}
}
