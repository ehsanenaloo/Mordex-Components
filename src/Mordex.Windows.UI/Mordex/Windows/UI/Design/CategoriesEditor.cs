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
	public class CategoriesEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		private void FillCategoryList(MxCommandManager uiCommandManager, CommandCategory selectedCategory, ListBox listBox)
		{
			listBox.ItemHeight = (int)listBox.Font.GetHeight() + 4;
			int selectedIndex = -1;
			listBox.Items.Add(new ON(null, "(none)"));
			if (uiCommandManager != null)
			{
				for (int i = 0; i < uiCommandManager.Categories.Count; i++)
				{
					CommandCategory uiCommandCategory = uiCommandManager.Categories[i];
					listBox.Items.Add(new ON(uiCommandCategory, uiCommandCategory.CategoryName));
					if (selectedCategory == uiCommandCategory)
					{
						selectedIndex = listBox.Items.Count - 1;
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
					MxCommandManager uiCommandManager = null;
					if (!(context.Instance is MxCommand))
					{
						return value;
					}
					uiCommandManager = ((MxCommand)context.Instance).CommandManager;
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillCategoryList(uiCommandManager, (CommandCategory)value, listBox);
					_editorService.DropDownControl(listBox);
					if (listBox.SelectedItem == null)
					{
						return null;
					}
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
