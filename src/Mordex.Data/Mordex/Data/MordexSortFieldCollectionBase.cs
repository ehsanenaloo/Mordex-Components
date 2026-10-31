using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Data
{
	public class MordexSortFieldCollectionBase : MordexCollectionBase, IMordexSortFieldCollection, IEnumerable
	{
		protected const int ErrorCodeFieldNotSet = 11;

		protected const int ErrorCodeFieldNotInTable = 12;

		protected MordexTableBase BaseTable => (MordexTableBase)owner;

		IMordexTable IMordexSortFieldCollection.Table => BaseTable;

		IMordexSortField IMordexSortFieldCollection.this[int index] => (IMordexSortField)base.List[index];

		internal static MordexSortFieldCollectionBase CreateForOwner(MordexTableBase owner)
		{
			MordexSortFieldCollectionBase mordexSortFieldCollectionBase = new MordexSortFieldCollectionBase();
			mordexSortFieldCollectionBase.owner = owner;
			return mordexSortFieldCollectionBase;
		}

		private MordexSortFieldCollectionBase()
			: base(null)
		{
		}

		protected MordexSortFieldCollectionBase(MordexTableBase table)
			: base(table)
		{
		}

		protected int BaseAdd(MordexSortFieldBase sortField)
		{
			return base.List.Add(sortField);
		}

		protected MordexSortFieldBase BaseGet(int index)
		{
			return (MordexSortFieldBase)base.List[index];
		}

		protected MordexSortFieldBase BaseGet(MordexFieldBase field)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					IMordexSortField mordexSortField = (IMordexSortField)enumerator.Current;
					if (mordexSortField.Field == field)
					{
						return (MordexSortFieldBase)mordexSortField;
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
			return null;
		}

		protected int BaseIndexOf(string fieldKey)
		{
			if (fieldKey == null || fieldKey.Length == 0)
			{
				return -1;
			}
			for (int i = 0; i < base.Count; i++)
			{
				IMordexSortField mordexSortField = BaseGet(i);
				if (mordexSortField.Field != null)
				{
					IMordexField field = mordexSortField.Field;
					if (string.Compare(field.Key, fieldKey, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						return i;
					}
				}
			}
			return -1;
		}

		protected MordexSortFieldBase BaseGet(string fieldKey)
		{
			int index = BaseIndexOf(fieldKey);
			if (index == -1)
			{
				return null;
			}
			return BaseGet(index);
		}

		protected void BaseRemove(MordexSortFieldBase sortField)
		{
			base.List.Remove(sortField);
		}

		protected void BaseRemove(string fieldKey)
		{
			int index = BaseIndexOf(fieldKey);
			base.List.RemoveAt(index);
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected override bool OnInsert(int index, object value)
		{
			MordexSortFieldBase mordexSortFieldBase = (MordexSortFieldBase)value;
			if (mordexSortFieldBase.GetOwningCollection() != null)
			{
				throw new InvalidOperationException(GetErrorMessage(4));
			}
			IMordexSortField mordexSortField = mordexSortFieldBase;
			if (mordexSortField.Field == null)
			{
				throw new ArgumentException(GetErrorMessage(11));
			}
			if (mordexSortField.Field.Table != BaseTable)
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
				return "The Field specified in the SortField being added must belong to the table this collection belongs.";
			case 4:
				return "Cannot add a sort field when it is owned by another collection.\n\r\nRemove the field from its owner collection and try again.";
			case 11:
				return "Field must be specified in the SortField before adding it to the collection.";
			default:
				return base.GetErrorMessage(errorCode);
			}
		}

		int IMordexSortFieldCollection.Add(IMordexSortField sortField)
		{
			return BaseAdd((MordexSortFieldBase)sortField);
		}

		void IMordexSortFieldCollection.Clear()
		{
			Clear();
		}

		void IMordexSortFieldCollection.Remove(IMordexSortField sortField)
		{
			BaseRemove((MordexSortFieldBase)sortField);
		}

		void IMordexSortFieldCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}
	}
}
