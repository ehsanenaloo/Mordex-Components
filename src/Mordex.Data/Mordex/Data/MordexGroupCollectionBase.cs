using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexGroupCollectionBase : MordexCollectionBase, IMordexGroupCollection, IEnumerable
	{
		protected const int ErrorCodeFieldNotSet = 11;

		protected const int ErrorCodeFieldNotInTable = 12;

		protected const int ErrorCodeGroupCollectionCustom = 20;

		protected MordexTableBase BaseTable => (MordexTableBase)GetOwner();

		IMordexTable IMordexGroupCollection.Table => BaseTable;

		IMordexGroup IMordexGroupCollection.this[int index] => (IMordexGroup)base.List[index];

		internal static MordexGroupCollectionBase CreateForOwner(MordexTableBase owner)
		{
			MordexGroupCollectionBase mordexGroupCollectionBase = new MordexGroupCollectionBase();
			mordexGroupCollectionBase.owner = owner;
			return mordexGroupCollectionBase;
		}

		private MordexGroupCollectionBase()
			: base(null)
		{
		}

		protected MordexGroupCollectionBase(MordexTableBase table)
			: base(table)
		{
		}

		protected int BaseAdd(MordexGroupBase group)
		{
			return base.List.Add(group);
		}

		protected MordexGroupBase BaseGet(int index)
		{
			return (MordexGroupBase)base.List[index];
		}

		public int IndexOf(string fieldKey)
		{
			if (fieldKey == null || fieldKey.Length == 0)
			{
				return -1;
			}
			for (int i = 0; i < base.Count; i++)
			{
				IMordexGroup mordexGroup = BaseGet(i);
				if (mordexGroup.Field != null && string.Compare(mordexGroup.Field.Key, fieldKey, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected MordexGroupBase BaseGet(string fieldKey)
		{
			int index = IndexOf(fieldKey);
			if (index == -1)
			{
				return null;
			}
			return BaseGet(index);
		}

		protected bool BaseContains(MordexGroupBase group)
		{
			return base.List.Contains(group);
		}

		protected int BaseIndexOf(MordexGroupBase group)
		{
			return base.List.IndexOf(group);
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected void BaseRemove(MordexGroupBase group)
		{
			base.List.Remove(group);
		}

		protected void BaseRemove(string fieldKey)
		{
			int index = IndexOf(fieldKey);
			base.List.RemoveAt(index);
		}

		protected void BaseInsert(int index, MordexGroupBase group)
		{
			base.List.Insert(index, group);
		}

		protected override bool OnInsert(int index, object value)
		{
			MordexGroupBase mordexGroupBase = (MordexGroupBase)value;
			if (mordexGroupBase.GetOwningCollection() != null)
			{
				throw new InvalidOperationException(GetErrorMessage(4));
			}
			IMordexGroup mordexGroup = (IMordexGroup)value;
			if (mordexGroup.Field == null && mordexGroup.CustomGroup == null && mordexGroup.Fields == null)
			{
				throw new ArgumentException(GetErrorMessage(11));
			}
			if (mordexGroup.Field != null && mordexGroup.Field.MultipleValues)
			{
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							IMordexGroup existingGroup = (IMordexGroup)enumerator.Current;
							if (existingGroup.Field != null && existingGroup.Field.MultipleValues)
							{
								throw new InvalidOperationException("Can not group by more than one MultipleValue fields at the same time.");
							}
						}
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
			}
			if (!BaseTable.CanUseGroup(mordexGroupBase))
			{
				throw new InvalidOperationException(GetErrorMessage(12));
			}
			return base.OnInsert(index, value);
		}

		protected override string GetErrorMessage(int errorCode)
		{
			switch (errorCode)
			{
			case 12:
				return "The Field specified in the group being added must belong to the table this collection belongs.";
			case 4:
				return "Cannot add a group when it is owned by another collection.\n\r\nRemove the group from its owner collection and try again.";
			case 11:
				return "Field must be specified in the group before adding it to the collection.";
			default:
				return base.GetErrorMessage(errorCode);
			}
		}

		int IMordexGroupCollection.Add(IMordexGroup group)
		{
			return BaseAdd((MordexGroupBase)group);
		}

		IMordexGroup IMordexGroupCollection.Add(IMordexField field, SortOrder sortOrder)
		{
			IMordexGroup mordexGroup = CreateGroup();
			mordexGroup.Field = field;
			mordexGroup.SortOrder = sortOrder;
			BaseAdd((MordexGroupBase)mordexGroup);
			return mordexGroup;
		}

		void IMordexGroupCollection.Remove(IMordexGroup group)
		{
			BaseRemove((MordexGroupBase)group);
		}

		void IMordexGroupCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		internal MordexGroupBase CreateNewGroup()
		{
			return CreateGroup();
		}

		protected virtual MordexGroupBase CreateGroup()
		{
			return MordexGroupBase.CreateDefault();
		}
	}
}
