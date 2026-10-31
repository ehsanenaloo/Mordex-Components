using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class TableFilterConverter : TypeConverter
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
			if (value is GridFilterCondition)
			{
				return ((GridFilterCondition)value).ToString();
			}
			if (value is GridStoredFilter)
			{
				GridStoredFilter gridStoredFilter = (GridStoredFilter)value;
				if (gridStoredFilter.Table != null)
				{
					return "[" + gridStoredFilter.Table.StoredFilters.IndexOf(gridStoredFilter) + "] - '" + gridStoredFilter.Key + "'";
				}
				return "{GridStoredFilter '" + gridStoredFilter.Key + "'}";
			}
			return "(none)";
		}
	}
}
