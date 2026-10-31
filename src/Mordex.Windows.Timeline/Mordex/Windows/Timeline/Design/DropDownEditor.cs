using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Timeline.Design
{
	public class DropDownEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					DropDownDesigner dropDownDesigner = new DropDownDesigner();
					dropDownDesigner.Calendar = ((MxTimeline)context.Instance).DropDownCalendar;
					_iWindowsFormsEditorService.ShowDialog(dropDownDesigner);
					IComponentChangeService componentChangeService = (IComponentChangeService)provider.GetService(typeof(IComponentChangeService));
					if (componentChangeService != null)
					{
						PropertyDescriptor member = TypeDescriptor.GetProperties(this)["DropDownCalendar"];
						componentChangeService.OnComponentChanging(context.Instance, member);
						componentChangeService.OnComponentChanged(context.Instance, member, null, ((MxTimeline)context.Instance).DropDownCalendar);
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
