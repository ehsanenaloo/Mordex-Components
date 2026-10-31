using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ValueListEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		public ValueListEditor()
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
					// Several selected controls arrive as an array; the value list of one control cannot stand for all of them.
					if (!(context.Instance is MxValueListUpDown valueListUpDown) || !context.OnComponentChanging())
					{
						return value;
					}
					using (Form dialog = DesignHelper.CreateValueListDesigner(valueListUpDown.ValueList, null, valueListUpDown.ImageList))
					{
						if (edSvc.ShowDialog(dialog) == DialogResult.OK)
						{
							context.OnComponentChanged();
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
