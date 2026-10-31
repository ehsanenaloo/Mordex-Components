using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Mordex.Data
{
	public class MordexValueItemCollectionBase : IList, ICollection, IMordexValueItemCollection, IEnumerable, ILookUpTable
	{
		private ArrayList _items;

		private Hashtable _itemsByValue;

		private MordexFieldBase _owner;

		private bool _replaceValues;

		public int Count => _items.Count;

		protected MordexFieldBase BaseOwner => _owner;

		protected bool BaseReplaceValues
		{
			get
			{
				return _replaceValues;
			}
			set
			{
				_replaceValues = value;
			}
		}

		bool ICollection.IsSynchronized => _items.IsSynchronized;

		object ICollection.SyncRoot => _items.SyncRoot;

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return _items[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		IMordexValueItem IMordexValueItemCollection.this[int index] => BaseGet(index);

		IMordexValueItem IMordexValueItemCollection.this[object value] => BaseGet(value);

		int IMordexValueItemCollection.Count => Count;

		bool IMordexValueItemCollection.ReplaceValues
		{
			get
			{
				return BaseReplaceValues;
			}
			set
			{
				BaseReplaceValues = value;
			}
		}

		bool ILookUpTable.ReplaceValues => BaseReplaceValues;

		internal static MordexValueItemCollectionBase CreateForOwner(MordexFieldBase owner)
		{
			return new MordexValueItemCollectionBase(owner);
		}

		public MordexValueItemCollectionBase()
			: this(null)
		{
		}

		protected MordexValueItemCollectionBase(MordexFieldBase owner)
		{
			_replaceValues = true;
			_owner = owner;
			_items = new ArrayList();
			_itemsByValue = new Hashtable();
		}

		public void RemoveAt(int index)
		{
			BaseRemove(BaseGet(index));
		}

		protected MordexValueItemBase BaseAdd(object value, string text)
		{
			MordexValueItemBase mordexValueItemBase = CreateValueItem(value, text, null);
			BaseAdd(mordexValueItemBase);
			return mordexValueItemBase;
		}

		protected MordexValueItemBase BaseAdd(object value, string text, int imageIndex)
		{
			MordexValueItemBase mordexValueItemBase = CreateValueItem(value, text, null);
			((IMordexValueItem)mordexValueItemBase).ImageIndex = imageIndex;
			BaseAdd(mordexValueItemBase);
			return mordexValueItemBase;
		}

		protected MordexValueItemBase BaseAdd(object value, string text, Image image)
		{
			MordexValueItemBase mordexValueItemBase = CreateValueItem(value, text, image);
			BaseAdd(mordexValueItemBase);
			return mordexValueItemBase;
		}

		protected virtual MordexValueItemBase CreateValueItem(object value, string text, Image image)
		{
			return new MordexValueItemBase(value, text, image);
		}

		protected int BaseAdd(MordexValueItemBase item)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			if (item.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException("Item already belongs to a ValueList");
			}
			object value = ((IMordexValueItem)item).Value;
			int result = _items.Add(item);
			item.SetOwnerCollection(this);
			RegisterKey(value, item);
			return result;
		}

		protected void BaseInsert(int index, MordexValueItemBase item)
		{
			if (item == null)
			{
				throw new ArgumentNullException();
			}
			if (item.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException("Item already belongs to a ValueList");
			}
			object value = ((IMordexValueItem)item).Value;
			_items.Insert(index, item);
			item.SetOwnerCollection(this);
			RegisterKey(value, item);
		}

		internal void UnregisterKey(object key, MordexValueItemBase itemToRemove)
		{
			if (key != null)
			{
				MordexValueItemBase mordexValueItemBase = BaseGet(key);
				if (mordexValueItemBase == itemToRemove)
				{
					_itemsByValue.Remove(key);
				}
			}
		}

		internal void RegisterKey(object key, MordexValueItemBase mordexValueItemBase)
		{
			if (key != null && !_itemsByValue.ContainsKey(key))
			{
				_itemsByValue.Add(key, mordexValueItemBase);
			}
		}

		protected bool BaseContains(object value)
		{
			if (value != null)
			{
				return _itemsByValue.ContainsKey(value);
			}
			return BaseGet(value) != null;
		}

		protected MordexValueItemBase BaseGet(int index)
		{
			return (MordexValueItemBase)_items[index];
		}

		protected MordexValueItemBase BaseGet(object value)
		{
			if (value != null && _itemsByValue.ContainsKey(value))
			{
				return (MordexValueItemBase)_itemsByValue[value];
			}
			CultureInfo cultureInfo = GetCulture();
			foreach (IMordexValueItem item in _items)
			{
				if (DataValueHelper.CompareInDomain(cultureInfo, item.Value, value, null, true, _owner?.GetValueDomain()) == 0)
				{
					return (MordexValueItemBase)item;
				}
			}
			return null;
		}

		private CultureInfo GetCulture()
		{
			DataSourceManager dataSourceManager = null;
			if (_owner != null)
			{
				IMordexTable table = ((IMordexField)_owner).Table;
				if (table != null)
				{
					dataSourceManager = table.DataSourceManager;
				}
			}
			return DataValueHelper.GetCulture(dataSourceManager);
		}

		protected MordexValueItemBase BaseGet(string text)
		{
			CultureInfo cultureInfo = GetCulture();
			foreach (IMordexValueItem item in _items)
			{
				if (DataValueHelper.CompareValues(cultureInfo, item.Text, text, null, true, false) == 0)
				{
					return (MordexValueItemBase)item;
				}
			}
			return null;
		}

		protected int BaseIndexOf(MordexValueItemBase item)
		{
			if (item == null)
			{
				return -1;
			}
			return _items.IndexOf(item);
		}

		protected int BaseIndexOf(object value)
		{
			CultureInfo cultureInfo = GetCulture();
			for (int i = 0; i < _items.Count; i++)
			{
				IMordexValueItem mordexValueItem = (IMordexValueItem)_items[i];
				if (DataValueHelper.CompareInDomain(cultureInfo, mordexValueItem.Value, value, null, true, _owner?.GetValueDomain()) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		protected void BaseRemove(MordexValueItemBase item)
		{
			if (item.GetOwnerCollection() == this)
			{
				_items.Remove(item);
				UnregisterKey(((IMordexValueItem)item).Value, item);
				item.SetOwnerCollection(null);
			}
		}

		protected void BaseRemoveAt(int index)
		{
			BaseRemove(BaseGet(index));
		}

		protected void BaseClear()
		{
			foreach (MordexValueItemBase item in _items)
			{
				item.SetOwnerCollection(null);
			}
			_items.Clear();
			_itemsByValue.Clear();
		}

		protected void BasePopulateValueList(IEnumerable list, string valueMember, string textMember, string imageMember, Color imageMaskColor, Size imageSize)
		{
			BaseClear();
			PropertyDescriptor valueDescriptor = null;
			PropertyDescriptor textDescriptor = null;
			PropertyDescriptor imageDescriptor = null;
			bool descriptorsResolved = false;
			foreach (object item in list)
			{
				if (!descriptorsResolved)
				{
					PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(item);
					if (valueMember.Length > 0)
					{
						valueDescriptor = properties.Find(valueMember, ignoreCase: true);
					}
					if (textMember.Length > 0)
					{
						textDescriptor = properties.Find(textMember, ignoreCase: true);
					}
					if (imageMember.Length > 0)
					{
						imageDescriptor = properties.Find(imageMember, ignoreCase: true);
					}
					descriptorsResolved = true;
				}
				object value = valueDescriptor != null ? valueDescriptor.GetValue(item) : item;
				if (value == null)
				{
					continue;
				}
				string text = textDescriptor == null ? item.ToString() : string.Concat(textDescriptor.GetValue(item));
				BaseAdd(value, text, ReadItemImage(imageDescriptor, item, imageMaskColor, imageSize));
			}
		}

		private static Image ReadItemImage(PropertyDescriptor imageDescriptor, object item, Color imageMaskColor, Size imageSize)
		{
			if (imageDescriptor == null)
			{
				return null;
			}
			object value = imageDescriptor.GetValue(item);
			if (DataValueHelper.IsNullOrDBNull(value))
			{
				return null;
			}
			Image source;
			bool ownsSource = false;
			if (imageDescriptor.PropertyType == typeof(byte[]))
			{
				try
				{
					using MemoryStream stream = new MemoryStream((byte[])value);
					using Image decoded = Image.FromStream(stream);
					// The decoded image needs its stream while it is alive, so keep a copy.
					source = new Bitmap(decoded);
					ownsSource = true;
				}
				catch (ArgumentException)
				{
					return null;
				}
			}
			else
			{
				source = value as Image;
			}
			if (source == null || (imageSize.IsEmpty && imageMaskColor.IsEmpty))
			{
				return source;
			}
			Bitmap bitmap = imageSize.IsEmpty ? new Bitmap(source) : new Bitmap(source, imageSize);
			if (ownsSource)
			{
				source.Dispose();
			}
			if (!imageMaskColor.IsEmpty)
			{
				bitmap.MakeTransparent(imageMaskColor);
			}
			return bitmap;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		void ICollection.CopyTo(Array destination, int index)
		{
			_items.CopyTo(destination, index);
		}

		int IList.Add(object item)
		{
			return BaseAdd((MordexValueItemBase)item);
		}

		void IList.Clear()
		{
			BaseClear();
		}

		bool IList.Contains(object item)
		{
			return _items.Contains(item);
		}

		int IList.IndexOf(object item)
		{
			return _items.IndexOf(item);
		}

		void IList.Insert(int index, object item)
		{
			throw new NotImplementedException();
		}

		void IList.Remove(object item)
		{
			BaseRemove((MordexValueItemBase)item);
		}

		protected void BaseSort()
		{
			_items.Sort(new ValueItemComparer(GetCulture(), useValue: false));
		}

		internal void SortByValue()
		{
			_items.Sort(new ValueItemComparer(GetCulture(), useValue: true));
		}

		void IMordexValueItemCollection.Sort()
		{
			BaseSort();
		}

		void IMordexValueItemCollection.Clear()
		{
			BaseClear();
		}

		IMordexValueItem IMordexValueItemCollection.Add(object value, string text)
		{
			return BaseAdd(value, text);
		}

		IMordexValueItem IMordexValueItemCollection.Add(object value, string text, int imageIndex)
		{
			return BaseAdd(value, text, imageIndex);
		}

		IMordexValueItem IMordexValueItemCollection.Add(object value, string text, Image image)
		{
			return BaseAdd(value, text, image);
		}

		void IMordexValueItemCollection.Remove(IMordexValueItem item)
		{
			BaseRemove((MordexValueItemBase)item);
		}

		void IMordexValueItemCollection.RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		bool IMordexValueItemCollection.Contains(object value)
		{
			return BaseContains(value);
		}

		IMordexValueItem ILookUpTable.GetItemFromValue(object value)
		{
			return BaseGet(value);
		}

		IMordexValueItem ILookUpTable.GetItemFromText(string text)
		{
			foreach (IMordexValueItem item in (IEnumerable)this)
			{
				if (string.Compare(text, item.Text, ignoreCase: true) == 0)
				{
					return item;
				}
			}
			return null;
		}

		int IMordexValueItemCollection.Add(IMordexValueItem item)
		{
			return BaseAdd((MordexValueItemBase)item);
		}

		void IMordexValueItemCollection.Insert(int index, IMordexValueItem item)
		{
			BaseInsert(index, (MordexValueItemBase)item);
		}
	}
}
