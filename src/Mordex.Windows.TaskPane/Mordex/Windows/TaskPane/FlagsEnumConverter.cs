using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.TaskPane
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class FlagsEnumConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, type);
		}

		/// <summary>Shows a flags value as its name, or as the comma separated names of the set flags.</summary>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value == null)
			{
				return "";
			}
			return value.ToString();
		}
	}
}
