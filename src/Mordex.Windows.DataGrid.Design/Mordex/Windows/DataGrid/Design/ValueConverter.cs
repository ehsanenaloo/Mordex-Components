using System;
using System.ComponentModel;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid.Design
{
	public class ValueConverter : TypeConverter
	{
		public ValueConverter()
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
				return "";
			}
			return value.ToString();
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string && (string)value == "")
			{
				if (context != null && context.PropertyDescriptor != null)
				{
					if (context.PropertyDescriptor.Name == "CheckBoxTrueValue")
					{
						return true;
					}
					if (context.PropertyDescriptor.Name == "CheckBoxFalseValue")
					{
						return false;
					}
					return null;
				}
				return null;
			}
			if (value is string)
			{
				string text = '"'.ToString();
				text += '"';
				if ((string)value == text)
				{
					value = "";
				}
				if (context != null && context.Instance is GridColumn)
				{
					GridColumn gridColumn = (GridColumn)context.Instance;
					if (gridColumn.Type != null && gridColumn.Type != typeof(object))
					{
						return PropertyValue.ConvertValue((string)value, gridColumn.Type);
					}
				}
			}
			return value;
		}
	}
}
