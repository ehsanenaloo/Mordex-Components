using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DataMemberEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void FillMemberList(MxScheduler scheduler, string strA, ListBox listBox, bool includeNoneItem)
		{
			int selectedItemIndex = -1;
			if (scheduler != null)
			{
				string[] availableMembers = scheduler.GetFieldNames();
				string[] memberNames = availableMembers;
				foreach (string text in memberNames)
				{
					listBox.Items.Add(text);
					if (string.Compare(strA, text, ignoreCase: true) == 0)
					{
						selectedItemIndex = listBox.Items.Count - 1;
					}
				}
			}
			listBox.Items.Add("(none)");
			if (selectedItemIndex == -1)
			{
				listBox.SelectedIndex = listBox.Items.Count - 1;
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
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					MxScheduler scheduler = null;
					bool includeNoneItem = true;
					if (context.Instance is SchedulerField)
					{
						scheduler = ((SchedulerField)context.Instance).Scheduler;
					}
					FillMemberList(scheduler, (string)value, listBox, includeNoneItem);
					_iWindowsFormsEditorService.DropDownControl(listBox);
					value = ((listBox.SelectedIndex != listBox.Items.Count - 1) ? listBox.SelectedItem : "");
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
