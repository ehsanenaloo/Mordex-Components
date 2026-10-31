using System;

namespace Mordex.Windows.Timeline
{
	internal interface ITimelineWorkingHoursOwner
	{
		MxTimeline Timeline { get; }

		DayOfWeek FirstDayOfWeek { get; }

		DayOfWeek WorkWeek { get; }

		DateTime MinDate { get; }

		void InvalidateWorkingTime();
	}
}
