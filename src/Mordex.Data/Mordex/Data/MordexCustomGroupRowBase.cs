using System;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public class MordexCustomGroupRowBase : MordexCollectionElement, IMordexCustomGroupRow, IComparable
	{
		private string _groupCaption;

		private MordexConditionBase _condition;

		private bool _showWhenEmpty;

		internal bool _showRemainingRows;

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

		protected string BaseGroupCaption
		{
			get
			{
				return _groupCaption;
			}
			set
			{
				_groupCaption = value;
			}
		}

		protected bool BaseShowWhenEmpty
		{
			get
			{
				return _showWhenEmpty;
			}
			set
			{
				_showWhenEmpty = value;
			}
		}

		protected bool BaseShowRemainingRows
		{
			get
			{
				return _showRemainingRows;
			}
			set
			{
				if (_showRemainingRows == value)
				{
					return;
				}
				MordexCustomGroupRowCollectionBase mordexCustomGroupRowCollectionBase = GetRowCollection();
				if (mordexCustomGroupRowCollectionBase != null)
				{
					if (value)
					{
						mordexCustomGroupRowCollectionBase.SetRemainingRowsRow(this);
					}
					else
					{
						mordexCustomGroupRowCollectionBase.SetRemainingRowsRow(null);
					}
				}
				_showRemainingRows = value;
			}
		}

		IMordexCondition IMordexCustomGroupRow.Condition
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

		string IMordexCustomGroupRow.GroupCaption
		{
			get
			{
				return BaseGroupCaption;
			}
			set
			{
				BaseGroupCaption = value;
			}
		}

		bool IMordexCustomGroupRow.ShowWhenEmpty
		{
			get
			{
				return BaseShowWhenEmpty;
			}
			set
			{
				BaseShowWhenEmpty = value;
			}
		}

		bool IMordexCustomGroupRow.ShowRemainingRows
		{
			get
			{
				return BaseShowRemainingRows;
			}
			set
			{
				BaseShowRemainingRows = value;
			}
		}

		protected MordexCustomGroupRowBase()
		{
			_groupCaption = "";
		}

		internal static MordexCustomGroupRowBase CreateDefault()
		{
			MordexCustomGroupRowBase mordexCustomGroupRowBase = new MordexCustomGroupRowBase();
			mordexCustomGroupRowBase._showRemainingRows = true;
			mordexCustomGroupRowBase._showWhenEmpty = false;
			return mordexCustomGroupRowBase;
		}

		private MordexCustomGroupRowCollectionBase GetRowCollection()
		{
			return GetOwningCollection() as MordexCustomGroupRowCollectionBase;
		}

		int IComparable.CompareTo(object value)
		{
			MordexCustomGroupRowBase mordexCustomGroupRowBase = (MordexCustomGroupRowBase)value;
			return Index.CompareTo(mordexCustomGroupRowBase.Index);
		}
	}
}
