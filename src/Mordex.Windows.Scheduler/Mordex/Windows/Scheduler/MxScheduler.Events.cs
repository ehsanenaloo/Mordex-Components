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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		public event DrawSchedulerEventHandler DrawSchedulerArea
		{
			add
			{
				base.Events.AddHandler(_value, value);
			}
			remove
			{
				base.Events.RemoveHandler(_value, value);
			}
		}

		[Description("Event fired when an appointment should be reminded to the user.")]
		public event AppointmentEventHandler Reminder
		{
			add
			{
				base.Events.AddHandler(_reminderEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_reminderEventKey, value);
			}
		}

		[Description("Event fired when a row in the data source is read resulting in the creation of a new SchedulerAppointment.")]
		public event AppointmentEventHandler AppointmentRead
		{
			add
			{
				base.Events.AddHandler(_appointmentReadEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentReadEventKey, value);
			}
		}

		[Description("Event fired when a SchedulerAppointment was added to the Appointments collection")]
		public event AppointmentEventHandler AppointmentAdded
		{
			add
			{
				base.Events.AddHandler(_appointmentAddedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentAddedEventKey, value);
			}
		}

		[Description("Event fired when a SchedulerAppointment was removed from the Appointments collection")]
		public event AppointmentEventHandler AppointmentRemoved
		{
			add
			{
				base.Events.AddHandler(_appointmentRemovedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentRemovedEventKey, value);
			}
		}

		[Description("Event fired when an occurrence in a recurrence pattern removed.")]
		public event AppointmentEventHandler RecurrentAppointmentRemoved
		{
			add
			{
				base.Events.AddHandler(_recurrentAppointmentRemovedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_recurrentAppointmentRemovedEventKey, value);
			}
		}

		[Description("Event fired when one of the SchedulerAppointment objects in the Appointments collection is changing in one its properties")]
		public event AppointmentChangeCancelEventHandler AppointmentChanging
		{
			add
			{
				base.Events.AddHandler(_appointmentChangingEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentChangingEventKey, value);
			}
		}

		[Description("Event fired when one of the SchedulerAppointment objects in the Appointments collection has changed in one its properties")]
		public event AppointmentChangeEventHandler AppointmentChanged
		{
			add
			{
				base.Events.AddHandler(_appointmentChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentChangedEventKey, value);
			}
		}

		[Description("Event fired after the appointment collection is loaded using the LoadAppointments methos.")]
		public event EventHandler AppointmentsLoaded
		{
			add
			{
				base.Events.AddHandler(_appointmentsLoadedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentsLoadedEventKey, value);
			}
		}

		[Description("Event fired when a SchedulerAppointment is about to be added to the Appointments collection")]
		public event AddAppointmentEventHandler AddingAppointment
		{
			add
			{
				base.Events.AddHandler(_addingAppointmentEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_addingAppointmentEventKey, value);
			}
		}

		[Description("Event fired before setting the format for the rows in the Schedule control and allows you to modify that format based on the date.")]
		public event QueryHourRangeEventHandler QueryHourRange
		{
			add
			{
				base.Events.AddHandler(_queryHourRangeEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_queryHourRangeEventKey, value);
			}
		}

		[Description("Event fired when the Schedule control is about to add a new appointment")]
		public event GetNewObjectEventHandler GetNewObject
		{
			add
			{
				base.Events.AddHandler(_getNewObjectEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_getNewObjectEventKey, value);
			}
		}

		public event AppointmentCopiedCancelEventHandler CopyingAppointment
		{
			add
			{
				base.Events.AddHandler(_copyingAppointmentEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_copyingAppointmentEventKey, value);
			}
		}

		public event AppointmentCopiedEventHandler AppointmentCopied
		{
			add
			{
				base.Events.AddHandler(_appointmentCopiedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentCopiedEventKey, value);
			}
		}

		[Description("Event fired when the user makes double click on a SchedulerAppointment object")]
		public event AppointmentEventHandler AppointmentDoubleClick
		{
			add
			{
				base.Events.AddHandler(_appointmentDoubleClickEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentDoubleClickEventKey, value);
			}
		}

		[Description("Event fired when the label indicating more appointments is clicked")]
		public event HiddenAppointmentsEventHandler HiddenAppointmentClick
		{
			add
			{
				base.Events.AddHandler(_hiddenAppointmentClickEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_hiddenAppointmentClickEventKey, value);
			}
		}

		[Description("Event fired when when the Text of the appointment is being changed by the user.")]
		public event ValidatingAppointmentTextEventHandler ValidatingAppointmentText
		{
			add
			{
				base.Events.AddHandler(_validatingAppointmentTextEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_validatingAppointmentTextEventKey, value);
			}
		}

		[Description("Event raised when a SchedulerAppointment in the Schedule control is clicked in the edit area.")]
		public event AppointmentCancelEventHandler EditingAppointment
		{
			add
			{
				base.Events.AddHandler(_editingAppointmentEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_editingAppointmentEventKey, value);
			}
		}

		[Description("Event fired when the appointment is about to be dropped.")]
		public event DroppingAppointmentEventHandler DroppingAppointment
		{
			add
			{
				base.Events.AddHandler(_droppingAppointmentEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_droppingAppointmentEventKey, value);
			}
		}

		[Description("Event fired when an appointment was dropped")]
		public event AppointmentEventHandler AppointmentDropped
		{
			add
			{
				base.Events.AddHandler(_appointmentDroppedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentDroppedEventKey, value);
			}
		}

		[Description("Event fired when an appointment is about to be dragged in the Schedule control.")]
		public event AppointmentDragEventHandler AppointmentDrag
		{
			add
			{
				base.Events.AddHandler(_appointmentDragEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentDragEventKey, value);
			}
		}

		[Description("Event fired when the edit operation of a SchedulerAppointment in the Schedule is ended")]
		public event AppointmentEventHandler AppointmentEdited
		{
			add
			{
				base.Events.AddHandler(_appointmentEditedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_appointmentEditedEventKey, value);
			}
		}

		[Description("Event fired when a SchedulerAppointment is about to be removed from the Appointments collection.")]
		public event AppointmentCancelEventHandler RemovingAppointment
		{
			add
			{
				base.Events.AddHandler(_removingAppointmentEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_removingAppointmentEventKey, value);
			}
		}

		[Description("Event fired when an occurrence in a recurrence pattern is about to be removed.")]
		public event RemovingRecurrentAppointmentEventHandler RemovingRecurrentAppointment
		{
			add
			{
				base.Events.AddHandler(_removingRecurrentAppointmentEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_removingRecurrentAppointmentEventKey, value);
			}
		}

		[Description("Event fired when an exception for a recurrence pattern is about to be created.")]
		public event AppointmentCancelEventHandler CreatingRecurrenceException
		{
			add
			{
				base.Events.AddHandler(_creatingRecurrenceExceptionEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_creatingRecurrenceExceptionEventKey, value);
			}
		}

		[Description("Event fired when an exception for a recurrence pattern is created.")]
		public event AppointmentEventHandler RecurrenceExceptionCreated
		{
			add
			{
				base.Events.AddHandler(_recurrenceExceptionCreatedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_recurrenceExceptionCreatedEventKey, value);
			}
		}

		[Description("Event fired when the View property is changed")]
		public event EventHandler ViewChanged
		{
			add
			{
				base.Events.AddHandler(_viewChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_viewChangedEventKey, value);
			}
		}

		[Description("Event fired when the value of the MinDate property is changed")]
		public event EventHandler MinDateChanged
		{
			add
			{
				base.Events.AddHandler(_minDateChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_minDateChangedEventKey, value);
			}
		}

		[Description("Event fired when the value of the MaxDate property is changed")]
		public event EventHandler MaxDateChanged
		{
			add
			{
				base.Events.AddHandler(_maxDateChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_maxDateChangedEventKey, value);
			}
		}

		[Description("Event fired when the SelectedAppointments collection of the Schedule is changed.")]
		public event EventHandler SelectedAppointmentsChanged
		{
			add
			{
				base.Events.AddHandler(_selectedAppointmentsChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedAppointmentsChangedEventKey, value);
			}
		}

		public event EventHandler CurrentOwnerChanged
		{
			add
			{
				base.Events.AddHandler(_currentOwnerChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentOwnerChangedEventKey, value);
			}
		}

		public event EventHandler SelectedDaysChanged
		{
			add
			{
				base.Events.AddHandler(_selectedDaysChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedDaysChangedEventKey, value);
			}
		}

		public event EventHandler OwnersChanged
		{
			add
			{
				base.Events.AddHandler(_ownersChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_ownersChangedEventKey, value);
			}
		}

		public event EventHandler FilterApplied
		{
			add
			{
				base.Events.AddHandler(_filterAppliedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_filterAppliedEventKey, value);
			}
		}

		[Description("Occurs before the appointments in the control are filtered.")]
		public event CancelEventHandler ApplyingFilter
		{
			add
			{
				base.Events.AddHandler(_applyingFilterEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_applyingFilterEventKey, value);
			}
		}

		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(_layoutLoadEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_layoutLoadEventKey, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangedEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_currentLayoutChangedEventKey, value);
			}
		}

		[Description("Occurs when the user scrolls the Schedule control.")]
		public event EventHandler Scroll
		{
			add
			{
				base.Events.AddHandler(_scrollEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_scrollEventKey, value);
			}
		}

		[Description("Occurs before the CurrentLayout property changes")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangingEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_currentLayoutChangingEventKey, value);
			}
		}

		[Description("Event fired when the Dates collection of the Schedule is changed.")]
		public event EventHandler DatesChanged
		{
			add
			{
				base.Events.AddHandler(_datesChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_datesChangedEventKey, value);
			}
		}

		[Description("Event fired when the DatesSource property of the Schedule changes.")]
		public event EventHandler DataSourceChanged
		{
			add
			{
				base.Events.AddHandler(_dataSourceChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dataSourceChangedEventKey, value);
			}
		}

		[Description("Event fired when the Interval property of the Schedule changes.")]
		public event EventHandler IntervalChanged
		{
			add
			{
				base.Events.AddHandler(_intervalChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_intervalChangedEventKey, value);
			}
		}

		private void RaiseDueReminder()
		{
			try
			{
				if (_reminderTimer != null)
				{
					_reminderTimer.Stop();
				}
				SchedulerAppointment schedulerAppointment = _nextReminderAppointment;
				DateTime dateTime = DateTime.MinValue;
				if (_nextReminderAppointment != null)
				{
					if (_nextReminderAppointment.MasterAppointment != null && _nextReminderAppointment.MasterAppointment.Reminder != null)
					{
						if (_nextReminderAppointment.RecurrenceState != RecurrenceState.Exception)
						{
							if (_nextReminderAppointment.MasterAppointment.RecurrencePattern != null)
							{
								dateTime = _nextReminderAppointment.MasterAppointment.RecurrencePattern.GetEffectiveReminderDismissTime();
								_nextReminderAppointment.MasterAppointment.RecurrencePattern._reminderFiredTime = DateTime.Now;
							}
						}
						else
						{
							_nextReminderAppointment.Reminder.UpdateActive(false);
						}
					}
					else
					{
						_nextReminderAppointment.Reminder.UpdateActive(false);
					}
				}
				_nextReminderTime = DateTime.MaxValue;
				AppointmentEventArgs e = new AppointmentEventArgs(_nextReminderAppointment);
				OnReminder(e);
				ResetReminders();
				if (schedulerAppointment == null)
				{
					return;
				}
				if (schedulerAppointment.MasterAppointment != null && schedulerAppointment.MasterAppointment.RecurrencePattern != null)
				{
					if (dateTime != DateTime.MinValue)
					{
						if (schedulerAppointment.MasterAppointment.RecurrencePattern != null)
						{
							schedulerAppointment.MasterAppointment.RecurrencePattern._reminderFiredTime = DateTime.MinValue;
						}
					}
					else
					{
						schedulerAppointment.Reminder.UpdateActive(true);
						WriteSettingsToDataSource(schedulerAppointment);
					}
				}
				else
				{
					schedulerAppointment.Reminder.UpdateActive(true);
					WriteSettingsToDataSource(schedulerAppointment);
				}
			}
			catch
			{
			}
		}

		internal void RaiseSelectedAppointmentsChanged()
		{
			OnSelectedAppointmentsChanged(EventArgs.Empty);
		}

		internal void RaiseAppointmentChanging(AppointmentChangeCancelEventArgs eventArgs)
		{
			if (!_isAppointmentDragActive && _shouldRaiseChangeEvents)
			{
				bool wasLayoutDirty = _isAppointmentLayoutStale;
				_isAppointmentLayoutStale = false;
				OnAppointmentChanging(eventArgs);
				if (wasLayoutDirty)
				{
					_isAppointmentLayoutStale = wasLayoutDirty;
					Invalidate();
				}
			}
		}

		internal void RaiseRecurrenceChanged(EventArgs eventArgs)
		{
			if (base.Events[_appointmentsChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		internal void RaiseAppointmentEdited(SchedulerAppointment app)
		{
			AppointmentEventArgs e = new AppointmentEventArgs(app);
			OnAppointmentEdited(e);
		}
	}
}
