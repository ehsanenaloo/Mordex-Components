using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ControlEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void AddChildControls(Control selectedControl, Control parentControl, ListBox listBox, Control excludedControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				if (control.GetType() == typeof(MxCommandBarHost) || control.GetType() == typeof(MxCommandBar) || control == excludedControl)
				{
					continue;
				}
				string name = control.Name;
				if (name != null && name.Length > 0)
				{
					listBox.Items.Add(new ON(control, name));
					if (selectedControl == control)
					{
						listBox.SelectedIndex = listBox.Items.Count - 1;
					}
					AddChildControls(selectedControl, control, listBox, excludedControl);
				}
			}
		}

		private void FillForCommand(MxCommand uiCommand, ListBox listBox)
		{
			listBox.Items.Add(new ON(null, "(none)"));
			if (uiCommand.CommandManager != null)
			{
				Control control = uiCommand.CommandManager.GetForm();
				if (control == null)
				{
					control = uiCommand.CommandManager.ContainerControl;
				}
				AddChildControls(uiCommand.Control, control, listBox, null);
				if (uiCommand.Control == null)
				{
					listBox.SelectedIndex = 0;
				}
				else if (listBox.SelectedItem == null)
				{
					string name = uiCommand.Control.Name;
					if (name != null && name.Length > 0)
					{
						listBox.Items.Add(new ON(uiCommand.Control, name));
						listBox.SelectedIndex = listBox.Items.Count - 1;
					}
				}
			}
			listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		private void FillForScrollPanel(MxScrollPanel uiPager, ListBox listBox)
		{
			listBox.Items.Add(new ON(null, "(none)"));
			Control control = uiPager.FindForm();
			if (control == null)
			{
				control = RenderingHelper.GetParentControl(uiPager);
			}
			if (uiPager.FindForm() != null)
			{
				AddChildControls(uiPager.ChildControl, control, listBox, uiPager);
				if (uiPager.ChildControl == null)
				{
					listBox.SelectedIndex = 0;
				}
				else if (listBox.SelectedItem == null)
				{
					string name = uiPager.ChildControl.Name;
					if (name != null && name.Length > 0)
					{
						listBox.Items.Add(new ON(uiPager.ChildControl, name));
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
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					if (context.Instance is MxCommand)
					{
						MxCommand uiCommand = (MxCommand)context.Instance;
						if (uiCommand.CommandType != CommandType.ControlContainer && uiCommand.CommandType != CommandType.ControlPopup)
						{
							return null;
						}
						FillForCommand(uiCommand, listBox);
					}
					else
					{
						if (!(context.Instance is MxScrollPanel))
						{
							throw new InvalidOperationException();
						}
						MxScrollPanel uiPager = (MxScrollPanel)context.Instance;
						FillForScrollPanel(uiPager, listBox);
					}
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
