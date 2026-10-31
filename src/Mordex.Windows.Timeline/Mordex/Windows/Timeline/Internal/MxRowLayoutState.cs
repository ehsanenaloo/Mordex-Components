using System.Drawing;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline.Internal
{
	internal class MxRowLayoutState
	{
		private MxRowLayoutState _parentRow;

		private MordexRow _node;

		private TimelineGroupRow _timelineGroupRow;

		private bool _isHot;

		private Rectangle _rowBounds = Rectangle.Empty;

		private Rectangle _mordexRowLayoutStateRectangle = Rectangle.Empty;

		private RowType _type = RowType.Record;

		private Rectangle _expandBoxBounds = Rectangle.Empty;

		private int _recordRowCount;

		private int _rowIndex = -1;

		private int _groupIndex = -1;

		public MxRowLayoutState(RowType type, int groupIndex, MxRowLayoutState parentRow)
		{
			_type = type;
			_groupIndex = groupIndex;
			_parentRow = parentRow;
		}

		public MxRowLayoutState(RowType type, MordexRow node, MxRowLayoutState parentRow)
		{
			_type = type;
			_node = node;
			_parentRow = parentRow;
		}

		public MxRowLayoutState GetParentRow()
		{
			return _parentRow;
		}

		public MordexRow GetNode()
		{
			return _node;
		}

		public TimelineGroupRow GetTimelineGroupRow()
		{
			return _timelineGroupRow;
		}

		public void SetTimelineGroupRow(TimelineGroupRow timelineGroupRow)
		{
			_timelineGroupRow = timelineGroupRow;
		}

		public bool IsGroupSelected()
		{
			if (_node is MordexGroupRow mordexGroupRow && mordexGroupRow.DataSourceManager is TimelineDataSourceManager timelineDataSourceManager)
			{
				TimelineGroupRow timelineGroupRow = timelineDataSourceManager.GetGroupRow(mordexGroupRow);
				return timelineGroupRow.Selected;
			}
			return false;
		}

		public bool IsHot()
		{
			return _isHot;
		}

		public void SetHot(bool isHot)
		{
			_isHot = isHot;
		}

		public Rectangle GetRowBounds()
		{
			return _rowBounds;
		}

		public void SetRowBounds(Rectangle rectangle)
		{
			_rowBounds = rectangle;
		}

		public Rectangle GetMordexRowLayoutStateRectangle()
		{
			return _mordexRowLayoutStateRectangle;
		}

		public void SetMordexRowLayoutStateRectangle(Rectangle mordexRowLayoutStateRectangle)
		{
			_mordexRowLayoutStateRectangle = mordexRowLayoutStateRectangle;
		}

		internal RowType GetRowType()
		{
			return _type;
		}

		public Rectangle GetExpandBoxBounds()
		{
			return _expandBoxBounds;
		}

		public void SetExpandBoxBounds(Rectangle expandBoxBounds)
		{
			_expandBoxBounds = expandBoxBounds;
		}

		public void SetRecordRowCount(int recordRowCount)
		{
			_recordRowCount = recordRowCount;
		}

		public int GetRowIndex()
		{
			return _rowIndex;
		}

		public void SetRowIndex(int mordexRowLayoutStateInteger)
		{
			_rowIndex = mordexRowLayoutStateInteger;
		}

		public int GetGroupIndex()
		{
			return _groupIndex;
		}

		public void SetGroupIndex(int groupIndex)
		{
			_groupIndex = groupIndex;
		}

		public bool IsExpanded()
		{
			if (_node != null)
			{
				return _node.Expanded;
			}
			return true;
		}
	}
}
