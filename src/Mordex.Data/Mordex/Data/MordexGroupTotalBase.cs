namespace Mordex.Data
{
	public class MordexGroupTotalBase : MordexCollectionElement, IMordexGroupTotal, IConditionOwner
	{
		private MordexFieldBase _field;

		private AggregateFunction _aggregateFunction;

		private string _key;

		private MordexConditionBase _condition;

		protected MordexConditionBase BaseCondition
		{
			get
			{
				return _condition;
			}
			set
			{
				if (value != _condition)
				{
					_condition = value;
				}
			}
		}

		protected MordexFieldBase BaseField
		{
			get
			{
				return _field;
			}
			set
			{
				_field = value;
			}
		}

		protected AggregateFunction BaseAggregateFunction
		{
			get
			{
				return _aggregateFunction;
			}
			set
			{
				_aggregateFunction = value;
			}
		}

		protected MordexTableBase BaseTable => (MordexTableBase)base.CollectionOwner;

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

		AggregateFunction IMordexGroupTotal.AggregateFunction
		{
			get
			{
				return BaseAggregateFunction;
			}
			set
			{
				BaseAggregateFunction = value;
			}
		}

		IMordexField IMordexGroupTotal.Field
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

		IMordexCondition IMordexGroupTotal.Condition
		{
			get
			{
				return BaseCondition;
			}
			set
			{
				BaseCondition = (MordexConditionBase)value;
			}
		}

		IMordexTable IMordexGroupTotal.Table => BaseTable;

		string IMordexGroupTotal.Key
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

		IMordexTable IConditionOwner.Table => BaseTable;

		public MordexGroupTotalBase()
		{
			_key = "";
			_aggregateFunction = AggregateFunction.None;
		}
	}
}
