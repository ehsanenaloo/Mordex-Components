using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ObjectTypeConverter : TypeConverter
	{
		public ObjectTypeConverter()
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
			if (value == null)
			{
				return "{Null}";
			}
			if (value == DBNull.Value)
			{
				return "(DBNull)";
			}
			if (value is string && ((string)value).Length == 0)
			{
				return "{Empty String}";
			}
			return value.ToString();
		}
	}
}
