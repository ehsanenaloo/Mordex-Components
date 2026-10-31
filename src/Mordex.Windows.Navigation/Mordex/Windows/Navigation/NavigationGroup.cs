using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	[TypeConverter(typeof(NavigationGroupConverter))]
	[DefaultProperty("Text")]
	[DesignerSerializer("Mordex.Windows.Navigation.Design.NavigationGroupSerializer, Mordex.Windows.Navigation.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public class NavigationGroup : INavigationBarOwner, IMordexXmlLayoutsSupport
	{
		internal NavigationGroupCollection _owner;

		private Cursor _cursor;

		private object _tag;

		private string _tagString = "";

		internal int _index;

		private AccessibleObject _accessibleObject;

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole = AccessibleRole.Default;

		private string _key = string.Empty;

		private NavigationBarView _view = NavigationBarView.LargeIcons;

		private Alignment _textAlignment;

		private Alignment _imageAlignment;

		private bool _enabled = true;

		private bool _visible = true;

		internal ItemState _itemState;

		private Rectangle _bounds;

		private NavigationItemCollection _items;

		private Icon _icon;

		private Image _image;

		private int _focusImageIndex = -1;

		private string _focusImageKey = "";

		private int _disabledImageIndex = -1;

		private string _disabledImageKey = "";

		private int _imageIndex = -1;

		private string _imageKey = "";

		private string _text = "";

		private string _toolTipText = "";

		private bool _selected;

		private Image _backgroundImage;

		private ImageDrawMode _backgroundImageDrawMode;

		internal bool _container;

		private NavigationBarContainerControl _containerControl;

		private NavigationBarFormatStyle _formatStyle;

		private NavigationBarStateStyles _stateStyles;

		private NavigationBarFormatStyle _itemsFormatStyle;

		private NavigationBarStateStyles _itemsStateStyles;

		private NavigationBarFormatStyle _backgroundFormatStyle;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The cursor that is displayed when the mouse pointer is over the group.")]
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

		[Description("An object that contains data to associate with the NavigationGroup.")]
		[Browsable(false)]
		[DefaultValue(null)]
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

		[DefaultValue("")]
		[Description("A string that contains data to associate with the NavigationGroup.")]
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

		[Description("The position of the NavigationGroup in the NavigationGroupCollection.")]
		[Browsable(false)]
		public int Index => _index;

		[DefaultValue(AccessibleRole.Default)]
		[Description("The role that will be reported to accessibility clients.")]
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

		[DefaultValue(null)]
		[Description("The description that will be reported to accessibility clients.")]
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

		[DefaultValue("")]
		[Description("A string that uniquely identifies this NavigationGroup in a collection.")]
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

		[DefaultValue(NavigationBarView.LargeIcons)]
		[Description("Specifies how NavigationItem objects whitin this group are displayed in the NavigationBar control.")]
		public NavigationBarView View
		{
			get
			{
				return _view;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NavigationBarView), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_view != value)
				{
					_view = value;
					RelayoutIfSelected();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(Alignment.Center)]
		[Description("The alignment of the text in the NavigationGroup.")]
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
					RepaintBar();
				}
			}
		}

		[DefaultValue(Alignment.Near)]
		[Localizable(true)]
		[Description("The alignment of the image in the NavigationGroup.")]
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
					RepaintBar();
				}
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		[Description("Indicates whether the NavigationGroup is enabled.")]
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
					RepaintBar();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(true)]
		[Description("Indicates whether the NavigationGroup is visble.")]
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
					if (_owner != null)
					{
						_owner.GetNavigationBar().OnGroupVisibilityChanged(this, value);
					}
				}
			}
		}

		[Browsable(false)]
		public Rectangle Bounds => _bounds;

		[Browsable(false)]
		public int HeaderHeight => _bounds.Height;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		[Description("A collection containing all the items in this group.")]
		public NavigationItemCollection Items => _items;

		[Description("Gets or sets the icon that is displayed on the NavigationGroup.")]
		[Localizable(true)]
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
					RelayoutIfSelected();
				}
			}
		}

		[Description("The image that is displayed on the NavigationGroup.")]
		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
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
					RelayoutIfSelected();
				}
			}
		}

		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed by the NavigationGroup when the mouse moves over it.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[DefaultValue(-1)]
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
					RelayoutIfSelected();
				}
			}
		}

		[Description("The key accessor for the image in the ImageList displayed by the NavigationGroup when the mouse moves over it.")]
		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
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
					RelayoutIfSelected();
				}
			}
		}

		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Description("The image list index value of the image displayed by the NavigationGroup when it is disabled.")]
		[DefaultValue(-1)]
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
					RelayoutIfSelected();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the NavigationGroup when it is disabled.")]
		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		public string DisabledImageKey
		{
			get
			{
				return _disabledImageKey;
			}
			set
			{
				if (_disabledImageKey == null)
				{
					_disabledImageKey = "";
				}
				if (_disabledImageKey != value)
				{
					_disabledImageKey = value;
					_disabledImageIndex = -1;
					RelayoutIfSelected();
				}
			}
		}

		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Description("The image list index value of the image displayed by the NavigationGroup.")]
		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
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
					RelayoutIfSelected();
				}
			}
		}

		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		[Description("The or sets the key accessor for the image in the ImageList displayed by the NavigationGroup.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
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
					RelayoutIfSelected();
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("The text displayed on the NavigationGroup.")]
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
					RepaintBar();
				}
			}
		}

		[Description("The text that appears as a ToolTip for the NavigationGroup.")]
		[DefaultValue("")]
		[Localizable(true)]
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

		[Browsable(false)]
		[Description("Indicates whether this NavigationGroup is selected.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (value != _selected)
				{
					if (!value || _owner == null)
					{
						throw new InvalidOperationException();
					}
					_owner.GetNavigationBar().SelectedGroup = this;
				}
			}
		}

		[DefaultValue(null)]
		[Description("The background image displayed by the NavigationBar when this NavigationGroup is selected.")]
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
					RepaintBar();
				}
			}
		}

		[Description("Specifies how the background image is draw on the NavigationBar.")]
		[DefaultValue(ImageDrawMode.Stretched)]
		public ImageDrawMode BackGroundImageDrawMode
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
					RepaintBar();
				}
			}
		}

		[Description("Indicates whether the group should act as a container for other controls.")]
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
						_owner.UpdateGroupContainer(this);
					}
				}
			}
		}

		[DefaultValue(null)]
		[Description("The NavigationBarContainerControl associated to a NavigationGroup acting as container for other controls.")]
		public NavigationBarContainerControl ContainerControl
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
						_containerControl._owner = _owner.GetNavigationBar();
						_containerControl._group = this;
					}
					RelayoutIfSelected();
				}
			}
		}

		[Browsable(false)]
		public MxNavigationBar NavigationBar
		{
			get
			{
				if (_owner != null)
				{
					return _owner.GetNavigationBar();
				}
				return null;
			}
		}

		[Obsolete("'FormatStyle' is obsolete: This property has been deprecated. Use the StateStyles.FormatStyle property instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Gets the NavigationBarFormatStyle used to draw this NavigationGroup in the NavigationBar control.")]
		[Browsable(false)]
		public NavigationBarFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new NavigationBarFormatStyle(this);
				}
				return _formatStyle;
			}
		}

		[Description("Represents the object that holds the format styles used to draw the header in its different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarStateStyles StateStyles => _stateStyles;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The NavigationBarFormatStyle used to draw the NavigationItem objects whitin this group.")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'ItemsFormatStyle' is obsolete: This property has been deprecated. Use the ItemsStateStyles.FormatStyle property instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationBarFormatStyle ItemsFormatStyle
		{
			get
			{
				if (_itemsFormatStyle == null)
				{
					_itemsFormatStyle = new NavigationBarFormatStyle(this);
				}
				return _itemsFormatStyle;
			}
		}

		[Description("Represents the object that holds the format styles used to draw the items whitin this group in their different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarStateStyles ItemsStateStyles => _itemsStateStyles;

		[Description("Gets the NavigationBarFormatStyle used to draw the background of this NavigationGroup in the NavigationBar control.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarFormatStyle BackgroundFormatStyle
		{
			get
			{
				if (_backgroundFormatStyle == null)
				{
					_backgroundFormatStyle = new NavigationBarFormatStyle(this);
				}
				return _backgroundFormatStyle;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_backgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (ImageDrawMode)LayoutReader.GetPropertyValue(info, "BackgroundImageDrawMode", typeof(ImageDrawMode), ImageDrawMode.Stretched);
			Container = (bool)LayoutReader.GetPropertyValue(info, "Container", typeof(bool), false);
			_cursor = (Cursor)LayoutReader.GetPropertyValue(info, "Cursor", typeof(Cursor), null);
			_disabledImageIndex = (int)LayoutReader.GetPropertyValue(info, "DisabledImageIndex", typeof(int), -1);
			_disabledImageKey = (string)LayoutReader.GetPropertyValue(info, "DisabledImageKey", typeof(string), "");
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			_focusImageIndex = (int)LayoutReader.GetPropertyValue(info, "FocusImageIndex", typeof(int), -1);
			_focusImageKey = (string)LayoutReader.GetPropertyValue(info, "FocusImageKey", typeof(string), "");
			_itemsStateStyles.Reset();
			_stateStyles.Reset();
			info.DeserializeObject("ItemsStateStyles", _itemsStateStyles);
			info.DeserializeObject("StateStyles", _stateStyles);
			info["FormatStyle"]?.DeserializeObject(_stateStyles.FormatStyle);
			info["ItemsFormatStyle"]?.DeserializeObject(_itemsStateStyles.FormatStyle);
			DeserializeNavigationGroupState(info, "BackgroundFormatStyle", BackgroundFormatStyle);
			_icon = (Icon)LayoutReader.GetPropertyValue(info, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_imageAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "ImageAlignment", typeof(Alignment), Alignment.Near);
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			PropertyValue propertyValue = info["Items"];
			_items = new NavigationItemCollection(this);
			propertyValue?.DeserializeObject(_items);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_selected = (bool)LayoutReader.GetPropertyValue(info, "Selected", typeof(bool), false);
			string tagText = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			_tag = LayoutTypePolicy.ConvertOrKeepText(tagText, (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null));
			_tagString = (string)LayoutReader.GetPropertyValue(info, "TagString", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), "");
			_textAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "TextAlignment", typeof(Alignment), Alignment.Near);
			_toolTipText = (string)LayoutReader.GetPropertyValue(info, "ToolTipText", typeof(string), "");
			_view = (NavigationBarView)LayoutReader.GetPropertyValue(info, "View", typeof(NavigationBarView), NavigationBarView.LargeIcons);
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
		}

		private void DeserializeNavigationGroupState(LayoutReader mordexLayoutReader, string propertyName, NavigationBarFormatStyle targetInstance)
		{
			targetInstance.Reset();
			mordexLayoutReader.DeserializeObject(propertyName, targetInstance);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_backgroundImage != null)
			{
				info.AddValue("BackgroundImage", _backgroundImage);
			}
			if (_backgroundImageDrawMode != ImageDrawMode.Stretched)
			{
				info.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_container)
			{
				info.AddValue("Container", _container);
			}
			if (_cursor != null)
			{
				info.AddValue("Cursor", _cursor);
			}
			if (_disabledImageIndex != -1)
			{
				info.AddValue("DisabledImageIndex", _disabledImageIndex);
			}
			if (_disabledImageKey.Length > 0)
			{
				info.AddValue("DisabledImageKey", _disabledImageKey);
			}
			if (!_enabled)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (_focusImageIndex != -1)
			{
				info.AddValue("FocusImageIndex", _focusImageIndex);
			}
			if (_focusImageKey.Length > 0)
			{
				info.AddValue("FocusImageKey", _focusImageKey);
			}
			if (_icon != null)
			{
				info.AddValue("Icon", _icon);
			}
			if (_image != null)
			{
				info.AddValue("Image", _image);
			}
			if (_imageAlignment != Alignment.Near)
			{
				info.AddValue("ImageAlignment", _imageAlignment);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_items.Count > 0)
			{
				info.AddValue("Items", _items);
			}
			if (_itemsStateStyles.ShouldSerialize())
			{
				info.AddValue("ItemsStateStyles", _itemsStateStyles);
			}
			if (_stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (ShouldSerializeBackgroundFormatStyle())
			{
				info.AddValue("BackgroundFormatStyle", _backgroundFormatStyle);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (_selected)
			{
				info.AddValue("Selected", _selected);
			}
			if (_tag != null)
			{
				if (!(_tag is string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
			if (_tagString != "")
			{
				info.AddValue("TagString", _tagString);
			}
			if (_text != "")
			{
				info.AddValue("Text", _text);
			}
			if (_textAlignment != Alignment.Near)
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_toolTipText != "")
			{
				info.AddValue("ToolTipText", _toolTipText);
			}
			if (_view != NavigationBarView.LargeIcons)
			{
				info.AddValue("View", _view);
			}
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
		}

		public NavigationGroup()
			: this("")
		{
		}

		public NavigationGroup(string text)
		{
			_text = text;
			_items = new NavigationItemCollection(this);
			_textAlignment = Alignment.Center;
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_stateStyles = new NavigationBarStateStyles(this, FormatStyle, null, null);
#pragma warning restore CS0618
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_itemsStateStyles = new NavigationBarStateStyles(this, ItemsFormatStyle, null, null);
#pragma warning restore CS0618
		}

		internal void RelayoutIfSelected()
		{
			if (_owner != null && _owner.GetNavigationBar() != null && this == _owner.GetNavigationBar().SelectedGroup)
			{
				_owner.RelayoutBar();
			}
		}

		internal void InvalidateLayout()
		{
			if (_owner != null)
			{
				_owner.InvalidateItemLayout();
			}
		}

		internal void RepaintBar()
		{
			if (_owner != null)
			{
				_owner.RequestRepaint();
			}
		}

		internal NavigationGroup CloneDetached()
		{
			NavigationGroup navigationBarGroup = (NavigationGroup)MemberwiseClone();
			navigationBarGroup._items = new NavigationItemCollection(navigationBarGroup);
			navigationBarGroup._owner = null;
			navigationBarGroup._containerControl = _containerControl;
			return navigationBarGroup;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new NavigationGroupAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal AccessibleStates ComputeAccessibleStates()
		{
			AccessibleStates accessibleStates = AccessibleStates.Focusable;
			if (Selected)
			{
				return accessibleStates | AccessibleStates.Expanded;
			}
			return accessibleStates | AccessibleStates.Collapsed;
		}

		internal AccessibleRole GetAccessibleRole()
		{
			if (AccessibleRole != AccessibleRole.Default)
			{
				return AccessibleRole;
			}
			return AccessibleRole.OutlineItem;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle bounds)
		{
			_bounds = bounds;
		}

		internal void SetSelected(bool selected)
		{
			_selected = selected;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFormatStyle()
		{
			if (_formatStyle == null)
			{
				return false;
			}
			return _formatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			if (_formatStyle == null)
			{
				_formatStyle = new NavigationBarFormatStyle(this);
			}
			else if (_formatStyle.ShouldSerialize())
			{
				_formatStyle.Reset();
				RelayoutIfSelected();
			}
		}

		internal void ResetStateStyles()
		{
			if (_stateStyles != null && _stateStyles.ShouldSerialize())
			{
				_stateStyles.Reset();
				RelayoutIfSelected();
			}
		}

		internal void ResetItemsStateStyles()
		{
			if (_itemsStateStyles != null && _itemsStateStyles.ShouldSerialize())
			{
				_itemsStateStyles.Reset();
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackgroundFormatStyle()
		{
			if (_backgroundFormatStyle == null)
			{
				return false;
			}
			return _backgroundFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackgroundFormatStyle()
		{
			if (_backgroundFormatStyle == null)
			{
				_backgroundFormatStyle = new NavigationBarFormatStyle(this);
			}
			else if (_backgroundFormatStyle.ShouldSerialize())
			{
				_backgroundFormatStyle.Reset();
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsFormatStyle()
		{
			if (_itemsFormatStyle == null)
			{
				return false;
			}
			return _itemsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsFormatStyle()
		{
			if (_itemsFormatStyle == null)
			{
				_itemsFormatStyle = new NavigationBarFormatStyle(this);
			}
			else if (_itemsFormatStyle.ShouldSerialize())
			{
				_itemsFormatStyle.Reset();
				InvalidateLayout();
			}
		}
	}
}
