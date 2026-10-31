using System.Collections;
using System.ComponentModel;
using System.Drawing;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class TimelineValueListItemCollection : MordexValueItemCollectionBase, ITypedList, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		public TimelineValueListItem this[int index] => (TimelineValueListItem)BaseGet(index);

		public TimelineValueListItem this[object value] => (TimelineValueListItem)BaseGet(value);

		public TimelineValueListItem this[string text] => (TimelineValueListItem)BaseGet(text);

		public bool ReplaceValues
		{
			get
			{
				return base.BaseReplaceValues;
			}
			set
			{
				base.BaseReplaceValues = value;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			BaseClear();
			ReplaceValues = (bool)LayoutReader.GetPropertyValue(info, "ReplaceValues", typeof(bool), true);
			int itemIndex = 0;
			for (TimelineValueListItem timelineValueListItem = (TimelineValueListItem)LayoutReader.GetPropertyValue(info, "Item" + itemIndex, typeof(TimelineValueListItem), null); timelineValueListItem != null; timelineValueListItem = (TimelineValueListItem)LayoutReader.GetPropertyValue(info, "Item" + itemIndex, typeof(TimelineValueListItem), null))
			{
				Add(timelineValueListItem);
				itemIndex++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!ReplaceValues)
			{
				info.AddValue("ReplaceValues", ReplaceValues);
			}
			int itemIndex = 0;
			foreach (TimelineValueListItem item in (IEnumerable)this)
			{
				info.AddValue("Item" + itemIndex, item);
				itemIndex++;
			}
		}

		public TimelineValueListItemCollection()
		{
		}

		internal TimelineValueListItemCollection(TimelineField field)
			: base(field)
		{
		}

		internal MxTimeline GetTimeline()
		{
			return ((TimelineField)base.BaseOwner)?.Timeline;
		}

		public void PopulateValueList(IEnumerable list)
		{
			PopulateValueList(list, string.Empty, string.Empty, "", Color.Empty, Size.Empty);
		}

		public void PopulateValueList(IEnumerable list, string textMember)
		{
			PopulateValueList(list, string.Empty, textMember, "", Color.Empty, Size.Empty);
		}

		public void PopulateValueList(IEnumerable list, string valueMember, string textMember)
		{
			PopulateValueList(list, valueMember, textMember, "", Color.Empty, Size.Empty);
		}

		public void PopulateValueList(IEnumerable list, string valueMember, string textMember, string imageMember, Color imageMaskColor, Size imageSize)
		{
			BasePopulateValueList(list, valueMember, textMember, imageMember, imageMaskColor, imageSize);
		}

		public void Add(TimelineValueListItem item)
		{
			BaseAdd(item);
		}

		public void AddRange(TimelineValueListItem[] items)
		{
			foreach (TimelineValueListItem item in items)
			{
				Add(item);
			}
		}

		public int IndexOf(TimelineValueListItem item)
		{
			return BaseIndexOf(item);
		}

		public void Sort()
		{
			BaseSort();
		}

		public void Remove(TimelineValueListItem item)
		{
			BaseRemove(item);
		}

		public void Clear()
		{
			BaseClear();
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] properties)
		{
			if (properties == null)
			{
				return TypeDescriptor.GetProperties(typeof(TimelineValueListItem));
			}
			return null;
		}

		string ITypedList.GetListName(PropertyDescriptor[] properties)
		{
			if (properties == null)
			{
				return "ValueList";
			}
			return "";
		}

		protected override MordexValueItemBase CreateValueItem(object value, string text, Image image)
		{
			TimelineValueListItem timelineValueListItem = new TimelineValueListItem(value, text, -1);
			timelineValueListItem.Image = image;
			return timelineValueListItem;
		}
	}
}
