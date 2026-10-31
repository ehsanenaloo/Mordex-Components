using System;
using System.Collections;

namespace Mordex.Windows.UI.Controls
{
	internal class ComboBoxItemIndexComparer : IComparer
	{
		internal ComboBoxItemIndexComparer()
		{
		}

		public int Compare(object value, object secondValue)
		{
			if (!(value is ComboBoxItem) || !(secondValue is ComboBoxItem))
			{
				throw new InvalidOperationException();
			}
			int index = ((ComboBoxItem)value)._originalIndex;
			int secondIndex = ((ComboBoxItem)secondValue)._originalIndex;
			return index.CompareTo(secondIndex);
		}
	}
}
