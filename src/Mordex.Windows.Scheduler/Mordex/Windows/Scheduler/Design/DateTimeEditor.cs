using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DateTimeEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private MxCalendarView _calendar;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					_calendar = new MxCalendarView();
					_calendar.MouseUp += OnCalendarMouseUp;
					_calendar.BorderStyle = BorderStyle.None;
					_calendar.CurrentDate = (DateTime)value;
					_iWindowsFormsEditorService.DropDownControl(_calendar);
					value = _calendar.CurrentDate;
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
			if (_iWindowsFormsEditorService != null && _calendar.FindDayCellAt(eventArgs.X, eventArgs.Y, false) != null)
			{
				_iWindowsFormsEditorService.CloseDropDown();
			}
		}
	}
}
