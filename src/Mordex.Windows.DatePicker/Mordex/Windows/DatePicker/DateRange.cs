using System;

namespace Mordex.Windows.DatePicker
{
	internal struct DateRange
	{
		public DateTime _start;

		public DateTime _end;

		internal DateRange(DateTime start, DateTime end)
		{
			_start = start;
			_end = end;
		}
	}
}
