using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexChildTableCollectionBase : MordexCollectionBase, IMordexChildTableCollection, IEnumerable
	{
		protected MordexTableBase BaseTable => (MordexTableBase)owner;

		IMordexTable IMordexChildTableCollection.this[int index] => (IMordexTable)base.List[index];

		IMordexTable IMordexChildTableCollection.this[string key] => BaseGet(key);

		IMordexTable IMordexChildTableCollection.ParentTable => BaseTable;

		internal static MordexChildTableCollectionBase CreateForOwner(MordexTableBase owner)
		{
			MordexChildTableCollectionBase mordexChildTableCollectionBase = new MordexChildTableCollectionBase();
			mordexChildTableCollectionBase.owner = owner;
			return mordexChildTableCollectionBase;
		}

		private MordexChildTableCollectionBase()
			: base(null)
		{
		}

		protected MordexChildTableCollectionBase(MordexTableBase table)
			: base(table)
		{
		}

		protected int BaseAdd(MordexTableBase table)
		{
			return base.List.Add(table);
		}

		protected MordexTableBase BaseGet(int index)
		{
			return (MordexTableBase)base.List[index];
		}

		protected int BaseIndexOf(MordexTableBase table)
		{
			return base.List.IndexOf(table);
		}

		protected int BaseIndexOf(string key)
		{
			if (key == null || key.Length == 0)
			{
				return -1;
			}
			for (int i = 0; i < base.Count; i++)
			{
				IMordexTable mordexTable = BaseGet(i);
				if (string.Compare(mordexTable.Key, key, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected MordexTableBase BaseGet(string key)
		{
			int index = BaseIndexOf(key);
			if (index == -1)
			{
				return null;
			}
			return BaseGet(index);
		}

		protected void BaseRemove(MordexTableBase table)
		{
			base.List.Remove(table);
		}

		protected void BaseRemove(string key)
		{
			int index = BaseIndexOf(key);
			base.List.RemoveAt(index);
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected bool BaseContains(string key)
		{
			return BaseIndexOf(key) != -1;
		}

		protected bool BaseContains(MordexTableBase table)
		{
			return base.List.Contains(table);
		}

		protected override bool OnInsert(int index, object value)
		{
			MordexTableBase mordexTableBase = (MordexTableBase)value;
			if (mordexTableBase.GetOwningCollection() != null)
			{
				throw new InvalidOperationException(GetErrorMessage(4));
			}
			return base.OnInsert(index, value);
		}

		protected override string GetErrorMessage(int errorCode)
		{
			if (errorCode == 4)
			{
				return "Cannot add a table when it is owned by another collection.\n\r\nRemove the table from its owner collection and try again.";
			}
			return base.GetErrorMessage(errorCode);
		}

		int IMordexChildTableCollection.Add(IMordexTable table)
		{
			return BaseAdd((MordexTableBase)table);
		}

		void IMordexChildTableCollection.Remove(IMordexTable table)
		{
			BaseRemove((MordexTableBase)table);
		}

		void IMordexChildTableCollection.Remove(string key)
		{
			BaseRemove(key);
		}

		void IMordexChildTableCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}
	}
}
