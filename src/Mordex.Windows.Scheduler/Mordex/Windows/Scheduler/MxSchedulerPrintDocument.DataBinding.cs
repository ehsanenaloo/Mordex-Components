using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	public partial class MxSchedulerPrintDocument
	{
		private void UpdateTimeColumnWidth(Graphics graphics)
		{
			StringFormat stringFormat = new StringFormat();
			stringFormat.LineAlignment = StringAlignment.Center;
			UpdateTimeLabelFont();
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string text = currentCulture.DateTimeFormat.AMDesignator;
			if (text == "")
			{
				text = "00";
			}
			int amWidth = (int)graphics.MeasureString(text, PrintDayFont, 0).Width + 2;
			int timeColumnWidth = 0;
			timeColumnWidth = ((_interval == Interval.SixtyMinutes) ? ((int)graphics.MeasureString("12:", _timeLabelFont, 0, stringFormat).Width + amWidth) : ((int)graphics.MeasureString("12", _timeLabelFont, 0, stringFormat).Width + amWidth));
			stringFormat.Dispose();
			stringFormat = null;
			_dayColumnsLeft = _marginBounds.Left + timeColumnWidth + 10;
		}

		private void LayoutDayColumn(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (schedulerHourRangeCollectionLayoutState == null)
			{
				LayoutMultiDayColumns(graphics);
				return;
			}
			int slotsPerHour = (int)_interval;
			double visibleHours = EndTime.TotalHours - StartTime.TotalHours;
			int slotCount = (int)(visibleHours * (double)slotsPerHour);
			if (slotCount == 0)
			{
				slotCount++;
			}
			else if (slotCount < 0)
			{
				throw new InvalidOperationException();
			}
			if (_rowHeight == 0)
			{
				FitSlotHeight(slotCount, graphics, _marginBounds.Height);
			}
			UpdateTimeColumnWidth(graphics);
			if (_timeGridTop == 0)
			{
				_timeGridTop = _marginBounds.Top + _rowHeight;
				if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
				{
					_timeGridTop += _rowHeight;
				}
				if (_scheduler.ShowAllDayArea)
				{
					_timeGridTop += 5;
					if (_scheduler._allDayAppointmentViews.Count > 0)
					{
						_timeGridTop += _rowHeight;
					}
				}
			}
			int gridHeight = _marginBounds.Bottom - _timeGridTop;
			FitSlotHeight(slotCount, graphics, gridHeight);
			slotCount = (int)(visibleHours * (double)_interval);
			int columnWidth = (_dayColumnWidth = _marginBounds.Right - _dayColumnsLeft);
			if (LayoutAllDayAppointments(graphics, schedulerHourRangeCollectionLayoutState))
			{
				return;
			}
			double extraHeight = ((double)_marginBounds.Bottom - (double)_timeGridTop) / (double)_rowHeight;
			int visibleSlotCount = slotCount;
			extraHeight = (extraHeight - (double)visibleSlotCount) * (double)_rowHeight;
			int roundedExtraHeight = (int)Math.Round(extraHeight);
			if (_scheduler.ShowAllDayArea)
			{
				_timeGridTop += roundedExtraHeight;
			}
			else
			{
				_timeGridTop = _marginBounds.Top + _rowHeight;
				if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
				{
					_timeGridTop += _rowHeight;
				}
			}
			_currentRowTop = _timeGridTop;
			int x = _dayColumnsLeft;
			int width = columnWidth;
			int slotMinutes = 60 / (int)_interval;
			SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = new SchedulerHourRangeCollectionLayoutState(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetOwner());
			schedulerHourRangeCollectionLayoutStateArgument.SetLaneCount(schedulerHourRangeCollectionLayoutState.GetLaneCount());
			schedulerHourRangeCollectionLayoutStateArgument.SetSchedulerHourRangeCollection(schedulerHourRangeCollectionLayoutState.GetSchedulerHourRangeCollection());
			_currentRowTop = _timeGridTop;
			DateTime dateTime = schedulerHourRangeCollectionLayoutStateArgument.GetDate().Add(StartTime);
			for (int i = 0; i < slotCount; i++)
			{
				Rectangle bounds = new Rectangle(x, _currentRowTop, width, _rowHeight);
				SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(i, bounds);
				schedulerAppointmentOwnerLayoutState.UpdateDate(dateTime);
				_currentRowTop += _rowHeight;
				schedulerAppointmentOwnerLayoutState.SetDayOfWeek(schedulerHourRangeCollectionLayoutStateArgument.GetDate().DayOfWeek);
				schedulerAppointmentOwnerLayoutState.SetWorkingHour(_scheduler.IsWorkingTimeSlot(schedulerHourRangeCollectionLayoutStateArgument.GetSchedulerHourRangeCollection(), dateTime, new TimeSpan(0, _scheduler.MinuteInterval, 0)));
				schedulerAppointmentOwnerLayoutState.SetIndex(i);
				schedulerHourRangeCollectionLayoutStateArgument.GetTimeSlots().Add(schedulerAppointmentOwnerLayoutState);
				schedulerAppointmentOwnerLayoutState.SetSchedulerHourRangeCollectionLayoutState(schedulerHourRangeCollectionLayoutStateArgument);
				dateTime = dateTime.AddMinutes(slotMinutes);
			}
			_timeGridBottom = _currentRowTop;
			_gridBounds = new Rectangle(_dayColumnsLeft - 5, _timeGridTop, _marginBounds.Right - (_dayColumnsLeft - 5), _timeGridBottom - _timeGridTop);
			schedulerHourRangeCollectionLayoutStateArgument._bounds = new Rectangle(x, _timeGridTop, width, _timeGridBottom - _timeGridTop);
			_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutStateArgument);
		}

		private void LayoutMultiDayColumns(Graphics graphics)
		{
			int slotsPerHour = (int)_interval;
			double visibleHours = EndTime.TotalHours - StartTime.TotalHours;
			int slotCount = (int)(visibleHours * (double)slotsPerHour);
			if (slotCount == 0)
			{
				slotCount++;
			}
			else if (slotCount < 0)
			{
				throw new InvalidOperationException();
			}
			if (_rowHeight == 0)
			{
				FitSlotHeight(slotCount, graphics, _marginBounds.Height);
			}
			UpdateTimeColumnWidth(graphics);
			if (_timeGridTop == 0)
			{
				_timeGridTop = _marginBounds.Top + _rowHeight;
				if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
				{
					_timeGridTop += _rowHeight;
				}
				if (_scheduler.ShowAllDayArea)
				{
					_timeGridTop += 5;
					if (_scheduler._allDayAppointmentViews.Count > 0)
					{
						_timeGridTop += _rowHeight;
					}
				}
			}
			int gridHeight = _marginBounds.Bottom - _timeGridTop;
			FitSlotHeight(slotCount, graphics, gridHeight);
			slotCount = (int)(visibleHours * (double)_interval);
			int columnCount = _scheduler.Dates.Count;
			if (_scheduler.IsMultiOwnerActive())
			{
				columnCount *= _scheduler._visibleOwners.Count;
			}
			int columnWidth = (_dayColumnWidth = (_marginBounds.Right - _dayColumnsLeft - (columnCount - 1) * 5) / columnCount);
			LayoutAllDayAppointments(graphics, null);
			double extraHeight = ((double)_marginBounds.Bottom - (double)_timeGridTop) / (double)_rowHeight;
			int visibleSlotCount = slotCount;
			extraHeight = (extraHeight - (double)visibleSlotCount) * (double)_rowHeight;
			int roundedExtraHeight = (int)Math.Round(extraHeight);
			if (_scheduler.ShowAllDayArea)
			{
				_timeGridTop += roundedExtraHeight;
			}
			else
			{
				_timeGridTop = _marginBounds.Top + _rowHeight;
				if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
				{
					_timeGridTop += _rowHeight;
				}
			}
			_currentRowTop = _timeGridTop;
			int currentLeft = _dayColumnsLeft;
			int widthRemainder = _marginBounds.Right - (columnWidth * columnCount + (columnCount - 1) * 5 + _dayColumnsLeft);
			int currentColumnWidth = columnWidth;
			int slotMinutes = 60 / (int)_interval;
			_dayLayouts = new SchedulerHourRangeLayoutCollection();
			foreach (SchedulerHourRangeCollectionLayoutState item in _scheduler._dayLayouts)
			{
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item.GetDate(), item.GetOwner());
				schedulerHourRangeCollectionLayoutState.SetLaneCount(item.GetLaneCount());
				schedulerHourRangeCollectionLayoutState.SetSchedulerHourRangeCollection(item.GetSchedulerHourRangeCollection());
				if (item.GetDayIndex() == _scheduler._dayLayouts.Count - 1)
				{
					currentColumnWidth += widthRemainder;
				}
				_currentRowTop = _timeGridTop;
				DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate().Add(StartTime);
				for (int i = 0; i < slotCount; i++)
				{
					Rectangle bounds = new Rectangle(currentLeft, _currentRowTop, currentColumnWidth, _rowHeight);
					SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(i, bounds);
					schedulerAppointmentOwnerLayoutState.UpdateDate(dateTime);
					_currentRowTop += _rowHeight;
					schedulerAppointmentOwnerLayoutState.SetWorkingHour(_scheduler.IsWorkingTimeSlot(schedulerHourRangeCollectionLayoutState.GetSchedulerHourRangeCollection(), dateTime, new TimeSpan(0, _scheduler.MinuteInterval, 0)));
					schedulerAppointmentOwnerLayoutState.SetIndex(i);
					schedulerHourRangeCollectionLayoutState.GetTimeSlots().Add(schedulerAppointmentOwnerLayoutState);
					schedulerAppointmentOwnerLayoutState.SetSchedulerHourRangeCollectionLayoutState(schedulerHourRangeCollectionLayoutState);
					dateTime = dateTime.AddMinutes(slotMinutes);
				}
				_timeGridBottom = _currentRowTop;
				schedulerHourRangeCollectionLayoutState._bounds = new Rectangle(currentLeft, _timeGridTop, currentColumnWidth, _timeGridBottom - _timeGridTop);
				currentLeft += currentColumnWidth + 5;
				_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutState);
			}
			_gridBounds = new Rectangle(_dayColumnsLeft - 5, _timeGridTop, _marginBounds.Right - (_dayColumnsLeft - 5), _timeGridBottom - _timeGridTop);
		}
	}
}
