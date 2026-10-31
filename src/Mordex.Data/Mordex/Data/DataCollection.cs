using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Mordex.Data
{
	[DefaultMember("Item")]
	internal class DataCollection : CollectionBase
	{
		private CultureInfo _cultureInfo;

		private IComparer _comparer;

		private bool _ignoreCase;

		internal DataCollection(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo;
		}

		internal IComparer GetComparer()
		{
			return _comparer;
		}

		internal void SetComparer(IComparer iComparer)
		{
			_comparer = iComparer;
		}

		internal void AddDistinct(object value)
		{
			if (DataValueHelper.IsNullOrDBNull(value))
			{
				return;
			}
			int insertIndex = GetInsertIndex(value);
			if (insertIndex > -1)
			{
				if (insertIndex == base.Count)
				{
					base.InnerList.Add(value);
				}
				else
				{
					base.InnerList.Insert(insertIndex, value);
				}
			}
		}

		internal object GetItemAtInsertIndex(object value)
		{
			int insertIndex = SearchInsertIndex(value, 0, base.Count - 1);
			if (insertIndex != -1)
			{
				return GetItem(insertIndex);
			}
			return null;
		}

		private int GetInsertIndex(object value)
		{
			if (base.Count == 0)
			{
				return 0;
			}
			return SearchInsertIndex(value, 0, base.Count - 1);
		}

		private int SearchInsertIndex(object value, int integer, int high)
		{
			int comparison;
			if (high <= integer)
			{
				comparison = CompareValues(value, GetItem(integer));
				if (comparison > 0)
				{
					return integer + 1;
				}
				if (comparison < 0)
				{
					return integer;
				}
				return -1;
			}
			if (high == integer + 1)
			{
				comparison = CompareValues(value, GetItem(integer));
				if (comparison > 0)
				{
					return SearchInsertIndex(value, high, high);
				}
				if (comparison < 0)
				{
					return integer;
				}
				return -1;
			}
			int middle = integer + (high - integer) / 2;
			comparison = CompareValues(value, GetItem(middle));
			if (comparison > 0)
			{
				return SearchInsertIndex(value, middle, high);
			}
			if (comparison < 0)
			{
				return SearchInsertIndex(value, integer, middle);
			}
			return -1;
		}

		internal int CompareValues(object value, object other)
		{
			return DataValueHelper.CompareValues(_cultureInfo, value, other, _comparer, _ignoreCase, false);
		}

		internal bool GetIgnoreCase()
		{
			return _ignoreCase;
		}

		internal void SetIgnoreCase(bool boolean)
		{
			_ignoreCase = boolean;
		}

		public object GetItem(int integer)
		{
			return base.InnerList[integer];
		}
	}
}
