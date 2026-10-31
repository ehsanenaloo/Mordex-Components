using System;
using System.ComponentModel;

namespace Mordex.Windows.DataGrid.Design
{
	internal class GridColumnState : ICustomTypeDescriptor
	{
		private GridColumn _propertyOwner;

		internal GridColumnState(GridColumn column)
		{
			_propertyOwner = column;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(_propertyOwner);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return "GridColumn";
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return "Column[" + _propertyOwner.Index + "] - '" + _propertyOwner.Key + "'";
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(_propertyOwner);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(_propertyOwner);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetProperties(_propertyOwner).Find("Caption", ignoreCase: true);
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(_propertyOwner, editorBaseType);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(_propertyOwner, attributes);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(_propertyOwner);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return GetHeaderProperties();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return GetHeaderProperties();
		}

		private PropertyDescriptorCollection GetHeaderProperties()
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(_propertyOwner);
			return new PropertyDescriptorCollection(new PropertyDescriptor[10]
			{
				properties["Caption"],
				properties["ColSpan"],
				properties["ColumnSetColumn"],
				properties["ColumnSetRow"],
				properties["HeaderAlignment"],
				properties["HeaderImage"],
				properties["HeaderImageIndex"],
				properties["HeaderLineAlignment"],
				properties["HeaderStyle"],
				properties["RowSpan"]
			});
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return _propertyOwner;
		}
	}
}
