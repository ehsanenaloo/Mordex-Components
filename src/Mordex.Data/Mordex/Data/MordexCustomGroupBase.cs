using System;
using System.ComponentModel;

namespace Mordex.Data
{
	public class MordexCustomGroupBase : MordexCollectionElement, ICollectionOwner, IMordexCustomGroup
	{
		private CustomGroupType _customGroupType;

		private MordexCustomGroupRowCollectionBase _groupRows;

		private MordexFieldBase[] _compositeFields;

		private string _key;

		protected MordexFieldBase[] BaseCompositeFields
		{
			get
			{
				return _compositeFields;
			}
			set
			{
				if (value != null)
				{
					IMordexTable mordexTable = null;
					for (int i = 0; i < value.Length; i++)
					{
						if (value[i] == null)
						{
							throw new ArgumentException("Field can not be null.");
						}
						IMordexField mordexField = value[i];
						if (mordexField.MultipleValues && value.Length > 1)
						{
							throw new ArgumentException("MultipleValues fields can not be used in a CompositeField group.");
						}
						if (mordexTable == null)
						{
							mordexTable = mordexField.Table;
						}
						else if (mordexTable != mordexField.Table)
						{
							throw new InvalidOperationException("Fields in multiple fields group should belong to the same table.");
						}
					}
				}
				_compositeFields = value;
			}
		}

		protected string BaseKey
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		protected CustomGroupType BaseCustomGroupType
		{
			get
			{
				return _customGroupType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CustomGroupType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_customGroupType != value)
				{
					_customGroupType = value;
				}
			}
		}

		protected MordexCustomGroupRowCollectionBase BaseGroupRows => _groupRows;

		IMordexCustomGroupRowCollection IMordexCustomGroup.GroupRows => BaseGroupRows;

		CustomGroupType IMordexCustomGroup.CustomGroupType
		{
			get
			{
				return BaseCustomGroupType;
			}
			set
			{
				BaseCustomGroupType = value;
			}
		}

		IMordexField[] IMordexCustomGroup.CompositeFields
		{
			get
			{
				return BaseCompositeFields;
			}
			set
			{
				if (value != null)
				{
					MordexFieldBase[] fields = new MordexFieldBase[value.Length];
					for (int i = 0; i < value.Length; i++)
					{
						fields[i] = (MordexFieldBase)value[i];
					}
					BaseCompositeFields = fields;
				}
				else
				{
					BaseCompositeFields = null;
				}
			}
		}

		string IMordexCustomGroup.Key
		{
			get
			{
				return BaseKey;
			}
			set
			{
				BaseKey = value;
			}
		}

		protected MordexCustomGroupBase()
		{
			_groupRows = CreateGroupRows();
			_key = string.Empty;
			_customGroupType = CustomGroupType.ConditionalGroupRows;
		}

		internal MordexFieldBase[] GetCompositeFields()
		{
			return BaseCompositeFields;
		}

		protected virtual MordexCustomGroupRowCollectionBase CreateGroupRows()
		{
			return MordexCustomGroupRowCollectionBase.CreateForOwner(this);
		}

		void ICollectionOwner.OnCollectionChanged(object collection, CollectionChangeType change, object item)
		{
		}

		CollectionChangingAction ICollectionOwner.OnCollectionChanging(MordexCollectionBase collection, CollectionChangeType change, object item)
		{
			return CollectionChangingAction.Continue;
		}
	}
}
