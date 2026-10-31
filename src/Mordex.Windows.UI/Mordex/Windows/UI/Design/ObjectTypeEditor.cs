using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.UI.Design
{
	public class ObjectTypeEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _editorService;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_editorService != null)
				{
					ObjectTypeDialog objectTypeDialog = new ObjectTypeDialog();
					objectTypeDialog.Value = value;
					if (_editorService.ShowDialog(objectTypeDialog) == DialogResult.OK)
					{
						value = objectTypeDialog.Value;
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
