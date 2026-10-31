using System;
using System.Runtime.CompilerServices;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal class CalendarEventArgs : EventArgs
	{
		private DateTime _date;

		public CalendarEventArgs(DateTime date)
		{
			_date = date;
		}
	}
}
