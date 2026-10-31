using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexFieldCollectionBase : MordexCollectionBase, IMordexFieldCollection, IEnumerable
	{
		protected MordexTableBase BaseTable => (MordexTableBase)GetOwner();

		IMordexField IMordexFieldCollection.this[int index] => (IMordexField)base.List[index];

		IMordexField IMordexFieldCollection.this[string key] => BaseGet(key);

		IMordexTable IMordexFieldCollection.Table => BaseTable;

		internal static MordexFieldCollectionBase CreateForOwner(MordexTableBase owner)
		{
			MordexFieldCollectionBase mordexFieldCollectionBase = new MordexFieldCollectionBase();
			mordexFieldCollectionBase.owner = owner;
			return mordexFieldCollectionBase;
		}

		private MordexFieldCollectionBase()
			: base(null)
		{
		}

		protected MordexFieldCollectionBase(MordexTableBase table)
			: base(table)
		{
		}

		protected void BaseInsert(int index, MordexFieldBase field)
		{
			base.List.Insert(index, field);
		}

		protected int BaseAdd(MordexFieldBase field)
		{
			return base.List.Add(field);
		}

		protected MordexFieldBase BaseGet(int index)
		{
			return (MordexFieldBase)base.List[index];
		}

		protected int BaseIndexOf(string key)
		{
			if (key == null || key.Length == 0)
			{
				return -1;
			}
			for (int i = 0; i < base.Count; i++)
			{
				IMordexField mordexField = BaseGet(i);
				if (string.Compare(mordexField.Key, key, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected int BaseIndexOf(MordexFieldBase field)
		{
			return base.List.IndexOf(field);
		}

		protected bool BaseContains(string key)
		{
			return BaseIndexOf(key) != -1;
		}

		protected bool BaseContains(MordexFieldBase field)
		{
			return base.List.Contains(field);
		}

		protected MordexFieldBase BaseGet(string key)
		{
			int index = BaseIndexOf(key);
			if (index == -1)
			{
				return null;
			}
			return BaseGet(index);
		}

		protected void BaseRemove(MordexFieldBase field)
		{
			base.List.Remove(field);
		}

		protected void BaseRemove(string key)
		{
			base.List.Remove(BaseGet(key));
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			BaseTable._suppressCollectionEvents = true;
			try
			{
				BaseTable.RemoveFieldReferences(null);
			}
			finally
			{
				BaseTable._suppressCollectionEvents = false;
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			BaseTable.NotifyFieldAdded((MordexFieldBase)value);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			BaseTable.RemoveFieldReferences((MordexFieldBase)value);
		}

		int IMordexFieldCollection.Add(IMordexField field)
		{
			return BaseAdd((MordexFieldBase)field);
		}

		void IMordexFieldCollection.Insert(int index, IMordexField field)
		{
			BaseInsert(index, (MordexFieldBase)field);
		}

		void IMordexFieldCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		void IMordexFieldCollection.Remove(IMordexField field)
		{
			BaseRemove((MordexFieldBase)field);
		}

		void IMordexFieldCollection.Remove(string key)
		{
			BaseRemove(key);
		}
	}
}
