using System;

namespace Mordex.Data
{
	public class MordexStoredFilterBase : MordexCollectionElement, IFilterCondition, IConditionOwner, IMordexStoredFilter
	{
		private MordexConditionBase _filterCondition;

		private string _key;

		private string _description;

		private object _tag;

		private bool _isCurrent;

		protected MordexConditionBase BaseFilterCondition
		{
			get
			{
				return _filterCondition;
			}
			set
			{
				if (_filterCondition != value)
				{
					if (value != null && value.GetConditionOwner() != null)
					{
						throw new InvalidOperationException("Cannot use a condition associated with another stored filter or assigned as filter in a table.");
					}
					if (_filterCondition != null)
					{
						_filterCondition.SetConditionOwner(null);
					}
					_filterCondition = value;
					if (_filterCondition != null)
					{
						_filterCondition.SetConditionOwner(this);
					}
				}
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

		protected string BaseDescription
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		protected object BaseTag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		protected MordexTableBase BaseTable => (MordexTableBase)base.CollectionOwner;

		IMordexCondition IFilterCondition.FilterCondition => BaseFilterCondition;

		string IMordexStoredFilter.Key
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

		object IMordexStoredFilter.Tag
		{
			get
			{
				return BaseTag;
			}
			set
			{
				BaseTag = value;
			}
		}

		string IMordexStoredFilter.Description
		{
			get
			{
				return BaseDescription;
			}
			set
			{
				BaseDescription = value;
			}
		}

		IMordexTable IMordexStoredFilter.Table => BaseTable;

		IMordexCondition IMordexStoredFilter.Condition
		{
			get
			{
				return BaseFilterCondition;
			}
			set
			{
				BaseFilterCondition = (MordexConditionBase)value;
			}
		}

		IMordexTable IConditionOwner.Table => BaseTable;

		protected bool BaseIsCurrent
		{
			get
			{
				return _isCurrent;
			}
			set
			{
				_isCurrent = value;
			}
		}

		public MordexStoredFilterBase()
		{
			_key = "";
			_description = "";
		}

		internal void SetIsCurrent(bool boolean)
		{
			BaseIsCurrent = boolean;
		}
	}
}
