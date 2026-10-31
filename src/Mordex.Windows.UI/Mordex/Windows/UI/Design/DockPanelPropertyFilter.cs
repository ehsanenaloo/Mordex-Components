using System;
using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	internal class DockPanelPropertyFilter : ICustomTypeDescriptor
	{
		private DockPanelBase _propertyOwner;

		private string _filter;

		internal DockPanelPropertyFilter(DockPanelBase panel, string filter)
		{
			_propertyOwner = panel;
			_filter = filter;
		}

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(_propertyOwner);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return "UIPanel";
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return _propertyOwner.Name;
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
			return null;
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
			return GetFilteredProperties();
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return GetFilteredProperties();
		}

		private PropertyDescriptorCollection GetFilteredProperties()
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(_propertyOwner);
			PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(null);
			if (_filter == "Behavior")
			{
				try
				{
					propertyDescriptorCollection.Add(properties["AllowResize"]);
					propertyDescriptorCollection.Add(properties["AllowPanelDrag"]);
					propertyDescriptorCollection.Add(properties["AllowPanelDrop"]);
					propertyDescriptorCollection.Add(properties["CaptionDoubleClickAction"]);
					propertyDescriptorCollection.Add(properties["UseActiveCaptionStyle"]);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
			}
			else if (_filter == "Appearance")
			{
				propertyDescriptorCollection.Add(properties["CaptionVisible"]);
				propertyDescriptorCollection.Add(properties["InfoText"]);
				propertyDescriptorCollection.Add(properties["TextAlignment"]);
				propertyDescriptorCollection.Add(properties["ImageAlignment"]);
				propertyDescriptorCollection.Add(properties["InfoTextAlignment"]);
				propertyDescriptorCollection.Add(properties["CaptionDisplayMode"]);
				propertyDescriptorCollection.Add(properties["CaptionHeight"]);
				propertyDescriptorCollection.Add(properties["AutoHideTabDisplay"]);
				propertyDescriptorCollection.Add(properties["AutoHideButtonVisible"]);
				propertyDescriptorCollection.Add(properties["CloseButtonVisible"]);
				propertyDescriptorCollection.Add(properties["ImageIndex"]);
				propertyDescriptorCollection.Add(properties["Image"]);
				propertyDescriptorCollection.Add(properties["Icon"]);
				propertyDescriptorCollection.Add(properties["LargeImageIndex"]);
				propertyDescriptorCollection.Add(properties["LargeImage"]);
				propertyDescriptorCollection.Add(properties["LargeIcon"]);
				propertyDescriptorCollection.Add(properties["BorderCaption"]);
				propertyDescriptorCollection.Add(properties["BorderCaptionColor"]);
				propertyDescriptorCollection.Add(properties["BorderPanel"]);
				propertyDescriptorCollection.Add(properties["BorderPanelColor"]);
				propertyDescriptorCollection.Add(properties["CaptionStyle"]);
				propertyDescriptorCollection.Add(properties["CaptionFormatStyle"]);
				propertyDescriptorCollection.Add(properties["ActiveCaptionFormatStyle"]);
				propertyDescriptorCollection.Add(properties["InfoTextFormatStyle"]);
				if (_propertyOwner is MxDockPanel)
				{
					propertyDescriptorCollection.Add(properties["InnerAreaStyle"]);
					propertyDescriptorCollection.Add(properties["InnerContainerFormatStyle"]);
				}
			}
			else if (_filter == "All")
			{
				propertyDescriptorCollection = properties;
			}
			return propertyDescriptorCollection;
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return _propertyOwner;
		}
	}
}
