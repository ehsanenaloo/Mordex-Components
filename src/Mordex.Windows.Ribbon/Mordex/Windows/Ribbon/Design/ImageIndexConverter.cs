using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Ribbon.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ImageIndexConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, type);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				if (!(value is int index) || index == -1)
				{
					return "(none)";
				}
				return "Image[" + index + "]";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
