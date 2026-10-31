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
		private void SetHoveredItem(TimelineItem timelineTimelineItem)
		{
			if (_timelineTimelineItem == timelineTimelineItem)
			{
				return;
			}
			if (_timelineTimelineItem != null)
			{
				if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
				}
				if (_timelineWindowMessageHandler != null)
				{
					_timelineWindowMessageHandler.DisposeAll();
					_timelineWindowMessageHandler = null;
				}
			}
			_timelineTimelineItem = timelineTimelineItem;
			if (_timelineTimelineItem != null)
			{
				string text = "";
				switch (_itemToolTip)
				{
				case ItemToolTip.UseToolTipText:
					text = _timelineTimelineItem.ToolTipText;
					break;
				case ItemToolTip.UseItemText:
					text = _timelineTimelineItem.Text;
					break;
				case ItemToolTip.UseToolTipMember:
					text = GetToolTipMemberText(_timelineTimelineItem);
					break;
				}
				if (text.Length > 0)
				{
					ShowToolTip(_timelineTimelineItem, text);
				}
			}
		}

		private void BeginItemDrag()
		{
			TryCommitItemEdit();
			_dragItem = _currentItem;
			_draggedItems = new ArrayList();
			foreach (ITimelineObject selectedItem in SelectedItems)
			{
				if (selectedItem.ItemType != ItemType.TimelineItem)
				{
					continue;
				}
				ItemDragEventArgs e = new ItemDragEventArgs(selectedItem.Item, _itemDragType);
				OnItemDrag(e);
				if (!e.Cancel)
				{
					TimelineItem value = selectedItem.Item.CreateCopy();
					_draggedItems.Add(value);
					if (selectedItem.Item == _currentItem)
					{
						_dragItem = value;
					}
					selectedItem.Item.SetVisible(false);
				}
				else if (e.Item == _currentItem)
				{
					_dragItem = null;
				}
			}
			if (_draggedItems.Count == 0)
			{
				return;
			}
			if (_dragItem == null)
			{
				_dragItem = (TimelineItem)_draggedItems[0];
			}
			_dragGrabOffset = (_dragStartPoint.X - (int)_dragItem._rectangleF.X) / _intervalWidth;
			bool allowDrop = AllowDrop;
			TimelineUtilities.SetAllowDrop(this, true);
			_isDraggingItems = true;
			try
			{
				if (_itemDragType == ItemDragType.Move)
				{
					DoDragDrop(_dragItem, DragDropEffects.All);
				}
				else
				{
					DoDragDrop(_dragItem, DragDropEffects.Move);
				}
				if (_isDropPending)
				{
					CommitDrop(PointToClient(Cursor.Position));
				}
			}
			finally
			{
				_isDropPending = false;
				foreach (TimelineItem item in _draggedItems)
				{
					item.GetSourceItem().SetVisible(true);
				}
				_isDraggingItems = false;
				TimelineUtilities.SetAllowDrop(this, allowDrop);
				_draggedItems = null;
				_dragStartPoint = Point.Empty;
			}
			Invalidate();
			Update();
		}

		private void ApplyGroupValues(TimelineGroupRow timelineGroupRow, TimelineItem timelineItem)
		{
			timelineItem.SetValue(timelineGroupRow.Group.Field, timelineGroupRow.Value);
			if (timelineGroupRow.ParentRow != null)
			{
				ApplyGroupValues(timelineGroupRow.ParentRow, timelineItem);
			}
		}

		private void MoveDraggedItems(DateTime dateTime, Point pt)
		{
			try
			{
				TimelineGroupRow groupRowAt = GetGroupRowAt(pt);
				if (_dragItem == null || _draggedItems == null)
				{
					return;
				}
				DateTime start = AddIntervalUnits(dateTime, 0f - (float)_dragGrabOffset);
				DraggingItemEventArgs e = new DraggingItemEventArgs(_dragItem.GetSourceItem(), _itemDragType, start, start.Add(_dragItem.Duration), groupRowAt);
				OnDraggingItem(e);
				if (e.Cancel)
				{
					return;
				}
				double dropOffset = GetIntervalDistance(MinDate, dateTime);
				dropOffset -= _dragItem._leftOffset;
				int targetRowIndex = -1;
				MxRowLayoutState mordexRowLayoutState = ComputeMordexRowLayoutState(pt);
				if (CanChangeItemGroup(groupRowAt) && mordexRowLayoutState != null && mordexRowLayoutState.GetRowType() == RowType.Record)
				{
					targetRowIndex = ((mordexRowLayoutState.GetParentRow() == null) ? mordexRowLayoutState.GetRowIndex() : ((mordexRowLayoutState.GetParentRow().GetRowType() != RowType.Record && mordexRowLayoutState.GetTimelineGroupRow() == null) ? (mordexRowLayoutState.GetParentRow().GetRowIndex() + 1) : mordexRowLayoutState.GetParentRow().GetRowIndex()));
				}
				foreach (TimelineItem item in _draggedItems)
				{
					item._startOffset += dropOffset - _dragGrabOffset;
					item._estimatedStartOffset += dropOffset - _dragGrabOffset;
					item._leftOffset += dropOffset - _dragGrabOffset;
					item._doubleValue += dropOffset - _dragGrabOffset;
					if (targetRowIndex >= 0)
					{
						item.SetRowIndex(targetRowIndex);
						item.SetSubRowIndex(mordexRowLayoutState.GetRowIndex() - targetRowIndex);
					}
				}
			}
			catch (Exception ex)
			{
				Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
			}
		}

		private void SetFlashingItem(TimelineItem flashingItem)
		{
			if (_blinkingItem != flashingItem)
			{
				_blinkTickCount = 0;
				if (_blinkingItem != null)
				{
					_blinkingItem.SetLabelVisible(true);
				}
				_blinkingItem = flashingItem;
				if (_blinkingItem != null)
				{
					_blinkingItem.SetLabelVisible(false);
					_timelineTimer.Interval = 300;
					_timelineTimer.Start();
				}
			}
		}

		internal void SetEditedItem(TimelineItem editedItem)
		{
			if (_editingItem != editedItem)
			{
				StopItemTimer();
				if (_timelineFormatStyleControl != null && _editingItem != null)
				{
					_editingItem.Text = _timelineFormatStyleControl._textBoxControl.Text;
					RaiseItemEdited(_editingItem);
					_timelineFormatStyleControl.Dispose();
					_timelineFormatStyleControl = null;
				}
				_editingItem = editedItem;
				if (_editingItem != null && _allowEdit)
				{
					_timelineTimer.Start();
				}
			}
		}
	}
}
