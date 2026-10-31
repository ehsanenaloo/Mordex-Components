using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.TaskPane
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FlagsEnumEditor : UITypeEditor
	{
		private object _noneValue;

		private object _allValue;

		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void FillCheckedList(object flagsValue, CheckedListBox checkedListBox)
		{
			Type type = flagsValue.GetType();
			Array values = Enum.GetValues(type);
			for (int i = 0; i < values.Length; i++)
			{
				checkedListBox.Items.Add(values.GetValue(i));
				if ((int)values.GetValue(i) != 0)
				{
					if (((int)flagsValue & (int)values.GetValue(i)) == (int)values.GetValue(i))
					{
						checkedListBox.SetItemChecked(i, value: true);
					}
				}
				else if ((int)flagsValue == 0)
				{
					checkedListBox.SetItemChecked(i, value: true);
				}
			}
			checkedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					if (value.GetType() == typeof(OwnerDrawnArea))
					{
						_noneValue = OwnerDrawnArea.None;
						_allValue = OwnerDrawnArea.All;
					}
					CheckedListBox checkedListBox = new CheckedListBox();
					checkedListBox.CheckOnClick = true;
					checkedListBox.ItemCheck += OnControlItemCheck;
					FillCheckedList(value, checkedListBox);
					_iWindowsFormsEditorService.DropDownControl(checkedListBox);
					if (value.GetType() == typeof(OwnerDrawnArea))
					{
						OwnerDrawnArea ownerDrawnArea = OwnerDrawnArea.None;
						for (int j = 0; j < checkedListBox.CheckedItems.Count; j++)
						{
							ownerDrawnArea |= (OwnerDrawnArea)checkedListBox.CheckedItems[j];
						}
						value = ownerDrawnArea;
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

		private void OnControlItemCheck(object sender, ItemCheckEventArgs eventArgs)
		{
			CheckedListBox checkedListBox = (CheckedListBox)sender;
			object item = checkedListBox.Items[eventArgs.Index];
			if (TaskPanePaintHelper.CompareValues(_allValue, item, null) == 0)
			{
				if (eventArgs.NewValue != CheckState.Checked)
				{
					return;
				}
				for (int i = 0; i < checkedListBox.Items.Count; i++)
				{
					if (TaskPanePaintHelper.CompareValues(checkedListBox.Items[i], _noneValue, null) == 0)
					{
						checkedListBox.SetItemChecked(i, value: false);
					}
					else if (TaskPanePaintHelper.CompareValues(checkedListBox.Items[i], _allValue, null) != 0)
					{
						checkedListBox.SetItemChecked(i, value: true);
					}
				}
			}
			else if (TaskPanePaintHelper.CompareValues(item, _noneValue, null) == 0)
			{
				if (eventArgs.NewValue != CheckState.Checked)
				{
					return;
				}
				for (int j = 0; j < checkedListBox.Items.Count; j++)
				{
					if (TaskPanePaintHelper.CompareValues(checkedListBox.Items[j], _noneValue, null) != 0)
					{
						checkedListBox.SetItemChecked(j, value: false);
					}
				}
			}
			else
			{
				checkedListBox.SetItemChecked(0, value: false);
				checkedListBox.SetItemChecked(checkedListBox.Items.Count - 1, value: false);
			}
		}
	}
}
