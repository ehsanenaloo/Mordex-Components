using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		internal void RefreshRowLayouts()
		{
			if (_mordexRowLayoutState != null)
			{
				Graphics graphics = CreateGraphics();
				int rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
				BuildRowLayouts(graphics, _timelineDataSourceManager.RootRow.Rows, _mordexRowLayoutState, rowTop);
				graphics.Dispose();
				graphics = null;
				LayoutRows();
				InvalidateLayout();
			}
		}

		private void LayoutRows()
		{
			if (_areRowsDirty)
			{
				RebuildRows();
			}
			_rowLayouts = new ArrayList();
			_rowItems = new ArrayList();
			if (_allRowLayouts == null || _allRowLayouts.Count == 0)
			{
				_rowsBottom = base.ClientRectangle.Bottom;
				return;
			}
			bool isGroupExpanded = true;
			int lastGroupIndex = 0;
			int firstVisibleRowIndex = 0;
			if (_firstVisibleRowLayout != null)
			{
				firstVisibleRowIndex = _firstVisibleRowLayout.GetRowIndex();
			}
			foreach (MxRowLayoutState item in _allRowLayouts)
			{
				item.SetRowBounds(Rectangle.Empty);
				if (item.GetRowType() == RowType.Group)
				{
					if (item.GetNode().Group == null || (!isGroupExpanded && item.GetNode().Group.Index > lastGroupIndex))
					{
						continue;
					}
					item.SetRowIndex(_rowLayouts.Count);
					if ((item.GetNode().Group != null && item.GetNode().Group.Index != Groups.Count - 1) || !ShowLastGroupAsRowHeader)
					{
						_rowLayouts.Add(item);
						isGroupExpanded = item.IsExpanded();
					}
					else
					{
						isGroupExpanded = true;
					}
					lastGroupIndex = item.GetNode().Group.Index;
					if (item.GetNode().Group == null || !isGroupExpanded)
					{
						continue;
					}
					foreach (MordexRow childRow in (IEnumerable)item.GetNode().Rows)
					{
						if (childRow is MordexDataRow mordexDataRow && mordexDataRow.Visible && mordexDataRow.DataHolder != null && mordexDataRow.DataHolder.Tag is TimelineItemState timelineItemState)
						{
							if (ShowLastGroupAsRowHeader)
							{
								timelineItemState._timelineItem.SetRowIndex(item.GetRowIndex());
							}
							else
							{
								timelineItemState._timelineItem.SetRowIndex(item.GetRowIndex() + 1);
							}
							_rowItems.Add(timelineItemState._timelineItem);
						}
					}
				}
				else if (isGroupExpanded && item.GetRowType() == RowType.Record)
				{
					item.SetRowIndex(_rowLayouts.Count);
					_rowLayouts.Add(item);
				}
			}
			int rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
			int visibleRowCount = GetVisibleRowCount();
			if (visibleRowCount <= 0 && _allRowLayouts.Count > 0)
			{
				firstVisibleRowIndex = 0;
				_firstVisibleRowLayout = _allRowLayouts.GetRowLayoutAt(firstVisibleRowIndex);
				visibleRowCount = GetVisibleRowCount();
			}
			rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
			if (firstVisibleRowIndex > 0 && firstVisibleRowIndex > _allRowLayouts.Count - visibleRowCount)
			{
				firstVisibleRowIndex = _allRowLayouts.Count - visibleRowCount;
				if (firstVisibleRowIndex < 0)
				{
					firstVisibleRowIndex = 0;
				}
				if (firstVisibleRowIndex > _allRowLayouts.Count - 1)
				{
					firstVisibleRowIndex = _allRowLayouts.Count - 1;
				}
				_firstVisibleRowLayout = _allRowLayouts.GetRowLayoutAt(firstVisibleRowIndex);
			}
			if (_rowLayouts.Count - visibleRowCount > 0 && firstVisibleRowIndex > _rowLayouts.Count - visibleRowCount)
			{
				firstVisibleRowIndex = _rowLayouts.Count - visibleRowCount;
			}
			while (firstVisibleRowIndex > _rowLayouts.Count)
			{
				firstVisibleRowIndex = _rowLayouts.Count - visibleRowCount;
			}
			if (firstVisibleRowIndex < 0)
			{
				firstVisibleRowIndex = 0;
			}
			for (int i = firstVisibleRowIndex; i < _rowLayouts.Count; i++)
			{
				MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[i];
				if (mordexRowLayoutState.GetRowType() == RowType.Group)
				{
					int groupLeft = base.ClientRectangle.Left + _groupIndent * mordexRowLayoutState.GetGroupIndex();
					mordexRowLayoutState.SetRowBounds(new Rectangle(groupLeft, rowTop, base.ClientRectangle.Right - groupLeft, GetGroupRowHeight()));
					rowTop += GetGroupRowHeight() - 1;
					continue;
				}
				int rowLeft = base.ClientRectangle.Left + _groupIndent * mordexRowLayoutState.GetGroupIndex();
				mordexRowLayoutState.SetRowBounds(new Rectangle(rowLeft, rowTop, base.ClientRectangle.Right - rowLeft, RowHeight));
				if (mordexRowLayoutState.GetTimelineGroupRow() != null && !mordexRowLayoutState.GetMordexRowLayoutStateRectangle().IsEmpty)
				{
					mordexRowLayoutState.SetMordexRowLayoutStateRectangle(new Rectangle(mordexRowLayoutState.GetMordexRowLayoutStateRectangle().Left, rowTop, mordexRowLayoutState.GetMordexRowLayoutStateRectangle().Width, mordexRowLayoutState.GetMordexRowLayoutStateRectangle().Height));
				}
				rowTop += RowHeight - 1;
			}
			_rowsBottom = rowTop;
			rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
			for (int previousRowIndex = firstVisibleRowIndex - 1; previousRowIndex >= 0; previousRowIndex--)
			{
				MxRowLayoutState mordexRowLayoutStateArgument = (MxRowLayoutState)_rowLayouts[previousRowIndex];
				if (mordexRowLayoutStateArgument.GetRowType() == RowType.Group)
				{
					int groupLeftAbove = base.ClientRectangle.Left + _groupIndent * mordexRowLayoutStateArgument.GetGroupIndex();
					rowTop -= GetGroupRowHeight() - 1;
					mordexRowLayoutStateArgument.SetRowBounds(new Rectangle(groupLeftAbove, rowTop, base.ClientRectangle.Right - groupLeftAbove, GetGroupRowHeight()));
				}
				else
				{
					int rowLeftAbove = base.ClientRectangle.Left + _groupIndent * mordexRowLayoutStateArgument.GetGroupIndex();
					rowTop -= RowHeight - 1;
					mordexRowLayoutStateArgument.SetRowBounds(new Rectangle(rowLeftAbove, rowTop, base.ClientRectangle.Right - rowLeftAbove, RowHeight));
					if (mordexRowLayoutStateArgument.GetTimelineGroupRow() != null && !mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().IsEmpty)
					{
						mordexRowLayoutStateArgument.SetMordexRowLayoutStateRectangle(new Rectangle(mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Left, rowTop, mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Width, mordexRowLayoutStateArgument.GetMordexRowLayoutStateRectangle().Height));
					}
				}
			}
			if (Groups.Count == 0)
			{
				foreach (MordexRow topLevelRow in (IEnumerable)_timelineDataSourceManager.RootRow.Rows)
				{
					if (topLevelRow.Visible)
					{
						TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(topLevelRow);
						if (timelineItem != null)
						{
							timelineItem.SetRowIndex(0);
							_rowItems.Add(timelineItem);
						}
					}
				}
			}
			LayoutVisibleItems();
			if (_firstVisibleRowLayout == null)
			{
				_firstVisibleRowLayout = (MxRowLayoutState)_rowLayouts[0];
			}
			else if (_firstVisibleRowLayout.GetRowIndex() > _rowLayouts.Count - 1)
			{
				_firstVisibleRowLayout = (MxRowLayoutState)_rowLayouts[_rowLayouts.Count - 1];
			}
			UpdateVerticalScrollBar(0, _rowLayouts.Count - 1, visibleRowCount, _firstVisibleRowLayout.GetRowIndex());
			if (visibleRowCount >= _rowLayouts.Count)
			{
				base.SetVerticalScrollRequired(false);
			}
			else
			{
				base.SetVerticalScrollRequired(true);
			}
		}

		internal void UpdateTimelineSelectedItemCollection(ITimelineObject item)
		{
			if (_selectedItems == null)
			{
				_selectedItems = new TimelineSelectedItemCollection(this);
			}
			_selectedItems.Add(item);
		}

		private void UpdateRowHeights()
		{
			_rowHeight = (int)Font.GetHeight() + 4;
			_groupRowHeight = _rowHeight + 4;
			if (GetImageList() != null)
			{
				int imageRowHeight = GetImageList().ImageSize.Height + 2;
				if (imageRowHeight > _rowHeight)
				{
					_rowHeight = imageRowHeight;
				}
				if (imageRowHeight > _groupRowHeight)
				{
					_groupRowHeight = imageRowHeight;
				}
			}
			if (_showItemsDurationBar)
			{
				_rowHeight += ItemsDurationBarHeight + 1;
			}
			if (_showItemsEstimatedDurationBar)
			{
				_rowHeight += ItemsDurationBarHeight + 1;
			}
		}

		private void LayoutVisibleItems()
		{
			if (_rowItems == null)
			{
				return;
			}
			_visibleItems = new ArrayList();
			foreach (TimelineItem item in _rowItems)
			{
				if (IsInVisibleRange(item._leftOffset, item._doubleValue))
				{
					MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[item.GetRowIndex()];
					if (mordexRowLayoutState != null && !mordexRowLayoutState.GetRowBounds().IsEmpty)
					{
						float x = (float)base.ClientRectangle.Left + (float)((item._leftOffset - _doubleValue) * (double)_intervalWidth);
						float itemTop = mordexRowLayoutState.GetRowBounds().Y + 1;
						itemTop += (float)(item.GetSubRowIndex() * (RowHeight - 1));
						item._rectangleF = new RectangleF(x, itemTop, (float)((item._doubleValue - item._leftOffset) * (double)_intervalWidth), RowHeight - 2);
						item._durationWidth = item._durationLength * (double)_intervalWidth;
						item._estimatedDurationWidth = item._estimatedDurationLength * (double)_intervalWidth;
						item.UpdateTimelineItemRectangle((float)base.ClientRectangle.Left + (float)((item._estimatedStartOffset - _doubleValue) * (double)_intervalWidth), ItemsDurationBarHeight);
						item.UpdateDurationBarBounds((float)base.ClientRectangle.Left + (float)((item._startOffset - _doubleValue) * (double)_intervalWidth), ItemsDurationBarHeight);
						_visibleItems.Add(item);
					}
				}
			}
			if (_draggedItems == null || _draggedItems.Count <= 0)
			{
				return;
			}
			foreach (TimelineItem draggedItem in _draggedItems)
			{
				if (draggedItem.GetRowIndex() >= 0 && draggedItem.GetRowIndex() < _rowLayouts.Count)
				{
					MxRowLayoutState mordexRowLayoutStateArgument = (MxRowLayoutState)_rowLayouts[draggedItem.GetRowIndex()];
					if (mordexRowLayoutStateArgument != null && !mordexRowLayoutStateArgument.GetRowBounds().IsEmpty)
					{
						float x2 = (float)base.ClientRectangle.Left + (float)((draggedItem._leftOffset - _doubleValue) * (double)_intervalWidth);
						float draggedItemTop = mordexRowLayoutStateArgument.GetRowBounds().Y + 1;
						draggedItemTop += (float)(draggedItem.GetSubRowIndex() * (RowHeight - 1));
						draggedItem._rectangleF = new RectangleF(x2, draggedItemTop, (float)((draggedItem._doubleValue - draggedItem._leftOffset) * (double)_intervalWidth), RowHeight - 2);
						draggedItem._durationWidth = draggedItem._durationLength * (double)_intervalWidth;
						draggedItem._estimatedDurationWidth = draggedItem._estimatedDurationLength * (double)_intervalWidth;
						draggedItem.UpdateTimelineItemRectangle((float)base.ClientRectangle.Left + (float)((draggedItem._estimatedStartOffset - _doubleValue) * (double)_intervalWidth), ItemsDurationBarHeight);
						draggedItem.UpdateDurationBarBounds((float)base.ClientRectangle.Left + (float)((draggedItem._startOffset - _doubleValue) * (double)_intervalWidth), ItemsDurationBarHeight);
					}
				}
			}
		}

		internal float MeasureItemWidth(Graphics graphics, string text, TimelineItem itemArgument, TextLayoutRenderer textLayoutRenderer, TimelineFormatStyle timelineFormatStyleArgument)
		{
			if (GetItemSizeMode() == ItemSizeMode.DurationBased)
			{
				return _minItemWidth;
			}
			float itemWidth = 0f;
			itemWidth += (float)GetItemImageWidth(itemArgument);
			TimelineFormatStyle timelineFormatStyle = timelineFormatStyleArgument;
			if (!itemArgument.FormatStyle.HasNoFontOverrides())
			{
				timelineFormatStyle = timelineFormatStyle.MergeWith(itemArgument.FormatStyle);
			}
			TimelineFormatStyle mergedStyle = timelineFormatStyle;
			foreach (TimelineFormatCondition item in _formatConditions)
			{
				if (!item.Enabled || !item.EvaluateItem(itemArgument))
				{
					continue;
				}
				if (itemWidth == 0f && (item.ImageIndex != -1 || item.ImageKey.Length > 0))
				{
					ImageList imageList = GetImageList();
					if (imageList != null)
					{
						if (imageList.Images.ContainsKey(item.ImageKey))
						{
							itemWidth = imageList.ImageSize.Width;
						}
						else if (item.ImageIndex >= 0 && item.ImageIndex < imageList.Images.Count)
						{
							itemWidth = imageList.ImageSize.Width;
						}
					}
				}
				mergedStyle = ((!item.AllowMerge) ? timelineFormatStyle.MergeWith(item.FormatStyle) : mergedStyle.MergeWith(item.FormatStyle));
			}
			if (itemWidth > 0f)
			{
				itemWidth += 2f;
			}
			itemWidth += textLayoutRenderer.RenderContent(graphics, text, mergedStyle.ResolveFont(Font), 0).Width + 2f;
			mergedStyle.DisposeResources();
			timelineFormatStyle.DisposeResources();
			return itemWidth;
		}

		internal void RebuildRows()
		{
			if (_isRefreshEnabled && !_isInitializing)
			{
				if (_allRowLayouts == null)
				{
					_allRowLayouts = new TimelineCollection();
				}
				_allRowLayouts.Clear();
				MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(RowType.SourceRow, _timelineDataSourceManager.RootRow, null);
				Graphics graphics = CreateGraphics();
				int rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
				_areRowsDirty = false;
				BuildRowLayouts(graphics, _timelineDataSourceManager.RootRow.Rows, mordexRowLayoutState, rowTop);
				LayoutRows();
				graphics.Dispose();
				graphics = null;
			}
			_isLayoutDirty = true;
			Invalidate();
		}

		private MordexRow FindRowForItem(MordexRowCollection mordexRowCollection, TimelineItem timelineItemArgument)
		{
			if (mordexRowCollection != null)
			{
				foreach (MordexRow item in (IEnumerable)mordexRowCollection)
				{
					TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(item);
					if (timelineItem == timelineItemArgument)
					{
						return item;
					}
					MordexRow childMatch = FindRowForItem(item.Rows, timelineItemArgument);
					if (childMatch != null)
					{
						return childMatch;
					}
				}
			}
			return null;
		}

		internal void InvalidateRowsAndLayout()
		{
			InvalidateRows();
			InvalidateLayout();
		}

		internal void InvalidateRows()
		{
			_areRowsDirty = true;
			Invalidate();
		}

		private MxRowLayoutState ComputeMordexRowLayoutState(Point point)
		{
			return GetRowLayoutAt(point.X, point.Y);
		}

		private void UpdateTimelineMordexRowLayoutState(MxRowLayoutState timelineMordexRowLayoutState)
		{
			if (_timelineMordexRowLayoutState == timelineMordexRowLayoutState)
			{
				return;
			}
			if (_timelineMordexRowLayoutState != null)
			{
				_timelineMordexRowLayoutState.SetHot(false);
				Invalidate(_timelineMordexRowLayoutState.GetExpandBoxBounds());
			}
			_timelineMordexRowLayoutState = timelineMordexRowLayoutState;
			if (_timelineMordexRowLayoutState != null)
			{
				_timelineMordexRowLayoutState.SetHot(true);
				Invalidate(_timelineMordexRowLayoutState.GetExpandBoxBounds());
			}
		}

		internal TimelineGroupRow GetGroupRow(MordexRow mordexRow)
		{
			if (mordexRow is MordexGroupRow mordexGroupRow)
			{
				return _timelineDataSourceManager.GetGroupRow(mordexGroupRow);
			}
			return null;
		}
	}
}
