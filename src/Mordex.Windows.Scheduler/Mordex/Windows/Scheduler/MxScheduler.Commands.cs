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
		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
				_currentLayout = _layouts.FindCurrentLayout();
				if (_currentLayout != null && _currentLayout != _schedulerLayout)
				{
					_currentLayout._isCurrentLayout = true;
					_currentLayout.SetSchedule(this);
					ApplyCurrentLayout();
				}
				OnLayoutLoad(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_fields.Clear();
			_owners.Clear();
			_formatConditions.Clear();
			FilterCondition = null;
			_storedFilters.Clear();
			_hourBackgroundStyle.Reset();
			_alternatingHourBackgroundStyle.Reset();
			_alternatingWorkingHourBackgroundStyle.Reset();
			_workingHourBackgroundStyle.Reset();
			_selectedFormatStyle.Reset();
			_selectedInactiveFormatStyle.Reset();
			_allDayAreaBackgroundStyle.Reset();
			_appointmentsFormatStyle.Reset();
			_timeNavigatorFormatStyle.Reset();
			_headersFormatStyle.Reset();
			_weekDaysBackgroundStyle.Reset();
			_monthDaysFormatStyle.Reset();
			_alternatingMonthDaysFormatStyle.Reset();
			InvalidateAppointmentLayout();
		}

		private bool CanMoveSelectionCursor(TimeSpan timeSpan)
		{
			DateTime dateTime = _selectionCursor.Add(timeSpan);
			if (dateTime < _minDate || dateTime.Date > _maxDate)
			{
				return false;
			}
			return true;
		}

		internal void ShowEditor(SchedulerAppointmentView schedulerOwner, string strB)
		{
			bool isSameText = string.Compare(schedulerOwner.GetAppointment().Text, strB, ignoreCase: true) == 0;
			if (RaiseEditingAppointment(schedulerOwner.GetAppointment(), strB))
			{
				if (isSameText)
				{
					strB = schedulerOwner.GetAppointment().Text;
				}
				SetEditor(new SchedulerAppointmentEditControl(this, schedulerOwner.GetAppointment(), _useDefaultMenu));
				if (_view == SchedulerView.DayView && _allDayAppointmentViews.Contains(schedulerOwner))
				{
					_allDayAreaControl.Controls.Add(_appointmentEditor);
				}
				else
				{
					base.Controls.Add(_appointmentEditor);
				}
				_isEditingAppointment = true;
				_schedulerOwner = schedulerOwner;
				using (Graphics graphics = CreateGraphics())
				{
					schedulerOwner.RenderContent(graphics, _appointmentEditor);
				}
				if (schedulerOwner._backColor != Color.Transparent && !schedulerOwner._backColor.IsEmpty && schedulerOwner._backColor.A == byte.MaxValue)
				{
					_appointmentEditor._textBoxControl.BackColor = schedulerOwner._backColor;
				}
				_appointmentEditor._textBoxControl.ForeColor = schedulerOwner._schedulerRenderer4Color;
				_appointmentEditor._textBoxControl.Text = strB;
				_appointmentEditor._textBoxControl.SelectionStart = strB.Length;
				_appointmentEditor._textBoxControl.ScrollToCaret();
				SchedulerRenderer.FocusControl(_appointmentEditor._textBoxControl);
			}
			else
			{
				_schedulerOwner = schedulerOwner;
			}
		}

		internal void SetEditor(SchedulerAppointmentEditControl editor)
		{
			if (_appointmentEditor == editor || (editor == null && !_canChangeEditor))
			{
				return;
			}
			if (_appointmentEditor != null)
			{
				_canChangeEditor = false;
				ToggleControlEventHandlers(_appointmentEditor, true);
				_appointmentEditor._app.Text = _appointmentEditor._textBoxControl.Text;
				_appointmentEditor._app._isEditingText = false;
				RaiseAppointmentEdited(_appointmentEditor._app);
				_isEditingAppointment = false;
				_newAppointmentBeingEdited = null;
				if (_appointmentEditor != null)
				{
					_appointmentEditor.Dispose();
					_appointmentEditor = null;
				}
			}
			_canChangeEditor = true;
			if (editor == null)
			{
				_isEditingAppointment = false;
			}
			_appointmentEditor = editor;
			if (_appointmentEditor != null)
			{
				_appointmentEditor._app._isEditingText = true;
				ToggleControlEventHandlers(_appointmentEditor, false);
			}
			Invalidate(invalidateChildren: true);
		}

		internal void CommitEdit()
		{
			if (_isEditingAppointment && _appointmentEditor != null)
			{
				SchedulerAppointment app = _appointmentEditor._app;
				if (TryCommitEditor())
				{
					app.Selected = false;
				}
			}
		}

		internal void SetCursor(Cursor cursor)
		{
			_cursor = cursor;
		}

		public void EndEdit()
		{
			if (GetEditor() != null)
			{
				CommitEdit();
			}
		}

		public void EndEdit(bool cancel)
		{
			if (GetEditor() != null)
			{
				CancelAppointmentEdit(GetEditor()._app);
			}
		}

		public void StartEdit(SchedulerAppointment appointment)
		{
			if (!_allowEdit || !_publicAppointments.Contains(appointment))
			{
				return;
			}
			CommitEdit();
			_currentAppointment = appointment;
			_currentAppointment.Selected = true;
			EnsureVisible(appointment);
			SchedulerAppointmentView appointmentView = FindAppointmentView(appointment);
			if (appointmentView == null)
			{
				if (_view == SchedulerView.DayView)
				{
					LayoutDayViewAppointments();
				}
				else if (_view == SchedulerView.WeekView)
				{
					LayoutWeekViewAppointments();
				}
				else if (_view == SchedulerView.MonthView)
				{
					LayoutMonthViewAppointments();
				}
				appointmentView = FindAppointmentView(appointment);
			}
			if (appointmentView != null)
			{
				ShowEditor(appointmentView, _currentAppointment.Text);
			}
		}

		public void MoveNext()
		{
			int dayOffset = 0;
			if (_view == SchedulerView.MonthView)
			{
				dayOffset = _weeks * 7;
				Date = _date.AddDays(dayOffset);
			}
			else if (View == SchedulerView.WeekView)
			{
				dayOffset = 7;
				Date = _date.AddDays(dayOffset);
			}
			else if (View == SchedulerView.DayView)
			{
				if (_dates.Count > 0)
				{
					DateTime dateTime = _dates[_dates.Count - 1].AddDays(1.0);
					if (!(dateTime > MaxDate))
					{
						_dates.RemoveAtWithoutNotify(0);
						_dates.Add(dateTime);
					}
				}
			}
			else
			{
				Date = _date.AddDays(7.0);
			}
		}

		public void MovePrevious()
		{
			int dayOffset = 0;
			if (_view == SchedulerView.MonthView)
			{
				dayOffset = _weeks * 7;
				Date = _date.AddDays(-dayOffset);
			}
			else if (_view == SchedulerView.WeekView)
			{
				dayOffset = 7;
				Date = _date.AddDays(-dayOffset);
			}
			else if (View == SchedulerView.DayView)
			{
				if (_dates.Count > 0)
				{
					DateTime dateTime = _dates[0].AddDays(-1.0);
					if (!(dateTime < MinDate))
					{
						_dates.RemoveAtWithoutNotify(_dates.Count - 1);
						_dates.Add(dateTime);
					}
				}
			}
			else
			{
				Date = _date.AddDays(-7.0);
			}
		}

		internal SchedulerLayout CreateLayout(PersistenceMode persistenceMode)
		{
			SchedulerLayout schedulerLayout = new SchedulerLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			WriteLayoutSections(mordexLayoutWriter, schedulerLayout.GetPersistenceMode());
			LayoutWriter.StoreInLayout("SchedulerLayoutData", mordexLayoutWriter, schedulerLayout);
			schedulerLayout.DataMember = DataMember;
			schedulerLayout.DataSource = DataSource;
			return schedulerLayout;
		}

		internal void SetCurrencyManager(CurrencyManager schedulerCurrencyManager)
		{
			if (_schedulerCurrencyManager != schedulerCurrencyManager)
			{
				DetachCurrencyManagerEvents();
				_schedulerCurrencyManager = schedulerCurrencyManager;
				if (_schedulerCurrencyManager != null)
				{
					_iList = GetCurrencyManager().List;
				}
				else
				{
					_iList = null;
				}
				ReloadAppointmentsFromDataSource();
				AttachCurrencyManagerEvents();
			}
		}

		private void UpdatePropertyDescriptors()
		{
			if (_schedulerCurrencyManager != null)
			{
				_schedulerPropertyDescriptor = null;
				_endTimeDescriptor = null;
				_textDescriptor = null;
				_propertyDescriptor = null;
				_descriptionDescriptor = null;
				_recurrenceInfoDescriptor = null;
				_appointmentSettingsDescriptor = null;
				PropertyDescriptorCollection itemProperties = _schedulerCurrencyManager.GetItemProperties();
				if (GetStartTimeMemberName().Length > 0)
				{
					_schedulerPropertyDescriptor = itemProperties.Find(GetStartTimeMemberName(), ignoreCase: true);
				}
				if (GetEndTimeMemberName().Length > 0)
				{
					_endTimeDescriptor = itemProperties.Find(GetEndTimeMemberName(), ignoreCase: true);
				}
				if (GetTextMemberName().Length > 0)
				{
					_textDescriptor = itemProperties.Find(GetTextMemberName(), ignoreCase: true);
				}
				if (GetRecurrenceInfoFieldName().Length > 0)
				{
					_recurrenceInfoDescriptor = itemProperties.Find(GetRecurrenceInfoFieldName(), ignoreCase: true);
				}
				if (GetAppointmentSettingsMemberName().Length > 0)
				{
					_appointmentSettingsDescriptor = itemProperties.Find(GetAppointmentSettingsMemberName(), ignoreCase: true);
				}
				if (GetDescriptionMemberName().Length > 0)
				{
					_descriptionDescriptor = itemProperties.Find(GetDescriptionMemberName(), ignoreCase: true);
				}
				if (GetOwnerMemberName().Length > 0)
				{
					_propertyDescriptor = itemProperties.Find(GetOwnerMemberName(), ignoreCase: true);
				}
			}
		}

		internal void SetUnboundMode(bool isUnbound)
		{
			if (_usesInternalTable != isUnbound)
			{
				if (_usesInternalTable)
				{
					DisposeDataTable();
				}
				_usesInternalTable = isUnbound;
				if (_usesInternalTable)
				{
					UpdateDataTable();
				}
			}
		}

		internal void UpdateSchedule()
		{
			RaiseAppointmentsReset(EventArgs.Empty);
		}

		private void SetSchedulerFont(SchedulerFormatStyle schedulerFormatStyle)
		{
			_timeDesignatorFont = schedulerFormatStyle.ResolveFont(DayFont);
		}

		private void SetNextViewIndex(int nextViewIndex)
		{
			_usedAppointmentViewCount = nextViewIndex;
		}


		private bool IsTimeSlotSelected(DateTime dateTimeArgument)
		{
			DateTime dateTime = dateTimeArgument.AddMinutes(_minuteInterval);
			if (SelectedDays.End.Date > SelectedDays.Start.Date && SelectedDays.Start.TimeOfDay == TimeSpan.Zero && SelectedDays.End.TimeOfDay == TimeSpan.Zero && _showAllDayArea)
			{
				return false;
			}
			if (_selectedDays.Start <= dateTimeArgument && _selectedDays.End > dateTimeArgument)
			{
				return true;
			}
			if (_selectedDays.End >= dateTime && _selectedDays.Start < dateTime)
			{
				return true;
			}
			return false;
		}



		internal void ApplyThemeToFormatStyles()
		{
			InvalidateAppointmentStyles();
			AllDayAreaBackgroundStyle.ApplyTheme();
			WorkingHourBackgroundStyle.ApplyTheme();
			HourBackgroundStyle.ApplyTheme();
			SelectedFormatStyle.ApplyTheme();
			SelectedInactiveFormatStyle.ApplyTheme();
			AppointmentsFormatStyle.ApplyTheme();
			TimeNavigatorFormatStyle.ApplyTheme();
			HeadersFormatStyle.ApplyTheme();
			AlternatingMonthDaysFormatStyle.ApplyTheme();
		}

		internal void RefreshLayoutIfDirty(Graphics graphics)
		{
			if (_isFullLayoutStale)
			{
				RenderCurrentView(graphics);
			}
			else
			{
				if (!_isAppointmentLayoutStale)
				{
					return;
				}
				if (_view == SchedulerView.DayView)
				{
					LayoutAllDayArea(graphics);
					LayoutDayViewAppointments();
				}
				else if (_view == SchedulerView.MonthView)
				{
					LayoutMonthViewAppointments();
				}
				else
				{
					LayoutWeekViewAppointments();
				}
				if (_schedulerSchedulerAppointment != null && _schedulerSchedulerAppointment.RecurrenceState == RecurrenceState.Occurrence)
				{
					SchedulerAppointment[] occurrence = _schedulerSchedulerAppointment.RecurrencePattern.GetOccurrence(_schedulerSchedulerAppointment.StartTime);
					if (occurrence != null && occurrence.Length == 1)
					{
						_schedulerSchedulerAppointment = occurrence[0];
					}
				}
			}
		}

		internal void UpdateThemedRendering()
		{
			if (_themeRenderer == null)
			{
				_themeRenderer = new SchedulerThemeColors();
			}
		}

		internal void AddAppointmentsResetHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_appointmentsResetEventKey, eventHandler);
		}

		internal void RemoveAppointmentsResetHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_appointmentsResetEventKey, eventHandler);
		}
	}
}
