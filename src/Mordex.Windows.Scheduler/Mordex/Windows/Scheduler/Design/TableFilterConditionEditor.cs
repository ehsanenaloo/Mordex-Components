using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class TableFilterConditionEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void FillFilterList(MxScheduler scheduler, IFilterCondition iFilterCondition, ListBox listBox)
		{
			listBox.Items.Add(new ON(null, "(none)"));
			listBox.Items.Add(new ON(new object(), "Custom..."));
			if (scheduler.FilterCondition is SchedulerFilterCondition)
			{
				listBox.SelectedIndex = 1;
			}
			else if (scheduler.FilterCondition == null)
			{
				listBox.SelectedIndex = 0;
			}
			foreach (SchedulerStoredFilter storedFilter in scheduler.StoredFilters)
			{
				listBox.Items.Add(new ON(storedFilter, "[" + scheduler.StoredFilters.IndexOf(storedFilter) + "] - '" + storedFilter.Key + "'"));
				if (scheduler.FilterCondition == storedFilter)
				{
					listBox.SelectedIndex = listBox.Items.Count - 1;
				}
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
					MxScheduler scheduler = (MxScheduler)context.Instance;
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillFilterList(scheduler, scheduler.FilterCondition, listBox);
					_iWindowsFormsEditorService.DropDownControl(listBox);
					if (listBox.SelectedIndex == 1)
					{
						MxFilterBuilderDialog filterBuilderDialogDialog = new MxFilterBuilderDialog();
						filterBuilderDialogDialog.Text = "Schedule FilterCondition";
						filterBuilderDialogDialog.SetConditionOwner(scheduler);
						if (scheduler.FilterCondition is SchedulerFilterCondition)
						{
							filterBuilderDialogDialog.SetFilterCondition((SchedulerFilterCondition)((SchedulerFilterCondition)scheduler.FilterCondition).Clone());
						}
						else
						{
							filterBuilderDialogDialog.SetFilterCondition(new SchedulerFilterCondition());
						}
						if (_iWindowsFormsEditorService.ShowDialog(filterBuilderDialogDialog) == DialogResult.OK)
						{
							value = filterBuilderDialogDialog.GetFilterCondition();
						}
					}
					else
					{
						if (listBox.SelectedItem != null)
						{
							value = ((ON)listBox.SelectedItem).Object;
						}
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

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_iWindowsFormsEditorService != null)
			{
				_iWindowsFormsEditorService.CloseDropDown();
			}
		}
	}
}
