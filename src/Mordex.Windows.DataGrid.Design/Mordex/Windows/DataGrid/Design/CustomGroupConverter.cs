using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class CustomGroupConverter : TypeConverter
	{
		public CustomGroupConverter()
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
				return "(none)";
			}
			GridCustomGroup gridCustomGroup = (GridCustomGroup)value;
			return "[" + gridCustomGroup.Index + "] - '" + gridCustomGroup.Key + "'";
		}
	}
}
