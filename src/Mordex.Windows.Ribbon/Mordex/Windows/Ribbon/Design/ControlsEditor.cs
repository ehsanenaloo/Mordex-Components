using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Ribbon.Design
{
	public class ControlsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
				if (_editorService != null && context.Instance is RibbonContextualTab tab)
				{
					IDesignerHost designerHost = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;
					DesignerTransaction designerTransaction = designerHost?.CreateTransaction("Controls");
					bool accepted = false;
					try
					{
						using ControlsDesignerDialog dialog = new ControlsDesignerDialog(tab);
						accepted = _editorService.ShowDialog(dialog) == DialogResult.OK;
					}
					finally
					{
						if (accepted)
						{
							designerTransaction?.Commit();
						}
						else
						{
							designerTransaction?.Cancel();
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
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}
}
