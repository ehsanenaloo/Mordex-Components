using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexGroupTotalCollectionBase : MordexCollectionBase, IMordexGroupTotalCollection, IEnumerable
	{
		protected const int ErrorCodeFieldNotSet = 11;

		protected const int ErrorCodeFieldNotInTable = 12;

		protected const int ErrorCodeGroupCollectionCustom = 20;

		protected MordexTableBase BaseTable => (MordexTableBase)GetOwner();

		IMordexTable IMordexGroupTotalCollection.Table => BaseTable;

		IMordexGroupTotal IMordexGroupTotalCollection.this[int index] => (IMordexGroupTotal)base.List[index];

		IMordexGroupTotal IMordexGroupTotalCollection.this[string key] => BaseGet(key);

		internal static MordexGroupTotalCollectionBase CreateForOwner(MordexTableBase owner)
		{
			MordexGroupTotalCollectionBase mordexGroupTotalCollectionBase = new MordexGroupTotalCollectionBase();
			mordexGroupTotalCollectionBase.owner = owner;
			return mordexGroupTotalCollectionBase;
		}

		private MordexGroupTotalCollectionBase()
			: base(null)
		{
		}

		protected MordexGroupTotalCollectionBase(MordexTableBase table)
			: base(table)
		{
		}

		protected int BaseAdd(MordexGroupTotalBase group)
		{
			return base.List.Add(group);
		}

		protected MordexGroupTotalBase BaseGet(int index)
		{
			return (MordexGroupTotalBase)base.List[index];
		}

		public int IndexOf(string key)
		{
			if (key == null || key.Length == 0)
			{
				return -1;
			}
			for (int i = 0; i < base.Count; i++)
			{
				IMordexGroupTotal mordexGroupTotal = BaseGet(i);
				if (string.Compare(mordexGroupTotal.Key, key, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected MordexGroupTotalBase BaseGet(string key)
		{
			int index = IndexOf(key);
			if (index == -1)
			{
				return null;
			}
			return BaseGet(index);
		}

		protected bool BaseContains(MordexGroupTotalBase groupTotal)
		{
			return base.List.Contains(groupTotal);
		}

		protected bool BaseContains(string key)
		{
			return BaseIndexOf(key) != -1;
		}

		protected int BaseIndexOf(MordexGroupTotalBase groupTotal)
		{
			return base.List.IndexOf(groupTotal);
		}

		protected int BaseIndexOf(string key)
		{
			for (int i = 0; i < base.Count; i++)
			{
				IMordexGroupTotal mordexGroupTotal = (IMordexGroupTotal)base.List[i];
				if (string.Compare(mordexGroupTotal.Key, key, ignoreCase: true) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected void BaseRemove(MordexGroupTotalBase groupTotal)
		{
			base.List.Remove(groupTotal);
		}

		protected void BaseRemove(string key)
		{
			int index = IndexOf(key);
			base.List.RemoveAt(index);
		}

		protected void BaseInsert(int index, MordexGroupTotalBase groupTotal)
		{
			base.List.Insert(index, groupTotal);
		}

		protected override bool OnInsert(int index, object value)
		{
			MordexGroupTotalBase mordexGroupTotalBase = (MordexGroupTotalBase)value;
			if (mordexGroupTotalBase.GetOwningCollection() != null)
			{
				throw new InvalidOperationException(GetErrorMessage(4));
			}
			return base.OnInsert(index, value);
		}

		int IMordexGroupTotalCollection.Add(IMordexGroupTotal groupTotal)
		{
			return BaseAdd((MordexGroupTotalBase)groupTotal);
		}

		void IMordexGroupTotalCollection.Remove(IMordexGroupTotal groupTotal)
		{
			BaseRemove((MordexGroupTotalBase)groupTotal);
		}

		void IMordexGroupTotalCollection.Remove(string key)
		{
			BaseRemove(key);
		}

		void IMordexGroupTotalCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}
	}
}
