using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.Common.Layouts
{
	public class LayoutReferenceConverter : TypeConverter
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
				LayoutReference mordexLayoutReference = (LayoutReference)value;
				ConstructorInfo constructor = typeof(LayoutReference).GetConstructor(new Type[1] { typeof(string) });
				return new InstanceDescriptor(constructor, new object[1] { mordexLayoutReference.ReferenceId }, isComplete: false);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
