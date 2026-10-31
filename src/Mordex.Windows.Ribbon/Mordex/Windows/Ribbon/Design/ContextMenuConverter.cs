using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Ribbon.Design
{
	public class ContextMenuConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value == null)
			{
				return "(none)";
			}
			if (value.GetType() == typeof(MxRibbonContextMenu))
			{
				MxRibbonContextMenu ribbonContextMenu = (MxRibbonContextMenu)value;
				return ribbonContextMenu.Key;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
