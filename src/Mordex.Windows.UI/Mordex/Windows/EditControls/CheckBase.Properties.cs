using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class CheckBase
	{
		Control IFormatStyleOwner.Owner => this;

		int IBaseLineControl.Offset => _textBaseline;

		[Browsable(false)]
		public new Size PreferredSize
		{
			get
			{
				if (_isLayoutDirty && base.IsHandleCreated)
				{
					Update();
				}
				if (!_preferredSize.IsEmpty)
				{
					return _preferredSize;
				}
				return base.PreferredSize;
			}
		}

		[Description("Determines the appearance of the check box control")]
		[DefaultValue(System.Windows.Forms.Appearance.Normal)]
		public System.Windows.Forms.Appearance Appearance
		{
			get
			{
				return _appearance;
			}
			set
			{
				if (_appearance != value)
				{
					_appearance = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the Checked or CheckState values and the check box's appearance are automatically changed when the check box is clicked.")]
		public bool AutoCheck
		{
			get
			{
				return _autoCheck;
			}
			set
			{
				_autoCheck = value;
			}
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[Localizable(true)]
		[Description("Indicates whether the control will automatically size itself to fit its contents.")]
		public new bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				if (value != _autoSize)
				{
					_autoSize = value;
					InvalidateLayout();
				}
			}
		}

		[Description("The image that is displayed on the control.")]
		[Localizable(true)]
		[DefaultValue(null)]
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
					InvalidateLayout();
				}
			}
		}

		[Description("Determines the location of the check box inside the control")]
		[DefaultValue(ContentAlignment.MiddleLeft)]
		public ContentAlignment CheckAlign
		{
			get
			{
				return _checkAlign;
			}
			set
			{
				if (_checkAlign != value)
				{
					_checkAlign = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Determines the vertical alignment of the image.")]
		[DefaultValue(Mordex.Windows.UI.Controls.ImageAlignment.Center)]
		public ImageAlignment ImageVerticalAlignment
		{
			get
			{
				return _imageVerticalAlignment;
			}
			set
			{
				if (_imageVerticalAlignment != value)
				{
					_imageVerticalAlignment = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Determines the vertical alignment of the text.")]
		[Localizable(false)]
		[DefaultValue(TextAlignment.Center)]
		public TextAlignment TextVerticalAlignment
		{
			get
			{
				return _textVerticalAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentException();
				}
				if (value != _textVerticalAlignment)
				{
					_textVerticalAlignment = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The icon that is displayed on the control.")]
		public Icon Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The ImageList that contains the images used in the control.")]
		public ImageList ImageList
		{
			get
			{
				return _imageList;
			}
			set
			{
				if (_imageList != value)
				{
					_imageList = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("Represents the index of the image in the ImageList associated with this control.")]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
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
					_imageKey = "";
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList associated with this control.")]
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
					_imageIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[Description("The size used to draw images and icons on panels")]
		public Size ImageSize
		{
			get
			{
				return _imageSize;
			}
			set
			{
				if (_imageSize != value)
				{
					_imageSize = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		[Description("The text that is shown when the mouse hovers over the control.")]
		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
			}
		}

		[Description("Indicates whether the control should display focus rectangles.")]
		[DefaultValue(true)]
		public bool ShowFocusRectangle
		{
			get
			{
				return _showFocusRectangle;
			}
			set
			{
				if (_showFocusRectangle != value)
				{
					_showFocusRectangle = value;
					Invalidate();
				}
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[Description("Determines the horizontal alignment of the image.")]
		[DefaultValue(ImageHorizontalAlignment.BeforeText)]
		[Localizable(false)]
		public ImageHorizontalAlignment ImageAlignment
		{
			get
			{
				return _imageAlignment;
			}
			set
			{
				if (_imageAlignment != value)
				{
					_imageAlignment = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(TextAlignment.Near)]
		[Localizable(false)]
		[Description("Determines the alignment of the text.")]
		public TextAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentException();
				}
				if (value == TextAlignment.Empty)
				{
					value = TextAlignment.Near;
				}
				if (value != _textAlignment)
				{
					_textAlignment = value;
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the control.")]
		public FormatStyle FormatStyle => _formatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the control when it is disabled. Values set here win over FormatStyle and the theme.")]
		public FormatStyle DisabledFormatStyle => _disabledFormatStyle;

		[DefaultValue(true)]
		[Description("Indicates whether the control automatically wraps words to the beginning of the next line when necessary.")]
		public bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				if (_wordWrap != value)
				{
					_wordWrap = value;
					InvalidateLayout();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			if (_imageSize.Width == DefaultImageSide)
			{
				return _imageSize.Height != DefaultImageSide;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = new Size(DefaultImageSide, DefaultImageSide);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}
	}
}
