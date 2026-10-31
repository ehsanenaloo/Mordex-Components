using System;
using System.Collections;

namespace Mordex.Windows.Timeline
{
	internal class TimelineComparer : IComparer
	{
		internal TimelineComparer()
		{
		}

		public int Compare(object value, object valueArgument)
		{
			if (value == null || valueArgument == null)
			{
				return 0;
			}
			if (value == valueArgument)
			{
				return 0;
			}
			TimelineItem timelineItem = (TimelineItem)value;
			TimelineItem otherItem = (TimelineItem)valueArgument;
			DateTime date = timelineItem.StartTime.Date;
			DateTime otherDate = otherItem.StartTime.Date;
			int comparison = date.CompareTo(otherDate);
			if (comparison == 0)
			{
				TimeSpan timeOfDay = timelineItem.StartTime.TimeOfDay;
				TimeSpan otherTimeOfDay = otherItem.StartTime.TimeOfDay;
				comparison = timeOfDay.CompareTo(otherTimeOfDay);
				if (comparison == 0)
				{
					timeOfDay = timelineItem.Duration;
					otherTimeOfDay = otherItem.Duration;
					comparison = -timeOfDay.CompareTo(otherTimeOfDay);
				}
				if (comparison == 0)
				{
					string text = timelineItem.Text;
					string otherText = otherItem.Text;
					comparison = text.CompareTo(otherText);
				}
				if (comparison == 0)
				{
					int index = timelineItem.Index;
					int otherIndex = otherItem.Index;
					comparison = index.CompareTo(otherIndex);
				}
			}
			return comparison;
		}
	}
}
