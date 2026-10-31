using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	internal class ValueItemComparer : IComparer
	{
		private bool _useValue;

		private CultureInfo _culture;

		internal ValueItemComparer(CultureInfo cInfo, bool useValue)
		{
			_useValue = useValue;
			_culture = cInfo;
		}

		public int Compare(object value, object other)
		{
			IMordexValueItem mordexValueItem = value as IMordexValueItem;
			IMordexValueItem otherItem = other as IMordexValueItem;
			if (mordexValueItem == null || otherItem == null)
			{
				throw new InvalidOperationException();
			}
			if (_useValue)
			{
				return DataValueHelper.CompareValues(_culture, mordexValueItem.Value, otherItem.Value, null, true, false);
			}
			return string.Compare(mordexValueItem.Text, otherItem.Text, ignoreCase: true, _culture);
		}
	}
}
