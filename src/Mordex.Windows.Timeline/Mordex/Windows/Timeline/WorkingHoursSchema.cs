using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class WorkingHoursSchema : IMordexXmlLayoutsSupport
	{
		private MxTimeline _timeline;

		private TimelineHourRangeCollection _workingHoursRange;

		private WorkingHourExceptionCollection _exceptions;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.ComponentModel.Design.CollectionEditor, System.Design", typeof(UITypeEditor))]
		[Description("Represents the collection of TimelineHourRange objects used to format the background of the Timeline control.")]
		public TimelineHourRangeCollection WorkingHoursRange => _workingHoursRange;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.ComponentModel.Design.CollectionEditor, System.Design", typeof(UITypeEditor))]
		[Description("Represents the collection of TimelineExceptionDay objects that will follow a different working time.")]
		public WorkingHourExceptionCollection Exceptions => _exceptions;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			PropertyValue propertyValue = info["WorkingHoursRange"];
			_workingHoursRange = new TimelineHourRangeCollection(_timeline);
			propertyValue?.DeserializeObject(_workingHoursRange);
			PropertyValue exceptionsValue = info["Exceptions"];
			_exceptions = new WorkingHourExceptionCollection(_timeline);
			exceptionsValue?.DeserializeObject(_exceptions);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_workingHoursRange.Count > 0)
			{
				info.AddValue("WorkingHoursRange", _workingHoursRange);
			}
			if (_exceptions.Count > 0)
			{
				info.AddValue("Exceptions", _exceptions);
			}
		}

		internal WorkingHoursSchema(MxTimeline timeline)
		{
			_timeline = timeline;
			_workingHoursRange = new TimelineHourRangeCollection(_timeline);
			_exceptions = new WorkingHourExceptionCollection(_timeline);
		}

		internal void AddExceptionHourRanges(DateRange dateRange, TimelineHourRangeCollection timelineHourRangeCollection)
		{
			foreach (WorkingHourException item in _exceptions)
			{
				if (item.DateRange.Start < dateRange.End && item.DateRange.End > dateRange.Start && TimelineUtilities.IsRangeInWorkingHours(dateRange, item.HourRange.DayOfWeek, item.HourRange))
				{
					timelineHourRangeCollection.Add(item.HourRange);
				}
			}
		}

		internal TimelineHourRange FindHourRange(DateRange range)
		{
			foreach (WorkingHourException item in _exceptions)
			{
				if (item.DateRange.Start < range.End && item.DateRange.End > range.Start && TimelineUtilities.IsRangeInWorkingHours(range, item.HourRange.DayOfWeek, item.HourRange))
				{
					return item.HourRange;
				}
			}
			foreach (TimelineHourRange workingHourRange in WorkingHoursRange)
			{
				if (workingHourRange.RecurrencePattern != null)
				{
					TimelineHourRange[] occurrences = workingHourRange.RecurrencePattern.GetOccurrences(range);
					TimelineHourRange[] occurrenceArray = occurrences;
					foreach (TimelineHourRange occurrence in occurrenceArray)
					{
						if (TimelineUtilities.IsRangeInWorkingHours(range, occurrence.DayOfWeek, occurrence))
						{
							return occurrence;
						}
					}
				}
				else if (TimelineUtilities.IsRangeInWorkingHours(range, workingHourRange.DayOfWeek, workingHourRange))
				{
					return workingHourRange;
				}
			}
			return null;
		}

		internal TimelineHourRangeCollection GetHourRangesInRange(DateRange range)
		{
			TimelineHourRangeCollection timelineHourRangeCollection = new TimelineHourRangeCollection(null);
			AddExceptionHourRanges(range, timelineHourRangeCollection);
			foreach (TimelineHourRange item in WorkingHoursRange)
			{
				if (item.RecurrencePattern != null)
				{
					timelineHourRangeCollection.AddRange(item.RecurrencePattern.GetOccurrences(range));
				}
				else if (TimelineUtilities.IsRangeInWorkingHours(range, item.DayOfWeek, item))
				{
					timelineHourRangeCollection.Add(item);
				}
			}
			return timelineHourRangeCollection;
		}
	}
}
