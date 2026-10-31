using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class CompositeColumnsEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public CompositeColumnsEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null && context.Instance is GridCustomGroup gridCustomGroup)
				{
					CompositeColumnsDialog compositeColumnsDialog = new CompositeColumnsDialog();
					compositeColumnsDialog.TargetTable = DesignHelper.GetDesignModeTable(gridCustomGroup);
					compositeColumnsDialog.Columns = gridCustomGroup.CompositeColumns;
					if (compositeColumnsDialog.ShowDialog() == DialogResult.OK)
					{
						value = compositeColumnsDialog.Columns;
						if (value != null)
						{
							gridCustomGroup.CustomGroupType = CustomGroupType.CompositeColumns;
						}
					}
					compositeColumnsDialog.Dispose();
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
