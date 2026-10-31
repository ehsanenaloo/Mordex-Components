using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	[DesignerSerializer("Mordex.Windows.Navigation.Design.NavigationItemSerializer, Mordex.Windows.Navigation.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[DefaultProperty("Text")]
	[TypeConverter(typeof(NavigationItemConverter))]
	public class NavigationItem : INavigationBarOwner, IMordexXmlLayoutsSupport
	{
		private Cursor _cursor;

		private AccessibleObject _accessibleObject;

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole = AccessibleRole.Default;

		private string _key = string.Empty;

		private object _tag;

		private string _tagString = "";

		internal int _fullWidth;

		internal NavigationItemCollection _owner;

		private string _text = string.Empty;

		private string _toolTipText = "";

		internal int _index;

		private bool _enabled = true;

		private bool _visible = true;

		private Alignment _imageAlignment;

		private Alignment _textAlignment;

		private Rectangle _imageBounds = Rectangle.Empty;

		private int _imageX;

		private int _imageY;

		private Rectangle _textBounds = Rectangle.Empty;

		private int _textX;

		private int _textY;

		private int _textWidth;

		private int _textHeight;

		private Rectangle _bounds;

		private int _boundsX;

		private int _boundsY;

		private int _boundsWidth;

		private Icon _icon;

		private Image _image;

		private int _largeImageIndex = -1;

		private string _largeImageKey = "";

		private int _largeImageDisabledIndex = -1;

		private string _largeImageDisabledKey = "";

		private int _largeImageFocusIndex = -1;

		private string _largeImageFocusKey = "";

		private int _smallImageDisabledIndex = -1;

		private string _smallImageDisabledKey = "";

		private int _smallImageFocusIndex = -1;

		private string _smallImageFocusKey = "";

		private int _smallImageIndex = -1;

		private string _smallImageKey = "";

		internal ItemState _itemState;

		private bool _selected;

		private NavigationBarStateStyles _stateStyles;

		private NavigationBarFormatStyle _formatStyle;

		private ItemEventHandler _itemEventHandler;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(null)]
		[Description("The cursor that is displayed when the mouse pointer is over the item.")]
		[Localizable(true)]
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

		[DefaultValue(null)]
		[Description("The name that will be reported to accessibility clients.")]
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

		[Description("A string that uniquely identifies this NavigationItem in a collection.")]
		[DefaultValue("")]
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

		[Browsable(false)]
		[DefaultValue(null)]
		[Description("An object that contains data to associate with the NavigationItem.")]
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
		[Description("A string that contains data to associate with the NavigationItem.")]
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

		[Description("The text displayed on the NavigationItem.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value != _text)
				{
					_text = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[Description("The text that appears as a ToolTip for the NavigationItem.")]
		[DefaultValue("")]
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
		public int Index => _index;

		[DefaultValue(true)]
		[Description("Indicates whether the NavigationItem is enabled.")]
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
					RepaintBar();
				}
			}
		}

		[Description("Indicates whether the NavigationItem is visible.")]
		[DefaultValue(true)]
		[Localizable(true)]
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
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(Alignment.Near)]
		[Description("The alignment of the image in the NavigationItem.")]
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

		[Description("The alignment of the text in the NavigationItem.")]
		[Localizable(true)]
		[DefaultValue(Alignment.Near)]
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

		[Browsable(false)]
		public Rectangle Bounds
		{
			get
			{
				if (Group != null && Group.View == NavigationBarView.LargeIcons && NavigationBar != null && NavigationBar.SelectionArea == SelectionArea.Image)
				{
					Rectangle selectionBounds = _bounds;
					selectionBounds.X = GetTextBounds().X;
					selectionBounds.Width = GetTextBounds().Width;
					selectionBounds.Height += GetTextBounds().Bottom - _bounds.Bottom;
					return selectionBounds;
				}
				return _bounds;
			}
		}

		[DefaultValue(null)]
		[Description("The icon that is displayed on the NavigationItem.")]
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
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Description("The image that is displayed on the NavigationItem.")]
		[Localizable(true)]
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

		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed by the NavigationItem when it is in large icons view.")]
		[DefaultValue(-1)]
		public int LargeImageIndex
		{
			get
			{
				return _largeImageIndex;
			}
			set
			{
				if (_largeImageIndex != value)
				{
					_largeImageIndex = value;
					_largeImageKey = "";
					InvalidateLayout();
				}
			}
		}

		[Description("Gets or sets the key accessor for the image in the ImageList displayed by the NavigationItem when it is in large icons view.")]
		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		public string LargeImageKey
		{
			get
			{
				return _largeImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeImageKey != value)
				{
					_largeImageKey = value;
					_largeImageIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The image list index value of the image displayed by the NavigationItem when it is in large icons view and disabled.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		public int LargeImageDisabledIndex
		{
			get
			{
				return _largeImageDisabledIndex;
			}
			set
			{
				if (_largeImageDisabledIndex != value)
				{
					_largeImageDisabledIndex = value;
					_largeImageDisabledKey = "";
					InvalidateLayout();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the NavigationItem when it is in large icons view and disabled.")]
		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		public string LargeImageDisabledKey
		{
			get
			{
				return _largeImageDisabledKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeImageDisabledKey != value)
				{
					_largeImageDisabledKey = value;
					_largeImageDisabledIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The image list index value of the image displayed by the NavigationItem when it is in large icons view and the mouse is over it.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		public int LargeImageFocusIndex
		{
			get
			{
				return _largeImageFocusIndex;
			}
			set
			{
				if (_largeImageFocusIndex != value)
				{
					_largeImageFocusIndex = value;
					_largeImageFocusKey = "";
					InvalidateLayout();
				}
			}
		}

		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the NavigationItem when it is in large icons view and the mouse is over it.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		public string LargeImageFocusKey
		{
			get
			{
				return _largeImageFocusKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeImageFocusKey != value)
				{
					_largeImageFocusKey = value;
					_largeImageFocusIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed by the NavigationItem when it is in small icons view and disabled.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		[DefaultValue(-1)]
		public int SmallImageDisabledIndex
		{
			get
			{
				return _smallImageDisabledIndex;
			}
			set
			{
				if (_smallImageDisabledIndex != value)
				{
					_smallImageDisabledIndex = value;
					_smallImageDisabledKey = "";
					InvalidateLayout();
				}
			}
		}

		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		[Description("The key accessor for the image in the ImageList displayed by the NavigationItem when it is in small icons view and disabled.")]
		public string SmallImageDisabledKey
		{
			get
			{
				return _smallImageDisabledKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_smallImageDisabledKey != value)
				{
					_smallImageDisabledKey = value;
					_smallImageFocusIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed by the NavigationItem when it is in small icons view and the mouse is over it.")]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		public int SmallImageFocusIndex
		{
			get
			{
				return _smallImageFocusIndex;
			}
			set
			{
				if (_smallImageFocusIndex != value)
				{
					_smallImageFocusIndex = value;
					_smallImageFocusKey = "";
					InvalidateLayout();
				}
			}
		}

		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the NavigationItem when it is in small icons view and the mouse is over it.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		public string SmallImageFocusKey
		{
			get
			{
				return _smallImageFocusKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_smallImageFocusKey != value)
				{
					_smallImageFocusKey = value;
					_smallImageFocusIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Description("The image list index value of the image displayed by the NavigationItem when it is in small view.")]
		[Editor(typeof(SmallImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageIndexConverter))]
		public int SmallImageIndex
		{
			get
			{
				return _smallImageIndex;
			}
			set
			{
				if (_smallImageIndex != value)
				{
					_smallImageIndex = value;
					_smallImageKey = "";
					InvalidateLayout();
				}
			}
		}

		[Description("The key accessor for the image in the ImageList displayed by the NavigationItem when it is in small view.")]
		[TypeConverter(typeof(Mordex.Windows.Navigation.Design.ImageKeyConverter))]
		[Editor(typeof(SmallImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string SmallImageKey
		{
			get
			{
				return _smallImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_smallImageKey != value)
				{
					_smallImageKey = value;
					_smallImageIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		public bool Selected => _selected;

		[Browsable(false)]
		public MxNavigationBar NavigationBar
		{
			get
			{
				if (_owner != null)
				{
					return _owner.GetGroup().NavigationBar;
				}
				return null;
			}
		}

		[Browsable(false)]
		public NavigationGroup Group
		{
			get
			{
				if (_owner != null)
				{
					return _owner.GetGroup();
				}
				return null;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the item in its different states.")]
		public NavigationBarStateStyles StateStyles => _stateStyles;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The NavigationBarFormatStyle object used to draw this item in the NavigationBar control.")]
		[Browsable(false)]
		[Obsolete("'FormatStyle' is obsolete: This property has been deprecated. Use the StateStyles.FormatStyle property instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
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

		[Description("Occurs when the NavigationItem is clicked.")]
		public event ItemEventHandler Click
		{
			add
			{
				ItemEventHandler itemEventHandler = _itemEventHandler;
				ItemEventHandler snapshot;
				do
				{
					snapshot = itemEventHandler;
					ItemEventHandler combined = (ItemEventHandler)Delegate.Combine(snapshot, value);
					itemEventHandler = Interlocked.CompareExchange(ref _itemEventHandler, combined, snapshot);
				}
				while ((object)itemEventHandler != snapshot);
			}
			remove
			{
				ItemEventHandler itemEventHandler = _itemEventHandler;
				ItemEventHandler snapshot;
				do
				{
					snapshot = itemEventHandler;
					ItemEventHandler remaining = (ItemEventHandler)Delegate.Remove(snapshot, value);
					itemEventHandler = Interlocked.CompareExchange(ref _itemEventHandler, remaining, snapshot);
				}
				while ((object)itemEventHandler != snapshot);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_cursor != null)
			{
				info.AddValue("Cursor", _cursor);
			}
			if (!_enabled)
			{
				info.AddValue("Enabled", _enabled);
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
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (_largeImageDisabledIndex != -1)
			{
				info.AddValue("LargeImageDisabledIndex", _largeImageDisabledIndex);
			}
			if (_largeImageDisabledKey.Length > 0)
			{
				info.AddValue("LargeImageDisabledKey", _largeImageDisabledKey);
			}
			if (_largeImageFocusIndex != -1)
			{
				info.AddValue("LargeImageFocusIndex", _largeImageFocusIndex);
			}
			if (_largeImageFocusKey.Length > 0)
			{
				info.AddValue("LargeImageFocusKey", _largeImageFocusKey);
			}
			if (_largeImageIndex != -1)
			{
				info.AddValue("LargeImageIndex", _largeImageIndex);
			}
			if (_largeImageKey.Length > 0)
			{
				info.AddValue("LargeImageKey", _largeImageKey);
			}
			if (_selected)
			{
				info.AddValue("Selected", _selected);
			}
			if (_stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (_smallImageDisabledIndex != -1)
			{
				info.AddValue("SmallImageDisabledIndex", _smallImageDisabledIndex);
			}
			if (_smallImageDisabledKey.Length > 0)
			{
				info.AddValue("SmallImageDisabledKey", _smallImageDisabledKey);
			}
			if (_smallImageFocusIndex != -1)
			{
				info.AddValue("SmallImageFocusIndex", _smallImageFocusIndex);
			}
			if (_smallImageFocusKey.Length > 0)
			{
				info.AddValue("SmallImageFocusKey", _smallImageFocusKey);
			}
			if (_smallImageIndex != -1)
			{
				info.AddValue("SmallImageIndex", _smallImageIndex);
			}
			if (_smallImageKey.Length > 0)
			{
				info.AddValue("SmallImageKey", _smallImageKey);
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
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_cursor = (Cursor)LayoutReader.GetPropertyValue(info, "Cursor", typeof(Cursor), null);
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			_icon = (Icon)LayoutReader.GetPropertyValue(info, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_imageAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "ImageAlignment", typeof(Alignment), Alignment.Near);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_largeImageDisabledIndex = (int)LayoutReader.GetPropertyValue(info, "LargeImageDisabledIndex", typeof(int), -1);
			_largeImageDisabledKey = (string)LayoutReader.GetPropertyValue(info, "LargeImageDisabledKey", typeof(string), "");
			_largeImageFocusIndex = (int)LayoutReader.GetPropertyValue(info, "LargeImageFocusIndex", typeof(int), -1);
			_largeImageFocusKey = (string)LayoutReader.GetPropertyValue(info, "LargeImageFocusKey", typeof(string), "");
			_largeImageIndex = (int)LayoutReader.GetPropertyValue(info, "LargeImageIndex", typeof(int), -1);
			_largeImageKey = (string)LayoutReader.GetPropertyValue(info, "LargeImageKey", typeof(string), "");
			_selected = (bool)LayoutReader.GetPropertyValue(info, "Selected", typeof(bool), false);
			_stateStyles.Reset();
			info.DeserializeObject("StateStyles", _stateStyles);
			info["FormatStyle"]?.DeserializeObject(_stateStyles.FormatStyle);
			_smallImageDisabledIndex = (int)LayoutReader.GetPropertyValue(info, "SmallImageDisabledIndex", typeof(int), -1);
			_smallImageDisabledKey = (string)LayoutReader.GetPropertyValue(info, "SmallImageDisabledKey", typeof(string), "");
			_smallImageFocusIndex = (int)LayoutReader.GetPropertyValue(info, "SmallImageFocusIndex", typeof(int), -1);
			_smallImageFocusKey = (string)LayoutReader.GetPropertyValue(info, "SmallImageFocusKey", typeof(string), "");
			_smallImageIndex = (int)LayoutReader.GetPropertyValue(info, "SmallImageIndex", typeof(int), -1);
			_smallImageKey = (string)LayoutReader.GetPropertyValue(info, "SmallImageKey", typeof(string), "");
			string tagText = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			_tag = LayoutTypePolicy.ConvertOrKeepText(tagText, (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null));
			_tagString = (string)LayoutReader.GetPropertyValue(info, "TagString", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), "");
			_textAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "TextAlignment", typeof(Alignment), Alignment.Near);
			_toolTipText = (string)LayoutReader.GetPropertyValue(info, "ToolTipText", typeof(string), "");
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
		}

		public NavigationItem()
			: this("")
		{
		}

		public NavigationItem(string text)
			: this(text, -1)
		{
		}

		public NavigationItem(string text, int smallImageIndex)
		{
			_text = text;
			_textAlignment = Alignment.Near;
			_smallImageIndex = smallImageIndex;
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_stateStyles = new NavigationBarStateStyles(this, FormatStyle, null, null);
#pragma warning restore CS0618
		}

		internal NavigationItem CloneDetached()
		{
			NavigationItem navigationBarItem = (NavigationItem)MemberwiseClone();
			navigationBarItem._owner = null;
			return navigationBarItem;
		}

		public void InvokeOnClick()
		{
			OnClick(new ItemEventArgs(this));
			if (NavigationBar != null)
			{
				NavigationBar.OnItemClickCore(this);
			}
		}

		public NavigationItem(string text, int smallImageIndex, int largeImageIndex)
		{
			_text = text;
			TextAlignment = Alignment.Near;
			_smallImageIndex = smallImageIndex;
			_largeImageIndex = largeImageIndex;
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_stateStyles = new NavigationBarStateStyles(this, FormatStyle, null, null);
#pragma warning restore CS0618
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new NavigationItemAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal AccessibleStates ComputeAccessibleStates()
		{
			AccessibleStates accessibleStates = AccessibleStates.Selectable;
			if (!Visible)
			{
				accessibleStates |= AccessibleStates.Invisible;
			}
			else if (!Enabled)
			{
				accessibleStates |= AccessibleStates.Unavailable;
			}
			else
			{
				accessibleStates |= AccessibleStates.Focusable;
				if (_itemState == ItemState.Focus || _itemState == ItemState.SelectedFocus)
				{
					accessibleStates |= AccessibleStates.HotTracked;
				}
				if (Selected && NavigationBar != null && NavigationBar.KeepSelection)
				{
					accessibleStates |= AccessibleStates.Checked;
				}
			}
			return accessibleStates;
		}

		internal AccessibleRole GetAccessibleRole()
		{
			if (AccessibleRole != AccessibleRole.Default)
			{
				return AccessibleRole;
			}
			return AccessibleRole.OutlineItem;
		}

		internal Rectangle GetImageBounds()
		{
			return _imageBounds;
		}

		internal void UpdateImageBounds(Rectangle imageBounds)
		{
			if (_imageBounds != imageBounds)
			{
				_imageBounds = imageBounds;
				SyncImageOrigin();
			}
		}

		private void SyncImageOrigin()
		{
			_imageX = _imageBounds.Left;
			_imageY = _imageBounds.Top;
		}

		internal int GetImageX()
		{
			return _imageX;
		}

		internal void UpdateImageX(int imageX)
		{
			if (_imageX != imageX)
			{
				_imageX = imageX;
				_imageBounds.X = _imageX;
			}
		}

		internal void UpdateImageY(int imageY)
		{
			if (_imageY != imageY)
			{
				_imageY = imageY;
				_imageBounds.Y = _imageY;
			}
		}

		internal Rectangle GetTextBounds()
		{
			return _textBounds;
		}

		internal void SetTextBounds(Rectangle textBounds)
		{
			if (_textBounds != textBounds)
			{
				_textBounds = textBounds;
				CacheTextBoundsParts();
			}
		}

		private void CacheTextBoundsParts()
		{
			_textX = _textBounds.X;
			_textY = _textBounds.Y;
			_textWidth = _textBounds.Width;
			_textHeight = _textBounds.Height;
		}

		internal int GetTextX()
		{
			return _textX;
		}

		internal void UpdateTextX(int textX)
		{
			if (_textX != textX)
			{
				_textX = textX;
				_textBounds.X = _textX;
			}
		}

		internal void UpdateTextY(int textY)
		{
			if (_textY != textY)
			{
				_textY = textY;
				_textBounds.Y = _textY;
			}
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void UpdateBounds(Rectangle bounds)
		{
			if (_bounds != bounds)
			{
				_bounds = bounds;
				CacheBoundsParts();
			}
		}

		internal int GetItemBottom()
		{
			if (Group != null)
			{
				if (Group.View == NavigationBarView.LargeIcons)
				{
					return _textBounds.Bottom;
				}
				return _bounds.Bottom;
			}
			return 0;
		}

		private void CacheBoundsParts()
		{
			_boundsX = _bounds.X;
			_boundsY = _bounds.Y;
			_boundsWidth = _bounds.Width;
		}

		internal int GetBoundsX()
		{
			return _boundsX;
		}

		internal void UpdateBoundsX(int boundsX)
		{
			if (_boundsX != boundsX)
			{
				_boundsX = boundsX;
				_bounds.X = _boundsX;
			}
		}

		internal int GetBoundsY()
		{
			return _boundsY;
		}

		internal void UpdateBoundsY(int boundsY)
		{
			if (_boundsY != boundsY)
			{
				_boundsY = boundsY;
				_bounds.Y = _boundsY;
			}
		}

		internal void UpdateBoundsWidth(int boundsWidth)
		{
			if (_boundsWidth != boundsWidth)
			{
				_boundsWidth = boundsWidth;
				_bounds.Width = _boundsWidth;
			}
		}

		internal void UpdateSelected(bool selected)
		{
			if (selected != _selected)
			{
				if (_owner == null)
				{
					throw new InvalidOperationException();
				}
				_selected = selected;
			}
		}

		private void RepaintBar()
		{
			if (_owner != null && _owner.GetGroup().Selected)
			{
				_owner.RepaintBar();
			}
		}

		private void InvalidateLayout()
		{
			if (_owner != null && _owner.GetGroup().Selected)
			{
				_owner.RelayoutGroup();
			}
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
			if (_formatStyle.ShouldSerialize())
			{
				_formatStyle.Reset();
				if (NavigationBar != null)
				{
					NavigationBar.InvalidateItemLayout();
				}
			}
		}

		internal void ResetStateStyles()
		{
			if (_stateStyles != null && _stateStyles.ShouldSerialize())
			{
				_stateStyles.Reset();
				if (NavigationBar != null)
				{
					NavigationBar.InvalidateItemLayout();
				}
			}
		}

		protected virtual void OnClick(ItemEventArgs e)
		{
			if (_itemEventHandler != null)
			{
				_itemEventHandler(this, e);
			}
		}
	}
}
