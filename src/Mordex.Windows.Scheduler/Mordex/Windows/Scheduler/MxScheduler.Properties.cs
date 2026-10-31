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
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		MxScheduler ISchedulerWorkingHoursOwner.Scheduler => this;

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.FirstDayOfWeek => FirstDayOfWeek;

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.WorkWeek => _workWeek;

		MxScheduler ISchedulerFilterOwner.Scheduler => this;

		MxScheduler ISchedulerFormatStyleOwner.Scheduler => this;

		[Category("Appointments")]
		[DefaultValue(StringAlignment.Center)]
		[Description("Specifies the alignment of the text in the all-day appointments")]
		public StringAlignment AllDayEventsAlignment
		{
			get
			{
				return _allDayEventsAlignment;
			}
			set
			{
				if (_allDayEventsAlignment != value)
				{
					_allDayEventsAlignment = value;
					Invalidate();
				}
			}
		}

		[Category("Appointments")]
		[Localizable(true)]
		[DefaultValue("")]
		[Description("Represents the string displayed before the text of the appointments.")]
		public string AppointmentsPrefix
		{
			get
			{
				return _appointmentsPrefix;
			}
			set
			{
				if (_appointmentsPrefix != value)
				{
					_appointmentsPrefix = value;
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public SchedulerAppointmentOwnerCollection Owners => _owners;

		[Browsable(false)]
		public SchedulerAppointmentCollection Appointments => _publicAppointments;

		[Browsable(false)]
		public RecurrenceAppointmentCollection RecurrenceAppointments => _recurrenceAppointmentCollection;

		[Description("Represents the first visible date in the control")]
		[Localizable(true)]
		[Category("Dates/Times")]
		public DateTime Date
		{
			get
			{
				return _date;
			}
			set
			{
				if (_date != value.Date && TryCommitEditor())
				{
					if (_schedulerOwner != null)
					{
						_schedulerOwner.CloseEditorIfOpen();
						_schedulerOwner = null;
					}
					DateTime dateTime = _date;
					_date = value.Date;
					_date = GetWeekStart(_date);
					if (View == SchedulerView.DayView)
					{
						_dates.ClearWithoutNotify();
						Dates.AddWithoutNotify(_date.Date);
						_isUpdatingDates = true;
						_isHourRangeCacheStale = true;
						OnDatesChanged(EventArgs.Empty);
						_isUpdatingDates = false;
						EnsureSelectionInRange();
					}
					else if (View == SchedulerView.WorkWeek)
					{
						RebuildDates();
					}
					else
					{
						int weekOffset = (int)((_date.Ticks - dateTime.Ticks) / 6048000000000L);
						_verticalScrollPosition += weekOffset;
						StyledScrollBar scrollBar = GetVerticalScrollBarControl();
						scrollBar.UpdateScrollValue(scrollBar.GetScrollValue() + weekOffset);
						RebuildDates();
					}
					if (View != SchedulerView.DayView)
					{
						EnsureSelectionInRange();
					}
					_isAppointmentCacheStale = true;
					InvalidateAppointmentLayout();
				}
			}
		}

		[Editor("System.ComponentModel.Design.CollectionEditor, System.Design", typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Dates/Times")]
		[Description("Represents the collection of the dates displayed by this Schedule")]
		public DateCollection Dates => _dates;

		[Description("Represents the collection of SchedulerHourRange object used to format the rows in the Schedule control.")]
		[Obsolete("This property is obsolete. Use WorkingHourSchema.WorkingHoursRange property instead.")]
		[Browsable(false)]
		public SchedulerHourRangeCollection DayViewHourRanges => _dayViewHourRanges;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WorkingHoursSchema WorkingHourSchema => _workingHourSchema;

		[Category("Appearance")]
		[Description("Specifies the style used to display dates in a Schedule control.")]
		[DefaultValue(SchedulerView.DayView)]
		public SchedulerView View
		{
			get
			{
				return _schedulerView;
			}
			set
			{
				if (!Enum.IsDefined(typeof(SchedulerView), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_schedulerView == value)
				{
					return;
				}
				_isHourRangeCacheStale = true;
				if (!TryCommitEditor())
				{
					return;
				}
				Invalidate(invalidateChildren: true);
				foreach (SchedulerAppointmentView item in _appointmentViews)
				{
					item._bounds = Rectangle.Empty;
				}
				if (_view == SchedulerView.DayView)
				{
					if (_allDayAreaControl != null)
					{
						ToggleControlEventHandlers(_allDayAreaControl, true);
						_allDayAreaControl.Dispose();
						_allDayAreaControl = null;
						base.Controls.Remove(_allDayAreaControl);
						RemoveUnusedAllDayRenderers();
					}
					SchedulerComparer comparer = new SchedulerComparer();
					_publicAppointments._sortedAppointments.Sort(comparer);
					DateTime dateTime = (_selectionCursor = (_selectionAnchor = _selectedDays.Start.Date));
					if (value == SchedulerView.WeekView || value == SchedulerView.MonthView)
					{
						if (_selectedDays.End.Date == dateTime || _selectedDays.End.TimeOfDay != TimeSpan.Zero)
						{
							UpdateSelectedDays(new DateRange(dateTime, _selectedDays.End.Date.AddDays(1.0)));
						}
						else
						{
							UpdateSelectedDays(new DateRange(dateTime, _selectedDays.End.Date));
						}
					}
					else
					{
						_selectionAnchor = _selectionAnchor.Add(_selectedDays.Start.TimeOfDay);
						_selectionCursor = _selectionAnchor;
						UpdateSelectedDays(new DateRange(dateTime, dateTime.AddMinutes(_minuteInterval)));
					}
				}
				else
				{
					if (_date < _minDate.Date)
					{
						_date = _minDate.Date;
					}
					if (_date > _maxDate.Date)
					{
						_date = _maxDate.Date;
					}
					SchedulerStartTimeComparer monthViewComparer = new SchedulerStartTimeComparer();
					_publicAppointments._sortedAppointments.Sort(monthViewComparer);
					DateTime start = (_selectionCursor = (_selectionAnchor = _selectedDays.Start.Date.Add(_workStartTime)));
					UpdateSelectedDays(new DateRange(start, start.AddMinutes(_minuteInterval)));
				}
				DisposeUnusedHiddenAppointmentButtons();
				if (value == SchedulerView.DayView || value == SchedulerView.WeekView)
				{
					_dates.ClearWithoutNotify();
				}
				if (value == SchedulerView.WorkWeek)
				{
					_isWorkWeekView = true;
					value = SchedulerView.DayView;
					_schedulerView = SchedulerView.WorkWeek;
					_isAppointmentCacheStale = true;
					_isFullLayoutStale = true;
					_view = value;
					SetWorkWeekDates(_date);
				}
				else
				{
					_isWorkWeekView = false;
					_schedulerView = value;
				}
				_view = value;
				EventArgs e = new EventArgs();
				_isAppointmentCacheStale = true;
				UpdateRowHeight();
				InvalidateAppointmentLayout();
				if (_view != SchedulerView.DayView)
				{
					RebuildDates();
				}
				else if (_schedulerView == SchedulerView.DayView)
				{
					_dates.AddWithoutNotify(_date);
					RefreshAfterDatesChange();
				}
				OnViewChanged(e);
				EnsureSelectionInRange();
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
			}
		}

		[DefaultValue(HourEvenOdd.None)]
		[Description("The mode of the alternating backcolor on day view")]
		[Category("Appearance")]
		public HourEvenOdd HourEvenOdd
		{
			get
			{
				return _hourEvenOdd;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HourEvenOdd), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_hourEvenOdd != value)
				{
					_hourEvenOdd = value;
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DateRange DateRange
		{
			get
			{
				if (_dates.Count > 0)
				{
					return new DateRange(_dates[0], _dates[_dates.Count - 1]);
				}
				return new DateRange(DateTime.MinValue, DateTime.MinValue);
			}
		}

		[DefaultValue(SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday)]
		[TypeConverter(typeof(FlagsEnumConverter))]
		[Description("Indicates the working days of the week")]
		[Category("Dates/Times")]
		[Localizable(true)]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		public SchedulerDayOfWeek WorkWeek
		{
			get
			{
				return _workWeek;
			}
			set
			{
				if (_workWeek != value)
				{
					if (value == (SchedulerDayOfWeek)0)
					{
						throw new ArgumentException();
					}
					_workWeek = value;
					if (_isWorkWeekView)
					{
						SetWorkWeekDates(_date);
						_isAppointmentCacheStale = true;
					}
					InvalidateAppointmentLayout();
				}
			}
		}

		[Category("Dates/Times")]
		[Description("The first day of the week as displayed in the Schedule")]
		[Localizable(true)]
		[DefaultValue(SchedulerDayOfWeek.Sunday)]
		public SchedulerDayOfWeek FirstDayOfWeek
		{
			get
			{
				return _schedulerFirstDayOfWeek;
			}
			set
			{
				if (_schedulerFirstDayOfWeek != value)
				{
					_schedulerFirstDayOfWeek = value;
					_firstDayOfWeek = value;
					InvalidateAppointmentLayout();
				}
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the day-navigation buttons are displayed")]
		[DefaultValue(false)]
		public bool DayNavigationButtons
		{
			get
			{
				return _dayNavigationButtons;
			}
			set
			{
				if (_dayNavigationButtons != value)
				{
					_dayNavigationButtons = value;
					if (_allDayAreaControl != null)
					{
						_allDayAreaControl.UpdateDayNavigationButtons(value);
					}
				}
			}
		}

		[Description("Indicates the start of the hour range displayed by the Schedule on DayView.")]
		[DefaultValue(0)]
		[Category("Dates/Times")]
		public int DayStartHour
		{
			get
			{
				return _dayStartHour;
			}
			set
			{
				if (value >= DayEndHour)
				{
					throw new ArgumentException("The DayEndHour must be bigger than the DayStartHour");
				}
				if (value < 0 || value > 23)
				{
					throw new ArgumentException("Value must be between 0 and 23");
				}
				if (_dayStartHour != value)
				{
					_dayStartHour = value;
					FirstVisibleTime = ClampToVisibleHours(FirstVisibleTime);
					if (_view == SchedulerView.DayView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[Category("Dates/Times")]
		[Description("Indicates the end of the hour range displayed by the Schedule on DayView.")]
		[DefaultValue(24)]
		public int DayEndHour
		{
			get
			{
				return _dayEndHour;
			}
			set
			{
				if (value <= _dayStartHour)
				{
					throw new ArgumentException("The DayEndHour must be bigger than the DayStartHour");
				}
				if (value < 1 || value > 24)
				{
					throw new ArgumentException("Value must be between 1 and 24");
				}
				if (_dayEndHour != value)
				{
					_dayEndHour = value;
					FirstVisibleTime = ClampToVisibleHours(FirstVisibleTime);
					if (_view == SchedulerView.DayView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[Category("Dates/Times")]
		[Description("Indicates the start time of the day for the working hours.")]
		public TimeSpan WorkStartTime
		{
			get
			{
				return _workStartTime;
			}
			set
			{
				if (_workStartTime != value)
				{
					_workStartTime = value;
					if (_selectedDays.Start.TimeOfDay == _workStartTime && _selectedDays.End.TimeOfDay == _workStartTime.Add(new TimeSpan(0, _minuteInterval, 0)))
					{
						SelectDateRange(_selectedDays.Start.Date.Add(_workStartTime), _selectedDays.Start.AddMinutes(_minuteInterval));
					}
					InvalidateAppointmentLayout();
				}
			}
		}

		[Description("Indicates the end time of the day for the working hours.")]
		[Localizable(true)]
		[Category("Dates/Times")]
		public TimeSpan WorkEndTime
		{
			get
			{
				return _workEndTime;
			}
			set
			{
				if (_workEndTime != value)
				{
					_workEndTime = value;
					InvalidateAppointmentLayout();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateRange SelectedDays
		{
			get
			{
				return _selectedDays;
			}
			set
			{
				if (_selectedDays.Start != value.Start || _selectedDays.End != value.End)
				{
					_selectedDays = value;
					OnSelectedDaysChanged(EventArgs.Empty);
					_selectionAnchor = _selectedDays.Start;
					EnsureSelectionInRange();
					_selectionAnchor = _selectedDays.Start;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Localizable(true)]
		[Description("Indicates the number of weeks displayed by the Schedule on month view.")]
		[DefaultValue(5)]
		[Category("Dates/Times")]
		public int Weeks
		{
			get
			{
				return _weeks;
			}
			set
			{
				if (_weeks != value && value <= 6 && value >= 1)
				{
					if (value > 0)
					{
						_weeks = value;
					}
					else
					{
						_weeks = 1;
					}
					InvalidateAppointmentLayout();
				}
			}
		}

		protected CurrencyManager CurrencyManager => _currencyManager;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public SchedulerFieldCollection Fields => _fields;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public SchedulerLayout DesignTimeLayout
		{
			get
			{
				try
				{
					if (ShouldSerializeDesignTimeLayout())
					{
						SchedulerLayout schedulerLayout = new SchedulerLayout();
						schedulerLayout._owner = this;
						schedulerLayout.SetPersistenceMode(PersistenceMode.Fields | PersistenceMode.FormatConditions | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.InternalImageList | PersistenceMode.Dates);
						LayoutWriter mordexLayoutWriter = new LayoutWriter();
						WriteLayoutSections(mordexLayoutWriter, schedulerLayout.GetPersistenceMode());
						LayoutWriter.StoreInLayout("SchedulerLayoutData", mordexLayoutWriter, schedulerLayout);
						schedulerLayout.DataMember = DataMember;
						schedulerLayout.DataSource = DataSource;
						return schedulerLayout;
					}
					return null;
				}
				catch
				{
					return null;
				}
			}
			set
			{
				if (value != null)
				{
					value.SetPersistenceMode(PersistenceMode.Fields | PersistenceMode.FormatConditions | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.InternalImageList | PersistenceMode.Dates);
					LoadLayout(value);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public SchedulerLayoutCollection Layouts => _layouts;

		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[Description("Specifies the property of the data source to map to the Owner property of the SchedulerAppointment objects.")]
		public string OwnerMember
		{
			get
			{
				return _schedulerBindingMemberInfo.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_schedulerBindingMemberInfo))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_schedulerBindingMemberInfo = bindingMemberInfo;
					_ownerField = _schedulerBindingMemberInfo.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_schedulerBindingMemberInfo = bindingMemberInfo;
					_ownerField = _schedulerBindingMemberInfo.BindingField;
				}
			}
		}

		[DefaultValue("")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies the property of the data source to map to the RecurrencePattern property of the SchedulerAppointment objects.")]
		public string RecurrenceInfoMember
		{
			get
			{
				return _recurrenceInfoBindingMember.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_recurrenceInfoBindingMember))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_recurrenceInfoBindingMember = bindingMemberInfo;
					_recurrenceInfoField = _recurrenceInfoBindingMember.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_recurrenceInfoBindingMember = bindingMemberInfo;
					_recurrenceInfoField = _recurrenceInfoBindingMember.BindingField;
				}
			}
		}

		[DefaultValue("")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies specifying the property of the data source to map to some layout properties of the SchedulerAppointment objects.")]
		public string AppointmentSettingsMember
		{
			get
			{
				return _appointmentSettingsBindingMember.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_appointmentSettingsBindingMember))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_appointmentSettingsBindingMember = bindingMemberInfo;
					_appointmentSettingsField = _appointmentSettingsBindingMember.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_appointmentSettingsBindingMember = bindingMemberInfo;
					_appointmentSettingsField = _appointmentSettingsBindingMember.BindingField;
				}
			}
		}

		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[Category("Data")]
		[DefaultValue("")]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies the property of the data source to map to the StartTime property of the SchedulerAppointment objects.")]
		public string StartTimeMember
		{
			get
			{
				return _startTimeBindingMember.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_startTimeBindingMember))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_startTimeBindingMember = bindingMemberInfo;
					_startTimeField = _startTimeBindingMember.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_startTimeBindingMember = bindingMemberInfo;
					_startTimeField = _startTimeBindingMember.BindingField;
				}
			}
		}

		[DefaultValue("")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies the property of the data source to map to the EndTime property of the SchedulerAppointment objects.")]
		public string EndTimeMember
		{
			get
			{
				return _endTimeBindingMember.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_endTimeBindingMember))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_endTimeBindingMember = bindingMemberInfo;
					_endTimeField = _endTimeBindingMember.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_endTimeBindingMember = bindingMemberInfo;
					_endTimeField = _endTimeBindingMember.BindingField;
				}
			}
		}

		[Description("Specifies the property of the data source to map to the Description property of the SchedulerAppointment objects.")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[DefaultValue("")]
		public string DescriptionMember
		{
			get
			{
				return _descriptionBindingMember.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_descriptionBindingMember))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_descriptionBindingMember = bindingMemberInfo;
					_descriptionField = _descriptionBindingMember.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_descriptionBindingMember = bindingMemberInfo;
					_descriptionField = _descriptionBindingMember.BindingField;
				}
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Represents the filter condition applied to the appointments that belongs to the Schedule.")]
		[Editor(typeof(TableFilterConditionEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(TableFilterConverter))]
		public IFilterCondition FilterCondition
		{
			get
			{
				return _filterCondition;
			}
			set
			{
				if (value != null && value.FilterCondition != null && !value.FilterCondition.IsValidForScheduler(this))
				{
					throw new InvalidOperationException("Filter is not properly set or is using fields that doesn't belong to the Schedule.");
				}
				if (_filterCondition == value)
				{
					return;
				}
				if (_filterCondition != null)
				{
					if (_filterCondition is SchedulerFilterCondition)
					{
						((SchedulerFilterCondition)_filterCondition)._filterOwner = null;
					}
					else
					{
						((SchedulerStoredFilter)_filterCondition).SetIsCurrent(false);
					}
				}
				CancelEventArgs e = new CancelEventArgs();
				OnApplyingFilter(e);
				if (e.Cancel)
				{
					return;
				}
				_filterCondition = value;
				if (_filterCondition != null)
				{
					if (_filterCondition is SchedulerFilterCondition)
					{
						((SchedulerFilterCondition)_filterCondition)._filterOwner = this;
					}
					else
					{
						((SchedulerStoredFilter)_filterCondition).SetIsCurrent(true);
					}
				}
				if (_schedulerCurrencyManager != null)
				{
					RebuildFilteredAppointments();
				}
				OnFilterApplied(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		public SchedulerFilterCondition FilterConditionApplied
		{
			get
			{
				if (_filterCondition != null)
				{
					return _filterCondition.FilterCondition;
				}
				return null;
			}
		}

		[Browsable(false)]
		public SchedulerSelectedAppointments SelectedAppointments => _schedulerSelectedAppointments;

		[Category("Appearance")]
		[Description("Determines the appearance of the headers in the Schedule")]
		[DefaultValue(Appearance.Light3D)]
		public Appearance HeaderAppearance
		{
			get
			{
				return _headerAppearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Appearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_headerAppearance != value)
				{
					_headerAppearance = value;
					Invalidate();
					if (_view == SchedulerView.DayView && _allDayAreaControl != null)
					{
						_allDayAreaControl.Invalidate();
					}
				}
			}
		}

		[Description("Indicates whether the weekends should be compressed in one day")]
		[DefaultValue(false)]
		[Localizable(true)]
		[Category("Behavior")]
		public bool CompressWeekEnds
		{
			get
			{
				return _compressWeekEnds;
			}
			set
			{
				if (_compressWeekEnds != value)
				{
					_compressWeekEnds = value;
					if (_view == SchedulerView.MonthView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[Description("Determines whether the Schedule control should scroll its contents when the user drag the scroll thumb along the vertical scroll bar.")]
		[Category("Behavior")]
		[DefaultValue(true)]
		public bool ContinuousScroll
		{
			get
			{
				return _continuousScroll;
			}
			set
			{
				_continuousScroll = value;
			}
		}

		[DefaultValue(0)]
		[Description("Represents the current position of the scroll box on the vertical scroll bar control.")]
		[Browsable(false)]
		public int VerticalScrollPosition
		{
			get
			{
				if (GetVerticalScrollBarControl() != null)
				{
					return GetVerticalScrollBarControl().GetScrollValue();
				}
				return -1;
			}
			set
			{
				if (GetVerticalScrollBarControl() != null)
				{
					if (_view != SchedulerView.DayView)
					{
						_verticalScrollPosition = value;
					}
					Update();
					GetVerticalScrollBarControl().UpdateScrollValue(value);
				}
			}
		}

		[DefaultValue(0)]
		[Browsable(false)]
		[Description("Represents the current position of the scroll box on the horizontal scroll bar control.")]
		public int HorizontalScrollPosition
		{
			get
			{
				if (GetHorizontalScrollBarControl() != null)
				{
					return GetHorizontalScrollBarControl().GetScrollValue();
				}
				return -1;
			}
			set
			{
				if (GetHorizontalScrollBarControl() != null)
				{
					GetHorizontalScrollBarControl().UpdateScrollValue(value);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public SchedulerStoredFilterCollection StoredFilters => _storedFilters;

		[Browsable(false)]
		public Rectangle DayAreaBounds
		{
			get
			{
				if (_isFullLayoutStale)
				{
					RenderCurrentView(null);
				}
				if (_dayLayouts != null)
				{
					if (_view == SchedulerView.DayView)
					{
						return new Rectangle(_dayAreaLeft - _columnGap, _dayAreaTop + 1, base.ClientRectangle.Width - _dayAreaLeft + _columnGap, base.ClientRectangle.Height - _dayAreaTop - 1);
					}
					int top = base.ClientRectangle.Top + _actualRowHeight;
					if (_showOwnersHeader && _currentOwner != null)
					{
						top += _actualRowHeight;
					}
					return new Rectangle(base.ClientRectangle.Left, top, base.ClientRectangle.Width, base.ClientRectangle.Height - top);
				}
				return base.ClientRectangle;
			}
		}

		[Description("Indicates the number of day columns visible in a Schedule control.")]
		[Category("Behavior")]
		[DefaultValue(-1)]
		public int DayColumns
		{
			get
			{
				return _dayColumns;
			}
			set
			{
				if (_dayColumns != value)
				{
					_dayColumns = value;
					if (_view == SchedulerView.DayView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}
	}
}
