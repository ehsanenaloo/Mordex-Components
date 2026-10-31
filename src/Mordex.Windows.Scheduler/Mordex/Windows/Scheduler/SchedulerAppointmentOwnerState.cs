using System;
using System.Collections;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerAppointmentOwnerState
	{
		private SchedulerAppointmentOwner _user;

		private SchedulerAppointmentCollection _parentCollection;

		public DateTime _date;

		private ArrayList _appointments;

		internal SchedulerAppointmentOwnerState(SchedulerAppointmentCollection parentCollection, DateTime date, bool AllDay, SchedulerAppointmentOwner user)
		{
			_appointments = new ArrayList();
			_parentCollection = parentCollection;
			_date = date;
			_user = user;
			UpdateSchedulerAppointmentOwnerState();
		}

		public SchedulerAppointmentOwner GetUser()
		{
			return _user;
		}

		public ArrayList GetAppointments()
		{
			return _appointments;
		}

		internal void UpdateSchedulerAppointmentOwnerState()
		{
			if (_user == null)
			{
				_appointments = _parentCollection.GetVisibleAppointmentsOnDate(_date);
			}
			else
			{
				_appointments = _parentCollection.GetOwnerAppointmentsOnDate(_date, _user.Value);
			}
		}
	}
}
