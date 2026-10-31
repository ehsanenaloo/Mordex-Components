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
		internal bool ShouldShowAllDayAreaScrollBar()
		{
			return _showAllDayAreaScrollBar == TriState.True;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHourBackgroundStyle()
		{
			return _hourBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHourBackgroundStyle()
		{
			_hourBackgroundStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWeekDaysBackgroundStyle()
		{
			return _weekDaysBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWeekDaysBackgroundStyle()
		{
			_weekDaysBackgroundStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMonthDaysFormatStyle()
		{
			return _monthDaysFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMonthDaysFormatStyle()
		{
			_monthDaysFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlternatingMonthDaysFormatStyle()
		{
			return _alternatingMonthDaysFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlternatingMonthDaysFormatStyle()
		{
			_alternatingMonthDaysFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlternatingHourBackgroundStyle()
		{
			return _alternatingHourBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlternatingHourBackgroundStyle()
		{
			_alternatingHourBackgroundStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedFormatStyle()
		{
			return _selectedFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedFormatStyle()
		{
			_selectedFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedInactiveFormatStyle()
		{
			return _selectedInactiveFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedInactiveFormatStyle()
		{
			_selectedInactiveFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTimeNavigatorFormatStyle()
		{
			return _timeNavigatorFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTimeNavigatorFormatStyle()
		{
			_timeNavigatorFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeadersFormatStyle()
		{
			return _headersFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderFormatStyle()
		{
			_headersFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAllDayAreaBackgroundStyle()
		{
			return _allDayAreaBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAllDayAreaBackgroundStyle()
		{
			_allDayAreaBackgroundStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAppointmentsFormatStyle()
		{
			return _appointmentsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAppointmentFormatStyle()
		{
			_appointmentsFormatStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWorkingHourBackgroundStyle()
		{
			return _workingHourBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWorkingHourBackgroundStyle()
		{
			_workingHourBackgroundStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlternatingWorkingHourBackgroundStyle()
		{
			return _alternatingWorkingHourBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlternatingWorkingHourBackgroundStyle()
		{
			_alternatingWorkingHourBackgroundStyle.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDate()
		{
			return _date != DateTime.Today;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDate()
		{
			Date = DateTime.Today;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDates()
		{
			if (_currentLayout != null)
			{
				return false;
			}
			if (_view == SchedulerView.DayView)
			{
				return _dates.Count > 1;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDates()
		{
			_dates = new DateCollection(this);
			if (_allDayAreaControl != null)
			{
				_allDayAreaControl.SetDateCollection(_dates);
			}
			_isAppointmentCacheStale = true;
			InvalidateAppointmentLayout();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDayViewHourRange()
		{
			if (_currentLayout != null)
			{
				return false;
			}
			return _dayViewHourRanges.Count > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDayViewHourRange()
		{
			_dayViewHourRanges.Clear();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWatermarkImage()
		{
			if (_watermarkImage == null)
			{
				return false;
			}
			return _watermarkImage.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWatermarkImage()
		{
			WatermarkImage.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMonthFont()
		{
			return _monthFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMonthFont()
		{
			MonthFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWeekFont()
		{
			return _weekFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWeekFont()
		{
			WeekFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateAppointmentLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDayFont()
		{
			return _dayFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDayFont()
		{
			DayFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWorkStartTime()
		{
			return !_workStartTime.Equals(new TimeSpan(8, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWorkStartTime()
		{
			WorkStartTime = new TimeSpan(8, 0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWorkEndTime()
		{
			return !_workEndTime.Equals(new TimeSpan(17, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWorkEndTime()
		{
			WorkEndTime = new TimeSpan(17, 0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGridLineColorLight()
		{
			return !_gridLineColorLight.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGridLineColorLight()
		{
			GridLineColorLight = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGridLineColor()
		{
			return !_gridLineColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGridLineColor()
		{
			GridLineColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFirstVisibleTime()
		{
			return !_firstVisibleTime.Equals(new TimeSpan(8, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFirstVisibleTime()
		{
			FirstVisibleTime = new TimeSpan(8, 0, 0);
		}

		private void ResetRemindersIfBound()
		{
			if (_schedulerCurrencyManager != null && _schedulerCurrencyManager.Count > 0)
			{
				ResetReminders();
			}
		}

		private void ResetBindingOnDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			BindDataSource(null, new BindingMemberInfo(""), true);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDesignTimeLayout()
		{
			if (_fields.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.Fields) == 0))
			{
				return true;
			}
			if (_formatConditions.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.FormatConditions) == 0))
			{
				return true;
			}
			if (_storedFilters.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.StoredFilters) == 0))
			{
				return true;
			}
			if (_filterCondition != null && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.FilterCondition) == 0))
			{
				return true;
			}
			if (_internalImageList != null && _internalImageList.Images.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.InternalImageList) == 0))
			{
				return true;
			}
			if ((_workingHourSchema.Exceptions.Count > 0 || _workingHourSchema.WorkingHoursRange.Count > 0) && _currentLayout == null)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaxDate()
		{
			MaxDate = DateTime.MaxValue.Date.AddYears(-1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaxDate()
		{
			return !_maxDate.Equals(DateTime.MaxValue.Date.AddYears(-1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinDate()
		{
			return !_minDate.Equals(DateTime.MinValue.AddMonths(1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinDate()
		{
			MinDate = DateTime.MinValue.AddMonths(1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAppointmentOutLineColor()
		{
			return _isAppointmentOutLineColorSet;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAppointmentOutLineColor()
		{
			bool changed = _isAppointmentOutLineColorSet;
			_appointmentOutLineColor = Color.Black;
			_isAppointmentOutLineColorSet = false;
			if (changed)
			{
				Invalidate(invalidateChildren: true);
			}
		}

		public void ResetReminders()
		{
			DateTime maxValue = DateTime.MaxValue;
			_nextReminderAppointment = null;
			DateTime dateTime = DateTime.MaxValue;
			foreach (SchedulerAppointment item in _publicAppointments)
			{
				if (!item.HasActiveReminder())
				{
					continue;
				}
				SchedulerAppointment reminderAppointment = item;
				if (item._recurrenceState == RecurrenceState.Master)
				{
					reminderAppointment = item.GetNextReminderAppointment();
					if (reminderAppointment == null)
					{
						continue;
					}
				}
				maxValue = reminderAppointment.GetReminderTimeOrMax();
				if (maxValue < dateTime)
				{
					dateTime = maxValue;
					_nextReminderAppointment = reminderAppointment;
				}
			}
			if (_nextReminderAppointment != null)
			{
				if (dateTime <= DateTime.Now)
				{
					RaiseDueReminder();
				}
				else
				{
					UpdateNextReminder(_nextReminderAppointment);
				}
			}
		}

		public void ResetComponentSettings()
		{
			if (!base.DesignMode)
			{
				GetApplicationSettings().Reset();
				LoadComponentSettings();
			}
		}
	}
}
