using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ImageKeyEditor : UITypeEditor
	{
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
				if (service != null)
				{
					object owner = ImageListEditorSupport.GetPropertyOwner(context);
					if (!ImageListEditorSupport.IsSmallImageOwner(owner))
					{
						throw new InvalidOperationException();
					}
					value = ImageListEditorSupport.ShowDropDown(service, ImageListEditorSupport.GetSmallImageList(owner), value, true);
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.DropDown;
			}
			return base.GetEditStyle(context);
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return context != null && ImageListEditorSupport.IsSmallImageOwner(ImageListEditorSupport.GetPropertyOwner(context));
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			if (e.Context == null || e.Context.Instance == null)
			{
				return;
			}
			ImageListEditorSupport.PaintValue(e, ImageListEditorSupport.GetSmallImageList(ImageListEditorSupport.GetPropertyOwner(e.Context)), true);
		}
	}
}
