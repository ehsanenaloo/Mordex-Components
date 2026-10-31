using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerAppointmentCollection : IList, ICollection, IEnumerable, IMordexXmlLayoutsSupport
	{
		private ArrayList _appointments;

		internal ArrayList _sortedAppointments = new ChangeCountingList();

		// Counts every change of the list, so a cached view of it can tell when it is out of date.
		private sealed class ChangeCountingList : ArrayList
		{
			internal int Version;

			public override int Add(object value)
			{
				Version++;
				return base.Add(value);
			}

			public override void AddRange(ICollection c)
			{
				Version++;
				base.AddRange(c);
			}

			public override void Insert(int index, object value)
			{
				Version++;
				base.Insert(index, value);
			}

			public override void InsertRange(int index, ICollection c)
			{
				Version++;
				base.InsertRange(index, c);
			}

			public override void Remove(object obj)
			{
				Version++;
				base.Remove(obj);
			}

			public override void RemoveAt(int index)
			{
				Version++;
				base.RemoveAt(index);
			}

			public override void RemoveRange(int index, int count)
			{
				Version++;
				base.RemoveRange(index, count);
			}

			public override void Clear()
			{
				Version++;
				base.Clear();
			}

			public override void Reverse()
			{
				Version++;
				base.Reverse();
			}

			public override void Reverse(int index, int count)
			{
				Version++;
				base.Reverse(index, count);
			}

			public override void SetRange(int index, ICollection c)
			{
				Version++;
				base.SetRange(index, c);
			}

			public override void Sort()
			{
				Version++;
				base.Sort();
			}

			public override void Sort(IComparer comparer)
			{
				Version++;
				base.Sort(comparer);
			}

			public override void Sort(int index, int count, IComparer comparer)
			{
				Version++;
				base.Sort(index, count, comparer);
			}

			public override object this[int index]
			{
				get { return base[index]; }
				set
				{
					Version++;
					base[index] = value;
				}
			}
		}

		// Start day and running maximum of the end time of _sortedAppointments, in list order. When the start days
		// do not decrease, a query only has to look at a window of the list.
		private sealed class TimeIndex
		{
			internal ArrayList List;

			internal int ListVersion;

			internal int TimesVersion;

			internal SchedulerAppointment[] Items;

			internal long[] StartDays;

			internal long[] MaxEnds;

			internal bool IsOrdered;
		}

		private TimeIndex _timeIndex;

		internal void NoteTimeChange()
		{
			_scheduler._appointmentTimesVersion++;
		}

		// The items of _sortedAppointments that can match a query for appointments that end on or after "from" and
		// start on or before the day of "to", in list order. The caller still applies its own test to each item.
		private SchedulerAppointment[] Candidates(DateTime from, DateTime to, out int first, out int last)
		{
			ChangeCountingList list = (ChangeCountingList)_sortedAppointments;
			TimeIndex index = _timeIndex;
			if (index == null || !ReferenceEquals(index.List, list) || index.ListVersion != list.Version || index.TimesVersion != _scheduler._appointmentTimesVersion)
			{
				int count = list.Count;
				index = new TimeIndex
				{
					List = list,
					ListVersion = list.Version,
					TimesVersion = _scheduler._appointmentTimesVersion,
					Items = new SchedulerAppointment[count],
					StartDays = new long[count],
					MaxEnds = new long[count],
					IsOrdered = true
				};
				long maxEnd = long.MinValue;
				for (int i = 0; i < count; i++)
				{
					SchedulerAppointment item = (SchedulerAppointment)list[i];
					index.Items[i] = item;
					long startDay = item._startTime.Date.Ticks;
					index.StartDays[i] = startDay;
					if (i > 0 && startDay < index.StartDays[i - 1])
					{
						index.IsOrdered = false;
					}
					maxEnd = Math.Max(maxEnd, item._endTime.Ticks);
					index.MaxEnds[i] = maxEnd;
				}
				_timeIndex = index;
			}
			if (index.IsOrdered)
			{
				first = FirstIndexAtLeast(index.MaxEnds, from.Ticks);
				last = FirstIndexAbove(index.StartDays, to.Date.Ticks);
			}
			else
			{
				first = 0;
				last = index.Items.Length;
			}
			return index.Items;
		}

		private static int FirstIndexAtLeast(long[] values, long target)
		{
			int low = 0;
			int high = values.Length;
			while (low < high)
			{
				int middle = low + (high - low) / 2;
				if (values[middle] >= target)
				{
					high = middle;
				}
				else
				{
					low = middle + 1;
				}
			}
			return low;
		}

		private static int FirstIndexAbove(long[] values, long target)
		{
			int low = 0;
			int high = values.Length;
			while (low < high)
			{
				int middle = low + (high - low) / 2;
				if (values[middle] > target)
				{
					high = middle;
				}
				else
				{
					low = middle + 1;
				}
			}
			return low;
		}

		// Reference membership of _appointments, so a membership test does not scan the list.
		private readonly MemberSet _members = new MemberSet();

		private sealed class MemberSet
		{
			private readonly HashSet<SchedulerAppointment> _set = new HashSet<SchedulerAppointment>();

			internal int Version;

			internal bool Contains(SchedulerAppointment item)
			{
				return _set.Contains(item);
			}

			internal void Add(SchedulerAppointment item)
			{
				Version++;
				_set.Add(item);
			}

			internal void Remove(SchedulerAppointment item)
			{
				Version++;
				_set.Remove(item);
			}

			internal void Clear()
			{
				Version++;
				_set.Clear();
			}
		}

		// Changes whenever an appointment joins or leaves the collection.
		internal int MembershipVersion => _members.Version;

		private SchedulerComparer _schedulerComparer = new SchedulerComparer();

		private TimeSpan _defaultDuration;

		private MxScheduler _scheduler;

		internal bool _isPublic = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		bool IList.IsFixedSize => _appointments.IsFixedSize;

		object IList.this[int index]
		{
			get
			{
				return _appointments[index];
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		string IMordexXmlLayoutsSupport.CollectionElementName => "Appointment";

		public TimeSpan DefaultDuration
		{
			get
			{
				return _defaultDuration;
			}
			set
			{
				_defaultDuration = value;
			}
		}

		public MxScheduler Scheduler => _scheduler;

		public int Count => _appointments.Count;

		public object SyncRoot => _appointments.SyncRoot;

		public bool IsReadOnly => _appointments.IsReadOnly;

		public bool IsSynchronized => _appointments.IsSynchronized;

		public SchedulerAppointment this[int index] => (SchedulerAppointment)_appointments[index];

		public SchedulerAppointment this[string key]
		{
			get
			{
				foreach (SchedulerAppointment item in _appointments)
				{
					if (string.Compare(item.Key, key, ignoreCase: true) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_appointments = new ArrayList();
			_sortedAppointments = new ChangeCountingList();
			_members.Clear();
			int appointmentIndex = 0;
			PropertyValue propertyValue = info["Appointment" + appointmentIndex];
			while (propertyValue != null)
			{
				SchedulerAppointment schedulerAppointment = null;
				schedulerAppointment = ((Scheduler != null) ? Scheduler.BuildAppointment(DateTime.Today, DateTime.Today, "") : new SchedulerAppointment());
				schedulerAppointment._parentCollection = this;
				schedulerAppointment._index = _appointments.Count;
				_appointments.Add(schedulerAppointment);
				_members.Add(schedulerAppointment);
				appointmentIndex++;
				_scheduler.UpdateLayout(this, schedulerAppointment, EditType.Add);
				propertyValue.DeserializeObject(schedulerAppointment);
				_scheduler.ReadAppointmentFields(schedulerAppointment, propertyValue.GetLayoutReader());
				_sortedAppointments.Insert(FindSortedInsertIndex(schedulerAppointment, 0, _sortedAppointments.Count - 1, _schedulerComparer, 0), schedulerAppointment);
				propertyValue = info["Appointment" + appointmentIndex];
				AppointmentEventArgs e = new AppointmentEventArgs(schedulerAppointment);
				_scheduler.RaiseAppointmentRead(e);
			}
		}

		int IList.IndexOf(object value)
		{
			return _appointments.IndexOf(value);
		}

		void IList.Remove(object value)
		{
			if (_appointments.Contains(value))
			{
				_appointments.Remove(value);
				_members.Remove((SchedulerAppointment)value);
			}
		}

		void IList.Insert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.GetType() != typeof(SchedulerAppointment))
			{
				throw new InvalidCastException();
			}
			Insert(index, (SchedulerAppointment)value);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int serializedCount = 0;
			foreach (SchedulerAppointment item in _appointments)
			{
				if (item._recurrenceState != RecurrenceState.Exception)
				{
					info.AddValue("Appointment" + serializedCount, item);
					serializedCount++;
				}
			}
		}

		internal SchedulerAppointmentCollection(MxScheduler scheduler, int minutes, bool isPublic)
		{
			_appointments = new ArrayList();
			_sortedAppointments = new ChangeCountingList();
			_defaultDuration = new TimeSpan(0, minutes, 0);
			_scheduler = scheduler;
			_scheduler.AppointmentChanged += OnSchedulerAppointmentChanged;
			_isPublic = isPublic;
		}

		/// <summary>Stops listening to the scheduler so the collection can be collected.</summary>
		internal void Detach()
		{
			_scheduler.AppointmentChanged -= OnSchedulerAppointmentChanged;
		}

		private bool ContainsAppointment(SchedulerAppointment item)
		{
			return _members.Contains(item);
		}

		// The stored index finds the position in constant time in the normal case.
		private int PositionOf(SchedulerAppointment item)
		{
			int hint = _isPublic ? item._collectionIndex : item._index;
			if (hint >= 0 && hint < _appointments.Count && ReferenceEquals(_appointments[hint], item))
			{
				return hint;
			}
			return _appointments.IndexOf(item);
		}

		private void RemoveFromList(SchedulerAppointment item)
		{
			int position = PositionOf(item);
			if (position >= 0)
			{
				_appointments.RemoveAt(position);
			}
			_members.Remove(item);
		}

		internal void RemoveWithoutNotify(SchedulerAppointment item)
		{
			if (!ContainsAppointment(item))
			{
				return;
			}
			int appointmentIndex = item._collectionIndex;
			if (!_isPublic)
			{
				appointmentIndex = item._index;
			}
			for (int i = appointmentIndex + 1; i < _appointments.Count; i++)
			{
				if (_isPublic)
				{
					((SchedulerAppointment)_appointments[i])._collectionIndex--;
				}
				else
				{
					((SchedulerAppointment)_appointments[i])._index--;
				}
			}
			item._parentCollection = null;
			item._scheduler = null;
			RemoveFromList(item);
			_sortedAppointments.Remove(item);
			if (_isPublic)
			{
				item._collectionIndex = -1;
			}
			else
			{
				item._index = -1;
			}
		}

		internal void UpdateSchedulerAppointmentCollection()
		{
			foreach (SchedulerAppointment schedulerAppointment in _appointments)
			{
				schedulerAppointment._parentCollection = null;
				schedulerAppointment._scheduler = null;
				if (_isPublic)
				{
					schedulerAppointment._collectionIndex = -1;
				}
				else
				{
					schedulerAppointment._index = -1;
				}
			}
			_appointments.Clear();
			_sortedAppointments.Clear();
			_members.Clear();
		}

		public bool Contains(string key)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SchedulerAppointment schedulerAppointment = (SchedulerAppointment)enumerator.Current;
					if (string.Compare(schedulerAppointment.Key, key, ignoreCase: true) == 0)
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

		public bool Contains(SchedulerAppointment appointment)
		{
			return appointment != null && ContainsAppointment(appointment);
		}

		bool IList.Contains(object value)
		{
			return value is SchedulerAppointment appointment && _members.Contains(appointment);
		}

		public void Remove(SchedulerAppointment appointment)
		{
			if (_isPublic && (!_scheduler.CanRemoveAppointment(appointment) || !ContainsAppointment(appointment)))
			{
				return;
			}
			int appointmentIndex = appointment._collectionIndex;
			if (!_isPublic)
			{
				appointmentIndex = appointment._index;
			}
			for (int i = appointmentIndex + 1; i < _appointments.Count; i++)
			{
				if (_isPublic)
				{
					((SchedulerAppointment)_appointments[i])._collectionIndex--;
				}
				else
				{
					((SchedulerAppointment)_appointments[i])._index--;
				}
			}
			RemoveFromList(appointment);
			_sortedAppointments.Remove(appointment);
			appointment._parentCollection = null;
			appointment._scheduler = null;
			if (_isPublic)
			{
				appointment._collectionIndex = -1;
			}
			InvalidateScheduler();
			NotifyLayoutChanged(appointment, EditType.Remove);
			if (_isPublic)
			{
				_scheduler.HandleAppointmentRemoved(appointment);
			}
		}

		public void RemoveAt(int index)
		{
			SchedulerAppointment appointment = (SchedulerAppointment)_appointments[index];
			Remove(appointment);
		}

		public void Insert(int index, SchedulerAppointment appointment)
		{
			if (!_scheduler.CanAddAppointment(appointment))
			{
				return;
			}
			appointment._parentCollection = this;
			for (int i = index; i < _appointments.Count; i++)
			{
				if (_isPublic)
				{
					((SchedulerAppointment)_appointments[i])._collectionIndex++;
				}
				else
				{
					((SchedulerAppointment)_appointments[i])._index++;
				}
			}
			if (appointment.Duration == TimeSpan.Zero)
			{
				appointment.Duration = _defaultDuration;
			}
			_appointments.Insert(index, appointment);
			_members.Add(appointment);
			if (_isPublic)
			{
				appointment._collectionIndex = index;
			}
			else
			{
				appointment._index = index;
			}
			SchedulerComparer schedulerComparer = new SchedulerComparer();
			_sortedAppointments.Insert(FindSortedInsertIndex(appointment, 0, _sortedAppointments.Count - 1, schedulerComparer, 0), appointment);
			_scheduler._isAppointmentCacheStale = true;
			NotifyLayoutChanged(appointment, EditType.Add);
			if (_isPublic)
			{
				_scheduler.HandleAppointmentAdded(appointment);
			}
		}

		internal void MoveAppointment(int index, int indexArgument)
		{
			SchedulerAppointment value = (SchedulerAppointment)_appointments[index];
			_appointments.RemoveAt(index);
			_appointments.Insert(indexArgument, value);
			_members.Version++;
		}

		public void CopyTo(Array destination, int index)
		{
			_appointments.CopyTo(destination, index);
		}

		public IEnumerator GetEnumerator()
		{
			return _appointments.GetEnumerator();
		}

		internal ArrayList GetVisibleAppointmentsInRange(DateRange dateRange, bool sortByStartTime)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointmentOwner schedulerAppointmentOwner = null;
			SchedulerAppointment[] candidates = Candidates(dateRange.Start, dateRange.End, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (item.IsRecurring || !item.Visible || !(item.StartTime.Date <= dateRange.End) || (!(item.EndTime > dateRange.Start) && (!(item.EndTime == dateRange.Start) || !(item.Duration == TimeSpan.Zero))))
				{
					continue;
				}
				if (_scheduler._hasOwners)
				{
					schedulerAppointmentOwner = _scheduler.FindOwnerByValue(item.Owner);
					if (schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible)
					{
						arrayList.Add(item);
					}
				}
				else
				{
					arrayList.Add(item);
				}
			}
			foreach (SchedulerAppointment recurrenceAppointment in _scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(new DateRange(dateRange.Start, dateRange.End.AddDays(1.0)));
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (!(occurrence.EndTime >= dateRange.Start) || !(occurrence.StartTime.Date <= dateRange.End))
					{
						continue;
					}
					if (_scheduler._hasOwners)
					{
						schedulerAppointmentOwner = _scheduler.FindOwnerByValue(occurrence.Owner);
						if ((schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible) && Scheduler.MatchesFilter(occurrence))
						{
							arrayList.Add(occurrence);
						}
					}
					else if (_scheduler != null && Scheduler.MatchesFilter(occurrence))
					{
						arrayList.Add(occurrence);
					}
				}
			}
			if (sortByStartTime)
			{
				SchedulerStartTimeComparer comparer = new SchedulerStartTimeComparer();
				arrayList.Sort(comparer);
			}
			else
			{
				SchedulerComparer dayComparer = new SchedulerComparer();
				arrayList.Sort(dayComparer);
			}
			return arrayList;
		}

		internal ArrayList GetAppointmentsForOwner(DateRange dateRange, bool sortByStartTime, SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointment[] candidates = Candidates(dateRange.Start, dateRange.End, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (!item.IsRecurring && item.Visible && item.StartTime.Date <= dateRange.End && (item.EndTime > dateRange.Start || (item.EndTime == dateRange.Start && item.Duration == TimeSpan.Zero)) && (item.Owner == schedulerAppointmentOwner.Value || (schedulerAppointmentOwner.Value != null && schedulerAppointmentOwner.Value.Equals(item.Owner))))
				{
					arrayList.Add(item);
				}
			}
			foreach (SchedulerAppointment recurrenceAppointment in _scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(new DateRange(dateRange.Start, dateRange.End.AddDays(1.0)));
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (occurrence.EndTime >= dateRange.Start && occurrence.StartTime <= dateRange.End.AddDays(1.0) && (occurrence.Owner == schedulerAppointmentOwner.Value || (schedulerAppointmentOwner.Value != null && schedulerAppointmentOwner.Value.Equals(occurrence.Owner))) && _scheduler != null && Scheduler.MatchesFilter(occurrence))
					{
						arrayList.Add(occurrence);
					}
				}
			}
			if (sortByStartTime)
			{
				SchedulerStartTimeComparer comparer = new SchedulerStartTimeComparer();
				arrayList.Sort(comparer);
			}
			else
			{
				SchedulerComparer dayComparer = new SchedulerComparer();
				arrayList.Sort(dayComparer);
			}
			return arrayList;
		}

		internal ArrayList GetAppointmentsForOwnerValue(DateRange range, object value)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointmentOwner schedulerAppointmentOwner = null;
			SchedulerAppointment[] candidates = Candidates(range.Start, range.End, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (!item.IsRecurring && item.EndTime >= range.Start && item.StartTime <= range.End)
				{
					schedulerAppointmentOwner = _scheduler.FindOwnerByValue(item.Owner);
					if (schedulerAppointmentOwner != null && schedulerAppointmentOwner.ValueEquals(value))
					{
						arrayList.Add(item);
					}
				}
			}
			bool hasOccurrences = false;
			foreach (SchedulerAppointment recurrenceAppointment in _scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(range);
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (!_scheduler._hasOwners || !(occurrence.EndTime >= range.Start) || !(occurrence.StartTime <= range.End))
					{
						continue;
					}
					schedulerAppointmentOwner = _scheduler.FindOwnerByValue(occurrence.Owner);
					if (schedulerAppointmentOwner != null && schedulerAppointmentOwner.ValueEquals(value))
					{
						hasOccurrences = true;
						if (_scheduler != null && Scheduler.MatchesFilter(occurrence))
						{
							arrayList.Add(occurrence);
						}
					}
				}
			}
			if (hasOccurrences)
			{
				if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
				{
					SchedulerComparer comparer = new SchedulerComparer();
					arrayList.Sort(comparer);
				}
				else
				{
					SchedulerStartTimeComparer startTimeComparer = new SchedulerStartTimeComparer();
					arrayList.Sort(startTimeComparer);
				}
			}
			return arrayList;
		}

		internal ArrayList GetAppointmentsOverlappingRange(DateRange range)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointment[] candidates = Candidates(range.Start, range.End, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (!item.IsRecurring && item.EndTime >= range.Start && item.StartTime <= range.End)
				{
					arrayList.Add(item);
				}
			}
			bool hasOccurrences = false;
			foreach (SchedulerAppointment recurrenceAppointment in _scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(range);
				if (occurrences == null)
				{
					continue;
				}
				hasOccurrences = true;
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (occurrence.EndTime >= range.Start && occurrence.StartTime <= range.End && _scheduler != null && Scheduler.MatchesFilter(occurrence))
					{
						arrayList.Add(occurrence);
					}
				}
			}
			if (hasOccurrences)
			{
				if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
				{
					SchedulerComparer comparer = new SchedulerComparer();
					arrayList.Sort(comparer);
				}
				else
				{
					SchedulerStartTimeComparer startTimeComparer = new SchedulerStartTimeComparer();
					arrayList.Sort(startTimeComparer);
				}
			}
			return arrayList;
		}

		internal ArrayList GetVisibleAppointmentsOverlappingRange(DateRange dateRange)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointmentOwner schedulerAppointmentOwner = null;
			SchedulerAppointment[] candidates = Candidates(dateRange.Start, dateRange.End, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (item.IsRecurring || !item.Visible || !(item.EndTime >= dateRange.Start) || !(item.StartTime.Date <= dateRange.End))
				{
					continue;
				}
				if (_scheduler._hasOwners)
				{
					schedulerAppointmentOwner = _scheduler.FindOwnerByValue(item.Owner);
					if (schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible)
					{
						arrayList.Add(item);
					}
				}
				else
				{
					arrayList.Add(item);
				}
			}
			bool hasOccurrences = false;
			foreach (SchedulerAppointment recurrenceAppointment in _scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(new DateRange(dateRange.Start, dateRange.End.AddDays(1.0)));
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (occurrence.EndTime >= dateRange.Start && occurrence.StartTime.Date <= dateRange.End && _scheduler._hasOwners)
					{
						schedulerAppointmentOwner = _scheduler.FindOwnerByValue(occurrence.Owner);
						if (schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible)
						{
							hasOccurrences = true;
							if (Scheduler.MatchesFilter(occurrence))
							{
								arrayList.Add(occurrence);
							}
						}
					}
					else
					{
						hasOccurrences = true;
						if (_scheduler != null && Scheduler.MatchesFilter(occurrence))
						{
							arrayList.Add(occurrence);
						}
					}
				}
			}
			if (hasOccurrences)
			{
				if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
				{
					SchedulerComparer comparer = new SchedulerComparer();
					arrayList.Sort(comparer);
				}
				else
				{
					SchedulerStartTimeComparer startTimeComparer = new SchedulerStartTimeComparer();
					arrayList.Sort(startTimeComparer);
				}
			}
			return arrayList;
		}

		private void OnSchedulerAppointmentChanged(object sender, AppointmentChangeEventArgs eventArgs)
		{
			if (_sortedAppointments.Contains(eventArgs.Appointment))
			{
				_sortedAppointments.Remove(eventArgs.Appointment);
				SchedulerComparer schedulerComparer = new SchedulerComparer();
				_sortedAppointments.Insert(FindSortedInsertIndex(eventArgs.Appointment, 0, _sortedAppointments.Count - 1, schedulerComparer, 0), eventArgs.Appointment);
			}
		}

		// An appointment that ends at the range start or starts at the range end does not overlap it. A range of
		// zero length overlaps an appointment that covers or starts at that moment.
		internal static bool OverlapsRange(SchedulerAppointment appointment, DateTime start, DateTime end)
		{
			return appointment.EndTime > start && (appointment.StartTime < end || (start == end && appointment.StartTime == start));
		}

		// A null owner matches every appointment.
		internal static bool MatchesOwner(SchedulerAppointment appointment, object owner)
		{
			return owner == null || (appointment.Owner != null && (appointment.Owner == owner || appointment.Owner.Equals(owner)));
		}

		internal ArrayList GetOwnerAppointmentsOnDate(DateTime dateTime, object owner)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointment[] candidates = Candidates(dateTime, dateTime, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (!item.IsRecurring && item.Visible && item.StartTime.Date <= dateTime && item.EndTime >= dateTime && (item.Owner == owner || (item.Owner != null && item.Owner.Equals(owner))) && (!(item._endTime == dateTime) || !(item.EndTime != item._startTime)))
				{
					arrayList.Add(item);
				}
			}
			DateRange range = new DateRange(dateTime.Date, dateTime.Date.AddDays(1.0));
			foreach (SchedulerAppointment recurrenceAppointment in _scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(range);
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if ((!(occurrence.StartTime.Date <= dateTime) || !(occurrence.EndTime >= dateTime) || !(occurrence._endTime == dateTime) || !(occurrence.EndTime != occurrence._startTime)) && (occurrence.Owner == owner || (occurrence.Owner != null && occurrence.Owner.Equals(owner))) && (!(occurrence._endTime == dateTime) || !(occurrence.EndTime != occurrence._startTime)) && _scheduler != null && _scheduler.MatchesFilter(occurrence))
					{
						arrayList.Add(occurrence);
					}
				}
			}
			SchedulerComparer comparer = new SchedulerComparer();
			arrayList.Sort(comparer);
			return arrayList;
		}

		internal SchedulerAppointment FindNextAppointment(DateTime dateTime)
		{
			return SearchNextAppointment(dateTime, 0, _sortedAppointments.Count - 1, null);
		}

		internal SchedulerAppointment FindPreviousAppointment(DateTime dateTime)
		{
			return SearchPreviousAppointment(dateTime, 0, _sortedAppointments.Count - 1, null);
		}

		private SchedulerAppointment SearchPreviousAppointment(DateTime dateTime, int lowIndex, int integerArgument, SchedulerAppointment schedulerAppointmentArgument)
		{
			if (lowIndex > integerArgument)
			{
				if (lowIndex < _sortedAppointments.Count)
				{
					SchedulerAppointment schedulerAppointment = (SchedulerAppointment)_sortedAppointments[lowIndex];
					if ((schedulerAppointmentArgument == null || schedulerAppointment.StartTime > schedulerAppointmentArgument.StartTime) && schedulerAppointment.StartTime.Date < dateTime)
					{
						return schedulerAppointment;
					}
				}
				return schedulerAppointmentArgument;
			}
			int middleIndex = (integerArgument + lowIndex) / 2;
			SchedulerAppointment middleAppointment = (SchedulerAppointment)_sortedAppointments[middleIndex];
			if (middleAppointment.StartTime < dateTime && (schedulerAppointmentArgument == null || middleAppointment.StartTime > schedulerAppointmentArgument.StartTime))
			{
				schedulerAppointmentArgument = middleAppointment;
			}
			if (middleAppointment.StartTime == dateTime)
			{
				if (middleIndex < _sortedAppointments.Count)
				{
					return (SchedulerAppointment)_sortedAppointments[middleIndex];
				}
				return null;
			}
			if (middleAppointment.StartTime > dateTime)
			{
				return SearchPreviousAppointment(dateTime, lowIndex, middleIndex - 1, schedulerAppointmentArgument);
			}
			return SearchPreviousAppointment(dateTime, middleIndex + 1, integerArgument, schedulerAppointmentArgument);
		}

		private SchedulerAppointment SearchNextAppointment(DateTime dateTime, int lowIndex, int integerArgument, SchedulerAppointment schedulerAppointmentArgument)
		{
			if (lowIndex > integerArgument)
			{
				if (lowIndex < _sortedAppointments.Count)
				{
					SchedulerAppointment schedulerAppointment = (SchedulerAppointment)_sortedAppointments[lowIndex];
					if ((schedulerAppointmentArgument == null || schedulerAppointment.StartTime < schedulerAppointmentArgument.StartTime) && schedulerAppointment.StartTime.Date > dateTime)
					{
						return schedulerAppointment;
					}
				}
				return schedulerAppointmentArgument;
			}
			int middleIndex = (integerArgument + lowIndex) / 2;
			SchedulerAppointment middleAppointment = (SchedulerAppointment)_sortedAppointments[middleIndex];
			if (middleAppointment.StartTime > dateTime && (schedulerAppointmentArgument == null || middleAppointment.StartTime < schedulerAppointmentArgument.StartTime))
			{
				schedulerAppointmentArgument = middleAppointment;
			}
			if (middleAppointment.StartTime.Date == dateTime)
			{
				if (middleIndex < _sortedAppointments.Count)
				{
					return (SchedulerAppointment)_sortedAppointments[middleIndex];
				}
				return null;
			}
			if (middleAppointment.StartTime > dateTime)
			{
				return SearchNextAppointment(dateTime, lowIndex, middleIndex - 1, schedulerAppointmentArgument);
			}
			return SearchNextAppointment(dateTime, middleIndex + 1, integerArgument, schedulerAppointmentArgument);
		}

		internal ArrayList GetVisibleAppointmentsOnDate(DateTime dateTime)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointmentOwner schedulerAppointmentOwner = null;
			SchedulerAppointment[] candidates = Candidates(dateTime, dateTime, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (item.IsRecurring || !item.Visible || !(item.StartTime.Date <= dateTime) || !(item.EndTime >= dateTime) || (item._endTime == dateTime && item.EndTime != item._startTime))
				{
					continue;
				}
				if (_scheduler._hasOwners)
				{
					schedulerAppointmentOwner = _scheduler.FindOwnerByValue(item.Owner);
					if (schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible)
					{
						arrayList.Add(item);
					}
				}
				else
				{
					arrayList.Add(item);
				}
			}
			DateRange range = new DateRange(dateTime.Date, dateTime.Date.AddDays(1.0));
			foreach (SchedulerAppointment recurrenceAppointment in Scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(range);
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (occurrence.StartTime.Date <= dateTime && occurrence.EndTime >= dateTime && occurrence._endTime == dateTime && occurrence.EndTime != occurrence._startTime)
					{
						continue;
					}
					if (_scheduler._hasOwners)
					{
						schedulerAppointmentOwner = _scheduler.FindOwnerByValue(occurrence.Owner);
						if ((schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible) && Scheduler.MatchesFilter(occurrence))
						{
							arrayList.Add(occurrence);
						}
					}
					else if (_scheduler != null && _scheduler.MatchesFilter(occurrence))
					{
						arrayList.Add(occurrence);
					}
				}
			}
			SchedulerComparer comparer = new SchedulerComparer();
			arrayList.Sort(comparer);
			return arrayList;
		}

		internal ArrayList GetAppointmentsOnDate(DateTime dateTime)
		{
			if (!_scheduler.Created)
			{
				_scheduler.CreateControl();
			}
			ArrayList arrayList = new ArrayList();
			SchedulerAppointmentOwner schedulerAppointmentOwner = null;
			SchedulerAppointment[] candidates = Candidates(dateTime, dateTime, out int first, out int last);
			for (int position = first; position < last; position++)
			{
				SchedulerAppointment item = candidates[position];
				if (item.IsRecurring || !(item.StartTime.Date <= dateTime) || !(item.EndTime >= dateTime) || (item._endTime == dateTime && item.EndTime != item._startTime))
				{
					continue;
				}
				if (_scheduler._hasOwners)
				{
					schedulerAppointmentOwner = _scheduler.FindOwnerByValue(item.Owner);
					if (schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible)
					{
						arrayList.Add(item);
					}
				}
				else
				{
					arrayList.Add(item);
				}
			}
			DateRange range = new DateRange(dateTime.Date, dateTime.Date.AddDays(1.0));
			foreach (SchedulerAppointment recurrenceAppointment in Scheduler.RecurrenceAppointments)
			{
				SchedulerAppointment[] occurrences = recurrenceAppointment.RecurrencePattern.GetOccurrences(range);
				if (occurrences == null)
				{
					continue;
				}
				SchedulerAppointment[] occurrenceItems = occurrences;
				foreach (SchedulerAppointment occurrence in occurrenceItems)
				{
					if (occurrence.StartTime.Date <= dateTime && occurrence.EndTime >= dateTime && occurrence._endTime == dateTime && occurrence.EndTime != occurrence._startTime)
					{
						continue;
					}
					if (_scheduler._hasOwners)
					{
						schedulerAppointmentOwner = _scheduler.FindOwnerByValue(occurrence.Owner);
						if ((schedulerAppointmentOwner == null || schedulerAppointmentOwner.Visible) && Scheduler.MatchesFilter(occurrence))
						{
							arrayList.Add(occurrence);
						}
					}
					else if (_scheduler != null && _scheduler.MatchesFilter(occurrence))
					{
						arrayList.Add(occurrence);
					}
				}
			}
			SchedulerComparer comparer = new SchedulerComparer();
			arrayList.Sort(comparer);
			return arrayList;
		}

		internal void NotifyLayoutChanged(SchedulerAppointment schedulerAppointment, EditType editType)
		{
			_scheduler.UpdateLayout(this, schedulerAppointment, editType);
		}

		public void Clear()
		{
			_scheduler._shouldHandleRemovals = false;
			if (_isPublic && _scheduler.CanDropAllAppointments(this, out bool needsFullLayout))
			{
				if (needsFullLayout)
				{
					_scheduler._isFullLayoutStale = true;
				}
				_scheduler._appointments.UpdateSchedulerAppointmentCollection();
				UpdateSchedulerAppointmentCollection();
			}
			else
			{
				for (int index = _appointments.Count - 1; index >= 0; index--)
				{
					RemoveAt(index);
				}
			}
			_scheduler._shouldHandleRemovals = true;
			_scheduler.ClearDataSourceRows();
			_sortedAppointments.Clear();
			InvalidateScheduler();
			NotifyLayoutChanged(null, EditType.Clear);
		}

		internal void AddValidated(SchedulerAppointment schedulerAppointment)
		{
			if (!_isPublic || _scheduler.CanAddAppointment(schedulerAppointment))
			{
				schedulerAppointment._parentCollection = this;
				if (_isPublic)
				{
					schedulerAppointment._collectionIndex = _appointments.Count;
				}
				else if (schedulerAppointment._index < 0)
				{
					schedulerAppointment._index = _appointments.Count;
				}
				SchedulerComparer schedulerComparer = new SchedulerComparer();
				_appointments.Add(schedulerAppointment);
				_members.Add(schedulerAppointment);
				if (_isPublic)
				{
					_scheduler.HandleAppointmentAdded(schedulerAppointment);
				}
				_sortedAppointments.Insert(FindSortedInsertIndex(schedulerAppointment, 0, _sortedAppointments.Count - 1, schedulerComparer, 0), schedulerAppointment);
			}
		}

		internal void AddUnchecked(SchedulerAppointment schedulerAppointment)
		{
			schedulerAppointment._parentCollection = this;
			if (_isPublic)
			{
				schedulerAppointment._collectionIndex = _appointments.Count;
			}
			else if (schedulerAppointment._index < 0)
			{
				schedulerAppointment._index = _appointments.Count;
			}
			_appointments.Add(schedulerAppointment);
			_members.Add(schedulerAppointment);
			SchedulerComparer schedulerComparer = new SchedulerComparer();
			_sortedAppointments.Insert(FindSortedInsertIndex(schedulerAppointment, 0, _sortedAppointments.Count - 1, schedulerComparer, 0), schedulerAppointment);
		}

		private int FindSortedInsertIndex(SchedulerAppointment schedulerAppointment, int lowIndex, int integerArgument, SchedulerComparer schedulerComparer, int middleIndex)
		{
			// An item that sorts after the last one goes to the end; the search would reach the same index.
			if (lowIndex == 0 && integerArgument >= 0 && integerArgument == _sortedAppointments.Count - 1 && schedulerComparer.Compare(_sortedAppointments[integerArgument], schedulerAppointment) < 0)
			{
				return integerArgument + 1;
			}
			if (lowIndex > integerArgument)
			{
				return lowIndex;
			}
			middleIndex = (integerArgument + lowIndex) / 2;
			SchedulerAppointment x = (SchedulerAppointment)_sortedAppointments[middleIndex];
			int comparison = schedulerComparer.Compare(x, schedulerAppointment);
			if (comparison == 0)
			{
				return middleIndex;
			}
			if (comparison > 0)
			{
				return FindSortedInsertIndex(schedulerAppointment, lowIndex, middleIndex - 1, schedulerComparer, middleIndex);
			}
			return FindSortedInsertIndex(schedulerAppointment, middleIndex + 1, integerArgument, schedulerComparer, middleIndex);
		}

		public void Add(DateTime appointmentStart)
		{
			SchedulerAppointment appointment = new SchedulerAppointment(appointmentStart);
			Add(appointment);
		}

		public void Add(DateTime appointmentStart, DateTime appointmentEnd)
		{
			SchedulerAppointment appointment = new SchedulerAppointment(appointmentStart, appointmentEnd);
			Add(appointment);
		}

		public void Add(DateTime appointmentStart, DateTime appointmentEnd, string text)
		{
			SchedulerAppointment appointment = new SchedulerAppointment(appointmentStart, appointmentEnd, text);
			Add(appointment);
		}

		public void Add(DateTime appointmentStart, string text)
		{
			SchedulerAppointment appointment = new SchedulerAppointment(appointmentStart, text);
			Add(appointment);
		}

		public void Add(SchedulerAppointment appointment)
		{
			if (appointment._parentCollection != null)
			{
				string text = "";
				text = ((appointment._parentCollection != this) ? "In order to add an appointment to this collection, you must first remove it from it´s current parent collection." : "This appointment already belongs to this collection.");
				throw new InvalidOperationException(text);
			}
			if (!_isPublic || _scheduler.CanAddAppointment(appointment))
			{
				if (appointment.IsDataRowEditStarted())
				{
					if (appointment.DataRow is IEditableObject)
					{
						((IEditableObject)appointment.DataRow).EndEdit();
					}
					appointment._scheduler._shouldHandleItemAdded = true;
					appointment.SetDataRowEditStarted(false);
					appointment._scheduler = null;
					appointment._dataRow = null;
				}
				if (appointment.Duration == TimeSpan.Zero && appointment.EndTime == DateTime.MinValue)
				{
					appointment.Duration = _defaultDuration;
				}
				appointment._parentCollection = this;
				if (_isPublic)
				{
					appointment._collectionIndex = _appointments.Count;
				}
				else
				{
					appointment._index = _appointments.Count;
				}
				SchedulerComparer schedulerComparer = new SchedulerComparer();
				_appointments.Add(appointment);
				_members.Add(appointment);
				_sortedAppointments.Insert(FindSortedInsertIndex(appointment, 0, _sortedAppointments.Count - 1, schedulerComparer, 0), appointment);
				_scheduler._isAppointmentCacheStale = true;
				NotifyLayoutChanged(appointment, EditType.Add);
				if (_isPublic)
				{
					_scheduler.HandleAppointmentAdded(appointment);
				}
			}
			else if (appointment.IsDataRowEditStarted())
			{
				if (appointment.DataRow is IEditableObject)
				{
					((IEditableObject)appointment.DataRow).CancelEdit();
				}
				appointment._scheduler._shouldHandleItemAdded = true;
				appointment.SetDataRowEditStarted(false);
				appointment._scheduler = null;
				appointment._dataRow = null;
			}
		}

		internal void InvalidateScheduler()
		{
			_scheduler._isAppointmentCacheStale = true;
			_scheduler.Invalidate();
		}

		int IList.Add(object value)
		{
			if (!(value is SchedulerAppointment))
			{
				throw new ArgumentException();
			}
			Add((SchedulerAppointment)value);
			return Count - 1;
		}
	}
}
