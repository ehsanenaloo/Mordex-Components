using System;
using System.Collections;

namespace Mordex.Windows.UI.Controls
{
	internal class ComboBoxItemTextComparer : IComparer
	{
		internal ComboBoxItemTextComparer()
		{
		}

		public int Compare(object value, object secondValue)
		{
			if (!(value is ComboBoxItem) || !(secondValue is ComboBoxItem))
			{
				throw new InvalidOperationException();
			}
			string firstText = ((ComboBoxItem)value).Text;
			string secondText = ((ComboBoxItem)secondValue).Text;
			return firstText.CompareTo(secondText);
		}
	}
}
