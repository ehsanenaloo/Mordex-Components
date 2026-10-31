using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class StatusControlEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void AddChildControls(Control selectedControl, Control parentControl, ListBox listBox)
		{
			foreach (Control control in parentControl.Controls)
			{
				listBox.Items.Add(new ON(control, control.Name));
				if (selectedControl == control)
				{
					listBox.SelectedIndex = listBox.Items.Count - 1;
				}
				AddChildControls(selectedControl, control, listBox);
			}
		}

		private void FillControlList(MxStatusBarPanel uiStatusBarPanel, ListBox listBox)
		{
			listBox.Items.Add(new ON(null, "(none)"));
			Control control = (uiStatusBarPanel._owner == null) ? null : RenderingHelper.GetParentControl(uiStatusBarPanel._owner._statusBar);
			if (uiStatusBarPanel._owner != null && control != null)
			{
				AddChildControls(uiStatusBarPanel.Control, control, listBox);
				if (uiStatusBarPanel.Control == null)
				{
					listBox.SelectedIndex = 0;
				}
				else if (listBox.SelectedItem == null)
				{
					string name = uiStatusBarPanel.Control.Name;
					if (name != null && name.Length > 0)
					{
						listBox.Items.Add(new ON(uiStatusBarPanel.Control, name));
						listBox.SelectedIndex = listBox.Items.Count - 1;
					}
				}
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
					MxStatusBarPanel uiStatusBarPanel = (MxStatusBarPanel)context.Instance;
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillControlList(uiStatusBarPanel, listBox);
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
