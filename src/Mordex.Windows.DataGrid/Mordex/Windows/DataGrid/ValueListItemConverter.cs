using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace Mordex.Windows.DataGrid
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
				GridValueListItem gridValueListItem = (GridValueListItem)value;
				Type[] array = ((gridValueListItem.ImageIndex == -1) ? new Type[2] : new Type[3]);
				array[0] = typeof(object);
				array[1] = typeof(string);
				if (gridValueListItem.ImageIndex != -1)
				{
					array[2] = typeof(int);
				}
				ConstructorInfo constructor = typeof(GridValueListItem).GetConstructor(array);
				object[] arguments = ((gridValueListItem.ImageIndex == -1) ? new object[2] : new object[3]);
				arguments[0] = gridValueListItem.Value;
				arguments[1] = gridValueListItem.Text;
				if (gridValueListItem.ImageIndex != -1)
				{
					arguments[2] = gridValueListItem.ImageIndex;
				}
				return new InstanceDescriptor(constructor, arguments, gridValueListItem.Image == null);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
