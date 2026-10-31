using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Scheduler.Design
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
			if (value is SchedulerFilterCondition)
			{
				return ((SchedulerFilterCondition)value).ToString();
			}
			if (value is SchedulerStoredFilter)
			{
				SchedulerStoredFilter schedulerStoredFilter = (SchedulerStoredFilter)value;
				return "[" + schedulerStoredFilter.GetSchedulerStoredFilterCollection().IndexOf(schedulerStoredFilter) + "] - '" + schedulerStoredFilter.Key + "'";
			}
			return "(none)";
		}
	}
}
