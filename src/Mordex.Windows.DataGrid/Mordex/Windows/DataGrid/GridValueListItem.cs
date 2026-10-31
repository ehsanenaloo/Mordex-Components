using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[TypeConverter(typeof(ValueListItemConverter))]
	public class GridValueListItem : MordexValueItemBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier
		{
			get
			{
				if (GridHelper.IsNullOrDBNull(Value))
				{
					return "";
				}
				return Value.ToString();
			}
		}

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public object Value
		{
			get
			{
				return base.BaseValue;
			}
			set
			{
				base.BaseValue = value;
			}
		}

		public string Text
		{
			get
			{
				return base.BaseText;
			}
			set
			{
				base.BaseText = value;
			}
		}

		public int ImageIndex
		{
			get
			{
				return base.BaseImageIndex;
			}
			set
			{
				base.BaseImageIndex = value;
			}
		}

		public string ImageKey
		{
			get
			{
				return base.BaseImageKey;
			}
			set
			{
				base.BaseImageKey = value;
			}
		}

		public Image Image
		{
			get
			{
				return base.BaseImage;
			}
			set
			{
				base.BaseImage = value;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			ImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			Text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), string.Empty);
			ImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), string.Empty);
			string text = (string)LayoutReader.GetPropertyValue(info, "ValueType", typeof(string), "");
			if (text == "")
			{
				Value = LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
				return;
			}
			Value = LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
			Value = LayoutTypeResolver.ConvertOrKeepText((string)Value, text);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Image != null)
			{
				info.AddValue("Image", Image);
			}
			if (ImageIndex != -1)
			{
				info.AddValue("ImageIndex", ImageIndex);
			}
			if (Text != string.Empty)
			{
				info.AddValue("Text", Text);
			}
			if (ImageKey.Length > 0)
			{
				info.AddValue("ImageKey", ImageKey);
			}
			if (Value != null)
			{
				if (!(Value is string))
				{
					info.AddValue("ValueType", Value.GetType().FullName);
				}
				info.AddValue("Value", Value);
			}
		}

		public GridValueListItem()
			: this(null, "", -1)
		{
		}

		public GridValueListItem(object value, string text)
			: this(value, text, -1)
		{
		}

		public GridValueListItem(object value, string text, string imageKey)
			: base(value, text, imageKey)
		{
		}

		public GridValueListItem(object value, string text, int imageIndex)
			: base(value, text, imageIndex)
		{
		}

		internal GridValueListItemCollection GetOwnerCollection()
		{
			return (GridValueListItemCollection)base.BaseOwner;
		}

		internal Image ResolveImage(ImageList imageList)
		{
			if (Image != null)
			{
				return Image;
			}
			if (imageList != null)
			{
				if (ImageKey.Length > 0 && imageList.Images.ContainsKey(ImageKey))
				{
					return imageList.Images[ImageKey];
				}
				if (ImageIndex >= 0 && ImageIndex < imageList.Images.Count)
				{
					return imageList.Images[ImageIndex];
				}
			}
			return null;
		}
	}
}
