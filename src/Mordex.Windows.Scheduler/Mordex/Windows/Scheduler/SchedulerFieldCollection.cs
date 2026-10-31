using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerFieldCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxScheduler _scheduler;

		private bool _shouldNotifyOwner = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Field";

		public MxScheduler Scheduler => _scheduler;

		public SchedulerField this[int index] => (SchedulerField)base.List[index];

		public SchedulerField this[string key]
		{
			get
			{
				foreach (SchedulerField item in base.List)
				{
					if (item.Key == key)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["Field" + index]; propertyValue != null; propertyValue = info["Field" + index])
			{
				SchedulerField schedulerField = new SchedulerField();
				base.InnerList.Add(schedulerField);
				schedulerField._owner = this;
				schedulerField._index = index;
				index++;
			}
			index = 0;
			for (PropertyValue propertyValue = info["Field" + index]; propertyValue != null; propertyValue = info["Field" + index])
			{
				propertyValue.DeserializeObject(this[index]);
				_scheduler.SyncDataTableColumn(this[index], FieldEditType.Add);
				index++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerField value = (SchedulerField)enumerator.Current;
					info.AddValue("Field" + index, value);
					index++;
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

		internal SchedulerFieldCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void Add(SchedulerField field)
		{
			if (field._owner != null)
			{
				string text = "";
				text = ((field._owner != this) ? "In order to add a field to this collection, you must first remove it from it´s current parent collection." : "This field already belongs to this collection.");
				throw new InvalidOperationException(text);
			}
			field._index = base.List.Count;
			field._owner = this;
			base.List.Add(field);
		}

		public void AddRange(SchedulerField[] fields)
		{
			foreach (SchedulerField schedulerField in fields)
			{
				if (schedulerField._owner != null)
				{
					string text = "";
					text = ((schedulerField._owner != this) ? "In order to add a field to this collection, you must first remove it from it´s current parent collection." : "This field already belongs to this collection.");
					throw new InvalidOperationException(text);
				}
				schedulerField._index = base.List.Count;
				schedulerField._owner = this;
				base.List.Add(schedulerField);
			}
		}

		public void Remove(SchedulerField field)
		{
			if (base.List.Contains(field))
			{
				field._owner = null;
				field._index = -1;
				base.List.Remove(field);
			}
		}

		public bool Contains(SchedulerField field)
		{
			return base.List.Contains(field);
		}

		public int IndexOf(SchedulerField field)
		{
			return base.List.IndexOf(field);
		}

		public bool Contains(string fieldKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerField schedulerField = (SchedulerField)enumerator.Current;
					if (string.Compare(schedulerField.Key, fieldKey, ignoreCase: true) == 0)
					{
						return true;
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
			return false;
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (_shouldNotifyOwner)
			{
				((SchedulerField)value)._owner = null;
				((SchedulerField)value)._index = -1;
				SyncUnboundColumn((SchedulerField)value, FieldEditType.Remove);
			}
			UpdateSchedulerFieldCollection();
		}

		private void UpdateSchedulerFieldCollection()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((SchedulerField)base.InnerList[i])._index = i;
			}
		}

		protected override void OnClearComplete()
		{
			if (_shouldNotifyOwner)
			{
				SyncUnboundColumn(null, FieldEditType.Clear);
			}
		}

		protected override void OnClear()
		{
			foreach (SchedulerField item in base.List)
			{
				item._owner = null;
				item._index = -1;
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			if (value.GetType() != typeof(SchedulerField))
			{
				throw new InvalidCastException();
			}
			if (_shouldNotifyOwner)
			{
				SyncUnboundColumn((SchedulerField)value, FieldEditType.Add);
			}
		}

		internal void SyncUnboundColumn(SchedulerField schedulerField, FieldEditType fieldEditType)
		{
			if (_scheduler != null)
			{
				_scheduler.SyncDataTableColumn(schedulerField, fieldEditType);
			}
		}
	}
}
