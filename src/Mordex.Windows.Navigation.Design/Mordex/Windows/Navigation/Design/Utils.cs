using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mordex.Windows.Navigation.Design
{
	internal static class Utils
	{
		internal static string GetObjectKey(Type type, string objectName, int index, ArrayList buffer)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(type).Find("Key", ignoreCase: true);
			if (propertyDescriptor == null)
			{
				return "";
			}
			HashSet<string> existingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (object item in buffer)
			{
				existingKeys.Add((string)propertyDescriptor.GetValue(item));
			}
			while (existingKeys.Contains(objectName + index))
			{
				index++;
			}
			return objectName + index;
		}
	}
}
