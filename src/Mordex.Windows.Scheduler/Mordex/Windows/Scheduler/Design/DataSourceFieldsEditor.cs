using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DataSourceFieldsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void FillFieldList(MxScheduler scheduler, string text, ListBox listBox, bool shouldListFields)
		{
			int selectedIndex = -1;
			if (shouldListFields && scheduler.GetCurrencyManager() != null)
			{
				PropertyDescriptorCollection itemProperties = scheduler.GetCurrencyManager().GetItemProperties();
				foreach (PropertyDescriptor item in itemProperties)
				{
					if (!typeof(IList).IsAssignableFrom(item.PropertyType))
					{
						listBox.Items.Add(item.Name);
						if (item.Name == text)
						{
							selectedIndex = listBox.Items.Count - 1;
						}
					}
				}
			}
			listBox.Items.Add("(none)");
			if (selectedIndex == -1)
			{
				listBox.SelectedIndex = listBox.Items.Count - 1;
			}
			else
			{
				listBox.SelectedIndex = selectedIndex;
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
					MxScheduler scheduler = null;
					if (!(context.Instance is MxScheduler))
					{
						return value;
					}
					scheduler = (MxScheduler)context.Instance;
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillFieldList(scheduler, (string)value, listBox, !scheduler.IsUnboundMode());
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
