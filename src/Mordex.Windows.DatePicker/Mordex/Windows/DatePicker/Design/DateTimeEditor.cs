using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DatePicker.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DateTimeEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private DatePickerCalendar _calendar;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					_calendar = new DatePickerCalendar();
					try
					{
						_calendar.MouseUp += OnCalendarMouseUp;
						_calendar.BorderStyle = BorderStyle.None;
						_calendar.SetSelectedDate((DateTime)value);
						_iWindowsFormsEditorService.DropDownControl(_calendar);
						value = _calendar.GetSelectedDate();
					}
					finally
					{
						_calendar.MouseUp -= OnCalendarMouseUp;
						_calendar.Dispose();
						_calendar = null;
					}
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.DropDown;
			}
			return base.GetEditStyle(context);
		}

		private void OnCalendarMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_iWindowsFormsEditorService != null && _calendar.GetCellAt(eventArgs.X, eventArgs.Y, false) != null)
			{
				_iWindowsFormsEditorService.CloseDropDown();
			}
		}
	}
}
