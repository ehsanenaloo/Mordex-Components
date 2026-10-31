namespace Mordex.Windows.DatePicker
{
	internal interface IDatePickerElement
	{
		DatePickerCalendar Calendar { get; }

		void Invalidate();
	}
}
