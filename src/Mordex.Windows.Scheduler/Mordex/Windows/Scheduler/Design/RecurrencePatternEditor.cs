using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class RecurrencePatternEditor : UITypeEditor
	{
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				WorkingHourRecurrencePattern workingHourRecurrencePattern = value as WorkingHourRecurrencePattern;
				RecurrenceForm recurrenceForm = new RecurrenceForm();
				recurrenceForm.ShowPatternDialog(workingHourRecurrencePattern);
				if (recurrenceForm.DialogResult == DialogResult.OK)
				{
					value = recurrenceForm.GetWorkingHourRecurrencePattern();
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}
}
