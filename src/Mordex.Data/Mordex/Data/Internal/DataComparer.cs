using System.Collections;
using System.Globalization;

namespace Mordex.Data.Internal
{
	internal class DataComparer : IComparer
	{
		private CultureInfo _cultureInfo;

		internal DataComparer(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo;
		}

		int IComparer.Compare(object value, object other)
		{
			object[] keys = null;
			keys = value as object[];
			if (keys == null && value is MordexDataRow mordexDataRow)
			{
				keys = mordexDataRow.DataRowKeys;
			}
			object[] otherKeys = null;
			otherKeys = other as object[];
			if (otherKeys == null && other is MordexDataRow otherRow)
			{
				otherKeys = otherRow.DataRowKeys;
			}
			if (keys == null)
			{
				if (otherKeys == null)
				{
					return 0;
				}
				return -1;
			}
			if (otherKeys == null)
			{
				return 1;
			}
			if (otherKeys.Length != keys.Length)
			{
				return keys.Length.CompareTo(otherKeys.Length);
			}
			for (int i = 0; i < keys.Length; i++)
			{
				int comparison = DataValueHelper.CompareValues(_cultureInfo, keys[i], otherKeys[i], null, true, false);
				if (comparison != 0)
				{
					return comparison;
				}
			}
			return 0;
		}
	}
}
