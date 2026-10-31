using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[Designer("Mordex.Windows.Ribbon.Design.GalleryDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class Gallery : DropDownCommand
	{
		private GalleryItemCollection _items;

		private int _minGalleryColumns;

		private int _maxGalleryRows;

		private ContentAlignment _itemsTextAlignment;

		private ContentAlignment _itemsImageAlignment;

		private TextImageRelation _itemsTextImageRelation;

		private int _maxGalleryColumns;

		private int _minGalleryRows;

		private int _itemsHeight;

		private int _itemsWidth;

		private bool _allowResizeColumns;

		private ImageList _itemImageList;

		private Size _itemImageSize;

		private Color _itemsBackColor;

		private CommandStyle _itemsStyle;

		private GalleryItem _selectedItem;

		internal int _scrollRowOffset = -1;

		private static readonly object s_selectedItemChangedKey = new object();

		internal override AccessibleRole Role
		{
			get
			{
				if (base.AccessibleRole != AccessibleRole.Default)
				{
					return base.AccessibleRole;
				}
				return AccessibleRole.List;
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
				OnSiteChanged();
			}
		}

		internal override CommandType CommandType => CommandType.Gallery;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GalleryItemCollection GalleryItems => _items;

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The ImageList that contains the Images to use for the gallery items.")]
		public ImageList ImageList
		{
			get
			{
				return _itemImageList;
			}
			set
			{
				if (_itemImageList != value)
				{
					_itemImageList = value;
					NotifyTextChanged("ImageList");
				}
			}
		}

		[Description("Determines the size of the image in the items.")]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Size ImageSize
		{
			get
			{
				return _itemImageSize;
			}
			set
			{
				if (_itemImageSize != value)
				{
					_itemImageSize = value;
					NotifyTextChanged("ImageSize");
				}
			}
		}

		[Description("Represents the default background color for the items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Color ItemsBackColor
		{
			get
			{
				return _itemsBackColor;
			}
			set
			{
				if (_itemsBackColor != value)
				{
					_itemsBackColor = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(CommandStyle.ImageAndText)]
		[Description("Represents the default style of the items in the gallery.")]
		public CommandStyle ItemsStyle
		{
			get
			{
				return _itemsStyle;
			}
			set
			{
				if (_itemsStyle != value)
				{
					_itemsStyle = value;
					NotifyTextChanged("ItemsStyle");
				}
			}
		}

		[Browsable(false)]
		[Description("The GalleryItem object currently selected.")]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GalleryItem SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				if (_selectedItem == value)
				{
					return;
				}
				if (_selectedItem != null)
				{
					_selectedItem._galleryItemChecked = false;
				}
				_selectedItem = value;
				if (_selectedItem != null)
				{
					_selectedItem._galleryItemChecked = true;
				}
				InvalidateLayout();
				OnSelectedItemChanged(EventArgs.Empty);
				if (base.Ribbon != null)
				{
					if (base.Ribbon._mergeHost != null)
					{
						base.Ribbon._mergeHost.NotifyGallerySelectionChanged(this, _selectedItem);
					}
					else
					{
						base.Ribbon.NotifyGallerySelectionChanged(this, _selectedItem);
					}
				}
			}
		}

		[Description("Indicates whether the user can resize the width of the gallery's menu window")]
		[DefaultValue(false)]
		public bool AllowResizeColumns
		{
			get
			{
				return _allowResizeColumns;
			}
			set
			{
				if (_allowResizeColumns != value)
				{
					_allowResizeColumns = value;
					NotifyTextChanged("AllowResizeColumns");
				}
			}
		}

		[Description("Represents the minimum allowed rows in the gallery's menu window.")]
		[DefaultValue(-1)]
		public int MinGalleryColumns
		{
			get
			{
				return _minGalleryColumns;
			}
			set
			{
				if (_minGalleryColumns != value)
				{
					_minGalleryColumns = value;
					NotifyTextChanged("MinGalleryColumns");
				}
			}
		}

		[DefaultValue(-1)]
		[Description("Represents the maximum allowed rows in the gallery's menu window.")]
		public int MaxGalleryRows
		{
			get
			{
				return _maxGalleryRows;
			}
			set
			{
				if (_maxGalleryRows != value)
				{
					_maxGalleryRows = value;
					NotifyTextChanged("MaxGalleryRows");
				}
			}
		}

		[Description("Indicates the width of the items in the gallery")]
		[DefaultValue(-1)]
		public int ItemsWidth
		{
			get
			{
				return _itemsWidth;
			}
			set
			{
				if (_itemsWidth != value)
				{
					_itemsWidth = value;
					NotifyTextChanged("ItemsWidth");
				}
			}
		}

		[Description("Indicates the height of the items in the gallery")]
		[DefaultValue(-1)]
		public int ItemsHeight
		{
			get
			{
				return _itemsHeight;
			}
			set
			{
				if (_itemsHeight != value)
				{
					_itemsHeight = value;
					NotifyTextChanged("ItemsHeight");
				}
			}
		}

		[DefaultValue(TextImageRelation.ImageBeforeText)]
		[Description("Indicates the position of item\u00b4s text and image relative to each other.")]
		public TextImageRelation ItemsTextImageRelation
		{
			get
			{
				return _itemsTextImageRelation;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextImageRelation), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_itemsTextImageRelation != value)
				{
					_itemsTextImageRelation = value;
					NotifyTextChanged("ItemsTextImageRelation");
				}
			}
		}

		[DefaultValue(ContentAlignment.MiddleLeft)]
		[Description("Determines the alignment of the text on the items.")]
		public ContentAlignment ItemsTextAlignment
		{
			get
			{
				return _itemsTextAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ContentAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_itemsTextAlignment != value)
				{
					_itemsTextAlignment = value;
					NotifyTextChanged("ItemsTextAlignment");
				}
			}
		}

		[Description("Determines the alignment of the image on the items.")]
		[DefaultValue(ContentAlignment.MiddleLeft)]
		public ContentAlignment ItemsImageAlignment
		{
			get
			{
				return _itemsImageAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ContentAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_itemsImageAlignment != value)
				{
					_itemsImageAlignment = value;
					NotifyTextChanged("ItemsImageAlignment");
				}
			}
		}

		[Description("Represents the maximum allowed columns in the gallery's menu window.")]
		[DefaultValue(-1)]
		public int MaxGalleryColumns
		{
			get
			{
				return _maxGalleryColumns;
			}
			set
			{
				if (_maxGalleryColumns != value)
				{
					_maxGalleryColumns = value;
					NotifyTextChanged("MaxGalleryColumns");
				}
			}
		}

		[DefaultValue(-1)]
		[Description("Represents the minimum allowed rows in the gallery's menu window.")]
		public int MinGalleryRows
		{
			get
			{
				return _minGalleryRows;
			}
			set
			{
				if (_minGalleryRows != value)
				{
					_minGalleryRows = value;
					NotifyTextChanged("MinGalleryRows");
				}
			}
		}

		[Description("Event fired when the SelectedItem property has changed.")]
		public event EventHandler SelectedItemChanged
		{
			add
			{
				base.Events.AddHandler(s_selectedItemChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectedItemChangedKey, value);
			}
		}

		public Gallery()
		{
		}

		public Gallery(string key)
			: base(key)
		{
		}

		public Gallery(string key, string text)
			: this(key, text, -1)
		{
		}

		public Gallery(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_items = new GalleryItemCollection(this);
			_minGalleryColumns = -1;
			_minGalleryRows = -1;
			_itemsTextAlignment = ContentAlignment.MiddleLeft;
			_itemsImageAlignment = ContentAlignment.MiddleLeft;
			_itemsTextImageRelation = TextImageRelation.ImageBeforeText;
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.ImageListProvider = this;
			_maxGalleryColumns = -1;
			_maxGalleryRows = -1;
			_itemsHeight = -1;
			_itemsWidth = -1;
			_itemsStyle = CommandStyle.ImageAndText;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _items != null)
			{
				for (int i = 0; i < _items.Count; i++)
				{
					CommandBase commandBase = _items[i];
					commandBase.SetParentContainer(null);
					commandBase.CommandsOwner = null;
					commandBase.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private void OnSiteChanged()
		{
		}

		public override object Clone()
		{
			Gallery gallery = new Gallery();
			gallery.AllowDropDownResize = base.AllowDropDownResize;
			gallery._allowResizeColumns = _allowResizeColumns;
			gallery.DropDownStyle = base.DropDownStyle;
			gallery._maxGalleryColumns = _maxGalleryColumns;
			gallery._dropDownItems = base.Commands.CloneWithParent(gallery);
			gallery.CommandsSizeStyle = base.CommandsSizeStyle;
			gallery.Description = base.Description;
			gallery.Enabled = Enabled;
			gallery._items = _items.CloneWithParent(gallery);
			gallery.Icon = base.Icon;
			gallery.Image = base.Image;
			gallery.ItemsImageAlignment = ItemsImageAlignment;
			gallery.ImageIndex = base.ImageIndex;
			gallery.ImageKey = base.ImageKey;
			gallery.LargeImageKey = base.LargeImageKey;
			gallery.ActAsOptionButton = base.ActAsOptionButton;
			gallery.ItemsHeight = ItemsHeight;
			gallery._itemsStyle = _itemsStyle;
			gallery._itemsBackColor = _itemsBackColor;
			gallery.ItemsWidth = ItemsWidth;
			gallery.Key = base.Key;
			gallery.KeyTip = base.KeyTip;
			gallery.LargeIcon = base.LargeIcon;
			gallery.LargeImage = base.LargeImage;
			gallery.LargeImageIndex = base.LargeImageIndex;
			gallery.MaxDropDownItems = base.MaxDropDownItems;
			gallery._minGalleryColumns = _minGalleryColumns;
			gallery.MaxGalleryRows = _maxGalleryRows;
			gallery._minGalleryRows = _minGalleryRows;
			gallery.Shortcut = base.Shortcut;
			gallery.SizeStyle = SizeStyle;
			gallery.Style = base.Style;
			gallery.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			if (_superTipSettings != null)
			{
				gallery._superTipSettings = _superTipSettings.Clone();
				gallery._superTipSettings.ImageListProvider = gallery;
			}
			gallery.Tag = base.Tag;
			gallery.Text = base.Text;
			gallery.ItemsTextAlignment = ItemsTextAlignment;
			gallery.ItemsTextImageRelation = ItemsTextImageRelation;
			gallery.Visible = base.Visible;
			return gallery;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_allowResizeColumns)
			{
				writer.AddValue("AllowResizeColumns", _allowResizeColumns);
			}
			if (_items.Count > 0)
			{
				writer.AddValue("GalleryItems", _items);
			}
			if (!_itemsBackColor.IsEmpty)
			{
				writer.AddValue("ItemsBackColor", _itemsBackColor);
			}
			if (_itemsStyle != CommandStyle.ImageAndText)
			{
				writer.AddValue("ItemsStyle", _itemsStyle);
			}
			if (_itemsHeight != -1)
			{
				writer.AddValue("ItemsHeight", _itemsHeight);
			}
			if (_itemsImageAlignment != ContentAlignment.MiddleLeft)
			{
				writer.AddValue("ItemsImageAlignment", _itemsImageAlignment);
			}
			if (_itemsTextAlignment != ContentAlignment.MiddleLeft)
			{
				writer.AddValue("ItemsTextAlignment", _itemsTextAlignment);
			}
			if (_itemsTextImageRelation != TextImageRelation.ImageBeforeText)
			{
				writer.AddValue("ItemsTextImageRelation", _itemsTextImageRelation);
			}
			if (_itemsWidth != -1)
			{
				writer.AddValue("ItemsWidth", _itemsWidth);
			}
			if (_maxGalleryColumns != -1)
			{
				writer.AddValue("MaxGalleryColumns", _maxGalleryColumns);
			}
			if (_maxGalleryRows != -1)
			{
				writer.AddValue("MaxGalleryRows", _maxGalleryRows);
			}
			if (_minGalleryColumns != -1)
			{
				writer.AddValue("MinGalleryColumns", _minGalleryColumns);
			}
			if (_minGalleryRows != -1)
			{
				writer.AddValue("MinGalleryRows", _minGalleryRows);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_allowResizeColumns = (bool)LayoutReader.GetPropertyValue(reader, "AllowResizeColumns", typeof(bool), false);
			_itemsHeight = (int)LayoutReader.GetPropertyValue(reader, "ItemsHeight", typeof(int), -1);
			_itemsImageAlignment = (ContentAlignment)LayoutReader.GetPropertyValue(reader, "ItemsImageAlignment", typeof(ContentAlignment), ContentAlignment.MiddleLeft);
			_itemsTextAlignment = (ContentAlignment)LayoutReader.GetPropertyValue(reader, "ItemsTextAlignment", typeof(ContentAlignment), ContentAlignment.MiddleLeft);
			_itemsTextImageRelation = (TextImageRelation)LayoutReader.GetPropertyValue(reader, "ItemsTextImageRelation", typeof(TextImageRelation), TextImageRelation.ImageBeforeText);
			_itemsWidth = (int)LayoutReader.GetPropertyValue(reader, "ItemsWidth", typeof(int), -1);
			_maxGalleryColumns = (int)LayoutReader.GetPropertyValue(reader, "MaxGalleryColumns", typeof(int), -1);
			_maxGalleryRows = (int)LayoutReader.GetPropertyValue(reader, "MaxGalleryRows", typeof(int), -1);
			_minGalleryColumns = (int)LayoutReader.GetPropertyValue(reader, "MinGalleryColumns", typeof(int), -1);
			_minGalleryRows = (int)LayoutReader.GetPropertyValue(reader, "MinGalleryRows", typeof(int), -1);
			_itemsStyle = (CommandStyle)LayoutReader.GetPropertyValue(reader, "ItemsStyle", typeof(CommandStyle), CommandStyle.ImageAndText);
			_itemsBackColor = (Color)LayoutReader.GetPropertyValue(reader, "ItemsBackColor", typeof(Color), Color.Empty);
			DeserializeGalleryState(reader);
		}

		internal override void CommandDisposed(CommandBase cmd)
		{
			base.CommandDisposed(cmd);
			if (cmd is GalleryItem && _items.Contains(cmd as GalleryItem))
			{
				GalleryItems.Remove(cmd as GalleryItem);
			}
		}

		private void DeserializeGalleryState(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["GalleryItems"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_items);
			}
			else
			{
				_items.DisposeAll();
			}
			foreach (GalleryItem item in _items)
			{
				if (item.IsSelected())
				{
					SelectedItem = item;
					return;
				}
			}
			SelectedItem = null;
		}

		internal int GetEffectiveMinColumns()
		{
			if (_minGalleryColumns <= 0)
			{
				return 1;
			}
			return _minGalleryColumns;
		}

		internal int GetEffectiveMinRows()
		{
			if (_minGalleryRows <= 0)
			{
				return 1;
			}
			return _minGalleryRows;
		}

		protected virtual void OnSelectedItemChanged(EventArgs e)
		{
			if (base.Events[s_selectedItemChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
