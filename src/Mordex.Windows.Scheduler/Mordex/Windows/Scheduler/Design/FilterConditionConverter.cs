using System;
using System.ComponentModel;
using System.Globalization;

namespace Mordex.Windows.Scheduler.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class FilterConditionConverter : TypeConverter
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
			SchedulerFilterCondition schedulerFilterCondition = (SchedulerFilterCondition)value;
			return schedulerFilterCondition.ToString();
		}
	}
}
