using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Timeline
{
	public class TimelineItem : ITimelineOwner, ITimelineObject, IMordexXmlLayoutsSupport
	{
		internal MordexRow _row;

		private bool _allDayEvent;

		private object _tag;

		private Image _image;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private bool _isLabelVisible = true;

		private int _groupIndex;

		private TimelineFormatStyle _itemEstimatedBarFormatStyle;

		private TimelineFormatStyle _itemBarFomatStyle;

		private TimelineFormatStyle _formatStyle;

		internal TimeSpan _duration = TimeSpan.Zero;

		private DateTime _startTime = DateTime.MinValue;

		private DateTime _estimatedStartTime = DateTime.MinValue;

		private DateTime _endTime = DateTime.MinValue;

		internal DateTime _estimatedEndTime = DateTime.MinValue;

		private string _text = "";

		private string _toolTipText = "";

		private string _comments = "";

		private bool _isVisible = true;

		private TimelineItem _sourceItem;

		private string _key = "";

		internal TimelineItemCollection _owner;

		internal double _doubleValue;

		internal double _leftOffset;

		internal double _estimatedStartOffset;

		internal double _estimatedDurationWidth;

		internal double _estimatedDurationLength;

		internal double _durationLength;

		internal double _durationWidth;

		internal double _startOffset;

		internal Rectangle _durationBarBounds = Rectangle.Empty;

		internal RectangleF _rectangleF = Rectangle.Empty;

		internal Rectangle _estimatedBarBounds = Rectangle.Empty;

		private int _rowIndex;

		internal bool _selected;

		private int _subRowIndex;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Specifies whether the TimelineItem is an all-day event.")]
		public bool AllDayEvent
		{
			get
			{
				if (_allDayEvent && (StartTime.TimeOfDay != TimeSpan.Zero || EndTime.TimeOfDay != TimeSpan.Zero))
				{
					return false;
				}
				return _allDayEvent;
			}
			set
			{
				if (AllDayEvent != value || value)
				{
					_allDayEvent = value;
					if (_allDayEvent)
					{
						StartTime = StartTime.Date;
						EndTime = EndTime.Date.AddDays(1.0);
					}
					else
					{
						EndTime = StartTime;
					}
				}
			}
		}

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

		public float Width => _rectangleF.Width;

		public float DurationBarWidth => _durationBarBounds.Width;

		[Browsable(false)]
		[DefaultValue(-1)]
		public int Index
		{
			get
			{
				if (_owner != null)
				{
					return _owner.IndexOf(this);
				}
				return -1;
			}
		}

		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					InvalidateControl();
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
					InvalidateControl();
				}
			}
		}

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
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					InvalidateControl();
				}
			}
		}

		ItemType ITimelineObject.ItemType => ItemType.TimelineItem;

		TimelineGroupRow ITimelineObject.GroupRow => null;

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

		TimelineItem ITimelineObject.Item => this;

		public TimelineFormatStyle ItemEstimatedBarFormatStyle
		{
			get
			{
				if (_itemEstimatedBarFormatStyle == null)
				{
					_itemEstimatedBarFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
				}
				return _itemEstimatedBarFormatStyle;
			}
		}

		public TimelineFormatStyle ItemBarFomatStyle
		{
			get
			{
				if (_itemBarFomatStyle == null)
				{
					_itemBarFomatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
				}
				return _itemBarFomatStyle;
			}
		}

		public TimelineFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
				}
				return _formatStyle;
			}
		}

		public TimeSpan Duration
		{
			get
			{
				try
				{
					DateTime endTime = EndTime;
					DateTime startTime = StartTime;
					if (endTime <= startTime)
					{
						return TimeSpan.Zero;
					}
					_duration = endTime.Subtract(startTime);
					return _duration;
				}
				catch
				{
					return TimeSpan.MinValue;
				}
			}
			set
			{
				if (_duration != value)
				{
					_duration = value;
					EndTime = StartTime.AddTicks(_duration.Ticks);
					InvalidateControl();
				}
			}
		}

		public DateTime StartTime
		{
			get
			{
				if (Timeline != null)
				{
					return GetRowDateTime(Timeline.GetStartTimeField(), _startTime);
				}
				return _startTime;
			}
			set
			{
				if (!(StartTime != value))
				{
					return;
				}
				ItemChangeCancelEventArgs e = new ItemChangeCancelEventArgs(this, ItemChange.StartTime, value);
				RaiseItemChanging(e);
				if (!e.Cancel)
				{
					value = (DateTime)e.ProposedValue;
					_startTime = value;
					if (value > EndTime)
					{
						EndTime = value;
					}
					if (Timeline != null)
					{
						WriteRowValue(Timeline.GetStartTimeField(), value, true);
					}
					RaiseItemChanged(ItemChange.StartTime);
				}
			}
		}

		public DateTime EstimatedStartTime
		{
			get
			{
				if (Timeline != null)
				{
					return GetRowDateTime(Timeline.GetEstimatedStartTimeField(), _estimatedStartTime);
				}
				return _estimatedStartTime;
			}
			set
			{
				if (!(_estimatedStartTime != value))
				{
					return;
				}
				ItemChangeCancelEventArgs e = new ItemChangeCancelEventArgs(this, ItemChange.EstimatedStartTime, value);
				RaiseItemChanging(e);
				if (!e.Cancel)
				{
					value = (DateTime)e.ProposedValue;
					_estimatedStartTime = value;
					if (value > _estimatedEndTime && _estimatedEndTime == DateTime.MinValue)
					{
						EstimatedEndTime = value;
					}
					if (Timeline != null)
					{
						WriteRowValue(Timeline.GetEstimatedStartTimeField(), value, true);
					}
					RaiseItemChanged(ItemChange.EstimatedStartTime);
				}
			}
		}

		public DateTime EndTime
		{
			get
			{
				if (Timeline != null)
				{
					return GetRowDateTime(Timeline.GetEndTimeField(), _endTime);
				}
				return _endTime;
			}
			set
			{
				if (!(EndTime != value))
				{
					return;
				}
				ItemChangeCancelEventArgs e = new ItemChangeCancelEventArgs(this, ItemChange.EndTime, value);
				RaiseItemChanging(e);
				if (!e.Cancel)
				{
					value = (DateTime)e.ProposedValue;
					_endTime = value;
					if (value < StartTime)
					{
						StartTime = value;
					}
					if (Timeline != null)
					{
						WriteRowValue(Timeline.GetEndTimeField(), value, true);
					}
					RaiseItemChanged(ItemChange.EndTime);
				}
			}
		}

		public DateTime EstimatedEndTime
		{
			get
			{
				if (Timeline != null)
				{
					return GetRowDateTime(Timeline.GetEstimatedEndTimeField(), _estimatedEndTime);
				}
				return _estimatedEndTime;
			}
			set
			{
				if (!(_estimatedEndTime != value))
				{
					return;
				}
				ItemChangeCancelEventArgs e = new ItemChangeCancelEventArgs(this, ItemChange.EstimatedEndTime, value);
				RaiseItemChanging(e);
				if (!e.Cancel)
				{
					value = (DateTime)e.ProposedValue;
					_estimatedEndTime = value;
					if (value < _estimatedStartTime)
					{
						EstimatedStartTime = value;
					}
					if (Timeline != null)
					{
						WriteRowValue(Timeline.GetEstimatedEndTimeField(), value, true);
					}
					RaiseItemChanged(ItemChange.EstimatedEndTime);
				}
			}
		}

		public string Text
		{
			get
			{
				if (Timeline != null)
				{
					return string.Concat(GetRowValueOrDefault(Timeline.GetTextField(), _text));
				}
				return _text;
			}
			set
			{
				if (!(Text != value))
				{
					return;
				}
				ItemChangeCancelEventArgs e = new ItemChangeCancelEventArgs(this, ItemChange.Text, value);
				RaiseItemChanging(e);
				if (!e.Cancel)
				{
					value = (string)e.ProposedValue;
					_text = value;
					if (Timeline != null)
					{
						WriteRowValue(Timeline.GetTextField(), value, true);
					}
					RaiseItemChanged(ItemChange.Text);
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("The text that is shown when the mouse hovers over this item")]
		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
			}
		}

		[DefaultValue("")]
		public string Comments
		{
			get
			{
				return _comments;
			}
			set
			{
				if (_comments != value)
				{
					_comments = value;
				}
			}
		}

		public object DataRow
		{
			get
			{
				if (GetSourceItem() != null)
				{
					return GetSourceItem().DataRow;
				}
				if (_row != null)
				{
					return _row.DataRow;
				}
				return null;
			}
		}

		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		public MxTimeline Timeline
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Timeline;
				}
				return null;
			}
		}

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected != value)
				{
					if (Timeline != null && _selected)
					{
						Timeline.RemoveFromSelection(this);
					}
					_selected = value;
					if (Timeline != null && _selected)
					{
						Timeline.UpdateTimelineSelectedItemCollection(this);
					}
				}
			}
		}

		public TimelineGroupRow ParentRow
		{
			get
			{
				if (Timeline != null)
				{
					MordexRow mordexRow = Timeline.GetTimelineDataSourceManager().FindDataRow(DataRow);
					if (mordexRow != null)
					{
						return Timeline.GetGroupRow(mordexRow.ParentRow);
					}
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Text != "")
			{
				info.AddValue("Text", Text);
			}
			if (_allDayEvent)
			{
				info.AddValue("AllDayEvent", _allDayEvent);
			}
			if (EndTime != DateTime.MinValue)
			{
				info.AddValue("EndTime", EndTime);
			}
			if (ShouldSerializeFormatStyle())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_image != null)
			{
				info.AddValue("Image", _image);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_toolTipText != "")
			{
				info.AddValue("ToolTipText", _toolTipText);
			}
			if (ShouldSerializeItemBarFormatStyle())
			{
				info.AddValue("ItemBarFormatStyle", _itemBarFomatStyle);
			}
			if (ShouldSerializeItemEstimatedBarFormatStyle())
			{
				info.AddValue("ItemEstimatedBarFormatStyle", _itemEstimatedBarFormatStyle);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (StartTime != DateTime.MinValue)
			{
				info.AddValue("StartTime", StartTime);
			}
			if (EstimatedEndTime != DateTime.MaxValue)
			{
				info.AddValue("EstimatedEndTime", _estimatedEndTime);
			}
			if (EstimatedStartTime != DateTime.MaxValue)
			{
				info.AddValue("EstimatedStartTime", _estimatedStartTime);
			}
			if (_comments != "")
			{
				info.AddValue("Comments", _comments);
			}
			if (Timeline != null)
			{
				int fieldIndex = 0;
				foreach (TimelineField field in Timeline.Fields)
				{
					if (field.DataMemberMapping == DataMemberMapping.Custom)
					{
						object value = GetValue(field);
						if (value != null)
						{
							info.AddValue("Field" + fieldIndex, value);
						}
					}
					fieldIndex++;
				}
			}
			if (_tag != null)
			{
				if (!(_tag is string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), "");
			_allDayEvent = (bool)LayoutReader.GetPropertyValue(info, "AllDayEvent", typeof(bool), false);
			EndTime = (DateTime)LayoutReader.GetPropertyValue(info, "EndTime", typeof(DateTime), DateTime.MinValue);
			_toolTipText = (string)LayoutReader.GetPropertyValue(info, "ToolTipText", typeof(string), "");
			info.DeserializeObject("FormatStyle", FormatStyle);
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			_comments = (string)LayoutReader.GetPropertyValue(info, "Comments", typeof(string), "");
			info.DeserializeObject("ItemBarFormatStyle", ItemBarFomatStyle);
			info.DeserializeObject("ItemEstimatedBarFormatStyle", ItemEstimatedBarFormatStyle);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			StartTime = (DateTime)LayoutReader.GetPropertyValue(info, "StartTime", typeof(DateTime), DateTime.MinValue);
			EstimatedEndTime = (DateTime)LayoutReader.GetPropertyValue(info, "EstimatedEndTime", typeof(DateTime), DateTime.MinValue);
			EstimatedStartTime = (DateTime)LayoutReader.GetPropertyValue(info, "EstimatedStartTime", typeof(DateTime), DateTime.MinValue);
			_duration = EndTime.Subtract(StartTime);
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text == null)
			{
				return;
			}
			string tagTypeName = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
			if (tagTypeName != null)
			{
				try
				{
					Type type = LayoutTypeResolver.Resolve(tagTypeName);
					_tag = PropertyValue.ConvertValue(text, type);
					return;
				}
				catch
				{
					_tag = text;
					return;
				}
			}
			_tag = text;
		}

		internal TimelineItem(MordexRow row)
		{
			_row = row;
		}

		public TimelineItem()
		{
		}

		public TimelineItem(DateTime startTime, string text)
		{
			StartTime = startTime;
			Text = text;
		}

		public TimelineItem(DateTime startTime, DateTime endTime, string text, int imageIndex)
		{
			StartTime = startTime;
			EndTime = endTime;
			Text = text;
			_imageIndex = imageIndex;
		}

		public TimelineItem(DateTime startTime, DateTime endTime, string text)
			: this(startTime, endTime, text, -1)
		{
		}

		public TimelineItem(DateTime startTime, DateTime endTime)
			: this(startTime, endTime, "", -1)
		{
		}

		public void SetValue(TimelineField field, object value)
		{
			if (_row != null && field != null)
			{
				_row.BeginEdit();
				_row.SetValue(field, value);
				_row.EndEdit();
			}
		}

		internal bool IsLabelVisible()
		{
			return _isLabelVisible;
		}

		internal void SetLabelVisible(bool isLabelVisible)
		{
			_isLabelVisible = isLabelVisible;
		}

		public void SetValue(string fieldKey, object value)
		{
			if (_row != null)
			{
				TimelineField field = (TimelineField)((IMordexTable)_row.Table).Fields[fieldKey];
				SetValue(field, value);
			}
		}

		internal int GetGroupIndex()
		{
			return _groupIndex;
		}

		internal void SetGroupIndex(int timelineItemInteger)
		{
			_groupIndex = timelineItemInteger;
		}

		public void SetValue(int fieldIndex, object value)
		{
			if (_row != null)
			{
				TimelineField field = (TimelineField)((IMordexTable)_row.Table).Fields[fieldIndex];
				SetValue(field, value);
			}
		}

		public object GetValue(TimelineField field)
		{
			if (_row != null)
			{
				return _row.GetValue(field);
			}
			return null;
		}

		public object GetValue(string fieldKey)
		{
			if (_row != null)
			{
				IMordexField mordexField = ((IMordexTable)_row.Table).Fields[fieldKey];
				if (mordexField != null)
				{
					return _row.GetValue((MordexFieldBase)mordexField);
				}
			}
			return null;
		}

		public object GetValue(int fieldIndex)
		{
			if (_row != null)
			{
				IMordexField mordexField = ((IMordexTable)_row.Table).Fields[fieldIndex];
				return _row.GetValue((MordexFieldBase)mordexField);
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemEstimatedBarFormatStyle()
		{
			if (_itemEstimatedBarFormatStyle != null)
			{
				_itemEstimatedBarFormatStyle = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemEstimatedBarFormatStyle()
		{
			if (_itemEstimatedBarFormatStyle != null)
			{
				return !_itemEstimatedBarFormatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemBarFormatStyle()
		{
			if (_itemBarFomatStyle != null)
			{
				_itemBarFomatStyle = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemBarFormatStyle()
		{
			if (_itemBarFomatStyle != null)
			{
				return !_itemBarFomatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			if (_formatStyle != null)
			{
				_formatStyle = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle != null)
			{
				return !_formatStyle.HasOnlyDefaultValues();
			}
			return false;
		}

		internal TimeSpan GetEstimatedDuration()
		{
			try
			{
				DateTime estimatedEndTime = EstimatedEndTime;
				DateTime estimatedStartTime = EstimatedStartTime;
				if (estimatedEndTime <= estimatedStartTime)
				{
					return TimeSpan.Zero;
				}
				return estimatedEndTime.Subtract(estimatedStartTime);
			}
			catch
			{
				return TimeSpan.MinValue;
			}
		}

		private object GetRowValueOrDefault(string dataMember, object value)
		{
			if (!string.IsNullOrEmpty(dataMember) && _row != null)
			{
				return _row.GetValue(dataMember);
			}
			return value;
		}

		private DateTime GetRowDateTime(string text, DateTime dateTime)
		{
			object mappedValue = GetRowValueOrDefault(text, dateTime);
			if (mappedValue is DateTime)
			{
				return (DateTime)mappedValue;
			}
			return dateTime;
		}

		private void WriteRowValue(string dataMember, object newValue, bool shouldWrapInEdit)
		{
			if (_row != null && !string.IsNullOrEmpty(dataMember))
			{
				if (shouldWrapInEdit)
				{
					_row.BeginEdit();
				}
				_row.SetValue(dataMember, newValue);
				if (shouldWrapInEdit)
				{
					_row.EndEdit();
				}
			}
		}

		internal void RaiseItemChanged(ItemChange itemChange)
		{
			if (Timeline != null)
			{
				Timeline.RaiseItemChanged(this, itemChange);
			}
		}

		internal void RaiseItemChanging(ItemChangeCancelEventArgs eventArgs)
		{
			if (Timeline != null)
			{
				Timeline.RaiseItemChanging(eventArgs);
			}
		}

		public TimelineItem Clone()
		{
			TimelineItem timelineItem = CreateCopy();
			timelineItem.SetSourceItem(null);
			return timelineItem;
		}

		internal TimelineItem CreateCopy()
		{
			TimelineItem timelineItem = new TimelineItem();
			timelineItem._allDayEvent = AllDayEvent;
			timelineItem._comments = Comments;
			timelineItem._duration = Duration;
			timelineItem._endTime = EndTime;
			timelineItem._formatStyle = new TimelineFormatStyle(FormatStyle);
			timelineItem._formatStyle._owner = timelineItem;
			timelineItem._image = Image;
			timelineItem._imageIndex = ImageIndex;
			timelineItem._imageKey = ImageKey;
			timelineItem._itemBarFomatStyle = new TimelineFormatStyle(ItemBarFomatStyle);
			timelineItem._itemEstimatedBarFormatStyle = new TimelineFormatStyle(ItemEstimatedBarFormatStyle);
			timelineItem._itemBarFomatStyle._owner = timelineItem;
			timelineItem._itemEstimatedBarFormatStyle._owner = timelineItem;
			timelineItem._key = Key;
			timelineItem._selected = Selected;
			timelineItem._startTime = StartTime;
			timelineItem._estimatedEndTime = EstimatedEndTime;
			timelineItem._estimatedStartTime = EstimatedStartTime;
			timelineItem._startOffset = _startOffset;
			timelineItem._estimatedStartOffset = _estimatedStartOffset;
			timelineItem._tag = Tag;
			timelineItem.Text = Text;
			timelineItem._toolTipText = ToolTipText;
			timelineItem._row = _row;
			timelineItem._leftOffset = _leftOffset;
			timelineItem._doubleValue = _doubleValue;
			timelineItem._rectangleF = _rectangleF;
			timelineItem.SetRowIndex(GetRowIndex());
			timelineItem.SetSubRowIndex(GetSubRowIndex());
			timelineItem._durationLength = _durationLength;
			timelineItem._estimatedDurationLength = _estimatedDurationLength;
			timelineItem._durationWidth = _durationWidth;
			timelineItem._estimatedDurationWidth = _estimatedDurationWidth;
			timelineItem._estimatedBarBounds = _estimatedBarBounds;
			timelineItem._durationBarBounds = _durationBarBounds;
			timelineItem.SetSourceItem(this);
			return timelineItem;
		}

		internal bool IsVisibleInRow()
		{
			if (_isVisible && _row != null)
			{
				return _row.Visible;
			}
			return _isVisible;
		}

		internal void SetVisible(bool timelineItemBoolean)
		{
			_isVisible = timelineItemBoolean;
		}

		internal TimelineItem GetSourceItem()
		{
			return _sourceItem;
		}

		internal void SetSourceItem(TimelineItem timelineItem)
		{
			_sourceItem = timelineItem;
		}

		internal void UpdateTimelineItem()
		{
			if (_sourceItem != null)
			{
				_leftOffset = _sourceItem._leftOffset;
				_estimatedStartOffset = _sourceItem._estimatedStartOffset;
				_startOffset = _sourceItem._startOffset;
				_doubleValue = _sourceItem._doubleValue;
				_rectangleF = _sourceItem._rectangleF;
				SetRowIndex(_sourceItem.GetRowIndex());
				SetSubRowIndex(_sourceItem.GetSubRowIndex());
				_startTime = _sourceItem.StartTime;
				_endTime = _sourceItem.EndTime;
				_estimatedStartTime = _sourceItem.EstimatedStartTime;
				_estimatedEndTime = _sourceItem.EstimatedEndTime;
			}
		}

		public void Move(DateTime startTime, DateTime endTime)
		{
			if (endTime < startTime)
			{
				throw new InvalidOperationException();
			}
			ItemChange itemChange = ItemChange.StartTimeEndTime;
			ItemChangeCancelEventArgs e = new ItemChangeCancelEventArgs(this, itemChange, startTime);
			RaiseItemChanging(e);
			if (e.Cancel)
			{
				return;
			}
			startTime = (DateTime)e.ProposedValue;
			_startTime = startTime;
			_endTime = endTime;
			if (Timeline != null && _row != null)
			{
				_row.BeginEdit();
				if (startTime < EndTime)
				{
					if (!string.IsNullOrEmpty(Timeline.GetStartTimeField()))
					{
						_row.SetValue(Timeline.GetStartTimeField(), startTime);
					}
					if (!string.IsNullOrEmpty(Timeline.GetEndTimeField()))
					{
						_row.SetValue(Timeline.GetEndTimeField(), endTime);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(Timeline.GetEndTimeField()))
					{
						_row.SetValue(Timeline.GetEndTimeField(), endTime);
					}
					if (!string.IsNullOrEmpty(Timeline.GetStartTimeField()))
					{
						_row.SetValue(Timeline.GetStartTimeField(), startTime);
					}
				}
				_row.EndEdit();
			}
			RaiseItemChanged(itemChange);
		}

		internal int GetRecordPosition()
		{
			if (_row != null)
			{
				return _row.RecordPosition;
			}
			return -1;
		}

		internal void UpdateTimelineItemRectangle(double doubleValue, int barHeight)
		{
			RectangleF rectangleF = _rectangleF;
			rectangleF.X = (float)doubleValue;
			rectangleF.Height = barHeight;
			rectangleF.Width = (float)_estimatedDurationWidth;
			Rectangle rectangle = TimelineUtilities.RoundToRectangle(rectangleF);
			if (rectangle.Width <= 0 && EstimatedEndTime != DateTime.MinValue)
			{
				rectangle.Width = 1;
			}
			else if (rectangle.Width < 0)
			{
				rectangle.Width = 0;
			}
			_estimatedBarBounds = rectangle;
		}

		internal void UpdateDurationBarBounds(double doubleValue, int barHeight)
		{
			RectangleF rectangleF = _rectangleF;
			MxTimeline timeline = Timeline;
			if (timeline == null && _sourceItem != null)
			{
				timeline = _sourceItem.Timeline;
			}
			if (timeline != null && timeline.ShowItemsEstimatedDurationBar)
			{
				rectangleF.Y += barHeight + 1;
			}
			rectangleF.X = (float)doubleValue;
			rectangleF.Height = barHeight;
			rectangleF.Width = (float)_durationWidth;
			Rectangle rectangle = TimelineUtilities.RoundToRectangle(rectangleF);
			if (rectangle.Width <= 0)
			{
				rectangle.Width = 1;
			}
			_durationBarBounds = rectangle;
		}

		internal int GetRowIndex()
		{
			return _rowIndex;
		}

		internal void SetRowIndex(int rowIndex)
		{
			_rowIndex = rowIndex;
		}

		private void InvalidateControl()
		{
			if (_owner != null)
			{
				_owner.Timeline.Invalidate();
			}
		}

		internal int GetSubRowIndex()
		{
			return _subRowIndex;
		}

		internal void SetSubRowIndex(int subRowIndex)
		{
			_subRowIndex = subRowIndex;
		}
	}
}
