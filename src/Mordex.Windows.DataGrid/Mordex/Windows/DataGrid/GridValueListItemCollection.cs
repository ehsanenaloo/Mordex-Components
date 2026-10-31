using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridValueListItemCollection : MordexValueItemCollectionBase, IMordexXmlLayoutsSupport, ITypedList
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		public GridValueListItem this[int index] => (GridValueListItem)BaseGet(index);

		public GridValueListItem this[object value] => (GridValueListItem)BaseGet(value);

		public GridValueListItem this[string text] => (GridValueListItem)BaseGet(text);

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
			ReplaceValues = (bool)LayoutReader.GetPropertyValue(info, "ReplaceValues", typeof(bool), true);
			int value = 0;
			for (GridValueListItem gridValueListItem = (GridValueListItem)LayoutReader.GetPropertyValue(info, "Item" + value, typeof(GridValueListItem), null); gridValueListItem != null; gridValueListItem = (GridValueListItem)LayoutReader.GetPropertyValue(info, "Item" + value, typeof(GridValueListItem), null))
			{
				BaseAdd(gridValueListItem);
				value++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!ReplaceValues)
			{
				info.AddValue("ReplaceValues", ReplaceValues);
			}
			int value = 0;
			foreach (GridValueListItem item in (IEnumerable)this)
			{
				info.AddValue("Item" + value, item);
				value++;
			}
		}

		public GridValueListItemCollection()
		{
		}

		internal GridValueListItemCollection(GridColumn column)
			: base(column)
		{
		}

		public void PopulateValueList(IEnumerable list)
		{
			PopulateValueList(list, string.Empty, string.Empty);
		}

		public void PopulateValueList(IEnumerable list, string textMember)
		{
			PopulateValueList(list, string.Empty, textMember);
		}

		public void PopulateValueList(IEnumerable list, string valueMember, string textMember)
		{
			PopulateValueList(list, valueMember, textMember, string.Empty, Color.Empty, Size.Empty);
		}

		public void PopulateValueList(IEnumerable list, string valueMember, string textMember, string imageMember, Color imageMaskColor, Size imageSize)
		{
			BasePopulateValueList(list, valueMember, textMember, imageMember, imageMaskColor, imageSize);
		}

		public void Add(GridValueListItem item)
		{
			BaseAdd(item);
		}

		public void AddRange(GridValueListItem[] items)
		{
			foreach (GridValueListItem gridValueListItem in items)
			{
				if (gridValueListItem.GetOwnerCollection() != null)
				{
					GridValueListItem valueListItem = new GridValueListItem();
					valueListItem.Image = gridValueListItem.Image;
					valueListItem.ImageIndex = gridValueListItem.ImageIndex;
					valueListItem.ImageKey = gridValueListItem.ImageKey;
					valueListItem.Text = gridValueListItem.Text;
					valueListItem.Value = gridValueListItem.Value;
					BaseAdd(valueListItem);
				}
				else
				{
					BaseAdd(gridValueListItem);
				}
			}
		}

		public GridValueListItem Add(object value, string text)
		{
			return (GridValueListItem)BaseAdd(value, text, null);
		}

		public GridValueListItem Add(object value, string text, int imageIndex)
		{
			GridValueListItem gridValueListItem = new GridValueListItem(value, text, imageIndex);
			BaseAdd(gridValueListItem);
			return gridValueListItem;
		}

		public GridValueListItem Add(object value, string text, string imageKey)
		{
			GridValueListItem gridValueListItem = new GridValueListItem(value, text, imageKey);
			BaseAdd(gridValueListItem);
			return gridValueListItem;
		}

		public GridValueListItem Add(object value, string text, Image image)
		{
			GridValueListItem gridValueListItem = new GridValueListItem(value, text);
			gridValueListItem.Image = image;
			BaseAdd(gridValueListItem);
			return gridValueListItem;
		}

		public int IndexOf(GridValueListItem item)
		{
			return BaseIndexOf(item);
		}

		public void Sort()
		{
			BaseSort();
		}

		public void Clear()
		{
			BaseClear();
		}

		public void Remove(GridValueListItem item)
		{
			BaseRemove(item);
		}

		internal GridValueListItem FindItemStartingWith(string strB)
		{
			foreach (GridValueListItem item in (IEnumerable)this)
			{
				string text = item.Text;
				if (text.Length >= strB.Length && string.Compare(text, 0, strB, 0, strB.Length, ignoreCase: true) == 0)
				{
					return item;
				}
			}
			return null;
		}

		internal GridValueListItem FindItemByPrefix(int value, string strB)
		{
			int otherValue = value;
			do
			{
				otherValue++;
				if (otherValue >= base.Count)
				{
					if (value == -1)
					{
						return null;
					}
					otherValue = 0;
				}
				string text = this[otherValue].Text;
				if (text.Length >= strB.Length && string.Compare(text, 0, strB, 0, strB.Length, ignoreCase: true) == 0)
				{
					return this[otherValue];
				}
			}
			while (otherValue != value);
			return null;
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] properties)
		{
			if (properties == null)
			{
				return TypeDescriptor.GetProperties(typeof(GridValueListItem));
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
			GridValueListItem gridValueListItem = new GridValueListItem(value, text);
			gridValueListItem.Image = image;
			return gridValueListItem;
		}

		public bool Contains(object value)
		{
			return BaseContains(value);
		}
	}
}
