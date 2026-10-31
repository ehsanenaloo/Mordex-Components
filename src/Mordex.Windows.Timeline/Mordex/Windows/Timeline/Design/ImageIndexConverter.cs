using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Timeline.Design
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
			if ((int)value == -1)
			{
				return "(none)";
			}
			return "Image[" + (int)value + "]";
		}
	}
}
