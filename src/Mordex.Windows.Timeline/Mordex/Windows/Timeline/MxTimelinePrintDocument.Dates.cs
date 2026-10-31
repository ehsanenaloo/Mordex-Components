using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimelinePrintDocument
	{
		private void UpdatePageEndDate(DateTime dateTime, float floatValue)
		{
			if (_timeline.BottomTier.Interval == TimelineInterval.Year)
			{
				int value = (int)(floatValue * (float)_timeline.BottomTier.Count);
				_pageEndDate = dateTime.AddYears(value);
			}
			else if (_timeline.BottomTier.Interval == TimelineInterval.Month)
			{
				int months = (int)(floatValue * (float)_timeline.BottomTier.Count);
				_pageEndDate = dateTime.AddMonths(months);
			}
			else
			{
				_pageEndDate = dateTime.AddSeconds((float)_timeline._intervalSeconds * floatValue);
			}
		}

		private void SetPageStartDate(DateTime dateTime)
		{
			_pageStartDate = dateTime;
			if (_startDate.Equals(DateTime.MinValue))
			{
				return;
			}
			long indentSeconds = 0;
			if (_timeline.Groups.Count > 0)
			{
				indentSeconds = (long)((double)(_timeline.GroupIndent * (_timeline.Groups.Count - 1)) / (double)_intervalWidth * (double)_timeline._intervalSeconds) + 1;
				if (_timeline.ShowLastGroupAsRowHeader)
				{
					indentSeconds += (long)((double)_timeline.RowHeaderWidth / (double)_intervalWidth * (double)_timeline._intervalSeconds) + 1;
				}
			}
			_pageStartDate = _pageStartDate.AddSeconds(-indentSeconds);
		}

		private void CollectRowsAndItems()
		{
			_rowLayouts = new ArrayList();
			_printItems = new ArrayList();
			if (_rowLayoutCollection.Count == 0)
			{
				return;
			}
			bool isParentExpanded = true;
			int previousGroupIndex = 0;
			_totalRowsHeight = 0;
			foreach (MxRowLayoutState item in _rowLayoutCollection)
			{
				if (item.GetRowType() == RowType.Group)
				{
					if (!isParentExpanded && !PrintCollapsedRows && item.GetNode().Group.Index > previousGroupIndex)
					{
						continue;
					}
					item.SetRowIndex(_rowLayouts.Count);
					if (item.GetNode().Group.Index != Timeline.Groups.Count - 1 || !Timeline.ShowLastGroupAsRowHeader)
					{
						_rowLayouts.Add(item);
						isParentExpanded = item.GetNode().Expanded;
						_totalRowsHeight += _groupRowHeight - 1;
					}
					else
					{
						isParentExpanded = true;
					}
					previousGroupIndex = item.GetNode().Group.Index;
					if (item.GetNode().Rows.Group != null || (!isParentExpanded && !_printCollapsedRows))
					{
						continue;
					}
					foreach (MordexRow row in (IEnumerable)item.GetNode().Rows)
					{
						if (row is MordexDataRow mordexDataRow && mordexDataRow.Visible && mordexDataRow.DataHolder != null && mordexDataRow.DataHolder.Tag is TimelineItemState)
						{
							TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(row);
							timelineItem = timelineItem.CreateCopy();
							timelineItem.SetRowIndex(_rowLayouts.Count);
							_printItems.Add(timelineItem);
						}
					}
				}
				else if ((isParentExpanded || PrintCollapsedRows) && item.GetRowType() == RowType.Record)
				{
					item.SetRowIndex(_rowLayouts.Count);
					_rowLayouts.Add(item);
					_totalRowsHeight += _rowHeight;
				}
			}
			if (Timeline.Groups.Count != 0)
			{
				return;
			}
			foreach (MordexRow rootChildRow in (IEnumerable)_timeline.GetTimelineDataSourceManager().RootRow.Rows)
			{
				if (rootChildRow.Visible)
				{
					TimelineItem rootTimelineItem = TimelineDataSourceManager.GetTimelineItem(rootChildRow);
					rootTimelineItem = rootTimelineItem.CreateCopy();
					rootTimelineItem.SetRowIndex(0);
					_printItems.Add(rootTimelineItem);
				}
			}
		}

		private void PreparePageTimeRange(Graphics graphics)
		{
			_pageStartOffset = _timeline.GetIntervalDistance(_timeline.MinDate, _pageStartDate);
			_ = _pageContentBounds.Width / _intervalWidth;
			_timescaleOriginX = _pageContentBounds.Left;
			DateTime dateTime = _timeline.AlignToInterval(_pageStartDate, Timeline.BottomTier.Interval, Timeline.BottomTier.Count);
			long secondsPerInterval = _timeline._intervalSeconds;
			switch (Timeline.BottomTier.Interval)
			{
			case TimelineInterval.Second:
				if (dateTime != _pageStartDate)
				{
					_timescaleOriginX -= (float)_pageStartDate.Subtract(dateTime).TotalSeconds / (float)secondsPerInterval * (float)_intervalWidth;
				}
				break;
			case TimelineInterval.Minute:
				if (dateTime != _pageStartDate)
				{
					_timescaleOriginX -= (float)_pageStartDate.Subtract(dateTime).TotalSeconds / (float)secondsPerInterval * (float)_intervalWidth;
				}
				else
				{
					_timescaleOriginX -= (float)_pageStartDate.TimeOfDay.Seconds / (float)secondsPerInterval * (float)_intervalWidth;
				}
				break;
			case TimelineInterval.Hour:
				if (dateTime != _pageStartDate)
				{
					_timescaleOriginX -= (float)_pageStartDate.Subtract(dateTime).TotalSeconds / (float)secondsPerInterval * (float)_intervalWidth;
				}
				else
				{
					_timescaleOriginX -= (float)(_pageStartDate.TimeOfDay.Minutes * 60 + _pageStartDate.TimeOfDay.Seconds) / (float)secondsPerInterval * (float)_intervalWidth;
				}
				break;
			case TimelineInterval.Month:
			case TimelineInterval.Year:
				_timescaleOriginX -= (float)(_timeline.GetIntervalDistance(dateTime, _pageStartDate) * (double)_intervalWidth);
				break;
			case TimelineInterval.Week:
				_timescaleOriginX -= (float)_pageStartDate.Subtract(dateTime).TotalSeconds / (float)secondsPerInterval * (float)_intervalWidth;
				break;
			case TimelineInterval.Day:
				if (dateTime != _pageStartDate)
				{
					_timescaleOriginX -= (float)_pageStartDate.Subtract(dateTime).TotalSeconds / (float)secondsPerInterval * (float)_intervalWidth;
				}
				else
				{
					_timescaleOriginX -= (float)_pageStartDate.TimeOfDay.TotalSeconds / (float)secondsPerInterval * (float)_intervalWidth;
				}
				break;
			}
			_pageEndOffset = _timeline.GetIntervalDistance(_timeline.MinDate, _pageEndDate);
			UpdatePageItems();
		}
	}
}
