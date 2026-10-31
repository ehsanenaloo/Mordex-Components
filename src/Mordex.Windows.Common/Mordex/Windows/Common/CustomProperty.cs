using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Common
{
	public class CustomProperty : IMordexXmlLayoutsSupport
	{
		private string _name;

		private object _value;

		private CustomPropertyCollection _owner;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					if (_owner != null)
					{
						throw new InvalidOperationException("Name can not be changed ince the property belongs to a collection.");
					}
					_name = value;
				}
			}
		}

		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		public CustomProperty()
			: this(null, "", null)
		{
		}

		public CustomProperty(string name, object propertyValue)
			: this(null, name, propertyValue)
		{
		}

		internal CustomProperty(CustomPropertyCollection owner, string name, object propertyValue)
		{
			_name = name;
			_value = propertyValue;
			_owner = owner;
		}

		internal CustomPropertyCollection GetOwner()
		{
			return _owner;
		}

		internal void SetOwner(CustomPropertyCollection owner)
		{
			_owner = owner;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_name = (string)LayoutReader.GetPropertyValue(info, "Name", typeof(string), "");
			string text = (string)LayoutReader.GetPropertyValue(info, "ValueType", typeof(string), "");
			if (text == "")
			{
				_value = LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
				return;
			}
			_value = LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
			Type valueType = LayoutTypeResolver.Resolve(text);
			if (valueType == null)
			{
				return;
			}
			try
			{
				_value = PropertyValue.ConvertValue((string)_value, valueType);
			}
			catch (Exception ex) when (PropertyValue.IsConversionFailure(ex))
			{
				// The stored text is kept when it cannot be converted to the stored type.
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			if (_name != null && _name.Length > 0)
			{
				writer.AddValue("Name", _name);
			}
			if (_value != null)
			{
				if (!(_value is string))
				{
					writer.AddValue("ValueType", _value.GetType().FullName);
				}
				writer.AddValue("Value", _value);
			}
		}
	}
}
