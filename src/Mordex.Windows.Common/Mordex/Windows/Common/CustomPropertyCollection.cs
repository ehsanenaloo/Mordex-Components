using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Common
{
	public class CustomPropertyCollection : IList, ICollection, IEnumerable, IMordexXmlLayoutsSupport
	{
		private ArrayList _properties;

		public int Count => _properties.Count;

		public CustomProperty this[string name]
		{
			get
			{
				int index = IndexOf(name);
				if (index != -1)
				{
					return (CustomProperty)_properties[index];
				}
				return null;
			}
		}

		public CustomProperty this[int index] => (CustomProperty)_properties[index];

		bool ICollection.IsSynchronized => _properties.IsSynchronized;

		object ICollection.SyncRoot => _properties.SyncRoot;

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return _properties[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		public CustomPropertyCollection()
		{
			_properties = new ArrayList();
		}

		public CustomProperty Add(string name, object value)
		{
			if (name == null || name.Length == 0)
			{
				throw new ArgumentException("Name can not be null nor an empty string.");
			}
			if (Contains(name))
			{
				throw new InvalidOperationException("There is already another property with that name.");
			}
			CustomProperty mordexCustomProperty = new CustomProperty(this, name, value);
			_properties.Add(mordexCustomProperty);
			return mordexCustomProperty;
		}

		public int Add(CustomProperty property)
		{
			if (property.GetOwner() != null)
			{
				if (property.GetOwner() == this)
				{
					return IndexOf(property);
				}
				throw new InvalidOperationException("Property belongs to another collection.");
			}
			if (property.Name == null || property.Name.Length == 0)
			{
				throw new ArgumentException("Name can not be null nor an empty string.");
			}
			if (Contains(property.Name))
			{
				throw new InvalidOperationException("There is already another property with that name.");
			}
			property.SetOwner(this);
			return _properties.Add(property);
		}

		public void Remove(string propertyName)
		{
			int index = IndexOf(propertyName);
			if (index != -1)
			{
				RemoveAt(index);
			}
		}

		public void Remove(CustomProperty property)
		{
			int index = IndexOf(property);
			if (index != -1)
			{
				RemoveAt(index);
			}
		}

		public void RemoveAt(int index)
		{
			CustomProperty mordexCustomProperty = this[index];
			mordexCustomProperty.SetOwner(null);
			_properties.RemoveAt(index);
		}

		public bool Contains(string propertyName)
		{
			return IndexOf(propertyName) != -1;
		}

		public bool Contains(CustomProperty property)
		{
			return _properties.Contains(property);
		}

		public int IndexOf(CustomProperty property)
		{
			return _properties.IndexOf(property);
		}

		public int IndexOf(string propertyName)
		{
			for (int i = 0; i < _properties.Count; i++)
			{
				CustomProperty mordexCustomProperty = (CustomProperty)_properties[i];
				if (string.Equals(mordexCustomProperty.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}

		public void Clear()
		{
			foreach (CustomProperty item in _properties)
			{
				item.SetOwner(null);
			}
			_properties.Clear();
		}

		void ICollection.CopyTo(Array destination, int index)
		{
			_properties.CopyTo(destination, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _properties.GetEnumerator();
		}

		int IList.Add(object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			return Add((CustomProperty)value);
		}

		bool IList.Contains(object value)
		{
			return _properties.Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return _properties.IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object value)
		{
			Remove((CustomProperty)value);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			for (int index = 0; index < info.Count; index++)
			{
				PropertyValue propertyValue = info[index];
				if (!string.Equals(propertyValue.Name, "Property" + index, StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				CustomProperty property = new CustomProperty();
				propertyValue.DeserializeObject(property);
				_properties.Add(property);
				property.SetOwner(this);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int propertyIndex = 0;
			foreach (CustomProperty item in (IEnumerable)this)
			{
				info.AddValue("Property" + propertyIndex, item);
				propertyIndex++;
			}
		}
	}
}
