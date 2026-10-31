using System.Collections;

namespace Mordex.Data
{
	public class MordexCustomGroupCollectionBase : MordexCollectionBase, IMordexCustomGroupCollection, IEnumerable
	{
		int IMordexCustomGroupCollection.Count => base.Count;

		IMordexTable IMordexCustomGroupCollection.Table => (IMordexTable)GetOwner();

		IMordexCustomGroup IMordexCustomGroupCollection.this[int index] => BaseGet(index);

		IMordexCustomGroup IMordexCustomGroupCollection.this[string key] => BaseGet(key);

		internal static MordexCustomGroupCollectionBase CreateForOwner(MordexTableBase owner)
		{
			return new MordexCustomGroupCollectionBase(owner);
		}

		protected MordexCustomGroupCollectionBase(MordexTableBase owner)
			: base(owner)
		{
		}

		protected int BaseAdd(MordexCustomGroupBase item)
		{
			return base.List.Add(item);
		}

		protected void BaseInsert(int index, MordexCustomGroupBase customGroup)
		{
			base.List.Insert(index, customGroup);
		}

		protected MordexCustomGroupBase BaseGet(int index)
		{
			return (MordexCustomGroupBase)base.List[index];
		}

		protected MordexCustomGroupBase BaseGet(string key)
		{
			return BaseGet(BaseIndexOf(key));
		}

		protected void BaseRemove(MordexCustomGroupBase item)
		{
			base.List.Remove(item);
		}

		protected void BaseRemove(string key)
		{
			int index = BaseIndexOf(key);
			BaseRemoveAt(index);
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected int BaseIndexOf(MordexCustomGroupBase item)
		{
			return base.List.IndexOf(item);
		}

		protected int BaseIndexOf(string key)
		{
			for (int i = 0; i < base.List.Count; i++)
			{
				IMordexCustomGroup mordexCustomGroup = (IMordexCustomGroup)base.List[i];
				if (string.Compare(mordexCustomGroup.Key, key, ignoreCase: true) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected bool BaseContains(string key)
		{
			return BaseIndexOf(key) != -1;
		}

		protected bool BaseContains(MordexCustomGroupBase customGroup)
		{
			return base.List.Contains(customGroup);
		}

		int IMordexCustomGroupCollection.Add(IMordexCustomGroup customGroup)
		{
			return BaseAdd((MordexCustomGroupBase)customGroup);
		}

		void IMordexCustomGroupCollection.Remove(IMordexCustomGroup customGroup)
		{
			BaseRemove((MordexCustomGroupBase)customGroup);
		}

		void IMordexCustomGroupCollection.Remove(string key)
		{
			BaseRemove(key);
		}

		void IMordexCustomGroupCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		void IMordexCustomGroupCollection.Insert(int index, IMordexCustomGroup customGroup)
		{
			BaseInsert(index, (MordexCustomGroupBase)customGroup);
		}
	}
}
