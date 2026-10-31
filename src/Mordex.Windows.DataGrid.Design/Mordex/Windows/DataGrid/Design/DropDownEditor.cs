using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class DropDownEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public DropDownEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void FillDropDownList(GridColumn editingColumn, ListBox editor)
		{
			MxDataGrid dataGrid = editingColumn.DataGrid;
			editor.DisplayMember = "Text";
			int selectedIndex = -1;
			if (dataGrid != null)
			{
				foreach (GridDropDown dropDown in dataGrid.DropDowns)
				{
					editor.Items.Add(new GridValueListItem(dropDown, dropDown.Key));
					if (dropDown == editingColumn.DropDown)
					{
						selectedIndex = editor.Items.Count - 1;
					}
				}
			}
			editor.Items.Add(new GridValueListItem(null, "(none)"));
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
					ListBox listBox = new ListBox();
					listBox.MouseUp += listBox_MouseUp;
					FillDropDownList((GridColumn)context.Instance, listBox);
					edSvc.DropDownControl(listBox);
					if (listBox.SelectedItem != null)
					{
						value = ((GridValueListItem)listBox.SelectedItem).Value;
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

		private void listBox_MouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (edSvc != null)
			{
				edSvc.CloseDropDown();
			}
		}
	}
}
