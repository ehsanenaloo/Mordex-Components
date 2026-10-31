using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView
	{
		MxScheduler ISchedulerFormatStyleOwner.Scheduler => null;

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.FirstDayOfWeek => ToSchedulerDayOfWeek(FirstDayOfWeek);

		MxScheduler ISchedulerWorkingHoursOwner.Scheduler => null;

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.WorkWeek
		{
			get
			{
				if (_scheduler != null)
				{
					return _scheduler.WorkWeek;
				}
				return SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
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

		[DefaultValue(null)]
		[Description("The Mordex.Windows.Scheduler.MxScheduler control associated with the Calendar.")]
		public MxScheduler Scheduler
		{
			get
			{
				return _scheduler;
			}
			set
			{
				if (_scheduler == value)
				{
					return;
				}
				if (value == null)
				{
					_scheduler.OwnersChanged -= OnSchedulerCurrentOwnerChanged;
					_scheduler.CurrentOwnerChanged -= OnSchedulerCurrentOwnerChanged;
					_scheduler.DatesChanged -= OnSchedulerDatesChanged;
					_scheduler.AppointmentAdded -= OnSchedulerAppointmentAdded;
					_scheduler.AppointmentRemoved -= OnSchedulerAppointmentRemoved;
					_scheduler.AppointmentChanged -= OnSchedulerAppointmentChanged;
					_scheduler.AppointmentsLoaded -= OnSchedulerFilterApplied;
					_scheduler.RemoveAppointmentsResetHandler(OnSchedulerFilterApplied);
					_scheduler.DataSourceChanged -= OnSchedulerFilterApplied;
					_scheduler.FilterApplied -= OnSchedulerFilterApplied;
					_scheduler.RemoveRecurrenceChangedHandler(OnSchedulerFilterApplied);
					_scheduler.MinDateChanged -= OnSchedulerMinDateChanged;
					_scheduler.MaxDateChanged -= OnSchedulerMaxDateChanged;
					ClearAppointmentBoldDates();
				}
				_scheduler = value;
				if (_scheduler != null && ToCalendarDayOfWeek(_scheduler.FirstDayOfWeek) != FirstDayOfWeek)
				{
					_scheduler.FirstDayOfWeek = ToSchedulerDayOfWeek(FirstDayOfWeek);
				}
				Invalidate();
				if (_scheduler == null)
				{
					return;
				}
				if (SelectionStyle == CalendarSelectionStyle.Schedule)
				{
					_minDate = _scheduler.MinDate;
					_calendarControl._minDate = _minDate;
					MaxDate = _scheduler.MaxDate;
					if (_minDate > FirstMonth)
					{
						FirstMonth = _minDate;
					}
				}
				_scheduler.OwnersChanged += OnSchedulerCurrentOwnerChanged;
				_scheduler.CurrentOwnerChanged += OnSchedulerCurrentOwnerChanged;
				_scheduler.DatesChanged += OnSchedulerDatesChanged;
				_scheduler.AppointmentAdded += OnSchedulerAppointmentAdded;
				_scheduler.AppointmentRemoved += OnSchedulerAppointmentRemoved;
				_scheduler.AppointmentChanged += OnSchedulerAppointmentChanged;
				_scheduler.AppointmentsLoaded += OnSchedulerFilterApplied;
				_scheduler.DataSourceChanged += OnSchedulerFilterApplied;
				_scheduler.AddAppointmentsResetHandler(OnSchedulerFilterApplied);
				_scheduler.AddRecurrenceChangedHandler(OnSchedulerFilterApplied);
				_scheduler.FilterApplied += OnSchedulerFilterApplied;
				_scheduler.MinDateChanged += OnSchedulerMinDateChanged;
				_scheduler.MaxDateChanged += OnSchedulerMaxDateChanged;
				if (_selectionStyle == CalendarSelectionStyle.Schedule && !_isUpdatingScheduler)
				{
					DateTime[] schedulerDates = new DateTime[_scheduler.Dates.Count];
					int dateIndex = 0;
					_isWeekSelection = false;
					foreach (DateTime date in _scheduler.Dates)
					{
						schedulerDates.SetValue(date, dateIndex);
						dateIndex++;
					}
					if (schedulerDates.Length > 0)
					{
						ScrollToShowDate(schedulerDates[0]);
					}
					SelectedDates = schedulerDates;
				}
				RequestBoldDatesRefresh();
			}
		}

		[DefaultValue(Appearance.Light3D)]
		[Description("Determines the appearance of the headers in the Calendar")]
		public Appearance HeaderAppearance
		{
			get
			{
				return _headerAppearance;
			}
			set
			{
				if (_headerAppearance != value)
				{
					_headerAppearance = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(CalendarWeekRule.FirstDay)]
		[Description("Specifies which rule is used to determine the first calendar week of the year.")]
		[Localizable(true)]
		public CalendarWeekRule WeekRule
		{
			get
			{
				return _weekRule;
			}
			set
			{
				if (_weekRule != value)
				{
					_weekRule = value;
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(false)]
		[Description("Indicates whether the Calendar control displays week numbers (1-52) to the left of each row of days.")]
		public bool WeekNumbers
		{
			get
			{
				return _weekNumbers;
			}
			set
			{
				if (_weekNumbers != value)
				{
					_weekNumbers = value;
					MeasureMonthLayout();
					UpdateMonthGrid();
					UpdateWorkingHoursSchemaLayoutStateArray(false);
					RefreshMonthLayouts(true);
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifying whether a window with a list of months is displayed when the user clicks on the month header")]
		public bool UseMonthSelector
		{
			get
			{
				return _useMonthSelector;
			}
			set
			{
				_useMonthSelector = value;
			}
		}

		[DefaultValue(true)]
		[Description("Specifies if the control should take the focus when the user press it.")]
		public bool FocusOnClick
		{
			get
			{
				return _focusOnClick;
			}
			set
			{
				_focusOnClick = value;
			}
		}

		[Description("The first day of the week displayed in the Calendar.")]
		[Localizable(true)]
		[DefaultValue(CalendarDayOfWeek.Default)]
		public CalendarDayOfWeek FirstDayOfWeek
		{
			get
			{
				return _firstDayOfWeek;
			}
			set
			{
				if (_firstDayOfWeek != value)
				{
					_firstDayOfWeek = value;
					if (_scheduler != null && ToCalendarDayOfWeek(_scheduler.FirstDayOfWeek) != _firstDayOfWeek)
					{
						_scheduler.FirstDayOfWeek = ToSchedulerDayOfWeek(_firstDayOfWeek);
					}
					UpdateWorkingHoursSchemaLayoutStateArray(false);
					RefreshMonthLayouts(true);
					UpdateWeekdayNames();
					Invalidate();
				}
			}
		}

		[Description("Indicates the format for the days of the week.")]
		[DefaultValue(DayOfWeekAbbreviation.UseFirstLetter)]
		[Localizable(true)]
		public DayOfWeekAbbreviation DayOfWeekAbbreviation
		{
			get
			{
				return _dayOfWeekAbbreviation;
			}
			set
			{
				if (_dayOfWeekAbbreviation != value)
				{
					_dayOfWeekAbbreviation = value;
					UpdateWeekdayNames();
					MeasureMonthLayout();
					UpdateMonthGrid();
					UpdateWorkingHoursSchemaLayoutStateArray(false);
					RefreshMonthLayouts(true);
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		public bool ScrollSelectedDates
		{
			get
			{
				return _scrollSelectedDates;
			}
			set
			{
				_scrollSelectedDates = value;
			}
		}

		[Description("Indicates the first month displayed by the Calendar control.")]
		public DateTime FirstMonth
		{
			get
			{
				return _firstMonth;
			}
			set
			{
				try
				{
					if (!(_firstMonth != value))
					{
						return;
					}
					if (value.Day != 1)
					{
						value = new DateTime(value.Year, value.Month, 1);
					}
					int monthOffset = value.Month - _firstMonth.Month;
					int yearOffset = value.Year - _firstMonth.Year;
					monthOffset += yearOffset * 12;
					DateTime dateTime = value;
					if (CreateDateTime(_minDate) > value)
					{
						FirstMonth = _minDate;
						return;
					}
					DateTime previousFirstMonth = _firstMonth;
					DateTime newFirstMonth = value;
					if (_workingHoursSchemaLayoutStateArray.Length > 1)
					{
						dateTime = value.AddMonths(_workingHoursSchemaLayoutStateArray.Length - 1);
					}
					dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
					if (GetFirstUnavailableMonth() <= dateTime)
					{
						if (_workingHoursSchemaLayoutStateArray.Length == 0)
						{
							FirstMonth = GetFirstUnavailableMonth().AddMonths(-1);
						}
						else
						{
							FirstMonth = GetFirstUnavailableMonth().AddMonths(-_workingHoursSchemaLayoutStateArray.Length);
						}
						return;
					}
					_firstMonth = value;
					UpdateWorkingHoursSchemaLayoutStateArray(false);
					ArrayList arrayList = new ArrayList();
					ArrayList newSelectedDateList = new ArrayList();
					if (_shouldShiftSelectionOnScroll && _scrollSelectedDates && _calendarSelectedDates != null)
					{
						DateTime[] previousSelectedDates = _calendarSelectedDates;
						foreach (DateTime previousSelectedDate in previousSelectedDates)
						{
							arrayList.Add(previousSelectedDate);
						}
						ClearSelectedDays();
					}
					RefreshMonthLayouts(false);
					if (_shouldShiftSelectionOnScroll && _scrollSelectedDates && arrayList.Count > 0)
					{
						bool isSelectionOutOfView = false;
						arrayList.Sort();
						if ((DateTime)arrayList[0] < newFirstMonth || (DateTime)arrayList[0] > dateTime)
						{
							isSelectionOutOfView = true;
						}
						if (isSelectionOutOfView)
						{
							newSelectedDateList = new ArrayList();
							DateTime firstSelectedDate = (DateTime)arrayList[0];
							DateTime shiftedDate = AddMonthsClamped(firstSelectedDate, monthOffset);
							if (_selectionStyle == CalendarSelectionStyle.Schedule && _scheduler != null && _scheduler.View != SchedulerView.DayView && _scheduler.View != SchedulerView.WorkWeek)
							{
								newSelectedDateList = GetWeekDates(FirstMonth, _calendarSelectedDates.Length);
							}
							else
							{
								if (_selectionStyle == CalendarSelectionStyle.Schedule && _scheduler != null && _scheduler.View == SchedulerView.WorkWeek)
								{
									while (previousFirstMonth.Month == shiftedDate.Month && previousFirstMonth.Year == shiftedDate.Year)
									{
										shiftedDate = GetWeekStart(shiftedDate).AddDays(8.0);
									}
								}
								if (!AreSelectedDatesContiguous())
								{
									UpdateCalendarSelectedDates();
								}
								for (int j = 0; j < _calendarSelectedDates.Length; j++)
								{
									if (shiftedDate < MinDate)
									{
										shiftedDate = MinDate;
									}
									if (shiftedDate > MaxDate)
									{
										shiftedDate = MaxDate;
									}
									if (!newSelectedDateList.Contains(shiftedDate))
									{
										newSelectedDateList.Add(shiftedDate);
									}
									if (_selectionStyle == CalendarSelectionStyle.Schedule && _scheduler != null && _scheduler.View == SchedulerView.WorkWeek)
									{
										if (shiftedDate.Month != firstSelectedDate.Month || shiftedDate.Year != firstSelectedDate.Year)
										{
											break;
										}
										newSelectedDateList.Remove(shiftedDate);
										shiftedDate = shiftedDate.AddDays(1.0);
									}
									else
									{
										shiftedDate = shiftedDate.AddDays(1.0);
									}
								}
							}
						}
						else
						{
							foreach (DateTime item in arrayList)
							{
								newSelectedDateList.Add(item);
							}
						}
						DateTime[] newSelectedDates = new DateTime[newSelectedDateList.Count];
						int dateIndex = 0;
						foreach (DateTime newSelectedDate in newSelectedDateList)
						{
							newSelectedDates.SetValue(newSelectedDate, dateIndex);
							dateIndex++;
						}
						if (!HasSameSelectedDates(newSelectedDateList))
						{
							if (SelectionStyle == CalendarSelectionStyle.Schedule && _scheduler != null && _scheduler.View == SchedulerView.WorkWeek)
							{
								_scrollSelectedDates = false;
							}
							SelectedDates = newSelectedDates;
							_scrollSelectedDates = true;
						}
					}
					UpdateSelectionRange(true);
					Invalidate();
				}
				catch
				{
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DateRange DatesRange => _dateRange;

		[Description("The scroll rate for Calendar control.")]
		[DefaultValue(1)]
		public int ScrollChange
		{
			get
			{
				return _scrollChange;
			}
			set
			{
				if (value == 0)
				{
					value = 1;
				}
				_scrollChange = value;
			}
		}

		[Description("The array of DateTime objects that determines which nonrecurring dates are displayed in bold.")]
		[Localizable(true)]
		public DateTime[] BoldedDates
		{
			get
			{
				return _boldedDates;
			}
			set
			{
				if (!_boldedDates.Equals(value))
				{
					RemoveBoldDates(_boldedDates, false);
					_boldedDates = value;
					UpdateSelectionRange(false);
					Invalidate();
				}
			}
		}

		[Description("The array of DateTime objects that determines which annual days are displayed in bold.")]
		[Localizable(true)]
		public DateTime[] AnnuallyBoldedDates
		{
			get
			{
				return _annuallyBoldedDates;
			}
			set
			{
				if (!_annuallyBoldedDates.Equals(value))
				{
					RemoveBoldDates(_annuallyBoldedDates, true);
					_annuallyBoldedDates = value;
					UpdateSelectionRange(false);
					Invalidate();
				}
			}
		}

		[Description("Indicates whether today's date its framed.")]
		[DefaultValue(true)]
		public bool TodayRect
		{
			get
			{
				return _todayRect;
			}
			set
			{
				if (_todayRect != value)
				{
					_todayRect = value;
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WorkingHoursSchema WorkingHourSchema => _workingHoursSchema;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DateTime[] SelectedDates
		{
			get
			{
				return _calendarSelectedDates;
			}
			set
			{
				if (_calendarSelectedDates == value)
				{
					return;
				}
				ArrayList arrayList = new ArrayList();
				int maxSelectedCount = 42;
				if (_selectionStyle == CalendarSelectionStyle.Single)
				{
					maxSelectedCount = 1;
				}
				else if (_selectionStyle != CalendarSelectionStyle.Schedule)
				{
					maxSelectedCount = _maxSelectionCount;
				}
				int addedCount = 0;
				if (value != null)
				{
					foreach (DateTime dateTime in value)
					{
						if (addedCount >= maxSelectedCount)
						{
							break;
						}
						arrayList.Add(dateTime);
						addedCount++;
					}
				}
				if (_calendarSelectedDates == null || !HasSameSelectedDates(arrayList))
				{
					_calendarSelectedDates = value;
					if (_calendarSelectedDates != null)
					{
						RebuildSelectionFromDates();
						if (_calendarSelectedDates.Length > 0)
						{
							_selectionCursorDate = _calendarSelectedDates[0];
							_selectionAnchorDate = _selectionCursorDate;
						}
						if (_calendarSelectedDates.Length > 14 || (_scheduler != null && _scheduler.View != SchedulerView.DayView && _scheduler.View != SchedulerView.WorkWeek))
						{
							_anchorWeekRange = new DateRange(GetWeekStart(_selectionCursorDate), GetWeekEnd(_selectionCursorDate));
							_isWeekSelection = true;
						}
					}
					else
					{
						ClearSelectedDays();
					}
					_selectedDates = value;
					EventArgs e = new EventArgs();
					OnSelectionChanged(e);
				}
				Invalidate();
			}
		}
	}
}
