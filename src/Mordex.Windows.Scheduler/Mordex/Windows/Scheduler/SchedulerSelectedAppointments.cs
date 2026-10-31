using System;
using System.Collections;

namespace Mordex.Windows.Scheduler
{
	public class SchedulerSelectedAppointments : ICollection, IEnumerable
	{
		private ArrayList _appointments;

		internal MxScheduler _scheduler;

		public SchedulerAppointment this[int index] => (SchedulerAppointment)_appointments[index];

		public int Count => _appointments.Count;

		public object SyncRoot => _appointments.SyncRoot;

		public bool IsReadOnly => _appointments.IsReadOnly;

		public bool IsSynchronized => _appointments.IsSynchronized;

		internal SchedulerSelectedAppointments(MxScheduler scheduler)
		{
			_appointments = new ArrayList();
			_scheduler = scheduler;
		}

		public bool Contains(SchedulerAppointment appointment)
		{
			return _appointments.Contains(appointment);
		}

		public void Clear()
		{
			if (_appointments.Count <= 0)
			{
				return;
			}
			foreach (SchedulerAppointment item in _appointments)
			{
				item._selected = false;
			}
			_appointments.Clear();
			UpdateSchedulerSelectedAppointments();
		}

		internal void ClearWithoutLayoutUpdate()
		{
			if (_appointments.Count <= 0)
			{
				return;
			}
			foreach (SchedulerAppointment item in _appointments)
			{
				item._selected = false;
			}
			_appointments.Clear();
		}

		private void UpdateSchedulerSelectedAppointments()
		{
			_scheduler.UpdateLayout(null, null, EditType.None);
			_scheduler.RaiseSelectedAppointmentsChanged();
		}

		public void Remove(SchedulerAppointment appointment)
		{
			if (_appointments.Contains(appointment))
			{
				appointment._selected = false;
				_appointments.Remove(appointment);
				UpdateSchedulerSelectedAppointments();
			}
		}

		public void Add(SchedulerAppointment appointment)
		{
			if (!_appointments.Contains(appointment) && (_scheduler.Appointments.Contains(appointment) || appointment.IsRecurring))
			{
				if (!_scheduler.MultiSelect && Count > 0)
				{
					Clear();
				}
				appointment._selected = true;
				_appointments.Add(appointment);
				UpdateSchedulerSelectedAppointments();
			}
		}

		public void Replace(int oldIndex, SchedulerAppointment newApp)
		{
			_appointments[oldIndex] = newApp;
			newApp._selected = true;
		}

		public void CopyTo(Array destination, int index)
		{
			_appointments.CopyTo(destination, index);
		}

		public IEnumerator GetEnumerator()
		{
			return _appointments.GetEnumerator();
		}
	}
}
