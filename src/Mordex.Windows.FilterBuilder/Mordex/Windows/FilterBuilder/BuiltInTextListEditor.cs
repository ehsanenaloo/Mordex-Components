using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.FilterBuilder
{
	public class BuiltInTextListEditor : UITypeEditor
	{
		private const string BuiltInTextsDataProperty = "BuiltInTextsData";

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context == null || context.Instance == null || provider == null)
			{
				return value;
			}
			IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (editorService == null)
			{
				return value;
			}
			MxFilterBuilder filterBuilder = (MxFilterBuilder)context.Instance;
			DialogResult result;
			using (BuiltInTextEditor dialog = new BuiltInTextEditor(filterBuilder.BuiltInTextList))
			{
				result = editorService.ShowDialog(dialog);
			}
			if (result == DialogResult.OK)
			{
				IComponentChangeService changeService = (IComponentChangeService)provider.GetService(typeof(IComponentChangeService));
				if (changeService != null)
				{
					PropertyDescriptor member = TypeDescriptor.GetProperties(filterBuilder)[BuiltInTextsDataProperty];
					changeService.OnComponentChanging(filterBuilder, member);
					changeService.OnComponentChanged(filterBuilder, member, "", filterBuilder.BuiltInTextsData);
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
