using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Mordex.Windows.Common.Layouts
{
	public class LayoutReader
	{
		private readonly List<PropertyValue> _values = new List<PropertyValue>();

		private IMordexLayout _layout;

		public int Count => _values.Count;

		public PropertyValue this[int index] => _values[index];

		public PropertyValue this[string propertyName]
		{
			get
			{
				foreach (PropertyValue item in _values)
				{
					if (string.Equals(item.Name, propertyName, StringComparison.OrdinalIgnoreCase))
					{
						return item;
					}
				}
				return null;
			}
		}

		public static LayoutReader FromLayout(IMordexLayout layout)
		{
			LayoutReader reader = new LayoutReader();
			reader._layout = layout;
			reader.ReadChildren(LayoutXml.LoadString(layout.LayoutString), owned: true);
			return reader;
		}

		public static LayoutReader FromStream(Stream stream)
		{
			LayoutReader reader = new LayoutReader();
			reader.ReadChildren(LayoutXml.LoadStream(stream), owned: true);
			return reader;
		}

		public static LayoutReader FromString(string xmlString)
		{
			LayoutReader reader = new LayoutReader();
			reader.ReadChildren(LayoutXml.LoadString(xmlString), owned: true);
			return reader;
		}

		internal static LayoutReader FromNode(XmlNode node, IMordexLayout layout)
		{
			LayoutReader reader = new LayoutReader();
			reader._layout = layout;
			reader.ReadChildren(node, owned: true);
			return reader;
		}

		public static LayoutReader FromParentNode(XmlNode parentNode)
		{
			LayoutReader reader = new LayoutReader();
			reader.ReadChildren(parentNode, owned: false);
			return reader;
		}

		public LayoutReader()
		{
		}

		public static object GetPropertyValue(LayoutReader reader, string propertyName, Type destinationType, object defaultValue)
		{
			PropertyValue propertyValue = reader[propertyName];
			if (propertyValue == null)
			{
				return defaultValue;
			}
			try
			{
				object value = propertyValue.GetValue(destinationType);
				if (value == null && destinationType.IsValueType)
				{
					return defaultValue;
				}
				return value;
			}
			catch (Exception ex) when (PropertyValue.IsConversionFailure(ex))
			{
				// A value that cannot be converted falls back to the default so the rest of the layout still loads.
				return defaultValue;
			}
		}

		// owned is true when the nodes belong to this reader (a document it loaded, or a node a PropertyValue
		// already copied). A caller's node is copied once so later changes to it do not reach the values.
		private void ReadChildren(XmlNode parent, bool owned)
		{
			foreach (XmlNode child in parent.ChildNodes)
			{
				if (child.NodeType == XmlNodeType.Element)
				{
					_values.Add(owned ? PropertyValue.FromOwnedNode(child, _layout) : PropertyValue.FromXmlNode(child, _layout));
				}
			}
		}

		public bool DeserializeObject(string propertyName, object targetInstance)
		{
			PropertyValue propertyValue = this[propertyName];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(targetInstance);
				return true;
			}
			return false;
		}

		public object GetValue(string name, Type destinationType)
		{
			return this[name].GetValue(destinationType);
		}
	}
}
