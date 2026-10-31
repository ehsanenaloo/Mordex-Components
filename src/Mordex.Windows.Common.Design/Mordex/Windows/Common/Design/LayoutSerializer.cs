using System;
using System.ComponentModel.Design.Serialization;
using CodeDomSerializer = Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Common.Design
{
	[CLSCompliant(false)]
	public abstract class LayoutSerializer : CodeDomSerializer
	{
		protected abstract Type LayoutType { get; }

		public LayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private CodeDomSerializer CreateSerializerFromTypeAssembly(Type tp)
		{
			Type type = tp.Assembly.GetType(GetType().FullName, throwOnError: true, ignoreCase: true);
			ConstructorInfo constructor = type.GetConstructor(new Type[0]);
			return (CodeDomSerializer)constructor.Invoke(null);
		}

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			object result = null;
			try
			{
				Type type = value.GetType();
				while (type != null && string.Compare(type.FullName, LayoutType.FullName, ignoreCase: true, CultureInfo.InvariantCulture) != 0)
				{
					type = type.BaseType;
				}
				if (type != null && type != LayoutType && string.Compare(type.FullName, LayoutType.FullName, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					CodeDomSerializer properSerializer = CreateSerializerFromTypeAssembly(type);
					return properSerializer.Serialize(manager, value);
				}
				if (value is IMordexLayout mordexLayout)
				{
					mordexLayout.Update();
					string layoutName = GetLayoutName(mordexLayout);
					if (layoutName != null)
					{
						manager.SetName(value, layoutName);
						for (int i = 0; i < mordexLayout.LayoutReferences.Count; i++)
						{
							LayoutReference instance = mordexLayout.LayoutReferences[i];
							manager.SetName(instance, layoutName + "_Reference_" + i);
						}
					}
				}
				Type serializedType = type ?? LayoutType;
				CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(serializedType.BaseType, typeof(CodeDomSerializer));
				result = codeDomSerializer.Serialize(manager, value);
				return result;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "LayoutSerializer.Serialize");
				return result;
			}
		}

		private string GetLayoutName(IMordexLayout layout)
		{
			if (layout.Owner is Control control)
			{
				return control.Name + "_" + layout.DesignTimeName;
			}
			if (layout.Owner is IMordexLayout parentLayout)
			{
				return GetLayoutName(parentLayout) + "_" + layout.DesignTimeName;
			}
			return null;
		}
	}
}
