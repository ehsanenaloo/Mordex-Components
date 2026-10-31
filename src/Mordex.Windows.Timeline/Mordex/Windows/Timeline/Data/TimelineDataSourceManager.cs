using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.Timeline.Data
{
	internal class TimelineDataSourceManager : DataSourceManager
	{
		private MxTimeline _timeline;

		public override bool InvokeRequired => _timeline.InvokeRequired;

		public TimelineDataSourceManager(MxTimeline timeline)
		{
			_timeline = timeline;
			base.DynamicFiltering = true;
			base.HasNonGroupedView = true;
		}

		protected override DataTable GetDataTableCore(IList list)
		{
			if (list is BindingSource bindingSource && bindingSource.List is DataView dataView)
			{
				return dataView.Table;
			}
			return base.GetDataTableCore(list);
		}

		public override void Invoke(Delegate method, params object[] args)
		{
			_timeline.Invoke(method, args);
		}

		protected override void OnRowChange(MordexRow row, RowChangeType change)
		{
			base.OnRowChange(row, change);
			TimelineItemIndex index = _timeline?.Items?._index;
			// Other row trees (their root rows are not this one) send their changes here too; only the rows of this tree are in
			// the order. A deleted row may have left its tree already, so a removal is always taken.
			if (index != null && (row.RootRow == base.RootRow || change == RowChangeType.BeforeDelete || change == RowChangeType.Delete))
			{
				switch (change)
				{
				case RowChangeType.Add:
					index.Add(row, GetTimelineItem(row));
					break;
				case RowChangeType.Change:
				case RowChangeType.Move:
					// A move only changes the place of the row in its sorted list; the set of rows stays the same.
					index.Update(GetTimelineItem(row));
					break;
				case RowChangeType.BeforeDelete:
				case RowChangeType.Delete:
					index.Remove(GetTimelineItem(row));
					break;
				case RowChangeType.ChildRowReset:
					index.ChildRowsReset(row);
					break;
				case RowChangeType.VisibleChanged:
					index.Invalidate();
					break;
				}
			}
			if (_timeline != null && row.RootRow == base.RootRow)
			{
				switch (change)
				{
				case RowChangeType.VisibleChanged:
					_timeline.InvalidateRows();
					break;
				case RowChangeType.Delete:
					_timeline.InvalidateRows();
					break;
				case RowChangeType.BeforeDelete:
					_timeline.ReleaseRowItem(row);
					_timeline.InvalidateRows();
					break;
				case RowChangeType.Change:
				case RowChangeType.Add:
				case RowChangeType.ExpandedChanged:
					_timeline.InvalidateRows();
					break;
				case RowChangeType.ChildRowReset:
					_timeline.ClearSelection();
					_timeline.InvalidateRows();
					break;
				case RowChangeType.Move:
				case RowChangeType.TotalChanged:
					break;
				}
			}
		}

		protected override void OnNonGroupRowManagerChange()
		{
			base.OnNonGroupRowManagerChange();
			_timeline?.Items?._index.RowsChanged(base.NonGroupedRowManager);
		}

		protected override MordexFieldBase CreateMordexField()
		{
			return new TimelineField();
		}

		protected override MordexTableBase CreateMordexTable()
		{
			return new TimelineMordexTableBase(_timeline);
		}

		protected override void OnDataRowRead(DataHolderRow row)
		{
			if (row.Tag == null)
			{
				row.Tag = new TimelineItemState();
			}
			if (_timeline != null)
			{
				_timeline.AttachItemToRow(row);
			}
		}

		protected override void OnDataRowDelete(DataHolderRow row)
		{
			if (row.Tag is TimelineItemState timelineItemState && timelineItemState._timelineItem != null)
			{
				timelineItemState._timelineItem.Selected = false;
				timelineItemState._timelineItem._row = null;
				timelineItemState._timelineItem._owner = null;
				row.Tag = null;
			}
		}

		protected override void GroupComputeEnd()
		{
			_timeline.InvalidateRows();
		}

		internal static TimelineItem GetTimelineItem(MordexRow mordexRow)
		{
			if (mordexRow is MordexDataRow mordexDataRow && mordexDataRow.DataHolder != null && mordexDataRow.DataHolder.Tag is TimelineItemState timelineItemState)
			{
				return timelineItemState._timelineItem;
			}
			return null;
		}

		internal TimelineGroupRow GetGroupRow(MordexGroupRow mordexGroupRow)
		{
			TimelineGroupRowState timelineGroupRowState = mordexGroupRow.Tag as TimelineGroupRowState;
			if (timelineGroupRowState == null)
			{
				timelineGroupRowState = (TimelineGroupRowState)(mordexGroupRow.Tag = new TimelineGroupRowState());
				timelineGroupRowState._timelineGroupRow = CreateFormattedGroupRow(mordexGroupRow);
			}
			return timelineGroupRowState._timelineGroupRow;
		}

		protected override object CreateNewRow(MordexNewDataRow row)
		{
			return _timeline.RaiseGetNewObject(row);
		}

		private TimelineGroupRow CreateFormattedGroupRow(MordexRow mordexRow)
		{
			TimelineGroupRow timelineGroupRow = new TimelineGroupRow(mordexRow as MordexGroupRow, _timeline);
			_timeline.RaiseFormattingGroupRow(timelineGroupRow);
			return timelineGroupRow;
		}
	}
}
