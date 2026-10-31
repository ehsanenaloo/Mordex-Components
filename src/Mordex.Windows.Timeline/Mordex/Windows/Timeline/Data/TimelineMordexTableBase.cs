using System;
using System.Collections;
using Mordex.Data;

namespace Mordex.Windows.Timeline.Data
{
	internal class TimelineMordexTableBase : MordexTableBase
	{
		private MxTimeline _timeline;

		internal TimelineMordexTableBase(MxTimeline timeline)
		{
			_timeline = timeline;
		}

		internal MxTimeline GetTimeline()
		{
			return _timeline;
		}

		protected override MordexFieldCollectionBase CreateFieldCollection()
		{
			return new TimelineFieldCollection(this);
		}

		protected override MordexGroupCollectionBase CreateGroupsCollection()
		{
			return new TimelineGroupCollection(this);
		}

		protected override MordexStoredFilterCollectionBase CreateStoredFiltersCollection()
		{
			return new TimelineStoredFilterCollection(this);
		}

		protected override MordexGroupTotalCollectionBase CreateGroupTotalCollection()
		{
			return new TimelineGroupHeaderTotalCollection(this);
		}

		protected override MordexConditionBase CreateCondition()
		{
			return new TimelineFilterCondition();
		}

		protected override int CompareDataRows(MordexDataRow drx, MordexDataRow dry)
		{
			if (drx.RootRow == _timeline.GetTimelineDataSourceManager().NonGroupedRootRow)
			{
				return base.CompareDataRows(drx, dry);
			}
			TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(drx);
			TimelineItem secondItem = TimelineDataSourceManager.GetTimelineItem(dry);
			if (timelineItem == null || secondItem == null)
			{
				return base.CompareDataRows(drx, dry);
			}
			DateTime dateTime = timelineItem.StartTime;
			DateTime secondStart = secondItem.StartTime;
			if (timelineItem.EstimatedStartTime != DateTime.MinValue && timelineItem.EstimatedStartTime < dateTime)
			{
				dateTime = timelineItem.EstimatedStartTime;
			}
			if (secondItem.EstimatedStartTime != DateTime.MinValue && secondItem.EstimatedStartTime < secondStart)
			{
				secondStart = secondItem.EstimatedStartTime;
			}
			int comparison = dateTime.CompareTo(secondStart);
			if (comparison != 0)
			{
				return comparison;
			}
			comparison = -timelineItem.EndTime.CompareTo(secondItem.EndTime);
			if (comparison != 0)
			{
				return comparison;
			}
			return base.CompareDataRows(drx, dry);
		}

		private void ExpandGroupRow(MordexRow mordexRow, TimelineGroup timelineGroup)
		{
			if (mordexRow is MordexGroupRow mordexGroupRow && mordexGroupRow.Group == timelineGroup)
			{
				mordexGroupRow.Expanded = true;
				return;
			}
			foreach (MordexRow item in (IEnumerable)mordexRow.Rows)
			{
				ExpandGroupRow(item, timelineGroup);
			}
		}

		protected override void OnGroupSettingsChanged(CollectionChangeType change, MordexGroupBase group)
		{
			base.OnGroupSettingsChanged(change, group);
			if (GetTimeline() == null || !GetTimeline().ShowLastGroupAsRowHeader || base.BaseGroups.Count <= 0)
			{
				return;
			}
			TimelineGroup timelineGroup = (TimelineGroup)((IMordexGroupCollection)base.BaseGroups)[base.BaseGroups.Count - 1];
			if (GetTimeline().GroupMode == GroupMode.Collapsed)
			{
				if (GetTimeline().RowHeaderGroupMode == GroupMode.Expanded)
				{
					ExpandGroupRow(GetTimeline().GetTimelineDataSourceManager().RootRow, timelineGroup);
				}
			}
			else if (GetTimeline().RowHeaderGroupMode == GroupMode.Collapsed)
			{
				GetTimeline().CollapseGroup(timelineGroup);
			}
		}
	}
}
