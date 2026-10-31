using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Timeline
{
	[TypeConverter(typeof(ValueListItemConverter))]
	public class TimelineValueListItem : MordexValueItemBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier
		{
			get
			{
				if (TimelineUtilities.IsNullOrDBNull(Value))
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
			ImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			Text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), string.Empty);
			string text = (string)LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
			string valueTypeName = "";
			if (text == null)
			{
				return;
			}
			valueTypeName = (string)LayoutReader.GetPropertyValue(info, "TypeNameValue", typeof(string), null);
			if (valueTypeName == null)
			{
				Value = text;
				return;
			}
			try
			{
				Value = PropertyValue.ConvertValue(text, LayoutTypeResolver.Resolve(valueTypeName));
			}
			catch
			{
				Value = text;
			}
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
			if (ImageKey.Length > 0)
			{
				info.AddValue("ImageKey", ImageKey);
			}
			if (Text != string.Empty)
			{
				info.AddValue("Text", Text);
			}
			if (Value != null)
			{
				if (!(Value is string))
				{
					info.AddValue("TypeNameValue", Value.GetType().FullName);
				}
				info.AddValue("Value", Value);
			}
		}

		public TimelineValueListItem()
			: this(null, "", -1)
		{
		}

		public TimelineValueListItem(object value, string text)
			: this(value, text, -1)
		{
		}

		public TimelineValueListItem(object value, string text, int imageIndex)
			: base(value, text, imageIndex)
		{
		}

		internal TimelineValueListItemCollection GetParentCollection()
		{
			return (TimelineValueListItemCollection)base.BaseOwner;
		}

		internal MxTimeline GetOwnerTimeline()
		{
			if (GetParentCollection() == null)
			{
				return null;
			}
			return GetParentCollection().GetTimeline();
		}

		protected override void OnPropertyChanged()
		{
			UpdateTimelineValueListItem();
		}

		internal void UpdateTimelineValueListItem()
		{
			GetOwnerTimeline()?.Invalidate();
		}
	}
}
