using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FilterConditionEditor : UITypeEditor
	{
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				SchedulerFilterCondition schedulerFilterCondition = (SchedulerFilterCondition)value;
				if (schedulerFilterCondition != null)
				{
					schedulerFilterCondition = (SchedulerFilterCondition)schedulerFilterCondition.Clone();
				}
				MxFilterBuilderDialog filterBuilderDialogDialog = new MxFilterBuilderDialog();
				filterBuilderDialogDialog.SetFilterCondition(schedulerFilterCondition);
				filterBuilderDialogDialog.SetConditionOwner((ISchedulerFilterOwner)context.Instance);
				filterBuilderDialogDialog.Text = context.PropertyDescriptor.Name;
				filterBuilderDialogDialog.ShowDialog();
				if (filterBuilderDialogDialog.DialogResult == DialogResult.OK)
				{
					value = filterBuilderDialogDialog.GetFilterCondition();
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
