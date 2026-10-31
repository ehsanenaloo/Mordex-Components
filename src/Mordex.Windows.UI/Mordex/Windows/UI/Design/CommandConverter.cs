using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.UI.Design
{
	public class CommandConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				string text = (string)value.GetType().GetProperty("Key").GetValue(value, new object[0]);
				ConstructorInfo constructor = value.GetType().GetConstructor(new Type[1] { typeof(string) });
				return new InstanceDescriptor(constructor, new object[1] { text }, isComplete: false);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
