using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	public class TabPagesEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_editorService != null)
				{
					IDesignerHost designerHost = (IDesignerHost)provider.GetService(typeof(IDesignerHost));
					TabPagesDesignerDialog dialog = new TabPagesDesignerDialog((MxTabControl)context.Instance);
					if (_editorService.ShowDialog(dialog) == DialogResult.OK)
					{
						context.OnComponentChanging();
						context.OnComponentChanged();
					}
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}
}
