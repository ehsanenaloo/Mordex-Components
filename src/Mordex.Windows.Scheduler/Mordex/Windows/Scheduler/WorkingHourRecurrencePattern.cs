using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class WorkingHourRecurrencePattern : RecurrencePatternBase
	{
		internal SchedulerHourRange _hourRange;

		public SchedulerHourRange HourRange => _hourRange;

		public static WorkingHourRecurrencePattern FromXMLString(string xml)
		{
			WorkingHourRecurrencePattern workingHourRecurrencePattern = new WorkingHourRecurrencePattern();
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xml);
			mordexLayoutReader["RecurrencePattern"]?.DeserializeObject(workingHourRecurrencePattern);
			return workingHourRecurrencePattern;
		}

		internal override void InvalidateOwner()
		{
			if (!_isEditing && HourRange != null)
			{
				HourRange.UpdateSchedulerHourRange();
			}
		}

		public SchedulerHourRange[] GetOccurrences(DateRange range)
		{
			if (range.Start.TimeOfDay != TimeSpan.Zero)
			{
				range.Start = range.Start.Date;
			}
			if (range.End.TimeOfDay != TimeSpan.Zero)
			{
				range.End = range.End.Date.AddDays(1.0);
			}
			return CreateExpander().GetHourRanges(range);
		}

		/// <summary>Last moment of the pattern, or <see cref="DateTime.MaxValue"/> when it never ends.</summary>
		internal DateTime GetRecurrenceEnd()
		{
			return CreateExpander().GetRecurrenceEnd();
		}

		private SchedulerWorkingHourExpander CreateExpander()
		{
			SchedulerDayOfWeek firstDayOfWeek = SchedulerDayOfWeek.Sunday;
			SchedulerDayOfWeek workWeek = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			if (_hourRange != null && _hourRange.GetOwner() != null)
			{
				firstDayOfWeek = _hourRange.GetOwner().FirstDayOfWeek;
				workWeek = _hourRange.GetOwner().WorkWeek;
			}
			return new SchedulerWorkingHourExpander(this, firstDayOfWeek, workWeek);
		}

		public SchedulerHourRange[] GetOccurrence(DateTime date)
		{
			DateRange range = new DateRange(date.Date, date.Date.AddDays(1.0));
			return GetOccurrences(range);
		}

		public new string ToString()
		{
			PropertyValue root = new PropertyValue("RecurrencePattern", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
			return mordexLayoutWriter.ToString();
		}
	}
}
