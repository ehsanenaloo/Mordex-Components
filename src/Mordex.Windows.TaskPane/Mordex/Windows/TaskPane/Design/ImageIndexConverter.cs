using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.TaskPane.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
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
				if ((int)value == -1)
				{
					return "(none)";
				}
				return "Image[" + (int)value + "]";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
