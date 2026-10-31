using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.CommandBars
{
	[TypeConverter(typeof(CommandCategoryConverter))]
	public class CommandCategory : IMordexXmlLayoutsSupport
	{
		private string _categoryName = string.Empty;

		private object _tag;

		string IMordexXmlLayoutsSupport.Identifier => _categoryName;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Represents the name of the UICommandCategory.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string CategoryName
		{
			get
			{
				return _categoryName;
			}
			set
			{
				if (_categoryName != value)
				{
					_categoryName = value;
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				if (_tag != value)
				{
					_tag = value;
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			string text = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null);
				_tag = tagTypeName != null ? LayoutTypePolicy.ConvertOrKeepText(text, tagTypeName) : text;
			}
			else
			{
				_tag = null;
			}
			_categoryName = (string)LayoutReader.GetPropertyValue(reader, "CategoryName", typeof(string), "");
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_categoryName.Length > 0)
			{
				info.AddValue("CategoryName", _categoryName);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
		}

		public CommandCategory()
		{
		}

		public CommandCategory(string category)
		{
			_categoryName = category;
		}
	}
}
