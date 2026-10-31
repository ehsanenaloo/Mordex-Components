using System;
using System.Globalization;

namespace Mordex.Data.Internal
{
	internal class OutlookDateRangeCalculator
	{
		internal enum DateRangeType
		{
			NullDate,
			DateRange,
			LessThanMinimum,
			GreaterThanMaximum,
			Empty
		}

		internal class DateRangeBucket
		{
			internal object _nullValue;

			internal DateTime _min;

			internal DateTime _max;

			internal string _name;

			internal DateRangeType _rangeType;

			internal DateRangeBucket(DateTime min, DateTime max, DateRangeType rangeType, string name)
			{
				_min = min;
				_max = max;
				_name = name;
				_rangeType = rangeType;
			}

			internal DateRangeBucket(object nullValue, DateRangeType rangeType, string name)
			{
				_nullValue = nullValue;
				_name = name;
				_rangeType = rangeType;
			}

			internal bool Contains(object value)
			{
				if (_rangeType == DateRangeType.NullDate)
				{
					if (value == null || value == DBNull.Value || value == _nullValue)
					{
						return true;
					}
					return false;
				}
				if (!(value is DateTime dateTime))
				{
					return false;
				}
				switch (_rangeType)
				{
				case DateRangeType.LessThanMinimum:
					return dateTime < _min;
				case DateRangeType.GreaterThanMaximum:
					return dateTime >= _max;
				case DateRangeType.DateRange:
				{
					DateTime exclusiveEnd = _max.AddDays(1.0);
					if (dateTime >= _min)
					{
						return dateTime < exclusiveEnd;
					}
					return false;
				}
				default:
					return false;
				}
			}
		}

		private DateRangeBucket[] _buckets;

		private DataSourceManager _manager;

		private DateTime _today;

		internal DateTime Today => _today;

		internal OutlookDateRangeCalculator(DataSourceManager manager)
		{
			_manager = manager;
			_today = DateTime.Today;
			BuildBuckets(null);
		}

		private string GetRangeName(OutlookDateRange outlookDateRange)
		{
			return _manager.GetOutlookRangeName(outlookDateRange);
		}

		private void BuildBuckets(object nullValue)
		{
			_buckets = new DateRangeBucket[23];
			Calendar calendar = CultureInfo.CurrentCulture.Calendar;
			DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
			DateTime dateTime = GetWeekStart(calendar, dateTimeFormat, _today);
			DateTime weekEnd = GetWeekEnd(calendar, dateTimeFormat, _today);
			_buckets[0] = new DateRangeBucket(nullValue, DateRangeType.NullDate, GetRangeName(OutlookDateRange.NullDate));
			DateTime rangeStart = GetMonthStart(_today.AddMonths(-1));
			_buckets[1] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.LessThanMinimum, GetRangeName(OutlookDateRange.Older));
			DateTime max = GetMonthEnd(rangeStart);
			_buckets[2] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.LastMonth));
			rangeStart = calendar.AddWeeks(dateTime, -3);
			if (rangeStart > GetMonthStart(_today))
			{
				_buckets[3] = new DateRangeBucket(GetMonthStart(_today), rangeStart.AddDays(-1.0), DateRangeType.DateRange, GetRangeName(OutlookDateRange.EarlierThisMonth));
			}
			else
			{
				_buckets[2]._max = rangeStart.AddDays(-1.0);
				_buckets[3] = new DateRangeBucket(null, DateRangeType.Empty, "");
			}
			max = GetWeekEnd(calendar, dateTimeFormat, rangeStart);
			_buckets[4] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.ThreeWeeksAgo));
			rangeStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, rangeStart);
			_buckets[5] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.TwoWeeksAgo));
			rangeStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, rangeStart);
			_buckets[6] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.LastWeek));
			rangeStart = _today.AddDays(-1.0);
			_buckets[7] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.DateRange, GetRangeName(OutlookDateRange.Yesterday));
			_buckets[8] = new DateRangeBucket(null, DateRangeType.Empty, "");
			rangeStart = _today.AddDays(1.0);
			_buckets[16] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.DateRange, GetRangeName(OutlookDateRange.Tomorrow));
			rangeStart = dateTime;
			int slotIndex = 9;
			while (rangeStart <= weekEnd)
			{
				if (rangeStart == _buckets[7]._min)
				{
					_buckets[7]._rangeType = DateRangeType.Empty;
					_buckets[slotIndex] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.DateRange, GetRangeName(OutlookDateRange.Yesterday));
				}
				else if (rangeStart == _today)
				{
					_buckets[slotIndex] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.DateRange, GetRangeName(OutlookDateRange.Today));
				}
				else if (rangeStart == _buckets[16]._min)
				{
					_buckets[16]._rangeType = DateRangeType.Empty;
					_buckets[slotIndex] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.DateRange, GetRangeName(OutlookDateRange.Tomorrow));
				}
				else
				{
					string dayName = dateTimeFormat.GetDayName(calendar.GetDayOfWeek(rangeStart));
					_buckets[slotIndex] = new DateRangeBucket(rangeStart, rangeStart, DateRangeType.DateRange, dayName);
				}
				slotIndex++;
				rangeStart = rangeStart.AddDays(1.0);
			}
			rangeStart = weekEnd.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, rangeStart);
			_buckets[17] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.NextWeek));
			rangeStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, rangeStart);
			_buckets[18] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.TwoWeeksAway));
			rangeStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, rangeStart);
			_buckets[19] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.ThreeWeeksAway));
			if (max < GetMonthEnd(_today))
			{
				_buckets[20] = new DateRangeBucket(max.AddDays(1.0), GetMonthEnd(_today), DateRangeType.DateRange, GetRangeName(OutlookDateRange.LaterThisMonth));
				rangeStart = GetMonthStart(_today.AddMonths(1));
			}
			else
			{
				_buckets[20] = new DateRangeBucket(null, DateRangeType.Empty, "");
				rangeStart = max.AddDays(1.0);
			}
			max = GetMonthEnd(rangeStart);
			_buckets[21] = new DateRangeBucket(rangeStart, max, DateRangeType.DateRange, GetRangeName(OutlookDateRange.NextMonth));
			max.AddDays(1.0);
			_buckets[22] = new DateRangeBucket(max, max, DateRangeType.GreaterThanMaximum, GetRangeName(OutlookDateRange.BeyondNextMonth));
		}

		internal OutlookDateRange FindRange(object value)
		{
			for (int i = 0; i < _buckets.Length; i++)
			{
				DateRangeBucket a = _buckets[i];
				if (a.Contains(value))
				{
					return (OutlookDateRange)i;
				}
			}
			return OutlookDateRange.NullDate;
		}

		private static DateTime GetWeekStart(Calendar calendar, DateTimeFormatInfo dateTimeFormatInfo, DateTime time)
		{
			DayOfWeek dayOfWeek = calendar.GetDayOfWeek(time);
			DayOfWeek firstDayOfWeek = dateTimeFormatInfo.FirstDayOfWeek;
			int dayOffset = firstDayOfWeek - dayOfWeek;
			if (dayOffset > 0)
			{
				dayOffset -= 7;
			}
			return time.AddDays(dayOffset);
		}

		private static DateTime GetWeekEnd(Calendar calendar, DateTimeFormatInfo dateTimeFormatInfo, DateTime dateTime)
		{
			return GetWeekStart(calendar, dateTimeFormatInfo, dateTime).AddDays(6.0);
		}

		private static DateTime GetMonthStart(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1);
		}

		private static DateTime GetMonthEnd(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
		}

		internal string GetName(OutlookDateRange outlookDateRange)
		{
			return _buckets[(int)outlookDateRange]._name;
		}
	}
}
