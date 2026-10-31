using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI;

namespace Mordex.Windows.UI.Controls
{
	public class ComboBoxItemCollection : CollectionBase, IMordexXmlLayoutsSupport, ITypedList
	{
		internal MxComboBase _owner;

		internal bool _isSorted;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		public ComboBoxItem this[int index] => (ComboBoxItem)base.List[index];

		public ComboBoxItem this[object value]
		{
			get
			{
				foreach (ComboBoxItem inner in base.InnerList)
				{
					if (RenderingHelper.CompareValuesCurrentCulture(value, inner.Value, null, true) == 0)
					{
						return inner;
					}
				}
				return null;
			}
		}

		public ComboBoxItem this[string text]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (ComboBoxItem inner in base.InnerList)
				{
					if (string.Compare(inner.Text, text, ignoreCase: true, invariantCulture) == 0)
					{
						return inner;
					}
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int index = 0;
			for (ComboBoxItem uiComboBoxItem = (ComboBoxItem)LayoutReader.GetPropertyValue(info, "Item" + index, typeof(ComboBoxItem), null); uiComboBoxItem != null; uiComboBoxItem = (ComboBoxItem)LayoutReader.GetPropertyValue(info, "Item" + index, typeof(ComboBoxItem), null))
			{
				base.InnerList.Add(uiComboBoxItem);
				uiComboBoxItem.SetOwnerCollection(this);
				index++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ComboBoxItem value = (ComboBoxItem)enumerator.Current;
					info.AddValue("Item" + index, value);
					index++;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		internal ComboBoxItemCollection(MxComboBase comboBox)
		{
			_owner = comboBox;
		}

		public void Add(ComboBoxItem item)
		{
			if (!_owner.IsDataSourceEmpty())
			{
				throw new InvalidOperationException("Can't modify the Items collection when the DataSource property is set");
			}
			if (item == null)
			{
				throw new ArgumentNullException("item", "Can not add a null item");
			}
			if (item.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException("The item you are trying to add, already belongs to a UIComboBoxItemCollection");
			}
			AddItemUnchecked(item);
		}

		public void Insert(int index, ComboBoxItem item)
		{
			if (!_owner.IsDataSourceEmpty())
			{
				throw new InvalidOperationException("Can't modify the Items collection when the DataSource property is set");
			}
			if (item == null)
			{
				throw new ArgumentNullException("item", "Can not add a null item");
			}
			if (item.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException("The item you are trying to add, already belongs to a UIComboBoxItemCollection");
			}
			if (index < 0 || index > base.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			InsertItemUnchecked(index, item);
		}

		public void AddRange(ComboBoxItem[] items)
		{
			foreach (ComboBoxItem item in items)
			{
				Add(item);
			}
		}

		public ComboBoxItem Add(object value)
		{
			ComboBoxItem uiComboBoxItem = new ComboBoxItem(value);
			Add(uiComboBoxItem);
			return uiComboBoxItem;
		}

		private int FindSortedIndex(ComboBoxItem item, int low, int high, ComboBoxItemTextComparer windowsComparer, int middle)
		{
			if (low > high)
			{
				return low;
			}
			middle = (high + low) / 2;
			ComboBoxItem x = (ComboBoxItem)base.List[middle];
			int comparison = windowsComparer.Compare(x, item);
			if (comparison == 0)
			{
				return middle;
			}
			if (comparison > 0)
			{
				return FindSortedIndex(item, low, middle - 1, windowsComparer, middle);
			}
			return FindSortedIndex(item, middle + 1, high, windowsComparer, middle);
		}

		public ComboBoxItem Add(string text, object value)
		{
			ComboBoxItem uiComboBoxItem = new ComboBoxItem(text, value);
			Add(uiComboBoxItem);
			return uiComboBoxItem;
		}

		internal void AddItemUnchecked(ComboBoxItem item)
		{
			if (_isSorted)
			{
				ComboBoxItemTextComparer windowsComparer = new ComboBoxItemTextComparer();
				item._originalIndex = base.List.Count;
				int index = FindSortedIndex(item, 0, base.List.Count - 1, windowsComparer, 0);
				base.List.Insert(index, item);
				ReindexItems();
			}
			else
			{
				item._listIndex = base.List.Count;
				base.List.Add(item);
			}
			item.SetOwnerCollection(this);
		}

		internal void InsertItemUnchecked(int position, ComboBoxItem item)
		{
			if (position >= base.List.Count)
			{
				AddItemUnchecked(item);
				return;
			}
			if (_isSorted)
			{
				ComboBoxItemTextComparer windowsComparer = new ComboBoxItemTextComparer();
				item._originalIndex = position;
				int index = FindSortedIndex(item, 0, base.List.Count - 1, windowsComparer, 0);
				base.List.Insert(index, item);
			}
			else
			{
				item._listIndex = position;
				base.List.Insert(position, item);
			}
			item.SetOwnerCollection(this);
			ReindexItems();
		}

		internal ComboBoxItem GetItemAt(int index, bool byOriginalIndex)
		{
			if (index >= 0 && index < base.Count)
			{
				if (byOriginalIndex)
				{
					foreach (ComboBoxItem item in base.List)
					{
						if (item._originalIndex == index)
						{
							return item;
						}
					}
				}
				return (ComboBoxItem)base.List[index];
			}
			return null;
		}

		public bool Contains(object value)
		{
			foreach (ComboBoxItem inner in base.InnerList)
			{
				if (RenderingHelper.CompareValuesCurrentCulture(value, inner.Value, null, true) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool Contains(string text)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (ComboBoxItem inner in base.InnerList)
			{
				if (string.Compare(inner.Text, text, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(ComboBoxItem item)
		{
			if (item == null)
			{
				return -1;
			}
			return base.InnerList.IndexOf(item);
		}

		internal void RestoreOriginalOrder()
		{
			if (_isSorted)
			{
				_isSorted = false;
				ComboBoxItemIndexComparer comparer = new ComboBoxItemIndexComparer();
				base.InnerList.Sort(comparer);
				ReindexItems();
			}
		}

		public void Sort()
		{
			if (!_isSorted)
			{
				for (int i = 0; i < base.List.Count; i++)
				{
					((ComboBoxItem)base.List[i])._originalIndex = i;
				}
			}
			_isSorted = true;
			ComboBoxItemTextComparer comparer = new ComboBoxItemTextComparer();
			base.InnerList.Sort(comparer);
			ReindexItems();
		}

		public void Remove(ComboBoxItem item)
		{
			if (!_owner.IsDataSourceEmpty())
			{
				throw new InvalidOperationException("Can't modify the Items collection when the DataSource property is set");
			}
			RemoveItemUnchecked(item);
		}

		internal void RemoveItemUnchecked(ComboBoxItem item)
		{
			item._listIndex = -1;
			base.InnerList.Remove(item);
			item.SetOwnerCollection(null);
			ReindexItems();
		}

		private void ReindexItems()
		{
			for (int i = 0; i < base.List.Count; i++)
			{
				((ComboBoxItem)base.List[i])._listIndex = i;
			}
		}

		internal void MoveItem(ComboBoxItem item, int selectedIndex)
		{
			RemoveItemUnchecked(item);
			item._listIndex = selectedIndex;
			base.InnerList.Insert(selectedIndex, item);
			item.SetOwnerCollection(this);
			ReindexItems();
		}

		internal void DetachAllItems()
		{
			foreach (ComboBoxItem item in base.List)
			{
				item._listIndex = -1;
				item.DataRow = null;
				item.SetOwnerCollection(null);
			}
			base.InnerList.Clear();
		}

		protected override void OnClear()
		{
			if (!_owner.IsDataSourceEmpty())
			{
				throw new InvalidOperationException("Can't modify the Items collection when the DataSource property is set");
			}
			foreach (ComboBoxItem inner in base.InnerList)
			{
				inner._listIndex = -1;
				inner.SetOwnerCollection(null);
			}
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			if (_owner is MxComboBox uiComboBox && uiComboBox.SelectedItem != null)
			{
				uiComboBox.SelectedItem = null;
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (_owner != null && _owner is MxComboBox && _owner is MxComboBox uiComboBox && uiComboBox.SelectedItem == value)
			{
				uiComboBox.SelectedItem = null;
			}
		}

		internal ComboBoxItem FindItemByTextPrefix(string prefix)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (ComboBoxItem inner in base.InnerList)
			{
				string text = inner.Text;
				if (text != null && text.Length >= prefix.Length && string.Compare(text, 0, prefix, 0, prefix.Length, ignoreCase: true, invariantCulture) == 0)
				{
					return inner;
				}
			}
			return null;
		}

		internal ComboBoxItem FindNextItemByTextPrefix(int startIndex, string prefix)
		{
			int index = startIndex;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			do
			{
				index++;
				if (index >= base.Count)
				{
					if (startIndex == -1)
					{
						return null;
					}
					index = 0;
				}
				string text = this[index].Text;
				if (text != null && text.Length >= prefix.Length && string.Compare(text, 0, prefix, 0, prefix.Length, ignoreCase: true, invariantCulture) == 0)
				{
					return this[index];
				}
			}
			while (index != startIndex);
			return null;
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] properties)
		{
			if (properties == null)
			{
				return TypeDescriptor.GetProperties(typeof(ComboBoxItem));
			}
			return null;
		}

		string ITypedList.GetListName(PropertyDescriptor[] properties)
		{
			if (properties == null)
			{
				return "UIComboBoxItemCollection";
			}
			return "";
		}
	}
}
