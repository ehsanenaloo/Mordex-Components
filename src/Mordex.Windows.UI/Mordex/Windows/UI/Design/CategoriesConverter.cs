using System;
using System.ComponentModel;
using System.Globalization;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class CategoriesConverter : TypeConverter
	{
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
			if (value.GetType() == typeof(CommandCategory))
			{
				CommandCategory uiCommandCategory = (CommandCategory)value;
				return uiCommandCategory.CategoryName;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
