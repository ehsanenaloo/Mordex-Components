using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridFormatStyle
	{
		string IMordexXmlLayoutsSupport.Identifier
		{
			get
			{
				if (_owner is GridFormatStyleCollection)
				{
					return _identifier;
				}
				return null;
			}
		}

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxDataGrid DataGrid
		{
			get
			{
				if (_owner == null)
				{
					return null;
				}
				return _owner.DataGrid;
			}
		}

		[Description("The key used to identify an object in a colletion.")]
		[DefaultValue("")]
		public string Key
		{
			get
			{
				return _identifier;
			}
			set
			{
				ThrowIfReadOnly();
				_identifier = value;
			}
		}

		[DefaultValue(-1)]
		[Description("Indicates the alpha component that will be applied to colors in the FormatStyle.")]
		public int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				ThrowIfReadOnly();
				if (value < -1 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _alpha)
				{
					_alpha = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Represents the background color of the GridFormatStyle.")]
		public Color BackColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				ThrowIfReadOnly();
				if (value != _backgroundColor)
				{
					_backgroundColor = value;
					_hasExplicitBackColor = _backgroundColor != _baseBackColor;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Specifies the background color used in a linear gradient pathern.")]
		public Color BackColorGradient
		{
			get
			{
				return _backColorGradient;
			}
			set
			{
				ThrowIfReadOnly();
				if (value != _backColorGradient)
				{
					_backColorGradient = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Specifies if a linear gradient will be applied and its direction.")]
		[DefaultValue(BackgroundGradientMode.Empty)]
		public BackgroundGradientMode BackgroundGradientMode
		{
			get
			{
				return _backgroundGradientMode;
			}
			set
			{
				ThrowIfReadOnly();
				if (_backgroundGradientMode != value)
				{
					_backgroundGradientMode = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(AlphaMode.Empty)]
		[Description("Determines how alpha should be combined with the background color.")]
		public AlphaMode BackColorAlphaMode
		{
			get
			{
				return _backColorAlphaMode;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backColorAlphaMode)
				{
					_backColorAlphaMode = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Specifies the foreground color of the GridFormatStyle.")]
		public Color ForeColor
		{
			get
			{
				return _textColor;
			}
			set
			{
				ThrowIfReadOnly();
				if (value != _textColor)
				{
					_textColor = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(AlphaMode.Empty)]
		[Description("Determines how alpha should be combined with the foreground color.")]
		public AlphaMode ForeColorAlphaMode
		{
			get
			{
				return _foreColorAlphaMode;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _foreColorAlphaMode)
				{
					_foreColorAlphaMode = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represent the font used to draw a text.")]
		public Font Font
		{
			get
			{
				return _baseFont;
			}
			set
			{
				ThrowIfReadOnly();
				if (value != _baseFont)
				{
					ReleaseBaseFont();
					ReleaseResolvedFont();
					_baseFont = value;
					_isUserFont = value != null;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Specifies the 3D style used to draw a grid area.")]
		public Appearance Appearance
		{
			get
			{
				return _appearance;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(Appearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _appearance)
				{
					_appearance = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Represents the background image.")]
		public Image BackgroundImage
		{
			get
			{
				return _backgroundImage;
			}
			set
			{
				ThrowIfReadOnly();
				_backgroundImage = value;
				NotifyOwnerLayoutChanged();
			}
		}

		[Description("Determines the horizontal alignment of the image.")]
		[DefaultValue(ImageHorizontalAlignment.Empty)]
		public ImageHorizontalAlignment ImageHorizontalAlignment
		{
			get
			{
				return _imageHorizontalAlignment;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(ImageHorizontalAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _imageHorizontalAlignment)
				{
					_imageHorizontalAlignment = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Determines the vertical alignment of the image.")]
		[DefaultValue(ImageVerticalAlignment.Empty)]
		public ImageVerticalAlignment ImageVerticalAlignment
		{
			get
			{
				return _imageVerticalAlignment;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(ImageVerticalAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _imageVerticalAlignment)
				{
					_imageVerticalAlignment = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Determines the way a background image is drawn.")]
		[DefaultValue(BackgroundImageDrawMode.Empty)]
		public BackgroundImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(BackgroundImageDrawMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backgroundImageDrawMode)
				{
					_backgroundImageDrawMode = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object used is bold.")]
		public TriState FontBold
		{
			get
			{
				return _fontBold;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontBold)
				{
					_fontBold = value;
					ReleaseResolvedFont();
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object used is italic.")]
		public TriState FontItalic
		{
			get
			{
				return _fontItalic;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontItalic)
				{
					_fontItalic = value;
					ReleaseResolvedFont();
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Indicates whether the Font object used is underlined.")]
		public TriState FontUnderline
		{
			get
			{
				return _fontUnderline;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontUnderline)
				{
					_fontUnderline = value;
					ReleaseResolvedFont();
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Indicates whether the Font object used has a horizontal line through the font.")]
		[DefaultValue(TriState.Empty)]
		public TriState FontStrikeout
		{
			get
			{
				return _fontStrikeout;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontStrikeout)
				{
					_fontStrikeout = value;
					ReleaseResolvedFont();
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Indicates the face name of the font used in the GridFormatStyle object.")]
		[DefaultValue("")]
		public string FontName
		{
			get
			{
				return _fontName;
			}
			set
			{
				ThrowIfReadOnly();
				if (value != _fontName)
				{
					_fontName = value;
					ReleaseResolvedFont();
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("The size of the font used in the GridFormatStyle object.")]
		[DefaultValue(0f)]
		public float FontSize
		{
			get
			{
				return _fontSize;
			}
			set
			{
				ThrowIfReadOnly();
				if (!float.IsFinite(value) || value < 0f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _fontSize)
				{
					_fontSize = value;
					ReleaseResolvedFont();
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Determines how the text is aligned.")]
		public TextAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _textAlignment)
				{
					_textAlignment = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Description("Determines how the line(s) of text are aligned.")]
		public TextAlignment LineAlignment
		{
			get
			{
				return _lineAlignment;
			}
			set
			{
				ThrowIfReadOnly();
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _lineAlignment)
				{
					_lineAlignment = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[Browsable(false)]
		public PredefinedStyle PredefinedStyle => _predefinedStyle;

		[Description("Indicates the percentage of the BlendColor to blend with the BackColor.")]
		[DefaultValue(-1f)]
		public float Blend
		{
			get
			{
				return _blend;
			}
			set
			{
				ThrowIfReadOnly();
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blend)
				{
					_blend = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}

		[DefaultValue(-1f)]
		[Description("Indicates the percentage of the BlendColor to blend with the BackColorGradient.")]
		public float BlendGradient
		{
			get
			{
				return _blendGradient;
			}
			set
			{
				ThrowIfReadOnly();
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blendGradient)
				{
					_blendGradient = value;
					NotifyOwnerLayoutChanged();
				}
			}
		}
	}
}
