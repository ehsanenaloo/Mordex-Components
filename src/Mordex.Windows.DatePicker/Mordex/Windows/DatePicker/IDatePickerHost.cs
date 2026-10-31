using System.Drawing;

namespace Mordex.Windows.DatePicker
{
	internal interface IDatePickerHost
	{
		Pen FlatBorderPen { get; }

		MxDatePicker DatePicker { get; }

		void OnControlStyleChanged();
	}
}
