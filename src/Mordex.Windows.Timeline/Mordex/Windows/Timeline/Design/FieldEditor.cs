using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Timeline.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class FieldEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void PopulateFieldList(MxTimeline timeline, TimelineField timelineField, ListBox listBox, bool allowNone)
		{
			int selectedItemIndex = -1;
			if (timeline != null)
			{
				foreach (TimelineField field in timeline.Fields)
				{
					listBox.Items.Add(new ObjectName(field, "[" + field.Index + "] - '" + field.Key + "'"));
					if (field == timelineField)
					{
						selectedItemIndex = listBox.Items.Count - 1;
					}
				}
			}
			if (allowNone)
			{
				listBox.Items.Add(new ObjectName(null, "(none)"));
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
					MxTimeline timeline = null;
					if (context.Instance is TimelineFormatCondition)
					{
						if (context.PropertyDescriptor.Name == "TargetField")
						{
							allowNone = true;
						}
						timeline = ((TimelineFormatCondition)context.Instance).Timeline;
					}
					else if (context.Instance is TimelineGroup)
					{
						timeline = ((TimelineGroup)context.Instance).Timeline;
					}
					else
					{
						if (!(context.Instance is TimelineGroupHeaderTotal))
						{
							return value;
						}
						timeline = ((TimelineGroupHeaderTotal)context.Instance).Timeline;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					PopulateFieldList(timeline, (TimelineField)value, listBox, allowNone);
					_iWindowsFormsEditorService.DropDownControl(listBox);
					value = ((listBox.SelectedIndex != -1) ? ((ObjectName)listBox.SelectedItem).Object : null);
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
