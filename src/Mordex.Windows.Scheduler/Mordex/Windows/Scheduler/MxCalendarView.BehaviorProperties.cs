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
		[Description("Indicates if the days with appointments should be displayed in bold.")]
		[DefaultValue(BoldDatesMode.All)]
		public BoldDatesMode BoldDatesMode
		{
			get
			{
				return _boldDatesMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BoldDatesMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_boldDatesMode != value)
				{
					_boldDatesMode = value;
					if (value != BoldDatesMode.None && _scheduler != null)
					{
						RequestBoldDatesRefresh();
					}
					else
					{
						ClearAppointmentBoldDates();
					}
					Invalidate();
				}
			}
		}

		[Description("The current date. This property has only effect when the SelectionStyle property of the Calendar control is set as Single. Use the SelectedDates property when the SelectionStyle is other than Single.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Bindable(BindableSupport.Yes)]
		public DateTime CurrentDate
		{
			get
			{
				if (_selectionStyle == CalendarSelectionStyle.Single)
				{
					return _currentDate;
				}
				if (_calendarSelectedDates != null && _calendarSelectedDates.Length > 0)
				{
					return _calendarSelectedDates[0];
				}
				return DateTime.MinValue;
			}
			set
			{
				value = value.Date;
				if (SelectionStyle != CalendarSelectionStyle.Single)
				{
					return;
				}
				if (value != _currentDate)
				{
					InvalidateDay(_currentDate);
					_currentDate = value;
					if (_workingHoursSchemaLayoutStateArray.Length == 0)
					{
						FirstMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
						CommitSelection();
						return;
					}
					if (_currentDate < _firstMonth)
					{
						_shouldShiftSelectionOnScroll = false;
						FirstMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
						_shouldShiftSelectionOnScroll = true;
					}
					else
					{
						DateTime dateTime = ((_workingHoursSchemaLayoutStateArray.Length <= 1) ? _firstMonth.AddMonths(-1) : _firstMonth.AddMonths(_workingHoursSchemaLayoutStateArray.Length - 1));
						dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
						if (_currentDate > dateTime)
						{
							_shouldShiftSelectionOnScroll = false;
							dateTime = new DateTime(_currentDate.Year, _currentDate.Month, 1).AddMonths(-(_workingHoursSchemaLayoutStateArray.Length - 1));
							FirstMonth = dateTime;
							_shouldShiftSelectionOnScroll = true;
							Invalidate();
						}
						else
						{
							InvalidateDay(_currentDate);
						}
					}
					CommitSelection();
					OnCurrentDateChanged(EventArgs.Empty);
				}
				else if (_currentDate < _firstMonth)
				{
					_shouldShiftSelectionOnScroll = false;
					FirstMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
					_shouldShiftSelectionOnScroll = true;
				}
				else
				{
					DateTime lastVisibleDay = ((_workingHoursSchemaLayoutStateArray.Length <= 1) ? _firstMonth.AddMonths(-1) : _firstMonth.AddMonths(_workingHoursSchemaLayoutStateArray.Length - 1));
					lastVisibleDay = new DateTime(lastVisibleDay.Year, lastVisibleDay.Month, DateTime.DaysInMonth(lastVisibleDay.Year, lastVisibleDay.Month));
					if (_currentDate > lastVisibleDay)
					{
						_shouldShiftSelectionOnScroll = false;
						lastVisibleDay = new DateTime(_currentDate.Year, _currentDate.Month, 1).AddMonths(-(_workingHoursSchemaLayoutStateArray.Length - 1));
						FirstMonth = lastVisibleDay;
						_shouldShiftSelectionOnScroll = true;
						Invalidate();
					}
					else
					{
						InvalidateDay(_currentDate);
					}
				}
			}
		}

		[Localizable(true)]
		[Description("The maximum allowable date.")]
		public DateTime MaxDate
		{
			get
			{
				return _maxDate;
			}
			set
			{
				if (_maxDate != value.Date)
				{
					if (value.Date < _minDate)
					{
						throw new ArgumentOutOfRangeException("The MaxDate must be greater or equal than the MinDate");
					}
					if (value.Date > new DateTime(9998, 12, 31))
					{
						throw new ArgumentOutOfRangeException(value.ToShortDateString() + " is not a valid value for 'MaxDate'.  'MaxDate' must be lower than or equal to 12/31/9998");
					}
					_maxDate = value.Date;
					if (_scheduler != null && SelectionStyle == CalendarSelectionStyle.Schedule)
					{
						_scheduler.MaxDate = _maxDate;
					}
					_calendarControl._maxDate = _maxDate;
					DateTime dateTime = _firstMonth.AddMonths(1);
					if (_workingHoursSchemaLayoutStateArray.Length > 1)
					{
						dateTime = _firstMonth.AddMonths(_workingHoursSchemaLayoutStateArray.Length - 1);
					}
					dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
					if (_maxDate < dateTime)
					{
						FirstMonth = _maxDate.AddMonths(-(_workingHoursSchemaLayoutStateArray.Length - 1));
						return;
					}
					MeasureMonthLayout();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					RefreshMonthLayouts(true);
					UpdateMonthGrid();
					Invalidate();
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.DateTimeConverter))]
		[Editor(typeof(DateTimeEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[Description("The minimum allowable date.")]
		public DateTime MinDate
		{
			get
			{
				return _minDate;
			}
			set
			{
				if (_minDate != value.Date)
				{
					if (value.Date > _maxDate)
					{
						throw new ArgumentOutOfRangeException("The MinDate must be lower or equal than the MaxDate");
					}
					if (value.Date < new DateTime(1, 2, 1))
					{
						throw new ArgumentOutOfRangeException(value.ToShortDateString() + " is not a valid value for 'MinDate'.  'MinDate' must be greater than or equal to 2/1/0001");
					}
					_minDate = value.Date;
					if (_scheduler != null && SelectionStyle == CalendarSelectionStyle.Schedule)
					{
						_scheduler.MinDate = _minDate;
					}
					_calendarControl._minDate = _minDate;
					if (_minDate > FirstMonth)
					{
						FirstMonth = _minDate;
						return;
					}
					MeasureMonthLayout();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					RefreshMonthLayouts(true);
					UpdateMonthGrid();
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DateRange SelectionRange
		{
			get
			{
				if (_selectionStyle != CalendarSelectionStyle.Range)
				{
					if (AreSelectedDatesContiguous())
					{
						_selectionRange = new DateRange(_calendarSelectedDates[0], _calendarSelectedDates[_calendarSelectedDates.Length - 1]);
					}
					else
					{
						_selectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
					}
				}
				return _selectionRange;
			}
			set
			{
				Region region = new Region(Rectangle.Empty);
				DateRange dateRange = value;
				if (dateRange.Start > dateRange.End)
				{
					DateTime start = dateRange.Start;
					dateRange.Start = dateRange.End;
					dateRange.End = start;
				}
				int dayCount = GetInclusiveDayCount(dateRange.Start, dateRange.End);
				if (dayCount > _maxSelectionCount)
				{
					if (_selectionAnchorDate == DateTime.MinValue)
					{
						dateRange.End = dateRange.Start.AddDays(_maxSelectionCount);
					}
					else if (_selectionAnchorDate > dateRange.Start)
					{
						dateRange.Start = _selectionAnchorDate.AddDays(-_maxSelectionCount + 1);
					}
					else
					{
						dateRange.End = _selectionAnchorDate.AddDays(_maxSelectionCount - 1);
					}
				}
				if (dateRange.Start == _selectionRange.Start && dateRange.End == _selectionRange.End)
				{
					return;
				}
				if (!_selectionRange.IsIncomplete())
				{
					DateTime start = _selectionRange.Start;
					while (start <= _selectionRange.End)
					{
						SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = FindDayCellByDate(start);
						if (schedulerFormatStyleLayoutState != null)
						{
							schedulerFormatStyleLayoutState.SetSelected(false);
							region.Union(schedulerFormatStyleLayoutState.GetBounds());
							schedulerFormatStyleLayoutState = null;
						}
						start = start.AddDays(1.0);
					}
				}
				if (dateRange.Start == DateTime.MinValue)
				{
					dateRange.Start = dateRange.End;
				}
				if (dateRange.End == DateTime.MinValue)
				{
					dateRange.End = dateRange.Start;
				}
				_selectionRange = dateRange;
				if (!_selectionRange.IsIncomplete())
				{
					DateTime start = _selectionRange.Start;
					while (start <= _selectionRange.End)
					{
						SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = FindDayCellByDate(start);
						if (schedulerFormatStyleLayoutStateArgument != null)
						{
							schedulerFormatStyleLayoutStateArgument.SetSelected(true);
							region.Union(schedulerFormatStyleLayoutStateArgument.GetBounds());
							schedulerFormatStyleLayoutStateArgument = null;
						}
						start = start.AddDays(1.0);
					}
				}
				Invalidate(region);
				region.Dispose();
				region = null;
				CommitSelection();
			}
		}

		[Description("The maximum number of days that can be selected in the Calendar control.")]
		[DefaultValue(7)]
		public int MaxSelectionCount
		{
			get
			{
				return _maxSelectionCount;
			}
			set
			{
				if (_maxSelectionCount != value)
				{
					_maxSelectionCount = value;
					if (_selectionStyle == CalendarSelectionStyle.Range)
					{
						SelectionRange = _selectionRange;
					}
					else if (_selectionStyle == CalendarSelectionStyle.Free)
					{
						Invalidate();
					}
				}
			}
		}
	}
}
