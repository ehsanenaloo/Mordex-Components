using System;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Forms;
using CodeDomSerializer = Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;

namespace Mordex.Windows.Navigation.Design
{
	[CLSCompliant(false)]
	public class NavigationItemSerializer : CodeDomSerializer
	{
		public NavigationItemSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		// A second copy of this assembly can own the value type; its own serializer must handle the value then.
		private CodeDomSerializer GetOwningAssemblySerializer(Type valueType)
		{
			Type type = valueType.Assembly.GetType(GetType().FullName, throwOnError: false, ignoreCase: true);
			if (type == null || !typeof(CodeDomSerializer).IsAssignableFrom(type) || type.GetConstructor(Type.EmptyTypes) == null)
			{
				return null;
			}
			return (CodeDomSerializer)Activator.CreateInstance(type);
		}

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			object serializedCode = null;
			try
			{
				if (value.GetType() != typeof(NavigationItem) && string.Compare(value.GetType().FullName, typeof(NavigationItem).FullName, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					CodeDomSerializer owningSerializer = GetOwningAssemblySerializer(value.GetType());
					if (owningSerializer != null)
					{
						return owningSerializer.Serialize(manager, value);
					}
				}
				if (value is NavigationItem element)
				{
					string elementName = GetElementName(element);
					if (elementName != null)
					{
						manager.SetName(value, elementName);
					}
				}
				CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(value.GetType().BaseType, typeof(CodeDomSerializer));
				serializedCode = codeDomSerializer.Serialize(manager, value);
				return serializedCode;
			}
			catch (Exception ex) when (ControlFailure.IsRecoverable(ex))
			{
				manager.ReportError(ex);
				return serializedCode;
			}
		}

		private string GetElementName(NavigationItem element)
		{
			Control navigationBar = element.NavigationBar;
			if (navigationBar == null)
			{
				return null;
			}
			try
			{
				return navigationBar.Name + "_Item_" + element.Group.Index + "_" + element.Index;
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return null;
			}
		}
	}
}
