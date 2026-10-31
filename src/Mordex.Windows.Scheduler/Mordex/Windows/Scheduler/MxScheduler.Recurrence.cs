using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		internal void DeleteRecurrenceOccurrence(SchedulerAppointment app)
		{
			RemovingRecurrentAppointmentEventArgs e = new RemovingRecurrentAppointmentEventArgs(app);
			OnRemovingRecurrentAppointment(e);
			if (e.Cancel)
			{
				return;
			}
			if (app.Selected)
			{
				UpdateCurrentAppointment(app);
			}
			if (e.DeleteTheSeries)
			{
				app.MasterAppointment.Delete();
			}
			else
			{
				app._isDeletedOccurrence = true;
				if (!app.EnsureRecurrenceException())
				{
					app._isDeletedOccurrence = false;
					return;
				}
				app._isDeletedOccurrence = false;
				app.GetRecurrenceException().Deleted = true;
			}
			if (app.RecurrenceState == RecurrenceState.Exception && _appointments.Contains(app))
			{
				WriteRecurrenceToDataSource(app);
				if (app.MasterAppointment != null)
				{
					WriteRecurrenceToDataSource(app.MasterAppointment);
				}
			}
			RaiseRecurrenceChanged(EventArgs.Empty);
			AppointmentEventArgs removedArgs = new AppointmentEventArgs(app);
			OnRecurrentAppointmentRemoved(removedArgs);
		}

		internal void UpdateRecurrenceCollection(SchedulerAppointment appointment, bool shouldWriteToDataSource)
		{
			_isAppointmentCacheStale = true;
			_isAppointmentLayoutStale = true;
			if (appointment.RecurrenceState == RecurrenceState.Master || appointment.RecurrenceState == RecurrenceState.NotRecurring)
			{
				if (appointment.RecurrencePattern == null)
				{
					if (_recurrenceAppointmentCollection.Contains(appointment))
					{
						_recurrenceAppointmentCollection.RemoveIfNotRecurring(appointment);
					}
				}
				else if (!_recurrenceAppointmentCollection.Contains(appointment))
				{
					_recurrenceAppointmentCollection.AddIfRecurring(appointment);
				}
			}
			if (shouldWriteToDataSource)
			{
				WriteRecurrenceToDataSource(appointment);
			}
			RaiseRecurrenceChanged(EventArgs.Empty);
			Invalidate();
		}

		internal bool IsRecurrenceExceptionCancelled(SchedulerAppointment app)
		{
			AppointmentCancelEventArgs e = new AppointmentCancelEventArgs(app);
			if (app.DataRow == null)
			{
				AddToDataSource(app);
				foreach (SchedulerField field in Fields)
				{
					if (!IsBuiltInField(field) && field.IncludeInCopy)
					{
						app.SetValue(field, app.MasterAppointment.GetValue(field));
					}
				}
			}
			OnCreatingRecurrenceException(e);
			if (e.Cancel)
			{
				app._recurrenceState = RecurrenceState.Occurrence;
				if (app.IsDataRowEditStarted())
				{
					if (app.DataRow is IEditableObject)
					{
						((IEditableObject)app.DataRow).CancelEdit();
					}
					_shouldHandleItemAdded = true;
					app.SetDataRowEditStarted(false);
					app._scheduler = null;
					app._dataRow = null;
				}
			}
			else
			{
				if (app.IsDataRowEditStarted())
				{
					if (app.DataRow is IEditableObject)
					{
						((IEditableObject)app.DataRow).EndEdit();
					}
					_shouldHandleItemAdded = true;
					app.SetDataRowEditStarted(false);
					app._scheduler = null;
					app._dataRow = null;
				}
				if (!_appointments.Contains(app))
				{
					_appointments.AddValidated(app);
				}
				_isAppointmentCacheStale = true;
				InvalidateAppointmentCacheAndLayout();
			}
			return e.Cancel;
		}

		internal void RaiseRecurrenceExceptionCreated(SchedulerAppointment app)
		{
			AppointmentEventArgs e = new AppointmentEventArgs(app);
			OnRecurrenceExceptionCreated(e);
		}

		internal void AddRecurrenceChangedHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_appointmentsChangedEventKey, eventHandler);
		}

		internal void RemoveRecurrenceChangedHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_appointmentsChangedEventKey, eventHandler);
		}

		protected virtual void OnCreatingRecurrenceException(AppointmentCancelEventArgs e)
		{
			if (base.Events[_creatingRecurrenceExceptionEventKey] is AppointmentCancelEventHandler appointmentCancelEventHandler)
			{
				appointmentCancelEventHandler(this, e);
			}
		}

		protected virtual void OnRecurrenceExceptionCreated(AppointmentEventArgs e)
		{
			if (base.Events[_recurrenceExceptionCreatedEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}
	}
}
