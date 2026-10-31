using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.Timeline
{
	public class ValueListItemConverter : TypeConverter
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
				TimelineValueListItem timelineValueListItem = (TimelineValueListItem)value;
				Type[] constructorTypes = ((timelineValueListItem.ImageIndex == -1) ? new Type[2] : new Type[3]);
				constructorTypes[0] = typeof(object);
				constructorTypes[1] = typeof(string);
				if (timelineValueListItem.ImageIndex != -1)
				{
					constructorTypes[2] = typeof(int);
				}
				ConstructorInfo constructor = typeof(TimelineValueListItem).GetConstructor(constructorTypes);
				object[] constructorArguments = ((timelineValueListItem.ImageIndex == -1) ? new object[2] : new object[3]);
				constructorArguments[0] = timelineValueListItem.Value;
				constructorArguments[1] = timelineValueListItem.Text;
				if (timelineValueListItem.ImageIndex != -1)
				{
					constructorArguments[2] = timelineValueListItem.ImageIndex;
				}
				return new InstanceDescriptor(constructor, constructorArguments, timelineValueListItem.Image == null);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
