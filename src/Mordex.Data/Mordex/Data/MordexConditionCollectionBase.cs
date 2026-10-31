using System;
using System.Collections;

namespace Mordex.Data
{
	public class MordexConditionCollectionBase : MordexCollectionBase, IMordexConditionCollection, IEnumerable
	{
		int IMordexConditionCollection.Count => base.Count;

		IMordexCondition IMordexConditionCollection.this[int index] => BaseGet(index);

		internal static MordexConditionCollectionBase CreateForOwner(MordexConditionBase owner)
		{
			MordexConditionCollectionBase mordexConditionCollectionBase = new MordexConditionCollectionBase();
			mordexConditionCollectionBase.owner = owner;
			return mordexConditionCollectionBase;
		}

		private MordexConditionCollectionBase()
			: base(null)
		{
		}

		protected MordexConditionCollectionBase(MordexConditionBase owner)
			: base(owner)
		{
		}

		protected int BaseAdd(MordexConditionBase condition)
		{
			return base.List.Add(condition);
		}

		internal void AddWithDefaultOperator(MordexConditionBase mordexConditionBase)
		{
			if (((IMordexCondition)mordexConditionBase).LogicalOperator == LogicalOperator.None)
			{
				((IMordexCondition)mordexConditionBase).LogicalOperator = LogicalOperator.And;
			}
			base.List.Add(mordexConditionBase);
		}

		internal void AddWithOperator(LogicalOperator logicalOperator, MordexConditionBase mordexConditionBase)
		{
			((IMordexCondition)mordexConditionBase).LogicalOperator = logicalOperator;
			AddWithDefaultOperator(mordexConditionBase);
		}

		internal void RemoveCondition(MordexConditionBase mordexConditionBase)
		{
			base.List.Remove(mordexConditionBase);
		}

		protected MordexConditionBase BaseGet(int index)
		{
			return (MordexConditionBase)base.List[index];
		}

		protected override bool OnInsert(int index, object value)
		{
			if (!(value is MordexConditionBase))
			{
				throw new ArgumentException("Only MordexConditionBase objects can be added to the collection.");
			}
			return base.OnInsert(index, value);
		}

		internal void AddClonesFrom(MordexConditionCollectionBase mordexConditionCollectionBase)
		{
			foreach (MordexConditionBase item in mordexConditionCollectionBase)
			{
				MordexConditionBase condition = (MordexConditionBase)((ICloneable)item).Clone();
				BaseAdd(condition);
			}
		}
	}
}
