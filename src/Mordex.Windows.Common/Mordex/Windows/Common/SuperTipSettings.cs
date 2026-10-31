using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Common
{
	[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design", typeof(UITypeEditor))]
	[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design")]
	public class SuperTipSettings : IMordexXmlLayoutsSupport
	{
		private IImageListProvider _imageListProvider;

		private string _text;

		private string _headerText;

		private string _footerText;

		private bool _wordWrap;

		private ToolTipStyle _toolTipStyle = ToolTipStyle.Office2007;

		private Image _image;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private Image _headerImage;

		private Image _footerImage;

		private int _footerImageIndex = -1;

		private string _footerImageKey = "";

		private int _headerImageIndex = -1;

		private string _headerImageKey = "";

		[DefaultValue("")]
		[Description("The text to display as tool tip for a control.")]
		[Localizable(true)]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value ?? "";
			}
		}

		public IImageListProvider ImageListProvider
		{
			get
			{
				return _imageListProvider;
			}
			set
			{
				_imageListProvider = value;
			}
		}

		[DefaultValue(false)]
		[Description("Determines whether the Text in a tooltip should be wrapped or treated as a single line.")]
		public bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				_wordWrap = value;
			}
		}

		[Description("The text displayed as the header in a tool tip.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string HeaderText
		{
			get
			{
				return _headerText;
			}
			set
			{
				_headerText = value ?? "";
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("The text displayed as the footer in a tool tip.")]
		public string FooterText
		{
			get
			{
				return _footerText;
			}
			set
			{
				_footerText = value ?? "";
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public bool IsEmpty
		{
			get
			{
				if (string.IsNullOrEmpty(_text) && string.IsNullOrEmpty(_headerText) && string.IsNullOrEmpty(_footerText) && HasNoImage() && HasNoHeaderImage())
				{
					return HasNoFooterImage();
				}
				return false;
			}
		}

		[Description("Specifies the visual style of a tool tip.")]
		[DefaultValue(ToolTipStyle.Office2007)]
		public ToolTipStyle ToolTipStyle
		{
			get
			{
				return _toolTipStyle;
			}
			set
			{
				_toolTipStyle = value;
			}
		}

		[DefaultValue(null)]
		[Description("Specifies the Image displayed in a tool tip.")]
		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					if (_image != null)
					{
						_imageIndex = -1;
						_imageKey = "";
					}
				}
			}
		}

		[Description("The ImageList index of the image displayed in a tool tip.")]
		[DefaultValue(-1)]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					if (_imageIndex >= 0)
					{
						_imageKey = "";
						_image = null;
					}
				}
			}
		}

		[Description("The key of the image displayed in a tool tip.")]
		[DefaultValue("")]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					if (_imageKey.Length > 0)
					{
						_imageIndex = -1;
						_image = null;
					}
				}
			}
		}

		[Description("Specifies the Image displayed in the header of a tool tip.")]
		[DefaultValue(null)]
		public Image HeaderImage
		{
			get
			{
				return _headerImage;
			}
			set
			{
				if (_headerImage != value)
				{
					_headerImage = value;
					if (_headerImage != null)
					{
						_headerImageIndex = -1;
						_headerImageKey = "";
					}
				}
			}
		}

		[Description("Specifies the Image displayed in the footer of a tool tip.")]
		[DefaultValue(null)]
		public Image FooterImage
		{
			get
			{
				return _footerImage;
			}
			set
			{
				if (_footerImage != value)
				{
					_footerImage = value;
					if (_footerImage != null)
					{
						_footerImageIndex = -1;
						_footerImageKey = "";
					}
				}
			}
		}

		[Description("The ImageList index of the image displayed in the footer of a tool tip.")]
		[DefaultValue(-1)]
		public int FooterImageIndex
		{
			get
			{
				return _footerImageIndex;
			}
			set
			{
				if (_footerImageIndex != value)
				{
					_footerImageIndex = value;
					if (_footerImageIndex >= 0)
					{
						_footerImageKey = "";
						_footerImage = null;
					}
				}
			}
		}

		[Description("The key of the image displayed in the footer of a tool tip.")]
		[DefaultValue("")]
		public string FooterImageKey
		{
			get
			{
				return _footerImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_footerImageKey != value)
				{
					_footerImageKey = value;
					if (_footerImageKey.Length > 0)
					{
						_footerImageIndex = -1;
						_footerImage = null;
					}
				}
			}
		}

		[Description("The ImageList index of the image displayed in the header of a tool tip.")]
		[DefaultValue(-1)]
		public int HeaderImageIndex
		{
			get
			{
				return _headerImageIndex;
			}
			set
			{
				if (_headerImageIndex != value)
				{
					_headerImageIndex = value;
					if (_headerImageIndex >= 0)
					{
						_headerImageKey = "";
						_headerImage = null;
					}
				}
			}
		}

		[Description("The key of the image displayed in the header of a tool tip.")]
		[DefaultValue("")]
		public string HeaderImageKey
		{
			get
			{
				return _headerImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_headerImageKey != value)
				{
					_headerImageKey = value;
					if (_headerImageKey.Length > 0)
					{
						_headerImageIndex = -1;
						_headerImage = null;
					}
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => "";

		public SuperTipSettings()
		{
			_text = "";
			_headerText = "";
			_footerText = "";
		}

		private bool HasNoImage()
		{
			if (_image == null && _imageIndex == -1)
			{
				return _imageKey.Length == 0;
			}
			return false;
		}

		private bool HasNoHeaderImage()
		{
			if (_headerImage == null && _headerImageIndex == -1)
			{
				return _headerImageKey.Length == 0;
			}
			return false;
		}

		private bool HasNoFooterImage()
		{
			if (_footerImage == null && _footerImageIndex == -1)
			{
				return _footerImageKey.Length == 0;
			}
			return false;
		}

		internal bool HasNoHeaderOrFooter()
		{
			if (string.IsNullOrEmpty(_headerText) && string.IsNullOrEmpty(_footerText) && HasNoHeaderImage())
			{
				return HasNoFooterImage();
			}
			return false;
		}

		internal Image GetImage(MxSuperTip superTip)
		{
			ImageList imageList = GetImageList(superTip);
			if (_image != null)
			{
				return _image;
			}
			if (_imageKey.Length > 0)
			{
				if (imageList != null && imageList.Images.ContainsKey(_imageKey))
				{
					return imageList.Images[_imageKey];
				}
			}
			else if (imageList != null && _imageIndex >= 0 && _imageIndex < imageList.Images.Count)
			{
				return imageList.Images[_imageIndex];
			}
			return null;
		}

		internal ImageList GetImageList(MxSuperTip superTip)
		{
			if (ImageListProvider != null && ImageListProvider.ImageList != null)
			{
				return ImageListProvider.ImageList;
			}
			return superTip?.ImageList;
		}

		internal Image GetHeaderImage(MxSuperTip superTip)
		{
			ImageList imageList = GetImageList(superTip);
			if (_headerImage != null)
			{
				return _headerImage;
			}
			if (_headerImageKey.Length > 0)
			{
				if (imageList != null && imageList.Images.ContainsKey(_headerImageKey))
				{
					return imageList.Images[_headerImageKey];
				}
			}
			else if (imageList != null && _headerImageIndex >= 0 && _headerImageIndex < imageList.Images.Count)
			{
				return imageList.Images[_headerImageIndex];
			}
			return null;
		}

		internal Image GetFooterImage(MxSuperTip superTip)
		{
			ImageList imageList = GetImageList(superTip);
			if (_footerImage != null)
			{
				return _footerImage;
			}
			if (_footerImageKey.Length > 0)
			{
				if (imageList != null && imageList.Images.ContainsKey(_footerImageKey))
				{
					return imageList.Images[_footerImageKey];
				}
			}
			else if (imageList != null && _footerImageIndex >= 0 && _footerImageIndex < imageList.Images.Count)
			{
				return imageList.Images[_footerImageIndex];
			}
			return null;
		}

		public SuperTipSettings Clone()
		{
			SuperTipSettings superTipSettings = new SuperTipSettings();
			superTipSettings._footerImage = _footerImage;
			superTipSettings._footerImageIndex = _footerImageIndex;
			superTipSettings._footerImageKey = _footerImageKey;
			superTipSettings._footerText = _footerText;
			superTipSettings._headerImage = _headerImage;
			superTipSettings._headerImageIndex = _headerImageIndex;
			superTipSettings._headerImageKey = _headerImageKey;
			superTipSettings._headerText = _headerText;
			superTipSettings._image = _image;
			superTipSettings._imageIndex = _imageIndex;
			superTipSettings._imageKey = _imageKey;
			superTipSettings._text = _text;
			superTipSettings._toolTipStyle = _toolTipStyle;
			superTipSettings._wordWrap = _wordWrap;
			return superTipSettings;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter mordexWriter)
		{
			if (_footerImage != null)
			{
				mordexWriter.AddValue("FooterImage", _footerImage);
			}
			if (_footerImageIndex != -1)
			{
				mordexWriter.AddValue("FooterImageIndex", _footerImageIndex);
			}
			if (_footerImageKey.Length > 0)
			{
				mordexWriter.AddValue("FooterImageKey", _footerImageKey);
			}
			if (_footerText.Length > 0)
			{
				mordexWriter.AddValue("FooterText", _footerText);
			}
			if (_headerImage != null)
			{
				mordexWriter.AddValue("HeaderImage", _headerImage);
			}
			if (_headerImageIndex != -1)
			{
				mordexWriter.AddValue("HeaderImageIndex", _headerImageIndex);
			}
			if (_headerImageKey.Length > 0)
			{
				mordexWriter.AddValue("HeaderImageKey", _headerImageKey);
			}
			if (_headerText.Length > 0)
			{
				mordexWriter.AddValue("HeaderText", _headerText);
			}
			if (_image != null)
			{
				mordexWriter.AddValue("Image", _image);
			}
			if (_imageIndex != -1)
			{
				mordexWriter.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				mordexWriter.AddValue("ImageKey", _imageKey);
			}
			if (_text.Length > 0)
			{
				mordexWriter.AddValue("Text", _text);
			}
			if (_toolTipStyle != ToolTipStyle.Office2007)
			{
				mordexWriter.AddValue("ToolTipStyle", _toolTipStyle);
			}
			if (_wordWrap)
			{
				mordexWriter.AddValue("WordWrap", _wordWrap);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader mordexReader)
		{
			_footerImage = (Image)LayoutReader.GetPropertyValue(mordexReader, "FooterImage", typeof(Image), null);
			_footerImageIndex = (int)LayoutReader.GetPropertyValue(mordexReader, "FooterImageIndex", typeof(int), -1);
			_footerImageKey = (string)LayoutReader.GetPropertyValue(mordexReader, "FooterImageKey", typeof(string), "");
			_footerText = (string)LayoutReader.GetPropertyValue(mordexReader, "FooterText", typeof(string), "");
			_headerImage = (Image)LayoutReader.GetPropertyValue(mordexReader, "HeaderImage", typeof(Image), null);
			_headerImageIndex = (int)LayoutReader.GetPropertyValue(mordexReader, "HeaderImageIndex", typeof(int), -1);
			_headerImageKey = (string)LayoutReader.GetPropertyValue(mordexReader, "HeaderImageKey", typeof(string), "");
			_headerText = (string)LayoutReader.GetPropertyValue(mordexReader, "HeaderText", typeof(string), "");
			_image = (Image)LayoutReader.GetPropertyValue(mordexReader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(mordexReader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(mordexReader, "ImageKey", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(mordexReader, "Text", typeof(string), "");
			_toolTipStyle = (ToolTipStyle)LayoutReader.GetPropertyValue(mordexReader, "ToolTipStyle", typeof(ToolTipStyle), ToolTipStyle.Office2007);
			_wordWrap = (bool)LayoutReader.GetPropertyValue(mordexReader, "WordWrap", typeof(bool), false);
		}
	}
}
