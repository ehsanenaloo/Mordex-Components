using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public class TimelineStoredFilter : MordexStoredFilterBase, IFilterCondition, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue("")]
		[Description("Represents the key used to identify an object in a colletion.")]
		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				base.BaseKey = value;
			}
		}

		[Description("The description of the stored filter.")]
		[DefaultValue("")]
		public string Description
		{
			get
			{
				return base.BaseDescription;
			}
			set
			{
				base.BaseDescription = value;
			}
		}

		[DefaultValue(null)]
		[TypeConverter("System.ComponentModel.StringConverter")]
		[Description("Represents an object that contains data to associate with the field.")]
		public object Tag
		{
			get
			{
				return base.BaseTag;
			}
			set
			{
				base.BaseTag = value;
			}
		}

		[DefaultValue(null)]
		[Description("The TimelineFilterCondition instance associated to this instance.")]
		[TypeConverter(typeof(FilterConditionConverter))]
		[Editor(typeof(FilterConditionEditor), typeof(UITypeEditor))]
		public TimelineFilterCondition FilterCondition
		{
			get
			{
				return (TimelineFilterCondition)base.BaseFilterCondition;
			}
			set
			{
				base.BaseFilterCondition = value;
			}
		}

		[Browsable(false)]
		public MxTimeline Timeline
		{
			get
			{
				if (base.BaseTable is TimelineMordexTableBase timelineMordexTableBase)
				{
					return timelineMordexTableBase.GetTimeline();
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Description = (string)LayoutReader.GetPropertyValue(info, "Description", typeof(string), "");
			SetBaseIsCurrent((bool)LayoutReader.GetPropertyValue(info, "IsCurrent", typeof(bool), false));
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (tagTypeName != null)
				{
					try
					{
						Type type = LayoutTypeResolver.Resolve(tagTypeName);
						Tag = PropertyValue.ConvertValue(text, type);
					}
					catch
					{
						Tag = text;
					}
				}
				else
				{
					Tag = text;
				}
			}
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				FilterCondition = new TimelineFilterCondition();
				FilterCondition.SetConditionOwner(this);
				propertyValue.DeserializeObject(FilterCondition);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Description != "")
			{
				info.AddValue("Description", Description);
			}
			if (FilterCondition != null)
			{
				info.AddValue("FilterCondition", FilterCondition);
			}
			if (GetBaseIsCurrent())
			{
				info.AddValue("IsCurrent", GetBaseIsCurrent());
			}
			if (Key != "")
			{
				info.AddValue("Key", Key);
			}
			if (Tag != null)
			{
				if (!(Tag is string))
				{
					info.AddValue("TagTypeName", Tag.GetType().FullName);
				}
				info.AddValue("Tag", Tag);
			}
		}

		internal TimelineStoredFilterCollection GetParentCollection()
		{
			return (TimelineStoredFilterCollection)base.BaseParentCollection;
		}

		internal void SetParentCollection(TimelineStoredFilterCollection parent)
		{
			BaseSetParentCollection(parent);
		}

		internal bool GetBaseIsCurrent()
		{
			return base.BaseIsCurrent;
		}

		internal void SetBaseIsCurrent(bool isCurrent)
		{
			base.BaseIsCurrent = isCurrent;
		}
	}
}
