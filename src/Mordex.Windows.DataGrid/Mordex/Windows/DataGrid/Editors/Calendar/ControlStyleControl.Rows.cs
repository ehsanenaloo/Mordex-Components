using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal partial class ControlStyleControl
	{
		private void InitializeChildControls()
		{
			_innerCalendarControl = new ScrollingCalendarControl();
			_innerCalendarControl._owner = this;
			_timer = new Timer();
			_innerCalendarControl.Visible = false;
			_innerCalendarControl.Location = new Point(7, 34);
			_innerCalendarControl.TabIndex = 0;
			_innerCalendarControl.Size = new Size(0, 0);
			_timer.Interval = Timing.AutoRepeatInterval;
			_timer.Tick += OnTimerTick;
			BackColor = SystemColors.Window;
			Text = "CalendarDropDown";
		}

		private void SelectToday()
		{
			SetSelectedValue(DateTime.Now.Date);
			CloseDropDown(false);
		}

		private void SelectNone()
		{
			_isValueEmpty = true;
			CloseDropDown(false);
		}

		internal void SelectMonthFromPopup(object value)
		{
			if (value != null)
			{
				SetVisibleMonth(((DateTime)value).AddMonths(-_clickedMonthIndex));
			}
			GridHelper.SetCapture(this, true);
		}

		private void ResetRangeSelection()
		{
			try
			{
				if (_selectionStyleEnum == SelectionStyleEnum.Range)
				{
					CalendarValueState controlStyleControlCalendarValueState = _controlStyleControlCalendarValueState;
					_controlStyleControlCalendarValueState = new CalendarValueState(null, null);
					ApplyRangeSelection(controlStyleControlCalendarValueState);
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		public object GetSelectedValue()
		{
			if (!_isValueEmpty)
			{
				return _controlStyleControlDateTime;
			}
			return null;
		}

		public void SetSelectedValue(object value)
		{
			_isValueEmpty = false;
			if (value == null)
			{
				value = DateTime.Now.Date;
				_isValueEmpty = true;
			}
			InvalidateDate(_controlStyleControlDateTime);
			_controlStyleControlDateTime = (DateTime)value;
			if (_controlStyleControlDateTime < _dateTime)
			{
				SetVisibleMonth(new DateTime(_controlStyleControlDateTime.Year, _controlStyleControlDateTime.Month, 1));
				return;
			}
			int otherValue = _monthBlocks.Length;
			if (otherValue == 0)
			{
				otherValue = 1;
			}
			DateTime dateTime = _dateTime.AddMonths(otherValue - 1);
			dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
			if (_controlStyleControlDateTime > dateTime)
			{
				dateTime = new DateTime(_controlStyleControlDateTime.Year, _controlStyleControlDateTime.Month, 1);
				SetVisibleMonth(dateTime);
			}
			else
			{
				InvalidateDate(_controlStyleControlDateTime);
			}
		}

		public void ApplyRangeSelection(CalendarValueState calendarValueState)
		{
			Region region = new Region(new Rectangle(0, 0, 0, 0));
			CalendarValueState otherCalendarValueState = calendarValueState;
			if (GridHelper.CompareDates(otherCalendarValueState.GetStart(), otherCalendarValueState.GetEnd()) > 0)
			{
				DateTime dateTime = (DateTime)otherCalendarValueState.GetStart();
				otherCalendarValueState.UpdateStart(otherCalendarValueState.GetEnd());
				otherCalendarValueState.UpdateEnd(dateTime);
			}
			int subtractCore = CountDaysInclusive((DateTime)otherCalendarValueState.GetStart(), (DateTime)otherCalendarValueState.GetEnd());
			if (subtractCore > _maxRangeDays)
			{
				if (_rangeStartDate == null)
				{
					otherCalendarValueState.UpdateEnd(((DateTime)otherCalendarValueState.GetStart()).AddDays(_maxRangeDays));
				}
				else if (GridHelper.CompareDates(_rangeStartDate, otherCalendarValueState.GetStart()) < 0)
				{
					otherCalendarValueState.UpdateStart(((DateTime)_rangeStartDate).AddDays(-_maxRangeDays + 1));
				}
				else
				{
					otherCalendarValueState.UpdateEnd(((DateTime)_rangeStartDate).AddDays(_maxRangeDays - 1));
				}
			}
			if (otherCalendarValueState.GetStart() == _controlStyleControlCalendarValueState.GetStart() && otherCalendarValueState.GetEnd() == _controlStyleControlCalendarValueState.GetEnd())
			{
				return;
			}
			if (_controlStyleControlCalendarValueState.GetStart() != null && _controlStyleControlCalendarValueState.GetEnd() != null)
			{
				DateTime dateTime = (DateTime)_controlStyleControlCalendarValueState.GetStart();
				while (GridHelper.CompareDates(dateTime, _controlStyleControlCalendarValueState.GetEnd()) <= 0)
				{
					CalendarDayCell dayCellByDateInBlocks = FindDayCellByDateInBlocks(dateTime);
					if (dayCellByDateInBlocks != null)
					{
						dayCellByDateInBlocks.SetIsSelected(false);
						region.Union(dayCellByDateInBlocks.GetBounds());
						dayCellByDateInBlocks = null;
					}
					dateTime = dateTime.AddDays(1.0);
				}
			}
			if (otherCalendarValueState.GetStart() == null)
			{
				otherCalendarValueState.UpdateStart(otherCalendarValueState.GetEnd());
			}
			if (otherCalendarValueState.GetEnd() == null)
			{
				otherCalendarValueState.UpdateEnd(otherCalendarValueState.GetStart());
			}
			_controlStyleControlCalendarValueState = otherCalendarValueState;
			if (_controlStyleControlCalendarValueState.GetStart() != null && _controlStyleControlCalendarValueState.GetEnd() != null)
			{
				DateTime dateTime = (DateTime)_controlStyleControlCalendarValueState.GetStart();
				while (GridHelper.CompareDates(dateTime, _controlStyleControlCalendarValueState.GetEnd()) <= 0)
				{
					CalendarDayCell calendarLayoutState2Argument = FindDayCellByDateInBlocks(dateTime);
					if (calendarLayoutState2Argument != null)
					{
						calendarLayoutState2Argument.SetIsSelected(true);
						region.Union(calendarLayoutState2Argument.GetBounds());
						calendarLayoutState2Argument = null;
					}
					dateTime = dateTime.AddDays(1.0);
				}
			}
			Invalidate(region);
		}

		internal GridFormatStyle GetSelectedStyle()
		{
			if (_gridFormatStyle == null)
			{
				_gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.SelectedFormatStyle);
			}
			return _gridFormatStyle;
		}

		internal void SetSelectedStyle(GridFormatStyle gridFormatStyle)
		{
			_gridFormatStyle = gridFormatStyle;
			if (_gridFormatStyle != null)
			{
				_gridFormatStyle = new GridFormatStyle(_gridFormatStyle);
				if (_gridFormatStyle.ForeColor == Color.Empty)
				{
					_gridFormatStyle.ForeColor = SystemColors.HighlightText;
				}
				if (_gridFormatStyle.BackColor == Color.Empty)
				{
					_gridFormatStyle.ForeColor = SystemColors.Highlight;
				}
			}
		}
	}
}
