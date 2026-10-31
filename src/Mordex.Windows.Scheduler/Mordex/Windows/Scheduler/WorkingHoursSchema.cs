using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class WorkingHoursSchema : IMordexXmlLayoutsSupport
	{
		private ISchedulerWorkingHoursOwner _owner;

		private SchedulerHourRangeCollection _workingHoursRange;

		private WorkingHourExceptionCollection _exceptions;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public MxScheduler Scheduler
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Scheduler;
				}
				return null;
			}
		}

		[Description("Represents the collection of SchedulerHourRange objects used to format the background of the Schedule control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("System.ComponentModel.Design.CollectionEditor, System.Design", typeof(UITypeEditor))]
		public SchedulerHourRangeCollection WorkingHoursRange => _workingHoursRange;

		[Editor("System.ComponentModel.Design.CollectionEditor, System.Design", typeof(UITypeEditor))]
		[Description("Represents the collection of SchedulerExceptionDay objects that will follow a different working time.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public WorkingHourExceptionCollection Exceptions => _exceptions;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			PropertyValue propertyValue = info["WorkingHoursRange"];
			_workingHoursRange = new SchedulerHourRangeCollection(_owner);
			propertyValue?.DeserializeObject(_workingHoursRange);
			PropertyValue exceptionsValue = info["Exceptions"];
			_exceptions = new WorkingHourExceptionCollection(_owner);
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

		internal WorkingHoursSchema(ISchedulerWorkingHoursOwner owner)
		{
			_owner = owner;
			_workingHoursRange = new SchedulerHourRangeCollection(_owner);
			_exceptions = new WorkingHourExceptionCollection(_owner);
		}

		internal WorkingHoursSchema(ISchedulerWorkingHoursOwner owner, SchedulerHourRangeCollection hourRangeCollection)
		{
			_owner = owner;
			_workingHoursRange = hourRangeCollection;
			_exceptions = new WorkingHourExceptionCollection(_owner);
		}

		internal void AddExceptionHourRanges(DateRange dateRange, SchedulerHourRangeCollection schedulerHourRangeCollection)
		{
			foreach (WorkingHourException item in _exceptions)
			{
				if (item.DateRange.Start < dateRange.End && item.DateRange.End > dateRange.Start && SchedulerRenderer.IsWithinWorkingHours(dateRange, item.HourRange.DayOfWeek, item.HourRange))
				{
					schedulerHourRangeCollection.Add(item.HourRange);
				}
			}
		}

		internal SchedulerHourRange FindHourRange(DateRange range)
		{
			foreach (WorkingHourException item in _exceptions)
			{
				if (item.DateRange.Start < range.End && item.DateRange.End > range.Start && SchedulerRenderer.IsWithinWorkingHours(range, item.HourRange.DayOfWeek, item.HourRange))
				{
					return item.HourRange;
				}
			}
			foreach (SchedulerHourRange workingHourRange in WorkingHoursRange)
			{
				if (workingHourRange.RecurrencePattern != null)
				{
					SchedulerHourRange[] occurrences = workingHourRange.RecurrencePattern.GetOccurrences(range);
					if (occurrences.Length > 0)
					{
						return occurrences[0];
					}
				}
				else if (SchedulerRenderer.IsWithinWorkingHours(range, workingHourRange.DayOfWeek, workingHourRange))
				{
					return workingHourRange;
				}
			}
			return null;
		}

		internal SchedulerHourRangeCollection GetHourRangesInRange(DateRange range)
		{
			SchedulerHourRangeCollection schedulerHourRangeCollection = new SchedulerHourRangeCollection(null);
			AddExceptionHourRanges(range, schedulerHourRangeCollection);
			foreach (SchedulerHourRange item in WorkingHoursRange)
			{
				if (item.RecurrencePattern != null)
				{
					schedulerHourRangeCollection.AddRange(item.RecurrencePattern.GetOccurrences(range));
				}
				else if (SchedulerRenderer.IsWithinWorkingHours(range, item.DayOfWeek, item))
				{
					schedulerHourRangeCollection.Add(item);
				}
			}
			return schedulerHourRangeCollection;
		}

		internal SchedulerHourRangeCollection GetCombinedHourRanges(DateRange range, WorkingHoursSchema workingHoursSchema)
		{
			SchedulerHourRangeCollection schedulerHourRangeCollection = new SchedulerHourRangeCollection(null);
			workingHoursSchema.AddExceptionHourRanges(range, schedulerHourRangeCollection);
			AddExceptionHourRanges(range, schedulerHourRangeCollection);
			foreach (SchedulerHourRange item in workingHoursSchema.WorkingHoursRange)
			{
				if (item.RecurrencePattern != null)
				{
					schedulerHourRangeCollection.AddRange(item.RecurrencePattern.GetOccurrences(range));
				}
				else if (SchedulerRenderer.IsWithinWorkingHours(range, item.DayOfWeek, item))
				{
					schedulerHourRangeCollection.Add(item);
				}
			}
			foreach (SchedulerHourRange ownHourRange in WorkingHoursRange)
			{
				if (ownHourRange.RecurrencePattern != null)
				{
					schedulerHourRangeCollection.AddRange(ownHourRange.RecurrencePattern.GetOccurrences(range));
				}
				else if (SchedulerRenderer.IsWithinWorkingHours(range, ownHourRange.DayOfWeek, ownHourRange))
				{
					schedulerHourRangeCollection.Add(ownHourRange);
				}
			}
			return schedulerHourRangeCollection;
		}

		internal void UpdateWorkingHoursSchema()
		{
			_exceptions.Clear();
			_workingHoursRange.Clear();
		}

		internal bool IsCustomized()
		{
			if (_exceptions.Count > 0)
			{
				return true;
			}
			if (_workingHoursRange.Count > 0)
			{
				return true;
			}
			return false;
		}
	}
}
