using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DataMemberEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public DataMemberEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void FillDataMemberList(GridTable table, string value, ListBox editor, bool retrieveFields, bool includeChildLists)
		{
			int selectedIndex = -1;
			if (table != null)
			{
				PropertyDescriptor[] tableAvailableFields = DesignHelper.GetTableAvailableFields(table, retrieveFields, includeChildLists);
				if (tableAvailableFields != null)
				{
					PropertyDescriptor[] array = tableAvailableFields;
					foreach (PropertyDescriptor propertyDescriptor in array)
					{
						editor.Items.Add(propertyDescriptor.Name);
						if (string.Compare(value, propertyDescriptor.Name, ignoreCase: true) == 0)
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
					ListBox listBox = new ListBox();
					listBox.MouseUp += listBox_MouseUp;
					GridTable gridTable = null;
					bool retrieveFields = true;
					bool includeChildLists = false;
					if (context.Instance is GridColumn)
					{
						GridColumn gridColumn = (GridColumn)context.Instance;
						gridTable = gridColumn.Table;
						if (gridColumn.MultipleValues)
						{
							includeChildLists = true;
							retrieveFields = false;
						}
					}
					else if (context.Instance is GridSelfReferencingSettings)
					{
						gridTable = ((GridSelfReferencingSettings)context.Instance).Table;
						if (string.Compare(context.PropertyDescriptor.Name, "ChildListMember", ignoreCase: true) == 0)
						{
							retrieveFields = false;
							includeChildLists = true;
						}
					}
					else if (context.Instance is GridTable)
					{
						gridTable = (GridTable)context.Instance;
						if (context.PropertyDescriptor.Name == "DataMember")
						{
							retrieveFields = false;
							includeChildLists = true;
							gridTable = gridTable.ParentTable;
						}
					}
					else if (context.Instance is GridDropDown)
					{
						gridTable = ((MxDataGrid)context.Instance).RootTable;
					}
					FillDataMemberList(gridTable, (string)value, listBox, retrieveFields, includeChildLists);
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
