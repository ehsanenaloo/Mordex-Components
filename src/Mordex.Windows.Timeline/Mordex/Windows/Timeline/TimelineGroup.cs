using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public class TimelineGroup : MordexGroupBase, ITimelineOwner, IMordexXmlLayoutsSupport
	{
		private TimelineFormatStyle _formatStyle;

		internal TimelineGroupCollection _owner;

		private string _identifier = "";

		private string _groupPrefix = "";

		string IMordexXmlLayoutsSupport.Identifier
		{
			get
			{
				if (_identifier == "")
				{
					return "Group" + Index;
				}
				return _identifier;
			}
		}

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxTimeline Timeline
		{
			get
			{
				if (_owner != null)
				{
					return _owner.GetTimeline();
				}
				if (GetParentCollection() != null)
				{
					return GetParentCollection().GetTimeline();
				}
				return null;
			}
		}

		[Description("Represents the TimelineField in which the grouping occurs.")]
		[DefaultValue(null)]
		[Editor(typeof(FieldEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(FieldConverter))]
		public TimelineField Field
		{
			get
			{
				return (TimelineField)base.BaseField;
			}
			set
			{
				base.BaseField = value;
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The TimelineFormatStyle object that will be applied to the group rows of this group.")]
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

		[DefaultValue("")]
		[Description("Indicates the format to use when rendering group row values.")]
		public string GroupFormatString
		{
			get
			{
				return base.BaseGroupFormatString;
			}
			set
			{
				base.BaseGroupFormatString = value;
			}
		}

		[Description("Determines how values will be formatted in group header rows.")]
		public FormatMode GroupFormatMode
		{
			get
			{
				return DataState.ToTimelineFormatMode(base.BaseGroupFormatMode);
			}
			set
			{
				base.BaseGroupFormatMode = DataState.ToDataFormatMode(value);
			}
		}

		[DefaultValue(GroupInterval.Default)]
		[Description("Determines thow groups should be calculated")]
		public GroupInterval GroupInterval
		{
			get
			{
				return DataState.ToTimelineGroupInterval(base.BaseGroupInterval);
			}
			set
			{
				base.BaseGroupInterval = DataState.ToDataGroupInterval(value);
			}
		}

		[Description("Represents the key used to identify an object in a colletion.")]
		[DefaultValue("")]
		public string Key
		{
			get
			{
				return _identifier;
			}
			set
			{
				_identifier = value;
			}
		}

		[Description("Indicates the order in which groups will be sorted.")]
		[DefaultValue(SortOrder.Ascending)]
		public SortOrder SortOrder
		{
			get
			{
				return DataState.ToTimelineSortOrder(base.BaseSortOrder);
			}
			set
			{
				base.BaseSortOrder = DataState.ToDataSortOrder(value);
			}
		}

		[DefaultValue("")]
		[Description("Represents the string displayed in a group header row before the group row value.")]
		public string GroupPrefix
		{
			get
			{
				return _groupPrefix;
			}
			set
			{
				if (value != _groupPrefix)
				{
					_groupPrefix = value;
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (Timeline != null)
			{
				int fieldIndex = (int)LayoutReader.GetPropertyValue(info, "FieldIndex", typeof(int), -1);
				if (fieldIndex >= 0 && fieldIndex < Timeline.Fields.Count)
				{
					Field = Timeline.Fields[fieldIndex];
				}
			}
			GroupFormatString = (string)LayoutReader.GetPropertyValue(info, "GroupFormatString", typeof(string), null);
			GroupPrefix = (string)LayoutReader.GetPropertyValue(info, "GroupPrefix", typeof(string), null);
			SortOrder = (SortOrder)LayoutReader.GetPropertyValue(info, "SortOrder", typeof(SortOrder), SortOrder.Ascending);
			_identifier = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			GroupFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "GroupFormatMode", typeof(FormatMode), FormatMode.UseDefault);
			GroupInterval = (GroupInterval)LayoutReader.GetPropertyValue(info, "GroupInterval", typeof(GroupInterval), GroupInterval.Default);
			info.DeserializeObject("FormatStyle", FormatStyle);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Field != null && Field.Index != -1)
			{
				info.AddValue("FieldIndex", Field.Index);
			}
			if (GroupFormatString != null)
			{
				info.AddValue("GroupFormatString", GroupFormatString);
			}
			if (_groupPrefix != null)
			{
				info.AddValue("GroupPrefix", _groupPrefix);
			}
			if (SortOrder != SortOrder.Ascending)
			{
				info.AddValue("SortOrder", SortOrder);
			}
			if (_identifier != "")
			{
				info.AddValue("Key", _identifier);
			}
			if (ShouldSerializeFormatStyle())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (GroupFormatMode != FormatMode.UseDefault)
			{
				info.AddValue("GroupFormatMode", GroupFormatMode);
			}
			if (ShouldSerializeGroupInterval())
			{
				info.AddValue("GroupInterval", GroupInterval);
			}
		}

		public TimelineGroup(TimelineField field)
			: base(field)
		{
		}

		public TimelineGroup()
		{
		}

		public TimelineGroup(TimelineField field, SortOrder sortOrder)
			: base(field, DataState.ToDataSortOrder(sortOrder))
		{
		}

		internal TimelineGroupCollection GetParentCollection()
		{
			return (TimelineGroupCollection)base.BaseParentCollection;
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupInterval()
		{
			return ShouldSerializeBaseGroupInterval();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupInterval()
		{
			GroupInterval = GroupInterval.Default;
		}

		internal bool BelongsToTimeline(MxTimeline timeline)
		{
			if (timeline == null)
			{
				return true;
			}
			if (Field == null)
			{
				return false;
			}
			if (timeline == Field.Timeline)
			{
				return true;
			}
			return false;
		}

		internal void SetParentCollection(TimelineGroupCollection parent)
		{
			BaseSetParentCollection(parent);
		}
	}
}
