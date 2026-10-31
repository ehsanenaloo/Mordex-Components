using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Mordex.Windows.TaskPane.Design
{
	public class GroupEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					GroupsDesigner groupsDesigner = new GroupsDesigner();
					groupsDesigner.TaskPane = (MxTaskPane)context.Instance;
					_iWindowsFormsEditorService.ShowDialog(groupsDesigner);
					IComponentChangeService componentChangeService = (IComponentChangeService)provider.GetService(typeof(IComponentChangeService));
					if (componentChangeService != null)
					{
						PropertyDescriptor member = TypeDescriptor.GetProperties(this)["Groups"];
						componentChangeService.OnComponentChanging(context.Instance, member);
						componentChangeService.OnComponentChanged(context.Instance, member, null, ((MxTaskPane)context.Instance).Groups);
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
