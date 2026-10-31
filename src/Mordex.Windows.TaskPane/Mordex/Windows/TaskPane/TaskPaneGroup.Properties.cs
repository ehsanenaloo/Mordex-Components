using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class TaskPaneGroup
	{
		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(1)]
		[Description("Indicates the number of columns in the TaskPane control that the group spans.")]
		public int ColumnSpan
		{
			get
			{
				return _columnSpan;
			}
			set
			{
				if (_columnSpan != value)
				{
					if (value < 1)
					{
						throw new InvalidOperationException();
					}
					_columnSpan = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("The role that will be reported to accessibility clients.")]
		[DefaultValue(AccessibleRole.Default)]
		public AccessibleRole AccessibleRole
		{
			get
			{
				return _accessibleRole;
			}
			set
			{
				if (_accessibleRole != value)
				{
					_accessibleRole = value;
				}
			}
		}

		[Description("The name that will be reported to accessibility clients.")]
		[DefaultValue(null)]
		public string AccessibleName
		{
			get
			{
				return _accessibleName;
			}
			set
			{
				if (_accessibleName != value)
				{
					_accessibleName = value;
				}
			}
		}

		[Description("The description that will be reported to accessibility clients.")]
		[DefaultValue(null)]
		public string AccessibleDescription
		{
			get
			{
				return _accessibleDescription;
			}
			set
			{
				if (_accessibleDescription != value)
				{
					_accessibleDescription = value;
				}
			}
		}

		[DefaultValue(false)]
		public bool Container
		{
			get
			{
				return _container;
			}
			set
			{
				if (_container != value)
				{
					_container = value;
					if (_owner != null)
					{
						_owner.SyncGroupContainer(this);
					}
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The height of the header of the TaskPaneGroup in pixels.")]
		[Localizable(true)]
		public int HeaderHeight
		{
			get
			{
				return _headerHeight;
			}
			set
			{
				if (_headerHeight != value)
				{
					_headerHeight = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Localizable(true)]
		[Description("The height of the TaskPaneContainerControl in pixels.")]
		[DefaultValue(-1)]
		public int ContainerHeight
		{
			get
			{
				return _containerHeight;
			}
			set
			{
				if (_containerHeight != value)
				{
					_containerHeight = value;
					if (_container)
					{
						InvalidateOwnerLayout();
					}
				}
			}
		}

		[DefaultValue(null)]
		public TaskPaneContainerControl ContainerControl
		{
			get
			{
				return _containerControl;
			}
			set
			{
				if (value != _containerControl)
				{
					if (_container && value == null)
					{
						Container = false;
					}
					_containerControl = value;
					if (_owner != null && _containerControl != null)
					{
						_containerControl._owner = _owner.GetOwnerTaskPane();
						_containerControl._group = this;
					}
					if (_containerControl != null)
					{
						_containerControl._group = this;
					}
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		[Description("An object that contains data to associate with the TaskPaneGroup.")]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[Description("The icon that is displayed on the TaskPaneGroup.")]
		[DefaultValue(null)]
		[Localizable(true)]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue("")]
		[Description("A string that contains data to associate with the TaskPaneGroup.")]
		public string TagString
		{
			get
			{
				return _tagString;
			}
			set
			{
				_tagString = value;
			}
		}

		[Browsable(false)]
		public int Index => _index;

		[DefaultValue("")]
		[Description("A string that uniquely identifies this TaskPaneGroup in a collection.")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[Localizable(true)]
		[DefaultValue(Alignment.Near)]
		[Description("The alignment of the text in the TaskPaneGroup.")]
		public Alignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Alignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_textAlignment != value)
				{
					_textAlignment = value;
					InvalidateTaskPane();
				}
			}
		}

		[Description("The alignment of the image in the TaskPaneGroup.")]
		[Localizable(true)]
		[DefaultValue(Alignment.Near)]
		public Alignment ImageAlignment
		{
			get
			{
				return _imageAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Alignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_imageAlignment != value)
				{
					_imageAlignment = value;
					InvalidateTaskPane();
				}
			}
		}

		[Description("Indicates whether the TaskPaneGroup is enabled.")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					if (_containerControl != null)
					{
						_containerControl.Enabled = value;
					}
					InvalidateTaskPane();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(true)]
		[Description("A value indicating whether the TaskPaneGroup is visble.")]
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					_visible = value;
					if (_containerControl != null)
					{
						_containerControl.Visible = value;
					}
					InvalidateOwnerLayout();
				}
			}
		}

		[Browsable(false)]
		public Rectangle HeaderBounds => _headerBounds;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Localizable(true)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("Indicates the default alignment of the expand and close buttons on the TaskPaneGroup.")]
		[Obsolete("'ButtonAlignment' is obsolete: This property has been deprecated. Use the CloseButtonAlignment and ExpandButtonAlignment properties instead")]
		public ButtonAlignment ButtonAlignment
		{
			get
			{
				return _buttonAlignment;
			}
			set
			{
				if (_buttonAlignment != value)
				{
					_buttonAlignment = value;
					LayoutHeaderButtons();
					InvalidateTaskPane();
				}
			}
		}

		[Localizable(true)]
		[Description("Indicates the default alignment of the close button on the TaskPaneGroup.")]
		[DefaultValue(ButtonAlignment.Default)]
		public ButtonAlignment CloseButtonAlignment
		{
			get
			{
				return _closeButtonAlignment;
			}
			set
			{
				if (_closeButtonAlignment != value)
				{
					_closeButtonAlignment = value;
					if (_showCloseButton)
					{
						LayoutHeaderButtons();
						InvalidateTaskPane();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("Indicates the default alignment of the expand button on the TaskPaneGroup.")]
		[DefaultValue(ButtonAlignment.Default)]
		public ButtonAlignment ExpandButtonAlignment
		{
			get
			{
				return _expandButtonAlignment;
			}
			set
			{
				if (_expandButtonAlignment != value)
				{
					_expandButtonAlignment = value;
					if (_expandable)
					{
						LayoutHeaderButtons();
						InvalidateTaskPane();
					}
				}
			}
		}

		[Description("The cursor that is displayed when the mouse pointer is over the group.")]
		[Localizable(true)]
		[DefaultValue(null)]
		public Cursor Cursor
		{
			get
			{
				return _cursor;
			}
			set
			{
				_cursor = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the expand button is displayed on the group.")]
		public bool Expandable
		{
			get
			{
				return _expandable;
			}
			set
			{
				if (_expandable != value)
				{
					_expandable = value;
					_headerBounds = Rectangle.Empty;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Represents the image list index value of the image displayed instead of the default collapsed button image.")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		public int CollapsedButtonImageIndex
		{
			get
			{
				return _collapsedButtonImageIndex;
			}
			set
			{
				if (_collapsedButtonImageIndex != value)
				{
					_collapsedButtonImageIndex = value;
					_collapsedButtonImageKey = "";
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("The key accessor for the image in the ImageList displayed instead of the default collapsed button image.")]
		public string CollapsedButtonImageKey
		{
			get
			{
				return _collapsedButtonImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_collapsedButtonImageKey != value)
				{
					_collapsedButtonImageKey = value;
					_collapsedButtonImageIndex = -1;
					InvalidateTaskPane();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Description("Represents the image list index value of the image displayed instead of the default expand button image.")]
		[DefaultValue(-1)]
		public int ExpandButtonImageIndex
		{
			get
			{
				return _expandButtonImageIndex;
			}
			set
			{
				if (_expandButtonImageIndex != value)
				{
					_expandButtonImageIndex = value;
					_expandButtonImageKey = "";
					InvalidateTaskPane();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[Description("Represents the key accessor for the image in the ImageList displayed instead of the default expand button image.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string ExpandButtonImageKey
		{
			get
			{
				return _expandButtonImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_expandButtonImageKey != value)
				{
					_expandButtonImageKey = value;
					_expandButtonImageIndex = -1;
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the close button is displayed on the group.")]
		public bool ShowCloseButton
		{
			get
			{
				return _showCloseButton;
			}
			set
			{
				if (_showCloseButton != value)
				{
					_showCloseButton = value;
					_headerBounds = Rectangle.Empty;
					InvalidateOwnerLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[Description("A collection containing all the items in this group.")]
		public TaskPaneItemCollection Items => _items;

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The image displayed on the TaskPaneGroup.")]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The index value of the image displayed by the TaskPaneGroup when the mouse moves over it.")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		public int FocusImageIndex
		{
			get
			{
				return _focusImageIndex;
			}
			set
			{
				if (_focusImageIndex != value)
				{
					_focusImageIndex = value;
					_focusImageKey = "";
					InvalidateOwnerLayout();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneGroup when the mouse moves over it.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		public string FocusImageKey
		{
			get
			{
				return _focusImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_focusImageKey != value)
				{
					_focusImageKey = value;
					_focusImageIndex = -1;
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The index value of the image displayed by the TaskPaneGroup when it is disabled.")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneGroup when it is disabled.")]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("The index value of the image displayed by the TaskPaneGroup.")]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneGroup.")]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether a border is drawn around the group.")]
		public TriState ShowGroupBorder
		{
			get
			{
				return _showGroupBorder;
			}
			set
			{
				if (_showGroupBorder != value)
				{
					_showGroupBorder = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("Indicates how to draw a watermark image in the background.")]
		public TaskPaneWatermarkImage WatermarkImage
		{
			get
			{
				if (_watermarkImage == null)
				{
					_watermarkImage = new TaskPaneWatermarkImage(this);
				}
				return _watermarkImage;
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		[Description("The text displayed on the TaskPaneGroup.")]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (_text != value)
				{
					_text = value;
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("The text that appears as a ToolTip for the TaskPaneGroup.")]
		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				if (_toolTipText != value)
				{
					_toolTipText = value;
				}
			}
		}

		[Description("Indicates whether the group should be displayed as a special group.")]
		[DefaultValue(false)]
		public bool SpecialGroup
		{
			get
			{
				return _specialGroup;
			}
			set
			{
				if (_specialGroup != value)
				{
					_specialGroup = value;
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The background image displayed in the background of the group.")]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image BackgroundImage
		{
			get
			{
				return _backgroundImage;
			}
			set
			{
				if (_backgroundImage != value)
				{
					_backgroundImage = value;
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue(ImageDrawMode.Stretched)]
		[Description("Specifies how the background image is draw in the TaskPane.")]
		public ImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				if (_backgroundImageDrawMode != value)
				{
					_backgroundImageDrawMode = value;
					InvalidateTaskPane();
				}
			}
		}

		[Browsable(false)]
		public MxTaskPane TaskPane
		{
			get
			{
				if (_owner != null)
				{
					return _owner.GetOwnerTaskPane();
				}
				return null;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'FormatStyle' is obsolete: This property has been deprecated. Use the StateStyles.FormatStyle property instead")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TaskPaneFormatStyle used to draw this TaskPaneGroup in the TaskPane control.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new TaskPaneFormatStyle(this);
				}
				return _formatStyle;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the TaskPaneGroup is expanded.")]
		public bool Expanded
		{
			get
			{
				return _expanded;
			}
			set
			{
				if (_expanded != value)
				{
					if (_owner != null)
					{
						_owner.ToggleGroup(this);
					}
					else
					{
						_expanded = value;
					}
				}
			}
		}

		[Description("Indicates whether the header section is displayed in the TaskPane control.")]
		[DefaultValue(true)]
		public bool ShowGroupCaption
		{
			get
			{
				return _showGroupCaption;
			}
			set
			{
				if (_showGroupCaption != value)
				{
					_showGroupCaption = value;
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Represents the object that holds the format styles used to draw the items whitin this group in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TaskPaneStateStyles ItemsStateStyles => _itemsStateStyles;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the header in its different states.")]
		public TaskPaneStateStyles StateStyles => _stateStyles;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Obsolete("'ItemsFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.FormatStyle property instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The TaskPaneFormatStyle used to draw the TaskPaneItem objects whitin this group.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TaskPaneFormatStyle ItemsFormatStyle
		{
			get
			{
				if (_itemsFormatStyle == null)
				{
					_itemsFormatStyle = new TaskPaneFormatStyle(this);
				}
				return _itemsFormatStyle;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWatermarkImage()
		{
			if (_watermarkImage == null)
			{
				return false;
			}
			return _watermarkImage.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWatermarkImage()
		{
			WatermarkImage.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			if (_formatStyle != null)
			{
				_formatStyle._owner = null;
			}
			_formatStyle = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle != null)
			{
				return !_formatStyle.IsDefault();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsFormatStyle()
		{
			if (_itemsFormatStyle != null)
			{
				_itemsFormatStyle._owner = null;
			}
			_itemsFormatStyle = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsFormatStyle()
		{
			if (_itemsFormatStyle != null)
			{
				return !_itemsFormatStyle.IsDefault();
			}
			return false;
		}
	}
}
