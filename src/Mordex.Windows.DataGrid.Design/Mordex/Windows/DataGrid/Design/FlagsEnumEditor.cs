using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class FlagsEnumEditor : UITypeEditor
	{
		private object noneValue;

		private object allValue;

		private IWindowsFormsEditorService edSvc;

		private void FillFlagsList(object value, CheckedListBox editor)
		{
			Type type = value.GetType();
			Array values = Enum.GetValues(type);
			for (int i = 0; i < values.Length; i++)
			{
				editor.Items.Add(values.GetValue(i));
				if ((int)values.GetValue(i) != 0)
				{
					if (((int)value & (int)values.GetValue(i)) == (int)values.GetValue(i))
					{
						editor.SetItemChecked(i, value: true);
					}
				}
				else if ((int)value == 0)
				{
					editor.SetItemChecked(i, value: true);
				}
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
					if (value.GetType() == typeof(ColumnOwnerDrawnMode))
					{
						noneValue = ColumnOwnerDrawnMode.None;
						allValue = ColumnOwnerDrawnMode.All;
					}
					else if (value.GetType() == typeof(GridOwnerDrawnArea))
					{
						noneValue = GridOwnerDrawnArea.None;
						allValue = null;
					}
					CheckedListBox checkedListBox = new CheckedListBox();
					checkedListBox.CheckOnClick = true;
					FillFlagsList(value, checkedListBox);
					checkedListBox.ItemCheck += listBox_ItemCheck;
					edSvc.DropDownControl(checkedListBox);
					if (value.GetType() == typeof(ColumnOwnerDrawnMode))
					{
						ColumnOwnerDrawnMode columnOwnerDrawnMode = ColumnOwnerDrawnMode.None;
						for (int i = 0; i < checkedListBox.CheckedItems.Count; i++)
						{
							columnOwnerDrawnMode |= (ColumnOwnerDrawnMode)checkedListBox.CheckedItems[i];
						}
						value = columnOwnerDrawnMode;
					}
					else if (value.GetType() == typeof(GridOwnerDrawnArea))
					{
						GridOwnerDrawnArea gridOwnerDrawnArea = GridOwnerDrawnArea.None;
						for (int j = 0; j < checkedListBox.CheckedItems.Count; j++)
						{
							gridOwnerDrawnArea |= (GridOwnerDrawnArea)checkedListBox.CheckedItems[j];
						}
						value = gridOwnerDrawnArea;
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

		private bool AreValuesEqual(object item, object otherItem)
		{
			if (item == null)
			{
				if (otherItem == null)
				{
					return true;
				}
				return false;
			}
			if (otherItem == null)
			{
				return false;
			}
			if (otherItem == item)
			{
				return true;
			}
			if (item is IComparable comparable)
			{
				try
				{
					if (comparable.CompareTo(otherItem) == 0)
					{
						return true;
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
			return false;
		}

		private void listBox_ItemCheck(object sender, ItemCheckEventArgs eventArgs)
		{
			CheckedListBox checkedListBox = (CheckedListBox)sender;
			object item = checkedListBox.Items[eventArgs.Index];
			if (AreValuesEqual(allValue, item))
			{
				if (eventArgs.NewValue != CheckState.Checked)
				{
					return;
				}
				for (int i = 0; i < checkedListBox.Items.Count; i++)
				{
					if (AreValuesEqual(checkedListBox.Items[i], noneValue))
					{
						checkedListBox.SetItemChecked(i, value: false);
					}
					else if (AreValuesEqual(checkedListBox.Items[i], allValue))
					{
						checkedListBox.SetItemChecked(i, value: true);
					}
				}
			}
			else if (AreValuesEqual(item, noneValue))
			{
				if (eventArgs.NewValue != CheckState.Checked)
				{
					return;
				}
				for (int j = 0; j < checkedListBox.Items.Count; j++)
				{
					if (AreValuesEqual(checkedListBox.Items[j], noneValue))
					{
						checkedListBox.SetItemChecked(j, value: false);
					}
				}
			}
			else
			{
				if (eventArgs.NewValue != CheckState.Unchecked)
				{
					return;
				}
				for (int k = 0; k < checkedListBox.Items.Count; k++)
				{
					if (AreValuesEqual(checkedListBox.Items[k], allValue))
					{
						checkedListBox.SetItemChecked(k, value: false);
					}
				}
			}
		}
	}
}
