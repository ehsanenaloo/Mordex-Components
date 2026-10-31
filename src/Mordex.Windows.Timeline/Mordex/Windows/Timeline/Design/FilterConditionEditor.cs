using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FilterConditionEditor : UITypeEditor
	{
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				TimelineFilterCondition timelineFilterCondition = (TimelineFilterCondition)value;
				if (timelineFilterCondition != null)
				{
					timelineFilterCondition = (TimelineFilterCondition)timelineFilterCondition.Clone();
				}
				MxFilterBuilderDialog filterBuilderDialogDialog = new MxFilterBuilderDialog();
				filterBuilderDialogDialog.SetFilterCondition(timelineFilterCondition);
				if (context.Instance is TimelineStoredFilter)
				{
					filterBuilderDialogDialog.UpdateIConditionOwner(((TimelineStoredFilter)context.Instance).Timeline.GetRootTable());
				}
				else if (context.Instance is TimelineGroupHeaderTotal)
				{
					filterBuilderDialogDialog.UpdateIConditionOwner(((TimelineGroupHeaderTotal)context.Instance).Timeline.GetRootTable());
				}
				else if (context.Instance is TimelineFormatCondition)
				{
					filterBuilderDialogDialog.UpdateIConditionOwner(((TimelineFormatCondition)context.Instance).Timeline.GetRootTable());
				}
				else
				{
					filterBuilderDialogDialog.UpdateIConditionOwner(((MxTimeline)context.Instance).GetRootTable());
				}
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
