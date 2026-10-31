using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class CustomGroupEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		private void AddCustomGroupsToList(ArrayList list, GridTable table)
		{
			foreach (GridCustomGroup customGroup in table.CustomGroups)
			{
				list.Add(new GridValueListItem(customGroup, "[" + customGroup.Index + "] - '" + customGroup.Key + "'"));
			}
		}

		private void FillCustomGroupList(GridTable table, GridCustomGroup customGroupValue, ListBox editor)
		{
			int selectedIndex = -1;
			editor.DisplayMember = "Text";
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new GridValueListItem(null, "(none)"));
			if (table != null)
			{
				AddCustomGroupsToList(arrayList, table);
			}
			editor.DataSource = arrayList;
			editor.BindingContext = new BindingContext();
			if (customGroupValue != null)
			{
				for (int i = 0; i < arrayList.Count; i++)
				{
					if (((GridValueListItem)arrayList[i]).Value == customGroupValue)
					{
						selectedIndex = i;
						break;
					}
				}
			}
			editor.SelectedIndex = selectedIndex;
			editor.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null)
				{
					GridTable designModeTable = DesignHelper.GetDesignModeTable(context.Instance);
					if (designModeTable == null)
					{
						return value;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += listBox_MouseUp;
					FillCustomGroupList(designModeTable, (GridCustomGroup)value, listBox);
					edSvc.DropDownControl(listBox);
					value = ((listBox.SelectedIndex != -1) ? ((GridValueListItem)listBox.SelectedItem).Value : null);
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
