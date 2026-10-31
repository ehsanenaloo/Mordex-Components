using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.Navigation
{
	public class NavigationGroupConverter : TypeConverter
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
				ConstructorInfo constructor = typeof(NavigationGroup).GetConstructor(new Type[0]);
				return new InstanceDescriptor(constructor, new object[0], isComplete: false);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
