using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class AssociatedCommandEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void FillCommandList(MxContextMenu uiContextMenu, string strA, ListBox listBox)
		{
			int selectedIndex = -1;
			listBox.Items.Add(new ON("", "(none)"));
			if (uiContextMenu != null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (MxCommand command in uiContextMenu.Commands)
				{
					listBox.Items.Add(new ON(command.Key, command.Key));
					foreach (MxCommand matchCommand in uiContextMenu.Commands)
					{
						if (string.Compare(strA, matchCommand.Key, ignoreCase: true, invariantCulture) == 0)
						{
							selectedIndex = listBox.Items.Count - 1;
						}
					}
				}
			}
			if (selectedIndex == -1)
			{
				listBox.SelectedIndex = 0;
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
					MxContextMenu uiContextMenu = null;
					object propertyOwner = null;
					propertyOwner = ((!(context.Instance is ICustomTypeDescriptor)) ? context.Instance : ((ICustomTypeDescriptor)context.Instance).GetPropertyOwner(context.PropertyDescriptor));
					if (propertyOwner is MxButton)
					{
						uiContextMenu = ((MxButton)propertyOwner).DropDownContextMenu;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillCommandList(uiContextMenu, (string)value, listBox);
					_editorService.DropDownControl(listBox);
					if (listBox.SelectedItem != null)
					{
						value = ((ON)listBox.SelectedItem).Object;
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

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_editorService != null)
			{
				_editorService.CloseDropDown();
			}
		}
	}
}
