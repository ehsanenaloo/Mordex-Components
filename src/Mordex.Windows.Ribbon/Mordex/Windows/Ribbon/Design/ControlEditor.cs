using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ControlEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void AddChildControls(Control selectedControl, Control parentControl, ListBox listBox, Control excludedControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				if (control == excludedControl)
				{
					continue;
				}
				string name = control.Name;
				if (name != null && name.Length > 0)
				{
					listBox.Items.Add(new DictionaryEntry(name, control));
					if (selectedControl == control)
					{
						listBox.SelectedIndex = listBox.Items.Count - 1;
					}
					AddChildControls(selectedControl, control, listBox, excludedControl);
				}
			}
		}

		private void PopulateControlList(ContainerControlCommand containerControlCommand, ListBox listBox)
		{
			listBox.Items.Add(new DictionaryEntry("(none)", null));
			Control control = null;
			if (containerControlCommand.Ribbon != null)
			{
				control = containerControlCommand.Ribbon.Parent;
			}
			else if (containerControlCommand.GetOwningStatusBar() != null)
			{
				control = containerControlCommand.GetOwningStatusBar().FindForm();
				if (control == null)
				{
					control = containerControlCommand.GetOwningStatusBar().Parent;
				}
			}
			else if (containerControlCommand.CommandsOwner != null && containerControlCommand.CommandsOwner._parent is MxRibbonContextMenu)
			{
				Form form = RibbonDrawingUtils.FindParentForm(((MxRibbonContextMenu)containerControlCommand.CommandsOwner._parent).SourceControl);
				control = form;
			}
			if (control != null)
			{
				AddChildControls(containerControlCommand.Control, control, listBox, null);
				if (containerControlCommand.Control == null)
				{
					listBox.SelectedIndex = 0;
				}
				else if (listBox.SelectedItem == null)
				{
					string name = containerControlCommand.Control.Name;
					if (name != null && name.Length > 0)
					{
						listBox.Items.Add(new DictionaryEntry(name, containerControlCommand.Control));
						listBox.SelectedIndex = listBox.Items.Count - 1;
					}
				}
			}
			listBox.BorderStyle = BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
				if (_editorService != null)
				{
					if (!(context.Instance is ContainerControlCommand containerControlCommand))
					{
						throw new InvalidOperationException();
					}
					using ListBox listBox = new ListBox();
					listBox.DisplayMember = "Key";
					listBox.ValueMember = "Value";
					listBox.MouseUp += OnControlMouseUp;
					PopulateControlList(containerControlCommand, listBox);
					_editorService.DropDownControl(listBox);
					if (listBox.SelectedItem != null)
					{
						value = ((DictionaryEntry)listBox.SelectedItem).Value;
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
