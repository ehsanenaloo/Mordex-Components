using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Timeline.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DataSourceFieldsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void FillFieldList(MxTimeline timeline, string text, ListBox listBox, bool shouldListFields)
		{
			int selectedIndex = -1;
			if (shouldListFields && timeline.GetCurrencyManager() != null)
			{
				PropertyDescriptorCollection itemProperties = timeline.GetCurrencyManager().GetItemProperties();
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
					MxTimeline timeline = null;
					if (!(context.Instance is MxTimeline))
					{
						return value;
					}
					timeline = (MxTimeline)context.Instance;
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillFieldList(timeline, (string)value, listBox, !timeline.IsUnbound());
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
