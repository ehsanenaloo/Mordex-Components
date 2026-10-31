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
		private MxRowLayoutState GetNextGroupRowLayout()
		{
			MxRowLayoutState mordexRowLayoutState = null;
			if (_focusedRowLayout != null)
			{
				for (int i = _focusedRowLayout.GetRowIndex() + 1; i < _rowLayouts.Count; i++)
				{
					mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[i];
					if (mordexRowLayoutState.GetRowType() == RowType.Group)
					{
						return mordexRowLayoutState;
					}
				}
			}
			return null;
		}

		private MxRowLayoutState GetPreviousGroupRowLayout()
		{
			MxRowLayoutState mordexRowLayoutState = null;
			if (_focusedRowLayout != null)
			{
				for (int i = _focusedRowLayout.GetRowIndex() - 1; i >= 0; i--)
				{
					mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[i];
					if (mordexRowLayoutState.GetRowType() == RowType.Group)
					{
						return mordexRowLayoutState;
					}
				}
			}
			return null;
		}

		public TimelineGroupRow[] GetGroupRows()
		{
			ArrayList arrayList = new ArrayList();
			if (Groups.Count > 0)
			{
				foreach (MordexRow item in (IEnumerable)_timelineDataSourceManager.RootRow.Rows)
				{
					if (item is MordexGroupRow mordexGroupRow)
					{
						arrayList.Add(_timelineDataSourceManager.GetGroupRow(mordexGroupRow));
					}
				}
			}
			TimelineGroupRow[] groupRows = new TimelineGroupRow[arrayList.Count];
			for (int i = 0; i < arrayList.Count; i++)
			{
				TimelineGroupRow value = (TimelineGroupRow)arrayList[i];
				groupRows.SetValue(value, i);
			}
			return groupRows;
		}

		private int GetVisibleRowCount()
		{
			int firstRowIndex = 0;
			if (_firstVisibleRowLayout != null)
			{
				firstRowIndex = _firstVisibleRowLayout.GetRowIndex();
			}
			int rowTop = GetHeaderHeight() + base.ClientRectangle.Top + 1;
			int rowCount = 0;
			if (_rowLayouts != null)
			{
				for (int i = firstRowIndex; i < _rowLayouts.Count; i++)
				{
					MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[i];
					if (mordexRowLayoutState.GetRowType() == RowType.Group)
					{
						if (rowTop + GetGroupRowHeight() - 1 > base.ClientRectangle.Bottom)
						{
							break;
						}
						rowTop += GetGroupRowHeight() - 1;
					}
					else
					{
						if (rowTop + RowHeight - 1 > base.ClientRectangle.Bottom)
						{
							break;
						}
						rowTop += RowHeight - 1;
					}
					rowCount++;
				}
			}
			return rowCount;
		}

		internal int GetGroupRowHeight()
		{
			if (_timelineRowHeight > 0)
			{
				return _timelineRowHeight;
			}
			return _groupRowHeight;
		}

		internal string GetItemLabelText(TimelineItem timelineItem)
		{
			string text = "";
			if (_itemsPrefix != string.Empty)
			{
				text = _itemsPrefix + " ";
			}
			text += timelineItem.Text;
			if (_itemsSuffix != string.Empty)
			{
				text = text + " " + _itemsSuffix;
			}
			text = text.Replace("\r\n", " ");
			if (_maximumLabelLength >= 0 && text.Length > _maximumLabelLength)
			{
				text = text.Remove(_maximumLabelLength, text.Length - _maximumLabelLength) + "...";
			}
			return text;
		}

		private ItemSizeMode GetItemSizeMode()
		{
			return ItemSizeMode;
		}

		public TimelineGroupRow GetGroupRowAt()
		{
			Point pt = PointToClient(Cursor.Position);
			return GetGroupRowAt(pt);
		}

		public TimelineGroupRow GetGroupRowAt(Point pt)
		{
			return GetGroupRowAt(pt.X, pt.Y);
		}

		public TimelineGroupRow GetGroupRowAt(int x, int y)
		{
			try
			{
				MxRowLayoutState mordexRowLayoutState = GetRowLayoutAt(x, y);
				if (mordexRowLayoutState != null && mordexRowLayoutState.GetRowType() != RowType.SourceRow)
				{
					if (mordexRowLayoutState.GetRowType() == RowType.Group)
					{
						MordexGroupRow mordexGroupRow = mordexRowLayoutState.GetNode() as MordexGroupRow;
						return GetGroupRow(mordexGroupRow);
					}
					for (mordexRowLayoutState = mordexRowLayoutState.GetParentRow(); mordexRowLayoutState != null; mordexRowLayoutState = mordexRowLayoutState.GetParentRow())
					{
						if (mordexRowLayoutState.GetRowType() == RowType.Group)
						{
							return GetGroupRow(mordexRowLayoutState.GetNode());
						}
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		private MxRowLayoutState GetRowLayoutAt(int x, int integerArgument)
		{
			if (_rowLayouts != null)
			{
				foreach (MxRowLayoutState item in _rowLayouts)
				{
					if (item.GetRowBounds().Contains(x, integerArgument))
					{
						return item;
					}
				}
			}
			return null;
		}

		private MxRowLayoutState GetRowLayoutByIndex(int rowIndex)
		{
			if (_rowLayouts != null)
			{
				foreach (MxRowLayoutState item in _rowLayouts)
				{
					if (item.GetRowIndex() == rowIndex)
					{
						return item;
					}
				}
			}
			return null;
		}

		public TimelineItem GetNextItem(TimelineItem item)
		{
			if (Groups.Count == 0)
			{
				SelectNextItem((MxRowLayoutState)_rowLayouts[item.GetSubRowIndex()], item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
				return _currentItem;
			}
			foreach (MxRowLayoutState rowLayout in _rowLayouts)
			{
				if ((rowLayout.GetRowType() == RowType.Group || rowLayout.GetTimelineGroupRow() != null) && rowLayout.GetGroupIndex() == item.GetGroupIndex())
				{
					SelectNextItem(rowLayout, item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
					return _currentItem;
				}
			}
			return null;
		}

		public TimelineItem GetPreviousItem(TimelineItem item)
		{
			if (Groups.Count == 0)
			{
				SelectPreviousItem((MxRowLayoutState)_rowLayouts[item.GetSubRowIndex()], item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
				return _currentItem;
			}
			foreach (MxRowLayoutState rowLayout in _rowLayouts)
			{
				if ((rowLayout.GetRowType() == RowType.Group || rowLayout.GetTimelineGroupRow() != null) && rowLayout.GetGroupIndex() == item.GetGroupIndex())
				{
					SelectPreviousItem(rowLayout, item.StartTime, item.GetRecordPosition(), item.GetSubRowIndex());
					return _currentItem;
				}
			}
			return null;
		}

		public TimelineItem[] GetItemsFromRange(DateRange range)
		{
			ArrayList arrayList = _items.GetItemsInRange(range);
			TimelineItem[] result = new TimelineItem[arrayList.Count];
			int index = 0;
			foreach (TimelineItem item in arrayList)
			{
				result.SetValue(item, index);
				index++;
			}
			return result;
		}

		public TimelineItem GetItemAt(int x, int y)
		{
			if (_visibleItems == null)
			{
				return null;
			}
			if (x < GetItemsLeft(base.ClientRectangle.Left))
			{
				return null;
			}
			if (MiddleTier.GetBounds().Contains(x, y))
			{
				return null;
			}
			if (BottomTier.GetBounds().Contains(x, y))
			{
				return null;
			}
			if (TopTier.GetBounds().Contains(x, y))
			{
				return null;
			}
			for (int i = _visibleItems.Count - 1; i >= 0; i--)
			{
				TimelineItem timelineItem = (TimelineItem)_visibleItems[i];
				if (timelineItem._rectangleF.Contains(x, y))
				{
					return timelineItem;
				}
			}
			return null;
		}

		public TimelineItem GetItemAt(Point pt)
		{
			return GetItemAt(pt.X, pt.Y);
		}

		public TimelineItem GetItemAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetItemAt(point.X, point.Y);
		}

		private bool CanChangeItemGroup(TimelineGroupRow timelineGroupRow)
		{
			if (timelineGroupRow != null && (_allowItemDrag & AllowItemDrag.GroupChange) != AllowItemDrag.None)
			{
				while (timelineGroupRow != null)
				{
					if (timelineGroupRow.Group.Field.ReadOnly)
					{
						return false;
					}
					timelineGroupRow = timelineGroupRow.ParentRow;
				}
				return true;
			}
			return false;
		}

		private bool TrySetItemDragType(int mouseX)
		{
			if (_currentItem == null)
			{
				return false;
			}
			Rectangle rectangle = _currentItem._durationBarBounds;
			Rectangle field_ = _currentItem._estimatedBarBounds;
			bool isBarNarrow = false;
			bool isEstimatedBarNarrow = false;
			if (rectangle.Width < 10)
			{
				rectangle.Width = 10;
				isBarNarrow = true;
			}
			if (field_.Width < 10)
			{
				field_.Width = 10;
				isEstimatedBarNarrow = true;
			}
			if (_showItemsDurationBar && (_allowItemDrag & AllowItemDrag.Resize) != AllowItemDrag.None && rectangle.Contains(_dragStartPoint))
			{
				if (isBarNarrow)
				{
					if (_dragStartPoint.X < mouseX)
					{
						_itemDragType = ItemDragType.EndTimeChange;
					}
					else
					{
						_itemDragType = ItemDragType.StartTimeChange;
					}
					return true;
				}
				if (_dragStartPoint.X >= rectangle.Right - 5)
				{
					_itemDragType = ItemDragType.EndTimeChange;
					return true;
				}
				if (_dragStartPoint.X <= rectangle.X + 5)
				{
					_itemDragType = ItemDragType.StartTimeChange;
					return true;
				}
				if ((_allowItemDrag & AllowItemDrag.Move) != AllowItemDrag.None)
				{
					_itemDragType = ItemDragType.Move;
					return true;
				}
			}
			else if (_showItemsEstimatedDurationBar && (_allowItemDrag & AllowItemDrag.EstimatedDurationChange) != AllowItemDrag.None && field_.Contains(_dragStartPoint))
			{
				if (isEstimatedBarNarrow)
				{
					if (_dragStartPoint.X < mouseX)
					{
						_itemDragType = ItemDragType.EstimatedEndTimeChange;
					}
					else
					{
						_itemDragType = ItemDragType.EstimatedStartTimeChange;
					}
					return true;
				}
				if (_dragStartPoint.X >= field_.Right - 5)
				{
					_itemDragType = ItemDragType.EstimatedEndTimeChange;
					return true;
				}
				if (_dragStartPoint.X <= field_.X + 5)
				{
					_itemDragType = ItemDragType.EstimatedStartTimeChange;
					return true;
				}
				if ((_allowItemDrag & AllowItemDrag.Move) != AllowItemDrag.None)
				{
					_itemDragType = ItemDragType.Move;
					return true;
				}
			}
			else if ((_allowItemDrag & AllowItemDrag.Move) != AllowItemDrag.None || (_allowItemDrag & AllowItemDrag.GroupChange) != AllowItemDrag.None)
			{
				_itemDragType = ItemDragType.Move;
				return true;
			}
			return false;
		}

		internal bool TryCommitItemEdit()
		{
			if (_timelineFormatStyleControl != null)
			{
				string text = _timelineFormatStyleControl._textBoxControl.Text;
				ValidatingItemTextEventArgs e = new ValidatingItemTextEventArgs(_editingItem, text);
				OnValidatingItemText(e);
				if (!e.Cancel)
				{
					SetEditedItem(null);
				}
				else
				{
					TimelineUtilities.FocusControl(_timelineFormatStyleControl._textBoxControl);
				}
				return !e.Cancel;
			}
			SetEditedItem(null);
			StopItemTimer();
			return true;
		}

		internal TimelineItem GetEditedItem()
		{
			return _editingItem;
		}

		public TimelineItem GetItem(object dataRow)
		{
			if (_isUnbound)
			{
				throw new InvalidOperationException();
			}
			if (dataRow == null)
			{
				return null;
			}
			if (_timelineDataSourceManager.FindDataRow(dataRow) is MordexDataRow mordexDataRow)
			{
				return TimelineDataSourceManager.GetTimelineItem(mordexDataRow);
			}
			return null;
		}

		internal Image GetItemImage(TimelineItem timelineItem, int imageIndex)
		{
			try
			{
				if (timelineItem.Image != null)
				{
					return timelineItem.Image;
				}
				ImageList imageList = GetImageList();
				if (imageList != null && imageIndex != -1)
				{
					return imageList.Images[imageIndex];
				}
				if (GetImageField() != null)
				{
					Image image = GetImageField().GetItemImage(timelineItem);
					if (image != null)
					{
						return image;
					}
				}
				if (imageList != null)
				{
					if (_defaultItemsImageKey.Length > 0 && imageList.Images.ContainsKey(_defaultItemsImageKey))
					{
						return imageList.Images[_defaultItemsImageKey];
					}
					if (_defaultItemsImageIndex >= 0 && _defaultItemsImageIndex < imageList.Images.Count)
					{
						return imageList.Images[_defaultItemsImageIndex];
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		private int GetItemImageWidth(TimelineItem timelineItem)
		{
			try
			{
				if (timelineItem.Image != null)
				{
					return timelineItem.Image.Width;
				}
				ImageList imageList = GetImageList();
				if (imageList != null && (timelineItem.ImageIndex != -1 || timelineItem.ImageKey.Length > 0))
				{
					return imageList.ImageSize.Width;
				}
				if (GetImageField() != null)
				{
					int providerWidth = GetImageField().GetImageWidth(timelineItem);
					if (providerWidth != 0)
					{
						return providerWidth;
					}
				}
				if (imageList != null && (_defaultItemsImageKey.Length > 0 || _defaultItemsImageIndex != -1))
				{
					return imageList.ImageSize.Width;
				}
				return 0;
			}
			catch
			{
				return 0;
			}
		}

		internal Image GetGroupRowImage(TimelineGroupRow timelineGroupRow)
		{
			try
			{
				if (timelineGroupRow.Image != null)
				{
					return timelineGroupRow.Image;
				}
				ImageList imageList = GetImageList();
				if (imageList != null)
				{
					if (timelineGroupRow.ImageKey.Length > 0 && imageList.Images.ContainsKey(timelineGroupRow.ImageKey))
					{
						return imageList.Images[timelineGroupRow.ImageKey];
					}
					if (timelineGroupRow.ImageIndex >= 0 && timelineGroupRow.ImageIndex < imageList.Images.Count)
					{
						return imageList.Images[timelineGroupRow.ImageIndex];
					}
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		internal int GetItemsLeft(int clientLeft)
		{
			if (Groups.Count > 0)
			{
				int itemsLeft = clientLeft + _groupIndent * (Groups.Count - 1) + 1;
				if (ShowLastGroupAsRowHeader)
				{
					itemsLeft += RowHeaderWidth;
				}
				return itemsLeft;
			}
			return clientLeft;
		}
	}
}
