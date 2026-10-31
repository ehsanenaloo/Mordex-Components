using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Scheduler.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class DateTimeConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
		{
			if (type == typeof(DateTime) || type == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, type);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value.GetType() == typeof(DateTime))
			{
				return ((DateTime)value).ToShortDateString();
			}
			if (value.GetType() == typeof(string))
			{
				string text = (string)value;
				return DateTime.Parse(text, culture.DateTimeFormat);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
