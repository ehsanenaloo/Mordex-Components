using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class TableFilterConditionEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public TableFilterConditionEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void FillFilterList(GridTable table, IFilterCondition filter, ListBox editor)
		{
			editor.DisplayMember = "Text";
			editor.Items.Add(new GridValueListItem(null, "(none)"));
			editor.Items.Add(new GridValueListItem(new object(), "Custom..."));
			if (table.FilterCondition is GridFilterCondition)
			{
				editor.SelectedIndex = 1;
			}
			else if (table.FilterCondition == null)
			{
				editor.SelectedIndex = 0;
			}
			foreach (GridStoredFilter storedFilter in table.StoredFilters)
			{
				editor.Items.Add(new GridValueListItem(storedFilter, "[" + table.StoredFilters.IndexOf(storedFilter) + "] - '" + storedFilter.Key + "'"));
				if (table.FilterCondition == storedFilter)
				{
					editor.SelectedIndex = editor.Items.Count - 1;
				}
			}
			editor.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				try
				{
					edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					if (edSvc != null)
					{
						GridTable gridTable = (GridTable)context.Instance;
						ListBox listBox = new ListBox();
						listBox.MouseUp += listBox_MouseUp;
						FillFilterList(gridTable, gridTable.FilterCondition, listBox);
						edSvc.DropDownControl(listBox);
						if (listBox.SelectedIndex == 1)
						{
							GridFilterCondition gridFilterCondition = null;
							gridFilterCondition = ((!(gridTable.FilterCondition is GridFilterCondition)) ? new GridFilterCondition() : ((GridFilterCondition)gridTable.FilterCondition).Clone());
							Form form = DesignHelper.CreateFilterBuilderDesigner(gridTable, gridFilterCondition);
							form.Text = "Table FilterCondition";
							if (edSvc.ShowDialog(form) == DialogResult.OK && form is IDesignDialog designDialog)
							{
								value = designDialog.Value as GridFilterCondition;
							}
						}
						else
						{
							if (listBox.SelectedItem != null)
							{
								value = ((GridValueListItem)listBox.SelectedItem).Value;
							}
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
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
