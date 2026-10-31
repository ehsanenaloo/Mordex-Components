using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace Mordex.Data
{
	internal class BoundListInspector
	{
		private IList _boundList;

		private DataSourceManager _manager;

		internal BoundListInspector(IList list, DataSourceManager manager)
		{
			_manager = manager;
			_boundList = list;
		}

		internal bool IsDataView()
		{
			return _boundList is DataView;
		}

		internal string GetListName(PropertyDescriptor[] listAccessors)
		{
			if (_boundList is ITypedList)
			{
				return ((ITypedList)_boundList).GetListName(listAccessors);
			}
			if (listAccessors == null || listAccessors.Length == 0)
			{
				return _boundList.GetType().Name;
			}
			return listAccessors[listAccessors.Length - 1].Name;
		}

		internal DataTable GetDataTableForTable(IMordexTable iMordexTable)
		{
			if (iMordexTable.ParentTable == null)
			{
				return GetDataTableForList(_boundList);
			}
			if (iMordexTable.ParentTable != null)
			{
				DataTable dataTable = GetDataTableForTable(iMordexTable.ParentTable);
				return GetChildRelationTable(dataTable, iMordexTable.DataMember);
			}
			return null;
		}

		private DataTable GetDataTableForList(IList iList)
		{
			DataTable dataTable = _manager.GetDataTableForList(iList);
			if (dataTable != null)
			{
				return dataTable;
			}
			if (iList is DataView dataView)
			{
				return dataView.Table;
			}
			return null;
		}

		private DataTable GetChildRelationTable(DataTable dataTable, string name)
		{
			if (dataTable != null && dataTable.ChildRelations.Contains(name))
			{
				DataRelation dataRelation = dataTable.ChildRelations[name];
				if (dataRelation != null)
				{
					return dataRelation.ChildTable;
				}
			}
			return null;
		}

		internal PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			if (_boundList is ITypedList)
			{
				return ((ITypedList)_boundList).GetItemProperties(listAccessors);
			}
			return GetListItemProperties(_boundList, 0, listAccessors);
		}

		private PropertyDescriptorCollection GetListItemProperties(IList iList, int integer, PropertyDescriptor[] propertyDescriptorArray)
		{
			if (iList == null)
			{
				return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
			}
			if (iList.Count > 0 && typeof(ICustomTypeDescriptor).IsAssignableFrom(iList[0].GetType()))
			{
				return GetComponentProperties(iList[0], integer, propertyDescriptorArray);
			}
			PropertyDescriptorCollection propertyDescriptorCollection = GetItemPropertiesForType(iList.GetType(), integer, propertyDescriptorArray);
			if (propertyDescriptorCollection == null)
			{
				if (iList.Count > 0)
				{
					return GetComponentProperties(iList[0], integer, propertyDescriptorArray);
				}
				return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
			}
			return propertyDescriptorCollection;
		}

		public Type GetGenericItemType(Type sourceType, Type genericDefinition)
		{
			if (sourceType == null)
			{
				throw new ArgumentNullException("type");
			}
			Predicate<Type> predicate = (Type candidate) => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == genericDefinition;
			if (predicate(sourceType))
			{
				return sourceType.GetGenericArguments()[0];
			}
			Type[] interfaces = sourceType.GetInterfaces();
			Type[] interfaceTypes = interfaces;
			foreach (Type type in interfaceTypes)
			{
				if (predicate(type))
				{
					return type.GetGenericArguments()[0];
				}
			}
			return null;
		}

		public bool ImplementsGenericType(Type sourceType, Type type)
		{
			if (sourceType == null)
			{
				throw new ArgumentNullException("type");
			}
			Predicate<Type> predicate = (Type candidate) => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == type;
			if (predicate(sourceType))
			{
				return true;
			}
			Type[] interfaces = sourceType.GetInterfaces();
			Type[] interfaceTypes = interfaces;
			foreach (Type obj in interfaceTypes)
			{
				if (predicate(obj))
				{
					return true;
				}
			}
			return false;
		}

		private PropertyDescriptorCollection GetItemPropertiesForType(Type componentType, int integer, PropertyDescriptor[] propertyDescriptorArray)
		{
			Type type = null;
			if (componentType.IsGenericType)
			{
				type = GetGenericItemType(componentType, typeof(IList<>));
				if (type == null && typeof(IListSource).IsAssignableFrom(componentType) && ImplementsGenericType(componentType, typeof(ICollection<>)))
				{
					type = GetGenericItemType(componentType, typeof(ICollection<>));
				}
			}
			if (typeof(IList).IsAssignableFrom(componentType) || type != null)
			{
				if (type == null)
				{
					PropertyInfo[] properties = componentType.GetProperties();
					PropertyInfo[] propertyInfos = properties;
					foreach (PropertyInfo propertyInfo in propertyInfos)
					{
						ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
						if (indexParameters.Length > 0 && propertyInfo.PropertyType != typeof(object))
						{
							type = propertyInfo.PropertyType;
							break;
						}
					}
				}
				if (type == null)
				{
					return null;
				}
				if (propertyDescriptorArray == null || integer >= propertyDescriptorArray.Length)
				{
					return TypeDescriptor.GetProperties(type, new Attribute[1]
					{
						new BrowsableAttribute(browsable: true)
					});
				}
				return GetItemPropertiesForType(type, integer, propertyDescriptorArray);
			}
			if (propertyDescriptorArray != null && propertyDescriptorArray.Length > integer && propertyDescriptorArray[integer] != null)
			{
				PropertyDescriptorCollection typeProperties = TypeDescriptor.GetProperties(componentType);
				PropertyDescriptor propertyDescriptor = null;
				if (typeProperties != null)
				{
					propertyDescriptor = TypeDescriptor.GetProperties(componentType).Find(propertyDescriptorArray[integer].Name, ignoreCase: true);
				}
				if (propertyDescriptor != null)
				{
					return GetItemPropertiesForType(propertyDescriptor.PropertyType, integer + 1, propertyDescriptorArray);
				}
				return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
			}
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}

		private PropertyDescriptorCollection GetComponentProperties(object component, int integer, PropertyDescriptor[] propertyDescriptorArray)
		{
			if (propertyDescriptorArray == null || integer >= propertyDescriptorArray.Length)
			{
				return TypeDescriptor.GetProperties(component, new Attribute[1]
				{
					new BrowsableAttribute(browsable: true)
				});
			}
			if (propertyDescriptorArray[integer].GetValue(component) is IList list)
			{
				return GetListItemProperties(list, integer + 1, propertyDescriptorArray);
			}
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}
	}
}
