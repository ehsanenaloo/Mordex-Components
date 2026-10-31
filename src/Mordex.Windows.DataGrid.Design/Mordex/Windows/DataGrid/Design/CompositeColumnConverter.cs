using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class CompositeColumnConverter : TypeConverter
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
			string text = "";
			GridColumn[] array = value as GridColumn[];
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					text += " - ";
				}
				text += CompositeColumnsChooser.GetName(array[i].Index, array[i]);
			}
			return text;
		}
	}
}
