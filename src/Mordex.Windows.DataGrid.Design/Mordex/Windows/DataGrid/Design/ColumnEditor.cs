using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ColumnEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		private bool includeChildFields;

		public ColumnEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void AddColumnsToList(ArrayList list, GridTable table, bool includeTable, int level)
		{
			string text = new string(' ', level * 4);
			if (includeTable)
			{
				list.Add(new GridValueListItem(null, new string(' ', level * 4 - 2) + "ChildTable[" + table.Key + "]"));
			}
			foreach (GridColumn column in table.Columns)
			{
				list.Add(new GridValueListItem(column, text + "[" + column.Index + "] - '" + column.Key + "'"));
			}
			if (!includeChildFields)
			{
				return;
			}
			foreach (GridTable childTable in table.ChildTables)
			{
				AddColumnsToList(list, childTable, includeTable: true, level + 1);
			}
		}

		private void FillColumnList(GridTable table, GridColumn columnValue, ListBox editor, bool allowNulls)
		{
			int selectedIndex = -1;
			editor.DisplayMember = "Text";
			ArrayList arrayList = new ArrayList();
			if (allowNulls)
			{
				arrayList.Add(new GridValueListItem(null, "(none)"));
			}
			if (table != null)
			{
				AddColumnsToList(arrayList, table, includeTable: false, 0);
				editor.DataSource = arrayList;
			}
			if (selectedIndex == -1)
			{
				if (allowNulls)
				{
					editor.SelectedIndex = editor.Items.Count - 1;
				}
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
					bool allowNulls = false;
					GridTable gridTable = null;
					includeChildFields = false;
					if (context.Instance is MxDataGrid)
					{
						allowNulls = true;
						gridTable = ((MxDataGrid)context.Instance).RootTable;
					}
					else if (context.Instance is GridFormatCondition)
					{
						if (context.PropertyDescriptor.Name == "TargetColumn")
						{
							allowNulls = true;
						}
						gridTable = ((GridFormatCondition)context.Instance).Table;
					}
					else if (context.Instance is GridGroup)
					{
						gridTable = DesignHelper.GetDesignModeTable(context.Instance);
						if (gridTable != null)
						{
							includeChildFields = gridTable.AllowChildTableGroups == InheritableBoolean.True
								|| (gridTable.AllowChildTableGroups == InheritableBoolean.Default && gridTable.DataGrid.AllowChildTableGroups);
						}
					}
					else if (context.Instance is GridSortKey)
					{
						gridTable = ((GridSortKey)context.Instance).Table;
					}
					else if (context.Instance is GridGroupHeaderTotal)
					{
						allowNulls = true;
						gridTable = DesignHelper.GetDesignModeTable(context.Instance);
					}
					else
					{
						if (!(context.Instance is GridSelfReferencingSettings))
						{
							return value;
						}
						gridTable = ((GridSelfReferencingSettings)context.Instance).Table;
					}
					using (ListBox listBox = new ListBox())
					{
						listBox.MouseUp += listBox_MouseUp;
						FillColumnList(gridTable, (GridColumn)value, listBox, allowNulls);
						edSvc.DropDownControl(listBox);
						value = ((listBox.SelectedIndex != -1) ? ((GridValueListItem)listBox.SelectedItem).Value : null);
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
