using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Design
{
	public class FormatStylePropertyEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public FormatStylePropertyEditor()
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
					object propertyOwner = context.Instance;
					if (propertyOwner is ICustomTypeDescriptor)
					{
						propertyOwner = ((ICustomTypeDescriptor)propertyOwner).GetPropertyOwner(context.PropertyDescriptor);
					}
					if (!(propertyOwner is IFormatStyleOwner) || ((IFormatStyleOwner)propertyOwner).DataGrid == null)
					{
						return value;
					}
					using (FormatStyleDialog formatStyleDialog = new FormatStyleDialog())
					{
						formatStyleDialog.CreateControl();
						formatStyleDialog.AvailableStyles = ((IFormatStyleOwner)propertyOwner).DataGrid.FormatStyles;
						formatStyleDialog.SelectedFormatStyle = value as GridFormatStyle;
						edSvc.ShowDialog(formatStyleDialog);
						if (formatStyleDialog.DialogResult == DialogResult.OK)
						{
							return formatStyleDialog.SelectedFormatStyle;
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
