using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerFilterConditionCollection : ICollection, IEnumerable, ICloneable, IMordexXmlLayoutsSupport
	{
		private ArrayList _conditions;

		private SchedulerFilterCondition _schedulerFilterCondition;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "FilterCondition";

		public SchedulerFilterCondition this[int index]
		{
			get
			{
				return (SchedulerFilterCondition)_conditions[index];
			}
			set
			{
				ValidateReplace(_conditions[index], value);
				_conditions[index] = value;
			}
		}

		public SchedulerFilterCondition this[string key]
		{
			get
			{
				foreach (SchedulerFilterCondition item in (IEnumerable)this)
				{
					if (string.Compare(item.Key, key, ignoreCase: true) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' in this collection.");
			}
		}

		public int Count => _conditions.Count;

		bool ICollection.IsSynchronized => _conditions.IsSynchronized;

		object ICollection.SyncRoot => _conditions.SyncRoot;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int conditionIndex = 0;
			for (PropertyValue propertyValue = info["FilterCondition" + conditionIndex]; propertyValue != null; propertyValue = info["FilterCondition" + conditionIndex])
			{
				SchedulerFilterCondition schedulerFilterCondition = new SchedulerFilterCondition();
				schedulerFilterCondition._filterOwner = _schedulerFilterCondition._filterOwner;
				propertyValue.DeserializeObject(schedulerFilterCondition);
				schedulerFilterCondition._filterOwner = null;
				_conditions.Add(schedulerFilterCondition);
				schedulerFilterCondition._owner = this;
				conditionIndex++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int conditionIndex = 0;
			foreach (SchedulerFilterCondition item in (IEnumerable)this)
			{
				info.AddValue("FilterCondition" + conditionIndex, item);
				conditionIndex++;
			}
		}

		internal SchedulerFilterConditionCollection(SchedulerFilterCondition condition)
		{
			SetSchedulerFilterCondition(condition);
			_conditions = new ArrayList();
		}

		public void Remove(SchedulerFilterCondition condition)
		{
			ValidateRemove(condition);
			_conditions.Remove(condition);
			SetOwner(condition);
		}

		private void ValidateReplace(object value, object valueArgument)
		{
			if (valueArgument == null)
			{
				throw new ArgumentNullException();
			}
			if (value != valueArgument)
			{
				SchedulerFilterCondition schedulerFilterCondition = (SchedulerFilterCondition)valueArgument;
				if (schedulerFilterCondition._filterOwner != null || schedulerFilterCondition._owner != null)
				{
					throw new InvalidOperationException("The FilterCondition already belongs to another object.");
				}
			}
		}

		internal void AddWithOperator(LogicalOperator logicalOperator, SchedulerFilterCondition condition)
		{
			condition.LogicalOperator = logicalOperator;
			Add(condition);
		}

		public void Add(SchedulerFilterCondition condition)
		{
			if (condition == null)
			{
				throw new ArgumentNullException();
			}
			if (condition.LogicalOperator == LogicalOperator.None)
			{
				condition.LogicalOperator = LogicalOperator.And;
			}
			ValidateAdd(-1, condition);
			_conditions.Add(condition);
			AttachCondition(Count - 1, condition);
		}

		public int IndexOf(SchedulerFilterCondition condition)
		{
			return _conditions.IndexOf(condition);
		}

		internal SchedulerFilterCondition GetSchedulerFilterCondition()
		{
			return _schedulerFilterCondition;
		}

		internal void SetSchedulerFilterCondition(SchedulerFilterCondition schedulerFilterCondition)
		{
			_schedulerFilterCondition = schedulerFilterCondition;
		}

		object ICloneable.Clone()
		{
			SchedulerFilterConditionCollection schedulerFilterConditionCollection = new SchedulerFilterConditionCollection(null);
			foreach (SchedulerFilterCondition item in (IEnumerable)this)
			{
				schedulerFilterConditionCollection.Add((SchedulerFilterCondition)item.Clone());
			}
			return schedulerFilterConditionCollection;
		}

		private void UpdateSchedulerFilterConditionCollection()
		{
			if (_schedulerFilterCondition != null && _schedulerFilterCondition._filterOwner != null)
			{
				throw new InvalidOperationException(SchedulerFilterCondition._text);
			}
			foreach (SchedulerFilterCondition item in (IEnumerable)this)
			{
				item._owner = null;
			}
		}

		internal void ClearConditions()
		{
			UpdateSchedulerFilterConditionCollection();
			_conditions.Clear();
		}

		private void ValidateAdd(int index, object value)
		{
			if (_schedulerFilterCondition != null && _schedulerFilterCondition._filterOwner != null && _schedulerFilterCondition._filterOwner is MxScheduler && _schedulerFilterCondition.Scheduler != null && !_schedulerFilterCondition.Scheduler._isLoadingLayout)
			{
				throw new InvalidOperationException(SchedulerFilterCondition._text);
			}
			SchedulerFilterCondition schedulerFilterCondition = (SchedulerFilterCondition)value;
			if (schedulerFilterCondition == _schedulerFilterCondition)
			{
				throw new InvalidOperationException("A Filter Condition can be its own parent.");
			}
			if (schedulerFilterCondition._owner != null || schedulerFilterCondition._filterOwner != null)
			{
				throw new InvalidOperationException("The FilterCondition already belongs to another object.");
			}
		}

		private void AttachCondition(int index, object value)
		{
			((SchedulerFilterCondition)value)._owner = this;
		}

		private void ValidateRemove(object value)
		{
			if (_schedulerFilterCondition != null && _schedulerFilterCondition._filterOwner != null)
			{
				throw new InvalidOperationException(SchedulerFilterCondition._text);
			}
		}

		private void SetOwner(object value)
		{
			((SchedulerFilterCondition)value)._owner = null;
		}

		void ICollection.CopyTo(Array destination, int arrayIndex)
		{
			_conditions.CopyTo(destination, arrayIndex);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _conditions.GetEnumerator();
		}
	}
}
