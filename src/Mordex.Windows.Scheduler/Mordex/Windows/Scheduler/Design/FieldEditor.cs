using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class FieldEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void PopulateFieldList(MxScheduler scheduler, SchedulerField schedulerField, ListBox listBox, bool allowNone)
		{
			int selectedItemIndex = -1;
			foreach (SchedulerField field in scheduler.Fields)
			{
				listBox.Items.Add(new ON(field, "[" + field._index + "] - '" + field.Key + "'"));
				if (field == schedulerField)
				{
					selectedItemIndex = listBox.Items.Count - 1;
				}
			}
			if (allowNone)
			{
				listBox.Items.Add(new ON(null, "(none)"));
			}
			if (selectedItemIndex == -1)
			{
				if (allowNone)
				{
					listBox.SelectedIndex = listBox.Items.Count - 1;
				}
			}
			else
			{
				listBox.SelectedIndex = selectedItemIndex;
			}
			listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					bool allowNone = false;
					MxScheduler scheduler = null;
					if (context.Instance is SchedulerFormatCondition)
					{
						if (context.PropertyDescriptor.Name == "TargetField")
						{
							allowNone = true;
						}
						scheduler = ((SchedulerFormatCondition)context.Instance).Scheduler;
						ListBox listBox = new ListBox();
						listBox.MouseUp += OnControlMouseUp;
						PopulateFieldList(scheduler, (SchedulerField)value, listBox, allowNone);
						_iWindowsFormsEditorService.DropDownControl(listBox);
						if (listBox.SelectedItem != null)
						{
							return ((ON)listBox.SelectedItem).Object;
						}
						return null;
					}
					return value;
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

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_iWindowsFormsEditorService != null)
			{
				_iWindowsFormsEditorService.CloseDropDown();
			}
		}
	}
}
