using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ImageIndexConverter : TypeConverter
	{
		public ImageIndexConverter()
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
			if (value is int)
			{
				if ((int)value == -1)
				{
					return "(none)";
				}
				if ((int)value == -2)
				{
					if (context.PropertyDescriptor.Name == "ErrorImageIndex")
					{
						return "(Default)";
					}
					return "";
				}
				return "Image[" + (int)value + "]";
			}
			if (value is string)
			{
				if (((string)value).Length == 0)
				{
					return "(none)";
				}
				return value;
			}
			return value.ToString();
		}
	}
}
