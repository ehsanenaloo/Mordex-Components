using System;
using System.ComponentModel;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[Serializable]
	[TypeConverter(typeof(DateRangeConverter))]
	public struct DateRange
	{
		private DateTime _start;

		private DateTime _end;

		public DateTime Start
		{
			get
			{
				return _start;
			}
			set
			{
				_start = value;
			}
		}

		public DateTime End
		{
			get
			{
				return _end;
			}
			set
			{
				_end = value;
			}
		}

		public DateRange(DateTime start, DateTime end)
		{
			_start = start;
			_end = end;
		}

		internal bool IsIncomplete()
		{
			if (Start == DateTime.MinValue)
			{
				return true;
			}
			if (End == DateTime.MinValue)
			{
				return true;
			}
			return false;
		}
	}
}
