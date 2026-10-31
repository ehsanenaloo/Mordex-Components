using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.TaskPane
{
	public class TaskPaneLayoutConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}
			return false;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor) && value != null)
			{
				ConstructorInfo constructor = value.GetType().GetConstructor(new Type[0]);
				return new InstanceDescriptor(constructor, new object[0], isComplete: false);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
