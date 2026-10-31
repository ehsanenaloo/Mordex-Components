using System;
using System.ComponentModel;

namespace Mordex.Data
{
	public class MordexSortFieldBase : MordexCollectionElement, IMordexSortField
	{
		protected const int ErrorCodeFieldNotInTable = 11;

		protected const int ErrorCodeFieldNull = 12;

		protected const int ErrorCodeSortFieldCustom = 20;

		private IMordexField _field;

		private SortOrder _sortOrder;

		protected MordexFieldBase BaseField
		{
			get
			{
				return (MordexFieldBase)_field;
			}
			set
			{
				if (value == null && base.CollectionOwner != null)
				{
					throw new ArgumentException(GetErrorMessage(12));
				}
				if (_field != value)
				{
					if (!IsValidField(value))
					{
						throw new InvalidOperationException(GetErrorMessage(11));
					}
					if (OnSettingsChanging())
					{
						_field = value;
						OnSettingsChanged();
					}
				}
			}
		}

		protected SortOrder BaseSortOrder
		{
			get
			{
				return _sortOrder;
			}
			set
			{
				if (!Enum.IsDefined(typeof(SortOrder), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_sortOrder != value && OnSettingsChanging())
				{
					_sortOrder = value;
					OnSettingsChanged();
				}
			}
		}

		protected MordexTableBase BaseTable => (MordexTableBase)base.CollectionOwner;

		IMordexField IMordexSortField.Field
		{
			get
			{
				return BaseField;
			}
			set
			{
				BaseField = (MordexFieldBase)value;
			}
		}

		SortOrder IMordexSortField.SortOrder
		{
			get
			{
				return BaseSortOrder;
			}
			set
			{
				BaseSortOrder = value;
			}
		}

		IMordexTable IMordexSortField.Table => BaseTable;

		public MordexSortFieldBase()
			: this(null)
		{
		}

		public MordexSortFieldBase(MordexFieldBase field)
			: this(field, SortOrder.Ascending)
		{
		}

		public MordexSortFieldBase(MordexFieldBase field, SortOrder sortOrder)
		{
			_sortOrder = sortOrder;
			_field = field;
		}

		protected virtual bool IsValidField(MordexFieldBase field)
		{
			if (BaseTable != null && ((IMordexField)field).Table != BaseTable)
			{
				return false;
			}
			return true;
		}

		protected override string GetErrorMessage(int errorCode)
		{
			switch (errorCode)
			{
			case 11:
				return "The Field you are trying to sort is not part of the table.";
			case 12:
				return "Field property can not be null (Nothing) in a SortField that is part of a collection.";
			default:
				return base.GetErrorMessage(errorCode);
			}
		}

		internal int CompareValues(object value, object otherValue)
		{
			int result = BaseField.CompareValues(value, otherValue);
			if (_sortOrder == SortOrder.Descending)
			{
				return -result;
			}
			return result;
		}
	}
}
