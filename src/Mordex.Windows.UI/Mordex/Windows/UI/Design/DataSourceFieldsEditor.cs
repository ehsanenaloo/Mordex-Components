using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DataSourceFieldsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void FillFieldList(MxComboBox uiComboBox, string text, ListBox listBox, bool hasDataSource)
		{
			int selectedIndex = -1;
			if (hasDataSource && uiComboBox.GetActiveCurrencyManager() != null)
			{
				PropertyDescriptorCollection itemProperties = uiComboBox.GetActiveCurrencyManager().GetItemProperties();
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
				_editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_editorService != null)
				{
					MxComboBox uiComboBox = null;
					if (!(context.Instance is MxComboBox))
					{
						return value;
					}
					uiComboBox = (MxComboBox)context.Instance;
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillFieldList(uiComboBox, (string)value, listBox, !uiComboBox.IsDataSourceEmpty());
					_editorService.DropDownControl(listBox);
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
			if (_editorService != null)
			{
				_editorService.CloseDropDown();
			}
		}
	}
}
