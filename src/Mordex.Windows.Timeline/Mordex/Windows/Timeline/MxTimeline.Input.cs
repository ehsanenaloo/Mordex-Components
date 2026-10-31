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
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled)
			{
				return;
			}
			if (GetFocusZone() == FocusZone.Items && (_focusedRowLayout != null || _currentItem != null))
			{
				_ = _focusedRowLayout;
				NavigateRowsWithKey(e.KeyData);
			}
			else
			{
				if (!(_selectionAnchorDate != DateTime.MinValue))
				{
					return;
				}
				TimeSpan timeSpan = TimeSpan.Zero;
				switch (e.KeyData)
				{
				case Keys.Left:
				case Keys.Down:
					timeSpan = TimeSpan.FromSeconds(-GetSelectionStepSeconds(true));
					break;
				case Keys.Up:
				case Keys.Right:
					timeSpan = TimeSpan.FromSeconds(GetSelectionStepSeconds(false));
					break;
				}
				if (timeSpan != TimeSpan.Zero)
				{
					DateTime dateTime = _selectionAnchorDate.Add(timeSpan);
					DateTime selectionEnd = dateTime.AddSeconds(GetZoneStepSeconds(dateTime, false));
					_selectionAnchorDate = dateTime;
					if (dateTime < _firstDate)
					{
						FirstDate = dateTime;
					}
					else if (selectionEnd >= _lastVisibleDate)
					{
						FirstDate = _firstDate.AddSeconds(GetSelectionStepSeconds(false));
					}
					SelectedRange = new DateRange(dateTime, selectionEnd);
					Invalidate();
				}
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Left || keyData == Keys.Right)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (_isDropDownOpen)
			{
				if (_acceptsTab && keyData == Keys.Tab)
				{
					if (_isDropDownOpen)
					{
						if (_dropDownCalendar._calendarControl != null && _dropDownCalendar._calendarControl._isDroppedDown)
						{
							_dropDownCalendar._calendarControl.UnDrop(cancel: false);
						}
						_dropDownCalendar.UnDrop(cancel: false);
					}
				}
				else if (_dropDownCalendar.HandleDialogKey(keyData))
				{
					return true;
				}
			}
			switch (keyData)
			{
			case Keys.Return:
			case Keys.F2:
				if (_selectedItems.Count != 1 || _selectedItems[0].ItemType != ItemType.TimelineItem)
				{
					break;
				}
				_editingItem = _selectedItems[0].Item;
				StopItemTimer();
				if (AllowEdit && RaiseEditingItem(_editingItem))
				{
					_timelineFormatStyleControl = new TimelineFormatStyleControl(this, _editingItem, _useDefaultMenu);
					base.Controls.Add(_timelineFormatStyleControl);
					Rectangle bounds = TimelineUtilities.RoundToRectangle(_editingItem._rectangleF);
					int itemsAreaLeft = GetItemsLeft(base.ClientRectangle.Left);
					if (bounds.X < itemsAreaLeft)
					{
						int clippedLeft = itemsAreaLeft - bounds.X;
						_timelineFormatStyleControl._hasLeftBorder = false;
						bounds.Width -= clippedLeft;
						bounds.X = itemsAreaLeft;
					}
					if (bounds.Right > base.ClientRectangle.Right)
					{
						int clippedRight = bounds.Right - base.ClientRectangle.Right;
						_timelineFormatStyleControl._hasRightBorder = false;
						bounds.Width -= clippedRight;
					}
					_timelineFormatStyleControl.Bounds = bounds;
				}
				break;
			case Keys.Tab:
				if (_acceptsTab)
				{
					MoveFocusToNextZone();
					return true;
				}
				break;
			}
			TimeSpan timeSpan = TimeSpan.Zero;
			switch (keyData)
			{
			case Keys.Left | Keys.Shift:
			case Keys.Down | Keys.Shift:
				timeSpan = TimeSpan.FromSeconds(-GetSelectionStepSeconds(true));
				break;
			case Keys.Up | Keys.Shift:
			case Keys.Right | Keys.Shift:
				timeSpan = TimeSpan.FromSeconds(GetSelectionStepSeconds(false));
				break;
			}
			if (timeSpan != TimeSpan.Zero && _selectionAnchorDate != DateTime.MinValue)
			{
				DateTime dateTime = _selectedRange.Start;
				DateTime end = _selectedRange.End;
				if (timeSpan > TimeSpan.Zero)
				{
					if (dateTime != _selectionAnchorDate)
					{
						dateTime = dateTime.Add(timeSpan);
						end = _selectionAnchorDate.Add(timeSpan);
					}
					else
					{
						end = end.Add(timeSpan);
					}
				}
				else
				{
					dateTime = _selectedRange.Start;
					end = _selectedRange.End.Add(timeSpan);
					if (end <= _selectionAnchorDate)
					{
						dateTime = dateTime.Add(timeSpan);
						end = _selectionAnchorDate.Add(-timeSpan);
					}
				}
				if (dateTime < _firstDate)
				{
					FirstDate = dateTime;
				}
				else if (end >= _lastVisibleDate)
				{
					FirstDate = _firstDate.AddSeconds(GetSelectionStepSeconds(false));
				}
				SelectedRange = new DateRange(dateTime, end);
				Invalidate(BottomTier.GetBounds());
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		private void HandleDropDownClosed(bool isCancelled)
		{
			_isDropDownOpen = false;
			if (!isCancelled)
			{
				int hour = _firstDate.Hour;
				int minute = _firstDate.Minute;
				int second = _firstDate.Second;
				FirstDate = new DateTime(_dropDownCalendar.CurrentDate.Year, _dropDownCalendar.CurrentDate.Month, _dropDownCalendar.CurrentDate.Day, hour, minute, second);
			}
			OnCalendarCloseUp(EventArgs.Empty);
		}

		protected virtual void OnDraggingItem(DraggingItemEventArgs e)
		{
			if (base.Events[_draggingItemEventKey] is DraggingItemEventHandler draggingItemEventHandler)
			{
				draggingItemEventHandler(this, e);
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheel(e.Delta);
		}

		public TimelineArea HitTest()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return HitTest(position.X, position.Y);
		}

		public TimelineArea HitTest(Point pt)
		{
			return HitTest(pt.X, pt.Y);
		}

		public TimelineArea HitTest(int x, int y)
		{
			if (base.ClientRectangle.Contains(x, y))
			{
				if (GetItemAt(x, y) != null)
				{
					return TimelineArea.TimelineItem;
				}
				if (MiddleTier.GetBounds().Contains(x, y))
				{
					return TimelineArea.MiddleTier;
				}
				if (BottomTier.GetBounds().Contains(x, y))
				{
					return TimelineArea.BottomTier;
				}
				if (TopTier.GetBounds().Contains(x, y))
				{
					return TimelineArea.TopTier;
				}
				if (_rowLayouts != null)
				{
					foreach (MxRowLayoutState item in _rowLayouts)
					{
						if (!item.GetRowBounds().Contains(x, y))
						{
							continue;
						}
						if (item.GetRowType() == RowType.Group)
						{
							if (item.GetExpandBoxBounds().Contains(x, y))
							{
								return TimelineArea.ExpandBox;
							}
							return TimelineArea.GroupRow;
						}
						if (item.GetTimelineGroupRow() != null)
						{
							if (item.GetTimelineGroupRow()._bounds.Contains(x, y))
							{
								return TimelineArea.ExpandBox;
							}
							int rowLeft = base.ClientRectangle.Left + _groupIndent * item.GetGroupIndex();
							if (x > rowLeft && x < rowLeft + RowHeaderWidth)
							{
								return TimelineArea.RowHeader;
							}
						}
						return TimelineArea.Background;
					}
				}
				return TimelineArea.GroupIndent;
			}
			return TimelineArea.None;
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			if (_isDraggingItems && _itemDragType == ItemDragType.Move)
			{
				e.Effect = DragDropEffects.Move;
			}
			base.OnDragOver(e);
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			if (_isDraggingItems)
			{
				RestoreDraggedItems();
				Invalidate();
				Update();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left && !_dragStartPoint.IsEmpty && IsOutsideDragThreshold(e.X, e.Y) && TrySetItemDragType(e.X))
			{
				BeginItemDrag();
				return;
			}
			if (_isSelectingRange)
			{
				DateTime dateTime = GetDateAtX(e.X);
				if (dateTime == DateTime.MinValue)
				{
					if (e.X >= base.ClientRectangle.Right)
					{
						dateTime = GetDateAtX(base.ClientRectangle.Right).AddSeconds(GetSelectionStepSeconds(false));
						FirstDate = _firstDate.AddSeconds(GetSelectionStepSeconds(false));
					}
					else if (e.X <= base.ClientRectangle.Left)
					{
						dateTime = (FirstDate = _firstDate.AddSeconds(-GetSelectionStepSeconds(true)));
					}
				}
				dateTime = AlignToInterval(dateTime, BottomTier.Interval, BottomTier.Count);
				if (dateTime < _selectionAnchorDate)
				{
					if (dateTime != _selectedRange.Start)
					{
						SelectedRange = new DateRange(dateTime, _selectionAnchorDate.AddSeconds(GetSelectionStepSeconds(false)));
					}
				}
				else if (dateTime != _selectedRange.Start || dateTime != _selectedRange.End)
				{
					SelectedRange = new DateRange(_selectionAnchorDate, dateTime.AddSeconds(GetSelectionStepSeconds(false)));
				}
				Invalidate(BottomTier.GetBounds());
				return;
			}
			TimelineItem itemAt = GetItemAt(e.X, e.Y);
			SetHoveredItem(itemAt);
			if (itemAt != null)
			{
				return;
			}
			MxRowLayoutState mordexRowLayoutState = GetRowLayoutAt(e.X, e.Y);
			if (mordexRowLayoutState != null)
			{
				if (mordexRowLayoutState.GetRowType() == RowType.Group)
				{
					if (mordexRowLayoutState.GetExpandBoxBounds().Contains(e.X, e.Y))
					{
						UpdateTimelineMordexRowLayoutState(mordexRowLayoutState);
						return;
					}
				}
				else if (mordexRowLayoutState.GetTimelineGroupRow() != null && mordexRowLayoutState.GetTimelineGroupRow()._bounds.Contains(e.X, e.Y))
				{
					UpdateTimelineMordexRowLayoutState(mordexRowLayoutState);
					return;
				}
			}
			UpdateTimelineMordexRowLayoutState(null);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (_isDropDownOpen)
			{
				if (_dropDownCalendar._calendarControl != null && _dropDownCalendar._calendarControl._isDroppedDown)
				{
					_dropDownCalendar._calendarControl.UnDrop(cancel: false);
				}
				_dropDownCalendar.UnDrop(cancel: false);
			}
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHoveredItem(null);
			UpdateTimelineMordexRowLayoutState(null);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			SetHoveredItem(null);
			if (!TryCommitItemEdit())
			{
				return;
			}
			StopItemTimer();
			TimelineUtilities.FocusControl(this);
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			if (BottomTier.GetBounds().Contains(e.X, e.Y))
			{
				SetFocusZone(FocusZone.BottomTierScale);
				_selectedItems.Clear();
				_isSelectingRange = true;
				DateTime dateTime = AlignToInterval(GetDateAtX(e.X), BottomTier.Interval, BottomTier.Count);
				UpdateTimelineDateTime(dateTime);
				return;
			}
			if (_dropDownNavigator && IsInTierHeaderArea(e.X, e.Y))
			{
				ShowDropDownCalendar(true);
				return;
			}
			if (_selectionAnchorDate != DateTime.MinValue)
			{
				_isSelectingRange = false;
				_selectionAnchorDate = DateTime.MinValue;
				Invalidate();
			}
			TimelineItem itemAt = GetItemAt(e.X, e.Y);
			SelectedRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
			_selectionAnchorDate = DateTime.MinValue;
			_isSelectingRange = false;
			MxRowLayoutState mordexRowLayoutState = GetRowLayoutAt(e.X, e.Y);
			if (itemAt != null)
			{
				SetFocusZone(FocusZone.Items);
				_focusedRowLayout = mordexRowLayoutState;
				_currentItem = itemAt;
				SelectItemOnClick(itemAt);
				if (itemAt.Selected)
				{
					_dragStartPoint = new Point(e.X, e.Y);
				}
				return;
			}
			if (mordexRowLayoutState != null)
			{
				SetFocusZone(FocusZone.Items);
				CurrentItem = null;
				_currentItem = null;
				_focusedRowLayout = mordexRowLayoutState;
				if (mordexRowLayoutState.GetRowType() == RowType.Group)
				{
					SelectGroupRowOnClick(mordexRowLayoutState);
					if (mordexRowLayoutState.GetExpandBoxBounds().Contains(e.X, e.Y))
					{
						mordexRowLayoutState.GetNode().Expanded = !mordexRowLayoutState.IsExpanded();
						LayoutRows();
						Invalidate();
					}
					return;
				}
				if (mordexRowLayoutState.GetTimelineGroupRow() != null && mordexRowLayoutState.GetTimelineGroupRow()._bounds.Contains(e.X, e.Y))
				{
					mordexRowLayoutState.GetParentRow().GetNode().Expanded = !mordexRowLayoutState.GetParentRow().GetNode().Expanded;
					InvalidateRows();
					return;
				}
			}
			_selectedItems.Clear();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_dragStartPoint = Point.Empty;
			_isSelectingRange = false;
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			if (GetEditedItem() != null)
			{
				SetEditedItem(null);
			}
			Point point = PointToClient(Cursor.Position);
			MxRowLayoutState mordexRowLayoutState = GetRowLayoutAt(point.X, point.Y);
			if (mordexRowLayoutState != null && (mordexRowLayoutState.GetRowType() == RowType.Group || mordexRowLayoutState.GetTimelineGroupRow() != null))
			{
				Rectangle rectangle = mordexRowLayoutState.GetExpandBoxBounds();
				if (rectangle.IsEmpty && mordexRowLayoutState.GetTimelineGroupRow() != null)
				{
					rectangle = mordexRowLayoutState.GetTimelineGroupRow()._bounds;
				}
				if (!rectangle.Contains(point.X, point.Y))
				{
					if (mordexRowLayoutState.GetTimelineGroupRow() != null)
					{
						if (AllowExpandRowHeader)
						{
							int headerLeft = base.ClientRectangle.Left + _groupIndent * mordexRowLayoutState.GetGroupIndex();
							if (point.X > headerLeft && point.X < headerLeft + RowHeaderWidth)
							{
								mordexRowLayoutState.GetParentRow().GetNode().Expanded = !mordexRowLayoutState.GetParentRow().GetNode().Expanded;
								InvalidateRows();
							}
						}
					}
					else
					{
						mordexRowLayoutState.GetNode().Expanded = !mordexRowLayoutState.IsExpanded();
						LayoutRows();
						Invalidate();
					}
				}
			}
			base.OnDoubleClick(e);
		}
	}
}
