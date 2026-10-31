using System;
using System.Drawing;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public class DrawTimelineAreaEventArgs : EventArgs
	{
		private MxTimeline _timeline;

		private object _itemDrawn;

		private Graphics _graphics;

		private Rectangle _bounds = Rectangle.Empty;

		private DateTime _areaDate = DateTime.MinValue;

		private MxRowLayoutState _mordexRowLayoutState;

		private Brush _backBrush;

		private Brush _foreBrush;

		private TimelineFormatStyle _formatStyle;

		private OwnerDrawnArea _area;

		private bool _handled;

		public Graphics Graphics => _graphics;

		public Rectangle Bounds => _bounds;

		public DateTime DateTime
		{
			get
			{
				if (Item != null)
				{
					return Item.StartTime;
				}
				return _areaDate;
			}
		}

		public TimelineItem Item
		{
			get
			{
				if (_itemDrawn is TimelineItem)
				{
					return _itemDrawn as TimelineItem;
				}
				return null;
			}
		}

		public TimelineGroupRow GroupRow
		{
			get
			{
				try
				{
					if (_itemDrawn != null)
					{
						if (OwnerDrawnArea == OwnerDrawnArea.GroupRow)
						{
							if (_itemDrawn is MxRowLayoutState mordexRowLayoutState)
							{
								return _timeline.GetGroupRow(mordexRowLayoutState.GetNode());
							}
						}
						else if (OwnerDrawnArea == OwnerDrawnArea.RowHeader)
						{
							if (_itemDrawn is MxRowLayoutState mordexRowLayoutStateArgument)
							{
								return mordexRowLayoutStateArgument.GetTimelineGroupRow();
							}
						}
						else if (OwnerDrawnArea == OwnerDrawnArea.Items && _itemDrawn is TimelineItem timelineItem)
						{
							return timelineItem.ParentRow;
						}
					}
				}
				catch
				{
					return null;
				}
				return null;
			}
		}

		public Brush BackBrush
		{
			get
			{
				if (_backBrush == null)
				{
					_backBrush = _timeline.GetAreaBackgroundBrush(this);
				}
				return _backBrush;
			}
		}

		public Brush ForeBrush
		{
			get
			{
				if (_foreBrush == null)
				{
					_foreBrush = _timeline.GetAreaForeBrush(this);
				}
				return _foreBrush;
			}
		}

		public TimelineFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = _timeline.GetAreaFormatStyle(this);
				}
				return _formatStyle;
			}
		}

		public OwnerDrawnArea OwnerDrawnArea => _area;

		public bool Handled
		{
			get
			{
				return _handled;
			}
			set
			{
				_handled = value;
			}
		}

		internal DrawTimelineAreaEventArgs(Graphics graphics, MxTimeline timeline, OwnerDrawnArea area, Rectangle rect, object itemDrawn, Brush foreBrush, DateTime dateTime)
		{
			_graphics = graphics;
			_area = area;
			_bounds = rect;
			_itemDrawn = itemDrawn;
			_timeline = timeline;
			_foreBrush = foreBrush;
			_areaDate = dateTime;
		}

		internal MxRowLayoutState GetMordexRowLayoutState()
		{
			return _mordexRowLayoutState;
		}

		internal void SetMordexRowLayoutState(MxRowLayoutState mordexRowLayoutState)
		{
			_mordexRowLayoutState = mordexRowLayoutState;
		}

		public void PaintBackground()
		{
			_timeline.DrawDefaultArea(this, _itemDrawn);
		}
	}
}
