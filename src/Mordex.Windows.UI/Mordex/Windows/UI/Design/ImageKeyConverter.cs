using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.UI.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ImageKeyConverter : TypeConverter
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
				if ((string)value == "")
				{
					return "(none)";
				}
				return (string)value;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
