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
		private void RenderMonthView(Graphics graphics)
		{
			if (IsHorizontalScrollRequired())
			{
				SetHorizontalScrollRequired(false);
				return;
			}
			base.SetHeaderHeight(0);
			if (_weeks == 1)
			{
				View = SchedulerView.WeekView;
				RenderWeekView(graphics);
				return;
			}
			RebuildDates();
			UpdateRowHeight();
			int weekHeaderWidth = 0;
			bool hasWeekHeaders = false;
			if (_showWeekHeaders == TriState.True)
			{
				weekHeaderWidth = _actualRowHeight;
				hasWeekHeaders = true;
			}
			int rowHeight = _actualRowHeight;
			int gridHeight = base.ClientRectangle.Height - rowHeight;
			int cellLeft = base.ClientRectangle.Left + weekHeaderWidth;
			int cellTop = rowHeight + base.ClientRectangle.Top;
			if (IsMultiOwnerActive() && _showOwnersHeader)
			{
				cellTop += rowHeight + 1;
				gridHeight -= rowHeight + 1;
			}
			_ = base.ClientRectangle.Top;
			GetVerticalScrollBarControl().UpdateMaximum(104);
			UpdateVerticalScrollBar(0, 104, Weeks, _verticalScrollPosition);
			base.SetVerticalScrollRequired(true);
			int dayColumnCount = 7;
			if (_compressWeekEnds)
			{
				dayColumnCount--;
			}
			int extraWidth = (base.ClientRectangle.Width - weekHeaderWidth) % dayColumnCount;
			int extraHeight = gridHeight % _weeks;
			extraWidth++;
			extraHeight++;
			int weekHeight = gridHeight / _weeks;
			int cellWidth = (base.ClientRectangle.Width - weekHeaderWidth) / dayColumnCount;
			_dayLayouts.Clear();
			Rectangle empty = Rectangle.Empty;
			int halfWeekHeight = weekHeight / 2;
			int halfWeekRemainder = weekHeight % 2;
			int weekRow = 1;
			string text = string.Empty;
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			bool isSaturday = false;
			bool isSunday = false;
			int columnNumber = 1;
			int headerCount = 0;
			_headerLayouts = new SchedulerOwnerLayoutCollection();
			_gridBounds = new Rectangle(base.ClientRectangle.Left + weekHeaderWidth, cellTop, base.ClientRectangle.Width - weekHeaderWidth, gridHeight);
			int headerTop = base.ClientRectangle.Top;
			_weekHeaderLayouts = new SchedulerOwnerLayoutCollection();
			if (IsMultiOwnerActive() && _showOwnersHeader)
			{
				headerTop += _actualRowHeight;
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			foreach (DateTime item in _dates)
			{
				if (text == string.Empty || item.Day == 1)
				{
					string monthName = currentInfo.GetMonthName(item.Month);
					text = currentInfo.GetMonthName(item.Month) + " " + item.Day;
					int monthNameWidth = (int)textLayoutRenderer.RenderContent(graphics, text, MonthFont, 0).Width;
					if (monthNameWidth >= cellWidth)
					{
						text = currentInfo.GetAbbreviatedMonthName(item.Month) + " " + item.Day;
					}
				}
				else
				{
					text = item.Day.ToString(Application.CurrentCulture);
				}
				int cellHeight = weekHeight;
				if (_compressWeekEnds)
				{
					if (item.DayOfWeek == DayOfWeek.Saturday)
					{
						cellHeight = halfWeekHeight;
						isSaturday = true;
					}
					else if (item.DayOfWeek == DayOfWeek.Sunday)
					{
						cellHeight = halfWeekHeight + halfWeekRemainder;
						isSunday = true;
					}
				}
				if (weekRow == _weeks && !isSaturday)
				{
					cellHeight += extraHeight;
				}
				empty = new Rectangle(cellLeft, cellTop, cellWidth, cellHeight);
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(item, 0, _dateStyleOverrides.Count - 1);
				if (schedulerHourRangeCollectionLayoutState == null)
				{
					schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item, empty, text);
				}
				else
				{
					schedulerHourRangeCollectionLayoutState._bounds = empty;
					schedulerHourRangeCollectionLayoutState.UpdateCaption(text);
				}
				schedulerHourRangeCollectionLayoutState._schedulerAppointmentOwnerLayoutState = null;
				if (item < _minDate || item > _maxDate)
				{
					schedulerHourRangeCollectionLayoutState.SetIsDateInRange(false);
				}
				else
				{
					schedulerHourRangeCollectionLayoutState.SetIsDateInRange(true);
				}
				schedulerHourRangeCollectionLayoutState.SetColumnIndex(columnNumber);
				if (empty.Right + cellWidth > GetClientRight())
				{
					schedulerHourRangeCollectionLayoutState._bounds.Width += extraWidth;
					if (schedulerHourRangeCollectionLayoutState._schedulerAppointmentOwnerLayoutState != null)
					{
						schedulerHourRangeCollectionLayoutState._schedulerAppointmentOwnerLayoutState._bounds.Width += extraWidth;
					}
					if (isSunday)
					{
						if (hasWeekHeaders)
						{
							int weekendHeaderOffset = 0;
							int weekHeaderTop = cellTop + weekendHeaderOffset;
							if (_compressWeekEnds)
							{
								weekHeaderTop -= halfWeekHeight;
							}
							Rectangle rect = new Rectangle(base.ClientRectangle.Left, weekHeaderTop, weekHeaderWidth, weekHeight - weekendHeaderOffset);
							SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(rect, "");
							schedulerAppointmentOwnerLayoutState._dateRange = new DateRange(item.AddDays(-6.0), item);
							_weekHeaderLayouts.Add(schedulerAppointmentOwnerLayoutState);
						}
						cellTop += cellHeight;
						cellLeft = base.ClientRectangle.Left + weekHeaderWidth;
						weekRow++;
						columnNumber = 1;
					}
					else if (!isSaturday)
					{
						if (hasWeekHeaders)
						{
							int headerOffset = 0;
							Rectangle weekHeaderBounds = new Rectangle(base.ClientRectangle.Left, cellTop + headerOffset, weekHeaderWidth, cellHeight - headerOffset);
							SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutStateArgument = new SchedulerAppointmentOwnerLayoutState(weekHeaderBounds, "");
							schedulerAppointmentOwnerLayoutStateArgument._dateRange = new DateRange(item.AddDays(-6.0), item);
							_weekHeaderLayouts.Add(schedulerAppointmentOwnerLayoutStateArgument);
						}
						cellLeft = base.ClientRectangle.Left + weekHeaderWidth;
						cellTop += cellHeight;
						weekRow++;
						columnNumber = 1;
					}
					else
					{
						cellTop += cellHeight;
					}
				}
				else
				{
					cellLeft += cellWidth;
					columnNumber++;
					if (_compressWeekEnds)
					{
						if (isSaturday)
						{
							cellTop += cellHeight;
							cellLeft -= cellWidth;
							columnNumber--;
						}
						else if (isSunday)
						{
							cellTop -= cellHeight - halfWeekRemainder;
							if (weekRow == _weeks)
							{
								cellTop += extraHeight;
							}
						}
					}
				}
				if (headerCount < 7)
				{
					bool shouldAddDayHeader = false;
					DayOfWeek dayOfWeek = DayOfWeek.Monday;
					if (isSaturday)
					{
						shouldAddDayHeader = true;
						dayOfWeek = item.DayOfWeek;
					}
					else if (!isSunday)
					{
						shouldAddDayHeader = true;
						dayOfWeek = item.DayOfWeek;
					}
					if (shouldAddDayHeader)
					{
						SchedulerAppointmentOwnerLayoutState dayHeaderCell = new SchedulerAppointmentOwnerLayoutState(new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left, headerTop, schedulerHourRangeCollectionLayoutState._bounds.Width, _actualRowHeight), "");
						if (dayHeaderCell._bounds.Right > GetClientRight())
						{
							dayHeaderCell._bounds.Width--;
						}
						dayHeaderCell.SetDayOfWeek(dayOfWeek);
						dayHeaderCell.UpdateDate(schedulerHourRangeCollectionLayoutState.GetDate());
						_headerLayouts.Add(dayHeaderCell);
					}
					headerCount++;
				}
				isSaturday = false;
				isSunday = false;
				if (IsMultiOwnerActive())
				{
					schedulerHourRangeCollectionLayoutState.SetOwner(CurrentOwner);
				}
				_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutState);
			}
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
			RenderContent(graphics, textLayoutRenderer, schedulerFormatStyle.ResolveFont(MonthFont), false);
			textLayoutRenderer.ResetFormat();
			schedulerFormatStyle.DisposeResources();
			LayoutMonthViewAppointments();
			_isFullLayoutStale = false;
			SetDayNameHeaderTexts(graphics);
		}

		private void RenderWeekView(Graphics graphics)
		{
			bool ownsGraphics = false;
			if (IsHorizontalScrollRequired())
			{
				SetHorizontalScrollRequired(false);
				return;
			}
			base.SetHeaderHeight(0);
			_weekHeaderLayouts = null;
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			RebuildDates();
			UpdateRowHeight();
			int rowHeight = _actualRowHeight;
			UpdateVerticalScrollBar(0, 104, 1, _verticalScrollPosition);
			base.SetVerticalScrollRequired(true);
			int cellHeight = 0;
			int cellTop = base.ClientRectangle.Top;
			if (!IsMultiOwnerActive() || !_showOwnersHeader)
			{
				cellHeight = base.ClientRectangle.Height / 3;
			}
			else
			{
				cellHeight = (base.ClientRectangle.Height - rowHeight) / 3;
				cellTop += rowHeight;
			}
			int columnTop = cellTop;
			int cellWidth = base.ClientRectangle.Width / 2;
			int extraWidth = base.ClientRectangle.Width % 2;
			int extraHeight = base.ClientRectangle.Height % 3;
			extraWidth++;
			extraHeight++;
			_dayLayouts.Clear();
			Rectangle empty = Rectangle.Empty;
			int weekendCellHeight = cellHeight / 2;
			int weekendRemainder = cellHeight % 2;
			int columnIndex = 1;
			int cellLeft = base.ClientRectangle.Left;
			_headerLayouts = new SchedulerOwnerLayoutCollection();
			_gridBounds = base.ClientRectangle;
			foreach (DateTime item in _dates)
			{
				if (columnIndex == 2)
				{
					cellWidth += extraWidth;
					columnIndex = 0;
				}
				int currentCellHeight = cellHeight;
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState;
				if (item.DayOfWeek == DayOfWeek.Saturday)
				{
					empty = new Rectangle(cellLeft, cellTop, cellWidth, weekendCellHeight);
					schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(item, 0, _dateStyleOverrides.Count - 1);
					if (schedulerHourRangeCollectionLayoutState == null)
					{
						schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item, empty);
					}
					else
					{
						schedulerHourRangeCollectionLayoutState._bounds = empty;
					}
					currentCellHeight = weekendCellHeight;
				}
				else if (item.DayOfWeek == DayOfWeek.Sunday)
				{
					empty = new Rectangle(cellLeft, cellTop, cellWidth, weekendCellHeight + weekendRemainder);
					schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(item, 0, _dateStyleOverrides.Count - 1);
					if (schedulerHourRangeCollectionLayoutState == null)
					{
						schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item, empty);
					}
					else
					{
						schedulerHourRangeCollectionLayoutState._bounds = empty;
					}
					currentCellHeight = weekendCellHeight + weekendRemainder;
				}
				else
				{
					empty = new Rectangle(cellLeft, cellTop, cellWidth, cellHeight);
					schedulerHourRangeCollectionLayoutState = FindDateStyleOverride(item, 0, _dateStyleOverrides.Count - 1);
					if (schedulerHourRangeCollectionLayoutState == null)
					{
						schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(item, empty);
					}
					else
					{
						schedulerHourRangeCollectionLayoutState._bounds = empty;
					}
				}
				if (empty.Bottom + currentCellHeight > GetClientBottom())
				{
					schedulerHourRangeCollectionLayoutState._bounds.Height += extraHeight;
					if (item.DayOfWeek != DayOfWeek.Saturday)
					{
						cellLeft += cellWidth;
						cellTop = columnTop;
						columnIndex++;
					}
				}
				else
				{
					cellTop += currentCellHeight;
				}
				if (item < _minDate || item > _maxDate)
				{
					schedulerHourRangeCollectionLayoutState.SetIsDateInRange(false);
				}
				else
				{
					schedulerHourRangeCollectionLayoutState.SetIsDateInRange(true);
				}
				if (IsMultiOwnerActive())
				{
					schedulerHourRangeCollectionLayoutState.SetOwner(CurrentOwner);
				}
				_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutState);
			}
			CreateWeekDayHeaders(graphics);
			LayoutWeekViewAppointments();
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
			_isFullLayoutStale = false;
		}

		private void RenderDayView(Graphics graphics)
		{
			bool ownsGraphics = false;
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			_weekHeaderLayouts = null;
			int slotsPerHour = (int)_interval;
			int slotsPerDay = slotsPerHour * 24;
			int visibleSlotCount = slotsPerHour * (DayEndHour - DayStartHour);
			CreateTimeNavigatorFont();
			UpdateRowHeight();
			bool haveDatesChanged = false;
			_columnGap = 5;
			if (_dates.Count == 0)
			{
				Dates.AddWithoutNotify(_date.Date);
				haveDatesChanged = true;
			}
			for (int i = 0; i < _dates.Count; i++)
			{
				DateTime dateTime = _dates[i];
				if (dateTime.Date < _minDate.Date || dateTime.Date > _maxDate)
				{
					_dates.RemoveAtWithoutNotify(i);
					haveDatesChanged = true;
					i--;
				}
			}
			if (_dates.Count == 0)
			{
				Dates.AddWithoutNotify(_date.Date);
				haveDatesChanged = true;
			}
			if (!_dates.Contains(_date))
			{
				_date = _dates[0];
			}
			if (_isWorkWeekView && _dates.Count == 1)
			{
				SetWorkWeekDates(_dates[0]);
				haveDatesChanged = false;
			}
			if (haveDatesChanged)
			{
				_isUpdatingDates = true;
				_isHourRangeCacheStale = true;
				OnDatesChanged(EventArgs.Empty);
				_isUpdatingDates = false;
				EnsureSelectionInRange();
			}
			ArrayList arrayList = null;
			if (!_isHourRangeCacheStale)
			{
				arrayList = new ArrayList();
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					arrayList.Add(item);
				}
			}
			_dayLayouts = new SchedulerHourRangeLayoutCollection();
			if (IsMultiOwnerActive())
			{
				foreach (DateTime ownerDate in _dates)
				{
					foreach (SchedulerAppointmentOwner owner in _visibleOwners)
					{
						SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = new SchedulerHourRangeCollectionLayoutState(ownerDate, owner);
						if (_isHourRangeCacheStale)
						{
							SchedulerHourRangeCollection hourRanges = GetHourRangesForOwner(new DateRange(ownerDate, ownerDate.AddDays(1.0)), owner);
							QueryHourRangeEventArgs e = new QueryHourRangeEventArgs(hourRanges, ownerDate, owner);
							OnQueryHourRange(e);
							schedulerHourRangeCollectionLayoutState.SetSchedulerHourRangeCollection(e.HourRanges);
						}
						else
						{
							SchedulerHourRangeCollection schedulerHourRangeCollection = FindCachedHourRanges(arrayList, ownerDate, owner);
							schedulerHourRangeCollectionLayoutState.SetSchedulerHourRangeCollection(schedulerHourRangeCollection);
						}
						_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutState);
					}
				}
			}
			else
			{
				foreach (DateTime singleDate in _dates)
				{
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = new SchedulerHourRangeCollectionLayoutState(singleDate);
					if (_isHourRangeCacheStale)
					{
						SchedulerHourRangeCollection singleDayHourRanges = GetHourRangesForOwner(new DateRange(singleDate, singleDate.AddDays(1.0)), null);
						QueryHourRangeEventArgs singleDayQueryArgs = new QueryHourRangeEventArgs(singleDayHourRanges, singleDate, null);
						OnQueryHourRange(singleDayQueryArgs);
						schedulerHourRangeCollectionLayoutStateArgument.SetSchedulerHourRangeCollection(singleDayQueryArgs.HourRanges);
					}
					else
					{
						SchedulerHourRangeCollection cachedHourRanges = FindCachedHourRanges(arrayList, singleDate, null);
						schedulerHourRangeCollectionLayoutStateArgument.SetSchedulerHourRangeCollection(cachedHourRanges);
					}
					_dayLayouts.AddLayout(schedulerHourRangeCollectionLayoutStateArgument);
				}
			}
			arrayList = null;
			_isHourRangeCacheStale = false;
			if (_firstVisibleSlot != null)
			{
				_firstVisibleSlotIndex = _firstVisibleSlot.GetIndex();
				if (new TimeSpan(0, _minuteInterval * _firstVisibleSlotIndex, 0).Hours < DayStartHour)
				{
					_firstVisibleSlotIndex = DayStartHour * 60 / _minuteInterval;
				}
			}
			else if (_firstVisibleSlotIndex < 0)
			{
				_firstVisibleSlotIndex = (int)_firstVisibleTime.TotalMinutes / _minuteInterval;
			}
			int count = _dayLayouts.Count;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			_ = _firstVisibleSlotTime.Minutes;
			int amDesignatorWidth = 0;
			int timeLabelWidth = 0;
			string text = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
			if (text == "")
			{
				text = "00";
			}
			amDesignatorWidth = (int)textLayoutRenderer.RenderContent(graphics, text, DayFont, 0).Width;
			if (UseCompatibleTextRendering)
			{
				amDesignatorWidth += 2;
			}
			timeLabelWidth = ((_interval != Interval.SixtyMinutes) ? ((int)textLayoutRenderer.RenderContent(graphics, "12", _baseFont, 0).Width + amDesignatorWidth) : ((int)textLayoutRenderer.RenderContent(graphics, "12:", _baseFont, 0).Width + amDesignatorWidth));
			textLayoutRenderer.ResetFormat();
			_dayAreaLeft = timeLabelWidth + 5 + _columnGap + base.ClientRectangle.Left;
			_headerLayouts.Clear();
			int columnWidth = 0;
			columnWidth = ((_dayColumns >= count || _dayColumns <= 0) ? ((GetClientRight() - _dayAreaLeft - (count - 1) * _columnGap) / count) : ((GetClientRight() - _dayAreaLeft - (_dayColumns - 1) * _columnGap) / _dayColumns));
			if (_allDayAreaControl == null)
			{
				Date.ToLongDateString();
				_allDayAreaControl = new SchedulerAllDayArea(this, _dates, _actualRowHeight, _dayAreaLeft - _columnGap - base.ClientRectangle.Left - _firstVisibleColumnIndex * (columnWidth + _columnGap), _workWeek, _firstDayOfWeek);
				_allDayAreaControl.SetLeftPanelWidth(_dayAreaLeft - _columnGap - base.ClientRectangle.Left - _firstVisibleColumnIndex * (columnWidth + _columnGap));
				_allDayAreaControl._isMultiOwner = IsMultiOwnerActive();
				_allDayAreaControl.Font = DayFont;
				_allDayAreaControl._columnWidthWithGap = columnWidth + _columnGap;
				_allDayAreaControl.BringToFront();
				_allDayAreaControl.Bounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, GetHeaderControlWidth(), _dayAreaTop);
				base.Controls.Add(_allDayAreaControl);
				_allDayAreaControl._columnCount = count;
				if (LayoutAllDayArea(graphics))
				{
					ToggleControlEventHandlers(_allDayAreaControl, false);
					if (ownsGraphics)
					{
						graphics.Dispose();
					}
					return;
				}
				if (_showAllDayArea)
				{
					_dayAreaTop = CalculateAllDayAreaHeight() + 2;
				}
				else
				{
					_dayAreaTop = _defaultRowHeight;
					if (IsMultiOwnerActive() && _showOwnersHeader)
					{
						_dayAreaTop += _defaultRowHeight;
					}
				}
				ToggleControlEventHandlers(_allDayAreaControl, false);
			}
			else
			{
				_allDayAreaControl.Visible = false;
				_allDayAreaControl.SetLeftPanelWidth(_dayAreaLeft - _columnGap - base.ClientRectangle.Left - _firstVisibleColumnIndex * (columnWidth + _columnGap));
				_allDayAreaControl._isMultiOwner = IsMultiOwnerActive();
				_allDayAreaControl.Bounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, GetHeaderControlWidth(), _dayAreaTop);
				_allDayAreaControl._columnWidthWithGap = columnWidth + _columnGap;
				_allDayAreaControl._columnCount = count;
				if (LayoutAllDayArea(graphics))
				{
					if (ownsGraphics)
					{
						graphics.Dispose();
					}
					return;
				}
				_dayAreaTop = CalculateAllDayAreaHeight() + 2;
				_allDayAreaControl._rowHeight = _actualRowHeight;
				_allDayAreaControl.SetDateCollection(_dates);
				_allDayAreaControl._isLayoutStale = true;
			}
			if (_showAllDayArea)
			{
				_dayAreaTop = CalculateAllDayAreaHeight() + 2;
				_allDayAreaControl._rowHeight = _actualRowHeight;
			}
			else
			{
				_dayAreaTop = _defaultRowHeight;
				if (IsMultiOwnerActive() && _showOwnersHeader)
				{
					_dayAreaTop += _defaultRowHeight;
				}
				_allDayAreaControl._rowHeight = _defaultRowHeight;
			}
			_allDayAreaControl.SetLeftPanelWidth(_dayAreaLeft - _columnGap - base.ClientRectangle.Left - _firstVisibleColumnIndex * (columnWidth + _columnGap));
			double exactVisibleRows = ((double)base.ClientRectangle.Height - (double)_dayAreaTop) / (double)_actualRowHeight;
			int wholeVisibleRows = (base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight;
			exactVisibleRows = (exactVisibleRows - (double)wholeVisibleRows) * (double)_actualRowHeight;
			_leftoverHeight = (int)Math.Round(exactVisibleRows);
			if (_leftoverHeight > 0)
			{
				if (_showAllDayArea)
				{
					_dayAreaTop += _leftoverHeight;
				}
				else if ((base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight > 0)
				{
					_actualRowHeight += _leftoverHeight / ((base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight);
				}
			}
			int visibleRowCount = (base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight;
			if (visibleRowCount > visibleSlotCount)
			{
				_actualRowHeight = (base.ClientRectangle.Height - _dayAreaTop) / visibleSlotCount;
				if (_showAllDayArea)
				{
					_dayAreaTop += base.ClientRectangle.Height - _dayAreaTop - visibleSlotCount * _actualRowHeight;
				}
				visibleRowCount = visibleSlotCount;
			}
			int endSlotIndex = _firstVisibleSlotIndex + visibleRowCount;
			if (endSlotIndex > slotsPerDay)
			{
				_firstVisibleSlotIndex -= endSlotIndex - slotsPerDay;
			}
			if (_firstVisibleSlotIndex + visibleRowCount > DayEndHour * slotsPerHour)
			{
				_firstVisibleSlotIndex -= _firstVisibleSlotIndex + visibleRowCount - DayEndHour * slotsPerHour;
				if (_firstVisibleSlotIndex < DayStartHour * slotsPerHour)
				{
					_firstVisibleSlotIndex = DayStartHour * slotsPerHour;
				}
			}
			if (_firstVisibleSlotIndex < 0)
			{
				_firstVisibleSlotIndex = 0;
			}
			_firstVisibleSlotTime = new TimeSpan(0, _firstVisibleSlotIndex * _minuteInterval, 0);
			if (_firstVisibleSlot != null && _firstVisibleSlot.GetIndex() != _firstVisibleSlotIndex)
			{
				_firstVisibleSlot.SetIndex(_firstVisibleSlotIndex);
			}
			if (_showAllDayArea && base.ClientRectangle.Height - (slotsPerDay - _firstVisibleSlotIndex) * _actualRowHeight - _dayAreaTop > 0)
			{
				_dayAreaTop = base.ClientRectangle.Height - (slotsPerDay - _firstVisibleSlotIndex) * _actualRowHeight;
			}
			if (!ShouldShowAllDayAreaScrollBar())
			{
				base.SetHeaderHeight(0);
			}
			else
			{
				base.SetHeaderHeight(_dayAreaTop);
			}
			_isFullLayoutStale = false;
			if ((base.ClientRectangle.Height - _dayAreaTop) / _actualRowHeight >= visibleSlotCount)
			{
				base.SetVerticalScrollRequired(false);
			}
			else
			{
				base.SetVerticalScrollRequired(true);
			}
			columnWidth = ((_dayColumns >= count || _dayColumns <= 0) ? ((GetClientRight() - _dayAreaLeft - (count - 1) * _columnGap) / count) : ((GetClientRight() - _dayAreaLeft - (_dayColumns - 1) * _columnGap) / _dayColumns));
			int gridTop = _dayAreaTop + base.ClientRectangle.Top;
			_gridBounds = new Rectangle(_dayAreaLeft - _columnGap, _dayAreaTop, GetClientRight() - (_dayAreaLeft - _columnGap), base.ClientRectangle.Height - _dayAreaTop);
			int columnLeft = _dayAreaLeft - _firstVisibleColumnIndex * (columnWidth + _columnGap);
			int extraColumnWidth = 0;
			int baseColumnWidth = columnWidth;
			foreach (SchedulerHourRangeCollectionLayoutState dayColumn in _dayLayouts)
			{
				dayColumn.GetTimeSlots().Clear();
				extraColumnWidth = ((GetClientRight() - columnLeft - columnWidth < count && GetClientRight() - columnLeft - columnWidth > 0) ? ((count <= columnWidth || dayColumn.GetDayIndex() >= _dayLayouts.Count - 1) ? (GetClientRight() - columnLeft - columnWidth) : 0) : 0);
				dayColumn._bounds = new Rectangle(columnLeft, _dayAreaTop + base.ClientRectangle.Top, baseColumnWidth + extraColumnWidth, base.ClientRectangle.Height - _dayAreaTop);
				gridTop = _dayAreaTop + base.ClientRectangle.Top;
				DateTime slotTime = dayColumn.GetDate().Add(_firstVisibleSlotTime.Subtract(new TimeSpan(0, _minuteInterval, 0)));
				for (int slotIndex = _firstVisibleSlotIndex - 1; slotIndex >= 0; slotIndex--)
				{
					gridTop -= _actualRowHeight;
					Rectangle bounds = new Rectangle(columnLeft, gridTop, baseColumnWidth + extraColumnWidth, _actualRowHeight);
					SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = new SchedulerAppointmentOwnerLayoutState(slotIndex, bounds);
					schedulerAppointmentOwnerLayoutState.UpdateDate(slotTime);
					schedulerAppointmentOwnerLayoutState.SetDayOfWeek(dayColumn.GetDate().DayOfWeek);
					schedulerAppointmentOwnerLayoutState.SetWorkingHour(IsWorkingTimeSlot(dayColumn.GetSchedulerHourRangeCollection(), slotTime, new TimeSpan(0, _minuteInterval, 0)));
					schedulerAppointmentOwnerLayoutState.SetIndex(slotIndex);
					dayColumn.GetTimeSlots().Add(schedulerAppointmentOwnerLayoutState);
					schedulerAppointmentOwnerLayoutState.SetSchedulerHourRangeCollectionLayoutState(dayColumn);
					slotTime = slotTime.Subtract(new TimeSpan(0, _minuteInterval, 0));
				}
				dayColumn.GetTimeSlots().Sort();
				gridTop = _dayAreaTop + base.ClientRectangle.Top;
				slotTime = dayColumn.GetDate().Add(_firstVisibleSlotTime);
				for (int j = _firstVisibleSlotIndex; j < slotsPerDay; j++)
				{
					Rectangle slotBounds = new Rectangle(columnLeft, gridTop, baseColumnWidth + extraColumnWidth, _actualRowHeight);
					SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutStateArgument = new SchedulerAppointmentOwnerLayoutState(j, slotBounds);
					schedulerAppointmentOwnerLayoutStateArgument.UpdateDate(slotTime);
					gridTop += _actualRowHeight;
					schedulerAppointmentOwnerLayoutStateArgument.SetDayOfWeek(dayColumn.GetDate().DayOfWeek);
					schedulerAppointmentOwnerLayoutStateArgument.SetWorkingHour(IsWorkingTimeSlot(dayColumn.GetSchedulerHourRangeCollection(), slotTime, new TimeSpan(0, _minuteInterval, 0)));
					dayColumn.GetTimeSlots().Add(schedulerAppointmentOwnerLayoutStateArgument);
					schedulerAppointmentOwnerLayoutStateArgument.SetSchedulerHourRangeCollectionLayoutState(dayColumn);
					slotTime = slotTime.AddMinutes(_minuteInterval);
				}
				columnLeft += baseColumnWidth + _columnGap + extraColumnWidth;
			}
			if (_firstVisibleSlot == null)
			{
				_firstVisibleSlot = _dayLayouts.GetLayoutAt(0).GetTimeSlots().GetLayoutAt(_firstVisibleSlotIndex);
			}
			else
			{
				_firstVisibleSlot = _dayLayouts.GetLayoutAt(0).GetTimeSlots().GetLayoutAt(_firstVisibleSlot.GetIndex());
			}
			int scrollPosition = _firstVisibleSlotIndex - GetDayStartSlotIndex();
			GetVerticalScrollBarControl().UpdateScrollValue(scrollPosition);
			UpdateVerticalScrollBar(0, visibleSlotCount - 1, visibleRowCount, scrollPosition);
			_allDayAreaControl.Size = new Size(GetHeaderControlWidth(), _dayAreaTop);
			_allDayAreaControl.Visible = true;
			LayoutDayViewAppointments();
			_firstVisibleSlotIndex = _firstVisibleSlot.GetIndex();
			if (_firstVisibleSlotIndex + GetVerticalScrollBarControl().GetLargeChange() >= slotsPerDay)
			{
				int emptyBottomSpace = GetClientBottom() - _dayLayouts.GetLayoutAt(0).GetTimeSlots().GetLayoutAt(slotsPerDay - 1)
					._bounds.Bottom;
				int emptyRowCount = emptyBottomSpace / _actualRowHeight;
				if (emptyRowCount > 0)
				{
					StyledScrollBar scrollBar = GetVerticalScrollBarControl();
					scrollBar.UpdateScrollValue(scrollBar.GetScrollValue() - -emptyRowCount);
				}
			}
			if (ownsGraphics)
			{
				graphics.Dispose();
			}
			try
			{
				if (columnWidth <= 1)
				{
					UpdateHorizontalScrollBar(0, count - 1, 1, _firstVisibleColumnIndex);
					if (count > 1)
					{
						SetHorizontalScrollRequired(true);
					}
					else
					{
						SetHorizontalScrollRequired(false);
					}
				}
				else
				{
					UpdateHorizontalScrollBar(0, count - 1, (GetClientRight() - _dayAreaLeft) / columnWidth, _firstVisibleColumnIndex);
					if ((GetClientRight() - _dayAreaLeft) / (columnWidth - 1) >= count)
					{
						SetHorizontalScrollRequired(false);
					}
					else
					{
						SetHorizontalScrollRequired(true);
					}
				}
			}
			catch
			{
				SetHorizontalScrollRequired(true);
			}
		}

		private void DrawDayViewCells(Graphics graphics, Rectangle rectangle)
		{
			_hourRangeStyleCache = new Dictionary<(SchedulerHourRange, bool, SchedulerFormatStyle), SchedulerFormatStyle>();
			try
			{
				DrawDayViewCellsWithStyleCache(graphics, rectangle);
			}
			finally
			{
				_hourRangeStyleCache = null;
			}
		}

		private void DrawDayViewCellsWithStyleCache(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = GetGridLinePen();
			Pen slotLinePen = GetLightGridLinePen();
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_hourBackgroundStyle);
			SchedulerFormatStyle alternatingHourStyle = schedulerFormatStyle.MergeWith(_alternatingHourBackgroundStyle);
			SchedulerFormatStyle workingHourStyle = schedulerFormatStyle.MergeWith(_workingHourBackgroundStyle);
			SchedulerFormatStyle alternatingWorkingHourStyle = workingHourStyle.MergeWith(_alternatingWorkingHourBackgroundStyle);
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				int rowTop = _dayAreaTop + base.ClientRectangle.Top;
				for (int i = _firstVisibleSlot.GetIndex(); i < item.GetTimeSlots().Count; i++)
				{
					SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = item.GetTimeSlots().GetLayoutAt(i);
					Rectangle bounds = schedulerAppointmentOwnerLayoutState._bounds;
					if (Rectangle.Intersect(rectangle, bounds).IsEmpty)
					{
						continue;
					}
					if (!IsAreaDrawnByHandler(graphics, OwnerDrawnArea.DayViewRow, schedulerAppointmentOwnerLayoutState._bounds, schedulerAppointmentOwnerLayoutState))
					{
						DrawHourCell(graphics, schedulerAppointmentOwnerLayoutState, pen, slotLinePen, schedulerFormatStyle, alternatingHourStyle, workingHourStyle, alternatingWorkingHourStyle);
						rowTop += _actualRowHeight;
						if (rowTop >= base.ClientRectangle.Bottom)
						{
							break;
						}
					}
				}
				graphics.DrawLine(pen, item._bounds.Left, item._bounds.Top, item._bounds.Left, item._bounds.Bottom);
				graphics.DrawLine(pen, item._bounds.Right, item._bounds.Top, item._bounds.Right, item._bounds.Bottom);
			}
			alternatingWorkingHourStyle.DisposeResources();
			workingHourStyle.DisposeResources();
			alternatingHourStyle.DisposeResources();
			schedulerFormatStyle.DisposeResources();
		}

		private void DrawMonthView(Graphics graphics, Rectangle rectangleArgument)
		{
			try
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Near);
				SchedulerFormatStyle schedulerFormatStyle = null;
				if (GetVerticalScrollBarControl() != null && GetVerticalScrollBarControl().Visible)
				{
					Pen pen = GetGridLinePen();
					graphics.DrawLine(pen, base.ClientRectangle.Right - 1, base.ClientRectangle.Top, base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom - 1);
				}
				foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
				{
					Rectangle bounds = item._bounds;
					if (Rectangle.Intersect(rectangleArgument, bounds).IsEmpty)
					{
						continue;
					}
					if (IsAreaDrawnByHandler(graphics, OwnerDrawnArea.MonthViewDay, bounds, item))
					{
						continue;
					}
					schedulerFormatStyle = new SchedulerFormatStyle(_monthDaysFormatStyle);
					if (item.GetDate().Month % 2 == 0)
					{
						schedulerFormatStyle = schedulerFormatStyle.MergeWith(_alternatingMonthDaysFormatStyle);
					}
					if (item.GetSchedulerFormatStyle() != null)
					{
						schedulerFormatStyle = schedulerFormatStyle.MergeWith(item.GetSchedulerFormatStyle());
					}
					Brush brush = schedulerFormatStyle.GetBackBrush(bounds);
					DrawBackground(graphics, item, brush, bounds);
					Rectangle rectangle = bounds;
					if (item._schedulerAppointmentOwnerLayoutState != null)
					{
						DrawDateHeader(graphics, item._schedulerAppointmentOwnerLayoutState, item._schedulerAppointmentOwnerLayoutState.IsHovered(), item._schedulerAppointmentOwnerLayoutState._bounds, false);
						textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
						textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
						rectangle = item._schedulerAppointmentOwnerLayoutState._bounds;
					}
					if (IsDateSelected(item.GetDate()) && item._schedulerAppointmentOwnerLayoutState == null)
					{
						Rectangle captionRect = rectangle;
						captionRect.Height = _actualRowHeight;
						captionRect.Inflate(-1, -1);
						captionRect.Width--;
						DrawDayCellHeader(graphics, captionRect, item, item.GetCaption(), textLayoutRenderer);
					}
					else
					{
						rectangle.Inflate(-1, -1);
						rectangle.Width--;
						if (item.GetIsDateInRange())
						{
							textLayoutRenderer.DrawText(graphics, item.GetCaption(), rectangle, schedulerFormatStyle.ResolveFont(MonthFont), schedulerFormatStyle.ResolveForeColor(GetThemedForeColor()));
						}
						else
						{
							brush = MxThemeManager.ResolveDefault(DeviceDpi).Brush(ThemeRole.TextDisabled);
							textLayoutRenderer.DrawTextWithBrush(graphics, item.GetCaption(), rectangle, schedulerFormatStyle.ResolveFont(MonthFont), brush);
							brush = null;
						}
						bounds = item._bounds;
						bounds.X++;
						bounds.Y++;
					}
					schedulerFormatStyle.DisposeResources();
				}
				if (_weekHeaderLayouts != null)
				{
					foreach (SchedulerAppointmentOwnerLayoutState weekHeader in _weekHeaderLayouts)
					{
						if (!IsAreaDrawnByHandler(graphics, OwnerDrawnArea.WeekHeader, weekHeader._bounds, weekHeader))
						{
							DrawVerticalHeader(graphics, weekHeader, true);
						}
					}
				}
				if (IsMultiOwnerActive() && _showOwnersHeader)
				{
					DrawOwnerHeader(graphics);
				}
				else
				{
					_ownerHeaderBounds = Rectangle.Empty;
				}
			}
			catch
			{
			}
		}

		private void DrawWeekView(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = GetGridLinePen();
			if (GetVerticalScrollBarControl() != null && GetVerticalScrollBarControl().Visible)
			{
				Pen scrollBarEdgePen = GetGridLinePen();
				graphics.DrawLine(scrollBarEdgePen, base.ClientRectangle.Right - 1, base.ClientRectangle.Top, base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom - 1);
			}
			SchedulerFormatStyle schedulerFormatStyle = null;
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				Rectangle bounds = item._bounds;
				if (!Rectangle.Intersect(rectangle, bounds).IsEmpty)
				{
					bounds.Y += _actualRowHeight;
					bounds.Height -= _actualRowHeight;
					if (!IsAreaDrawnByHandler(graphics, OwnerDrawnArea.WeekViewDay, bounds, item))
					{
						DrawWeekDayBackground(graphics, item, bounds);
					}
				}
			}
			DrawDateHeaders(graphics);
			if (IsMultiOwnerActive() && _showOwnersHeader)
			{
				DrawOwnerHeader(graphics);
			}
			else
			{
				_ownerHeaderBounds = Rectangle.Empty;
			}
			if (_headerAppearance == Appearance.FlatBorderless)
			{
				int centerX = base.ClientRectangle.Width / 2;
				graphics.DrawLine(pen, centerX, base.ClientRectangle.Top, centerX, base.ClientRectangle.Bottom);
			}
			schedulerFormatStyle?.DisposeResources();
		}
	}
}
