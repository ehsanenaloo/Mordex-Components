using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DataSourceFieldsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		private void FillFieldList(MxMultiColumnComboBox combo, string fieldValue, ListBox editor)
		{
			int selectedIndex = -1;
			BindingContext bindingContext = combo.BindingContext;
			CurrencyManager currencyManager = null;
			if (bindingContext != null && combo.DataSource != null)
			{
				currencyManager = bindingContext[combo.DataSource, combo.DataMember] as CurrencyManager;
			}
			if (currencyManager != null)
			{
				PropertyDescriptorCollection itemProperties = currencyManager.GetItemProperties();
				foreach (PropertyDescriptor item in itemProperties)
				{
					if (!typeof(IList).IsAssignableFrom(item.PropertyType))
					{
						editor.Items.Add(item.Name);
						if (item.Name == fieldValue)
						{
							selectedIndex = editor.Items.Count - 1;
						}
					}
				}
			}
			editor.Items.Add("(none)");
			if (selectedIndex == -1)
			{
				editor.SelectedIndex = editor.Items.Count - 1;
			}
			else
			{
				editor.SelectedIndex = selectedIndex;
			}
			editor.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null)
				{
					MxMultiColumnComboBox multiColumnCombo = null;
					if (!(context.Instance is MxMultiColumnComboBox))
					{
						return value;
					}
					multiColumnCombo = (MxMultiColumnComboBox)context.Instance;
					ListBox listBox = new ListBox();
					listBox.MouseUp += listBox_MouseUp;
					FillFieldList(multiColumnCombo, (string)value, listBox);
					edSvc.DropDownControl(listBox);
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

		private void listBox_MouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (edSvc != null)
			{
				edSvc.CloseDropDown();
			}
		}
	}
}
