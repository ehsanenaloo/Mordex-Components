using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class DefaultPanelSettings : IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private Size _imageSize;

		private Size _largeImageSize;

		private TabDisplayMode _autoHideTabDisplay;

		private TabDisplayMode _tabDisplay;

		private TabAlignment _tabAlignment;

		private bool _useActiveCaptionStyle;

		private PanelInnerAreaStyle _innerAreaStyle;

		private PanelCaptionStyle _captionStyle;

		private MxDockManager _dockManager;

		private bool _captionVisible;

		private bool _closeButtonVisible;

		private bool _autoHideButtonVisible;

		private Size _minimumSize;

		private Size _maximumSize;

		private FormatStyle _formatStyleActiveCaption;

		private FormatStyle _formatStyleCaption;

		private FormatStyle _formatStyleDarkCaption;

		private FormatStyle _formatStyleLightCaption;

		private FormatStyle _formatStyleInnerContainer;

		private FormatStyle _formatStyleInfoText;

		private StateStyles _tabStateStyles;

		private Color _borderCaptionColor;

		private Color _borderPanelColor;

		private bool _borderCaption;

		private bool _borderPanel;

		private ActiveCaptionMode _activeCaptionMode;

		private PanelCaptionDisplayMode _captionDisplayMode;

		private CaptionDoubleClickAction _captionDoubleClickAction;

		private int _captionHeight;

		private PanelTextAlignment _textAlignment;

		private PanelImageAlignment _imageAlignment;

		private Image _closeButtonImage;

		private Image _pinButtonImage;

		private Image _unpinButtonImage;

		private PanelTextAlignment _infoTextAlignment;

		[Description("Determines which action is executed when the user double clicks the caption of a panel.")]
		[DefaultValue(CaptionDoubleClickAction.ToggleDockState)]
		public CaptionDoubleClickAction CaptionDoubleClickAction
		{
			get
			{
				return _captionDoubleClickAction;
			}
			set
			{
				_captionDoubleClickAction = value;
			}
		}

		[Description("The UIStateStyles that holds the format styles used to the tabs of a panel group in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public StateStyles TabStateStyles => _tabStateStyles;

		[DefaultValue(ActiveCaptionMode.PanelFocus)]
		[Description("Determines when the panel should paints its caption bar using the active caption style.")]
		public ActiveCaptionMode ActiveCaptionMode
		{
			get
			{
				return _activeCaptionMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ActiveCaptionMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == ActiveCaptionMode.UseDefault)
				{
					value = ActiveCaptionMode.PanelFocus;
				}
				if (_activeCaptionMode != value)
				{
					_activeCaptionMode = value;
					NotifySettingChanged("ActiveCaptionMode");
				}
			}
		}

		[DefaultValue(true)]
		[Browsable(false)]
		[Description("Determines whether the active panels should use the ActiveCaptionFormatStyle to draw its caption bar.")]
		[Localizable(false)]
		[Obsolete("This property is obsolete. Use ActiveCaptionMode property instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool UseActiveCaptionStyle
		{
			get
			{
				return _useActiveCaptionStyle;
			}
			set
			{
				if (_useActiveCaptionStyle != value)
				{
					_useActiveCaptionStyle = value;
					if (value)
					{
						ActiveCaptionMode = ActiveCaptionMode.PanelFocus;
					}
					else
					{
						ActiveCaptionMode = ActiveCaptionMode.Never;
					}
				}
			}
		}

		[Localizable(false)]
		[Description("Determines how the inner area of a panel is drawn.")]
		[DefaultValue(PanelInnerAreaStyle.UseFormatStyle)]
		public PanelInnerAreaStyle InnerAreaStyle
		{
			get
			{
				return _innerAreaStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelInnerAreaStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == PanelInnerAreaStyle.UseDefault)
				{
					value = PanelInnerAreaStyle.UseFormatStyle;
				}
				if (_innerAreaStyle != value)
				{
					_innerAreaStyle = value;
					NotifySettingChanged("InnerAreaStyle");
				}
			}
		}

		[Description("Determines which predefined style will be used to draw the background of the caption bar in a panel.")]
		[Localizable(false)]
		[DefaultValue(PanelCaptionStyle.Standard)]
		public PanelCaptionStyle CaptionStyle
		{
			get
			{
				return _captionStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelCaptionStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == PanelCaptionStyle.UseDefault)
				{
					value = PanelCaptionStyle.Standard;
				}
				if (value != _captionStyle)
				{
					_captionStyle = value;
					NotifySettingChanged("CaptionStyle");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the caption of a panel is drawn with a border around it.")]
		[Localizable(true)]
		public bool BorderCaption
		{
			get
			{
				return _borderCaption;
			}
			set
			{
				if (_borderCaption != value)
				{
					_borderCaption = value;
					NotifySettingChanged("BorderCaption");
				}
			}
		}

		[Description("Determines what is displayed in the caption bar of a panel.")]
		[DefaultValue(PanelCaptionDisplayMode.ImageAndText)]
		[Localizable(false)]
		public PanelCaptionDisplayMode CaptionDisplayMode
		{
			get
			{
				return _captionDisplayMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelCaptionDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == PanelCaptionDisplayMode.UseDefault)
				{
					value = PanelCaptionDisplayMode.ImageAndText;
				}
				if (_captionDisplayMode != value)
				{
					_captionDisplayMode = value;
					NotifySettingChanged("CaptionDisplayMode");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the panels are drawn with a border around it.")]
		public bool BorderPanel
		{
			get
			{
				return _borderPanel;
			}
			set
			{
				if (_borderPanel != value)
				{
					_borderPanel = value;
					NotifySettingChanged("BorderPanel");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the caption bar is shown.")]
		[Localizable(true)]
		public bool CaptionVisible
		{
			get
			{
				return _captionVisible;
			}
			set
			{
				if (_captionVisible != value)
				{
					_captionVisible = value;
					NotifySettingChanged("CaptionVisible");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the close button is visible.")]
		[Localizable(true)]
		public bool CloseButtonVisible
		{
			get
			{
				return _closeButtonVisible;
			}
			set
			{
				if (_closeButtonVisible != value)
				{
					_closeButtonVisible = value;
					NotifySettingChanged("CloseButtonVisible");
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the custom image that will be displayed in the close button.")]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image CloseButtonImage
		{
			get
			{
				return _closeButtonImage;
			}
			set
			{
				if (_closeButtonImage != value)
				{
					_closeButtonImage = value;
					NotifySettingChanged("CloseButtonImage");
				}
			}
		}

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the custom image that will be displayed in the pin button.")]
		public Image PinButtonImage
		{
			get
			{
				return _pinButtonImage;
			}
			set
			{
				if (_pinButtonImage != value)
				{
					_pinButtonImage = value;
					NotifySettingChanged("PinButtonImage");
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the custom image that will be displayed in the unpin button.")]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image UnpinButtonImage
		{
			get
			{
				return _unpinButtonImage;
			}
			set
			{
				if (_unpinButtonImage != value)
				{
					_unpinButtonImage = value;
					NotifySettingChanged("UnpinButtonImage");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the auto hide button is visible.")]
		[Localizable(true)]
		public bool AutoHideButtonVisible
		{
			get
			{
				return _autoHideButtonVisible;
			}
			set
			{
				if (_autoHideButtonVisible != value)
				{
					_autoHideButtonVisible = value;
					NotifySettingChanged("AutoHideButtonVisible");
				}
			}
		}

		[Localizable(true)]
		[Description("Default minimum size for panels.")]
		public Size MinimumSize
		{
			get
			{
				return _minimumSize;
			}
			set
			{
				if (value.Height < -1 || value.Width < -1)
				{
					throw new ArgumentException("Minimum Size must be greater than or equal to 0. or -1 to use the default minimum (the size of the panel caption).");
				}
				if (value != _minimumSize)
				{
					_minimumSize = value;
					NotifySettingChanged("MinimumSize");
				}
			}
		}

		[Description("Default maximum size for panels.")]
		[Localizable(true)]
		public Size MaximumSize
		{
			get
			{
				return _maximumSize;
			}
			set
			{
				if (value.Height < 0 || value.Width < 0)
				{
					throw new ArgumentException("Maximum Size must be greater than or equal to 0.");
				}
				if (value != _maximumSize)
				{
					_maximumSize = value;
					NotifySettingChanged("MaximumSize");
				}
			}
		}

		[Localizable(true)]
		[Description("The default height of the caption bar in a panel or -1 to size the caption automatically.")]
		[DefaultValue(-1)]
		public int CaptionHeight
		{
			get
			{
				return _captionHeight;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException("CaptionHeight must be greater than or equal to 0 or use -1 to specify auto size in panel captions.");
				}
				if (value != _captionHeight)
				{
					_captionHeight = value;
					NotifySettingChanged("CaptionHeight");
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Settings used to draw the caption bar of panels with CaptionStyle 'Standard'.")]
		public FormatStyle CaptionFormatStyle => _formatStyleCaption;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Settings used to draw the info text in the caption bar a panel.")]
		public FormatStyle InfoTextFormatStyle => _formatStyleInfoText;

		[Description("Settings used to draw the inner area of panels with InnerAreaStyle defined as 'UseFormatStyle'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle InnerContainerFormatStyle => _formatStyleInnerContainer;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Settings used to draw the caption bar of panels with CaptionStyle 'Dark'.")]
		public FormatStyle DarkCaptionFormatStyle => _formatStyleDarkCaption;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Settings used to draw the caption bar of panels with CaptionStyle 'Light'.")]
		public FormatStyle LightCaptionFormatStyle => _formatStyleLightCaption;

		[Description("Settings used to draw the caption bar of an active panel.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("FormatStyles")]
		public FormatStyle ActiveCaptionFormatStyle => _formatStyleActiveCaption;

		[Description("Indicates the border color of a panel.")]
		public Color BorderPanelColor
		{
			get
			{
				if (_borderPanelColor.IsEmpty)
				{
					return _dockManager.GetDefaultColor(PanelColors.Border);
				}
				return _borderPanelColor;
			}
			set
			{
				if (_borderPanelColor != value)
				{
					_borderPanelColor = value;
					NotifySettingChanged("BorderPanelColor");
				}
			}
		}

		[Description("Indicates the border color of the caption bar in a panel.")]
		public Color BorderCaptionColor
		{
			get
			{
				if (_borderCaptionColor.IsEmpty)
				{
					return _dockManager.GetDefaultColor(PanelColors.Border);
				}
				return _borderCaptionColor;
			}
			set
			{
				if (_borderCaptionColor != value)
				{
					_borderCaptionColor = value;
					NotifySettingChanged("BorderCaptionColor");
				}
			}
		}

		Control IFormatStyleOwner.Owner => null;

		[DefaultValue(TabDisplayMode.ImageAndText)]
		[Description("Specifies the how the tab pages will be drawn on a tab group.")]
		[Localizable(false)]
		public TabDisplayMode TabDisplay
		{
			get
			{
				return _tabDisplay;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == TabDisplayMode.UseDefault)
				{
					value = TabDisplayMode.ImageAndText;
				}
				if (_tabDisplay != value)
				{
					_tabDisplay = value;
					NotifySettingChanged("TabDisplay");
				}
			}
		}

		[Localizable(false)]
		[Description("Specifies the area of the panel group where the tabs are aligned.")]
		[DefaultValue(TabAlignment.Bottom)]
		public TabAlignment TabAlignment
		{
			get
			{
				return _tabAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == TabAlignment.UseDefault)
				{
					value = TabAlignment.Bottom;
				}
				if (_tabAlignment != value)
				{
					_tabAlignment = value;
					NotifySettingChanged("TabAlignment");
				}
			}
		}

		[Localizable(false)]
		[Description("Determines how tabs are displayed in an autohide strip.")]
		[DefaultValue(TabDisplayMode.ImageAndTextOnSelected)]
		public TabDisplayMode AutoHideTabDisplay
		{
			get
			{
				return _autoHideTabDisplay;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == TabDisplayMode.UseDefault)
				{
					value = TabDisplayMode.ImageAndTextOnSelected;
				}
				if (_autoHideTabDisplay != value)
				{
					_autoHideTabDisplay = value;
					NotifySettingChanged("AutoHideTabDisplay");
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
					NotifySettingChanged("ImageSize");
				}
			}
		}

		[Description("The size used to draw images and icons on panel buttons of an Outlook Navigator Group.")]
		[Localizable(true)]
		public Size LargeImageSize
		{
			get
			{
				return _largeImageSize;
			}
			set
			{
				if (_largeImageSize != value)
				{
					_largeImageSize = value;
					NotifySettingChanged("LargeImageSize");
				}
			}
		}

		[DefaultValue(PanelTextAlignment.Near)]
		[Description("Determines the alignment of the text in the panel caption.")]
		public PanelTextAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelTextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == PanelTextAlignment.UseDefault)
				{
					value = PanelTextAlignment.Near;
				}
				if (value != _textAlignment)
				{
					_textAlignment = value;
					NotifySettingChanged("PanelTextAlignment");
				}
			}
		}

		[DefaultValue(PanelImageAlignment.Near)]
		[Description("Determines the alignment of the image in the panel caption.")]
		public PanelImageAlignment ImageAlignment
		{
			get
			{
				return _imageAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelImageAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == PanelImageAlignment.UseDefault)
				{
					value = PanelImageAlignment.Near;
				}
				if (value != _imageAlignment)
				{
					_imageAlignment = value;
					NotifySettingChanged("ImageAlignment");
				}
			}
		}

		[Description("Determines the alignment of the InfoText in the panel caption.")]
		[DefaultValue(PanelTextAlignment.Far)]
		public PanelTextAlignment InfoTextAlignment
		{
			get
			{
				return _infoTextAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelTextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == PanelTextAlignment.UseDefault)
				{
					value = PanelTextAlignment.Far;
				}
				if (value != _infoTextAlignment)
				{
					_infoTextAlignment = value;
					NotifySettingChanged("InfoTextAlignment");
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal DefaultPanelSettings(MxDockManager panelManager)
		{
			InitializeDefaults();
			_dockManager = panelManager;
		}

		private void ReleaseStyleOwners()
		{
			_tabStateStyles._owner = null;
			_tabStateStyles = null;
			_formatStyleInnerContainer._owner = null;
			_formatStyleInnerContainer = null;
			_formatStyleActiveCaption._owner = null;
			_formatStyleActiveCaption = null;
			_formatStyleDarkCaption._owner = null;
			_formatStyleDarkCaption = null;
			_formatStyleCaption._owner = null;
			_formatStyleCaption = null;
			_formatStyleLightCaption._owner = null;
			_formatStyleLightCaption = null;
			_formatStyleInfoText._owner = null;
			_formatStyleInfoText = null;
		}

		private void InitializeDefaults()
		{
			_imageAlignment = PanelImageAlignment.Near;
			_textAlignment = PanelTextAlignment.Near;
			_infoTextAlignment = PanelTextAlignment.Far;
			_imageSize = new Size(0, 0);
			_largeImageSize = new Size(24, 24);
			_autoHideTabDisplay = TabDisplayMode.ImageAndTextOnSelected;
			_tabDisplay = TabDisplayMode.ImageAndText;
			_tabAlignment = TabAlignment.Bottom;
			_captionDoubleClickAction = CaptionDoubleClickAction.ToggleDockState;
			_tabStateStyles = new StateStyles(this);
			_formatStyleInnerContainer = new FormatStyle(this);
			_useActiveCaptionStyle = true;
			_innerAreaStyle = PanelInnerAreaStyle.UseFormatStyle;
			ResetBorderCaptionColor();
			ResetBorderPanelColor();
			_borderCaption = true;
			_borderPanel = true;
			_captionVisible = true;
			_minimumSize = new Size(40, 40);
			_maximumSize = new Size(0, 0);
			_captionHeight = -1;
			_closeButtonVisible = true;
			_autoHideButtonVisible = true;
			_activeCaptionMode = ActiveCaptionMode.PanelFocus;
			_captionDisplayMode = PanelCaptionDisplayMode.ImageAndText;
			_formatStyleCaption = new FormatStyle(this);
			_formatStyleDarkCaption = new FormatStyle(this);
			_formatStyleLightCaption = new FormatStyle(this);
			_formatStyleActiveCaption = new FormatStyle(this);
			_formatStyleInfoText = new FormatStyle(this);
			_captionStyle = PanelCaptionStyle.Standard;
			_closeButtonImage = null;
			_pinButtonImage = null;
			_unpinButtonImage = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			ReleaseStyleOwners();
			InitializeDefaults();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_tabStateStyles.ShouldSerialize())
			{
				return true;
			}
			if (!_formatStyleInnerContainer.IsDefaultStyle())
			{
				return true;
			}
			if (!_useActiveCaptionStyle)
			{
				return true;
			}
			if (_innerAreaStyle != PanelInnerAreaStyle.UseFormatStyle)
			{
				return true;
			}
			if (ShouldSerializeBorderCaptionColor())
			{
				return true;
			}
			if (ShouldSerializeBorderPanelColor())
			{
				return true;
			}
			if (!_borderCaption)
			{
				return true;
			}
			if (!_borderPanel)
			{
				return true;
			}
			if (!_captionVisible)
			{
				return true;
			}
			if (_minimumSize != new Size(40, 40))
			{
				return true;
			}
			if (_maximumSize != new Size(0, 0))
			{
				return true;
			}
			if (_captionHeight != -1)
			{
				return true;
			}
			if (!_closeButtonVisible)
			{
				return true;
			}
			if (!_autoHideButtonVisible)
			{
				return true;
			}
			if (_captionDisplayMode != PanelCaptionDisplayMode.ImageAndText)
			{
				return true;
			}
			if (!_formatStyleCaption.IsDefaultStyle())
			{
				return true;
			}
			if (!_formatStyleDarkCaption.IsDefaultStyle())
			{
				return true;
			}
			if (!_formatStyleLightCaption.IsDefaultStyle())
			{
				return true;
			}
			if (!_formatStyleActiveCaption.IsDefaultStyle())
			{
				return true;
			}
			if (_captionStyle != PanelCaptionStyle.Standard)
			{
				return true;
			}
			if (_captionDoubleClickAction != CaptionDoubleClickAction.ToggleDockState)
			{
				return true;
			}
			if (!_imageSize.IsEmpty)
			{
				return true;
			}
			if (ShouldSerializeLargeImageSize())
			{
				return true;
			}
			if (_infoTextAlignment != PanelTextAlignment.Far)
			{
				return true;
			}
			if (_textAlignment != PanelTextAlignment.Near)
			{
				return true;
			}
			if (_imageAlignment != PanelImageAlignment.Near)
			{
				return true;
			}
			if (!_formatStyleInfoText.IsDefaultStyle())
			{
				return true;
			}
			if (_pinButtonImage != null)
			{
				return true;
			}
			if (_unpinButtonImage != null)
			{
				return true;
			}
			if (_closeButtonImage != null)
			{
				return true;
			}
			return false;
		}

		private void NotifySettingChanged(string text)
		{
			if (_dockManager != null)
			{
				_dockManager.RefreshPanelSettings(text);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinimumSize()
		{
			return _minimumSize != new Size(40, 40);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinimumSize()
		{
			MinimumSize = new Size(0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaximumSize()
		{
			return _maximumSize != new Size(0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaximumSize()
		{
			MaximumSize = new Size(0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBorderPanelColor()
		{
			return !_borderPanelColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBorderPanelColor()
		{
			_borderPanelColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBorderCaptionColor()
		{
			return _borderCaptionColor != Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBorderCaptionColor()
		{
			BorderCaptionColor = Color.Empty;
		}

		void IFormatStyleOwner.PropertyChange()
		{
			NotifySettingChanged("FormatStyle");
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return !_imageSize.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = new Size(0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageSize()
		{
			if (_largeImageSize.Width == 24)
			{
				return _largeImageSize.Height != 24;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageSize()
		{
			LargeImageSize = new Size(24, 24);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			PropertyValue propertyValue = reader["TabStateStyles"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabStateStyles);
			}
			else
			{
				_tabStateStyles.Reset();
			}
			propertyValue = reader["FormatStyleInnerContainer"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleInnerContainer);
			}
			else
			{
				_formatStyleInnerContainer.Reset();
			}
			propertyValue = reader["FormatStyleCaption"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleCaption);
			}
			else
			{
				_formatStyleCaption.Reset();
			}
			propertyValue = reader["FormatStyleDarkCaption"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleDarkCaption);
			}
			else
			{
				_formatStyleDarkCaption.Reset();
			}
			propertyValue = reader["FormatStyleLightCaption"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleLightCaption);
			}
			else
			{
				_formatStyleLightCaption.Reset();
			}
			propertyValue = reader["FormatStyleActiveCaption"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleActiveCaption);
			}
			else
			{
				_formatStyleActiveCaption.Reset();
			}
			propertyValue = reader["FormatStyleInfoText"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleInfoText);
			}
			else
			{
				_formatStyleInfoText.Reset();
			}
			_activeCaptionMode = (ActiveCaptionMode)LayoutReader.GetPropertyValue(reader, "ActiveCaptionMode", typeof(ActiveCaptionMode), ActiveCaptionMode.PanelFocus);
			_autoHideTabDisplay = (TabDisplayMode)LayoutReader.GetPropertyValue(reader, "AutoHideTabDisplay", typeof(TabDisplayMode), TabDisplayMode.ImageAndTextOnSelected);
			_tabDisplay = (TabDisplayMode)LayoutReader.GetPropertyValue(reader, "TabDisplay", typeof(TabDisplayMode), TabDisplayMode.ImageAndText);
			_tabAlignment = (TabAlignment)LayoutReader.GetPropertyValue(reader, "TabAlignment", typeof(TabAlignment), TabAlignment.Bottom);
			// Old layouts store UseActiveCaptionStyle; it applies only when ActiveCaptionMode is absent.
			if (reader["UseActiveCaptionStyle"] != null && reader["ActiveCaptionMode"] == null)
			{
				_useActiveCaptionStyle = (bool)LayoutReader.GetPropertyValue(reader, "UseActiveCaptionStyle", typeof(bool), true);
				_activeCaptionMode = (_useActiveCaptionStyle ? ActiveCaptionMode.PanelFocus : ActiveCaptionMode.Never);
			}
			_captionDoubleClickAction = (CaptionDoubleClickAction)LayoutReader.GetPropertyValue(reader, "CaptionDoubleClickAction", typeof(CaptionDoubleClickAction), CaptionDoubleClickAction.ToggleDockState);
			_innerAreaStyle = (PanelInnerAreaStyle)LayoutReader.GetPropertyValue(reader, "InnerAreaStyle", typeof(PanelInnerAreaStyle), PanelInnerAreaStyle.UseFormatStyle);
			_borderCaptionColor = (Color)LayoutReader.GetPropertyValue(reader, "BorderCaptionColor", typeof(Color), Color.Empty);
			_borderPanelColor = (Color)LayoutReader.GetPropertyValue(reader, "BorderPanelColor", typeof(Color), Color.Empty);
			_borderCaption = (bool)LayoutReader.GetPropertyValue(reader, "BorderCaption", typeof(bool), true);
			_borderPanel = (bool)LayoutReader.GetPropertyValue(reader, "BorderPanel", typeof(bool), true);
			_captionVisible = (bool)LayoutReader.GetPropertyValue(reader, "CaptionVisible", typeof(bool), true);
			_minimumSize = (Size)LayoutReader.GetPropertyValue(reader, "MinimumSize", typeof(Size), new Size(40, 40));
			_maximumSize = (Size)LayoutReader.GetPropertyValue(reader, "MaximumSize", typeof(Size), new Size(0, 0));
			_captionHeight = (int)LayoutReader.GetPropertyValue(reader, "CaptionHeight", typeof(int), -1);
			_closeButtonVisible = (bool)LayoutReader.GetPropertyValue(reader, "CloseButtonVisible", typeof(bool), true);
			_autoHideButtonVisible = (bool)LayoutReader.GetPropertyValue(reader, "AutoHideButtonVisible", typeof(bool), true);
			_captionDisplayMode = (PanelCaptionDisplayMode)LayoutReader.GetPropertyValue(reader, "CaptionDisplayMode", typeof(PanelCaptionDisplayMode), PanelCaptionDisplayMode.ImageAndText);
			_captionStyle = (PanelCaptionStyle)LayoutReader.GetPropertyValue(reader, "CaptionStyle", typeof(PanelCaptionStyle), PanelCaptionStyle.Standard);
			_imageSize = (Size)LayoutReader.GetPropertyValue(reader, "ImageSize", typeof(Size), new Size(0, 0));
			_largeImageSize = (Size)LayoutReader.GetPropertyValue(reader, "LargeImageSize", typeof(Size), new Size(24, 24));
			_textAlignment = (PanelTextAlignment)LayoutReader.GetPropertyValue(reader, "TextAlignment", typeof(PanelTextAlignment), PanelTextAlignment.Near);
			_infoTextAlignment = (PanelTextAlignment)LayoutReader.GetPropertyValue(reader, "InfoTextAlignment", typeof(PanelTextAlignment), PanelTextAlignment.Far);
			_imageAlignment = (PanelImageAlignment)LayoutReader.GetPropertyValue(reader, "ImageAlignment", typeof(PanelImageAlignment), PanelImageAlignment.Near);
			_pinButtonImage = (Image)LayoutReader.GetPropertyValue(reader, "PinButtonImage", typeof(Image), null);
			_unpinButtonImage = (Image)LayoutReader.GetPropertyValue(reader, "UnpinButtonImage", typeof(Image), null);
			_closeButtonImage = (Image)LayoutReader.GetPropertyValue(reader, "CloseButtonImage", typeof(Image), null);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!_formatStyleActiveCaption.IsDefaultStyle())
			{
				info.AddValue("FormatStyleActiveCaption", _formatStyleActiveCaption);
			}
			if (_activeCaptionMode != ActiveCaptionMode.PanelFocus)
			{
				info.AddValue("ActiveCaptionMode", _activeCaptionMode);
			}
			if (!_autoHideButtonVisible)
			{
				info.AddValue("AutoHideButtonVisible", _autoHideButtonVisible);
			}
			if (_autoHideTabDisplay != TabDisplayMode.ImageAndTextOnSelected)
			{
				info.AddValue("AutoHideTabDisplay", _autoHideTabDisplay);
			}
			if (!_borderCaption)
			{
				info.AddValue("BorderCaption", _borderCaption);
			}
			if (_borderCaptionColor != Color.Empty)
			{
				info.AddValue("BorderCaptionColor", _borderCaptionColor);
			}
			if (!_borderPanel)
			{
				info.AddValue("BorderPanel", _borderPanel);
			}
			if (_borderPanelColor != Color.Empty)
			{
				info.AddValue("BorderPanelColor", _borderPanelColor);
			}
			if (!_formatStyleDarkCaption.IsDefaultStyle())
			{
				info.AddValue("FormatStyleDarkCaption", _formatStyleDarkCaption);
			}
			if (!_formatStyleInfoText.IsDefaultStyle())
			{
				info.AddValue("FormatStyleInfoText", _formatStyleInfoText);
			}
			if (_captionDisplayMode != PanelCaptionDisplayMode.ImageAndText)
			{
				info.AddValue("CaptionDisplayMode", _captionDisplayMode);
			}
			if (_captionDoubleClickAction != CaptionDoubleClickAction.ToggleDockState)
			{
				info.AddValue("CaptionDoubleClickAction", _captionDoubleClickAction);
			}
			if (!_formatStyleCaption.IsDefaultStyle())
			{
				info.AddValue("FormatStyleCaption", _formatStyleCaption);
			}
			if (!_formatStyleLightCaption.IsDefaultStyle())
			{
				info.AddValue("FormatStyleLightCaption", _formatStyleLightCaption);
			}
			if (!_captionVisible)
			{
				info.AddValue("CaptionVisible", _captionVisible);
			}
			if (!_closeButtonVisible)
			{
				info.AddValue("CloseButtonVisible", _closeButtonVisible);
			}
			if (!_formatStyleInnerContainer.IsDefaultStyle())
			{
				info.AddValue("FormatStyleInnerContainer", _formatStyleInnerContainer);
			}
			if (_minimumSize != new Size(40, 40))
			{
				info.AddValue("MinimumSize", _minimumSize);
			}
			if (_maximumSize != new Size(0, 0))
			{
				info.AddValue("MaximumSize", _maximumSize);
			}
			if (_captionHeight != -1)
			{
				info.AddValue("CaptionHeight", _captionHeight);
			}
			if (_captionStyle != PanelCaptionStyle.Standard)
			{
				info.AddValue("CaptionStyle", _captionStyle);
			}
			if (_innerAreaStyle != PanelInnerAreaStyle.UseFormatStyle)
			{
				info.AddValue("InnerAreaStyle", _innerAreaStyle);
			}
			if (_tabAlignment != TabAlignment.Bottom)
			{
				info.AddValue("TabAlignment", _tabAlignment);
			}
			if (_tabDisplay != TabDisplayMode.ImageAndText)
			{
				info.AddValue("TabDisplay", _tabDisplay);
			}
			if (_tabStateStyles.ShouldSerialize())
			{
				info.AddValue("TabStateStyles", _tabStateStyles);
			}
			if (!_imageSize.IsEmpty)
			{
				info.AddValue("ImageSize", _imageSize);
			}
			if (ShouldSerializeLargeImageSize())
			{
				info.AddValue("LargeImageSize", _largeImageSize);
			}
			if (_textAlignment != PanelTextAlignment.Near)
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_imageAlignment != PanelImageAlignment.Near)
			{
				info.AddValue("ImageAlignment", _imageAlignment);
			}
			if (_infoTextAlignment != PanelTextAlignment.Far)
			{
				info.AddValue("InfoTextAlignment", _infoTextAlignment);
			}
			if (_closeButtonImage != null)
			{
				info.AddValue("CloseButtonImage", _closeButtonImage);
			}
			if (_pinButtonImage != null)
			{
				info.AddValue("PinButtonImage", _pinButtonImage);
			}
			if (_unpinButtonImage != null)
			{
				info.AddValue("UnpinButtonImage", _unpinButtonImage);
			}
		}
	}
}
