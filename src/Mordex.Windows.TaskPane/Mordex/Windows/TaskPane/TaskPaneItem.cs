using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	[DefaultProperty("Text")]
	[TypeConverter(typeof(TaskPaneItemConverter))]
	public class TaskPaneItem : ITaskPaneOwner, IMordexXmlLayoutsSupport
	{
		private AccessibleObject _accessibleObject;

		private string _accessibleName;

		private string _accessibleDescription;

		private AccessibleRole _accessibleRole = AccessibleRole.Default;

		private string _key = string.Empty;

		private Shortcut _shortcut;

		internal bool _selected;

		private int _indent;

		private int _height = -1;

		private Cursor _cursor;

		private object _tag;

		private string _tagString = "";

		private Icon _icon;

		internal TaskPaneItemCollection _owner;

		private string _text = string.Empty;

		private bool _wordWrap = true;

		private bool _effectiveWordWrap = true;

		private string _toolTipText = "";

		internal int _index;

		private bool _enabled = true;

		private bool _visible = true;

		private Alignment _imageAlignment;

		private Alignment _textAlignment;

		private Rectangle _bounds;

		private int _boundsX;

		private int _boundsY;

		private int _boundsWidth;

		private TaskPaneContainerControl _containerControl;

		internal bool _container;

		private int _containerHeight = -1;

		private Image _image;

		private int _imageDisabledIndex = -1;

		private string _imageDisabledKey = "";

		private int _imageSelectedIndex = -1;

		private string _imageSelectedKey = "";

		private int _imageFocusIndex = -1;

		private string _imageFocusKey = "";

		private ItemType _itemType;

		private int _imageIndex = -1;

		private string _imageKey = "";

		internal ItemState _itemState;

		private TaskPaneStateStyles _stateStyles;

		private TaskPaneFormatStyle _formatStyle;

		private ItemEventHandler _itemEventHandler;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

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

		[DefaultValue("")]
		[Description("A string that uniquely identifies this TaskPaneItem in a collection.")]
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

		[Description("Represents the shortcut key associated with the item. ")]
		[DefaultValue(Shortcut.None)]
		public Shortcut Shortcut
		{
			get
			{
				return _shortcut;
			}
			set
			{
				_shortcut = value;
			}
		}

		[DefaultValue(false)]
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected == value)
				{
					return;
				}
				_selected = value;
				if (TaskPane != null)
				{
					if (value)
					{
						TaskPane.SelectedItem = this;
					}
					else
					{
						TaskPane.SelectedItem = null;
					}
				}
			}
		}

		[Description("Indicates the distance to indent the item")]
		[DefaultValue(0)]
		public int Indent
		{
			get
			{
				return _indent;
			}
			set
			{
				if (_indent != value)
				{
					_indent = value;
					InvalidateTaskPane();
				}
			}
		}

		[DefaultValue(-1)]
		[Localizable(true)]
		[Description("The height of the TaskPaneItem in pixels.")]
		public int Height
		{
			get
			{
				return _height;
			}
			set
			{
				if (_height != value)
				{
					_height = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The cursor that is displayed when the mouse pointer is over the item.")]
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

		[DefaultValue(null)]
		[Browsable(false)]
		[Description("Contains data to associate with the TaskPaneItem.")]
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
		[Description("Contains data to associate with the TaskPaneItem.")]
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

		[Localizable(true)]
		[Description("The icon that is displayed on the TaskPaneItem.")]
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

		[Localizable(true)]
		[DefaultValue("")]
		[Description("The text displayed on the TaskPaneItem.")]
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

		[Description("Indicates whether the item automatically wraps words to the beginning of the next line when necessary.")]
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
					InvalidateTaskPane();
				}
			}
		}

		[Localizable(true)]
		[Description("The text that appears as a ToolTip for the TaskPaneItem.")]
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
		[Description("Gets the position of the TaskPaneItem in the TaskPaneItemCollection.")]
		public int Index => _index;

		[Description("Indicates whether the TaskPaneItem is enabled.")]
		[Localizable(true)]
		[DefaultValue(true)]
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
		[Description("Indicates whether the TaskPaneItem is visible.")]
		[DefaultValue(true)]
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

		[DefaultValue(Alignment.Near)]
		[Localizable(true)]
		[Description("The alignment of the image in the TaskPaneItem.")]
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

		[Localizable(true)]
		[DefaultValue(Alignment.Near)]
		[Description("The alignment of the text in the TaskPaneItem.")]
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

		[Browsable(false)]
		public Rectangle Bounds => _bounds;

		[DefaultValue(null)]
		[Description("The TaskPaneContainerControl associated to the item acting as container for other controls.")]
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
						_containerControl._owner = TaskPane;
					}
					if (_containerControl != null)
					{
						_containerControl._item = this;
					}
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the item should act as a container for other controls.")]
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
					if (TaskPane != null)
					{
						TaskPane.SyncItemContainer(this);
					}
				}
			}
		}

		[Description("The height of the TaskPaneContainerControl in pixels.")]
		[Localizable(true)]
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
						InvalidateLayout();
					}
				}
			}
		}

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The image that is displayed on the TaskPaneItem.")]
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

		[Description("The index value of the image displayed by the TaskPaneGroup when it is disabled.")]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		public int ImageDisabledIndex
		{
			get
			{
				return _imageDisabledIndex;
			}
			set
			{
				if (_imageDisabledIndex != value)
				{
					_imageDisabledIndex = value;
					_imageDisabledKey = "";
					InvalidateLayout();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneGroup when it is disabled.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		public string ImageDisabledKey
		{
			get
			{
				return _imageDisabledKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageDisabledKey != value)
				{
					_imageDisabledKey = value;
					_imageDisabledIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(-1)]
		[Description("The index value of the image displayed by the TaskPaneItem when the item is selected.")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		public int ImageSelectedIndex
		{
			get
			{
				return _imageSelectedIndex;
			}
			set
			{
				if (_imageSelectedIndex != value)
				{
					_imageSelectedIndex = value;
					_imageSelectedKey = "";
					InvalidateLayout();
				}
			}
		}

		[DefaultValue("")]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneItem when the item is selected.")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		public string ImageSelectedKey
		{
			get
			{
				return _imageSelectedKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageSelectedKey != value)
				{
					_imageSelectedKey = value;
					_imageSelectedIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Description("The index value of the image displayed by the TaskPaneItem when the mouse moves over it.")]
		public int ImageFocusIndex
		{
			get
			{
				return _imageFocusIndex;
			}
			set
			{
				if (_imageFocusIndex != value)
				{
					_imageFocusIndex = value;
					_imageFocusKey = "";
					InvalidateLayout();
				}
			}
		}

		[DefaultValue("")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneItem when the mouse moves over it.")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		public string ImageFocusKey
		{
			get
			{
				return _imageFocusKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageFocusKey != value)
				{
					_imageFocusKey = value;
					_imageFocusIndex = -1;
					InvalidateLayout();
				}
			}
		}

		[Description("Indicates the type of the item represented by the TaskPaneItem object in the TaskPane control.")]
		[DefaultValue(ItemType.LinkButton)]
		public ItemType ItemType
		{
			get
			{
				return _itemType;
			}
			set
			{
				if (_itemType != value)
				{
					_itemType = value;
					InvalidateLayout();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageIndexConverter))]
		[Description("The index value of the image displayed by the TaskPaneItem.")]
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

		[DefaultValue("")]
		[TypeConverter(typeof(Mordex.Windows.TaskPane.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("The key accessor for the image in the ImageList displayed by the TaskPaneItem.")]
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

		[Browsable(false)]
		public MxTaskPane TaskPane
		{
			get
			{
				if (_owner != null)
				{
					return _owner.GetGroup().TaskPane;
				}
				return null;
			}
		}

		[Browsable(false)]
		public TaskPaneGroup Group
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
		public TaskPaneStateStyles StateStyles => _stateStyles;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Description("The TaskPaneFormatStyle object used to draw this item in the TaskPane control.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("'FormatStyle' is obsolete: This property has been deprecated. Use the StateStyles.FormatStyle property instead")]
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

		[Description("Occurs when the TaskPaneItem is clicked.")]
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
			if (_height != -1)
			{
				info.AddValue("Height", _height);
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
			if (_imageDisabledIndex != -1)
			{
				info.AddValue("ImageDisabledIndex", _imageDisabledIndex);
			}
			if (_imageDisabledKey.Length > 0)
			{
				info.AddValue("ImageDisabledKey", _imageDisabledKey);
			}
			if (_imageFocusIndex != -1)
			{
				info.AddValue("ImageFocusIndex", _imageFocusIndex);
			}
			if (_imageFocusKey.Length > 0)
			{
				info.AddValue("ImageFocusKey", _imageFocusKey);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_imageSelectedIndex != -1)
			{
				info.AddValue("ImageSelectedIndex", _imageSelectedIndex);
			}
			if (_imageSelectedKey.Length > 0)
			{
				info.AddValue("ImageSelectedKey", _imageSelectedKey);
			}
			if (_itemType != ItemType.LinkButton)
			{
				info.AddValue("ItemType", _itemType);
			}
			if (!_wordWrap)
			{
				info.AddValue("WordWrap", _wordWrap);
			}
			if (_stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (_shortcut != Shortcut.None)
			{
				info.AddValue("Shortcut", _shortcut);
			}
			if (_container)
			{
				info.AddValue("Container", _container);
			}
			if (_containerHeight >= 0)
			{
				info.AddValue("ControlHeight", _containerHeight);
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
			if (_indent != 0)
			{
				info.AddValue("Indent", _indent);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_cursor = (Cursor)LayoutReader.GetPropertyValue(info, "Cursor", typeof(Cursor), null);
			_enabled = (bool)LayoutReader.GetPropertyValue(info, "Enabled", typeof(bool), true);
			_height = (int)LayoutReader.GetPropertyValue(info, "Height", typeof(int), -1);
			_icon = (Icon)LayoutReader.GetPropertyValue(info, "Icon", typeof(Icon), null);
			_indent = (int)LayoutReader.GetPropertyValue(info, "Indent", typeof(int), 0);
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_imageAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "ImageAlignment", typeof(Alignment), Alignment.Near);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_imageDisabledIndex = (int)LayoutReader.GetPropertyValue(info, "ImageDisabledIndex", typeof(int), -1);
			_imageDisabledKey = (string)LayoutReader.GetPropertyValue(info, "ImageDisabledKey", typeof(string), "");
			_imageSelectedIndex = (int)LayoutReader.GetPropertyValue(info, "ImageSelectedIndex", typeof(int), -1);
			_imageSelectedKey = (string)LayoutReader.GetPropertyValue(info, "ImageSelectedKey", typeof(string), "");
			_imageFocusIndex = (int)LayoutReader.GetPropertyValue(info, "ImageFocusIndex", typeof(int), -1);
			_imageFocusKey = (string)LayoutReader.GetPropertyValue(info, "ImageFocusKey", typeof(string), "");
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			_itemType = (ItemType)LayoutReader.GetPropertyValue(info, "ItemType", typeof(ItemType), ItemType.LinkButton);
			_wordWrap = (bool)LayoutReader.GetPropertyValue(info, "WordWrap", typeof(bool), true);
			_shortcut = (Shortcut)LayoutReader.GetPropertyValue(info, "Shortcut", typeof(Shortcut), Shortcut.None);
			_stateStyles.Reset();
			info.DeserializeObject("StateStyles", _stateStyles);
			info["FormatStyle"]?.DeserializeObject(_stateStyles.FormatStyle);
			_containerHeight = (int)LayoutReader.GetPropertyValue(info, "ControlHeight", typeof(int), -1);
			Container = (bool)LayoutReader.GetPropertyValue(info, "Container", typeof(bool), false);
			string tagString = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			_tag = LayoutTypePolicy.ConvertOrKeepText(tagString, (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null));
			_tagString = (string)LayoutReader.GetPropertyValue(info, "TagString", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), "");
			_textAlignment = (Alignment)LayoutReader.GetPropertyValue(info, "TextAlignment", typeof(Alignment), Alignment.Near);
			_toolTipText = (string)LayoutReader.GetPropertyValue(info, "ToolTipText", typeof(string), "");
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
		}

		public TaskPaneItem()
			: this("")
		{
		}

		public TaskPaneItem(string text)
			: this(text, -1)
		{
		}

		public TaskPaneItem(string text, ItemType itemType)
		{
			_text = text;
			_itemType = itemType;
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_stateStyles = new TaskPaneStateStyles(this, FormatStyle, null, null);
#pragma warning restore CS0618
		}

		public TaskPaneItem(string text, int imageIndex)
		{
			_text = text;
			_textAlignment = Alignment.Near;
			_imageIndex = imageIndex;
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			_stateStyles = new TaskPaneStateStyles(this, FormatStyle, null, null);
#pragma warning restore CS0618
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new TaskPaneItemAccessibleObject(this);
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
				switch (ItemType)
				{
				case ItemType.Description:
					accessibleStates |= AccessibleStates.ReadOnly;
					break;
				case ItemType.Separator:
					accessibleStates = AccessibleStates.None;
					break;
				default:
					accessibleStates = AccessibleStates.Focusable | AccessibleStates.Selectable;
					if (_itemState == ItemState.Focus || _itemState == ItemState.SelectedFocus)
					{
						accessibleStates |= AccessibleStates.HotTracked;
					}
					if (Selected && TaskPane != null && TaskPane.KeepSelection)
					{
						accessibleStates |= AccessibleStates.Checked;
					}
					break;
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
			switch (_itemType)
			{
			case ItemType.Separator:
				return AccessibleRole.Separator;
			case ItemType.Description:
				return AccessibleRole.StaticText;
			default:
				return AccessibleRole.OutlineItem;
			}
		}

		internal TaskPaneItem CloneDetached()
		{
			TaskPaneItem taskPaneItem = (TaskPaneItem)MemberwiseClone();
			taskPaneItem._owner = null;
			return taskPaneItem;
		}

		internal bool IsWordWrapped()
		{
			return _effectiveWordWrap;
		}

		internal void SetWordWrapped(bool isWordWrapped)
		{
			_effectiveWordWrap = isWordWrapped;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle newBounds)
		{
			if (!(_bounds != newBounds))
			{
				return;
			}
			_bounds = newBounds;
			if (_containerControl != null)
			{
				Rectangle bounds = newBounds;
				if (TaskPane != null && TaskPane.ShouldShowGroupBorder(Group))
				{
					bounds.X++;
					bounds.Width -= 2;
				}
				if (!newBounds.IsEmpty)
				{
					_containerControl.Bounds = bounds;
				}
			}
			SyncBoundsParts();
		}

		private void SyncBoundsParts()
		{
			_boundsX = _bounds.X;
			_boundsY = _bounds.Y;
			_boundsWidth = _bounds.Width;
		}

		private void InvalidateTaskPane()
		{
			if (_owner != null)
			{
				_owner.InvalidateTaskPane();
			}
		}

		private void InvalidateLayout()
		{
			if (_owner != null)
			{
				_owner.InvalidateGroupLayout();
			}
		}

		public void InvokeOnClick()
		{
			OnClick(new ItemEventArgs(this));
			if (TaskPane != null)
			{
				TaskPane.OnItemClickCore(this);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFormatStyle()
		{
			if (_formatStyle != null)
			{
				_formatStyle._owner = null;
			}
			_formatStyle = new TaskPaneFormatStyle(this);
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

		protected virtual void OnClick(ItemEventArgs e)
		{
			if (_itemEventHandler != null)
			{
				_itemEventHandler(this, e);
			}
		}
	}
}
