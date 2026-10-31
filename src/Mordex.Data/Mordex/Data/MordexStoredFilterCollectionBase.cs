using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexStoredFilterCollectionBase : MordexCollectionBase, IMordexStoredFilterCollection, IEnumerable
	{
		protected MordexTableBase BaseTable => (MordexTableBase)GetOwner();

		IMordexTable IMordexStoredFilterCollection.Table => BaseTable;

		IMordexStoredFilter IMordexStoredFilterCollection.this[int index] => (IMordexStoredFilter)base.List[index];

		IMordexStoredFilter IMordexStoredFilterCollection.this[string key] => BaseGet(key);

		internal static MordexStoredFilterCollectionBase CreateForOwner(MordexTableBase owner)
		{
			MordexStoredFilterCollectionBase mordexStoredFilterCollectionBase = new MordexStoredFilterCollectionBase();
			mordexStoredFilterCollectionBase.owner = owner;
			return mordexStoredFilterCollectionBase;
		}

		private MordexStoredFilterCollectionBase()
			: base(null)
		{
		}

		protected MordexStoredFilterCollectionBase(MordexTableBase table)
			: base(table)
		{
		}

		protected int BaseAdd(MordexStoredFilterBase storedFilter)
		{
			return base.List.Add(storedFilter);
		}

		protected MordexStoredFilterBase BaseGet(int index)
		{
			return (MordexStoredFilterBase)base.List[index];
		}

		protected int BaseIndexOf(string key)
		{
			if (key == null || key.Length == 0)
			{
				return -1;
			}
			for (int i = 0; i < base.Count; i++)
			{
				IMordexStoredFilter mordexStoredFilter = (IMordexStoredFilter)base.List[i];
				if (string.Compare(mordexStoredFilter.Key, key, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected int BaseIndexOf(MordexStoredFilterBase storedFilter)
		{
			return base.List.IndexOf(storedFilter);
		}

		protected bool BaseContains(MordexStoredFilterBase storedFilter)
		{
			return base.List.Contains(storedFilter);
		}

		protected bool BaseContains(string key)
		{
			return BaseIndexOf(key) != -1;
		}

		protected MordexStoredFilterBase BaseGet(string key)
		{
			int index = BaseIndexOf(key);
			if (index == -1)
			{
				return null;
			}
			return BaseGet(index);
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected void BaseRemove(MordexStoredFilterBase storedFilter)
		{
			base.List.Remove(storedFilter);
		}

		protected void BaseRemove(string key)
		{
			int index = BaseIndexOf(key);
			base.List.RemoveAt(index);
		}

		protected void BaseInsert(int index, MordexStoredFilterBase storedFilter)
		{
			base.List.Insert(index, storedFilter);
		}

		void IMordexStoredFilterCollection.Clear()
		{
			Clear();
		}

		int IMordexStoredFilterCollection.Add(IMordexStoredFilter storedFilter)
		{
			return BaseAdd((MordexStoredFilterBase)storedFilter);
		}

		void IMordexStoredFilterCollection.Remove(string key)
		{
			BaseRemove(key);
		}

		void IMordexStoredFilterCollection.Remove(IMordexStoredFilter storedFilter)
		{
			BaseRemove((MordexStoredFilterBase)storedFilter);
		}

		void IMordexStoredFilterCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}
	}
}
