using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxButton
	{
		[Description("The dialog result produced in a modal form by clicking the button.")]
		[DefaultValue(DialogResult.None)]
		public DialogResult DialogResult
		{
			get
			{
				return _dialogResult;
			}
			set
			{
				_dialogResult = value;
			}
		}

		Control IFormatStyleOwner.Owner => this;

		int IBaseLineControl.Offset
		{
			get
			{
				if (!_textBounds.IsEmpty)
				{
					return _textBounds.Bottom - BaselineOffsetFromBottom;
				}
				return 0;
			}
		}

		[Category("Appearance")]
		[Description("The face color of the enabled button in place of the theme face. The hot and pressed faces are a darker shade of a light color and a lighter shade of a dark one. A disabled button keeps the disabled face of the theme. A back color set in StateStyles or in the command's StateStyles wins over this color for its state.")]
		public new Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				if (_backColor != value)
				{
					_backColor = value;
					Invalidate();
				}
			}
		}

		[Browsable(true)]
		[DefaultValue(false)]
		[Localizable(true)]
		[Description("Indicates whether the control will automatically size itself to fit its contents.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[Description("Indicates whether the control automatically wraps words to the beginning of the next line when necessary.")]
		[DefaultValue(true)]
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the image that is displayed on the button control.")]
		public Image Image
		{
			get
			{
				return _buttonImage;
			}
			set
			{
				if (_buttonImage != value)
				{
					_buttonImage = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Description("Represents the Icon displayed on the button.")]
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
					InvalidateLayoutAndPaint();
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(ButtonStyle.Default)]
		[Description("Specifies the style and behavior of the button")]
		public ButtonStyle ButtonStyle
		{
			get
			{
				return _buttonStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_buttonStyle != value)
				{
					_buttonStyle = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Description("The image list to get the image to display in the face of the control.")]
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Description("The index of the image in the image list to display in the face of the control.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Localizable(true)]
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
					if (_imageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[Localizable(true)]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed on the face of the control.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
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
					if (_imageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[Description("Represents the image list index value of the image displayed by the control when the mouse is over it.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[Localizable(true)]
		public int HotImageIndex
		{
			get
			{
				return _hotImageIndex;
			}
			set
			{
				if (_hotImageIndex != value)
				{
					_hotImageIndex = value;
					_hotImageKey = "";
					if (ImageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Localizable(true)]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("The key accessor for the image in the ImageList displayed by the control when the mouse is over it.")]
		public string HotImageKey
		{
			get
			{
				return _hotImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_hotImageKey != value)
				{
					_hotImageKey = value;
					_hotImageIndex = -1;
					if (ImageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[DefaultValue(-1)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed by the button when it is disabled.")]
		[Localizable(true)]
		public int DisabledImageIndex
		{
			get
			{
				return _disabledImageIndex;
			}
			set
			{
				if (_disabledImageIndex != value)
				{
					_disabledImageIndex = value;
					_disabledImageKey = "";
					if (!base.Enabled && ImageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[Description("The key accessor for the image in the ImageList displayed by the button when it is disabled.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		public string DisabledImageKey
		{
			get
			{
				return _disabledImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_disabledImageKey != value)
				{
					_disabledImageKey = value;
					_disabledImageIndex = -1;
					if (!base.Enabled && ImageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[DefaultValue(-1)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed by the control when it is pressed.")]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		public int PressedImageIndex
		{
			get
			{
				return _pressedImageIndex;
			}
			set
			{
				if (_pressedImageIndex != value)
				{
					_pressedImageIndex = value;
					_pressedImageKey = "";
					if (ImageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[Description("The key accessor for the image in the ImageList displayed by the control when it is pressed.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		public string PressedImageKey
		{
			get
			{
				return _pressedImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_pressedImageKey != value)
				{
					_pressedImageKey = value;
					_pressedImageIndex = -1;
					if (ImageList != null)
					{
						InvalidateLayoutAndPaint();
					}
				}
			}
		}

		[DefaultValue(TextAlignment.Center)]
		[Localizable(false)]
		[Description("Determines the horizontal alignment of the text.")]
		public TextAlignment TextHorizontalAlignment
		{
			get
			{
				return _textHorizontalAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentException();
				}
				if (value != _textHorizontalAlignment)
				{
					_textHorizontalAlignment = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(TextAlignment.Center)]
		[Localizable(false)]
		[Description("Determines the vertical alignment of the text.")]
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Description("Determines the horizontal alignment of the image.")]
		[DefaultValue(ImageHorizontalAlignment.BeforeText)]
		public ImageHorizontalAlignment ImageHorizontalAlignment
		{
			get
			{
				return _imageHorizontalAlignment;
			}
			set
			{
				if (_imageHorizontalAlignment != value)
				{
					_imageHorizontalAlignment = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(ImageVerticalAlignment.Center)]
		[Description("Determines the vertical alignment of the image.")]
		public ImageVerticalAlignment ImageVerticalAlignment
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(Mordex.Windows.UI.Appearance.Light3D)]
		[Description("Specifies the visual appearance of the control.")]
		public Mordex.Windows.UI.Appearance Appearance
		{
			get
			{
				return _appearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Mordex.Windows.UI.Appearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_appearance != value)
				{
					_appearance = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
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
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(true)]
		[Description("If True the first character preceded by an ampersand (&) will be used as the button's mnemonic key")]
		public bool UseMnemonic
		{
			get
			{
				return _useMnemonic;
			}
			set
			{
				if (_useMnemonic != value)
				{
					_useMnemonic = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the object used to draw the button when it has the focus.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle ActiveFormatStyle => _activeFormatStyle;

		[Description("Represents the object that holds the format styles used to draw the button in its different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public StateStyles StateStyles => _stateStyles;

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

		[Description("The color of the border of the control when it\u00b4s border is Flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
				}
				return _flatBorderColor;
			}
			set
			{
				if (_flatBorderColor != value)
				{
					_flatBorderColor = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicaties whether the button will be highlighted when active.")]
		public bool HighlightActiveButton
		{
			get
			{
				return _highlightActiveButton;
			}
			set
			{
				if (_highlightActiveButton != value)
				{
					_highlightActiveButton = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the control should display focus rectangles on the tab pages.")]
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

		[Editor(typeof(AssociatedCommandEditor), typeof(UITypeEditor))]
		[Description("Represents the Key of the UICommand in the DropDownContextMenu that will behave as the default command. For instance when the user clicks in the button the Click event of the command will be fired")]
		[TypeConverter(typeof(AssociatedCommandConverter))]
		[Localizable(true)]
		[DefaultValue("")]
		public string AssociatedUICommand
		{
			get
			{
				return _associatedCommandKey;
			}
			set
			{
				if (_associatedCommandKey != value)
				{
					_associatedCommandKey = value;
				}
			}
		}

		[Editor(typeof(ContextMenuEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the menu to be displayed in the drop-down button.")]
		[TypeConverter(typeof(ContextMenuConverter))]
		public virtual MxContextMenu DropDownContextMenu
		{
			get
			{
				return _dropDownContextMenu;
			}
			set
			{
				if (_dropDownContextMenu == value)
				{
					return;
				}
				_dropDownContextMenu = value;
				if (_dropDownContextMenu != null)
				{
				}
				InvalidateLayoutAndPaint();
			}
		}

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

		protected virtual string ButtonText
		{
			get
			{
				if (ButtonStyle == ButtonStyle.Ellipsis)
				{
					return "...";
				}
				return Text;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetBackColor()
		{
			BackColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !_backColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return _imageSize != DefaultImageSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = DefaultImageSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayoutAndPaint();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}
	}
}
