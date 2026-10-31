using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxGroupBox
	{
		Control IFormatStyleOwner.Owner => this;

		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
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

		public override Rectangle DisplayRectangle
		{
			get
			{
				int captionHeight = 0;
				// The base constructor reads this property before the format style exists.
				if (_formatStyle != null)
				{
					try
					{
						captionHeight = GetCaptionHeight();
					}
					catch (Exception ex) when (ex is ArgumentException || ex is ObjectDisposedException || ex is ExternalException)
					{
						// A font that cannot be created gives no caption space.
					}
				}
				Rectangle displayRectangle = base.DisplayRectangle;
				if (_frameStyle == FrameStyle.Frame)
				{
					if (captionHeight == 0)
					{
						captionHeight = FrameTopGap;
					}
					displayRectangle.Width -= FrameSideMargin * 2;
					displayRectangle.X += FrameSideMargin;
					displayRectangle.Height -= captionHeight + FrameTopPadding + FrameBottomPadding;
					displayRectangle.Y += captionHeight + FrameTopPadding;
				}
				else if (captionHeight > 0 || _frameStyle != FrameStyle.None)
				{
					if (captionHeight == 0)
					{
						captionHeight = FrameTopGap;
					}
					displayRectangle.Height -= captionHeight + FrameTopPadding + FrameBottomPadding;
					displayRectangle.Y += captionHeight + FrameTopPadding;
				}
				return displayRectangle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				if (!_isInitializing && value.A < byte.MaxValue && _backgroundStyle != BackgroundStyle.Default)
				{
					throw new ArgumentException("Transparent colors can only be set if the BackgroundStyle property is set as Default");
				}
				base.BackColor = value;
			}
		}

		[Description("Specifies the background style for the control")]
		[DefaultValue(BackgroundStyle.Default)]
		public BackgroundStyle BackgroundStyle
		{
			get
			{
				return _backgroundStyle;
			}
			set
			{
				if (_backgroundStyle != value)
				{
					_backgroundStyle = value;
					if (_backgroundStyle != BackgroundStyle.Default && BackColor.A != byte.MaxValue)
					{
						BackColor = SystemColors.Control;
					}
					Invalidate();
				}
			}
		}

		[DefaultValue(FrameStyle.Frame)]
		[Description("Specifies the frame style of the control.")]
		public FrameStyle FrameStyle
		{
			get
			{
				return _frameStyle;
			}
			set
			{
				if (_frameStyle != value)
				{
					_frameStyle = value;
					base.OnResize(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[Description("The icon that is displayed on the control.")]
		[DefaultValue(null)]
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

		[Description("The image that is displayed on the control.")]
		[DefaultValue(null)]
		[Localizable(true)]
		public Image Image
		{
			get
			{
				return _displayImage;
			}
			set
			{
				if (_displayImage != value)
				{
					_displayImage = value;
					base.OnResize(EventArgs.Empty);
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the index of the image in the ImageList associated with this control.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
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
					_imageKey = "";
					base.OnResize(EventArgs.Empty);
					InvalidateLayout();
				}
			}
		}

		[Description("The key accessor for the image in the ImageList associated with this control.")]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
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
					_imageIndex = -1;
					base.OnResize(EventArgs.Empty);
					InvalidateLayout();
				}
			}
		}

		[Description("The ImageList that contains the images used in the control.")]
		[DefaultValue(null)]
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
					base.OnResize(EventArgs.Empty);
					InvalidateLayout();
				}
			}
		}

		[Description("The size used to draw images and icons on panels")]
		[Localizable(true)]
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
					base.OnResize(EventArgs.Empty);
					InvalidateLayout();
				}
			}
		}

		[Localizable(false)]
		[Description("Determines the horizontal alignment of the image.")]
		[DefaultValue(ImageHorizontalAlignment.BeforeText)]
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

		[DefaultValue(TextAlignment.Near)]
		[Description("Determines the alignment of the text.")]
		[Localizable(false)]
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

		[Description("Represents the format style properties used to draw the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle FormatStyle => _formatStyle;

		[Description("Represents the color of the border around a the control. This property works only when the control is not painted using Windows XP Visual Stlyes.")]
		public Color BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				if (_borderColor != value)
				{
					_borderColor = value;
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the control when it is disabled.")]
		public FormatStyle DisabledFormatStyle => _disabledFormatStyle;

		[Description("The distance from the margin to the start location of the text")]
		[DefaultValue(DefaultTextOffset)]
		public int TextOffset
		{
			get
			{
				return _textOffset;
			}
			set
			{
				if (_textOffset != value)
				{
					_textOffset = value;
					InvalidateLayout();
				}
			}
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			if (_imageSize.Width == DefaultImageEdge)
			{
				return _imageSize.Height != DefaultImageEdge;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = new Size(DefaultImageEdge, DefaultImageEdge);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBorderColor()
		{
			return !_borderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBorderColor()
		{
			BorderColor = Color.Empty;
		}
	}
}
