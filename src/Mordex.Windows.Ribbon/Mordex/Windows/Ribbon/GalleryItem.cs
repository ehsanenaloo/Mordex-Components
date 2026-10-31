using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	public class GalleryItem : CommandBase, IKeyTipProvider, IMouseEventInvoker, IInvalidatable
	{
		private string _galleryItemText;

		private string _galleryItemDescription;

		private SuperTipSettings _superTipSettings;

		private Image _galleryItemImage;

		private CommandStyle _galleryItemStyle;

		private GalleryItemType _itemType;

		private Icon _galleryItemIcon;

		internal bool _galleryItemChecked;

		private RibbonForegroundStyle _galleryItemForegroundStyle;

		private RibbonForegroundStyle _galleryItemDescriptionForegroundStyle;

		private Color _backColor;

		private int _galleryItemImageIndex;

		private string _galleryItemImageKey;

		private bool _selected;

		private static readonly object s_mouseEnterEventKey = new object();

		private static readonly object s_mouseLeaveEventKey = new object();

		private static readonly object s_mouseMoveEventKey = new object();

		private static readonly object s_mouseDownEventKey = new object();

		private static readonly object s_mouseUpEventKey = new object();

		bool IKeyTipProvider.HasKeyTip => false;

		string IKeyTipProvider.KeyTip => "";

		[Browsable(false)]
		public Gallery Gallery
		{
			get
			{
				if (GetParentContainer() != null)
				{
					return GetParentContainer() as Gallery;
				}
				return null;
			}
		}

		[Description("Represents the background color for the item.")]
		public Color BackColor
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
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(GalleryItemType.Item)]
		[Description("Specifies whether the item is represented as a button item or a separator.")]
		public GalleryItemType ItemType
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
					NotifyTextChanged("ItemType");
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the the color and FontStyle used to draw the item\u00b4s description.")]
		public RibbonForegroundStyle DescriptionForegroundStyle => _galleryItemDescriptionForegroundStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the the color and FontStyle used to draw the item\u00b4s text.")]
		public RibbonForegroundStyle ForegroundStyle => _galleryItemForegroundStyle;

		[Localizable(true)]
		[DefaultValue("")]
		[Description("Represents the text displayed on the item.")]
		public string Text
		{
			get
			{
				return _galleryItemText;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_galleryItemText != value)
				{
					_galleryItemText = value;
					NotifyTextChanged("Text");
				}
			}
		}

		[Description("Represents a string that can be used to display an additional description for the item.")]
		[DefaultValue("")]
		[Localizable(true)]
		public string Description
		{
			get
			{
				return _galleryItemDescription;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_galleryItemDescription != value)
				{
					_galleryItemDescription = value;
					NotifyTextChanged("Description");
				}
			}
		}

		[Description("Reppresents the image displayed on the gallery item.")]
		[DefaultValue(null)]
		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image Image
		{
			get
			{
				return _galleryItemImage;
			}
			set
			{
				if (_galleryItemImage != value)
				{
					_galleryItemImage = value;
					NotifyTextChanged("Image");
				}
			}
		}

		[Description("Represents the Icon displayed on the item.")]
		[DefaultValue(null)]
		[Localizable(true)]
		public Icon Icon
		{
			get
			{
				return _galleryItemIcon;
			}
			set
			{
				if (_galleryItemIcon != value)
				{
					_galleryItemIcon = value;
					NotifyTextChanged("Icon");
				}
			}
		}

		[DefaultValue(-1)]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed on the item.")]
		public int ImageIndex
		{
			get
			{
				return _galleryItemImageIndex;
			}
			set
			{
				if (_galleryItemImageIndex != value)
				{
					_galleryItemImageIndex = value;
					NotifyTextChanged("ImageIndex");
				}
			}
		}

		[Description("Represents the key accessor for the image in the ImageList displayed on the item.")]
		[DefaultValue(null)]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
		[Localizable(true)]
		public string ImageKey
		{
			get
			{
				return _galleryItemImageKey;
			}
			set
			{
				if (_galleryItemImageKey != value)
				{
					_galleryItemImageKey = value;
					NotifyTextChanged("ImageKey");
				}
			}
		}

		[Description("Represents the settings used to customize the supertip of the command.")]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SuperTipSettings SuperTipSettings => _superTipSettings;

		[Browsable(false)]
		[Localizable(true)]
		public string SupertipSettingsData
		{
			get
			{
				PropertyValue root = new PropertyValue("SupertipSettings", _superTipSettings);
				LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
				return mordexLayoutWriter.ToString();
			}
			set
			{
				try
				{
					LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
					mordexLayoutReader["SupertipSettings"]?.DeserializeObject(_superTipSettings);
				}
				catch (Exception ex) when (ex is System.Xml.XmlException || ex is FormatException || ex is InvalidCastException || ex is ArgumentException || ex is InvalidOperationException)
				{
					// Invalid saved data keeps the current supertip settings.
				}
			}
		}

		[DefaultValue(CommandStyle.Default)]
		[Description("Specifies how the item will be drawn.")]
		public CommandStyle Style
		{
			get
			{
				return _galleryItemStyle;
			}
			set
			{
				if (_galleryItemStyle != value)
				{
					_galleryItemStyle = value;
					NotifyTextChanged("Style");
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the item is pressed or not pressed. ")]
		public bool Checked
		{
			get
			{
				if (Gallery != null)
				{
					return Gallery.SelectedItem == this;
				}
				return _galleryItemChecked;
			}
			set
			{
				if (Checked == value)
				{
					return;
				}
				if (Gallery != null)
				{
					if (value)
					{
						Gallery.SelectedItem = this;
					}
					else
					{
						Gallery.SelectedItem = null;
					}
				}
				else
				{
					_galleryItemChecked = value;
				}
				InvalidateLayout();
			}
		}

		internal override CommandType CommandType => CommandType.GalleryItem;

		[Description("Occurs when the mouse pointer enters the item.")]
		public event EventHandler MouseEnter
		{
			add
			{
				base.Events.AddHandler(s_mouseEnterEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseEnterEventKey, value);
			}
		}

		[Description("Occurs when the mouse pointer leaves the item.")]
		public event EventHandler MouseLeave
		{
			add
			{
				base.Events.AddHandler(s_mouseLeaveEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseLeaveEventKey, value);
			}
		}

		[Description("Occurs when the mouse pointer is moved over the item.")]
		public event MouseEventHandler MouseMove
		{
			add
			{
				base.Events.AddHandler(s_mouseMoveEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseMoveEventKey, value);
			}
		}

		[DefaultValue("Occurs when the mouse pointer is over the item and the left button is pressed.")]
		public event MouseEventHandler MouseDown
		{
			add
			{
				base.Events.AddHandler(s_mouseDownEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseDownEventKey, value);
			}
		}

		[Description("Occurs when the mouse pointer is over the item and the left mouse button is released.")]
		public event MouseEventHandler MouseUp
		{
			add
			{
				base.Events.AddHandler(s_mouseUpEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseUpEventKey, value);
			}
		}

		public GalleryItem()
		{
		}

		public GalleryItem(string key)
			: base(key)
		{
		}

		public GalleryItem(string key, string text)
			: this(key, text, null)
		{
		}

		public GalleryItem(string key, GalleryItemType itemType)
			: base(key)
		{
			_itemType = itemType;
		}

		public GalleryItem(string key, string text, Image image)
			: base(key)
		{
			base.Key = key;
			Text = text;
			_galleryItemImage = image;
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_galleryItemText = "";
			_galleryItemDescription = "";
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.ImageListProvider = this;
			_galleryItemStyle = CommandStyle.Default;
			_itemType = GalleryItemType.Item;
			_galleryItemForegroundStyle = new RibbonForegroundStyle(this);
			_galleryItemDescriptionForegroundStyle = new RibbonForegroundStyle(this);
			_backColor = Color.Empty;
			_galleryItemImageIndex = -1;
		}

		void IInvalidatable.Invalidate()
		{
			InvalidateLayout();
		}

		public bool ShouldSerializeSupertipSettingsData()
		{
			if (_superTipSettings.FooterImage != null)
			{
				return true;
			}
			if (_superTipSettings.FooterImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.FooterImageKey != null && _superTipSettings.FooterImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.FooterText != null && _superTipSettings.FooterText.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.HeaderImage != null)
			{
				return true;
			}
			if (_superTipSettings.HeaderImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.HeaderImageKey != null && _superTipSettings.HeaderImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.HeaderText != null && _superTipSettings.HeaderText.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.Image != null)
			{
				return true;
			}
			if (_superTipSettings.ImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.ImageKey != null && _superTipSettings.ImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.Text != null && _superTipSettings.Text.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.ToolTipStyle != ToolTipStyle.Office2007)
			{
				return true;
			}
			if (_superTipSettings.WordWrap)
			{
				return true;
			}
			return false;
		}

		public void ResetSupertipSettingsData()
		{
			_superTipSettings.FooterImage = null;
			_superTipSettings.FooterImageIndex = -1;
			_superTipSettings.FooterImageKey = "";
			_superTipSettings.FooterText = "";
			_superTipSettings.HeaderImage = null;
			_superTipSettings.HeaderImageIndex = -1;
			_superTipSettings.HeaderImageKey = "";
			_superTipSettings.HeaderText = "";
			_superTipSettings.Image = null;
			_superTipSettings.ImageIndex = -1;
			_superTipSettings.ImageKey = "";
			_superTipSettings.Text = "";
			_superTipSettings.ToolTipStyle = ToolTipStyle.Office2007;
			_superTipSettings.WordWrap = false;
		}

		internal bool IsSelected()
		{
			return _selected;
		}

		internal CommandStyle GetEffectiveStyle()
		{
			if (_galleryItemStyle == CommandStyle.Default)
			{
				if (Gallery != null && Gallery.ItemsStyle != CommandStyle.Default)
				{
					return Gallery.ItemsStyle;
				}
				return CommandStyle.ImageAndText;
			}
			return _galleryItemStyle;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColor()
		{
			BackColor = Color.Empty;
		}

		public override object Clone()
		{
			GalleryItem galleryItem = new GalleryItem();
			galleryItem._galleryItemDescription = _galleryItemDescription;
			galleryItem.Enabled = Enabled;
			galleryItem.Image = _galleryItemImage;
			galleryItem.ItemType = _itemType;
			galleryItem.Key = base.Key;
			galleryItem.Style = _galleryItemStyle;
			galleryItem._galleryItemChecked = _galleryItemChecked;
			galleryItem._galleryItemImageIndex = _galleryItemImageIndex;
			galleryItem._galleryItemImageKey = _galleryItemImageKey;
			galleryItem._backColor = _backColor;
			if (_superTipSettings != null)
			{
				galleryItem._superTipSettings = _superTipSettings.Clone();
				galleryItem._superTipSettings.ImageListProvider = galleryItem;
			}
			galleryItem.Tag = base.Tag;
			galleryItem._galleryItemText = _galleryItemText;
			galleryItem.Visible = base.Visible;
			galleryItem._galleryItemForegroundStyle = _galleryItemForegroundStyle.CloneForOwner(galleryItem);
			galleryItem._galleryItemDescriptionForegroundStyle = _galleryItemDescriptionForegroundStyle.CloneForOwner(galleryItem);
			galleryItem.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			return galleryItem;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (!_backColor.IsEmpty)
			{
				writer.AddValue("BackColor", _backColor);
			}
			if (_galleryItemImageIndex != -1)
			{
				writer.AddValue("ImageIndex", _galleryItemImageIndex);
			}
			if (_galleryItemImageKey != null)
			{
				writer.AddValue("ImageKey", _galleryItemImageKey);
			}
			if (Gallery != null && Gallery.SelectedItem == this)
			{
				writer.AddValue("Selected", true);
			}
			if (_galleryItemDescription.Length > 0)
			{
				writer.AddValue("Description", _galleryItemDescription);
			}
			if (_galleryItemDescriptionForegroundStyle.HasCustomStyle())
			{
				writer.AddValue("DescriptionForegroundStyle", _galleryItemDescriptionForegroundStyle);
			}
			if (_galleryItemForegroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _galleryItemForegroundStyle);
			}
			if (_galleryItemIcon != null)
			{
				writer.AddValue("Icon", _galleryItemIcon);
			}
			if (_galleryItemImage != null)
			{
				writer.AddValue("Image", _galleryItemImage);
			}
			if (Checked)
			{
				writer.AddValue("Checked", Checked);
			}
			if (_itemType != GalleryItemType.Item)
			{
				writer.AddValue("ItemType", _itemType);
			}
			if (_galleryItemStyle != CommandStyle.Default)
			{
				writer.AddValue("Style", _galleryItemStyle);
			}
			if (_superTipSettings != null && !_superTipSettings.IsEmpty)
			{
				writer.AddValue("SuperTipSettings", _superTipSettings);
			}
			if (_galleryItemText.Length > 0)
			{
				writer.AddValue("Text", _galleryItemText);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_galleryItemDescription = (string)LayoutReader.GetPropertyValue(reader, "Description", typeof(string), "");
			PropertyValue propertyValue = reader["DescriptionForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_galleryItemDescriptionForegroundStyle);
			}
			else
			{
				_galleryItemDescriptionForegroundStyle.ResetAll();
			}
			propertyValue = reader["ForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_galleryItemForegroundStyle);
			}
			else
			{
				_galleryItemForegroundStyle.ResetAll();
			}
			propertyValue = reader["SuperTipSettings"] ?? reader["SupperTipSettings"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(SuperTipSettings);
			}
			else
			{
				_superTipSettings = new SuperTipSettings();
				_superTipSettings.ImageListProvider = this;
			}
			_galleryItemIcon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_galleryItemImage = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			Checked = (bool)LayoutReader.GetPropertyValue(reader, "Checked", typeof(bool), false);
			_itemType = (GalleryItemType)LayoutReader.GetPropertyValue(reader, "ItemType", typeof(GalleryItemType), GalleryItemType.Item);
			_galleryItemStyle = (CommandStyle)LayoutReader.GetPropertyValue(reader, "Style", typeof(CommandStyle), CommandStyle.Default);
			_galleryItemText = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			_backColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), Color.Empty);
			_galleryItemImageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_galleryItemImageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), null);
			_selected = (bool)LayoutReader.GetPropertyValue(reader, "Selected", typeof(bool), false);
		}

		private protected override void ApplyClickState()
		{
			Checked = true;
		}

		void IMouseEventInvoker.InvokeMouseEnter()
		{
			if (IsEnabled)
			{
				OnMouseEnter(EventArgs.Empty);
			}
		}

		void IMouseEventInvoker.InvokeMouseLeave()
		{
			if (IsEnabled)
			{
				OnMouseLeave(EventArgs.Empty);
			}
		}

		void IMouseEventInvoker.InvokeMouseMove(MouseEventArgs eventArgs)
		{
			if (IsEnabled)
			{
				OnMouseMove(eventArgs);
			}
		}

		void IMouseEventInvoker.InvokeMouseDown(MouseEventArgs eventArgs)
		{
			if (IsEnabled)
			{
				OnMouseDown(eventArgs);
			}
		}

		void IMouseEventInvoker.InvokeMouseUp(MouseEventArgs eventArgs)
		{
			if (IsEnabled)
			{
				OnMouseUp(eventArgs);
			}
		}

		protected virtual void OnMouseEnter(EventArgs e)
		{
			if (base.Events[s_mouseEnterEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnMouseLeave(EventArgs e)
		{
			if (base.Events[s_mouseLeaveEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnMouseMove(MouseEventArgs e)
		{
			if (base.Events[s_mouseMoveEventKey] is MouseEventHandler mouseEventHandler)
			{
				mouseEventHandler(this, e);
			}
		}

		protected virtual void OnMouseDown(MouseEventArgs e)
		{
			if (base.Events[s_mouseDownEventKey] is MouseEventHandler mouseEventHandler)
			{
				mouseEventHandler(this, e);
			}
		}

		protected virtual void OnMouseUp(MouseEventArgs e)
		{
			if (base.Events[s_mouseUpEventKey] is MouseEventHandler mouseEventHandler)
			{
				mouseEventHandler(this, e);
			}
		}
	}
}
