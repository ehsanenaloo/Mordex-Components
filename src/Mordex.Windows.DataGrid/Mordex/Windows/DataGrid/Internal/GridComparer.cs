using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridComparer : IComparer
	{
		internal enum DateRangeType
		{
			NullDate,
			DateRange,
			LessThanMinimum,
			GreaterThanMaximum,
			Empty
		}

		internal class DateBucket
		{
			internal object _nullValue;

			internal DateTime _min;

			internal DateTime _max;

			internal string _name;

			internal DateRangeType _rangeType;

			internal DateBucket(DateTime min, DateTime max, DateRangeType rangeType, string name)
			{
				_min = min;
				_max = max;
				_name = name;
				_rangeType = rangeType;
			}

			internal DateBucket(object nullValue, DateRangeType rangeType, string name)
			{
				_nullValue = nullValue;
				_name = name;
				_rangeType = rangeType;
			}

			internal bool ContainsValue(object value)
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
					DateTime addDays = _max.AddDays(1.0);
					if (dateTime >= _min)
					{
						return dateTime < addDays;
					}
					return false;
				}
				default:
					return false;
				}
			}
		}

		private DateBucket[] _gridState27Array;

		private MxDataGrid _grid;

		private DateTime _dateTime;

		private bool _followsToday;

		private readonly System.Collections.Generic.Dictionary<DateTime, int> _bucketIndexCache = new System.Collections.Generic.Dictionary<DateTime, int>();

		internal GridComparer(MxDataGrid grid)
		{
			_grid = grid;
			_dateTime = DateTime.Today;
			_followsToday = true;
			BuildDateBuckets(null);
		}

		internal DateTime GetReferenceDate()
		{
			return _dateTime;
		}

		private string GetBucketName(int value)
		{
			GridBuiltInText textID;
			switch (value)
			{
			case 1:
				textID = GridBuiltInText.OutlookDateOlder;
				break;
			case 2:
				textID = GridBuiltInText.OutlookDateLastMonth;
				break;
			case 3:
				textID = GridBuiltInText.OutlookDateEarlierThisMonth;
				break;
			case 4:
				textID = GridBuiltInText.OutlookDateThreeWeeksAgo;
				break;
			case 5:
				textID = GridBuiltInText.OutlookDateTwoWeeksAgo;
				break;
			case 6:
				textID = GridBuiltInText.OutlookDateLastWeek;
				break;
			case 17:
				textID = GridBuiltInText.OutlookDateNextWeek;
				break;
			case 18:
				textID = GridBuiltInText.OutlookDateTwoWeeksAway;
				break;
			case 19:
				textID = GridBuiltInText.OutlookDateThreeWeeksAway;
				break;
			case 20:
				textID = GridBuiltInText.OutlookDateLaterThisMonth;
				break;
			case 21:
				textID = GridBuiltInText.OutlookDateNextMonth;
				break;
			case 22:
				textID = GridBuiltInText.OutlookDateBeyondNextMonth;
				break;
			case 8:
				textID = GridBuiltInText.OutlookDateToday;
				break;
			case 7:
				textID = GridBuiltInText.OutlookDateYesterday;
				break;
			case 16:
				textID = GridBuiltInText.OutlookDateTomorrow;
				break;
			default:
				textID = GridBuiltInText.OutlookDateEmpty;
				break;
			}
			return _grid.BuiltInTexts[textID];
		}

		private void BuildDateBuckets(object nullValue)
		{
			_bucketIndexCache.Clear();
			_gridState27Array = new DateBucket[23];
			Calendar calendar = CultureInfo.CurrentCulture.Calendar;
			DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
			DateTime dateTime = GetReferenceDate();
			DateTime weekStart = GetWeekStart(calendar, dateTimeFormat, dateTime);
			DateTime weekEnd = GetWeekEnd(calendar, dateTimeFormat, dateTime);
			_gridState27Array[0] = new DateBucket(nullValue, DateRangeType.NullDate, GetBucketName(0));
			DateTime monthStart = GetMonthStart(dateTime.AddMonths(-1));
			_gridState27Array[1] = new DateBucket(monthStart, monthStart, DateRangeType.LessThanMinimum, GetBucketName(1));
			DateTime max = GetMonthEnd(calendar, monthStart);
			_gridState27Array[2] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(2));
			monthStart = calendar.AddWeeks(weekStart, -3);
			if (monthStart > GetMonthStart(dateTime))
			{
				_gridState27Array[3] = new DateBucket(GetMonthStart(dateTime), monthStart.AddDays(-1.0), DateRangeType.DateRange, GetBucketName(3));
			}
			else
			{
				_gridState27Array[2]._max = monthStart.AddDays(-1.0);
				_gridState27Array[3] = new DateBucket(null, DateRangeType.Empty, "");
			}
			max = GetWeekEnd(calendar, dateTimeFormat, monthStart);
			_gridState27Array[4] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(4));
			monthStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, monthStart);
			_gridState27Array[5] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(5));
			monthStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, monthStart);
			_gridState27Array[6] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(6));
			monthStart = dateTime.AddDays(-1.0);
			_gridState27Array[7] = new DateBucket(monthStart, monthStart, DateRangeType.DateRange, GetBucketName(7));
			_gridState27Array[8] = new DateBucket(null, DateRangeType.Empty, "");
			monthStart = dateTime.AddDays(1.0);
			_gridState27Array[16] = new DateBucket(monthStart, monthStart, DateRangeType.DateRange, GetBucketName(16));
			monthStart = weekStart;
			int value = 9;
			while (monthStart <= weekEnd)
			{
				if (monthStart == _gridState27Array[7]._min)
				{
					_gridState27Array[7]._rangeType = DateRangeType.Empty;
					_gridState27Array[value] = new DateBucket(monthStart, monthStart, DateRangeType.DateRange, GetBucketName(7));
				}
				else if (monthStart == dateTime)
				{
					_gridState27Array[value] = new DateBucket(monthStart, monthStart, DateRangeType.DateRange, GetBucketName(8));
				}
				else if (monthStart == _gridState27Array[16]._min)
				{
					_gridState27Array[16]._rangeType = DateRangeType.Empty;
					_gridState27Array[value] = new DateBucket(monthStart, monthStart, DateRangeType.DateRange, GetBucketName(16));
				}
				else
				{
					string dayName = dateTimeFormat.GetDayName(calendar.GetDayOfWeek(monthStart));
					_gridState27Array[value] = new DateBucket(monthStart, monthStart, DateRangeType.DateRange, dayName);
				}
				value++;
				monthStart = monthStart.AddDays(1.0);
			}
			monthStart = weekEnd.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, monthStart);
			_gridState27Array[17] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(17));
			monthStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, monthStart);
			_gridState27Array[18] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(18));
			monthStart = max.AddDays(1.0);
			max = GetWeekEnd(calendar, dateTimeFormat, monthStart);
			_gridState27Array[19] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(19));
			if (max < GetMonthEnd(calendar, dateTime))
			{
				_gridState27Array[20] = new DateBucket(max.AddDays(1.0), GetMonthEnd(calendar, dateTime), DateRangeType.DateRange, GetBucketName(20));
				monthStart = GetMonthStart(dateTime.AddMonths(1));
			}
			else
			{
				_gridState27Array[20] = new DateBucket(null, DateRangeType.Empty, "");
				monthStart = max.AddDays(1.0);
			}
			max = GetMonthEnd(calendar, monthStart);
			_gridState27Array[21] = new DateBucket(monthStart, max, DateRangeType.DateRange, GetBucketName(21));
			max = max.AddDays(1.0);
			_gridState27Array[22] = new DateBucket(max, max, DateRangeType.GreaterThanMaximum, GetBucketName(22));
		}

		private int FindBucketIndex(object value)
		{
			if (value is DateTime date)
			{
				if (!_bucketIndexCache.TryGetValue(date, out int cached))
				{
					cached = ScanBuckets(value);
					_bucketIndexCache[date] = cached;
				}
				return cached;
			}
			return ScanBuckets(value);
		}

		private int ScanBuckets(object value)
		{
			for (int i = 0; i < _gridState27Array.Length; i++)
			{
				DateBucket a = _gridState27Array[i];
				if (a.ContainsValue(value))
				{
					return i;
				}
			}
			return 0;
		}

		int IComparer.Compare(object value, object valueArgument)
		{
			if (_followsToday && _dateTime != DateTime.Today)
			{
				_dateTime = DateTime.Today;
				BuildDateBuckets(null);
			}
			return FindBucketIndex(value).CompareTo(FindBucketIndex(valueArgument));
		}

		private DateTime GetWeekStart(Calendar calendar, DateTimeFormatInfo dateTimeFormatInfo, DateTime time)
		{
			DayOfWeek dayOfWeek = calendar.GetDayOfWeek(time);
			DayOfWeek firstDayOfWeek = dateTimeFormatInfo.FirstDayOfWeek;
			int value = firstDayOfWeek - dayOfWeek;
			if (value > 0)
			{
				value -= 7;
			}
			return time.AddDays(value);
		}

		private DateTime GetWeekEnd(Calendar calendar, DateTimeFormatInfo dateTimeFormatInfo, DateTime dateTime)
		{
			return GetWeekStart(calendar, dateTimeFormatInfo, dateTime).AddDays(6.0);
		}

		private DateTime GetMonthStart(DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, 1);
		}

		private DateTime GetMonthEnd(Calendar calendar, DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, calendar.GetDaysInMonth(dateTime.Year, dateTime.Month));
		}
	}
}
