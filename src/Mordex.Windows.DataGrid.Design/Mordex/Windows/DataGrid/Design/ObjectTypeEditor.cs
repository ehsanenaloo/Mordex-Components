using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	public class ObjectTypeEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public ObjectTypeEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null)
				{
					ObjectTypeDialog objectTypeDialog = new ObjectTypeDialog();
					objectTypeDialog.Value = value;
					if (edSvc.ShowDialog(objectTypeDialog) == DialogResult.OK)
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
