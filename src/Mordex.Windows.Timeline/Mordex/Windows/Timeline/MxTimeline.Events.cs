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
		[Description("Event fired when the drop-down Calendar is shown.")]
		public event EventHandler DropDown
		{
			add
			{
				base.Events.AddHandler(_dropDownEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dropDownEventKey, value);
			}
		}

		[Description("Event fired when the drop-down calendar is dismissed and disappears.")]
		public event EventHandler CalendarCloseUp
		{
			add
			{
				base.Events.AddHandler(_calendarCloseUpEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_calendarCloseUpEventKey, value);
			}
		}

		public event DrawTimelineEventHandler DrawTimelineArea
		{
			add
			{
				base.Events.AddHandler(_drawTimelineAreaEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_drawTimelineAreaEventKey, value);
			}
		}

		[Description("Event fired when the SelectedItems collection of the Timeline is changed.")]
		public event EventHandler SelectedItemsChanged
		{
			add
			{
				base.Events.AddHandler(_selectedItemsChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedItemsChangedEventKey, value);
			}
		}

		[Description("Event raised when a TimelineItem in the Timeline control is clicked in the edit area.")]
		public event ItemCancelEventHandler EditingItem
		{
			add
			{
				base.Events.AddHandler(_editingItemEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_editingItemEventKey, value);
			}
		}

		[Description("Event fired each time a TimelineGroupRow is loaded for display")]
		public event FormattingGroupRowEventHandler FormattingGroupRow
		{
			add
			{
				base.Events.AddHandler(_formattingGroupRowEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_formattingGroupRowEventKey, value);
			}
		}

		public event EventHandler FilterApplied
		{
			add
			{
				base.Events.AddHandler(_filterAppliedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_filterAppliedEventKey, value);
			}
		}

		[Description("Event fired when a row in the data source is read resulting in the creation of a new TimelineItem.")]
		public event ItemEventHandler ItemRead
		{
			add
			{
				base.Events.AddHandler(_itemReadEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemReadEventKey, value);
			}
		}

		[Description("Occurs before the items in the control are filtered.")]
		public event CancelEventHandler ApplyingFilter
		{
			add
			{
				base.Events.AddHandler(_applyingFilterEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_applyingFilterEventKey, value);
			}
		}

		[Description("Event fired when when the Text of the item is being changed by the user.")]
		public event ValidatingItemTextEventHandler ValidatingItemText
		{
			add
			{
				base.Events.AddHandler(_validatingItemTextEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_validatingItemTextEventKey, value);
			}
		}

		[Description("Event fired when a TimelineItem was dropped")]
		public event ItemEventHandler ItemDropped
		{
			add
			{
				base.Events.AddHandler(_itemDroppedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemDroppedEventKey, value);
			}
		}

		[Description("Event fired when the item is about to be dropped.")]
		public event DroppingItemEventHandler DroppingItem
		{
			add
			{
				base.Events.AddHandler(_droppingItemEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_droppingItemEventKey, value);
			}
		}

		[Description("Event fired when an item is about to be dragged in the Timeline control.")]
		public event ItemDragEventHandler ItemDrag
		{
			add
			{
				base.Events.AddHandler(_itemDragEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemDragEventKey, value);
			}
		}

		[Description("Event fired when an item is been dragged in the Timeline control.")]
		public event DraggingItemEventHandler DraggingItem
		{
			add
			{
				base.Events.AddHandler(_draggingItemEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_draggingItemEventKey, value);
			}
		}

		[Description("Event fired when the Timeline control is about to add a new item")]
		public event GetNewObjectEventHandler GetNewObject
		{
			add
			{
				base.Events.AddHandler(_getNewObjectEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_getNewObjectEventKey, value);
			}
		}

		[Description("Event fired when a TimelineItem is about to be added to the Items collection")]
		public event ItemCancelEventHandler AddingItem
		{
			add
			{
				base.Events.AddHandler(_addingItemEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_addingItemEventKey, value);
			}
		}

		[Description("Event fired when one of the TimelineItem objects in the Items collection is changing in one its properties")]
		public event ItemChangeCancelEventHandler ItemChanging
		{
			add
			{
				base.Events.AddHandler(_itemChangingEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemChangingEventKey, value);
			}
		}

		[Description("Event fired when one of the TimelineItem objects in the Items collection has changed in one its properties")]
		public event ItemChangeEventHandler ItemChanged
		{
			add
			{
				base.Events.AddHandler(_itemChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemChangedEventKey, value);
			}
		}

		[Description("Event fired when a TimelineItem was added to the Items collection")]
		public event ItemEventHandler ItemAdded
		{
			add
			{
				base.Events.AddHandler(_itemAddedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemAddedEventKey, value);
			}
		}

		[Description("Event fired when a TimelineItem was removed from the Items collection")]
		public event ItemEventHandler ItemRemoved
		{
			add
			{
				base.Events.AddHandler(_itemRemovedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemRemovedEventKey, value);
			}
		}

		[Description("Event fired when a TimelineItem is about to be removed from the Items collection.")]
		public event ItemCancelEventHandler RemovingItem
		{
			add
			{
				base.Events.AddHandler(_removingItemEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_removingItemEventKey, value);
			}
		}

		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(_layoutLoadEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_layoutLoadEventKey, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangedEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_currentLayoutChangedEventKey, value);
			}
		}

		[Description("Event fired when SelectedRange property of the Timeline changes.")]
		public event EventHandler SelectedRangeChanged
		{
			add
			{
				base.Events.AddHandler(_selectedRangeChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedRangeChangedEventKey, value);
			}
		}

		[Description("Event fired when LastDate property of the Timeline changes.")]
		public event EventHandler LastDateChanged
		{
			add
			{
				base.Events.AddHandler(_lastDateChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_lastDateChangedEventKey, value);
			}
		}

		[Description("Occurs when the user scrolls the Timeline control.")]
		public event EventHandler VerticalScrollPositionChanged
		{
			add
			{
				base.Events.AddHandler(_verticalScrollPositionChangedEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_verticalScrollPositionChangedEventKey, value);
			}
		}

		[Description("Event fired when FirstDate property of the Timeline changes.")]
		public event EventHandler FirstDateChanged
		{
			add
			{
				base.Events.AddHandler(_firstDateChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_firstDateChangedEventKey, value);
			}
		}

		[Description("Occurs before the CurrentLayout property changes")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangingEventKey, value);
			}
			remove
			{
				base.Events.AddHandler(_currentLayoutChangingEventKey, value);
			}
		}

		[Description("Event fired when the edit operation of a TimelineItem in the Timeline is ended")]
		public event ItemEventHandler ItemEdited
		{
			add
			{
				base.Events.AddHandler(_itemEditedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemEditedEventKey, value);
			}
		}

		[Description("Event fired after the items collection is loaded using the LoadItems methos.")]
		public event EventHandler ItemsLoaded
		{
			add
			{
				base.Events.AddHandler(_itemsLoadedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_itemsLoadedEventKey, value);
			}
		}

		event EventHandler ISupportFilterBuilder.RootTableChanged
		{
			add
			{
				base.Events.AddHandler(_rootTableChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_rootTableChangedEventKey, value);
			}
		}
	}
}
