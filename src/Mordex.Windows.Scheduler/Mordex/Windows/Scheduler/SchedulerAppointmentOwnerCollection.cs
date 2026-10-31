using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerAppointmentOwnerCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxScheduler _scheduler;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Owner";

		public MxScheduler Scheduler => _scheduler;

		public SchedulerAppointmentOwner this[int index] => (SchedulerAppointmentOwner)base.List[index];

		public SchedulerAppointmentOwner this[string key]
		{
			get
			{
				foreach (SchedulerAppointmentOwner item in base.List)
				{
					if (item.Key == key)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		public SchedulerAppointmentOwner this[object value]
		{
			get
			{
				foreach (SchedulerAppointmentOwner item in base.List)
				{
					if (item.ValueEquals(value))
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with that value in the collection");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["Owner" + index]; propertyValue != null; propertyValue = info["Owner" + index])
			{
				SchedulerAppointmentOwner schedulerAppointmentOwner = new SchedulerAppointmentOwner();
				base.InnerList.Add(schedulerAppointmentOwner);
				schedulerAppointmentOwner._owner = this;
				schedulerAppointmentOwner._index = index;
				index++;
			}
			index = 0;
			for (PropertyValue propertyValue = info["Owner" + index]; propertyValue != null; propertyValue = info["Owner" + index])
			{
				propertyValue.DeserializeObject(this[index]);
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
					SchedulerAppointmentOwner value = (SchedulerAppointmentOwner)enumerator.Current;
					info.AddValue("Owner" + index, value);
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

		internal SchedulerAppointmentOwnerCollection(MxScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public void Add(object Value, string text)
		{
			SchedulerAppointmentOwner owner = new SchedulerAppointmentOwner(Value, text);
			Add(owner);
		}

		public void Add(object Value, string text, int imageIndex)
		{
			SchedulerAppointmentOwner owner = new SchedulerAppointmentOwner(Value, text, imageIndex);
			Add(owner);
		}

		public void Add(SchedulerAppointmentOwner owner)
		{
			if (owner._owner != null)
			{
				string text = "";
				text = ((owner._owner != this) ? "In order to add a user to this collection, you must first remove it from it´s current parent collection." : "This user already belongs to this collection.");
				throw new InvalidOperationException(text);
			}
			EnsureValueIsUnique(owner.Value);
			owner._owner = this;
			owner._index = base.List.Count;
			base.InnerList.Add(owner);
			UpdateSchedulerAppointmentOwnerCollection();
		}

		public void Insert(int index, SchedulerAppointmentOwner owner)
		{
			if (index > base.List.Count)
			{
				index = base.List.Count;
			}
			if (owner._owner != null)
			{
				string text = "";
				text = ((owner._owner != this) ? "In order to add a user to this collection, you must first remove it from it´s current parent collection." : "This user already belongs to this collection.");
				throw new InvalidOperationException(text);
			}
			EnsureValueIsUnique(owner.Value);
			owner._owner = this;
			owner._index = index;
			base.List.Insert(index, owner);
		}

		public void AddRange(SchedulerAppointmentOwner[] owners)
		{
			foreach (SchedulerAppointmentOwner schedulerAppointmentOwner in owners)
			{
				if (schedulerAppointmentOwner._owner != null)
				{
					string text = "";
					text = ((schedulerAppointmentOwner._owner != this) ? "In order to add a user to this collection, you must first remove it from it´s current parent collection." : "This user already belongs to this collection.");
					throw new InvalidOperationException(text);
				}
				EnsureValueIsUnique(schedulerAppointmentOwner.Value);
				schedulerAppointmentOwner._owner = this;
				schedulerAppointmentOwner._index = base.List.Count;
				base.InnerList.Add(schedulerAppointmentOwner);
			}
			UpdateSchedulerAppointmentOwnerCollection();
		}

		private void UpdateSchedulerAppointmentOwnerCollection()
		{
			_scheduler.RefreshOwners(true);
		}

		public bool Contains(SchedulerAppointmentOwner owner)
		{
			return base.List.Contains(owner);
		}

		public int IndexOf(SchedulerAppointmentOwner owner)
		{
			return base.List.IndexOf(owner);
		}

		public bool Contains(string ownerKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerAppointmentOwner schedulerAppointmentOwner = (SchedulerAppointmentOwner)enumerator.Current;
					if (string.Compare(schedulerAppointmentOwner.Key, ownerKey, ignoreCase: true) == 0)
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

		internal SchedulerAppointmentOwner FindOwnerByValue(object value)
		{
			foreach (SchedulerAppointmentOwner item in base.List)
			{
				if (item.ValueEquals(value))
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnInsertComplete(int index, object value)
		{
			UpdateSchedulerAppointmentOwnerCollection();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			SchedulerAppointmentOwner schedulerAppointmentOwner = (SchedulerAppointmentOwner)value;
			schedulerAppointmentOwner._owner = null;
			schedulerAppointmentOwner._index = -1;
			UpdateSchedulerAppointmentOwnerCollection();
			UpdateOwnerIndexes();
		}

		protected override void OnClearComplete()
		{
			UpdateSchedulerAppointmentOwnerCollection();
		}

		protected override void OnClear()
		{
			foreach (SchedulerAppointmentOwner item in base.List)
			{
				item._owner = null;
				item._index = -1;
			}
			base.OnClear();
		}

		public void Remove(SchedulerAppointmentOwner owner)
		{
			owner._owner = null;
			owner._index = -1;
			base.List.Remove(owner);
		}

		private void UpdateOwnerIndexes()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._index = i;
			}
		}

		internal void EnsureValueIsUnique(object value)
		{
			foreach (SchedulerAppointmentOwner item in base.List)
			{
				if (item.Value != null && value != null && item.Value.Equals(value))
				{
					string message = "The SchedulerAppointmentOwnerCollection can not contain two members with the same value";
					try
					{
						message = string.Format("The value {0}, is being declared already. The SchedulerAppointmentOwnerCollection can not contain two members with the same value", value.ToString());
					}
					catch
					{
					}
					throw new InvalidOperationException(message);
				}
			}
		}
	}
}
