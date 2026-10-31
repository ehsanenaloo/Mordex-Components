using System;
using System.Collections;
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
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		public static string GetTimelineVersion()
		{
			string localPath = typeof(MxTimeline).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		void ITimelineWorkingHoursOwner.InvalidateWorkingTime()
		{
			Invalidate();
		}

		protected virtual void OnCalendarCloseUp(EventArgs e)
		{
			if (base.Events[_calendarCloseUpEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		private void SetLastVisibleDate(DateTime dateTime)
		{
			if (_lastVisibleDate != dateTime)
			{
				_lastVisibleDate = dateTime;
				_hasPendingLastDateChange = true;
			}
		}

		private void UpdateTimelineLayout()
		{
			_timelineLayout = _currentLayout;
			TimelineLayout timelineLayout = null;
			if (_currentLayout != null)
			{
				timelineLayout = _currentLayout.GetEffectiveLayout();
			}
			if (timelineLayout == null || timelineLayout.LayoutString == "")
			{
				DeserializeLayout(new LayoutReader(), (PersistenceMode)0);
				ApplyDataBinding(null, new BindingMemberInfo(""));
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(timelineLayout);
				PropertyValue propertyValue = mordexLayoutReader["TimelineLayoutData"];
				InvalidateRows();
				propertyValue?.DeserializeObject(this);
				ApplyDataBinding(_currentLayout.DataSource, new BindingMemberInfo(_currentLayout.DataMember));
				if (!_isInitializing)
				{
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		internal TimelineLayout CaptureLayout(PersistenceMode persistenceMode)
		{
			TimelineLayout timelineLayout = new TimelineLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			SerializeLayout(mordexLayoutWriter, timelineLayout.GetPersistenceMode());
			LayoutWriter.StoreInLayout("TimelineLayoutData", mordexLayoutWriter, timelineLayout);
			timelineLayout.DataMember = DataMember;
			timelineLayout.DataSource = DataSource;
			return timelineLayout;
		}

		protected virtual void OnDrawTimelineArea(DrawTimelineAreaEventArgs e)
		{
			if (base.Events[_drawTimelineAreaEventKey] is DrawTimelineEventHandler drawTimelineEventHandler)
			{
				drawTimelineEventHandler(this, e);
			}
		}

		internal void RaiseDrawTimelineArea(DrawTimelineAreaEventArgs eventArgs)
		{
			if ((_ownerDrawnArea & eventArgs.OwnerDrawnArea) != OwnerDrawnArea.None)
			{
				OnDrawTimelineArea(eventArgs);
			}
		}

		protected virtual void OnLastDateChanged(EventArgs e)
		{
			if (base.Events[_lastDateChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnFirstDateChanged(EventArgs e)
		{
			if (base.Events[_firstDateChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal double GetIntervalDistance(DateTime dateTime, DateTime dateTimeArgument)
		{
			double intervalOffset = 0.0;
			switch (BottomTier.Interval)
			{
			case TimelineInterval.Year:
			{
				intervalOffset = (double)(dateTimeArgument.Year - dateTime.Year) / (double)BottomTier.Count;
				double dayOffset = (double)dateTimeArgument.DayOfYear - (double)dateTime.DayOfYear;
				if (DateTime.IsLeapYear(dateTimeArgument.Year))
				{
					return intervalOffset + dayOffset / 366.0 / (double)BottomTier.Count;
				}
				return intervalOffset + dayOffset / 365.0 / (double)BottomTier.Count;
			}
			case TimelineInterval.Month:
			{
				int day = dateTime.Day;
				dateTime = new DateTime(dateTime.Year, dateTime.Month, 1);
				intervalOffset = dateTimeArgument.Year - dateTime.Year;
				intervalOffset *= 12.0 / (double)_bottomTier.Count;
				intervalOffset += (double)(dateTimeArgument.Month - dateTime.Month) / (double)_bottomTier.Count;
				int daysInMonth = DateTime.DaysInMonth(dateTimeArgument.Year, dateTimeArgument.Month);
				int dayDifference = dateTimeArgument.Day - day;
				return intervalOffset + (double)dayDifference / (double)daysInMonth / (double)_bottomTier.Count;
			}
			case TimelineInterval.Week:
				intervalOffset = dateTimeArgument.Date.Subtract(dateTime.Date).TotalDays / 7.0 * (86400.0 / ((double)_intervalSeconds / 7.0));
				return intervalOffset + (dateTimeArgument.TimeOfDay.TotalDays - dateTime.TimeOfDay.TotalDays) / 7.0 * (86400.0 / ((double)_intervalSeconds / 7.0));
			default:
				intervalOffset = dateTimeArgument.Date.Subtract(dateTime.Date).TotalDays * (86400.0 / (double)_intervalSeconds);
				return intervalOffset + (dateTimeArgument.TimeOfDay.TotalDays - dateTime.TimeOfDay.TotalDays) * (86400.0 / (double)_intervalSeconds);
			}
		}

		private void UpdateTimescaleMetrics()
		{
			if (_timelineTimescaleTiers == TimescaleTiers.ThreeTiers)
			{
				TopTier.SetHeight((int)_topTier.Font.GetHeight() + 14);
				_upperTierCharWidth = TextRenderer.MeasureText("W", _topTier.Font, Size.Empty).Width;
			}
			else
			{
				TopTier.SetHeight(0);
			}
			if (_timelineTimescaleTiers != TimescaleTiers.OneTier)
			{
				MiddleTier.SetHeight((int)_middleTier.Font.GetHeight() + 14);
				if (TimescaleTiers == TimescaleTiers.TwoTiers)
				{
					_upperTierCharWidth = TextRenderer.MeasureText("W", MiddleTier.Font, Size.Empty).Width;
				}
			}
			else
			{
				MiddleTier.SetHeight(0);
			}
			BottomTier.SetHeight((int)_bottomTier.Font.GetHeight() + 14);
			_bottomTierCharWidth = TextRenderer.MeasureText("W", BottomTier.Font, Size.Empty).Width;
			base.SetHeaderHeight(TopTier.GetHeight() + MiddleTier.GetHeight() + BottomTier.GetHeight());
			_isTimescaleDirty = false;
			if (!BottomTier.IsLayoutDirty())
			{
				return;
			}
			if (BottomTier.Interval == TimelineInterval.Day)
			{
				_selectedRange.Start = _selectedRange.Start.Date;
				if (_selectedRange.End.TimeOfDay != TimeSpan.Zero)
				{
					_selectedRange.End = _selectedRange.End.Date.AddDays(1.0);
				}
			}
			DisposeResources();
			if (BottomTier.Interval == TimelineInterval.Year || BottomTier.Interval == TimelineInterval.Month)
			{
				FirstDate = _firstDate.Date;
			}
			_intervalSeconds = GetFixedIntervalSeconds(BottomTier.Interval) * BottomTier.Count;
			_selectionAnchorDate = _selectedRange.End;
			InvalidateRows();
			InvalidateLayout();
			BottomTier.SetLayoutDirty(false);
		}

		private void LayoutTiers()
		{
			int tierTop = base.ClientRectangle.Top;
			int left = base.ClientRectangle.Left;
			if (_timelineTimescaleTiers == TimescaleTiers.ThreeTiers)
			{
				TopTier.SetBounds(new Rectangle(left, tierTop, base.ClientRectangle.Width, TopTier.GetHeight()));
				tierTop += TopTier.GetHeight() + 1;
			}
			else
			{
				TopTier.SetBounds(Rectangle.Empty);
			}
			if (_timelineTimescaleTiers != TimescaleTiers.OneTier)
			{
				MiddleTier.SetBounds(new Rectangle(left, tierTop, base.ClientRectangle.Width, MiddleTier.GetHeight()));
				tierTop += _middleTier.GetHeight() + 1;
			}
			else
			{
				MiddleTier.SetBounds(Rectangle.Empty);
			}
			BottomTier.SetBounds(new Rectangle(left, tierTop, base.ClientRectangle.Width, BottomTier.GetHeight()));
		}

		internal TimelineFormatStyle GetFormatStyleForRange(DateRange dateRange, TimelineFormatStyle timelineFormatStyleArgument, TimelineFormatStyle workingTimeStyle)
		{
			TimelineFormatStyle timelineFormatStyle = null;
			TimelineHourRange timelineHourRange = WorkingHourSchema.FindHourRange(dateRange);
			timelineFormatStyle = ((!IsWorkingSlotInRange(timelineHourRange, dateRange.Start, dateRange.End.Subtract(dateRange.Start))) ? timelineFormatStyleArgument : workingTimeStyle);
			if (timelineHourRange != null)
			{
				timelineFormatStyle = timelineFormatStyle.MergeWith(timelineHourRange.FormatStyle);
			}
			return timelineFormatStyle;
		}

		public void SuspendTimescaleValidation()
		{
			_isTimescaleValidationEnabled = false;
			StoreTimescaleTiers();
		}

		public void ResumeTimescaleValidation()
		{
			_isTimescaleValidationEnabled = true;
			if (!AreTimescaleTiersValid(_timelineTimescaleTiers))
			{
				RestoreTimescaleTiers();
				throw new ArgumentException("An upper tier has less time per tick than one of the tiers below it.");
			}
			InvalidateTimescale();
		}

		private void StoreTimescaleTiers()
		{
			_timescaleTiers = _timelineTimescaleTiers;
			_topTier.UpdateTimescale();
			_middleTier.UpdateTimescale();
			_bottomTier.UpdateTimescale();
		}

		private void RestoreTimescaleTiers()
		{
			_timelineTimescaleTiers = _timescaleTiers;
			_topTier.RestoreSavedSettings();
			_middleTier.RestoreSavedSettings();
			_bottomTier.RestoreSavedSettings();
		}

		internal void InvalidateTimescale()
		{
			_isTimescaleDirty = true;
			InvalidateRowsAndLayout();
		}

		internal bool AreTimescaleTiersValid(TimescaleTiers timescaleTiers)
		{
			if (_isTimescaleValidationEnabled && !_isInitializing && TimescaleTiers != TimescaleTiers.OneTier)
			{
				if (!IsTierOrderValid(BottomTier, MiddleTier))
				{
					return false;
				}
				if (TimescaleTiers == TimescaleTiers.ThreeTiers)
				{
					if (IsTierOrderValid(MiddleTier, TopTier))
					{
						return true;
					}
					return false;
				}
			}
			return true;
		}

		internal bool IsTierOrderValid(Timescale timescale, Timescale timescaleArgument)
		{
			long tierSeconds = 0;
			long upperTierSeconds = 0;
			switch (timescale.Interval)
			{
			case TimelineInterval.Second:
				tierSeconds = timescale.Count;
				break;
			case TimelineInterval.Minute:
				tierSeconds = 60L * timescale.Count;
				break;
			case TimelineInterval.Hour:
				tierSeconds = 3600L * timescale.Count;
				break;
			case TimelineInterval.Day:
				tierSeconds = 86400L * timescale.Count;
				break;
			case TimelineInterval.Week:
				tierSeconds = 604800L * timescale.Count;
				break;
			case TimelineInterval.Month:
				tierSeconds = 2419200L * timescale.Count;
				break;
			case TimelineInterval.Year:
				tierSeconds = 31536000L * timescale.Count;
				break;
			}
			switch (timescaleArgument.Interval)
			{
			case TimelineInterval.Second:
				upperTierSeconds = timescaleArgument.Count;
				break;
			case TimelineInterval.Minute:
				upperTierSeconds = 60L * timescaleArgument.Count;
				break;
			case TimelineInterval.Hour:
				upperTierSeconds = 3600L * timescaleArgument.Count;
				break;
			case TimelineInterval.Day:
				upperTierSeconds = 86400L * timescaleArgument.Count;
				break;
			case TimelineInterval.Week:
				upperTierSeconds = 604800L * timescaleArgument.Count;
				break;
			case TimelineInterval.Month:
				upperTierSeconds = 2419200L * timescaleArgument.Count;
				break;
			case TimelineInterval.Year:
				upperTierSeconds = 31536000L * timescaleArgument.Count;
				break;
			}
			if (tierSeconds > upperTierSeconds)
			{
				return false;
			}
			return true;
		}

		internal DateTime AlignToInterval(DateTime time, TimelineInterval timelineInterval, int intervalCount)
		{
			switch (timelineInterval)
			{
			case TimelineInterval.Second:
				if (intervalCount != 1)
				{
					int secondRemainder = time.Second % intervalCount;
					if (secondRemainder > 0)
					{
						time = time.AddSeconds(-secondRemainder);
					}
				}
				break;
			case TimelineInterval.Day:
				time = time.Date;
				if (intervalCount != 1)
				{
					int dayRemainder = (int)(time.Ticks / 864000000000L) % intervalCount;
					if (dayRemainder > 0)
					{
						time = time.AddDays(-dayRemainder);
					}
				}
				break;
			case TimelineInterval.Hour:
				time = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
				if (intervalCount != 1)
				{
					int hourRemainder = (int)(time.Ticks / 36000000000L) % intervalCount;
					if (hourRemainder > 0)
					{
						time = time.AddHours(-hourRemainder);
					}
				}
				break;
			case TimelineInterval.Minute:
				time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
				if (intervalCount != 1)
				{
					int minuteRemainder = (int)(time.Ticks / 600000000) % intervalCount;
					if (minuteRemainder > 0)
					{
						time = time.AddMinutes(-minuteRemainder);
					}
				}
				break;
			case TimelineInterval.Year:
				time = new DateTime(time.Year, 1, 1, 0, 0, 0);
				if (intervalCount != 1)
				{
					int yearRemainder = time.Year % intervalCount;
					if (yearRemainder != 0)
					{
						time = time.AddYears(-yearRemainder);
					}
				}
				break;
			case TimelineInterval.Month:
				time = new DateTime(time.Year, time.Month, 1, 0, 0, 0);
				if (intervalCount != 1)
				{
					int monthRemainder = (time.Year * 12 + time.Month) % intervalCount;
					if (monthRemainder != 0)
					{
						time = time.AddMonths(-monthRemainder);
					}
				}
				break;
			case TimelineInterval.Week:
				time = GetWeekStart(time);
				if (intervalCount != 1)
				{
					int weekOfYear = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(time, WeekRule, TimelineUtilities.ConvertToSystemDayOfWeek(FirstDayOfWeek));
					int weekRemainder = weekOfYear % intervalCount;
					if (weekRemainder > 0)
					{
						time = time.AddDays(-(weekRemainder * 7));
					}
				}
				break;
			}
			return time;
		}

		private void UpdateTimeline()
		{
			if (_isAutoScrollVertical)
			{
				if (_isAutoScrollForward)
				{
					GetVerticalScrollBarControl().UpdateScrollValue(GetVerticalScrollBarControl().GetScrollValue() + 1);
				}
				else
				{
					GetVerticalScrollBarControl().UpdateScrollValue(GetVerticalScrollBarControl().GetScrollValue() - 1);
				}
			}
			else if (_isAutoScrollForward)
			{
				GetHorizontalScrollBarControl().UpdateScrollValue(GetHorizontalScrollBarControl().GetScrollValue() + 1);
				OnHorizontalScroll(ScrollEventType.SmallIncrement);
			}
			else
			{
				GetHorizontalScrollBarControl().UpdateScrollValue(GetHorizontalScrollBarControl().GetScrollValue() - 1);
				OnHorizontalScroll(ScrollEventType.SmallDecrement);
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			Invalidate();
		}

		private bool IsInTierHeaderArea(int x, int integerArgument)
		{
			Rectangle rectangle = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, base.ClientRectangle.Width, 0);
			if (TimescaleTiers == TimescaleTiers.OneTier)
			{
				rectangle.Height = _bottomTier.GetHeight();
			}
			else
			{
				rectangle.Height = _middleTier.GetBounds().Bottom;
			}
			if (rectangle.Contains(x, integerArgument))
			{
				return true;
			}
			return false;
		}

		public DateTime GetDateTimeAt()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return GetDateTimeAt(position);
		}

		public DateTime GetDateTimeAt(Point pt)
		{
			if (!base.ClientRectangle.Contains(pt))
			{
				return DateTime.MinValue;
			}
			return GetDateTimeAt(pt.X, pt.Y);
		}

		public DateTime GetDateTimeAt(int x, int y)
		{
			if (!base.ClientRectangle.Contains(x, y))
			{
				return DateTime.MinValue;
			}
			return GetDateAtX(x);
		}

		public DateTime GetDateTimeAt(int x, int y, bool roundDate)
		{
			if (!base.ClientRectangle.Contains(x, y))
			{
				return DateTime.MinValue;
			}
			DateTime dateTime = GetDateAtX(x);
			if (roundDate)
			{
				dateTime = AlignToInterval(dateTime, BottomTier.Interval, BottomTier.Count);
			}
			return dateTime;
		}

		private DateTime GetDateAtX(int x)
		{
			float unitsFromLeft = x;
			unitsFromLeft -= (float)base.ClientRectangle.Left;
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				unitsFromLeft += 1f;
			}
			DateTime dateTime = _firstDate;
			unitsFromLeft /= (float)_intervalWidth;
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				dateTime = dateTime.AddYears((int)unitsFromLeft * BottomTier.Count);
				return dateTime.AddDays((unitsFromLeft - (float)(int)unitsFromLeft) * (float)BottomTier.Count * (float)GetDaysInYear(dateTime.Year));
			}
			if (BottomTier.Interval == TimelineInterval.Month)
			{
				dateTime = dateTime.AddMonths((int)unitsFromLeft * BottomTier.Count);
				float monthFraction = (unitsFromLeft - (float)(int)unitsFromLeft) * (float)BottomTier.Count;
				return dateTime.AddDays(monthFraction * (float)DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
			}
			return dateTime.AddSeconds((float)_intervalSeconds * unitsFromLeft);
		}

		private DateTime AddIntervalUnits(DateTime dateTime, float floatValue)
		{
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				dateTime = dateTime.AddYears((int)floatValue * BottomTier.Count);
				dateTime = dateTime.AddDays((floatValue - (float)(int)floatValue) * (float)BottomTier.Count * (float)GetDaysInYear(dateTime.Year));
			}
			else if (BottomTier.Interval == TimelineInterval.Month)
			{
				dateTime = dateTime.AddMonths((int)floatValue * BottomTier.Count);
				float monthFraction = (floatValue - (float)(int)floatValue) * (float)BottomTier.Count;
				dateTime = dateTime.AddDays(monthFraction * (float)DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
			}
			else
			{
				dateTime = dateTime.AddSeconds((float)_intervalSeconds * floatValue);
			}
			return dateTime;
		}

		private long GetBottomTierStepSeconds(DateTime dateTime, bool usePreviousPeriod)
		{
			if (BottomTier.Interval == TimelineInterval.Year || BottomTier.Interval == TimelineInterval.Month)
			{
				return GetCalendarStepSeconds(BottomTier.Interval, BottomTier.Count, dateTime, usePreviousPeriod);
			}
			return GetFixedIntervalSeconds(BottomTier.Interval) * BottomTier.Count;
		}

		// Length of the count years or months that start with the period of dateTime, or that end just
		// before it. The span is cut at the DateTime limits instead of throwing.
		private static long GetCalendarStepSeconds(TimelineInterval interval, int count, DateTime dateTime, bool usePreviousPeriod)
		{
			DateTime periodStart = interval == TimelineInterval.Year ? new DateTime(dateTime.Year, 1, 1) : new DateTime(dateTime.Year, dateTime.Month, 1);
			int monthsPerPeriod = interval == TimelineInterval.Year ? 12 : 1;
			long months = (long)monthsPerPeriod * count;
			DateTime start = usePreviousPeriod ? AddMonthsClamped(periodStart, -months) : periodStart;
			DateTime end = usePreviousPeriod ? periodStart : AddMonthsClamped(periodStart, months);
			return (long)(end - start).TotalSeconds;
		}

		private static DateTime AddMonthsClamped(DateTime dateTime, long months)
		{
			long monthIndex = (long)dateTime.Year * 12 + dateTime.Month - 1 + months;
			if (monthIndex < 12)
			{
				return DateTime.MinValue;
			}
			if (monthIndex >= 10000L * 12)
			{
				return DateTime.MaxValue;
			}
			return new DateTime((int)(monthIndex / 12), (int)(monthIndex % 12) + 1, 1);
		}

		// Timeline dates are Gregorian, so the culture calendar must not be used for day counts.
		private static int GetDaysInYear(int year)
		{
			return DateTime.IsLeapYear(year) ? 366 : 365;
		}

		internal void UpdateTimelineDateTime(DateTime timelineDateTime)
		{
			if (Control.ModifierKeys == Keys.Shift && _selectionAnchorDate != DateTime.MinValue)
			{
				if (timelineDateTime > _selectionAnchorDate)
				{
					SelectedRange = new DateRange(_selectionAnchorDate, timelineDateTime.AddSeconds(GetSelectionStepSeconds(false)));
				}
				else
				{
					SelectedRange = new DateRange(timelineDateTime, _selectionAnchorDate.AddSeconds(GetSelectionStepSeconds(false)));
				}
			}
			else
			{
				SelectedRange = new DateRange(timelineDateTime, timelineDateTime.AddSeconds(GetSelectionStepSeconds(false)));
				_selectionAnchorDate = timelineDateTime;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Height = BottomTier.GetBounds().Bottom;
			Invalidate(clientRectangle);
		}

		private int GetSundayBasedDayNumber(System.DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case System.DayOfWeek.Monday:
				return 2;
			case System.DayOfWeek.Tuesday:
				return 3;
			case System.DayOfWeek.Wednesday:
				return 4;
			case System.DayOfWeek.Thursday:
				return 5;
			case System.DayOfWeek.Friday:
				return 6;
			case System.DayOfWeek.Saturday:
				return 7;
			default:
				return 1;
			}
		}

		private int GetFirstDayOfWeekNumber(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case DayOfWeek.Monday:
				return 2;
			case DayOfWeek.Tuesday:
				return 3;
			case DayOfWeek.Wednesday:
				return 4;
			case DayOfWeek.Thursday:
				return 5;
			case DayOfWeek.Friday:
				return 6;
			case DayOfWeek.Saturday:
				return 7;
			default:
				return 1;
			}
		}

		private DateTime GetWeekStart(DateTime dateTime)
		{
			int dayNumber = GetSundayBasedDayNumber(dateTime.DayOfWeek);
			int firstDayNumber = GetFirstDayOfWeekNumber(_firstDayOfWeek);
			int daysSinceWeekStart = 7;
			daysSinceWeekStart = ((dayNumber > firstDayNumber) ? (dayNumber - firstDayNumber) : ((dayNumber != firstDayNumber) ? (7 - (firstDayNumber - dayNumber)) : 0));
			return dateTime.Subtract(new TimeSpan(daysSinceWeekStart, 0, 0, 0)).Date;
		}

		private string GetBottomTierFormat()
		{
			if (BottomTier.CustomFormat.Length > 0)
			{
				return BottomTier.CustomFormat;
			}
			switch (BottomTier.Interval)
			{
			case TimelineInterval.Second:
				return "ss";
			case TimelineInterval.Minute:
				return "mm";
			case TimelineInterval.Hour:
				if (_timeFormat == TimeFormat.TwentyFourHours)
				{
					return "%H";
				}
				return "h tt";
			case TimelineInterval.Week:
				return "d";
			case TimelineInterval.Year:
				return "yyy";
			case TimelineInterval.Month:
				return "MMMM";
			case TimelineInterval.Day:
				return "W dd";
			default:
				return "";
			}
		}

		public bool IsWorkingTime(DateTime startTime, DateTime endTime)
		{
			TimeSpan stepLength = TimeSpan.FromSeconds(GetBottomTierStepSeconds(startTime, false));
			DateTime dateTime = startTime;
			// A range in a collection applies to every day of its week days, so each piece is checked with the
			// ranges loaded for the day it lies in. A piece ends at the end of the span and at midnight at the latest.
			DateTime loadedDay = DateTime.MinValue;
			TimelineHourRangeCollection timelineHourRangeCollection = null;
			while (dateTime < endTime)
			{
				TimeSpan piece = endTime - dateTime < stepLength ? endTime - dateTime : stepLength;
				TimeSpan toMidnight = TimeSpan.FromDays(1.0) - dateTime.TimeOfDay;
				if (toMidnight < piece)
				{
					piece = toMidnight;
				}
				if (timelineHourRangeCollection == null || dateTime.Date != loadedDay)
				{
					DateTime dayEnd = dateTime.Date < DateTime.MaxValue.Date ? dateTime.Date.AddDays(1.0) : DateTime.MaxValue;
					timelineHourRangeCollection = WorkingHourSchema.GetHourRangesInRange(new DateRange(dateTime.Date, dayEnd));
					loadedDay = dateTime.Date;
				}
				if (!IsWorkingSlot(timelineHourRangeCollection, dateTime, piece))
				{
					return false;
				}
				dateTime = dateTime.Add(piece);
			}
			return true;
		}
	}
}
