using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Timeline.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FlagsEnumEditor : UITypeEditor
	{
		private object _value;

		private object _allValue;

		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private void FillFlagsList(object value, CheckedListBox checkedListBox)
		{
			try
			{
				Type type = value.GetType();
				Array values = Enum.GetValues(type);
				for (int i = 0; i < values.Length; i++)
				{
					checkedListBox.Items.Add(values.GetValue(i));
					if ((int)values.GetValue(i) != 0)
					{
						if (((int)value & (int)values.GetValue(i)) == (int)values.GetValue(i))
						{
							checkedListBox.SetItemChecked(i, value: true);
						}
					}
					else if ((int)value == 0)
					{
						checkedListBox.SetItemChecked(i, value: true);
					}
				}
				checkedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			try
			{
				if (context != null && context.Instance != null && provider != null)
				{
					_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
					if (_iWindowsFormsEditorService != null)
					{
						if (value.GetType() == typeof(OwnerDrawnArea))
						{
							_value = OwnerDrawnArea.None;
							_allValue = OwnerDrawnArea.All;
						}
						else if (value.GetType() == typeof(AllowItemDrag))
						{
							_value = AllowItemDrag.None;
							_allValue = AllowItemDrag.All;
						}
						CheckedListBox checkedListBox = new CheckedListBox();
						checkedListBox.CheckOnClick = true;
						checkedListBox.ItemCheck += OnControlItemCheck;
						FillFlagsList(value, checkedListBox);
						_iWindowsFormsEditorService.DropDownControl(checkedListBox);
						if (value.GetType() == typeof(DayOfWeek))
						{
							DayOfWeek dayOfWeek = (DayOfWeek)0;
							if (checkedListBox.CheckedItems.Count == 0)
							{
								dayOfWeek = DayOfWeek.Monday;
							}
							for (int i = 0; i < checkedListBox.CheckedItems.Count; i++)
							{
								dayOfWeek |= (DayOfWeek)checkedListBox.CheckedItems[i];
							}
							value = dayOfWeek;
						}
						else if (value.GetType() == typeof(AllowItemDrag))
						{
							AllowItemDrag allowItemDrag = AllowItemDrag.None;
							for (int k = 0; k < checkedListBox.CheckedItems.Count; k++)
							{
								allowItemDrag |= (AllowItemDrag)checkedListBox.CheckedItems[k];
							}
							value = allowItemDrag;
						}
						else if (value.GetType() == typeof(OwnerDrawnArea))
						{
							OwnerDrawnArea ownerDrawnArea = OwnerDrawnArea.None;
							for (int l = 0; l < checkedListBox.CheckedItems.Count; l++)
							{
								ownerDrawnArea |= (OwnerDrawnArea)checkedListBox.CheckedItems[l];
							}
							value = ownerDrawnArea;
						}
					}
				}
				return value;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return null;
			}
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
			if (item.GetType() != typeof(AllowItemDrag) && item.GetType() != typeof(OwnerDrawnArea))
			{
				return;
			}
			if (TimelineUtilities.CompareValuesIgnoreCase(_allValue, item, null) == 0)
			{
				if (eventArgs.NewValue != CheckState.Checked)
				{
					return;
				}
				for (int i = 0; i < checkedListBox.Items.Count; i++)
				{
					if (TimelineUtilities.CompareValuesIgnoreCase(checkedListBox.Items[i], _value, null) == 0)
					{
						checkedListBox.SetItemChecked(i, value: false);
					}
					else if (TimelineUtilities.CompareValuesIgnoreCase(checkedListBox.Items[i], _allValue, null) != 0)
					{
						checkedListBox.SetItemChecked(i, value: true);
					}
				}
			}
			else if (TimelineUtilities.CompareValuesIgnoreCase(item, _value, null) == 0)
			{
				if (eventArgs.NewValue != CheckState.Checked)
				{
					return;
				}
				for (int j = 0; j < checkedListBox.Items.Count; j++)
				{
					if (TimelineUtilities.CompareValuesIgnoreCase(checkedListBox.Items[j], _value, null) != 0)
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
