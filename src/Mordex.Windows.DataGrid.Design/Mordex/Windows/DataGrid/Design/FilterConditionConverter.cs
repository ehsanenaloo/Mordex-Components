using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FilterConditionConverter : TypeConverter
	{
		public FilterConditionConverter()
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
			GridFilterCondition gridFilterCondition = (GridFilterCondition)value;
			return gridFilterCondition.ToString();
		}
	}
}
