using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class DateCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxScheduler _scheduler;

		private bool _notifyOnInsert = true;

		private bool _notifyOnRemove = true;

		private bool _notifyOnClear = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Date";

		public DateTime this[int index] => (DateTime)base.InnerList[index];

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int dateIndex = 0;
			for (PropertyValue propertyValue = info["Date" + dateIndex]; propertyValue != null; propertyValue = info["Date" + dateIndex])
			{
				DateTime date = (DateTime)propertyValue.GetValue(typeof(DateTime));
				Add(date);
				dateIndex++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int dateIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					DateTime dateTime = (DateTime)enumerator.Current;
					info.AddValue("Date" + dateIndex, dateTime);
					dateIndex++;
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

		internal DateCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		protected override void OnClearComplete()
		{
			if (_notifyOnClear)
			{
				UpdateDateCollection();
				NotifyDatesChanged();
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (_notifyOnRemove)
			{
				UpdateDateCollection();
				NotifyDatesChanged();
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			if (_notifyOnInsert)
			{
				UpdateDateCollection();
				NotifyDatesChanged();
			}
		}

		internal void ClearWithoutNotify()
		{
			_notifyOnClear = false;
			Clear();
			_notifyOnClear = true;
		}

		internal void UpdateDateCollection()
		{
			if (_scheduler != null)
			{
				_scheduler._isAppointmentCacheStale = true;
				_scheduler.InvalidateAppointmentLayout();
			}
		}

		internal void RemoveAtWithoutNotify(int index)
		{
			_notifyOnRemove = false;
			base.InnerList.RemoveAt(index);
			_notifyOnRemove = true;
		}

		public void Add(DateTime date)
		{
			date = date.Date;
			if (!base.InnerList.Contains(date))
			{
				base.InnerList.Add(date);
				base.InnerList.Sort();
				UpdateDateCollection();
				NotifyDatesChanged();
			}
		}

		internal void AddWithoutNotify(DateTime item)
		{
			item = item.Date;
			if (!base.InnerList.Contains(item))
			{
				_notifyOnInsert = false;
				base.InnerList.Add(item);
				base.InnerList.Sort();
				_notifyOnInsert = true;
			}
		}

		public bool Contains(DateTime date)
		{
			return base.InnerList.Contains(date);
		}

		private void NotifyDatesChanged()
		{
			if (_scheduler != null)
			{
				_scheduler.RefreshAfterDatesChange();
			}
		}
	}
}
