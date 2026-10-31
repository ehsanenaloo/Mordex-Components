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
using System.Runtime.CompilerServices;
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
		private void NavigateRowsWithKey(Keys keys)
		{
			if (_focusedRowLayout == null && _currentItem != null)
			{
				_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
			}
			if (_focusedRowLayout == null)
			{
				return;
			}
			MxRowLayoutState mordexRowLayoutState = null;
			switch (keys)
			{
			case Keys.Return:
				if (_focusedRowLayout.GetRowType() == RowType.Group)
				{
					_focusedRowLayout.GetNode().Expanded = !_focusedRowLayout.GetNode().Expanded;
					LayoutRows();
					Invalidate();
				}
				break;
			case Keys.Subtract:
				if (_focusedRowLayout.GetRowType() == RowType.Group && _focusedRowLayout.IsExpanded())
				{
					_focusedRowLayout.GetNode().Expanded = false;
					LayoutRows();
					Invalidate();
				}
				break;
			case Keys.Add:
				if (_focusedRowLayout.GetRowType() == RowType.Group && !_focusedRowLayout.IsExpanded())
				{
					_focusedRowLayout.GetNode().Expanded = true;
					LayoutRows();
					Invalidate();
				}
				break;
			case Keys.Left:
				if (_focusedRowLayout.GetRowType() == RowType.Group)
				{
					if (_focusedRowLayout.IsExpanded())
					{
						_focusedRowLayout.GetNode().Expanded = false;
						LayoutRows();
						Invalidate();
						break;
					}
					mordexRowLayoutState = GetPreviousGroupRowLayout();
					if (mordexRowLayoutState != null)
					{
						SelectGroupRowOnClick(mordexRowLayoutState);
						_focusedRowLayout = mordexRowLayoutState;
						_currentItem = null;
						if (mordexRowLayoutState.GetRowIndex() < GetVerticalScrollBarControl().GetScrollValue())
						{
							GetVerticalScrollBarControl().UpdateScrollValue(mordexRowLayoutState.GetRowIndex());
						}
					}
				}
				else if (_focusedRowLayout.GetTimelineGroupRow() != null)
				{
					if (_currentItem != null)
					{
						SelectPreviousItemInGroup(_focusedRowLayout, _currentItem.StartTime, _currentItem.GetRecordPosition(), _currentItem.GetSubRowIndex(), _focusedRowLayout.GetTimelineGroupRow().GetGroupRow().GroupValue);
					}
					else
					{
						SelectPreviousItem(_focusedRowLayout, FirstDate, _focusedRowLayout.GetRowIndex(), _focusedRowLayout.GetRowIndex());
					}
				}
				else if (_currentItem != null)
				{
					mordexRowLayoutState = GetPreviousGroupRowLayout();
					SelectPreviousItem(mordexRowLayoutState, _currentItem.StartTime, _currentItem.GetRecordPosition(), _currentItem.GetSubRowIndex());
				}
				break;
			case Keys.Right:
				if (_focusedRowLayout.GetRowType() == RowType.Group)
				{
					if (!_focusedRowLayout.IsExpanded())
					{
						_focusedRowLayout.GetNode().Expanded = true;
						LayoutRows();
						Invalidate();
						break;
					}
					if (_focusedRowLayout.GetNode().Rows.Group == null)
					{
						SelectNextItem(_focusedRowLayout, _firstDate, -1, -1);
						break;
					}
					mordexRowLayoutState = GetNextGroupRowLayout();
					if (mordexRowLayoutState != null)
					{
						SelectGroupRowOnClick(mordexRowLayoutState);
						_focusedRowLayout = mordexRowLayoutState;
						_currentItem = null;
						if (mordexRowLayoutState.GetRowIndex() + 1 > GetVerticalScrollBarControl().GetScrollValue() + GetVerticalScrollBarControl().GetLargeChange())
						{
							GetVerticalScrollBarControl().UpdateScrollValue(mordexRowLayoutState.GetRowIndex() + 1 - GetVerticalScrollBarControl().GetLargeChange());
						}
					}
				}
				else if (_focusedRowLayout.GetTimelineGroupRow() != null)
				{
					if (_currentItem != null)
					{
						SelectNextItemInGroup(_focusedRowLayout, _currentItem.StartTime, _currentItem.GetRecordPosition(), _currentItem.GetSubRowIndex(), _focusedRowLayout.GetTimelineGroupRow().GetGroupRow().GroupValue);
					}
					else
					{
						SelectNextItem(_focusedRowLayout, FirstDate, _focusedRowLayout.GetRowIndex(), _focusedRowLayout.GetRowIndex());
					}
				}
				else if (_currentItem != null)
				{
					mordexRowLayoutState = GetPreviousGroupRowLayout();
					SelectNextItem(mordexRowLayoutState, _currentItem.StartTime, _currentItem.GetRecordPosition(), _currentItem.GetSubRowIndex());
				}
				break;
			case Keys.Down:
				mordexRowLayoutState = GetNextGroupRowLayout();
				if (mordexRowLayoutState != null)
				{
					SelectGroupRowOnClick(mordexRowLayoutState);
					_focusedRowLayout = mordexRowLayoutState;
					_currentItem = null;
					if (mordexRowLayoutState.GetRowIndex() + 1 > GetVerticalScrollBarControl().GetScrollValue() + GetVerticalScrollBarControl().GetLargeChange())
					{
						GetVerticalScrollBarControl().UpdateScrollValue(mordexRowLayoutState.GetRowIndex() + 1 - GetVerticalScrollBarControl().GetLargeChange());
					}
				}
				break;
			case Keys.Up:
				mordexRowLayoutState = GetPreviousGroupRowLayout();
				if (mordexRowLayoutState != null)
				{
					SelectGroupRowOnClick(mordexRowLayoutState);
					_focusedRowLayout = mordexRowLayoutState;
					_currentItem = null;
					if (mordexRowLayoutState.GetRowIndex() < GetVerticalScrollBarControl().GetScrollValue())
					{
						GetVerticalScrollBarControl().UpdateScrollValue(mordexRowLayoutState.GetRowIndex());
					}
				}
				break;
			}
		}

		private void OnCurrencyManagerItemChanged(object sender, ItemChangedEventArgs eventArgs)
		{
			if (eventArgs.Index == -1 && _timelineDataSourceManager.BindableRootList is DataBindableList dataBindableList && dataBindableList.List != _currencyManager.List)
			{
				_timelineDataSourceManager.BindableRootList = new DataBindableList(_currencyManager);
			}
		}

		internal void RaiseSelectedItemsChanged()
		{
			if (_shouldRaiseSelectedItemsChanged)
			{
				OnSelectedItemsChanged(EventArgs.Empty);
			}
		}

		protected virtual void OnSelectedItemsChanged(EventArgs e)
		{
			if (base.Events[_selectedItemsChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnEditingItem(ItemCancelEventArgs e)
		{
			if (base.Events[_editingItemEventKey] is ItemCancelEventHandler itemCancelEventHandler)
			{
				itemCancelEventHandler(this, e);
			}
		}

		protected virtual void OnFormattingGroupRow(FormattingGroupRowEventArgs e)
		{
			if (base.Events[_formattingGroupRowEventKey] is FormattingGroupRowEventHandler formattingGroupRowEventHandler)
			{
				formattingGroupRowEventHandler(this, e);
			}
		}

		protected virtual void OnItemRead(ItemEventArgs e)
		{
			if (base.Events[_itemReadEventKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		protected virtual void OnValidatingItemText(ValidatingItemTextEventArgs e)
		{
			if (base.Events[_validatingItemTextEventKey] is ValidatingItemTextEventHandler validatingItemTextEventHandler)
			{
				validatingItemTextEventHandler(this, e);
			}
		}

		protected virtual void OnItemDropped(ItemEventArgs e)
		{
			if (base.Events[_itemDroppedEventKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		protected virtual void OnDroppingItem(DroppingItemEventArgs e)
		{
			if (base.Events[_droppingItemEventKey] is DroppingItemEventHandler droppingItemEventHandler)
			{
				droppingItemEventHandler(this, e);
			}
		}

		protected virtual void OnItemDrag(ItemDragEventArgs e)
		{
			if (base.Events[_itemDragEventKey] is ItemDragEventHandler itemDragEventHandler)
			{
				itemDragEventHandler(this, e);
			}
		}

		internal bool RaiseAddingItem(TimelineItem item)
		{
			if (_isLoadingItems)
			{
				return true;
			}
			ItemCancelEventArgs e = new ItemCancelEventArgs(item);
			OnAddingItem(e);
			return !e.Cancel;
		}

		protected virtual void OnAddingItem(ItemCancelEventArgs e)
		{
			if (base.Events[_addingItemEventKey] is ItemCancelEventHandler itemCancelEventHandler)
			{
				itemCancelEventHandler(this, e);
			}
		}

		internal void RaiseItemChanging(ItemChangeCancelEventArgs eventArgs)
		{
			if (!_isLoadingItems)
			{
				OnItemChanging(eventArgs);
			}
		}

		protected virtual void OnItemChanging(ItemChangeCancelEventArgs e)
		{
			if (base.Events[_itemChangingEventKey] is ItemChangeCancelEventHandler itemChangeCancelEventHandler)
			{
				itemChangeCancelEventHandler(this, e);
			}
		}

		internal void RaiseItemChanged(TimelineItem item, ItemChange propertyChanged)
		{
			if (propertyChanged == ItemChange.StartTime || propertyChanged == ItemChange.EndTime)
			{
				_items?._index.Update(item);
			}
			if (!_isLoadingItems)
			{
				ItemChangeEventArgs e = new ItemChangeEventArgs(item, propertyChanged);
				OnItemChanged(e);
			}
		}

		protected virtual void OnItemChanged(ItemChangeEventArgs e)
		{
			if (base.Events[_itemChangedEventKey] is ItemChangeEventHandler itemChangeEventHandler)
			{
				itemChangeEventHandler(this, e);
			}
		}

		protected virtual void OnItemAdded(ItemEventArgs e)
		{
			if (base.Events[_itemAddedEventKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		internal void RaiseItemRemoved(TimelineItem item)
		{
			if (!_isLoadingItems)
			{
				ItemEventArgs e = new ItemEventArgs(item);
				OnItemRemoved(e);
			}
		}

		internal void ReleaseRowItem(MordexRow mordexRow)
		{
			TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(mordexRow);
			if (timelineItem != null)
			{
				if (SelectedItems.Contains(timelineItem))
				{
					SelectedItems.Remove(timelineItem);
				}
				if (timelineItem == CurrentItem)
				{
					CurrentItem = null;
				}
				if (timelineItem == GetEditedItem())
				{
					SetEditedItem(null);
				}
			}
		}

		internal void RaiseItemAdded(TimelineItem item)
		{
			if (!_isLoadingItems)
			{
				ItemEventArgs e = new ItemEventArgs(item);
				OnItemAdded(e);
			}
		}

		protected virtual void OnItemRemoved(ItemEventArgs e)
		{
			if (base.Events[_itemRemovedEventKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		internal bool RaiseRemovingItem(TimelineItem item)
		{
			if (_isLoadingItems)
			{
				return true;
			}
			ItemCancelEventArgs e = new ItemCancelEventArgs(item);
			OnRemovingItem(e);
			return !e.Cancel;
		}

		protected virtual void OnRemovingItem(ItemCancelEventArgs e)
		{
			if (base.Events[_removingItemEventKey] is ItemCancelEventHandler itemCancelEventHandler)
			{
				itemCancelEventHandler(this, e);
			}
		}

		protected virtual void OnItemEdited(ItemEventArgs e)
		{
			if (base.Events[_itemEditedEventKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		protected virtual void OnItemsLoaded(EventArgs e)
		{
			if (base.Events[_itemsLoadedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal void RaiseFormattingGroupRow(TimelineGroupRow groupRow)
		{
			FormattingGroupRowEventArgs e = new FormattingGroupRowEventArgs(groupRow);
			OnFormattingGroupRow(e);
		}

		public void ExpandGroups()
		{
			if (_areRowsDirty)
			{
				RebuildRows();
			}
			foreach (MxRowLayoutState item in _allRowLayouts)
			{
				if (item.GetRowType() == RowType.Group)
				{
					item.GetNode().Expanded = true;
				}
			}
			LayoutRows();
			Invalidate();
		}

		public void CollapseGroups()
		{
			if (_areRowsDirty)
			{
				RebuildRows();
			}
			foreach (MxRowLayoutState item in _allRowLayouts)
			{
				if (item.GetRowType() == RowType.Group)
				{
					item.GetNode().Expanded = false;
				}
			}
			LayoutRows();
			Invalidate();
		}

		public void CollapseGroup(TimelineGroup group)
		{
			_timelineDataSourceManager.CollapseGroup(group);
			if (_areRowsDirty)
			{
				Invalidate();
				return;
			}
			LayoutRows();
			Invalidate();
		}

		public void ExpandGroup(TimelineGroup group)
		{
			_timelineDataSourceManager.ExpandGroup(group);
			LayoutRows();
			Invalidate();
		}

		private int BuildRowLayouts(Graphics graphics, MordexRowCollection mordexRowCollection, MxRowLayoutState parentRow, int rowTop)
		{
			if (mordexRowCollection.Group == null)
			{
				rowTop = ((parentRow.GetNode() == null || parentRow.GetNode().Group == null) ? BuildRecordRowLayouts(mordexRowCollection, graphics, parentRow, 0, rowTop) : BuildRecordRowLayouts(mordexRowCollection, graphics, parentRow, parentRow.GetNode().Group.Index, rowTop));
			}
			else
			{
				foreach (MordexRow item in (IEnumerable)mordexRowCollection)
				{
					if (item.Visible)
					{
						MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(RowType.Group, item, parentRow);
						mordexRowLayoutState.SetGroupIndex(mordexRowCollection.Group.Index);
						_allRowLayouts.AddRowLayout(mordexRowLayoutState);
						rowTop = BuildRowLayouts(graphics, item.Rows, mordexRowLayoutState, rowTop);
					}
				}
			}
			return rowTop;
		}

		private int BuildRecordRowLayouts(MordexRowCollection mordexRowCollection, Graphics graphics, MxRowLayoutState parentRow, int groupIndex, int rowTop)
		{
			int recordRowCount = 0;
			TimelineItemLaneCollection lanes = new TimelineItemLaneCollection();
			float intervalPixelWidth = _intervalWidth;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			string text = "";
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_itemsFormatStyle);
			bool shouldUseSingleLane = ShowLastGroupAsRowHeader && Groups.Count > 0 && !parentRow.IsExpanded();
			if (!shouldUseSingleLane && _itemLayoutMode == ItemLayoutMode.AllItemsOnOneRow)
			{
				shouldUseSingleLane = true;
			}
			bool hasRowHeaders = ShowLastGroupAsRowHeader && Groups.Count > 0;
			int count = _allRowLayouts.Count;
			foreach (MordexRow item in (IEnumerable)mordexRowCollection)
			{
				if (!item.Visible)
				{
					continue;
				}
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(item);
				timelineItem.SetGroupIndex(groupIndex);
				object startTimeValue = timelineItem.StartTime;
				DateTime dateTime = timelineItem.EstimatedStartTime;
				DateTime estimatedEndTime = timelineItem.EstimatedEndTime;
				DateTime endTime = timelineItem.EndTime;
				if (startTimeValue == DBNull.Value)
				{
					continue;
				}
				DateTime startTime = DateTime.MinValue;
				try
				{
					startTime = (DateTime)startTimeValue;
				}
				catch
				{
				}
				double startOffset = GetIntervalDistance(_minDate, startTime);
				double estimatedStartOffset;
				if (dateTime != DateTime.MinValue)
				{
					estimatedStartOffset = GetIntervalDistance(_minDate, dateTime);
				}
				else
				{
					estimatedStartOffset = startOffset;
					dateTime = startTime;
				}
				if (_showLabel)
				{
					text = GetItemLabelText(timelineItem);
				}
				bool isNewLaneNeeded = true;
				if (shouldUseSingleLane || _itemLayoutMode == ItemLayoutMode.NewRowOnOverlap)
				{
					foreach (TimelineItemLane lane in lanes)
					{
						if (shouldUseSingleLane || (lane.GetDoubleValue() <= startOffset && lane.GetDoubleValue() <= estimatedStartOffset))
						{
							isNewLaneNeeded = false;
							double durationOffset = GetIntervalDistance(startTime, endTime);
							double estimatedDurationOffset = ((!(estimatedEndTime != DateTime.MinValue)) ? 0.0 : GetIntervalDistance(dateTime, estimatedEndTime));
							lane.SetDoubleValue(startOffset + (double)(MeasureItemWidth(graphics, text, timelineItem, textLayoutRenderer, timelineFormatStyle) / intervalPixelWidth));
							if (durationOffset < 0.0)
							{
								durationOffset = 0.0;
							}
							timelineItem._durationLength = durationOffset;
							timelineItem._estimatedDurationLength = estimatedDurationOffset;
							if (durationOffset + startOffset > lane.GetDoubleValue())
							{
								lane.SetDoubleValue(durationOffset + startOffset);
							}
							if (estimatedDurationOffset + estimatedStartOffset > lane.GetDoubleValue())
							{
								lane.SetDoubleValue(estimatedStartOffset + estimatedDurationOffset);
							}
							timelineItem._startOffset = startOffset;
							timelineItem._estimatedStartOffset = estimatedStartOffset;
							timelineItem.SetSubRowIndex(lane.GetLaneIndex());
							if (timelineItem._startOffset <= timelineItem._estimatedStartOffset)
							{
								timelineItem._leftOffset = timelineItem._startOffset;
							}
							else
							{
								timelineItem._leftOffset = timelineItem._estimatedStartOffset;
							}
							timelineItem._doubleValue = lane.GetDoubleValue();
							break;
						}
					}
				}
				if (isNewLaneNeeded)
				{
					recordRowCount++;
					MxRowLayoutState mordexRowLayoutState = new MxRowLayoutState(RowType.Record, groupIndex, parentRow);
					if (hasRowHeaders && parentRow != null)
					{
						MordexGroupRow mordexGroupRow = parentRow.GetNode() as MordexGroupRow;
						TimelineGroupRow timelineGroupRow = GetGroupRow(mordexGroupRow);
						mordexRowLayoutState.SetTimelineGroupRow(timelineGroupRow);
					}
					TimelineItemLane newLane = lanes.AddLane();
					double newDurationOffset = GetIntervalDistance(startTime, endTime);
					double newEstimatedDurationOffset = ((!(estimatedEndTime != DateTime.MinValue)) ? 0.0 : GetIntervalDistance(dateTime, estimatedEndTime));
					newLane.SetDoubleValue(startOffset + (double)(MeasureItemWidth(graphics, text, timelineItem, textLayoutRenderer, timelineFormatStyle) / intervalPixelWidth));
					if (newDurationOffset < 0.0)
					{
						newDurationOffset = 0.0;
					}
					timelineItem._durationLength = newDurationOffset;
					timelineItem._estimatedDurationLength = newEstimatedDurationOffset;
					if (newDurationOffset + startOffset > newLane.GetDoubleValue())
					{
						newLane.SetDoubleValue(newDurationOffset + startOffset);
					}
					if (newEstimatedDurationOffset + estimatedStartOffset > newLane.GetDoubleValue())
					{
						newLane.SetDoubleValue(estimatedStartOffset + newEstimatedDurationOffset);
					}
					timelineItem._startOffset = startOffset;
					timelineItem._estimatedStartOffset = estimatedStartOffset;
					timelineItem.SetSubRowIndex(newLane.GetLaneIndex());
					if (timelineItem._startOffset <= timelineItem._estimatedStartOffset)
					{
						timelineItem._leftOffset = timelineItem._startOffset;
					}
					else
					{
						timelineItem._leftOffset = timelineItem._estimatedStartOffset;
					}
					timelineItem._doubleValue = newLane.GetDoubleValue();
					_ = base.ClientRectangle.Left;
					_allRowLayouts.AddRowLayout(mordexRowLayoutState);
					rowTop += RowHeight;
				}
			}
			if (hasRowHeaders)
			{
				MxRowLayoutState mordexRowLayoutStateArgument = _allRowLayouts.GetRowLayoutAt(count);
				int rowHeaderWidth = RowHeaderWidth;
				int x = base.ClientRectangle.Left + _groupIndent * groupIndex;
				Rectangle rectangle = new Rectangle(x, 0, rowHeaderWidth, RowHeight * (_allRowLayouts.Count - count) - (_allRowLayouts.Count - (count + 1)));
				mordexRowLayoutStateArgument.SetMordexRowLayoutStateRectangle(rectangle);
				for (int i = count + 1; i < _allRowLayouts.Count; i++)
				{
					MxRowLayoutState childRowLayout = _allRowLayouts.GetRowLayoutAt(i);
					childRowLayout.SetMordexRowLayoutStateRectangle(Rectangle.Empty);
				}
			}
			parentRow.SetRecordRowCount(recordRowCount);
			timelineFormatStyle.DisposeResources();
			return rowTop;
		}

		private void SelectPreviousItem(MxRowLayoutState mordexRowLayoutState, DateTime dateTime, int recordPosition, int integerArgument)
		{
			MordexRow mordexRow = null;
			MordexRow targetRow = null;
			mordexRow = ((mordexRowLayoutState == null || mordexRowLayoutState.GetNode() == null) ? GetTimelineDataSourceManager().RootRow : mordexRowLayoutState.GetNode());
			MordexRow[] dataRows = mordexRow.GetDataRows((MordexTableBase)_timelineDataSourceManager.RootTable);
			for (int i = dataRows.Length - 1; i >= 0; i--)
			{
				MordexRow candidateRow = dataRows[i];
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(candidateRow);
				if (timelineItem != null && dateTime >= timelineItem.StartTime && (!(dateTime == timelineItem.StartTime) || (timelineItem.GetRecordPosition() != recordPosition && timelineItem.GetSubRowIndex() <= integerArgument)))
				{
					targetRow = candidateRow;
					break;
				}
			}
			if (targetRow != null)
			{
				TimelineItem targetItem = TimelineDataSourceManager.GetTimelineItem(targetRow);
				if (targetItem != null && targetItem.StartTime > MinDate && targetItem.StartTime < MaxDate)
				{
					_currentItem = targetItem;
					SelectItemOnClick(_currentItem);
					ScrollRowIntoView(targetRow);
					Update();
					_currentItem = targetItem;
					_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
					return;
				}
			}
			MxRowLayoutState mordexRowLayoutStateArgument = GetPreviousGroupRowLayout();
			if (mordexRowLayoutStateArgument != null)
			{
				_focusedRowLayout = mordexRowLayoutStateArgument;
				_currentItem = null;
				SelectGroupRowOnClick(_focusedRowLayout);
				ScrollRowIntoView(_focusedRowLayout.GetNode());
			}
		}

		private void SelectPreviousItemInGroup(MxRowLayoutState mordexRowLayoutState, DateTime dateTime, int recordPosition, int integerArgument, object valueArgument)
		{
			MordexRow mordexRow = null;
			MordexRow targetRow = null;
			mordexRow = ((mordexRowLayoutState == null || mordexRowLayoutState.GetNode() == null) ? GetTimelineDataSourceManager().RootRow : mordexRowLayoutState.GetNode());
			MordexRow[] dataRows = mordexRow.GetDataRows((MordexTableBase)_timelineDataSourceManager.RootTable);
			for (int i = dataRows.Length - 1; i >= 0; i--)
			{
				MordexRow candidateRow = dataRows[i];
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(candidateRow);
				if (timelineItem != null)
				{
					object value = timelineItem.GetValue(Groups[Groups.Count - 1].Field);
					if ((valueArgument == null || valueArgument.Equals(value)) && (valueArgument != null || valueArgument == value) && dateTime >= timelineItem.StartTime && (!(dateTime == timelineItem.StartTime) || (timelineItem.GetRecordPosition() != recordPosition && timelineItem.GetSubRowIndex() <= integerArgument)))
					{
						targetRow = candidateRow;
						break;
					}
				}
			}
			if (targetRow != null)
			{
				TimelineItem targetItem = TimelineDataSourceManager.GetTimelineItem(targetRow);
				if (targetItem != null && targetItem.StartTime > MinDate && targetItem.StartTime < MaxDate)
				{
					_currentItem = targetItem;
					SelectItemOnClick(_currentItem);
					ScrollRowIntoView(targetRow);
					Update();
					_currentItem = targetItem;
					_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
					return;
				}
			}
			MxRowLayoutState mordexRowLayoutStateArgument = GetPreviousGroupRowLayout();
			if (mordexRowLayoutStateArgument != null)
			{
				_currentItem = null;
				_focusedRowLayout = mordexRowLayoutStateArgument;
				SelectGroupRowOnClick(_focusedRowLayout);
				ScrollRowIntoView(_focusedRowLayout.GetNode());
			}
		}

		private void SelectNextItemInGroup(MxRowLayoutState mordexRowLayoutState, DateTime dateTime, int recordPosition, int integerArgument, object valueArgument)
		{
			MordexRow mordexRow = null;
			MordexRow targetRow = null;
			mordexRow = ((mordexRowLayoutState != null && mordexRowLayoutState.GetNode() != null) ? mordexRowLayoutState.GetNode() : ((mordexRowLayoutState == null || mordexRowLayoutState.GetTimelineGroupRow() == null) ? GetTimelineDataSourceManager().RootRow : mordexRowLayoutState.GetParentRow().GetNode()));
			MordexRow[] dataRows = mordexRow.GetDataRows((MordexTableBase)_timelineDataSourceManager.RootTable);
			foreach (MordexRow candidateRow in dataRows)
			{
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(candidateRow);
				if (timelineItem != null)
				{
					object value = timelineItem.GetValue(Groups[Groups.Count - 1].Field);
					if ((valueArgument == null || valueArgument.Equals(value)) && (valueArgument != null || valueArgument == value) && dateTime <= timelineItem.StartTime && (!(dateTime == timelineItem.StartTime) || (timelineItem.GetRecordPosition() != recordPosition && timelineItem.GetSubRowIndex() >= integerArgument)))
					{
						targetRow = candidateRow;
						break;
					}
				}
			}
			if (targetRow != null)
			{
				TimelineItem targetItem = (_currentItem = TimelineDataSourceManager.GetTimelineItem(targetRow));
				if (targetItem != null && targetItem.StartTime > MinDate && targetItem.EndTime < MaxDate)
				{
					SelectItemOnClick(_currentItem);
					ScrollRowIntoView(targetRow);
					Update();
					_currentItem = targetItem;
					_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
					return;
				}
			}
			foreach (MordexRow fallbackRow in dataRows)
			{
				TimelineItem fallbackItem = TimelineDataSourceManager.GetTimelineItem(fallbackRow);
				if (fallbackItem != null && fallbackItem.StartTime > MinDate)
				{
					_currentItem = fallbackItem;
					SelectItemOnClick(_currentItem);
					ScrollRowIntoView(fallbackRow);
					Update();
					_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
					return;
				}
			}
			_focusedRowLayout = GetNextGroupRowLayout();
			if (_focusedRowLayout != null)
			{
				_currentItem = null;
				SelectGroupRowOnClick(_focusedRowLayout);
				ScrollRowIntoView(_focusedRowLayout.GetNode());
			}
		}

		private void SelectNextItem(MxRowLayoutState mordexRowLayoutState, DateTime dateTime, int recordPosition, int integerArgument)
		{
			MordexRow mordexRow = null;
			MordexRow targetRow = null;
			mordexRow = ((mordexRowLayoutState == null || mordexRowLayoutState.GetNode() == null) ? GetTimelineDataSourceManager().RootRow : mordexRowLayoutState.GetNode());
			MordexRow[] dataRows = mordexRow.GetDataRows((MordexTableBase)_timelineDataSourceManager.RootTable);
			foreach (MordexRow candidateRow in dataRows)
			{
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(candidateRow);
				if (timelineItem != null && dateTime <= timelineItem.StartTime && (!(dateTime == timelineItem.StartTime) || (timelineItem.GetRecordPosition() != recordPosition && timelineItem.GetSubRowIndex() >= integerArgument)))
				{
					targetRow = candidateRow;
					break;
				}
			}
			if (targetRow != null)
			{
				TimelineItem targetItem = TimelineDataSourceManager.GetTimelineItem(targetRow);
				if (targetItem != null && targetItem.StartTime > MinDate && targetItem.StartTime < MaxDate)
				{
					_currentItem = targetItem;
					SelectItemOnClick(_currentItem);
					ScrollRowIntoView(targetRow);
					Update();
					_currentItem = targetItem;
					if (_currentItem != null)
					{
						_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
					}
					return;
				}
			}
			foreach (MordexRow fallbackRow in dataRows)
			{
				TimelineItem fallbackItem = TimelineDataSourceManager.GetTimelineItem(fallbackRow);
				if (fallbackItem != null && fallbackItem.StartTime > MinDate)
				{
					_currentItem = fallbackItem;
					SelectItemOnClick(_currentItem);
					ScrollRowIntoView(fallbackRow);
					Update();
					_currentItem = fallbackItem;
					if (_currentItem != null)
					{
						_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
					}
					return;
				}
			}
			_focusedRowLayout = GetNextGroupRowLayout();
			if (_focusedRowLayout != null)
			{
				_currentItem = null;
				SelectGroupRowOnClick(_focusedRowLayout);
				ScrollRowIntoView(_focusedRowLayout.GetNode());
			}
		}

		private void ScrollRowIntoView(MordexRow mordexRow)
		{
			TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(mordexRow);
			if (timelineItem == null || !timelineItem._row.Visible)
			{
				return;
			}
			mordexRow.EnsureVisible();
			int lastRowIndex = timelineItem.GetRowIndex() + timelineItem.GetSubRowIndex();
			if (GetVerticalScrollBarControl().GetScrollValue() + GetVerticalScrollBarControl().GetLargeChange() < lastRowIndex + 1)
			{
				GetVerticalScrollBarControl().UpdateScrollValue(lastRowIndex + 1 - GetVerticalScrollBarControl().GetLargeChange());
			}
			else if (GetVerticalScrollBarControl().GetScrollValue() > lastRowIndex)
			{
				GetVerticalScrollBarControl().UpdateScrollValue(lastRowIndex);
			}
			long leftOffsetSeconds = 0;
			if (BottomTier.Interval == TimelineInterval.Year || BottomTier.Interval == TimelineInterval.Month)
			{
				EnsureVisible(timelineItem.StartTime);
			}
			else
			{
				if (timelineItem.GetGroupIndex() > 0)
				{
					leftOffsetSeconds = (long)((double)(_groupIndent * timelineItem.GetGroupIndex()) / (double)_intervalWidth * (double)_intervalSeconds) + 1;
					if (_showLastGroupAsRowHeader)
					{
						leftOffsetSeconds += (long)((double)_rowHeaderWidth / (double)_intervalWidth * (double)_intervalSeconds) + 1;
					}
				}
				else if (Groups.Count > 0 && ShowLastGroupAsRowHeader)
				{
					leftOffsetSeconds += (long)((double)_rowHeaderWidth / (double)_intervalWidth * (double)_intervalSeconds) + 1;
				}
				DateTime startTime = timelineItem.StartTime;
				if (startTime > DateTime.MinValue)
				{
					startTime = AlignToInterval(startTime, BottomTier.Interval, BottomTier.Count).AddSeconds(-leftOffsetSeconds);
					FirstDate = startTime;
				}
				else
				{
					FirstDate = _minDate;
				}
			}
			SetFlashingItem(timelineItem);
		}

		private void RestoreDraggedItems()
		{
			foreach (TimelineItem item in _draggedItems)
			{
				item.UpdateTimelineItem();
			}
			LayoutVisibleItems();
		}

		internal void DisposeItemEditor()
		{
			_editingItem = null;
			_timelineFormatStyleControl.Dispose();
			_timelineFormatStyleControl = null;
		}

		internal void SelectGroupRowOnClick(MxRowLayoutState mordexRowLayoutState)
		{
			if (mordexRowLayoutState.GetRowType() != RowType.Group)
			{
				return;
			}
			TimelineGroupRow timelineGroupRow = GetGroupRow(mordexRowLayoutState.GetNode());
			if (_multiSelect && (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.Shift))
			{
				if (SelectedItems.Contains(timelineGroupRow))
				{
					RemoveFromSelection(timelineGroupRow);
				}
				else
				{
					UpdateTimelineSelectedItemCollection(timelineGroupRow);
				}
			}
			else
			{
				if (mordexRowLayoutState.IsGroupSelected() && _selectedItems.Count == 1)
				{
					return;
				}
				if (_selectedItems != null)
				{
					_shouldRaiseSelectedItemsChanged = false;
					_selectedItems.Clear();
					_shouldRaiseSelectedItemsChanged = true;
				}
				UpdateTimelineSelectedItemCollection(timelineGroupRow);
			}
			Invalidate();
		}

		internal void SelectItemOnClick(TimelineItem timelineItem)
		{
			if (_multiSelect && (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.Shift))
			{
				timelineItem.Selected = !timelineItem.Selected;
			}
			else if (timelineItem._selected && _selectedItems.Count == 1)
			{
				if (AllowEdit)
				{
					SetEditedItem(timelineItem);
				}
			}
			else
			{
				if (_selectedItems != null)
				{
					_shouldRaiseSelectedItemsChanged = false;
					_selectedItems.Clear();
					_shouldRaiseSelectedItemsChanged = true;
				}
				UpdateTimelineSelectedItemCollection(timelineItem);
			}
			Invalidate();
		}

		internal bool RaiseEditingItem(TimelineItem item)
		{
			ItemCancelEventArgs e = new ItemCancelEventArgs(item);
			OnEditingItem(e);
			return !e.Cancel;
		}

		internal void RaiseItemEdited(TimelineItem item)
		{
			ItemEventArgs e = new ItemEventArgs(item);
			OnItemEdited(e);
		}

		private void OnItemTimerTick(object sender, EventArgs eventArgs)
		{
			if (_editingItem != null)
			{
				StopItemTimer();
				if (!AllowEdit)
				{
					return;
				}
				if (RaiseEditingItem(_editingItem))
				{
					_timelineFormatStyleControl = new TimelineFormatStyleControl(this, _editingItem, _useDefaultMenu);
					base.Controls.Add(_timelineFormatStyleControl);
					Rectangle bounds = TimelineUtilities.RoundToRectangle(_editingItem._rectangleF);
					int itemsLeft = GetItemsLeft(base.ClientRectangle.Left);
					if (bounds.X < itemsLeft)
					{
						int leftClip = itemsLeft - bounds.X;
						_timelineFormatStyleControl._hasLeftBorder = false;
						bounds.Width -= leftClip;
						bounds.X = itemsLeft;
					}
					if (bounds.Right > base.ClientRectangle.Right)
					{
						int rightClip = bounds.Right - base.ClientRectangle.Right;
						_timelineFormatStyleControl._hasRightBorder = false;
						bounds.Width -= rightClip;
					}
					_timelineFormatStyleControl.Bounds = bounds;
				}
				else
				{
					Invalidate();
				}
			}
			else if (_blinkingItem != null)
			{
				_blinkingItem.SetLabelVisible(!_blinkingItem.IsLabelVisible());
				Invalidate(TimelineUtilities.RoundToRectangle(_blinkingItem._rectangleF));
				_blinkTickCount++;
				if (_blinkTickCount > 4)
				{
					StopItemTimer();
				}
			}
		}

		private void StopItemTimer()
		{
			if (_timelineTimer != null && _timelineTimer.Enabled)
			{
				_timelineTimer.Stop();
				_timelineTimer.Interval = SystemInformation.DoubleClickTime;
			}
			SetFlashingItem(null);
		}

		private static object GetSourceRowKey(MordexRow mordexRow)
		{
			object source = mordexRow.DataRow;
			return source is DataRowView dataRowView ? dataRowView.Row : source;
		}

		private void RememberSourceRowItem(MordexRow mordexRow, TimelineItem item)
		{
			object key = GetSourceRowKey(mordexRow);
			if (key != null && item != null)
			{
				_itemsBySourceRow.Remove(key);
				_itemsBySourceRow.Add(key, item);
			}
		}

		internal void AttachItemToRow(MordexRow mordexRow)
		{
			TimelineItemState timelineItemState = (TimelineItemState)mordexRow.Tag;
			if (timelineItemState._timelineItem == null)
			{
				if (_timelineItem != null)
				{
					_timelineItem._row = mordexRow as DataHolderRow;
					timelineItemState._timelineItem = _timelineItem;
				}
				else if (GetSourceRowKey(mordexRow) is object sourceKey && _itemsBySourceRow.TryGetValue(sourceKey, out TimelineItem knownItem))
				{
					// The data source was bound again: the item keeps its identity and format styles.
					knownItem._row = mordexRow as DataHolderRow;
					knownItem._owner = _items;
					timelineItemState._timelineItem = knownItem;
				}
				else
				{
					timelineItemState._timelineItem = new TimelineItem(mordexRow as DataHolderRow);
					timelineItemState._timelineItem._owner = _items;
					OnItemRead(new ItemEventArgs(timelineItemState._timelineItem));
				}
				RememberSourceRowItem(mordexRow, timelineItemState._timelineItem);
			}
		}
	}
}
