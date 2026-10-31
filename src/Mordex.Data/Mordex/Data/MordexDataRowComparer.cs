using System.Collections;

namespace Mordex.Data
{
	public class MordexDataRowComparer : IComparer
	{
		internal long _compareCount;

		private MordexSortFieldBase[] _sortFields;

		public MordexDataRowComparer(IMordexTable table)
		{
			_sortFields = new MordexSortFieldBase[table.SortFields.Count];
			for (int i = 0; i < table.SortFields.Count; i++)
			{
				_sortFields[i] = (MordexSortFieldBase)table.SortFields[i];
			}
		}

		internal bool IsSorted(ArrayList arrayList)
		{
			for (int i = arrayList.Count - 1; i > 0; i--)
			{
				MordexDataRow drx = arrayList[i] as MordexDataRow;
				MordexDataRow dry = arrayList[i - 1] as MordexDataRow;
				if (Compare(drx, dry) < 0)
				{
					return false;
				}
			}
			return true;
		}

		public int Compare(MordexDataRow drx, MordexDataRow dry)
		{
			_compareCount++;
			int result = 0;
			for (int i = 0; i < _sortFields.Length; i++)
			{
				result = _sortFields[i].CompareValues(drx._sortValues[i], dry._sortValues[i]);
				if (result != 0)
				{
					return result;
				}
			}
			return drx.Table.CompareRows(drx, dry);
		}

		int IComparer.Compare(object value, object otherValue)
		{
			return Compare((MordexDataRow)value, (MordexDataRow)otherValue);
		}
	}
}
