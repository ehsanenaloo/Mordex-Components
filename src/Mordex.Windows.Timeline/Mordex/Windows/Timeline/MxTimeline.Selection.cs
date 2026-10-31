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
		void ITimelineDropDownOwner.OnDropListHide(bool cancel)
		{
			HandleDropDownClosed(cancel);
		}

		protected virtual void OnCalendarDropDown(EventArgs e)
		{
			if (base.Events[_dropDownEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[_currentLayoutChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnSelectedRangeChanged(EventArgs e)
		{
			if (base.Events[_selectedRangeChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[_currentLayoutChangingEventKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		internal void RemoveFromSelection(ITimelineObject item)
		{
			if (_selectedItems != null && _selectedItems.Contains(item))
			{
				if (item.Item != null)
				{
					item.Item._selected = false;
				}
				_selectedItems.Remove(item);
			}
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
		{
			if (e.EscapePressed)
			{
				e.Action = DragAction.Cancel;
			}
			else
			{
				Point point = PointToClient(Cursor.Position);
				AutoScrollNearEdge(point);
				DateTime dateTime = GetDateTimeAt(point);
				if (dateTime == DateTime.MinValue)
				{
					RestoreDraggedItems();
				}
				else
				{
					if (_forceDropOnIntervals)
					{
						dateTime = AlignToInterval(dateTime, BottomTier.Interval, BottomTier.Count);
					}
					switch (_itemDragType)
					{
					case ItemDragType.Move:
						if ((AllowItemDrag & AllowItemDrag.Move) == 0)
						{
							dateTime = _dragItem.GetSourceItem().StartTime;
							_dragGrabOffset = 0.0;
						}
						MoveDraggedItems(dateTime, point);
						LayoutVisibleItems();
						break;
					case ItemDragType.EndTimeChange:
						if (_forceDropOnIntervals)
						{
							dateTime = dateTime.AddSeconds(GetBottomTierStepSeconds(dateTime, false));
						}
						if (dateTime < _dragItem.StartTime)
						{
							dateTime = _dragItem.StartTime;
						}
						UpdateEndDrag(dateTime, point);
						break;
					case ItemDragType.StartTimeChange:
						if (dateTime > _dragItem.EndTime)
						{
							dateTime = _dragItem.EndTime;
						}
						UpdateStartDrag(dateTime, point);
						break;
					case ItemDragType.EstimatedStartTimeChange:
						if (dateTime > _dragItem.EstimatedEndTime)
						{
							if (_dragItem.EstimatedEndTime == DateTime.MinValue)
							{
								_dragItem.EstimatedEndTime = _dragItem.StartTime;
							}
							else
							{
								dateTime = _dragItem.EstimatedEndTime;
							}
						}
						UpdateEstimatedStartDrag(dateTime, point);
						break;
					case ItemDragType.EstimatedEndTimeChange:
						if (_forceDropOnIntervals)
						{
							dateTime = dateTime.AddSeconds(GetBottomTierStepSeconds(dateTime, false));
						}
						if (_dragItem.EstimatedStartTime == DateTime.MinValue)
						{
							_dragItem.EstimatedStartTime = _dragItem.StartTime;
						}
						if (dateTime < _dragItem.EstimatedStartTime)
						{
							dateTime = _dragItem.EstimatedStartTime;
						}
						UpdateEstimatedEndDrag(dateTime, point);
						break;
					}
					Invalidate();
				}
			}
			if (e.Action == DragAction.Drop)
			{
				_isDraggingItems = false;
				if (base.ClientRectangle.Contains(PointToClient(Cursor.Position)))
				{
					_isDropPending = true;
				}
			}
			base.OnQueryContinueDrag(e);
		}

		private void CommitDrop(Point point)
		{
			GetRowLayoutByIndex(_dragItem.GetRowIndex());
			switch (_itemDragType)
			{
			case ItemDragType.Move:
			{
				TimelineGroupRow timelineGroupRow = GetGroupRowAt(new Point(point.X, (int)_dragItem._rectangleF.Y));
				if (!CanChangeItemGroup(timelineGroupRow))
				{
					timelineGroupRow = null;
				}
				LayoutVisibleItems();
				{
					foreach (TimelineItem item in _draggedItems)
					{
						DateTime dateTime;
						DateTime estimatedStart;
						if (_forceDropOnIntervals)
						{
							dateTime = GetDateAtX(item._durationBarBounds.X + 1);
							estimatedStart = GetDateAtX(item._estimatedBarBounds.X + 1);
							dateTime = AlignToInterval(dateTime, BottomTier.Interval, BottomTier.Count);
							estimatedStart = AlignToInterval(estimatedStart, BottomTier.Interval, BottomTier.Count);
						}
						else
						{
							dateTime = GetDateAtX(item._durationBarBounds.X);
							estimatedStart = GetDateAtX(item._estimatedBarBounds.X);
						}
						DateTime end = dateTime + item.Duration;
						DateTime estimatedEnd = estimatedStart + item.GetEstimatedDuration();
						DroppingItemEventArgs e = new DroppingItemEventArgs(item.GetSourceItem(), dateTime, end, timelineGroupRow);
						OnDroppingItem(e);
						if (e.Cancel)
						{
							continue;
						}
						if (timelineGroupRow != null)
						{
							ApplyGroupValues(timelineGroupRow, item.GetSourceItem());
						}
						if (item.EstimatedStartTime != DateTime.MinValue)
						{
							if (estimatedEnd > item.GetSourceItem().EstimatedStartTime)
							{
								item.GetSourceItem().EstimatedEndTime = estimatedEnd;
								item.GetSourceItem().EstimatedStartTime = estimatedStart;
							}
							else
							{
								item.GetSourceItem().EstimatedStartTime = estimatedStart;
								item.GetSourceItem().EstimatedEndTime = estimatedEnd;
							}
						}
						item.GetSourceItem().Move(dateTime, end);
						ItemEventArgs droppedArgs = new ItemEventArgs(item.GetSourceItem());
						OnItemDropped(droppedArgs);
					}
					break;
				}
			}
			case ItemDragType.EndTimeChange:
				_dragItem.GetSourceItem().EndTime = _dragItem.EndTime;
				break;
			case ItemDragType.StartTimeChange:
				_dragItem.GetSourceItem().StartTime = _dragItem.StartTime;
				break;
			case ItemDragType.EstimatedStartTimeChange:
				if (_dragItem.GetSourceItem().EstimatedEndTime == DateTime.MinValue)
				{
					_dragItem.GetSourceItem().EstimatedEndTime = _dragItem.EstimatedEndTime;
				}
				_dragItem.GetSourceItem().EstimatedStartTime = _dragItem.EstimatedStartTime;
				break;
			case ItemDragType.EstimatedEndTimeChange:
				if (_dragItem.GetSourceItem().EstimatedStartTime == DateTime.MinValue)
				{
					_dragItem.GetSourceItem().EstimatedStartTime = _dragItem.EstimatedStartTime;
				}
				_dragItem.GetSourceItem().EstimatedEndTime = _dragItem.EstimatedEndTime;
				break;
			}
		}

		private void UpdateEstimatedEndDrag(DateTime end, Point pt)
		{
			TimelineGroupRow groupRowAt = GetGroupRowAt(pt);
			DraggingItemEventArgs e = new DraggingItemEventArgs(_dragItem.GetSourceItem(), _itemDragType, _dragItem.GetSourceItem().EstimatedStartTime, end, groupRowAt);
			OnDraggingItem(e);
			if (!e.Cancel)
			{
				double durationOffset = GetIntervalDistance(_dragItem.EstimatedStartTime, end);
				_dragItem.EstimatedEndTime = end;
				_dragItem._estimatedDurationLength = durationOffset;
				_dragItem._estimatedDurationWidth = durationOffset * (double)_intervalWidth;
				float barLeft = (float)base.ClientRectangle.Left + (float)((_dragItem._estimatedStartOffset - _doubleValue) * (double)_intervalWidth);
				_dragItem.UpdateTimelineItemRectangle(barLeft, ItemsDurationBarHeight);
			}
		}

		private void UpdateEstimatedStartDrag(DateTime start, Point pt)
		{
			TimelineGroupRow groupRowAt = GetGroupRowAt(pt);
			DraggingItemEventArgs e = new DraggingItemEventArgs(_dragItem.GetSourceItem(), _itemDragType, start, _dragItem.GetSourceItem().EstimatedEndTime, groupRowAt);
			OnDraggingItem(e);
			if (!e.Cancel)
			{
				_dragItem.EstimatedStartTime = start;
				double startOffset = GetIntervalDistance(_minDate, start);
				float barLeft = (float)base.ClientRectangle.Left + (float)((startOffset - _doubleValue) * (double)_intervalWidth);
				_dragItem._estimatedStartOffset = startOffset;
				double durationOffset = GetIntervalDistance(start, _dragItem.EstimatedEndTime);
				_dragItem._estimatedDurationLength = durationOffset;
				_dragItem._estimatedDurationWidth = durationOffset * (double)_intervalWidth;
				_dragItem.UpdateTimelineItemRectangle(barLeft, ItemsDurationBarHeight);
			}
		}

		private void UpdateEndDrag(DateTime end, Point pt)
		{
			TimelineGroupRow groupRowAt = GetGroupRowAt(pt);
			DraggingItemEventArgs e = new DraggingItemEventArgs(_dragItem.GetSourceItem(), _itemDragType, _dragItem.GetSourceItem().StartTime, end, groupRowAt);
			OnDraggingItem(e);
			if (!e.Cancel)
			{
				double durationOffset = GetIntervalDistance(_dragItem.StartTime, end);
				_dragItem.EndTime = end;
				_dragItem._durationLength = durationOffset;
				_dragItem._durationWidth = durationOffset * (double)_intervalWidth;
				float barLeft = (float)base.ClientRectangle.Left + (float)((_dragItem._startOffset - _doubleValue) * (double)_intervalWidth);
				_dragItem.UpdateDurationBarBounds(barLeft, ItemsDurationBarHeight);
			}
		}

		private void UpdateStartDrag(DateTime start, Point pt)
		{
			TimelineGroupRow groupRowAt = GetGroupRowAt(pt);
			DraggingItemEventArgs e = new DraggingItemEventArgs(_dragItem.GetSourceItem(), _itemDragType, start, _dragItem.GetSourceItem().EndTime, groupRowAt);
			OnDraggingItem(e);
			if (!e.Cancel)
			{
				_dragItem.StartTime = start;
				double startOffset = GetIntervalDistance(_minDate, start);
				float barLeft = (float)base.ClientRectangle.Left + (float)((startOffset - _doubleValue) * (double)_intervalWidth);
				_dragItem._startOffset = startOffset;
				double durationUnits = GetIntervalDistance(start, _dragItem.EndTime);
				_dragItem._durationLength = durationUnits;
				_dragItem._durationWidth = durationUnits * (double)_intervalWidth;
				_dragItem.UpdateDurationBarBounds(barLeft, ItemsDurationBarHeight);
			}
		}

		private FocusZone GetNextFocusZone(FocusZone focusZone)
		{
			switch (focusZone)
			{
			case FocusZone.Items:
				if (TimescaleTiers == TimescaleTiers.ThreeTiers)
				{
					return FocusZone.TopTierScale;
				}
				return GetNextFocusZone(FocusZone.TopTierScale);
			case FocusZone.TopTierScale:
				if (TimescaleTiers != TimescaleTiers.OneTier)
				{
					return FocusZone.MiddleTierScale;
				}
				return GetNextFocusZone(FocusZone.MiddleTierScale);
			case FocusZone.MiddleTierScale:
				return FocusZone.BottomTierScale;
			case FocusZone.BottomTierScale:
				return FocusZone.Items;
			default:
				return FocusZone.BottomTierScale;
			}
		}

		private void MoveFocusToNextZone()
		{
			FocusZone focusZone = GetNextFocusZone(_focusZone);
			SetFocusZone(focusZone);
			switch (focusZone)
			{
			case FocusZone.MiddleTierScale:
				_isSelectingRange = false;
				_selectedItems.Clear();
				UpdateTimelineDateTime(AlignToInterval(_firstDate, MiddleTier.Interval, MiddleTier.Count));
				break;
			case FocusZone.TopTierScale:
				_isSelectingRange = false;
				_selectedItems.Clear();
				UpdateTimelineDateTime(AlignToInterval(_firstDate, TopTier.Interval, TopTier.Count));
				break;
			case FocusZone.Items:
				SelectedRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
				_selectionAnchorDate = DateTime.MinValue;
				_isSelectingRange = false;
				if (Groups.Count > 0 && _firstVisibleRowLayout != null)
				{
					MxRowLayoutState targetRow = _firstVisibleRowLayout;
					if (targetRow.GetRowType() != RowType.Group)
					{
						_focusedRowLayout = targetRow;
						targetRow = GetPreviousGroupRowLayout();
						if (targetRow == null)
						{
							SelectNextItem(null, _firstDate, _firstVisibleRowLayout.GetRowIndex(), -1);
							return;
						}
						if (targetRow.GetRowIndex() < GetVerticalScrollBarControl().GetScrollValue())
						{
							GetVerticalScrollBarControl().UpdateScrollValue(targetRow.GetRowIndex());
						}
					}
					_currentItem = null;
					_focusedRowLayout = targetRow;
					SelectGroupRowOnClick(targetRow);
				}
				else if (_firstVisibleRowLayout != null)
				{
					SelectNextItem(null, _firstDate, _firstVisibleRowLayout.GetRowIndex(), -1);
				}
				break;
			case FocusZone.BottomTierScale:
				_isSelectingRange = false;
				UpdateTimelineDateTime(AlignToInterval(_firstDate, BottomTier.Interval, BottomTier.Count));
				break;
			}
			Invalidate();
		}

		private void ShowDropDownCalendar(bool captureMouse)
		{
			DropDownCalendar.ApplyTheme();
			_dropDownCalendar.UpdateDayOfWeek(FirstDayOfWeek);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Width = 200;
			clientRectangle.Location = Cursor.Position;
			try
			{
				_dropDownCalendar.CurrentDate = _firstDate;
			}
			catch
			{
			}
			_isDropDownOpen = true;
			_dropDownCalendar.Drop(clientRectangle, this);
			TimelineUtilities.SetControlCapture(_dropDownCalendar, captureMouse);
			OnCalendarDropDown(EventArgs.Empty);
		}

		private long GetSelectionStepSeconds(bool usePreviousPeriod)
		{
			if (_selectedRange.Start == DateTime.MinValue)
			{
				if (_currentItem != null)
				{
					return GetZoneStepSeconds(_currentItem.StartTime, usePreviousPeriod);
				}
				return GetZoneStepSeconds(DateTime.Today, usePreviousPeriod);
			}
			return GetZoneStepSeconds(_selectedRange.Start, usePreviousPeriod);
		}

		private RectangleF GetSelectedRangeBounds(Rectangle rectangle)
		{
			double selectionLeft = (GetIntervalDistance(_minDate, _selectedRange.Start) - _doubleValue) * (double)_intervalWidth + (double)base.ClientRectangle.Left;
			double selectionWidth = 0.0;
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				selectionWidth = (SelectedRange.End.Year - SelectedRange.Start.Year) * _intervalWidth;
				if (SelectedRange.End.DayOfYear != SelectedRange.Start.DayOfYear)
				{
					selectionWidth += (double)(SelectedRange.End.DayOfYear - (SelectedRange.Start.DayOfYear - 1)) / (double)GetDaysInYear(SelectedRange.End.Year) * (double)_intervalWidth;
				}
			}
			else if (BottomTier.Interval == TimelineInterval.Month)
			{
				selectionWidth = (SelectedRange.End.Year - SelectedRange.Start.Year) * 12 * _intervalWidth;
				selectionWidth += (double)(SelectedRange.End.Month - SelectedRange.Start.Month) * (double)_intervalWidth;
				if (SelectedRange.End.Day != SelectedRange.Start.Day)
				{
					selectionWidth += (double)(SelectedRange.End.Day - (SelectedRange.Start.Day - 1)) / (double)DateTime.DaysInMonth(SelectedRange.End.Year, SelectedRange.End.Month) * (double)_intervalWidth;
				}
			}
			else
			{
				selectionWidth = (double)GetSelectionDurationSeconds() / (double)_intervalSeconds * (double)_intervalWidth;
			}
			return new RectangleF((float)selectionLeft, rectangle.Y, (float)selectionWidth, rectangle.Height);
		}

		private FocusZone GetFocusZone()
		{
			return _focusZone;
		}

		private void SetFocusZone(FocusZone focusZone)
		{
			if (_focusZone != focusZone)
			{
				switch (_focusZone)
				{
				case FocusZone.Items:
					_currentItem = null;
					_focusedRowLayout = null;
					break;
				case FocusZone.BottomTierScale:
					BottomTier.SetHasFocus(false);
					break;
				case FocusZone.MiddleTierScale:
					MiddleTier.SetHasFocus(false);
					break;
				case FocusZone.TopTierScale:
					TopTier.SetHasFocus(false);
					break;
				}
				_focusZone = focusZone;
				switch (_focusZone)
				{
				case FocusZone.BottomTierScale:
					BottomTier.SetHasFocus(true);
					break;
				case FocusZone.MiddleTierScale:
					MiddleTier.SetHasFocus(true);
					break;
				case FocusZone.TopTierScale:
					TopTier.SetHasFocus(true);
					break;
				}
			}
		}
	}
}
