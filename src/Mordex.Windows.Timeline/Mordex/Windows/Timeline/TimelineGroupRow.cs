using System;
using System.Collections;
using System.Drawing;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public class TimelineGroupRow : ITimelineObject
	{
		internal Rectangle _bounds;

		private MordexGroupRow _groupRow;

		private MxTimeline _timeline;

		private object _tag;

		private TimelineFormatStyle _timelineFormatStyle;

		private string _imageKey = "";

		private int _imageIndex = -1;

		private string _groupCaption = "";

		private bool _selected;

		private Image _groupImage;

		public bool Expanded
		{
			get
			{
				return _groupRow.Expanded;
			}
			set
			{
				_groupRow.Expanded = value;
			}
		}

		public MxTimeline Timeline => _timeline;

		public TimelineGroup Group => (TimelineGroup)_groupRow.Group;

		public object Value => _groupRow.OriginalGroupValue;

		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		public TimelineGroupRow ParentRow
		{
			get
			{
				if (_groupRow.ParentRow is MordexGroupRow mordexGroupRow && _timeline != null)
				{
					return _timeline.GetTimelineDataSourceManager().GetGroupRow(mordexGroupRow);
				}
				return null;
			}
		}

		ItemType ITimelineObject.ItemType => ItemType.GroupRow;

		bool ITimelineObject.Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
			}
		}

		TimelineGroupRow ITimelineObject.GroupRow => this;

		TimelineItem ITimelineObject.Item => null;

		public TimelineFormatStyle FormatStyle => _timelineFormatStyle;

		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (value != _imageKey)
				{
					_imageKey = value;
					_imageIndex = -1;
					InvalidateTimeline();
				}
			}
		}

		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					InvalidateTimeline();
				}
			}
		}

		public string GroupCaption
		{
			get
			{
				return _groupCaption;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_groupCaption != value)
				{
					_groupCaption = value;
					InvalidateTimeline();
				}
			}
		}

		public bool Selected => _selected;

		public Image Image
		{
			get
			{
				return _groupImage;
			}
			set
			{
				if (_groupImage != value)
				{
					_groupImage = value;
					InvalidateTimeline();
				}
			}
		}

		internal TimelineGroupRow(MordexGroupRow groupRow, MxTimeline timeline)
		{
			_groupRow = groupRow;
			_timeline = timeline;
			_timelineFormatStyle = new TimelineFormatStyle(timeline, PredefinedStyle.None);
			TimelineField field = ((TimelineGroup)groupRow.Group).Field;
			_groupCaption = groupRow.GroupText;
			_imageIndex = field.ImageIndex;
			_imageKey = field.ImageKey;
			if (field.HasValueList && field.ValueList.ReplaceValues)
			{
				TimelineValueListItem timelineValueListItem = field.ValueList[Value];
				if (timelineValueListItem != null)
				{
					if (timelineValueListItem.Image != null)
					{
						_groupImage = timelineValueListItem.Image;
					}
					else if (timelineValueListItem.ImageKey.Length > 0)
					{
						_imageKey = timelineValueListItem.ImageKey;
					}
					else if (timelineValueListItem.ImageIndex != -1)
					{
						_imageIndex = timelineValueListItem.ImageIndex;
					}
					_groupCaption = timelineValueListItem.Text;
				}
			}
			_groupCaption = ((TimelineGroup)groupRow.Group).GroupPrefix + " " + _groupCaption;
		}

		internal string GetCaptionSuffix()
		{
			return GetTotalsText();
		}

		internal MordexGroupRow GetGroupRow()
		{
			return _groupRow;
		}

		private string GetTotalsText()
		{
			string text = "";
			if (_timeline.GroupHeaderTotals.Count > 0)
			{
				foreach (TimelineGroupHeaderTotal groupHeaderTotal in _timeline.GroupHeaderTotals)
				{
					if (!groupHeaderTotal.IsValid(_timeline))
					{
						continue;
					}
					TimelineField timelineField = groupHeaderTotal.Field;
					if (timelineField == null && _timeline.Fields.Count > 0)
					{
						timelineField = _timeline.Fields[0];
					}
					if (timelineField != null)
					{
						object subTotal = GetSubTotal(timelineField, groupHeaderTotal.AggregateFunction, groupHeaderTotal.FilterCondition);
						string totalText = (TimelineUtilities.IsNullOrDBNull(subTotal) ? "" : ((groupHeaderTotal.TotalFormatMode == FormatMode.UseIFormattable) ? ((!(subTotal is IFormattable) || groupHeaderTotal.TotalFormatString.Length <= 0) ? subTotal.ToString() : ((IFormattable)subTotal).ToString(groupHeaderTotal.TotalFormatString, null)) : ((groupHeaderTotal.TotalFormatString.Length <= 0) ? subTotal.ToString() : string.Format(groupHeaderTotal.TotalFormatString, subTotal))));
						if (totalText.Length > 0)
						{
							totalText = groupHeaderTotal.TotalPrefix + totalText;
							totalText += groupHeaderTotal.TotalSuffix;
							text = text + " " + totalText;
						}
					}
				}
			}
			return text;
		}

		public object GetSubTotal(TimelineField field, AggregateFunction aggregateFunction, TimelineFilterCondition filterCondition)
		{
			return _groupRow.GetTotal(field, DataState.ToDataAggregateFunction(aggregateFunction), filterCondition);
		}

		public object GetSubTotal(TimelineField field, AggregateFunction aggregateFunction)
		{
			return GetSubTotal(field, aggregateFunction, null);
		}

		internal void AddItemsTo(ArrayList arrayList)
		{
			MordexRow[] dataRows = _groupRow.GetDataRows(_groupRow.Table);
			MordexRow[] rows = dataRows;
			foreach (MordexRow mordexRow in rows)
			{
				TimelineItem timelineItem = TimelineDataSourceManager.GetTimelineItem(mordexRow);
				if (timelineItem != null)
				{
					arrayList.Add(timelineItem);
				}
			}
		}

		public ArrayList GetTimelineItems()
		{
			ArrayList arrayList = new ArrayList();
			AddItemsTo(arrayList);
			return arrayList;
		}

		public TimelineGroupRow[] GetGroupRows()
		{
			MxTimeline timeline = Timeline;
			if (_groupRow.Rows.Group != null && timeline != null)
			{
				TimelineDataSourceManager timelineDataSourceManager = timeline.GetTimelineDataSourceManager();
				TimelineGroupRow[] childGroupRows = new TimelineGroupRow[_groupRow.Rows.Count];
				for (int i = 0; i < _groupRow.Rows.Count; i++)
				{
					if (_groupRow.Rows[i] is MordexGroupRow mordexGroupRow)
					{
						childGroupRows[i] = timelineDataSourceManager.GetGroupRow(mordexGroupRow);
					}
				}
				return childGroupRows;
			}
			return new TimelineGroupRow[0];
		}

		public object[] GetChildren()
		{
			MxTimeline timeline = Timeline;
			if (timeline != null)
			{
				TimelineDataSourceManager timelineDataSourceManager = timeline.GetTimelineDataSourceManager();
				object[] children = new object[_groupRow.Rows.Count];
				for (int i = 0; i < _groupRow.Rows.Count; i++)
				{
					if (_groupRow.Rows[i] is MordexGroupRow mordexGroupRow)
					{
						children[i] = timelineDataSourceManager.GetGroupRow(mordexGroupRow);
					}
					else
					{
						children[i] = TimelineDataSourceManager.GetTimelineItem(_groupRow.Rows[i]);
					}
				}
				return children;
			}
			return new object[0];
		}

		private void InvalidateTimeline()
		{
			if (_timeline != null)
			{
				_timeline.Invalidate();
			}
		}
	}
}
