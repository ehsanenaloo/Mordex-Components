using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ContextMenuEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void FillContextMenuList(MxCommandManager uiCommandManager, ListBox listBox, Component component)
		{
			listBox.Items.Add(new ON(null, "(none)"));
			if (uiCommandManager != null && uiCommandManager.ContextMenus.Count > 0)
			{
				int selectedIndex = -1;
				MxContextMenu contextMenu = uiCommandManager.GetContextMenu(component);
				string text = null;
				if (contextMenu != null && contextMenu.Key.Length > 0)
				{
					text = contextMenu.Key;
				}
				foreach (MxContextMenu candidateMenu in uiCommandManager.ContextMenus)
				{
					if (candidateMenu.Key.Length > 0)
					{
						listBox.Items.Add(new ON(candidateMenu, candidateMenu.Key));
						if (text != null && string.Compare(text, candidateMenu.Key) == 0)
						{
							selectedIndex = listBox.Items.Count - 1;
						}
					}
				}
				if (selectedIndex != -1)
				{
					listBox.SelectedIndex = selectedIndex;
				}
				else
				{
					listBox.SelectedIndex = 0;
				}
			}
			listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.PropertyDescriptor != null && context.Container != null)
			{
				_editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_editorService != null)
				{
					string displayName = context.PropertyDescriptor.DisplayName;
					char[] separators = new char[1];
					separators.SetValue(' ', 0);
					int lastSeparatorIndex = displayName.LastIndexOfAny(separators);
					displayName = displayName.Substring(lastSeparatorIndex + 1);
					bool isDropDownMenuProperty = false;
					if (context.PropertyDescriptor.DisplayName == "DropDownContextMenu")
					{
						isDropDownMenuProperty = true;
					}
					foreach (IComponent component in context.Container.Components)
					{
						MxCommandManager uiCommandManager = null;
						if (component == null || !(component is MxCommandManager))
						{
							continue;
						}
						uiCommandManager = (MxCommandManager)component;
						if (uiCommandManager != null && (isDropDownMenuProperty || string.Compare(uiCommandManager.Site.Name, displayName) == 0))
						{
							ListBox listBox = new ListBox();
							listBox.MouseUp += OnControlMouseUp;
							FillContextMenuList(uiCommandManager, listBox, (Component)context.Instance);
							_editorService.DropDownControl(listBox);
							if (listBox.SelectedItem != null)
							{
								if (listBox.SelectedItem != null)
								{
									value = ((ON)listBox.SelectedItem).Object;
								}
							}
						}
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
