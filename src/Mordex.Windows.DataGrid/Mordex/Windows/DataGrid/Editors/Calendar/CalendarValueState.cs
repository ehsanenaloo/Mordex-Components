using System;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal struct CalendarValueState
	{
		private object _start;

		private object _end;

		public CalendarValueState(object Start, object End)
		{
			_start = Start;
			_end = End;
		}

		public object GetStart()
		{
			return _start;
		}

		public void UpdateStart(object start)
		{
			if (start != null && !(start is DateTime))
			{
				throw new ArgumentException();
			}
			_start = start;
		}

		public object GetEnd()
		{
			return _end;
		}

		public void UpdateEnd(object end)
		{
			if (end != null && !(end is DateTime))
			{
				throw new ArgumentException();
			}
			_end = end;
		}
	}
}
