using System;
using System.Collections;

namespace Mordex.Data
{
	public class MordexCustomGroupRowCollectionBase : MordexCollectionBase, IMordexCustomGroupRowCollection, IEnumerable
	{
		private MordexCustomGroupRowBase _remainingRowsRow;

		IMordexCustomGroupRow IMordexCustomGroupRowCollection.this[int index] => BaseGet(index);

		int IMordexCustomGroupRowCollection.Count => base.Count;

		internal static MordexCustomGroupRowCollectionBase CreateForOwner(MordexCustomGroupBase parent)
		{
			return new MordexCustomGroupRowCollectionBase(parent);
		}

		protected MordexCustomGroupRowCollectionBase(MordexCustomGroupBase parent)
			: base(parent)
		{
		}

		protected MordexCustomGroupRowBase BaseGet(int index)
		{
			return (MordexCustomGroupRowBase)base.List[index];
		}

		protected int BaseAdd(MordexCustomGroupRowBase customGroupRow)
		{
			return base.List.Add(customGroupRow);
		}

		protected void BaseInsert(int index, MordexCustomGroupRowBase customGroupRow)
		{
			base.List.Insert(index, customGroupRow);
		}

		protected void BaseRemove(MordexCustomGroupRowBase customGroupRow)
		{
			base.List.Remove(customGroupRow);
		}

		protected void BaseRemoveAt(int index)
		{
			base.List.RemoveAt(index);
		}

		protected int BaseIndexOf(MordexCustomGroupRowBase customGroupRow)
		{
			return base.List.IndexOf(customGroupRow);
		}

		protected bool BaseContains(MordexCustomGroupRowBase customGroupRow)
		{
			return base.List.Contains(customGroupRow);
		}

		internal MordexCustomGroupRowBase GetRemainingRowsRow()
		{
			return _remainingRowsRow;
		}

		internal void SetRemainingRowsRow(MordexCustomGroupRowBase mordexCustomGroupRowBase)
		{
			if (_remainingRowsRow != mordexCustomGroupRowBase)
			{
				if (_remainingRowsRow != null)
				{
					_remainingRowsRow._showRemainingRows = false;
				}
				_remainingRowsRow = mordexCustomGroupRowBase;
				if (_remainingRowsRow != null)
				{
					_remainingRowsRow._showRemainingRows = true;
				}
			}
		}

		private void FindRemainingRowsRow()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MordexCustomGroupRowBase mordexCustomGroupRowBase = (MordexCustomGroupRowBase)enumerator.Current;
					if (mordexCustomGroupRowBase._showRemainingRows)
					{
						_remainingRowsRow = mordexCustomGroupRowBase;
						return;
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
			_remainingRowsRow = null;
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			FindRemainingRowsRow();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			FindRemainingRowsRow();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			FindRemainingRowsRow();
		}

		int IMordexCustomGroupRowCollection.Add(IMordexCustomGroupRow customGroupRow)
		{
			return BaseAdd((MordexCustomGroupRowBase)customGroupRow);
		}

		void IMordexCustomGroupRowCollection.Remove(IMordexCustomGroupRow customGroupRow)
		{
			BaseRemove((MordexCustomGroupRowBase)customGroupRow);
		}

		void IMordexCustomGroupRowCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		void IMordexCustomGroupRowCollection.Insert(int index, IMordexCustomGroupRow customGroupRow)
		{
			BaseInsert(index, (MordexCustomGroupRowBase)customGroupRow);
		}
	}
}
