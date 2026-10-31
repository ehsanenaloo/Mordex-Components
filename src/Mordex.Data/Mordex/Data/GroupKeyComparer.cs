using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	internal class GroupKeyComparer : IComparer, IEqualityComparer
	{
		private IMordexGroup _group;

		private DataSourceManager _manager;

		internal GroupKeyComparer(DataSourceManager manager, IMordexGroup group)
		{
			_manager = manager;
			_group = group;
		}

		int IComparer.Compare(object value, object otherValue)
		{
			IList list = value as IList;
			IList otherList = otherValue as IList;
			if (list == null)
			{
				if (otherList == null)
				{
					return 0;
				}
				return -1;
			}
			if (otherList == null)
			{
				return 1;
			}
			if (_group.Fields == null || list.Count != otherList.Count || list.Count != _group.Fields.Length)
			{
				throw new InvalidOperationException();
			}
			IMordexField[] fields = _group.Fields;
			for (int i = 0; i < fields.Length; i++)
			{
				IComparer groupComparer = fields[i].GroupComparer;
				int result = DataValueHelper.CompareValues(_manager.CurrentCulture, list[i], otherList[i], groupComparer, fields[i].CompareIgnoreCase, false);
				if (result != 0)
				{
					return result;
				}
			}
			return 0;
		}

		bool IEqualityComparer.Equals(object value, object otherValue)
		{
			return ((IComparer)this).Compare(value, otherValue) == 0;
		}

		int IEqualityComparer.GetHashCode(object value)
		{
			if (!(value is IList list) || _group.Fields == null)
			{
				return 0;
			}
			IMordexField[] fields = _group.Fields;
			IComparer[] comparers = new IComparer[fields.Length];
			bool[] ignoreCase = new bool[fields.Length];
			for (int i = 0; i < fields.Length; i++)
			{
				comparers[i] = fields[i].GroupComparer;
				ignoreCase[i] = fields[i].CompareIgnoreCase;
			}
			return GetKeyHashCode(_manager.CurrentCulture, list, comparers, ignoreCase);
		}

		// Must agree with Compare: keys that compare equal get the same hash. Text is hashed in the given culture with
		// the field's case rule, null and DBNull hash the same, and a field with a custom comparer adds nothing unless
		// that comparer can hash.
		internal static int GetKeyHashCode(CultureInfo culture, IList values, IComparer[] comparers, bool[] ignoreCase)
		{
			int hash = 17;
			for (int i = 0; i < values.Count; i++)
			{
				IComparer comparer = i < comparers.Length ? comparers[i] : null;
				bool fieldIgnoresCase = i < ignoreCase.Length && ignoreCase[i];
				hash = unchecked(hash * 31 + GetValueHashCode(culture, values[i], comparer, fieldIgnoresCase));
			}
			return hash;
		}

		private static int GetValueHashCode(CultureInfo culture, object value, IComparer comparer, bool ignoreCase)
		{
			if (comparer != null)
			{
				return comparer is IEqualityComparer equality && value != null ? equality.GetHashCode(value) : 0;
			}
			if (DataValueHelper.IsNullOrDBNull(value))
			{
				return 0;
			}
			if (value is string text)
			{
				return (culture ?? CultureInfo.CurrentCulture).CompareInfo.GetHashCode(text, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
			}
			return value.GetHashCode();
		}
	}
}
