using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Scheduler.Design
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
			if ((int)value == -1)
			{
				return "(none)";
			}
			if ((int)value == -2)
			{
				if (context.PropertyDescriptor.Name == "RecurrenceImageIndex")
				{
					return "(Default)";
				}
				if (context.PropertyDescriptor.Name == "ReminderImageIndex")
				{
					return "(Default)";
				}
				if (context.PropertyDescriptor.Name == "RecurrenceExceptionImageIndex")
				{
					return "(Default)";
				}
				return "";
			}
			return "Image[" + (int)value + "]";
		}
	}
}
