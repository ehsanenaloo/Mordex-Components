using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridStoredFilter : MordexStoredFilterBase, IFilterCondition, IMordexXmlLayoutsSupport
	{
		internal GridTable _ownerTable;

		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("The key used to identify an object in a colletion.")]
		[DefaultValue("")]
		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				base.BaseKey = value;
			}
		}

		[DefaultValue("")]
		[Description("The description of the stored filter.")]
		public string Description
		{
			get
			{
				return base.BaseDescription;
			}
			set
			{
				base.BaseDescription = value;
			}
		}

		[DefaultValue(null)]
		[TypeConverter("System.ComponentModel.StringConverter")]
		[Description("User defined data to associate with the stored filter.")]
		public object Tag
		{
			get
			{
				return base.BaseTag;
			}
			set
			{
				base.BaseTag = value;
			}
		}

		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.FilterConditionEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Represents the GridFilterCondition instance associated to this instance.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.FilterConditionConverter, Mordex.Windows.DataGrid.Design")]
		public GridFilterCondition FilterCondition
		{
			get
			{
				return (GridFilterCondition)base.BaseFilterCondition;
			}
			set
			{
				base.BaseFilterCondition = value;
			}
		}

		[Browsable(false)]
		public GridTable Table => (GridTable)base.BaseTable;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Description = (string)LayoutReader.GetPropertyValue(info, "Description", typeof(string), "");
			SetIsCurrent((bool)LayoutReader.GetPropertyValue(info, "IsCurrent", typeof(bool), false));
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text != null)
			{
				string otherPropertyValue = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (otherPropertyValue != null)
				{
					Tag = LayoutTypeResolver.ConvertOrKeepText(text, otherPropertyValue);
				}
				else
				{
					Tag = text;
				}
			}
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				GridFilterCondition gridFilterCondition = new GridFilterCondition();
				gridFilterCondition._owner = GetEffectiveTable();
				propertyValue.DeserializeObject(gridFilterCondition);
				gridFilterCondition._owner = null;
				FilterCondition = gridFilterCondition;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Description != "")
			{
				info.AddValue("Description", Description);
			}
			if (FilterCondition != null)
			{
				info.AddValue("FilterCondition", FilterCondition);
			}
			if (GetBaseIsCurrent())
			{
				info.AddValue("IsCurrent", GetBaseIsCurrent());
			}
			if (Key != "")
			{
				info.AddValue("Key", Key);
			}
			if (Tag != null)
			{
				if (!(Tag is string))
				{
					info.AddValue("TagTypeName", Tag.GetType().FullName);
				}
				info.AddValue("Tag", Tag);
			}
		}

		internal bool GetBaseIsCurrent()
		{
			return base.BaseIsCurrent;
		}

		internal void SetIsCurrent(bool baseIsCurrent)
		{
			base.BaseIsCurrent = baseIsCurrent;
		}

		internal GridStoredFilterCollection GetParentFilters()
		{
			return (GridStoredFilterCollection)base.BaseParentCollection;
		}

		internal GridTable GetEffectiveTable()
		{
			if (_ownerTable != null)
			{
				return _ownerTable;
			}
			return Table;
		}
	}
}
