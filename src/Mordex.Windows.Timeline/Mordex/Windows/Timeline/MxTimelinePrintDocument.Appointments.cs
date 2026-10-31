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
		internal void BuildRowLayouts(Graphics graphics)
		{
			_rowLayoutCollection = new TimelineCollection();
			if (_timeline.GetTimelineDataSourceManager().RootRow != null)
			{
				MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(RowType.SourceRow, _timeline.GetTimelineDataSourceManager().RootRow, null);
				CollectRowLayouts(graphics, _timeline.GetTimelineDataSourceManager().RootRow.Rows, mordexRowLayoutState);
				CollectRowsAndItems();
			}
		}

		private void CollectRowLayouts(Graphics graphics, MordexRowCollection mordexRowCollection, MxRowLayoutState parentRow)
		{
			if (mordexRowCollection.Group == null)
			{
				if (parentRow.GetNode().Group != null)
				{
					AssignItemRows(mordexRowCollection, graphics, parentRow, parentRow.GetNode().Group.Index);
				}
				else
				{
					AssignItemRows(mordexRowCollection, graphics, parentRow, 0);
				}
				return;
			}
			foreach (MordexRow item in (IEnumerable)mordexRowCollection)
			{
				if (item.Visible)
				{
					MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(RowType.Group, item, parentRow);
					mordexRowLayoutState.SetGroupIndex(mordexRowCollection.Group.Index);
					_rowLayoutCollection.AddRowLayout(mordexRowLayoutState);
					CollectRowLayouts(graphics, item.Rows, mordexRowLayoutState);
				}
			}
		}

		private void AssignItemRows(MordexRowCollection mordexRowCollection, Graphics graphics, MxRowLayoutState parentRow, int groupIndex)
		{
			int newRowCount = 0;
			TimelineItemLaneCollection rowSlots = new TimelineItemLaneCollection();
			float intervalWidth = _intervalWidth;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			string text = "";
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_timeline.ItemsFormatStyle);
			bool shouldShareRow = _timeline.ShowLastGroupAsRowHeader && _timeline.Groups.Count > 0 && !parentRow.IsExpanded() && !PrintCollapsedRows;
			if (!shouldShareRow && Timeline.ItemLayoutMode == ItemLayoutMode.AllItemsOnOneRow)
			{
				shouldShareRow = true;
			}
			bool hasRowHeader = _timeline.ShowLastGroupAsRowHeader && _timeline.Groups.Count > 0;
			int count = _rowLayoutCollection.Count;
			foreach (MordexRow item in (IEnumerable)mordexRowCollection)
			{
				if (!item.Visible)
				{
					continue;
				}
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(item);
				timelineItem.SetGroupIndex(groupIndex);
				object startValue = timelineItem.StartTime;
				DateTime endTime = timelineItem.EndTime;
				DateTime dateTime = timelineItem.EstimatedStartTime;
				DateTime estimatedEndTime = timelineItem.EstimatedEndTime;
				if (startValue == DBNull.Value)
				{
					continue;
				}
				DateTime startTime = DateTime.MinValue;
				try
				{
					startTime = (DateTime)startValue;
				}
				catch
				{
				}
				double startPosition = _timeline.GetIntervalDistance(_timeline.MinDate, startTime);
				double estimatedStartPosition;
				if (dateTime != DateTime.MinValue)
				{
					estimatedStartPosition = _timeline.GetIntervalDistance(_timeline.MinDate, dateTime);
				}
				else
				{
					estimatedStartPosition = startPosition;
					dateTime = startTime;
				}
				if (_timeline.ShowLabel)
				{
					text = _timeline.GetItemLabelText(timelineItem);
				}
				bool needsNewRow = true;
				if (shouldShareRow || _timeline.ItemLayoutMode == ItemLayoutMode.NewRowOnOverlap)
				{
					foreach (TimelineItemLane rowSlot in rowSlots)
					{
						if (shouldShareRow || (rowSlot.GetDoubleValue() <= startPosition && rowSlot.GetDoubleValue() <= estimatedStartPosition))
						{
							needsNewRow = false;
							double duration = _timeline.GetIntervalDistance(startTime, endTime);
							double estimatedDuration = ((!(estimatedEndTime != DateTime.MinValue)) ? 0.0 : _timeline.GetIntervalDistance(dateTime, estimatedEndTime));
							rowSlot.SetDoubleValue(startPosition + (double)(_timeline.MeasureItemWidth(graphics, text, timelineItem, textLayoutRenderer, timelineFormatStyle) / intervalWidth));
							timelineItem._durationLength = duration;
							timelineItem._estimatedDurationLength = estimatedDuration;
							if (duration + startPosition > rowSlot.GetDoubleValue())
							{
								rowSlot.SetDoubleValue(duration + startPosition);
							}
							if (estimatedDuration + estimatedStartPosition > rowSlot.GetDoubleValue())
							{
								rowSlot.SetDoubleValue(estimatedStartPosition + estimatedDuration);
							}
							timelineItem._startOffset = startPosition;
							timelineItem._leftOffset = startPosition;
							timelineItem.SetSubRowIndex(rowSlot.GetLaneIndex());
							if (timelineItem._startOffset <= timelineItem._estimatedStartOffset)
							{
								timelineItem._leftOffset = timelineItem._startOffset;
							}
							else
							{
								timelineItem._leftOffset = timelineItem._estimatedStartOffset;
							}
							timelineItem._doubleValue = rowSlot.GetDoubleValue();
							break;
						}
					}
				}
				if (needsNewRow)
				{
					newRowCount++;
					MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(RowType.Record, groupIndex, parentRow);
					if (hasRowHeader && parentRow != null)
					{
						TimelineGroupRow timelineGroupRow = _timeline.GetGroupRow(parentRow.GetNode());
						mordexRowLayoutState.SetTimelineGroupRow(timelineGroupRow);
					}
					TimelineItemLane newRowSlot = rowSlots.AddLane();
					double newRowDuration = _timeline.GetIntervalDistance(startTime, endTime);
					double newRowEstimatedDuration = ((!(estimatedEndTime != DateTime.MinValue)) ? 0.0 : _timeline.GetIntervalDistance(dateTime, estimatedEndTime));
					newRowSlot.SetDoubleValue(startPosition + (double)(_timeline.MeasureItemWidth(graphics, text, timelineItem, textLayoutRenderer, timelineFormatStyle) / intervalWidth));
					if (newRowDuration < 0.0)
					{
						newRowDuration = 0.0;
					}
					timelineItem._durationLength = newRowDuration;
					timelineItem._estimatedDurationLength = newRowEstimatedDuration;
					if (newRowDuration + startPosition > newRowSlot.GetDoubleValue())
					{
						newRowSlot.SetDoubleValue(newRowDuration + startPosition);
					}
					if (newRowEstimatedDuration + estimatedStartPosition > newRowSlot.GetDoubleValue())
					{
						newRowSlot.SetDoubleValue(estimatedStartPosition + newRowEstimatedDuration);
					}
					timelineItem._startOffset = startPosition;
					timelineItem._estimatedStartOffset = estimatedStartPosition;
					timelineItem.SetSubRowIndex(newRowSlot.GetLaneIndex());
					if (timelineItem._startOffset <= timelineItem._estimatedStartOffset)
					{
						timelineItem._leftOffset = timelineItem._startOffset;
					}
					else
					{
						timelineItem._leftOffset = timelineItem._estimatedStartOffset;
					}
					timelineItem._doubleValue = newRowSlot.GetDoubleValue();
					timelineItem.SetSubRowIndex(newRowSlot.GetLaneIndex());
					_rowLayoutCollection.AddRowLayout(mordexRowLayoutState);
				}
			}
			if (hasRowHeader)
			{
				MxRowLayoutState mordexRowLayoutStateArgument = _rowLayoutCollection.GetRowLayoutAt(count);
				int rowHeaderWidth = _timeline.RowHeaderWidth;
				int x = _pageContentBounds.Left + _timeline.GroupIndent * groupIndex - 1;
				Rectangle rectangle = new Rectangle(x, 0, rowHeaderWidth, _rowHeight * (_rowLayoutCollection.Count - count) - (_rowLayoutCollection.Count - (count + 1)));
				mordexRowLayoutStateArgument.SetMordexRowLayoutStateRectangle(rectangle);
				for (int i = count + 1; i < _rowLayoutCollection.Count; i++)
				{
					MxRowLayoutState childRowLayout = _rowLayoutCollection.GetRowLayoutAt(i);
					childRowLayout.SetMordexRowLayoutStateRectangle(Rectangle.Empty);
				}
			}
			parentRow.SetRecordRowCount(newRowCount);
			timelineFormatStyle.DisposeResources();
		}
	}
}
