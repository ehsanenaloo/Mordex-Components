using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class EditBoxImageIndexConverter : TypeConverter
	{
		public EditBoxImageIndexConverter()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

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
			if (context.PropertyDescriptor.PropertyType == typeof(int))
			{
				if ((int)value == -1)
				{
					return "(none)";
				}
				return "Image[" + (int)value + "]";
			}
			return value.ToString();
		}
	}
}
