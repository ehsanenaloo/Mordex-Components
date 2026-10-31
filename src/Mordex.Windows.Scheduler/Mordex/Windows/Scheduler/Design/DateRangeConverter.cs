using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.Scheduler.Design
{
	public class DateRangeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				string text = (string)value;
				if (text.Length == 0)
				{
					return null;
				}
				char c = culture.TextInfo.ListSeparator[0];
				char[] separator = new char[1] { c };
				string[] dateTexts = text.Split(separator);
				DateTime[] dates = new DateTime[dateTexts.Length];
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(DateTime));
				for (int i = 0; i < dates.Length; i++)
				{
					ref DateTime reference = ref dates[i];
					reference = (DateTime)converter.ConvertFromString(context, culture, dateTexts[i]);
				}
				if (dates.Length == 2)
				{
					return new DateRange(dates[0], dates[1]);
				}
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
			{
				throw new ArgumentNullException("destinationType");
			}
			if (destinationType == typeof(string) && value is DateRange dateRange)
			{
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				string separator = culture.TextInfo.ListSeparator + " ";
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(DateTime));
				string[] dateTexts = new string[2];
				int textIndex = 0;
				dateTexts[textIndex++] = converter.ConvertToString(context, culture, dateRange.Start);
				dateTexts[textIndex++] = converter.ConvertToString(context, culture, dateRange.End);
				return string.Join(separator, dateTexts);
			}
			if (destinationType == typeof(InstanceDescriptor) && value is DateRange descriptorRange)
			{
				Type[] types = new Type[2]
				{
					typeof(DateTime),
					typeof(DateTime)
				};
				ConstructorInfo constructor = typeof(DateRange).GetConstructor(types);
				if (constructor != null)
				{
					object[] arguments = new object[2] { descriptorRange.Start, descriptorRange.End };
					return new InstanceDescriptor(constructor, arguments);
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
		{
			return new DateRange((DateTime)propertyValues["Start"], (DateTime)propertyValues["End"]);
		}

		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(DateRange), attributes);
			string[] names = new string[2]
			{
				"Start",
				"End"
			};
			return properties.Sort(names);
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
