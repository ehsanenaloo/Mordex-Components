using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public class TimelineField : MordexFieldBase, IMordexXmlLayoutsSupport
	{
		private string _defaultGroupPrefix;

		private bool _readOnly;

		private DataMemberMapping _dataMemberMapping = DataMemberMapping.Custom;

		private TypeCode _typeCode = TypeCode.String;

		private Type _type;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private bool _itemsImage;

		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Determines how groups should be calculated")]
		[DefaultValue(GroupInterval.Value)]
		public GroupInterval DefaultGroupInterval
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

		[DefaultValue(true)]
		[Description("Indicates whether the casing should be ignored when comparing text.")]
		public bool CompareIgnoreCase
		{
			get
			{
				return base.BaseCompareIgnoreCase;
			}
			set
			{
				base.BaseCompareIgnoreCase = value;
			}
		}

		[Description("Determines how values will be formatted in total rows.")]
		public FormatMode DefaultGroupFormatMode
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

		[Description("The format to use when rendering group row values.")]
		public string DefaultGroupFormatString
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

		[Description("Represents the string displayed in a group header row before the group row value.")]
		[DefaultValue(null)]
		public string DefaultGroupPrefix
		{
			get
			{
				if (_defaultGroupPrefix == null)
				{
					return "";
				}
				return _defaultGroupPrefix;
			}
			set
			{
				if (value != _defaultGroupPrefix)
				{
					_defaultGroupPrefix = value;
					UpdateLayoutIfGrouped();
				}
			}
		}

		[Description("Indicates the name of a property in the data source from which the field gets its data.")]
		[DefaultValue("")]
		public string DataMember
		{
			get
			{
				return base.BaseDataMember;
			}
			set
			{
				_dataMemberMapping = DataMemberMapping.Custom;
				base.BaseDataMember = value;
			}
		}

		[DefaultValue(FieldCompareTarget.Value)]
		[Description("Indicates whether sort, find and filter operations sholud be done against the value or the display text of an item")]
		public FieldCompareTarget CompareTarget
		{
			get
			{
				return DataState.ToTimelineFieldCompareTarget(base.BaseCompareTarget);
			}
			set
			{
				base.BaseCompareTarget = DataState.ToDataCompareTarget(value);
			}
		}

		[Description("Represents the text displayed in a group row when the value grouped is an empty string or null.")]
		[DefaultValue("(none)")]
		public string GroupEmptyString
		{
			get
			{
				return base.BaseGroupEmptyString;
			}
			set
			{
				base.BaseGroupEmptyString = value;
			}
		}

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				_readOnly = value;
			}
		}

		[Description("Specifies to which property of a TimelineItem the field is bound to.")]
		[DefaultValue(DataMemberMapping.Custom)]
		public DataMemberMapping DataMemberMapping
		{
			get
			{
				return _dataMemberMapping;
			}
			set
			{
				if (_dataMemberMapping != value)
				{
					_dataMemberMapping = value;
					ApplyDataMemberMapping();
				}
			}
		}

		[Description("Represents the TypeCode of the values in the field.")]
		[DefaultValue(TypeCode.String)]
		public TypeCode TypeCode
		{
			get
			{
				return _typeCode;
			}
			set
			{
				if (Timeline != null && !Timeline.IsUnbound())
				{
					throw new InvalidOperationException("This property can only be set in unbound mode and before the field is added to the collection");
				}
				_typeCode = value;
			}
		}

		[Browsable(false)]
		public Type Type
		{
			get
			{
				TypeCode typeCode = _typeCode;
				if (Timeline != null && !Timeline.IsUnbound() && base.BasePropertyDescriptor != null)
				{
					return base.BasePropertyDescriptor.PropertyType;
				}
				switch (typeCode)
				{
				case TypeCode.Object:
					if (_type == null)
					{
						return typeof(object);
					}
					return _type;
				case TypeCode.Boolean:
					return typeof(bool);
				case TypeCode.Byte:
					return typeof(byte);
				case TypeCode.Char:
					return typeof(char);
				case TypeCode.DateTime:
					return typeof(DateTime);
				case TypeCode.DBNull:
					return typeof(DBNull);
				case TypeCode.Decimal:
					return typeof(decimal);
				case TypeCode.Double:
					return typeof(double);
				case TypeCode.Empty:
					return null;
				case TypeCode.Int16:
					return typeof(short);
				case TypeCode.Int32:
					return typeof(int);
				case TypeCode.Int64:
					return typeof(long);
				case TypeCode.SByte:
					return typeof(sbyte);
				case TypeCode.Single:
					return typeof(float);
				case TypeCode.String:
					return typeof(string);
				case TypeCode.UInt16:
					return typeof(ushort);
				case TypeCode.UInt32:
					return typeof(uint);
				case TypeCode.UInt64:
					return typeof(ulong);
				default:
					return typeof(string);
				}
			}
			set
			{
				if (TypeCode != TypeCode.Object)
				{
					throw new InvalidOperationException("The TypeCode property must be set to TypeCode.Object in order to set this property");
				}
				if (_type == null || _type != value)
				{
					_type = value;
				}
			}
		}

		[Browsable(false)]
		public MxTimeline Timeline
		{
			get
			{
				if (base.BaseTable != null)
				{
					return ((TimelineMordexTableBase)base.BaseTable).GetTimeline();
				}
				return null;
			}
		}

		[Description("Indicates whether the field should have a ValueList.")]
		[DefaultValue(false)]
		public bool HasValueList
		{
			get
			{
				return base.BaseHasValueList;
			}
			set
			{
				base.BaseHasValueList = value;
			}
		}

		[Description("The TimelineValueListItemCollection associated with the field.")]
		[Browsable(false)]
		[DefaultValue(null)]
		public TimelineValueListItemCollection ValueList => (TimelineValueListItemCollection)base.BaseValueList;

		[TypeConverter(typeof(Mordex.Windows.Timeline.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
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
					UpdateLayout();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.Timeline.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
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
					UpdateLayout();
				}
			}
		}

		[Description("Indicates whether the image from this field will be used as the image of the item when the item has no image defined.")]
		[DefaultValue(false)]
		public bool ItemsImage
		{
			get
			{
				return _itemsImage;
			}
			set
			{
				if (_itemsImage == value)
				{
					return;
				}
				_itemsImage = value;
				if (Timeline != null)
				{
					if (value)
					{
						Timeline.UpdateTimelineField(this);
					}
					else
					{
						Timeline.UpdateTimelineField(null);
					}
				}
			}
		}

		[Description("Represents the key used to identify this object in a colletion.")]
		[DefaultValue("")]
		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				base.BaseKey = value;
				if (string.IsNullOrEmpty(DataMember) && _dataMemberMapping == DataMemberMapping.Custom)
				{
					base.BaseDataMember = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IComparer GroupComparer
		{
			get
			{
				return base.BaseGroupComparer;
			}
			set
			{
				base.BaseGroupComparer = value;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			CompareTarget = (FieldCompareTarget)LayoutReader.GetPropertyValue(info, "CompareTarget", typeof(FieldCompareTarget), FieldCompareTarget.Value);
			DataMemberMapping = (DataMemberMapping)LayoutReader.GetPropertyValue(info, "DataMemberMapping", typeof(DataMemberMapping), DataMemberMapping.Custom);
			if (_dataMemberMapping == DataMemberMapping.Custom)
			{
				DataMember = (string)LayoutReader.GetPropertyValue(info, "DataMember", typeof(string), string.Empty);
			}
			GroupEmptyString = (string)LayoutReader.GetPropertyValue(info, "GroupEmptyString", typeof(string), "(none)");
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			_typeCode = (TypeCode)LayoutReader.GetPropertyValue(info, "TypeCode", typeof(TypeCode), TypeCode.String);
			_ = info["TypeCode"];
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			ItemsImage = (bool)LayoutReader.GetPropertyValue(info, "ItemsImage", typeof(bool), false);
			DefaultGroupFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "DefaultGroupFormatMode", typeof(FormatMode), FormatMode.UseDefault);
			DefaultGroupFormatString = (string)LayoutReader.GetPropertyValue(info, "DefaultGroupFormatString", typeof(string), null);
			DefaultGroupInterval = (GroupInterval)LayoutReader.GetPropertyValue(info, "DefaultGroupFormatString", typeof(GroupInterval), GroupInterval.Value);
			_defaultGroupPrefix = (string)LayoutReader.GetPropertyValue(info, "DefaultGroupPrefix", typeof(string), "");
			_readOnly = (bool)LayoutReader.GetPropertyValue(info, "ReadOnly", typeof(bool), false);
			string text = (string)LayoutReader.GetPropertyValue(info, "TypeName", typeof(string), null);
			if (text != null && TypeCode == TypeCode.Object)
			{
				try
				{
					Type = LayoutTypeResolver.Resolve(text);
				}
				catch
				{
					_type = null;
				}
			}
			else
			{
				_type = null;
			}
			HasValueList = (bool)LayoutReader.GetPropertyValue(info, "HasValueList", typeof(bool), false);
			if (HasValueList)
			{
				info.DeserializeObject("ValueList", ValueList);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (CompareTarget != FieldCompareTarget.Value)
			{
				info.AddValue("CompareTarget", CompareTarget);
			}
			if (DataMember != string.Empty)
			{
				info.AddValue("DataMember", DataMember);
			}
			if (GroupEmptyString != "(none)")
			{
				info.AddValue("GroupEmptyString", GroupEmptyString);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (Key != string.Empty)
			{
				info.AddValue("Key", Key);
			}
			if (_itemsImage)
			{
				info.AddValue("ItemsImage", _itemsImage);
			}
			if (TypeCode != TypeCode.String)
			{
				info.AddValue("TypeCode", TypeCode);
			}
			if (TypeCode == TypeCode.Object && Type != typeof(object))
			{
				info.AddValue("TypeName", Type.FullName);
			}
			if (HasValueList)
			{
				info.AddValue("HasValueList", HasValueList);
			}
			if (ValueList != null && ValueList.Count > 0)
			{
				info.AddValue("ValueList", ValueList);
			}
			if (_dataMemberMapping != DataMemberMapping.Custom)
			{
				info.AddValue("DataMemberMapping", _dataMemberMapping);
			}
			if (DefaultGroupFormatMode != FormatMode.UseDefault)
			{
				info.AddValue("DefaultGroupFormatMode", DefaultGroupFormatMode);
			}
			if (ShouldSerializeDefaultGroupFormatString())
			{
				info.AddValue("DefaultGroupFormatString", DefaultGroupFormatString);
			}
			if (DefaultGroupInterval != GroupInterval.Value)
			{
				info.AddValue("DefaultGroupFormatString", DefaultGroupInterval);
			}
			if (_defaultGroupPrefix != "")
			{
				info.AddValue("DefaultGroupPrefix", _defaultGroupPrefix);
			}
			if (_readOnly)
			{
				info.AddValue("ReadOnly", _readOnly);
			}
		}

		public TimelineField(string dataMember)
			: base(dataMember)
		{
			Key = dataMember;
		}

		public TimelineField()
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDefaultGroupFormatMode()
		{
			return DefaultGroupFormatMode != FormatMode.UseDefault;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDefaultGroupFormatMode()
		{
			DefaultGroupFormatMode = FormatMode.UseDefault;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDefaultGroupFormatString()
		{
			return DefaultGroupFormatString != "";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDefaultGroupFormatString()
		{
			DefaultGroupFormatString = "";
		}

		internal TimelineFieldCollection GetParentCollection()
		{
			return (TimelineFieldCollection)base.BaseParentCollection;
		}

		internal void AssignParentCollection(TimelineFieldCollection parent)
		{
			BaseSetParentCollection(parent);
		}

		private void UpdateLayout()
		{
			if (GetParentCollection() != null)
			{
				GetParentCollection().Timeline.Invalidate();
			}
		}

		internal void UpdateLayoutIfGrouped()
		{
			if (GetParentCollection() == null)
			{
				return;
			}
			foreach (TimelineGroup group in Timeline.Groups)
			{
				if (group.Field == this)
				{
					UpdateLayout();
					break;
				}
			}
		}

		internal void ApplyDataMemberMapping()
		{
			if (Timeline != null)
			{
				switch (_dataMemberMapping)
				{
				case DataMemberMapping.EndTimeMember:
					base.BaseDataMember = Timeline.GetEndTimeField();
					break;
				case DataMemberMapping.StartTimeMember:
					base.BaseDataMember = Timeline.GetStartTimeField();
					break;
				case DataMemberMapping.DescriptionMember:
					base.BaseDataMember = Timeline.GetTextField();
					break;
				case DataMemberMapping.EstimatedStartTimeMember:
					base.BaseDataMember = Timeline.GetEstimatedStartTimeField();
					break;
				case DataMemberMapping.EstimatedEndTimeMember:
					base.BaseDataMember = Timeline.GetEstimatedEndTimeField();
					break;
				case DataMemberMapping.Custom:
					break;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetType()
		{
			if (TypeCode == TypeCode.Object)
			{
				_type = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeType()
		{
			if (TypeCode == TypeCode.Object)
			{
				return _type != null;
			}
			return false;
		}

		protected override MordexValueItemCollectionBase CreateValueList()
		{
			return new TimelineValueListItemCollection(this);
		}

		internal int GetImageWidth(TimelineItem timelineItem)
		{
			try
			{
				if (_itemsImage)
				{
					if (HasValueList)
					{
						object value = timelineItem.GetValue(this);
						TimelineValueListItem timelineValueListItem = ValueList[value];
						if (timelineValueListItem != null)
						{
							if (timelineValueListItem.Image != null)
							{
								return timelineValueListItem.Image.Width;
							}
							ImageList imageList = Timeline.GetImageList();
							if (imageList != null)
							{
								if (timelineValueListItem.ImageKey.Length > 0 && imageList.Images.ContainsKey(timelineValueListItem.ImageKey))
								{
									return imageList.ImageSize.Width;
								}
								if (timelineValueListItem.ImageIndex >= 0 && timelineValueListItem.ImageIndex < imageList.Images.Count)
								{
									return imageList.ImageSize.Width;
								}
							}
						}
					}
					ImageList fieldImageList = Timeline.GetImageList();
					if (fieldImageList != null)
					{
						if (_imageKey.Length > 0 && fieldImageList.Images.ContainsKey(_imageKey))
						{
							return fieldImageList.ImageSize.Width;
						}
						if (_imageIndex >= 0 && _imageIndex < fieldImageList.Images.Count)
						{
							return fieldImageList.ImageSize.Width;
						}
					}
				}
				return 0;
			}
			catch
			{
				return 0;
			}
		}

		internal Image GetItemImage(TimelineItem timelineItem)
		{
			try
			{
				if (_itemsImage)
				{
					if (HasValueList)
					{
						object value = timelineItem.GetValue(this);
						TimelineValueListItem timelineValueListItem = ValueList[value];
						if (timelineValueListItem != null)
						{
							if (timelineValueListItem.Image != null)
							{
								return timelineValueListItem.Image;
							}
							ImageList imageList = Timeline.GetImageList();
							if (imageList != null)
							{
								if (timelineValueListItem.ImageKey.Length > 0 && imageList.Images.ContainsKey(timelineValueListItem.ImageKey))
								{
									return imageList.Images[timelineValueListItem.ImageKey];
								}
								if (timelineValueListItem.ImageIndex >= 0 && timelineValueListItem.ImageIndex < imageList.Images.Count)
								{
									return imageList.Images[timelineValueListItem.ImageIndex];
								}
							}
						}
					}
					ImageList fieldImageList = Timeline.GetImageList();
					if (fieldImageList != null)
					{
						if (_imageKey.Length > 0 && fieldImageList.Images.ContainsKey(_imageKey))
						{
							return fieldImageList.Images[_imageKey];
						}
						if (_imageIndex >= 0 && _imageIndex < fieldImageList.Images.Count)
						{
							return fieldImageList.Images[_imageIndex];
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

		protected override void Initialize(PropertyDescriptor propertyDescriptor)
		{
			base.Initialize(propertyDescriptor);
			if (propertyDescriptor != null)
			{
				TypeCode = base.BaseDataTypeCode;
			}
		}
	}
}
